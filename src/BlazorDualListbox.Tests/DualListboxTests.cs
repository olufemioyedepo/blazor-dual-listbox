using Bunit;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorDualListbox.Tests;

public class DualListboxTests : TestContext
{
    private static IEnumerable<string> Abc() => new List<string> { "A", "B", "C" };

    // Renders the first (source) pane's option elements.
    private static IReadOnlyList<AngleSharp.Dom.IElement> Options(
        IRenderedComponent<DualListbox<string>> cut, bool source)
        => cut.FindAll(".dl-pane")[source ? 0 : 1].QuerySelectorAll("li.dl-option").ToList();

    [Fact]
    public void Renders_items_in_both_panes()
    {
        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, Abc())
            .Add(x => x.Selected, new List<string> { "X" }));

        Assert.Equal(3, Options(cut, source: true).Count);
        Assert.Single(Options(cut, source: false));
    }

    [Fact]
    public void Uses_TextSelector_for_display()
    {
        Func<Person, string> text = person => person.Name;
        var cut = RenderComponent<DualListbox<Person>>(p => p
            .Add(x => x.Source, new List<Person> { new(1, "Alice") })
            .Add(x => x.TextSelector, text));

        Assert.Contains("Alice", cut.Markup);
    }

    [Fact]
    public void Shows_EmptyText_when_a_pane_is_empty()
    {
        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, Abc())
            .Add(x => x.Selected, new List<string>())
            .Add(x => x.EmptyText, "Nothing here"));

        Assert.Contains("Nothing here", cut.Markup);
    }

    [Fact]
    public void Clicking_option_toggles_aria_selected()
    {
        var cut = RenderComponent<DualListbox<string>>(p => p.Add(x => x.Source, Abc()));

        Options(cut, source: true)[1].Click();

        Assert.Equal("true", Options(cut, source: true)[1].GetAttribute("aria-selected"));
        Assert.Equal("false", Options(cut, source: true)[0].GetAttribute("aria-selected"));
    }

    [Fact]
    public void Move_selected_right_moves_highlighted_item_and_raises_callbacks()
    {
        var source = Abc().ToList();
        var selected = new List<string>();
        DualListboxMoveEventArgs<string>? moved = null;
        var changedFired = false;

        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, source)
            .Add(x => x.Selected, selected)
            .Add(x => x.SourceChanged, (IEnumerable<string> v) => source = v.ToList())
            .Add(x => x.SelectedChanged, (IEnumerable<string> v) => selected = v.ToList())
            .Add(x => x.OnChanged, () => changedFired = true)
            .Add(x => x.OnMoved, (DualListboxMoveEventArgs<string> e) => moved = e));

        Options(cut, source: true)[1].Click(); // highlight "B"
        cut.Find("button[title='Move selected right']").Click();

        Assert.Equal(new[] { "A", "C" }, source);
        Assert.Equal(new[] { "B" }, selected);
        Assert.True(changedFired);
        Assert.NotNull(moved);
        Assert.Equal(MoveDirection.ToSelected, moved!.Direction);
        Assert.Equal(new[] { "B" }, moved.Items);
    }

    [Fact]
    public void Double_click_moves_single_item_and_updates_dom_when_rebound()
    {
        var source = Abc().ToList();
        var selected = new List<string>();

        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, source)
            .Add(x => x.Selected, selected)
            .Add(x => x.SourceChanged, (IEnumerable<string> v) => source = v.ToList())
            .Add(x => x.SelectedChanged, (IEnumerable<string> v) => selected = v.ToList()));

        Options(cut, source: true)[1].DoubleClick(); // move "B"

        // Simulate the parent re-rendering with the updated bound collections.
        cut.SetParametersAndRender(p => p
            .Add(x => x.Source, source)
            .Add(x => x.Selected, selected));

        Assert.Equal(2, Options(cut, source: true).Count);
        Assert.Single(Options(cut, source: false));
        Assert.Contains("B", cut.FindAll(".dl-pane")[1].TextContent);
    }

    [Fact]
    public void Move_all_right_moves_every_item()
    {
        var source = Abc().ToList();
        var selected = new List<string>();

        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, source)
            .Add(x => x.Selected, selected)
            .Add(x => x.SourceChanged, (IEnumerable<string> v) => source = v.ToList())
            .Add(x => x.SelectedChanged, (IEnumerable<string> v) => selected = v.ToList()));

        cut.Find("button[title='Move all right']").Click();

        Assert.Empty(source);
        Assert.Equal(new[] { "A", "B", "C" }, selected);
    }

    [Fact]
    public void Move_all_only_moves_filtered_items()
    {
        var source = new List<string> { "Apple", "Banana", "Cherry" };
        var selected = new List<string>();

        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, source)
            .Add(x => x.Selected, selected)
            .Add(x => x.Filterable, true)
            .Add(x => x.SourceChanged, (IEnumerable<string> v) => source = v.ToList())
            .Add(x => x.SelectedChanged, (IEnumerable<string> v) => selected = v.ToList()));

        cut.FindAll(".dl-pane")[0].QuerySelector("input.dl-filter")!.Input("an"); // matches Banana only
        cut.Find("button[title='Move all right']").Click();

        Assert.Equal(new[] { "Banana" }, selected);
        Assert.Equal(new[] { "Apple", "Cherry" }, source);
    }

    [Fact]
    public void Filter_hides_non_matching_options()
    {
        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, new List<string> { "Apple", "Banana", "Cherry" })
            .Add(x => x.Filterable, true));

        cut.FindAll(".dl-pane")[0].QuerySelector("input.dl-filter")!.Input("err"); // Cherry

        var options = Options(cut, source: true);
        Assert.Single(options);
        Assert.Contains("Cherry", options[0].TextContent);
    }

    [Fact]
    public void Ctrl_click_selects_multiple_items()
    {
        var source = Abc().ToList();
        var selected = new List<string>();

        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, source)
            .Add(x => x.Selected, selected)
            .Add(x => x.SourceChanged, (IEnumerable<string> v) => source = v.ToList())
            .Add(x => x.SelectedChanged, (IEnumerable<string> v) => selected = v.ToList()));

        Options(cut, source: true)[0].Click();
        Options(cut, source: true)[2].Click(new MouseEventArgs { CtrlKey = true });
        cut.Find("button[title='Move selected right']").Click();

        Assert.Equal(new[] { "A", "C" }, selected);
        Assert.Equal(new[] { "B" }, source);
    }

    [Fact]
    public void Shift_click_selects_a_range()
    {
        var source = new List<string> { "A", "B", "C", "D" };
        var selected = new List<string>();

        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, source)
            .Add(x => x.Selected, selected)
            .Add(x => x.SourceChanged, (IEnumerable<string> v) => source = v.ToList())
            .Add(x => x.SelectedChanged, (IEnumerable<string> v) => selected = v.ToList()));

        Options(cut, source: true)[1].Click(); // anchor at "B"
        Options(cut, source: true)[3].Click(new MouseEventArgs { ShiftKey = true }); // extend to "D"
        cut.Find("button[title='Move selected right']").Click();

        Assert.Equal(new[] { "B", "C", "D" }, selected);
        Assert.Equal(new[] { "A" }, source);
    }

    [Fact]
    public void Enter_key_moves_highlighted_item()
    {
        var source = Abc().ToList();
        var selected = new List<string>();

        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, source)
            .Add(x => x.Selected, selected)
            .Add(x => x.SourceChanged, (IEnumerable<string> v) => source = v.ToList())
            .Add(x => x.SelectedChanged, (IEnumerable<string> v) => selected = v.ToList()));

        Options(cut, source: true)[0].Click(); // highlight "A"
        cut.FindAll("ul.dl-list")[0].KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal(new[] { "A" }, selected);
    }

    [Fact]
    public void ValueSelector_identifies_items_without_value_equality()
    {
        var alice = new MutablePerson { Id = 1, Name = "Alice" };
        var bob = new MutablePerson { Id = 2, Name = "Bob" };
        var source = new List<MutablePerson> { alice, bob };
        var selected = new List<MutablePerson>();
        Func<MutablePerson, object> key = person => person.Id;

        var cut = RenderComponent<DualListbox<MutablePerson>>(p => p
            .Add(x => x.Source, source)
            .Add(x => x.Selected, selected)
            .Add(x => x.ValueSelector, key)
            .Add(x => x.TextSelector, (Func<MutablePerson, string>)(person => person.Name))
            .Add(x => x.SourceChanged, (IEnumerable<MutablePerson> v) => source = v.ToList())
            .Add(x => x.SelectedChanged, (IEnumerable<MutablePerson> v) => selected = v.ToList()));

        cut.FindAll(".dl-pane")[0].QuerySelectorAll("li.dl-option")[0].DoubleClick();

        Assert.Single(selected);
        Assert.Equal(1, selected[0].Id);
        Assert.Single(source);
        Assert.Equal(2, source[0].Id);
    }


    [Fact]
    public void List_has_expected_aria_roles()
    {
        var cut = RenderComponent<DualListbox<string>>(p => p.Add(x => x.Source, Abc()));

        var list = cut.FindAll("ul.dl-list")[0];
        Assert.Equal("listbox", list.GetAttribute("role"));
        Assert.Equal("true", list.GetAttribute("aria-multiselectable"));
        Assert.Equal("option", Options(cut, source: true)[0].GetAttribute("role"));
    }

    [Fact]
    public void Button_classes_compose_shared_and_per_button_classes()
    {
        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, Abc())
            .Add(x => x.Selected, new List<string> { "X" })
            .Add(x => x.ButtonClass, "shared")
            .Add(x => x.AddSingleButtonClass, "add-single")
            .Add(x => x.AddAllButtonClass, "add-all")
            .Add(x => x.RemoveAllButtonClass, "remove-all")
            .Add(x => x.RemoveSingleButtonClass, "remove-single"));

        // Class is composed as: built-in "dl-btn" + shared ButtonClass + the per-button class.
        Assert.Equal("dl-btn shared add-single",
            cut.Find("button[title='Move selected right']").GetAttribute("class"));
        Assert.Equal("dl-btn shared add-all",
            cut.Find("button[title='Move all right']").GetAttribute("class"));
        Assert.Equal("dl-btn shared remove-all",
            cut.Find("button[title='Move all left']").GetAttribute("class"));
        Assert.Equal("dl-btn shared remove-single",
            cut.Find("button[title='Move selected left']").GetAttribute("class"));
    }

    [Fact]
    public void Buttons_keep_only_dl_btn_when_no_custom_classes_given()
    {
        var cut = RenderComponent<DualListbox<string>>(p => p
            .Add(x => x.Source, Abc())
            .Add(x => x.Selected, new List<string> { "X" }));

        Assert.Equal("dl-btn", cut.Find("button[title='Move selected right']").GetAttribute("class"));
        Assert.Equal("dl-btn", cut.Find("button[title='Move all right']").GetAttribute("class"));
    }

    private record Person(int Id, string Name);

    private sealed class MutablePerson
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
