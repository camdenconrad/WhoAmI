using System.Collections.Immutable;

namespace WhoAmI.Models;

/// <summary>
/// A behavioral vignette that tests specific trait activations.
/// Uses forced-choice, concrete situations rather than self-assessment.
/// </summary>
public record Vignette(
    string Id,
    string Scenario,
    SituationalContext Context,
    ImmutableArray<VignetteOption> Options
);
