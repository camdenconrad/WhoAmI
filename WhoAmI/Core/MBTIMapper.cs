using WhoAmI.Models;

namespace WhoAmI.Core;

/// <summary>
/// Maps high-dimensional personality manifold to 16 Personalities / MBTI type.
/// This is a PROJECTION from high-dimensional space to 4-bit space (16 types).
/// Information is lost, but provides compatibility with popular framework.
/// </summary>
public class MBTIMapper
{
    private readonly PersonalityProfile _profile;

    public MBTIMapper(PersonalityProfile profile)
    {
        _profile = profile;
    }

    /// <summary>
    /// Maps to the closest MBTI type based on trait patterns.
    /// </summary>
    public MBTIType MapToMBTI()
    {
        // Extract the 4 MBTI dimensions
        var e_i = GetExtraversionIntroversion();
        var s_n = GetSensingIntuition();
        var t_f = GetThinkingFeeling();
        var j_p = GetJudgingPerceiving();

        // Construct the type
        return (e_i, s_n, t_f, j_p) switch
        {
            ('I', 'N', 'T', 'J') => MBTIType.INTJ,
            ('I', 'N', 'T', 'P') => MBTIType.INTP,
            ('E', 'N', 'T', 'J') => MBTIType.ENTJ,
            ('E', 'N', 'T', 'P') => MBTIType.ENTP,
            ('I', 'N', 'F', 'J') => MBTIType.INFJ,
            ('I', 'N', 'F', 'P') => MBTIType.INFP,
            ('E', 'N', 'F', 'J') => MBTIType.ENFJ,
            ('E', 'N', 'F', 'P') => MBTIType.ENFP,
            ('I', 'S', 'T', 'J') => MBTIType.ISTJ,
            ('I', 'S', 'F', 'J') => MBTIType.ISFJ,
            ('E', 'S', 'T', 'J') => MBTIType.ESTJ,
            ('E', 'S', 'F', 'J') => MBTIType.ESFJ,
            ('I', 'S', 'T', 'P') => MBTIType.ISTP,
            ('I', 'S', 'F', 'P') => MBTIType.ISFP,
            ('E', 'S', 'T', 'P') => MBTIType.ESTP,
            ('E', 'S', 'F', 'P') => MBTIType.ESFP,
            _ => MBTIType.INTP // default fallback
        };
    }

    /// <summary>
    /// Get confidence scores for each MBTI dimension (0-1).
    /// </summary>
    public MBTIConfidence GetConfidenceScores()
    {
        var traits = _profile.DominantTraits.ToDictionary(t => t.TraitId, t => t.TotalStrength);

        // E vs I
        var extraversionScore = GetTraitStrength(traits, "initiates_interaction");
        var introversionScore = GetTraitStrength(traits, "responds_to_interaction");
        var e_i_confidence = Math.Abs(extraversionScore - introversionScore) / Math.Max(extraversionScore + introversionScore, 0.1f);

        // S vs N
        var sensingScore = GetTraitStrength(traits, "analytical_parsing") + GetTraitStrength(traits, "bottom_up");
        var intuitionScore = GetTraitStrength(traits, "holistic_sensing") + GetTraitStrength(traits, "top_down");
        var s_n_confidence = Math.Abs(sensingScore - intuitionScore) / Math.Max(sensingScore + intuitionScore, 0.1f);

        // T vs F
        var thinkingScore = GetTraitStrength(traits, "rule_first") + GetTraitStrength(traits, "analytical_parsing");
        var feelingScore = GetTraitStrength(traits, "exception_first") + GetTraitStrength(traits, "context_dependent_comm");
        var t_f_confidence = Math.Abs(thinkingScore - feelingScore) / Math.Max(thinkingScore + feelingScore, 0.1f);

        // J vs P
        var judgingScore = GetTraitStrength(traits, "control_seeking") + GetTraitStrength(traits, "deadline_driven");
        var perceivingScore = GetTraitStrength(traits, "trust_delegating") + GetTraitStrength(traits, "steady_pace");
        var j_p_confidence = Math.Abs(judgingScore - perceivingScore) / Math.Max(judgingScore + perceivingScore, 0.1f);

        return new MBTIConfidence(
            (float)e_i_confidence,
            (float)s_n_confidence,
            (float)t_f_confidence,
            (float)j_p_confidence
        );
    }

    private char GetExtraversionIntroversion()
    {
        var traits = _profile.DominantTraits.ToDictionary(t => t.TraitId, t => t.TotalStrength);
        var extraversionScore = GetTraitStrength(traits, "initiates_interaction");
        var introversionScore = GetTraitStrength(traits, "responds_to_interaction");
        return extraversionScore >= introversionScore ? 'E' : 'I';
    }

    private char GetSensingIntuition()
    {
        var traits = _profile.DominantTraits.ToDictionary(t => t.TraitId, t => t.TotalStrength);
        var sensingScore = GetTraitStrength(traits, "analytical_parsing") + GetTraitStrength(traits, "bottom_up");
        var intuitionScore = GetTraitStrength(traits, "holistic_sensing") + GetTraitStrength(traits, "top_down");
        return sensingScore >= intuitionScore ? 'S' : 'N';
    }

    private char GetThinkingFeeling()
    {
        var traits = _profile.DominantTraits.ToDictionary(t => t.TraitId, t => t.TotalStrength);
        var thinkingScore = GetTraitStrength(traits, "rule_first") + GetTraitStrength(traits, "analytical_parsing");
        var feelingScore = GetTraitStrength(traits, "exception_first") + GetTraitStrength(traits, "context_dependent_comm");
        return thinkingScore >= feelingScore ? 'T' : 'F';
    }

    private char GetJudgingPerceiving()
    {
        var traits = _profile.DominantTraits.ToDictionary(t => t.TraitId, t => t.TotalStrength);
        var judgingScore = GetTraitStrength(traits, "control_seeking") + GetTraitStrength(traits, "deadline_driven");
        var perceivingScore = GetTraitStrength(traits, "trust_delegating") + GetTraitStrength(traits, "steady_pace");
        return judgingScore >= perceivingScore ? 'J' : 'P';
    }

    private float GetTraitStrength(Dictionary<string, float> traits, string traitId)
    {
        return traits.GetValueOrDefault(traitId, 0f);
    }
}

/// <summary>
/// Confidence scores for MBTI dimensions (0 = ambiguous, 1 = very clear).
/// </summary>
public record MBTIConfidence(
    float ExtraversionIntroversion,
    float SensingIntuition,
    float ThinkingFeeling,
    float JudgingPerceiving
);
