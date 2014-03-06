using System;
using System.ComponentModel;
using System.Globalization;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A Type Converter that converts from a string to a <see cref="ColumnWidth"/> object;
	/// </summary>
	public class ColumnWidthTypeConverter : TypeConverter
	{
		/// <summary>
		/// Determines whether this converter can convert an object of the specified type to a <see cref="ColumnWidth"/>.
		/// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="sourceType">The type that you want to convert from.</param>
		/// <returns>true if sourceType is a <see cref="String"/> or of the specified type.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType == typeof(string));
		}

		/// <summary>
		/// Converts the specified object to a <see cref="ColumnWidth"/>.
		/// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the currentIndex culture.</param>
        /// <param name="value">The object to covert to the <see cref="ColumnWidth"/>.</param>
		/// <returns></returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				string val = value.ToString().ToUpper(CultureInfo.InvariantCulture);

				// First Test to see if this is a numeric column
				double outVal = -1;
				if (double.TryParse(val, out outVal))
					return new ColumnWidth(outVal, false);

				// Then check if its a star column via the #* notation.
				if (val.IndexOf('*') != -1)
				{
					val = val.Replace('*', ' ');

					if (double.TryParse(val, out outVal))
						return new ColumnWidth(outVal, true);
					else
						return ColumnWidth.Star;
				}

				// Finally validate if its an Enum val. 
				if (val == "AUTO")
					return ColumnWidth.Auto;
				if (val == "SIZETOCELLS")
					return ColumnWidth.SizeToCells;
				if (val == "SIZETOHEADER")
					return ColumnWidth.SizeToHeader;
                if (val == "INITIALAUTO")
                    return ColumnWidth.InitialAuto;
				if (val == "STAR")
					return ColumnWidth.Star;
			}

			return null;
		}

		/// <summary>
		/// Allows the ColumnWidth property to be converted to Strings.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		/// <summary>
		/// Converts the ColumnWidth object to a String. 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <param name="destinationType"></param>
		/// <returns>A string.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			string retVal = "";
			if (value != null)
			{
				ColumnWidth cw = (ColumnWidth)value;

				if (cw.WidthType == ColumnWidthType.Numeric)
					retVal = cw.Value.ToString();
				else if (cw.WidthType == ColumnWidthType.Star)
					retVal = cw.Value.ToString() + "*";
				else
					retVal = cw.WidthType.ToString();
			}
			return retVal;
		}
	}

    /// <summary>
    /// A Type Converter that converts from a string to a <see cref="Nullable{ColumnWidth}"/> object;
    /// </summary>
    public class NullableColumnWidthTypeConverter : ColumnWidthTypeConverter
    {
        /// <summary>
        /// Converts the specified object to a <see cref="ColumnWidth"/>.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the currentIndex culture.</param>
        /// <param name="value">The object to covert to the <see cref="ColumnWidth"/>.</param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            object returnValue = base.ConvertFrom(context, culture, value);

            if (returnValue != null)
            {
                return (ColumnWidth?)returnValue;
            }

            return null;
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