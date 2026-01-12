using WhoAmI.Models;

namespace WhoAmI.Core;

/// <summary>
/// Sparse multi-hot representation of a person's behavioral profile.
/// NOT a simple weighted sum - preserves co-activation patterns and context.
/// </summary>
public class PersonalityManifold
{
    // Sparse vector: only activated dimensions are stored
    private readonly Dictionary<string, List<ContextualActivation>> _traitActivations = new();

    // Co-activation matrix: tracks which traits appear together
    private readonly Dictionary<(string, string), int> _coActivationCounts = new();

    // Context-specific profiles: allows different trait expression in different situations
    private readonly Dictionary<SituationalContext, Dictionary<string, float>> _contextualProfiles = new();

    /// <summary>
    /// Records a response WITHOUT collapsing to a single dimension.
    /// Preserves context and co-activation patterns.
    /// </summary>
    public void RecordResponse(VignetteOption chosen, SituationalContext context)
    {
        var activatedTraits = chosen.TraitActivations.Keys.ToList();

        // Record each trait activation with context
        foreach (var (traitId, strength) in chosen.TraitActivations)
        {
            if (!_traitActivations.ContainsKey(traitId))
                _traitActivations[traitId] = new List<ContextualActivation>();

            _traitActivations[traitId].Add(new ContextualActivation(traitId, strength, context));

            // Update context-specific profile
            if (!_contextualProfiles.ContainsKey(context))
                _contextualProfiles[context] = new Dictionary<string, float>();

            if (!_contextualProfiles[context].ContainsKey(traitId))
                _contextualProfiles[context][traitId] = 0;

            _contextualProfiles[context][traitId] += strength;
        }

        // Track co-activation patterns (key to preserving "two things can be true")
        for (int i = 0; i < activatedTraits.Count; i++)
        {
            for (int j = i + 1; j < activatedTraits.Count; j++)
            {
                var pair = (activatedTraits[i], activatedTraits[j]);
                _coActivationCounts[pair] = _coActivationCounts.GetValueOrDefault(pair) + 1;
            }
        }
    }

    /// <summary>
    /// Get trait strength in a specific context (not global average).
    /// </summary>
    public float GetContextualStrength(string traitId, SituationalContext context)
    {
        if (!_contextualProfiles.ContainsKey(context) ||
            !_contextualProfiles[context].ContainsKey(traitId))
            return 0f;

        return _contextualProfiles[context][traitId];
    }

    /// <summary>
    /// Calculate co-activation probability: P(trait2 | trait1 active).
    /// This captures conditional relationships between traits.
    /// </summary>
    public float GetCoActivationProbability(string trait1, string trait2)
    {
        var pair = (trait1, trait2);
        if (!_coActivationCounts.ContainsKey(pair))
            return 0f;

        var coCount = _coActivationCounts[pair];
        var trait1Count = _traitActivations.GetValueOrDefault(trait1)?.Count ?? 0;

        return trait1Count > 0 ? (float)coCount / trait1Count : 0f;
    }

    /// <summary>
    /// Get the raw sparse vector (all activations with context preserved).
    /// </summary>
    public IReadOnlyDictionary<string, List<ContextualActivation>> GetSparseVector() => _traitActivations;

    /// <summary>
    /// Get co-activation patterns (the manifold topology).
    /// </summary>
    public IReadOnlyDictionary<(string, string), int> GetCoActivationMatrix() => _coActivationCounts;

    /// <summary>
    /// Get context-specific profiles.
    /// </summary>
    public IReadOnlyDictionary<SituationalContext, Dictionary<string, float>> GetContextualProfiles() => _contextualProfiles;
}
