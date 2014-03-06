using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;

namespace Infragistics.Windows
{
    /// <summary>
    /// A structure that represents a unit of length supporting explicit values and different unit types.
    /// </summary>
    [TypeConverter(typeof(DeviceUnitLengthConverter))]
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public struct DeviceUnitLength : IEquatable<DeviceUnitLength>
    {
        #region Member Variables

		private const int TwipsPerInch = 1440;
		private const int LogicalDpi = 96;
		private const double CentimetersPerInch = 2.54;
		private const int TwipsPerPoint = 20;

        private double _value;
        private DeviceUnitType _unit;        

        #endregion //Member Variables

        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="DeviceUnitLength"/> with the specified value.  The <see cref="UnitType"/> property
        /// will default to <i>DeviceIndependentUnit</i>.
        /// </summary>
        /// <param name="value">The length of the unit.</param>
        public DeviceUnitLength(double value)
            : this(value, DeviceUnitType.DeviceIndependentUnit)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DeviceUnitLength"/>
        /// </summary>
        /// <param name="value">The numeric value for the DeviceUnitLength.</param>
        /// <param name="type">The type of length that the DeviceUnitLength instance represents</param>
        public DeviceUnitLength(double value, DeviceUnitType type)
        {
            
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			// AS 2/17/11 NA 2011.1 Word Writer
			// Moved to helper routine.
			//
			ValidateUnitLengthValue(value);

            this._value = value;
            this._unit = type;
        }
        #endregion //Constructor

        #region Methods

		#region Public Methods

		#region ConvertToUnitType
		/// <summary>
		/// Converts the current <see cref="Value"/> to the specified <see cref="DeviceUnitType"/>
		/// </summary>
		/// <param name="unitType">The unit type to convert to.</param>
		/// <returns>The converted value.</returns>
		public double ConvertToUnitType(DeviceUnitType unitType)
		{
			if (this.UnitType == unitType)
				return this.Value;

			#region Refactored
			// AS 2/17/11 NA 2011.1 Word Writer
			// These calculations were incorrect. Refactored into simpler routine and accounted 
			// for the new unit types.
			//
			//switch (unitType)
			//{
			//    case DeviceUnitType.DeviceIndependentUnit:
			//        int pixelValue = Utilities.ConvertFromLogicalPixels(this.Value);
			//
			//        // TODO: Does this need to take into account DPI?
			//        int pointValue = (int)Math.Round(pixelValue * (72.0 / 96), MidpointRounding.AwayFromZero);
			//
			//        switch (this.UnitType)
			//        {
			//            case DeviceUnitType.Pixel:
			//                return pixelValue;
			//
			//            case DeviceUnitType.Point:
			//                return pointValue;
			//
			//            case DeviceUnitType.Twip:
			//                return pointValue * 20;
			//        }
			//        break;
			//
			//    case DeviceUnitType.Pixel:
			//        // TODO: Does this need to take into account DPI?
			//        int sizeInPoints = (int)Math.Round(this.Value * (72.0 / 96), MidpointRounding.AwayFromZero);
			//
			//        switch (this.UnitType)
			//        {
			//            case DeviceUnitType.DeviceIndependentUnit:
			//                return Utilities.ConvertToLogicalPixels((int)this.Value);
			//
			//            case DeviceUnitType.Point:
			//                return sizeInPoints;
			//
			//            case DeviceUnitType.Twip:
			//                return sizeInPoints * 20;
			//        }
			//        break;
			//
			//    case DeviceUnitType.Point:
			//        // TODO: Does this need to take into account DPI?
			//        int ptInPixels = (int)Math.Round(this.Value * (96.0 / 72), MidpointRounding.AwayFromZero);
			//
			//        switch (this.UnitType)
			//        {
			//            case DeviceUnitType.DeviceIndependentUnit:
			//                return Utilities.ConvertToLogicalPixels(ptInPixels);
			//
			//            case DeviceUnitType.Pixel:
			//                return ptInPixels;
			//
			//            case DeviceUnitType.Twip:
			//                return (int)Math.Round(this.Value / 20, MidpointRounding.AwayFromZero);
			//        }
			//        break;
			//
			//    case DeviceUnitType.Twip:
			//        int twInPoints = (int)Math.Round(this.Value * 20, MidpointRounding.AwayFromZero);
			//
			//        // TODO: Does this need to take into account DPI?
			//        int twInPixels = (int)Math.Round(twInPoints * (96.0 / 72), MidpointRounding.AwayFromZero); 
			//
			//        switch (this.UnitType)
			//        {
			//            case DeviceUnitType.DeviceIndependentUnit:
			//                return Utilities.ConvertToLogicalPixels(twInPixels);
			//
			//            case DeviceUnitType.Pixel:
			//                return twInPixels;
			//
			//            case DeviceUnitType.Point:
			//                return twInPoints;
			//        }
			//        break;
			//}
			//
			//Debug.Fail("Unknown unit type");
			//return this.Value;
			#endregion //Refactored
			double twips = ConvertToTwips(this.Value, this.UnitType);
			return ConvertFromTwips(twips, unitType);
		}
		#endregion //ConvertToUnitType

		#endregion //Public Methods

		#region Internal Methods

		// AS 2/17/11 NA 2011.1 Word Writer
		#region ConvertFromTwips
		internal static double ConvertFromTwips(double twips, DeviceUnitType targetUnit)
		{
			switch (targetUnit)
			{
				case DeviceUnitType.Centimeter:
					return (twips / TwipsPerInch) * CentimetersPerInch;
				case DeviceUnitType.DeviceIndependentUnit:
					return (twips / TwipsPerInch) * LogicalDpi;
				case DeviceUnitType.Inch:
					return twips / TwipsPerInch;
				case DeviceUnitType.Pixel:
					{
						// convert to logical dpi
						double value = (twips / TwipsPerInch) * LogicalDpi;

						// then convert to device coords
						var matrix = Utilities.GetDeviceMatrix(true, null);
						return matrix.M22 * value;
					}
				case DeviceUnitType.Point:
					return twips / TwipsPerPoint;
				case DeviceUnitType.Twip:
					return twips;
				default:
					Debug.Fail("Unrecognized unit:" + targetUnit.ToString());
					return twips;
			}
		}
		#endregion //ConvertFromTwips

		// AS 2/17/11 NA 2011.1 Word Writer
		#region ConvertToTwips
		internal static double ConvertToTwips(double value, DeviceUnitType sourceUnit)
		{
			switch (sourceUnit)
			{
				case DeviceUnitType.Centimeter:
					return (value / CentimetersPerInch) * TwipsPerInch;
				case DeviceUnitType.DeviceIndependentUnit:
					return (value / LogicalDpi) * TwipsPerInch;
				case DeviceUnitType.Inch:
					return (value * TwipsPerInch);
				case DeviceUnitType.Pixel:
					{
						// convert to logical pixels
						var matrix = Utilities.GetDeviceMatrix(false, null);
						value = matrix.M11 * value;
						return (value / LogicalDpi) * TwipsPerInch;
					}
				case DeviceUnitType.Point:
					return value * TwipsPerPoint;
				case DeviceUnitType.Twip:
					return value;
				default:
					Debug.Fail("Unrecognized unit:" + sourceUnit.ToString());
					return value;
			}
		}
		#endregion //ConvertToTwips

		// AS 2/17/11 NA 2011.1 Word Writer
		#region ValidateUnitLengthValue
		internal static void ValidateUnitLengthValue(double value)
		{
			Utilities.ValidateIsNotNan(value);
			Utilities.ValidateIsNotInfinity(value);
			Utilities.ValidateIsNotNegative(value);
		} 
		#endregion //ValidateUnitLengthValue

		#endregion //Internal Methods

        #endregion //Methods

        #region Base class overrides

        /// <summary>
        /// Compares the specified <see cref="DeviceUnitLength"/> to this instance's values.
        /// </summary>
        /// <param name="obj">The object to compare to the current instance</param>
        /// <returns>True if the object is a DeviceUnitLength with the same <see cref="Value"/> and <see cref="UnitType"/></returns>
        public override bool Equals(object obj)
        {
            if (obj is DeviceUnitLength)
                return this.Equals((DeviceUnitLength)obj);

            return false;
        }

        /// <summary>
        /// Returns a hash value for the <see cref="DeviceUnitLength"/>
        /// </summary>
        /// <returns>A hash value for the DeviceUnitLength</returns>
        public override int GetHashCode()
        {
            return _unit.GetHashCode() | _value.GetHashCode();
        }
        #endregion //Base class overrides

        #region Operator Overloads

        /// <summary>
        /// Compares two <see cref="DeviceUnitLength"/> instances to determine if they are equal
        /// </summary>
        /// <param name="length1">The first DeviceUnitLength instance</param>
        /// <param name="length2">The second DeviceUnitLength instance</param>
        /// <returns>True if the objects have the same <see cref="Value"/> and <see cref="UnitType"/>; otherwise false.</returns>
        public static bool operator ==(DeviceUnitLength length1, DeviceUnitLength length2)
        {
            return length1._unit == length2._unit &&
                length1._value.Equals(length2._value);
        }

        /// <summary>
        /// Compares two <see cref="DeviceUnitLength"/> instances to determine if they are not equal
        /// </summary>
        /// <param name="length1">The first DeviceUnitLength instance</param>
        /// <param name="length2">The second DeviceUnitLength instance</param>
        /// <returns>True if the objects have a different <see cref="Value"/> and/or <see cref="UnitType"/>; otherwise false.</returns>
        public static bool operator !=(DeviceUnitLength length1, DeviceUnitLength length2)
        {
            return !(length1 == length2);
        }

        #endregion //Operator Overloads

        #region Properties

        /// <summary>
        /// Returns a boolean indicating if the DeviceUnitType represents a Deveice Independant Unit.
        /// </summary>
        public bool IsDeviceIndependentUnit
        {
            get { return _unit == DeviceUnitType.DeviceIndependentUnit; }
        }

        /// <summary>
        /// Returns a boolean indicating if the DeviceUnitType represents an absolute pixel value.
        /// </summary>
        public bool IsPixel
        {
            get { return _unit == DeviceUnitType.Pixel; }
        }

        /// <summary>
        /// Returns a boolean indicating if the DeviceUnitType represents an absolute point value.
        /// </summary>
        public bool IsPoint
        {
            get { return _unit == DeviceUnitType.Point; }
        }

        /// <summary>
        /// Returns a boolean indicating if the DeviceUnitType represents an absolute twip value.
        /// </summary>
        public bool IsTwip
        {
            get { return _unit == DeviceUnitType.Twip; }
        }

        /// <summary>
        /// Returns an enumeration indicating the type of <see cref="Value"/> that the DeviceUnitLength represents.
        /// </summary>
        public DeviceUnitType UnitType
        {
            get { return _unit; }
            set { this._unit = value; }
        }

        /// <summary>
        /// Returns the numeric value of the DeviceUnitLength.
        /// </summary>
        public double Value
        {
            get { return _value; }
			// AS 2/17/11 NA 2011.1 Word Writer
			// We validated in the ctor but not here.
			//
			//set { this._value = value; }
            set 
			{
				ValidateUnitLengthValue(value);
				this._value = value; 
			}
        }
        #endregion //Properties

        #region IEquatable<DeviceUnitLength> Members

        /// <summary>
        /// Compares the value of this DeviceUnitLength instance to the specified <see cref="DeviceUnitLength"/>.
        /// </summary>
        /// <param name="other">The instance of the <see cref="DeviceUnitLength"/> to compare</param>
        /// <returns>True if the specified DeviceUnitLength has the same <see cref="Value"/> and <see cref="UnitType"/>; otherwise false.</returns>
        public bool Equals(DeviceUnitLength other)
        {
            return this == other;
        }

        #endregion //IEquatable<DeviceUnitLength> Members
    }

