using BlazorDualListbox.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorDualListbox;

/// <summary>
/// A generic dual list box: two side-by-side list boxes with controls to move items between them.
/// Items are moved via the buttons, by double-clicking an item, or with the keyboard.
/// The component is "controlled" — bind <see cref="Source"/> and <see cref="Selected"/> with
/// <c>@bind-Source</c> / <c>@bind-Selected</c> so moves are reflected back to the parent.
/// </summary>
/// <typeparam name="TItem">The type of item held by the list boxes.</typeparam>
public partial class DualListbox<TItem> : ComponentBase
{
    // ---- Data parameters ----

    /// <summary>The items in the left (available) list box.</summary>
    [Parameter] public IEnumerable<TItem>? Source { get; set; }

    /// <summary>Raised when <see cref="Source"/> changes. Enables <c>@bind-Source</c>.</summary>
    [Parameter] public EventCallback<IEnumerable<TItem>> SourceChanged { get; set; }

    /// <summary>The items in the right (selected) list box.</summary>
    [Parameter] public IEnumerable<TItem>? Selected { get; set; }

    /// <summary>Raised when <see cref="Selected"/> changes. Enables <c>@bind-Selected</c>.</summary>
    [Parameter] public EventCallback<IEnumerable<TItem>> SelectedChanged { get; set; }

    // ---- Presentation parameters ----

    /// <summary>Projects an item to the text shown for it. Defaults to <c>ToString()</c>.</summary>
    [Parameter] public Func<TItem, string>? TextSelector { get; set; }

    /// <summary>
    /// Projects an item to a stable identity used for equality/selection. Defaults to the item
    /// itself. Supply this when items do not implement value equality but expose a key (e.g. an Id).
    /// </summary>
    [Parameter] public Func<TItem, object>? ValueSelector { get; set; }

    /// <summary>Optional custom rendering for each item. Overrides <see cref="TextSelector"/> output.</summary>
    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>Optional heading shown above the source (left) list box.</summary>
    [Parameter] public string? SourceHeader { get; set; }

    /// <summary>Optional heading shown above the selected (right) list box.</summary>
    [Parameter] public string? SelectedHeader { get; set; }

    /// <summary>Text shown inside a list box when it has no items.</summary>
    [Parameter] public string EmptyText { get; set; } = "No items";

    /// <summary>Text shown on the "add single item" button from source to selected.</summary>
    [Parameter] public string AddSingleButtonText { get; set; } = BlazorDualistBoxConstants.DefaultAddSingleItemToSelectedButtonText;

    /// <summary>Text shown on the "add all items" button from source to selected.</summary>
    [Parameter] public string AddAllButtonText { get; set; } = BlazorDualistBoxConstants.DefaultAddAllToSelectedButtonText;

    /// <summary>Text shown on the "remove single item" button from selected to source.</summary>
    [Parameter] public string RemoveSingleButtonText { get; set; } = BlazorDualistBoxConstants.DefaultRemoveSingleItemFromSelectedButtonText;

    /// <summary>Text shown on the "remove all items" button from selected to source.</summary>
    [Parameter] public string RemoveAllButtonText { get; set; } = BlazorDualistBoxConstants.DefaultRemoveAllFromSelectedButtonText;

    /// <summary>
    /// Additional CSS class(es) applied to all four move buttons, appended to the built-in
    /// <c>dl-btn</c> class. Use this to apply your own or a framework's button styling
    /// (e.g. <c>"btn btn-primary"</c>).
    /// </summary>
    [Parameter] public string? ButtonClass { get; set; }

    /// <summary>Additional CSS class(es) for the "add single item" (move selected right) button only.</summary>
    [Parameter] public string? AddSingleButtonClass { get; set; }

    /// <summary>Additional CSS class(es) for the "add all items" (move all right) button only.</summary>
    [Parameter] public string? AddAllButtonClass { get; set; }

    /// <summary>Additional CSS class(es) for the "remove all items" (move all left) button only.</summary>
    [Parameter] public string? RemoveAllButtonClass { get; set; }

    /// <summary>Additional CSS class(es) for the "remove single item" (move selected left) button only.</summary>
    [Parameter] public string? RemoveSingleButtonClass { get; set; }

    // ---- Behaviour parameters ----

    /// <summary>When <c>true</c>, shows a filter box above each list.</summary>
    [Parameter] public bool Filterable { get; set; }

    /// <summary>Placeholder text for the filter boxes.</summary>
    [Parameter] public string FilterPlaceholder { get; set; } = "Filter…";

    /// <summary>When <c>true</c> (default), shows the "move all" buttons.</summary>
    [Parameter] public bool AllowMoveAll { get; set; } = true;

    /// <summary>When <c>true</c>, the entire component is non-interactive.</summary>
    [Parameter] public bool Disabled { get; set; }

