using System.Collections.Immutable;
using WhoAmI.Models;
using WhoAmI.Services;

namespace WhoAmI.Data;

/// <summary>
/// Universal personality test with 32+ behavioral vignettes.
/// General-purpose questions suitable for anyone, regardless of profession or background.
/// All test data is loaded from test-data.json for easy editing.
/// </summary>
public static class TestData
{
    private static ImmutableArray<TraitDimension> _traits;
    private static ImmutableArray<Vignette> _vignettes;
    private static ImmutableArray<MirrorPair> _mirrorPairs;
    private static bool _isLoaded = false;
    private static string? _dataDirectory = null;

    private static void EnsureLoaded()
    {
        if (_isLoaded) return;

        try
        {
            // Try multiple possible paths
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "test-data.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "Data", "test-data.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-data.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "test-data.json")
            };

            string? dataPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    dataPath = path;
                    break;
                }
            }

            if (dataPath == null)
            {
                throw new FileNotFoundException(
                    $"Could not find test-data.json. Searched:\n" +
                    string.Join("\n", possiblePaths));
            }

            _dataDirectory = Path.GetDirectoryName(dataPath);

            var (traits, vignettes) = JsonTestDataLoader.LoadFromFile(dataPath);
            _traits = traits;
            _vignettes = vignettes;

            // Load mirror pairs from counter-examples.json
            if (_dataDirectory != null)
            {
                _mirrorPairs = JsonTestDataLoader.LoadMirrorPairs(_dataDirectory);
            }
            else
            {
                _mirrorPairs = ImmutableArray<MirrorPair>.Empty;
            }

            _isLoaded = true;

            Console.WriteLine($"Loaded {_vignettes.Length} total vignettes (including counter-examples)");
            Console.WriteLine($"Loaded {_mirrorPairs.Length} mirror pairs");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load test data: {ex.Message}", ex);
        }
    }

    public static ImmutableArray<TraitDimension> GetTraits()
    {
        EnsureLoaded();
        return _traits;
    }

    public static ImmutableArray<Vignette> GetVignettes()
    {
        EnsureLoaded();
        return _vignettes;
    }

    public static ImmutableArray<MirrorPair> GetMirrorPairs()
    {
        EnsureLoaded();
        return _mirrorPairs;
    }
}
