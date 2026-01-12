using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using WhoAmI.ViewModels;

namespace WhoAmI.UI;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void StartTest_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.StartTest();
    }

    private void AnswerOption_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Parent?.Parent is ItemsControl itemsControl)
        {
            var index = itemsControl.IndexFromContainer(button.Parent);
            if (index >= 0)
            {
                ViewModel.AnswerQuestion(index);
            }
        }
    }

    private async void SaveResults_Click(object? sender, RoutedEventArgs e)
    {
        var fileName = $"WhoAmI_Results_{DateTime.Now:yyyyMMdd_HHmmss}";
        
        var storageProvider = StorageProvider;
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Personality Results",
            SuggestedFileName = fileName,
            DefaultExtension = "json",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            }
        });

        if (file != null)
        {
            var basePath = file.Path.LocalPath;
            // Remove any extension from the path
            if (basePath.EndsWith(".json") || basePath.EndsWith(".txt"))
            {
                basePath = basePath.Substring(0, basePath.LastIndexOf('.'));
            }
            
            await ViewModel.SaveResultsAsync(basePath);
            
            // Show confirmation
            var messageBox = new Window
            {
                Title = "Success",
                Width = 300,
                Height = 150,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Children =
                    {
                        new TextBlock { Text = "Results saved successfully!", Margin = new Avalonia.Thickness(0, 0, 0, 20) },
                        new TextBlock { Text = $"{basePath}.json", TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new TextBlock { Text = $"{basePath}.txt", TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Avalonia.Thickness(0, 5, 0, 0) }
                    }
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            await messageBox.ShowDialog(this);
        }
    }

    private void RestartTest_Click(object? sender, RoutedEventArgs e)
    {
        DataContext = new MainViewModel();
    }
}
