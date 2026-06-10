# lc-tool-standards

Canonical templates and shared pipeline definitions for all LED Connection AI Tools in the [Azure DevOps AI Tools project](https://dev.azure.com/LEDConnection/AI%20Tools).

## What's in here

| Path | Purpose |
|------|---------|
| `CLAUDE.md` | Architecture standard template. Copy into every new tool repo and fill in the Tool Identity section at the top. |
| `pipeline-templates/build-and-deploy.yml` | Shared Azure Pipelines YAML template. Reference from each tool's `azure-pipelines.yml`. |
| `.claude/hooks/` | Session hooks: commit convention, context tracking, session orientation, stop checklist. Copy to each tool's `.claude/hooks/`. |
| `.claude/commands/` | Slash commands: `/sitrep`, `/ship`, `/wind-down`, `/verify`, `/review-pr`, and more. Copy to each tool's `.claude/commands/`. |
| `.claude/settings.json` | Claude Code hooks configuration. Copy to each tool's `.claude/`. |

---

## How to start a new tool

### 1. Create the Azure DevOps repo

In the [AI Tools project](https://dev.azure.com/LEDConnection/AI%20Tools), create a new empty repo named `lc-{tool-name}` (e.g., `lc-proposal-generator`).

### 2. Bootstrap locally from this repo

```powershell
$toolName = "lc-your-tool-name"
$toolDir = "C:\Users\billiejos\Projects\LED AI Tools\$toolName"

git clone https://LEDConnection@dev.azure.com/LEDConnection/AI%20Tools/_git/lc-tool-standards $toolDir
cd $toolDir
git remote set-url origin https://LEDConnection@dev.azure.com/LEDConnection/AI%20Tools/_git/$toolName
git push -u origin main
```

### 3. Fill in CLAUDE.md Tool Identity

Open `CLAUDE.md` and complete the four fields at the top:

```
Tool Name:       [e.g., Proposal Generator]
Azure DevOps ID: [e.g., AD-500]
Repository Name: [e.g., lc-proposal-generator]
Purpose:         [one paragraph]
```

### 4. Replace all `{{PLACEHOLDER}}` tokens

Search `.claude/` for `{{...}}` tokens and replace them:

| Token | Replace with |
|-------|-------------|
| `{{TOOL_NAME}}` | Human-readable tool name (e.g., `Proposal Generator`) |
| `{{TEST_COMMAND}}` | Fast test command (e.g., `dotnet test`) |
| `{{BUILD_COMMAND}}` | Slow Angular build (e.g., `cd lc-workspace && npx ng build app-proposal-generator --configuration production`) |
| `{{POST_BUILD_COMMAND}}` | Post-build test command (usually same as `{{TEST_COMMAND}}`) |
| `{{PIPELINE_SENTINEL}}` | File that only exists after the build runs (e.g., `lc-workspace/dist/app-proposal-generator/index.html`) |
| `{{ANGULAR_APP_NAME}}` | Angular app name in `lc-workspace` (e.g., `app-proposal-generator`) |

### 5. Create `azure-pipelines.yml` in the tool repo root

```yaml
trigger:
  branches:
    include:
      - main
      - AD-*

resources:
  repositories:
    - repository: standards
      type: git
      name: AI Tools/lc-tool-standards

stages:
  - template: pipeline-templates/build-and-deploy.yml@standards
    parameters:
      toolName: your-tool-name
      azureServiceConnection: lc-azure-service-connection
```

### 6. Azure DevOps setup (before first pipeline run)

- **Variable groups**: create `{toolName}-dev`, `{toolName}-staging`, `{toolName}-prod` in the AI Tools project; populate all `#{...}#` tokens from `appsettings.json`
- **App Services**: create `{toolName}-api-{env}` and `{toolName}-proxy-{env}` in Azure (dev/staging/prod)
- **Service connection**: confirm `lc-azure-service-connection` exists in AI Tools project settings, or create it
- **Replace Tokens extension**: install in the LEDConnection Azure DevOps org — [marketplace.visualstudio.com/items?itemName=qetza.replacetokens](https://marketplace.visualstudio.com/items?itemName=qetza.replacetokens)
- **Prod approval gate**: Project Settings → Environments → `{toolName}-prod` → Approvals and Checks → Approvals

### 7. Complete the checklist in CLAUDE.md

Work through every item in the "Checklist — Starting a New Tool" section before first deploy.

---

## Critical Decisions

| Decision | Reason |
|----------|--------|
| Standards repo, copied CLAUDE.md per tool | Each tool's CLAUDE.md is customized (Tool Identity + project-specific invariants). Live submodule adds friction — `git submodule update` is easily forgotten. |
| Pipeline templates via `stages: template:` | Single source for build/deploy logic. Changes propagate automatically on next pipeline run. |
| `#{...}#` tokens replaced by Replace Tokens extension | Variable group values stay in Azure DevOps, never in source. Pattern is already established by the CLAUDE.md standard. |
| Hooks and commands copied per tool | Each tool needs minor adaptations (`{{TEST_COMMAND}}`, sentinel path). A remote-fetch pattern for hooks adds complexity with no benefit. |

---

## Keeping tools in sync with the standard

When the framework evolves (new Angular version, new layer convention, etc.):

1. Update `CLAUDE.md` and any affected templates in this repo
2. Open a PR in each active tool repo propagating the relevant changes
3. There is no automated sync — changes are applied by hand per tool

This is intentional. Each tool's CLAUDE.md will diverge in the "Current State" and "Active Invariants" sections; a forced overwrite would destroy that content.
