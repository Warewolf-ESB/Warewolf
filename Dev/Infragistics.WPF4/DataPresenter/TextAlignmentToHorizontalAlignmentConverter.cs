using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Windows;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Value converter for converting a <see cref="TextAlignment"/> to a <see cref="HorizontalAlignment"/> that is used by the LabelPresenter to correctly align the label text and SortIndicator based on the Field's LabelTextAlignment setting.
	/// </summary>
	public class TextAlignmentToHorizontalAlignmentConverter : IValueConverter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TextAlignmentToHorizontalAlignmentConverter"/>
		/// </summary>
		public TextAlignmentToHorizontalAlignmentConverter()
		{
		}
		#endregion //Constructor

		#region IValueConverter Members

		/// <summary>
		/// Converts a <see cref="TextAlignment"/> to a <see cref="HorizontalAlignment"/>.
		/// </summary>
		/// <param name="value">The TextAlignment value to be convertered to a HorizontalAlignment value.</param>
		/// <param name="targetType">The type the converter is to create. This must be a <see cref="HorizontalAlignment"/>.</param>
		/// <param name="parameter">No parameter is expected.</param>
		/// <param name="culture">This parameter is not used.</param>
		/// <returns>A </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// targetType must be a HorizontalAlignment
			if (targetType == null || typeof(HorizontalAlignment).IsAssignableFrom(targetType) == false)
				return DependencyProperty.UnsetValue;

			// the value must be a TextAlignment
			if (value is TextAlignment == false)
				return DependencyProperty.UnsetValue;

			TextAlignment ta = (TextAlignment)value;
			switch (ta)
			{
				case TextAlignment.Left :
					return HorizontalAlignment.Left;

				case TextAlignment.Center :
					return HorizontalAlignment.Center;

				case TextAlignment.Right :
					return HorizontalAlignment.Right;

				case TextAlignment.Justify :
					return HorizontalAlignment.Stretch;

				default :
					return HorizontalAlignment.Left;
			}
		}

		#region ConvertBack
		/// <summary>
		/// This method is not implemented for this converter.
		/// </summary>
		/// <returns><see cref="Binding.DoNothing"/> is always returned.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		}
		#endregion //ConvertBack

		#endregion //IValueConverter Members		
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