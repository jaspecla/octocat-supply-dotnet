# Application Insights + GitHub Copilot MCP demo

## Intentional defect: preserve DEMO-HOME-001

The home-page **Explore Products** button deliberately appears unresponsive for
five seconds, then navigates to the working product catalog. This is an
intentional diagnostic exercise, not an accidental performance regression.
**Do not optimize away, shorten, or remove the wait or its telemetry unless the
user explicitly requests that change.** The source and regression tests also
identify this defect.

The delay is an asynchronous, cancellation-aware wait scoped to this one
interaction. It does not block a server thread, slow readiness checks, or delay
the navigation bar's Products link. Duplicate clicks are disabled during the
wait; leaving the page cancels it. Only disposable demo data belongs in this
publicly reachable, intentionally vulnerable application. Its ASP.NET Core
environment is `Production`, but it is not suitable for real production use.

## Live demo resources

- Frontend: <https://octocat-supply-w-ycxczwbwrj7vs.bluesmoke-d34b8b5e.westus2.azurecontainerapps.io>
- Resource group: `GHCP_App_Insights_MCP_Demo`
- Region: `westus2`
- Subscription: `0bf5af35-8a8e-4906-9a03-38747dad40e5`
- Tenant: `3887ea37-8523-473d-8bd3-5d5cb3272470`
- Application Insights: `octocat-supply-w-ycxczwbwrj7vs-insights`
- Log Analytics workspace: `octocat-supply-w-ycxczwbwrj7vs-logs`
- Workspace ID: `f8e61f4b-3c49-4652-8409-5759e7fa3305`
- Web cloud role: `octocat-supply-web`

Both server processes use OpenTelemetry and authenticated Azure Monitor ingestion
with the app's managed identity. No credentials are needed in source control.
The deployment provisions Application Insights and its ingestion role.

## Before presenting

1. Open this repository in VS Code with GitHub Copilot and the
   [Azure MCP Server extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-mcp-server).
   The existing Azure MCP server's `monitor` tools can query the workspace.
   Enable those tools in Copilot Agent mode. No custom MCP server is necessary.
2. Authenticate to the demo tenant/subscription. Your querying identity needs
   permission to read the workspace and its logs (for example **Log Analytics
   Reader** on this workspace). In this environment, pass the **explicit tenant**
   to Azure MCP calls: omitting it can select the wrong identity and return 403.
   Use the **workspace name**, not its GUID, in this MCP server version: the GUID
   returned empty results during rehearsal while the name returned telemetry.
3. Open the live app in a fresh browser tab after deployment; old Blazor circuits
   may need a reload. Click **Explore Products** in the hero, not the navigation
   bar's Products link. Expect roughly five seconds before the catalog appears.
4. Repeat two or three times by returning home. Allow a few minutes for Azure
   Monitor ingestion before presenting; this is not a real-time log viewer.
5. Run the first query below through Copilot's MCP tools to ensure recent rows
   exist. Generate fresh clicks tomorrow so the one-hour query window includes
   your demo. Widen it to 24 hours if you want to inspect rehearsal evidence.

## Suggested Copilot prompt

> The deployed Octocat Supply home page pauses for several seconds when I click
> Explore Products, but the Products navigation link is fast. Use the Azure MCP
> server to query Application Insights telemetry in workspace
> octocat-supply-w-ycxczwbwrj7vs-logs, subscription
> 0bf5af35-8a8e-4906-9a03-38747dad40e5, explicitly using tenant
> 3887ea37-8523-473d-8bd3-5d5cb3272470. Correlate a recent slow interaction,
> its dependencies, and logs, then inspect this repository to explain the exact
> cause. Cite the operation ID, measured durations, and source method. Diagnose
> only; do not modify or fix this intentional demo defect.

Follow up with: "Is the delay caused by the database, an outbound HTTP call, or
application code? Show the evidence rather than guessing."

## Queries for the Azure MCP server

Use `monitor_workspace_log_query` with workspace `octocat-supply-w-ycxczwbwrj7vs-logs`, `table` set to
`AppRequests`, `hours` set to `1`, and the explicit subscription and tenant.
Application Insights' workspace tables use `AppRequests`, `AppDependencies`,
and `AppTraces` (rather than the classic `requests`, `dependencies`, `traces`).

Find slow interactions:

```kusto
AppRequests
| where TimeGenerated > ago(1h)
| where AppRoleName == "octocat-supply-web" and Name == "Home.ExploreProducts"
| project TimeGenerated, Name, DurationMs, Success, OperationId, Id, Properties
| order by TimeGenerated desc
| take 10
```

Correlate the latest interaction and its exact cause:

