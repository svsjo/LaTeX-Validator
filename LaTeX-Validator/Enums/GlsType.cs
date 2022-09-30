// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlsType.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using LaTeX_Validator.Extensions;

namespace LaTeX_Validator.Enums;

public enum GlsType
{
    [EnumExtension("Akronym")]
    AcrShort,
    [EnumExtension("Langbezeichner")]
    AcrLong,
    [EnumExtension("Glossareintrag")]
    Gls,
    [EnumExtension("Label")]
    Label,
    [EnumExtension("Quell-Label")]
    CitationLabel,
    [EnumExtension("Füllwort")]
    Fillword,
    [EnumExtension("Bereichsdefinition")]
    Area,
    [EnumExtension("Satz")]
    Sentences,
    [EnumExtension("Datei")]
    File
}