    // ---- Events ----

    /// <summary>Raised after any move, once the bound collections have been updated.</summary>
    [Parameter] public EventCallback OnChanged { get; set; }

    /// <summary>Raised after a move with details of what moved and in which direction.</summary>
    [Parameter] public EventCallback<DualListboxMoveEventArgs<TItem>> OnMoved { get; set; }

    /// <summary>Arbitrary attributes splatted onto the root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    // ---- Internal state ----

    private readonly string _idPrefix = "dl-" + Guid.NewGuid().ToString("N").Substring(0, 8);
    private readonly HashSet<object> _sourceHighlight = new();
    private readonly HashSet<object> _selectedHighlight = new();
    private string _sourceFilter = string.Empty;
    private string _selectedFilter = string.Empty;
    private object? _sourceActive;
    private object? _selectedActive;
    private int _sourceAnchor = -1;
    private int _selectedAnchor = -1;

    // ---- Projections ----

    private object Key(TItem item) => ValueSelector?.Invoke(item) ?? (object)item!;

    private string GetText(TItem item) => TextSelector?.Invoke(item) ?? item?.ToString() ?? string.Empty;

    private RenderFragment RenderItem(TItem item) => ItemTemplate is not null
        ? ItemTemplate(item)
        : builder => builder.AddContent(0, GetText(item));

    private HashSet<object> Highlight(bool isSource) => isSource ? _sourceHighlight : _selectedHighlight;

    private string RootClass() => Disabled ? "dl-root dl-disabled" : "dl-root";

    // Composes the button class: built-in "dl-btn" + shared ButtonClass + the per-button class.
    private string BtnClass(string? perButtonClass = null)
    {
        var parts = new[] { "dl-btn", ButtonClass, perButtonClass }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(" ", parts);
    }
    private bool SourceIsEmpty => Source is null || !Source.Any();
    private bool SelectedIsEmpty => Selected is null || !Selected.Any();

    private string OptionClass(TItem item, bool isSource)
    {
        var cls = "dl-option";
        if (IsHighlighted(item, isSource)) cls += " dl-highlight";
        if (IsActive(item, isSource)) cls += " dl-active";
        return cls;
    }

