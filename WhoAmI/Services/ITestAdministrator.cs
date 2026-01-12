using WhoAmI.Core;

namespace WhoAmI.Services;

/// <summary>
/// Interface for administering personality tests.
/// </summary>
public interface ITestAdministrator
{
    PersonalityManifold AdministerTest();
}
