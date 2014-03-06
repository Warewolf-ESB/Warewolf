using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;
using System.ComponentModel.Design.Serialization;
using Infragistics.Shared;

namespace Infragistics.Windows.Editors
{
    /// <summary>
    /// Structure used to represents a range of dates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(CalendarDateRangeConverter))]
    public struct CalendarDateRange : IEquatable<CalendarDateRange>,
        IComparable<CalendarDateRange>
    {
        #region Member Variables

        private DateTime _start;
        private DateTime _end;

        #endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="CalendarDateRange"/>
        /// </summary>
        /// <param name="date">The date to use for the start and end dates.</param>
        public CalendarDateRange(DateTime date)
            : this(date, date)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CalendarDateRange"/>
        /// </summary>
        /// <param name="start">The start date</param>
        /// <param name="end">The end date</param>
        public CalendarDateRange(DateTime start, DateTime end)
        {
            this._start = start;
            this._end = end;
        } 
        #endregion //Constructor

        #region Base class overrides

        #region GetHashCode
        /// <summary>
        /// Returns the hash code of the structure.
        /// </summary>
        /// <returns>A hash code for this instance</returns>
        public override int GetHashCode()
        {
            return this._start.GetHashCode() ^ this._end.GetHashCode();
        }
        #endregion //GetHashCode

        #region Equals
        /// <summary>
        /// Compares the specified object to this object to see if they are equivalent.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the objects are equal; otherwise false</returns>
        public override bool Equals(object obj)
        {
            if (obj is CalendarDateRange)
            {
                return this == (CalendarDateRange)obj;
            }

            return false;
        }
        #endregion //Equals

        #region Operator Overloads
        /// <summary>
        /// Compares the values of two <see cref="CalendarDateRange"/> structures for equality
        /// </summary>
        /// <param name="range1">The first structure</param>
        /// <param name="range2">The other structure</param>
        /// <returns>true if the two instances are equal; otherwise false</returns>
        public static bool operator ==(CalendarDateRange range1, CalendarDateRange range2)
        {
            return range1._start == range2._start && range1._end == range2._end;
        }

        /// <summary>
        /// Compares the values of two <see cref="CalendarDateRange"/> structures for inequality
        /// </summary>
        /// <param name="range1">The first structure</param>
        /// <param name="range2">The other structure</param>
        /// <returns>true if the two instances are not equal; otherwise false</returns>
        public static bool operator !=(CalendarDateRange range1, CalendarDateRange range2)
        {
            return !(range1 == range2);
        }
        #endregion //Operator Overloads

        #region ToString
        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string that represents this <see cref="CalendarDateRange"/></returns>
        public override string ToString()
        {
            return this.ToString(CultureInfo.CurrentCulture);
        }
        #endregion //ToString

        #endregion //Base class overrides

        #region Properties
        /// <summary>
        /// Returns or sets the earliest/start date for the range.
        /// </summary>
        public DateTime Start
        {
            get 
            {
                return this._start; 
            }
            set
            {
				// AS 1/8/10
                //this._start = value.Date;
				this._start = value;
            }
        }

        /// <summary>
        /// Returns or sets the latest/end date for the range.
        /// </summary>
        public DateTime End
        {
            get 
            {
                return this._end; 
            }
            set
            {
				// AS 1/8/10
				//this._end = value.Date;
                this._end = value;
            }
        } 
        #endregion //Properties

        #region Methods

        #region Public

        /// <summary>
        /// Indicates if the specified date falls within the <see cref="Start"/> and <see cref="End"/> dates for this range.
        /// </summary>
        /// <param name="date">The date to evaluate</param>
        /// <returns>True if the specified date is greater than or equal to the <see cref="Start"/> and on or before the <see cref="End"/>.</returns>
        public bool Contains(DateTime date)
        {
			// AS 1/8/10
			//return date >= this._start && date <= this._end;
			CalendarDateRange src = Normalize(this);

			return date >= src._start && date <= src._end;
        }

        /// <summary>
        /// Indicates if the dates of the specified range fall completely within the dates of this range instance.
        /// </summary>
        /// <param name="range">The range to evaluate</param>
        /// <returns>True if the start and end date fall entirely within or equal to the start and end of this range instance.</returns>
        public bool Contains(CalendarDateRange range)
        {
            Utils.ValidateNull("range", range);

			// AS 1/8/10
			//return range._start >= this._start && range._end <= this._end;
			range.Normalize();
			CalendarDateRange src = Normalize(this);

            return range._start >= src._start && range._end <= src._end;
        }

        /// <summary>
        /// Indicates if the <see cref="Start"/> and <see cref="End"/> of the specified <see cref="CalendarDateRange"/> intersects with this object's dates.
        /// </summary>
        /// <param name="range">The range to evaluate.</param>
        /// <returns>True if any date within the specified range overlaps with the start/end of this range.</returns>
        /// <exception cref="ArgumentNullException">The 'range' cannot be null.</exception>
        public bool IntersectsWith(CalendarDateRange range)
        {
            Utils.ValidateNull("range", range);

			// AS 1/8/10
			// We need to normalize the values because the end may have been before the start.
			//
			range.Normalize();
			CalendarDateRange src = Normalize(this);

            // they intersect as long as this one doesn't end before
            // or start after the specified one 
			// AS 1/8/10
			//return !(this._end < range._start || this._start > range._end); 
			return !(src._end < range._start || src._start > range._end);
        }

		// AS 1/8/10
		// Changed from internal and removed stripping the time and put 
		// that into a separate function - RemoveTime.
		//
		#region Normalize
		/// <summary>
		/// Ensures that the start date is less than or equal to the end date.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void Normalize()
		{
			if (this._end < this._start)
			{
				DateTime other = this._start;
				this._start = this._end;
				this._end = other;
			}

			// AS 1/8/10
			// This was moved into a separate method.
			//
			//this._start = this._start.Date;
			//this._end = this._end.Date;
		}

		/// <summary>
		/// Returns a CalendarDateRange where the <see cref="Start"/> is less than or equal to the end time
		/// </summary>
		/// <param name="range">The range to normalize</param>
		public static CalendarDateRange Normalize(CalendarDateRange range)
		{
			range.Normalize();
			return range;
		}
		#endregion //Normalize

		// AS 1/8/10
		#region RemoveTime
		/// <summary>
		/// Removes the time portion from the <see cref="Start"/> and <see cref="End"/>
		/// </summary>
		public void RemoveTime()
		{
			this._start = this._start.Date;
			this._end = this._end.Date;
		}
		#endregion //RemoveTime

        #endregion //Public

        #region Internal
        internal static CalendarDateRange FromString(string value, CultureInfo culture)
        {
            culture = culture ?? CultureInfo.CurrentCulture;

            string[] values = value.Split('-');

            if (values.Length >= 1 && values.Length <= 2)
            {
                bool hasEmptyValues = false;

                // AS 10/17/08
                // If one of the values is an empty string we will generate an exception which
                // will not provide meaningful information - like an IndexOutOfRangeException.
                //
                foreach (string v in values)
                {
                    if (string.IsNullOrEmpty(v) || v.Trim().Length == 0)
                    {
                        hasEmptyValues = true;
                        break;
                    }
                }

                if (false == hasEmptyValues)
                {
                    TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(DateTime));

                    DateTime start = (DateTime)intConverter.ConvertFromString(null, culture, values[0]);
                    DateTime end = values.Length == 2
                        ? (DateTime)intConverter.ConvertFromString(null, culture, values[1])
                        : start;

                    return new CalendarDateRange(start, end);
                }
            }

            throw new FormatException(GetString("LE_InvalidDateRangeString", value));
        }

