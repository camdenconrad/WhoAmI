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

    /// <summary>
    /// TENSOR REGRESSION: Project a query trait pattern onto the manifold to predict activation strength.
    /// Uses the learned co-activation structure (the manifold) to perform regression.
    /// </summary>
    /// <param name="queryTraits">Input trait activations to project</param>
    /// <param name="targetTrait">The trait we want to predict</param>
    /// <returns>Predicted activation strength for the target trait</returns>
    public float TensorRegression(Dictionary<string, float> queryTraits, string targetTrait)
    {
        if (!_traitActivations.ContainsKey(targetTrait))
            return 0f;

        // If target trait is in query, return it directly
        if (queryTraits.ContainsKey(targetTrait))
            return queryTraits[targetTrait];

        // Compute influence from co-activated traits
        var prediction = 0f;
        var totalWeight = 0f;

        foreach (var (queryTraitId, queryStrength) in queryTraits)
        {
            // Get co-activation probability: P(target | query trait active)
            var coActivationProb = GetCoActivationProbability(queryTraitId, targetTrait);

            if (coActivationProb > 0)
            {
                // Weight by both query strength and co-activation probability
                var weight = queryStrength * coActivationProb;

                // Get average activation strength when these co-occur
                var targetActivations = _traitActivations[targetTrait];
                var avgTargetStrength = targetActivations.Average(a => a.Strength);

                prediction += weight * avgTargetStrength;
                totalWeight += weight;
            }
        }

        return totalWeight > 0 ? prediction / totalWeight : 0f;
    }

    /// <summary>
    /// CONTEXTUALIZED TENSOR REGRESSION: Predict trait activation in a specific context.
    /// Projects query through the manifold structure while respecting contextual variations.
    /// </summary>
    public float ContextualTensorRegression(
        Dictionary<string, float> queryTraits,
        string targetTrait,
        SituationalContext context)
    {
        if (!_traitActivations.ContainsKey(targetTrait))
            return 0f;

        // If we have direct contextual data, use it
        if (_contextualProfiles.ContainsKey(context) &&
            _contextualProfiles[context].ContainsKey(targetTrait))
        {
            // Weight direct observation heavily but still consider co-activations
            var directObservation = _contextualProfiles[context][targetTrait];
            var tensorPrediction = TensorRegression(queryTraits, targetTrait);

            // Blend: favor direct observation but smooth with tensor prediction
            return 0.7f * directObservation + 0.3f * tensorPrediction;
        }

        // No direct observation - use tensor regression weighted by context
        var prediction = 0f;
        var totalWeight = 0f;

        foreach (var (queryTraitId, queryStrength) in queryTraits)
        {
            var coActivationProb = GetCoActivationProbability(queryTraitId, targetTrait);

            if (coActivationProb > 0)
            {
                // Get context-specific strength for target trait
                var contextualTargetActivations = _traitActivations[targetTrait]
                    .Where(a => a.Context.HasFlag(context))
                    .ToList();

                if (contextualTargetActivations.Any())
                {
                    var avgContextualStrength = contextualTargetActivations.Average(a => a.Strength);
                    var weight = queryStrength * coActivationProb;

                    prediction += weight * avgContextualStrength;
                    totalWeight += weight;
                }
            }
        }

        return totalWeight > 0 ? prediction / totalWeight : 0f;
    }

    /// <summary>
    /// MULTI-TARGET TENSOR REGRESSION: Predict multiple traits simultaneously.
    /// Returns a full trait vector prediction given partial observations.
    /// </summary>
    public Dictionary<string, float> MultiTargetTensorRegression(
        Dictionary<string, float> queryTraits,
        IEnumerable<string>? targetTraits = null)
    {
        // If no target traits specified, predict all known traits
        var targets = targetTraits?.ToList() ?? _traitActivations.Keys.ToList();

        var predictions = new Dictionary<string, float>();

        foreach (var target in targets)
        {
            if (!queryTraits.ContainsKey(target)) // Don't re-predict observed traits
            {
                predictions[target] = TensorRegression(queryTraits, target);
            }
            else
            {
                predictions[target] = queryTraits[target];
            }
        }

        return predictions;
    }
}
