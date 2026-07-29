# BlazorDualListbox

[![NuGet](https://img.shields.io/nuget/v/BlazorDualListbox.svg)](https://www.nuget.org/packages/BlazorDualListbox)
[![NuGet downloads](https://img.shields.io/nuget/dt/BlazorDualListbox.svg)](https://www.nuget.org/packages/BlazorDualListbox)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A generic, accessible **dual-listbox** (pick-list) component for Blazor. Two side-by-side
list boxes with controls to move items between them — via buttons, double-click, or the
keyboard.

- **Render-mode agnostic** — works in Blazor Server, WebAssembly, and .NET 8+ unified hosting.
- **Multi-targets** `net6.0`, `net8.0`, and `net10.0`.
- **No CSS framework dependency** — clean default styling via CSS isolation, themeable with
  CSS custom properties. Bring Bootstrap/Tailwind/your own classes if you want them.
- **Accessible** — `role="listbox"`/`role="option"`, `aria-multiselectable`, `aria-selected`,
  managed focus, and full keyboard operability.
- **Two-way bindable** — the parent stays the source of truth; the component only mutates the
  bound collections on an actual move.

## Installation

```bash
dotnet add package BlazorDualListbox
```

Add the namespace to `_Imports.razor` (or the individual page):

```razor
@using BlazorDualListbox
```

## Quick start

```razor
@using BlazorDualListbox

<DualListbox TItem="Person"
             @bind-Source="available"
             @bind-Selected="chosen"
             TextSelector="p => p.Name"
             ValueSelector="p => p.Id"
             Filterable="true"
             SourceHeader="Available"
             SelectedHeader="Selected"
             OnMoved="HandleMoved" />

@code {
    private IEnumerable<Person> available = new List<Person>
    {
        new(1, "Alice"), new(2, "Bob"), new(3, "Carol"),
    };
    private IEnumerable<Person> chosen = new List<Person>();

    private void HandleMoved(DualListboxMoveEventArgs<Person> e)
        => Console.WriteLine($"{e.Items.Count} item(s) moved {e.Direction}");

    public record Person(int Id, string Name);
}
```

`Source` and `Selected` are both `@bind`-able. Because the component raises the change
callbacks on every move, your bound fields always reflect the current state.

## Moving items

| Action | How |
| --- | --- |
| Move selected item(s) → | `›` button, `Enter`, or double-click an item |
| Move all → | `»` button (moves only *filtered/visible* items) |
| Move selected item(s) ← | `‹` button, `Enter`, or double-click an item |
| Move all ← | `«` button |

**Selection:** click to select, `Ctrl`/`Cmd`+click to toggle, `Shift`+click to select a range,
`Ctrl`/`Cmd`+`A` to select all visible.

**Keyboard:** focus a list, then `↑`/`↓`/`Home`/`End` to navigate, `Space` to toggle
selection, `Enter` to move highlighted items.

## Parameters

### Data

| Parameter | Type | Description |
| --- | --- | --- |
| `Source` | `IEnumerable<TItem>?` | Items in the left (available) list. `@bind`-able. |
| `Selected` | `IEnumerable<TItem>?` | Items in the right (selected) list. `@bind`-able. |

### Presentation

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `TextSelector` | `Func<TItem,string>?` | `ToString()` | Projects an item to its display text. |
| `ValueSelector` | `Func<TItem,object>?` | the item itself | Stable identity for equality/selection. Supply this when items lack value equality but have a key (e.g. an `Id`). |
| `ItemTemplate` | `RenderFragment<TItem>?` | — | Custom rendering per item. Overrides `TextSelector` output. |
| `SourceHeader` | `string?` | — | Heading above the source list. |
| `SelectedHeader` | `string?` | — | Heading above the selected list. |
| `EmptyText` | `string` | `"No items"` | Shown in a list that has no items. |
| `AddSingleButtonText` | `string` | `›` | Text for the move-selected-right button. |
| `AddAllButtonText` | `string` | `»` | Text for the move-all-right button. |
| `RemoveSingleButtonText` | `string` | `‹` | Text for the move-selected-left button. |
| `RemoveAllButtonText` | `string` | `«` | Text for the move-all-left button. |

### Button styling

| Parameter | Type | Description |
| --- | --- | --- |
| `ButtonClass` | `string?` | CSS class(es) applied to **all four** move buttons. |
| `AddSingleButtonClass` | `string?` | Extra class(es) for the move-selected-right button only. |
| `AddAllButtonClass` | `string?` | Extra class(es) for the move-all-right button only. |
| `RemoveAllButtonClass` | `string?` | Extra class(es) for the move-all-left button only. |
| `RemoveSingleButtonClass` | `string?` | Extra class(es) for the move-selected-left button only. |

Classes compose as **`dl-btn` + `ButtonClass` + the per-button class** — the built-in
`dl-btn` is always kept, so you're adding to the default styling rather than replacing it.

```razor
<DualListbox TItem="Person"
             @bind-Source="available"
             @bind-Selected="chosen"
             ButtonClass="btn"
             AddSingleButtonClass="btn-primary"
             RemoveSingleButtonClass="btn-danger" />
```

The `›` button above renders as `class="dl-btn btn btn-primary"`.

### Behaviour

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Filterable` | `bool` | `false` | Shows a search box above each list. |
| `FilterPlaceholder` | `string` | `"Filter…"` | Placeholder for the filter boxes. |
| `AllowMoveAll` | `bool` | `true` | Shows the "move all" (`»`/`«`) buttons. |
| `Disabled` | `bool` | `false` | Makes the whole component non-interactive. |

Any unmatched attributes (e.g. `class`, `id`, `data-*`) are splatted onto the root element.

### Events

| Parameter | Type | Description |
| --- | --- | --- |
| `SourceChanged` | `EventCallback<IEnumerable<TItem>>` | Enables `@bind-Source`. |
| `SelectedChanged` | `EventCallback<IEnumerable<TItem>>` | Enables `@bind-Selected`. |
| `OnChanged` | `EventCallback` | Raised after any move, once the bound collections are updated. |
| `OnMoved` | `EventCallback<DualListboxMoveEventArgs<TItem>>` | Raised after a move with the moved items and `Direction` (`ToSelected` / `ToSource`). |

## Theming

The component ships with CSS isolation and exposes CSS custom properties. Override them from
your own stylesheet on `.dl-root` (or any ancestor):

```css
.dl-root {
    --dl-gap: 0.5rem;
    --dl-border-color: #ccc;
    --dl-radius: 6px;
    --dl-bg: #fff;
    --dl-fg: #1a1a1a;
    --dl-muted-fg: #666;
    --dl-hover-bg: rgba(0, 0, 0, 0.05);
    --dl-highlight-bg: #2563eb;
    --dl-highlight-fg: #fff;
    --dl-accent: #2563eb;
    --dl-list-height: 16rem;
    --dl-min-width: 12rem;
}
```

For button styling specifically, prefer the `*ButtonClass` parameters described above.

## Accessibility

Each list is a `role="listbox"` with `aria-multiselectable="true"`; items are `role="option"`
with `aria-selected`. Focus is tracked with `aria-activedescendant`, and the component is fully
operable by keyboard. This is a deliberate differentiator versus many existing components.

## Requirements

- .NET 6, .NET 8, or .NET 10 (the package multi-targets `net6.0;net8.0;net10.0`).
- Works with any Blazor render mode (Server, WebAssembly, or .NET 8+ Auto/unified hosting).

## Contributing

Issues and pull requests are welcome. The repository contains the component (RCL), a demo app,
and a bUnit test suite:

```bash
git clone <repository-url>
cd blazor-dual-listbox/src
dotnet build
dotnet test
```

Run the demo app to explore the component interactively:

```bash
dotnet run --project DemoApp/DemoApp.csproj
```

Then browse to `/dual-listbox`.

## License

[MIT](LICENSE)
