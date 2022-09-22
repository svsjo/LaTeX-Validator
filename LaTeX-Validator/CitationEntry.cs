// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CitationEntry.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

namespace LaTeX_Validator;

public class CitationEntry
{
    public string? label { get; set; }
    public string? file { get; set; }
    public int line { get; set; }
    public int pos { get; set; }
}
