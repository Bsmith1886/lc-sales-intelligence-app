<!-- TEMPLATE PLACEHOLDERS:
  docs/PITFALLS.md: path to the project's pitfalls/lessons doc.
-->
Universal workstream landing sequence. Run after all PRs for a wave are merged to main.

$ARGUMENTS is optional: workstream identifier (e.g., "WS10 Wave 1"). If empty, infer from recent commits and memory.

## 1. Sync & Clean

```bash
git checkout main && git pull origin main
git branch --merged main | grep -v '\*\|main' | xargs -r git branch -d
git worktree list
```
For any stale worktrees: `rm -rf <worktree-path>`, then `git worktree prune`.

Verify: `git status` is clean.

## 2. Full Quality Gate

Run `/verify`. All checks must pass. Fix any failures before proceeding.

## 3. Update Documentation

Read each file before editing:
- **CLAUDE.md "Current State"**: Update workstream entry with status, key metrics, what was delivered.
- **docs/PITFALLS.md**: Add any new lessons learned discovered during the wave.
- **CHANGELOG.md**: Append user-visible changes with date and workstream tag.
- **DECISIONS.md**: Append any architectural decisions made.

## 4. Update Memory

Read MEMORY.md (if present) for the active workstream. Mark completed waves with date, update remaining waves' status, note any new blockers.

## 5. Next Actions

Using `gh issue list` and workstream memory:
- List remaining issues in the workstream
- State which are now unblocked
- Note any blockers
- Recommend next wave composition

## 6. Commit & Push

```bash
git add CLAUDE.md CHANGELOG.md DECISIONS.md docs/PITFALLS.md
git status
```
Commit: `docs({workstream}): land {wave}, update status, docs, and memory`
```bash
git push origin main
```

## 7. Report

10 lines or less: what was landed, key metrics, what's next, any blockers.
