using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Windows;
using System.Runtime.CompilerServices;

using System.Linq;
using System.Linq.Expressions;




namespace Infragistics
{
    /// <summary>
    /// Represents a cached column of doubles in a fast datasource.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    sealed class FastItemColumn : IFastItemColumnInternal, IFastItemColumn<double>
    {




        internal FastItemColumn(FastItemsSource fastItemsSource, string propertyName



            )
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

        [Weak]
        private FastItemsSource fastItemsSource;

        [DontObfuscate]
        private string _propertyName = null;
        public string PropertyName 
        { 
            get
            {
                return _propertyName;
            }
            private set
            {
                _propertyName = value;
            }
        }

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
                if (double.IsNaN(minimum) && this.Values != null)
                {
                    minimum = double.PositiveInfinity;

                    foreach (double value in this.Values)
                    {
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
                if (double.IsNaN(maximum) && this.Values != null)
                {
                    maximum = double.NegativeInfinity;

                    foreach (double value in this.Values)
                    {
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



                return this.Values[index];

            }
            set
            {
                Values[index] = value;
            }
        }

        public IEnumerator<double> GetEnumerator()
        {
            return this.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Values.GetEnumerator();
        }

        public bool Contains(double item)
        {
            return this.Values.Contains(item);
        }

        public void CopyTo(double[] array, int arrayIndex)
        {
            this.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.Values.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(double item)
        {
            return this.Values.IndexOf(item);
        }
        #endregion

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

        public bool Reset()
        {
            this.Values = null;
            this.Minimum = double.NaN;
            this.Maximum = double.NaN;

            return FastItemsSource != null ? InsertRange(0, FastItemsSource.Count) : true;
        }



#region Infragistics Source Cleanup (Region)





















































































































































#endregion // Infragistics Source Cleanup (Region)

        public bool InsertRange(int position, int count)
        {
            List<double> newValues = new List<double>() { Capacity = count };
            FastItemsSource source = fastItemsSource;
            
            double minimum = Minimum;
            double maximum = Maximum;
            bool minimumIsNaN = double.IsNaN(Minimum);
            bool maximumIsNaN = double.IsNaN(Maximum);

            double newValue;

            for (int i_ = position; i_ < position + count; ++i_)
            {
                newValue = ToDouble(source[i_]);

#pragma warning disable 1718
                bool valIsNaN = (newValue != newValue); // possibly not a good idea, but might be loads faster. esp for IE js.
#pragma warning restore 1718
                if (minimumIsNaN || newValue < minimum)
                {
                    minimum = newValue;
                    minimumIsNaN = valIsNaN;
                }

                if (maximumIsNaN || newValue > maximum)
                {
                    maximum = newValue;
                    maximumIsNaN = valIsNaN;
                }

                newValues.Add(newValue);
            }

            Minimum = minimum;
            Maximum = maximum;

            if (this.Values == null)
            {
                this.Values = newValues;
            }
            else
            {
                this.Values.InsertRange(position, newValues);
            }
            return true;
        }

       
        public bool RemoveRange(int position, int count)
        {
            for (int i = position; i < position + count && !double.IsNaN(Minimum) && !double.IsNaN(Maximum); ++i)
            {
                if (this[i] == Minimum)
                {
                    Minimum = double.NaN;
                }

                if (this[i] == Maximum)
                {
                    Maximum = double.NaN;
                }
            }

            this.Values.RemoveRange(position, count);

            return true;
        }
        public void ReplaceMinMax(double oldValue, double newValue)
        {
            if (double.IsNaN(oldValue))
            {
                if (!double.IsNaN(newValue))
                {
                    if (!double.IsNaN(Minimum))
                    {
                        Minimum = System.Math.Min(newValue, Minimum);
                    }
                    if (!double.IsNaN(Maximum))
                    {
                        Maximum = System.Math.Max(newValue, Maximum);
                    }
                }

                return;
            }

            if (double.IsNaN(newValue))
            {
                Minimum = !double.IsNaN(Minimum) && oldValue == Minimum ? double.NaN : Minimum;
                Maximum = !double.IsNaN(Maximum) && oldValue == Maximum ? double.NaN : Maximum;

                return;
            }

            if (!double.IsNaN(Minimum))
            {
                if (oldValue == Minimum && newValue > Minimum)
                {
                    Minimum = double.NaN;
                }
                else
                {
                    Minimum = System.Math.Min(newValue, Minimum);
                }
            }

            if (!double.IsNaN(Maximum))
            {
                if (oldValue == Maximum && newValue < Maximum)
                {
                    Maximum = double.NaN;
                }
                else
                {
                    Maximum = System.Math.Max(newValue, Maximum);
                }
            }
        }
        public bool ReplaceRange(int position, int count)
        {
            bool ret = false;

            for (int i = 0; i < count; ++i)
            {
                double oldValue = this.Values[position + i];
                double newValue = ToDouble(FastItemsSource[position + i]);

                if (oldValue != newValue)
                {
                    this.Values[position + i] = newValue;
                    ret = true;

                    ReplaceMinMax(oldValue, newValue);
                }
            }

            return ret;
        }



        private FastReflectionHelper fastReflectionHelper;


        private double ToDouble(object item)
        {
            if (item == null)
            {
                return double.NaN;
            }



#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

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




#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)



            if (item is double)
            {
                return (double)item;
            }


            
            if (item is Duration)
            {
                item = ((Duration)item).TimeSpan;
            }

            if (item is DateTime)
            {
                return ((DateTime)item).Ticks;
            }

            if (item is TimeSpan)
            {
                return ((TimeSpan)item).Ticks;
            }



            if (item is IConvertible)
            {
                try
                {
                    return Convert.ToDouble(item);
                }
                catch
                {
                }
            }

            return double.NaN;

        }

        internal List<double> Values { get; private set;}
        public static List<int> GetSortedIndices(IList values)
        {


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            IEnumerable<int> result = Enumerable.Range(0, values.Count);
            result = result.OrderBy((i) => values[i]);
            return result.ToList();

        }
        internal List<int> GetSortedIndices()
        {
            return FastItemColumn.GetSortedIndices(this.Values);
        }



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

    }
    



    internal 

        sealed class FastItemDateTimeColumn : IFastItemColumnInternal, IFastItemColumn<DateTime>
    {





        internal FastItemDateTimeColumn(FastItemsSource fastItemsSource, string propertyName



            )
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

        [DontObfuscate]
        private string _propertyName = null;
        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            private set
            {
                _propertyName = value;
            }
        }

        private bool hasMinimum = false;
        private bool hasMaximum = false;
        /// <summary>
        /// Gets the minimum value for the current column.
        /// </summary>
        /// <remarks>
        /// Getting the minimum value takes amortized constant time.
        /// </remarks>
        public DateTime Minimum
        {
            get
            {
                if (!hasMinimum && Values != null)
                {
                    foreach (DateTime value in Values)
                    {
                        if (value < minimum)
                        {
                            minimum = value;
                        }
                    }
                    if (Values.Count > 0)
                    {
                        hasMinimum = true;
                    }
                }

                return minimum;
            }
            private set { minimum = value; }
        }
        DateTime minimum;

        /// <summary>
        /// Gets the maximum value for the current column.
        /// </summary>
        /// <remarks>
        /// Getting the maximum value takes amortized constant time.
        /// </remarks>
        public DateTime Maximum
        {
            get
            {
                if (!hasMaximum && Values != null)
                {
                    foreach (DateTime value in Values)
                    {
                        if (value > maximum)
                        {
                            maximum = value;
                        }
                    }
                    if (Values.Count > 0)
                    {
                        hasMaximum = true;
                    }
                }

                return maximum;
            }
            private set { maximum = value; }
        }
        DateTime maximum;

        #region IList<DateTime> implementation
        public DateTime this[int index]
        {
            get
            {



                return Values[index];

            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerator<DateTime> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public bool Contains(DateTime item)
        {
            return Values.Contains(item);
        }

        public void CopyTo(DateTime[] array, int arrayIndex)
        {
            Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Values.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(DateTime item)
        {
            return Values.IndexOf(item);
        }
        #endregion

        #region IList<double> non-implementation

        public void Add(DateTime item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(DateTime item)
        {
            throw new NotImplementedException();
        }
        public void Insert(int index, DateTime item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        #endregion

        public bool Reset()
        {
            Values = null;
            hasMinimum = false;
            hasMaximum = false;
            
            return FastItemsSource != null ? InsertRange(0, FastItemsSource.Count) : true;
        }



#region Infragistics Source Cleanup (Region)


























































































































































#endregion // Infragistics Source Cleanup (Region)

        public bool InsertRange(int position, int count)
        {
            List<DateTime> newValues = new List<DateTime>() { Capacity = count };
            FastItemsSource source = fastItemsSource;

            DateTime minimum = Minimum;
            DateTime maximum = Maximum;
            DateTime newValue;
            
            for (int i_ = position; i_ < position + count; ++i_)
            {
                newValue = ToDateTime(source[i_]);

                if (!hasMinimum)
                {
                    minimum = newValue;
                    hasMinimum = true;
                }
                else if (newValue < minimum)
                {
                    minimum = newValue;
                }

                if (!hasMaximum)
                {
                    maximum = newValue;
                    hasMaximum = true;
                }
                else if (newValue > maximum)
                {
                    maximum = newValue;
                }

                newValues.Add(newValue);
            }
            Minimum = minimum;
            Maximum = maximum;

            if (this.Values == null)
            {
                this.Values = newValues;
            }
            else
            {
                this.Values.InsertRange(position, newValues);
            }

            return true;
        }


        public bool RemoveRange(int position, int count)
        {
            for (int i = position; i < position + count; ++i)
            {
                if (this[i] == Minimum)
                {
                    hasMinimum = false;
                }

                if (this[i] == Maximum)
                {
                    hasMaximum = false;
                }
            }

            Values.RemoveRange(position, count);

            return true;
        }
        public void ReplaceMinMax(DateTime oldValue, DateTime newValue)
        {
            if (oldValue != DateTime.MinValue)
            {
                if (newValue != DateTime.MinValue)
                {
                    Minimum = newValue < Minimum ? newValue : Minimum;
                    Maximum = newValue > Maximum ? newValue : Maximum;
                }

                return;
            }

            Minimum = newValue < Minimum ? newValue : Minimum;
            Maximum = newValue > Maximum ? newValue : Maximum;
        }
        public bool ReplaceRange(int position, int count)
        {
            bool ret = false;

            for (int i = 0; i < count; ++i)
            {
                DateTime oldValue = Values[position + i];
                DateTime newValue = ToDateTime(FastItemsSource[position + i]);

                if (oldValue != newValue)
                {
                    Values[position + i] = newValue;
                    ret = true;

                    ReplaceMinMax(oldValue, newValue);
                }
            }

            return ret;
        }



        private FastReflectionHelper fastReflectionHelper;


        private DateTime ToDateTime(object item)
        {
            if (item == null)
            {
                return DateTime.MinValue;
            }



#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)


            if (fastReflectionHelper == null)
            {
                fastReflectionHelper = new FastReflectionHelper(false, PropertyName);
            }

            if (!fastReflectionHelper.Invalid)
            {
                item = fastReflectionHelper.GetPropertyValue(item.GetType(), item);

                if (item == null)
                {
                    return DateTime.MinValue;
                }
            }


            if (item is DateTime)
            {
                return (DateTime)item;
            }



            if (item is double)
            {
                return DateTime.FromOADate((double)item);
            }
            
            if (item is Duration)
            {
                item = ((Duration)item).TimeSpan;
            }

            if (item is TimeSpan)
            {
                return new DateTime(((TimeSpan)item).Ticks);
            }



            if (item is IConvertible)
            {
                try
                {
                    return Convert.ToDateTime(item);
                }
                catch
                {
                }
            }


            return DateTime.MinValue;

        }

        private List<DateTime> Values { get; set; }
        public List<int> GetSortedIndices()
        {
            return FastItemColumn.GetSortedIndices(this.Values);
        }



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

    }
    
    internal sealed class FastItemObjectColumn : IFastItemColumnInternal, IFastItemColumn<object>
    {




        internal FastItemObjectColumn(FastItemsSource fastItemsSource, string propertyName



            )
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

        [DontObfuscate]
        private string _propertyName;
        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            private set
            {
                _propertyName = value;
            }
        }

        /// <summary>
        /// Gets the minimum value for the current column.
        /// </summary>
        /// <remarks>
        /// Getting the minimum value takes amortized constant time.
        /// </remarks>
        public object Minimum
        {
            get
            {
                return _minimum;
            }
            private set 
            { 
                _minimum = value;
            }
        }
        object _minimum;

        /// <summary>
        /// Gets the maximum value for the current column.
        /// </summary>
        /// <remarks>
        /// Getting the maximum value takes amortized constant time.
        /// </remarks>
        public object Maximum
        {
            get
            {
                return _maximum;
            }
            private set 
            { 
                _maximum = value; 
            }
        }
        object _maximum;

        #region IList<object> implementation
        public object this[int index]
        {
            get
            {
                return Values[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public bool Contains(object item)
        {
            return Values.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Values.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(object item)
        {
            return Values.IndexOf(item);
        }
        #endregion

        #region IList<object> non-implementation

        public void Add(object item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(object item)
        {
            throw new NotImplementedException();
        }
        public void Insert(int index, object item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        #endregion

        public bool Reset()
        {
            Values = null;

            return FastItemsSource != null ? InsertRange(0, FastItemsSource.Count) : true;
        }
        public bool InsertRange(int position, int count)
        {



            List<object> newValues = new List<object>() { Capacity = count };


            for (int i = position; i < position + count; ++i)
            {
                object newValue = ToObject(FastItemsSource[i]);
                newValues.Add(newValue);
            }

            if (Values == null)
            {
                Values = newValues;
            }
            else
            {
                Values.InsertRange(position, newValues);
            }

            return true;
        }
        public bool ReplaceRange(int position, int count)
        {
            bool ret = false;

            for (int i = 0; i < count; ++i)
            {
                object oldValue = Values[position + i];
                object newValue = ToObject(FastItemsSource[position + i]);

                if (oldValue != newValue)
                {
                    Values[position + i] = newValue;
                    ret = true;
                }
            }

            return ret;
        }
        public bool RemoveRange(int position, int count)
        {
            Values.RemoveRange(position, count);

            return true;
        }
       

        private FastReflectionHelper fastReflectionHelper;


        private object ToObject(object item)
        {
            if (item == null)
            {
                return null;
            }



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

            if (fastReflectionHelper == null)
            {
                fastReflectionHelper = new FastReflectionHelper(false, PropertyName);
            }

            if (!fastReflectionHelper.Invalid)
            {
                item = fastReflectionHelper.GetPropertyValue(item.GetType(), item);
            }


            return item;

        }


        private List<object> Values { get; set; }
        internal List<int> GetSortedIndices()
        {
            return FastItemColumn.GetSortedIndices(this.Values);
        }



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

    }


    internal sealed class FastItemIntColumn : IFastItemColumnInternal, IFastItemColumn<int>
    {




        internal FastItemIntColumn(FastItemsSource fastItemsSource, string propertyName



            )
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

        [DontObfuscate]
        private string _propertyName = null;
        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            private set
            {
                _propertyName = value;
            }
        }

        /// <summary>
        /// Gets the minimum value for the current column.
        /// </summary>
        /// <remarks>
        /// Getting the minimum value takes amortized constant time.
        /// </remarks>
        public int Minimum
        {
            get
            {
                return _minimum;
            }
            private set
            {
                _minimum = value;
            }
        }
        int _minimum;

        /// <summary>
        /// Gets the maximum value for the current column.
        /// </summary>
        /// <remarks>
        /// Getting the maximum value takes amortized constant time.
        /// </remarks>
        public int Maximum
        {
            get
            {
                return _maximum;
            }
            private set
            {
                _maximum = value;
            }
        }
        int _maximum;

        #region IList<int> implementation
        public int this[int index]
        {
            get
            {
                return Values[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public bool Contains(int item)
        {
            return Values.Contains(item);
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Values.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(int item)
        {
            return Values.IndexOf(item);
        }
        #endregion

        #region IList<int> non-implementation

        public void Add(int item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(int item)
        {
            throw new NotImplementedException();
        }
        public void Insert(int index, int item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        #endregion

        public bool Reset()
        {
            Values = null;

            return FastItemsSource != null ? InsertRange(0, FastItemsSource.Count) : true;
        }



#region Infragistics Source Cleanup (Region)






















































































#endregion // Infragistics Source Cleanup (Region)

        public bool InsertRange(int position, int count)
        {
            List<int> newValues = new List<int>() { Capacity = count };
            FastItemsSource source = fastItemsSource;

            int minimum = Minimum;
            int maximum = Maximum;

            for (int i_ = position; i_ < position + count; ++i_)
            {
                int newValue = ToInt(source[i_]);

                newValues.Add(newValue);
            }

            if (Values == null)
            {
                this.Values = newValues;
            }
            else
            {
                Values.InsertRange(position, newValues);
            }

            return true;
        }


        public bool ReplaceRange(int position, int count)
        {
            bool ret = false;

            for (int i = 0; i < count; ++i)
            {
                int oldValue = Values[position + i];
                int newValue = ToInt(FastItemsSource[position + i]);

                if (oldValue != newValue)
                {
                    Values[position + i] = newValue;
                    ret = true;
                }
            }

            return ret;
        }
        public bool RemoveRange(int position, int count)
        {
            Values.RemoveRange(position, count);

            return true;
        }


        private FastReflectionHelper fastReflectionHelper;


        private int ToInt(object item)
        {


#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

            if (item == null)
            {
                return int.MinValue;
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
                    return int.MinValue;
                }
            }
            if (item is int)
            {
                return (int)item;
            }
            

            if (item is Duration)
            {
                item = ((Duration)item).TimeSpan;
            }

            if (item is TimeSpan)
            {
                item = ((TimeSpan)item).Ticks;
            }


            if (item is DateTime)
            {
                item = ((DateTime)item).Ticks;
            }


            if (item is IConvertible)
            {
                try
                {
                    return Convert.ToInt32(item);
                }
                catch
                {
                }
            }


            return int.MinValue;

        }


        private List<int> Values { get; set; }
        internal List<int> GetSortedIndices()
        {
            return FastItemColumn.GetSortedIndices(this.Values);
        }



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

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