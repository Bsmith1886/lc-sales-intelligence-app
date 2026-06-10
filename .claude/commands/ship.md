<!-- TEMPLATE PLACEHOLDERS:
  Sales Intelligence App: human-readable project name (e.g., "Proposal Generator").
  dotnet test LC/LC.sln: fast test command (e.g., "dotnet test").
  cd lc-workspace && npx ng build app-sales-intelligence-app --configuration production: slow build command — run as a dedicated Bash call, never chained
                     (e.g., "cd lc-workspace && npx ng build app-proposal-generator --configuration production").
  dotnet test LC/LC.sln: post-build verification (usually same as TEST_COMMAND).
  docs/PITFALLS.md: path to pitfalls doc.
-->
Run the Sales Intelligence App shipping sequence for the current feature branch. Execute ALL steps in order. Do not skip any. If any step fails, stop and report.

## 1. Full Quality Gate
```bash
dotnet test LC/LC.sln
```
```bash
cd lc-workspace && npx ng build app-sales-intelligence-app --configuration production
```
```bash
dotnet test LC/LC.sln
```

## 2. Docs Update
Update these files if any changes were made this session:
- `CLAUDE.md` "Current State" section
- `docs/PITFALLS.md` (if new lessons learned)
- `DECISIONS.md` (if architectural decisions were made)
- `CHANGELOG.md` (if user-visible changes were made)

## 3. Commit and Push
```bash
git add -A
git status
```
Commit with conventional format: `{type}({scope}): {description}`
Push: `git push origin HEAD`

## 4. Create/Update PR

If no PR exists for this branch, create one via Azure DevOps:
```bash
az repos pr create --title "{title}" --description "Closes AB#{issue}" --repository {repo-name} --project "AI Tools"
```

## 5. Report
Summarize: what was accomplished, which tests passed, what the operator should do next.
