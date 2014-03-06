
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Data Point class represents a basic data element which appears on 
    /// the chart (Column, pie slice, line segment, etc). This class also 
    /// keeps information about visual appearance of the element.
    /// </summary>
    public class DataPoint : ChartFrameworkContentElement, IWeakEventListener
    {
        #region Fields

        // Private Fields
        private object _chartParent;

        private Dictionary<string, string> _bindNotifyDict;

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
        /// Initializes a new instance of the DataPoint class. 
        /// </summary>
        public DataPoint()
        {
        }

        /// <summary>
        /// Invoked when an unhandled Mouse.MouseEnter attached event is raised on this element. 
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The MouseEventArgs that contains the event data.</param>
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            if (!IsMouseOver)
            {
                this.SetValue(DataPoint.IsMouseOverPropertyKey, true);
            }
            
            base.OnMouseEnter(e);
        }

        /// <summary>
        /// Invoked when an unhandled Mouse.MouseLeave attached event is raised on this element. 
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The MouseEventArgs that contains the event data.</param>
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (IsMouseOver)
            {
                this.SetValue(DataPoint.IsMouseOverPropertyKey, false);
            }

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Add child objects to logical tree
        /// </summary>
        internal void AddChildren()
        {
            AddChild(this.Marker);
            AddChild(this.Animation);
        }

        /// <summary>
        /// Gets an enumerator for this element's logical child elements.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                ArrayList _list = new ArrayList();

                _list.Add(this.Marker);
                _list.Add(this.Animation);

                return (IEnumerator)_list.GetEnumerator();
            }
        }

        /// <summary>
        /// Sends notice when the specified property has been invalidated. 
        /// </summary>
        /// <param name="e">Event arguments that describe the property that changed, including the old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataPoint.ToolTipProperty)
            {
                XamChart control = XamChart.GetControl(this);
                if (control != null && e.NewValue != e.OldValue)
                {
                    bool stringEqual =
                        e.NewValue is string &&
                        e.OldValue is string &&
                        string.Equals(
                        (string)e.NewValue, 
                        (string)e.OldValue, 
                        StringComparison.Ordinal);

                    if (!stringEqual)
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
            if (e.Property.Name == "IsMouseOver")
            {
                return;
            }
            XamChart control = XamChart.GetControl(d);
            if (control != null && e.NewValue != e.OldValue)
            {
                control.RefreshProperty();
            }
        }

        public static bool IsColorFromPoint(Series series)
        {
            bool colorFromPoint = ChartSeries.GetChartAttribute(ChartTypeAttribute.ColorFromPoint, series);

            if (series.DataPointColor == DataPointColor.Same)
            {
                colorFromPoint = false;
            }
            else if (series.DataPointColor == DataPointColor.Different)
            {
                colorFromPoint = true;
            }
            return colorFromPoint;
        }

        /// <summary>
        /// Get the parent series for this data point.
        /// </summary>
        internal Series GetSeries()
        {
            DataPointCollection dataPointCollection = _chartParent as DataPointCollection;
            if (dataPointCollection == null)
            {
                return null;
            }
            return dataPointCollection.ChartParent as Series;
        }

        /// <summary>
        /// Get the parent IChart for this data point.
        /// </summary>
        internal IChart GetIChart()
        {
            DataPointCollection dataPointCollection = _chartParent as DataPointCollection;
            Series series = dataPointCollection.ChartParent as Series;
            SeriesCollection seriesCollection = series.ChartParent as SeriesCollection;
            if (seriesCollection == null)
            {
                // Series does not have a parent.
                throw new InvalidOperationException(ErrorString.Exc31);
            }

            return seriesCollection.ChartParent as IChart;
        }

        /// <summary>
        /// Create fill for data point
        /// </summary>
        /// <param name="seriesIndex">Data series index</param>
        /// <param name="pointIndex">Data point index</param>
        /// <returns>Fill Brush from data point</returns>
        private Brush GetFill(int seriesIndex, int pointIndex)
        {
            Brush fill = null;

            // Find parent series
            Series series = GetSeries();
            
            bool colorFromPoint = DataPoint.IsColorFromPoint(series);
            
            // Set the brush for the data point
            if (Fill != null)
            {
                fill = this.Fill;
            }
            else if (series.Fill != null)
            {
                // Set brush from series if not defined for the data point.
                fill = series.Fill;
            }
            else
            {
                if (colorFromPoint)
                {
                    fill = series.GetPredefinedBrush(pointIndex, colorFromPoint);
                }
                else
                {
                    fill = series.GetPredefinedBrush(seriesIndex, colorFromPoint);
                }
            }

            return fill;
        }


        /// <summary>
        /// Create fill for Series
        /// </summary>
        /// <param name="seriesIndex">Data series index</param>
        /// <returns>Fill Brush from Series</returns>
        internal Brush GetSeriesFill(int seriesIndex)
        {
            Brush fill = null;

            // Find parent series
            Series series = GetSeries();

            if (series.Fill != null)
            {
                // Set brush from series.
                fill = series.Fill;
            }
            else
            {
                fill = series.GetPredefinedBrush(seriesIndex, false);
            }

            return fill;
        }

        /// <summary>
        /// Gets the legend fill.
        /// </summary>
        /// <param name="seriesIndex">Index of the series.</param>
        /// <param name="pointIndex">Index of the point.</param>
        /// <returns></returns>
        internal Brush GetLegendFill(int seriesIndex, int pointIndex)
        {
            Brush fill = null;

            // Find parent series
            Series series = GetSeries();

            bool colorFromPoint = DataPoint.IsColorFromPoint(series);

            // Set the brush for the data point
            if (colorFromPoint && Fill != null)
            {
                fill = this.Fill;
            }
            else if (series.Fill != null)
            {
                // Set brush from series if not defined for the data point.
                fill = series.Fill;
            }
            else
            {
                if (colorFromPoint)
                {
                    fill = series.GetPredefinedBrush(pointIndex, colorFromPoint);
                }
                else
                {
                    fill = series.GetPredefinedBrush(seriesIndex, colorFromPoint);
                }
            }

            return fill;
        }
        
        /// <summary>
        /// Create fill stroke for data point
        /// </summary>
        /// <param name="seriesIndex">Data series index</param>
        /// <param name="pointIndex">Data point index</param>
        /// <returns>Stroke brush for data point</returns>
        private Brush GetStroke(int seriesIndex, int pointIndex)
        {
            Brush stroke = null;

            // Find parent series
            Series series = GetSeries();
            bool colorFromPoint = DataPoint.IsColorFromPoint(series);
            bool strokeMainColor = ChartSeries.GetChartAttribute(ChartTypeAttribute.StrokeMainColor, series);

            // Set the stroke for the data point
            if (!strokeMainColor)
            {
                if (Stroke != null)
                {
                    stroke = this.Stroke;
                }
                else if (series.Stroke != null)
                {
                    // Set stroke from series if not defined for data point.
                    stroke = series.Stroke;
                }
                else
                {
                    stroke = Brushes.Black;
                }
            }
            else
            {
                if (Fill != null)
                {
                    stroke = this.Fill;
                }
                else if (series.Fill != null)
                {
                    // Set brush from series if not defined for the data point.
                    stroke = series.Fill;
                }
                else
                {
                    if (colorFromPoint)
                    {
                        stroke = series.GetPredefinedBrush(pointIndex, colorFromPoint);
                    }
                    else
                    {
                        stroke = series.GetPredefinedBrush(seriesIndex, colorFromPoint);
                    }
                }
            }

            return stroke;
        }
        

        /// <summary>
        /// Create fill stroke for Series
        /// </summary>
        /// <param name="seriesIndex">Data series index</param>
        /// <returns>Stroke brush for Series</returns>
        internal Brush GetSeriesStroke(int seriesIndex)
        {
            Brush stroke = null;

            // Find parent series
            Series series = GetSeries();
            bool strokeMainColor = ChartSeries.GetChartAttribute(ChartTypeAttribute.StrokeMainColor, series);

            // Set the stroke for series
            if (!strokeMainColor)
            {
                if (series.Stroke != null)
                {
                    // Set stroke from series.
                    stroke = series.Stroke;
                }
                else
                {
                    stroke = Brushes.Black;
                }
            }
            else
            {
                if (series.Fill != null)
                {
                    // Set brush from series.
                    stroke = series.Fill;
                }
                else
                {
                    stroke = series.GetPredefinedBrush(seriesIndex, false);
                }
            }

            return stroke;
        }

        /// <summary>
        /// Gets the legend stroke. 
        /// </summary>
        /// <remarks>
        /// In 2d and 3d charts the stroke for the chart and the legend are different
        /// </remarks>
        /// <returns></returns>
        internal Brush GetLegendStroke()
        {
            Brush stroke = null;

            // Find parent series
            Series series = GetSeries();

            if(!HasLegendStroke())
                return null;


            bool colorFromPoint = DataPoint.IsColorFromPoint(series);

            // Set the stroke for the data point
            if (colorFromPoint && Stroke != null)
            {
                stroke = this.Stroke;
            }
            else if (series.Stroke != null)
            {
                // Set stroke from series if not defined for data point.
                stroke = series.Stroke;
            }
            else
            {
                stroke = Brushes.Black;
            }

            return stroke;
        }


        /// <summary>
        /// Create stroke thickness for data point
        /// </summary>
        /// <param name="seriesIndex">Data series index</param>
        /// <param name="pointIndex">Data point index</param>
        /// <returns>Stroke thickness for data point</returns>
        internal double GetStrokeThickness(int seriesIndex, int pointIndex)
        {
            double strokeThickness = 1;

            // Find parent series
            Series series = GetSeries();
            
            // Set the stroke thickness for the data point
            if (ReadLocalValue(DataPoint.StrokeThicknessProperty) != DependencyProperty.UnsetValue || (double)DataPoint.StrokeThicknessProperty.DefaultMetadata.DefaultValue != this.StrokeThickness)
            {
                strokeThickness = this.StrokeThickness;
            }
            else if (series.ReadLocalValue(Series.StrokeThicknessProperty) != DependencyProperty.UnsetValue || (double)Series.StrokeThicknessProperty.DefaultMetadata.DefaultValue != series.StrokeThickness)
            {
                // Set stroke thickness from series if not defined for data point.
                strokeThickness = series.StrokeThickness;
            }
            
            return strokeThickness;
        }

        /// <summary>
        /// Create stroke thickness for series
        /// </summary>
        /// <param name="seriesIndex">Data series index</param>
        /// <returns>Stroke thickness for series</returns>
        internal double GetSeriesStrokeThickness(int seriesIndex)
        {
            double strokeThickness = 1;

            // Find parent series
            Series series = GetSeries();

            // Set the stroke thickness for series
            if (series.ReadLocalValue(Series.StrokeThicknessProperty) != DependencyProperty.UnsetValue || (double)Series.StrokeThicknessProperty.DefaultMetadata.DefaultValue != series.StrokeThickness)
            {
                // Set stroke thickness from series.
                strokeThickness = series.StrokeThickness;
            }

            return strokeThickness;
        }

        /// <summary>
        /// Gets the legend stroke thickness.
        /// </summary>
        /// <returns></returns>
        internal double GetLegendStrokeThickness()
        {
            double strokeThickness = 1;

            // Find parent series
            Series series = GetSeries();

            if(!HasLegendStroke())
                return 0;

            bool colorFromPoint = DataPoint.IsColorFromPoint(series);

            // Set the stroke thickness for the data point
            if (colorFromPoint && (ReadLocalValue(DataPoint.StrokeThicknessProperty) != DependencyProperty.UnsetValue || (double)DataPoint.StrokeThicknessProperty.DefaultMetadata.DefaultValue != this.StrokeThickness))
            {
                strokeThickness = this.StrokeThickness;
            }
            else if (series.ReadLocalValue(Series.StrokeThicknessProperty) != DependencyProperty.UnsetValue || (double)Series.StrokeThicknessProperty.DefaultMetadata.DefaultValue != series.StrokeThickness)
            {
                // Set stroke thickness from series if not defined for data point.
                strokeThickness = series.StrokeThickness;
            }

            return strokeThickness;
        }

        /// <summary>
        /// Determines whether the legend should have stroke.
        /// The 3d charts do not have stroke as well as some 2d charts
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has legend stroke]; otherwise, <c>false</c>.
        /// </returns>
        internal bool HasLegendStroke()
        {
            XamChart control = XamChart.GetControl(this);

            // Find parent series
            Series series = GetSeries();

            // the legend for the line and stock charts should not have stroke as the charts themselves
            if (control.View3D ||
                series.ChartType == ChartType.Stock || 
                series.ChartType == ChartType.Line ||
                series.ChartType == ChartType.Scatter ||
                series.ChartType == ChartType.ScatterLine ||
                series.ChartType == ChartType.Spline ||
                series.ChartType == ChartType.Point)
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        /// <summary>
        /// Gets marker from data point. If marker doesn�t exist gets marker from series.
        /// </summary>
        /// <returns>Marker from data point or data series</returns>
        internal Marker GetMarker()
        {
            if (Marker != null)
            {
                return Marker;
            }
            else
            {
                // Find parent series
                Series series = GetSeries();
                return series.Marker;
            }
        }

        /// <summary>
        /// Gets ToolTip from data point. If ToolTip doesn�t exist gets ToolTip from series.
        /// </summary>
        /// <returns>ToolTip from data point or data series</returns>
        internal object GetToolTip()
        {
            if (ToolTip != null)
            {
                return ToolTip;
            }
            else
            {
                // Find parent series
                Series series = GetSeries();
                return series.ToolTip;
            }
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal Brush GetParameterValueBrush(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return parameter.GetDefaultBrush() as Brush;
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return extra.GetDefaultBrush() as Brush;
            }

            // Find parent series
            Series series = GetSeries();
            return series.GetParameterValueBrush(type) as Brush;
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal Animation GetParameterValueAnimation(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return parameter.GetDefaultAnimation() as Animation;
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return extra.GetDefaultAnimation() as Animation;
            }

            // Find parent series
            Series series = GetSeries();
            return series.GetParameterValueAnimation(type) as Animation;
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal int GetParameterValueInt(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return (int)parameter.GetDefaultInt();
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return (int)extra.GetDefaultInt();
            }

            // Find parent series
            Series series = GetSeries();
            return (int)series.GetParameterValueInt(type);
        }

        /// <summary>
        /// Search for a ChartParameter in ExtraParameters property which keeps a reference to 
        /// additional ChartParameterCollection. This collection is used for chart 
        /// parameter styling purposes.
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>A chart parameter from ExtraParameters</returns>
        private ChartParameter GetExtraParameter(ChartParameterType type)
        {
            if (ExtraParameters != null)
            {
                if (ExtraParameters is ChartParameterCollection)
                {
                    ChartParameterCollection extra = ExtraParameters as ChartParameterCollection;
                    foreach (ChartParameter parameter in extra)
                    {
                        if (parameter.Type == type)
                        {
                            return parameter;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal bool GetParameterValueBool(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return (bool)parameter.GetDefaultBool();
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return (bool)extra.GetDefaultBool();
            }

            // Find parent series
            Series series = GetSeries();
            return (bool)series.GetParameterValueBool(type);
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal bool IsDateTimeType(ChartParameterType type)
        {
            bool isDateTimeType = false;
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    parameter.GetDefaultDouble(out isDateTimeType);
                    return isDateTimeType;
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                extra.GetDefaultDouble(out isDateTimeType);
                return isDateTimeType;
            }

            ChartParameter newAttribute = new ChartParameter();
            newAttribute.Type = type;
            newAttribute.GetDefaultDouble(out isDateTimeType);
            return isDateTimeType;
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal double GetParameterValueDouble(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return (double)parameter.GetDefaultDouble();
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return (double)extra.GetDefaultDouble();
            }

            // Find parent series
            Series series = GetSeries();
            return (double)series.GetParameterValueDouble(type);
        }

        /// <summary>
        /// Check if chart parameter value is set
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>True if parameter is set</returns>
        internal bool IsParameterSet(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return true;
                }
            }

            // Find parent series
            Series series = GetSeries();
            return series.IsParameterSet(type);
        }

        /// <summary>
        /// Returns a double animation from data point or parent series for spline chart types (speciail case).
        /// </summary>
        /// <param name="pointIndex">Data point index from Data point collection</param>
        /// <param name="maxSpline">The size of entire spline segment in X Axis units.</param>
        /// <param name="splineStart">The start of a spline segment related to the current data point.</param>
        /// <param name="splineEnd">The end of a spline segment related to the current data point.</param>
        /// <returns>Double animation</returns>
        internal DoubleAnimation GetDoubleAnimationSpline(int pointIndex, double maxSpline, double splineStart, double splineEnd)
        {
            DoubleAnimation doubleAnimation = null;
            Animation animation = null;

            // Find parent series
            Series series = GetSeries();

            // Data point animation exist
            if (Animation != null)
            {
                animation = Animation;
            }
            else // Animation from series
            {
                if (series.Animation != null)
                {
                    // Sequential animation
                    if (series.Animation.Sequential)
                    {
                        animation = series.Animation;

                        // Calculate begin time and duration for sequential animation.
                        long tickDuration = 0;
                        Duration duration = Duration.Automatic;
                        if (animation.Duration.HasTimeSpan)
                        {
                            tickDuration = (long)(((double)animation.Duration.TimeSpan.Ticks) / maxSpline * (splineEnd - splineStart));
                            duration = new Duration(new TimeSpan(tickDuration));
                        }

                        long beginTime = animation.BeginTime.Ticks + (long)(((double)animation.Duration.TimeSpan.Ticks) / maxSpline * splineStart);

                        // Fill double animation from chart animation
                        doubleAnimation = new DoubleAnimation();
                        doubleAnimation.From = 0;
                        doubleAnimation.To = 1;
                        doubleAnimation.BeginTime = new TimeSpan(beginTime);
                        doubleAnimation.Duration = duration;
                        doubleAnimation.AccelerationRatio = animation.AccelerationRatio;
                        doubleAnimation.DecelerationRatio = animation.DecelerationRatio;
                        doubleAnimation.RepeatBehavior = animation.RepeatBehavior;

                        return doubleAnimation;
                    }
                    else
                    {
                        // Non sequential animation from series.
                        animation = series.Animation;
                    }
                }
            }

            // Fill double animation from chart animation
            if (animation != null)
            {
                doubleAnimation = new DoubleAnimation();
                doubleAnimation.From = 0;
                doubleAnimation.To = 1;
                doubleAnimation.BeginTime = animation.BeginTime;
                doubleAnimation.Duration = animation.Duration;
                doubleAnimation.AccelerationRatio = animation.AccelerationRatio;
                doubleAnimation.DecelerationRatio = animation.DecelerationRatio;
                doubleAnimation.RepeatBehavior = animation.RepeatBehavior;
            }

            return doubleAnimation;
        }

        private object _oldBindingObject;

        /// <summary>
        /// This method set a data point property value from a data source. It converts 
        /// data value type to property type.
        /// </summary>
        /// <param name="propertyName">Data point's property name</param>
        /// <param name="valueName">Objects Value Name</param>
        /// <param name="value">Property value from a data source</param>
        /// <param name="series">Parent series</param>
        internal void SetBindValueObject(string propertyName, string valueName, object value, Series series)
        {
            INotifyPropertyChanged notify = value as INotifyPropertyChanged;
            if (notify != null)
            {
                if (_oldBindingObject != null)
                {
                    INotifyPropertyChanged oldNotify = _oldBindingObject as INotifyPropertyChanged;
                    PropertyChangedEventManager.RemoveListener(oldNotify, this, "");
                }
                PropertyChangedEventManager.AddListener(notify, this, "");

                if (_bindNotifyDict == null)
                {
                    _bindNotifyDict = new Dictionary<string, string>();
                }

                _bindNotifyDict.Add(valueName, propertyName);
            }

            _oldBindingObject = value;
            Binding binding = new Binding();
            binding.Source = value;
            binding.Path = new PropertyPath(valueName);
            DataPoint point = new DataPoint();
            point.SetBinding(DataPoint.ToolTipProperty, binding);
            if (point.ToolTip == null)
            {
                //MaxR - Don't plot the datapoint if it's bound to a null value.
                this.NullValue = true;                
                return;
                // Data Binding Error - Invalid binding property name. Check DataMapping formatting string.
                // throw new InvalidOperationException(ErrorString.Exc57);
            }
            SetBindValue(propertyName, point.ToolTip, series);
        }
        
        /// <summary>
        /// Represents the method that will handle the PropertyChanged event raised when a property is changed on a component.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A PropertyChangedEventArgs that contains the event data.</param>
        void BindPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            XamChart control = XamChart.GetControl(this);
            if (control == null || string.IsNullOrEmpty(e.PropertyName))
            {
                return;
            }
            if (this._bindNotifyDict.ContainsKey(e.PropertyName))
            {
                Binding binding = new Binding();
                binding.Source = sender;
                binding.Path = new PropertyPath(e.PropertyName);
                DataPoint point = new DataPoint();
                point.SetBinding(DataPoint.ToolTipProperty, binding);
                //if (point.ToolTip == null) // [DN 7/16/2010:35931] proceed in silence.  this could be a proper databinding if the property you're binding to is nullable.
                
                string valueName = _bindNotifyDict[e.PropertyName];

                this.SetBindValue(valueName, point.ToolTip, this.GetSeries());
            }
        }

        /// <summary>
        /// This method set a data point property value from a data source. It converts 
        /// data value type to property type.
        /// </summary>
        /// <param name="propertyName">Data point's property name</param>
        /// <param name="value">Property value from a data source</param>
        /// <param name="series">Parent series</param>
        internal void SetBindValue(string propertyName, object value, Series series)
        {
            if (propertyName == null)
            {
                return;
            }

            CultureInfo cultureToUse = CultureInformation.CultureToUse;

            propertyName = propertyName.ToUpperInvariant();
            if (propertyName == "LABEL")
            {
                string stringValue = value as string;
                if (stringValue != null)
                {
                    this.Label = stringValue;
                }
                else if (value is DateTime)
                {
                    if (series != null)
                    {
                        Axis axis = series.GetAxisX();
                        if (axis != null)
                        {
                            Label label = axis.Label;
                            if (label != null)
                            {
                                if (!string.IsNullOrEmpty(label.Format))
                                {
                                    this.Label = string.Format(cultureToUse, label.Format, value);
                                }
                                else
                                {
                                    this.Label = ((DateTime)value).ToString(cultureToUse);
                                }
                            }
                        }
                    }
                }
                else if (value != null)
                {
                    this.Label = value.ToString();
                }
                return;
            }
            else if (propertyName == "VALUEX" && value is DateTime)
            {
                SetChartAttributeObject(ChartParameterType.ValueX, value);
                return;
            }
            else if (propertyName == "TOOLTIP")
            {
                this.ToolTip = value;
                return;
            }

            // Convert data base values
            double dataValue;
            try
            {
                this.NullValue = value is DBNull || value == null; 
                if (value is int)
                {
                    int intValue = (int)value;
                    dataValue = (double)intValue;
                }
                else if (value is string)
                {
                    dataValue = double.Parse((string)value, CultureInfo.InvariantCulture);
                }
                else if (value is short)
                {
                    dataValue = (double)(short)value;
                }
                else if (value is double)
                {
                    dataValue = (double)(value);
                }
                else if (value is decimal)
                {
                    dataValue = (double)(decimal)value;
                }
                else if (value is float)
                {
                    dataValue = (double)(decimal)(float)value; // [DN Nov 19 2011 : 88664] insanity, but it will get rid of the precision mess that happens when you cast to double from float.
                }
                else if (this.NullValue)
                {
                    dataValue = 0.0;
                }
                else
                {
                    object obj = value;
                    string str = obj.ToString();
                    dataValue = double.Parse(str, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                // Invalid Data Type from Data Source.
                throw new InvalidOperationException(ErrorString.Exc32);
            }

            // Set Value or chart parameter
            switch (propertyName)
            {
                case "VALUE":
                    Value = dataValue;
                    break;
                case "VALUEX":
                    SetChartAttribute(ChartParameterType.ValueX, dataValue);
                    break;
                case "VALUEY":
                    SetChartAttribute(ChartParameterType.ValueY, dataValue);
                    break;
                case "VALUEZ":
                    SetChartAttribute(ChartParameterType.ValueZ, dataValue);
                    break;
                case "HIGH":
                    SetChartAttribute(ChartParameterType.High, dataValue);
                    break;
                case "LOW":
                    SetChartAttribute(ChartParameterType.Low, dataValue);
                    break;
                case "OPEN":
                    SetChartAttribute(ChartParameterType.Open, dataValue);
                    break;
                case "CLOSE":
                    SetChartAttribute(ChartParameterType.Close, dataValue);
                    break;
                case "RADIUS":
                    SetChartAttribute(ChartParameterType.Radius, dataValue);
                    break;
                default:
                    // Data Binding Error - Wrong Data Point value.
                    throw new InvalidOperationException(ErrorString.Exc33);
            }
        }

        /// <summary>
        /// This method will remove all chart parameters which have specified chart 
        /// parameter type and add a new parameter with new value.
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <param name="value">Chart parameter value</param>
        private void SetChartAttributeObject(ChartParameterType type, object value)
        {
            // Array copy
            ArrayList attributeList = new ArrayList();
            foreach (ChartParameter parameter in ChartParameters)
            {
                attributeList.Add(parameter);
            }

            // Remove all items which have this chart parameter type
            for (int attributeIndx = 0; attributeIndx < attributeList.Count; attributeIndx++)
            {
                if (((ChartParameter)attributeList[attributeIndx]).Type == type)
                {
                    ChartParameters.Remove((ChartParameter)attributeList[attributeIndx]);
                }
            }

            // Set a new value for chart attibute type.
            ChartParameters.Add(type, value);
        }

        /// <summary>
        /// This method will remove all chart parameters which have specified chart 
        /// parameter type and add a new parameter with new value.
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <param name="value">Chart parameter value</param>
        private void SetChartAttribute(ChartParameterType type, double value)
        {
            // Array copy
            ArrayList attributeList = new ArrayList();
            foreach (ChartParameter parameter in ChartParameters)
            {
                attributeList.Add(parameter);
            }

            // Remove all items which have this chart parameter type
            for (int attributeIndx = 0; attributeIndx < attributeList.Count; attributeIndx++)
            {
                if (((ChartParameter)attributeList[attributeIndx]).Type == type)
                {
                    ChartParameters.Remove((ChartParameter)attributeList[attributeIndx]);
                }
            }

            // Set a new value for chart attibute type.
            ChartParameters.Add(type, value);
        }

        /// <summary>
        /// Returns a double animation from data point or parent series.
        /// </summary>
        /// <param name="pointIndex">Data point index from Data point collection</param>
        /// <returns>Animation values</returns>
        internal DoubleAnimation GetDoubleAnimation(int pointIndex)
        {
            DoubleAnimation doubleAnimation = null;
            Animation animation = null;

            // Find parent series
            Series series = GetSeries();

            // Get number of points
            int numOfPoints = series.DataPoints.Count;

            // Data point animation exist
            if (Animation != null)
            {
                animation = Animation;
            }
            else // Animation from series
            {
                if (series.Animation != null)
                {
                    // Sequential animation
                    if (series.Animation.Sequential)
                    {
                        animation = series.Animation;

                        // Calculate begin time and duration for sequential animation.
                        long tickDuration = 0;
                        Duration duration = Duration.Automatic;
                        if (animation.Duration.HasTimeSpan)
                        {
                            tickDuration = animation.Duration.TimeSpan.Ticks / numOfPoints;
                            duration = new Duration(new TimeSpan(tickDuration));
                        }

                        long beginTime = animation.BeginTime.Ticks + tickDuration * pointIndex;

                        // Fill double animation from chart animation
                        doubleAnimation = new DoubleAnimation();
                        doubleAnimation.From = 0;
                        doubleAnimation.To = 1;
                        doubleAnimation.BeginTime = new TimeSpan(beginTime);
                        doubleAnimation.Duration = duration;
                        doubleAnimation.AccelerationRatio = animation.AccelerationRatio;
                        doubleAnimation.DecelerationRatio = animation.DecelerationRatio;
                        doubleAnimation.RepeatBehavior = animation.RepeatBehavior;

                        return doubleAnimation;
                    }
                    else
                    {
                        // Non sequential animation from series.
                        animation = series.Animation;
                    }
                }
            }

            // Fill double animation from chart animation
            if (animation != null)
            {
                doubleAnimation = new DoubleAnimation();
                doubleAnimation.From = 0;
                doubleAnimation.To = 1;
                doubleAnimation.BeginTime = animation.BeginTime;
                doubleAnimation.Duration = animation.Duration;
                doubleAnimation.AccelerationRatio = animation.AccelerationRatio;
                doubleAnimation.DecelerationRatio = animation.DecelerationRatio;
                doubleAnimation.RepeatBehavior = animation.RepeatBehavior;
            }

            return doubleAnimation;
        }

        /// <summary>
        /// Returns a point animation from data point or parent series.
        /// </summary>
        /// <param name="pointIndex">Data point index from Data point collection</param>
        /// <returns>Animation values</returns>
        internal PointAnimation GetPointAnimation(int pointIndex)
        {
            PointAnimation pointAnimation = null;
            DoubleAnimation doubleAnimation = GetDoubleAnimation(pointIndex);

            // Fill point animation from double animation
            if (doubleAnimation != null)
            {
                pointAnimation = new PointAnimation();
                pointAnimation.BeginTime = doubleAnimation.BeginTime;
                pointAnimation.Duration = doubleAnimation.Duration;
                pointAnimation.AccelerationRatio = doubleAnimation.AccelerationRatio;
                pointAnimation.DecelerationRatio = doubleAnimation.DecelerationRatio;
                pointAnimation.RepeatBehavior = doubleAnimation.RepeatBehavior;
            }
            
            return pointAnimation;
        }


        /// <summary>
        /// Updates the actual stroke.
        /// </summary>
        /// <param name="seriesIndex">Index of the series.</param>
        /// <param name="pointIndex">Index of the point.</param>
        internal void UpdateActualStroke(int seriesIndex, int pointIndex)
        {
            this.ActualStroke = GetStroke(seriesIndex, pointIndex);

            if (this.DataPointTemplate != null)
            {
                this.DataPointTemplate.Stroke = this.ActualStroke;
            }
        }

        /// <summary>
        /// Updates the actual fill.
        /// </summary>
        /// <param name="seriesIndex">Index of the series.</param>
        /// <param name="pointIndex">Index of the point.</param>
        internal void UpdateActualFill(int seriesIndex, int pointIndex)
        {
            this.ActualFill = GetFill(seriesIndex, pointIndex);

            if (this.DataPointTemplate != null)
            {
                this.DataPointTemplate.Fill = this.ActualFill;
            }
        }

        private static void OnFillPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataPoint dataPoint = d as DataPoint;

            Series series = dataPoint.GetSeries();

            XamChart control = XamChart.GetControl(d);

            if (series == null || control == null)
            {
                return;
            }
            
            // if it is performance rendering - recreate the whole chart
            bool isPerformance = control.IsPerformance();

            if (isPerformance)
            {
                OnPropertyChanged(d, e);

                return;
            }

            if (control.Legend != null && control.Legend.LegendPane != null)
            {
                control.Legend.LegendPane.Draw();
            }

            int seriesIndex = control.Series.IndexOf(series);
            int pointIndex = series.DataPoints.IndexOf(dataPoint);

            dataPoint.UpdateActualFill(seriesIndex, pointIndex);
        }

        private static void OnStrokePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataPoint dataPoint = d as DataPoint;

            Series series = dataPoint.GetSeries();

            XamChart control = XamChart.GetControl(d);

            if (series == null || control == null)
            {
                return;
            }
            
            // if it is performance rendering - recreate the whole chart
            bool isPerformance = control.IsPerformance();

            if (isPerformance)
            {
                OnPropertyChanged(d, e);

                return;
            }

            if (control.Legend != null && control.Legend.LegendPane != null)
            {
                control.Legend.LegendPane.Draw();
            }

            int seriesIndex = control.Series.IndexOf(series);
            int pointIndex = series.DataPoints.IndexOf(dataPoint);

            dataPoint.UpdateActualStroke(seriesIndex, pointIndex);
        }


        #endregion Methods

        #region Public Properties

        #region ChartParameters

        private ChartParameterCollection _chartParameters = new ChartParameterCollection();

        /// <summary>
        /// Gets or sets chart parameters for this data point.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Chart parameters are used to decrese the number of public 
        /// properties used in series and data points.  Used for numerous 
        /// number of parameters which are different for every chart type.
        /// </p>
        /// <p class="body">
        /// If chart parameter is specified for a series it will apply on every 
        /// data point from the series. If a chart parameter is set for a data point, 
        /// the chart parameter from the series will be ignored.
        /// </p>
        /// </remarks>
        //[Description("Gets or sets chart parameters for this data point.")]
        //[Category("Data")]
        public ChartParameterCollection ChartParameters
        {
            get
            {
                if (_chartParameters.ChartParent == null)
                {
                    _chartParameters.ChartParent = this;
                }
                return _chartParameters;
            }
        }

        #endregion ChartParameters

        #region Value

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
            typeof(double), typeof(DataPoint), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the value of a data point. The value property is not used for all chart types. 
        /// Scatter,Stock, Candlestick and Bubble chart types use ChartParameters: ValueX, ValueY, ValueZ, 
        /// Radius, High, Low, Open, Close. 
        /// </summary>
        /// <seealso cref="ValueProperty"/>
        //[Description("Gets or sets the value of a data point. The value property is not used for all chart types. Scatter,Stock, Candlestick and Bubble chart types use ChartParameters: ValueX, ValueY, ValueZ, Radius, High, Low, Open, Close.")]
        //[Category("Data")]
        public double Value
        {
            get
            {
                return (double)this.GetValue(DataPoint.ValueProperty);
            }
            set
            {
                this.SetValue(DataPoint.ValueProperty, value);
            }
        }

        #endregion Value

        #region Label

        /// <summary>
        /// Identifies the <see cref="Label"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label",
            typeof(string), typeof(DataPoint), new FrameworkPropertyMetadata(String.Empty, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets data point label. Data point label stores text values for axis labels.
        /// </summary>
        /// <remarks>
        /// <p class="body">To change appearance of the axis labels, font properties from 
        /// the Labels (See Labels property from Axis) has to be used. The label property of 
        /// a DataPoint is used to store text value only. Some chart types like pie or 
        /// doughnut use data point labels for legend items.</p>
        /// <p class="body">Important! If a chart has multiple series only Data Points 
        /// from the first series are used to fill axis labels. All other series are ignored.</p>
        /// </remarks>
        /// <seealso cref="LabelProperty"/>
        //[Description("Gets or sets data point label. Data point label stores text values for axis labels.")]
        //[Category("Data")]
        public string Label
        {
            get
            {
                return (string)this.GetValue(DataPoint.LabelProperty);
            }
            set
            {
                this.SetValue(DataPoint.LabelProperty, value);
            }
        }

        #endregion Label

        #region ExtraParameters

        /// <summary>
        /// Identifies the <see cref="ExtraParameters"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ExtraParametersProperty = DependencyProperty.Register("ExtraParameters",
            typeof(object), typeof(DataPoint), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));


        /// <summary>
        /// Gets or sets a reference to additional ChartParameterCollection. This collection is used for chart 
        /// parameter styling purposes.
        /// </summary>
        /// <seealso cref="ExtraParametersProperty"/>
        //[Description("Gets or sets a reference to additional ChartParameterCollection. This collection is used for chart parameter styling purposes.")]
        public object ExtraParameters
        {
            get
            {
                return (object)this.GetValue(DataPoint.ExtraParametersProperty);
            }
            set
            {
                this.SetValue(DataPoint.ExtraParametersProperty, value);
            }
        }

        #endregion ExtraParameters

        #region Fill

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill",
            typeof(Brush), typeof(DataPoint), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFillPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the interior of the shape. 
        /// </summary>
        /// <seealso cref="FillProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the interior of the shape. ")]
        //[Category("Brushes")]
        public Brush Fill
        {
            get
            {
                return (Brush)this.GetValue(DataPoint.FillProperty);
            }
            set
            {
                this.SetValue(DataPoint.FillProperty, value);
            }
        }

        #endregion Fill

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke",
            typeof(Brush), typeof(DataPoint), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnStrokePropertyChanged)));

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
                return (Brush)this.GetValue(DataPoint.StrokeProperty);
            }
            set
            {
                this.SetValue(DataPoint.StrokeProperty, value);
            }
        }

        #endregion Stroke

        #region IsMouseOver

        private static readonly DependencyPropertyKey IsMouseOverPropertyKey =
            DependencyProperty.RegisterReadOnly("IsMouseOver",
            typeof(bool), typeof(DataPoint), new FrameworkPropertyMetadata((bool)false));

        /// <summary>
        /// Identifies the <see cref="IsMouseOver"/> dependency property
        /// </summary>
        public new static readonly DependencyProperty IsMouseOverProperty =
            IsMouseOverPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a value that indicates whether the mouse pointer is located over this element. 
        /// </summary>
        /// <seealso cref="IsMouseOverProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(true)]
        [ReadOnly(true)]
        public new bool IsMouseOver
        {
            get
            {
                return (bool)this.GetValue(DataPoint.IsMouseOverProperty);
            }
        }

        #endregion IsMouseOver

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness",
            typeof(double), typeof(DataPoint), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)));

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
                return (double)this.GetValue(DataPoint.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(DataPoint.StrokeThicknessProperty, value);
            }
        }

        #endregion StrokeThickness	

        #region Animation

        /// <summary>
        /// Identifies the <see cref="Animation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register("Animation",
            typeof(Animation), typeof(DataPoint), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the animation for Data Point. This animation is different for every chart type.
        /// </summary>
        /// <remarks>
        /// This animation is used only when one data point has to be animated asynchronously from another 
        /// data points. The most common use of animation is from series, when data points are animated together. 
        /// This animation is only used to create growing effect, but data point animation could be also created 
        /// using brush property and WPF animation.
        /// </remarks>
        /// <seealso cref="AnimationProperty"/>
        //[Description("Gets or sets the animation for Data Point. This animation is different for every chart type.")]
        //[Category("Appearance")]
        public Animation Animation
        {
            get
            {
                Animation obj = (Animation)this.GetValue(DataPoint.AnimationProperty);
                if (obj != null)
                {
                    obj.ChartParent = this;
                }

                return obj;
            }
            set
            {
                this.SetValue(DataPoint.AnimationProperty, value);
            }
        }

        #endregion Animation

        #region Marker

        /// <summary>
        /// Identifies the <see cref="Marker"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarkerProperty = DependencyProperty.Register("Marker",
            typeof(Marker), typeof(DataPoint), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the marker. Marker is a colored shape which shows exact value of 
        /// a Data Point. Marker has corresponding marker label. Used in combination with 
        /// different chart types. 
        /// </summary>
        /// <remarks>
        /// <p class="body">Markers can be defined for series or data points. If Marker is 
        /// not defined for DataPoint, the marker from parent series is used.</p>
        /// <p class="body">Some chart types don�t use marker shapes or marker labels. 
        /// Chart types without Axis don�t have marker shapes (pie or doughnut charts). 
        /// 3D Charts don�t have marker shapes, they have marker labels only.</p>
        /// </remarks>
        /// <seealso cref="MarkerProperty"/>
        //[Description("Gets or sets the marker. Marker is a colored shape which shows exact value of a Data Point. Marker has corresponding marker label. Used in combination with different chart types.")]
        //[Category("Data")]
        public Marker Marker
        {
            get
            {
                Marker obj = (Marker)this.GetValue(DataPoint.MarkerProperty);
                if (obj != null)
                {
                    obj.ChartParent = this;
                }

                return obj;
            }
            set
            {
                this.SetValue(DataPoint.MarkerProperty, value);
            }
        }

        #endregion Marker

        #region ToolTip

        /// <summary>
        /// Gets or sets the tool-tip object that is displayed for <see cref="Infragistics.Windows.Chart.DataPoint"/>. 
        /// </summary>
        //[Description("Gets or sets the tool-tip object that is displayed for DataPoint")]
        //[Category("Data")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new Object ToolTip
        {
            get
            {
                return base.ToolTip;
            }
            set
            {
                base.ToolTip = value;
            }
        }

        #endregion ToolTip

        #region NullValue

        /// <summary>
        /// Identifies the <see cref="NullValue"/> dependency property
        /// </summary>
        public static readonly DependencyProperty NullValueProperty = DependencyProperty.Register("NullValue",
            typeof(bool), typeof(DataPoint), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the Data Point has Null value.
        /// </summary>
        /// <remarks>
        /// By default NullValue is set to false. If NullValue is set to true data point is not 
        /// drawn. After data binding every value from the data source which is DBNull will be 
        /// set to 0 and NullValue will be set to true. 
        /// </remarks>
        /// <seealso cref="NullValueProperty"/>
        //[Description("Gets or sets a value that indicates whether the Data Point has Null value.")]
        //[Category("Data")]
        public bool NullValue
        {
            get
            {
                return (bool)this.GetValue(DataPoint.NullValueProperty);
            }
            set
            {
                this.SetValue(DataPoint.NullValueProperty, value);
            }
        }

        #endregion NullValue


        #region ActualFill

        internal const string ActualFillPropertyName = "ActualFill";

        /// <summary>
        /// Gets or sets the actual Brush that specifies how to paint the interior of the shape. .
        /// </summary>
        /// <value>The actual fill.</value>
        public Brush ActualFill
        {
            get { return (Brush)GetValue(ActualFillProperty); }
            internal set { SetValue(ActualFillProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ActualFill"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualFillProperty =
            DependencyProperty.Register(ActualFillPropertyName, typeof(Brush), typeof(DataPoint),
              new PropertyMetadata(null));

        #endregion

        #region ActualStroke

        internal const string ActualStrokePropertyName = "ActualStroke";

        /// <summary>
        /// Gets or sets the actual Brush that specifies how to paint the Shape outline.
        /// </summary>
        /// <value>The actual stroke.</value>
        public Brush ActualStroke
        {
            get { return (Brush)GetValue(ActualStrokeProperty); }
            internal set { SetValue(ActualStrokeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ActualStroke"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualStrokeProperty =
            DependencyProperty.Register(ActualStrokePropertyName, typeof(Brush), typeof(DataPoint),
              new PropertyMetadata(null));

        #endregion


        #endregion Public Properties

        internal DataPointTemplate DataPointTemplate { get; set; }
       
        #region IWeakEventListener Members

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            this.BindPropertyChanged(sender, e as PropertyChangedEventArgs);
            return true;
        }

        #endregion
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