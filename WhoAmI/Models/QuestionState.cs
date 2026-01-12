using System;
using System.Collections.Generic;

namespace WhoAmI.Models;

/// <summary>
/// Tracks the full state and history of a single question response.
/// Captures timing, changes, flags - everything needed for meta-analysis.
/// </summary>
public class QuestionState
{
    public string VignetteId { get; set; } = string.Empty;
    public string? CurrentAnswer { get; set; }
    public bool IsFlagged { get; set; }
    public bool HasBeenAnswered => CurrentAnswer != null;
    public bool HasBeenChanged => AnswerHistory.Count > 1;

    public DateTime? FirstViewedAt { get; set; }
    public DateTime? LastViewedAt { get; set; }
    public DateTime? FirstAnsweredAt { get; set; }
    public DateTime? LastAnsweredAt { get; set; }

    public List<AnswerHistoryEntry> AnswerHistory { get; } = new();
    public int ViewCount { get; set; }
    public long TotalTimeOnQuestionMs { get; set; }

    /// <summary>
    /// Records a view of this question.
    /// </summary>
    public void RecordView(DateTime timestamp)
    {
        if (FirstViewedAt == null)
            FirstViewedAt = timestamp;

        LastViewedAt = timestamp;
        ViewCount++;
    }

    /// <summary>
    /// Records an answer (new or changed).
    /// </summary>
    public void RecordAnswer(string option, DateTime timestamp)
    {
        if (FirstAnsweredAt == null)
            FirstAnsweredAt = timestamp;

        LastAnsweredAt = timestamp;

        AnswerHistory.Add(new AnswerHistoryEntry
        {
            Option = option,
            Timestamp = timestamp,
            ResponseTimeMs = FirstViewedAt.HasValue
                ? (long)(timestamp - FirstViewedAt.Value).TotalMilliseconds
                : 0
        });

        CurrentAnswer = option;
    }

    /// <summary>
    /// Creates a VignetteResponse for analysis.
    /// </summary>
    public VignetteResponse ToVignetteResponse()
    {
        if (CurrentAnswer == null)
            throw new InvalidOperationException("Cannot create response for unanswered question");

        var firstEntry = AnswerHistory[0];
        var lastEntry = AnswerHistory[^1];

        return new VignetteResponse(
            VignetteId: VignetteId,
            SelectedOption: CurrentAnswer,
            ResponseTimeMs: lastEntry.ResponseTimeMs,
            WasChanged: HasBeenChanged,
            PreviousSelection: AnswerHistory.Count > 1 ? AnswerHistory[^2].Option : null,
            TimeToFirstAnswer: firstEntry.ResponseTimeMs,
            TimeToFinalAnswer: lastEntry.ResponseTimeMs,
            ChangeCount: AnswerHistory.Count - 1
        );
    }
}

/// <summary>
/// Single entry in the answer change history.
/// </summary>
public class AnswerHistoryEntry
{
    public string Option { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public long ResponseTimeMs { get; set; }
}