    /// <summary>
    /// Converts <see cref="DeviceUnitLength"/> instances to and from other types.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class DeviceUnitLengthConverter : TypeConverter
    {
		#region Member Variables

		// AS 2/17/11 NA 2011.1 Word Writer
		private static Dictionary<DeviceUnitType, string> _suffixTable; 

		#endregion //Member Variables

		#region Constructor
		static DeviceUnitLengthConverter()
		{
			var suffixTable = new Dictionary<DeviceUnitType, string>();
			suffixTable[DeviceUnitType.Centimeter] = "cm";
			suffixTable[DeviceUnitType.Inch] = "in";
			suffixTable[DeviceUnitType.DeviceIndependentUnit] = string.Empty;
			suffixTable[DeviceUnitType.Pixel] = "px";
			suffixTable[DeviceUnitType.Point] = "pt";
			suffixTable[DeviceUnitType.Twip] = "twip";

			_suffixTable = suffixTable;
		}

		/// <summary>
		/// Initializes a new <see cref="DeviceUnitLengthConverter"/>
		/// </summary>
		public DeviceUnitLengthConverter()
		{
		}
		#endregion //Constructor

        #region Base class overrides

        /// <summary>
        /// Determines whether an object of the specified source type can be converted to a <see cref="DeviceUnitLength"/>
        /// </summary>
        /// <param name="context">Provides additional information about the operation</param>
        /// <param name="sourceType">The type from which the conversion could occur</param>
        /// <returns>True for numeric types; otherwise false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
			// AS 2/17/11 NA 2011.1 Word Writer
			// We supported going to string but not from it
			//
			if (sourceType == typeof(string))
				return true;

			if (Utilities.IsNumericType(sourceType))
                return true;

            return false;
        }

