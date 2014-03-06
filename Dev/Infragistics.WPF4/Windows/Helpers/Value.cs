using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;

namespace Infragistics.Windows.Helpers
{
    #region ValueBase abstract base class

    /// <summary>
    /// Abstract base class used by type specific value and range based converters for comparing relative values and calculating percent in range.
    /// </summary>
    [TypeConverter(typeof(ValueBaseTypeConverter))]
    public abstract class ValueBase : IComparable
    {
        #region Properties (abstract)

            #region CanConvertToDouble

        /// <summary>
        /// Returns whether the value can be converted to a double (read-only abstract)
        /// </summary>
        /// <seealso cref="UnderlyingValue"/>
        /// <seealso cref="ValueAsDouble"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public abstract bool CanConvertToDouble { get; }

            #endregion //CanConvertToDouble

            #region UnderlyingValue

        /// <summary>
        /// Returns the underlying value (read-only abstract)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="ValueAsDouble"/>
        protected abstract object UnderlyingValue { get; }

            #endregion //UnderlyingValue

            #region ValueAsDouble

        /// <summary>
        /// Returns the value converted to a double (read-only abstract)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="UnderlyingValue"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public abstract double ValueAsDouble { get; }

           #endregion //ValueAsDouble

        #endregion //Properties (abstract)

        #region Methods

            #region CalculatePercentInRange

        /// <summary>
        /// Calculates the percent within a specified range from this value to a specified end where a specified value falls.
        /// </summary>
        /// <param name="valueToSlot">The value whose percent in the range is to be calculated.</param>
        /// <param name="endValue">The end value of the range.</param>
        /// <param name="provider">An optional provider to use to do any needed conversion.</param>
        /// <returns>A value between 0 and 1.</returns>
        /// <remarks>
        /// <para>If this value is greater than the endValue the percent is calculated appropriately and a positive value between 0 and 1 is still returned.</para>
        /// <para>The base implemenation tries to convert all values to doubles. However, if the <see cref="CanConvertToDouble"/> property of any of the values returns false then 0 is returned.</para>
        /// </remarks>
        public virtual double CalculatePercentInRange(object valueToSlot, ValueBase endValue, IFormatProvider provider)
        {
            if (valueToSlot == null || valueToSlot is DBNull)
                return 0.0d;

            if (endValue == null)
                throw new ArgumentNullException("endValue");

            if (!endValue.CanConvertToDouble ||
                 !this.CanConvertToDouble)
                return 0.0d;

            double from = this.ValueAsDouble;
            double to = endValue.ValueAsDouble;
            double value;

            if (valueToSlot is ValueBase)
                value = ((ValueBase)valueToSlot).ValueAsDouble;
            else
                value = this.ConvertValueToDouble(valueToSlot, provider);

            // if any of the values are not a number then return 0
            if (double.IsNaN(value) ||
                double.IsNaN(from) ||
                double.IsNaN(to))
                return 0.0;

            bool rangeFlipped = from > to;

            double range = Math.Abs(to - from);

            if (range == 0.0d)
                return 0.0;

            if (rangeFlipped)
            {
                if (value <= to)
                    return 0.0d;

                if (value >= from)
                    return 1.0d;

                return (from - value) / range;
            }
            else
            {
                if (value <= from)
                    return 0.0d;

                if (value >= to)
                    return 1.0d;

                return (value - from) / range;
            }
        }

            #endregion //CalculatePercentInRange

            #region CompareTo

        /// <summary>
        /// Compares 2 values
        /// </summary>
        /// <param name="obj">value to compare</param>
        /// <returns>Returns 0 if values are equal, 1 if the passed in value is less than this value or -1 if its greater.</returns>
        public virtual int CompareTo(object obj)
        {
            if (obj == null ||
                obj is DBNull)
                return 1;
 
            double valueToTest;
            double valueAsDouble;
			ValueBase valueBase = obj as ValueBase;

            if (null != valueBase &&
                valueBase.CanConvertToDouble &&
                this.CanConvertToDouble)
            {
                valueToTest = valueBase.ValueAsDouble;
                valueAsDouble = this.ValueAsDouble;

                return valueAsDouble.CompareTo(valueToTest);
            }

            object value = this.UnderlyingValue;

            if (value is IComparable &&
                value.GetType() == obj.GetType())
                return ((IComparable)value).CompareTo(obj);

            valueToTest = this.ConvertValueToDouble(obj, null);
            valueAsDouble = this.ValueAsDouble;

            return valueAsDouble.CompareTo(valueToTest);
        }

