# Sales Intelligence App — Decisions & Setup Log

Running log of architectural decisions, setup choices, and configuration details.
Update this file whenever a meaningful decision is made or a setup step is completed.

---

## Pre-Work Status (as of 2026-06-10)

| Task | Status | Notes |
|------|--------|-------|
| Azure DevOps Feature created | ✅ Done | Feature: "Sales Intelligence App" under AI Tools project |
| Azure DevOps repo created | ✅ Done | `lc-sales-intelligence-app` |
| GitHub repo created | ✅ Done | `lc-sales-intelligence-app` — both remotes set (origin = ADO, github = GitHub) |
| CLAUDE.md filled in | ✅ Done | Tool Identity section complete (AD-505) |
| .claude/ hooks and commands filled in | ✅ Done | All `{{PLACEHOLDER}}` tokens replaced |
| Azure App Registration created | ✅ Done | See Auth Decisions below |
| Notion internal integration created | ✅ Done | See Notion Setup below |
| NetSuite Integration Record created (sandbox) | ✅ Done | See NetSuite Setup below |
| NetSuite Role created (sandbox) | ✅ Done | See NetSuite Setup below |
| NetSuite M2M certificate mapped (sandbox) | ✅ Done | See NetSuite Setup below |
| Add secrets to .NET User Secrets | ✅ Done | All 8 keys set in LC.Host.Api (sandbox values). UserSecretsId: 438c7130-870a-4d95-9bd4-aca146573433 |
| Pipeline variable groups | ⏳ Deferred | Create before first Azure deployment |
| NetSuite Production setup | ⏳ Deferred | Repeat sandbox steps in prod when ready to deploy |

---

## Architecture Decisions

### Stack
**Decision:** Angular 21 + ASP.NET Core 10 + YARP proxy + .NET Aspire  
**Why:** LED Connection standard stack (same as Audit App). Consistency reduces ramp-up time and reuses shared infrastructure patterns.

### IDesign Layering
**Decision:** Controller → Manager → Accessor. No skipping, no sideways calls.  
**Why:** Enforced boundary makes each layer independently testable. All NetSuite and Notion HTTP calls live in the Accessor layer only.

### Authentication (Users)
**Decision:** Microsoft Entra ID / MSAL Angular (PKCE redirect flow) + `Microsoft.Identity.Web` JWT validation on the backend.  
**Why:** LED Connection employees already have Microsoft accounts. No separate identity system needed. Same pattern as other LED AI Tools.

### Notion Access Pattern (v1)
**Decision:** Internal Integration token (server-to-server). No OAuth per-user flow.  
**Why:** The app reads/writes a single shared Transcripts database on behalf of the organization, not on behalf of individual users. An internal integration token is simpler and sufficient.

### Transcript Sync (v1)
**Decision:** Manual sync — reps use Claude MCP + Plaud skill to push recordings to Notion themselves. No automation in v1.  
**Why:** Automation adds complexity and a Make.com/n8n dependency. Manual flow is already working. Automate in Phase 7 after everything else is stable.

### NetSuite Authentication
**Decision:** OAuth 2.0 Machine-to-Machine (Client Credentials grant), NOT TBA (Token-Based Authentication).  
**Why:** M2M does not consume a NetSuite user license. No user needs to log in. The integration runs as the app identity, not as a named employee.  
**How it works:** The app signs a JWT with its private key → sends to NetSuite token endpoint → receives an access token → uses it for all API calls. Token is cached and refreshed on 401.

### Certificate for NetSuite M2M
**Decision:** Self-signed RSA 4096-bit certificate generated with PowerShell `New-SelfSignedCertificate`.  
**Why:** NetSuite requires a certificate for M2M to verify the JWT signature. Self-signed is sufficient — NetSuite only uses it to validate the JWT assertion, not as a TLS trust anchor.  
**Note on key size:** 2048-bit was rejected by NetSuite sandbox with "invalid bit length". 4096-bit was accepted.  
**Local dev:** Certificate lives in `Cert:\CurrentUser\My`. App loads it by thumbprint at runtime.  
**Production:** Export as PFX → store in Azure Key Vault → load from Key Vault at startup.

### Testing — No Database Mocks
**Decision:** Integration and acceptance tests use TestContainers against real SQL. No in-memory EF Core.  
**Why:** Past incident where mocked tests passed but a prod migration failed. Real containers catch schema issues mocks don't.

### NetSuite Environment Strategy
**Decision:** All NetSuite write-back development and testing happens against the sandbox. Production NetSuite only touched after sandbox validation.  
**Why:** Prevents accidental data corruption in production CRM records.

### Notion Environment Strategy
**Decision:** Development and testing use real Notion data (read-only analysis). No separate Notion sandbox.  
**Why:** Notion doesn't have a sandbox concept. The app only reads/analyzes transcripts in v1 — no risk of corrupting data. When write-back is added (coaching notes), evaluate creating a separate test database.

---

## NetSuite Setup (Sandbox)

| Item | Value |
|------|-------|
| Integration Record Name | Sales Intelligence App - OAuth |
| Grant Type | OAuth 2.0 Client Credentials (Machine to Machine) |
| Scope | REST Web Services |
| Role Name | LED Connection - Sales Intelligence Read Only |
| M2M Entity | Billie Jo Smith |
| Certificate Thumbprint | `243090B361AD78CDBD473744595D2A4E382AF56E` |
| Certificate Expiry | 2028-06-10 |
| Certificate Location (local) | `Cert:\CurrentUser\My` (Windows Certificate Store) |
| Certificate File (public key only) | `SalesIntelligenceApp.pem` in project root |

