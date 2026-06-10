#!/bin/bash
# TEMPLATE PLACEHOLDERS:
#   {{PIPELINE_SENTINEL}}: path to a file that exists only after the build/pipeline
#                          has been run (e.g., "lc-workspace/dist/app-proposal-generator/index.html"
#                          or "LC/Host/LC.Host.Api/bin/Release/net10.0/LC.Host.Api.dll").
#                          Set to empty string to disable the check.
set -euo pipefail

# Dynamic session context: surfaces actionable state at session start.

# Helper: clean and remove a worktree directory.
# Shared with worktree-clean-before-remove.sh. Keep in sync.
_clean_worktree_dir() {
  local wt_path="$1"
  [ -d "$wt_path" ] || return 0
  if ! git worktree remove --force "$wt_path" 2>/dev/null; then
    rm -rf "$wt_path" 2>/dev/null || true
    git worktree prune 2>/dev/null || true
  fi
}

# Ghost-cwd guard: if this session's $PWD no longer exists on disk or is
# inside a worktree directory, fail LOUDLY with recovery directive.
REAL_PWD=$(pwd -P 2>/dev/null || echo "")
if [ -z "$REAL_PWD" ] || [ ! -d "$REAL_PWD" ]; then
  echo "⚠ GHOST CWD: session cwd does not exist on disk." >&2
  echo "  MANDATORY: cd to the project root as your FIRST action before ANY other command." >&2
elif [[ "$REAL_PWD" == */.claude/worktrees/* ]]; then
  echo "⚠ WORKTREE CWD: session launched from inside a worktree: $REAL_PWD" >&2
  echo "  Worktrees are ephemeral. When removed, this session's shell dies." >&2
  echo "  MANDATORY: cd to the project root as your FIRST action before ANY other command." >&2
fi

if git rev-parse --is-inside-work-tree > /dev/null 2>&1; then
  TOPLEVEL=$(git rev-parse --show-toplevel 2>/dev/null)

  # Active worktree count
  WT_COUNT=$(git worktree list 2>/dev/null | grep -v "^${TOPLEVEL} " | grep -c . || true)
  if [ "$WT_COUNT" -gt 0 ]; then
    echo "Worktrees: $WT_COUNT active" >&2
  fi

  # Ghost-worktree detection
  if [ -d "${TOPLEVEL}/.claude/worktrees" ]; then
    # Registered worktrees whose directories are gone
    while IFS= read -r wt_path; do
      [ -z "$wt_path" ] && continue
      [ "$wt_path" = "$TOPLEVEL" ] && continue
      if [ ! -d "$wt_path" ]; then
        echo "CRITICAL: Registered worktree has no directory: $wt_path" >&2
        echo "  Run 'git worktree prune' from main repo." >&2
      fi
    done < <(git worktree list 2>/dev/null | awk '{print $1}')

    # Orphan directories not registered with git
    for entry in "${TOPLEVEL}/.claude/worktrees"/*; do
      [ -d "$entry" ] || continue
      if ! git worktree list 2>/dev/null | awk '{print $1}' | grep -qx "$entry"; then
        echo "WARNING: Orphan directory in .claude/worktrees/ not registered: $entry" >&2
        echo "  Likely crashed-session leftover. Safe to 'rm -rf' after verifying no pending work." >&2
      fi
    done
  fi

  # Garbage-collect prunable worktrees
  GC_COUNT=0
  while IFS= read -r wt_line; do
    [ -z "$wt_line" ] && continue
    echo "$wt_line" | grep -q "prunable" || continue
    wt_path=$(echo "$wt_line" | awk '{print $1}')
    [ -z "$wt_path" ] && continue
    [ "$wt_path" = "$TOPLEVEL" ] && continue
    _clean_worktree_dir "$wt_path"
    GC_COUNT=$((GC_COUNT + 1))
  done < <(git worktree list 2>/dev/null)

  if [ "$GC_COUNT" -gt 0 ]; then
    git worktree prune 2>/dev/null || true
    echo "GC: cleaned $GC_COUNT stale worktree(s)" >&2
  fi

  # Branch state
  BRANCH=$(git branch --show-current 2>/dev/null || echo "")
  if [ -n "$BRANCH" ] && [ "$BRANCH" != "main" ]; then
    DIRTY=$(git status --porcelain 2>/dev/null | head -1 || echo "")
    if [ -n "$DIRTY" ]; then
      echo "WARNING: On branch '$BRANCH' with uncommitted changes" >&2
    else
      echo "On branch '$BRANCH' (clean)" >&2
    fi
  fi

  # Pipeline sentinel check — fill in {{PIPELINE_SENTINEL}} with the build output path
  SENTINEL="lc-workspace/dist/app-sales-intelligence-app/browser/index.html"
  if [ -n "$SENTINEL" ] && [ ! -f "$SENTINEL" ]; then
    echo "WARNING: Build not run (missing $SENTINEL). Run /verify." >&2
  fi

  # Unfilled placeholder check
  UNFILLED=$(grep -rn '{{[A-Z_]*}}' .claude/commands/ CLAUDE.md 2>/dev/null | head -3 || true)
  if [ -n "$UNFILLED" ]; then
    UNFILLED_COUNT=$(grep -rn '{{[A-Z_]*}}' .claude/commands/ CLAUDE.md 2>/dev/null | wc -l | tr -d ' ')
    echo "NOTE: $UNFILLED_COUNT unfilled {{PLACEHOLDER}} tokens found. Fill them before starting work." >&2
  fi

  # Open PRs (timeout-guarded, non-fatal)
  PR_COUNT=$(timeout 5 gh pr list --json number --jq 'length' 2>/dev/null || echo "")
  if [ -n "$PR_COUNT" ] && [ "$PR_COUNT" != "0" ]; then
    echo "Open PRs: $PR_COUNT" >&2
  fi
fi

exit 0
