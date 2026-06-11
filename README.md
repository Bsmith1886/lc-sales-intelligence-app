# lc-sales-intelligence-app

Centralizes LED Connection sales call transcripts from Plaud devices into a structured Notion database, auto-enriches each record with NetSuite deal data, and provides a manager coaching workflow powered by the Claude API.

**Azure DevOps:** AD-505 | **Stack:** Angular 21 / ASP.NET Core 10 / YARP / .NET Aspire

---

## Project Name & Purpose

The Sales Intelligence App solves a critical gap in how LED Connection captures and uses sales call data. Currently, recordings exist in silos on individual Plaud devices — unanalyzed and unconnected to deal data. This app creates a living, searchable repository of sales conversations by:

- Pulling transcripts from each rep's Plaud device via Claude's Plaud MCP connector
- Storing them in a shared Notion Transcripts database with full metadata
- Enriching each record with NetSuite opportunity data (deal stage, company, contact, rep name)
- Enabling AI-powered coaching summaries and playbook development for managers

In v1, sync is manual — reps run a Claude prompt daily to push their recordings to Notion. Automation (Make.com/n8n) is planned for Phase 7 after the core stack is stable.

---

## Critical Decisions

| Decision | Reason |
|----------|--------|
| Manual sync in v1 (reps run a Claude prompt) | Automation adds Make.com/n8n dependency with no benefit until the rest of the stack is stable. Manual flow works and ships sooner. |
| Notion internal integration token (not per-user OAuth) | App reads/writes a single shared database on behalf of the org. Internal token is simpler and sufficient. |
| Synced By field uses Notion `created_by` system property | Each rep connects their own Notion account to Claude via OAuth. Pages created by Claude are attributed to the rep's Notion user automatically — no prompt input needed. |
| `Audience` field (Internal/External) inferred by Claude at sync time | Claude reads the transcript and title to classify; more reliable than asking reps to tag manually. Leave blank when uncertain rather than guess wrong. |
| Transcript-only sync — no AI summary, no audio | Summaries are generated on-demand by the coaching workflow. Pulling them at sync time wastes tokens and adds noise to the database. |
| Skip recordings with no transcript entirely — no empty records | Empty records pollute the database and confuse enrichment. A recording with no transcript has no value until it is manually transcribed first. |
| Duplicate detection by Recording ID (Plaud file ID) | Name-based dedup breaks when a recording is renamed after the first sync. File ID is stable and unique. |
| NetSuite OAuth 2.0 M2M (Client Credentials) — not TBA | M2M does not consume a NetSuite user license. App authenticates as itself, not as a named employee. |
| Self-signed RSA 4096-bit certificate for NetSuite M2M | NetSuite requires a certificate to verify the JWT assertion. 2048-bit was rejected by the sandbox — 4096-bit accepted. |
| TestContainers for all integration and acceptance tests | Past incident where mocked tests passed but a prod migration failed. Real containers catch schema issues that mocks don't. |

---

## Deployment Instructions

> Pipeline variable groups must exist before the first run: `sales-intelligence-dev`, `sales-intelligence-staging`, `sales-intelligence-prod`

### Azure DevOps Pipeline (automated)

1. Push to `main` — pipeline triggers automatically
2. Steps: restore → `dotnet test` (fails pipeline on failure) → `ng build --configuration production` → Replace Tokens → publish artifacts
3. Deploy to **dev** automatically
4. Deploy to **staging** automatically after dev succeeds
5. Deploy to **prod** requires manual approval gate (Project Settings → Environments → `sales-intelligence-prod`)

### First-time Azure setup (before first deploy)

- Create App Services: `sales-intelligence-api-{env}` and `sales-intelligence-proxy-{env}` for dev, staging, prod
- Create Azure SQL database per environment
- Create Azure Key Vault per environment and populate all secrets (see Dependencies)
- Confirm `lc-azure-service-connection` service connection exists in AI Tools project settings
- Install Replace Tokens extension in LEDConnection Azure DevOps org

### Production NetSuite setup

Repeat the sandbox certificate and integration record steps against production NetSuite before deploying to prod. See DECISIONS.md — NetSuite Setup section.

---

## Setup / Getting Started

**Prerequisites:** .NET 10 SDK, Node.js 22+, Docker Desktop (for TestContainers), Angular CLI 21

### 1. Clone and restore

```powershell
git clone https://LEDConnection@dev.azure.com/LEDConnection/AI%20Tools/_git/lc-sales-intelligence-app
cd lc-sales-intelligence-app
dotnet restore
cd lc-workspace && npm install && cd ..
```

### 2. Set local secrets

All secrets live in .NET User Secrets under `LC.Host.Api` (UserSecretsId: `438c7130-870a-4d95-9bd4-aca146573433`). See DECISIONS.md for the full list of keys.

```powershell
cd LC/Host/LC.Host.Api
dotnet user-secrets set "NetSuiteConfiguration:ClientId" "<value>"
dotnet user-secrets set "NetSuiteConfiguration:ClientSecret" "<value>"
dotnet user-secrets set "NetSuiteConfiguration:AccountId" "<value>"
dotnet user-secrets set "NotionConfiguration:ApiToken" "<value>"
dotnet user-secrets set "NotionConfiguration:TranscriptsDatabaseId" "<value>"
dotnet user-secrets set "AzureAd:TenantId" "<value>"
dotnet user-secrets set "AzureAd:ClientId" "<value>"
```

### 3. Run locally

```powershell
cd LC/Host/LC.Host.Orchestrator
dotnet run
```

Aspire dashboard: `https://localhost:17042`
App (via proxy): `https://localhost:7100`

Always develop through the proxy — never connect directly to the API port.

---

## Dependencies

| Dependency | Purpose | Notes |
|------------|---------|-------|
| Notion (Operations workspace) | Stores all transcript records | Internal integration token required; Transcripts database must be shared with the integration |
| NetSuite (sandbox + prod) | Source of deal data for enrichment | OAuth 2.0 M2M; certificate in Windows cert store (dev) / Azure Key Vault (prod) |
| Microsoft Entra ID | User authentication | App Registration required; Tenant ID + Client ID in `environment.ts` |
| Azure SQL | App database | EF Core 10 code-first; one DbContext in `LC.Access.Common` |
| Azure Blob Storage | File storage (future phases) | Not used in v1 |
| Azure Key Vault | Secrets in staging/prod | All `#{...}#` tokens populated from pipeline variable groups |
| Plaud MCP (Claude connector) | Transcript source for manual sync | Each rep connects their own Plaud account via Claude Settings → Connectors |
| Claude API | Coaching summaries (Phase 5) | Anthropic API key stored in Key Vault |
| .NET Aspire | Local orchestration | Dashboard at `https://localhost:17042` |
