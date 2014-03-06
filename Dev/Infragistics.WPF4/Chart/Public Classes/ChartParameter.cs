
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Globalization;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Provides a base set of parameters for specific chart 
    /// types. This class is used to decrese number of public 
    /// properties used in series and data points.  Used for numerous 
    /// number of parameters which are different for every chart type.
    /// </summary>
    public class ChartParameter : DependencyObject
    {
        #region Fields

        // Private fields
        private object _chartParent;

        #endregion Fields

        #region Internal Properties

        /// <summary>
        /// The parent object
        /// </summary>
        internal object ChartParent
        {
            get
            {
                return _chartParent;
            }
            set
            {
                _chartParent = value;
            }
        }

        #endregion Internal Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the ChartParameter class. 
        /// </summary>
        public ChartParameter()
        {
        }

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamChart control = XamChart.GetControl(d);
            if (control != null && e.NewValue != e.OldValue)
            {
                Validate(d, e);
                control.RefreshProperty();
            }
        }

        /// <summary>
        /// Validate Chart parameter properties
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void Validate(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChartParameterType type = (ChartParameterType)d.GetValue(TypeProperty);
            object value = (object)d.GetValue(ValueProperty);
            int intValue;
            double doubleValue;
            
            // Validate Value property
            if (value != null && value != (object)"")
            {
                switch (type)
                {
                    case ChartParameterType.Column3DNumberOfSides:
                        intValue = GetInt(value, type);
                        if (intValue < 3 || intValue > 8)
                        {
                            // The value for �Column3DNumberOfSides� chart parameter has to be between 3 and 8.
                            throw new ArgumentOutOfRangeException("Value", intValue, ErrorString.Exc21);
                        }
                        break;
                    case ChartParameterType.Column3DStarNumberOfSides:
                        intValue = GetInt(value, type);
                        if (intValue < 4 || intValue > 8)
                        {
                            // The value for �Column3DStarNumberOfSides� chart parameter has to be between 4 and 8.
                            throw new ArgumentOutOfRangeException("Value", intValue, ErrorString.Exc22);
                        }
                        break;
                    case ChartParameterType.ExplodedRadius:
                        doubleValue = GetDouble(value, type);
                        if (doubleValue < 0 || doubleValue > 1)
                        {
                            // The value for �ExplodedRadius� chart parameter has to be between 0 and 1.
                            throw new ArgumentOutOfRangeException("Value", doubleValue, ErrorString.Exc23);
                        }
                        break;
                    case ChartParameterType.Pie3DRounding:
                        doubleValue = GetDouble(value, type);
                        if (doubleValue < 1 || doubleValue > 3)
                        {
                            // The value for �Pie3DRounding� chart parameter has to be between 1 and 3. Default value is 2.
                            throw new ArgumentOutOfRangeException("Value", doubleValue, ErrorString.Exc25);
                        }
                        break;
                    case ChartParameterType.EdgeSize3D:
                        doubleValue = GetDouble(value, type);
                        if (doubleValue < 0 || doubleValue > 5)
                        {
                            // The value for �EdgeSize3D� chart parameter has to be between 0 and 5. Default value is 1.
                            throw new ArgumentOutOfRangeException("Value", doubleValue, ErrorString.Exc24);
                        }
                        break;
                    case ChartParameterType.Radius:
                        doubleValue = GetDouble(value, type);
                        if (doubleValue < 0)
                        {
                            // The value for �Radius� chart parameter has to be Greater or equal to 0. Default value is 0.
                            throw new ArgumentOutOfRangeException("Value", doubleValue, ErrorString.Exc26);
                        }
                        break;
                    case ChartParameterType.RectangleRounding:
                        doubleValue = GetDouble(value, type);
                        if (doubleValue < 0)
                        {
                            // The value for �RectangleRounding� chart parameter has to be greater or equal to 0. Default value is 0.
                            throw new ArgumentOutOfRangeException("Value", doubleValue, ErrorString.Exc53);
                        }
                        break;
                    case ChartParameterType.PointWidth:
                        doubleValue = GetDouble(value, type);
                        if (doubleValue < 0 || doubleValue > 2)
                        {
                            // The value for �PointWidth� chart parameter has to be between 0 and 2. Default value is 1.
                            throw new ArgumentOutOfRangeException("Value", doubleValue, ErrorString.Exc54);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Returns value from object if possible.
        /// </summary>
        /// <param name="value">Chart parameter value as an object.</param>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Returns value as strong type.</returns>
        private static Brush GetBrush(object value, ChartParameterType type)
        {
            if (value is Brush)
            {
                return value as Brush;
            }

            if (value == null || (value is string && String.IsNullOrEmpty(value as string)))
            {
                return null;
            }

            BrushConverter brushConv = new BrushConverter();
            // Integer Types
            Brush brushValue;
            try
            {
                brushValue = brushConv.ConvertFromInvariantString((string)value) as Brush;
            }
            catch (Exception e)
            {
                // Value from a Chart parameter cannot be converted to Brush.
                throw new ArgumentException(ErrorString.Exc27, type.ToString(), e);
            }

            return brushValue;
        }

        /// <summary>
        /// Returns value from object if possible.
        /// </summary>
        /// <param name="value">Chart parameter value as an object.</param>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Returns value as strong type.</returns>
        private static Animation GetAnimation(object value, ChartParameterType type)
        {
            if (value is Animation)
            {
                return value as Animation;
            }

            return null;
        }

       
        /// <summary>
        /// Returns value from object if possible.
        /// </summary>
        /// <param name="value">Chart parameter value as an object.</param>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Returns value as strong type.</returns>
        private static int GetInt(object value, ChartParameterType type)
        {
            if (value == null || (value is string && String.IsNullOrEmpty(value as string)))
            {
                return int.MinValue;
            }

            // Integer Types
            int intValue;
            try
            {
                CultureInfo cultureToUse = CultureInformation.CultureToUse;

                if (value is int)
                {
                    intValue = (int)value;
                }
                else if (value is string)
                {
                    intValue = int.Parse((string)value, cultureToUse);
                }
                else
                {
                    intValue = int.Parse(value.ToString(), cultureToUse);
                }
            }
            catch(Exception e)
            {
                // Value from a Chart parameter cannot be converted to integer value.
                throw new ArgumentException(ErrorString.Exc28, type.ToString(), e);
            }

            return intValue;
        }

        /// <summary>
        /// Returns value from object if possible.
        /// </summary>
        /// <param name="value">Chart parameter value as an object.</param>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Returns value as strong type.</returns>
        private static bool GetBool(object value, ChartParameterType type)
        {
            if (value == null)
            {
                return true;
            }
            else if (value is string && String.IsNullOrEmpty((string)value))
            {
                return true;
            }

            // Bool Types
            bool boolValue;
            try
            {
                if (value is bool)
                {
                    boolValue = (bool)value;
                }
                else if (value is string)
                {
                    boolValue = bool.Parse((string)value);
                }
                else
                {
                    boolValue = bool.Parse(value.ToString());
                }
            }
            catch (Exception e)
            {
                // Value from a Chart parameter cannot be converted to boolean value.
                throw new ArgumentException(ErrorString.Exc29, type.ToString(), e);
            }

            return boolValue;
        }

        /// <summary>
        /// Returns value from object if possible.
        /// </summary>
        /// <param name="value">Chart parameter value as an object.</param>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Returns value as strong type.</returns>
        private static double GetDouble(object value, ChartParameterType type)
        {
            bool isDateType;
            return GetDouble(value, type, out isDateType);
        }


        /// <summary>
        /// Returns value from object if possible.
        /// </summary>
        /// <param name="value">Chart parameter value as an object.</param>
        /// <param name="type">Chart parameter type</param>
        /// <param name="isDateTime">True if value is Date Time</param>
        /// <returns>Returns value as strong type.</returns>
        private static double GetDouble(object value, ChartParameterType type, out bool isDateTime)
        {
            isDateTime = false;

            if (value == null || (value is string && String.IsNullOrEmpty(value as string)))
            {
                return double.NaN;
            }

            // Double Types
            double doubleValue;
            try
            {
                // [DN 6/16/2008:BR33929] we are parsing this value, not displaying it.  we should use InvariantCulture.
                CultureInfo cultureToUse = CultureInfo.InvariantCulture;

                if (value is double)
                {
                    doubleValue = (double)value;
                }
                else if (value is DateTime)
                {
                    DateTime dateTime = (DateTime)value;
                    doubleValue = dateTime.ToOADate();
                    isDateTime = true;
                }
                else if (value is string)
                {
                    double result;
                    bool parseSuccessful = double.TryParse((string)value, NumberStyles.Any, cultureToUse, out result);

                    if (parseSuccessful)
                    {
                        doubleValue = double.Parse((string)value, cultureToUse);
                    }
                    else
                    {
                        DateTime dateTime = DateTime.Parse((string)value, cultureToUse);
                        doubleValue = dateTime.ToOADate();
                        isDateTime = true;
                    }
                }
                else
                {
                    doubleValue = double.Parse(value.ToString(), cultureToUse);
                }
            }
            catch (Exception e)
            {
                // Value from a Chart parameter cannot be converted to double value.
                throw new ArgumentException(ErrorString.Exc30, type.ToString(), e);
            }

            return doubleValue;
        }

        /// <summary>
        /// Returns value for chart parameters. If not set default value is returned
        /// </summary>
        /// <returns>Chart parameter value.</returns>
        internal Brush GetDefaultBrush()
        {
            if (Value == null || Value == (object)"" || GetBrush(Value, Type) == null)
            {
                switch (Type)
                {
                    case ChartParameterType.CandlestickNegativeFill:
                        return Brushes.Black;
                    case ChartParameterType.CandlestickNegativeStroke:
                        return Brushes.Gray;
                }
            }

            return GetBrush(Value, Type);
        }

        /// <summary>
        /// Returns value for chart parameters. If not set default value is returned
        /// </summary>
        /// <returns>Chart parameter value.</returns>
        internal Animation GetDefaultAnimation()
        {
            if (Value == null || Value == (object)"" || GetAnimation(Value, Type) == null)
            {
                switch (Type)
                {
                    case ChartParameterType.ExplodedAnimation:
                        return null;
                }
            }

            return GetAnimation(Value, Type);
        }

        /// <summary>
        /// Returns value for chart parameters. If not set default value is returned
        /// </summary>
        /// <returns>Chart parameter value.</returns>
        internal int GetDefaultInt()
        {
            if (Value == null || Value == (object)"" || GetInt(Value, Type) == int.MinValue)
            {
                switch (Type)
                {
                    case ChartParameterType.Column3DNumberOfSides:
                        return 4;
                    case ChartParameterType.Column3DStarNumberOfSides:
                        return 5;
                }
            }

            return GetInt(Value, Type);
        }

        /// <summary>
        /// Returns value for chart parameters. If not set default value is returned.
        /// </summary>
        /// <returns>Chart parameter value.</returns>
        internal bool GetDefaultBool()
        {
            if (Value == null || Value == (object)"")
            {
                return true;
            }

            return GetBool(Value, Type);
        }

        /// <summary>
        /// Returns value for chart parameters. If not set default value is returned.
        /// </summary>
        /// <returns>Chart parameter value.</returns>
        internal double GetDefaultDouble()
        {
            bool isDateTime;
            return GetDefaultDouble(out isDateTime);
        }
        
        /// <summary>
        /// Returns value for chart parameters. If not set default value is returned.
        /// </summary>
        /// <param name="isDateTime">True if value is Date Time</param>
        /// <returns>Chart parameter value.</returns>
        internal double GetDefaultDouble(out bool isDateTime)
        {
            isDateTime = false;
            if (Value == null || Value == (object)"" || double.IsNaN(GetDouble(Value, Type)))
            {
                switch (Type)
                {
                    case ChartParameterType.ExplodedRadius:
                        return 0.2;
                    case ChartParameterType.EdgeSize3D:
                        return 1;
                    case ChartParameterType.Pie3DRounding:
                        return 2;
                    case ChartParameterType.ValueX:
                        return 0;
                    case ChartParameterType.ValueY:
                        return 0;
                    case ChartParameterType.ValueZ:
                        return 0;
                    case ChartParameterType.High:
                        return 0;
                    case ChartParameterType.Low:
                        return 0;
                    case ChartParameterType.Open:
                        return 0;
                    case ChartParameterType.Close:
                        return 0;
                    case ChartParameterType.Radius:
                        return 0;
                    case ChartParameterType.RectangleRounding:
                        return 0;
                    case ChartParameterType.PointWidth:
                        return 1;
                }
            }

            return GetDouble(Value, Type, out isDateTime);
        }
                   

        #endregion Methods

        #region Public Properties

        #region Value

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
            typeof(object), typeof(ChartParameter), new FrameworkPropertyMetadata((object)"", new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Chart Parameter value.
        /// </summary>
        /// <seealso cref="ValueProperty"/>
        //[Description("Chart Parameter value.")]
        //[Category("Data")]
        [TypeConverter(typeof(StringConverter))]
        public object Value
        {
            get
            {
                return (object)this.GetValue(ChartParameter.ValueProperty);
            }
            set
            {
                this.SetValue(ChartParameter.ValueProperty, value);
            }
        }

        #endregion Value               
      
        #region Type

        /// <summary>
        /// Identifies the <see cref="Type"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type",
            typeof(ChartParameterType), typeof(ChartParameter), new FrameworkPropertyMetadata(ChartParameterType.EdgeSize3D, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Chart Parameter Type
        /// </summary>
        /// <seealso cref="TypeProperty"/>
        //[Description("Chart Parameter Type.")]
        //[Category("Data")]
        public ChartParameterType Type
        {
            get
            {
                return (ChartParameterType)this.GetValue(ChartParameter.TypeProperty);
            }
            set
            {
                this.SetValue(ChartParameter.TypeProperty, value);
            }
        }

        #endregion Type

        #endregion Public Properties
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