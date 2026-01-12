namespace WhoAmI.ViewModels;

/// <summary>
/// Wrapper class for displaying trait information in the UI.
/// Needed because Avalonia compiled bindings don't work with tuples.
/// </summary>
public class TraitDisplayItem
{
    public string TraitId { get; set; } = string.Empty;
    public float Strength { get; set; }
}
