﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            this.lvGlsError.ItemsSource = this.allErrors;
            this.RefOptionPicker.IsChecked = this.configuration.ignoreSectionLabels;
            this.InitializeFromPersistentData();
        }

        private void InitializeFromPersistentData()
        {
            this.configuration.latexDirectoryPath = Settings.Default.RootDirectoryPath;
            this.configuration.preambleDirectoryPath = Settings.Default.PreambleDirectoryPath;
            this.configuration.glossaryPath = Settings.Default.GlossaryPath;

            this.LatexDirectoryBox.Text = Settings.Default.RootDirectoryPath;
            this.PreambleDirectoryBox.Text = Settings.Default.PreambleDirectoryPath;
            this.GlossaryPathBox.Text = Settings.Default.GlossaryPath;
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
                string.IsNullOrEmpty(this.configuration.preambleDirectoryPath)) return; // TODO Fehlermeldung

            var allFiles = Directory.GetFiles(
                this.configuration.latexDirectoryPath, "*.tex", SearchOption.AllDirectories).ToList();
            var beforeFiles = Directory.GetFiles(
                this.configuration.preambleDirectoryPath, "*.tex", SearchOption.AllDirectories).ToList();

            var allAcronymEntries = this.fileExtractor.GetAcronymEntries(this.configuration.glossaryPath).ToList();
            var allGlossaryEntries = this.fileExtractor.GetGlossaryEntries(this.configuration.glossaryPath);

            allFiles.Remove(this.configuration.glossaryPath);
            beforeFiles.Remove(this.configuration.glossaryPath);

            this.allErrors.AddRange(this.fileParser.FindAcrLongErrors(beforeFiles, allAcronymEntries));
            this.allErrors.AddRange(this.fileParser.FindMissingGlsErrors(allFiles, allAcronymEntries, allGlossaryEntries.ToList()));
            this.allErrors.AddRange(this.fileParser.FindTablesErrors(allFiles, allAcronymEntries));
            this.allErrors.AddRange(this.fileParser.FindMissingReferencesErrors(allFiles, this.configuration.ignoreSectionLabels));
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
    }
}
