// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileExtractor.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LaTeX_Validator.Enums;
using static LaTeX_Validator.DataClasses.DataTemplates;

namespace LaTeX_Validator;

internal class FileExtractor
{
    public IEnumerable<AcronymEntry> GetAcronymEntries(string path)
    {
        var allLines = this.GetAllLinesFromFile(path);
        const string regexAcronymPattern = "newacronym{(.*)}{(.*)}{(.*)}"; // Group 0=all, 1=Label, 2=short, 3=long
        var regex = new Regex(regexAcronymPattern, RegexOptions.Compiled);

        foreach (var line in allLines)
        {
            var matched = regex.Matches(line.Content);

            foreach (Match match in matched)
            {
                var groups = match.Groups;
                if (groups.Count < 4) continue;

                yield return new AcronymEntry
                {
                    Label = groups[1].ToString(),
                    Short = groups[2].ToString(),
                    Long = groups[3].ToString()
                };
            }
        }
    }

    public IEnumerable<string> GetGlossaryEntries(string path)
    {
        var allLines = this.GetAllLinesFromFile(path);
        const string regexGlossaryPattern = "newglossaryentry{.*}{name={(.*)},.*}}"; // Group 1 = name
        var regex = new Regex(regexGlossaryPattern, RegexOptions.Compiled);

        foreach (var line in allLines)
        {
            var matched = regex.Matches(line.Content);

            foreach (Match match in matched)
            {
                var groups = match.Groups;
                if (groups.Count < 2) continue;

                yield return groups[1].ToString();
            }
        }
    }

    public IEnumerable<FullLine> GetAllLinesFromFile(string path)
    {
        var fileReader = new StreamReader(path);
        var counter = 1;
        while (fileReader.ReadLine() is { } actualLine)
        {
            yield return new FullLine { Content = actualLine, Number = counter };
            counter++;
        }

    }

    public IEnumerable<CitationEntry> GetCitationEntries(string path)
    {
        var allLines = this.GetAllLinesFromFile(path);
        const string regexBibPattern = "@(.*?){(.*?),";
        var regex = new Regex(regexBibPattern, RegexOptions.Compiled);

        foreach (var line in allLines)
        {
            var matched = regex.Matches(line.Content);

            foreach (Match match in matched)
            {
                var groups = match.Groups;
                if (groups.Count < 3) continue;

                yield return new CitationEntry
                {
                    label = groups[2].ToString(),
                    file = path,
                    line = line.Number,
                    pos = match.Groups[2].Index,
                    type = groups[1].ToString()
                };
            }
        }
    }

