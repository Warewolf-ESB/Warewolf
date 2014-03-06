using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// Custom value converter that returns the <see cref="DestinationValue"/> if the <see cref="SourceValue"/> matches the value provided
	/// </summary>
	public class FixedValueConverter : IValueConverter
	{
		#region Member Variables

		private static readonly object UnsetValue = new object(); 

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="FixedValueConverter"/>
		/// </summary>
		public FixedValueConverter()
		{
			// default to an object that we can identify as having no source.
			// we'll special case this so it can be used as a fallback and always return the destination value
			this.SourceValue = UnsetValue;
		} 
		#endregion // Constructor

		#region Properties
		/// <summary>
		/// The value that is compared against the value provided to the Convert method and also the value potentially returned from the ConvertBack.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> If this value is not set the <see cref="DestinationValue"/> is always returned from the convert method.</p>
		/// </remarks>
		public object SourceValue
		{
			get;
			set;
		}

		/// <summary>
		/// The value returned from the Convert if the SourceValue matches and the value compared against the value in the ConvertBack.
		/// </summary>
		public object DestinationValue
		{
			get;
			set;
		} 
		#endregion // Properties

		#region Methods
		private static object ConvertHelper(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture, object source, object destination)
		{
			// if there is no destination value set then always bail
			if (destination == UnsetValue)
				return DependencyProperty.UnsetValue;

			// if the source wasn't set then always return the destination
			if (source == UnsetValue)
				return destination;

			if (object.Equals(source, value))
				return destination;

			return DependencyProperty.UnsetValue;
		}
		#endregion // Methods

		#region IValueConverter Members

		/// <summary>
		/// Returns the <see cref="DestinationValue"/> value if the <paramref name="value"/> equals the <see cref="SourceValue"/>
		/// </summary>
		/// <param name="value">The value to compare to the <see cref="SourceValue"/></param>
		/// <param name="targetType">The type of the target property</param>
		/// <param name="parameter">The parameter to use for the conversion. This parameter is not used.</param>
		/// <param name="culture">The culture to use for the conversion. This parameter is not used.</param>
		/// <returns>The <see cref="DestinationValue"/> or DependencyProperty.UnsetValue if the <see cref="SourceValue"/> doesn't match the <paramref name="value"/></returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return ConvertHelper(value, targetType, parameter, culture, this.SourceValue, this.DestinationValue);
		}

		/// <summary>
		/// Returns the <see cref="SourceValue"/> if the <paramref name="value"/> equals the <see cref="DestinationValue"/>
		/// </summary>
		/// <param name="value">The value to compare to the <see cref="DestinationValue"/></param>
		/// <param name="targetType">The type of the target property</param>
		/// <param name="parameter">The parameter to use for the conversion. This parameter is not used.</param>
		/// <param name="culture">The culture to use for the conversion. This parameter is not used.</param>
		/// <returns>The <see cref="SourceValue"/> or DependencyProperty.UnsetValue if the <see cref="DestinationValue"/> doesn't match the <paramref name="value"/></returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return ConvertHelper(value, targetType, parameter, culture, this.DestinationValue, this.SourceValue);
		}

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