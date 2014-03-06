
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class AxisValue
    {
        #region Fields

        // Private Fields
        private double _maximum;
        private double _minimum;
        private double _crossing;
        private bool _autoMinimum = true;
        private bool _autoMaximum = true;
        private bool _autoInterval = true;
        private double _interval;
        private double _intervalNumber = 3;
        private double _sizeInPixels;
        private double _roundedMinimum;
        private double _roundedMaximum;
        private double _roundedInterval;
        private bool _pixelOrientationNegative;
        private bool _zeroIncluded = true;
        private bool _indexed;
        private bool _indexedSeries = false;
        private double _logarithmicBase = 10;
        private bool _logarithmic;
        private bool _is3D = false;
        private Chart _chart;
        private AxisType _axisType;
        private double _scene3DSize = 1;
        private bool _labelsExist = false;
        private bool _isDateTime = false;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the value which indicates that the double value is date type.
        /// </summary>
        internal bool IsDateTime
        {
            get
            {
                return _isDateTime;
            }
            set
            {
                _isDateTime = value;
            }
        }

        /// <summary>
        /// Gets the maximum number of labels for category axes before Unit 
        /// value is changed from value 1.
        /// </summary>
        static internal int MaxNumberOfLabels
        {
            get
            {
                return 30;
            }
        }

        /// <summary>
        /// The depth of the 3D scene
        /// </summary>
        internal double Scene3DSize
        {
            get
            {
                return _scene3DSize;
            }
            set
            {
                _scene3DSize = value;
            }
        }

        internal bool LabelsExist
        {
            get { return _labelsExist; }
            set { _labelsExist = value; }
        }

        internal Chart Chart
        {
            get { return _chart; }
            set { _chart = value; }
        }

        internal AxisType AxisType
        {
            get { return _axisType; }
            set { _axisType = value; }
        }

        internal bool Logarithmic
        {
            get { return _logarithmic; }
        }

        internal double LogarithmicBase
        {
            get { return _logarithmicBase; }
        }

        internal bool PixelOrientationNegative
        {
            get { return _pixelOrientationNegative; }
            set { _pixelOrientationNegative = value; }
        }

        internal bool ZeroIncluded
        {
            get { return _zeroIncluded; }
            set { _zeroIncluded = value; }
        }

        internal bool Is3D
        {
            get { return _is3D; }
            set { _is3D = value; }
        }

        internal bool Indexed
        {
            get { return _indexed; }
            set { _indexed = value; }
        }

        internal bool IndexedSeries
        {
            get { return _indexedSeries; }
            set { _indexedSeries = value; }
        }

        internal double RoundedInterval
        {
            get { return _roundedInterval; }
        }

        internal double Crossing
        {
            get
            {
                {
                    return _crossing;
                }
            }
        }


        internal double RoundedMaximum
        {
            get { return _roundedMaximum; }
        }


        internal double RoundedMinimum
        {
            get { return _roundedMinimum; }
        }


        internal double SizeInPixels
        {
            get { return _sizeInPixels; }
            set { _sizeInPixels = value; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// This method checks if label properties are set for data points. Otherwise 
        /// data point indexes are used for labels. Used for category axes only.
        /// </summary>
        private void LabelsFromPointExist()
        {
            if (_chart.GetContainer().Series.Count > 0)
            {
                Series series = _chart.GetContainer().Series[0];
                foreach (DataPoint point in series.DataPoints)
                {
                    if (!string.IsNullOrEmpty(point.Label))
                    {
                        _labelsExist = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// This method sets range for axes. It will set calculate minimum, 
        /// maximum and/or unit (interval) if auto range is used. Calculation 
        /// is based on data point values.
        /// </summary>
        internal void Calculate()
        {
            double min, max;

            LabelsFromPointExist();

            // Set Date time scatter axis.
            if ((_axisType == AxisType.PrimaryX || _axisType == AxisType.SecondaryX) && _chart.ChartCreator.IsSceneType(SceneType.Scatter))
            {
                IsDateTime = GetPointsDateTime();
            }

            foreach (Axis axis in _chart.GetContainer().Axes)
            {
                if (axis.AxisType == _axisType)
                {
                    // Set crossing value
                    _crossing = axis.Crossing;

                    if (_crossing != 0 && axis.Logarithmic)
                    {
                        // Crossing cannot be used on logarithmic scale.
                        throw new InvalidOperationException(ErrorString.Exc64);
                    }

                    if (!axis.AutoRange)
                    {
                        if (double.IsNaN(axis.Maximum) || double.IsNaN(axis.Minimum) || double.IsNaN(axis.Unit) || axis.Unit <= 0)
                        {
                            // If the AutoRange is set to false, Minimum and Maximum have to be set and Unit value must be greater than zero.
                            throw new InvalidOperationException(ErrorString.Exc62);
                        }
                    }

                    if (axis.AutoRange)
                    {
                        _autoMaximum = true;
                        _autoMinimum = true;

                        if (axis.Unit != 0)
                        {
                            _autoInterval = false;
                            _interval = axis.Unit;
                            _roundedInterval = axis.Unit;
                        }

                    }
                    else if (_indexedSeries)
                    {
                        _autoMaximum = true;
                        _autoMinimum = true;
                        _autoInterval = false;
                        _interval = axis.Unit;
                        _roundedInterval = Math.Floor(axis.Unit);
                        if (_roundedInterval < 1)
                        {
                            _roundedInterval = 1;
                        }
                    }
                    else
                    {
                        _autoMaximum = false;
                        _autoMinimum = false;

                        if (_indexed)
                        {
                            _maximum = Math.Floor(axis.Maximum);
                            _minimum = Math.Floor(axis.Minimum);
                        }
                        else if (axis.Logarithmic)
                        {
                            _logarithmic = true;
                            _logarithmicBase = axis.LogarithmicBase;
                            _maximum = Math.Log(axis.Maximum, _logarithmicBase);
                            _minimum = Math.Log(axis.Minimum, _logarithmicBase);

                            if (double.IsNaN(_minimum) || double.IsNaN(_maximum) || double.IsNegativeInfinity(_minimum) || double.IsNegativeInfinity(_maximum))
                            {
                                // Logarithmic axes cannot show negative or zero values.
                                throw new InvalidOperationException(ErrorString.Exc9);
                            }
                        }
                        else
                        {
                            _maximum = axis.Maximum;
                            _minimum = axis.Minimum;
                        }

                        if (_minimum == _maximum)
                        {
                            _maximum = _minimum + 1;
                        }

                        _roundedMinimum = _minimum;
                        _roundedMaximum = _maximum;

                        if (axis.Unit == 0)
                        {
                            _autoInterval = true;
                            _roundedInterval = (_maximum - _minimum) / 5.0;
                        }
                        else if (_indexed)
                        {
                            _autoInterval = false;
                            _interval = Math.Floor(axis.Unit);
                            _roundedInterval = _interval;
                        }
                        {
                            _autoInterval = false;
                            _interval = axis.Unit;
                            _roundedInterval = _interval;
                        }

                        if (_roundedMinimum > _roundedMaximum)
                        {
                            // Invalid minimum and maximum values. Minimum cannot be greater than the maximum.
                            throw new InvalidOperationException(ErrorString.Exc7);
                        }

                        if ((_roundedMaximum - _roundedMinimum) / _roundedInterval > 100 || double.IsNaN((_roundedMaximum - _roundedMinimum) / _roundedInterval))
                        {
                            // The Unit value for this axis is too small for specified Minimum and Maximum values. Increase the Unit value.
                            throw new InvalidOperationException(ErrorString.Exc8);
                        }

                        // Crossing value has to be between minimum and maximum
                        CrossingCorrection();

                        return;
                    }

                    if (axis.Logarithmic)
                    {
                        _logarithmic = true;
                        _logarithmicBase = axis.LogarithmicBase;
                    }
                }
            }

            // Calculates minimum and maximum from data point values.
            ValuesFromSeries(out min, out max);

            if (min == double.MaxValue || max == double.MinValue)
            {
                min = 0;
                max = 0.8;
            }

            // Minimum, Maximum and interval for 100% stacked chart
            if (_chart.ChartCreator.IsSceneType(SceneType.Stacked100) && !_indexed && !_indexedSeries)
            {
                _roundedMinimum = min;
                _roundedMaximum = max;
                _roundedInterval = (max - min) / 5.0;
                return;
            }

            CalculateRounding(ref min, ref max, 0.1);

            //check the interval is correct compared to max and min
            IntervalCorrection(ref min, ref max);

            // Crossing value has to be between minimum and maximum
            CrossingCorrection();

            // Interval validation
            if (Math.Abs(_roundedMaximum - _roundedMinimum) / _roundedInterval > 200)
            {
                // Axis Unit value too small.
                throw new InvalidOperationException(ErrorString.Exc66);
            }
        }

        /// <summary>
        /// if min or max exactly rounds to the rounded interval, we
        /// may end up with too few tick marks, not leaving enough buffer area
        /// on the chart, so in the case where this happens, reround the min and
        /// max with a slightly larger margin ratio.
        /// </summary>
        private void IntervalCorrection(ref double min, ref double max)
        {
            if ((_autoMinimum &&
                    (Math.Abs(_roundedMinimum) == _roundedInterval) &&
                    (_roundedMinimum != min)) ||
                (_autoMaximum &&
                    (Math.Abs(_roundedMaximum) == _roundedInterval) &&
                    (_roundedMaximum != max)))
            {
                CalculateRounding(ref min, ref max, 0.11);
            }
        }

        /// <summary>
        /// This method rounds minimum and maximum values to 
        /// the most significant digits.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="marginRatio">Ratio for the margin</param>
        private void CalculateRounding(ref double min, ref double max, double marginRatio)
        {
            // Add margin
            double margin = Math.Abs(max - min) * marginRatio;

            if (_logarithmic)
            {
                SetRoundValues(min, max);
            }
            else if (this.Indexed || this.IndexedSeries)
            {
                if (min >= 0 && min - margin < 0 && !_logarithmic)
                {
                    double newMargin = min;
                    min = 0;
                    max += newMargin;
                    _roundedMinimum = min;
                    _roundedMaximum = max;
                    if (_autoInterval)
                    {
                        _roundedInterval = FindRoundedInterval(min, max);
                    }
                }
                else
                {
                    min -= margin;
                    max += margin;

                    SetRoundValues(min, max);
                }
            }
            else
            {
                if (min >= 0 && min - margin < 0)
                {
                    min = 0;
                    max += margin;
                    SetRoundValues(min, max);
                }
                else if (max <= 0 && max + margin > 0)
                {
                    max = 0;
                    min -= margin;
                    SetRoundValues(min, max);
                }
                else
                {
                    min -= margin;
                    max += margin;

                    SetRoundValues(min, max);
                }
            }
        }

        /// <summary>
        /// This method is used to set the rounded values for minimum and maximum for 
        /// logarithmic and no logarithmic axes.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum Value</param>
        private void SetRoundValues(double min, double max)
        {
            if (_logarithmic)
            {
                CalculateLogharithmic(min, max);
            }
            else
            {
                _roundedMinimum = FindRoundedMinimum(min, max);
                _roundedMaximum = FindRoundedMaximum(min, max);
                if (_autoInterval)
                {
                    _roundedInterval = FindRoundedInterval(min, max);
                }
            }
        }

        /// <summary>
        /// Crossing value has to be between minimum and maximum
        /// </summary>
        private void CrossingCorrection()
        {
            // Less than maximum
            if (_crossing > _roundedMaximum)
            {
                if (_logarithmic)
                {
                    _crossing = Math.Pow(_logarithmicBase, _roundedMaximum);
                }
                else
                {
                    _crossing = _roundedMaximum;
                }
            }

            // Greater then minimum
            if (_crossing < _roundedMinimum)
            {
                if (_logarithmic)
                {
                    _crossing = Math.Pow(_logarithmicBase, _roundedMinimum);
                }
                else
                {
                    _crossing = _roundedMinimum;
                }
            }
        }

        private void CalculateLogharithmic(double min, double max)
        {
            if (_indexed || _indexedSeries)
            {
                // The category X and Z axes cannot have Logarithmic scale.
                throw new InvalidOperationException(ErrorString.Exc59);
            }

            min = Math.Log(min, _logarithmicBase);
            max = Math.Log(max, _logarithmicBase);

            if (double.IsNaN(min) || double.IsNaN(max) || double.IsNegativeInfinity(min) || double.IsNegativeInfinity(max))
            {
                // Logarithmic axes cannot show negative or zero values.
                throw new InvalidOperationException(ErrorString.Exc9);
            }

            _roundedMinimum = FindRoundedMinimum(min, max);
            _roundedMaximum = FindRoundedMaximum(min, max);
            if (_autoInterval)
            {
                _roundedInterval = FindRoundedInterval(min, max);
            }
        }

        private bool GetPointsDateTime()
        {
            SeriesCollection seriesCollection = _chart.GetContainer().Series;

            foreach (Series series in seriesCollection)
            {
                foreach (DataPoint point in series.DataPoints)
                {
                    if (point.IsDateTimeType(ChartParameterType.ValueX))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Find minimum and maximum values from data series
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        internal void ValuesFromSeries(out double min, out double max)
        {
            min = 0;
            max = 0;

            // Minimum and maximum values for indexed axes (indexed X axis)
            if (_indexed)
            {
                SeriesCollection seriesCollection = _chart.GetContainer().Series;

                max = 0;
                foreach (Series series in seriesCollection)
                {
                    if (max < series.DataPoints.Count)
                    {
                        max = series.DataPoints.Count;
                    }
                }

                max += 1;
                min = 0;
            }
            else if (_indexedSeries) // Minimum and maximum values for indexed Z axes
            {
                max = GetNumberOfZRows(_chart.GetContainer().Series);
                max = Math.Max(max, 1);
                min = 0;
            }
            else // Minimum and maximum values for value axes (Y axis)
            {
                FindMinMax(out min, out max);

                if (_zeroIncluded && !_logarithmic)
                {
                    if (max < 0)
                    {
                        max = 0;
                    }

                    if (min > 0)
                    {
                        min = 0;
                    }
                }

                if (min == 0 && min >= max)
                {
                    max = min + 1;
                }
                else if (min >= max)
                {
                    if (min > 0)
                    {
                        max = min + min * 0.1;
                    }
                    else
                    {
                        max = min - min * 0.1;
                    }
                }
            }
        }

        /// <summary>
        /// Find minimum and maximum values from series.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        private void FindMinMax(out double min, out double max)
        {
            min = double.MaxValue;
            max = double.MinValue;

            SeriesCollection seriesCollection = _chart.GetContainer().Series;

            // Find minimum and maximum from scatter chart types
            if (_chart.ChartCreator.IsSceneType(SceneType.Scatter))
            {
                FindScatterMinMax(ref min, ref max);
                return;
            }

            // Find minimum and maximum from 100% stacked chart types
            if (_chart.ChartCreator.IsSceneType(SceneType.Stacked100))
            {
                FindStacked100MinMax(ref min, ref max);
                return;
            }

            // Find minimum and maximum from stacked chart types
            ArrayList stackedTypes = ChartSeries.GetStackedChartTypes();

            foreach (ChartType type in stackedTypes)
            {
                FindStackedMinMax(ref min, ref max, type);
            }

            // Stock charts min and max
            FindStockMinMax(ref min, ref max);

            // Series loop
            foreach (Series series in seriesCollection)
            {
                if (CheckSeriesAxis(series) && !ChartSeries.GetChartAttribute(ChartTypeAttribute.Stock, series) && !ChartSeries.GetChartAttribute(ChartTypeAttribute.Bubble, series) && !ChartSeries.GetChartAttribute(ChartTypeAttribute.Scatter, series))
                {
                    // Data point loop
                    foreach (DataPoint point in series.DataPoints)
                    {
                        // skip null values
                        if (point.NullValue || double.IsNaN(point.Value) || double.IsInfinity(point.Value))
                        {
                            continue;
                        }

                        if (min > point.Value)
                        {
                            min = point.Value;
                        }

                        if (max < point.Value)
                        {
                            max = point.Value;
                        }
                    }
                }
            }
        }

        internal bool CheckSeriesAxis(Series series)
        {
            Axis axis = _chart.GetContainer().Axes.GetAxis(AxisType);

            if (axis == null)
            {
                return false;
            }

            if (AxisType == AxisType.PrimaryX)
            {
                if (string.IsNullOrEmpty(series.AxisX) || string.Compare(series.AxisX, axis.Name) == 0)
                {
                    return true;
                }
            }
            else if (AxisType == AxisType.SecondaryX)
            {
                if (string.Compare(series.AxisX, axis.Name) == 0)
                {
                    return true;
                }
            }
            else if (AxisType == AxisType.PrimaryY)
            {
                if (string.IsNullOrEmpty(series.AxisY) || string.Compare(series.AxisY, axis.Name) == 0)
                {
                    return true;
                }
            }
            else if (AxisType == AxisType.SecondaryY)
            {
                if (string.Compare(series.AxisY, axis.Name) == 0)
                {
                    return true;
                }
            }
            else if (AxisType == AxisType.PrimaryZ)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds minimum and maximum values from series using 
        /// a scatter chart type.
        /// </summary>
        /// <param name="min">Current Minimum value</param>
        /// <param name="max">Current Maximum value</param>
        private void FindScatterMinMax(ref double min, ref double max)
        {
            // Find minimum and maximum from scatter chart types
            if (_chart.ChartCreator.IsSceneType(SceneType.Bubble))
            {
                FindBubbleMinMax(ref min, ref max);
                return;
            }

            double minimum = double.MaxValue;
            double maximum = double.MinValue;

            SeriesCollection seriesCollection = _chart.GetContainer().Series;

            // Find chart parameter type (ValueX, ValueY or ValueZ) from axis type.
            ChartParameterType attributeType;
            if (_axisType == AxisType.PrimaryX)
            {
                attributeType = ChartParameterType.ValueX;
            }
            else if (_axisType == AxisType.PrimaryY)
            {
                attributeType = ChartParameterType.ValueY;
            }
            else if (_axisType == AxisType.PrimaryZ)
            {
                attributeType = ChartParameterType.ValueZ;
            }
            else if (_axisType == AxisType.SecondaryX)
            {
                attributeType = ChartParameterType.ValueX;
            }
            else if (_axisType == AxisType.SecondaryY)
            {
                attributeType = ChartParameterType.ValueY;
            }
            else
            {
                // Axis Value � Axis type not defined.
                throw new InvalidOperationException(ErrorString.Exc10);
            }

            // Series loop
            foreach (Series series in seriesCollection)
            {
                if (CheckSeriesAxis(series))
                {
                    // Data point loop
                    foreach (DataPoint point in series.DataPoints)
                    {
                        // skip null values
                        if (point.NullValue)
                        {
                            continue;
                        }

                        // Find scatter minimum and maximum values
                        if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Scatter, series))
                        {
                            if (point.GetParameterValueDouble(attributeType) < minimum)
                            {
                                minimum = point.GetParameterValueDouble(attributeType);
                            }

                            if (point.GetParameterValueDouble(attributeType) > maximum)
                            {
                                maximum = point.GetParameterValueDouble(attributeType);
                            }
                        }
                    }
                }
            }

            if (minimum == double.MaxValue || maximum == double.MinValue)
            {
                minimum = 0;
                maximum = 1;
            }

            min = minimum;
            max = maximum;
        }


        /// <summary>
        /// Finds minimum and maximum values from series using 
        /// a bubble chart type.
        /// </summary>
        /// <param name="min">Current Minimum value</param>
        /// <param name="max">Current Maximum value</param>
        private void FindBubbleMinMax(ref double min, ref double max)
        {
            double minimum = double.MaxValue;
            double maximum = double.MinValue;

            SeriesCollection seriesCollection = _chart.GetContainer().Series;

            // Find chart parameter type (ValueX, ValueY or ValueZ) from axis type.
            ChartParameterType attributeType;
            if (_axisType == AxisType.PrimaryX)
            {
                attributeType = ChartParameterType.ValueX;
            }
            else if (_axisType == AxisType.PrimaryY)
            {
                attributeType = ChartParameterType.ValueY;
            }
            else if (_axisType == AxisType.PrimaryZ)
            {
                attributeType = ChartParameterType.ValueZ;
            }
            else if (_axisType == AxisType.SecondaryX)
            {
                attributeType = ChartParameterType.ValueX;
            }
            else if (_axisType == AxisType.SecondaryY)
            {
                attributeType = ChartParameterType.ValueY;
            }
            else
            {
                // Axis Value � Axis type not defined.
                throw new InvalidOperationException(ErrorString.Exc10);
            }

            double maxRadius = double.MinValue;

            // Find Max Radius
            foreach (Series series in seriesCollection)
            {
                if (CheckSeriesAxis(series))
                {
                    // Data point loop
                    foreach (DataPoint point in series.DataPoints)
                    {
                        // Find bubble radius minimum and maximum values
                        if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Bubble, series))
                        {
                            if (point.GetParameterValueDouble(ChartParameterType.Radius) > maxRadius)
                            {
                                maxRadius = point.GetParameterValueDouble(ChartParameterType.Radius);
                            }
                        }
                    }
                }
            }

            // Find Min and Max
            foreach (Series series in seriesCollection)
            {
                if (CheckSeriesAxis(series))
                {
                    // Data point loop
                    foreach (DataPoint point in series.DataPoints)
                    {
                        // Find scatter minimum and maximum values
                        if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Bubble, series))
                        {
                            if (point.GetParameterValueDouble(attributeType) < minimum)
                            {
                                minimum = point.GetParameterValueDouble(attributeType);
                            }

                            if (point.GetParameterValueDouble(attributeType) > maximum)
                            {
                                maximum = point.GetParameterValueDouble(attributeType);
                            }
                        }
                    }
                }
            }

            double range = Math.Abs(maximum - minimum);
            double seriesExtent;

            if (this.IsDateTime)
            {
                seriesExtent = range;
            }
            else
            {
                seriesExtent = Math.Max(range, maxRadius);
            }
            double radiusRelative = (maxRadius == 0) ? 0 : seriesExtent * 0.2 / maxRadius;

            minimum = double.MaxValue;
            maximum = double.MinValue;

            // Find Min and Max
            foreach (Series series in seriesCollection)
            {
                if (CheckSeriesAxis(series))
                {
                    // Data point loop
                    foreach (DataPoint point in series.DataPoints)
                    {
                        // Find scatter minimum and maximum values
                        if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Bubble, series))
                        {
                            double radius = point.GetParameterValueDouble(ChartParameterType.Radius) * radiusRelative;
                            double value = point.GetParameterValueDouble(attributeType);
                            if (value - radius < minimum)
                            {
                                minimum = value - radius;
                            }

                            if (value + radius > maximum)
                            {
                                maximum = value + radius;
                            }
                        }
                    }
                }
            }

            if (minimum == double.MaxValue || maximum == double.MinValue)
            {
                minimum = 0;
                maximum = 1;
            }

            if (minimum == maximum)
            {
                // this makes the datapoints to be drawn in the middle of the chart
                minimum -= 0.5;
                maximum += 0.5;
            }


            min = minimum;
            max = maximum;
        }

        /// <summary>
        /// Finds minimum and maximum values from series using 
        /// a stock chart types.
        /// </summary>
        /// <param name="min">Current Minimum value</param>
        /// <param name="max">Current Maximum value</param>
        private void FindStockMinMax(ref double min, ref double max)
        {
            double minimum = double.MaxValue;
            double maximum = double.MinValue;

            SeriesCollection seriesCollection = _chart.GetContainer().Series;

            // Series loop
            foreach (Series series in seriesCollection)
            {
                // Find stock minimum and maximum values
                if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Stock, series))
                {
                    if (CheckSeriesAxis(series))
                    {
                        // Data point loop
                        foreach (DataPoint point in series.DataPoints)
                        {

                            FindStockMinMaxForPoint(ref minimum, ref maximum, point, ChartParameterType.High);
                            FindStockMinMaxForPoint(ref minimum, ref maximum, point, ChartParameterType.Low);
                            FindStockMinMaxForPoint(ref minimum, ref maximum, point, ChartParameterType.Open);
                            FindStockMinMaxForPoint(ref minimum, ref maximum, point, ChartParameterType.Close);
                        }
                    }
                }
            }

            if (min > minimum)
            {
                min = minimum;
            }

            if (max < maximum)
            {
                max = maximum;
            }
        }

        private void FindStockMinMaxForPoint(ref double min, ref double max, DataPoint point, ChartParameterType attributeType)
        {
            if (point.GetParameterValueDouble(attributeType) < min)
            {
                min = point.GetParameterValueDouble(attributeType);
            }

            if (point.GetParameterValueDouble(attributeType) > max)
            {
                max = point.GetParameterValueDouble(attributeType);
            }
        }

        /// <summary>
        /// Finds minimum and maximum values from series using 
        /// a stacked chart type. Series are grouped by chart type. 
        /// </summary>
        /// <param name="min">Current Minimum value</param>
        /// <param name="max">Current Maximum value</param>
        /// <param name="type">Chart type (stacked)</param>
        private void FindStackedMinMax(ref double min, ref double max, ChartType type)
        {
            SeriesCollection seriesCollection = _chart.GetContainer().Series;

            // Find max number of data points for selected stacked chart type 
            int numOfPoints = 0;
            foreach (Series series in seriesCollection)
            {
                if (series.ChartType == type)
                {
                    if (numOfPoints < series.DataPoints.Count)
                    {
                        numOfPoints = series.DataPoints.Count;
                    }
                }
            }

            // Data point loop
            for (int pointIndx = 0; pointIndx < numOfPoints; pointIndx++)
            {
                double stackedMin = 0;
                double stackedMax = 0;

                // Series loop
                foreach (Series series in seriesCollection)
                {
                    // Find stacked minimum and maximum values
                    if (series.ChartType == type && series.DataPoints.Count > pointIndx)
                    {
                        if (series.DataPoints[pointIndx].Value < 0)
                        {
                            stackedMin += series.DataPoints[pointIndx].Value;
                        }

                        if (series.DataPoints[pointIndx].Value > 0)
                        {
                            stackedMax += series.DataPoints[pointIndx].Value;
                        }
                    }
                }
                // Set regular minimum and maximum values
                if (min > stackedMin)
                {
                    min = stackedMin;
                }

                if (max < stackedMax)
                {
                    max = stackedMax;
                }

                if (_logarithmic && min == 0)
                {
                    min = 1;
                }
            }
        }

        /// <summary>
        /// Finds minimum and maximum values from series using 
        /// a 100% stacked chart type. Series are grouped by chart type. 
        /// </summary>
        /// <param name="min">Current Minimum value</param>
        /// <param name="max">Current Maximum value</param>
        private void FindStacked100MinMax(ref double min, ref double max)
        {
            SeriesCollection seriesCollection = _chart.GetContainer().Series;

            // Find max number of data points for selected stacked chart type 
            bool positiveValues = false;
            bool negativeValues = false;
            foreach (Series series in seriesCollection)
            {
                foreach (DataPoint point in series.DataPoints)
                {
                    if (point.Value > 0)
                    {
                        positiveValues = true;
                    }

                    if (point.Value < 0)
                    {
                        negativeValues = true;
                    }
                }
            }

            if (negativeValues)
            {
                min = -100;
            }
            else
            {
                min = 0;
            }

            if (positiveValues)
            {
                max = 100;
            }
            else
            {
                max = 0;
            }

            if (_logarithmic)
            {
                // 100% Chart types do not support logarithmic axes.
                throw new InvalidOperationException(ErrorString.Exc61);
            }
        }

        internal double GetCrossingPosition()
        {
            double dataValue = _crossing;

            if (_logarithmic)
            {
                dataValue = Math.Log(dataValue, _logarithmicBase);
            }

            return GetPosition(dataValue);
        }

        internal double GetPixelValue(double pixelPosition)
        {
            double valuePosition = 0;
            if (_pixelOrientationNegative)
            {
                valuePosition = _roundedMaximum - pixelPosition * (_roundedMaximum - _roundedMinimum) / _sizeInPixels;
            }
            else
            {
                valuePosition = pixelPosition * (_roundedMaximum - _roundedMinimum) / _sizeInPixels + _roundedMinimum;
            }

            return valuePosition;
        }

        /// <summary>
        /// Gets the pixel value. If the axis is not logarithmic the value returned is the same as the one returned by GetPixelValue.
        /// </summary>
        /// <param name="pixelPosition">The pixel position.</param>
        /// <returns></returns>
        internal double GetPixelValueLogarithmic(double pixelPosition)
        {
            double valuePosition = GetPixelValue(pixelPosition);
            if (this.Logarithmic)
            {
                valuePosition = Math.Pow(this.LogarithmicBase, valuePosition);
            }
            return valuePosition;
        }
        
        internal double GetPosition(double valuePosition)
        {
            double position = 0;
            double roundedRange = this._roundedMaximum - this._roundedMinimum;
            if (roundedRange == 0.0)
            {
                // [DN 11/7/2008:10202] avoid NaN while we still CaN
                return 0.0;
            }
            if (_pixelOrientationNegative)
            {
                position = _sizeInPixels / roundedRange * (_roundedMaximum - valuePosition);
            }
            else
            {
                position = _sizeInPixels / roundedRange * (valuePosition - _roundedMinimum);
            }

            if (_is3D)
            {
                return position * _scene3DSize - _scene3DSize / 2.0;
            }

            return position;
        }

        internal double GetSize(double valueSize)
        {
            double size = Math.Abs(_sizeInPixels / (_roundedMaximum - _roundedMinimum) * (valueSize));

            if (_is3D)
            {
                size *= _scene3DSize;
            }
            return size;
        }


        internal double GetPositionLogarithmic(double valuePosition)
        {
            if (_logarithmic)
            {
                if (valuePosition <= 0)
                {
                    valuePosition = 1;
                }
                valuePosition = Math.Log(valuePosition, _logarithmicBase);
            }
            return GetPosition(valuePosition);

        }

        internal bool IsValueVisible(double value)
        {
            if (_logarithmic)
            {
                value = Math.Log(value, _logarithmicBase);
            }

            return value >= _roundedMinimum && value <= _roundedMaximum;
        }

        private double FindRoundedMinimum(double min, double max)
        {
            if (_autoMinimum)
            {
                double estimatedInterval = FindRoundedInterval((max - min) / (double)_intervalNumber);
                return AxisValue.Round(Math.Floor(min / estimatedInterval) * estimatedInterval);
            }
            else
            {
                return _minimum;
            }

        }

        private double FindRoundedMaximum(double min, double max)
        {
            if (_autoMaximum)
            {
                if (_indexed || _indexedSeries)
                {
                    return max;
                }
                double estimatedInterval = FindRoundedInterval((max - min) / _intervalNumber);
                return AxisValue.Round(Math.Ceiling(max / estimatedInterval) * estimatedInterval);
            }
            else
            {
                return _maximum;
            }
        }

        private double FindRoundedInterval(double min, double max)
        {
            if ((_indexed || _indexedSeries) && (max - min < MaxNumberOfLabels || _labelsExist))
            {
                return 1;
            }
            else if (_autoMaximum)
            {
                return FindRoundedInterval((max - min) / _intervalNumber);
            }
            else
            {
                return _interval;
            }
        }

        private double FindRoundedInterval(double interval)
        {
            if (interval <= 0)
            {
                // Axis interval cannot be negative value or zero.
                throw new ArgumentException(ErrorString.Exc11, "interval");
            }
            double baseInterval;

            if (_logarithmic)
            {
                baseInterval = Math.Floor(Math.Log10(Math.Abs(interval)));
                if (baseInterval == 0)
                {
                    baseInterval = 1;
                }
                return AxisValue.Round(Math.Floor((interval / baseInterval)) * baseInterval);
            }
            else
            {
                baseInterval = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(interval))));
                return Round(Math.Floor((interval / baseInterval)) * baseInterval);
            }
        }

        internal static double Round(double value)
        {
            double rec = 1.0 / value;
            if (Math.Abs(rec) > 10000)
            {
                double sigDigits = Math.Log10(Math.Abs(rec));
                if (sigDigits > 13)
                {
                    return value;
                }
            }

            return Math.Round(value, 14);
        }

        internal AxisValue Clone()
        {
            return (AxisValue)this.MemberwiseClone();
        }

        internal double GetLineValue(double dataPointValue)
        {
            double logValue = dataPointValue;
            if (_logarithmic)
            {
                logValue = Math.Log(dataPointValue, _logarithmicBase);
            }

            if (logValue > _roundedMaximum)
            {
                logValue = _roundedMaximum;
            }

            if (logValue < _roundedMinimum)
            {
                logValue = _roundedMinimum;
            }

            return GetPosition(logValue);
        }

        internal double GetAreaZeroValue()
        {
            return GetLineValue(this.Crossing);
        }

        internal double GetStackedColumnValue(double dataPointValue, double dataPointStackedValue)
        {
            if (dataPointValue > 0)
            {
                return GetLineValue(dataPointStackedValue);
            }
            else
            {
                return GetLineValue(dataPointStackedValue - dataPointValue);
            }
        }


        internal double GetStackedColumnHeight(double dataPointValue, double dataPointStackedValue)
        {
            double logValue = dataPointValue;
            double logStackedValue = dataPointStackedValue;
            if (_logarithmic)
            {
                logValue = Math.Log(dataPointValue, _logarithmicBase);
                logStackedValue = Math.Log(dataPointStackedValue, _logarithmicBase);
                if (double.IsInfinity(logStackedValue))
                {
                    logStackedValue = 0;
                }
            }

            if (logStackedValue > _roundedMaximum)
            {
                logValue = logValue - logStackedValue + _roundedMaximum;
            }

            if (logStackedValue < _roundedMinimum)
            {
                logValue = logValue - logStackedValue + _roundedMinimum;
            }

            return Math.Abs(GetPositionLogarithmic(0) - GetPosition(logValue));
        }

        internal double GetColumnValue(double dataPointValue)
        {
            return this.GetColumnValue(dataPointValue, true);
        }
        internal double GetColumnValue(double dataPointValue, bool minimumIsAxisCrossing)
        {
            double lineValue = this.GetLineValue(dataPointValue);
            if (minimumIsAxisCrossing)
            {
                return Math.Min(lineValue, this.GetPositionLogarithmic(Crossing));
            }
            else
            {
                return lineValue;
            }
        }

        internal double GetColumnHeight(double dataPointValue)
        {
            double logValue = dataPointValue;
            if (_logarithmic)
            {
                logValue = Math.Log(dataPointValue, _logarithmicBase);
            }

            if (logValue > _roundedMaximum)
            {
                logValue = _roundedMaximum;
            }
            else if (logValue < _roundedMinimum)
            {
                logValue = _roundedMinimum;
            }

            return Math.Abs(GetPositionLogarithmic(Crossing) - GetPosition(logValue));
        }

        /// <summary>
        /// Calculates the number of Z position for 3D charts. Stacked chart 
        /// types are grouped together by Z value.
        /// </summary>
        /// <param name="seriesList">The array of series</param>
        /// <returns>The number of Z positions</returns>
        private int GetNumberOfZRows(SeriesCollection seriesList)
        {
            int numOfRows = 0;

            // Find all stacked chart types
            ArrayList stackedTypes = ChartSeries.GetStackedChartTypes();

            // Creates �used flags� for every stacked chart type and set used to false.
            bool[] stackedFlag = new bool[stackedTypes.Count];

            for (int stackedIndex = 0; stackedIndex < stackedFlag.Length; stackedIndex++)
            {
                stackedFlag[stackedIndex] = false;
            }

            // Series loop
            foreach (Series series in seriesList)
            {
                // The series is not staked.
                bool stacked = false;
                for (int stackedIndex = 0; stackedIndex < stackedTypes.Count; stackedIndex++)
                {
                    if (series.ChartType == (ChartType)stackedTypes[stackedIndex])
                    {
                        stacked = true;
                    }
                }

                // Add Z rows for non stacked chart types
                if (!stacked)
                {
                    numOfRows++;
                }

                // Find the number of stacked type groups
                for (int stackedIndex = 0; stackedIndex < stackedTypes.Count; stackedIndex++)
                {
                    if (series.ChartType == (ChartType)stackedTypes[stackedIndex])
                    {
                        stackedFlag[stackedIndex] = true;
                    }
                }
            }

            // Add Z row for every stacked group.
            for (int stackedIndex = 0; stackedIndex < stackedFlag.Length; stackedIndex++)
            {
                if (stackedFlag[stackedIndex])
                {
                    numOfRows++;
                }
            }

            return numOfRows;
        }

        internal bool IsPrimary()
        {
            if (this.AxisType == AxisType.PrimaryX || this.AxisType == AxisType.PrimaryY || this.AxisType == AxisType.PrimaryZ)
            {
                return true;
            }

            return false;
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