```kusto
let operation = toscalar(
    AppRequests
    | where TimeGenerated > ago(1h)
    | where AppRoleName == "octocat-supply-web" and Name == "Home.ExploreProducts"
    | top 1 by TimeGenerated desc
    | project OperationId);
union
    (AppRequests | project TimeGenerated, Signal="request", Name, DurationMs,
        OperationId, ParentId, Id, Detail="", Properties),
    (AppDependencies | project TimeGenerated, Signal="dependency", Name, DurationMs,
        OperationId, ParentId, Id, Detail=Data, Properties),
    (AppTraces | project TimeGenerated, Signal="log", Name="", DurationMs=real(null),
        OperationId, ParentId, Id="", Detail=Message, Properties)
| where TimeGenerated > ago(1h) and OperationId == operation
| order by TimeGenerated asc
```

## Expected diagnosis (presenter notes)

### Verified rehearsal

On November 4, 2026 (UTC), the deployed web image
`demo-home-latency-20261104-4`, revision `0000003`, was verified in a browser:

- Explore Products remained on the home page with the control disabled during
  the wait, then navigated in approximately **5.3 seconds**.
- The destination catalog rendered **12 products** without a fetch error.
- The Products navigation link changed routes immediately, without the demo wait.
- The homepage and Blazor startup script both returned HTTP 200.
- All **26 tests** passed, including disabled mode, cancellation, trace parenting,
  measured delay, and actual Azure Monitor request/dependency/log export.

The correlation query above was executed successfully through Azure MCP:

| Signal | Name | Measured duration |
|---|---|---|
| Request | `Home.ExploreProducts` | 5,004.565 ms |
| Dependency | `ProductCatalog.Warmup` | 5,002.6352 ms |
| Warning logs | Warmup start and completion | Same operation and dependency parent |

Operation ID: `134b452ff3ec5ea2997a90cde2c477ff`.
The rehearsal occurred at `2026-11-04T03:40:51Z`; widen the query window when
using this historical example. Generate new clicks for the live presentation.

### Source and telemetry explanation

The `Home.ExploreProducts` server span becomes an Application Insights request.
Its `ProductCatalog.Warmup` child span accounts for approximately 5,000 ms and
becomes a dependency. Both warning logs have the same operation ID and the
warmup span as parent. The span attributes include:

- `demo.defect_id = DEMO-HOME-001`
- `demo.intentional = true`
- `demo.delay_ms = 5000`
- `demo.delay_mechanism = Task.Delay`
- `code.function.name = WarmupCatalogAsync`
- `code.file.path = src/OctocatSupply.Web/Services/ProductExplorationService.cs`
- `demo.elapsed_ms` records the actual server-side wait.

The [home-page handler](../src/OctocatSupply.Web/Components/Pages/Home.razor)
awaits [ProductExplorationService.PrepareAsync](../src/OctocatSupply.Web/Services/ProductExplorationService.cs),
which awaits `WarmupCatalogAsync`. That method intentionally awaits
`Task.Delay(TimeSpan.FromSeconds(5))` before navigation. There is no database
query or outbound HTTP dependency inside this wait. The catalog's normal API
request happens after navigation and is not the reason for the five-second pause.
Each click gets its own root operation rather than inheriting the long-lived
Blazor/SignalR connection, so you need not close the browser to export the span.
Demo mode uses an always-on trace sampler so individual clicks are not lost.

## Configuration and redeployment

`Demo:SlowProductExploration` defaults to `false` locally. The
[Container Apps template](../infra/modules/container-app.bicep) intentionally sets
`Demo__SlowProductExploration=true` on the **web** container for this demo.
To reproduce locally, set that environment variable in the web process; configure
Application Insights as described in the [README](../README.md#opentelemetry-and-application-insights).

To turn off the defect, set the web container's environment variable to `false`
and deploy a new revision. Do not remove the intentional source/test behavior.
For a persistent opt-out, also change the template's demo flag, otherwise the
next infrastructure deployment will re-enable it.

Redeploy this existing app without changing its name or region:

```powershell
.\infra\deploy.ps1 -ResourceGroup 'GHCP_App_Insights_MCP_Demo' `
    -Location 'westus2' -AppName 'octocat-supply-w'
```

Deployments reset the disposable database and Blazor sessions. Resources incur
Azure charges while retained; leave them running for the presentation and clean
up afterward according to the [deployment guide](deployment.md).

The web project explicitly requires ASP.NET web assets at restore time because
the Dockerfile restores before copying Razor files. Without this setting the
SDK's file-based detection omitted the Blazor boot script, leaving a rendered
homepage with nonfunctional controls. The Docker build now asserts that the
published script exists, and the deployment script verifies its public URL.
This bootstrap defect was **not** intentional and must not be reintroduced.
