<!-- TEMPLATE PLACEHOLDERS:
  dotnet test LC/LC.sln: fast test/validate command (e.g., "dotnet test").
  cd lc-workspace && npx ng build app-sales-intelligence-app --configuration production: slow build command — run as dedicated Bash call
                     (e.g., "cd lc-workspace && npx ng build app-proposal-generator --configuration production").
  dotnet test LC/LC.sln: post-build verification (usually same as TEST_COMMAND).
  docs/PITFALLS.md: path to pitfalls doc.
  CHANGELOG.md, DECISIONS.md: comma-separated append-only files.
-->
Comprehensive code review of a PR with conflict resolution, full verification, adversarial and baseline Codex reviews, and automated fixes. If $ARGUMENTS contains a PR number, review that PR. If empty, review the current branch's open PR.

This is the quality gate between agent implementation and human merge. **All stages are non-optional.**

## Hard rules

- Never merge. Human merges.
- Never skip a stage.
- Never `--no-verify`. Never disable a failing check to make it pass.
- Force-push only to feature branches and only with `--force-with-lease`. Never to main.
- Max 3 fix-loop iterations through Stages 5-7. On the third consecutive failure, stop and report the full diagnosis.
- If adversarial review reveals the design is fundamentally wrong, stop and report rather than silently pivoting.

## Stage 0: Setup worktree

Review work MUST happen in an isolated worktree.

```bash
gh pr view $ARGUMENTS --json headRefName --jq .headRefName
```

Enter a worktree: `EnterWorktree` with name `review-pr-{number}`.

Inside the worktree:

```bash
git fetch origin <branch-name>
git checkout <branch-name>
git fetch origin main
git rebase origin/main
```

If rebase conflicts arise:
1. For **append-only files** (CHANGELOG.md, DECISIONS.md): keep BOTH entries in chronological order
2. For **code conflicts**: read both sides, understand intent, merge correctly
3. After each resolution: `git add <file> && git rebase --continue`
4. After full rebase: `git push --force-with-lease origin HEAD`

If rebase succeeds cleanly, proceed without force-push.

## Stage 1: Claude review (enumerate only, do not fix yet)

### Verification gate

Run the full quality gate BEFORE reviewing code. Failures here are MUST FIX.

```bash
dotnet test LC/LC.sln
```
```bash
cd lc-workspace && npx ng build app-sales-intelligence-app --configuration production
```
```bash
dotnet test LC/LC.sln
```

### CI failure inspection

```bash
gh pr checks $ARGUMENTS
gh run view <run-id> --log-failed
```

Before classifying a failing check as MUST FIX, confirm the failure does not already exist on `origin/main`. Pre-existing failures are out of scope; record as CONSIDER with a note to file a separate issue.

### Diff review

```bash
gh pr diff $ARGUMENTS
gh pr view --json number,title,body
```

Read the PR description and all changed files in full (not just the diff).

### Checklist

#### A. Active Invariants (from CLAUDE.md)
Read CLAUDE.md "Active Invariants" section and verify each one against the diff.

#### B. Code Quality
- No security vulnerabilities (injection, XSS, OWASP top 10)
- No over-engineering or unnecessary abstractions
- Error handling only at system boundaries
- No dead code, unused imports, commented-out blocks
- Consistent style with surrounding code
- IDesign layering respected — calls flow downward only (Controller → Manager → Accessor)
- No domain entities crossing layer boundaries

#### C. Known Pitfalls
- Read `docs/PITFALLS.md` and review every item against the diff

### Classify Stage 1 findings

- **MUST FIX**: invariant violation, security issue, broken functionality, failing CI
- **SHOULD FIX**: code quality, missed edge cases, style inconsistency
- **CONSIDER**: suggestions, minor improvements

Record findings. Do NOT fix yet.

## Resolving `codex-companion.mjs` (shared by Stages 2 and 3)

Stages 2 and 3 shell out to `codex-companion.mjs`. Resolve the script path once, in order of preference:

1. `${CLAUDE_PLUGIN_ROOT}/scripts/codex-companion.mjs`: set when running inside the Claude Code plugin runtime.
2. `${CODEX_COMPANION_PATH}`: operator override, set in shell profile to run outside the plugin.
3. First match of `ls -1 ${HOME}/.claude-lx/plugins/cache/openai-codex/codex/*/scripts/codex-companion.mjs | sort -V | tail -n1`: picks the newest installed version from the plugin cache.

