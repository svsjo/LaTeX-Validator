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
            var wordConent = obj.WordContent;
            var actualForm = obj.ActualForm.ToString();
            var errorType = obj.ErrorType.ToString();
            var file = obj.File;
            var line = obj.Line.ToString();
            var pos = obj.LinePosition.ToString();
            var resultString = string.Join(",", wordConent, actualForm, errorType, file, line, pos);
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
            var splittedString = obj?.Split(",");
            if(splittedString == null || splittedString.Length < 6) continue;

            objectList.Add(new GlsError()
                           {
                               WordContent = splittedString.ElementAt(0),
                               ActualForm = ParseActualForm(splittedString.ElementAt(1)),
                               ErrorType = ParseErrorType(splittedString.ElementAt(2)),
                               File = splittedString.ElementAt(3),
                               Line = int.Parse(splittedString.ElementAt(4)),
                               LinePosition = int.Parse(splittedString.ElementAt(5)),
                               ErrorStatus = ErrorStatus.Ignored
                           });
        }

        return objectList;
    }

    private static ErrorType ParseErrorType(string element)
    {
       var type = element switch
        {
            "MissingGls" => ErrorType.MissingGls,
            "ShouldBeAcrLong" => ErrorType.ShouldBeAcrLong,
            "MissingRef" => ErrorType.MissingRef,
            "LabelNaming" => ErrorType.LabelNaming,
            "WrongRefType" => ErrorType.WrongRefType,
            "MissingCitation" => ErrorType.MissingCitation,
            _ => ErrorType.Serialize
        };

        return type;
    }

    private static GlsType ParseActualForm(string element)
    {
        var type = element switch
        {
            "AcrShort" => GlsType.AcrShort,
            "AcrLong" => GlsType.AcrLong,
            "Gls" => GlsType.Gls,
            "None" => GlsType.None,
            "Label" => GlsType.Label,
            "CitationLabel" => GlsType.CitationLabel,
            _ => GlsType.Serialize
        };

        return type;
    }
}