    private IReadOnlyList<TItem> View(bool isSource)
    {
        IEnumerable<TItem> items = (isSource ? Source : Selected) ?? Enumerable.Empty<TItem>();
        var filter = isSource ? _sourceFilter : _selectedFilter;
        if (Filterable && !string.IsNullOrWhiteSpace(filter))
        {
            items = items.Where(i => GetText(i).Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
        return items.ToList();
    }

    private bool IsHighlighted(TItem item, bool isSource) => Highlight(isSource).Contains(Key(item));

    private bool IsActive(TItem item, bool isSource) =>
        Equals(isSource ? _sourceActive : _selectedActive, Key(item));

    private string OptionId(int index, bool isSource) => $"{_idPrefix}-{(isSource ? "s" : "t")}-{index}";

    private string? ActiveDescendant(bool isSource, IReadOnlyList<TItem> view)
    {
        var active = isSource ? _sourceActive : _selectedActive;
        if (active is null) return null;
        for (var i = 0; i < view.Count; i++)
        {
            if (Equals(Key(view[i]), active)) return OptionId(i, isSource);
        }
        return null;
    }

    // ---- Selection ----

    private void SetAnchor(bool isSource, int index)
    {
        if (isSource) _sourceAnchor = index; else _selectedAnchor = index;
    }

    private void SetActive(bool isSource, object? key)
    {
        if (isSource) _sourceActive = key; else _selectedActive = key;
    }

    private void OnOptionClick(MouseEventArgs e, TItem item, int index, bool isSource)
    {
        if (Disabled) return;

        var hi = Highlight(isSource);
        var key = Key(item);
        var view = View(isSource);
        var anchor = isSource ? _sourceAnchor : _selectedAnchor;

        if (e.ShiftKey && anchor >= 0)
        {
            SelectRange(isSource, view, anchor, index);
        }
        else if (e.CtrlKey || e.MetaKey)
        {
            if (!hi.Add(key)) hi.Remove(key);
            SetAnchor(isSource, index);
        }
        else
        {
            hi.Clear();
            hi.Add(key);
            SetAnchor(isSource, index);
        }

        SetActive(isSource, key);
    }

    private void SelectRange(bool isSource, IReadOnlyList<TItem> view, int anchor, int index)
    {
        var hi = Highlight(isSource);
        hi.Clear();
        var lo = Math.Min(anchor, index);
        var high = Math.Max(anchor, index);
        for (var i = lo; i <= high && i < view.Count; i++)
        {
            hi.Add(Key(view[i]));
        }
    }

    private Task OnOptionDblClick(TItem item, bool isSource) =>
        MoveItems(new List<TItem> { item }, isSource);

    private void OnFilterChanged(ChangeEventArgs e, bool isSource)
    {
        var text = e.Value?.ToString() ?? string.Empty;
        if (isSource) _sourceFilter = text; else _selectedFilter = text;
        // Indices change with the filter, so the range anchor is no longer meaningful.
        SetAnchor(isSource, -1);
    }

    // ---- Keyboard ----

    private async Task OnKeyDown(KeyboardEventArgs e, bool isSource)
    {
        if (Disabled) return;

        var view = View(isSource);
        if (view.Count == 0) return;

        var current = IndexOfActive(isSource, view);

        switch (e.Key)
        {
            case "ArrowDown":
                MoveActive(isSource, view, current < 0 ? 0 : Math.Min(current + 1, view.Count - 1), e.ShiftKey);
                break;
            case "ArrowUp":
                MoveActive(isSource, view, current < 0 ? 0 : Math.Max(current - 1, 0), e.ShiftKey);
                break;
            case "Home":
                MoveActive(isSource, view, 0, e.ShiftKey);
                break;
            case "End":
                MoveActive(isSource, view, view.Count - 1, e.ShiftKey);
                break;
            case " ":
            case "Spacebar":
                if (current >= 0) ToggleHighlight(isSource, Key(view[current]));
                break;
            case "Enter":
                await MoveHighlighted(isSource);
                break;
            case "a":
            case "A":
                if (e.CtrlKey || e.MetaKey)
                {
                    var hi = Highlight(isSource);
                    hi.Clear();
                    foreach (var item in view) hi.Add(Key(item));
                }
                break;
        }
    }

    private int IndexOfActive(bool isSource, IReadOnlyList<TItem> view)
    {
        var active = isSource ? _sourceActive : _selectedActive;
        if (active is null) return -1;
        for (var i = 0; i < view.Count; i++)
        {
            if (Equals(Key(view[i]), active)) return i;
        }
        return -1;
    }

    private void MoveActive(bool isSource, IReadOnlyList<TItem> view, int index, bool extend)
    {
        if (index < 0 || index >= view.Count) return;

        var key = Key(view[index]);
        var hi = Highlight(isSource);

        if (extend)
        {
            var anchor = isSource ? _sourceAnchor : _selectedAnchor;
            if (anchor < 0)
            {
                anchor = index;
                SetAnchor(isSource, index);
            }
            SelectRange(isSource, view, anchor, index);
        }
        else
        {
            hi.Clear();
            hi.Add(key);
            SetAnchor(isSource, index);
        }

        SetActive(isSource, key);
    }

    private void ToggleHighlight(bool isSource, object key)
    {
        var hi = Highlight(isSource);
        if (!hi.Add(key)) hi.Remove(key);
    }

    // ---- Moves ----

    private Task MoveHighlighted(bool fromSource)
    {
        var hi = Highlight(fromSource);
        if (hi.Count == 0) return Task.CompletedTask;

        var origin = (fromSource ? Source : Selected) ?? Enumerable.Empty<TItem>();
        var toMove = origin.Where(i => hi.Contains(Key(i))).ToList();
        return MoveItems(toMove, fromSource);
    }

    private Task MoveAll(bool fromSource)
    {
        // Move only the currently visible (filtered) items.
        var toMove = View(fromSource).ToList();
        return MoveItems(toMove, fromSource);
    }

    private async Task MoveItems(IReadOnlyList<TItem> items, bool fromSource)
    {
        if (Disabled || items.Count == 0) return;

        var movingKeys = new HashSet<object>(items.Select(Key));

        var origin = ((fromSource ? Source : Selected) ?? Enumerable.Empty<TItem>()).ToList();
        var destination = ((fromSource ? Selected : Source) ?? Enumerable.Empty<TItem>()).ToList();

        origin.RemoveAll(i => movingKeys.Contains(Key(i)));
        destination.AddRange(items);

        // Clear highlight/active for the items that just left the origin pane.
        var originHighlight = Highlight(fromSource);
        foreach (var key in movingKeys) originHighlight.Remove(key);

        if (fromSource && _sourceActive is not null && movingKeys.Contains(_sourceActive)) _sourceActive = null;
        if (!fromSource && _selectedActive is not null && movingKeys.Contains(_selectedActive)) _selectedActive = null;

        var newSource = fromSource ? origin : destination;
        var newSelected = fromSource ? destination : origin;

        if (SourceChanged.HasDelegate) await SourceChanged.InvokeAsync(newSource);
        if (SelectedChanged.HasDelegate) await SelectedChanged.InvokeAsync(newSelected);
        await OnChanged.InvokeAsync();
        await OnMoved.InvokeAsync(new DualListboxMoveEventArgs<TItem>(
            items,
            fromSource ? MoveDirection.ToSelected : MoveDirection.ToSource));
    }
}
