---
description: 'Check DRY'
tools: ['search', 'web/fetch', 'web/githubRepo','read', 'execute', 'azure-mcp-server/search', 'playwright/*', 'github-remote/*']
---

# 🔍 DRY Violation Analysis & Refactoring Recommendations

## 🎯 Objective
Analyze the codebase for DRY (Don't Repeat Yourself) violations and provide actionable refactoring recommendations to improve code maintainability, reduce duplication, and enhance overall code quality.

## 🔎 Areas to Analyze

### 1. **API Controller Patterns** 🛣️
Examine all controller files in `src/OctocatSupply.Api/Controllers/` for:
- **CRUD Operations**: Identical GET, POST, PUT, DELETE action patterns
- **Swagger Documentation**: Repetitive OpenAPI attribute definitions
- **Error Handling**: Duplicate 404/400/500 error responses and ProblemDetails
- **Validation Logic**: Similar input validation patterns using Data Annotations
- **Response Formatting**: Repeated ActionResult response structures

### 2. **Component Patterns** ⚛️
Analyze Blazor components in `src/OctocatSupply.Web/` for:
- **UI Components**: Similar layout patterns, buttons, forms
- **Data Fetching**: Duplicate HttpClient/API service call patterns
- **State Management**: Repetitive parameter and cascading value patterns
- **Event Handlers**: Similar EventCallback handlers
- **Styling**: Duplicate CSS classes and styling patterns

### 3. **Business Logic** 💼
Check for duplicated business logic in:
- **Model Definitions**: Similar classes and record types
- **Data Transformation**: Repeated mapping/formatting methods
- **Validation Rules**: Duplicate validation logic
- **Utility Methods**: Similar helper methods and extension methods

### 4. **Configuration & Setup** ⚙️
Look for configuration duplication in:
- **Middleware Setup**: Repeated middleware pipeline patterns
- **Service Registration**: Similar dependency injection patterns in Program.cs
- **Build Configuration**: Duplicate MSBuild/project settings
- **Environment Variables**: Repeated configuration patterns in appsettings.json

## 📊 Analysis Framework

### **Severity Levels**
- 🔴 **Critical**: Extensive duplication (>5 instances)
- 🟡 **Moderate**: Notable duplication (3-5 instances)
- 🟢 **Minor**: Limited duplication (2-3 instances)

### **Refactoring Impact**
- ⚡ **High Impact**: Affects multiple files/modules
- 📈 **Medium Impact**: Affects single module/feature
- 🔧 **Low Impact**: Localized improvements

## 🛠️ Recommended Refactoring Patterns

### **For API Controllers:**
1. **Generic CRUD Controller**: Create base controller class
2. **Middleware Abstraction**: Extract common middleware/filter patterns
3. **Response Helpers**: Centralize response formatting with ActionResult helpers
4. **Validation Attributes**: Reusable Data Annotation and FluentValidation logic

### **For Blazor Components:**
1. **Base Components**: Inherit common functionality from shared base classes
2. **Services**: Extract reusable stateful logic into injectable services
3. **Component Composition**: Break down into smaller reusable Razor components
4. **Parameter Interfaces**: Standardize component parameter contracts

### **For Business Logic:**
1. **Service Layer**: Centralize business operations with dependency injection
2. **Factory Patterns**: Create object instances consistently
3. **Strategy Pattern**: Handle similar operations differently
4. **Extension Methods**: Group related helper methods as extensions

## 📝 Deliverables

### **1. Violation Report**
Create a detailed report containing:
- **File-by-file analysis** of duplication found
- **Code snippets** showing exact duplicated patterns
- **Severity assessment** using the framework above
- **Quantified metrics** (lines duplicated, files affected)

### **2. Refactoring Plan**
Provide a prioritized action plan with:
- **Quick Wins**: Easy refactoring opportunities (< 2 hours)
- **Medium Efforts**: Moderate refactoring tasks (2-8 hours)
- **Large Projects**: Comprehensive restructuring (> 8 hours)

### **3. Implementation Suggestions**
For each violation, provide:
- **Before/After Code Examples**: Show current vs. refactored code
- **Migration Steps**: Step-by-step refactoring instructions
- **Testing Strategy**: How to validate refactoring doesn't break functionality
- **Performance Impact**: Expected improvements in bundle size, maintainability

## 🎯 Success Criteria

- [ ] **Comprehensive Analysis**: All major code duplication identified
- [ ] **Actionable Recommendations**: Clear, implementable suggestions
- [ ] **Prioritized Backlog**: Tasks ranked by impact vs. effort
- [ ] **Code Examples**: Concrete before/after demonstrations
- [ ] **Metrics**: Quantified improvement estimates

## 🚀 Getting Started

1. **Scan Route Files**: Start with `api/src/routes/` directory
2. **Analyze Patterns**: Look for identical or nearly identical code blocks
3. **Document Findings**: Create violation inventory with severity ratings
4. **Propose Solutions**: Design generic abstractions and patterns
5. **Create Examples**: Show practical refactoring implementations

## 📚 Focus Areas for This Codebase

Based on the OctoCAT Supply Chain architecture:

### **High-Priority Targets:**
- **REST API Routes**: 8 route files with identical CRUD patterns
- **Swagger Documentation**: Repetitive OpenAPI schema definitions
- **Error Handling**: Duplicate 404/error responses across routes
- **Entity Models**: Similar TypeScript interfaces

### **Secondary Targets:**
- **React Components**: Product display and form components
- **API Integration**: Fetch/axios patterns in frontend
- **Testing Patterns**: Similar test structures across files
