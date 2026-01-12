using WhoAmI.Core;
using WhoAmI.Models;

namespace WhoAmI.Services;

/// <summary>
/// Console-based result presenter.
/// </summary>
public class ConsoleResultPresenter : IResultPresenter
{
    public void PresentResults(PersonalityProfile profile)
    {
        Console.WriteLine("\n\n=== Your Cognitive Manifold ===\n");
        Console.WriteLine("This is NOT a bucket or category. It's a high-dimensional representation.");
        Console.WriteLine("Contradictions and context-dependence are preserved, not averaged away.\n");

        // MBTI Type
        var mapper = new MBTIMapper(profile);
        var mbtiType = mapper.MapToMBTI();
        var mbtiInfo = MBTIPersonality.Definitions[mbtiType];

        Console.WriteLine("--- 16 Personalities Type ---\n");
        Console.WriteLine($"{mbtiType} - {mbtiInfo.Nickname}");
        Console.WriteLine($"{mbtiInfo.Description}");
        Console.WriteLine();

        // Dominant traits
        Console.WriteLine("--- Overall Dominant Traits ---\n");
        foreach (var (traitId, strength) in profile.DominantTraits)
        {
            Console.WriteLine($"  {traitId}: {strength:F2}");
        }
        Console.WriteLine();

        // Context-specific profiles
        Console.WriteLine("--- Context-Specific Trait Profiles ---\n");
        foreach (var (context, traits) in profile.ContextualProfiles)
        {
            Console.WriteLine($"[{context}]");
            foreach (var (traitId, strength) in traits)
            {
                Console.WriteLine($"  {traitId}: {strength:F2}");
            }
            Console.WriteLine();
        }

        // Co-activation patterns
        Console.WriteLine("--- Notable Co-activation Patterns ---");
        Console.WriteLine("(These show where seemingly contradictory traits appear together)\n");

        foreach (var pattern in profile.CoActivationPatterns)
        {
            Console.WriteLine($"{pattern.Trait1} ⊗ {pattern.Trait2}: {pattern.Count} times (P={pattern.Probability:F2})");
        }
        Console.WriteLine();

        // Metrics
        Console.WriteLine("--- Personality Metrics ---\n");
        Console.WriteLine($"Trait Diversity: {profile.TraitDiversity} (higher = more complex personality)");
        Console.WriteLine($"Context Variance: {profile.ContextVariance:F2} (higher = more context-dependent)");
        Console.WriteLine($"Contradiction Tolerance: {profile.ContradictionTolerance:F2} (higher = comfortable with paradox)");

        Console.WriteLine("\n\nYour personality exists in high-dimensional space, not a few buckets.");
        Console.WriteLine("The contradictions are features, not bugs.");
    }
}
