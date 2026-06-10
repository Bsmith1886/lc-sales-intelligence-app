Context-safe session wind-down. Use this when the context window is getting full
and you need to end THIS session cleanly WITHOUT clobbering work owned by other
parallel sessions in other workstreams.

**This is NOT `/ship`.** `/ship` is the happy-path session-end for a completed
workstream: it verifies, commits, pushes. `/wind-down` is the context-exhaustion
path: it preserves cross-session integrity, writes a workstream-scoped resume
prompt, and refuses to auto-commit or auto-stash work (you must choose).

## Step 1: Identify the workstream

Derive the workstream from three independent signals:
1. **Branch name**: `git branch --show-current`
2. **Recently touched memory files**: `ls -lt memory/ | head -5`
3. **Issues referenced in conversation**: scan for `#NNN`

If all three agree → auto-select. If they disagree → ask operator ONCE.

## Step 2: Snapshot cross-session state

```bash
git branch --show-current
git status --short
git log --oneline -5
git worktree list
gh pr list --limit 20 --json number,title,headRefName,state,mergeable
gh issue list --limit 20 --state open --json number,title,labels
```

## Step 3: Handle in-progress work deliberately

If uncommitted changes exist, STOP and present three options:
1. **Commit**: stage specific files, conventional message
2. **Stash**: `git stash push -u -m "wind-down {slug} {ISO-date}"`
3. **Discard**: `git restore . && git clean -fd`

Wait for operator to choose. Do NOT auto-pick.

## Step 4: Verify ground truth

```bash
git rev-parse HEAD
git log origin/main..HEAD 2>/dev/null
git status --short
```

## Step 5: Write workstream-scoped resume prompt

Target: `memory/prompt_{WORKSTREAM_SLUG}.md` (in the Claude memory directory for this project).

Use the standard resume prompt template with VERIFIED/ASSUMED/DO-FIRST sections.

## Step 6: Update MEMORY.md surgically

Edit exactly ONE line: the pointer to the resume prompt. Multi-line edits FORBIDDEN.

## Step 7: Final report

```
WIND-DOWN COMPLETE

Workstream: {slug}
Resume prompt: memory/prompt_{slug}.md

VERIFIED:
- {bullet list}

NEXT SESSION:
1. cd {project-directory}
2. claude
3. /resume

Safe to exit now.
```

Then STOP. Do not auto-exit, do not run /ship.
