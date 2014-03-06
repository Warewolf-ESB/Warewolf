
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Axis provides base set of properties and methods used for data range 
    /// calculation and visual presentation of axis line. Axis also contains 
    /// Gridlines, tickmarks and labels.
    /// </summary>
    /// <remarks>
    /// <p class="body">There are a few different ways how axis could be used and it depends on selected 
    /// chart type. For example, column chart display values on vertical axis and categories 
    /// on the horizontal axis. Bar chart uses horizontal axis for values and vertical for 
    /// categories. Scatter chart types use both horizontal and vertical axes for values. Some 
    /// chart types as doughnut or pie do not use axes.</p>    
    /// <p class="body">By default, the chart's axis does not exist in the Axes collection; however, 
    /// internally, default axes are created. If you don't want to modify the appearance 
    /// or range of the axes, grid lines, or axis labels, you can simply use the default 
    /// values of the axes. If you want to change the the default axis values, you need 
    /// to create an axis and add it to the Axes collection.</p>
    /// </remarks>
    public class Axis : ChartFrameworkContentElement
    {
        #region Fields

        // Private Fields
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
        /// Initializes a new instance of the Axis class. 
        /// </summary>
        public Axis()
        {
        }

        private Dictionary<DependencyProperty, object> _CachedSettings = new Dictionary<DependencyProperty,object>();
        private Dictionary<DependencyProperty, object> CachedSettings { get { return this._CachedSettings; } }
        
        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Axis ax = d as Axis;
            if (ax != null && e.NewValue is FrameworkContentElement)
            {
                ValueSource source = DependencyPropertyHelper.GetValueSource(d, e.Property);
                if ((int)source.BaseValueSource <= (int)BaseValueSource.ImplicitStyleReference && !source.IsCoerced)
                {
                    // using the actual value here will result in problems when adding the object to LogicalChildren -- in a scenario with more than one chart, this is not going to work.
                    // use a clone instead -- the clone will be created from the CoerceValueCallback.
                    ax.CachedSettings[e.Property] = e.NewValue;
                    ax.CoerceValue(e.Property);
                }
            }

            XamChart control = XamChart.GetControl(d);
            if (control != null && e.NewValue != e.OldValue)
            {
                control.RefreshProperty();
            }
        }
        private static object CoerceValueCallback(DependencyObject d, object baseValue, DependencyProperty property)
        {
            Axis ax = d as Axis;
            if (ax != null && ax.CachedSettings.ContainsKey(property) && baseValue == ax.CachedSettings[property])
            {
                // possible optimization: only create the clone once and cache it.
                return new CloneManager().Clone(baseValue);
            }
            return baseValue;
        }
        internal void AddChildren()
        {
            AddChild(this.Label);
            AddChild(this.Animation);
            AddChild(this.MajorGridline);
            AddChild(this.MajorTickMark);
            AddChild(this.MinorGridline);
            AddChild(this.MinorTickMark);

            foreach (Stripe stripe in this.Stripes)
            {
                AddChild(stripe);
            }
        }

        /// <summary>
        /// Gets an enumerator for this element's logical child elements.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                ArrayList _list = new ArrayList();

                _list.Add(this.Label);
                _list.Add(this.Animation);
                _list.Add(this.MajorGridline);
                _list.Add(this.MajorTickMark);
                _list.Add(this.MinorGridline);
                _list.Add(this.MinorTickMark);

                foreach (Stripe stripe in this.Stripes)
                {
                    _list.Add(stripe);
                }

                return (IEnumerator)_list.GetEnumerator();
            }
        }
               

        #endregion Methods

        #region Public Properties

        #region AxisType

        /// <summary>
        /// Identifies the <see cref="AxisType"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AxisTypeProperty = DependencyProperty.Register("AxisType",
            typeof(AxisType), typeof(Axis), new FrameworkPropertyMetadata(AxisType.PrimaryX, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the axis type for this axis. This property is also used to enable secondary axes.
        /// </summary>
        /// <remarks>
        /// The axis type can be PrimaryX, PrimaryY, PrimaryZ, SecondaryX and SecondaryY. The default value is PrimaryX.
        /// </remarks>
        /// <seealso cref="AxisTypeProperty"/>
        //[Description("Gets or sets the axis type for this axis. This property is also used to enable secondary axes.")]
		//[Category("Scale")]
        public AxisType AxisType
        {
            get
            {
                return (AxisType)this.GetValue(Axis.AxisTypeProperty);
            }
            set
            {
                this.SetValue(Axis.AxisTypeProperty, value);
            }
        }

        #endregion AxisType
        
        #region Maximum

        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum",
            typeof(double), typeof(Axis), new FrameworkPropertyMetadata((double)Double.NaN, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the maximum limit of the Axis range. To create manual range <see cref="AutoRange"/> property has to be set to false and <see cref="Minimum"/>, Maximum and <see cref="Unit"/> have to be set.
        /// </summary>
        /// <remarks>
        /// If minimum or maximum value is NaN the axis range is automatically calculated. If Maximum is used on a category axis, the value is rounded to the smallest integer greater than or equal to the specified number. The default value is NaN.
        /// </remarks>
        /// <seealso cref="MaximumProperty"/>
        //[Description("Gets or sets the maximum limit of the Axis range. To create manual range AutoRange property has to be set to false and Minimum, Maximum and Unit have to be set.")]
		//[Category("Range")]
        public double Maximum
        {
            get
            {
                return (double)this.GetValue(Axis.MaximumProperty);
            }
            set
            {
                this.SetValue(Axis.MaximumProperty, value);
            }
        }

        #endregion Maximum

        #region Minimum

        /// <summary>
        /// Identifies the <see cref="Minimum"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum",
            typeof(double), typeof(Axis), new FrameworkPropertyMetadata((double)Double.NaN, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the minimum limit of the Axis range. To create manual range <see cref="AutoRange"/> property has to be set to false and Minimum, <see cref="Maximum"/> and <see cref="Unit"/> have to be set.
        /// </summary>
        /// <remarks>
        /// If minimum or maximum value is NaN the axis range is automatically calculated. If Minimum is used on a category axis, the value is rounded to the smallest integer greater than or equal to the specified number. The default value is NaN.
        /// </remarks>
        /// <seealso cref="MinimumProperty"/>
        //[Description("Gets or sets the minimum limit of the Axis range. To create manual range AutoRange property has to be set to false and Minimum, Maximum and Unit have to be set.")]
        //[Category("Range")]
        public double Minimum
        {
            get
            {
                return (double)this.GetValue(Axis.MinimumProperty);
            }
            set
            {
                this.SetValue(Axis.MinimumProperty, value);
            }
        }

        #endregion Minimum

        #region Unit

        /// <summary>
        /// Identifies the <see cref="Unit"/> dependency property
        /// </summary>
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit",
            typeof(double), typeof(Axis), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(AxisUnitValidateCallback));

        /// <summary>
        /// Gets or sets a distance between two neighboring axis labels, gridlines and tick marks. The Unit value uses axis scale. If the Unit is 0, Axis Unit is automatically calculated.
        /// </summary>
        /// <remarks>
        /// For logarithmic axes, the Unit value will represent interval of the linearlized scale. For example if <see cref="Minimum"/> and <see cref="Maximum"/> values are 10 and 10000 the linearlized minimum and maximum will be 1 and 4 (1=Log(10) and 4=Log(10000). If the Unit for this linearlized scale is 1 the axis will have 4 labels: 10, 100, 1000, 10000. The default value for Unit is 0.
        /// </remarks>
        /// <seealso cref="UnitProperty"/>
        //[Description("Gets or sets a distance between two neighboring axis labels, gridlines and tick marks. The Unit value uses axis scale. If the Unit is 0, Axis Unit is automatically calculated.")]
        //[Category("Range")]
        public double Unit
        {
            get
            {
                return (double)this.GetValue(Axis.UnitProperty);
            }
            set
            {
                this.SetValue(Axis.UnitProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>true if the value was validated; false if the submitted value was invalid.</returns>
        private static bool AxisUnitValidateCallback(object value)
        {
            double unit = (double)value;

            return (unit >= 0);

        }



        #endregion Unit

        #region AutoRange

        /// <summary>
        /// Identifies the <see cref="AutoRange"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AutoRangeProperty = DependencyProperty.Register("AutoRange",
            typeof(bool), typeof(Axis), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Minimum"/>, <see cref="Maximum"/> and <see cref="Unit"/> are automatically calculated. To create manual range AutoRange property has to be set to false and Minimum, Maximum and Unit have to be set.
        /// </summary>
        /// <remarks>
        /// By default, chart determines the minimum and maximum range values of the axis. 
        /// You can customize the scale to better meet your needs. The default value for AutoRange is true.
        /// </remarks>
        /// <seealso cref="AutoRangeProperty"/>
        //[Description("Gets or sets a value indicating whether the Minimum, Maximum and Unit are automatically calculated. To create manual range AutoRange property has to be set to false and Minimum, Maximum and Unit have to be set.")]
		//[Category("Range")]
        public bool AutoRange
        {
            get
            {
                return (bool)this.GetValue(Axis.AutoRangeProperty);
            }
            set
            {
                this.SetValue(Axis.AutoRangeProperty, value);
            }
        }

        #endregion AutoRange

        #region RangeFromZero

        /// <summary>
        /// Identifies the <see cref="RangeFromZero"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RangeFromZeroProperty = DependencyProperty.Register("RangeFromZero",
            typeof(bool), typeof(Axis), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether the zero value is inside the auto range.
        /// </summary>
        /// <remarks>
        /// By default the chart control calculates minimum and maximum values for the axis. For non scatter chart types 0 value 
        /// will be inside the range defined by minimum and maximum values.  For example if data points have values: 
        /// 112, 117 and 114 the auto minimum and maximum values will be 0 and 160. If RangeFromZero property is set 
        /// to �false� the auto minimum and maximum values will be: 110 and 118. The default value is true.
        /// </remarks>
        /// <seealso cref="RangeFromZeroProperty"/>
        //[Description("Gets or sets a value indicating whether the zero value is inside the auto range.")]
        //[Category("Range")]
        public bool RangeFromZero
        {
            get
            {
                return (bool)this.GetValue(Axis.RangeFromZeroProperty);
            }
            set
            {
                this.SetValue(Axis.RangeFromZeroProperty, value);
            }
        }

        #endregion RangeFromZero

        #region Logarithmic

        /// <summary>
        /// Identifies the <see cref="Logarithmic"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LogarithmicProperty = DependencyProperty.Register("Logarithmic",
            typeof(bool), typeof(Axis), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether the logarithmic scale is enabled.
        /// </summary>
        /// <remarks>
        /// The logarithm for a base b and a number x is defined to be the inverse function of 
        /// taking b to the power x. This property is used for value axes only (in most cases it 
        /// is Y axes, except scatter chart types which have all axes as a value axis). For category 
        /// axes this property is ignored (for no scatter chart types category axes are X and Z). The 
        /// default value is false.
        /// </remarks>
        /// <seealso cref="LogarithmicProperty"/>
        //[Description("Gets or sets a value indicating whether the logarithmic scale is enabled.")]
        //[Category("Scale")]
        public bool Logarithmic
        {
            get
            {
                return (bool)this.GetValue(Axis.LogarithmicProperty);
            }
            set
            {
                this.SetValue(Axis.LogarithmicProperty, value);
            }
        }

        #endregion Logarithmic

        #region LogarithmicBase

        /// <summary>
        /// Identifies the <see cref="LogarithmicBase"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LogarithmicBaseProperty = DependencyProperty.Register("LogarithmicBase",
            typeof(double), typeof(Axis), new FrameworkPropertyMetadata((double)10, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnLogarithmicBaseValidate));

        /// <summary>
        /// Gets or sets a base for logarithmic scale.
        /// </summary>
        /// <remarks>
        /// The logarithm for a base b and a number x is defined to be the inverse function of 
        /// taking b to the power x. The default value for LogarithmicBase is 10.
        /// </remarks>
        /// <seealso cref="LogarithmicBaseProperty"/>
        //[Description("Gets or sets a base for logarithmic scale.")]
        //[Category("Scale")]
        public double LogarithmicBase
        {
            get
            {
                return (double)this.GetValue(Axis.LogarithmicBaseProperty);
            }
            set
            {
                this.SetValue(Axis.LogarithmicBaseProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>true if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnLogarithmicBaseValidate(object value)
        {
            double logarithmicBase = (double)value;

            return (logarithmicBase >= 2);
        }


        #endregion LogarithmicBase

        #region Crossing

        /// <summary>
        /// Identifies the <see cref="Crossing"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CrossingProperty = DependencyProperty.Register("Crossing",
            typeof(double), typeof(Axis), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets an axis value where this axis crosses another axis.
        /// </summary>
        /// <remarks>
        /// The default value for Crossing is 0.
        /// </remarks>
        /// <seealso cref="CrossingProperty"/>
        //[Description("Gets or sets an axis value where this axis crosses another axis.")]
        //[Category("Scale")]
        public double Crossing
        {
            get
            {
                return (double)this.GetValue(Axis.CrossingProperty);
            }
            set
            {
                this.SetValue(Axis.CrossingProperty, value);
            }
        }

        #endregion Crossing

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke",
            typeof(Brush), typeof(Axis), new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the axis line.
        /// </summary>
        /// <seealso cref="StrokeProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the axis line."),
        //[Category("Brushes")]
        public Brush Stroke
        {
            get
            {
                return (Brush)this.GetValue(Axis.StrokeProperty);
            }
            set
            {
                this.SetValue(Axis.StrokeProperty, value);
            }
        }

        #endregion Stroke

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness",
            typeof(double), typeof(Axis), new FrameworkPropertyMetadata((double)2, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the width of the axis line.
        /// </summary>
        /// <seealso cref="StrokeThicknessProperty"/>
        //[Description("Gets or sets the width of the axis line.")]
        //[Category("Appearance")]
        public double StrokeThickness
        {
            get
            {
                return (double)this.GetValue(Axis.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(Axis.StrokeThicknessProperty, value);
            }
        }

        #endregion StrokeThickness	

        #region Animation

        /// <summary>
        /// Identifies the <see cref="Animation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register("Animation", typeof(Animation), typeof(Axis), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged), delegate(DependencyObject d, object baseValue)
        {
            return Axis.CoerceValueCallback(d, baseValue, Axis.AnimationProperty);
        }));

        /// <summary>
        /// Gets or sets the animation for Axis line. This animation draws the line 
        /// from minimum to maximum axis position.
        /// </summary>
        /// <remarks>
        /// This animation is only used to create growing effect for the line, but Axis line animation could be 
        /// also created using brush property and WPF animation.
        /// </remarks>
        /// <seealso cref="AnimationProperty"/>
        //[Description("Gets or sets the animation for Axis line. This animation draws the line from minimum to maximum axis position.")]
        public Animation Animation
        {
            get
            {
                Animation obj = (Animation)this.GetValue(Axis.AnimationProperty);
                if (obj != null)
                {
                    obj.ChartParent = this;
                }
                
                return obj;
            }
            set
            {
                this.SetValue(Axis.AnimationProperty, value);
            }
        }

        #endregion Animation

        #region MajorGridline

        Mark _majorGridline = new Mark();

        /// <summary>
        /// Identifies the <see cref="MajorGridline"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MajorGridlineProperty = DependencyProperty.Register("MajorGridline", typeof(Mark), typeof(Axis), new FrameworkPropertyMetadata(null, Axis.OnPropertyChanged, delegate(DependencyObject d, object baseValue)
        {
            return Axis.CoerceValueCallback(d, baseValue, Axis.MajorGridlineProperty);
        }));

        /// <summary>
        /// Gets or sets the Major Gridline.
        /// </summary>
        /// <seealso cref="MajorGridlineProperty"/>
        //[Description("Gets or sets the Major Gridline.")]
        //[Category("Marks")]
        public Mark MajorGridline
        {
            get
            {
                Mark obj = (Mark)this.GetValue(Axis.MajorGridlineProperty);
                if (obj == null)
                {
                    obj = _majorGridline;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(Axis.MajorGridlineProperty, value);
            }
        }

        #endregion MajorGridline
     
        #region MinorGridline

        Mark _minorGridline = new Mark();

        /// <summary>
        /// Identifies the <see cref="MinorGridline"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinorGridlineProperty = DependencyProperty.Register("MinorGridline", typeof(Mark), typeof(Axis), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged), delegate(DependencyObject d, object baseValue)
        {
            return Axis.CoerceValueCallback(d, baseValue, Axis.MinorGridlineProperty);
        }));

        /// <summary>
        /// Gets or sets the Minor Gridline.
        /// </summary>
        /// <seealso cref="MinorGridlineProperty"/>
        //[Description("Gets or sets the Minor Gridline.")]
        //[Category("Marks")]
        public Mark MinorGridline
        {
            get
            {
                Mark obj = (Mark)this.GetValue(Axis.MinorGridlineProperty);
                if (obj == null)
                {
                    obj = _minorGridline;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(Axis.MinorGridlineProperty, value);
            }
        }

        #endregion MinorGridline

        #region MajorTickMark

        Mark _majorTickMark = new Mark();

        /// <summary>
        /// Identifies the <see cref="MajorTickMark"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MajorTickMarkProperty = DependencyProperty.Register("MajorTickMark", typeof(Mark), typeof(Axis), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged), delegate(DependencyObject d, object baseValue)
        {
            return Axis.CoerceValueCallback(d, baseValue, Axis.MajorTickMarkProperty);
        }));

        /// <summary>
        /// Gets or sets the Major TickMark.
        /// </summary>
        /// <seealso cref="MajorTickMarkProperty"/>
        //[Description("Gets or sets the Major TickMark.")]
        //[Category("Marks")]
        public Mark MajorTickMark
        {
            get
            {
                Mark obj = (Mark)this.GetValue(Axis.MajorTickMarkProperty);
                if (obj == null)
                {
                    obj = _majorTickMark;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(Axis.MajorTickMarkProperty, value);
            }
        }

        #endregion MajorTickMark

        #region MinorTickMark

        Mark _minorTickMark = new Mark();

        /// <summary>
        /// Identifies the <see cref="MinorTickMark"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinorTickMarkProperty = DependencyProperty.Register("MinorTickMark", typeof(Mark), typeof(Axis), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged), delegate(DependencyObject d, object baseValue)
        {
            return Axis.CoerceValueCallback(d, baseValue, Axis.MinorTickMarkProperty);
        }));

        /// <summary>
        /// Gets or sets the Minor TickMark.
        /// </summary>
        /// <seealso cref="MinorTickMarkProperty"/>
        //[Description("Gets or sets the Minor TickMark.")]
        //[Category("Marks")]
        public Mark MinorTickMark
        {
            get
            {
                Mark obj = (Mark)this.GetValue(Axis.MinorTickMarkProperty);
                if (obj == null)
                {
                    obj = _minorTickMark;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(Axis.MinorTickMarkProperty, value);
            }
        }

        #endregion MinorTickMark

        #region Stripes

        private StripeCollection _stripes = new StripeCollection();

        /// <summary>
        /// Gets the collection of stripes for this axis.
        /// </summary>
        //[Description("Gets the collection of stripes for this axis.")]
        //[Category("Data")]
        public StripeCollection Stripes
        {
            get
            {
                if (_stripes.ChartParent == null)
                {
                    _stripes.ChartParent = this;
                }
                return _stripes;
            }
        }

        #endregion Stripes

        #region Label

        Label _label = new Label(); 

        /// <summary>
        /// Identifies the <see cref="Label"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(Label), typeof(Axis), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged), delegate(DependencyObject d, object baseValue)
        {
            return Axis.CoerceValueCallback(d, baseValue, Axis.LabelProperty);
        }));

        /// <summary>
        /// Gets or sets the Axis Labels
        /// </summary>
        /// <seealso cref="LabelProperty"/>
        //[Description("Gets or sets the Axis Labels.")]
        //[Category("Marks")]
        public Label Label
        {
            get
            {
                Label obj = (Label)this.GetValue(Axis.LabelProperty);
                if (obj == null)
                {
                    obj = _label;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(Axis.LabelProperty, value);
            }
        }

        #endregion Label

        #region Visible

        /// <summary>
        /// Identifies the <see cref="Visible"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible",
            typeof(bool), typeof(Axis), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the axis is visible.
        /// </summary>
        /// <seealso cref="VisibleProperty"/>
        //[Description("Gets or sets a value that indicates whether the axis is visible.")]
        //[Category("Behavior")]
        public bool Visible
        {
            get
            {
                return (bool)this.GetValue(Axis.VisibleProperty);
            }
            set
            {
                this.SetValue(Axis.VisibleProperty, value);
            }
        }

        #endregion Visible

        #region Name

        /// <summary>
        /// Identifies the <see cref="Name"/> dependency property
        /// </summary>
        public new static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name",
            typeof(string), typeof(Axis), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Axis Name. Used for secondary Axes to make connection between Series and Axes.
        /// </summary>
        /// <remarks>
        /// Every series needs X and Y axes for 2D charts and X, Y and Z axes for 3D charts.  Every chart scene 
        /// can have 6 axes 3 Primary and 3 secondary. For example, series can use only one X axis, and it could 
        /// be primary or secondary.  To specify which axis we want to use we have to set the same text for AxisNameX 
        /// property of the Series and the Name property of an axis. If those properties are empty the primary axes are used.
        /// </remarks>
        /// <seealso cref="NameProperty"/>
        //[Description("Gets or sets the Axis Name. Used for secondary Axes to make connection between Series and Axes.")]
        public new string Name
        {
            get
            {
                return (string)this.GetValue(Axis.NameProperty);
            }
            set
            {
                this.SetValue(Axis.NameProperty, value);
            }
        }

        #endregion Name

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