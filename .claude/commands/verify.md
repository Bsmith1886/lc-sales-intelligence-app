<!-- TEMPLATE PLACEHOLDERS:
  dotnet test LC/LC.sln: fast test/validate command (e.g., "dotnet test").
  cd lc-workspace && npx ng build app-sales-intelligence-app --configuration production: slow Angular build command
                     (e.g., "cd lc-workspace && npx ng build app-proposal-generator --configuration production").
  300000: timeout in ms for the Angular build. Increase to 600000 for large workspaces.
  dotnet test LC/LC.sln: post-build verification (usually same as TEST_COMMAND).
-->
Run the full verification suite. Execute all steps and report results.

**EXECUTION RULES: read before running anything.**

1. **NEVER** chain `cd lc-workspace && npx ng build app-sales-intelligence-app --configuration production` into a compound `&&` command. The harness auto-backgrounds slow processes; chained commands silently stall and waste 10+ minutes. Run it as a **dedicated Bash call** with explicit `timeout: 300000`.
2. Run fast steps as one chained call.
3. Run `cd lc-workspace && npx ng build app-sales-intelligence-app --configuration production` alone, foreground, with `timeout: 300000`. If it gets backgrounded, **immediately** read the task output file the harness returns.
4. Run post-build checks as a third dedicated call after the build completes.

**Step 1: fast suite (single Bash call)**
```bash
dotnet test LC/LC.sln
```

**Step 2: Angular build (dedicated Bash call, `timeout: 300000`, never chained)**
```bash
cd lc-workspace && npx ng build app-sales-intelligence-app --configuration production
```

**Step 3: post-build verification + regressions (single Bash call)**
```bash
dotnet test LC/LC.sln
```

Report which checks passed and which failed. If all pass, say "All verification checks pass." If any fail, list the failures with details.
