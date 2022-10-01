#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using LaTeX_Validator.DataClasses;
using LaTeX_Validator.Enums;
using LaTeX_Validator.Extensions;
using Microsoft.WindowsAPICodePack.Dialogs;
using WindowsInput;
using WindowsInput.Native;

#endregion

namespace LaTeX_Validator;

/// <summary>
/// Interaction logic for GlsErrorWindow.xaml
/// </summary>
partial class GlsErrorWindow : Window
{
    #region Attributes

    private readonly ObservableCollection<GlsError> allErrors;
    private List<GlsError> persistentIgnoredErrors;
    private List<GlsError> transientIgnoredErrors;
    private readonly ConfigurationGlossary configuration;
    private readonly FileExtractor fileExtractor;
    private readonly FileParser fileParser;
    private GridViewColumnHeader? lastHeaderClicked;
    private ListSortDirection lastSortOrder = ListSortDirection.Ascending;
    private readonly InputSimulator InputSimulator = new();

    #endregion

    #region Initialization

    internal GlsErrorWindow()
    {
        configuration = new ConfigurationGlossary();
        fileExtractor = new FileExtractor();
        fileParser = new FileParser(fileExtractor);
        allErrors = new ObservableCollection<GlsError>();
        persistentIgnoredErrors = new List<GlsError>();
        transientIgnoredErrors = new List<GlsError>();
        InitializeComponent();
        InitializeUiData();
        lvGlsError.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnClick));
    }

    private void InitializeUiData()
    {
        InitializeFromPersistentData();

        lvGlsError.ItemsSource = allErrors;
        LatexDirectoryBox.Text = configuration.latexDirectoryPath;
        PreambleDirectoryBox.Text = configuration.preambleDirectoryPath;
        GlossaryPathBox.Text = configuration.glossaryPath;
        RefOptionPicker.IsChecked = configuration.ignoreSectionLabels;
        BibliographiePathBox.Text = configuration.bibPath;
        PopupDialog.FillwordsBox.Text = configuration.fillWords?.AggregateToString();
        PopupDialog.LabelsBox.Text = configuration.labelsToIgnore?.AggregateToString();

        var text = configuration.ignoreFilesWithMissingGls.ToList();
        if (text.Any()) IgnorableFilesMissingGlsBox.Text = text.Aggregate((x, y) => x + "\n" + y);
    }

    private void InitializeFromPersistentData()
    {
        configuration.latexDirectoryPath = Settings.Default.RootDirectoryPath;
        configuration.preambleDirectoryPath = Settings.Default.PreambleDirectoryPath;
        configuration.glossaryPath = Settings.Default.GlossaryPath;
        configuration.ignoreSectionLabels = Settings.Default.IgnoreSectionLabels;
        configuration.ignoreFilesWithMissingGls = Settings.Default.SettingsPaths;
        configuration.ignoreSettingsFile = Settings.Default.IgnoreSettingsFile;
        configuration.showIgnoredErrors = Settings.Default.ShowIgnoredErrors;
        persistentIgnoredErrors = Settings.Default.SerialzedGlsErrors.ToGlsError();
        configuration.bibPath = Settings.Default.BibPath;
        configuration.labelsToIgnore = Settings.Default.LabelsToIgnore;
        configuration.fillWords = Settings.Default.FillWords;
        configuration.searchFillWords = Settings.Default.SearchFillWords;
    }

    private void OnWindowClosing(object sender, CancelEventArgs e)
    {
        Settings.Default.GlossaryPath = configuration.glossaryPath;
        Settings.Default.PreambleDirectoryPath = configuration.preambleDirectoryPath;
        Settings.Default.RootDirectoryPath = configuration.latexDirectoryPath;
        Settings.Default.IgnoreSectionLabels = configuration.ignoreSectionLabels;
        Settings.Default.SettingsPaths = configuration.ignoreFilesWithMissingGls;
        Settings.Default.IgnoreSettingsFile = configuration.ignoreSettingsFile;
        Settings.Default.ShowIgnoredErrors = configuration.showIgnoredErrors;
        Settings.Default.BibPath = configuration.bibPath;
        Settings.Default.LabelsToIgnore = configuration.labelsToIgnore;
        Settings.Default.FillWords = configuration.fillWords;
        Settings.Default.SearchFillWords = configuration.searchFillWords;
        Settings.Default.SerialzedGlsErrors = persistentIgnoredErrors.ToStringCollection();
        Settings.Default.Save();
    }

    #endregion

    #region UiEvents

    #region ButtonClicked

    private void ButtonStart_Click(object sender, RoutedEventArgs e)
    {
        allErrors.Clear();
        transientIgnoredErrors.Clear();
        StartAnalysis();
    }

    private void ButtonReset_Click(object sender, RoutedEventArgs e)
    {
        allErrors.Clear();
        persistentIgnoredErrors.Clear();
        transientIgnoredErrors.Clear();
        Settings.Default.Reset();
    }

    private void PickerRefOption_Clicked(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        configuration.ignoreSectionLabels = checkBox?.IsChecked ?? false;
    }

    private void PickerShowFillwords_Clicked(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        configuration.searchFillWords = checkBox?.IsChecked ?? false;
    }

    private void ButtonJump_Clicked(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.DataContext is not GlsError data) return;
        JumpToError(data.File!, data.Line, data.LinePosition);
    }

    private void ButtonIgnore_Clicked(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.DataContext is not GlsError data) return;

        if (data.ErrorStatus == ErrorStatus.NotIgnored)
        {
            allErrors.Remove(data);
            data.ErrorStatus = ErrorStatus.Ignored;
            persistentIgnoredErrors.Add(data);
            transientIgnoredErrors.Add(data);
            if (configuration.showIgnoredErrors) allErrors.Add(data);
        }
        else
        {
            allErrors.Remove(data);
            transientIgnoredErrors.Remove(data);

            var error = persistentIgnoredErrors.First(x => x.IsEqual(data));
            persistentIgnoredErrors.Remove(error);

            data.ErrorStatus = ErrorStatus.NotIgnored;
            allErrors.Add(data);
        }
    }

    private void SelectFillwordsAndLabels_Clicked(object sender, RoutedEventArgs e)
    {
        PopupDialog.Visibility = Visibility.Visible;
    }

    private void PickerShowIgnoredErrors_Clicked(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        configuration.showIgnoredErrors = checkBox?.IsChecked ?? false;
        this.ToggleIgnoredErrors();
    }

    private void ToggleIgnoredErrors()
    {
        if (configuration.showIgnoredErrors) allErrors.AddRange(transientIgnoredErrors);
        else allErrors.RemoveRange(transientIgnoredErrors);
    }

    private void JumpToError(string path, int line, int pos)
    {
        if (line == -1) line = 0;
        if (pos == -1) pos = 0;

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "code",
                Arguments = $"--goto \"{path}\":{line}:{pos}",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        process.Start();

        MarkWord();
    }

    private void PopupDialog_OnWindowIsClosing(string labelsToIgnore, string fillWords)
    {
        configuration.labelsToIgnore = labelsToIgnore.ToStringCollection();
        configuration.fillWords = fillWords.ToStringCollection();
    }

    private void ShowMessageBox()
    {
        const string messageBoxText = "Definiere zuerst alle Pfade!";
        const string caption = "Fehler!";
        const MessageBoxButton button = MessageBoxButton.OK;
        const MessageBoxImage icon = MessageBoxImage.Warning;
        MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
    }

    private void ResetLatexDirectory(object sender, RoutedEventArgs e)
    {
        LatexDirectoryBox.Text = string.Empty;
        configuration.latexDirectoryPath = string.Empty;
    }

    private void ResetGlossaryPath(object sender, RoutedEventArgs e)
    {
        GlossaryPathBox.Text = string.Empty;
        configuration.glossaryPath = string.Empty;
    }

    private void ResetBibPath(object sender, RoutedEventArgs e)
    {
        BibliographiePathBox.Text = string.Empty;
        configuration.bibPath = string.Empty;
    }

    private void ResetIgnoredFiles(object sender, RoutedEventArgs e)
    {
        IgnorableFilesMissingGlsBox.Text = string.Empty;
        configuration.ignoreFilesWithMissingGls?.Clear();
    }

    private void ResetPreambleDirectory(object sender, RoutedEventArgs e)
    {
        PreambleDirectoryBox.Text = string.Empty;
        configuration.preambleDirectoryPath = string.Empty;
    }

    #endregion

    #region Sorting

    private void ColumnClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader header) return;

        var sortOrder = header != lastHeaderClicked ? ListSortDirection.Ascending :
            lastSortOrder == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

        var columnBinding = header.Column.DisplayMemberBinding as Binding;
        var sortBy = columnBinding?.Path.Path ?? header.Column.Header as string;

        if (string.IsNullOrEmpty(sortBy)) return;
        if (sortBy == "Umgebung") return;

        SortErrors(sortBy, sortOrder);

        lastSortOrder = sortOrder;
        lastHeaderClicked = header;
    }

    private void SortErrors(string sortBy, ListSortDirection sortOrder)
    {
        var dataView = CollectionViewSource.GetDefaultView(lvGlsError.ItemsSource);

        dataView.SortDescriptions.Clear();
        var sortDescription = new SortDescription(sortBy, sortOrder);
        dataView.SortDescriptions.Add(sortDescription);
        if (sortBy == "File")
        {
            var thenBy = new SortDescription("Line", sortOrder);
            var thenthenBy = new SortDescription("LinePosition", sortOrder);
            dataView.SortDescriptions.Add(thenBy);
            dataView.SortDescriptions.Add(thenthenBy);
        }

        dataView.Refresh();
    }

    #endregion

    #region SelectPaths

    private void SelectRootDirectory(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog();
        dialog.InitialDirectory = string.IsNullOrEmpty(configuration.latexDirectoryPath)
            ? "C:\\"
            : configuration.latexDirectoryPath;
        dialog.IsFolderPicker = true;

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

        LatexDirectoryBox.Text = dialog.FileName;
        configuration.latexDirectoryPath = dialog.FileName;
    }

    private void SelectGlossaryPath(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog();
        dialog.InitialDirectory = !string.IsNullOrEmpty(configuration.glossaryPath) ? configuration.glossaryPath :
            string.IsNullOrEmpty(configuration.latexDirectoryPath) ? "C:\\" : configuration.latexDirectoryPath;
        dialog.IsFolderPicker = false;

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

        GlossaryPathBox.Text = dialog.FileName;
        configuration.glossaryPath = dialog.FileName;
    }

    private void SelectBibliographiePath(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog();
        dialog.InitialDirectory = !string.IsNullOrEmpty(configuration.bibPath) ? configuration.bibPath :
            string.IsNullOrEmpty(configuration.latexDirectoryPath) ? "C:\\" : configuration.latexDirectoryPath;
        dialog.IsFolderPicker = false;

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

        BibliographiePathBox.Text = dialog.FileName;
        configuration.bibPath = dialog.FileName;
    }

    private void SelectPreambleDirectory(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog();
        dialog.InitialDirectory = !string.IsNullOrEmpty(configuration.preambleDirectoryPath)
            ? configuration.preambleDirectoryPath
            : string.IsNullOrEmpty(configuration.latexDirectoryPath)
                ? "C:\\"
                : configuration.latexDirectoryPath;
        dialog.IsFolderPicker = true;

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

        PreambleDirectoryBox.Text = dialog.FileName;
        configuration.preambleDirectoryPath = dialog.FileName;
    }

    private void SelectIgnorableFilesGls(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog();
        dialog.InitialDirectory = string.IsNullOrEmpty(configuration.latexDirectoryPath)
            ? "C:\\"
            : configuration.latexDirectoryPath;
        dialog.IsFolderPicker = false;
        dialog.Multiselect = true;

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

        configuration.ignoreFilesWithMissingGls ??= dialog.FileNames as StringCollection ?? new StringCollection();

        foreach (var dialogFileName in dialog.FileNames)
            if (configuration.ignoreFilesWithMissingGls.Contains(dialogFileName))
                configuration.ignoreFilesWithMissingGls.Remove(dialogFileName);
            else configuration.ignoreFilesWithMissingGls.Add(dialogFileName);

        var text = configuration.ignoreFilesWithMissingGls.ToList();
        IgnorableFilesMissingGlsBox.Text = text.Any() ? text.Aggregate((x, y) => x + "\n" + y) : "";
    }

    #endregion

    #endregion

    #region Logic

    private void MarkWord()
    {
        List<Process> vscList;
        do
        {
            vscList = Process.GetProcessesByName("Code").ToList();
        } while (!vscList.Any());

        Thread.Sleep(1500);

        InputSimulator.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
        InputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
        InputSimulator.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
        InputSimulator.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
        InputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
        InputSimulator.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
    }

    private void StartAnalysis()
    {
        if (configuration.IsPathMissing())
        {
            ShowMessageBox();
            return;
        }

        var allExtractions = new AllExtractions(configuration, fileExtractor);
        allExtractions.ExtractAll();

        var glsErrors = GetAllErrors(allExtractions);

        ActualisateErrors(glsErrors);
    }

    private IEnumerable<GlsError> GetAllErrors(AllExtractions allExtractions)
    {
        var result = new List<GlsError>();
        result.AddRange(fileParser.FindWrongGlossaryErrorsPreamble(allExtractions.beforeFiles,
            allExtractions.allAcronymEntries));
        result.AddRange(fileParser.FindMissingGls(allExtractions.missingGlsFiles, allExtractions.allAcronymEntries,
            allExtractions.allGlossaryEntries));
        result.AddRange(fileParser.FindWrongGlossary(allExtractions.allFiles, allExtractions.allAcronymEntries));
        result.AddRange(fileParser.FindMissingReferences(configuration.ignoreSectionLabels,
            configuration.labelsToIgnore.ToList(),
            allExtractions.allLabelEntries, allExtractions.allRefs));
        result.AddRange(fileParser.FindWrongLabelNaming(allExtractions.allLabelEntries));
        result.AddRange(fileParser.FindWrongRefUsage(allExtractions.allRefs));
        result.AddRange(fileParser.FindMissingCitations(allExtractions.allFiles, allExtractions.allCitationEntries,
            configuration.labelsToIgnore.ToList()));
        result.AddRange(fileParser.FindFillWords(allExtractions.allFiles, configuration.fillWords.ToList(),
            configuration.searchFillWords));
        result.AddRange(fileParser.FindNotExistendLabels(
            allExtractions.allCitationEntries,
            allExtractions.allCitations,
            allExtractions.allRefs, allExtractions.allLabelEntries));
        result.AddRange(fileParser.FindMissingCaptionOrLabel(allExtractions.allAreas));
        result.AddRange(fileParser.FindToLongSenetences(allExtractions.allSenetences));
        result.AddRange(fileParser.FindMissingIncludes(allExtractions.allFiles, allExtractions.allIncludes));

        return result;
    }

    private void ActualisateErrors(IEnumerable<GlsError> newErrors)
    {
        var isIgnored = allErrors.AddRangeReturnErrors(newErrors, persistentIgnoredErrors);

        transientIgnoredErrors = isIgnored;
        foreach (var error in isIgnored.Where(error => !persistentIgnoredErrors.Any(x => x.IsEqual(error))))
            persistentIgnoredErrors.Add(error);

        ToggleIgnoredErrors();
    }

    #endregion
}