using System;
using System.Linq;
using System.Reflection;

namespace LaTeX_Validator.Extensions;

internal static class EnumExtensions
{
    public static string? GetStringValue(this Enum actualEnum)
    {
        return actualEnum.GetType().GetMember(actualEnum.ToString()).FirstOrDefault()?.GetCustomAttribute<EnumExtensionAttribute>()?.Value;
    }
}