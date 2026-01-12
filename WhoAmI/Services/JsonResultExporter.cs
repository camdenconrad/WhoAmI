using System.Text.Json;
using System.Text.Json.Serialization;
using WhoAmI.Core;
using WhoAmI.Models;

namespace WhoAmI.Services;

/// <summary>
/// Exports personality results as JSON.
/// </summary>
public class JsonResultExporter : IResultExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task ExportAsync(PersonalityProfile profile, MBTIType mbtiType, string filePath)
    {
        var mbtiInfo = MBTIPersonality.Definitions[mbtiType];
        
        var export = new
        {
            Timestamp = DateTime.UtcNow,
            MBTI = new
            {
                Type = mbtiType.ToString(),
                mbtiInfo.Nickname,
                mbtiInfo.Description
            },
            HighDimensionalProfile = new
            {
                DominantTraits = profile.DominantTraits.Select(t => new
                {
                    TraitId = t.TraitId,
                    Strength = t.TotalStrength
                }),
                ContextualProfiles = profile.ContextualProfiles.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value.Select(t => new { TraitId = t.TraitId, Strength = t.Strength })
                ),
                CoActivationPatterns = profile.CoActivationPatterns.Select(p => new
                {
                    p.Trait1,
                    p.Trait2,
                    p.Count,
                    p.Probability
                }),
                Metrics = new
                {
                    profile.TraitDiversity,
                    profile.ContextVariance,
                    profile.ContradictionTolerance
                }
            }
        };

        var json = JsonSerializer.Serialize(export, Options);
        await File.WriteAllTextAsync(filePath, json);
    }
}
