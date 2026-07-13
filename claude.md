# BlazorDualListbox

An open-source Blazor component: two listboxes where items can be moved between them.
Intended to be published as a NuGet package and open-sourced.

## Confirmed decisions

- **Target frameworks:** Multi-target the RCL as `net6.0;net8.0` (in-support LTS lines),
  optionally add `net9.0`/`net10.0`. `.NET 6` is the floor for broad adoption.
  - Constraint: code must use APIs common to all targets. `@bind:after` and some newer
    render-mode APIs arrived in .NET 8, so v1 uses `net6.0`-compatible patterns
    (explicit `EventCallback` wiring rather than `@bind:after`).
- **Move UX (v1):** Buttons (→ ← ⇒ ⇐) + double-click to move. Fully keyboard-accessible.
  Drag-and-drop is parked as a later, optional phase.
- **Render model:** Razor Class Library (RCL), render-mode agnostic (works in Blazor
  Server, WASM, and .NET 8+ unified hosting).

## Solution structure

Everything lives under `src/` (per project decision). Solution is `.slnx` (the .NET 10
SDK's default XML solution format).

```
blazor-dual-listbox/
├─ claude.md  .gitignore
└─ src/
   ├─ BlazorDualListbox.slnx            # solution
   ├─ BlazorDualListbox/                # Razor Class Library (the NuGet package)
   │  ├─ BlazorDualListbox.csproj       #   multi-targets net6.0;net8.0, NuGet metadata
   │  └─ _Imports.razor
   ├─ DemoApp/                          # Blazor Web App (net10.0, interactive Server)
   └─ BlazorDualListbox.Tests/          # bUnit + xUnit (net8.0)
```

Still to add at repo root during OSS-scaffolding phase: `README.md`, `LICENSE`,
`CONTRIBUTING.md`, `CHANGELOG.md`, `.github/workflows/ci.yml`.

**Framework versions:** RCL references `Microsoft.AspNetCore.Components.Web` per TFM
(6.0.36 for net6.0, 8.0.11 for net8.0). DemoApp is net10.0; Tests is net8.0 (bUnit floor).

## Component API (core design)

Single generic component `DualListbox<TItem>` driven by two-way binding:

```razor
<DualListbox TItem="Person"
             @bind-Source="available"
             @bind-Selected="chosen"
             TextSelector="p => p.Name"
             Filterable="true"
             AllowMoveAll="true"
             OnChanged="HandleChanged" />
```

Key parameters:
- **`Source` / `Selected`** (`IEnumerable<TItem>`, both `@bind`-able) — the two lists.
- **`TextSelector` / `ValueSelector`** (`Func<TItem,string>`) — render/identify items.
- **`ItemTemplate`** (`RenderFragment<TItem>`) — optional custom item rendering.
- Move operations: move-selected (→/←), move-all (⇒/⇐), via buttons and double-click.
- **Multi-select** with Ctrl/Shift; keyboard navigation (arrows, space, enter).
- **` `** — per-box search text boxes.
- **`OnChanged` / `OnMoved`** callbacks.
- Header/label slots, disabled state, preserved ordering.

Internally: keep item UI state (selection highlight, filter text) in the component; only
mutate the bound collections on an actual move, raising bind callbacks so the parent stays
the source of truth.

## Styling

CSS isolation (`DualListbox.razor.css`) with a clean default and CSS custom properties
(`--dl-*`) for theming. No Bootstrap/dependency lock-in.

## Accessibility

`role="listbox"` / `role="option"`, `aria-multiselectable`, `aria-selected`, focus
management, full keyboard operability. A key differentiator vs. existing components.

## Testing

bUnit + xUnit for rendering/interaction (render, click move buttons, assert lists updated,
two-way binding fires). DemoApp for manual/visual verification.

## OSS scaffolding

NuGet metadata in `.csproj` (PackageId, description, tags, repo URL,
`PackageLicenseExpression=MIT`, README-in-package), MIT LICENSE, README with usage snippet
+ GIF, CONTRIBUTING.md, GitHub Actions workflow (build/test/pack on PR, publish on tag).

## Build order (phased)

1. Scaffold solution (RCL + DemoApp + test project + sln).
2. `DualListbox<TItem>` with buttons, single list-to-list moves, two-way binding.
3. Multi-select + move-all + double-click.
4. Filtering + keyboard nav + ARIA.
5. CSS isolation + theming.
6. bUnit tests.
7. OSS scaffolding + CI + NuGet packaging.
8. (Later) optional drag-and-drop, reorder-within-list.
