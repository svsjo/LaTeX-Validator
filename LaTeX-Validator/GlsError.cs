﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlsError.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

namespace LaTeX_Validator;

public class GlsError
{
    public string WordContent { get; set; }
    public GlsType ActualForm { get; set; }
    public ErrorType ErrorType { get; set; }
    public string File { get; set; }
    public int Line { get; set; }
}
