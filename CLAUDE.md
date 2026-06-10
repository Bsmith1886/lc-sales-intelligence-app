# LED Connection — New Tool Architecture Standard
> Copy this file as `CLAUDE.md` in each new tool repository. Fill in the `[brackets]`.
> These rules are authoritative for both developers and AI assistants. Do not introduce patterns not listed here.

---

## Tool Identity

```
Tool Name:       Sales Intelligence App
Azure DevOps ID: AD-505
Repository Name: lc-sales-intelligence-app
Purpose:         Web application that centralizes LED Connection sales call transcripts stored in Notion,
                 auto-enriches each record with deal data from NetSuite (opportunity ID, deal stage,
                 contact, rep name), and provides a manager coaching workflow powered by the Claude API.
                 Built on Angular 21 / ASP.NET Core 10 / YARP / .NET Aspire.
```

---

## 1. Architecture

All tools use the same stack as the Audit App: Angular 21 frontend, ASP.NET Core 10 backend, YARP proxy, .NET Aspire orchestration.

```
Browser (Angular 21 SPA)
    ↓
YARP Reverse Proxy   ← single entry point
    ├── /api/*       → ASP.NET Core 10 API
    ├── /blobs/*     → Azure Blob Storage
    └── /*           → Angular static assets
```

Backend follows **IDesign strict layering** — calls flow downward only, never sideways, never skip:

```
Controller  →  Manager  →  Accessor
```

---

## 2. Repository Structure

```
/
├── lc-workspace/
│   └── projects/
│       ├── app-[toolname]/
│       └── lib-utils/
├── LC/
│   ├── Access/
│   ├── Common/
│   ├── Host/
│   │   ├── LC.Host.Api/
│   │   ├── LC.Host.Proxy/
│   │   ├── LC.Host.Orchestrator/
│   │   └── LC.Host.Common/
│   ├── Manager/
│   └── Test/
├── README.md
├── .gitignore
└── CLAUDE.md
```

**Project naming:** `LC.{Layer}.{Domain}` — e.g., `LC.Access.Proposal`, `LC.Manager.Proposal`

Use the Audit App's Host projects as starting points. Do not rename them.

---

## 3. Backend Conventions

### Layer Responsibilities

| Layer | Does | Must NOT |
|-------|------|----------|
| **Controller** | Parse request, call one Manager method, return response | Contain logic, call Accessor directly |
| **Manager** | Orchestrate workflow, validate, map models | Call other Managers, write queries |
| **Accessor** | EF Core CRUD, Blob, Queue, external APIs (NetSuite) | Contain logic, call Managers |

### Naming

| Type | Pattern | Example |
|------|---------|---------|
| Interface | `I{Name}{Layer}` | `IProposalManager` |
| Implementation | `{Name}{Layer}` | `ProposalManager` |
| Domain entity | `{Name}` in `Domain/` | `Proposal` |
| Access model | `{Name}AccessModel` | `ProposalAccessModel` |
| Manager model | `{Name}Model` | `ProposalModel` |
| API model | `{Name}ApiModel` | `ProposalApiModel` |
| Request | `{Action}{Resource}{Layer}Request` | `CreateProposalAccessRequest` |

### Model Isolation

- Domain entities never leave the Access layer — map to `AccessModel` immediately
- `AccessModels` go to Manager only; `ApiModels` are the only models that cross the HTTP boundary

### Service Registration

Each layer exposes one static method. All services registered as `Scoped`. Call from `Program.cs`.

```csharp
// LC.Manager.Proposal/ServiceInjection.cs
public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IProposalManager, ProposalManager>();
        services.AddScoped<IProposalValidator, ProposalValidator>();
    }
}
```

### Configuration

Never read `IConfiguration` directly — use strongly-typed options. `#{...}#` tokens are injected by the pipeline at build time.

