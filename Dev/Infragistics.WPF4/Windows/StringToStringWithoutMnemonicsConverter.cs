using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Value converter used to remove mnemonic characters from a string.
	/// </summary>
	public sealed class StringWithoutMnemonicsConverter : IValueConverter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="StringWithoutMnemonicsConverter"/>
		/// </summary>
		public StringWithoutMnemonicsConverter()
		{
		}
		#endregion //Constructor

		#region Convert

		/// <summary>
		/// Converts a given <see cref="String"/> value to a string that removes '_' characters which would normally be used to identify that the character following be rendered as a mnemonic.
		/// </summary>
		/// <param name="value">A string whose text is to be cleared of unescaped '_' characters. Any instances where 2 consecutive '_' exist in the string are replaced with a single '_' character.</param>
		/// <param name="targetType">The type to which the converter must be convert the value. This parameter is not used. A String value is always returned</param>
		/// <param name="parameter">This parameter is not used.</param>
		/// <param name="culture">The culture to use when calling the <see cref="Utilities.StripMnemonics(string, bool)"/>.</param>
		/// <returns><see cref="DependencyProperty.UnsetValue"/> if the parameters do not match the required information. Otherwise a string with any '_' characters removed. If the string contains a sequence of two consecutive '_' characters then a single '_' is left in the string.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string text = value as string;

			if (null == text)
				return DependencyProperty.UnsetValue;

			return Utilities.StripMnemonics(text, false);
		}
		#endregion //Convert

		#region ConvertBack
		/// <summary>
		/// This method is not implemented for this converter.
		/// </summary>
		/// <returns><see cref="Binding.DoNothing"/> is always returned.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return Binding.DoNothing;
		}

		#endregion //IValueConverter
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