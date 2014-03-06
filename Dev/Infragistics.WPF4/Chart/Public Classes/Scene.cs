
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
    /// This class contains information related to scene appearance. The chart scene 
    /// is different for 2D and 3D chart. For 2D chart it is used for Background color 
    /// and position. For 3D chart it also containes thicknes of the 3D scene.
    /// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class Scene : ChartContentControl
    {
        #region Fields

        // Private Fields
        private object _chartParent;
        private ScenePane _scenePane;
        private bool _contentControlPropertyChanging = false;

        #endregion Fields

        #region Properties

        /// <summary>
        /// On property changed called and active
        /// </summary>
        internal bool ContentControlPropertyChanging
        {
            get
            {
                return _contentControlPropertyChanging;
            }
            set
            {
                _contentControlPropertyChanging = value;
            }
        }

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
        internal ScenePane ScenePane
        {
            get
            {
                return _scenePane;
            }
            set
            {
                _scenePane = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the Scene class. 
        /// </summary>
        public Scene()
        {
        }

        static Scene()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Scene), new FrameworkPropertyMetadata(typeof(Scene)));
        }

        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this 
        /// FrameworkElement has been updated. The specific dependency property that changed 
        /// is reported in the arguments parameter. Overrides OnPropertyChanged. 
        /// </summary>
        /// <param name="e">Event arguments that describe the property that changed, as well as old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            ContentControlPropertyChanging = true;
            XamChart control = XamChart.GetControl(this);
            if (control != null && e.NewValue != e.OldValue)
            {
                if (XamChart.PropertyNeedRefresh(this,e))
                {
                    if (!control.ContentControlPropertyChanging)
                    {
                        control.RefreshProperty();
                    }
                }
            }

            base.OnPropertyChanged(e);
            ContentControlPropertyChanging = false;
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

        #region GridArea

        GridArea _gridArea = new GridArea();

        /// <summary>
        /// Identifies the <see cref="GridArea"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GridAreaProperty = DependencyProperty.Register("GridArea",
            typeof(GridArea), typeof(Scene), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the GridArea which is the region inside the Scene which contains: DataPoints, Markers and gridlines. Axis Labels are drawn outside the GridArea. 3D chart do not have GridArea.
        /// </summary>
        /// <seealso cref="GridAreaProperty"/>
        //[Description("Gets or sets the GridArea which is the region inside the Scene which contains: DataPoints, Markers and gridlines. Axis Labels are drawn outside the GridArea. 3D chart do not have GridArea.")]
        //[Category("Chart Data")]
        public GridArea GridArea
        {
            get
            {
                GridArea obj = (GridArea)this.GetValue(Scene.GridAreaProperty);
                if (obj == null)
                {
                    obj = _gridArea;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(Scene.GridAreaProperty, value);
            }
        }

        #endregion GridArea

        #region Scene3DThickness

        /// <summary>
        /// Identifies the <see cref="Scene3DThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty Scene3DThicknessProperty = DependencyProperty.Register("Scene3DThickness",
            typeof(double), typeof(Scene), new FrameworkPropertyMetadata((double)2, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnScene3DThicknessValidate));

        /// <summary>
        /// Gets or sets 3D Scene Thickness.
        /// </summary>
        /// <seealso cref="Scene3DThicknessProperty"/>
        //[Description("Gets or sets 3D Scene Thickness.")]
        //[Category("Chart Data")]
        public double Scene3DThickness
        {
            get
            {
                return (double)this.GetValue(Scene.Scene3DThicknessProperty);
            }
            set
            {
                this.SetValue(Scene.Scene3DThicknessProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnScene3DThicknessValidate(object value)
        {
            double thickness = (double)value;
            return (thickness >= 0 && thickness <= 10);

        }

        #endregion Scene3DThickness

        #region Margin

        /// <summary>
        /// Identifies the <see cref="Margin"/> dependency property
        /// </summary>
        public new static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin",
            typeof(Thickness), typeof(Scene), new FrameworkPropertyMetadata(new Thickness(0), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the outer margin of an element using percent value from 0 to 100. This property doesn't have any effect if MarginType property is set to �Auto�.
        /// </summary>
        /// <remarks>
        /// The property uses percent as an unit, which means that the values for the left, top, right, and bottom of the bounding rectangle have to be values between 0 and 100. This relative layout will produce proportional resizing of the chart element.
        /// </remarks>
        /// <seealso cref="MarginType"/>
        /// <seealso cref="MarginProperty"/>
        //[Description("Gets or sets the outer margin of an element using percent value from 0 to 100. This property doesn't have any effect if MarginType property is set to �Auto�.")]
        //[Category("Layout")]
        public new Thickness Margin
        {
            get
            {
                return (Thickness)this.GetValue(Scene.MarginProperty);
            }
            set
            {
                this.SetValue(Scene.MarginProperty, value);
            }
        }

        #endregion Margin

        #region MarginType

        /// <summary>
        /// Identifies the <see cref="MarginType"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarginTypeProperty = DependencyProperty.Register("MarginType",
            typeof(MarginType), typeof(Scene), new FrameworkPropertyMetadata(MarginType.Auto, new PropertyChangedCallback(OnPropertyChanged)));


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
                return (MarginType)this.GetValue(Scene.MarginTypeProperty);
            }
            set
            {
                this.SetValue(Scene.MarginTypeProperty, value);
            }
        }

        #endregion MarginType

        #region Perspective

        /// <summary>
        /// Identifies the <see cref="Perspective"/> dependency property
        /// </summary>
        public static readonly DependencyProperty PerspectiveProperty = DependencyProperty.Register("Perspective",
            typeof(double), typeof(Scene), new FrameworkPropertyMetadata((double)70, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnPerspectiveValidate));

        /// <summary>
        /// Gets or sets perspective level for 3D charts, ranging in value from 0 to 100.
        /// </summary>
        /// <seealso cref="PerspectiveProperty"/>
        /// <remarks>
        /// A value of 100 represents 100%, meaning the chart is rendered at full perspective. Perspective is the 
        /// way in which objects appear to the eye based on their dimensions and the position. For example, 
        /// the parallel lines of a railway track are perceived as meeting at a distant point at the horizon. 
        /// As objects become more distant, they begin to appear smaller.
        /// </remarks>
        //[Description("Gets or sets perspective level for 3D charts, ranging in value from 0 to 100.")]
        //[Category("Chart Data")]
        public double Perspective
        {
            get
            {
                return (double)this.GetValue(Scene.PerspectiveProperty);
            }
            set
            {
                this.SetValue(Scene.PerspectiveProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnPerspectiveValidate(object value)
        {
            double perspective = (double)value;
            return (perspective >= 0 && perspective <= 100);

        }

        #endregion Perspective

        #region Scene3DBrush

        /// <summary>
        /// Identifies the <see cref="Scene3DBrush"/> dependency property
        /// </summary>
        public static readonly DependencyProperty Scene3DBrushProperty = DependencyProperty.Register("Scene3DBrush",
            typeof(Brush), typeof(Scene), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0xF4,0xEE,0xDD)), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the interior of the 3D scene. 
        /// </summary>
        /// <seealso cref="Scene3DBrushProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the interior of the 3D Scene. ")]
        //[Category("Brushes")]
        public Brush Scene3DBrush
        {
            get
            {
                return (Brush)this.GetValue(Scene.Scene3DBrushProperty);
            }
            set
            {
                this.SetValue(Scene.Scene3DBrushProperty, value);
            }
        }

        #endregion Scene3DBrush

        #region DataFilter

        /// <summary>
        /// Identifies the <see cref="DataFilter"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DataFilterProperty = DependencyProperty.Register("DataFilter",
            typeof(bool), typeof(Scene), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the series are filtered if they have more than 1000 data points. This property is used to improve rendering performance for Line 2D, ScatterLine 2D and Area 2D chart.
        /// </summary>
        /// <remarks>
        /// This property activates an algorithm which replaces a group of data points with one and increase performance. This algorithm is activated only if there are more than 1000 data points in the series. 
        /// </remarks>
        /// <seealso cref="DataFilterProperty"/>
        //[Description("Gets or sets a value that indicates whether the series are filtered if they have more than 1000 data points. This property is used to improve rendering performance for Line 2D, ScatterLine 2D and Area 2D chart.")]
        public bool DataFilter
        {
            get
            {
                return (bool)this.GetValue(Scene.DataFilterProperty);
            }
            set
            {
                this.SetValue(Scene.DataFilterProperty, value);
            }
        }

        #endregion DataFilter

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