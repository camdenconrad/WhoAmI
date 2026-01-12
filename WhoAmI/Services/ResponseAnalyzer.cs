using System;
using System.Collections.Generic;
using System.Linq;
using WhoAmI.Models;

namespace WhoAmI.Services;

/// <summary>
/// Analyzes response patterns to detect gaming, inconsistency, and compute meta-scores.
/// This is where we catch people trying to game the test - not by punishing, but by measuring.
/// </summary>
public class ResponseAnalyzer
{
    private const long FastResponseThresholdMs = 1500;  // Less than 1.5s = very fast
    private const long SlowResponseThresholdMs = 15000; // More than 15s = latency spike
    private const int HedgingThreshold = 3; // 3+ hedging answers = pattern

    /// <summary>
    /// Compute meta-scores from response patterns.
    /// These reveal HOW people take the test, not just WHAT they answer.
    /// </summary>
    public MetaScores ComputeMetaScores(
        IReadOnlyList<VignetteResponse> responses,
        IReadOnlyList<MirrorPair> mirrorPairs)
    {
        var selfModelStability = ComputeSelfModelStability(responses);
        var hedgingTendency = ComputeHedgingTendency(responses);
        var socialDesirability = ComputeSocialDesirability(responses);
        var theoryOfMindBias = ComputeTheoryOfMindBias(responses, mirrorPairs);
        var impressionManagement = ComputeImpressionManagement(responses);
        var latentContradiction = ComputeLatentContradiction(responses, mirrorPairs);
        var responseConfidence = ComputeResponseConfidence(responses);
        var cognitiveDissonance = ComputeCognitiveDissonance(responses, mirrorPairs);

        // Additional metrics for A/T variant
        var changedCount = responses.Count(r => r.WasChanged);
        var answerChangeRate = responses.Count > 0 ? (float)changedCount / responses.Count : 0f;
        var averageResponseTime = responses.Count > 0 ? (long)responses.Average(r => r.ResponseTimeMs) : 0;
        var mirrorConsistency = 1f - latentContradiction; // Inverse of contradiction

        return new MetaScores(
            selfModelStability,
            hedgingTendency,
            socialDesirability,
            theoryOfMindBias,
            impressionManagement,
            latentContradiction,
            responseConfidence,
            cognitiveDissonance
        )
        {
            AnswerChangeRate = answerChangeRate,
            AverageResponseTime = averageResponseTime,
            MirrorConsistency = mirrorConsistency
        };
    }

    /// <summary>
    /// Flag specific patterns worth noting.
    /// Not errors - diagnostic signals.
    /// </summary>
    public IEnumerable<ResponseFlag> DetectFlags(
        IReadOnlyList<VignetteResponse> responses,
        IReadOnlyList<MirrorPair> mirrorPairs)
    {
        var flags = new List<ResponseFlag>();

        // Check for hedging bias
        var hedgingCount = responses.Count(r => IsHedgingResponse(r));
        if (hedgingCount >= HedgingThreshold)
        {
            flags.Add(ResponseFlag.HedgingBias);
        }

        // Check for latent contradictions
        if (HasLatentContradictions(responses, mirrorPairs))
        {
            flags.Add(ResponseFlag.LatentContradiction);
        }

        // Check for As-Tu asymmetry
        if (HasSelfOtherAsymmetry(responses, mirrorPairs))
        {
            flags.Add(ResponseFlag.SelfOtherAsymmetry);
        }

        // Check for response latency spikes
        if (HasResponseLatencySpikes(responses))
        {
            flags.Add(ResponseFlag.ResponseLatencySpike);
        }

        // Check for backtrack patterns
        if (HasBacktrackPattern(responses))
        {
            flags.Add(ResponseFlag.BacktrackPattern);
        }

        // Check for fast uniform responses
        if (AllResponsesFast(responses))
        {
            flags.Add(ResponseFlag.FastUniformResponses);
        }

        // Check for social desirability clustering
        if (HasSocialDesirabilitySpike(responses))
        {
            flags.Add(ResponseFlag.SocialDesirabilitySpike);
        }

        return flags;
    }

    /// <summary>
    /// Few changes = stable self-model.
    /// Many changes = uncertain or gaming.
    /// NO temporal weighting - only counts changes.
    /// </summary>
    private float ComputeSelfModelStability(IReadOnlyList<VignetteResponse> responses)
    {
        if (!responses.Any()) return 0.5f;

        var changeProportion = responses.Count(r => r.WasChanged) / (float)responses.Count;

        // Lower change rate = higher stability (no time component)
        return 1f - changeProportion;
    }