		#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
		#endregion // GetString

        internal string ToString(CultureInfo culture)
        {
            culture = culture ?? CultureInfo.CurrentCulture;
            const string sep = "-";

			// AS 1/8/10
			// This wasn't accounting for a range that had time.
			//
			//const string format = "d";
			//
			//if (this._start == this._end)
			//    return this._start.ToString(format, culture);
			//else
			//    return this._start.ToString(format, culture) + sep + this._end.ToString(format, culture);
			bool hasTime = _start.TimeOfDay != TimeSpan.Zero || _end.TimeOfDay != TimeSpan.Zero;
			string format = hasTime ? "r" : "d";
			DateTime start = _start;
			DateTime end = _end;

			// since we are using round trip formatting we need to convert to utc first
			if (hasTime)
			{
				start = start.ToUniversalTime();
				end = end.ToUniversalTime();
			}

            if (start == end)
                return start.ToString(format, culture);
            else
                return start.ToString(format, culture) + sep + end.ToString(format, culture);
        } 
        #endregion //Internal 

        #endregion //Methods

        #region IEquatable<CalendarDateRange> Members

        /// <summary>
        /// Compares two <see cref="CalendarDateRange"/>
        /// </summary>
        /// <param name="other">The object to compare to this instance</param>
        /// <returns></returns>
        public bool Equals(CalendarDateRange other)
        {
            return this == other;
        }

