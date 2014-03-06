using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Globalization;
using System.Windows;

namespace Infragistics.Windows
{

	/// <summary>
	/// A structure that represents the thickness of a frame expressed in specific units as indicated by the <see cref="UnitType"/>.
	/// </summary>
	[TypeConverter(typeof(DeviceUnitThicknessConverter))]
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public struct DeviceUnitThickness : IEquatable<DeviceUnitThickness>
	{
		#region Member Variables

		private double _left;
		private double _top;
		private double _right;
		private double _bottom;
		private DeviceUnitType _unit;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="DeviceUnitThickness"/> with the specified value for the Left, Top, Right and Bottom.  The <see cref="UnitType"/> property
		/// will default to <i>DeviceIndependentUnit</i>.
		/// </summary>
		/// <param name="uniformLength">The length of the unit for all sides.</param>
		public DeviceUnitThickness(double uniformLength)
			: this(uniformLength, DeviceUnitType.DeviceIndependentUnit)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="DeviceUnitThickness"/>
		/// </summary>
		/// <param name="uniformLength">The numeric value for all sides of the DeviceUnitThickness.</param>
		/// <param name="type">The type of length that the DeviceUnitThickness instance represents</param>
		public DeviceUnitThickness(double uniformLength, DeviceUnitType type)
		{
			DeviceUnitLength.ValidateUnitLengthValue(uniformLength);

			_left = _right = _top = _bottom = uniformLength;
			_unit = type;
		}

		/// <summary>
		/// Initializes a new <see cref="DeviceUnitThickness"/>
		/// </summary>
		/// <param name="left">The numeric value for the left side.</param>
		/// <param name="top">The numeric value for the top side.</param>
		/// <param name="right">The numeric value for the right side.</param>
		/// <param name="bottom">The numeric value for the bottom side.</param>
		/// <param name="type">The type of length that the DeviceUnitLength instance represents</param>
		public DeviceUnitThickness(double left, double top, double right, double bottom, DeviceUnitType type)
		{
			_unit = type;

			// put temporary values so we can access the properties to not have 
			// to duplicate the validation logic
			_left = _top = _right = _bottom = 0;

			this.Left = left;
			this.Top = top;
			this.Bottom = bottom;
			this.Right = right;
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties
		/// <summary>
		/// Returns or sets the width of the bottom side in units indicated by the <see cref="UnitType"/>
		/// </summary>
		public double Bottom
		{
			get { return _bottom; }
			set { SetValue(ref _bottom, value); }
		}

		/// <summary>
		/// Returns or sets the width of the left side in units indicated by the <see cref="UnitType"/>
		/// </summary>
		public double Left
		{
			get { return _left; }
			set { SetValue(ref _left, value); }
		}

		/// <summary>
		/// Returns or sets the width of the right side in units indicated by the <see cref="UnitType"/>
		/// </summary>
		public double Right
		{
			get { return _right; }
			set { SetValue(ref _right, value); }
		}

		/// <summary>
		/// Returns or sets the width of the top side in units indicated by the <see cref="UnitType"/>
		/// </summary>
		public double Top
		{
			get { return _top; }
			set { SetValue(ref _top, value); }
		}

		/// <summary>
		/// Returns an enumeration indicating the type of unit that the values of the DeviceUnitThickness represent.
		/// </summary>
		public DeviceUnitType UnitType
		{
			get { return _unit; }
			set { this._unit = value; }
		}
		#endregion //Public Properties

		#region Internal Properties
		internal bool IsUniform
		{
			get
			{
				return Utilities.AreClose(_left, _top) &&
					Utilities.AreClose(_left, _right) &&
					Utilities.AreClose(_left, _bottom);
			}
		}
		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region ConvertToUnitType
		/// <summary>
		/// Converts the current values to the specified <see cref="DeviceUnitType"/>
		/// </summary>
		/// <param name="unitType">The unit type to convert to.</param>
		/// <returns>The converted value.</returns>
		public DeviceUnitThickness ConvertToUnitType(DeviceUnitType unitType)
		{
			if (this.UnitType == unitType)
				return this;

			double left = DeviceUnitLength.ConvertFromTwips(DeviceUnitLength.ConvertToTwips(_left, _unit), unitType);

			if (this.IsUniform)
				return new DeviceUnitThickness(left, unitType);

			return new DeviceUnitThickness(
				left,
				DeviceUnitLength.ConvertFromTwips(DeviceUnitLength.ConvertToTwips(_top, _unit), unitType),
				DeviceUnitLength.ConvertFromTwips(DeviceUnitLength.ConvertToTwips(_right, _unit), unitType),
				DeviceUnitLength.ConvertFromTwips(DeviceUnitLength.ConvertToTwips(_bottom, _unit), unitType),
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
		/// Compares the specified <see cref="DeviceUnitThickness"/> to this instance's values.
		/// </summary>
		/// <param name="obj">The object to compare to the current instance</param>
		/// <returns>True if the object is a DeviceUnitThickness with the same values and <see cref="UnitType"/></returns>
		public override bool Equals(object obj)
		{
			if (obj is DeviceUnitThickness)
				return this.Equals((DeviceUnitThickness)obj);

			return false;
		}

		/// <summary>
		/// Returns a hash value for the <see cref="DeviceUnitThickness"/>
		/// </summary>
		/// <returns>A hash value for the DeviceUnitThickness</returns>
		public override int GetHashCode()
		{
			return _unit.GetHashCode()
				^ _left.GetHashCode()
				^ _right.GetHashCode()
				^ _top.GetHashCode()
				^ _bottom.GetHashCode();
		}
		#endregion //Base class overrides

		#region Operator Overloads

		/// <summary>
		/// Compares two <see cref="DeviceUnitThickness"/> instances to determine if they are equal
		/// </summary>
		/// <param name="length1">The first DeviceUnitThickness instance</param>
		/// <param name="length2">The second DeviceUnitThickness instance</param>
		/// <returns>True if the objects have the same values and <see cref="UnitType"/>; otherwise false.</returns>
		public static bool operator ==(DeviceUnitThickness length1, DeviceUnitThickness length2)
		{
			return length1._unit == length2._unit &&
				length1._left.Equals(length2._left) &&
				length1._right.Equals(length2._right) &&
				length1._top.Equals(length2._top) &&
				length1._bottom.Equals(length2._bottom);
		}

		/// <summary>
		/// Compares two <see cref="DeviceUnitThickness"/> instances to determine if they are not equal
		/// </summary>
		/// <param name="length1">The first DeviceUnitThickness instance</param>
		/// <param name="length2">The second DeviceUnitThickness instance</param>
		/// <returns>True if the objects have a different values and/or <see cref="UnitType"/>; otherwise false.</returns>
		public static bool operator !=(DeviceUnitThickness length1, DeviceUnitThickness length2)
		{
			return !(length1 == length2);
		}

		#endregion //Operator Overloads

		#region IEquatable<DeviceUnitThickness> Members

		/// <summary>
		/// Compares the value of this instance to the specified <see cref="DeviceUnitThickness"/>.
		/// </summary>
		/// <param name="other">The instance of the <see cref="DeviceUnitThickness"/> to compare</param>
		/// <returns>True if the specified DeviceUnitThickness has the same values and <see cref="UnitType"/>; otherwise false.</returns>
		public bool Equals(DeviceUnitThickness other)
		{
			return this == other;
		}

		#endregion //IEquatable<DeviceUnitThickness> Members
	}

	/// <summary>
	/// Converts <see cref="DeviceUnitThickness"/> instances to and from other types.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public class DeviceUnitThicknessConverter : TypeConverter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DeviceUnitThicknessConverter"/>
		/// </summary>
		public DeviceUnitThicknessConverter()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanConvertFrom
		/// <summary>
		/// Determines whether an object of the specified source type can be converted to a <see cref="DeviceUnitThickness"/>
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
		/// Converts the specified value to a <see cref="DeviceUnitThickness"/>
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="culture">Culture information used to create the <see cref="DeviceUnitThickness"/> instance</param>
		/// <param name="value">The value being converted</param>
		/// <returns>An instance of a <see cref="DeviceUnitThickness"/></returns>
		/// <exception cref="ArgumentNullException">The value is null.</exception>
		/// <exception cref="ArgumentException">The value provided cannot be converted to a DeviceUnitThickness.</exception>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			Utilities.ValidateNotNull(value, "value");

			if (value is string)
			{
				string strValue = value as string;
				DeviceUnitType unit;
				DeviceUnitLengthConverter.ParseUnit(ref strValue, out unit);
				Thickness thickness = (Thickness)new ThicknessConverter().ConvertFromString(context, culture, strValue);
				return new DeviceUnitThickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom, unit);
			}

			if (!Utilities.IsNumericType(value.GetType()))
				throw this.GetConvertFromException(value);

			object convertedValue = Utilities.ConvertDataValue(value, typeof(double), culture, null);
			return new DeviceUnitLength(Convert.ToDouble(convertedValue));
		} 
		#endregion //ConvertFrom

		#region CanConvertTo
		/// <summary>
		/// Determines whether a <see cref="DeviceUnitThickness"/> instance can be converted to the specified type
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="destinationType">The type to which the DeviceUnitThickness should be converted</param>
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
		/// Converts a <see cref="DeviceUnitThickness"/> instance to the specified type
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="culture">Culture information used to create the <see cref="DeviceUnitThickness"/> instance</param>
		/// <param name="value">The <see cref="DeviceUnitThickness"/> being converted</param>
		/// <param name="destinationType">The type to which the DeviceUnitThickness should be converted</param>
		/// <returns>An instance of the specified type that represents the specified DeviceUnitThickness</returns>
		/// <exception cref="ArgumentNullException">The value or destinationType provided is null.</exception>
		/// <exception cref="ArgumentException">The value provided is not a DeviceUnitThickness or the destination type is not supported.</exception>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			Utilities.ValidateNotNull(value, "value");
			Utilities.ValidateNotNull(destinationType, "destinationType");

			if (value is DeviceUnitThickness == false)
				throw base.GetConvertToException(value, destinationType);

			DeviceUnitThickness thickness = (DeviceUnitThickness)value;

			if (destinationType == typeof(string))
			{
				if (thickness.IsUniform)
					return DeviceUnitLengthConverter.ToString(new DeviceUnitLength(thickness.Left, thickness.UnitType), culture);

				Thickness t = new Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
				string strValue = new ThicknessConverter().ConvertToString(context, culture, t);
				return strValue + DeviceUnitLengthConverter.GetSuffix(thickness.UnitType);
			}

			if (destinationType == typeof(InstanceDescriptor))
			{
				ConstructorInfo ci;

				if (thickness.IsUniform)
				{
					ci = typeof(DeviceUnitLength).GetConstructor(new Type[] { typeof(double), typeof(DeviceUnitType) });
					return new InstanceDescriptor(ci, new object[] { thickness.Left, thickness.UnitType }, true);
				}
				else
				{
					ci = typeof(DeviceUnitLength).GetConstructor(new Type[] { typeof(double), typeof(double), typeof(double), typeof(double), typeof(DeviceUnitType) });
					return new InstanceDescriptor(ci, new object[] { thickness.Left, thickness.Top, thickness.Right, thickness.Bottom, thickness.UnitType }, true);
				}
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