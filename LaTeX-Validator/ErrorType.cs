// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorType.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using LaTeX_Validator.Extensions;

namespace LaTeX_Validator;

public enum ErrorType
{
    [EnumExtension("GLS fehlt")]
    MissingGls,
    [EnumExtension("Sollte AcrLong/Short sein")]
    ShouldBeAcrLong,
    [EnumExtension("Referenzierung Fehlt")]
    MissingAutoref,
    [EnumExtension("Label falsch benannt")]
    LabelNaming
}
