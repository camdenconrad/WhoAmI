using WhoAmI.Models;

namespace WhoAmI.Core;

/// <summary>
/// Dedicated class for performing computations on the personality manifold
/// to extract patterns, find personality types, and analyze trait relationships.
/// </summary>
public class PersonalityAnalyzer
{
    private readonly PersonalityManifold _manifold;

    public PersonalityAnalyzer(PersonalityManifold manifold)
    {
        _manifold = manifold;
    }

    /// <summary>
    /// Get the dominant traits across all contexts.
    /// </summary>
    public IEnumerable<(string TraitId, float TotalStrength)> GetDominantTraits(int topN = 10)
    {
        return _manifold.GetSparseVector()
            .Select(kvp => (
                TraitId: kvp.Key,
                TotalStrength: kvp.Value.Sum(a => a.Strength)
            ))
            .OrderByDescending(x => x.TotalStrength)
            .Take(topN);
    }

    /// <summary>
    /// Get the dominant traits for a specific context.
    /// </summary>
    public IEnumerable<(string TraitId, float Strength)> GetContextualDominantTraits(
        SituationalContext context,
        int topN = 5)
    {
        return _manifold.GetSparseVector()
            .SelectMany(kvp => kvp.Value
                .Where(a => a.Context.HasFlag(context))
                .Select(a => (TraitId: kvp.Key, a.Strength)))
            .GroupBy(x => x.TraitId)
            .Select(g => (TraitId: g.Key, TotalStrength: g.Sum(x => x.Strength)))
            .OrderByDescending(x => x.TotalStrength)
            .Take(topN);
    }

    /// <summary>
    /// Find trait pairs that co-activate frequently (contradictions and synergies).
    /// </summary>
    public IEnumerable<CoActivationPattern> GetCoActivationPatterns(int topN = 10)
    {
        return _manifold.GetCoActivationMatrix()
            .Where(kvp => kvp.Value > 0)
            .Select(kvp =>
            {
                var (trait1, trait2) = kvp.Key;
                var probability = _manifold.GetCoActivationProbability(trait1, trait2);
                return new CoActivationPattern(trait1, trait2, kvp.Value, probability);
            })
            .OrderByDescending(p => p.Count)
            .Take(topN);
    }

    /// <summary>
    /// Calculate trait diversity - how many different traits are activated.
    /// High diversity = complex, nuanced personality.
    /// </summary>
    public int GetTraitDiversity()
    {
        return _manifold.GetSparseVector().Count;
    }

    /// <summary>
    /// Calculate context variance - how much personality changes across contexts.
    /// High variance = context-dependent behavior.
    /// </summary>
    public float GetContextVariance()
    {
        var profiles = _manifold.GetContextualProfiles();
        if (profiles.Count < 2) return 0f;

        var allTraits = _manifold.GetSparseVector().Keys.ToHashSet();
        float totalVariance = 0f;

        foreach (var traitId in allTraits)
        {
            var contextStrengths = new List<float>();
            foreach (var context in Enum.GetValues<SituationalContext>())
            {
                if (context == SituationalContext.None) continue;
                contextStrengths.Add(_manifold.GetContextualStrength(traitId, context));
            }

            if (contextStrengths.Any())
            {
                var mean = contextStrengths.Average();
                var variance = contextStrengths.Sum(s => (s - mean) * (s - mean)) / contextStrengths.Count;
                totalVariance += variance;
            }
        }

        return totalVariance / allTraits.Count;
    }

    /// <summary>
    /// Identify contradiction tolerance - how often opposite traits co-activate.
    /// This is a key feature that distinguishes from Big Five bucketing.
    /// </summary>
    public float GetContradictionTolerance()
    {
        var coActivations = _manifold.GetCoActivationMatrix();
        if (!coActivations.Any()) return 0f;

        // Pairs that might be considered "opposites" based on naming patterns
        var contradictionPairs = new[]
        {
            ("control_seeking", "trust_delegating"),
            ("reward_seeking", "loss_avoidance"),
            ("risk_sampling", "risk_filtering"),
            ("analytical_parsing", "holistic_sensing"),
            ("bottom_up", "top_down"),
            ("rule_first", "exception_first"),
            ("future_projection", "present_situational"),
            ("deadline_driven", "steady_pace"),
            ("challenge_seeking", "stability_seeking"),
            ("initiates_interaction", "responds_to_interaction"),
            ("explicit_verbalization", "context_dependent_comm")
        };

        var contradictionCount = 0;
        var contradictionSum = 0;

        foreach (var (trait1, trait2) in contradictionPairs)
        {
            if (coActivations.ContainsKey((trait1, trait2)))
            {
                contradictionCount++;
                contradictionSum += coActivations[(trait1, trait2)];
            }
            else if (coActivations.ContainsKey((trait2, trait1)))
            {
                contradictionCount++;
                contradictionSum += coActivations[(trait2, trait1)];
            }
        }

        return contradictionCount > 0 ? (float)contradictionSum / contradictionCount : 0f;
    }

    /// <summary>
    /// Generate a personality profile summary.
    /// </summary>
    public PersonalityProfile GenerateProfile()
    {
        // Get ALL traits (not just top N) for MBTI mapping
        var allTraits = _manifold.GetSparseVector()
            .Select(kvp => (
                TraitId: kvp.Key,
                TotalStrength: kvp.Value.Sum(a => a.Strength)
            ))
            .OrderByDescending(x => x.TotalStrength)
            .ToList();

        return new PersonalityProfile(
            DominantTraits: allTraits.Take(5).ToList(),
            AllTraits: allTraits,
            ContextualProfiles: GetAllContextualProfiles(),
            CoActivationPatterns: GetCoActivationPatterns(10).ToList(),
            TraitDiversity: GetTraitDiversity(),
            ContextVariance: GetContextVariance(),
            ContradictionTolerance: GetContradictionTolerance()
        );
    }

    private Dictionary<SituationalContext, List<(string TraitId, float Strength)>> GetAllContextualProfiles()
    {
        var result = new Dictionary<SituationalContext, List<(string TraitId, float Strength)>>();

        foreach (var context in Enum.GetValues<SituationalContext>())
        {
            if (context == SituationalContext.None) continue;

            var traits = GetContextualDominantTraits(context, 5).ToList();
            if (traits.Any())
            {
                result[context] = traits;
            }
        }

        return result;
    }
}

/// <summary>
/// Represents a co-activation pattern between two traits.
/// </summary>
public record CoActivationPattern(
    string Trait1,
    string Trait2,
    int Count,
    float Probability
);

/// <summary>
/// Complete personality profile with computed metrics.
/// </summary>
public record PersonalityProfile(
    List<(string TraitId, float TotalStrength)> DominantTraits,
    List<(string TraitId, float TotalStrength)> AllTraits,
    Dictionary<SituationalContext, List<(string TraitId, float Strength)>> ContextualProfiles,
    List<CoActivationPattern> CoActivationPatterns,
    int TraitDiversity,
    float ContextVariance,
    float ContradictionTolerance
);
