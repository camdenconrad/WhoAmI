using System.Collections.Immutable;
using System.Text.Json;
using WhoAmI.Models;

namespace WhoAmI.Services;

public static class JsonTestDataLoader
{
    private class TraitJson
    {
        public string id { get; set; } = "";
        public string name { get; set; } = "";
        public string category { get; set; } = "";
        public string description { get; set; } = "";
    }

    private class VignetteOptionJson
    {
        public string label { get; set; } = "";
        public string text { get; set; } = "";
        public Dictionary<string, float> traits { get; set; } = new();
    }

    private class VignetteJson
    {
        public string id { get; set; } = "";
        public string question { get; set; } = "";
        public List<string> context { get; set; } = new();
        public List<VignetteOptionJson> options { get; set; } = new();
    }

    private class TestDataJson
    {
        public List<TraitJson> traits { get; set; } = new();
        public List<VignetteJson> vignettes { get; set; } = new();
    }

    private class CounterExamplesJson
    {
        public List<VignetteJson> counter_example_vignettes { get; set; } = new();
        public List<MirrorPairJson> mirror_pairs { get; set; } = new();
    }

    private class MirrorPairJson
    {
        public string vignette1 { get; set; } = "";
        public string vignette2 { get; set; } = "";
        public string trait_probed { get; set; } = "";
        public string expected_correlation { get; set; } = "";
    }

    public static (ImmutableArray<TraitDimension>, ImmutableArray<Vignette>) LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test data file not found at: {filePath}");
        }

        var jsonString = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            throw new Exception("Test data file is empty");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var data = JsonSerializer.Deserialize<TestDataJson>(jsonString, options)
            ?? throw new Exception("Failed to deserialize test data");

        if (data.traits.Count == 0)
        {
            throw new Exception("No traits found in test data");
        }

        if (data.vignettes.Count == 0)
        {
            throw new Exception("No vignettes found in test data");
        }

        var traits = data.traits
            .Select(t => new TraitDimension(t.id, t.name, t.category, t.description))
            .ToImmutableArray();

        var vignettes = data.vignettes
            .Select(v => new Vignette(
                Id: v.id,
                Scenario: v.question,
                Context: ParseContext(v.context),
                Options: v.options
                    .Select(o => new VignetteOption(
                        Label: o.label,
                        Description: o.text,
                        TraitActivations: o.traits.ToImmutableDictionary()
                    ))
                    .ToImmutableArray()
            ))
            .ToList();

        // Try to load counter-examples and merge them
        var counterExamplesPath = Path.Combine(
            Path.GetDirectoryName(filePath) ?? "",
            "counter-examples.json"
        );

        Console.WriteLine($"[DEBUG] Looking for counter-examples at: {counterExamplesPath}");
        Console.WriteLine($"[DEBUG] Counter-examples file exists: {File.Exists(counterExamplesPath)}");
        Console.WriteLine($"[DEBUG] Vignettes before counter-examples: {vignettes.Count}");

        if (File.Exists(counterExamplesPath))
        {
            try
            {
                var counterJson = File.ReadAllText(counterExamplesPath);
                Console.WriteLine($"[DEBUG] Counter-examples JSON length: {counterJson.Length}");

                var counterData = JsonSerializer.Deserialize<CounterExamplesJson>(counterJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Console.WriteLine($"[DEBUG] Deserialized counter_example_vignettes count: {counterData?.counter_example_vignettes?.Count ?? 0}");

                if (counterData?.counter_example_vignettes != null)
                {
                    var counterVignettes = counterData.counter_example_vignettes
                        .Select(v => new Vignette(
                            Id: v.id,
                            Scenario: v.question,
                            Context: ParseContext(v.context),
                            Options: v.options
                                .Select(o => new VignetteOption(
                                    Label: o.label,
                                    Description: o.text,
                                    TraitActivations: o.traits.ToImmutableDictionary()
                                ))
                                .ToImmutableArray()
                        ))
                        .ToList();

                    Console.WriteLine($"[DEBUG] Converted {counterVignettes.Count} counter vignettes");
                    vignettes.AddRange(counterVignettes);
                    Console.WriteLine($"[DEBUG] Total vignettes after merge: {vignettes.Count}");
                }
            }
            catch (Exception ex)
            {
                // Counter-examples are optional, log but don't fail
                Console.WriteLine($"[ERROR] Could not load counter-examples: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            }
        }
        else
        {
            Console.WriteLine($"[DEBUG] Counter-examples file not found at expected path");
        }

        return (traits, vignettes.ToImmutableArray());
    }

    public static ImmutableArray<MirrorPair> LoadMirrorPairs(string dataDirectory)
    {
        var counterExamplesPath = Path.Combine(dataDirectory, "counter-examples.json");

        if (!File.Exists(counterExamplesPath))
        {
            return ImmutableArray<MirrorPair>.Empty;
        }

        try
        {
            var counterJson = File.ReadAllText(counterExamplesPath);
            var counterData = JsonSerializer.Deserialize<CounterExamplesJson>(counterJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (counterData?.mirror_pairs == null)
            {
                return ImmutableArray<MirrorPair>.Empty;
            }

            return counterData.mirror_pairs
                .Select(mp => new MirrorPair(
                    VignetteId1: mp.vignette1,
                    VignetteId2: mp.vignette2,
                    TraitProbed: mp.trait_probed,
                    ExpectedCorrelation: mp.expected_correlation
                ))
                .ToImmutableArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load mirror pairs: {ex.Message}");
            return ImmutableArray<MirrorPair>.Empty;
        }
    }

    private static SituationalContext ParseContext(List<string> contexts)
    {
        var result = SituationalContext.None;
        foreach (var ctx in contexts)
        {
            result |= ctx switch
            {
                "Work" => SituationalContext.Work,
                "Social" => SituationalContext.Social,
                "Stress" => SituationalContext.Stress,
                "Leisure" => SituationalContext.Leisure,
                "Conflict" => SituationalContext.Conflict,
                "Learning" => SituationalContext.Learning,
                "Creative" => SituationalContext.Creative,
                "Crisis" => SituationalContext.Crisis,
                _ => SituationalContext.None
            };
        }
        return result;
    }
}
