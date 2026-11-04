# OctoCAT Supply Chain Management System (.NET Edition)

A .NET port of the OctoCAT Supply Chain demo application, built with ASP.NET Core Web API and Blazor Server. This app was created for **GitHub Copilot demo purposes** and contains **intentional bugs and security vulnerabilities** to showcase Copilot's ability to detect and fix issues.

> ⚠️ **WARNING**: This application contains intentional security vulnerabilities. Do NOT deploy to production.

## Architecture

- **Backend**: ASP.NET Core Web API (.NET 10) with Entity Framework Core + SQLite
- **Frontend**: Blazor Server with Tailwind CSS
- **Database**: SQLite with EF Core migrations and seed data
- **API Docs**: Swagger/OpenAPI at `/swagger`

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Running the API

```bash
cd src/OctocatSupply.Api
dotnet run
```

The API will start at `http://localhost:3001`. Swagger UI is available at `http://localhost:3001/swagger`.

### Running the Blazor Frontend

```bash
cd src/OctocatSupply.Web
dotnet run
```

The frontend will start at `http://localhost:5000` (or the port shown in console).

### Building

```bash
dotnet build
```

### OpenTelemetry and Application Insights

The API and Blazor server collect OpenTelemetry traces, metrics, and
`Microsoft.Extensions.Logging` logs when an Application Insights connection
string is configured. Without one, Azure Monitor telemetry is disabled and
existing console logging and local startup are unchanged. The services use
distinct Application Insights role names, `octocat-supply-api` and
`octocat-supply-web`; HTTP trace context propagates between them. The API also
collects SQLite dependency spans through the prerelease EF Core instrumentation.
SQL statement text and parameter values are removed from those spans.

Set `APPLICATIONINSIGHTS_CONNECTION_STRING` in **both** processes before
starting them. For example, in each PowerShell terminal:

```powershell
$env:APPLICATIONINSIGHTS_CONNECTION_STRING = '<your Application Insights connection string>'
# Required if the Application Insights resource requires Microsoft Entra authentication:
# $env:AzureMonitor__Authentication = 'DefaultAzureCredential'
dotnet run --project .\src\OctocatSupply.Api
```

In the other terminal, use `.\src\OctocatSupply.Web` as the project. Do not
commit the connection string. For Entra authentication, sign in with `az login`
and have an administrator grant your account the **Monitoring Metrics
Publisher** role on the Application Insights resource.

Optional configuration:

- `AzureMonitor__Authentication`: `None` (default, for resources permitting
  local authentication), `DefaultAzureCredential` (local development), or
  `ManagedIdentity` (Azure). With `ManagedIdentity`, set `AZURE_CLIENT_ID`
  for a user-assigned identity; omit it for a system-assigned identity.
- `OTEL_SERVICE_NAME`: override a process's default service name. Use different
  values for the API and web processes to keep their roles distinct.
- `AzureMonitor__EnableLiveMetrics`: defaults to `true`.
- `AzureMonitor__DisableOfflineStorage`: defaults to `false`; set to `true` to
  disable local retry storage for telemetry that cannot be sent.
- `APPLICATIONINSIGHTS_SDKSTATS_DISABLED`: set to `true` to disable Azure
  Monitor SDK health statistics without disabling application telemetry.

The Azure Monitor distro applies rate-limited trace sampling by default. See
the [sampling configuration](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-sampling)
to adjust it. Telemetry ingestion can incur charges. Logs, URLs, exception
messages, and custom attributes may contain sensitive data even though SQLite
span query text is removed; use only disposable demo data and review logging
levels and access to your Application Insights resource.

### Running in containers

Install Docker with Linux container support. Run these commands from the repository root
(the build context for both Dockerfiles):

```powershell
docker build --file src\OctocatSupply.Api\Dockerfile --tag octocat-supply-api .
docker build --file src\OctocatSupply.Web\Dockerfile --tag octocat-supply-web .

docker network create octocat-supply
docker volume create octocat-supply-data

docker run --detach --name octocat-api --network octocat-supply --publish 127.0.0.1:3001:8080 --mount source=octocat-supply-data,target=/app/data octocat-supply-api
docker run --detach --name octocat-web --network octocat-supply --publish 127.0.0.1:5000:8080 --env ApiBaseUrl=http://octocat-api:8080/api/ octocat-supply-web
```

Open the frontend at `http://localhost:5000`. The API is available at
`http://localhost:3001/api/products`. API calls originate from the Blazor server,
so `ApiBaseUrl` uses the API container's network name, not `localhost`.

