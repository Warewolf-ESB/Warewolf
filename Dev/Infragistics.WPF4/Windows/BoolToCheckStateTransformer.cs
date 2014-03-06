using System;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Reflection;
using System.ComponentModel;

namespace Infragistics.Windows.Converters
{

	#region BoolToCheckStateConverter

	/// <summary>
	/// IValueConverter implementation that converts values between <b>bool</b> and <b>Nullable<bool></b>.
	/// When converting from <b>Nullable<bool></b>, null value is converted to <b>true</b>.
	/// </summary>
	public class BoolToCheckStateConverter : IValueConverter
	{
        /// <summary>
        /// Constructor
        /// </summary>
        public BoolToCheckStateConverter()
		{
		}

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
				return this.BoolToCheckState((bool)value);
			else
				return false;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Nullable<bool>)
				return this.CheckStateToBool((Nullable<bool>)value);
			else
				return false;
		}

		private bool CheckStateToBool(Nullable<bool> cs)
		{
            if (!cs.HasValue)
                return true;

            return cs.Value;
		}

		private Nullable<bool> BoolToCheckState(bool b)
		{
			return b;
		}
	}

	#endregion BoolToCheckStateConverter

	#region NotBoolToCheckStateConverter

	/// <summary>
	/// </summary>
	public class NotBoolToCheckStateConverter : IValueConverter
	{
        /// <summary>
        /// Constructor
        /// </summary>
        public NotBoolToCheckStateConverter()
		{
		}

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
				return this.BoolToCheckState(!(bool)value);
			else
				return true;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Nullable<bool>)
				return !this.CheckStateToBool((Nullable<bool>)value);
			else
				return true;
		}

		private bool CheckStateToBool(Nullable<bool> cs)
		{
            if (!cs.HasValue)
                return true;
            
           return cs.Value;
		}

		private Nullable<bool> BoolToCheckState(bool b)
		{
			return b;
		}
	}

	#endregion NotBoolToCheckStateConverter

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