**Client ID and Client Secret:** Store in .NET User Secrets as:
```
NetSuiteConfiguration:ClientId
NetSuiteConfiguration:ClientSecret
NetSuiteConfiguration:AccountId   (format: 1234567-SB1 for sandbox)
```

**Token Endpoint Pattern:**
```
https://{accountId}.suitetalk.api.netsuite.com/services/rest/auth/oauth2/v1/token
```

**How the M2M flow works (for the next developer):**
1. App builds a JWT: issuer = ClientId, subject = ClientId, audience = token endpoint URL, expiry = now + 60s
2. App signs the JWT with the private key from the certificate in `Cert:\CurrentUser\My` (thumbprint above)
3. App POSTs to token endpoint: `grant_type=client_credentials`, `client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer`, `client_assertion={signed JWT}`
4. NetSuite verifies signature using the uploaded public certificate → returns access token
5. App caches token; retries with fresh token on 401

---

## Notion Setup

| Item | Value |
|------|-------|
| Workspace | Operations |
| Teamspace | Technology (inside Operations workspace) |
| Integration Name | Sales Intelligence App (internal integration) |
| Integration Type | Internal (Access Token — not OAuth) |
| Database Shared | Transcripts |

**API Token:** Store in .NET User Secrets as:
```
NotionConfiguration:ApiToken
NotionConfiguration:TranscriptsDatabaseId
```

**Base URL:** `https://api.notion.com/v1`  
**Required headers:** `Authorization: Bearer {token}`, `Notion-Version: 2022-06-28`

---

## Entra ID / App Registration

| Item | Value |
|------|-------|
| App Name | (set during App Registration creation) |
| Auth Flow | MSAL Angular — redirect flow (PKCE, no client secret) |
| Backend Validation | `Microsoft.Identity.Web` JWT bearer |

**Config values:** Store in `environment.ts` (populated by pipeline — not secrets):
```
tenantId
clientId
```

---

## Repository

| Item | Value |
|------|-------|
| Local path | `C:\Users\billiejos\Projects\LED AI Tools\lc-sales-intelligence-app\` |
| Azure DevOps remote | `origin` |
| GitHub remote | `github` |
| Default branch | `main` |
| Branch naming | `AD-{ticket}-short-description` |
| Commit format | `AD-{ticket}: Description` |

**Both remotes must be pushed on every commit:**
```
git push origin main
git push github main
```

---

## Notion Database Schema Decisions (2026-06-11)

### Synced By — `created_by` system property (not a text field)
**Decision:** `Synced By` uses Notion's native `created_by` property type, not a plain text field.  
**Why:** Each rep connects their own Notion account to Claude via OAuth. When Claude creates a page via the rep's connector, Notion automatically attributes the page to that user. No prompt input, no risk of wrong or missing values.  
**Important:** This is distinct from `Rep Name` (the assigned sales rep from NetSuite). A manager or admin could sync on behalf of a rep — `Synced By` captures who ran the sync, not who owns the deal.

### Audience — inferred at sync time by Claude
**Decision:** `Audience` (Internal/External) is a Select field populated by Claude at sync time by reading the recording title and transcript.  
**Why:** Manual tagging by reps is unreliable. Claude can infer from context — "Weekly Meeting" in the title, all-internal speakers, internal project topics → Internal. A customer name, prospect company, or external party present → External.  
**Rule:** When uncertain, Claude leaves the field blank. A wrong value is worse than an empty one.

### Transcript-only sync — no AI summary, no audio
**Decision:** The sync prompt instructs Claude to fetch only the transcript (`get_transcript`). It must not call `get_note` (AI summary) or retrieve the audio/presigned URL.  
**Why:** Summaries are generated on-demand by the coaching workflow (Phase 5). Pulling them at sync time wastes tokens, adds noise, and may produce stale summaries before enrichment runs.

### Skip recordings with no transcript — no empty records
**Decision:** If `get_transcript` returns nothing, Claude skips the recording entirely. No Notion record is created.  
**Why:** Empty records pollute the database, break filters, and confuse enrichment. A recording has no value in this system until it has a transcript. Reps must manually trigger Plaud transcription for sub-200-word recordings before running the sync.

### Duplicate detection by Recording ID
**Decision:** Claude checks for an existing Notion entry matching the Plaud `Recording ID` field before creating a new record. Name-based matching is not used.  
**Why:** Recording names can be renamed in Plaud after the first sync. File ID is stable and unique — name-based dedup creates false duplicates or misses real ones.

---

## What's Next (Phase 2 — App Foundation)

Credentials are set in User Secrets. Next steps:

1. Scaffold the Aspire solution (copy Host projects from Audit App): `LC.Host.Orchestrator`, `LC.Host.Api`, `LC.Host.Proxy`, `LC.Host.Common`
2. Build `LC.Access.Notion` — `INotionTranscriptAccessor` / `NotionTranscriptAccessor`
3. Wire up MSAL Angular in the frontend
4. Build `transcript-list` and `transcript-detail` components

See the full build plan in Notion: Build Plan — Sales Intelligence Pipeline (page ID: 37afea4a-4b32-8173-94e2-e00f60f77a31)