        /// <summary>
        /// Converts the specified value to a <see cref="DeviceUnitLength"/>
        /// </summary>
        /// <param name="context">Provides additional information about the operation</param>
        /// <param name="culture">Culture information used to create the <see cref="DeviceUnitLength"/> instance</param>
        /// <param name="value">The value being converted</param>
        /// <returns>An instance of a <see cref="DeviceUnitLength"/></returns>
        /// <exception cref="ArgumentNullException">The value is null.</exception>
        /// <exception cref="ArgumentException">The value provided cannot be converted to a DeviceUnitLength.</exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                throw new ArgumentException("value");

			// AS 2/17/11 NA 2011.1 Word Writer
			// We supported going to string but not from it
			//
			if (value is string)
				return FromString(value as string, culture);

			// AS 2/17/11 NA 2011.1 Word Writer
			// We should validate the value type provided and not assume its a numeric type.
			//
			if (!Utilities.IsNumericType(value.GetType()))
				throw this.GetConvertFromException(value);

            object convertedValue = Utilities.ConvertDataValue(value, typeof(double), culture, null);
            double dblValue = Convert.ToDouble(convertedValue);

            if (double.IsNaN(dblValue))
                return new DeviceUnitLength(0);
            else
                return new DeviceUnitLength(dblValue);            
        }

        /// <summary>
        /// Determines whether a <see cref="DeviceUnitLength"/> instance can be converted to the specified type
        /// </summary>
        /// <param name="context">Provides additional information about the operation</param>
        /// <param name="destinationType">The type to which the DeviceUnitLength should be converted</param>
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
        /// Converts a <see cref="DeviceUnitLength"/> instance to the specified type
        /// </summary>
        /// <param name="context">Provides additional information about the operation</param>
        /// <param name="culture">Culture information used to create the <see cref="DeviceUnitLength"/> instance</param>
        /// <param name="value">The <see cref="DeviceUnitLength"/> being converted</param>
        /// <param name="destinationType">The type to which the DeviceUnitLength should be converted</param>
        /// <returns>An instance of the specified type that represents the specified DeviceUnitLength</returns>
        /// <exception cref="ArgumentNullException">The value or destinationType provided is null.</exception>
        /// <exception cref="ArgumentException">The value provided is not a DeviceUnitLength or the destination type is not supported.</exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
                throw new ArgumentException("value");

            if (destinationType == null)
                throw new ArgumentException("destinationType");            

            if (value is DeviceUnitLength == false)
                throw base.GetConvertToException(value, destinationType);

            DeviceUnitLength fl = (DeviceUnitLength)value;

			if (destinationType == typeof(string))
			{
				// AS 2/17/11 NA 2011.1 Word Writer
				// The unit is getting lost. We need to include that and we need to 
				// be able to parse it.
				//
				//return fl.Value.ToString(culture != null ? culture.NumberFormat : null);
				return ToString(fl, culture);
			}

            if (destinationType == typeof(InstanceDescriptor))
            {
                ConstructorInfo ci = typeof(DeviceUnitLength).GetConstructor(new Type[] { typeof(double), typeof(DeviceUnitType) });
                return new InstanceDescriptor(ci, new object[] { fl.Value, fl.UnitType }, true);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
        #endregion //Base class overrides

        #region Methods

		// AS 2/17/11 NA 2011.1 Word Writer
		#region FromString
		internal static DeviceUnitLength FromString(string strValue, CultureInfo culture)
		{
			DeviceUnitType unit = DeviceUnitType.DeviceIndependentUnit;
			ParseUnit(ref strValue, out unit);

			return new DeviceUnitLength(Convert.ToDouble(strValue, culture), unit);
		} 
		#endregion //FromString

		#region GetSuffix
		internal static string GetSuffix(DeviceUnitType unitType)
		{
			return _suffixTable[unitType];
		}
		#endregion //GetSuffix

		#region ParseUnit
		internal static void ParseUnit(ref string strValue, out DeviceUnitType unit)
		{
			unit = DeviceUnitType.DeviceIndependentUnit;

			foreach (var pair in _suffixTable)
			{
				if (!string.IsNullOrEmpty(pair.Value) && strValue.EndsWith(pair.Value))
				{
					unit = pair.Key;
					strValue = strValue.Substring(0, strValue.Length - pair.Value.Length);
					break;
				}
			}
		} 
		#endregion //ParseUnit

		// AS 2/17/11 NA 2011.1 Word Writer
		#region ToString
		internal static string ToString(DeviceUnitLength length, CultureInfo culture)
		{
			string unit = GetSuffix(length.UnitType);
			string value = Convert.ToString(length.Value, culture);

			if (!string.IsNullOrEmpty(unit))
				value += unit;

			return value;
		} 
		#endregion //ToString

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