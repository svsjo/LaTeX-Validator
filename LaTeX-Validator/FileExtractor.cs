// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileExtractor.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;

namespace LaTeX_Validator;

public class FileExtractor
{
    public IEnumerable<AcronymEntry> GetAcronymEntries(string path)
    {
        var allLines = this.GetAllLinesFromFile(path);
        const string regexAcronymPattern = @"newacronym{(.*)}{(.*)}{(.*)}"; // Group 0=all, 1=Label, 2=short, 3=long
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
        const string regexGlossaryPattern = @"newglossaryentry{.*}{name={(.*)},.*}}"; // Group 1 = name
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

    public IEnumerable<Line> GetAllLinesFromFile(string path)
    {
        var fileReader = new StreamReader(path);
        var counter = 1;
        while (fileReader.ReadLine() is { } actualLine)
        {
            yield return new Line { Content = actualLine, Number = counter };
            counter++;
        }

    }

    public IEnumerable<CitationEntry> GetCitationEntries(string path)
    {
        var allLines = this.GetAllLinesFromFile(path);
        const string regexBibPattern = @"@(.*?){(.*?),";
        var regex = new Regex(regexBibPattern, RegexOptions.Compiled);

        foreach (var line in allLines)
        {
            var matched = regex.Matches(line.Content);

            foreach (Match match in matched)
            {
                var groups = match.Groups;
                if (groups.Count < 3) continue;

                yield return new CitationEntry()
                             {
                                 label = groups[2].ToString(),
                                 file = path,
                                 line = line.Number,
                                 pos = match.Index,
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

                    yield return new CitationUsage()
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
        const string regexPatternLabel = @"label({|=)(.*?)(}|])";
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

    public class ReferenceUsage
    {
        public string label { get; set; }
        public string file { get; set; }
        public Line line { get; set; }
        public int pos { get; set; }
        public RefType RefType { get; set; }
    }

    public class LabelDefinition
    {
        public string label { get; set; }
        public string file { get; set; }
        public Line line { get; set; }
        public int pos { get; set; }
    }

    public class CitationUsage
    {
        public string label { get; set; }
        public Line line { get; set; }
        public string file { get; set; }
        public int pos { get; set; }
    }
}
