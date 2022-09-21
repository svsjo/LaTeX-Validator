using System.Collections.Generic;
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
            this.LatexDirectoryBox.Text = Settings.Default.RootDirectoryPath;
            this.RefOptionPicker.IsChecked = this.configuration.ignoreSectionLabels;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            this.allErrors.Clear();
            var latexDict = this.LatexDirectoryBox.Text;
            if (latexDict != this.configuration.latexDirectoryAbsolute) this.configuration.latexDirectoryAbsolute = latexDict;
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
            // Ignorieren (persistent)
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
            var allFiles = Directory.GetFiles(
                this.configuration.latexDirectoryAbsolute, "*.tex", SearchOption.AllDirectories).ToList();
            var beforeFiles = Directory.GetFiles(
                Path.Combine(this.configuration.latexDirectoryAbsolute, this.configuration.beforeDirectoryRelative), "*.tex",
                SearchOption.AllDirectories).ToList();

            var path = Path.Combine(this.configuration.latexDirectoryAbsolute, this.configuration.beforeDirectoryRelative, this.configuration.glossaryName);
            var allAcronymEntries = this.fileExtractor.GetAcronymEntries(path).ToList();
            var allGlossaryEntries = this.fileExtractor.GetGlossaryEntries(path);

            var glossaryPath = Path.Combine(this.configuration.latexDirectoryAbsolute, this.configuration.beforeDirectoryRelative, this.configuration.glossaryName);
            allFiles.Remove(glossaryPath);
            beforeFiles.Remove(glossaryPath);

            this.allErrors.AddRange(this.fileParser.FindAcrLongErrors(beforeFiles, allAcronymEntries));
            this.allErrors.AddRange(this.fileParser.FindMissingGlsErrors(allFiles, allAcronymEntries, allGlossaryEntries.ToList()));
            this.allErrors.AddRange(this.fileParser.FindTablesErrors(allFiles, allAcronymEntries));
            this.allErrors.AddRange(this.fileParser.FindMissingReferencesErrors(allFiles, this.configuration.ignoreSectionLabels));
        }


        private void SelectRootDirectory(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                LatexDirectoryBox.Text = dialog.FileName;
            }
        }
    }
}
