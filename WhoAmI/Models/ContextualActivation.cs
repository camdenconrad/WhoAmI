namespace WhoAmI.Models;

/// <summary>
/// Represents a single trait activation within a specific context.
/// </summary>
public record ContextualActivation(
    string TraitId,
    float Strength,
    SituationalContext Context
);
