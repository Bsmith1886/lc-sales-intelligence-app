Resume from where the last session left off.

1. Read the most recent `prompt_*.md` from memory directory
2. Run `/sitrep` to verify current state matches the prompt's assumptions
3. If state has drifted (new commits, open PRs, dirty tree), note discrepancies
4. Present the recommended next action from the prompt
5. Wait for operator confirmation before executing

Do NOT start implementing until the operator confirms the action.

## Context Management (active all session)

After resume completes, maintain awareness of context growth throughout the session.
Proactively recommend `/wind-down` when observable signals fire (see CLAUDE.md Context discipline).
Do not wait for the operator to notice. This is YOUR responsibility.