    /// <summary>
    /// Frequency of "depends", "both", "middle" answers.
    /// Not bad per se, but high rates suggest hedging or gaming.
    /// </summary>
    private float ComputeHedgingTendency(IReadOnlyList<VignetteResponse> responses)
    {
        if (!responses.Any()) return 0f;

        var hedgingCount = responses.Count(r => IsHedgingResponse(r));
        return hedgingCount / (float)responses.Count;
    }

    /// <summary>
    /// Correlation with culturally "ideal" answers.
    /// High score = possibly trying to look good.
    /// </summary>
    private float ComputeSocialDesirability(IReadOnlyList<VignetteResponse> responses)
    {
        // This would need cultural norms data to implement properly
        // For now, return placeholder
        var sociallyDesirableAnswers = new HashSet<string>
        {
            "ce06_other_cancel:A",
            "ce08_interrupt_self:B",
            "ce11_praise_agency:A"
        };

        var matches = responses.Count(r =>
            sociallyDesirableAnswers.Contains($"{r.VignetteId}:{r.SelectedOption}"));

        return responses.Any() ? matches / (float)responses.Count : 0f;
    }

    /// <summary>
    /// As vs Tu asymmetry - how differently you explain your vs others' behavior.
    /// This is the fundamental attribution error in action.
    /// </summary>
    private float ComputeTheoryOfMindBias(
        IReadOnlyList<VignetteResponse> responses,
        IReadOnlyList<MirrorPair> mirrorPairs)
    {
        var asTuPairs = mirrorPairs.Where(p =>
            p.ExpectedCorrelation.Contains("asymmetry", StringComparison.OrdinalIgnoreCase));

        if (!asTuPairs.Any()) return 0f;

        var asymmetryCount = 0;
        foreach (var pair in asTuPairs)
        {
            var r1 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId1);
            var r2 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId2);

