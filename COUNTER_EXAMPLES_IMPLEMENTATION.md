# Counter-Example Questions & Response Tracking Implementation

## Overview
This implementation adds **gaming detection**, **timing instrumentation**, and **As vs Tu testing** to detect personality test manipulation and capture meta-cognitive patterns.

## What's Been Implemented

### 1. **Counter-Example Questions** (`WhoAmI/Data/counter-examples.json`)
16 paired vignettes that test for inconsistency and gaming behavior:

#### Key Pairs:
- **ce01/ce02: Control Seeking**
  - "Take over when plan fails" vs "Someone changes your plan"
  - Tests if control-seeking is consistent across active/reactive frames
  - Gaming trap: A+A or B+B = latent contradiction

- **ce03/ce04: Decision Speed (Dominance Pressure)**
  - Forced choice: "Wrong call vs wait" → "Regret retrospective"
  - Option C repeatedly = hedging bias flag

- **ce05/ce06: Attribution Bias (As vs Tu)**
  - Why YOU cancel vs why OTHERS cancel
  - Catches fundamental attribution error (situational for self, dispositional for others)

- **ce07/ce08: Intent Inference (Theory of Mind)**
  - 3rd person: "Alex interrupts Jamie"
  - 2nd person: "Someone interrupts you"
  - Tests attribution style consistency

- **ce09/ce10: Authority Skepticism**
  - Single expert vs consensus pressure
  - Tests belief stability under social pressure

- **ce11/ce12: Agency & Locus of Control**
  - Praise for luck vs Blame for bad luck
  - Emotional frame flip catches self-serving bias

- **ce13/ce14: Deadline Pressure**
  - Preference vs retrospective feeling
  - Tests deadline-driven trait consistency

- **ce15/ce16: Confrontation Tolerance**
  - Initiating vs responding to conflict
  - Active vs reactive consistency

### 2. **Response Tracking Models** (`WhoAmI/Models/`)

#### `VignetteResponse.cs`
Captures the **meta-data** of each answer:
```csharp
- ResponseTimeMs          // How long to answer
- WasChanged              // Did they go back?
- PreviousSelection       // What was it before?
- ChangeCount             // How many times changed?
- TimeToFirstAnswer       // Gut reaction speed
- TimeToFinalAnswer       // Final decision time
```

#### `QuestionState.cs`
Full state tracking per question:
```csharp
- AnswerHistory[]         // Complete change log
- ViewCount               // How many times viewed
- TotalTimeOnQuestionMs   // Cumulative time spent
- IsFlagged               // User marked uncertain
- FirstViewedAt           // Timestamp tracking
```

#### `MetaScores`
Computed from response patterns:
- **SelfModelStability** - Low variance, few changes
- **HedgingTendency** - Frequency of "depends" answers
- **SocialDesirability** - Picking culturally "good" answers
- **TheoryOfMindBias** - As vs Tu attribution gap
- **ImpressionManagement** - Backtracking toward "better" answers
- **LatentContradiction** - Inconsistency on mirror pairs
- **ResponseConfidence** - Inverse of latency spikes

### 3. **Response Analyzer** (`WhoAmI/Services/ResponseAnalyzer.cs`)
Detects gaming patterns via flags:
- `LatentContradiction` - Inconsistent on non-obvious mirrors
- `HedgingBias` - 3+ "depends" answers
- `SelfOtherAsymmetry` - As-Tu attribution gap
- `ResponseLatencySpike` - Very slow on "sensitive" questions
- `BacktrackPattern` - Systematic changes toward neutral
- `FastUniformResponses` - Not reading carefully (< 1.5s avg)
- `SocialDesirabilitySpike` - All answers cluster toward "ideal"

### 4. **UI Enhancements** (`WhoAmI/UI/MainWindow.axaml`)

#### Navigation Controls:
- **← Previous / Next →** buttons
- **Jump to Flagged** - Quick nav to uncertain questions
- **Jump to Unanswered** - Find remaining questions
- **Review & Finish** - Only appears when all answered

#### Progress Indicators:
- **Answered count** (green)
- **Flagged count** (orange)
- **Current position** (Q X of Y)

