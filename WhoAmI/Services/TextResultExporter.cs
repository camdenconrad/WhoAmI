using System.Text;
using WhoAmI.Core;
using WhoAmI.Models;

namespace WhoAmI.Services;

/// <summary>
/// Exports personality results as human-readable text.
/// </summary>
public class TextResultExporter : IResultExporter
{
    public async Task ExportAsync(PersonalityProfile profile, MBTIType mbtiType, string filePath)
    {
        var sb = new StringBuilder();
        var mbtiInfo = MBTIPersonality.Definitions[mbtiType];

        sb.AppendLine("=== WhoAmI Personality Assessment Results ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // MBTI Type
        sb.AppendLine("--- 16 Personalities Type ---");
        sb.AppendLine($"{mbtiType} - {mbtiInfo.Nickname}");
        sb.AppendLine($"{mbtiInfo.Description}");
        sb.AppendLine();

        // Dominant traits
        sb.AppendLine("--- Overall Dominant Traits ---");
        foreach (var (traitId, strength) in profile.DominantTraits)
        {
            sb.AppendLine($"  {traitId}: {strength:F2}");
        }
        sb.AppendLine();

        // Context-specific profiles
        sb.AppendLine("--- Context-Specific Trait Profiles ---");
        foreach (var (context, traits) in profile.ContextualProfiles)
        {
            sb.AppendLine($"[{context}]");
            foreach (var (traitId, strength) in traits)
            {
                sb.AppendLine($"  {traitId}: {strength:F2}");
            }
            sb.AppendLine();
        }

        // Co-activation patterns
        sb.AppendLine("--- Notable Co-activation Patterns ---");
        sb.AppendLine("(These show where seemingly contradictory traits appear together)");
        sb.AppendLine();
        foreach (var pattern in profile.CoActivationPatterns)
        {
            sb.AppendLine($"{pattern.Trait1} ⊗ {pattern.Trait2}: {pattern.Count} times (P={pattern.Probability:F2})");
        }
        sb.AppendLine();

        // Metrics
        sb.AppendLine("--- Personality Metrics ---");
        sb.AppendLine($"Trait Diversity: {profile.TraitDiversity} (higher = more complex personality)");
        sb.AppendLine($"Context Variance: {profile.ContextVariance:F2} (higher = more context-dependent)");
        sb.AppendLine($"Contradiction Tolerance: {profile.ContradictionTolerance:F2} (higher = comfortable with paradox)");
        sb.AppendLine();

        sb.AppendLine("Your personality exists in high-dimensional space, not a few buckets.");
        sb.AppendLine("The contradictions are features, not bugs.");

        await File.WriteAllTextAsync(filePath, sb.ToString());
    }
}
