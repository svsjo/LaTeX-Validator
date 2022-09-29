using System;
using System.Linq;
using System.Reflection;

namespace LaTeX_Validator.Extensions
{
    internal static class EnumExtensions
    {
        public static string? GetStringValue(this Enum abc)
        {
            return abc?.GetType()?.GetMember(abc.ToString())?.FirstOrDefault()?.GetCustomAttribute<EnumExtensionAttribute>()?.Value;
        }
    }
}
