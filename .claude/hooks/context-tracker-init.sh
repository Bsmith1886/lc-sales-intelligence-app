#!/bin/bash
# Context Tracker: SessionStart hook
# Creates a session-specific state file for context monitoring.
# State file: /tmp/claude-ctx-{session_id}
#   line 1: transcript path
#   line 2: warn state (0-2)
#   line 3: session_id
#   line 4: enabled (1=enabled, 2=disabled)
#   line 5: budget_tokens

# Disable entirely via env var (add export CTX_ENABLED=0 to shell profile)
[ "${CTX_ENABLED:-1}" = "0" ] && exit 0

# Purge stale state files older than 1 day
find /tmp -maxdepth 1 -name 'claude-ctx-*' -mtime +1 -delete 2>/dev/null || true

INPUT=$(cat)
_after="${INPUT#*\"session_id\":\"}"
[ "$_after" = "$INPUT" ] && exit 0
SESSION_ID="${_after%%\"*}"
[ -z "$SESSION_ID" ] && exit 0

CTX_FILE="/tmp/claude-ctx-${SESSION_ID}"

# Transcript: ~/.claude-lx/projects/{slug}/{session_id}.jsonl
CONFIG_DIR="${HOME}/.claude-lx"
PROJECT_DIR="${CLAUDE_PROJECT_DIR:-$(pwd)}"
SLUG="${PROJECT_DIR//[^a-zA-Z0-9-]/-}"
TRANSCRIPT="${CONFIG_DIR}/projects/${SLUG}/${SESSION_ID}.jsonl"

# Atomic write: always-on with default budget
BUDGET="${CTX_BUDGET_TOKENS:-100000}"
_tmp="${CTX_FILE}.$$"
printf '%s\n0\n%s\n1\n%s\n' "$TRANSCRIPT" "$SESSION_ID" "$BUDGET" > "$_tmp"
mv "$_tmp" "$CTX_FILE"

exit 0
