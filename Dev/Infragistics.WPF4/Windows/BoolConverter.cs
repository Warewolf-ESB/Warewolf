using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using Infragistics.Windows.Helpers;
using System.Windows;

namespace Infragistics.Windows.Controls
{
	// AS 4/6/07 Moved from Editors
	#region BoolConverter Class

	/// <summary>
	/// A base converter class for converting true and false boolean values to corresponding values.
	/// </summary>
	public class BoolConverter : IValueConverter
	{
		/// <summary>
		/// This should be set by the derived class to the value that corresponds to <b>true</b>.
		/// </summary>
		protected object trueValue = KnownBoxes.TrueBox;

		/// <summary>
		/// This should be set by the derived class to the value that corresponds to <b>false</b>.
		/// </summary>
		protected object falseValue = KnownBoxes.FalseBox;

		/// <summary>
		/// Initializes a new <see cref="BoolConverter"/>
		/// </summary>
		public BoolConverter()
		{
		}

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool && (bool)value)
				return trueValue;
			else
				return falseValue;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (object.Equals(trueValue, value))
				return KnownBoxes.TrueBox;
			else
				return KnownBoxes.FalseBox;
		}
	}

	#endregion BoolConverter Class

	#region BoolToValueConverter Class

	// SSP 11/9/10 TFS33587
	// 
	/// <summary>
	/// A base converter class for converting true and false boolean values to corresponding values.
	/// </summary>
	public class BoolToValueConverter : BoolConverter
	{
		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="BoolConverter"/>
		/// </summary>
		public BoolToValueConverter( )
		{
		} 

		#endregion // Constructor

		#region FalseConvertValue

		/// <summary>
		/// False will be converted to this value.
		/// </summary>
		public object FalseConvertValue
		{
			get
			{
				return this.falseValue;
			}
			set
			{
				this.falseValue = value;
			}
		}

		#endregion // FalseConvertValue

		#region TrueConvertValue

		/// <summary>
		/// True will be converted to this value.
		/// </summary>
		public object TrueConvertValue
		{
			get
			{
				return this.trueValue;
			}
			set
			{
				this.trueValue = value;
			}
		}

		#endregion // TrueConvertValue
	}

	#endregion BoolToValueConverter Class

	#region BoolToHiddenConverter Class

	/// <summary>
	/// Converts <b>true</b> to <b>Visibility.Visible</b> and <b>false</b> to <b>Visibility.Hidden</b>.
	/// </summary>
	public class BoolToHiddenConverter : BoolConverter
	{
		/// <summary>
		/// Initializes a new <see cref="BoolToHiddenConverter"/>
		/// </summary>
		public BoolToHiddenConverter()
		{
			trueValue = KnownBoxes.VisibilityVisibleBox;
			falseValue = KnownBoxes.VisibilityHiddenBox;
		}
	}

	#endregion // BoolToHiddenConverter Class

	#region NotBoolToVisibilityConverter Class

	/// <summary>
	/// Converts <b>true</b> to <b>Visibility.Collapsed</b> and <b>false</b> to <b>Visibility.Visible</b>.
	/// </summary>
	public class NotBoolToVisibilityConverter : BoolConverter
	{
		/// <summary>
		/// Initializes a new <see cref="NotBoolToVisibilityConverter"/>
		/// </summary>
		public NotBoolToVisibilityConverter()
		{
			trueValue = KnownBoxes.VisibilityCollapsedBox;
			falseValue = KnownBoxes.VisibilityVisibleBox;
		}
	}

	#endregion // NotBoolToVisibilityConverter Class

	// SSP 3/12/10 TFS27090
	// 
	#region NullToFalseConverter Class

	/// <summary>
	/// A converter class that converts null and empty string values to false and any non-null non-empty string value to true.
	/// </summary>
	public class NullToFalseConverter : IValueConverter
	{
		object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return null != value && ( ! ( value is string ) || ((string)value).Length > 0 )
				? KnownBoxes.TrueBox : KnownBoxes.FalseBox;
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return Binding.DoNothing;
		}
	}

	#endregion // NullToFalseConverter Class

	// SSP 3/12/10 TFS27090
	// 
	#region NullToParameterConverter Class

	/// <summary>
	/// A converter class that converts null and empty string values to parameter in the Convert method and DoNothing in ConvertBack.
	/// If the value is non-null and non-empty string then returns the value itself.
	/// </summary>
	public class NullToParameterConverter : IValueConverter
	{
		private static bool IsNullOrEmptyString( object value )
		{
			return null == value || value is string && 0 == ( (string)value ).Length;
		}

		object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return IsNullOrEmptyString( value ) ? parameter : value;
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return Binding.DoNothing;
		}
	}

	#endregion // NullToParameterConverter Class

	// SSP 3/12/10 TFS27090
	// 
	#region TypeCheckConverter Class

	/// <summary>
	/// A converter class that converts to true if the value is of the type specified by the converter parameter.
	/// </summary>
	public class TypeCheckConverter : IValueConverter
	{
		private bool _allowDerivedTypes = true;

		object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			Type type = parameter as Type;

			return null != value && null != type
					&& ( _allowDerivedTypes ? type.IsInstanceOfType( value ) : type == value.GetType( ) )
				? KnownBoxes.TrueBox : KnownBoxes.FalseBox;
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return Binding.DoNothing;
		}

		/// <summary>
		/// Whether match derived types as well. Default is true.
		/// </summary>
		public bool AllowDerivedTypes
		{
			get
			{
				return _allowDerivedTypes;
			}
			set
			{
				_allowDerivedTypes = value;
			}
		}
	}

	#endregion // TypeCheckConverter Class

	// JM TFS21654 09-04-09 - Added
	#region VisibilityToBooleanConverter
	/// <summary>
	/// Represents the converter that converts System.Windows.Visibility enumeration values to and from Boolean values.
	/// </summary>
	public class VisibilityToBooleanConverter : IValueConverter
	{
		#region IValueConverter Members

		object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Visibility? b = (Visibility)value;
			if (b.HasValue && b.Value == Visibility.Visible)
			{
				return KnownBoxes.TrueBox;
			}
			return KnownBoxes.FalseBox;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool? b = (bool)value;
			if (b.HasValue && b.Value == true)
			{
				return KnownBoxes.VisibilityVisibleBox;
			}
			return KnownBoxes.VisibilityCollapsedBox;
		}

		#endregion
	}
	#endregion //VisibilityToBooleanConverter
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