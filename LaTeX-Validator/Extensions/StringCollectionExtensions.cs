// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringCollectionExtensions.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace LaTeX_Validator.Extensions;

internal static class StringCollectionExtensions
{
    public static List<string> ToList(this StringCollection? stringCollection)
    {
        var list = stringCollection?.Cast<string>().ToList() ?? new List<string>();

        return list;
    }

    public static StringCollection ToStringCollection(this List<string> list)
    {
        var stringCollection = new StringCollection();
        stringCollection.AddRange(list.ToArray());

        return stringCollection;
    }
}
