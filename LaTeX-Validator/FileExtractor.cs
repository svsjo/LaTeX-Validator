// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileExtractor.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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

            foreach (var match in matched)
            {
                var actualMatch = match as Match;
                var groups = actualMatch?.Groups;
                if (groups == null || groups.Count < 4) continue;

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

            foreach (var match in matched)
            {
                var actualMatch = match as Match;
                var groups = actualMatch?.Groups;
                if (groups == null || groups.Count < 2) continue;

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
}
