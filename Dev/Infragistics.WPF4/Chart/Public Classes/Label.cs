
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Label keeps information about axis label appearance and formatting. By default, 
    /// axis labels are positioned automatically and label collision algorithm is enabled. 
    /// </summary>
    /// <remarks>
    /// Axis label text is stored in the <see cref="Infragistics.Windows.Chart.DataPoint.Label"/> property of <see cref="Infragistics.Windows.Chart.Series.DataPoints"/>. 
    /// This class keeps information about Axis labels appearance only.
    /// </remarks>
    public class Label : ChartFrameworkContentElement
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
        /// Initializes a new instance of the Label class. 
        /// </summary>
        public Label()
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

        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this 
        /// FrameworkElement has been updated. The specific dependency property that changed 
        /// is reported in the arguments parameter. Overrides OnPropertyChanged. 
        /// </summary>
        /// <param name="e">Event arguments that describe the property that changed, as well as old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            XamChart control = XamChart.GetControl(this);
            if (control != null)
            {
                if (XamChart.PropertyNeedRefresh(this,e))
                {
                    control.RefreshProperty();
                }
            }

            base.OnPropertyChanged(e);
        }

        #endregion Methods

        #region Public Properties

        #region FontFamily

        /// <summary>
        /// Identifies the <see cref="FontFamily"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily",
            typeof(FontFamily), typeof(Label), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, new PropertyChangedCallback(OnPropertyChanged)));

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
                return (FontFamily)this.GetValue(Label.FontFamilyProperty);
            }
            set
            {
                this.SetValue(Label.FontFamilyProperty, value);
            }
        }

        #endregion FontFamily

        #region FontSize

        /// <summary>
        /// Identifies the <see cref="FontSize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize",
            typeof(double), typeof(Label), new FrameworkPropertyMetadata((double)SystemFonts.MessageFontSize, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnFontSizeValidate));

        /// <summary>
        /// Gets or sets the font size of the axis labels.
        /// </summary>
        /// <remarks>
        /// A double value specifying the desired font size to use in points (1 point = 1/72 of an inch = 96/72 device independent pixels). The default value is determined by the MessageFontSize metric.
        /// </remarks>
        /// <seealso cref="FontSizeProperty"/>
        //[Description("Gets or sets the font size of the axis labels.")]
        //[Category("Font")]
        [TypeConverterAttribute(typeof(FontSizeConverter))] 
        public double FontSize
        {
            get
            {
                return (double)this.GetValue(Label.FontSizeProperty);
            }
            set
            {
                this.SetValue(Label.FontSizeProperty, value);
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

        #region Angle

        /// <summary>
        /// Identifies the <see cref="Angle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle",
            typeof(double), typeof(Label), new FrameworkPropertyMetadata((double)Double.NaN, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnAngleValidate));

        /// <summary>
        /// Gets or sets an angle of labels.
        /// </summary>
        /// <seealso cref="AngleProperty"/>
        //[Description("Gets or sets an angle of labels.")]
        //[Category("Font")]
        public double Angle
        {
            get
            {
                return (double)this.GetValue(Label.AngleProperty);
            }
            set
            {
                this.SetValue(Label.AngleProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnAngleValidate(object value)
        {
            double angle = (double)value;
            return (Double.IsNaN(angle) || angle >= -90 && angle <= 90);
        }

        #endregion FontSize

        #region FontStyle

        /// <summary>
        /// Identifies the <see cref="FontStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register("FontStyle",
            typeof(FontStyle), typeof(Label), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the font style for axis labels.
        /// </summary>
        /// <remarks>
        /// The default value for FontStyle is Normal.
        /// </remarks>
        /// <seealso cref="FontStyleProperty"/>
        //[Description("Gets or sets the font style for axis labels.")]
        //[Category("Font")]
        public FontStyle FontStyle
        {
            get
            {
                return (FontStyle)this.GetValue(Label.FontStyleProperty);
            }
            set
            {
                this.SetValue(Label.FontStyleProperty, value);
            }
        }

        #endregion FontStyle

        #region FontWeight

        /// <summary>
        /// Identifies the <see cref="FontWeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight",
            typeof(FontWeight), typeof(Label), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

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
                return (FontWeight)this.GetValue(Label.FontWeightProperty);
            }
            set
            {
                this.SetValue(Label.FontWeightProperty, value);
            }
        }

        #endregion FontWeight

        #region FontStretch

        /// <summary>
        /// Identifies the <see cref="FontStretch"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register("FontStretch",
            typeof(FontStretch), typeof(Label), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

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
                return (FontStretch)this.GetValue(Label.FontStretchProperty);
            }
            set
            {
                this.SetValue(Label.FontStretchProperty, value);
            }
        }

        #endregion FontStretch
        
        #region Foreground

        /// <summary>
        /// Identifies the <see cref="Foreground"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground",
            typeof(Brush), typeof(Label), new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnPropertyChanged)));

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
                return (Brush)this.GetValue(Label.ForegroundProperty);
            }
            set
            {
                this.SetValue(Label.ForegroundProperty, value);
            }
        }


        #endregion Foreground

        #region Format

        /// <summary>
        /// Identifies the <see cref="Format"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format",
            typeof(string), typeof(Label), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets data point label format using the .NET Framework composite string formatting.
        /// </summary>
        /// <remarks>
        /// This property is used to format axis label string. Axis label should use axis label 
        /// value which will be formatted on specified way and inserted into a text. Axis label format uses 
        /// .NET Composite Formatting specification. The �{0}� text is replaced with axis label value. For 
        /// example, to create a label which displays a text �Label value is: 3� where 3 is axis label 
        /// value, we have to create a following format string �Label value is: {0}�. Few more examples: 
        /// 1. �Product price is: $3.00 dollars� -> �Product price is: {0:C} dollars�. 2. �Calculated value is: 
        /// 03.00� -> �Calculated value is: {0:00.00}�. For more information about format property see �Composite 
        /// Formatting� in .NET Framework Developer�s Guide. 
        /// </remarks>
        /// <seealso cref="FormatProperty"/>
        //[Description("Gets or sets data point label format using the .NET Framework composite string formatting.")]
        //[Category("Miscellaneous")]
        public string Format
        {
            get
            {
                return (string)this.GetValue(Label.FormatProperty);
            }
            set
            {
                this.SetValue(Label.FormatProperty, value);
            }
        }


        #endregion Format

        #region AutoResize

        /// <summary>
        /// Identifies the <see cref="AutoResize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AutoResizeProperty = DependencyProperty.Register("AutoResize",
            typeof(bool), typeof(Label), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the axis labels have automatic font size.
        /// </summary>
        /// <seealso cref="AutoResizeProperty"/>
        //[Description("Gets or sets a value that indicates whether the axis labels have automatic font size.")]
        //[Category("Behavior")]
        public bool AutoResize
        {
            get
            {
                return (bool)this.GetValue(Label.AutoResizeProperty);
            }
            set
            {
                this.SetValue(Label.AutoResizeProperty, value);
            }
        }

        #endregion AutoResize

        #region DistanceFromAxis

        /// <summary>
        /// Identifies the <see cref="DistanceFromAxis"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DistanceFromAxisProperty = DependencyProperty.Register("DistanceFromAxis",
            typeof(double), typeof(Label), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnDistanceFromAxisValidate));

        /// <summary>
        /// Gets or sets the distance between axis and labels. This is scaling factor applied to the default distance. Value 1 is default distance, a value between 0 and 1 decrease the default distance and a value greater than 1 increase the default distance. 
        /// </summary>
        /// <seealso cref="DistanceFromAxisProperty"/>
        //[Description("Gets or sets the distance between axis and labels. This is scaling factor applied to the default distance. Value 1 is default distance, a value between 0 and 1 decrease the default distance and a value greater than 1 increase the default distance.")]
        //[Category("Appearance")]
        public double DistanceFromAxis
        {
            get
            {
                return (double)this.GetValue(Label.DistanceFromAxisProperty);
            }
            set
            {
                this.SetValue(Label.DistanceFromAxisProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnDistanceFromAxisValidate(object value)
        {
            double distanceFromAxis = (double)value;
            return (distanceFromAxis >= 0);
        }

        #endregion DistanceFromAxis

        #region Visible

        /// <summary>
        /// Identifies the <see cref="Visible"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible",
            typeof(bool), typeof(Label), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the axis labels are visible.
        /// </summary>
        /// <seealso cref="VisibleProperty"/>
        //[Description("Gets or sets a value that indicates whether the axis labels are visible.")]
        //[Category("Behavior")]
        public bool Visible
        {
            get
            {
                return (bool)this.GetValue(Label.VisibleProperty);
            }
            set
            {
                this.SetValue(Label.VisibleProperty, value);
            }
        }

        #endregion Visible

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