namespace WhoAmI.Models;

/// <summary>
/// Contextual tags that allow trait expression to vary by situation.
/// This prevents flattening of nuanced behaviors into global averages.
/// </summary>
[Flags]
public enum SituationalContext
{
    None = 0,
    Work = 1 << 0,
    Social = 1 << 1,
    Stress = 1 << 2,
    Leisure = 1 << 3,
    Conflict = 1 << 4,
    Learning = 1 << 5,
    Creative = 1 << 6,
    Crisis = 1 << 7
}
