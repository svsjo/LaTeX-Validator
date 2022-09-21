// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcronymEntry.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

namespace LaTeX_Validator;

public class AcronymEntry
{
    public string Label { get; set; }
    public string Short { get; set; }
    public string Long { get; set; }
}