    public IEnumerable<CitationUsage> GetAllCitations(List<string> files)
    {
        const string regexPattern = @"\\cite{(.*?)}";
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.GetAllLinesFromFile(file).ToList();

            foreach (var line in allLines)
            {
                var matches = regex.Matches(line.Content);

                foreach (Match match in matches)
                {
                    var groups = match.Groups;
                    if (groups.Count < 2) continue;

                    yield return new CitationUsage
                    {
                        label = groups[1].Value,
                        line = line,
                        file = file,
                        pos = match.Index
                    };
                }
            }
        }
    }

    public IEnumerable<LabelDefinition> GetAllLabels(List<string> files)
    {
        const string regexPatternLabel = "label({|=)(.*?)(}|])";
        var regexLabel = new Regex(regexPatternLabel, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.GetAllLinesFromFile(file);
            foreach (var line in allLines)
            {
                var labelMatches = regexLabel.Matches(line.Content);

                foreach (Match labelMatch in labelMatches)
                {
                    var labelGroups = labelMatch.Groups;

                    if (labelGroups is { Count: > 2 })
                    {
                        yield return new LabelDefinition
                        {
                            label = labelGroups[2].Value,
                            file = file,
                            line = line,
                            pos = labelMatch.Index
                        };
                    }
                }
            }
        }
    }

    public IEnumerable<ReferenceUsage> GetAllRefs(List<string> files)
    {
        const string regexPatternRef = @"\\(.*?)ref{(.*?)}";
        var regexRef = new Regex(regexPatternRef, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.GetAllLinesFromFile(file);

            foreach (var line in allLines)
            {
                var refMatches = regexRef.Matches(line.Content);
                foreach (Match refMatch in refMatches)
                {
                    var refGroups = refMatch.Groups;

                    if (refGroups.Count < 3) continue;

                    var refType = string.IsNullOrEmpty(refGroups[1].Value) ? RefType.Normal : RefType.Auto;

                    yield return new ReferenceUsage
                    {
                        label = refGroups[2].Value,
                        RefType = refType,
                        file = file,
                        line = line,
                        pos = refMatch.Index
                    };
                }
            }
        }
    }

    public IEnumerable<Area> GetAllCriticalAreas(List<string> files)
    {
        const string regexBeginPattern = @"\\begin{(lstlisting|longtable|figure|tabular|table|tabularx)}(.*?)";
        var regexBegin = new Regex(regexBeginPattern, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.GetAllLinesFromFile(file).ToList();

            for (var i = 0; i < allLines.Count; i++)
            {
                var actualLine = allLines.ElementAt(i);

                if (!regexBegin.IsMatch(actualLine.Content)) continue;


                var allLinesFromArea = new List<FullLine> { actualLine };

                var match = regexBegin.Match(allLines.ElementAt(i).Content);
                var label = match.Groups[1].Value;

                i++;

                var regexEndPattern = @"\\end{" + label + "}";
                var regexEnd = new Regex(regexEndPattern, RegexOptions.Compiled);

                while (!regexEnd.IsMatch(allLines.ElementAt(i).Content))
                {
                    allLinesFromArea.Add(allLines.ElementAt(i));
                    i++;
                }

                allLinesFromArea.Add(allLines.ElementAt(i));

                yield return new Area
                {
                    allLines = allLinesFromArea,
                    label = label,
                    pos = 0,
                    file = file
                };
            }
        }
    }

    public IEnumerable<Sentence> GetAllSentences(List<string> files)
    {
        var toIgnore = new List<string>
        {
            @"\label",
            @"\section",
            @"\chapter",
            @"\subsection",
            @"\centering",
            @"\includegraphics",
            @"\begin",
            @"\end",
            @"\small",
            @"\renewcommand",
            @"\setlength",
            @"\vspace",
            @"\cellcolor",
            @"\hline",
            @"\pagebreak",
            @"\newpage",
            @"\FloatBarrier"
        };

        foreach (var file in files)
        {
            var allLines = this.GetAllLinesFromFile(file);

            foreach (var line in allLines)
            {
                var allSentences = line.Content.Split(".").ToList();

                foreach (var sentence in allSentences)
                {
                    if(string.IsNullOrWhiteSpace(sentence)) continue;
                    if(string.IsNullOrEmpty(sentence)) continue;
                    if(toIgnore.Any(x => sentence.Contains(x))) continue;
                    var commentIndex = sentence.IndexOf("%", StringComparison.Ordinal);
                    if (commentIndex is < 5 and >= 0) continue;
                    if(!sentence.Contains(" ")) continue;

                    yield return new Sentence
                                 {
                                     file = file,
                                     line = line,
                                     sentence = sentence
                                 };
                }
            }
        }
    }

    public IEnumerable<string> GetAllIncludes(List<string> files)
    {
        const string regexPattern = @"\\include{(.*?)}";
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        foreach (var file in files)
        {
            var allLines = this.GetAllLinesFromFile(file);

            foreach (var line in allLines)
            {
                var matches = regex.Matches(line.Content);
                foreach (Match match in matches)
                {
                    var matchStartIndex = match.Index;
                    var commentIndex = line.Content.IndexOf("%", StringComparison.Ordinal);

                    if (commentIndex != -1 && commentIndex < matchStartIndex) continue;

                    var val = match.Groups[1].Value;

                    if (val.Contains("/")) val = val.Replace("/", @"\");

                    yield return val;
                }
            }
        }
    }
}