```json
{
  "ConnectionStrings": {
    "SqlAzure": "#{SqlConnectionString}#",
    "AzureStorage": "#{AzureStorageConnectionString}#"
  },
  "NetSuiteConfiguration": {
    "AccountId": "#{NetSuiteAccountId}#",
    "ConsumerKey": "#{NetSuiteConsumerKey}#",
    "ConsumerSecret": "#{NetSuiteConsumerSecret}#",
    "TokenId": "#{NetSuiteTokenId}#",
    "TokenSecret": "#{NetSuiteTokenSecret}#",
    "BaseUrl": "#{NetSuiteBaseUrl}#"
  },
  "VersionData": {
    "Version": "#{VersionNumber}#",
    "CommitHash": "#{VersionCommitHash}#",
    "VersionDate": "#{VersionDate}#",
    "BranchName": "#{BranchName}#"
  }
}
```

Dev secrets via .NET User Secrets. Production secrets via Azure Key Vault. Never commit either.

### Exception Handling

Copy `GlobalExceptionHandler.cs` from the Audit App. All unhandled exceptions return RFC 7807 Problem Details. Never swallow exceptions silently.

---

## 4. Authentication & Authorization

| Layer | Provider | Method |
|-------|---------|--------|
| Frontend | Microsoft Entra ID | MSAL Angular — redirect flow |
| Backend | Microsoft Entra ID | `Microsoft.Identity.Web` JWT |
| Audit App | ASP.NET Identity | Custom JWT — **do not mix with the above** |

Users log in with their LED Connection Microsoft account. The same identity works across all tools.

**Backend setup:**
```csharp
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
```

**Frontend setup:**
```bash
npm install @azure/msal-angular @azure/msal-browser
```

```typescript
// app.config.ts
providers: [
  provideHttpClient(withInterceptors([msalInterceptor])),
  provideMsal({ interactionType: InteractionType.Redirect, authRequest: { scopes: ['user.read'] } }),
]
```

Tenant ID and Client ID come from `environment.ts`, populated by the pipeline. Define app roles in the Azure App Registration; validate with `[Authorize(Roles = "...")]`.

---

## 5. Data Access

### Database

- EF Core 10, code-first, SQL Server provider
- One `DbContext` per tool in `LC.Access.Common/` — never expose outside Access layer

```bash
dotnet ef migrations add MigrationName --project Access/LC.Access.Common --startup-project Host/LC.Host.Api
```

### NetSuite Integration

Access via Accessor layer only (`LC.Access.NetSuite/`). Use `IHttpClientFactory`. Credentials from `IOptions<NetSuiteConfiguration>`. Map all NetSuite responses to `AccessModels` before returning.

### Audit App Integration

Use the Audit App's REST API only — never connect to its database directly. Use a named `IHttpClientFactory` client. Base URL from `AuditAppConfiguration:BaseUrl`. Propagate auth tokens via a `DelegatingHandler`.

### Azure Storage

- **Blobs:** `BlobServiceClient` via DI; SAS URIs with 6-month read-only expiry
- **Queues:** Generic `QueueAccess<T> where T : BaseQueueMessage`; queue name from `MessageType` enum

---

## 6. Frontend Conventions

### Non-Negotiable Technology Choices

| Concern | Technology |
|---------|-----------|
| Framework | Angular 21 |
| Components | Standalone only — no NgModules |
| Change detection | `OnPush` everywhere |
| State | NgRx Signals Store only |
| CSS | Tailwind CSS v4 |
| Forms | Reactive Forms only |
| HTTP | Relative URLs — never hardcode domain or port |

### Feature Folder Structure

```
feature-name/
├── feature-list/          # Components
├── feature-form/
├── data-access/
│   ├── feature-api.service.ts
│   └── *.model.ts
├── store/
│   ├── feature.store.ts
│   └── feature.model.ts
└── feature.routes.ts
```

### API Service Pattern

```typescript
@Injectable({ providedIn: 'root' })
export class FeatureApiService {
  private httpClient = inject(HttpClient);
  private apiHelper = inject(ApiHelperService);
  private readonly path = 'api/features';

  get = (): Observable<ApiResult<FeatureApiResponse[]>> =>
    this.apiHelper.handleRequest(this.httpClient.get<FeatureApiResponse[]>(this.path, { observe: 'response' }));

  create = (request: CreateFeatureApiRequest): Observable<ApiResult<FeatureApiResponse>> =>
    this.apiHelper.handleRequest(this.httpClient.post<FeatureApiResponse>(this.path, request, { observe: 'response' }));
}
```