            #endregion //CompareTo

            #region ConvertValueToDouble

        /// <summary>
        /// Converts a passed in value to a double
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="provider">An optional provider to use to do the conversion.</param>
        /// <returns>The converted double value</returns>
        protected virtual double ConvertValueToDouble(object value, IFormatProvider provider)
        {
			IConvertible convertible = value as IConvertible;
            if (null != convertible)
                return convertible.ToDouble(provider);

            return double.NaN;
        }

            #endregion //ConvertValueToDouble	
    
        #endregion //Methods

        #region ValueBaseTypeConverter private class

        private class ValueBaseTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                // we will handle conversions from strings so return true here
                if (sourceType == typeof(string))
                    return true;

                // otherwise call the base implmenation
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                string str = value as string;

                // if the value is not a string then call the base implementation 
                if (str == null)
                    return base.ConvertFrom(context, culture, value);

                double result;

                // first try to parse the string as a double
                bool succeeded = double.TryParse(str, out result);

                // if successful return a DoubleValue
                if (succeeded)
                    return new DoubleValue(result);

                // otherwise try to parse it as a date
                try
                {
                    return new DateTimeValue(DateTime.Parse(str, culture));
                }
                catch
                {
                    throw new ArgumentException(SR.GetString("LE_ArgumentException_4", str));
                }
            }
        }

        #endregion //ValueBaseTypeConverter private class	
    
    }

    #endregion //ValueBase abstract base class

    #region DoubleValue

    /// <summary>
    /// A class that represents a value of type double
    /// </summary>
    /// <seealso cref="ValueBase"/>
    public class DoubleValue : ValueBase
    {
        #region Private members

        private double _value = double.NaN;

        #endregion //Private members	

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public DoubleValue() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">The initial value</param>
        public DoubleValue(double value)
        {
            this._value = value;
        }

        #endregion //Constructors	
    
        #region Base class overrides

        /// <summary>
        /// Returns whether the value can be converted to a double (read-only)
        /// </summary>
        /// <seealso cref="UnderlyingValue"/>
        /// <seealso cref="ValueAsDouble"/>
        public override bool CanConvertToDouble
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the underlying value (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="ValueAsDouble"/>
        protected override object UnderlyingValue
        {
            get { return this._value; }
        }

        /// <summary>
        /// Returns the value converted to a double (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="UnderlyingValue"/>
        public override double ValueAsDouble
        {
            get { return this._value; }
        }

        #endregion //Base class overrides

        #region Value

        /// <summary>
        /// Gets/sets the underyling value
        /// </summary>
        public double Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        #endregion //Value
    }

    #endregion //DoubleValue

    #region DateTimeValue

    /// <summary>
    /// A class that represents a value of type DateTime
    /// </summary>
    /// <seealso cref="ValueBase"/>
    public class DateTimeValue : ValueBase
    {
        #region Private members

        private DateTime _value;

        #endregion //Private members	

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public DateTimeValue() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">The initial value</param>
        public DateTimeValue(DateTime value)
        {
            this._value = value;
        }

        #endregion //Constructors	
    
        #region Base class overrides

            #region CanConvertToDouble

        /// <summary>
        /// Returns whether the value can be converted to a double (read-only)
        /// </summary>
        /// <seealso cref="Value"/>
        /// <seealso cref="ValueAsDouble"/>
        public override bool CanConvertToDouble
        {
            get { return true; }
        }

            #endregion //CanConvertToDouble	

            #region ConvertValueToDouble

        /// <summary>
        /// Converts a passed in value to a double
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="provider">An optional provider to use to do the conversion.</param>
        /// <returns>The converted double value</returns>
        protected override double ConvertValueToDouble(object value, IFormatProvider provider)
        {
            if (value is DateTime)
                return ((DateTime)value).Ticks;

            if (value is string)
            {
                double result;
                if ( double.TryParse(value as string, out result) )
                    return result;
            }

            return double.NaN;
        }

            #endregion //ConvertValueToDouble	
    
            #region UnderlyingValue

        /// <summary>
        /// Returns the underlying value (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="ValueAsDouble"/>
        protected override object UnderlyingValue
        {
            get { return this._value; }
        }

            #endregion //UnderlyingValue	
    
            #region ValueAsDouble

        /// <summary>
        /// Returns the value converted to a double (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="Value"/>
        public override double ValueAsDouble
        {
            get { return this.ConvertValueToDouble(this._value, null); }
        }

            #endregion //ValueAsDouble	
    
        #endregion //Base class overrides

        #region Value

        /// <summary>
        /// Gets/sets the underyling value
        /// </summary>
        public DateTime Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        #endregion //Value
    }

    #endregion //DateTimeValue

    #region Int32Value

    /// <summary>
    /// A class that represents a value of type Int32
    /// </summary>
    /// <seealso cref="ValueBase"/>
    public class Int32Value : ValueBase
    {
        #region Private members

        private Int32 _value;

        #endregion //Private members	

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public Int32Value() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">The initial value</param>
        public Int32Value(Int32 value)
        {
            this._value = value;
        }

        #endregion //Constructors	
    
        #region Base class overrides

        /// <summary>
        /// Returns whether the value can be converted to a double (read-only)
        /// </summary>
        /// <seealso cref="Value"/>
        /// <seealso cref="ValueAsDouble"/>
        public override bool CanConvertToDouble
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the underlying value (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="ValueAsDouble"/>
        protected override object UnderlyingValue
        {
            get { return this._value; }
        }

        /// <summary>
        /// Returns the value converted to a double (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="Value"/>
        public override double ValueAsDouble
        {
            get { return this._value; }
        }

        #endregion //Base class overrides

        #region Value

        /// <summary>
        /// Gets/sets the underyling value
        /// </summary>
        public Int32 Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        #endregion //Value
    }

    #endregion //Int32Value

    #region Int64Value

    /// <summary>
    /// A class that represents a value of type Int64
    /// </summary>
    /// <seealso cref="ValueBase"/>
    public class Int64Value : ValueBase
    {
        #region Private members

        private Int64 _value;

        #endregion //Private members	

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public Int64Value() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">The initial value</param>
        public Int64Value(Int64 value)
        {
            this._value = value;
        }

        #endregion //Constructors	
    
        #region Base class overrides

        /// <summary>
        /// Returns whether the value can be converted to a double (read-only)
        /// </summary>
        /// <seealso cref="Value"/>
        /// <seealso cref="ValueAsDouble"/>
        public override bool CanConvertToDouble
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the underlying value (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="ValueAsDouble"/>
        protected override object UnderlyingValue
        {
            get { return this._value; }
        }

        /// <summary>
        /// Returns the value converted to a double (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="Value"/>
        public override double ValueAsDouble
        {
            get { return this._value; }
        }

        #endregion //Base class overrides

        #region Value

        /// <summary>
        /// Gets/sets the underyling value
        /// </summary>
        public Int64 Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        #endregion //Value
    }

    #endregion //Int64Value

    #region DecimalValue

    /// <summary>
    /// A class that represents a value of type Decimal
    /// </summary>
    /// <seealso cref="ValueBase"/>
    public class DecimalValue : ValueBase
    {
        #region Private members

        private Decimal _value;

        #endregion //Private members	

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public DecimalValue() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">The initial value</param>
        public DecimalValue(Decimal value)
        {
            this._value = value;
        }

        #endregion //Constructors	
    
        #region Base class overrides

        /// <summary>
        /// Returns whether the value can be converted to a double (read-only)
        /// </summary>
        /// <seealso cref="Value"/>
        /// <seealso cref="ValueAsDouble"/>
        public override bool CanConvertToDouble
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the underlying value (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="ValueAsDouble"/>
        protected override object UnderlyingValue
        {
            get { return this._value; }
        }

        /// <summary>
        /// Returns the value converted to a double (read-only)
        /// </summary>
        /// <seealso cref="CanConvertToDouble"/>
        /// <seealso cref="Value"/>
        public override double ValueAsDouble
        {
            get { return (double)this._value; }
        }

        #endregion //Base class overrides

        #region Value

        /// <summary>
        /// Gets/sets the underyling value
        /// </summary>
        public Decimal Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        #endregion //Value
    }

    #endregion //DecimalValue

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