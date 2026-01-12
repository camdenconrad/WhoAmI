using WhoAmI.Core;

namespace WhoAmI.Services;

/// <summary>
/// Interface for presenting personality analysis results.
/// </summary>
public interface IResultPresenter
{
    void PresentResults(PersonalityProfile profile);
}