Both images use multi-stage .NET 10 builds, run as a non-root user, and listen on
HTTP port `8080`. Configure them using environment variables:

- **API**: `DbFile` defaults to `/app/data/app.db`. The named volume preserves SQLite
  data across container replacement; a new database is created and seeded at startup.
  Bind-mounted directories must be writable by the image's `app` user.
- **Frontend**: Set `ApiBaseUrl` to the backend URL, including the trailing `/api/`.
- **Both**: `ASPNETCORE_ENVIRONMENT` defaults to `Production`. Swagger is disabled
  unless the API runs with `ASPNETCORE_ENVIRONMENT=Development`. Pass the
  telemetry environment variables to each container to enable Application
  Insights; do not put connection strings in image builds.

For hosted deployments, terminate HTTPS at your ingress/reverse proxy and enable
WebSockets for Blazor Server. Frontend data-protection keys should be persisted
outside the container to retain protected browser state across replacements.
The `Production` environment setting does **not** make this intentionally vulnerable
demo safe for production; keep it isolated and do not expose it publicly.

Stop and remove the demo containers and network (the data volume is retained):

```powershell
docker rm --force octocat-web octocat-api
docker network rm octocat-supply
```

### Deploying to Azure Container Apps

The [Azure deployment guide](docs/deployment.md) provisions Azure Container
Registry, builds both images in ACR, and runs them in one Container App using
managed-identity image pulls:

```powershell
az login
az account set --subscription '<subscription-id>'
.\infra\deploy.ps1 -ResourceGroup 'rg-octocat-demo' -Location 'eastus2'
```

The local script requires PowerShell 7+, Azure CLI with Bicep, and permissions to
create resources and role assignments. This configuration exposes the
intentionally vulnerable demo over public HTTPS and uses **disposable SQLite
data**. Do not use real data or treat it as production-ready. The guide also
explains how to configure and run the manual GitHub Actions deployment with
Azure OIDC. See it for prerequisites, costs, and cleanup.

## Project Structure

```
src/
├── OctocatSupply.Api/          # ASP.NET Core Web API
│   ├── Controllers/            # 8 API controllers (CRUD for all entities)
│   ├── Data/                   # EF Core DbContext and seed data
│   ├── Models/                 # Entity models with EF Core annotations
│   └── Repositories/           # Repository pattern with interfaces
│
├── OctocatSupply.Observability/ # Shared OpenTelemetry configuration
└── OctocatSupply.Web/          # Blazor Server Frontend
    ├── Components/
    │   ├── Layout/             # MainLayout, NavMenu, Footer
    │   ├── Pages/              # Home, Products, Login, About, Admin
    │   └── Shared/             # ProductForm
    ├── Services/               # AuthService, ThemeService, ApiService
    └── wwwroot/images/         # Product images
```

## Entities

| Entity | Description |
|--------|-------------|
| Supplier | Product suppliers with contact info |
| Headquarters | Company HQ locations |
| Branch | Branch offices linked to HQ |
| Product | Products with pricing and images |
| Order | Orders placed at branches |
| OrderDetail | Line items in orders |
| Delivery | Supplier deliveries |
| OrderDetailDelivery | Junction linking order details to deliveries |

## Intentional Issues (For Copilot Demos)

This application contains deliberate bugs and security issues for demonstration:

### Security Vulnerabilities
- **SQL Injection** in `ProductRepository.FindByName()` — raw string interpolation in SQL
- **Command Injection** in `DeliveriesController.UpdateStatus()` — unsanitized input to `Process.Start`
- **XSS** in `Login.razor` — `MarkupString` renders raw HTML from URL parameters
- **Auth Spoofing** in `AuthService` — admin role determined by client-side email domain check

### Logic Bugs
- **Misleading Indentation** in `SuppliersController.ProcessSupplierStatus()` — missing braces
- **Misleading Indentation** in `HeadquartersController.ValidateHQName()` — missing braces
- **Broken Validator** in `HeadquartersController` — inconsistent constructor vs factory usage
- **Implicit Type Coercion** in `HeadquartersController.CalculateMetrics()` — uses `dynamic` types
- **Missing String Separators** in `HeadquartersController.CreateLocationLabel()`
- **Infinite Loop Direction** in `AdminProducts.razor` — `for(i=5; i>=0; i++)`
- **Dead Loop** in `Products.razor` — loop condition never true
- **No Protected Routes** — admin page uses client-side check only
