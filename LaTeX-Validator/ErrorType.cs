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
    MissingRef,
    [EnumExtension("Label falsch benannt")]
    LabelNaming,
    [EnumExtension("Kein Autoref verwendet")]
    WrongRefType,
    [EnumExtension("Serialisierungsfehler")]
    Serialize,
    [EnumExtension("Quelle nie angegeben")]
    MissingCitation
}
