using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WhoAmI.Core;
using WhoAmI.Data;
using WhoAmI.Models;
using WhoAmI.Services;

namespace WhoAmI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private int _currentQuestionIndex = 0;
    private PersonalityManifold? _manifold;
    private PersonalityProfile? _profile;
    private MBTIType? _mbtiType;
    private MetaScores? _metaScores;
    private char _assertiveTurbulent = 'A';
    private List<SubTrait> _subTraits = new();
    private bool _isTestComplete = false;
    private bool _isTestStarted = false;
    private DateTime _questionEnteredAt;

    private readonly Dictionary<string, QuestionState> _questionStates = new();
    private readonly List<MirrorPair> _mirrorPairs = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Vignette> Vignettes { get; }

    public MainViewModel()
    {
        Vignettes = new ObservableCollection<Vignette>(TestData.GetVignettes());
        InitializeQuestionStates();
    }

    private void InitializeQuestionStates()
    {
        foreach (var vignette in Vignettes)
        {
            _questionStates[vignette.Id] = new QuestionState
            {
                VignetteId = vignette.Id
            };
        }

        // Load mirror pairs
        var mirrorPairs = TestData.GetMirrorPairs();
        _mirrorPairs.Clear();
        _mirrorPairs.AddRange(mirrorPairs);
    }

    public bool IsTestStarted
    {
        get => _isTestStarted;
        set
        {
            _isTestStarted = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsWelcomeScreen));
            OnPropertyChanged(nameof(IsTestInProgress));
        }
    }

    public bool IsWelcomeScreen => !_isTestStarted;

    public bool IsTestComplete
    {
        get => _isTestComplete;
        set
        {
            _isTestComplete = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsTestInProgress));
        }
    }

    public bool IsTestInProgress => IsTestStarted && !IsTestComplete;

    public int CurrentQuestionIndex
    {
        get => _currentQuestionIndex;
        set
        {
            if (_currentQuestionIndex != value && CurrentQuestion != null)
            {
                // Record time spent on previous question
                RecordTimeOnQuestion();
            }

            _currentQuestionIndex = value;
            _questionEnteredAt = DateTime.Now;

            // Record view of new question
            if (CurrentQuestion != null)
            {
                var state = _questionStates[CurrentQuestion.Id];
                state.RecordView(DateTime.Now);
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
            OnPropertyChanged(nameof(IsCurrentQuestionAnswered));
            OnPropertyChanged(nameof(IsCurrentQuestionFlagged));
            OnPropertyChanged(nameof(FlaggedQuestionCount));
        }
    }

    public Vignette? CurrentQuestion =>
        CurrentQuestionIndex < Vignettes.Count ? Vignettes[CurrentQuestionIndex] : null;

    public string ProgressText => $"Question {CurrentQuestionIndex + 1} of {Vignettes.Count}";

    public int TotalQuestionCount => Vignettes.Count;

    public bool CanGoBack => CurrentQuestionIndex > 0;
    public bool CanGoForward => CurrentQuestionIndex < Vignettes.Count - 1;

    public bool IsCurrentQuestionAnswered
    {
        get
        {
            if (CurrentQuestion == null) return false;
            return _questionStates[CurrentQuestion.Id].HasBeenAnswered;
        }
    }

    public bool IsCurrentQuestionFlagged
    {
        get
        {
            if (CurrentQuestion == null) return false;
            return _questionStates[CurrentQuestion.Id].IsFlagged;
        }
    }

    public int AnsweredQuestionCount => _questionStates.Values.Count(s => s.HasBeenAnswered);
    public int FlaggedQuestionCount => _questionStates.Values.Count(s => s.IsFlagged);
    public int ChangedQuestionCount => _questionStates.Values.Count(s => s.HasBeenChanged);
    public bool AllQuestionsAnswered => AnsweredQuestionCount >= TotalQuestionCount;

    public PersonalityProfile? Profile
    {
        get => _profile;
        set
        {
            _profile = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DominantTraitsDisplay));
            OnPropertyChanged(nameof(TraitDiversityDisplay));
            OnPropertyChanged(nameof(ContextVarianceDisplay));
            OnPropertyChanged(nameof(ContradictionToleranceDisplay));
        }
    }

    public ObservableCollection<TraitDisplayItem> DominantTraitsDisplay
    {
        get
        {
            if (Profile == null) return new ObservableCollection<TraitDisplayItem>();

            return new ObservableCollection<TraitDisplayItem>(
                Profile.DominantTraits.Select(t => new TraitDisplayItem
                {
                    TraitId = t.TraitId,
                    Strength = t.TotalStrength
                })
            );
        }
    }

    public int TraitDiversityDisplay => Profile?.TraitDiversity ?? 0;
    public string ContextVarianceDisplay => Profile?.ContextVariance.ToString("F2") ?? "0.00";
    public string ContradictionToleranceDisplay => Profile?.ContradictionTolerance.ToString("F2") ?? "0.00";

    public MBTIType? MBTIType
    {
        get => _mbtiType;
        set
        {
            _mbtiType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MBTITypeDisplay));
        }
    }

    public string MBTITypeDisplay
    {
        get
        {
            if (MBTIType == null) return "";
            var info = MBTIPersonality.Definitions[MBTIType.Value];
            return $"{MBTIType} - {info.Nickname}\n{info.Description}";
        }
    }

    public string MBTITypeWithVariant => MBTIType != null ? $"{MBTIType}-{_assertiveTurbulent}" : "";

    public string AssertiveTurbulentDescription
    {
        get
        {
            if (_assertiveTurbulent == 'A')
            {
                return "Assertive: Self-assured, emotionally stable, resistant to stress. You trust your abilities and don't second-guess decisions.";
            }
            else
            {
                return "Turbulent: Self-conscious, success-driven, perfectionistic. You're sensitive to stress and often revisit decisions to improve them.";
            }
        }
    }

    public List<SubTrait> SubTraitsDisplay => _subTraits;

    public void StartTest()
    {
        IsTestStarted = true;
        CurrentQuestionIndex = 0;
        _manifold = new PersonalityManifold();
        _questionEnteredAt = DateTime.Now;

        if (CurrentQuestion != null)
        {
            var state = _questionStates[CurrentQuestion.Id];
            state.RecordView(DateTime.Now);
        }
    }

    public void AnswerQuestion(int optionIndex)
    {
        if (_manifold == null || CurrentQuestion == null) return;

        var option = CurrentQuestion.Options[optionIndex];
        _manifold.RecordResponse(option, CurrentQuestion.Context);

        // Record answer in state tracking
        var state = _questionStates[CurrentQuestion.Id];
        state.RecordAnswer(option.Label, DateTime.Now);

        OnPropertyChanged(nameof(IsCurrentQuestionAnswered));
        OnPropertyChanged(nameof(AnsweredQuestionCount));
        OnPropertyChanged(nameof(ChangedQuestionCount));
        OnPropertyChanged(nameof(AllQuestionsAnswered));

        // Auto-advance to next unanswered question
        GoToNextUnanswered();
    }

    public void GoBack()
    {
        if (CanGoBack)
        {
            CurrentQuestionIndex--;
        }
    }

    public void GoForward()
    {
        if (CanGoForward)
        {
            CurrentQuestionIndex++;
        }
    }

    public void GoToNextUnanswered()
    {
        // Find next unanswered question
        for (int i = CurrentQuestionIndex + 1; i < Vignettes.Count; i++)
        {
            if (!_questionStates[Vignettes[i].Id].HasBeenAnswered)
            {
                CurrentQuestionIndex = i;
                return;
            }
        }

        // If all ahead are answered, check if test is complete
        if (AnsweredQuestionCount == Vignettes.Count)
        {
            // Don't auto-complete, let user review
            if (CurrentQuestionIndex < Vignettes.Count - 1)
                CurrentQuestionIndex++;
        }
        else if (CurrentQuestionIndex < Vignettes.Count - 1)
        {
            CurrentQuestionIndex++;
        }
    }

    public void ToggleFlagCurrentQuestion()
    {
        if (CurrentQuestion == null) return;

        var state = _questionStates[CurrentQuestion.Id];
        state.IsFlagged = !state.IsFlagged;

        OnPropertyChanged(nameof(IsCurrentQuestionFlagged));
        OnPropertyChanged(nameof(FlaggedQuestionCount));
    }

    public void GoToFirstFlagged()
    {
        for (int i = 0; i < Vignettes.Count; i++)
        {
            if (_questionStates[Vignettes[i].Id].IsFlagged)
            {
                CurrentQuestionIndex = i;
                return;
            }
        }
    }

    public void GoToFirstUnanswered()
    {
        for (int i = 0; i < Vignettes.Count; i++)
        {
            if (!_questionStates[Vignettes[i].Id].HasBeenAnswered)
            {
                CurrentQuestionIndex = i;
                return;
            }
        }
    }

    private void RecordTimeOnQuestion()
    {
        if (CurrentQuestion == null) return;

        var state = _questionStates[CurrentQuestion.Id];
        var timeSpent = (long)(DateTime.Now - _questionEnteredAt).TotalMilliseconds;
        state.TotalTimeOnQuestionMs += timeSpent;
    }

    public void CompleteTest()
    {
        if (_manifold == null) return;

        // Record final time on current question
        RecordTimeOnQuestion();

        // Generate personality profile
        var analyzer = new PersonalityAnalyzer(_manifold);
        Profile = analyzer.GenerateProfile();

        var mapper = new MBTIMapper(Profile);
        MBTIType = mapper.MapToMBTI();

        // Compute meta-scores from response patterns
        var responses = _questionStates.Values
            .Where(s => s.HasBeenAnswered)
            .Select(s => s.ToVignetteResponse())
            .ToList();

        var responseAnalyzer = new ResponseAnalyzer();
        _metaScores = responseAnalyzer.ComputeMetaScores(responses, _mirrorPairs);

        // Calculate Assertive/Turbulent variant
        if (_metaScores != null)
        {
            _assertiveTurbulent = mapper.GetAssertiveTurbulent(_metaScores);
        }

        // Get sub-traits (strong non-dominant functions)
        _subTraits = mapper.GetSubTraits();

        OnPropertyChanged(nameof(MBTITypeWithVariant));
        OnPropertyChanged(nameof(AssertiveTurbulentDescription));
        OnPropertyChanged(nameof(SubTraitsDisplay));

        IsTestComplete = true;
    }

    public MetaScores? MetaScores
    {
        get => _metaScores;
        set
        {
            _metaScores = value;
            OnPropertyChanged();
        }
    }

    public async Task SaveResultsAsync(string basePath)
    {
        if (Profile == null || MBTIType == null) return;

        var jsonExporter = new JsonResultExporter();
        var textExporter = new TextResultExporter();

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var jsonPath = $"{basePath}_results_{timestamp}.json";
        var textPath = $"{basePath}_results_{timestamp}.txt";

        await jsonExporter.ExportAsync(Profile, MBTIType.Value, jsonPath);
        await textExporter.ExportAsync(Profile, MBTIType.Value, textPath);
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
