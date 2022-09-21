// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObservableCollectionExtensions.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LaTeX_Validator.Extensions;

internal static class ObservableCollectionExtensions
{
    public static void AddRange(this ObservableCollection<GlsError> originalCollection, IEnumerable<GlsError> collectionToAdd)
    {
        foreach (var glsError in collectionToAdd)
        {
            originalCollection.Add(glsError);
        }
    }

}
