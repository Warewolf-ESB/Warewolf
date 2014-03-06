using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.ComponentModel;


using System.ComponentModel.Design.Serialization;

//using Infragistics.Shared;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// Structure used to indicate the preferred rows/columns of <see cref="CalendarItemGroup"/> instances within a <see cref="CalendarBase"/>
    /// </summary>
    /// <seealso cref="CalendarBase.Dimensions"/>
    /// <seealso cref="CalendarBase.AutoAdjustDimensions"/>
    /// <seealso cref="CalendarItemGroupPanel"/>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(CalendarDimensionsConverter))]
    public struct CalendarDimensions : IEquatable<CalendarDimensions>
    {
        #region Member Variables

        private int _rows;
        private int _columns;

        #endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="CalendarDimensions"/>
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <exception cref="ArgumentOutOfRangeException">The rows and columns must be 0 or greater.</exception>
        public CalendarDimensions(int rows, int columns)
        {
            if (rows < 0 || columns < 0)
				throw new ArgumentOutOfRangeException(CalendarBase.GetString("LE_InvalidCalendarDimensions"));

            this._rows = rows;
            this._columns = columns;
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
            return this._columns.GetHashCode() ^ this._rows.GetHashCode();
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
            if (obj is CalendarDimensions)
            {
                return this == (CalendarDimensions)obj;
            }

            return false;
        }
        #endregion //Equals

        #region Operator Overloads
        /// <summary>
        /// Compares the values of two <see cref="CalendarDimensions"/> structures for equality
        /// </summary>
        /// <param name="item1">The first structure</param>
        /// <param name="item2">The other structure</param>
        /// <returns>true if the two instances are equal; otherwise false</returns>
        public static bool operator ==(CalendarDimensions item1, CalendarDimensions item2)
        {
            return item1._rows == item2._rows && item1._columns == item2._columns;
        }

        /// <summary>
        /// Compares the values of two <see cref="CalendarDimensions"/> structures for inequality
        /// </summary>
        /// <param name="item1">The first structure</param>
        /// <param name="item2">The other structure</param>
        /// <returns>true if the two instances are not equal; otherwise false</returns>
        public static bool operator !=(CalendarDimensions item1, CalendarDimensions item2)
        {
            return !(item1 == item2);
        } 
        #endregion //Operator Overloads

        #region ToString
        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string that represents this <see cref="CalendarDimensions"/></returns>
        public override string ToString()
        {
            return this.ToString(CultureInfo.CurrentCulture);
        }
        #endregion //ToString

        #endregion //Base class overrides

        #region Properties

        #region Columns
        /// <summary>
        /// Returns or sets the number of columns of groups.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value must be 0 or greater.</exception>
        public int Columns
        {
            get { return this._columns; }
            set
            {
                if (this._columns != value)
                {
                    if (value < 0)
						throw new ArgumentOutOfRangeException(CalendarBase.GetString("LE_InvalidCalendarDimensions"));

                    this._columns = value;
                }
            }
        }
        #endregion //Columns

        #region Rows
        /// <summary>
        /// Returns or sets the number of rows of groups.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value must be 0 or greater.</exception>
        public int Rows
        {
            get { return this._rows; }
            set
            {
                if (this._rows != value)
                {
                    if (value < 0)
						throw new ArgumentOutOfRangeException(CalendarBase.GetString("LE_InvalidCalendarDimensions"));

                    this._rows = value;
                }
            }
        }
        #endregion //Rows

        #endregion //Properties

        #region Methods
        internal static CalendarDimensions FromString(string value, CultureInfo culture)
        {
            culture = culture ?? CultureInfo.CurrentCulture;

            string[] values = value.Split(new string[] {culture.TextInfo.ListSeparator}, StringSplitOptions.None);

            if (values.Length == 2 && values[0].Trim().Length > 0 && values[1].Trim().Length > 0)
            {

                TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));




                int rows = (int)intConverter.ConvertFrom(null, culture, values[0]);
                int cols = (int)intConverter.ConvertFrom(null, culture, values[1]);

                return new CalendarDimensions(rows, cols);
            }

			throw new FormatException(CalendarBase.GetString("LE_InvalidDimensionsString", value));
        }

        internal string ToString(CultureInfo culture)
        {
            culture = culture ?? CultureInfo.CurrentCulture;
            string sep = culture.TextInfo.ListSeparator;

            // AS 10/1/08 TFS8398
            //return this._columns.ToString(culture) + sep + this._rows.ToString(culture);
            return this._rows.ToString(culture) + sep + this._columns.ToString(culture);
        } 
        #endregion //Methods 

        #region IEquatable<CalendarDimensions> Members

        /// <summary>
        /// Compares two <see cref="CalendarDimensions"/>
        /// </summary>
        /// <param name="other">The object to compare to this instance</param>
        /// <returns></returns>
        public bool Equals(CalendarDimensions other)
        {
            return this == other;
        }

        #endregion //IEquatable<CalendarDimensions> Members
    }

    
    
    
    
    #region CalendarDimensionsConverter
    /// <summary>
    /// Type converter for the <see cref="CalendarDimensions"/> structure
    /// </summary>
    public class CalendarDimensionsConverter : TypeConverter
    {
        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="CalendarDimensionsConverter"/>
        /// </summary>
        public CalendarDimensionsConverter()
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
            if ( destinationType == typeof(string) )
				return true;


			if (destinationType == typeof(InstanceDescriptor))
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
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            CalendarUtilities.ValidateNull("value", value);

            string strValue = value as string;

            if (null == strValue)
                GetConvertFromException(value);

            return CalendarDimensions.FromString(strValue, culture);
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
            CalendarUtilities.ValidateNull("value", value);
            CalendarUtilities.ValidateNull("destinationType", destinationType);

            if (value is CalendarDimensions == false)
                GetConvertToException(value, destinationType);

            CalendarDimensions dims = (CalendarDimensions)value;

            if (destinationType == typeof(string))
                return dims.ToString(culture);


			if (destinationType == typeof(InstanceDescriptor))
				return new InstanceDescriptor(typeof(CalendarDimensions).GetConstructor(new Type[] { typeof(int), typeof(int) }), new object[] { dims.Rows, dims.Columns });


            throw new ArgumentException(CalendarBase.GetString("LE_CannotConvertType", typeof(CalendarDimensions), destinationType.FullName));
        }

        #endregion //Base class overrides
    }
    #endregion //CalendarDimensionsConverter
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