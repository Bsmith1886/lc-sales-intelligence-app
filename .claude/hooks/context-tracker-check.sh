#!/bin/bash
# Context Tracker: PostToolUse hook
# Estimates token usage from transcript file size.
# 2-level warnings at 80% and 100% of effective context budget.
# Performance target: <10ms. Builtin-only session resolution.
# Always-on: skips if state file line 4 (enabled) is not 1 (manual disable).

# --- Resolve session ID (env var > stdin parse > project fallback) ---
_sid="${CLAUDE_SESSION_ID:-}"
if [ -z "$_sid" ]; then
  # Grab first ~2000 chars of stdin via builtin. Hook input is pre-buffered
  # in the pipe so this returns in microseconds. session_id is a top-level
  # JSON field that appears before the large tool_response blob.
  read -r -n 2000 -t 1 _buf 2>/dev/null || true
  if [ -n "${_buf:-}" ]; then
    _after="${_buf#*\"session_id\":\"}"
    [ "$_after" != "$_buf" ] && _sid="${_after%%\"*}"
  fi
fi

if [ -n "$_sid" ]; then
  CTX_FILE="/tmp/claude-ctx-${_sid}"
else
  # Fallback: project-scoped (cross-project isolation, not per-session)
  _dir="${CLAUDE_PROJECT_DIR:-$(pwd)}"
  CTX_FILE="/tmp/claude-ctx-${_dir//[^a-zA-Z0-9]/-}"
fi

[ ! -f "$CTX_FILE" ] && exit 0

# Read state with builtins (no sed/awk)
# Format: transcript, warn_state, session_id, enabled, budget_tokens
TRANSCRIPT="" WARN_STATE="" _fsid="" ENABLED="" FILE_BUDGET=""
{ read -r TRANSCRIPT; read -r WARN_STATE; read -r _fsid; read -r ENABLED; read -r FILE_BUDGET; } < "$CTX_FILE" 2>/dev/null || true

# Skip if manually disabled (2=disabled)
[ "${ENABLED:-0}" != "1" ] && exit 0

[ -z "$TRANSCRIPT" ] && exit 0
[ ! -f "$TRANSCRIPT" ] && exit 0

# Budget: use user-confirmed value from state file, fall back to env/default
BPT="${CTX_BYTES_PER_TOKEN:-6}"
if [ -n "$FILE_BUDGET" ] && [ "$FILE_BUDGET" -gt 0 ] 2>/dev/null; then
  BUDGET="$FILE_BUDGET"
else
  BUDGET="${CTX_BUDGET_TOKENS:-100000}"
fi
WARN1=$((BUDGET * 80 / 100))
WARN2=$((BUDGET))

FILE_BYTES=$(wc -c < "$TRANSCRIPT" 2>/dev/null) || exit 0
FILE_BYTES="${FILE_BYTES// /}"
[ -z "$FILE_BYTES" ] && exit 0
EST_TOKENS=$((FILE_BYTES / BPT))
EST_K=$((EST_TOKENS / 1000))
PCT=$((EST_TOKENS * 100 / BUDGET))

# Atomic state update (preserves all fields, changes warn level)
_update_warn() {
  local _t="${CTX_FILE}.$$"
  printf '%s\n%s\n%s\n%s\n%s\n' "$TRANSCRIPT" "$1" "${_fsid:-}" "${ENABLED:-1}" "${FILE_BUDGET:-$BUDGET}" > "$_t"
  mv "$_t" "$CTX_FILE"
}

# Level 2: WIND-DOWN (100%+)
if [ "${WARN_STATE:-0}" -lt 2 ] && [ "$EST_TOKENS" -ge "$WARN2" ]; then
  _update_warn 2
  echo "CONTEXT BUDGET REACHED [${PCT}%]: ~${EST_K}K tokens. Find a stopping point and /wind-down. (say 'disable context tracking' to turn off)" >&2
  printf '{"hookSpecificOutput":{"hookEventName":"PostToolUse","additionalContext":"CONTEXT BUDGET REACHED [%s%% of budget]: ~%sK tokens estimated. Find a good stopping point and run /wind-down. Finish your current thought, do not start new work. To disable context tracking if user asks: write enabled=2 (line 4) in state file %s."}}' "$PCT" "$EST_K" "$CTX_FILE"

# Level 1: HEADS-UP (80%+)
elif [ "${WARN_STATE:-0}" -lt 1 ] && [ "$EST_TOKENS" -ge "$WARN1" ]; then
  _update_warn 1
  echo "CONTEXT HEADS-UP [${PCT}%]: ~${EST_K}K tokens. Getting close to budget. (say 'disable context tracking' to turn off)" >&2
  printf '{"hookSpecificOutput":{"hookEventName":"PostToolUse","additionalContext":"CONTEXT HEADS-UP [%s%% of budget]: ~%sK tokens estimated. Getting close to context budget. Start wrapping up current work and plan for /wind-down soon. To disable context tracking if user asks: write enabled=2 (line 4) in state file %s."}}' "$PCT" "$EST_K" "$CTX_FILE"
fi

exit 0
