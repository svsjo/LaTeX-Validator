// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlsError.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using LaTeX_Validator.Enums;
using LaTeX_Validator.Extensions;

namespace LaTeX_Validator.DataClasses;

public class GlsError
{
    #region Attrbutes

    public string? WordContent { get; set; }
    public GlsType ActualForm { get; set; }
    public ErrorType ErrorType { get; set; }
    public string? File { get; set; }
    public int Line { get; set; }
    public int LinePosition { get; set; }
    public ErrorStatus ErrorStatus { get; set; }
    public string? DirectSuroundings { get; set; }
    public string? FullLine { get; set; }

    #endregion


    #region Transformers

    public string? FormText => this.ActualForm.GetStringValue();
    public string? ErrorText => this.ErrorType.GetStringValue();
    public string? ErrorStatusText => this.ErrorStatus.GetStringValue();
    public string? SuroundingsBefore
    {
        get
        {
            var split = this.DirectSuroundings?.Split(this.WordContent).ToList();
            return split?.ElementAt(0) ?? this.WordContent;
        }
    }
    public string? SuroundingsAfter
    {
        get
        {
            var split = this.DirectSuroundings?.Split(this.WordContent).ToList();
            return split is { Count: > 1 } ? split.ElementAt(1) : "";
        }
    }

    #endregion


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