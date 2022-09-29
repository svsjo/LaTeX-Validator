// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataTemplates.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using LaTeX_Validator.Enums;

namespace LaTeX_Validator.DataClasses;

public class DataTemplates
{
    public class Sentence
    {
        internal string? sentence { get; init; }
        internal Line? line { get; init; }
        internal string? file { get; init; }
    }

    public class ReferenceUsage
    {
        internal string? label { get; init; }
        internal string? file { get; init; }
        internal Line? line { get; init; }
        internal int pos { get; init; }
        internal RefType RefType { get; init; }
    }

    public class LabelDefinition
    {
        internal string? label { get; init; }
        internal string? file { get; init; }
        internal Line? line { get; init; }
        internal int pos { get; init; }
    }

    public class CitationUsage
    {
        internal string? label { get; init; }
        internal Line? line { get; init; }
        internal string? file { get; init; }
        internal int pos { get; init; }
    }

    public class Area
    {
        internal List<Line>? allLines { get; init; }
        internal string? label { get; init; }
        internal string? file { get; init; }
        internal int pos { get; init; }
    }

    public class CitationEntry
    {
        internal string? label { get; init; }
        internal string? file { get; init; }
        internal int line { get; init; }
        internal int pos { get; init; }
        internal string? type { get; init; }
    }

    public class Line
    {
        internal string Content { get; init; } = string.Empty;
        internal int Number { get; init; }
    }

    public class AcronymEntry
    {
        internal string Label { get; init; } = null!;
        internal string Short { get; init; } = null!;
        internal string Long { get; init; } = null!;
    }
}
