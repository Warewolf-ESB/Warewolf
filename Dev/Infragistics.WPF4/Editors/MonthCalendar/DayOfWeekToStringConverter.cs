using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace Infragistics.Windows.Editors
{
	/// <summary>
	/// Value converter for converting a <see cref="DayOfWeek"/> for a given <see cref="DayOfWeekHeaderFormat"/>
	/// </summary>
	public sealed class DayOfWeekToStringConverter : IMultiValueConverter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DayOfWeekToStringConverter"/>
		/// </summary>
		public DayOfWeekToStringConverter()
		{
		} 
		#endregion //Constructor

		#region Convert
		/// <summary>
		/// Converts a given <see cref="DayOfWeekHeaderFormat"/> and <see cref="DayOfWeek"/> to a string representing the <see cref="XamMonthCalendar.DayOfWeekHeaderFormat"/>
		/// </summary>
		/// <param name="values">A two item array containing a <see cref="DayOfWeek"/> and <see cref="DayOfWeekHeaderFormat"/> in that order. If a 1 element array is specified, it must contain the <see cref="DayOfWeek"/> to convert; the DayOfWeekHeaderFormat is assumed to be <b>Abbreviated</b> and the converter culture is used for the culture info.</param>
		/// <param name="targetType">The type to which the converter must be convert the value. This parameter is not used. A String value is always returned</param>
		/// <param name="parameter">This parameter is not used.</param>
		/// <param name="culture">The culture used to obtain the day of week names.</param>
		/// <returns><see cref="DependencyProperty.UnsetValue"/> if the parameters do not match the information. Otherwise a string representation of the day of week is returned.</returns>
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == null || values.Length < 1 || values.Length > 2)
				return DependencyProperty.UnsetValue;

			if (values[0] is DayOfWeek == false)
				return DependencyProperty.UnsetValue;

			DayOfWeek dow = (DayOfWeek)values[0];
			DayOfWeekHeaderFormat format = DayOfWeekHeaderFormat.Abbreviated;

			if (values.Length > 1)
			{
				if (values[1] is DayOfWeekHeaderFormat == false)
					return DependencyProperty.UnsetValue;

				format = (DayOfWeekHeaderFormat)values[1];
			}

			culture = culture ?? CultureInfo.CurrentCulture;
			DateTimeFormatInfo dtfi = culture.DateTimeFormat;

			return CalendarManager.GetDayOfWeekCaption(format, dow, dtfi);
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