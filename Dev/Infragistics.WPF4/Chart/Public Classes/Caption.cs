
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class keeps information about text value, font, color and 
    /// position for a Chart Control title.
    /// </summary>
    /// <remarks>
    /// The chart title position is calculated automatically, but the title could 
    /// be positioned manually using Position property.
    /// </remarks>
    public class Caption : ChartFrameworkContentElement
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
        /// Initializes a new instance of the Caption class. 
        /// </summary>
        public Caption()
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

        #region Text

        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string), typeof(Caption), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the text contents of a Caption. 
        /// </summary>
        /// <remarks>
        /// A string specifying the content of the Caption. The default value is Empty. 
        /// </remarks>
        /// <seealso cref="TextProperty"/>
        //[Description("The caption text")]
        //[Category("Data")]
        public string Text
        {
            get
            {
                return (string)this.GetValue(Caption.TextProperty);
            }
            set
            {
                this.SetValue(Caption.TextProperty, value);
            }
        }

        #endregion Text
                
        #region Margin

        /// <summary>
        /// Identifies the <see cref="Margin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin",
            typeof(Thickness), typeof(Caption), new FrameworkPropertyMetadata(new Thickness(0), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the outer margin of an element using percent value from 0 to 100. This property doesn't have any effect if MarginType property is set to �Auto�.
        /// </summary>
        /// <remarks>
        /// The property uses percent as an unit, which means that the values for the left, top, right, and bottom of the bounding rectangle have to be values between 0 and 100. This relative layout will produce proportional resizing of the chart elements.
        /// </remarks>
        /// <seealso cref="MarginProperty"/>
        //[Description("Gets or sets the outer margin of an element using percent value from 0 to 100. This property doesn't have any effect if MarginType property is set to �Auto�.")]
        //[Category("Layout")]
        public Thickness Margin
        {
            get
            {
                return (Thickness)this.GetValue(Caption.MarginProperty);
            }
            set
            {
                this.SetValue(Caption.MarginProperty, value);
            }
        }

        #endregion Margin

        #region MarginType

        /// <summary>
        /// Identifies the <see cref="MarginType"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarginTypeProperty = DependencyProperty.Register("MarginType",
            typeof(MarginType), typeof(Caption), new FrameworkPropertyMetadata(MarginType.Auto, new PropertyChangedCallback(OnPropertyChanged)));


        /// <summary>
        /// Gets or sets a margin type for the chart element. If MarginType property is �Percent�, the Margin property has to be set.
        /// </summary>
        /// <remarks>
        /// The margin type can be set to Auto or Percent. The default value is Auto, and the position of the chart element is automatically created. For manual positioning the MarginType has to be set to Percent.  
        /// </remarks>
        /// <seealso cref="Margin"/>
        /// <seealso cref="MarginTypeProperty"/>
        //[Description("Gets or sets a margin type for the chart element. If MarginType property is �Percent�, the Margin property has to be set.")]
        //[Category("Layout")]
        public MarginType MarginType
        {
            get
            {
                return (MarginType)this.GetValue(Caption.MarginTypeProperty);
            }
            set
            {
                this.SetValue(Caption.MarginTypeProperty, value);
            }
        }

        #endregion MarginType

        #region FontFamily

        /// <summary>
        /// Identifies the <see cref="FontFamily"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily",
            typeof(FontFamily), typeof(Caption), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, new PropertyChangedCallback(OnPropertyChanged)));

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
                return (FontFamily)this.GetValue(Caption.FontFamilyProperty);
            }
            set
            {
                this.SetValue(Caption.FontFamilyProperty, value);
            }
        }

        #endregion FontFamily

        #region FontSize

        /// <summary>
        /// Identifies the <see cref="FontSize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize",
            typeof(double), typeof(Caption), new FrameworkPropertyMetadata((double)SystemFonts.MessageFontSize, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnFontSizeValidate));

        /// <summary>
        /// Gets or sets the font size of the caption text.
        /// </summary>
        /// <remarks>
        /// A double value specifying the desired font size to use in points (1 point = 1/72 of an inch = 96/72 device independent pixels). The default value is determined by the MessageFontSize metric.
        /// </remarks>
        /// <seealso cref="FontSizeProperty"/>
        //[Description("Gets or sets the font size of the caption text.")]
        //[Category("Font")]
        [TypeConverterAttribute(typeof(FontSizeConverter))]
        public double FontSize
        {
            get
            {
                return (double)this.GetValue(Caption.FontSizeProperty);
            }
            set
            {
                this.SetValue(Caption.FontSizeProperty, value);
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
            typeof(FontStyle), typeof(Caption), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the font style for the caption text.
        /// </summary>
        /// <remarks>
        /// The default value for FontStyle is Normal.
        /// </remarks>
        /// <seealso cref="FontStyleProperty"/>
        //[Description("Gets or sets the font style for the caption text.")]
        //[Category("Font")]
        public FontStyle FontStyle
        {
            get
            {
                return (FontStyle)this.GetValue(Caption.FontStyleProperty);
            }
            set
            {
                this.SetValue(Caption.FontStyleProperty, value);
            }
        }

        #endregion FontStyle

        #region FontWeight

        /// <summary>
        /// Identifies the <see cref="FontWeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight",
            typeof(FontWeight), typeof(Caption), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

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
                return (FontWeight)this.GetValue(Caption.FontWeightProperty);
            }
            set
            {
                this.SetValue(Caption.FontWeightProperty, value);
            }
        }

        #endregion FontWeight

        #region FontStretch

        /// <summary>
        /// Identifies the <see cref="FontStretch"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register("FontStretch",
            typeof(FontStretch), typeof(Caption), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

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
                return (FontStretch)this.GetValue(Caption.FontStretchProperty);
            }
            set
            {
                this.SetValue(Caption.FontStretchProperty, value);
            }
        }

        #endregion FontStretch

        #region Foreground

        /// <summary>
        /// Identifies the <see cref="Foreground"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground",
            typeof(Brush), typeof(Caption), new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush to apply to the text contents of the caption.
        /// </summary>
        /// <seealso cref="ForegroundProperty"/>
        //[Description("Gets or sets the Brush to apply to the text contents of the caption.")]
        //[Category("Brushes")]
        public Brush Foreground
        {
            get
            {
                return (Brush)this.GetValue(Caption.ForegroundProperty);
            }
            set
            {
                this.SetValue(Caption.ForegroundProperty, value);
            }
        }


        #endregion Foreground

        #region Background

        /// <summary>
        /// Identifies the <see cref="Background"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background",
            typeof(Brush), typeof(Caption), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the interior of the caption. 
        /// </summary>
        /// <seealso cref="BackgroundProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the interior of the caption.")]
        //[Category("Brushes")]
        public Brush Background
        {
            get
            {
                return (Brush)this.GetValue(Caption.BackgroundProperty);
            }
            set
            {
                this.SetValue(Caption.BackgroundProperty, value);
            }
        }

        #endregion Background

        #region BorderBrush

        /// <summary>
        /// Identifies the <see cref="BorderBrush"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush",
            typeof(Brush), typeof(Caption), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that draws the outer border color. 
        /// </summary>
        /// <seealso cref="BorderBrushProperty"/>
        //[Description("Gets or sets the Brush that draws the outer border color.")]
        //[Category("Brushes")]
        public Brush BorderBrush
        {
            get
            {
                return (Brush)this.GetValue(Caption.BorderBrushProperty);
            }
            set
            {
                this.SetValue(Caption.BorderBrushProperty, value);
            }
        }

        #endregion BorderBrush

        #region BorderThickness

        /// <summary>
        /// Identifies the <see cref="BorderThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness",
            typeof(double), typeof(Caption), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnBorderThicknessValidate));

        /// <summary>
        /// Gets or sets the border thickness for the Caption Panel. 
        /// </summary>
        /// <seealso cref="FontSizeProperty"/>
        //[Description("Gets or sets the border thickness for the Caption Panel")]
        public double BorderThickness
        {
            get
            {
                return (double)this.GetValue(Caption.BorderThicknessProperty);
            }
            set
            {
                this.SetValue(Caption.BorderThicknessProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnBorderThicknessValidate(object value)
        {
            double thickness = (double)value;
            return (thickness > 0);

        }

        #endregion BorderThickness

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