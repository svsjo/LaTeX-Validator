// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlsErrorSerialization.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace LaTeX_Validator.Extensions;

internal static class GlsErrorSerialization
{
    /// <summary>
    /// Result-Strings Aufbau:
    /// "string WordContent,GlsType ActualForm,ErrorType ErrorType,string File,int Line,int LinePosition"
    /// ErrorStatus ist immer ErrorStatus.Ignored
    /// </summary>
    /// <param name="errorObjects"></param>
    /// <returns></returns>
    public static StringCollection Serialize(this List<GlsError> errorObjects)
    {
        var stringCollection = new StringCollection();

        foreach (var obj in errorObjects)
        {
            var resultString = JsonSerializer.Serialize(obj);
            stringCollection.Add(resultString);
        }

        return stringCollection;
    }

    public static List<GlsError> Deserialize(this StringCollection? serializedObjects)
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
