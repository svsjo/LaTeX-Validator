using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

namespace LaTeX_Validator
{
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
        private GridViewColumnHeader? lastHeaderClicked = null;
        private ListSortDirection lastSortOrder = ListSortDirection.Ascending;
        private readonly InputSimulator InputSimulator = new();

        #endregion

        #region Initialization

        public GlsErrorWindow()
        {
            this.configuration = new ConfigurationGlossary();
            this.fileExtractor = new FileExtractor();
            this.fileParser = new FileParser(this.fileExtractor);
            this.allErrors = new ObservableCollection<GlsError>();
            this.persistentIgnoredErrors = new List<GlsError>();
            this.transientIgnoredErrors = new List<GlsError>();
            this.InitializeComponent();
            this.InitializeUiData();
            this.lvGlsError.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(this.ColumnClick));
        }

        private void InitializeUiData()
        {
            this.InitializeFromPersistentData();

            this.lvGlsError.ItemsSource = this.allErrors;
            this.LatexDirectoryBox.Text = this.configuration.latexDirectoryPath;
            this.PreambleDirectoryBox.Text = this.configuration.preambleDirectoryPath;
            this.GlossaryPathBox.Text = this.configuration.glossaryPath;
            this.RefOptionPicker.IsChecked = this.configuration.ignoreSectionLabels;
            this.BibliographiePathBox.Text = this.configuration.bibPath;
            this.PopupDialog.FillwordsBox.Text = this.configuration.fillWords?.AggregateToString();
            this.PopupDialog.LabelsBox.Text = this.configuration.labelsToIgnore?.AggregateToString();

            var text = this.configuration.ignoreFilesWithMissingGls.ToList();
            if(text.Any()) this.IgnorableFilesMissingGlsBox.Text = text.Aggregate((x, y) => x + "\n" + y);
        }

        private void InitializeFromPersistentData()
        {
            this.configuration.latexDirectoryPath = Settings.Default.RootDirectoryPath;
            this.configuration.preambleDirectoryPath = Settings.Default.PreambleDirectoryPath;
            this.configuration.glossaryPath = Settings.Default.GlossaryPath;
            this.configuration.ignoreSectionLabels = Settings.Default.IgnoreSectionLabels;
            this.configuration.ignoreFilesWithMissingGls = Settings.Default.SettingsPaths;
            this.configuration.ignoreSettingsFile = Settings.Default.IgnoreSettingsFile;
            this.configuration.showIgnoredErrors = Settings.Default.ShowIgnoredErrors;
            this.persistentIgnoredErrors = Settings.Default.SerialzedGlsErrors.ToGlsError();
            this.configuration.bibPath = Settings.Default.BibPath;
            this.configuration.labelsToIgnore = Settings.Default.LabelsToIgnore;
            this.configuration.fillWords = Settings.Default.FillWords;
            this.configuration.searchFillWords = Settings.Default.SearchFillWords;
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Settings.Default.GlossaryPath = this.configuration.glossaryPath;
            Settings.Default.PreambleDirectoryPath = this.configuration.preambleDirectoryPath;
            Settings.Default.RootDirectoryPath = this.configuration.latexDirectoryPath;
            Settings.Default.IgnoreSectionLabels = this.configuration.ignoreSectionLabels;
            Settings.Default.SettingsPaths = this.configuration.ignoreFilesWithMissingGls;
            Settings.Default.IgnoreSettingsFile = this.configuration.ignoreSettingsFile;
            Settings.Default.ShowIgnoredErrors = this.configuration.showIgnoredErrors;
            Settings.Default.BibPath = this.configuration.bibPath;
            Settings.Default.LabelsToIgnore = this.configuration.labelsToIgnore;
            Settings.Default.FillWords = this.configuration.fillWords;
            Settings.Default.SearchFillWords = this.configuration.searchFillWords;
            Settings.Default.SerialzedGlsErrors = this.persistentIgnoredErrors.ToStringCollection();
            Settings.Default.Save();
        }

        #endregion

        #region UiEvents

        #region ButtonClicked

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            this.allErrors.Clear();
            this.StartAnalysis();
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            this.allErrors.Clear();
            this.persistentIgnoredErrors.Clear();
            this.transientIgnoredErrors.Clear();
        }

        private void PickerRefOption_Clicked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            this.configuration.ignoreSectionLabels = checkBox?.IsChecked ?? false;
        }

        private void PickerShowFillwords_Clicked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            this.configuration.searchFillWords = checkBox?.IsChecked ?? false;
        }

        private void ButtonJump_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is not GlsError data) return;
            this.JumpToError(data.File!, data.Line, data.LinePosition);
        }

        private void ButtonIgnore_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is not GlsError data) return;

            if (data.ErrorStatus == ErrorStatus.NotIgnored)
            {
                this.allErrors.Remove(data);
                data.ErrorStatus = ErrorStatus.Ignored;
                this.persistentIgnoredErrors.Add(data);
                this.transientIgnoredErrors.Add(data);
                if (this.configuration.showIgnoredErrors) this.allErrors.Add(data);
            }
            else
            {
                this.allErrors.Remove(data);
                data.ErrorStatus = ErrorStatus.NotIgnored;
                this.transientIgnoredErrors.Remove(data);
                this.persistentIgnoredErrors.Remove(data);
                this.allErrors.Add(data);
            }
        }

        private void SelectFillwordsAndLabels_Clicked(object sender, RoutedEventArgs e)
        {
            this.PopupDialog.Visibility = Visibility.Visible;
        }

        private void PickerShowIgnoredErrors_Clicked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            this.configuration.showIgnoredErrors = checkBox?.IsChecked ?? false;
            if (this.configuration.showIgnoredErrors)
            {
                this.allErrors.AddRange(this.transientIgnoredErrors);
            }
            else
            {
                this.allErrors.RemoveRange(this.transientIgnoredErrors);
            }
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

            this.MarkWord();
        }

        private void PopupDialog_OnWindowIsClosing(string labelsToIgnore, string fillWords)
        {
            this.configuration.labelsToIgnore = labelsToIgnore.ToStringCollection();
            this.configuration.fillWords = fillWords.ToStringCollection();
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
            this.LatexDirectoryBox.Text = string.Empty;
            this.configuration.latexDirectoryPath = string.Empty;
        }

        private void ResetGlossaryPath(object sender, RoutedEventArgs e)
        {
            this.GlossaryPathBox.Text = string.Empty;
            this.configuration.glossaryPath = string.Empty;
        }

        private void ResetBibPath(object sender, RoutedEventArgs e)
        {
            this.BibliographiePathBox.Text = string.Empty;
            this.configuration.bibPath = string.Empty;
        }

        private void ResetIgnoredFiles(object sender, RoutedEventArgs e)
        {
            this.IgnorableFilesMissingGlsBox.Text = string.Empty;
            this.configuration.ignoreFilesWithMissingGls?.Clear();
        }

        private void ResetPreambleDirectory(object sender, RoutedEventArgs e)
        {
            this.PreambleDirectoryBox.Text = string.Empty;
            this.configuration.preambleDirectoryPath = string.Empty;
        }

        #endregion

        #region Sorting

        private void ColumnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not GridViewColumnHeader header) return;

            var sortOrder = header != this.lastHeaderClicked ? ListSortDirection.Ascending :
                            this.lastSortOrder == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            var columnBinding = header.Column.DisplayMemberBinding as Binding;
            var sortBy = columnBinding?.Path.Path ?? header.Column.Header as string;

            if (string.IsNullOrEmpty(sortBy)) return;
            if (sortBy == "Umgebung") return;

            this.SortErrors(sortBy, sortOrder);

            this.lastSortOrder = sortOrder;
            this.lastHeaderClicked = header;
        }

        private void SortErrors(string sortBy, ListSortDirection sortOrder)
        {
            var dataView = CollectionViewSource.GetDefaultView(this.lvGlsError.ItemsSource);

            dataView.SortDescriptions.Clear();
            var sd = new SortDescription(sortBy, sortOrder);
            dataView.SortDescriptions.Add(sd);
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
            dialog.InitialDirectory = string.IsNullOrEmpty(this.configuration.latexDirectoryPath) ?
                                          "C:\\" : this.configuration.latexDirectoryPath;
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            this.LatexDirectoryBox.Text = dialog.FileName;
            this.configuration.latexDirectoryPath = dialog.FileName;
        }

        private void SelectGlossaryPath(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = !string.IsNullOrEmpty(this.configuration.glossaryPath) ?
                                          this.configuration.glossaryPath :
                                          string.IsNullOrEmpty(this.configuration.latexDirectoryPath) ?
                                              "C:\\" : this.configuration.latexDirectoryPath;
            dialog.IsFolderPicker = false;

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            this.GlossaryPathBox.Text = dialog.FileName;
            this.configuration.glossaryPath = dialog.FileName;
        }

        private void SelectBibliographiePath(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = !string.IsNullOrEmpty(this.configuration.bibPath) ?
                                          this.configuration.bibPath :
                                          string.IsNullOrEmpty(this.configuration.latexDirectoryPath) ?
                                              "C:\\" : this.configuration.latexDirectoryPath;
            dialog.IsFolderPicker = false;

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            this.BibliographiePathBox.Text = dialog.FileName;
            this.configuration.bibPath = dialog.FileName;
        }

        private void SelectPreambleDirectory(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = !string.IsNullOrEmpty(this.configuration.preambleDirectoryPath) ?
                                          this.configuration.preambleDirectoryPath :
                                          string.IsNullOrEmpty(this.configuration.latexDirectoryPath) ?
                                              "C:\\" : this.configuration.latexDirectoryPath;
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            this.PreambleDirectoryBox.Text = dialog.FileName;
            this.configuration.preambleDirectoryPath = dialog.FileName;
        }

        private void SelectIgnorableFilesGls(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = string.IsNullOrEmpty(this.configuration.latexDirectoryPath) ?
                                          "C:\\" : this.configuration.latexDirectoryPath;
            dialog.IsFolderPicker = false;
            dialog.Multiselect = true;

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            this.configuration.ignoreFilesWithMissingGls ??= dialog.FileNames as StringCollection ?? new StringCollection();

            foreach (var dialogFileName in dialog.FileNames)
            {
                if (this.configuration.ignoreFilesWithMissingGls.Contains(dialogFileName))
                    this.configuration.ignoreFilesWithMissingGls.Remove(dialogFileName);
                else this.configuration.ignoreFilesWithMissingGls.Add(dialogFileName);
            }

            var text = this.configuration.ignoreFilesWithMissingGls.ToList();
            this.IgnorableFilesMissingGlsBox.Text = text.Any() ? text.Aggregate((x, y) => x + "\n" + y) : "";
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

            System.Threading.Thread.Sleep(1500);

            this.InputSimulator.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
            this.InputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            this.InputSimulator.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            this.InputSimulator.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
            this.InputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            this.InputSimulator.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
        }

        private void StartAnalysis()
        {
            if (this.configuration.IsPathMissing())
            {
                this.ShowMessageBox();
                return;
            }

            var allExtractions = new AllExtractions(this.configuration, this.fileExtractor);
            allExtractions.ExtractAll();

            var glsErrors = this.GetAllErrors(allExtractions);

            this.ActualisateErrors(glsErrors);
        }

        private IEnumerable<GlsError> GetAllErrors(AllExtractions allExtractions)
        {
            var result = new List<GlsError>();
            result.AddRange(this.fileParser.FindWrongGlossaryErrorsPreamble(allExtractions.beforeFiles, allExtractions.allAcronymEntries));
            result.AddRange(this.fileParser.FindMissingGls(allExtractions.missingGlsFiles, allExtractions.allAcronymEntries,
                                                           allExtractions.allGlossaryEntries));
            result.AddRange(this.fileParser.FindWrongGlossary(allExtractions.allFiles, allExtractions.allAcronymEntries));
            result.AddRange(this.fileParser.FindMissingReferences(allExtractions.allFiles, this.configuration.ignoreSectionLabels,
                                                                  this.configuration.labelsToIgnore.ToList(), allExtractions.allLabelEntries,
                                                                  allExtractions.allRefs));
            result.AddRange(this.fileParser.FindWrongLabelNaming(allExtractions.allFiles, allExtractions.allLabelEntries));
            result.AddRange(this.fileParser.FindWrongRefUsage(allExtractions.allFiles, allExtractions.allRefs));
            result.AddRange(this.fileParser.FindMissingCitations(allExtractions.allFiles, allExtractions.allCitationEntries,
                                                                 this.configuration.labelsToIgnore.ToList()));
            result.AddRange(this.fileParser.FindFillWords(allExtractions.allFiles, this.configuration.fillWords.ToList(),
                                                          this.configuration.searchFillWords));
            result.AddRange(this.fileParser.FindNotExistendLabels(allExtractions.allFiles, allExtractions.allCitationEntries,
                                                                  allExtractions.allCitations,
                                                                  allExtractions.allRefs, allExtractions.allLabelEntries));
            result.AddRange(this.fileParser.FindMissingCaptionOrLabel(allExtractions.allAreas));
            result.AddRange(this.fileParser.FindToLongSenetences(allExtractions.allSenetences));
            result.AddRange(this.fileParser.FindMissingIncludes(allExtractions.allFiles, allExtractions.allIncludes));

            return result;
        }

        private void ActualisateErrors(IEnumerable<GlsError> newErrors)
        {
            var isIgnored = this.allErrors.AddRangeReturnErrors(newErrors, this.persistentIgnoredErrors);

            this.transientIgnoredErrors = isIgnored;
            this.persistentIgnoredErrors.AddRange(isIgnored);
            this.persistentIgnoredErrors = this.persistentIgnoredErrors.Distinct().ToList();
        }

        #endregion
    }
}
