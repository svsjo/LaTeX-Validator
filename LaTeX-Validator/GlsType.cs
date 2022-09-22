﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlsType.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using LaTeX_Validator.Extensions;

namespace LaTeX_Validator;

public enum GlsType
{
    [EnumExtension("Akronym")]
    AcrShort,
    [EnumExtension("Langbezeichner")]
    AcrLong,
    [EnumExtension("Glossareintrag")]
    Gls,
    [EnumExtension("Temp")]
    None,
    [EnumExtension("Label")]
    Label,
    [EnumExtension("Serialisierungsfehler")]
    Serialize
}
