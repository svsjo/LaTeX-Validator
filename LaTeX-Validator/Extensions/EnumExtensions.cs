using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
