using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Infragistics.Controls.Charts.Util
{
    /// <summary>
    /// Utility class for range partitioning.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Partitioner : Numeric
    {
        private Partitioner() {  }

        #region In-place List of Comparable by Value
        /// <summary>
        /// </summary>
        /// <remarks>
        /// The list is reordered such that list[0] to list[k-1] are less than list[k] and
        /// all items list[k+1] to list[list.Count-1] are greater than list[k].
        /// <para>
        /// The items within the upper and lower subranges are in no particular order,
        /// and specifically are not sorted.
        /// </para>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">List of values to partition.</param>
        /// <param name="value">Pivot value.</param>
        /// <param name="begin">Index of first item in subrange.</param>
        /// <param name="end">Index of last item in subrange.</param>
        /// <returns>Index of first item greater than the specified pivot value.</returns>
        public static int PartitionByValue<T>(IList<T> values, T value, int begin, int end) where T : IComparable
        {
            int i = begin;
            int j = end;

            while (i <= j)
            {
                while (i <= j && values[i].CompareTo(value) <= 0)
                {
                    ++i;
                }

                while(i < j && values[j].CompareTo(value) > 0)
                {
                    --j;
                }

                if (i < j)
                {
                    Swap(values, i, j);
                    --j;
                }

                ++i;
            }

            return i - 1;
        }
        #endregion

        #region Indirect List of Comparable by Value
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">List of values.</param>
        /// <param name="indices">List of indices to partition.</param>
        /// <param name="value">Pivot value.</param>
        /// <param name="begin">Index of first index in subrange.</param>
        /// <param name="end">Index of last index in subrange.</param>
        /// <returns></returns>
        public static int PartitionByValue<T>(IList<T> values, IList<int> indices, T value, int begin, int end) where T: IComparable
        {
            return PartitionByValue((i)=>values[i], indices, value, begin, end);
        }
        #endregion

        #region Indirect Comparable Delegate by Value
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="indices"></param>
        /// <param name="value"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int PartitionByValue<T>(ComparableDelegate values, IList<int> indices, T value, int begin, int end) where T : IComparable
        {
            int i = begin;
            int j = end;

            while (i <= j)
            {
                while (i < j && value.CompareTo(values(indices[i])) >= 0)
                {
                    ++i;
                }

                while (i < j && value.CompareTo(values(indices[j])) < 0)
                {
                    --j;
                }

                if (i < j)
                {
                    Swap(indices, i, j);
                    --j;
                }

                ++i;
            }

            return i - 1;
        }
        #endregion

        #region In-place List of Comparable by Order
        /// <summary>
        /// Calculates the kth biggest item in the current list object and partitions
        /// the list accordingly.
        /// </summary>
        /// <remarks>
        /// The list is reordered such that list[0] to list[k-1] are less than list[k] and
        /// all items list[k+1] to list[list.Count-1] are greater than list[k].
        /// <para>
        /// The items within the upper and lower subranges are in no particular order,
        /// and specifically are not sorted.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">Where IComparable.</typeparam>
        /// <param name="list">List object to partition.</param>
        /// <param name="k">Pivot item order.</param>
        /// <returns>Pivot item value.</returns>
        public static T Partition<T>(IList<T> list, int k) where T : IComparable
        {
            return Partition(list, k, 0, list.Count);
        }

        /// <summary>
        /// Calculates the kth biggest item in the specified subrange of the current
        /// list object and partitions it accordingly.
        /// </summary>
        /// <remarks>
        /// The list is reordered such that list[position] to list[position+k-1] are
        /// less than list[position+k] and
        /// all items list[position+k+1] to list[position+Count] are greater than list[k].
        /// <para>
        /// The items within the upper and lower subranges are in no particular order,
        /// and specifically are not sorted.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">Where IComparable.</typeparam>
        public static T Partition<T>(IList<T> list, int k, int position, int count) where T : IComparable
        {
            int left = position;
            int right = position + count - 1;

            for (; ; )
            {
                if (right <= left + 1)
                {
                    if (right == left + 1 && list[right].CompareTo(list[left]) < 0)
                    {
                        Swap(list, left, right);
                    }

                    return list[k];
                }

                int mid = (left + right) >> 1;
                Swap(list, mid, left + 1);

                if (list[left].CompareTo(list[right]) > 0)
                {
                    Swap(list, left, right);
                }

                if (list[left + 1].CompareTo(list[right]) > 0)
                {
                    Swap(list, left + 1, right);
                }

                if (list[left].CompareTo(list[left + 1]) > 0)
                {
                    Swap(list, left, left + 1);
                }

                int i = left + 1;
                int j = right;
                T a = list[left + 1];

                for (; ; )
                {
                    do
                    {
                        ++i;
                    } while (list[i].CompareTo(a) < 0);

                    do
                    {
                        --j;
                    } while (list[j].CompareTo(a) > 0);

                    if (j < i)
                    {
                        break;
                    }

                    Swap(list, i, j);
                }

                list[left + 1] = list[j];
                list[j] = a;

                if (j >= k)
                {
                    right = j - 1;
                }

                if (j <= k)
                {
                    left = i;
                }
            }
        }
        #endregion

        #region Indirect List of Comparable by Order
        /// 
        /// 
        /// <summary>
        /// Indirect selection and partition.
        /// </summary>
        /// <typeparam name="T">Where IComparable</typeparam>
        /// <param name="list"></param>
        /// <param name="k"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static T Partition<T>(IList<T> list, int k, IList<int> indices) where T : IComparable
        {
            return Partition(list, k, indices, 0, indices.Count);
        }

        /// <summary>
        /// Indirect selection and partition with subrange
        /// </summary>
        /// <typeparam name="T">Where IComparable</typeparam>
        /// <param name="list"></param>
        /// <param name="k"></param>
        /// <param name="indices">Indices of visible items</param>
        /// <param name="position"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static T Partition<T>(IList<T> list, int k, IList<int> indices, int position, int count) where T : IComparable
        {
            int left = position;
            int right = position + count - 1;

            for (; ; )
            {
                if (right <= left + 1)
                {
                    if (right == left + 1 && list[indices[right]].CompareTo(list[indices[left]]) < 0)
                    {
                        Swap(indices, left, right);
                    }

                    return list[indices[k]];
                }

                int mid = (left + right) >> 1;
                Swap(indices, mid, left + 1);

                if (list[indices[left]].CompareTo(list[indices[right]]) > 0)
                {
                    Swap(indices, left, right);
                }

                if (list[indices[left + 1]].CompareTo(list[indices[right]]) > 0)
                {
                    Swap(indices, left + 1, right);
                }

                if (list[indices[left]].CompareTo(list[indices[left + 1]]) > 0)
                {
                    Swap(indices, left, left + 1);
                }

                int i = left + 1;
                int j = right;
                int ia = indices[left + 1];
                T a = list[ia];

                for (; ; )
                {
                    do
                    {
                        ++i;
                    } while (list[indices[i]].CompareTo(a) < 0);

                    do
                    {
                        --j;
                    } while (list[indices[j]].CompareTo(a) > 0);

                    if (j < i)
                    {
                        break;
                    }

                    Swap(indices, i, j);
                }

                indices[left + 1] = indices[j];
                indices[j] = ia;

                if (j >= k)
                {
                    right = j - 1;
                }

                if (j <= k)
                {
                    left = i;
                }
            }
        }
        #endregion

        #region Indirect Comparable Delegate by Order
        /// <summary>
        /// Indirect Comparable Delegate by Order
        /// </summary>
        /// <param name="comparable"></param>
        /// <param name="k"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static IComparable Partition(ComparableDelegate comparable, int k, IList<int> indices)
        {
            return Partition(comparable, k, indices, 0, indices.Count);
        }

        /// <summary>
        /// Indirect Comparable Delegate by Order
        /// </summary>
        /// <param name="comparable"></param>
        /// <param name="k"></param>
        /// <param name="indices"></param>
        /// <param name="position"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IComparable Partition(ComparableDelegate comparable, int k, IList<int> indices, int position, int count)
        {
            int left = position;
            int right = position + count - 1;

            for (; ; )
            {
                if (right <= left + 1)
                {
                    if (right == left + 1 && comparable(indices[right]).CompareTo(comparable(indices[left])) < 0)
                    {
                        Swap(indices, left, right);
                    }

                    return comparable(indices[k]);
                }

                int mid = (left + right) >> 1;
                Swap(indices, mid, left + 1);

                if (comparable(indices[left]).CompareTo(comparable(indices[right])) > 0)
                {
                    Swap(indices, left, right);
                }

                if (comparable(indices[left + 1]).CompareTo(comparable(indices[right])) > 0)
                {
                    Swap(indices, left + 1, right);
                }

                if (comparable(indices[left]).CompareTo(comparable(indices[left + 1])) > 0)
                {
                    Swap(indices, left, left + 1);
                }

                int i = left + 1;
                int j = right;
                int ia = indices[left + 1];
                IComparable a = comparable(ia);

                for (; ; )
                {
                    do
                    {
                        ++i;
                    } while (comparable(indices[i]).CompareTo(a) < 0);

                    do
                    {
                        --j;
                    } while (comparable(indices[j]).CompareTo(a) > 0);

                    if (j < i)
                    {
                        break;
                    }

                    Swap(indices, i, j);
                }

                indices[left + 1] = indices[j];
                indices[j] = ia;

                if (j >= k)
                {
                    right = j - 1;
                }

                if (j <= k)
                {
                    left = i;
                }
            }
        }
        #endregion

        /// <summary>
        /// Swap two list items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">Current list object.</param>
        /// <param name="a">Index of first item to swap.</param>
        /// <param name="b">Index of second item to swap.</param>
        private static void Swap<T>(IList<T> list, int a, int b)
        {
            T tmp = list[a];
            list[a] = list[b];
            list[b] = tmp;
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