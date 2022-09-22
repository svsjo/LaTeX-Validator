// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorStatus.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using LaTeX_Validator.Extensions;

namespace LaTeX_Validator;

public enum ErrorStatus
{
    [EnumExtension("Nicht ignorieren")]
    Ignored,
    [EnumExtension("Ignorieren")]
    NotIgnored
}