            if (r1 != null && r2 != null && ShowsAsymmetry(r1, r2, pair))
            {
                asymmetryCount++;
            }
        }

        return asymmetryCount / (float)asTuPairs.Count();
    }

    /// <summary>
    /// Pattern of changing answers toward more neutral/positive.
    /// Suggests impression management.
    /// </summary>
    private float ComputeImpressionManagement(IReadOnlyList<VignetteResponse> responses)
    {
        var changedResponses = responses.Where(r => r.WasChanged);
        if (!changedResponses.Any()) return 0f;

        // If changes systematically move toward "middle" options (B or C in many cases)
        var middlewardChanges = changedResponses.Count(r =>
            r.PreviousSelection == "A" && (r.SelectedOption == "B" || r.SelectedOption == "C"));

        return middlewardChanges / (float)changedResponses.Count();
    }

    /// <summary>
    /// Inconsistency on mirror question pairs.
    /// Same trait, different angle - real people slip, gamers stay "logical".
    /// </summary>
    private float ComputeLatentContradiction(
        IReadOnlyList<VignetteResponse> responses,
        IReadOnlyList<MirrorPair> mirrorPairs)
    {
        if (!mirrorPairs.Any()) return 0f;

        var contradictionCount = 0;
        foreach (var pair in mirrorPairs)
        {
            var r1 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId1);
            var r2 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId2);

            if (r1 != null && r2 != null && IsContradictory(r1, r2, pair))
            {
                contradictionCount++;
            }
        }

        return contradictionCount / (float)mirrorPairs.Count;
    }

    /// <summary>
    /// Based on change count, not timing.
    /// Few changes = confident. Many changes = uncertain or editing.
    /// NO temporal component.
    /// </summary>
    private float ComputeResponseConfidence(IReadOnlyList<VignetteResponse> responses)
    {
        if (!responses.Any()) return 0.5f;

        var changeRatio = responses.Count(r => r.WasChanged) / (float)responses.Count;
        return 1f - changeRatio;
    }

    // Helper methods

    private bool IsHedgingResponse(VignetteResponse response)
    {
        // Typically option C in forced-choice questions
        // This is a heuristic - real implementation would check metadata
        // NO temporal component
        return response.SelectedOption == "C" && response.VignetteId.Contains("ce03");
    }

    private bool HasLatentContradictions(
        IReadOnlyList<VignetteResponse> responses,
        IReadOnlyList<MirrorPair> mirrorPairs)
    {
        return mirrorPairs.Any(pair =>
        {
            var r1 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId1);
            var r2 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId2);
            return r1 != null && r2 != null && IsContradictory(r1, r2, pair);
        });
    }

    private bool IsContradictory(VignetteResponse r1, VignetteResponse r2, MirrorPair pair)
    {
        // Check if responses contradict expected correlation
        if (pair.ExpectedCorrelation.Contains("negative", StringComparison.OrdinalIgnoreCase))
        {
            // For negative correlation, same answer is contradictory
            return r1.SelectedOption == r2.SelectedOption;
        }
        else if (pair.ExpectedCorrelation.Contains("positive", StringComparison.OrdinalIgnoreCase))
        {
            // For positive correlation, opposite extremes are contradictory
            return (r1.SelectedOption == "A" && r2.SelectedOption == "B") ||
                   (r1.SelectedOption == "B" && r2.SelectedOption == "A");
        }

        return false;
    }

    private bool ShowsAsymmetry(VignetteResponse r1, VignetteResponse r2, MirrorPair pair)
    {
        // Self (As) questions tend to get situational explanations (A)
        // Other (Tu) questions tend to get dispositional explanations (C)
        // Classic fundamental attribution error
        return (r1.SelectedOption == "A" && r2.SelectedOption == "C") ||
               (r1.SelectedOption == "B" && r2.SelectedOption == "C");
    }

    private bool HasSelfOtherAsymmetry(
        IReadOnlyList<VignetteResponse> responses,
        IReadOnlyList<MirrorPair> mirrorPairs)
    {
        var asTuPairs = mirrorPairs.Where(p =>
            p.ExpectedCorrelation.Contains("asymmetry", StringComparison.OrdinalIgnoreCase));

        return asTuPairs.Any(pair =>
        {
            var r1 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId1);
            var r2 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId2);
            return r1 != null && r2 != null && ShowsAsymmetry(r1, r2, pair);
        });
    }

    private bool HasResponseLatencySpikes(IReadOnlyList<VignetteResponse> responses)
    {
        // Removed - no temporal analysis
        return false;
    }

    private bool HasBacktrackPattern(IReadOnlyList<VignetteResponse> responses)
    {
        var changedResponses = responses.Where(r => r.WasChanged && r.ChangeCount > 1);
        return changedResponses.Count() >= 3; // Multiple backtracking instances
    }

    private bool AllResponsesFast(IReadOnlyList<VignetteResponse> responses)
    {
        // Removed - no temporal analysis
        return false;
    }

    private bool HasSocialDesirabilitySpike(IReadOnlyList<VignetteResponse> responses)
    {
        // High proportion of socially desirable answers
        return ComputeSocialDesirability(responses) > 0.7f;
    }

    /// <summary>
    /// Calculate COGNITIVE DISSONANCE: weighted inconsistency on mirror pairs,
    /// accounting for strength of opposing beliefs and change patterns.
    /// High dissonance = actively struggling with contradictory self-views.
    /// </summary>
    private float ComputeCognitiveDissonance(
        IReadOnlyList<VignetteResponse> responses,
        IReadOnlyList<MirrorPair> mirrorPairs)
    {
        if (!mirrorPairs.Any()) return 0f;

        var dissonanceSum = 0f;
        var pairCount = 0;

        foreach (var pair in mirrorPairs)
        {
            var r1 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId1);
            var r2 = responses.FirstOrDefault(r => r.VignetteId == pair.VignetteId2);

            if (r1 != null && r2 != null)
            {
                // Base contradiction check
                var isContradictory = IsContradictory(r1, r2, pair);

                if (isContradictory)
                {
                    // Weight by change count - more changes = more dissonance
                    var changeWeight = 1f + (r1.ChangeCount + r2.ChangeCount) * 0.2f;

                    // Weight by extremity - contradicting extremes shows more dissonance
                    var extremityWeight = 1f;
                    if ((r1.SelectedOption == "A" && r2.SelectedOption == "C") ||
                        (r1.SelectedOption == "C" && r2.SelectedOption == "A"))
                    {
                        extremityWeight = 1.5f;
                    }

                    dissonanceSum += changeWeight * extremityWeight;
                    pairCount++;
                }
            }
        }

        return pairCount > 0 ? dissonanceSum / mirrorPairs.Count : 0f;
    }
}
