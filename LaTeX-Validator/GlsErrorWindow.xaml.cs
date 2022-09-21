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
        private readonly ObservableCollection<GlsError> AllErrors;
        private readonly ConfigurationGlossary Configuration;
        private readonly FileExtractor FileExtractor;

        public GlsErrorWindow()
        {
            this.Configuration = new ConfigurationGlossary();
            this.FileExtractor = new FileExtractor();
            this.AllErrors = new ObservableCollection<GlsError>();
            this.InitializeComponent();
            this.lvGlsError.ItemsSource = this.AllErrors;
            this.LatexDirectoryBox.Text = Settings.Default.RootDirectoryPath;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            this.AllErrors.Clear();
            var latexDict = this.LatexDirectoryBox.Text;
            if (latexDict != this.Configuration.latexDirectoryAbsolute) this.Configuration.latexDirectoryAbsolute = latexDict;
            this.StartAnalysis();
        }
        private void ButtonJump_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is not GlsError data) return;
            this.JumpToError(data.File, data.Line);
        }

        private void JumpToError(string path, int line)
        {
            //  Aufbau: "code --goto {path}:{line}"
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
                this.Configuration.latexDirectoryAbsolute, "*.tex", SearchOption.AllDirectories).ToList();
            var beforeFiles = Directory.GetFiles(
                Path.Combine(this.Configuration.latexDirectoryAbsolute, this.Configuration.beforeDirectoryRelative), "*.tex",
                SearchOption.AllDirectories).ToList();

            var path = Path.Combine(this.Configuration.latexDirectoryAbsolute, this.Configuration.beforeDirectoryRelative, this.Configuration.glossaryName);
            var allAcronymEntries = this.FileExtractor.GetAcronymEntries(path).ToList();
            var allGlossaryEntries = this.FileExtractor.GetGlossaryEntries(path);

            var glossaryPath = Path.Combine(this.Configuration.latexDirectoryAbsolute, this.Configuration.beforeDirectoryRelative, this.Configuration.glossaryName);
            allFiles.Remove(glossaryPath);
            beforeFiles.Remove(glossaryPath);

            this.FindAcrLongErrors(beforeFiles, allAcronymEntries);
            this.FindMissingGlsErrors(allFiles, allAcronymEntries, allGlossaryEntries.ToList());
            this.FindTablesErrors(allFiles, allAcronymEntries);
            this.FindMissingReferencesErrors(allFiles);
        }


        /// <summary>
        /// In allen Texten vor der Einleitung sollte nur \acrlong verwendet werden.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="allAcronymEntries"></param>
        private void FindAcrLongErrors(List<string> files, IEnumerable<AcronymEntry> allAcronymEntries)
        {
            foreach (var file in files)
            {
                var allLines = this.FileExtractor.GetAllLinesFromFile(file);
                var affectedLines = allLines
                    .Where(line => allAcronymEntries
                               .Any(entry => line.Content.Contains($"{{{entry.Label}}}")))
                    .Select(line => new
                                    {
                                        number = line.Number,
                                        label = allAcronymEntries.First(entry => line.Content.Contains(entry.Label)).Label,
                                        content = line.Content
                                    });


                foreach (var line in affectedLines)
                {
                    var regexPattern = @"\\(.*){" + line.label + "}";
                    var regex = new Regex(regexPattern, RegexOptions.Compiled);
                    var matched = regex.Matches(line.content);

                    foreach (var match in matched)
                    {
                        var actualMatch = match as Match;
                        var groups = actualMatch?.Groups;
                        if (groups == null || groups.Count < 2) continue;

                        var type = groups[1].ToString();
                        if(type != "gls") continue;

                        this.AllErrors.Add(new GlsError
                                           {
                                          WordContent = line.label,
                                          ActualForm = GlsType.gls,
                                          ErrorType = ErrorType.ShouldBeAcrLong,
                                          File = file,
                                          Line = line.number
                                      });
                    }
                }
            }
        }

        /// <summary>
        /// Alle Kurz- und Langbezeichner aus dem Glossar sollten verwendet werden (\gls oder eben \acrlong \acrshort)
        /// </summary>
        private void FindMissingGlsErrors(List<string> files, IReadOnlyCollection<AcronymEntry> allAcronymEntries, IReadOnlyCollection<string> allGlossaryEntries)
        {
            foreach (var file in files)
            {
                var allLines = this.FileExtractor.GetAllLinesFromFile(file);
                var allEntriesConcatinated = allGlossaryEntries
                    .Concat(allAcronymEntries
                            .Select(x => x.Long)
                            .Concat(allAcronymEntries
                                        .Select(x => x.Short)));

                var missingWords = allEntriesConcatinated
                    .Where(entry => allLines
                               .Any(line => line.Content.Contains(entry)))
                    .Select(entry => new
                                     {
                                         word = entry,
                                         lines = allLines
                                             .Where(line => line.Content.Contains(entry))
                                             .Select(line => line.Number)
                                             .ToList()
                                     });

                foreach (var element in missingWords)
                {
                    foreach (var line in element.lines)
                    {
                        this.AllErrors.Add(new GlsError
                                           {
                                               WordContent = element.word,
                                               ActualForm = GlsType.none,
                                               ErrorType = ErrorType.MissingGls,
                                               File = file,
                                               Line = line
                                           });
                    }
                }
            }
        }

        /// <summary>
        /// In Abbildungen, Tabellen und Quellcode sollte nie \gls sondern nur \acrlong oder \acrshort verwendet werden
        /// </summary>
        private void FindTablesErrors(List<string> files, IEnumerable<AcronymEntry> allAcronymEntries)
        {
            const string regexPattern = @".*caption({|=)(.*)(}|,)";
            var regex = new Regex(regexPattern, RegexOptions.Compiled);

            foreach (var file in files)
            {
                var allLines = this.FileExtractor.GetAllLinesFromFile(file).ToList();
                var linesWithCaption = this.GetAllLinesWithCaption(allLines, regex);
                var affectedLines = linesWithCaption
                    .Where(line => line.Content.Contains(@"\gls") && allAcronymEntries
                                                                     .Select(entry => entry.Label)
                                                                     .Any(entry => line.Content.Contains(entry)))
                    .Select(line => new
                                    {
                                        number = line.Number,
                                        content = allAcronymEntries
                                                  .Select(entry => entry.Label)
                                                  .First(entry => line.Content.Contains(entry)),
                                        type = line.Content.Contains("acrlong") ? GlsType.acrlong : GlsType.acrshort
                                    });

                foreach (var line in affectedLines)
                {
                    this.AllErrors.Add(new GlsError
                                       {
                                           WordContent = line.content,
                                           ActualForm = line.type,
                                           ErrorType = ErrorType.ShouldBeAcrLong,
                                           File = file,
                                           Line = line.number
                                       });
                }
            }
        }

        private IEnumerable<Line> GetAllLinesWithCaption(List<Line> allLines, Regex regex)
        {
            var linesWithCaption = new List<Line>();

            foreach (var line in allLines)
            {
                var match = regex.Match(line.Content);
                var groups = match.Groups;
                if (groups.Count < 3) continue;

                linesWithCaption.Add(new Line { Content = groups[2].Value, Number = line.Number });
            }

            return linesWithCaption;
        }

        /// <summary>
        /// Wird auf alle Label von Tabellen, Quellcode und Bildern verwiesen?
        /// </summary>
        private void FindMissingReferencesErrors(List<string> files)
        {
            const string regexPatternLabel = @".*label({|=)(.*?)(}|])";
            const string regexPatternRef = @"autoref{(.*?)}";

            var regexLabel = new Regex(regexPatternLabel, RegexOptions.Compiled);
            var regexRef = new Regex(regexPatternRef, RegexOptions.Compiled);

            var allLabels = new List<(string label, string file, int line)>();
            var allRefs = new List<string>();

            foreach (var file in files)
            {
                var allLines = this.FileExtractor.GetAllLinesFromFile(file);
                foreach (var line in allLines)
                {
                    var labelMatches = regexLabel.Match(line.Content);
                    var labelGroups = labelMatches.Groups;

                    if (labelGroups is { Count: > 2 })
                    {
                        allLabels.Add((labelGroups[2].Value, file, line.Number));
                    }
                    else
                    {
                        var refMatches = regexRef.Match(line.Content);
                        var refGroups = refMatches.Groups;

                        if (refGroups.Count < 2) continue;
                        allRefs.Add(refGroups[1].Value);
                    }
                }
            }

            var haveReference = allLabels
                                .Where(entry => allRefs
                                           .Any(referencedLabel => referencedLabel == entry.label))
                                .ToList();

            var notReferenced = allLabels.Except(haveReference);

            foreach (var element in notReferenced)
            {
                this.AllErrors.Add(new GlsError
                                   {
                                       WordContent = element.label,
                                       ActualForm = GlsType.label,
                                       ErrorType = ErrorType.MissingAutoref,
                                       File = element.file,
                                       Line = element.line
                                   });
            }
        }

        public void FindMissingAutoRef()
        {
            // TODO
        }

        private void SelectRootDirectory(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                LatexDirectoryBox.Text = dialog.FileName;
                Settings.Default.RootDirectoryPath = dialog.FileName;
                Settings.Default.Save();
            }
        }
    }
}
