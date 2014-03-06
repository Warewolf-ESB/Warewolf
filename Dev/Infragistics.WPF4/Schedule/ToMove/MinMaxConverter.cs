using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// Custom value converter that deals with <see cref="IComparable"/> instances and ensures that the specified value is within a given range.
	/// </summary>
	public class MinMaxConverter : IValueConverter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="MinMaxConverter"/>
		/// </summary>
		public MinMaxConverter()
		{
		} 
		#endregion // Constructor

		#region Properties
		/// <summary>
		/// The minimum value returned by the Convert method or null to not constrain the minimum
		/// </summary>
		public object MinValue
		{
			get;
			set;
		}

		/// <summary>
		/// The maximum value returned by the Convert method or null to not constrain the maximum.
		/// </summary>
		public object MaxValue
		{
			get;
			set;
		}
		#endregion // Properties

		#region IValueConverter Members

		/// <summary>
		/// Returns a value between the <see cref="MinValue"/> and <see cref="MaxValue"/>
		/// </summary>
		/// <param name="value">The value to compare to the <see cref="MinValue"/> and <see cref="MaxValue"/></param>
		/// <param name="targetType">The type of the target property</param>
		/// <param name="parameter">The parameter to use for the conversion. This parameter is not used.</param>
		/// <param name="culture">The culture to use for the conversion. This parameter is not used.</param>
		/// <returns>The <paramref name="value"/> constrained by the range defined by the <see cref="MinValue"/> and <see cref="MaxValue"/></returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			object min = this.MinValue;
			object max = this.MaxValue;

			object newValue = System.Convert.ChangeType(value, targetType, culture);

			if (newValue is IComparable == false)
				return DependencyProperty.UnsetValue;

			IComparable minValue = min == null ? null : System.Convert.ChangeType(min, targetType, culture) as IComparable;
			IComparable maxValue = max == null ? null : System.Convert.ChangeType(max, targetType, culture) as IComparable;

			if (minValue == null && maxValue == null)
				return DependencyProperty.UnsetValue;

			if (maxValue != null)
			{
				if (maxValue.CompareTo(newValue) < 0)
					newValue = maxValue;
			}

			if (minValue != null)
			{
				if (minValue.CompareTo(newValue) > 0)
					newValue = minValue;
			}

			return newValue;
		}

		/// <summary>
		/// Returns DependencyProperty.UnsetValue since this method is not supported.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="targetType">The type of the target property</param>
		/// <param name="parameter">The parameter to use for the conversion. This parameter is not used.</param>
		/// <param name="culture">The culture to use for the conversion. This parameter is not used.</param>
		/// <returns>Returns DependencyProperty.UnsetValue</returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return DependencyProperty.UnsetValue;
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