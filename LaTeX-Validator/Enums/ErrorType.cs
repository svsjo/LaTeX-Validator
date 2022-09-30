// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorType.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using LaTeX_Validator.Extensions;

namespace LaTeX_Validator.Enums;

public enum ErrorType
{
    [EnumExtension("GLS-Befehl nicht verwendet")]
    MissingGls,
    [EnumExtension("Sollte acrlong/short sein")]
    ShouldBeAcrLong,
    [EnumExtension("Referenzierung fehlt")]
    MissingRef,
    [EnumExtension("Label falsch benannt")]
    LabelNaming,
    [EnumExtension("Kein Autoref verwendet")]
    WrongRefType,
    [EnumExtension("Quelle nie verwendet")]
    MissingCitation,
    [EnumExtension("Füllwort verwendet")]
    IsFillWord,
    [EnumExtension("Genutztes Label existiert nicht")]
    WrongLabel,
    [EnumExtension("Label nicht definiert")]
    NoLabel,
    [EnumExtension("Caption nicht definiert")]
    NoCaption,
    [EnumExtension("Satz zu lange")]
    LongSentence,
    [EnumExtension("Nie inkludiert")]
    NoInclude
}