#### Flagging System:
- **🚩 Flag button** on each question (top-right)
- Changes color when flagged
- "✓ Answered (you can change your answer)" banner
- Tracks all changes in history

#### Timing Instrumentation:
- Automatic timing when entering/leaving questions
- Cumulative time per question
- Response latency tracking
- Change history with timestamps

### 5. **ViewModel Logic** (`WhoAmI/ViewModels/MainViewModel.cs`)

New methods:
- `GoBack()` / `GoForward()` - Navigate questions
- `ToggleFlagCurrentQuestion()` - Mark uncertain
- `GoToFirstFlagged()` - Jump to review
- `GoToFirstUnanswered()` - Find gaps
- `GoToNextUnanswered()` - Auto-advance
- `RecordTimeOnQuestion()` - Timing instrumentation

New properties:
- `CanGoBack` / `CanGoForward` - Enable/disable nav
- `IsCurrentQuestionAnswered` - Show status
- `IsCurrentQuestionFlagged` - Show flag state
- `AnsweredQuestionCount` - Progress tracking
- `FlaggedQuestionCount` - Review counter
- `ChangedQuestionCount` - Turbulence metric

## How It Detects Gaming

### 1. **Non-Obvious Mirrors**
Not direct opposites, but different angles:
- **Active frame** ("I take over") vs **Reactive frame** ("Someone took over")
- **Agency frame** vs **Emotional frame** vs **Retrospective frame**

Real people are inconsistent across frames. Gamers stay "logically consistent."

### 2. **As vs Tu (Actor-Self vs Theory-User)**
Questions about YOUR behavior vs OTHERS' behavior:
- Fundamental attribution error: Situational for self, dispositional for others
- Gaming detection: If answers are perfectly symmetrical → suspicious

### 3. **Dominance Pressure Tests**
Forced discomfort where "depends" is the hedge:
- "I'd rather be wrong than wait" vs "I'd rather wait than be wrong"
- Option C: "Depends who's involved"
- 3+ hedges across test = hedging bias flag

### 4. **Response Timing Patterns**
- **Fast uniform** (< 1.5s all) = Not reading carefully
- **Latency spikes** (> 15s some) = Self-editing on sensitive items
- **High variance** = Uncertain self-model
- **Many changes** = Impression management

### 5. **Backtracking Patterns**
Direction of changes reveals motivation:
- A → B or A → C = Moving toward middle (hedging)
- Many changes = Uncertainty or gaming
- Changes after viewing later questions = Realization of contradiction

## Meta-Data as Dimensions

These are NOT errors - they're **personality dimensions**:

Two people with identical trait profiles can differ in:
- **Self-awareness** (low contradiction score)
- **Attribution style** (As-Tu asymmetry)
- **Gaming tendency** (social desirability)
- **Internal coherence** (self-model stability)
- **Decision confidence** (response speed variance)

This is differential psychology beyond Big Five.

## What's Still TODO

1. **Load mirror pairs from JSON** - Currently hardcoded empty list
2. **Expand to 60+ questions** - Need more vignettes
3. **Visual question grid** - See all Q's at once with status colors
4. **Meta-scores in results** - Display gaming detection results
5. **Export full response history** - JSON with timing data
6. **A/B test questions** - Some users get different question orders

## Usage Flow

1. User starts test
2. Answers questions (can go back/forth anytime)
3. Flags uncertain questions with 🚩
4. All timing/changes tracked automatically
5. "Review & Finish" appears when all answered
6. Can still navigate to review flagged questions
7. Clicks finish → computes both:
   - **Personality profile** (trait manifold)
   - **Meta-scores** (gaming detection, self-model stability)
8. Results show BOTH layers

## Key Insight

> "The test measures not just WHAT you answer, but HOW you answer it."

- Fast, confident answers with few changes = High self-model stability
- Slow, many backtracks = Low confidence or impression management
- As-Tu asymmetry = Normal human bias (fundamental attribution error)
- Perfect symmetry = Suspicious (gaming or unusual self-awareness)

The meta-layer is as interesting as the trait layer.
