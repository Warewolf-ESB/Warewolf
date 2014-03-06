using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.ComponentModel;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// Custom value converter where the results are based upon the value converters in the <see cref="Converters"/> collection
	/// </summary>
	/// <remarks>
	/// <p class="body">This value converter will enuerate the converters in the <see cref="Converters"/> collection when its 
	/// Convert and ConvertBack method are invoked. The first converter that returns a value other than DependencyProperty.UnsetValue will 
	/// be returned from the method.</p>
	/// </remarks>
	[ContentProperty("Converters")]
	public class ValueConverterGroup : IValueConverter
	{
		#region Member Variables

		private List<IValueConverter> _converters; 
		
		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ValueConverterGroup"/>
		/// </summary>
		public ValueConverterGroup()
		{
			_converters = new List<IValueConverter>();
		} 
		#endregion // Constructor

		#region Properties
		/// <summary>
		/// The collection of converters that will be used to convert the value.
		/// </summary>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

		public List<IValueConverter> Converters
		{
			get { return _converters; }
		} 
		#endregion // Properties

		#region IValueConverter Members

		/// <summary>
		/// Returns the result of the first converter in the <see cref="Converters"/> collection that returns a value for the specified parameters.
		/// </summary>
		/// <param name="value">The value to convert</param>
		/// <param name="targetType">The destination type of the conversion</param>
		/// <param name="parameter">The parameter for the conversion</param>
		/// <param name="culture">The culture to use during the conversion</param>
		/// <returns>The result of the first converter to return a value; otherwise DependencyProperty.UnsetValue if there is no converted value.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			object result = DependencyProperty.UnsetValue;

			foreach (IValueConverter converter in _converters)
			{
				result = converter.Convert(value, targetType, parameter, culture);

				if (result != DependencyProperty.UnsetValue)
					break;
			}

			return result;
		}

		/// <summary>
		/// Returns the result of the first converter in the <see cref="Converters"/> collection that returns a value for the specified parameters.
		/// </summary>
		/// <param name="value">The value to convert</param>
		/// <param name="targetType">The destination type of the conversion</param>
		/// <param name="parameter">The parameter for the conversion</param>
		/// <param name="culture">The culture to use during the conversion</param>
		/// <returns>The result of the first converter to return a value; otherwise DependencyProperty.UnsetValue if there is no converted value.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			object result = DependencyProperty.UnsetValue;

			foreach (IValueConverter converter in _converters)
			{
				result = converter.ConvertBack(value, targetType, parameter, culture);

				if (result != DependencyProperty.UnsetValue)
					break;
			}

			return result;
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