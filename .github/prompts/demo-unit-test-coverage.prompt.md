---
description: 'Demo: Improve API Test Coverage - Add Unit Tests for Missing Controllers.'
tools: ['search', 'edit', 'web','vscode/openSimpleBrowser', 'read', 'execute', 'azure-mcp-server/search', 'playwright/*', 'github/*']
---
# 🧪 Demo: Add Unit Tests for Product and Supplier Controllers

## 📊 Current State

- Only **1 test file exists**: `BranchesControllerTests.cs`


## 🎯 Objective
Increase API test coverage by implementing comprehensive unit tests for Product and Supplier controllers.

## 📋 Missing Test Files

### 🔗 Controller Tests (High Priority)
The following controller files need complete test coverage:


- [ ] `tests/OctocatSupply.Api.UnitTests/Controllers/ProductsControllerTests.cs`
- [ ] `tests/OctocatSupply.Api.UnitTests/Controllers/SuppliersControllerTests.cs`


## ✅ Test Coverage Requirements

### For Each Controller Test File:

- **CRUD Operations:**
  - ✅ GET all entities
  - ✅ GET single entity by ID
  - ✅ POST create new entity
  - ✅ PUT update existing entity
  - ✅ DELETE entity by ID

- **Error Scenarios:**
  - ❌ 404 for non-existent entities
  - ❌ 400 for invalid request payloads
  - ❌ 422 for validation errors
  - ❌ Edge cases (malformed IDs, empty requests)

## 🛠️ Implementation Guidelines

### Use Existing Pattern

Follow the pattern established in `tests/OctocatSupply.Api.UnitTests/Controllers/BranchesControllerTests.cs`:
```csharp
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OctocatSupply.Api.Controllers;
using OctocatSupply.Api.Repositories;
using OctocatSupply.Api.Models;
```

### Test Structure Template
```csharp
public class [Entity]ControllerTests
{
    private readonly Mock<I[Entity]Repository> _mockRepository;
    private readonly [Entity]Controller _controller;

    public [Entity]ControllerTests()
    {
        _mockRepository = new Mock<I[Entity]Repository>();
        _controller = new [Entity]Controller(_mockRepository.Object);
    }

    [Fact]
    public async Task Create_ReturnsCreatedResult_WhenValidInput() { /* POST test */ }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithAllEntities() { /* GET all test */ }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenEntityExists() { /* GET by ID test */ }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenEntityExists() { /* PUT test */ }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenEntityExists() { /* DELETE test */ }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenEntityDoesNotExist() { /* Error test */ }
}
```


## 🔧 Running Tests


```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test file
dotnet test --filter "FullyQualifiedName~ProductsControllerTests"
```


## 📈 Success Criteria
- [ ] Add controller test files for Product and Supplier
- [ ] All tests passing in CI/CD

## 🚀 Getting Started

1. Start with `ProductsControllerTests.cs` - copy `BranchesControllerTests.cs` pattern
2. Implement basic CRUD tests first
3. Add error scenarios incrementally
4. Run coverage after each file to track progress
5. Follow ERD relationships for cross-entity testing


## 📚 Related Files

- ERD Diagram: `docs/ERD.png`
- Existing test: `tests/OctocatSupply.Api.UnitTests/Controllers/BranchesControllerTests.cs`
- API project: `src/OctocatSupply.Api/OctocatSupply.Api.csproj`

