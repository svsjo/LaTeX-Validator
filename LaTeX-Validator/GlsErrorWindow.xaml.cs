using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using LaTeX_Validator.Extensions;
using Microsoft.WindowsAPICodePack.Dialogs;
using Path = System.IO.Path;

namespace LaTeX_Validator
{
    /// <summary>
    /// Interaction logic for GlsErrorWindow.xaml
    /// </summary>
    partial class GlsErrorWindow : Window
    {
        private readonly ObservableCollection<GlsError> allErrors;
        private readonly ConfigurationGlossary configuration;
        private readonly FileExtractor fileExtractor;
        private readonly FileParser fileParser;

        public GlsErrorWindow()
        {
            this.configuration = new ConfigurationGlossary();
            this.fileExtractor = new FileExtractor();
            this.fileParser = new FileParser(this.fileExtractor);
            this.allErrors = new ObservableCollection<GlsError>();
            this.InitializeComponent();
            this.InitializeUiData();
        }

        private void InitializeUiData()
        {
            this.InitializeFromPersistentData();

            this.lvGlsError.ItemsSource = this.allErrors;
            this.LatexDirectoryBox.Text = this.configuration.latexDirectoryPath;
            this.PreambleDirectoryBox.Text = this.configuration.preambleDirectoryPath;
            this.GlossaryPathBox.Text = this.configuration.glossaryPath;
            this.RefOptionPicker.IsChecked = this.configuration.ignoreSectionLabels;

            var text = this.configuration.ignoreFilesWithMissingGls?.Cast<string>().ToList();
            this.IgnorableFilesMissingGlsBox.Text = text?.Aggregate((x, y) => x + ", " + y);
        }

        private void InitializeFromPersistentData()
        {
            this.configuration.latexDirectoryPath = Settings.Default.RootDirectoryPath;
            this.configuration.preambleDirectoryPath = Settings.Default.PreambleDirectoryPath;
            this.configuration.glossaryPath = Settings.Default.GlossaryPath;
            this.configuration.ignoreSectionLabels = Settings.Default.IgnoreSectionLabels;
            this.configuration.ignoreFilesWithMissingGls = Settings.Default.SettingsPaths;
            this.configuration.ignoreSettingsFile = Settings.Default.IgnoreSettingsFile;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            this.allErrors.Clear();
            this.StartAnalysis();
        }

        private void PickerRefOption_Clicked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            this.configuration.ignoreSectionLabels = checkBox?.IsChecked ?? false;
        }

        private void PickerIgnoreSettings_Clicked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            this.configuration.ignoreSettingsFile = checkBox?.IsChecked ?? false;
        }

        private void ButtonJump_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is not GlsError data) return;
            this.JumpToError(data.File, data.Line);
        }

        private void ButtonIgnore_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is not GlsError data) return;
            // TODO @jdev
            // Ignorieren Option (persistent)
        }

        private void ButtonFilesIgnore_Clicked(object sender, RoutedEventArgs e)
        {
            this.SelectIgnorableFilesMissingGls();
        }

        private void JumpToError(string path, int line)
        {
            var process = new Process
                          {
                              StartInfo = new ProcessStartInfo
                                          {
                                              FileName = "code",
                                              Arguments = $"--goto \"{path}\":{line}",
                                              UseShellExecute = true,
                                              CreateNoWindow = true,
                                              WindowStyle = ProcessWindowStyle.Hidden
                                          }
                          };
            process.Start();
        }

        private void StartAnalysis()
        {
            if (string.IsNullOrEmpty(this.configuration.latexDirectoryPath) ||
                string.IsNullOrEmpty(this.configuration.glossaryPath) ||
                string.IsNullOrEmpty(this.configuration.preambleDirectoryPath))
            {
                this.ShowMessageBox();
                return;
            }

            var allFiles = Directory.GetFiles(
                this.configuration.latexDirectoryPath, "*.tex", SearchOption.AllDirectories).ToList();
            var beforeFiles = Directory.GetFiles(
                this.configuration.preambleDirectoryPath, "*.tex", SearchOption.AllDirectories).ToList();
            var missingGlsFiles = this.configuration.ignoreFilesWithMissingGls == null || !this.configuration.ignoreSettingsFile ?
                                      allFiles :allFiles
                                          .Except(this.configuration.ignoreFilesWithMissingGls.Cast<string>())
                                          .ToList();

            var allAcronymEntries = this.fileExtractor.GetAcronymEntries(this.configuration.glossaryPath).ToList();
            var allGlossaryEntries = this.fileExtractor.GetGlossaryEntries(this.configuration.glossaryPath);

            allFiles.Remove(this.configuration.glossaryPath);
            beforeFiles.Remove(this.configuration.glossaryPath);

            this.allErrors.AddRange(this.fileParser.FindAcrLongErrors(beforeFiles, allAcronymEntries));
            this.allErrors.AddRange(this.fileParser.FindMissingGlsErrors(missingGlsFiles, allAcronymEntries, allGlossaryEntries.ToList()));
            this.allErrors.AddRange(this.fileParser.FindTablesErrors(allFiles, allAcronymEntries));
            this.allErrors.AddRange(this.fileParser.FindMissingReferencesErrors(allFiles, this.configuration.ignoreSectionLabels));
            this.allErrors.AddRange(this.fileParser.FindLabelNamingErrors(allFiles));
        }

        private void ShowMessageBox()
        {
            const string messageBoxText = "Definiere zuerst alle Pfade!";
            const string caption = "Fehler!";
            const MessageBoxButton button = MessageBoxButton.OK;
            const MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }


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
        private void SelectIgnorableFilesMissingGls()
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

            var text = this.configuration.ignoreFilesWithMissingGls.Cast<string>().ToList();
            this.IgnorableFilesMissingGlsBox.Text = text.Any() ? text.Aggregate((x, y) => x + ", " + y) : "";
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Settings.Default.GlossaryPath = this.configuration.glossaryPath;
            Settings.Default.PreambleDirectoryPath = this.configuration.preambleDirectoryPath;
            Settings.Default.RootDirectoryPath = this.configuration.latexDirectoryPath;
            Settings.Default.IgnoreSectionLabels= this.configuration.ignoreSectionLabels;
            Settings.Default.SettingsPaths = this.configuration.ignoreFilesWithMissingGls;
            Settings.Default.IgnoreSettingsFile = this.configuration.ignoreSettingsFile;
            Settings.Default.Save();
        }
    }
}
