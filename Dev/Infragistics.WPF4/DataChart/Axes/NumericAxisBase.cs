using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Data;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for all XamDataChart numeric axes.
    /// </summary>
    [WidgetIgnoreDepends("ValueOverlay")]
    public abstract class NumericAxisBase : Axis
    {
        internal override AxisView CreateView()
        {
            return new NumericAxisBaseView(this);
        }
        internal override void OnViewCreated(AxisView view)
        {
            base.OnViewCreated(view);

            NumericView = (NumericAxisBaseView)view;
        }
        internal NumericAxisBaseView NumericView { get; set; }
        
        /// <summary>
        /// Constructs a new NumericAxisBase instance.
        /// </summary>
        protected NumericAxisBase()
        {
            LogarithmBaseCached = 10;
        }

        #region MinimumValue Dependency Property
        /// <summary>
        /// Gets or sets the MinimumValue property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public double MinimumValue
        {
            get
            {
                return (double)GetValue(MinimumValueProperty);
            }
            set
            {
                SetValue(MinimumValueProperty, value);
            }
        }

        internal const string MinimumValuePropertyName = "MinimumValue";

        /// <summary>
        /// Identifies the MinimumValue dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumValueProperty = DependencyProperty.Register(MinimumValuePropertyName, typeof(double), typeof(NumericAxisBase),
            new PropertyMetadata(double.NaN, (sender, e) =>
            {
                (sender as NumericAxisBase).RaisePropertyChanged(MinimumValuePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualMinimumValue Property
        internal const string ActualMinimumValuePropertyName = "ActualMinimumValue";

        /// <summary>
        /// Gets the effective minimum value for the current numeric axis object.
        /// </summary>
        public double ActualMinimumValue
        {
            get { return actualMinimumValue; }
            internal set
            {
                if (ActualMinimumValue != value)
                {
                    double oldValue = actualMinimumValue;

                    actualMinimumValue = value;
                    logActualMinimumValue = Math.Log(ActualMinimumValue);
                    RaisePropertyChanged(ActualMinimumValuePropertyName, oldValue, ActualMinimumValue);
                }
            }
        }
        private double actualMinimumValue;
        internal double logActualMinimumValue { get; private set; }
        #endregion

        #region MaximumValue Dependency Property
        /// <summary>
        /// Gets or sets the MaximumValue property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public double MaximumValue
        {
            get
            {
                return (double)GetValue(MaximumValueProperty);
            }
            set
            {
                SetValue(MaximumValueProperty, value);
            }
        }

        internal const string MaximumValuePropertyName = "MaximumValue";

        /// <summary>
        /// Identifies the MaximumValue dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumValueProperty = DependencyProperty.Register(MaximumValuePropertyName, typeof(double), typeof(NumericAxisBase),
            new PropertyMetadata(double.NaN, (sender, e) =>
            {
                (sender as NumericAxisBase).RaisePropertyChanged(MaximumValuePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualMaximumValue Property
        internal const string ActualMaximumValuePropertyName = "ActualMaximumValue";

        /// <summary>
        /// Gets the effective maximum value for the current numeric axis object.
        /// </summary>
        public double ActualMaximumValue
        {
            get { return actualMaximumValue; }
            internal set
            {
                if (ActualMaximumValue != value)
                {
                    double oldValue = actualMaximumValue;

                    actualMaximumValue = value;
                    logActualMaximumValue = Math.Log(ActualMaximumValue);
                    RaisePropertyChanged(ActualMaximumValuePropertyName, oldValue, ActualMaximumValue);
                }
            }
        }
        private double actualMaximumValue;
        internal double logActualMaximumValue { get; private set; }
        #endregion

        #region Interval Dependency Property
        internal const string IntervalPropertyName = "Interval";
        /// <summary>
        /// Identifies the Interval dependency property.
        /// </summary>
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(IntervalPropertyName, typeof(double), typeof(NumericAxisBase),



            new PropertyMetadata((sender, e) =>

            {
                (sender as NumericAxisBase).RaisePropertyChanged(IntervalPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// 
        /// </summary>
        public double Interval
        {
            get
            {
                return (double)GetValue(IntervalProperty);
            }
            set
            {
                SetValue(IntervalProperty, value);
            }
        }
        #endregion

        // there should be an "ActualReferenceValue" which gets updated automatically with MinimumValue and MaximumValue

        #region ReferenceValue Dependency Property
        /// <summary>
        /// Gets or sets the ReferenceValue property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(0.0)]
        public double ReferenceValue
        {
            get
            {
                return (double)GetValue(ReferenceValueProperty);
            }
            set
            {
                SetValue(ReferenceValueProperty, value);
            }
        }

        internal const string ReferenceValuePropertyName = "ReferenceValue";

        /// <summary>
        /// Identifies the ReferenceValue dependency property.
        /// </summary>
        public static readonly DependencyProperty ReferenceValueProperty = DependencyProperty.Register(ReferenceValuePropertyName, typeof(double), typeof(NumericAxisBase),
            new PropertyMetadata(0.0, (sender, e) =>
            {
                (sender as NumericAxisBase).RaisePropertyChanged(ReferenceValuePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region IsLogarithmic Dependency Property
        /// <summary>
        /// Gets or sets the IsLogarithmic property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultBoolean(false)]
        public bool IsLogarithmic
        {
            get
            {
                return (bool)GetValue(IsLogarithmicProperty);
            }
            set
            {
                SetValue(IsLogarithmicProperty, value);
            }
        }

        internal const string IsLogarithmicPropertyName = "IsLogarithmic";

        /// <summary>
        /// Identifies the IsLogarithmic dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLogarithmicProperty = DependencyProperty.Register(IsLogarithmicPropertyName, typeof(bool), typeof(NumericAxisBase),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as NumericAxisBase).RaisePropertyChanged(IsLogarithmicPropertyName, e.OldValue, e.NewValue);
            }));

        private const string ActualIsLogarithmicPropertyName = "ActualIsLogarithmic";
        private bool _actualIsLogarithmic;
        
        /// <summary>
        /// Determines if the axis has a valid logarithmic scale.
        /// </summary>
        public bool ActualIsLogarithmic
        {
            get { return _actualIsLogarithmic; }
            internal set
            {
                if (ActualIsLogarithmic != value)
                {
                    bool oldValue = _actualIsLogarithmic;

                    if (oldValue != value)
                    {
                        _actualIsLogarithmic = value;
                        RaisePropertyChanged(ActualIsLogarithmicPropertyName, oldValue, ActualIsLogarithmic);
                    }
                }
            }
        }
        
        #endregion

        internal bool IsReallyLogarithmic
        {
            get
            {
                return ActualIsLogarithmic && ActualMinimumValue > 0.0 && LogarithmBaseCached > 1;
            }
        }

        #region LogarithmBase Dependency Property
        /// <summary>
        /// Gets or sets the LogarithmBase property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(10)]
        public int LogarithmBase
        {
            get
            {
                return (int)GetValue(LogarithmBaseProperty);
            }
            set
            {
                SetValue(LogarithmBaseProperty, value);
            }
        }

        internal const string LogarithmBasePropertyName = "LogarithmBase";

        /// <summary>
        /// Identifies the LogarithmBase dependency property.
        /// </summary>
        public static readonly DependencyProperty LogarithmBaseProperty = DependencyProperty.Register(LogarithmBasePropertyName, typeof(int), typeof(NumericAxisBase),
            new PropertyMetadata(10, (sender, e) =>
            {
                (sender as NumericAxisBase).RaisePropertyChanged(LogarithmBasePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// Gets or sets the cached value of the log base.
        /// </summary>
        protected internal int LogarithmBaseCached { get; set; }

        /// <summary>
        /// Houses the rendering logic for the axis.
        /// </summary>
        internal NumericAxisRenderer Renderer { get; set; }

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the axis. Gives the axis a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case NumericAxisBase.MinimumValuePropertyName:
                    UpdateRange();
                    RenderAxis(false);
                    break;

                case NumericAxisBase.MaximumValuePropertyName:
                    UpdateRange();
                    RenderAxis(false);
                    break;

                case NumericAxisBase.IsLogarithmicPropertyName:
                    this.ActualIsLogarithmic = this.IsLogarithmic;
                    break;

                case Axis.CrossingValuePropertyName:
                case Axis.CrossingAxisPropertyName:
                case NumericAxisBase.IntervalPropertyName:
                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    break;

                case NumericAxisBase.LogarithmBasePropertyName:
                    LogarithmBaseCached = LogarithmBase;

                    if (this.ActualIsLogarithmic)
                    {
                        UpdateRange();
                        this.InvalidateSeries();
                        RenderAxis(false);
                    }
                    break;

                case NumericAxisBase.ReferenceValuePropertyName:
                    //the range doesn't really change but this will notify the series to redraw themselves.
                    AxisRangeChangedEventArgs ea =
                        new AxisRangeChangedEventArgs(
                            ActualMinimumValue,
                            ActualMinimumValue,
                            ActualMaximumValue,
                            ActualMaximumValue);
                    RaiseRangeChanged(ea);
                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    break;
                case NumericAxisBase.LabelSettingsPropertyName:
                    Renderer = CreateRenderer();
                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    break;
                case NumericAxisBase.TickmarkValuesPropertyName:
                    this.UpdateActualTickmarkValues();
                    break;
                case NumericAxisBase.ActualIsLogarithmicPropertyName:
                    //need to update the axis range first, because the current min range can be at 0, even though the values in the datasource are positive.
                    UpdateRange();
                    this.InvalidateSeries();

                    foreach (Series series in Series)
                    {
                        ValueOverlay overlay = series as ValueOverlay;

                        if (overlay != null)
                        {
                            NumericAxisBase numericAxis = overlay.Axis as NumericAxisBase;
                            numericAxis.ActualMinimumValue = double.PositiveInfinity;
                            numericAxis.ActualMaximumValue = double.NegativeInfinity;
                        }
                    }


                    MustInvalidateLabels = true;
                    this.UpdateActualTickmarkValues();
                    RenderAxis(false);
                    break;
                case NumericAxisBase.ActualTickmarkValuesPropertyName:
                    this.MustInvalidateLabels = true;
                    this.RenderAxis(false);
                    break;
            }

        }
        /// <summary>
        /// Invalidates all series associated with the current axis.
        /// </summary>
        protected void InvalidateSeries()
        {
            foreach (Series series in DirectSeries())
            {
                series.RenderSeries(false);
            }
        }

        internal AxisRange GetAxisRange()
        {
            AxisRange newRange = new AxisRange(double.PositiveInfinity, double.NegativeInfinity);
            bool rangeFound = false;
            if (SeriesViewer != null)
            {
                foreach (Series series in DirectSeries())
                {
                    AxisRange range = series.GetRange(this);

                    if (range != null)
                    {
                        rangeFound = true;
                        newRange = new AxisRange(
                            Math.Min(newRange.Minimum, range.Minimum),
                            Math.Max(newRange.Maximum, range.Maximum));
                    }
                }
            }
            if (rangeFound)
            {
                return newRange;
            }
            return null;
        }

        /// <summary>
        /// Calculates the axis range.
        /// </summary>
        /// <param name="target">Target axis</param>
        /// <param name="minimumValue">Minimum value</param>
        /// <param name="maximumValue">Maximum valuje</param>
        /// <param name="isLogarithmic">Whether or not the axis is logarithmic</param>
        /// <param name="logarithmBase">Logarithm base</param>
        /// <param name="actualMinimumValue">Actual coerced minimum value</param>
        /// <param name="actualMaximumValue">Actual coerced maximum value</param>
        protected internal virtual void CalculateRange(
            NumericAxisBase target, double minimumValue, 
            double maximumValue, bool isLogarithmic, 
            int logarithmBase, 
            out double actualMinimumValue, out double actualMaximumValue)
        {
            AutoRangeCalculator.CalculateRange(
                target, minimumValue, maximumValue,
                isLogarithmic, logarithmBase,
                out actualMinimumValue, out actualMaximumValue);
        }

        internal override bool UpdateRangeOverride()
        {
            bool isLogarithmic = this.ActualIsLogarithmic
                && !double.IsNaN(LogarithmBase)
                && !double.IsInfinity(LogarithmBase)
                && LogarithmBase > 1.0;

            double minimumValue;
            double maximumValue;

            CalculateRange(
                this,
                MinimumValue,
                MaximumValue,
                isLogarithmic,
                LogarithmBase,
                out minimumValue,
                out maximumValue);

            if (minimumValue != ActualMinimumValue || maximumValue != ActualMaximumValue)
            {
                AxisRangeChangedEventArgs ea =
                    new AxisRangeChangedEventArgs(ActualMinimumValue, minimumValue,
                        ActualMaximumValue, maximumValue);

                ActualMinimumValue = minimumValue;
                //logActualMinimumValue = Math.Log(minimumValue);

                ActualMaximumValue = maximumValue;
                //logActualMaximumValue = Math.Log(maximumValue);

                RaiseRangeChanged(ea);
                OnRangeChanged(ea);
                RenderAxis(true);

                return true;
            }

            return false;
        }

        internal virtual void OnRangeChanged(AxisRangeChangedEventArgs ea)
        {
            
        }

        /// <summary>
        /// Registers a series that uses an axis with the axis.
        /// </summary>
        /// <param name="series">The series to register.</param>
        /// <returns>If the registration was a success.</returns>
        public override bool RegisterSeries(Series series)
        {
            bool success = base.RegisterSeries(series);
            if (success)
            {
                UpdateRange();
            }
            return success;
        }

        /// <summary>
        /// Deregisters a series that uses an axis from the axis.
        /// </summary>
        /// <param name="series">The series to deregister.</param>
        /// <returns>If the deregistration was a success.</returns>
        public override bool DeregisterSeries(Series series)
        {
            bool success = base.DeregisterSeries(series);
            if (success)
            {
                UpdateRange();
            }
            return success;
        }

        internal virtual NumericAxisRenderer CreateRenderer()
        {
            AxisLabelManager labelManager = new AxisLabelManager()
            {
                Axis = this,
                LabelPositions = LabelPositions,
                LabelDataContext = LabelDataContext,
                TargetPanel = LabelPanel
            };

            if (LabelSettings != null)
            {
                LabelSettings.Axis = this;
            }

            NumericAxisRenderer renderer = new NumericAxisRenderer(labelManager);

            renderer.Clear = () =>
            {
                GeometryCollection axisGeometry = View.GetAxisLinesGeometry();
                GeometryCollection stripsGeometry = View.GetStripsGeometry();
                GeometryCollection majorGeometry = View.GetMajorLinesGeometry();
                GeometryCollection minorGeometry = View.GetMinorLinesGeometry();

                UpdateLineVisibility();

                ClearMarks(axisGeometry);
                ClearMarks(stripsGeometry);
                ClearMarks(majorGeometry);
                ClearMarks(minorGeometry);
            };

            renderer.ShouldRender = (viewport, window) =>
            {
                return !window.IsEmpty
                    && !viewport.IsEmpty;
            };

            renderer.CreateRenderingParams = (viewport, window) =>
            {
                return CreateRenderingParams(viewport, window);
            };

            renderer.GetLabelForItem = (item) =>
            {
                return GetLabel(item);
            };

            return renderer;
        }

        internal virtual NumericAxisRenderingParameters CreateRenderingParamsInstance()
        {
            return new NumericAxisRenderingParameters();
        }

        internal virtual void FloatLabelPanel()
        {
            
        }
        /// <summary>
        /// Creates a new numeric axis scaler.
        /// </summary>
        /// <returns>New axis scaler</returns>
        protected internal virtual NumericScaler CreateScalerOverride()
        {
            return null;
        }

        internal virtual NumericAxisRenderingParameters CreateRenderingParams(
            Rect viewportRect,
            Rect windowRect)
        {
            NumericAxisRenderingParameters parameters = CreateRenderingParamsInstance();
            GeometryCollection axisGeometry = View.GetAxisLinesGeometry();
            GeometryCollection stripsGeometry = View.GetStripsGeometry();
            GeometryCollection majorGeometry = View.GetMajorLinesGeometry();
            GeometryCollection minorGeometry = View.GetMinorLinesGeometry();

            parameters.AxisGeometry = axisGeometry;
            parameters.Strips = stripsGeometry;
            parameters.Major = majorGeometry;
            parameters.Minor = minorGeometry;
            parameters.ActualMaximumValue = ActualMaximumValue;
            parameters.ActualMinimumValue = ActualMinimumValue;
            
            parameters.HasUserMax = HasUserMaximum;
            
            parameters.TickmarkValues = this.ActualTickmarkValues;
            parameters.ViewportRect = viewportRect;
            parameters.WindowRect = windowRect;
            parameters.HasUserInterval = HasUserInterval();
            parameters.Interval = Interval;
            parameters.Label = Label;


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            parameters.ShouldRenderMinorLines = ShouldRenderMinorLines;

            return parameters;
        }

        /// <summary>
        /// Unscales a value from screen space into axis space.
        /// </summary>
        /// <param name="unscaledValue">The scaled value in screen coordinates to unscale into axis space.</param>
        /// <returns>The unscaled value in axis space.</returns>
        public double UnscaleValue(double unscaledValue)
        {
            ScalerParams sParams = new ScalerParams(SeriesViewer.WindowRect, ViewportRect, IsInverted);
            sParams.EffectiveViewportRect = SeriesViewer.EffectiveViewport;
            return GetUnscaledValue(unscaledValue, sParams);
        }



#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)




        internal bool HasUserInterval()
        {



            return ReadLocalValue(IntervalProperty) != DependencyProperty.UnsetValue;

        }

        /// <summary>
        /// Determines if the axis has a user-defined minimum.
        /// </summary>
        public bool HasUserMinimum
        {
            get
            {



                return ReadLocalValue(NumericAxisBase.MinimumValueProperty) != DependencyProperty.UnsetValue;

            }
        }

        /// <summary>
        /// Determines if the axis has a user-defined maximum.
        /// </summary>
        public bool HasUserMaximum {
            get
            {



                return ReadLocalValue(NumericAxisBase.MaximumValueProperty) != DependencyProperty.UnsetValue;

            }
        }
        private void UpdateActualTickmarkValues()
        {
            if (this.TickmarkValues != null)
            {
                this.ActualTickmarkValues = this.TickmarkValues;
            }
            else if (this.ActualIsLogarithmic)
            {
                this.ActualTickmarkValues = new LogarithmicTickmarkValues();
                
                NumericView.BindLogarithmBaseToActualTickmarks();
            }
            else
            {
                this.ActualTickmarkValues = new LinearTickmarkValues();
            }
        }

        private const string TickmarkValuesPropertyName = "TickmarkValues";

        /// <summary>
        /// Identifies the TickmarkValues dependency property.
        /// </summary>
        public static readonly DependencyProperty TickmarkValuesProperty = DependencyProperty.Register(TickmarkValuesPropertyName, typeof(TickmarkValues), typeof(NumericAxisBase), new PropertyMetadata((sender, e) =>
        {
            (sender as NumericAxisBase).RaisePropertyChanged(TickmarkValuesPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets tickmark values.
        /// </summary>
        [SuppressWidgetMember]
        public TickmarkValues TickmarkValues
        {
            get
            {
                return this.GetValue(TickmarkValuesProperty) as TickmarkValues;
            }
            set
            {
                this.SetValue(TickmarkValuesProperty, value);
            }
        }
        private const string ActualTickmarkValuesPropertyName = "ActualTickmarkValues";
        private TickmarkValues _actualTickmarkValues;

        /// <summary>
        /// Gets or sets the actual tickmark values.
        /// </summary>
        [SuppressWidgetMember]
        public TickmarkValues ActualTickmarkValues
        {
            get
            {
                if (this._actualTickmarkValues == null)
                {
                    this.UpdateActualTickmarkValues();
                }
                return this._actualTickmarkValues;
            }
            set
            {
                object oldValue = this._actualTickmarkValues;
                bool changed = oldValue != value;
                if (changed)
                {
                    this._actualTickmarkValues = value;
                    this.RaisePropertyChanged(ActualTickmarkValuesPropertyName, oldValue, value);
                }
            }
        }
    }

    /// <summary>
    /// Defines a set of basic methods and properties used to create a StranghtNumeric axis.
    /// </summary>
    public abstract class StraightNumericAxisBase : NumericAxisBase
    {
        /// <summary>
        /// Creates new instance of StraightNumeric axis.
        /// </summary>
        public StraightNumericAxisBase() : base()
        {
            // calling this in the ctor so it calls the Scaler getter which will create a default instance... yeah
            this.UpdateActualScaler();
        }
        internal override AxisView CreateView()
        {
            return new StraightNumericAxisBaseView(this);
        }
        internal override void OnViewCreated(AxisView view)
        {
            base.OnViewCreated(view);
            StraightView = (StraightNumericAxisBaseView)view;
        }
        internal StraightNumericAxisBaseView StraightView { get; set; }

        internal const string ScaleModePropertyName = "ScaleMode";

        /// <summary>
        /// Identifies the ScaleMode dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleModeProperty = DependencyProperty.Register(ScaleModePropertyName, typeof(NumericScaleMode), typeof(StraightNumericAxisBase), new PropertyMetadata(NumericScaleMode.Linear, (sender, e) =>
        {
            (sender as StraightNumericAxisBase).RaisePropertyChanged(ScaleModePropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the axis scale mode.
        /// </summary>
        [SuppressWidgetMember]
        public NumericScaleMode ScaleMode
        {
            get
            {
                return (NumericScaleMode)this.GetValue(ScaleModeProperty);
            }
            set
            {
                this.SetValue(ScaleModeProperty, value);
            }
        }
        
        internal const string ScalerPropertyName = "Scaler";

        /// <summary>
        /// Identifies the Scaler dependency property.
        /// </summary>
        public static readonly DependencyProperty ScalerProperty = DependencyProperty.Register(ScalerPropertyName, typeof(NumericScaler), typeof(StraightNumericAxisBase), new PropertyMetadata(null, OnScalerPropertyChanged));

        /// <summary>
        /// Gets or sets the axis scaler.
        /// </summary>
        [SuppressWidgetMember]
        public NumericScaler Scaler
        {
            get
            {
                return (NumericScaler)this.GetValue(ScalerProperty);
            }
            set
            {
                this.SetValue(ScalerProperty, value);
            }
        }

        /// <summary>
        /// Event Callback for when the Scaler Property Changes
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnScalerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var strNumAcxisBase = d as StraightNumericAxisBase;

            strNumAcxisBase.UpdateActualScaler();
            strNumAcxisBase.RaisePropertyChanged(ScalerPropertyName, e.OldValue, e.NewValue);
        }

        /// <summary>
        /// Creates a new linear scaler.
        /// </summary>
        /// <returns>New linear scaler</returns>
        protected virtual NumericScaler CreateLinearScaler()
        {
            return null;
        }

        private const string ActualScalerPropertyName = "ActualScaler";
        protected NumericScaler _actualScaler;

        /// <summary>
        /// Gets or sets the actual axis scaler.
        /// </summary>
        protected internal virtual NumericScaler ActualScaler
        {
            get
            {
                if (this._actualScaler == null)
                {
                    this.UpdateActualScaler();
                }
                return this._actualScaler;
            }
            set
            {
                bool changed = this._actualScaler != value;
                if (changed)
                {
                    object oldValue = this._actualScaler;
                    this._actualScaler = value;
                    this.RaisePropertyChanged(ActualScalerPropertyName, oldValue, value);
                }
            }
        }

        /// <summary>
        /// Calculates the axis range.
        /// </summary>
        /// <param name="target">target axis</param>
        /// <param name="minimumValue">minimum value</param>
        /// <param name="maximumValue">maximum value</param>
        /// <param name="isLogarithmic">whether the axis is logarithmic</param>
        /// <param name="logarithmBase">log base</param>
        /// <param name="actualMinimumValue">actual minimum value</param>
        /// <param name="actualMaximumValue">actual maximum value</param>
        protected internal override void CalculateRange(
           NumericAxisBase target,
           double minimumValue,
           double maximumValue,
           bool isLogarithmic,
           int logarithmBase,
           out double actualMinimumValue, out double actualMaximumValue)
        {
            this.ActualScaler.CalculateRange(
                target, minimumValue, maximumValue,
                out actualMinimumValue, out actualMaximumValue);
        }

        private void SuspendPropertyUpdatedAndExecute(Action a)
        {
            bool suspendPropertyUpdatedStored = this.SuspendPropertyUpdated;
            this.SuspendPropertyUpdated = true;
            a.Invoke();
            this.SuspendPropertyUpdated = suspendPropertyUpdatedStored;
        }
        private bool SuspendPropertyUpdated { get; set; }

        /// <summary>
        /// Updates the actual axis scaler.
        /// </summary>
        protected virtual void UpdateActualScaler()
        {
            NumericScaler scaler = this.Scaler;
            if (scaler == null)
            {
                scaler = this.CreateScalerOverride();
            }

            this.ActualScaler = scaler;
            
            if (this.ActualScaler == null)
            {
                throw new ArgumentNullException("ActualScaler");
            }
            BindScalerProperties();
        }

        /// <summary>
        /// Updates axis scaler's properties.
        /// </summary>
        protected internal virtual void BindScalerProperties()
        {
            StraightView.BindScalerProperties();
        }

        /// <summary>
        /// Handles property updates.
        /// </summary>
        /// <param name="sender">source object</param>
        /// <param name="propertyName">property name</param>
        /// <param name="oldValue">old property value</param>
        /// <param name="newValue">new property value</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            if (this.SuspendPropertyUpdated)
            {
                return;
            }
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case NumericAxisBase.LogarithmBasePropertyName:
                    this.UpdateActualScaler();
                    break;
                case NumericAxisBase.IsLogarithmicPropertyName:
                    this.UpdateActualScaler();
                    break;
                case StraightNumericAxisBase.ScaleModePropertyName:
                    this.UpdateActualScaler();
                    break;
                case StraightNumericAxisBase.ScalerPropertyName:
                    this.UpdateActualScaler();
                    break;
                case StraightNumericAxisBase.ActualScalerPropertyName:
                    this.ActualIsLogarithmic = this.ActualScaler is LogarithmicScaler;
                    BindScalerProperties();
                    this.UpdateRange();
                    RenderAxis(false);
                    break;
                case NumericAxisBase.ActualMaximumValuePropertyName:
                    OnActualMaximumValueChanged();
                    break;
                case NumericAxisBase.ActualMinimumValuePropertyName:
                    OnActualMinimumValueChanged();
                    this.UpdateActualScaler();
                    break;
            }
        }


        private void OnActualMinimumValueChanged()
        {



        }

        private void OnActualMaximumValueChanged()
        {



        }
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