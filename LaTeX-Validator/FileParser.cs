using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Shapes;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace LaTeX_Validator;

public class FileParser
{
    private readonly FileExtractor fileExtractor;

    public FileParser(FileExtractor fileExtractor1)
    {
        this.fileExtractor = fileExtractor1;
    }

    public IEnumerable<GlsError> FindFillWords(List<string> files, List<string> fillWords, bool doSearch)
    {
        if (!doSearch) yield break;

        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file).ToList();

            foreach (var line in allLines)
            {
                var foundFillWords = fillWords.Where(word => line.Content.Contains(word));

                foreach (var fillWord in foundFillWords)
                {
                    var regexPattern = $" {fillWord} ";
                    var regex = new Regex(regexPattern, RegexOptions.Compiled);
                    var matches = regex.Matches(line.Content);

                    foreach (Match match in matches)
                    {
                        yield return new GlsError()
                                     {
                                         WordContent = match.Value,
                                         ActualForm = GlsType.Fillword,
                                         ErrorType = ErrorType.IsFillWord,
                                         File = file,
                                         Line = line.Number,
                                         LinePosition = match.Index,
                                         ErrorStatus = ErrorStatus.NotIgnored,
                                         DirectSuroundings = this.GetDirectSuroundings(line.Content, match.Value)
                                     };
                    }
                }
            }
        }
    }

    public IEnumerable<GlsError> FindMissingCitations(List<string> files, List<CitationEntry> allCitationEntries, List<string> labelsToIgnore)
    {
        var regexPattern = @"\\cite{(.*?)}";
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file).ToList();

            foreach (var line in allLines)
            {
                var matches = regex.Matches(line.Content);

                foreach (Match match in matches)
                {
                    var groups = match.Groups;
                    if(groups.Count < 2) continue;

                    var label = groups[1].Value;
                    if(labelsToIgnore.Contains(label)) continue;

                    var entry = allCitationEntries.FirstOrDefault(ent => ent.label == label);
                    if(entry != default) allCitationEntries.Remove(entry);
                }
            }
        }

        foreach (var entry in allCitationEntries)
        {
            yield return new GlsError()
                         {
                             WordContent = entry.label,
                             ActualForm = GlsType.CitationLabel,
                             ErrorType = ErrorType.MissingCitation,
                             File = entry.file,
                             Line = entry.line,
                             LinePosition = entry.pos,
                             ErrorStatus = ErrorStatus.NotIgnored,
                             DirectSuroundings = $"@{entry.type}{{{entry.label},"
            };
        }
    }

    /// <summary>
    /// In allen Texten vor der Einleitung sollte nur \AcrLong verwendet werden.
    /// </summary>
    /// <param name="files"></param>
    /// <param name="allAcronymEntries"></param>
    public IEnumerable<GlsError> FindAcrLongErrors(List<string> files, IEnumerable<AcronymEntry> allAcronymEntries)
    {
        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file).ToList();
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

                foreach (Match match in matched)
                {
                    var groups = match.Groups;
                    if (groups.Count < 2) continue;

                    var type = groups[1].ToString();
                    if (type != "gls") continue;

                    yield return (new GlsError
                                  {
                                      WordContent = line.label,
                                      ActualForm = GlsType.Gls,
                                      ErrorType = ErrorType.ShouldBeAcrLong,
                                      File = file,
                                      Line = line.number,
                                      LinePosition = match.Index,
                                      ErrorStatus = ErrorStatus.NotIgnored,
                                      DirectSuroundings = this.GetDirectSuroundings(line.content, line.label)
                    });
                }
            }
        }
    }

    /// <summary>
    /// Alle Kurz- und Langbezeichner aus dem Glossar sollten verwendet werden (\Gls oder eben \AcrLong \AcrShort)
    /// </summary>
    public IEnumerable<GlsError> FindMissingGlsErrors(List<string> files, IReadOnlyCollection<AcronymEntry> allAcronymEntries, IReadOnlyCollection<string> allGlossaryEntries)
    {
        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file).ToList();
            var allEntriesConcatinated = allGlossaryEntries
                .Concat(allAcronymEntries
                        .Select(x => x.Long)
                        .Concat(allAcronymEntries
                                    .Select(x => x.Short)));



            var possiblyMissingWords = allEntriesConcatinated
                               .Where(entry => allLines
                                          .Any(line => line.Content.Contains(entry)))
                               .Select(entry => new
                                                {
                                                    word = entry,
                                                    lines = allLines
                                                            .Where(line => line.Content.Contains(entry))
                                                            .ToList()
                                                });


            foreach (var element in possiblyMissingWords)
            {
                var regexPatternToIgnore = @"\\.*({|=)(.*?)" + element.word + "(.*?)(}|])";
                var regexToIgnore = new Regex(regexPatternToIgnore, RegexOptions.Compiled);

                foreach (var line in element.lines.Where(line => !regexToIgnore.IsMatch(line.Content)))
                {
                    var type = allAcronymEntries.Select(x => x.Short).Any(x => x == element.word) ?
                                   GlsType.AcrShort : allAcronymEntries.Select(x => x.Long).Any(x => x == element.word) ?
                                       GlsType.AcrLong : GlsType.Gls;

                    yield return (new GlsError
                                  {
                                      WordContent = element.word,
                                      ActualForm = type,
                                      ErrorType = ErrorType.MissingGls,
                                      File = file,
                                      Line = line.Number,
                                      LinePosition = line.Content.IndexOf(element.word, StringComparison.Ordinal),
                                      ErrorStatus = ErrorStatus.NotIgnored,
                                      DirectSuroundings = this.GetDirectSuroundings(line.Content, element.word)
                    });
                }
            }
        }
    }

    /// <summary>
    /// In Abbildungen, Tabellen und Quellcode sollte nie \Gls sondern nur \AcrLong oder \AcrShort verwendet werden
    /// </summary>
    public IEnumerable<GlsError> FindTablesErrors(List<string> files, IEnumerable<AcronymEntry> allAcronymEntries)
    {
        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file).ToList();
            var linesWithCaption = this.GetAllLinesWithCaption(allLines).ToList();
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
                                                    type = line.Content.Contains("AcrLong") ? GlsType.AcrLong : GlsType.AcrShort,
                                                    fullLine = line
                                                });

            foreach (var line in affectedLines)
            {
                yield return (new GlsError
                              {
                                  WordContent = line.content,
                                  ActualForm = line.type,
                                  ErrorType = ErrorType.ShouldBeAcrLong,
                                  File = file,
                                  Line = line.number,
                                  LinePosition = line.fullLine.Content.IndexOf(line.content, StringComparison.Ordinal),
                                  ErrorStatus = ErrorStatus.NotIgnored,
                                  DirectSuroundings = this.GetDirectSuroundings(line.content, line.content)
                });
            }
        }
    }


    /// <summary>
    /// Wird auf alle Label von Tabellen, Quellcode und Bildern verwiesen?
    /// </summary>
    public IEnumerable<GlsError> FindMissingReferencesErrors(List<string> files, bool ignoreSections, List<string> labelsToIgnore)
    {
        var allLabels = this.GetAllLabels(files).ToList();
        var allRefs = this.GetAllRefs(files).Select(x => x.label).ToList();

        var haveReference = allLabels
                            .Where(entry => allRefs
                                       .Any(referencedLabel => referencedLabel == entry.label))
                            .ToList();

        var notReferenced = allLabels.Except(haveReference);

        foreach (var element in notReferenced)
        {
            if (labelsToIgnore.Contains(element.label)) continue;

            if (ignoreSections)
            {
                if(element.label.Contains("sec:") || element.label.Contains("chap:") || element.label.Contains("subsec:")) continue;
            }

            yield return (new GlsError
                          {
                              WordContent = element.label,
                              ActualForm = GlsType.Label,
                              ErrorType = ErrorType.MissingRef,
                              File = element.file,
                              Line = element.line.Number,
                              LinePosition = element.pos,
                              ErrorStatus = ErrorStatus.NotIgnored,
                              DirectSuroundings = this.GetDirectSuroundings(element.line.Content, element.label)
                          });
        }
    }

    public IEnumerable<GlsError> FindWrongRefUsage(List<string> files)
    {
        var allRefs = this.GetAllRefs(files).ToList();
        var problems = allRefs.Where(reference => reference.refType == RefType.Normal);

        foreach (var element in problems)
        {
            yield return (new GlsError()
                          {
                              WordContent = element.label,
                              ActualForm = GlsType.Label,
                              ErrorType = ErrorType.WrongRefType,
                              File = element.file,
                              Line = element.line.Number,
                              LinePosition = element.pos,
                              ErrorStatus = ErrorStatus.NotIgnored,
                              DirectSuroundings = this.GetDirectSuroundings(element.line.Content, element.label)
            });
        }
    }

    /// <summary>
        /// Prüft ob alle Labels richtig benannt wurden
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
    public IEnumerable<GlsError> FindLabelNamingErrors(List<string> files)
    {
        var allLabels = this.GetAllLabels(files).ToList();
        var possiblePres = new List<string>() { "chap:", "sec:", "subsec:", "fig:", "table:", "lst:", "label" };
        var problematicLabels = allLabels
            .Where(label => possiblePres
                       .All(pre => !label.label.Contains(pre)))
            .ToList();

        foreach (var problematicLabel in problematicLabels)
        {
            yield return (new GlsError()
                          {
                              WordContent = problematicLabel.label,
                              ActualForm = GlsType.Label,
                              ErrorType = ErrorType.LabelNaming,
                              File = problematicLabel.file,
                              Line = problematicLabel.line.Number,
                              LinePosition = problematicLabel.pos,
                              ErrorStatus = ErrorStatus.NotIgnored,
                              DirectSuroundings = this.GetDirectSuroundings(problematicLabel.line.Content, problematicLabel.label)
            });
        }
    }


    #region HelperMethods

    private string GetDirectSuroundings(string line, string word)
    {
        var position = line.IndexOf(word, StringComparison.Ordinal);

        var range = (80 - word.Length) / 2;

        var min = position - range;
        if (min < 0) min = 0;

        var max = position + range + word.Length;
        if (max > line.Length) max = line.Length;

        var length = max - min;

        return line.Substring(min, length);
    }

    private IEnumerable<Line> GetAllLinesWithCaption(List<Line> allLines)
    {
        const string regexPattern = @".*caption({|=)(.*)(}|,)";
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        var linesWithCaption = new List<Line>();

        foreach (var line in allLines)
        {
            var matches = regex.Matches(line.Content);
            foreach (Match match in matches)
            {
                var groups = match.Groups;
                if (groups.Count < 3) continue;

                linesWithCaption.Add(new Line { Content = groups[2].Value, Number = line.Number });
            }
        }

        return linesWithCaption;
    }

    private IEnumerable<(string label, string file, Line line, int pos)> GetAllLabels(List<string> files)
    {
        const string regexPatternLabel = @"label({|=)(.*?)(}|])";
        var regexLabel = new Regex(regexPatternLabel, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file);
            foreach (var line in allLines)
            {
                var labelMatches = regexLabel.Matches(line.Content);

                foreach (Match labelMatch in labelMatches)
                {
                    var labelGroups = labelMatch.Groups;

                    if (labelGroups is { Count: > 2 })
                    {
                        yield return (labelGroups[2].Value, file, line, labelMatch.Index);
                    }
                }
            }
        }
    }

    private IEnumerable<(string label, RefType refType, string file, Line line, int pos)> GetAllRefs(List<string> files)
    {
        const string regexPatternRef = @"\\(.*?)ref{(.*?)}";
        var regexRef = new Regex(regexPatternRef, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file);

            foreach (var line in allLines)
            {
                var refMatches = regexRef.Matches(line.Content);
                foreach (Match refMatch in refMatches)
                {
                    var refGroups = refMatch.Groups;

                    if (refGroups.Count < 3) continue;

                    var refType = string.IsNullOrEmpty(refGroups[1].Value) ? RefType.Normal : RefType.Auto;

                    yield return (refGroups[2].Value, refType, file, line, refMatch.Index);
                }
            }
        }
    }

    #endregion
}