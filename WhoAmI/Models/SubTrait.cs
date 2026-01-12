namespace WhoAmI.Models;

/// <summary>
/// Represents a significant non-dominant cognitive function.
/// Example: ENFP with strong Ti usage.
/// </summary>
public record SubTrait(
    string Function,        // e.g., "Ti"
    float Strength,         // Weighted score
    string Description      // Human-readable description
);
