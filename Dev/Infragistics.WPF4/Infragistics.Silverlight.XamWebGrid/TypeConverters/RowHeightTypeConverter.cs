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
using System.Globalization;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A Type Converter that converts from a string to a <see cref="RowHeight"/> object;
	/// </summary>
	public class RowHeightTypeConverter : TypeConverter
	{
		/// <summary>
		/// Determines whether this converter can convert an object of the specified type to a <see cref="RowHeight"/>.
		/// </summary>
		/// <param propertyName="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param propertyName="sourceType">The type that you want to convert from.</param>
		/// <returns>true if sourceType is a <see cref="String"/> or of the specified type.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType == typeof(string));
		}

		/// <summary>
		/// Converts the specified object to a <see cref="RowHeight"/>.
		/// </summary>
		/// <param propertyName="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param propertyName="culture">The <see cref="CultureInfo"/> to use as the currentIndex culture.</param>
		/// <param propertyName="value">The object to covert to the <see cref="RowHeight"/>.</param>
		/// <returns></returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				string val = value.ToString().ToUpper(CultureInfo.InvariantCulture);

				// First Test to see if this is a numeric column
				double outVal = -1;
				if (double.TryParse(val, out outVal))
					return new RowHeight(outVal);

				// Finally validate if its an Enum val. 
				if (val == "DYNAMIC")
					return RowHeight.Dynamic;
				if (val == "SIZETOLARGESTCELL")
					return RowHeight.SizeToLargestCell;
			}

			return null;
		}

        /// <summary>
        /// Allows the RowHeight property to be converted to Strings.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        /// <summary>
        /// Converts the RowHeight object to a String. 
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
                RowHeight rh = (RowHeight)value;

                if (rh.HeightType == RowHeightType.Numeric)
                    retVal = rh.Value.ToString();
                else
                    retVal = rh.HeightType.ToString();
            }
            return retVal;
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