### Store Pattern

```typescript
export const FeatureStore = signalStore(
  withState<FeatureState>(initialState),
  withMethods((store, api = inject(FeatureApiService)) => ({
    async loadItems() {
      const result = await firstValueFrom(api.get());
      if (result.success) patchState(store, { items: result.data ?? [] });
    },
  }))
);
```

Provide at route level: `{ path: 'features', component: FeatureListComponent, providers: [FeatureStore] }`

---

## 7. Local Development

**Prerequisites:** .NET 10 SDK, Node.js 22+, Docker Desktop, Angular CLI 21

```bash
cd LC/Host/LC.Host.Orchestrator && dotnet run
```

Aspire dashboard: `https://localhost:17042`. Frontend always goes through the proxy (`https://localhost:7100`), never directly to the API.

---

## 8. Deployment — Azure DevOps "AI Tools" Project

### Azure Resources Per Tool

App Service (API), App Service or Static Web App (proxy/SPA), Azure SQL, Azure Functions (if needed), Blob Storage, Key Vault, Application Insights.

### Pipeline Variable Groups

Create three groups in the "AI Tools" Azure DevOps project before first deploy:
`[ToolName]-dev`, `[ToolName]-staging`, `[ToolName]-prod` — these supply all `#{...}#` token values.

### Pipeline Steps (`azure-pipelines.yml`)

1. Build + restore .NET solution
2. `dotnet test` — fail pipeline on test failure
3. `ng build --configuration production`
4. Replace version tokens
5. Publish artifacts
6. Deploy: dev → staging → prod (approval gate before prod)

---

## 9. Testing

Use TestContainers for all integration and acceptance tests — no database or storage mocks.

| Type | Framework | Scope |
|------|-----------|-------|
| Integration | NUnit 4.x + TestContainers | Accessor layer vs real SQL |
| Acceptance | NUnit 4.x + WebApplicationFactory + TestContainers | Full API stack |
| Frontend | Jasmine + Karma | Stores and services |

Use **Bogus** for test data. **Moq** for infrastructure interfaces only.

---

## 10. Git

- **Branch:** `AD-{ticket}` (e.g., `AD-502-proposal-api`) — create the ticket before branching
- **Commits:** `AD-{ticket}: Description` — one meaningful change per commit
- **Merge:** PR in Azure DevOps, one approval required, merge to `main` only when stable

---

## 11. Code Standards

- Async/await everywhere — no `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`
- All injectable services need an interface (`I{Name}`)
- CORS at proxy only — never in the API project
- `ILogger<T>` for all logging — no `Console.WriteLine`, no logging of secrets or PII
- No hardcoded URLs or environment values — use configuration
- No `.env` files — use User Secrets (dev) and Key Vault (prod)
- SonarAnalyzer.CSharp warnings must not be suppressed without a documented reason

---

## 12. README Requirements

Every repo needs a `README.md` with: Project Name & Purpose, Critical Decisions, Deployment Instructions, Setup / Getting Started, Dependencies.

---

## Checklist — Starting a New Tool

- [ ] Repo created under `C:\Users\billiejos\Projects\LED AI Tools\`
- [ ] This file copied as `CLAUDE.md`, Tool Identity section filled in
- [ ] `README.md` complete with all 5 sections
- [ ] `.gitignore` added (Visual Studio + Angular template)
- [ ] Azure DevOps ticket created; ticket number used for all branches and commits
- [ ] Azure App Registration created in Entra ID
- [ ] Pipeline variable groups created for dev, staging, prod
- [ ] .NET User Secrets set locally for NetSuite and SQL credentials
- [ ] Aspire orchestrator running before starting API work

---

## 13. Coding Behavior

> Source: [andrej-karpathy-skills](https://github.com/multica-ai/andrej-karpathy-skills). Applies to all AI-assisted coding in this project.
> **Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

### Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them — don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

### Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it — don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

### Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.
