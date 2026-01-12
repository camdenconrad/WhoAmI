using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    private bool _isTestComplete = false;
    private bool _isTestStarted = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Vignette> Vignettes { get; }

    public MainViewModel()
    {
        Vignettes = new ObservableCollection<Vignette>(TestData.GetVignettes());
    }

    public bool IsTestStarted
    {
        get => _isTestStarted;
        set
        {
            _isTestStarted = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsWelcomeScreen));
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
            _currentQuestionIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(ProgressText));
        }
    }

    public Vignette? CurrentQuestion => 
        CurrentQuestionIndex < Vignettes.Count ? Vignettes[CurrentQuestionIndex] : null;

    public string ProgressText => $"Question {CurrentQuestionIndex + 1} of {Vignettes.Count}";

    public PersonalityProfile? Profile
    {
        get => _profile;
        set
        {
            _profile = value;
            OnPropertyChanged();
        }
    }

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

    public void StartTest()
    {
        IsTestStarted = true;
        CurrentQuestionIndex = 0;
        _manifold = new PersonalityManifold();
    }

    public void AnswerQuestion(int optionIndex)
    {
        if (_manifold == null || CurrentQuestion == null) return;

        var option = CurrentQuestion.Options[optionIndex];
        _manifold.RecordResponse(option, CurrentQuestion.Context);

        if (CurrentQuestionIndex < Vignettes.Count - 1)
        {
            CurrentQuestionIndex++;
        }
        else
        {
            CompleteTest();
        }
    }

    private void CompleteTest()
    {
        if (_manifold == null) return;

        var analyzer = new PersonalityAnalyzer(_manifold);
        Profile = analyzer.GenerateProfile();

        var mapper = new MBTIMapper(Profile);
        MBTIType = mapper.MapToMBTI();

        IsTestComplete = true;
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
