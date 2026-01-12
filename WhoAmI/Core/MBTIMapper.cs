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
    /// Calculate Assertive (-A) vs Turbulent (-T) variant.
    /// Based on meta-scores: self-assurance, question revisits, and speed.
    /// </summary>
    public char GetAssertiveTurbulent(MetaScores metaScores)
    {
        // Assertive traits: confident, doesn't second-guess, stable self-image
        // Turbulent traits: self-conscious, perfectionist, revisits decisions

        var assertivenessScore = 0f;

        // Low answer change rate suggests confidence (Assertive)
        assertivenessScore += (1f - metaScores.AnswerChangeRate) * 2f;

        // Fast responses suggest decisiveness (Assertive)
        if (metaScores.AverageResponseTime < 5000) // < 5 seconds
            assertivenessScore += 1.5f;
        else if (metaScores.AverageResponseTime > 15000) // > 15 seconds (overthinking)
            assertivenessScore -= 1.5f;

        // Low mirror consistency suggests spontaneity/confidence (Assertive)
        // High mirror consistency suggests careful deliberation (Turbulent)
        assertivenessScore += (1f - metaScores.MirrorConsistency) * 1.2f;

        // Return A if score > 2.0, otherwise T
        return assertivenessScore > 2.0f ? 'A' : 'T';
    }

    /// <summary>
    /// Get sub-traits: strong cognitive functions that aren't the dominant type.
    /// Example: ENFP (Fi-Ne) with strong Ti usage.
    /// </summary>
    public List<SubTrait> GetSubTraits()
    {
        var subTraits = new List<SubTrait>();
        var mbtiType = MapToMBTI();

        // Map cognitive functions to trait patterns
        var cognitiveScores = new Dictionary<string, float>
        {
            ["Te"] = GetGlobalTraitStrength("explicit_verbalization") + GetGlobalTraitStrength("rule_first") + GetGlobalTraitStrength("control_seeking"),
            ["Ti"] = GetGlobalTraitStrength("analytical_parsing") + GetGlobalTraitStrength("precision_seeking") * 1.5f + GetGlobalTraitStrength("internal_processing"),
            ["Fe"] = GetGlobalTraitStrength("affect_driven") + GetContextualTraitStrength("initiates_interaction", SituationalContext.Social) * 1.5f,
            ["Fi"] = GetGlobalTraitStrength("principle_driven") * 1.5f + GetGlobalTraitStrength("emotion_first"),
            ["Ne"] = GetGlobalTraitStrength("ambiguity_tolerant") + GetGlobalTraitStrength("optionality_preservation") * 1.5f + GetGlobalTraitStrength("iteration_first"),
            ["Ni"] = GetGlobalTraitStrength("future_projection") + GetGlobalTraitStrength("model_building") * 1.5f + GetGlobalTraitStrength("top_down"),
            ["Se"] = GetGlobalTraitStrength("present_situational") * 1.5f + GetContextualTraitStrength("present_situational", SituationalContext.Stress),
            ["Si"] = GetGlobalTraitStrength("sequence_dependent") + GetGlobalTraitStrength("specification_first") * 1.5f + GetGlobalTraitStrength("literal_interpretation")
        };

        // Get dominant functions for this type
        var dominantFunctions = GetDominantFunctions(mbtiType);

        // Find non-dominant functions with high scores
        foreach (var (func, score) in cognitiveScores.OrderByDescending(kv => kv.Value))
        {
            if (!dominantFunctions.Contains(func) && score > 2.0f) // Significant strength threshold
            {
                subTraits.Add(new SubTrait(func, score, GetFunctionDescription(func)));
            }
        }

        return subTraits.Take(3).ToList(); // Top 3 sub-traits
    }

    private List<string> GetDominantFunctions(MBTIType type)
    {
        // Return the top 2 functions for each type
        return type switch
        {
            MBTIType.ENFP => new List<string> { "Ne", "Fi" },
            MBTIType.INFP => new List<string> { "Fi", "Ne" },
            MBTIType.ENTP => new List<string> { "Ne", "Ti" },
            MBTIType.INTP => new List<string> { "Ti", "Ne" },
            MBTIType.ENFJ => new List<string> { "Fe", "Ni" },
            MBTIType.INFJ => new List<string> { "Ni", "Fe" },
            MBTIType.ENTJ => new List<string> { "Te", "Ni" },
            MBTIType.INTJ => new List<string> { "Ni", "Te" },
            MBTIType.ESFP => new List<string> { "Se", "Fi" },
            MBTIType.ISFP => new List<string> { "Fi", "Se" },
            MBTIType.ESTP => new List<string> { "Se", "Ti" },
            MBTIType.ISTP => new List<string> { "Ti", "Se" },
            MBTIType.ESFJ => new List<string> { "Fe", "Si" },
            MBTIType.ISFJ => new List<string> { "Si", "Fe" },
            MBTIType.ESTJ => new List<string> { "Te", "Si" },
            MBTIType.ISTJ => new List<string> { "Si", "Te" },
            _ => new List<string>()
        };
    }

    private string GetFunctionDescription(string function)
    {
        return function switch
        {
            "Te" => "Extraverted Thinking - External organization, efficiency, direct communication",
            "Ti" => "Introverted Thinking - Internal logic, systematic analysis, precision",
            "Fe" => "Extraverted Feeling - Social harmony, group values, reading the room",
            "Fi" => "Introverted Feeling - Personal values, authenticity, moral compass",
            "Ne" => "Extraverted Intuition - Exploring possibilities, brainstorming, pattern generation",
            "Ni" => "Introverted Intuition - Singular vision, future insight, convergent synthesis",
            "Se" => "Extraverted Sensing - Present awareness, sensory experience, action-oriented",
            "Si" => "Introverted Sensing - Memory details, procedures, concrete facts",
            _ => "Unknown function"
        };
    }

    /// <summary>
    /// Get confidence scores for each MBTI dimension (0-1).
    /// </summary>
    public MBTIConfidence GetConfidenceScores()
    {
        // E vs I
        var extraversionScore =
            GetContextualTraitStrength("initiates_interaction", SituationalContext.Social) +
            GetContextualTraitStrength("initiates_interaction", SituationalContext.Work) +
            GetContextualTraitStrength("explicit_verbalization", SituationalContext.Social) * 0.5f +
            GetGlobalTraitStrength("initiates_interaction");

        var introversionScore =
            GetContextualTraitStrength("responds_to_interaction", SituationalContext.Social) +
            GetContextualTraitStrength("responds_to_interaction", SituationalContext.Work) +
            GetContextualTraitStrength("internal_processing", SituationalContext.Social) * 0.5f +
            GetGlobalTraitStrength("responds_to_interaction");

        var e_i_confidence = Math.Abs(extraversionScore - introversionScore) / Math.Max(extraversionScore + introversionScore, 0.1f);

        // S vs N
        var sensingScore =
            GetGlobalTraitStrength("present_situational") * 1.5f +
            GetGlobalTraitStrength("precision_seeking") +
            GetGlobalTraitStrength("literal_interpretation") +
            GetGlobalTraitStrength("specification_first") +
            GetGlobalTraitStrength("sequence_dependent");

        var intuitionScore =
            GetGlobalTraitStrength("ambiguity_tolerant") * 1.5f +
            GetGlobalTraitStrength("iteration_first") * 1.2f +
            GetGlobalTraitStrength("optionality_preservation") +
            GetGlobalTraitStrength("holistic_sensing") +
            GetGlobalTraitStrength("top_down") +
            GetGlobalTraitStrength("model_building") +
            GetGlobalTraitStrength("future_projection") +
            GetGlobalTraitStrength("implied_meaning");

        var s_n_confidence = Math.Abs(sensingScore - intuitionScore) / Math.Max(sensingScore + intuitionScore, 0.1f);

        // T vs F
        var thinkingScore =
            GetGlobalTraitStrength("analytical_parsing") * 1.3f +
            GetGlobalTraitStrength("rule_first") +
            GetGlobalTraitStrength("affect_filtered") +
            GetGlobalTraitStrength("precision_seeking");

        var feelingScore =
            GetGlobalTraitStrength("principle_driven") * 1.5f +
            GetGlobalTraitStrength("emotion_first") * 1.3f +
            GetGlobalTraitStrength("affect_driven") +
            GetGlobalTraitStrength("exception_first") +
            GetGlobalTraitStrength("context_dependent_comm");

        var t_f_confidence = Math.Abs(thinkingScore - feelingScore) / Math.Max(thinkingScore + feelingScore, 0.1f);

        // J vs P
        var judgingScore =
            GetGlobalTraitStrength("control_seeking") +
            GetGlobalTraitStrength("deadline_driven") +
            GetGlobalTraitStrength("specification_first") +
            GetGlobalTraitStrength("sequence_dependent");

        var perceivingScore =
            GetGlobalTraitStrength("trust_delegating") +
            GetGlobalTraitStrength("steady_pace") +
            GetGlobalTraitStrength("iteration_first") +
            GetGlobalTraitStrength("ambiguity_tolerant") +
            GetGlobalTraitStrength("optionality_preservation") +
            GetGlobalTraitStrength("self_interruptible");

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
        // E/I is about energy direction: external vs internal focus
        // High explicit_verbalization + confrontation_tolerant suggests E
        var extraversionScore =
            GetContextualTraitStrength("initiates_interaction", SituationalContext.Social) +
            GetContextualTraitStrength("initiates_interaction", SituationalContext.Work) +
            GetGlobalTraitStrength("explicit_verbalization") * 1.2f +  // Te/Fe trait
            GetGlobalTraitStrength("initiates_interaction") +
            GetContextualTraitStrength("explicit_verbalization", SituationalContext.Social) * 0.8f +
            GetContextualTraitStrength("confrontation_tolerant", SituationalContext.Conflict) * 0.5f;

        var introversionScore =
            GetContextualTraitStrength("responds_to_interaction", SituationalContext.Social) +
            GetContextualTraitStrength("responds_to_interaction", SituationalContext.Work) +
            GetGlobalTraitStrength("responds_to_interaction") +
            GetGlobalTraitStrength("internal_processing") * 1.2f +
            GetContextualTraitStrength("internal_processing", SituationalContext.Social) * 0.8f;

        return extraversionScore > introversionScore ? 'E' : 'I';
    }

    private char GetSensingIntuition()
    {
        // Se (Sensing Extraverted) = present-focused, sensory experience, reactive
        // Si (Sensing Introverted) = detailed memory, procedure-following, concrete facts
        // Ne (Intuition Extraverted) = possibility exploration, pattern generation, conceptual play
        // Ni (Intuition Introverted) = singular vision, convergent insight, future certainty

        // S indicators: concrete, detailed, present-focused, procedural
        var sensingScore =
            GetGlobalTraitStrength("present_situational") * 1.5f +
            GetGlobalTraitStrength("precision_seeking") +
            GetGlobalTraitStrength("literal_interpretation") +
            GetGlobalTraitStrength("specification_first") +
            GetGlobalTraitStrength("sequence_dependent") +
            GetContextualTraitStrength("present_situational", SituationalContext.Work) +
            GetContextualTraitStrength("specification_first", SituationalContext.Work);

        // N indicators: abstract, pattern-seeking, possibility-oriented, conceptual
        var intuitionScore =
            GetGlobalTraitStrength("ambiguity_tolerant") * 1.5f +  // Ne hallmark
            GetGlobalTraitStrength("iteration_first") * 1.2f +      // Ne exploration
            GetGlobalTraitStrength("optionality_preservation") +    // Ne keeping doors open
            GetGlobalTraitStrength("holistic_sensing") +
            GetGlobalTraitStrength("top_down") +
            GetGlobalTraitStrength("model_building") +
            GetGlobalTraitStrength("future_projection") +
            GetGlobalTraitStrength("implied_meaning") +             // reading between lines
            GetContextualTraitStrength("exploration_oriented", SituationalContext.Stress) +
            GetContextualTraitStrength("opportunistic_planning", SituationalContext.Stress);

        return sensingScore > intuitionScore ? 'S' : 'N';
    }

    private char GetThinkingFeeling()
    {
        // Te (Thinking Extraverted) = external organization, efficiency, direct communication
        // Ti (Thinking Introverted) = internal logic, systematic analysis, precision
        // Fe (Feeling Extraverted) = social harmony, reading room, external values
        // Fi (Feeling Introverted) = internal values, authenticity, principle-driven

        // T indicators: analytical, logical, objective, system-focused
        var thinkingScore =
            GetGlobalTraitStrength("analytical_parsing") * 1.3f +
            GetGlobalTraitStrength("rule_first") +
            GetGlobalTraitStrength("affect_filtered") +
            GetGlobalTraitStrength("precision_seeking") +
            GetContextualTraitStrength("analytical_parsing", SituationalContext.Work) +
            GetContextualTraitStrength("analytical_parsing", SituationalContext.Learning);

        // F indicators: values-driven, people-focused, context-sensitive
        var feelingScore =
            GetGlobalTraitStrength("principle_driven") * 1.5f +    // Fi hallmark
            GetGlobalTraitStrength("emotion_first") * 1.3f +
            GetGlobalTraitStrength("affect_driven") +
            GetGlobalTraitStrength("exception_first") +            // Fi individualized
            GetGlobalTraitStrength("context_dependent_comm") +
            GetContextualTraitStrength("principle_driven", SituationalContext.Conflict) +  // Fi standing ground
            GetContextualTraitStrength("implied_meaning", SituationalContext.Social);

        return thinkingScore > feelingScore ? 'T' : 'F';
    }

    private char GetJudgingPerceiving()
    {
        // J = structured, planned, closure-seeking, decisive
        // P = flexible, spontaneous, open-ended, exploratory
        var judgingScore =
            GetGlobalTraitStrength("control_seeking") +
            GetGlobalTraitStrength("deadline_driven") +
            GetGlobalTraitStrength("specification_first") +
            GetGlobalTraitStrength("sequence_dependent") +
            GetContextualTraitStrength("focus_lock", SituationalContext.Work) +
            GetContextualTraitStrength("outcome_driven", SituationalContext.Work);

        var perceivingScore =
            GetGlobalTraitStrength("trust_delegating") +
            GetGlobalTraitStrength("steady_pace") +
            GetGlobalTraitStrength("iteration_first") +
            GetGlobalTraitStrength("ambiguity_tolerant") +
            GetGlobalTraitStrength("optionality_preservation") +
            GetGlobalTraitStrength("self_interruptible") +
            GetContextualTraitStrength("opportunistic_planning", SituationalContext.Stress) +
            GetContextualTraitStrength("exploration_oriented", SituationalContext.Stress);

        return judgingScore > perceivingScore ? 'J' : 'P';
    }

    /// <summary>
    /// Get global trait strength across all contexts.
    /// </summary>
    private float GetGlobalTraitStrength(string traitId)
    {
        var trait = _profile.AllTraits.FirstOrDefault(t => t.TraitId == traitId);
        return trait.TraitId != null ? trait.TotalStrength : 0f;
    }

    /// <summary>
    /// Get trait strength in a specific context.
    /// </summary>
    private float GetContextualTraitStrength(string traitId, SituationalContext context)
    {
        if (!_profile.ContextualProfiles.TryGetValue(context, out var contextProfile))
            return 0f;

        var trait = contextProfile.FirstOrDefault(t => t.TraitId == traitId);
        return trait.TraitId != null ? trait.Strength : 0f;
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
