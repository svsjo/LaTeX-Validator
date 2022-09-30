// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlsErrorSerialization.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using LaTeX_Validator.DataClasses;

namespace LaTeX_Validator.Extensions;

internal static class GlsErrorSerialization
{
    public static StringCollection ToStringCollection(this IEnumerable<GlsError> errorObjects)
    {
        var stringCollection = new StringCollection();

        foreach (var resultString in errorObjects.Select(obj => JsonSerializer.Serialize(obj))) stringCollection.Add(resultString);

        return stringCollection;
    }

    public static List<GlsError> ToGlsError(this StringCollection? serializedObjects)
    {
        var objectList = new List<GlsError>();
        if (serializedObjects == null) return objectList;

        foreach (var obj in serializedObjects)
        {
            if(string.IsNullOrEmpty(obj)) continue;
            var resultObj = JsonSerializer.Deserialize<GlsError>(obj);
            if(resultObj != null) objectList.Add(resultObj);
        }

        return objectList;
    }
}