        #endregion //IEquatable<CalendarDateRange> Members

        #region IComparable<CalendarDateRange> Members

        /// <summary>
        /// Compares this instance to the specified <see cref="CalendarDateRange"/>
        /// </summary>
        /// <param name="other">The range to compare</param>
        /// <returns>A signed number indicating the relative values of this and the specified range.</returns>
        public int CompareTo(CalendarDateRange other)
        {
            int comparison = this._start.CompareTo(other._start);

            if (comparison == 0)
                comparison = this._end.CompareTo(other._end);

            return comparison;
        }

        #endregion //IComparable<CalendarDateRange> Members
    }

    #region CalendarDateRangeConverter
    /// <summary>
    /// Type converter for the <see cref="CalendarDateRange"/> structure
    /// </summary>
    public class CalendarDateRangeConverter : TypeConverter
    {
        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="CalendarDateRangeConverter"/>
        /// </summary>
        public CalendarDateRangeConverter()
        {
        }
        #endregion //Constructor

        #region Base class overrides

        /// <summary>
        /// Returns true if the object can convert from the type.
        /// </summary>
        /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="sourceType"> A <see cref="Type"/> that represents the type you want to convert from.</param>
        /// <returns>True if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Returns true if the object can convert to that type.
        /// </summary>
        /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="destinationType"> A <see cref="Type"/> that represents the type you want to convert to.</param>
        /// <returns>True if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) ||
                destinationType == typeof(InstanceDescriptor);
        }

        /// <summary>
        /// Converts from one type to another.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="System.Globalization.CultureInfo"/> object. If null is passed, the current culture is assumed.</param>        
        /// <param name="value">The object to convert.</param>
        /// <returns>An object that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Utils.ValidateNull("value", value);

            string strValue = value as string;

            if (null == strValue)
                GetConvertFromException(value);

            return CalendarDateRange.FromString(strValue, culture);
        }

        /// <summary>
        /// Converts the object to the requested type.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="System.Globalization.CultureInfo"/> object. If null is passed, the current culture is assumed.</param>
        /// <param name="destinationType">A <see cref="Type"/> that represents the type you want to convert to.</param>
        /// <param name="value">The object to convert.</param>
        /// <returns>An object that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            Utils.ValidateNull("value", value);
            Utils.ValidateNull("destinationType", destinationType);

            if (value is CalendarDateRange == false)
                GetConvertToException(value, destinationType);

            CalendarDateRange range = (CalendarDateRange)value;

            if (destinationType == typeof(string))
                return range.ToString(culture);

            if (destinationType != typeof(InstanceDescriptor))
                throw new ArgumentException(CalendarDateRange.GetString("LE_CannotConvertType", typeof(CalendarDateRange), destinationType.FullName));

            if (range.Start == range.End)
                return new InstanceDescriptor(typeof(CalendarDateRange).GetConstructor(new Type[] { typeof(DateTime) }), new object[] { range.Start });

            return new InstanceDescriptor(typeof(CalendarDateRange).GetConstructor(new Type[] { typeof(DateTime), typeof(DateTime) }), new object[] { range.Start, range.End });
        }

        #endregion //Base class overrides
    }
    #endregion //CalendarDateRangeConverter
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