using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using LaTeX_Validator.Extensions;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using WindowsInput;
using WindowsInput.Native;

namespace LaTeX_Validator
{
    /// <summary>
    /// Interaction logic for GlsErrorWindow.xaml
    /// </summary>
    partial class GlsErrorWindow : Window
    {
        private readonly ObservableCollection<GlsError> allErrors;
        private List<GlsError> persistentIgnoredErrors;
        private List<GlsError> transientIgnoredErrors;
        private readonly ConfigurationGlossary configuration;
        private readonly FileExtractor fileExtractor;
        private readonly FileParser fileParser;
        private GridViewColumnHeader? lastHeaderClicked = null;
        private ListSortDirection lastSortOrder = ListSortDirection.Ascending;
        private readonly InputSimulator InputSimulator = new InputSimulator();


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
            if(!text.Any()) return;
            this.IgnorableFilesMissingGlsBox.Text = text.Aggregate((x, y) => x + "\n" + y);
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
            this.persistentIgnoredErrors = Settings.Default.SerialzedGlsErrors.Deserialize();
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
            Settings.Default.SerialzedGlsErrors = this.persistentIgnoredErrors.Serialize();
            Settings.Default.Save();
        }

        #endregion

        #region UiEvents

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
                if(this.configuration.showIgnoredErrors) this.allErrors.Add(data);
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

        private void PopupDialog_OnWindowIsClosing(string labelsToIgnore, string fillWords)
        {
            this.configuration.labelsToIgnore = labelsToIgnore.ToStringCollection();
            this.configuration.fillWords = fillWords.ToStringCollection();
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
            if (this.configuration.IsMissingSomething())
            {
                this.ShowMessageBox();
                return;
            }

            var allFiles = Directory.GetFiles(
                this.configuration.latexDirectoryPath!, "*.tex", SearchOption.AllDirectories).ToList();
            var beforeFiles = Directory.GetFiles(
                this.configuration.preambleDirectoryPath!, "*.tex", SearchOption.AllDirectories).ToList();
            var tempFiles = Directory.GetFiles(
                this.configuration.latexDirectoryPath!, "*.tex", SearchOption.TopDirectoryOnly).ToList();
            var contentFiles = allFiles.Except(tempFiles).ToList();

            var allAcronymEntries = this.fileExtractor.GetAcronymEntries(this.configuration.glossaryPath!).ToList();
            var allGlossaryEntries = this.fileExtractor.GetGlossaryEntries(this.configuration.glossaryPath!).ToList();
            var allCitationEntries = this.fileExtractor.GetCitationEntries(this.configuration.bibPath!).ToList();
            var allCitations = this.fileExtractor.GetAllCitations(allFiles).ToList();
            var allLabels = this.fileExtractor.GetAllLabels(allFiles).ToList();
            var allRefs = this.fileExtractor.GetAllRefs(allFiles).ToList();
            var allAreas = this.fileExtractor.GetAllCriticalAreas(allFiles).ToList();
            var allSenetences = this.fileExtractor.GetAllSentences(contentFiles).ToList();

            allFiles.Remove(this.configuration.glossaryPath!);
            beforeFiles.Remove(this.configuration.glossaryPath!);

            var missingGlsFiles = this.configuration.ignoreFilesWithMissingGls == null ? allFiles :
                                      allFiles
                                                 .Except(this.configuration.ignoreFilesWithMissingGls.ToList())
                                                 .ToList();

            this.ActualisateErrors(this.fileParser.FindWrongGlossaryErrorsPreamble(beforeFiles, allAcronymEntries),
                                   this.fileParser.FindMissingGls(missingGlsFiles, allAcronymEntries, allGlossaryEntries),
                                   this.fileParser.FindWrongGlossary(allFiles, allAcronymEntries),
                                   this.fileParser.FindMissingReferences(allFiles,
                                                                               this.configuration.ignoreSectionLabels,
                                                                               this.configuration.labelsToIgnore.ToList(),
                                                                               allLabels,
                                                                               allRefs),
                                   this.fileParser.FindWrongLabelNaming(allFiles, allLabels),
                                   this.fileParser.FindWrongRefUsage(allFiles, allRefs),
                                   this.fileParser.FindMissingCitations(allFiles, allCitationEntries,
                                           this.configuration.labelsToIgnore.ToList()),
                                   this.fileParser.FindFillWords(allFiles, this.configuration.fillWords.ToList(),
                                                                 this.configuration.searchFillWords),
                                   this.fileParser.FindNotExistendLabels(allFiles, allCitationEntries, allCitations, allRefs, allLabels),
                    this.fileParser.FindMissingCaptionOrLabel(allAreas),
                    this.fileParser.FindToLongSenetences(allSenetences));
        }

        private void ActualisateErrors(
            IEnumerable<GlsError> acrLongErrors, IEnumerable<GlsError> missingGlsErrors, IEnumerable<GlsError> tableErrors,
            IEnumerable<GlsError> missingReferenceErrors, IEnumerable<GlsError> labelNameErrors, IEnumerable<GlsError> refTypeErrors,
            IEnumerable<GlsError> missingCitationErrors, IEnumerable<GlsError> fillWordErrors, IEnumerable<GlsError> notExistantLabels,
            IEnumerable<GlsError> missingCaptionOrLabels, IEnumerable<GlsError> longSentencesErrors)
        {
            var isIgnored = this.allErrors
                                .AddRangeIfPossibleAndReturnErrors(acrLongErrors, this.persistentIgnoredErrors);
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(missingGlsErrors, this.persistentIgnoredErrors));
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(tableErrors, this.persistentIgnoredErrors));
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(missingReferenceErrors, this.persistentIgnoredErrors));
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(labelNameErrors, this.persistentIgnoredErrors));
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(refTypeErrors, this.persistentIgnoredErrors));
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(missingCitationErrors, this.persistentIgnoredErrors));
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(fillWordErrors, this.persistentIgnoredErrors));
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(notExistantLabels, this.persistentIgnoredErrors));
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(missingCaptionOrLabels, this.persistentIgnoredErrors));
            isIgnored.AddRange(this.allErrors
                                   .AddRangeIfPossibleAndReturnErrors(longSentencesErrors, this.persistentIgnoredErrors));


            this.transientIgnoredErrors = isIgnored;
            this.persistentIgnoredErrors.AddRange(isIgnored);
            this.persistentIgnoredErrors = this.transientIgnoredErrors.Distinct().ToList();
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
    }
}
