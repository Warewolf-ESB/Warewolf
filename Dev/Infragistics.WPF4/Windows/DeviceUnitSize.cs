using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Globalization;

namespace Infragistics.Windows
{
	/// <summary>
	/// A structure that represents a Size expressed in specific units as indicated by the <see cref="UnitType"/>.
	/// </summary>
	[TypeConverter(typeof(DeviceUnitSizeConverter))]
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public struct DeviceUnitSize : IEquatable<DeviceUnitSize>
	{
		#region Member Variables

		private double _width;
		private double _height;
		private DeviceUnitType _unit;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="DeviceUnitSize"/> with the specified value for the Left, Top, Right and Bottom.  The <see cref="UnitType"/> property
		/// will default to <i>DeviceIndependentUnit</i>.
		/// </summary>
		/// <param name="width">The width of the size being created</param>
		/// <param name="height">The height of the size being created</param>
		public DeviceUnitSize(double width, double height)
			: this(width, height, DeviceUnitType.DeviceIndependentUnit)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="DeviceUnitSize"/>
		/// </summary>
		/// <param name="width">The width of the size being created</param>
		/// <param name="height">The height of the size being created</param>
		/// <param name="type">The type of length that the DeviceUnitSize instance represents</param>
		public DeviceUnitSize(double width, double height, DeviceUnitType type)
		{
			DeviceUnitLength.ValidateUnitLengthValue(width);
			DeviceUnitLength.ValidateUnitLengthValue(height);

			_width = width;
			_height = height;
			_unit = type;
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties
		/// <summary>
		/// Returns or sets the width in units indicated by the <see cref="UnitType"/>
		/// </summary>
		public double Width
		{
			get { return _width; }
			set { SetValue(ref _width, value); }
		}

		/// <summary>
		/// Returns or sets the height in units indicated by the <see cref="UnitType"/>
		/// </summary>
		public double Height
		{
			get { return _height; }
			set { SetValue(ref _height, value); }
		}

		/// <summary>
		/// Returns an enumeration indicating the type of unit that the values of the DeviceUnitSize represent.
		/// </summary>
		public DeviceUnitType UnitType
		{
			get { return _unit; }
			set { this._unit = value; }
		}
		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region ConvertToUnitType
		/// <summary>
		/// Converts the current values to the specified <see cref="DeviceUnitType"/>
		/// </summary>
		/// <param name="unitType">The unit type to convert to.</param>
		/// <returns>The converted value.</returns>
		public DeviceUnitSize ConvertToUnitType(DeviceUnitType unitType)
		{
			if (this.UnitType == unitType)
				return this;

			return new DeviceUnitSize(
				DeviceUnitLength.ConvertFromTwips(DeviceUnitLength.ConvertToTwips(_width, _unit), unitType),
				DeviceUnitLength.ConvertFromTwips(DeviceUnitLength.ConvertToTwips(_height, _unit), unitType),
				unitType
				);
		}
		#endregion //ConvertToUnitType

		#endregion //Public Methods

		#region Private Methods

		#region SetValue
		private static void SetValue(ref double member, double newValue)
		{
			DeviceUnitLength.ValidateUnitLengthValue(newValue);
			member = newValue;
		}
		#endregion //SetValue

		#endregion //Private Methods

		#endregion //Methods

		#region Base class overrides

		/// <summary>
		/// Compares the specified <see cref="DeviceUnitSize"/> to this instance's values.
		/// </summary>
		/// <param name="obj">The object to compare to the current instance</param>
		/// <returns>True if the object is a DeviceUnitSize with the same values and <see cref="UnitType"/></returns>
		public override bool Equals(object obj)
		{
			if (obj is DeviceUnitSize)
				return this.Equals((DeviceUnitSize)obj);

			return false;
		}

		/// <summary>
		/// Returns a hash value for the <see cref="DeviceUnitSize"/>
		/// </summary>
		/// <returns>A hash value for the DeviceUnitSize</returns>
		public override int GetHashCode()
		{
			return _unit.GetHashCode()
				^ _width.GetHashCode()
				^ _height.GetHashCode();
		}
		#endregion //Base class overrides

		#region Operator Overloads

		/// <summary>
		/// Compares two <see cref="DeviceUnitSize"/> instances to determine if they are equal
		/// </summary>
		/// <param name="length1">The first DeviceUnitSize instance</param>
		/// <param name="length2">The second DeviceUnitSize instance</param>
		/// <returns>True if the objects have the same values and <see cref="UnitType"/>; otherwise false.</returns>
		public static bool operator ==(DeviceUnitSize length1, DeviceUnitSize length2)
		{
			return length1._unit == length2._unit &&
				length1._width.Equals(length2._width) &&
				length1._height.Equals(length2._height);
		}

		/// <summary>
		/// Compares two <see cref="DeviceUnitSize"/> instances to determine if they are not equal
		/// </summary>
		/// <param name="length1">The first DeviceUnitSize instance</param>
		/// <param name="length2">The second DeviceUnitSize instance</param>
		/// <returns>True if the objects have a different values and/or <see cref="UnitType"/>; otherwise false.</returns>
		public static bool operator !=(DeviceUnitSize length1, DeviceUnitSize length2)
		{
			return !(length1 == length2);
		}

		#endregion //Operator Overloads

		#region IEquatable<DeviceUnitSize> Members

		/// <summary>
		/// Compares the value of this instance to the specified <see cref="DeviceUnitSize"/>.
		/// </summary>
		/// <param name="other">The instance of the <see cref="DeviceUnitSize"/> to compare</param>
		/// <returns>True if the specified DeviceUnitSize has the same values and <see cref="UnitType"/>; otherwise false.</returns>
		public bool Equals(DeviceUnitSize other)
		{
			return this == other;
		}

		#endregion //IEquatable<DeviceUnitSize> Members
	}

	/// <summary>
	/// Converts <see cref="DeviceUnitSize"/> instances to and from other types.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public class DeviceUnitSizeConverter : TypeConverter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DeviceUnitSizeConverter"/>
		/// </summary>
		public DeviceUnitSizeConverter()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanConvertFrom
		/// <summary>
		/// Determines whether an object of the specified source type can be converted to a <see cref="DeviceUnitSize"/>
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="sourceType">The type from which the conversion could occur</param>
		/// <returns>True for numeric types; otherwise false.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;

			if (Utilities.IsNumericType(sourceType))
				return true;

			return false;
		}
		#endregion //CanConvertFrom

