namespace WhoAmI.Models;

/// <summary>
/// Captures not just WHAT was answered, but HOW and WHEN.
/// This meta-layer detects gaming, inconsistency, and self-model stability.
/// </summary>
public record VignetteResponse(
    string VignetteId,
    string SelectedOption,
    long ResponseTimeMs,
    bool WasChanged,
    string? PreviousSelection = null,
    long? TimeToFirstAnswer = null,
    long? TimeToFinalAnswer = null,
    int ChangeCount = 0
);

/// <summary>
/// Meta-scores computed from response patterns, not content.
/// These reveal self-awareness, impression management, and cognitive style.
/// </summary>
public record MetaScores(
    float SelfModelStability,      // Low variance in response times, few changes
    float HedgingTendency,          // Frequency of "depends/both/middle" answers
    float SocialDesirability,       // Correlation with culturally "ideal" answers
    float TheoryOfMindBias,         // As vs Tu asymmetry (self vs other attribution)
    float ImpressionManagement,     // Pattern of changing toward "better" answers
    float LatentContradiction,      // Inconsistency on mirror question pairs
    float ResponseConfidence        // Inverse of latency spikes on sensitive items
)
{
    // Additional computed properties for Assertive/Turbulent analysis
    public float AnswerChangeRate { get; init; }
    public long AverageResponseTime { get; init; }
    public float MirrorConsistency { get; init; }
};

/// <summary>
/// Flags for specific gaming or inconsistency patterns.
/// Not "errors" - these are valuable diagnostic signals.
/// </summary>
public enum ResponseFlag
{
    LatentContradiction,        // Inconsistent on non-obvious mirror questions
    HedgingBias,                 // Repeated selection of "depends/both" options
    SocialDesirabilitySpike,     // All answers cluster toward cultural ideal
    SelfOtherAsymmetry,         // As-Tu attribution gap (actor-observer effect)
    ResponseLatencySpike,        // Very slow answer on "sensitive" question
    BacktrackPattern,            // Systematic change toward neutral/positive
    FastUniformResponses         // All answers very fast (not reading carefully)
}

/// <summary>
/// A mirror question pair that probes the same trait from different angles.
/// Used to detect inconsistency and gaming.
/// </summary>
public record MirrorPair(
    string VignetteId1,
    string VignetteId2,
    string TraitProbed,
    string ExpectedCorrelation  // "positive", "negative", "independent"
);
