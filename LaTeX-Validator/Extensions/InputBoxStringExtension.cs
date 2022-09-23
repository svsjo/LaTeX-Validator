// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputBoxStringExtension.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Specialized;
using System.Linq;
using System.Xaml;

namespace LaTeX_Validator.Extensions;

internal static class InputBoxStringExtension
{
    public static StringCollection ToStringCollection(this string toSplit)
    {
        var stringCollection = new StringCollection();
        if (string.IsNullOrEmpty(toSplit)) return stringCollection;

        var elements = toSplit.Split(",");
        stringCollection.AddRange(elements);

        return stringCollection;
    }

    public static string AggregateToString(this StringCollection toAggregate)
    {
        if (toAggregate.Count == 0) return string.Empty;

        var enumerable = toAggregate.ToList();
        var returnString = enumerable.Aggregate((x,y) => x + "," + y);

        return returnString;
    }
}
