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
    /// Calculate TENSION: the average strength of conflicting trait pairs when they co-occur.
    /// High tension = person exhibits opposing behaviors simultaneously.
    /// </summary>
    public float GetTension()
    {
        var coActivations = _manifold.GetCoActivationMatrix();
        if (!coActivations.Any()) return 0f;

        // Define opposing trait pairs
        var opposingPairs = new[]
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

        var tensionSum = 0f;
        var pairCount = 0;

        foreach (var (trait1, trait2) in opposingPairs)
        {
            // Get the activation strengths for both traits
            var trait1Activations = _manifold.GetSparseVector().GetValueOrDefault(trait1);
            var trait2Activations = _manifold.GetSparseVector().GetValueOrDefault(trait2);

            if (trait1Activations != null && trait2Activations != null)
            {
                // Calculate the product of their average strengths when they co-occur
                var trait1Strength = trait1Activations.Sum(a => a.Strength);
                var trait2Strength = trait2Activations.Sum(a => a.Strength);

                // Tension is higher when both opposing traits are strongly activated
                var pairTension = (float)Math.Sqrt(trait1Strength * trait2Strength);

                // Weight by co-occurrence
                var coOccurs = coActivations.ContainsKey((trait1, trait2)) ? coActivations[(trait1, trait2)] :
                               coActivations.ContainsKey((trait2, trait1)) ? coActivations[(trait2, trait1)] : 0;

                if (coOccurs > 0)
                {
                    tensionSum += pairTension * coOccurs;
                    pairCount += coOccurs;
                }
            }
        }

        return pairCount > 0 ? tensionSum / pairCount : 0f;
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

        // Discover latent traits dynamically
        var latentTraits = DiscoverLatentTraits();

        return new PersonalityProfile(
            DominantTraits: allTraits.Take(5).ToList(),
            AllTraits: allTraits,
            ContextualProfiles: GetAllContextualProfiles(),
            CoActivationPatterns: GetCoActivationPatterns(10).ToList(),
            TraitDiversity: GetTraitDiversity(),
            ContextVariance: GetContextVariance(),
            ContradictionTolerance: GetContradictionTolerance(),
            Tension: GetTension(),
            LatentTraits: latentTraits
        );
    }

    /// <summary>
    /// LATENT TRAIT DISCOVERY: finds emergent dimensions from co-activation patterns.
    /// These are NOT predefined traits, but behavioral clusters discovered in the data.
    /// Dynamic dimensions that emerge from the manifold topology.
    /// </summary>
    public List<LatentTrait> DiscoverLatentTraits(int minClusterSize = 3, float minCoActivation = 0.3f)
    {
        var coActivations = _manifold.GetCoActivationMatrix();
        var sparseVector = _manifold.GetSparseVector();

        if (!coActivations.Any()) return new List<LatentTrait>();

        // Build adjacency lists based on co-activation strength
        var adjacency = new Dictionary<string, List<(string Trait, float Strength)>>();

        foreach (var ((trait1, trait2), count) in coActivations)
        {
            var prob1 = _manifold.GetCoActivationProbability(trait1, trait2);
            var prob2 = _manifold.GetCoActivationProbability(trait2, trait1);
            var strength = (prob1 + prob2) / 2f;

            if (strength >= minCoActivation)
            {
                if (!adjacency.ContainsKey(trait1))
                    adjacency[trait1] = new List<(string, float)>();
                if (!adjacency.ContainsKey(trait2))
                    adjacency[trait2] = new List<(string, float)>();

                adjacency[trait1].Add((trait2, strength));
                adjacency[trait2].Add((trait1, strength));
            }
        }

        // Find clusters using greedy community detection
        var visited = new HashSet<string>();
        var clusters = new List<List<string>>();

        foreach (var startTrait in adjacency.Keys.OrderByDescending(t =>
            sparseVector.GetValueOrDefault(t)?.Sum(a => a.Strength) ?? 0))
        {
            if (visited.Contains(startTrait)) continue;

            var cluster = new List<string> { startTrait };
            visited.Add(startTrait);

            // Expand cluster greedily
            var toExplore = new Queue<string>();
            toExplore.Enqueue(startTrait);

            while (toExplore.Any() && cluster.Count < 10) // Limit cluster size
            {
                var current = toExplore.Dequeue();

                if (!adjacency.ContainsKey(current)) continue;

                foreach (var (neighbor, strength) in adjacency[current]
                    .Where(x => !visited.Contains(x.Trait))
                    .OrderByDescending(x => x.Strength)
                    .Take(3)) // Only take top 3 connections
                {
                    if (strength >= minCoActivation)
                    {
                        cluster.Add(neighbor);
                        visited.Add(neighbor);
                        toExplore.Enqueue(neighbor);
                    }
                }
            }

            if (cluster.Count >= minClusterSize)
            {
                clusters.Add(cluster);
            }
        }

        // Convert clusters to latent traits
        var latentTraits = new List<LatentTrait>();
        for (int i = 0; i < clusters.Count; i++)
        {
            var cluster = clusters[i];

            // Calculate cluster strength
            var clusterStrength = cluster.Sum(t =>
                sparseVector.GetValueOrDefault(t)?.Sum(a => a.Strength) ?? 0) / cluster.Count;

            // Calculate internal cohesion
            var internalEdges = 0;
            var totalStrength = 0f;
            foreach (var t1 in cluster)
            {
                foreach (var t2 in cluster)
                {
                    if (t1 != t2 && coActivations.ContainsKey((t1, t2)))
                    {
                        internalEdges++;
                        totalStrength += _manifold.GetCoActivationProbability(t1, t2);
                    }
                }
            }
            var cohesion = internalEdges > 0 ? totalStrength / internalEdges : 0f;

            latentTraits.Add(new LatentTrait(
                Id: $"latent_{i + 1}",
                Name: $"Latent Dimension {i + 1}",
                ComponentTraits: cluster,
                Strength: clusterStrength,
                Cohesion: cohesion
            ));
        }

        return latentTraits.OrderByDescending(lt => lt.Strength).ToList();
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
/// Represents a dynamically discovered latent trait dimension.
/// These emerge from co-activation patterns in the data.
/// </summary>
public record LatentTrait(
    string Id,
    string Name,
    List<string> ComponentTraits,
    float Strength,
    float Cohesion
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
    float ContradictionTolerance,
    float Tension,
    List<LatentTrait> LatentTraits
);
