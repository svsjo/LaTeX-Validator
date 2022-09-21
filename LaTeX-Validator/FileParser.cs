using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Shapes;

namespace LaTeX_Validator;

public class FileParser
{
    private readonly FileExtractor fileExtractor;

    public FileParser(FileExtractor fileExtractor1)
    {
        this.fileExtractor = fileExtractor1;
    }

    /// <summary>
    /// In allen Texten vor der Einleitung sollte nur \acrlong verwendet werden.
    /// </summary>
    /// <param name="files"></param>
    /// <param name="allAcronymEntries"></param>
    public IEnumerable<GlsError> FindAcrLongErrors(List<string> files, IEnumerable<AcronymEntry> allAcronymEntries)
    {
        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file);
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
                    if (type != "gls") continue;

                    yield return (new GlsError
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
    public IEnumerable<GlsError> FindMissingGlsErrors(List<string> files, IReadOnlyCollection<AcronymEntry> allAcronymEntries, IReadOnlyCollection<string> allGlossaryEntries)
    {
        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file);
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
                var regexPatternLabel = "label({|=)(.*?)" + element.word + "(.*?)(}|])";
                var regex = new Regex(regexPatternLabel, RegexOptions.Compiled);

                foreach (var line in element.lines.Where(line => !regex.IsMatch(line.Content)))
                {
                    yield return (new GlsError
                                  {
                                      WordContent = element.word,
                                      ActualForm = GlsType.none,
                                      ErrorType = ErrorType.MissingGls,
                                      File = file,
                                      Line = line.Number
                                  });
                }
            }
        }
    }

    /// <summary>
    /// In Abbildungen, Tabellen und Quellcode sollte nie \gls sondern nur \acrlong oder \acrshort verwendet werden
    /// </summary>
    public IEnumerable<GlsError> FindTablesErrors(List<string> files, IEnumerable<AcronymEntry> allAcronymEntries)
    {
        const string regexPattern = @".*caption({|=)(.*)(}|,)";
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file).ToList();
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
                yield return (new GlsError
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


    /// <summary>
    /// Wird auf alle Label von Tabellen, Quellcode und Bildern verwiesen?
    /// </summary>
    public IEnumerable<GlsError> FindMissingReferencesErrors(List<string> files, bool ignoreSections)
    {
        var allLabels = this.GetAllLabels(files).ToList();
        var allRefs = this.GetAllRefs(files);

        var haveReference = allLabels
                            .Where(entry => allRefs
                                       .Any(referencedLabel => referencedLabel == entry.label))
                            .ToList();

        var notReferenced = allLabels.Except(haveReference);

        foreach (var element in notReferenced)
        {
            if (ignoreSections)
            {
                if(element.label.Contains("sec:") || element.label.Contains("chap:") || element.label.Contains("subsec:")) continue;
            }

            yield return (new GlsError
                              {
                                  WordContent = element.label,
                                  ActualForm = GlsType.label,
                                  ErrorType = ErrorType.MissingAutoref,
                                  File = element.file,
                                  Line = element.line
                              });
        }
    }


    #region HelperMethods

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

    private IEnumerable<(string label, string file, int line)> GetAllLabels(List<string> files)
    {
        const string regexPatternLabel = @"label({|=)(.*?)(}|])";
        var regexLabel = new Regex(regexPatternLabel, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file);
            foreach (var line in allLines)
            {
                var labelMatches = regexLabel.Match(line.Content);
                var labelGroups = labelMatches.Groups;

                if (labelGroups is { Count: > 2 })
                {
                    yield return (labelGroups[2].Value, file, line.Number);
                }
            }
        }
    }

    private IEnumerable<string> GetAllRefs(List<string> files)
    {
        const string regexPatternRef = @"autoref{(.*?)}";
        var regexRef = new Regex(regexPatternRef, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file);

            foreach (var line in allLines)
            {
                var refMatches = regexRef.Match(line.Content);
                var refGroups = refMatches.Groups;

                if (refGroups.Count < 2) continue;

                yield return refGroups[1].Value;
            }
        }
    }

    #endregion
}