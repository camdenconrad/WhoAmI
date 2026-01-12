using System.Collections.Immutable;

namespace WhoAmI.Models;

/// <summary>
/// Each option activates specific trait dimensions with different weights.
/// Multiple traits can activate simultaneously - this is the key to avoiding bucketing.
/// </summary>
public record VignetteOption(
    string Label,
    string Description,
    ImmutableDictionary<string, float> TraitActivations // TraitId -> activation strength [0-1]
);
