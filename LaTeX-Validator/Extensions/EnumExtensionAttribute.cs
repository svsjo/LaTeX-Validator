// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumExtensionAttribute.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Schwab, Jonathan</author>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace LaTeX_Validator.Extensions;

internal class EnumExtensionAttribute : Attribute
{
    public readonly string? Value;

    public EnumExtensionAttribute(string? value)
    {
        this.Value = value;
    }
}