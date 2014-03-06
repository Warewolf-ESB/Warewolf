
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Converts Double to CornerRadius
    /// </summary>
    [ValueConversion(typeof(double), typeof(CornerRadius))]
    public class DoubleToCornerRadiusConverter : IValueConverter
    {
        #region Methods

        /// <summary>
        /// Converts a value. The data binding engine calls this method when it propagates a value from the binding source to the binding target. 
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            string par = (string)parameter;
            string [] flags = par.Split('-');
            double radius = (double)value;
            CornerRadius corner = new CornerRadius(0,0,0,0);

            if (flags.Length > 0 && flags[0]=="1")
            {
                corner.TopLeft = radius;
            }

            if (flags.Length > 1 && flags[1] == "1")
            {
                corner.TopRight = radius;
            }

            if (flags.Length > 2 && flags[2] == "1")
            {
                corner.BottomRight = radius;
            }

            if (flags.Length > 3 && flags[3] == "1")
            {
                corner.BottomLeft = radius;
            }

            return corner;
        }

        /// <summary>
        /// Converts a value. The data binding engine calls this method when it propagates a value from the binding target to the binding source.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CornerRadius radius = (CornerRadius)value;
            return radius.TopLeft;
        }

        #endregion Methods
    }


    /// <summary>
    /// This class is used to create custom data point appearance using data templates. 
    /// </summary>
    public class DataPointTemplate : Infragistics.PropertyChangeNotifier
    {
        #region Fields

        // Private fields
        private Brush _fill = new SolidColorBrush(Colors.White);
        private Brush _stroke = new SolidColorBrush(Colors.Black);
        private double _strokeThickness;
        private object _toolTip;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the interior of the shape. 
        /// </summary>
        //[Description("Gets or sets the Brush that specifies how to paint the interior of the shape. ")]
        public Brush Fill
        {
            get { return _fill; }
            set { _fill = value; this.RaisePropertyChangedEvent("Fill");  }
        }

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the Shape outline.
        /// </summary>
        //[Description("Gets or sets the Brush that specifies how to paint the Shape outline.")]
        public Brush Stroke
        {
            get { return _stroke; }
            set { _stroke = value; this.RaisePropertyChangedEvent("Stroke"); }
        }

        /// <summary>
        /// Gets or sets the tool-tip object that is displayed for this element.
        /// </summary>
        //[Description("Gets or sets the tool-tip object that is displayed for this element.")]
        public object ToolTip
        {
            get { return _toolTip; }
            set { _toolTip = value; this.RaisePropertyChangedEvent("ToolTip"); }
        }

        /// <summary>
        /// Gets or sets the width of the Shape outline. 
        /// </summary>
        //[Description("Gets or sets the width of the Shape outline.")]
        public double StrokeThickness
        {
            get { return _strokeThickness; }
            set { _strokeThickness = value; this.RaisePropertyChangedEvent("StrokeThickness"); }
        }

        #endregion Properties
    }

    /// <summary>
    /// This class is used to create custom data point appearance using data templates. 
    /// </summary>
    public class ColumnChartTemplate : DataPointTemplate
    {
        #region Fields

        // Private fields
        private double _rectangleRounding;
        private CornerRadius _cornerRadius;
        private Thickness _borderThickness;
        private bool _isNegative = false;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value that indicates whether the data point value is negative.
        /// </summary>
        //[Description("Gets or sets a value that indicates whether the data point value is negative.")]
        public bool IsNegative
        {
            get { return _isNegative; }
            set { _isNegative = value; this.RaisePropertyChangedEvent("IsNegative"); }
        }

        /// <summary>
        /// Gets or sets radius of the ellipse that is used to round the corners of the 
        /// rectangle. Used for 2D chart which data point shape is rectangle (Column, Bar, 
        /// Stacked Column, Candlestick, etc.)
        /// </summary>
        //[Description("Gets or sets radius of the ellipse that is used to round the corners of the rectangle.")]
        public double RectangleRounding
        {
            get { return _rectangleRounding; }
            set { _rectangleRounding = value; this.RaisePropertyChangedEvent("RectangleRounding"); }
        }

        /// <summary>
        /// Gets or sets radius of the ellipse that is used to round the corners of the 
        /// rectangle. Used for 2D chart which data point shape is rectangle (Column, Bar, 
        /// Stacked Column, Candlestick, etc.)
        /// </summary>
        //[Description("Gets or sets radius of the ellipse that is used to round the corners of the rectangle.")]
        public CornerRadius CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value; this.RaisePropertyChangedEvent("CornerRadius"); }
        }

        /// <summary>
        /// Gets or sets the width of the Shape outline. 
        /// </summary>
        //[Description("Gets or sets the width of the Shape outline.")]
        public Thickness BorderThickness
        {
            get { return _borderThickness; }
            set { _borderThickness = value; this.RaisePropertyChangedEvent("BorderThickness"); }
        }

        #endregion Properties
    }

    /// <summary>
    /// This class is used to create custom data point appearance using data templates. 
    /// </summary>
    public class BarChartTemplate : DataPointTemplate
    {
        #region Fields

        // Private fields
        private double _rectangleRounding;
        private CornerRadius _cornerRadius;
        private Thickness _borderThickness;
        private bool _isNegative = false;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value that indicates whether the data point value is negative.
        /// </summary>
        //[Description("Gets or sets a value that indicates whether the data point value is negative.")]
        public bool IsNegative
        {
            get { return _isNegative; }
            set { _isNegative = value; this.RaisePropertyChangedEvent("IsNegative"); }
        }

        /// <summary>
        /// Gets or sets radius of the ellipse that is used to round the corners of the 
        /// rectangle. Used for 2D chart which data point shape is rectangle (Column, Bar, 
        /// Stacked Column, Candlestick, etc.)
        /// </summary>
        //[Description("Gets or sets radius of the ellipse that is used to round the corners of the rectangle.")]
        public double RectangleRounding
        {
            get { return _rectangleRounding; }
            set { _rectangleRounding = value; this.RaisePropertyChangedEvent("RectangleRounding"); }
        }

        /// <summary>
        /// Gets or sets radius of the ellipse that is used to round the corners of the 
        /// rectangle. Used for 2D chart which data point shape is rectangle (Column, Bar, 
        /// Stacked Column, Candlestick, etc.)
        /// </summary>
        //[Description("Gets or sets radius of the ellipse that is used to round the corners of the rectangle.")]
        public CornerRadius CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value; this.RaisePropertyChangedEvent("CornerRadius"); }
        }

        /// <summary>
        /// Gets or sets the width of the Shape outline. 
        /// </summary>
        //[Description("Gets or sets the width of the Shape outline.")]
        public Thickness BorderThickness
        {
            get { return _borderThickness; }
            set { _borderThickness = value; this.RaisePropertyChangedEvent("BorderThickness"); }
        }

        #endregion Properties
    }

    /// <summary>
    /// This class is used to create custom data point appearance using data templates. 
    /// </summary>
    public class AreaChartTemplate : DataPointTemplate
    {
        #region Fields

        // Private fields
        private Point _point1;
        private Point _point2;
        private Point _point3;
        private Point _point4;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The 1. area point.
        /// </summary>
        public Point Point1
        {
            get { return _point1; }
            set { _point1 = value; this.RaisePropertyChangedEvent("Point1"); }
        }

        /// <summary>
        /// The 2. area point.
        /// </summary>
        public Point Point2
        {
            get { return _point2; }
            set { _point2 = value; this.RaisePropertyChangedEvent("Point2"); }
        }

        /// <summary>
        /// The 3. area point.
        /// </summary>
        public Point Point3
        {
            get { return _point3; }
            set { _point3 = value; this.RaisePropertyChangedEvent("Point3"); }
        }

        /// <summary>
        /// The 4. area point.
        /// </summary>
        public Point Point4
        {
            get { return _point4; }
            set { _point4 = value; this.RaisePropertyChangedEvent("Point4"); }
        }

        #endregion Properties

    }

    /// <summary>
    /// This class is used to create custom data point appearance using data templates. 
    /// </summary>
    internal class LineChartTemplate : DataPointTemplate
    {
        #region Fields

        // Private fields
        private Point _point1;
        private Point _point2;
        private Point _regionPoint1;
        private Point _regionPoint2;
        private Point _regionPoint3;
        private Point _regionPoint4;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The 1. line point.
        /// </summary>
        public Point Point1
        {
            get { return _point1; }
            set { _point1 = value; this.RaisePropertyChangedEvent("Point1"); }
        }

        /// <summary>
        /// The 2. line point.
        /// </summary>
        public Point Point2
        {
            get { return _point2; }
            set { _point2 = value; this.RaisePropertyChangedEvent("Point2"); }
        }

        /// <summary>
        /// The region line 1. point.
        /// </summary>
        public Point RegionPoint1
        {
            get { return _regionPoint1; }
            set { _regionPoint1 = value; this.RaisePropertyChangedEvent("RegionPoint1"); }
        }

        /// <summary>
        /// The region line 2. point.
        /// </summary>
        public Point RegionPoint2
        {
            get { return _regionPoint2; }
            set { _regionPoint2 = value; this.RaisePropertyChangedEvent("RegionPoint2"); }
        }

        /// <summary>
        /// The region line 3. point.
        /// </summary>
        public Point RegionPoint3
        {
            get { return _regionPoint3; }
            set { _regionPoint3 = value; this.RaisePropertyChangedEvent("RegionPoint3"); }
        }

        /// <summary>
        /// The region line 4. point.
        /// </summary>
        public Point RegionPoint4
        {
            get { return _regionPoint4; }
            set { _regionPoint4 = value; this.RaisePropertyChangedEvent("RegionPoint4"); }
        }
      
        #endregion Properties
    }

    /// <summary>
    /// This class is used to create custom data point appearance using data templates. 
    /// </summary>
    public class BubbleChartTemplate : DataPointTemplate
    {
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