using System.Collections.Immutable;
using WhoAmI.Core;
using WhoAmI.Models;

namespace WhoAmI.Services;

/// <summary>
/// Console-based test administrator that presents vignettes and collects responses.
/// </summary>
public class ConsoleTestAdministrator : ITestAdministrator
{
    private readonly ImmutableArray<Vignette> _vignettes;

    public ConsoleTestAdministrator(ImmutableArray<Vignette> vignettes)
    {
        _vignettes = vignettes;
    }

    public PersonalityManifold AdministerTest()
    {
        var manifold = new PersonalityManifold();

        Console.WriteLine("=== WhoAmI: High-Dimensional Personality Assessment ===\n");
        Console.WriteLine("This test uses behavioral vignettes to map your cognitive patterns.");
        Console.WriteLine("Choose the option that best matches your ACTUAL behavior, not your ideal.\n");

        foreach (var vignette in _vignettes)
        {
            Console.WriteLine($"\n[{vignette.Context}] {vignette.Scenario}\n");

            for (int i = 0; i < vignette.Options.Length; i++)
            {
                var option = vignette.Options[i];
                Console.WriteLine($"{i + 1}. {option.Description}");
            }

            int choice;
            do
            {
                Console.Write("\nYour choice (1-{0}): ", vignette.Options.Length);
                var input = Console.ReadLine();
                if (int.TryParse(input, out choice) && choice >= 1 && choice <= vignette.Options.Length)
                    break;
                Console.WriteLine("Invalid choice. Please try again.");
            } while (true);

            manifold.RecordResponse(vignette.Options[choice - 1], vignette.Context);
        }

        return manifold;
    }
}
