
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Marker is a colored shape which shows exact value of a Data Point. Marker has 
    /// corresponding marker label. Used in combination with different chart types. Marker 
    /// class keeps information about marker shapes and corresponding label appearance. 
    /// Markers can be defined for series or data points. If Marker is not defined for 
    /// DataPoint, the marker from parent series is used.
    /// </summary>
    /// <remarks>
    /// Some chart types don�t use marker shapes or marker labels. Chart types without 
    /// Axis don�t have marker shapes (pie or doughnut charts). 3D Charts don�t have 
    /// marker shapes, they have marker labels only.
    /// </remarks>
    public class Marker : ChartFrameworkContentElement
    {
        #region Fields

        // Private Fields
        private object _chartParent;

        #endregion Fields

        #region Properties

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
                
        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the Marker class. 
        /// </summary>
        public Marker()
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
                control.RefreshProperty();
            }
        }

        #endregion Methods

        #region Public Properties

        #region Type

        /// <summary>
        /// Identifies the <see cref="Type"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type",
            typeof(MarkerType), typeof(Marker), new FrameworkPropertyMetadata(MarkerType.Circle, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets type of a marker shape.
        /// </summary>
        /// <seealso cref="Type"/>
        //[Description("Gets or sets type of a marker shape.")]
        //[Category("Appearance")]
        public MarkerType Type
        {
            get
            {
                return (MarkerType)this.GetValue(Marker.TypeProperty);
            }
            set
            {
                this.SetValue(Marker.TypeProperty, value);
            }
        }

        #endregion StrokeThickness	

        #region FontFamily

        /// <summary>
        /// Identifies the <see cref="FontFamily"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily",
            typeof(FontFamily), typeof(Marker), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the name of the specified font.
        /// </summary>
        /// <remarks>
        /// A FontFamily object specifying prefered font family. If font doesn�t exist, there is one or more fallback font families. The default value is the font determined by the MessageFontFamily metric. 
        /// </remarks>
        /// <seealso cref="FontFamilyProperty"/>
        //[Description("Gets or sets the name of the specified font.")]
        //[Category("Font")]
        public FontFamily FontFamily
        {
            get
            {
                return (FontFamily)this.GetValue(Marker.FontFamilyProperty);
            }
            set
            {
                this.SetValue(Marker.FontFamilyProperty, value);
            }
        }

        #endregion FontFamily

        #region FontSize

        /// <summary>
        /// Identifies the <see cref="FontSize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize",
            typeof(double), typeof(Marker), new FrameworkPropertyMetadata((double)SystemFonts.MessageFontSize, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnFontSizeValidate));

        /// <summary>
        /// Gets or sets the font size for Data Point Marker Label.
        /// </summary>
        /// <remarks>
        /// A double value specifying the desired font size to use in points (1 point = 1/72 of an inch = 96/72 device independent pixels). The default value is determined by the MessageFontSize metric.
        /// </remarks>
        /// <seealso cref="FontSizeProperty"/>
        //[Description("Gets or sets the font size for Data Point Marker Label.")]
        //[Category("Font")]
        [TypeConverterAttribute(typeof(FontSizeConverter))]
        public double FontSize
        {
            get
            {
                return (double)this.GetValue(Marker.FontSizeProperty);
            }
            set
            {
                this.SetValue(Marker.FontSizeProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnFontSizeValidate(object value)
        {
            double fontSize = (double)value;
            return (fontSize > 5);

        }

        #endregion FontSize

        #region FontStyle

        /// <summary>
        /// Identifies the <see cref="FontStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register("FontStyle",
            typeof(FontStyle), typeof(Marker), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the marker font style.
        /// </summary>
        /// <remarks>
        /// The default value for FontStyle is Normal.
        /// </remarks>
        /// <seealso cref="FontStyleProperty"/>
        //[Description("Gets or sets the marker font style.")]
        //[Category("Font")]
        public FontStyle FontStyle
        {
            get
            {
                return (FontStyle)this.GetValue(Marker.FontStyleProperty);
            }
            set
            {
                this.SetValue(Marker.FontStyleProperty, value);
            }
        }

        #endregion FontStyle

        #region FontWeight

        /// <summary>
        /// Identifies the <see cref="FontWeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight",
            typeof(FontWeight), typeof(Marker), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the weight or thickness of the specified font.
        /// </summary>
        /// <remarks>
        /// A FontWeight enumeration value. The default value is Normal. 
        /// </remarks>
        /// <seealso cref="FontWeightProperty"/>
        //[Description("Gets or sets the weight or thickness of the specified font.")]
        //[Category("Font")]
        public FontWeight FontWeight
        {
            get
            {
                return (FontWeight)this.GetValue(Marker.FontWeightProperty);
            }
            set
            {
                this.SetValue(Marker.FontWeightProperty, value);
            }
        }

        #endregion FontWeight

        #region FontStretch

        /// <summary>
        /// Identifies the <see cref="FontStretch"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register("FontStretch",
            typeof(FontStretch), typeof(Marker), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the degree to which a font is condensed or expanded on the screen.
        /// </summary>
        /// <remarks>
        /// A member of the FontStretch class specifying the desired font-stretching characteristics to use. The default value is Normal.
        /// </remarks>
        /// <seealso cref="FontStretchProperty"/>
        //[Description("Gets or sets the degree to which a font is condensed or expanded on the screen.")]
        //[Category("Font")]
        public FontStretch FontStretch
        {
            get
            {
                return (FontStretch)this.GetValue(Marker.FontStretchProperty);
            }
            set
            {
                this.SetValue(Marker.FontStretchProperty, value);
            }
        }

        #endregion FontStretch

        #region Foreground

        /// <summary>
        /// Identifies the <see cref="Foreground"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground",
            typeof(Brush), typeof(Marker), new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush to apply to the text contents of labels.
        /// </summary>
        /// <seealso cref="ForegroundProperty"/>
        //[Description("Gets or sets the Brush to apply to the text contents of labels.")]
        //[Category("Brushes")]
        public Brush Foreground
        {
            get
            {
                return (Brush)this.GetValue(Marker.ForegroundProperty);
            }
            set
            {
                this.SetValue(Marker.ForegroundProperty, value);
            }
        }


        #endregion Foreground

        #region Format

        /// <summary>
        /// Identifies the <see cref="Format"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format",
            typeof(string), typeof(Marker), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets data point label format using the composite string formatting.
        /// </summary>
        /// <remarks>
        /// This property is used to format data point label string. Data point label should use data point 
        /// value which will be formatted on specified way and inserted into a text. Data Label format uses 
        /// .NET Composite Formatting specification. The �{Value}� text is replaced with data point value. For Bubble, 
        /// scatter and stock chart types {ValueX}, {ValueY}, {Radius}, {High}, {Low}, etc. formats are used. For 
        /// example, to create a label which displays a text �Data point value is: 3� where 3 is data point 
        /// value, we have to create a following format string �Data point value is: {Value}�. Few more examples: 
        /// 1. �Product price is: $3.00 dollars� -> �Product price is: {Value:C} dollars�. 2. �Calculated value is: 
        /// 03.00� -> �Calculated value is: {Value:00.00}�. For more information about format property see �Composite 
        /// Formatting� in .NET Framework Developer�s Guide. 
        /// </remarks>
        /// <seealso cref="FormatProperty"/>
        //[Description("Gets or sets data point label format using the composite string formatting.")]
        //[Category("Miscellaneous")]
        public string Format
        {
            get
            {
                return (string)this.GetValue(Marker.FormatProperty);
            }
            set
            {
                this.SetValue(Marker.FormatProperty, value);
            }
        }


        #endregion Format

        #region MarkerSize

        /// <summary>
        /// Identifies the <see cref="MarkerSize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarkerSizeProperty = DependencyProperty.Register("MarkerSize",
            typeof(double), typeof(Marker), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnMarkerSizeValidate));

        /// <summary>
        /// Gets or sets the marker size. This is scaling factor applied to the default marker size. Value 1 is default size, a value between 0 and 1 decrease the default marker size and a value greater than 1 increase the default marker size.
        /// </summary>
        /// <seealso cref="MarkerSizeProperty"/>
        //[Description("Gets or sets the marker size. This is scaling factor applied to the default marker size. Value 1 is default size, a value between 0 and 1 decrease the default marker size and a value greater than 1 increase the default marker size.")]
        //[Category("Appearance")]
        public double MarkerSize
        {
            get
            {
                return (double)this.GetValue(Marker.MarkerSizeProperty);
            }
            set
            {
                this.SetValue(Marker.MarkerSizeProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnMarkerSizeValidate(object value)
        {
            double markerSize = (double)value;
            return (markerSize >= 0);
        }

        #endregion MarkerSize

        #region LabelDistance

        /// <summary>
        /// Identifies the <see cref="LabelDistance"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LabelDistanceProperty = DependencyProperty.Register("LabelDistance",
            typeof(double), typeof(Marker), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnLabelDistanceValidate));

        /// <summary>
        /// Gets or sets the label distance. This is scaling factor applied to the default label distance from the marker. Value 1 is default size, a value between 0 and 1 decrease the default distance and a value greater than 1 increase the default distance.
        /// </summary>
        /// <seealso cref="MarkerSizeProperty"/>
        //[Description("Gets or sets the label distance. This is scaling factor applied to the default label distance from the marker. Value 1 is default size, a value between 0 and 1 decrease the default distance and a value greater than 1 increase the default distance.")]
        //[Category("Appearance")]
        public double LabelDistance
        {
            get
            {
                return (double)this.GetValue(Marker.LabelDistanceProperty);
            }
            set
            {
                this.SetValue(Marker.LabelDistanceProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnLabelDistanceValidate(object value)
        {
            double labelDistance = (double)value;
            return (labelDistance >= 0);
        }

        #endregion LabelDistance

        #region Fill

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill",
            typeof(Brush), typeof(Marker), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the Shape.
        /// </summary>
        /// <seealso cref="FillProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the Shape.")]
        //[Category("Brushes")]
        public Brush Fill
        {
            get
            {
                return (Brush)this.GetValue(Marker.FillProperty);
            }
            set
            {
                this.SetValue(Marker.FillProperty, value);
            }
        }

        #endregion Fill

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke",
            typeof(Brush), typeof(Marker), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the Shape outline.
        /// </summary>
        /// <seealso cref="StrokeProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the Shape outline.")]
        //[Category("Brushes")]
        public Brush Stroke
        {
            get
            {
                return (Brush)this.GetValue(Marker.StrokeProperty);
            }
            set
            {
                this.SetValue(Marker.StrokeProperty, value);
            }
        }

        #endregion Stroke

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness",
            typeof(double), typeof(Marker), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the width of the Shape outline. 
        /// </summary>
        /// <seealso cref="StrokeThicknessProperty"/>
        //[Description("Gets or sets the width of the Shape outline.")]
        //[Category("Appearance")]
        public double StrokeThickness
        {
            get
            {
                return (double)this.GetValue(Marker.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(Marker.StrokeThicknessProperty, value);
            }
        }

        #endregion StrokeThickness	

        #region UseDataTemplate

        /// <summary>
        /// Identifies the <see cref="UseDataTemplate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty UseDataTemplateProperty = DependencyProperty.Register("UseDataTemplate",
            typeof(bool), typeof(Marker), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether data points use data template.
        /// </summary>
        /// <seealso cref="UseDataTemplateProperty"/>
        //[Description("Gets or sets a value that indicates whether data points use data template.")]
        //[Category("Behavior")]
        public bool UseDataTemplate
        {
            get
            {
                return (bool)this.GetValue(Marker.UseDataTemplateProperty);
            }
            set
            {
                this.SetValue(Marker.UseDataTemplateProperty, value);
            }
        }

        #endregion UseDataTemplate

        #region DataTemplate

        /// <summary>
        /// Gets or sets the data template for the marker.
        /// </summary>
        /// <value>The data template for the marker.</value>
        //[Description("Gets or sets the data template for the marker.")]
        //[Category("Behavior")]
        public DataTemplate DataTemplate
        {
            get { return (DataTemplate)GetValue(DataTemplateProperty); }
            set { SetValue(DataTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DataTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataTemplateProperty =
            DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(Marker),
              new PropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        #endregion

        #region LabelOverflow

        /// <summary>
        /// Identifies the <see cref="LabelOverflow"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LabelOverflowProperty = DependencyProperty.Register("LabelOverflow",
            typeof(LabelOverflow), typeof(Marker), new FrameworkPropertyMetadata(LabelOverflow.ClipToGridArea, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether the labels are clipped within the grid area.
        /// </summary>
        /// <value><c>true</c> if the labels are clipped within the scene; otherwise, <c>false</c>.</value>
        //[Description("Gets or sets a value indicating whether the labels are clipped within the grid area.")]
        //[Category("Behavior")]
        public LabelOverflow LabelOverflow
        {
            get
            {
                return (LabelOverflow)this.GetValue(Marker.LabelOverflowProperty);
            }
            set
            {
                this.SetValue(Marker.LabelOverflowProperty, value);
            }
        }

        #endregion LabelOverflow

        #region TextWrapping
        private const string TextWrappingPropertyName = "TextWrapping";
        /// <summary>
        /// Identifies the <see cref="LabelOverflow"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(TextWrappingPropertyName, 
            typeof(TextWrapping), typeof(Marker), new FrameworkPropertyMetadata(TextWrapping.Wrap, new PropertyChangedCallback(OnPropertyChanged)));
        /// <summary>
        /// Determines how text labels are wrapped when they exceed their horizontal bounds.
        /// </summary>
        /// <remarks>
        /// This property is only effective when used on a ChartType which limits the horizontal bounds of text labels, such as 2D Pie Chart.
        /// </remarks>
        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)this.GetValue(Marker.TextWrappingProperty);
            }
            set
            {
                this.SetValue(Marker.TextWrappingProperty, value);
            }
        }
        #endregion

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