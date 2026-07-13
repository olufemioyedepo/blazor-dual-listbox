namespace BlazorDualListbox;

/// <summary>
/// Indicates the direction in which items were moved within a <see cref="DualListbox{TItem}"/>.
/// </summary>
public enum MoveDirection
{
    /// <summary>Items moved from the source (available) list into the selected list.</summary>
    ToSelected,

    /// <summary>Items moved from the selected list back into the source (available) list.</summary>
    ToSource
}

/// <summary>
/// Describes a move that occurred within a <see cref="DualListbox{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The type of item held by the list boxes.</typeparam>
public sealed class DualListboxMoveEventArgs<TItem>
{
    /// <summary>Initializes a new instance of the <see cref="DualListboxMoveEventArgs{TItem}"/> class.</summary>
    /// <param name="items">The items that were moved.</param>
    /// <param name="direction">The direction of the move.</param>
    public DualListboxMoveEventArgs(IReadOnlyList<TItem> items, MoveDirection direction)
    {
        Items = items;
        Direction = direction;
    }

    /// <summary>Gets the items that were moved.</summary>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>Gets the direction of the move.</summary>
    public MoveDirection Direction { get; }
}
