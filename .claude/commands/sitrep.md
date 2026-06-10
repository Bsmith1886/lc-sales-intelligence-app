Quick project status orientation. Runs automatically at session start. Keep output compact.

## Sync Local Repo

Before gathering data, pull latest from origin to keep the local repo in sync:
```bash
git pull --ff-only origin main 2>/dev/null || true
```
If on a feature branch, skip the pull (don't mess with in-progress work).

## Gather Data

Run these in parallel:
```bash
git branch --show-current
git status --short | head -10
git worktree list
git branch --merged main | grep -v '\*\|main'
git log --oneline -3
gh pr list --limit 10
gh issue list --limit 10 --state open
```

Also read memory (MEMORY.md) for active workstream status.

## Read the most recent workstream resume prompt (REQUIRED)

`/wind-down` writes workstream-scoped resume files at `memory/prompt_{slug}.md`.
MEMORY.md only contains one-line pointers. The VERIFIED/ASSUMED/DO-FIRST handoff lives in the file itself.

Find the most recently modified `prompt_*.md` in the project's Claude memory directory. Read it. If its `written_at` frontmatter is within the last 72h, its `DO FIRST on resume` block is the authoritative next action. Surface it in the `Next:` line of the report.

## Report Worktree State (DO NOT auto-clean)

If worktrees or merged branches exist, **report them but do NOT delete**.
Other CLI sessions may be actively using them. Only the owning session or
the operator should clean up worktrees.

## Report Format (keep under 10 lines)

```
Branch: {branch} | {clean/dirty} | {worktrees: N active}
Last: {hash} {message} ({time})
PRs: {count} open | Issues: {count} open
Next: {single highest-priority action}
```

Expand with PR/issue details only if there are open items.

Be direct. No preamble.
