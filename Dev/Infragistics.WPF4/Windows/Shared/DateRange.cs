using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;


using System.ComponentModel.Design.Serialization;






namespace Infragistics

{
    /// <summary>
    /// Structure used to represents a range of dates.
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    [TypeConverter( typeof( DateRangeConverter ) )]
    public struct DateRange : IEquatable<DateRange>,
        IComparable<DateRange>
    {
        #region Member Variables

        private DateTime _start;
        private DateTime _end;

		// JJD 3/28/11
		// Made public
        //internal static readonly DateRange Infinite = new DateRange( DateTime.MinValue, DateTime.MaxValue );
        private static readonly DateRange _Infinite = new DateRange( DateTime.MinValue, DateTime.MaxValue );
 		/// <summary>
		/// Returns a data range from DateTime.MinValue to DateTime.MaxValue
		/// </summary>
		public static DateRange Infinite { get { return _Infinite; } }

        #endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="DateRange"/>
        /// </summary>
        /// <param name="date">The date to use for the start and end dates.</param>
        public DateRange( DateTime date )
            : this( date, date )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DateRange"/>
        /// </summary>
        /// <param name="start">The start date</param>
        /// <param name="end">The end date</param>
        public DateRange( DateTime start, DateTime end )
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
        public override int GetHashCode( )
        {
            return this._start.GetHashCode( ) ^ this._end.GetHashCode( );
        }
        #endregion //GetHashCode

        #region Equals
        /// <summary>
        /// Compares the specified object to this object to see if they are equivalent.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the objects are equal; otherwise false</returns>
        public override bool Equals( object obj )
        {
            if ( obj is DateRange )
            {
                return this == (DateRange)obj;
            }

            return false;
        }
        #endregion //Equals

        #region Operator Overloads
        /// <summary>
        /// Compares the values of two <see cref="DateRange"/> structures for equality
        /// </summary>
        /// <param name="range1">The first structure</param>
        /// <param name="range2">The other structure</param>
        /// <returns>true if the two instances are equal; otherwise false</returns>
        public static bool operator ==( DateRange range1, DateRange range2 )
        {
            return range1._start == range2._start && range1._end == range2._end;
        }

        /// <summary>
        /// Compares the values of two <see cref="DateRange"/> structures for inequality
        /// </summary>
        /// <param name="range1">The first structure</param>
        /// <param name="range2">The other structure</param>
        /// <returns>true if the two instances are not equal; otherwise false</returns>
        public static bool operator !=( DateRange range1, DateRange range2 )
        {
            return !( range1 == range2 );
        }
        #endregion //Operator Overloads

        #region ToString
        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string that represents this <see cref="DateRange"/></returns>
        public override string ToString( )
        {
            return this.ToString( null );
        }

		/// <summary>
		/// Returns a string representation of the range using the specified format to format the <see cref="Start"/> and <see cref="End"/>
		/// </summary>
		/// <param name="format">The format used to format the Start and End dates.</param>
		/// <returns>A string that represents this <see cref="DateRange"/></returns>
		public string ToString(string format)
		{
            return this.ToString( CultureInfo.CurrentCulture, format, false );
		}
        #endregion //ToString

        #endregion //Base class overrides

        #region Properties

		#region IsEmpty

		
		
		
		/// <summary>
		/// Returns true if the Start and End are the same values.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return _start == _end;
			}
		}

		#endregion // IsEmpty

        /// <summary>
        /// Returns or sets the earliest/start date for the range.
		/// </summary>
		// JJD 11/9/11 - TFS95711 - added type converter in SL so this property can be set in xaml to a string



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
		// JJD 11/9/11 - TFS95711 - added type converter in SL so this property can be set in xaml to a string



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
        public bool Contains( DateTime date )
        {
            // AS 1/8/10
            //return date >= this._start && date <= this._end;
            DateRange src = Normalize( this );

            return date >= src._start && date <= src._end;
        }

        /// <summary>
        /// Indicates if the dates of the specified range fall completely within the dates of this range instance.
        /// </summary>
        /// <param name="range">The range to evaluate</param>
        /// <returns>True if the start and end date fall entirely within or equal to the start and end of this range instance.</returns>
        public bool Contains( DateRange range )
        {
            // AS 1/8/10
            //return range._start >= this._start && range._end <= this._end;
            range.Normalize( );
            DateRange src = Normalize( this );

            return range._start >= src._start && range._end <= src._end;
        }

        /// <summary>
        /// Attempts to update the <see cref="Start"/> and <see cref="End"/> if the specified range intersects with this instance
        /// </summary>
        /// <param name="range">The range to intersect</param>
        /// <returns>True if the ranges intersect; false if the ranges do not intersect.</returns>
        /// <remarks>
        /// <p class="body">If the specified range does not intersect then this instance's Start and End will not be changed. If they 
        /// do intersect then the Start and End will be updated to reflect the intersection of the normalized two ranges.</p>
        /// </remarks>
        public bool Intersect( DateRange range )
        {
            range.Normalize( );
            DateRange src = Normalize( this );

            // if they don't intersect then we cannot perform an operation
            if ( src._end < range._start || src._start > range._end )
                return false;

            if ( range._start > src._start )
                _start = range._start;
            else
                _start = src._start;

            if ( range._end < src._end )
                _end = range._end;
            else
                _end = src._end;

            return true;
        }

        /// <summary>
        /// Indicates if the <see cref="Start"/> and <see cref="End"/> of the specified <see cref="DateRange"/> intersects with this object's dates.
        /// </summary>
        /// <param name="range">The range to evaluate.</param>
        /// <returns>True if any date within the specified range overlaps with the start/end of this range.</returns>
        /// <exception cref="ArgumentNullException">The 'range' cannot be null.</exception>
        public bool IntersectsWith( DateRange range )
        {
            //Utils.ValidateNull("range", range);

            // AS 1/8/10
            // We need to normalize the values because the end may have been before the start.
            //
            range.Normalize( );
            DateRange src = Normalize( this );

            // they intersect as long as this one doesn't end before
            // or start after the specified one 
            // AS 1/8/10
            //return !(this._end < range._start || this._start > range._end); 
            return !( src._end < range._start || src._start > range._end );
        }

		#region Offset
		/// <summary>
		/// Moves the date range by the specified offset.
		/// </summary>
		/// <param name="offset">The timespan used to adjust the <see cref="Start"/> and <see cref="End"/> dates</param>
		public void Offset(TimeSpan offset)
		{
			_start = _start.Add(offset);
			_end = _end.Add(offset);
		}
		#endregion // Offset

        // AS 1/8/10
        // Changed from internal and removed stripping the time and put 
        // that into a separate function - RemoveTime.
        //
        #region Normalize
        /// <summary>
        /// Ensures that the start date is less than or equal to the end date.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Advanced )]
        public void Normalize( )
        {
            if ( this._end < this._start )
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
        /// Returns a DateRange where the <see cref="Start"/> is less than or equal to the end time
        /// </summary>
        /// <param name="range">The range to normalize</param>
        public static DateRange Normalize( DateRange range )
        {
            range.Normalize( );
            return range;
        }
        #endregion //Normalize

        // AS 1/8/10
        #region RemoveTime
        /// <summary>
        /// Removes the time portion from the <see cref="Start"/> and <see cref="End"/>
        /// </summary>
        public void RemoveTime( )
        {
            this._start = this._start.Date;
            this._end = this._end.Date;
        }
        #endregion //RemoveTime

        #endregion //Public

        #region Internal

		#region CombineRanges
		internal static DateRange[] CombineRanges(ICollection<DateRange> ranges)
		{
			// copy them over
			DateRange[] sortedRanges = new DateRange[ranges.Count];
			ranges.CopyTo(sortedRanges, 0);

			if (sortedRanges.Length < 2)
				return sortedRanges;

			// ensure the start is before the end...
			for (int i = 0; i < sortedRanges.Length; i++)
				sortedRanges[i].Normalize();

			// ensure they are in order
			Array.Sort(sortedRanges);

			int destIndex = -1;

			// now combine them so we don't have duplicate times
			for (int sourceIndex = 0; sourceIndex < sortedRanges.Length; sourceIndex++)
			{
				DateRange source = sortedRanges[sourceIndex];

				if (destIndex >= 0 && source.Start <= sortedRanges[destIndex].End)
				{
					// if its not within the current range then update the end of that range
					if (source.End > sortedRanges[destIndex].End)
						sortedRanges[destIndex].End = source.End;
				}
				else
				{
					// create a new slot that starts with this item
					destIndex++;
					sortedRanges[destIndex] = source;
				}
			}

			// trim off the excess
			Array.Resize(ref sortedRanges, destIndex + 1);

			return sortedRanges;
		}
		#endregion // CombineRanges

		/// <summary>
		/// Indicates if the specified date falls within the <see cref="Start"/> and <see cref="End"/> dates for this range.
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>True if the specified date is greater than or equal to the <see cref="Start"/> and before the <see cref="End"/>.</returns>
		internal bool ContainsExclusive(DateTime date)
		{
			Debug.Assert(_end >= _start, "The range is not normalized!");
			return date >= _start && date < _end;
		}

		// AS 3/13/12 TFS104664
		internal bool Intersect(DateRange range, bool normalizeIntersection)
		{
			bool wasInverted = !normalizeIntersection && _start > _end;

			if (!this.Intersect(range))
				return false;

			if (wasInverted)
				CoreUtilities.Swap(ref _start, ref _end);

			return true;
		}

		/// <summary>
		/// Indicates if the <see cref="Start"/> and <see cref="End"/> of the specified <see cref="DateRange"/> intersects with this object's dates.
		/// </summary>
		/// <param name="range">The range to evaluate.</param>
		/// <returns>True if any date within the specified range overlaps with the start/end of this range considering the End of the ranges as exclusive.</returns>
		/// <exception cref="ArgumentNullException">The 'range' cannot be null.</exception>
		internal bool IntersectsWithExclusive(DateRange range)
		{
			range.Normalize();
			DateRange src = Normalize(this);

			return !(src._end <= range._start || src._start >= range._end);
		}


        internal static DateRange FromString( string value, CultureInfo culture )
        {
            culture = culture ?? CultureInfo.CurrentCulture;

            string[] values = value.Split( '-' );

            if ( values.Length >= 1 && values.Length <= 2 )
            {
                bool hasEmptyValues = false;

                // AS 10/17/08
                // If one of the values is an empty string we will generate an exception which
                // will not provide meaningful information - like an IndexOutOfRangeException.
                //
                foreach ( string v in values )
                {
                    if ( string.IsNullOrEmpty( v ) || v.Trim( ).Length == 0 )
                    {
                        hasEmptyValues = true;
                        break;
                    }
                }

                if ( false == hasEmptyValues )
                {

                    TypeConverter dateConverter = TypeDescriptor.GetConverter( typeof( DateTime ) );




                    DateTime start = (DateTime)dateConverter.ConvertFrom( null, culture, values[0] );
                    DateTime end = values.Length == 2
                        ? (DateTime)dateConverter.ConvertFrom( null, culture, values[1] )
                        : start;

                    return new DateRange( start, end );
                }
            }

			throw new FormatException(GetString("LE_InvalidDateRangeString", value)); 
        }

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args); 
#pragma warning restore 436
		}

        internal string ToString( CultureInfo culture, string format, bool allowConvertToUniversal )
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

			if (null == format)
	            format = hasTime ? "r" : "d";

            DateTime start = _start;
            DateTime end = _end;

            // since we are using round trip formatting we need to convert to utc first
            if ( hasTime && allowConvertToUniversal )
            {
                start = start.ToUniversalTime( );
                end = end.ToUniversalTime( );
            }

            if ( start == end )
                return start.ToString( format, culture );
            else
                return start.ToString( format, culture ) + sep + end.ToString( format, culture );
        }
        #endregion //Internal

        #endregion //Methods

        #region IEquatable<DateRange> Members

        /// <summary>
        /// Compares two <see cref="DateRange"/>
        /// </summary>
        /// <param name="other">The object to compare to this instance</param>
        /// <returns></returns>
        public bool Equals( DateRange other )
        {
            return this == other;
        }

        #endregion //IEquatable<DateRange> Members

        #region IComparable<DateRange> Members

        /// <summary>
        /// Compares this instance to the specified <see cref="DateRange"/>
        /// </summary>
        /// <param name="other">The range to compare</param>
        /// <returns>A signed number indicating the relative values of this and the specified range.</returns>
        public int CompareTo( DateRange other )
        {
            int comparison = this._start.CompareTo( other._start );

            if ( comparison == 0 )
                comparison = this._end.CompareTo( other._end );

            return comparison;
        }

        #endregion //IComparable<DateRange> Members
	}

    #region DateRangeConverter
    /// <summary>
    /// Type converter for the <see cref="DateRange"/> structure
    /// </summary>
    public class DateRangeConverter : TypeConverter
    {
        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="DateRangeConverter"/>
        /// </summary>
        public DateRangeConverter( )
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
        public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
        {
            if ( sourceType == typeof( string ) )
                return true;

            return base.CanConvertFrom( context, sourceType );
        }

        /// <summary>
        /// Returns true if the object can convert to that type.
        /// </summary>
        /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="destinationType"> A <see cref="Type"/> that represents the type you want to convert to.</param>
        /// <returns>True if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType )
        {
            if ( destinationType == typeof( string ) )
                return true;


            if ( destinationType == typeof( InstanceDescriptor ) )
                return true;


            return false;
        }



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Converts from one type to another.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="System.Globalization.CultureInfo"/> object. If null is passed, the current culture is assumed.</param>        
        /// <param name="value">The object to convert.</param>
        /// <returns>An object that represents the converted value.</returns>
        public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value )
        {
            CoreUtilities.ValidateNotNull( value, "value" );

            string strValue = value as string;

            if ( null == strValue )
                GetConvertFromException( value );

            return DateRange.FromString( strValue, culture );
        }

        /// <summary>
        /// Converts the object to the requested type.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="System.Globalization.CultureInfo"/> object. If null is passed, the current culture is assumed.</param>
        /// <param name="destinationType">A <see cref="Type"/> that represents the type you want to convert to.</param>
        /// <param name="value">The object to convert.</param>
        /// <returns>An object that represents the converted value.</returns>
		[System.Security.SecuritySafeCritical] // AS 10/17/11 TFS89764
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            CoreUtilities.ValidateNotNull( value, "value" );
            CoreUtilities.ValidateNotNull( destinationType, "destinationType" );

            if ( value is DateRange == false )
                GetConvertToException( value, destinationType );

            DateRange range = (DateRange)value;

            if ( destinationType == typeof( string ) )
                return range.ToString( culture, null, true );


			if (destinationType == typeof(InstanceDescriptor))
			{
				if (range.IsEmpty)
					return new InstanceDescriptor(typeof(DateRange).GetConstructor(new Type[] { typeof(DateTime) }), new object[] { range.Start });

				return new InstanceDescriptor(typeof(DateRange).GetConstructor(new Type[] { typeof(DateTime), typeof(DateTime) }), new object[] { range.Start, range.End });
			}


            throw new ArgumentException( DateRange.GetString( "LE_CannotConvertType", typeof( DateRange ), destinationType.FullName ) );
        }

        #endregion //Base class overrides
    }
    #endregion //DateRangeConverter
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