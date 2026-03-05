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

## Project Structure

```
src/
├── OctocatSupply.Api/          # ASP.NET Core Web API
│   ├── Controllers/            # 8 API controllers (CRUD for all entities)
│   ├── Data/                   # EF Core DbContext and seed data
│   ├── Models/                 # Entity models with EF Core annotations
│   └── Repositories/           # Repository pattern with interfaces
│
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
