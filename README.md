# WhoAmI - High-Dimensional Personality Assessment

A next-generation personality test that avoids traditional "Big Five" bucketing by using high-dimensional vector spaces to preserve nuance, contradictions, and context-dependent behavior.

## Features

### 🧠 High-Dimensional Analysis
- **Sparse Multi-Hot Vectors**: Each response activates multiple trait dimensions simultaneously
- **Co-Activation Tracking**: Preserves relationships where "opposite" traits can coexist
- **Context-Aware Profiles**: Traits vary by situation (Work, Social, Stress, etc.) instead of being averaged globally
- **Contradiction Tolerance**: Embraces paradoxes as features, not bugs

### 📊 Dual Output Format
1. **16 Personalities / MBTI Type**: Familiar framework for easy sharing
2. **High-Dimensional Profile**: Complete nuanced representation with:
   - Dominant traits across all contexts
   - Context-specific trait profiles
   - Co-activation patterns (trait pairs that appear together)
   - Personality metrics (diversity, context variance, contradiction tolerance)

### 💾 Export Capabilities
- **JSON**: Structured data for further analysis
- **TXT**: Human-readable report

### 🎨 Avalonia GUI
- Cross-platform desktop application (Windows, macOS, Linux)
- Clean, modern interface
- Step-by-step vignette administration
- Rich results visualization

## Architecture

```
WhoAmI/
├── Models/              # Core domain objects
│   ├── TraitDimension.cs
│   ├── SituationalContext.cs
│   ├── Vignette.cs
│   ├── VignetteOption.cs
│   ├── ContextualActivation.cs
│   └── MBTIType.cs
├── Core/                # Business logic
│   ├── PersonalityManifold.cs    # Stores sparse vectors & co-activations
│   ├── PersonalityAnalyzer.cs    # Computes patterns & metrics
│   └── MBTIMapper.cs             # Projects to 16 Personalities
├── Services/            # I/O operations
│   ├── ITestAdministrator.cs
│   ├── ConsoleTestAdministrator.cs
│   ├── IResultPresenter.cs
│   ├── ConsoleResultPresenter.cs
│   ├── IResultExporter.cs
│   ├── JsonResultExporter.cs
│   └── TextResultExporter.cs
├── Data/                # Test content
│   └── TestData.cs
├── ViewModels/          # MVVM layer
│   └── MainViewModel.cs
└── UI/                  # Avalonia views
    ├── MainWindow.axaml
    └── MainWindow.axaml.cs
```

## How It Works

### 1. Vignettes (Not Self-Assessment)
Instead of "Are you assertive?", we use concrete behavioral scenarios:
> "Your team's project deadline suddenly moves up by a week. No one has said anything yet. Do you:
> A) Immediately start reorganizing tasks and message the team
> B) Wait to see if someone else takes the lead
> C) Assess your own tasks first, then check with others"

### 2. Multi-Dimensional Activation
Each choice activates 3-4 traits with varying strengths:
- Option C activates BOTH `control_seeking` (0.5) AND `trust_delegating` (0.5)
- Traditional tests would force you into one bucket—we preserve the nuance

### 3. Context Preservation
Traits are tracked by situation:
- You might be `control_seeking` in Work contexts but `trust_delegating` in Social contexts
- No global averaging that loses information

### 4. Co-Activation Analysis
The system tracks which traits appear together:
- `risk_sampling` + `loss_avoidance` co-activating shows calculated risk-taking
- P(trait2 | trait1) conditional probabilities reveal behavioral patterns

### 5. Dual Output
- **MBTI**: 4-bit projection (16 types) for compatibility
- **High-Dimensional**: Full representation showing complexity

## Key Metrics

- **Trait Diversity**: Number of activated dimensions (complexity)
- **Context Variance**: How much personality changes across situations (adaptability)
- **Contradiction Tolerance**: How often "opposite" traits co-activate (nuance)

## Why Not Big Five?

Traditional personality tests collapse behavior into 5 dimensions (OCEAN). This system:
- Uses 20+ independent dimensions (expandable)
- Preserves contradictions instead of averaging them away
- Tracks context-dependent behavior
- Uses sparse vectors (only stores what's activated)
- Employs co-activation matrices to capture relationships

**The result**: "I'm both risk-seeking AND loss-avoidant depending on context" is valid, not contradictory.

## Running the Application

### Prerequisites
- .NET 9.0 SDK
- Linux / macOS / Windows

### Build & Run
```bash
dotnet restore
dotnet run --project WhoAmI/WhoAmI.csproj
```

Or open in JetBrains Rider and run the "WhoAmI" configuration.

## Future Enhancements

- [ ] More vignettes (currently 3 samples)
- [ ] Additional trait dimensions (Emotional Processing, Belief Formation, etc.)
- [ ] Web version
- [ ] Comparison mode (compare two profiles)
- [ ] Longitudinal tracking (test over time)
- [ ] Custom vignette builder
- [ ] API for third-party integrations

## License

MIT

## Contributing

Contributions welcome! Key areas:
- Adding more scientifically-validated vignettes
- Expanding trait dimension taxonomy
- Improving MBTI mapping algorithm
- UI/UX enhancements