		#region ConvertFrom
		/// <summary>
		/// Converts the specified value to a <see cref="DeviceUnitSize"/>
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="culture">Culture information used to create the <see cref="DeviceUnitSize"/> instance</param>
		/// <param name="value">The value being converted</param>
		/// <returns>An instance of a <see cref="DeviceUnitSize"/></returns>
		/// <exception cref="ArgumentNullException">The value is null.</exception>
		/// <exception cref="ArgumentException">The value provided cannot be converted to a DeviceUnitSize.</exception>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			Utilities.ValidateNotNull(value, "value");

			if (value is string)
			{
				string strValue = value as string;
				DeviceUnitType unit;
				DeviceUnitLengthConverter.ParseUnit(ref strValue, out unit);
				Size size = (Size)new SizeConverter().ConvertFromString(context, culture, strValue);

				if (size.IsEmpty)
					throw base.GetConvertFromException(value);

				return new DeviceUnitSize(size.Width, size.Height, unit);
			}

			if (!Utilities.IsNumericType(value.GetType()))
				throw this.GetConvertFromException(value);

			object convertedValue = Utilities.ConvertDataValue(value, typeof(double), culture, null);
			return new DeviceUnitLength(Convert.ToDouble(convertedValue));
		}
		#endregion //ConvertFrom

		#region CanConvertTo
		/// <summary>
		/// Determines whether a <see cref="DeviceUnitSize"/> instance can be converted to the specified type
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="destinationType">The type to which the DeviceUnitSize should be converted</param>
		/// <returns>True for string and InstanceDescriptor; otherwise false.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
				return true;

			if (destinationType == typeof(string))
				return true;

			return base.CanConvertTo(context, destinationType);
		}
		#endregion //CanConvertTo

		#region ConvertTo
		/// <summary>
		/// Converts a <see cref="DeviceUnitSize"/> instance to the specified type
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="culture">Culture information used to create the <see cref="DeviceUnitSize"/> instance</param>
		/// <param name="value">The <see cref="DeviceUnitSize"/> being converted</param>
		/// <param name="destinationType">The type to which the DeviceUnitSize should be converted</param>
		/// <returns>An instance of the specified type that represents the specified DeviceUnitSize</returns>
		/// <exception cref="ArgumentNullException">The value or destinationType provided is null.</exception>
		/// <exception cref="ArgumentException">The value provided is not a DeviceUnitSize or the destination type is not supported.</exception>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			Utilities.ValidateNotNull(value, "value");
			Utilities.ValidateNotNull(destinationType, "destinationType");

			if (value is DeviceUnitSize == false)
				throw base.GetConvertToException(value, destinationType);

			DeviceUnitSize size = (DeviceUnitSize)value;

			if (destinationType == typeof(string))
			{
				Size t = new Size(size.Width, size.Height);
				string strValue = new SizeConverter().ConvertToString(context, culture, t);
				return strValue + DeviceUnitLengthConverter.GetSuffix(size.UnitType);
			}

			if (destinationType == typeof(InstanceDescriptor))
			{
				ConstructorInfo ci = typeof(DeviceUnitLength).GetConstructor(new Type[] { typeof(double), typeof(double), typeof(double), typeof(double), typeof(DeviceUnitType) });
				return new InstanceDescriptor(ci, new object[] { size.Width, size.Height, size.UnitType }, true);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
		#endregion //ConvertTo

		#endregion //Base class overrides
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