
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Grid area is interior section of the scene. It is area where gridlines and data points 
    /// are drawn, but it exclude area with axis labels. Grid area exists only if 2D charts are used.
    /// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GridArea : ChartContentControl
    {
        #region Fields

        // Private Fields
        private object _chartParent;
        private PlottingPane _plottingPane2D;
        private PlottingPane3D _plottingPane3D;

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

        /// <summary>
        /// Gets or Sets the scene pane
        /// </summary>
        internal PlottingPane PlottingPane2D
        {
            get
            {
                return _plottingPane2D;
            }
            set
            {
                _plottingPane2D = value;
            }
        }

        /// <summary>
        /// Gets or Sets the scene pane
        /// </summary>
        internal PlottingPane3D PlottingPane3D
        {
            get
            {
                return _plottingPane3D;
            }
            set
            {
                _plottingPane3D = value;
            }
        }

        #endregion Properties

        #region Methods

        static GridArea()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridArea), new FrameworkPropertyMetadata(typeof(GridArea)));
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
            if (control != null && e.NewValue != e.OldValue)
            {
                if (XamChart.PropertyNeedRefresh(this, e) || e.Property == GridArea.BorderBrushProperty)
                {
                    if (!control.ContentControlPropertyChanging && control.DefaultChart != null && control.DefaultChart.ChartCreator != null && control.DefaultChart.ChartCreator.Scene != null && !control.DefaultChart.ChartCreator.Scene.ContentControlPropertyChanging)
                    {
                        control.RefreshProperty();
                    }
                }
            }

            base.OnPropertyChanged(e);
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

        #region RefreshPointsOnly

        /// <summary>
        /// Identifies the <see cref="RefreshPointsOnly"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RefreshPointsOnlyProperty = DependencyProperty.Register("RefreshPointsOnly",
            typeof(bool), typeof(GridArea), new FrameworkPropertyMetadata((bool)false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether only data points are refreshed. Drawings of all other chart elements are disabled. Used to improve performance of data point animation.
        /// </summary>
        /// <remarks>
        /// Used for 2D charts only. The default value is false.
        /// </remarks>
        /// <seealso cref="RefreshPointsOnlyProperty"/>
        //[Description("Gets or sets a value that indicates whether only data points are refreshed. Drawings of all other chart elements are disabled. Used to improve performance of data point animation.")]
        public bool RefreshPointsOnly
        {
            get
            {
                return (bool)this.GetValue(GridArea.RefreshPointsOnlyProperty);
            }
            set
            {
                this.SetValue(GridArea.RefreshPointsOnlyProperty, value);
            }
        }


        #endregion RefreshPointsOnly

        #region IsPieScene

        internal static readonly DependencyPropertyKey IsPieScenePropertyKey =
            DependencyProperty.RegisterReadOnly("IsPieScene",
            typeof(bool), typeof(GridArea), new FrameworkPropertyMetadata((bool)false));

        /// <summary>
        /// Identifies the <see cref="IsPieScene"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsPieSceneProperty =
            IsPieScenePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets a value that indicates whether the scene is used to draw Pie or Doughnut chart.
        /// </summary>
        /// <seealso cref="IsPieSceneProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(true)]
        [ReadOnly(true)]
        public bool IsPieScene
        {
            get
            {
                return (bool)this.GetValue(GridArea.IsPieSceneProperty);
            }
        }

        #endregion IsPieScene

        #region BorderThickness

        /// <summary>
        /// Gets or sets a thickness that describes the border thickness of a control. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Thickness BorderThickness
        {
            get
            {
                return base.BorderThickness;
            }
            set
            {
                base.BorderThickness = value;
            }
        }

        #endregion BorderThickness

        #region BorderBrush

        
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


        #endregion BorderBrush

        #region Margin

        /// <summary>
        /// Identifies the <see cref="Margin"/> dependency property
        /// </summary>
        public new static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin",
            typeof(Thickness), typeof(GridArea), new FrameworkPropertyMetadata(new Thickness(0), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the outer margin of an element using percent value from 0 to 100. This property doesn't have any effect if MarginType property is set to �Auto�.
        /// </summary>
        /// <remarks>
        /// The property uses percent as an unit, which means that the values for the left, top, right, and bottom of the bounding rectangle have to be values between 0 and 100. This relative layout will produce proportional resizing of the chart elements.
        /// </remarks>
        /// <seealso cref="MarginProperty"/>
        //[Description("Gets or sets the outer margin of an element using percent value from 0 to 100. This property doesn't have any effect if MarginType property is set to �Auto�.")]
        //[Category("Layout")]
        public new Thickness Margin
        {
            get
            {
                return (Thickness)this.GetValue(GridArea.MarginProperty);
            }
            set
            {
                this.SetValue(GridArea.MarginProperty, value);
            }
        }

        #endregion Margin

        #region MarginType

        /// <summary>
        /// Identifies the <see cref="MarginType"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarginTypeProperty = DependencyProperty.Register("MarginType",
            typeof(MarginType), typeof(GridArea), new FrameworkPropertyMetadata(MarginType.Auto, new PropertyChangedCallback(OnPropertyChanged)));


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
                return (MarginType)this.GetValue(GridArea.MarginTypeProperty);
            }
            set
            {
                this.SetValue(GridArea.MarginTypeProperty, value);
            }
        }

        #endregion MarginType

        #region RenderingOptions

        GridAreaRenderingOptions _renderingOptions = new GridAreaRenderingOptions();

        /// <summary>
        /// Identifies the <see cref="RenderingOptions"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RenderingOptionsProperty = DependencyProperty.Register("RenderingOptions",
            typeof(GridAreaRenderingOptions), typeof(GridArea), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets grid area rendering options. Rendering options are used to improve rendering performance 
        /// for a big number of data points. 
        /// </summary>
        /// <seealso cref="RenderingOptionsProperty"/>
        public GridAreaRenderingOptions RenderingOptions
        {
            get
            {
                GridAreaRenderingOptions obj = (GridAreaRenderingOptions)this.GetValue(GridArea.RenderingOptionsProperty);
                if (obj == null)
                {
                    obj = _renderingOptions;
                }

                obj.ChartParent = this;

                return obj;
            }
            set
            {
                this.SetValue(GridArea.RenderingOptionsProperty, value);
            }
        }

        #endregion RenderingOptions

        #endregion Public Properties
    }

    /// <summary>
    /// This class is used to keep rendering options. Rendering options are 
    /// used to improve rendering performance for a big number of data points.
    /// </summary>
    public class GridAreaRenderingOptions : ChartFrameworkContentElement
    {
        #region Fields

        // Private fields
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

        #region RenderingMode

        /// <summary>
        /// Identifies the <see cref="RenderingMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RenderingModeProperty = DependencyProperty.Register("RenderingMode",
            typeof(RenderingMode), typeof(GridAreaRenderingOptions), new FrameworkPropertyMetadata(RenderingMode.Auto, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets Rendering Mode. Rendering mode is used to improve rendering performance for a big number 
        /// of data points.  If the value is �Auto� Rendering mode depends on number of data 
        /// points. If the number of points is greater than 100 the performance mode is active. 
        /// When Performance mode is active, some wpf features are disabled (animation, backgrounds, 
        /// themes, data templates, etc). Auto is default value.
        /// </summary>
        /// <seealso cref="RenderingModeProperty"/>
        public RenderingMode RenderingMode
        {
            get
            {
                return (RenderingMode)this.GetValue(GridAreaRenderingOptions.RenderingModeProperty);
            }
            set
            {
                this.SetValue(GridAreaRenderingOptions.RenderingModeProperty, value);
            }
        }

        #endregion RenderingMode

        #region RenderingDetails

        GridAreaRenderingDetails _renderingDetails = new GridAreaRenderingDetails();

        /// <summary>
        /// Identifies the <see cref="RenderingDetails"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RenderingDetailsProperty = DependencyProperty.Register("RenderingDetails",
            typeof(GridAreaRenderingDetails), typeof(GridAreaRenderingOptions), new FrameworkPropertyMetadata());

        /// <summary>
        /// Gets grid area rendering details. Rendering details are used to improve rendering performance 
        /// for a big number of data points. 
        /// </summary>
        /// <seealso cref="RenderingDetailsProperty"/>
        public GridAreaRenderingDetails RenderingDetails
        {
            get
            {
                GridAreaRenderingDetails obj = (GridAreaRenderingDetails)this.GetValue(GridAreaRenderingOptions.RenderingDetailsProperty);
                if (obj == null)
                {
                    obj = _renderingDetails;
                }

                obj.ChartParent = this;

                return obj;
            }
            set
            {
                this.SetValue(GridAreaRenderingOptions.RenderingDetailsProperty, value);
            }
        }

        #endregion RenderingDetails

        #endregion Properties

        #region Methods

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamChart control;

            control = XamChart.GetControl(d);
            if (control != null && e.NewValue != e.OldValue)
            {
                control.RefreshProperty();
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// This class is used to keep rendering details. Rendering details are 
    /// used to improve rendering performance for a big number of data points.
    /// </summary>
    public class GridAreaRenderingDetails : ChartFrameworkContentElement
    {
        #region Fields

        // Private fields
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


        #region AntiAliasing

        /// <summary>
        /// Identifies the <see cref="AntiAliasing"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AntiAliasingProperty = DependencyProperty.Register("AntiAliasing",
            typeof(bool), typeof(GridAreaRenderingDetails), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the rendering quality for 'Performance' rendering mode.
        /// </summary>
        /// <remarks>
        /// By default AntiAliasing is set to false. The AntiAliasing specifies whether 
        /// lines, curves, and the edges of filled areas use smoothing (also called antialiasing).
        /// </remarks>
        /// <seealso cref="AntiAliasingProperty"/>
        public bool AntiAliasing
        {
            get
            {
                return (bool)this.GetValue(GridAreaRenderingDetails.AntiAliasingProperty);
            }
            set
            {
                this.SetValue(GridAreaRenderingDetails.AntiAliasingProperty, value);
            }
        }

        #endregion AntiAliasing

        #region ForceSolidBrush

        /// <summary>
        /// Identifies the <see cref="ForceSolidBrush"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ForceSolidBrushProperty = DependencyProperty.Register("ForceSolidBrush",
            typeof(bool), typeof(GridAreaRenderingDetails), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets Force Solid Brush. If the value is true only solid color brush 
        /// is used. This significantly improves performance and this is used only for 
        /// �Performance� rendering mode.
        /// </summary>
        /// <seealso cref="ForceSolidBrushProperty"/>
        public bool ForceSolidBrush
        {
            get
            {
                return (bool)this.GetValue(GridAreaRenderingDetails.ForceSolidBrushProperty);
            }
            set
            {
                this.SetValue(GridAreaRenderingDetails.ForceSolidBrushProperty, value);
            }
        }

        #endregion ForceSolidBrush

        #region AllowDataPointBrush

        /// <summary>
        /// Identifies the <see cref="AllowDataPointBrush"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowDataPointBrushProperty = DependencyProperty.Register("AllowDataPointBrush",
            typeof(bool), typeof(GridAreaRenderingDetails), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or set a value which indicates whether data point brushes are used. For �Performance� mode, 
        /// by default, only series colors are used and all data points have same colors. This significantly 
        /// improves performance and memory usage.
        /// </summary>
        /// <seealso cref="ForceSolidBrushProperty"/>
        public bool AllowDataPointBrush
        {
            get
            {
                return (bool)this.GetValue(GridAreaRenderingDetails.AllowDataPointBrushProperty);
            }
            set
            {
                this.SetValue(GridAreaRenderingDetails.AllowDataPointBrushProperty, value);
            }
        }

        #endregion AllowDataPointBrush

        #region AutoModePointsNumber

        /// <summary>
        /// Identifies the <see cref="AutoModePointsNumber"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AutoModePointsNumberProperty = DependencyProperty.Register("AutoModePointsNumber",
            typeof(int), typeof(GridAreaRenderingDetails), new FrameworkPropertyMetadata((int)1000, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets auto mode points number. This number is used when Rendering Mode 
        /// is set to auto. The Performance mode is active when the number of data points 
        /// from all series is greater than AutoModePointsNumber. If number of points is 
        /// less then this number the rendering mode is �Full�. When rendering mode is 
        /// set to �Full� or �Performance� this number is ignored. Default value is 1000.
        /// </summary>
        /// <seealso cref="AutoModePointsNumberProperty"/>
        public int AutoModePointsNumber
        {
            get
            {
                return (int)this.GetValue(GridAreaRenderingDetails.AutoModePointsNumberProperty);
            }
            set
            {
                this.SetValue(GridAreaRenderingDetails.AutoModePointsNumberProperty, value);
            }
        }

        #endregion AutoModePointsNumber

        #endregion Properties

        #region Methods

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamChart control;
           
            control = XamChart.GetControl(d);
            if (control != null && e.NewValue != e.OldValue)
            {
                control.RefreshProperty();
            }
        }

        #endregion Methods
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