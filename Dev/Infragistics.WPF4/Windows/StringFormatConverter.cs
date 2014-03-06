using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Value converter for invoking the <see cref="String.Format(IFormatProvider, string, object[])"/> method taking a specified format and one or more parameters.
	/// </summary>
	public sealed class StringFormatConverter : IMultiValueConverter
		// SSP 10/16/09 TFS19525
		// Implemented IValueConverter as well.
		// 
		, IValueConverter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="StringFormatConverter"/>
		/// </summary>
		public StringFormatConverter()
		{
		}
		#endregion //Constructor

		#region Convert
		/// <summary>
		/// Takes a series of values to use in a call to the <see cref="String.Format(IFormatProvider, string, object[])"/> to create a string.
		/// </summary>
		/// <param name="values">An array providing the arguments for the call to <see cref="String.Format(IFormatProvider, string, object[])"/>. If the first element is null or <see cref="DependencyProperty.UnsetValue"/>, the converter culture will be used.</param>
		/// <param name="targetType">The type to which the converter must be convert the value. This parameter is not used. A String value is always returned</param>
		/// <param name="parameter">The string to use as the format. If null, then the first value is assumed to be the format.</param>
		/// <param name="culture">The culture used as the IFormatProvider for the Format method call.</param>
		/// <returns><see cref="DependencyProperty.UnsetValue"/> if the parameters do not match the information. Otherwise a formatted string is returned.</returns>
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == null)
				return DependencyProperty.UnsetValue;

            if (parameter is string == false)
            {
                // AS 9/19/08
                // Since you cannot bind the parameter of a binding, I changed the implementation
                // of this converter to assume the first value is the format if the parameter
                // is not a string.
                //
                if (values.Length == 1 || values[0] is string == false)
                    return DependencyProperty.UnsetValue;

                parameter = values[0];
                object[] tempValues = new object[values.Length - 1];
                Array.Copy(values, 1, tempValues, 0, values.Length - 1);
                values = tempValues;
            }

			string format = (string)parameter;

			return string.Format(culture, format, values);
		}
		#endregion //Convert

		#region ConvertBack
		/// <summary>
		/// This method is not implemented for this converter.
		/// </summary>
		/// <returns><see cref="Binding.DoNothing"/> is always returned.</returns>
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return new object[] { Binding.DoNothing };
		}
		#endregion //ConvertBack

		// SSP 10/16/09 TFS19525
		// Implemented IValueConverter as well.
		// 
		#region IValueConverter Members

		object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return this.Convert( new object[] { value }, targetType, parameter, culture );
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return this.ConvertBack( value, new Type[] { targetType }, parameter, culture );
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