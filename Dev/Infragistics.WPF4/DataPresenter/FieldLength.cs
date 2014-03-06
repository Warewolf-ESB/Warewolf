using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A structure that represents a unit of length supporting explicit values, sizing based on content and star values.
	/// </summary>
	[TypeConverter(typeof(FieldLengthConverter))]
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
	public struct FieldLength : IEquatable<FieldLength>
	{
		#region Member Variables

		private double _value;
		private FieldLengthUnitType _unit;

		private static FieldLength _auto;
		private static FieldLength _initialAuto;
		private static FieldLength _empty;

		private const double EmptyValue = -1;

		#endregion //Member Variables

		#region Constructor
		static FieldLength()
		{
			_auto = new FieldLength(1, FieldLengthUnitType.Auto);
			_initialAuto = new FieldLength(1, FieldLengthUnitType.InitialAuto);

			// auto is always forced to 1 so we need a different value to identify an empty
			_empty = new FieldLength(1, FieldLengthUnitType.Auto);
			_empty._value = EmptyValue;
		}

		/// <summary>
		/// Initializes a new <see cref="FieldLength"/> with the specified pixel value.
		/// </summary>
		/// <param name="pixels">The number of logical pixels</param>
		public FieldLength(double pixels)
			: this(pixels, FieldLengthUnitType.Pixel)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="FieldLength"/>
		/// </summary>
		/// <param name="value">The numeric value for the FieldLength</param>
		/// <param name="type">The type of length that the FieldLength instance represents</param>
		public FieldLength(double value, FieldLengthUnitType type)
		{
			if (double.IsNaN(value))
				throw new ArgumentException(Infragistics.Windows.Resources.GetString("LE_ValueCannotBeNan"));

			if (double.IsInfinity(value))
				throw new ArgumentException("");

			if (value < 0)
				throw new ArgumentException("");

			_value = type == FieldLengthUnitType.Auto || type == FieldLengthUnitType.InitialAuto ? 1d : value;
			_unit = type;
		} 
		#endregion //Constructor

		#region Base class overrides
		/// <summary>
		/// Compares the specified <see cref="FieldLength"/> to this instance's values.
		/// </summary>
		/// <param name="obj">The object to compare to the current instance</param>
		/// <returns>True if the object is a FieldLength with the same <see cref="Value"/> and <see cref="UnitType"/></returns>
		public override bool Equals(object obj)
		{
			if (obj is FieldLength)
				return this.Equals((FieldLength)obj);

			return false;
		}

		/// <summary>
		/// Returns a hash value for the <see cref="FieldLength"/>
		/// </summary>
		/// <returns>A hash value for the FieldLength</returns>
		public override int GetHashCode()
		{
			return _unit.GetHashCode() | _value.GetHashCode();
		}

		/// <summary>
		/// Returns a string representation of the <see cref="FieldLength"/>
		/// </summary>
		/// <returns>A string representation of the <see cref="Value"/> and <see cref="UnitType"/></returns>
		public override string ToString()
		{
			return FieldLengthConverter.ToString(this, CultureInfo.InvariantCulture);
		} 
		#endregion //Base class overrides

		#region Operator Overloads
		/// <summary>
		/// Compares two <see cref="FieldLength"/> instances to determine if they are equal
		/// </summary>
		/// <param name="length1">The first FieldLength instance</param>
		/// <param name="length2">The second FieldLength instance</param>
		/// <returns>True if the objects have the same <see cref="Value"/> and <see cref="UnitType"/>; otherwise false.</returns>
		public static bool operator ==(FieldLength length1, FieldLength length2)
		{
			return length1._unit == length2._unit &&
				length1._value.Equals(length2._value);
		}

		/// <summary>
		/// Compares two <see cref="FieldLength"/> instances to determine if they are not equal
		/// </summary>
		/// <param name="length1">The first FieldLength instance</param>
		/// <param name="length2">The second FieldLength instance</param>
		/// <returns>True if the objects have a different <see cref="Value"/> and/or <see cref="UnitType"/>; otherwise false.</returns>
		public static bool operator !=(FieldLength length1, FieldLength length2)
		{
			return !(length1 == length2);
		} 
		#endregion //Operator Overloads

		#region Properties

		/// <summary>
		/// Returns a static <see cref="FieldLength"/> instance whose <see cref="IsAuto"/> is true.
		/// </summary>
		public static FieldLength Auto
		{
			get { return _auto; }
		}

		internal static FieldLength Empty
		{
			get { return _empty; }
		}

		/// <summary>
		/// Returns a static <see cref="FieldLength"/> instance whose <see cref="IsInitialAuto"/> is true.
		/// </summary>
		public static FieldLength InitialAuto
		{
			get { return _initialAuto; }
		}

		/// <summary>
		/// Returns a boolean indicating if the FieldLength represents an absolute pixel value.
		/// </summary>
		public bool IsAbsolute
		{
			get { return _unit == FieldLengthUnitType.Pixel; }
		}

		/// <summary>
		/// Returns a boolean indicating if the FieldLength represents a value that should be based on the size of the contents.
		/// </summary>
		public bool IsAuto
		{
			get { return _unit == FieldLengthUnitType.Auto && _value != EmptyValue; }
		}

		internal bool IsAnyAuto
		{
			get { return IsAuto || IsInitialAuto; }
		}

		/// <summary>
		/// Returns a boolean indicating if the FieldLength represents a value that should be based on the initial size of the contents.
		/// </summary>
		public bool IsInitialAuto
		{
			get { return _unit == FieldLengthUnitType.InitialAuto; }
		}

		internal bool IsEmpty
		{
			get { return this == _empty; }
		}

		/// <summary>
		/// Returns a boolean indicating if the FieldLength represents a value that is a weighted portion of the available area.
		/// </summary>
		public bool IsStar
		{
			get { return _unit == FieldLengthUnitType.Star; }
		}

		/// <summary>
		/// Returns an enumeration indicating the type of <see cref="Value"/> that the FieldLength represents.
		/// </summary>
		public FieldLengthUnitType UnitType
		{
			get { return _unit; }
		}

		/// <summary>
		/// Returns the numeric value of the FieldLength.
		/// </summary>
		public double Value
		{
			get { return _value; }
		}
		#endregion //Properties

		#region IEquatable<FieldLength> Members

		/// <summary>
		/// Compares the value of this FieldLength instance to the specified <see cref="FieldLength"/>.
		/// </summary>
		/// <param name="other">The instance of the <see cref="FieldLength"/> to compare</param>
		/// <returns>True if the specified FieldLength has the same <see cref="Value"/> and <see cref="UnitType"/>; otherwise false.</returns>
		public bool Equals(FieldLength other)
		{
			return this == other;
		}

		#endregion //IEquatable<FieldLength> Members
	}

	/// <summary>
	/// Converts <see cref="FieldLength"/> instances to and from other types.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
	public class FieldLengthConverter : TypeConverter
	{
		#region Member Variables

		private static Dictionary<string, double> _pixelFactors;

		#endregion //Member Variables

		#region Constructor
		static FieldLengthConverter()
		{
			Dictionary<string, double> factors = new Dictionary<string, double>();
			factors["px"] = 1;
			factors["in"] = 96;
			factors["cm"] = 96d / 2.54d;
			factors["pt"] = 96d / 72d;
			_pixelFactors = factors;
		}

		/// <summary>
		/// Initializes a new <see cref="FieldLengthConverter"/>
		/// </summary>
		public FieldLengthConverter()
		{
		} 
		#endregion //Constructor

		#region Base class overrides
		/// <summary>
		/// Determines whether an object of the specified source type can be converted to a <see cref="FieldLength"/>
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="sourceType">The type from which the conversion could occur</param>
		/// <returns>True for numeric types; otherwise false.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (Utilities.IsNumericType(sourceType))
				return true;

			if (sourceType == typeof(string))
				return true;

			return false;
		}

		/// <summary>
		/// Converts the specified value to a <see cref="FieldLength"/>
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="culture">Culture information used to create the <see cref="FieldLength"/> instance</param>
		/// <param name="value">The value being converted</param>
		/// <returns>An instance of a <see cref="FieldLength"/></returns>
		/// <exception cref="ArgumentNullException">The value is null.</exception>
		/// <exception cref="ArgumentException">The value provided cannot be converted to a FieldLength.</exception>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			GridUtilities.ValidateNotNull(value, "value");

			string strValue = value as string;

			if (null != strValue)
				return FromString(strValue, culture);

			object convertedValue = Utilities.ConvertDataValue(value, typeof(double), culture, null);
			double dblValue = Convert.ToDouble(convertedValue);

			if (double.IsNaN(dblValue))
				return FieldLength.Auto;
			// AS 3/25/10 TFS28851 - Was missing the "return"
			//else
			//    new FieldLength(dblValue);
			//
			//return base.ConvertFrom(context, culture, value);
			return new FieldLength(dblValue);
		}

		/// <summary>
		/// Determines whether a <see cref="FieldLength"/> instance can be converted to the specified type
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="destinationType">The type to which the FieldLength should be converted</param>
		/// <returns>True for string and InstanceDescriptor; otherwise false.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
				return true;

			if (destinationType == typeof(string))
				return true;

			return base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// Converts a <see cref="FieldLength"/> instance to the specified type
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="culture">Culture information used to create the <see cref="FieldLength"/> instance</param>
		/// <param name="value">The <see cref="FieldLength"/> being converted</param>
		/// <param name="destinationType">The type to which the FieldLength should be converted</param>
		/// <returns>An instance of the specified type that represents the specified FieldLength</returns>
		/// <exception cref="ArgumentNullException">The value or destinationType provided is null.</exception>
		/// <exception cref="ArgumentException">The value provided is not a FieldLength or the destination type is not supported.</exception>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			GridUtilities.ValidateNotNull(value, "value");
			GridUtilities.ValidateNotNull(destinationType, "destinationType");

			if (value is FieldLength == false)
				throw base.GetConvertToException(value, destinationType);

			FieldLength fl = (FieldLength)value;

			if (destinationType == typeof(string))
				return ToString(fl, culture);

			if (destinationType == typeof(InstanceDescriptor))
			{
				ConstructorInfo ci = typeof(FieldLength).GetConstructor(new Type[] { typeof(double), typeof(FieldLengthUnitType) });
				return new InstanceDescriptor(ci, new object[] { fl.Value, fl.UnitType }, true);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		} 
		#endregion //Base class overrides

		#region Methods

		#region FromString
		private static FieldLength FromString(string strValue, CultureInfo culture)
		{
			strValue = strValue.Trim().ToLower(culture ?? CultureInfo.InvariantCulture);

			if (strValue == "auto")
				return FieldLength.Auto;
			else if (strValue == "initialauto")
				return FieldLength.InitialAuto;
			else if (strValue == "*")
				return new FieldLength(1, FieldLengthUnitType.Star);
			else if (strValue == "px")
				return new FieldLength(1);

			double value;

			if (TryParseUnit(strValue, culture, "*", out value))
				return new FieldLength(value, FieldLengthUnitType.Star);

			foreach (KeyValuePair<string, double> pair in _pixelFactors)
			{
				if (TryParseUnit(strValue, culture, pair.Key, out value))
				{
					return new FieldLength(value * pair.Value, FieldLengthUnitType.Pixel);
				}
			}

			return new FieldLength(Convert.ToDouble(strValue, culture));
		} 
		#endregion //FromString

		#region ToString
		internal static string ToString(FieldLength fieldLength, CultureInfo cultureInfo)
		{
			switch (fieldLength.UnitType)
			{
				case FieldLengthUnitType.Auto:
					return "Auto";
				case FieldLengthUnitType.InitialAuto:
					return "InitialAuto";
				case FieldLengthUnitType.Star:
					if (GridUtilities.AreClose(1, fieldLength.Value))
						return "*";

					return Convert.ToString(fieldLength.Value, cultureInfo) + "*";
				default:
					return Convert.ToString(fieldLength.Value, cultureInfo);
			}
		}
		#endregion //ToString

		#region TryParseUnit
		private static bool TryParseUnit(string strValue, CultureInfo culture, string unitString, out double value)
		{
			value = double.NaN;

			if (strValue.EndsWith(unitString))
			{
				string strNum = strValue.Substring(0, strValue.Length - unitString.Length);
				value = Convert.ToDouble(strNum, culture);
				return true;
			}

			return false;
		} 
		#endregion //TryParseUnit

		#endregion //Methods
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