using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LaTeX_Validator.DataClasses;
using LaTeX_Validator.Enums;
using static LaTeX_Validator.DataClasses.DataTemplates;

namespace LaTeX_Validator;

public class FileParser
{
    #region Init

    private readonly FileExtractor fileExtractor;

    public FileParser(FileExtractor fileExtractor1)
    {
        this.fileExtractor = fileExtractor1;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Suche alle vorher definierten Füllworte im Text
    /// </summary>
    /// <param name="files"></param>
    /// <param name="fillWords"></param>
    /// <param name="doSearch"></param>
    /// <returns></returns>
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
                                         DirectSuroundings = this.GetDirectSuroundings(line.Content, match.Value),
                                         FullLine = line.Content
                                     };
                    }
                }
            }
        }
    }

    /// <summary>
    /// Suche alle Quellen, die nie angegeben wurden
    /// </summary>
    /// <param name="files"></param>
    /// <param name="allCitationEntries"></param>
    /// <param name="labelsToIgnore"></param>
    /// <returns></returns>
    public IEnumerable<GlsError> FindMissingCitations(List<string> files, List<CitationEntry> allCitationEntries, List<string> labelsToIgnore)
    {
        var regexPattern = @"\\cite{(.*?)}";
        var regex = new Regex(regexPattern, RegexOptions.Compiled);
        var allCitationEntriesCopy = allCitationEntries.GetRange(0, allCitationEntries.Count);

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

                    var entry = allCitationEntriesCopy.FirstOrDefault(ent => ent.label == label);
                    if(entry != default) allCitationEntriesCopy.Remove(entry);
                }
            }
        }

        foreach (var entry in allCitationEntriesCopy)
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
                             DirectSuroundings = $"@{entry.type}{{{entry.label},",
                             FullLine = $"@{entry.type}{{{entry.label},"
            };
        }
    }

    /// <summary>
    /// In allen Texten vor der Einleitung sollte nur \acrlong oder \acrshort statt \gls verwendet werden.
    /// </summary>
    /// <param name="files"></param>
    /// <param name="allAcronymEntries"></param>
    public IEnumerable<GlsError> FindWrongGlossaryErrorsPreamble(List<string> files, IEnumerable<AcronymEntry> allAcronymEntries)
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
                                      DirectSuroundings = this.GetDirectSuroundings(line.content, line.label),
                                      FullLine = line.content
                    });
                }
            }
        }
    }

    /// <summary>
    /// Alle Kurz- und Langbezeichner aus dem Glossar sollten verwendet werden (\Gls oder eben \AcrLong \AcrShort)
    /// </summary>
    public IEnumerable<GlsError> FindMissingGls(List<string> files, IReadOnlyCollection<AcronymEntry> allAcronymEntries,
                                                      IReadOnlyCollection<string> allGlossaryEntries)
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
                                      DirectSuroundings = this.GetDirectSuroundings(line.Content, element.word),
                                      FullLine = line.Content
                    });
                }
            }
        }
    }

    /// <summary>
    /// In Abbildungen, Tabellen und Quellcode und Überschriften sollte nie \Gls sondern nur \AcrLong oder \AcrShort verwendet werden
    /// </summary>
    public IEnumerable<GlsError> FindWrongGlossary(List<string> files, IEnumerable<AcronymEntry> allAcronymEntries)
    {
        foreach (var file in files)
        {
            var allLines = this.fileExtractor.GetAllLinesFromFile(file).ToList();
            var linesWithCaptionOrHeadline = this.GetAllLinesWithCaption(allLines).Concat(this.GetAllLinesWithHeadlines(allLines)).ToList();
            var affectedLines = linesWithCaptionOrHeadline
                                .Where(line => line.contentCaption.Contains(@"\gls") && allAcronymEntries
                                                                                        .Select(entry => entry.Label)
                                                                                        .Any(entry => line.contentCaption.Contains(entry)))
                                .Select(line => new
                                                {
                                                    number = line.line.Number,
                                                    containedLabel = allAcronymEntries
                                                              .Select(entry => entry.Label)
                                                              .First(entry => line.contentCaption.Contains(entry)),
                                                    type = line.contentCaption.Contains("acrlong") ? GlsType.AcrLong : GlsType.AcrShort,
                                                    fullLine = line
                                                });

            foreach (var line in affectedLines)
            {
                yield return (new GlsError
                              {
                                  WordContent = line.containedLabel,
                                  ActualForm = line.type,
                                  ErrorType = ErrorType.ShouldBeAcrLong,
                                  File = file,
                                  Line = line.number,
                                  LinePosition = line.fullLine.line.Content.IndexOf(line.containedLabel, StringComparison.Ordinal),
                                  ErrorStatus = ErrorStatus.NotIgnored,
                                  DirectSuroundings = this.GetDirectSuroundings(line.fullLine.line.Content, line.containedLabel),
                                  FullLine = line.fullLine.line.Content
                });
            }
        }
    }

    /// <summary>
    /// Wird auf alle Label von Tabellen, Quellcode und Bildern verwiesen?
    /// </summary>
    public IEnumerable<GlsError> FindMissingReferences(List<string> files, bool ignoreSections, List<string> labelsToIgnore,
                                                             List<LabelDefinition> allLabels, List<ReferenceUsage> allRefs)
    {
        var haveReference = allLabels
                            .Where(entry => allRefs
                                       .Any(referencedLabel => referencedLabel.label == entry.label))
                            .ToList();

        var notReferenced = allLabels.Except(haveReference);

        foreach (var element in notReferenced)
        {
            if (labelsToIgnore.Contains(element.label ?? string.Empty)) continue;

            if (ignoreSections)
            {
                if(element.label!.Contains("sec:") || element.label.Contains("chap:") || element.label.Contains("subsec:")) continue;
            }

            yield return (new GlsError
                          {
                              WordContent = element.label,
                              ActualForm = GlsType.Label,
                              ErrorType = ErrorType.MissingRef,
                              File = element.file,
                              Line = element.line!.Number,
                              LinePosition = element.pos,
                              ErrorStatus = ErrorStatus.NotIgnored,
                              DirectSuroundings = this.GetDirectSuroundings(element.line.Content, element.label ?? string.Empty),
                              FullLine = element.line.Content
                          });
        }
    }

    /// <summary>
    /// Findet alle Referenzierungen in denen \ref statt \autoref verwendet wurde
    /// </summary>
    /// <param name="files"></param>
    /// <param name="allRefs"></param>
    /// <returns></returns>
    public IEnumerable<GlsError> FindWrongRefUsage(List<string> files, List<ReferenceUsage> allRefs)
    {
        var problems = allRefs.Where(reference => reference.RefType == RefType.Normal);

        foreach (var element in problems)
        {
            yield return (new GlsError()
                          {
                              WordContent = element.label,
                              ActualForm = GlsType.Label,
                              ErrorType = ErrorType.WrongRefType,
                              File = element.file,
                              Line = element.line!.Number,
                              LinePosition = element.pos,
                              ErrorStatus = ErrorStatus.NotIgnored,
                              DirectSuroundings = this.GetDirectSuroundings(element.line.Content, element.label ?? string.Empty),
                              FullLine = element.line.Content
            });
        }
    }

    /// <summary>
    /// Prüft ob alle Labels richtig benannt wurden
    /// </summary>
    /// <param name="files"></param>
    /// <param name="allLabels"></param>
    /// <returns></returns>
    public IEnumerable<GlsError> FindWrongLabelNaming(List<string> files, List<LabelDefinition> allLabels)
    {
        var possiblePres = new List<string>() { "chap:", "sec:", "subsec:", "fig:", "table:", "lst:", "label" };
        var problematicLabels = allLabels
            .Where(label => possiblePres
                       .All(pre => !label.label!.Contains(pre)))
            .ToList();

        foreach (var problematicLabel in problematicLabels)
        {
            yield return (new GlsError()
                          {
                              WordContent = problematicLabel.label,
                              ActualForm = GlsType.Label,
                              ErrorType = ErrorType.LabelNaming,
                              File = problematicLabel.file,
                              Line = problematicLabel.line!.Number,
                              LinePosition = problematicLabel.pos,
                              ErrorStatus = ErrorStatus.NotIgnored,
                              DirectSuroundings = this.GetDirectSuroundings(problematicLabel.line.Content, problematicLabel.label ?? string.Empty),
                              FullLine = problematicLabel.line.Content
            });
        }
    }

    /// <summary>
    /// Finde bei Referenzierung und Zitation ob Label verwendet wurden, welches es nicht gibt.
    /// Bei Glossar erkennt es TexStudio selbst und wirft einen Fehler.
    /// </summary>
    /// <param name="files"></param>
    /// <param name="allCitationLabels"></param>
    /// <param name="allCitations"></param>
    /// <param name="allRefUsages"></param>
    /// <param name="allRefLabels"></param>
    /// <returns></returns>
    public IEnumerable<GlsError> FindNotExistendLabels(List<string> files, List<CitationEntry> allCitationLabels, List<CitationUsage> allCitations,
                                                       List<ReferenceUsage> allRefUsages, List<LabelDefinition> allRefLabels)
    {
        var wrongRefs = allRefUsages
                        .Where(usage => allRefLabels
                                   .All(label => label.label != usage.label))
                        .ToList();
        var wrongCitations = allCitations
                             .Where(usage => allCitationLabels
                                        .All(label => label.label != usage.label))
                             .ToList();

        foreach (var citation in wrongCitations)
        {
            yield return new GlsError()
                         {
                             WordContent = citation.label,
                             ActualForm = GlsType.CitationLabel,
                             ErrorType = ErrorType.WrongLabel,
                             File = citation.file,
                             Line = citation.line!.Number,
                             LinePosition = citation.pos,
                             ErrorStatus = ErrorStatus.NotIgnored,
                             DirectSuroundings = this.GetDirectSuroundings(citation.line.Content, citation.label ?? string.Empty),
                             FullLine = citation.line.Content
                         };
        }

        foreach (var reference in wrongRefs)
        {
            yield return new GlsError()
                         {
                             WordContent = reference.label,
                             ActualForm = GlsType.Label,
                             ErrorType = ErrorType.WrongLabel,
                             File = reference.file,
                             Line = reference.line!.Number,
                             LinePosition = reference.pos,
                             ErrorStatus = ErrorStatus.NotIgnored,
                             DirectSuroundings = this.GetDirectSuroundings(reference.line.Content, reference.label ?? string.Empty),
                             FullLine = reference.line.Content
                         };
        }
    }

    /// <summary>
    /// ÜBerprüfe ob eine Tabelle, Code oder Bild ohne Label oder Caption genutzt wird.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GlsError> FindMissingCaptionOrLabel(List<Area> allCriticalAreas)
    {
        foreach (var area in allCriticalAreas)
        {
            var lineWithCaption = area.allLines?.FirstOrDefault(line => line.Content.Contains("caption"));
            var lineWithLabel = area.allLines?.FirstOrDefault(line => line.Content.Contains("label"));

            if (lineWithCaption == default)
            {
                yield return new GlsError
                             {
                                 WordContent = area.label,
                                 ActualForm = GlsType.Area,
                                 ErrorType = ErrorType.NoCaption,
                                 File = area.file,
                                 Line = area.allLines?.FirstOrDefault()?.Number ?? 0,
                                 LinePosition = area.pos,
                                 ErrorStatus = ErrorStatus.NotIgnored,
                                 DirectSuroundings = area.allLines?.FirstOrDefault()?.Content ?? string.Empty,
                                 FullLine = string.Join("\n", area.allLines!.Select(x => x.Content))
                };
            }

            if (lineWithLabel == default)
            {
                yield return new GlsError
                             {
                                 WordContent = area.label,
                                 ActualForm = GlsType.Area,
                                 ErrorType = ErrorType.NoLabel,
                                 File = area.file,
                                 Line = area.allLines?.FirstOrDefault()?.Number ?? 0,
                                 LinePosition = area.pos,
                                 ErrorStatus = ErrorStatus.NotIgnored,
                                 DirectSuroundings = area.allLines?.FirstOrDefault()?.Content ?? string.Empty,
                                 FullLine = string.Join("\n", area.allLines!.Select(x => x.Content))
                };
            }
        }
    }

    /// <summary>
    /// Sätze mit >25 Wörtern als Warnung, >30 Wörtern als Error finden
    /// </summary>
    /// <param name="allSenetences"></param>
    /// <returns></returns>
    public IEnumerable<GlsError> FindToLongSenetences(List<Sentence> allSenetences)
    {
        foreach (var sentence in allSenetences)
        {
            var words = sentence.sentence?.Split(" ").ToList() ?? new List<string>();

            if (words.Count > 25)
            {
                yield return new GlsError
                             {
                                 WordContent = words.Count > 30 ? $"Fehler: Länge {words.Count}" : $"Warnung: Länge {words.Count}",
                                 ActualForm = GlsType.Sentences,
                                 ErrorType = ErrorType.LongSentence,
                                 File = sentence.file,
                                 Line = sentence.line?.Number ?? 0,
                                 LinePosition = sentence.line?.Content.IndexOf(sentence.sentence ?? string.Empty, StringComparison.Ordinal) ?? 0,
                                 ErrorStatus = ErrorStatus.NotIgnored,
                                 DirectSuroundings = sentence.sentence,
                                 FullLine = sentence.line?.Content
                };
            }
        }
    }

    #endregion

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

    private IEnumerable<(Line line, string contentCaption)> GetAllLinesWithCaption(List<Line> allLines)
    {
        const string regexPattern = @".*caption({|=)(.*)(}|,)";
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        foreach (var actualLine in allLines)
        {
            var matches = regex.Matches(actualLine.Content);
            foreach (Match match in matches)
            {
                var groups = match.Groups;
                if (groups.Count < 3) continue;

                yield return (actualLine, groups[2].Value);
            }
        }
    }

    private IEnumerable<(Line line, string contentCaption)> GetAllLinesWithHeadlines(List<Line> allLines)
    {
        const string regexPattern = @"\\(section|chapter|subsection){(.*?)}";
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        var linesWithCaption = new List<Line>();

        foreach (var actualLine in allLines)
        {
            var matches = regex.Matches(actualLine.Content);
            foreach (Match match in matches)
            {
                var groups = match.Groups;
                if (groups.Count < 3) continue;

                yield return (actualLine, groups[2].Value);
            }
        }
    }

    #endregion
}