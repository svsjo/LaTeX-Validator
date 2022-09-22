// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlsError.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using LaTeX_Validator.Extensions;

namespace LaTeX_Validator;

public class GlsError
{
    public string WordContent { get; set; }
    public GlsType ActualForm { get; set; }
    public string? FormText => this.ActualForm.GetStringValue();
    public ErrorType ErrorType { get; set; }
    public string? ErrorText => this.ErrorType.GetStringValue();
    public string File { get; set; }
    public int Line { get; set; }
    public int LinePosition { get; set; } = 0;
}
