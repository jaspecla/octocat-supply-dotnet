---
description: "Guidance for editing and reviewing frontend code changes in the Blazor application."
applyTo: "src/OctocatSupply.Web/**/*.razor, src/OctocatSupply.Web/**/*.cs, src/OctocatSupply.Web/**/*.css"
---
# Frontend Review Guidance
Focus on UX quality, accessibility, performance, and maintainability of the Blazor application.

## Key Principles
- Accessibility first: semantic HTML, proper labels, ARIA only when semantics insufficient, maintain focus order.
- State management: Use injectable services for shared state; keep local UI state local with component parameters; avoid excessive cascading parameters.
- Performance: Use lazy loading for routes, avoid large bundle additions, prefer streaming rendering for heavy pages.
- Styling: CSS isolation (`.razor.css`) preferred; abstract repeated styles into shared components or utility classes.
- Types: Use strong typing with C# models; avoid dynamic or object types; use nullable reference types properly.

## Review Checklist
1. Data fetching uses HttpClient/ApiService with proper error handling and loading states, not ad-hoc inline calls.
2. Components remain small & focused (< ~150 LOC). Suggest extraction when crossing concerns (data + complex layout + formatting).
3. Responsive: verify critical views at mobile (≤640px), md (~768px), lg (≥1024px).
4. Form inputs: keyboard accessible, visible focus ring, validation feedback with text, not only color. Use EditForm with DataAnnotationsValidator.
5. Images: optimized (correct size, `alt` text), avoid layout shift (width/height or aspect-ratio set).
6. Routing: use Blazor @page directives and NavigationManager; avoid deep nesting that causes rendering waterfalls.
7. Security: never interpolate untrusted HTML; use MarkupString only with sanitized content.

## Testing Guidance
- Encourage bUnit tests for complex logic (conditional rendering, form validation, component interaction).
- Snapshot tests only for stable presentational components.

## Performance Flags
- Re-render hotspots (large lists) should use virtualization (Virtualize component) when count > ~200.
- Avoid unnecessary StateHasChanged calls in event handlers.

## Anti-Patterns to Nudge
- Overuse of cascading parameters for simple property passing.
- Mixing data fetching + presentational markup in one large component.
- Custom CSS files duplicating styles already available in the design system.

## Example Feedback Style
"Consider extracting the price formatting into a `FormatCurrency()` extension method because it's duplicated in ProductCard.razor and OrderSummary.razor and risks divergence."
