# WhoAmI - Usage Guide

## Running the Application

### Option 1: Using JetBrains Rider
1. Open `WhoAmI.sln` in Rider
2. Select the "WhoAmI" run configuration
3. Click the Run button (▶️)

### Option 2: Command Line
```bash
cd /home/coffee/RiderProjects/WhoAmI
dotnet run --project WhoAmI/WhoAmI.csproj
```

## Taking the Test

### 1. Welcome Screen
- Read the instructions about behavioral vignettes
- Click "Start Assessment" when ready

### 2. Answering Questions
- Each vignette presents a realistic situation
- Choose the option that matches your **actual behavior**, not your ideal
- Context tags (e.g., `[Work, Stress]`) indicate the situation type
- Progress counter shows how many questions remain

### 3. View Results
After completing all questions, you'll see:

#### 16 Personalities Type (Top Section)
- Familiar MBTI format (e.g., INTJ - Architect)
- Quick summary for sharing with others
- This is a **projection** from the full high-dimensional profile

#### High-Dimensional Profile (Middle Section)
- **Dominant Traits**: Your top activated behavioral dimensions
- **Personality Metrics**:
  - `Trait Diversity`: How many dimensions you use (complexity)
  - `Context Variance`: How much you adapt to situations
  - `Contradiction Tolerance`: Comfort with paradox

### 4. Save Results
- Click "Save Results (JSON + TXT)"
- Choose a location and base filename
- Two files are created:
  - `filename_results_timestamp.json` - Structured data
  - `filename_results_timestamp.txt` - Human-readable report

### 5. Take Again
- Click "Take Test Again" to restart with fresh state
- Results are independent (no history tracking yet)

## Understanding Your Results

### MBTI Type vs High-Dimensional Profile

**MBTI (16 Personalities)**: 4 binary dimensions = 16 types
- Familiar, easy to share
- **Loses nuance** through averaging

**High-Dimensional Profile**: 20+ continuous dimensions
- Preserves contradictions (e.g., both risk-seeking AND loss-avoidant)
- Shows context-dependent behavior
- Reveals trait co-activation patterns

### Example Output

```
16 Personalities Type: INTJ - Architect
Strategic, analytical, independent thinker

High-Dimensional Profile:

Dominant Traits:
  analytical_parsing: 2.30
  control_seeking: 2.10
  rule_first: 1.80
  loss_avoidance: 1.20
  risk_sampling: 1.20  ← Note: both traits present!

Personality Metrics:
  Trait Diversity: 15 (complexity)
  Context Variance: 0.42 (adaptability)
  Contradiction Tolerance: 1.67 (nuance)
```

### What the Metrics Mean

**Trait Diversity (0-22)**
- Low (< 10): Focused, consistent behavior
- High (> 15): Complex, multifaceted personality

**Context Variance (0-5+)**
- Low (< 0.5): Consistent across situations
- High (> 1.0): Strong situational adaptation

**Contradiction Tolerance (0-3+)**
- Low (< 0.5): Clear preferences, little ambivalence
- High (> 1.5): Comfortable holding opposing tendencies

## Interpreting Co-Activation Patterns

If you see in your JSON export:
```json
"CoActivationPatterns": [
  {
    "Trait1": "risk_sampling",
    "Trait2": "loss_avoidance",
    "Count": 2,
    "Probability": 0.67
  }
]
```

This means:
- You activated both traits together in 2 responses
- When `risk_sampling` appears, `loss_avoidance` co-occurs 67% of the time
- **Interpretation**: You take calculated risks with backup plans

## File Formats

### JSON Export
```json
{
  "Timestamp": "2025-01-11T12:34:56Z",
  "MBTI": {
    "Type": "INTJ",
    "Nickname": "Architect",
    "Description": "Strategic, analytical, independent thinker"
  },
  "HighDimensionalProfile": {
    "DominantTraits": [...],
    "ContextualProfiles": {...},
    "CoActivationPatterns": [...],
    "Metrics": {...}
  }
}
```

### TXT Export
Human-readable report with:
- MBTI type summary
- Dominant traits list
- Context-specific profiles
- Co-activation patterns
- Personality metrics

## Tips for Accurate Results

1. **Be Honest**: Choose what you actually do, not what you think is "correct"
2. **Think Recent**: Consider your behavior in the past few weeks
3. **Context Matters**: The situation tags help—imagine yourself in that setting
4. **No "Wrong" Answers**: All combinations are valid personality patterns
5. **Contradictions Are OK**: You can be both control-seeking AND trust-delegating

## Common Questions

**Q: My MBTI type doesn't match my high-dimensional profile?**
A: That's expected! MBTI collapses 20+ dimensions into 4 bits. The high-dimensional profile is more accurate.

**Q: I got different results when taking the test twice?**
A: Small variations are normal. Focus on consistent patterns across attempts.

**Q: Can I add more questions?**
A: Yes! Edit `WhoAmI/Data/TestData.cs` and add vignettes to `GetVignettes()`. Follow the existing format.

**Q: How accurate is the MBTI mapping?**
A: It's a best-effort projection. Some high-dimensional profiles don't fit cleanly into 16 types. Use it as a rough approximation.

**Q: What makes this different from other personality tests?**
A: Most tests average traits into 5-10 dimensions. This preserves contradictions, tracks context-dependence, and uses sparse vectors instead of forced scoring.

## Extending the Test

### Adding Traits
Edit `WhoAmI/Data/TestData.cs` → `GetTraits()`:
```csharp
new TraitDimension(
    "your_trait_id",
    "Your Trait Name",
    "Category",
    "Description"
)
```

### Adding Vignettes
Edit `WhoAmI/Data/TestData.cs` → `GetVignettes()`:
```csharp
new Vignette(
    "unique_id",
    "Scenario description here...",
    SituationalContext.Work | SituationalContext.Social,
    ImmutableArray.Create(
        new VignetteOption("A", "Option A text",
            ImmutableDictionary.CreateRange(new[] {
                KeyValuePair.Create("trait_id_1", 0.8f),
                KeyValuePair.Create("trait_id_2", 0.6f)
            })),
        // ... more options
    )
)
```

## Technical Details

**Architecture**: MVVM pattern with Avalonia UI
**Storage**: In-memory during test, exported to JSON/TXT at end
**Platform**: Cross-platform (.NET 9.0)
**Dependencies**: Avalonia 11.2.2

For code structure, see `README.md`.