If none resolve, stop and report. Do not skip Stages 2 or 3. Ask the operator to install the openai-codex plugin or export `CODEX_COMPANION_PATH`.

Capture the resolved path as `$CODEX` for the stages below.

### Model resolution (used by API direct fallback)

```bash
REVIEW_MODEL="${OPENAI_REVIEW_MODEL:-}"
if [ -z "$REVIEW_MODEL" ]; then
  _CACHE="/tmp/openai-review-model-cache"
  if [ -f "$_CACHE" ] && [ -n "$(find "$_CACHE" -mmin -1440 2>/dev/null)" ]; then
    REVIEW_MODEL=$(cat "$_CACHE")
  elif [ -n "${OPENAI_API_KEY:-}" ]; then
    REVIEW_MODEL=$(curl -sf https://api.openai.com/v1/models \
      -H "Authorization: Bearer $OPENAI_API_KEY" \
      | python3 -c "
import sys, json
data = json.load(sys.stdin).get('data', [])
skip = {'mini','preview','audio','realtime','embed','tts','whisper','dall','search'}
flagships = [m for m in data if m['id'].startswith('gpt-')
             and not any(s in m['id'] for s in skip)]
print(max(flagships, key=lambda m: m.get('created',0))['id'] if flagships else '')
" 2>/dev/null) || true
    [ -n "$REVIEW_MODEL" ] && echo "$REVIEW_MODEL" > "$_CACHE"
  fi
  REVIEW_MODEL="${REVIEW_MODEL:-gpt-5.5}"
fi
echo "Review model: $REVIEW_MODEL"
```

## Stage 2: Codex adversarial review

Purpose: challenge the implementation approach, design choices, tradeoffs, assumptions. Ask: is this the right approach? What assumptions does it depend on? Where does the design fail under real-world conditions?

```bash
node "$CODEX" adversarial-review --wait --base origin/main --scope branch
```

Capture stdout verbatim. Parse findings into the MUST / SHOULD / CONSIDER schema. If the command exits non-zero or times out, fall back to the API direct path below.

## Stage 3: Codex normal review

Purpose: baseline pass for implementation defects, style, common pitfalls that Claude misses due to familiarity bias.

```bash
node "$CODEX" review --wait --base origin/main --scope branch
```

Capture stdout verbatim. Parse findings. Same failure policy as Stage 2.

### API direct fallback (mandatory if Codex CLI fails)

If either Stage 2 or 3 fails (quota, timeout, CLI not installed), fall back to the OpenAI API directly using `$REVIEW_MODEL`. The review MUST happen from a different model family. If all paths fail, do NOT mark the PR ready. Report: "Second-opinion review: BLOCKED."

## Stage 4: Consolidate findings

Merge all stages into a single findings list. De-duplicate overlapping findings. Present as one coherent set, tagged by originating stage.

## Stage 5: Implement fixes

Fix every MUST FIX and SHOULD FIX. Ask operator about CONSIDER items.

Commit with conventional format: `fix(review): address <stage>/<category> findings`.

## Stage 6: Re-verify

Rerun the exact Stage 1 verification gate. Loop back to Stage 5 if anything fails. Hard cap: 3 iterations.

```bash
dotnet test LC/LC.sln
```
```bash
cd lc-workspace && npx ng build app-sales-intelligence-app --configuration production
```
```bash
dotnet test LC/LC.sln
```

## Stage 7: Push and wait for CI

```bash
git push --force-with-lease origin HEAD
gh pr checks $ARGUMENTS --watch
```

If CI fails after push, loop back to Stage 5. Same 3-iteration cap applies across all fix loops.

## Stage 8: Quality gate (if configured)

If the project provides a quality gate script, run it and interpret the verdict:
- **pass**: proceed to report.
- **warn**: proceed. Include warnings in report under "Quality Gate: WARN".
- **block**: STOP. Report blocking reasons to operator.

If no quality gate script is configured, skip this stage.

## Stage 9: Report

Required fields:

- **Conflicts**: found and resolved, or "none"
- **Verification**: gate pass/fail before and after fixes
- **Stage 1 (Claude) findings**: count by severity + highlights
- **Stage 2 (Codex adversarial) findings**: count by severity + highlights
- **Stage 3 (Codex normal) findings**: count by severity + highlights
- **Fixes applied**: file + line + rationale for each
- **Final CI status**: green/red + run URL
- **PR ready for human merge**: YES / NO
- **Remaining CONSIDER items**: surfaced to operator

## Stage 10: Cleanup

Exit the worktree: `ExitWorktree` with action `remove`.
