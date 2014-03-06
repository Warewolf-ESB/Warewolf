using System;
using System.Collections;



using System.Linq;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics.CodeAnalysis;

namespace Infragistics
{
    /// <summary>
    /// Utility class for array operations.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Util is a word.")]
    public static class ArrayUtil
    {
        /// <summary>
        /// Shuffles the contents of the current IList object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list != null)
            {
                Random random = new Random();

                for (int n = list.Count - 1; n > 0; --n)
                {
                    int k = random.Next(n);

                    T temp = list[n];
                    list[n] = list[k];
                    list[k] = temp;
                }
            }
        }

        /// <summary>
        /// Returns the insertion index for an element in a sorted list.
        /// </summary>
        /// <remarks>
        /// The list must be sorted in ascending order prior to calling this method. 
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        /// <returns>Insertion index for the specified value.</returns>
        public static int InsertionIndex<T>(this IList<T> collection, T value) where T : IComparable
        {
            int index = -1;
            int b = 0;
            int e = collection.Count;

            while (index == -1)
            {
                if (e <= b)
                {
                    index = b;              // found an insertion position
                }
                else
                {
                    int m = (b + e) / 2;    // pivot is midpoint

                    switch (System.Math.Sign(value.CompareTo(collection[m])))
                    {
                        case -1:
                            e = m;          // recurse into lower half
                            break;

                        case 0:
                            index = m;      // found a match 
                            break;

                        case 1:
                            b = m + 1;      // recurse into upper half
                            break;
                    }
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the insertion index for an element in a sorted list.
        /// </summary>
        /// <remarks>
        /// The list must be sorted according to the specified comparison
        /// prior to calling this method. 
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="comparison"></param>
        /// <param name="value"></param>
        /// <returns>Insertion index for the specified value.</returns>
        public static int InsertionIndex<T>(this IList<T> collection, Comparison<T> comparison, T value)
        {
            int index = -1;

            int b = 0;
            int e = collection.Count;

            while (index == -1)
            {
                if (e <= b)
                {
                    index = b;              // found an insertion position
                }
                else
                {
                    int m = (b + e) / 2;    // pivot is midpoint

                    switch (System.Math.Sign(comparison(value, collection[m])))
                    {
                        case -1:
                            e = m;          // recurse into lower half
                            break;

                        case 0:
                            index = m;      // found a match 
                            break;

                        case 1:
                            b = m + 1;      // recurse into upper half
                            break;
                    }
                }
            }

            return index;
        }


        /// <summary>
        /// Permutes an enumeration of enumerators, one for each permutation of the input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Permute<T>(this IEnumerable<T> list, int count)
        {
            if (count == 0)
            {
                yield return new T[0];
            }
            else
            {
                foreach (T startingElement in list)
                {
                    IEnumerable<T> remainingItems = list.Except(new T[] { startingElement });

                    foreach (IEnumerable<T> permutationOfRemainder in Permute(remainingItems, count - 1))
                    {
                        yield return new T[] { startingElement }.Concat(permutationOfRemainder);
                    }
                }
            }
        }

        /// <summary>
        /// Conducts a binary search for a value in the list using a given comparison function.
        /// </summary>
        /// <typeparam name="T">The Type of items in the list.</typeparam>
        /// <param name="list">The list of items to search.</param>
        /// <param name="comparisonFunction">A comparison function used for finding the target item in the list.</param>
        /// <returns>The index of the found item in the list.</returns>
        public static int BinarySearch<T>(this IList<T> list, Func<T, int> comparisonFunction)
        {
            int currMin = 0;
            int currMax = list.Count - 1;

            while (currMin <= currMax)
            {
                int currMid = (currMin + ((currMax - currMin) >> 1));
                int compResult = comparisonFunction(list[currMid]);
                if (compResult < 0)
                {
                    currMax = currMid - 1;
                }
                else if (compResult > 0)
                {
                    currMin = currMid + 1;
                }
                else
                {
                    return currMid;
                }
            }

            return ~currMin;
        }
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved