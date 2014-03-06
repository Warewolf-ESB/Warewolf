using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Infragistics
{
    /// <summary>
    /// Represents a cached column of doubles in a fast datasource.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    sealed class FastItemProxyColumn : IFastItemColumnInternal, IFastItemColumn<double>
    {
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets the minimum value for the current column.
        /// </summary>
        /// <remarks>
        /// Getting the minimum value takes amortized constant time.
        /// </remarks>
        public double Minimum
        {
            get
            {
                if (double.IsNaN(minimum) && FastItemsSource != null)
                {
                    minimum = double.PositiveInfinity;

                    for (int i = 0; i < FastItemsSource.Count; ++i)
                    {
                        double value = ToDouble(FastItemsSource[i]);

                        if (!double.IsNaN(value))
                        {
                            minimum = System.Math.Min(minimum, value);
                        }
                    }
                }

                return minimum;
            }
            private set { minimum = value; }
        }
        double minimum;

        /// <summary>
        /// Gets the maximum value for the current column.
        /// </summary>
        /// <remarks>
        /// Getting the maximum value takes amortized constant time.
        /// </remarks>
        public double Maximum
        {
            get
            {
                if (double.IsNaN(maximum) && FastItemsSource != null)
                {
                    maximum = double.NegativeInfinity;

                    for (int i = 0; i < FastItemsSource.Count; ++i)
                    {
                        double value = ToDouble(FastItemsSource[i]);

                        if (!double.IsNaN(value))
                        {
                            maximum = System.Math.Max(maximum, value);
                        }
                    }
                }

                return maximum;
            }
            private set { maximum = value; }
        }
        double maximum;

        #region IList<double> implementation
        public double this[int index]
        {
            get
            {






                return ToDouble(FastItemsSource[index]);

            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerator<double> GetEnumerator()
        {
            if (FastItemsSource != null)
            {
                for (int i = 0; i < FastItemsSource.Count; ++i)
                {
                    yield return ToDouble(FastItemsSource[i]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (FastItemsSource != null)
            {
                for (int i = 0; i < FastItemsSource.Count; ++i)
                {
                    yield return ToDouble(FastItemsSource[i]);
                }
            }
        }

        public bool Contains(double item)
        {
            foreach (double value in this)
            {
                if (value == item)
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(double[] array, int arrayIndex)
        {
            throw new NotImplementedException();

            // values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return FastItemsSource != null ? FastItemsSource.Count : 0; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(double item)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (this[i] == item)
                {
                    return i;
                }
            }

            return -1;
        }
        #endregion

        internal FastItemProxyColumn(FastItemsSource fastItemsSource, string propertyName)
        {
            PropertyName = propertyName;
            FastItemsSource = fastItemsSource;
        }

        internal FastItemsSource FastItemsSource
        {
            get { return fastItemsSource; }
            set
            {
                fastItemsSource = value;
                Reset();
            }
        }
        private FastItemsSource fastItemsSource;

        private FastReflectionHelper fastReflectionHelper;
         
        private double ToDouble(object item)
        {
            if (item == null)
            {
                return double.NaN;
            }

            if (fastReflectionHelper == null)
            {
                fastReflectionHelper = new FastReflectionHelper(false, PropertyName);
            }

            if (!fastReflectionHelper.Invalid)
            {
                item = fastReflectionHelper.GetPropertyValue(item.GetType(), item);

                if (item == null)
                {
                    return double.NaN;
                }
            }

            if (item is double)
            {
                return (double)item;
            }

            if (item is IConvertible)
            {
                try
                {
                    return Convert.ToDouble(item);
                }
                catch
                {
                    return double.NaN;
                }
            }

            if (item is TimeSpan)
            {
                return ((TimeSpan)item).TotalMilliseconds;
            }

            if (item is DateTime)
            {
                return ((DateTime)item).ToOADate();
            }

            if (item is DateTime?)
            {
                return (item as DateTime?).Value.ToOADate();
            }

            return double.NaN;
        }

        public bool Reset()
        {
            Minimum = double.NaN;
            Maximum = double.NaN;

            return FastItemsSource != null ? InsertRange(0, FastItemsSource.Count) : true;
        }
        public bool InsertRange(int position, int count)
        {
            Minimum = double.NaN;
            Maximum = double.NaN;

            return true;
        }
        public bool RemoveRange(int position, int count)
        {
            Minimum = double.NaN;
            Maximum = double.NaN;

            return true;
        }
        public bool ReplaceRange(int position, int count)
        {
            Minimum = double.NaN;
            Maximum = double.NaN;

            return true;
        }

        #region IList<double> non-implementation
        public void Add(double item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(double item)
        {
            throw new NotImplementedException();
        }
        public void Insert(int index, double item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        #endregion
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