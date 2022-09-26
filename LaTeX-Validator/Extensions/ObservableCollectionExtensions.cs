// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObservableCollectionExtensions.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LaTeX_Validator.Extensions;

internal static class ObservableCollectionExtensions
{
    public static List<GlsError> AddRangeIfPossibleAndReturnErrors(this ObservableCollection<GlsError> originalCollection,
                                                      IEnumerable<GlsError> collectionToAdd,
                                                      List<GlsError> collectionToIgnore)
    {
        var ignore = new List<GlsError>();

        foreach (var glsError in collectionToAdd)
        {
            if (collectionToIgnore.Any(x => x.IsEqual(glsError)))
            {
                glsError.ErrorStatus = ErrorStatus.Ignored;
                ignore.Add(glsError);
            }
            else originalCollection.Add(glsError);
        }

        return ignore;
    }

    public static void AddRange(this ObservableCollection<GlsError> originalCollection,
                                                      IEnumerable<GlsError> collectionToAdd)
    {
        foreach (var glsError in collectionToAdd)
        {
           originalCollection.Add(glsError);
        }
    }

    public static void RemoveRange(this ObservableCollection<GlsError> originalCollection,
                                IEnumerable<GlsError> collectionToRemove)
    {
        foreach (var glsError in collectionToRemove)
        {
            originalCollection.Remove(glsError);
        }
    }

}
