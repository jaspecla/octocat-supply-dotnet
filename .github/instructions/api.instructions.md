---
description: "Guidance for editing and reviewing API code changes in the API."
applyTo: "src/OctocatSupply.Api/**/*.cs, tests/OctocatSupply.Api.UnitTests/**/*.cs"
---
# API Review Guidance

Focus on correctness, security, data integrity, and consistency in the ASP.NET Core + EF Core repository layer.


## API Principles

- Keep controllers thin: validation + orchestration; move logic to repositories/services.
- Use parameterized queries via EF Core always; never build raw query strings with user input.
- Return proper HTTP status codes via shared error classes (NotFound, Validation, Conflict).

- Preserve consistent naming: PascalCase in C# models, snake_case in SQL columns with EF Core column mapping.

- Always use Swagger Documentation via XML comments and OpenAPI attributes on controller actions. Update the spec when adding/modifying endpoints.

## Review Checklist
1. Input validation: basic type/shape checks using Data Annotations before hitting DB; reject ambiguous/partial updates.
2. Error propagation: repository throws domain errors -> middleware/exception filter -> correct status.
3. Transactions: group multi-table writes that must succeed or fail together using EF Core transactions.
4. Performance: watch for N+1 loops over rows triggering per-row SELECT; prefer Include/ThenInclude or JOIN queries.
5. Migrations: every schema change accompanied by new EF Core migration via `dotnet ef migrations add`; no edits to prior migrations.
6. Seed adjustments when adding required NOT NULL columns or reference data in SeedData.cs.
7. Swagger updated: new controller actions, models, examples, response codes.

## Data Integrity
- Enforce foreign keys (ensure EF Core config keeps them ON) & add indexes for new FK columns.
- Use CHECK constraints or Data Annotations for domain rules (e.g., quantity >= 0).

## Testing Guidance
- Add unit tests for new repository methods (happy path + error cases) using in-memory DB or Moq.

- For controller additions, unit test using Moq for repository dependencies verifying status + response shape.


## Security Considerations
- Sanitize / constrain pagination (max limits) to avoid table scans.
- Do not leak internal error stacks in production responses.
- CORS: restrict origins if production hardened later.

## Example Feedback Style
"`ordersRepo.GetWithDetailsAsync()` issues one SELECT per order detail (N+1). Consider using Include/ThenInclude or a single JOIN query returning flattened rows then post-process."
