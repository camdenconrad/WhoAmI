# WhoAmI Test - Question Summary

## Total Questions: 60

### Core Vignettes: 44 questions
**Source:** `WhoAmI/Data/test-data.json`

- v01-v32: Original 32 universal behavioral vignettes
- v33-v44: 12 additional vignettes covering:
  - Explaining complex ideas (learning style)
  - Hitting obstacles (emotional reaction + problem-solving)
  - Group discussion patterns (social interaction)
  - Vague instructions (ambiguity tolerance)
  - Deadline prioritization (stress management)
  - Correcting misinformation (social boundaries)
  - Learning new skills (cognitive style)
  - Public competence challenge (conflict response)
  - Starting over (emotional resilience)
  - Crisis response (leadership style)
  - Brainstorming preferences (creative process)
  - Scope creep handling (boundary assertion)

### Counter-Example Vignettes: 16 questions
**Source:** `WhoAmI/Data/counter-examples.json`

Gaming detection and consistency checking questions arranged in 8 mirror pairs:

1. **ce01/ce02** - Control seeking (active vs reactive frames)
2. **ce03/ce04** - Decision speed with dominance pressure
3. **ce05/ce06** - **As vs Tu:** Self vs other attribution (canceling plans)
4. **ce07/ce08** - **Theory of mind:** 3rd person vs 2nd person (interrupting)
5. **ce09/ce10** - Authority skepticism under pressure
6. **ce11/ce12** - Agency/locus asymmetry (praise vs blame)
7. **ce13/ce14** - Deadline pressure (preference vs retrospective)
8. **ce15/ce16** - Confrontation tolerance (initiating vs responding)

## Implementation Status

### ✅ Completed:
- [x] 44 core vignettes in test-data.json
- [x] 16 counter-example questions with mirror pairs
- [x] Automatic merging of both files via JsonTestDataLoader
- [x] Mirror pair loading for gaming detection
- [x] Full navigation UI (back/forward/jump)
- [x] Flagging system for uncertain questions
- [x] Complete timing instrumentation
- [x] Response history tracking
- [x] Meta-scoring computation

### 📊 Test Flow:
1. User sees all 60 questions in mixed order
2. Can navigate freely (← Previous / Next →)
3. Can flag uncertain questions with 🚩
4. Can change answers anytime (tracked in history)
5. All timing recorded automatically
6. "Review & Finish" appears when all answered
7. Meta-scores computed on completion

### 🎯 What Gets Measured:
- **Personality traits** (68 dimensions) from all 60 answers
- **Meta-scores** (7 dimensions) from HOW they answered:
  - Self-Model Stability
  - Hedging Tendency
  - Social Desirability
  - Theory of Mind Bias (As vs Tu)
  - Impression Management
  - Latent Contradiction
  - Response Confidence

## Question Distribution by Context:

- **Work:** ~35 questions
- **Social:** ~20 questions
- **Learning:** ~8 questions
- **Conflict:** ~6 questions
- **Emotional:** ~5 questions
- **Problem-solving:** ~8 questions
- **Stress/Crisis:** ~4 questions
- **Creative:** ~2 questions

(Note: Many questions span multiple contexts)

## Gaming Detection Coverage:

- **8 mirror pairs** testing consistency across frames
- **As vs Tu bias detection** (self vs other attribution)
- **Dominance pressure tests** (forced discomfort choices)
- **Hedging markers** (repeated "depends" selections)
- **Social desirability traps** (culturally "ideal" answers)
- **Response timing patterns** (fast uniform vs latency spikes)
- **Backtracking analysis** (direction and frequency of changes)

## Next Steps (Optional Enhancements):

1. Randomize question order per test-taker
2. Add question grid overview (see all 60 at once with status)
3. Display meta-scores in results screen
4. Export full response history to JSON
5. A/B test: Some users get different question orders
6. Add more mirror pairs (target: 12-16 pairs)
7. Reach 80-100 total questions for even better coverage

## File Structure:

```
WhoAmI/Data/
├── test-data.json           # 44 core vignettes + 68 trait definitions
└── counter-examples.json    # 16 counter vignettes + 8 mirror pairs

WhoAmI/Models/
├── Vignette.cs             # Question model
├── VignetteOption.cs       # Answer choice model
├── VignetteResponse.cs     # Response metadata + meta-scores
├── QuestionState.cs        # Full state tracking per question
└── MirrorPair.cs           # Gaming detection pair definition

WhoAmI/Services/
├── JsonTestDataLoader.cs   # Loads and merges both JSON files
└── ResponseAnalyzer.cs     # Computes meta-scores and flags

WhoAmI/ViewModels/
└── MainViewModel.cs        # Navigation, flagging, timing logic
```

The test now has **60 questions** with full navigation, timing, and gaming detection capabilities.
