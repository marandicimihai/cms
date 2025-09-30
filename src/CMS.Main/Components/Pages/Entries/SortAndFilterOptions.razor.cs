
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Entries;

public partial class SortAndFilterOptions : ComponentBase
{
    [Parameter]
    public EventCallback<(string, bool)> OnSortChanged { get; set; }

    [Parameter]
    public List<string> SortableProperties { get; set; } = [];

    [Parameter]
    public string InitialSortByProperty { get; set; } = "CreatedAt";

    [Parameter]
    public bool InitialDescending { get; set; } = false;

    protected override void OnInitialized()
    {
        sortByPropertyBacking = InitialSortByProperty;
        descendingBacking = InitialDescending;
    }

    #region Sorting

    private string sortByPropertyBacking = string.Empty;
    private string SortByProperty
    {
        get => sortByPropertyBacking;
        set
        {
            if (value == sortByPropertyBacking) return;
            sortByPropertyBacking = value;
            if (OnSortChanged.HasDelegate)
            {
                OnSortChanged.InvokeAsync((SortByProperty, Descending));
            }
        }
    }

    private bool descendingBacking = false;
    private bool Descending
    {
        get => descendingBacking;
        set
        {
            if (value == descendingBacking) return;
            descendingBacking = value;
            if (OnSortChanged.HasDelegate)
            {
                OnSortChanged.InvokeAsync((SortByProperty, Descending));
            }
        }
    }

    #endregion
}
