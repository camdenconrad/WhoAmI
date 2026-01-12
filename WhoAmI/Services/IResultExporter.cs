using WhoAmI.Core;
using WhoAmI.Models;

namespace WhoAmI.Services;

/// <summary>
/// Interface for exporting personality results to various formats.
/// </summary>
public interface IResultExporter
{
    Task ExportAsync(PersonalityProfile profile, MBTIType mbtiType, string filePath);
}
