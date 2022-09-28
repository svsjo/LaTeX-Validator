// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlsError.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using LaTeX_Validator.Extensions;

namespace LaTeX_Validator;

public class GlsError
{
    public string? WordContent { get; set; }
    public GlsType ActualForm { get; set; }
    public string? FormText => this.ActualForm.GetStringValue();
    public ErrorType ErrorType { get; set; }
    public string? ErrorText => this.ErrorType.GetStringValue();
    public string? File { get; set; }
    public int Line { get; set; }
    public int LinePosition { get; set; } = 0;
    public ErrorStatus ErrorStatus { get; set; }
    public string? ErrorStatusText => this.ErrorStatus.GetStringValue();
    public string? DirectSuroundings { get; set; }
    public string? SuroundingsBefore
    {
        get
        {
            var split = this.DirectSuroundings?.Split(this.WordContent).ToList();
            return split?.ElementAt(0) ?? this.WordContent;
        }
    }
    public string? FullLine { get; set; }

    public string? SuroundingsAfter
    {
        get
        {
            var split = this.DirectSuroundings?.Split(this.WordContent).ToList();
            return split is { Count: > 1 } ? split.ElementAt(1) : this.WordContent;
        }
    }


    public bool IsEqual(GlsError glsError)
    {
        if (this.WordContent != glsError.WordContent) return false;
        if (this.ActualForm != glsError.ActualForm) return false;
        if (this.ErrorType != glsError.ErrorType) return false;
        if (this.File != glsError.File) return false;

        if (glsError.ErrorType is ErrorType.MissingRef or ErrorType.LabelNaming or ErrorType.MissingCitation) return true;

        if (this.FullLine != glsError.FullLine) return false;
        if (this.DirectSuroundings != glsError.DirectSuroundings) return false;
        if (this.Line != glsError.Line) return false;
        if (this.LinePosition != glsError.LinePosition) return false;

        return true;
    }
}