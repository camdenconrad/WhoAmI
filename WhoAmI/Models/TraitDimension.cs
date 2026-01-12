namespace WhoAmI.Models;

/// <summary>
/// Represents a single behavioral trait dimension in our high-dimensional space.
/// Traits are NOT opposites - they can co-activate.
/// </summary>
public record TraitDimension(
    string Id,
    string Name,
    string Category,
    string Description
);
