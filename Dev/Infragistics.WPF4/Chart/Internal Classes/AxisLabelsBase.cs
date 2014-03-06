
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class AxisLabelsBase
    {
        #region Fields

        protected StringCollection _labels = new StringCollection();
        private DoubleCollection _positions = new DoubleCollection();
        protected FormattedText[] _formattedLabels;
        protected FontFamily _fontFamily = new FontFamily("Arial");
        protected FontStyle _fontStyle;
        protected FontWeight _fontWeight;
        protected FontStretch _fontStretch;
        protected double _fontSize = 11;
        protected string _fontFormat = string.Empty;
        protected double _textAngle = 0;
        private double _labelDistance;
        protected bool _autoAngleEnabled = true;
        protected bool _autoFontSizeEnabled = true;
        protected double _minimumFontSize = 10;
        protected bool _topAxisLabels;
        private AxisValue _axis;
        private Chart _chart;
        private double _shift;
        private bool _is3D;
        protected Brush _fontBrush = Brushes.Black;
        private double _width;
        private double _height;
        private bool _leftAxisLabels;
        private bool _visible = true;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the value which indicates wheater the labels belong to the left axis.
        /// </summary>
        internal bool LeftAxisLabels
        {
            get
            {
                return _leftAxisLabels;
            }
            set
            {
                _leftAxisLabels = value;
            }
        }

        /// <summary>
        /// Gets or sets the value which indicates wheater the labels are visible.
        /// </summary>
        internal bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
            }
        }

        /// <summary>
        /// Gets or sets Label area width
        /// </summary>
        internal double Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        /// <summary>
        /// Gets or sets Label area height
        /// </summary>
        internal double Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }
        
        internal bool Is3D
        {
            get
            {
                return _is3D;
            }
            set
            {
                _is3D = value;
            }
        }

        /// <summary>
        /// Gets or sets a collection of label positions.
        /// </summary>
        internal DoubleCollection Positions
        {
            get
            {
                return _positions;
            }
            set
            {
                _positions = value;
            }
        }
        
        /// <summary>
        /// Gets or sets axis which contains this labels
        /// </summary>
        internal AxisValue Axis
        {
            get
            {
                return _axis;
            }
            set
            {
                _axis = value;
            }
        }

        /// <summary>
        /// Gets or sets the chart which contains this labels
        /// </summary>
        internal Chart Chart
        {
            get
            {
                return _chart;
            }
            set
            {
                _chart = value;
            }
        }

        /// <summary>
        /// Gets or sets the chart scene which contains this labels
        /// </summary>
        internal ScenePane ScenePane
        {
            get
            {
                return _chart.ChartCreator.ScenePane;
            }
        }

        /// <summary>
        /// Gets or sets the shift of the first label from the border of the label canvas. 
        /// </summary>
        internal double Shift
        {
            get
            {
                return _shift;
            }
            set
            {
                _shift = value;
            }
        }

        /// <summary>
        /// Gets or sets the distance between axis and labels
        /// </summary>
        internal double LabelDistance
        {
            get
            {
                return _labelDistance;
            }
            set
            {
                _labelDistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum font Size for automatic labels. 
        /// </summary>
        internal double MinimumFontSize
        {
            get
            {
                return _minimumFontSize;
            }
            set
            {
                _minimumFontSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the value which indicates wheater the labels belong to the top axis.
        /// </summary>
        internal bool TopAxisLabels
        {
            get
            {
                return _topAxisLabels;
            }
            set
            {
                _topAxisLabels = value;
            }
        }

        #endregion Properties

        #region Methods

        internal virtual void Draw(DrawingContext context)
        {
        }

        /// <summary>
        /// Sets font size for all labels
        /// </summary>
        /// <param name="fontSize">The new font size</param>
        protected void SetFontSize(double fontSize)
        {
            for (int labelIndx = 0; labelIndx < _labels.Count; labelIndx++)
            {
                _formattedLabels[labelIndx].SetFontSize(fontSize);
            }
        }

        /// <summary>
        /// Set maximum text width for all labels
        /// </summary>
        /// <param name="width">The new text width</param>
        protected void SetMaxTextWidth(double width)
        {
            for (int labelIndx = 0; labelIndx < _labels.Count; labelIndx++)
            {
                if (width != 0)
                {
                    _formattedLabels[labelIndx].SetMaxTextWidths(new double[] { width });
                }
            }
        }

        /// <summary>
        /// Find the longest label
        /// </summary>
        /// <returns>The longest label index</returns>
        protected int FindLongestLabel()
        {
            // Find the longest label
            int longestLabelIndx = -1;
            double longestLabel = 0;
            for (int labelIndx = 0; labelIndx < _labels.Count; labelIndx++)
            {
                if (_formattedLabels[labelIndx].Width > longestLabel)
                {
                    longestLabel = _formattedLabels[labelIndx].Width;
                    longestLabelIndx = labelIndx;
                }
            }

            return longestLabelIndx;
        }

        protected double GetPosition(int index)
        {
            if (_axis == null)
            {
                // The axis of the labels not set.
                throw new InvalidOperationException(ErrorString.Exc4);
            }
            if (_chart == null)
            {
                // The chart not set.
                throw new InvalidOperationException(ErrorString.Exc5);
            }

            if (this is AxisLabelsHorizontal)
            {
                if (_is3D)
                {
                    if (_axis.AxisType == AxisType.PrimaryZ)
                    {
                        if (_chart.ChartCreator.IsSceneType(SceneType.Scatter))
                        {
                            return _shift + _axis.GetPosition(_positions[index]);
                        }
                        else
                        {
                            return _shift + _axis.GetPosition(_positions[index] - 0.5);
                        }
                    }
                    else
                    {
                        return _shift + _axis.GetPosition(_positions[index]);
                    }
                }
                else
                {
                    return ScenePane.GetAbsoluteX(_shift) + _axis.GetPosition(_positions[index]);
                }
            }
            else
            {
                if (_is3D)
                {
                    return _shift + _axis.GetPosition(_positions[index]);
                }
                else
                {
                    return ScenePane.GetAbsoluteY(_shift) + _axis.GetPosition(_positions[index]);
                }
            }
        }
        
        internal void FillLabels()
        {
            if (_chart.GetContainer().Axes.GetAxis(_axis.AxisType) == null)
            {
                return;
            }

            CultureInfo cultureToUse = CultureInformation.CultureToUse;

            // Set Label Distance
            SetLabelDistance();

            if (_axis.IndexedSeries)
            {
                SeriesCollection series = null;
                series = _chart.GetContainer().Series;

                for (double position = _axis.RoundedMinimum + _axis.RoundedInterval; position <= _axis.RoundedMaximum; position += _axis.RoundedInterval)
                {
                    int itemIndex = (int)(position - 1);
                    int labelTextPosition = (int)(position);

                    if (series.Count == 0)
                    {
                        continue;
                    }

                    if (series.Count > 0 && !string.IsNullOrEmpty(series[itemIndex].Label))
                    {
                        _labels.Add(series[itemIndex].Label);
                    }
                    else
                    {
                        string textValue;
                        if (!string.IsNullOrEmpty(_fontFormat))
                        {
                            textValue = string.Format(cultureToUse, _fontFormat, labelTextPosition);
                        }
                        else
                        {
                            textValue = labelTextPosition.ToString(cultureToUse);
                        }

                        _labels.Add(textValue);
                    }
                    Positions.Add(_axis.RoundedMaximum + 1 - position);
                }
            }
            else if (_axis.Indexed)
            {
                SeriesCollection series = this.Chart.GetContainer().Series;
                Series seriesToUse = null;
                foreach (Series s in series)
                {
                    if (this.Axis.CheckSeriesAxis(s))
                    {
                        seriesToUse = s;
                    }
                }
                if (seriesToUse == null)
                {
                    return;
                }
                int itemIndex = 0;
                for (double position = _axis.RoundedMinimum + _axis.RoundedInterval; position < _axis.RoundedMaximum; position += _axis.RoundedInterval)
                {
                    int labelIndex = (int)position - 1;

                    if (labelIndex >= seriesToUse.DataPoints.Count)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(seriesToUse.DataPoints[labelIndex].Label) && seriesToUse.DataPoints.Count > 0 && itemIndex < seriesToUse.DataPoints.Count && (seriesToUse.DataPoints.Count < AxisValue.MaxNumberOfLabels - 1 || _axis.LabelsExist))
                    {
                        if (labelIndex >= seriesToUse.DataPoints.Count)
                        {
                            break;
                        }

                        _labels.Add(seriesToUse.DataPoints[labelIndex].Label);
                    }
                    else
                    {   
                        string textValue;
                        if (!string.IsNullOrEmpty(_fontFormat))
                        {
                            textValue = string.Format(cultureToUse, _fontFormat, position);
                        }
                        else
                        {

                            textValue = position.ToString(cultureToUse);
                        }

                        _labels.Add(textValue);
                    }
                    Positions.Add(position);
                    itemIndex++;
                }
            }
            else
            {
                decimal roundedMin = Convert.ToDecimal(this._axis.RoundedMinimum);
                decimal roundedMax = Convert.ToDecimal(this._axis.RoundedMaximum);
                decimal roundedInterval = Convert.ToDecimal(this._axis.RoundedInterval);
                for (decimal position = roundedMin; position <= roundedMax; position += roundedInterval)
                {
                    double positionDouble = Convert.ToDouble(position);
                    if (!_axis.Logarithmic)
                    {
                        positionDouble = AxisValue.Round(positionDouble);
                    }

                    double logPosition = positionDouble;
                    if (_axis.Logarithmic)
                    {
                        logPosition = Math.Pow(_axis.LogarithmicBase, positionDouble);
                    }

                    string textValue;
                    if (!string.IsNullOrEmpty(_fontFormat) && !String.IsNullOrEmpty(_fontFormat))
                    {
                        if (_axis.IsDateTime)
                        {
                            DateTime dateTime = DateTime.FromOADate(logPosition);
                            textValue = string.Format(cultureToUse, _fontFormat, dateTime);
                        }
                        else
                        {
                            try
                            {
                                textValue = string.Format(cultureToUse, _fontFormat, logPosition);
                            }
                            catch
                            {
                                throw new InvalidOperationException(ErrorString.Exc68);
                            }
                        }
                    }
                    else
                    {
                        if (_axis.IsDateTime)
                        {
                            DateTime dateTime = DateTime.FromOADate(logPosition);
                            textValue = string.Format(cultureToUse, "{0:dd/MM/yy}", dateTime);
                        }
                        else
                        {  
                            if (_axis.RoundedInterval < 1)
                            {
                                // this makes the integers consistent in precison with the doubles
                                double x = _axis.RoundedInterval;
                                string format;
                                int precision = -(int)Math.Floor(Math.Log10(x));
                                format = "F" + precision.ToString();
                                textValue = logPosition.ToString(format, cultureToUse);
                            }
                            else
                            {
                                textValue = logPosition.ToString(cultureToUse);
                            }
                        }
                    }

                    _labels.Add(textValue);

                    if (_axis.AxisType == AxisType.PrimaryZ)
                    {
                        Positions.Add(_axis.RoundedMaximum + _axis.RoundedMinimum - positionDouble);
                    }
                    else
                    {
                        Positions.Add(positionDouble);
                    }
                }
            }
        }

        internal void SetLabelStyle(Chart chart, AxisType axisType)
        {
            Axis axis = chart.GetContainer().Axes.GetAxis(axisType);
            
            TextBlock textBlock = new TextBlock();
            if (chart.GetContainer().Scene != null && chart.GetContainer().Scene.ScenePane != null)
            {
                chart.GetContainer().Scene.ScenePane.Children.Add(textBlock);
            }
            _fontFamily = textBlock.FontFamily;
            _fontSize = textBlock.FontSize;
            _fontStretch = textBlock.FontStretch;
            _fontStyle = textBlock.FontStyle;
            _fontWeight = textBlock.FontWeight;
            _fontBrush = textBlock.Foreground;

            // Take appearance from font properties
            if (axis != null && axis.Label != null)
            {
                if (axis.Label.ReadLocalValue(Label.FontFamilyProperty) != DependencyProperty.UnsetValue || Label.FontFamilyProperty.DefaultMetadata.DefaultValue != axis.Label.FontFamily)
                {
                    _fontFamily = axis.Label.FontFamily;
                }

                if (axis.Label.ReadLocalValue(Label.FontSizeProperty) != DependencyProperty.UnsetValue || (double)Label.FontSizeProperty.DefaultMetadata.DefaultValue != axis.Label.FontSize)
                {
                    _fontSize = axis.Label.FontSize;
                }

                if (axis.Label.ReadLocalValue(Label.FontStretchProperty) != DependencyProperty.UnsetValue || (FontStretch)Label.FontStretchProperty.DefaultMetadata.DefaultValue != axis.Label.FontStretch)
                {
                    _fontStretch = axis.Label.FontStretch;
                }

                if (axis.Label.ReadLocalValue(Label.FontStyleProperty) != DependencyProperty.UnsetValue || (FontStyle)Label.FontStyleProperty.DefaultMetadata.DefaultValue != axis.Label.FontStyle)
                {
                    _fontStyle = axis.Label.FontStyle;
                }

                if (axis.Label.ReadLocalValue(Label.FontWeightProperty) != DependencyProperty.UnsetValue || (FontWeight)Label.FontWeightProperty.DefaultMetadata.DefaultValue != axis.Label.FontWeight)
                {
                    _fontWeight = axis.Label.FontWeight;
                }

                if (axis.Label.ReadLocalValue(Label.ForegroundProperty) != DependencyProperty.UnsetValue || Label.ForegroundProperty.DefaultMetadata.DefaultValue != axis.Label.Foreground)
                {
                    _fontBrush = axis.Label.Foreground;
                }

                if (axis.Label.ReadLocalValue(Label.FormatProperty) != DependencyProperty.UnsetValue || (string)Label.FormatProperty.DefaultMetadata.DefaultValue != axis.Label.Format)
                {
                    _fontFormat = axis.Label.Format;
                }

                if (double.IsNaN(axis.Label.Angle) == false)
                {
                    _textAngle = axis.Label.Angle;
                    _autoAngleEnabled = false;
                }

                _autoFontSizeEnabled = axis.Label.AutoResize;
            }

            if (chart.GetContainer().Scene != null && chart.GetContainer().Scene.ScenePane != null)
            {
                chart.GetContainer().Scene.ScenePane.Children.Remove(textBlock);
            }

        }

        /// <summary>
        /// Draw tick lines with or without animation.
        /// </summary>
        /// <param name="dc">Drawing Context</param>
        /// <param name="axisType">Axis Type</param>
        /// <param name="major">True if major TickMarks are drawn.</param>
        protected void DrawTickMarks(DrawingContext dc, AxisType axisType, bool major)
        {
            // Fill point animation from chart animation
            Pen pen = new Pen(Brushes.Gray, 1);
            Mark mark = Chart.GetContainer().Axes.GetMark(axisType, false, major);
            AxisValue axisValue = this.Axis;

            // Minor TickMarks disabled
            if (mark != null && !major && mark.Unit == 0)
            {
                return;
            }

            if (!mark.Visible)
            {
                return;
            }

            if (mark != null)
            {
                if (mark.Stroke != null)
                {
                    pen.Brush = mark.Stroke;
                }

                pen.Thickness = mark.StrokeThickness;
                pen.DashStyle = mark.DashStyle;
            }

            double tickSize = 5;
            if (mark != null)
            {
                tickSize *= mark.TickMarkSize;
            }

            int index = 0;
            double interval = axisValue.RoundedInterval;
            if (mark != null && mark.Unit != 0)
            {
                interval = mark.Unit;
            }

            // Interval validation
            if ((axisValue.RoundedMaximum - axisValue.RoundedMinimum) / interval > 50)
            {
                // Mark Unit value too small.
                //throw new InvalidOperationException(ErrorString.Exc6);
            }

            foreach (double position in GetTickMarkIntervals(axisValue, interval, major))            
            {
                if (axisValue.Indexed && ( position == axisValue.RoundedMinimum || position == axisValue.RoundedMaximum))
                {
                    continue;
                }
                if ((axisType == AxisType.PrimaryX || axisType == AxisType.SecondaryX) && _chart.ChartCreator.IsSceneType(SceneType.Bar) || (axisType == AxisType.PrimaryY || axisType == AxisType.SecondaryY) && !_chart.ChartCreator.IsSceneType(SceneType.Bar))
                {
                    // Draw tickmarks
                    if (_leftAxisLabels)
                    {
                        dc.DrawLine(pen, new Point(0, ScenePane.GetAbsoluteY(Shift) + position), new Point(tickSize, ScenePane.GetAbsoluteY(Shift) + position));
                    }
                    else
                    {
                        dc.DrawLine(pen, new Point(Width - tickSize, ScenePane.GetAbsoluteY(Shift) + position), new Point(Width, ScenePane.GetAbsoluteY(Shift) + position));
                    }
                }
                else
                {
                    // Draw tickmarks
                    if (_topAxisLabels)
                    {
                        dc.DrawLine(pen, new Point(ScenePane.GetAbsoluteX(Shift) + position, 0), new Point(ScenePane.GetAbsoluteX(Shift) + position, tickSize));
                    }
                    else
                    {
                        dc.DrawLine(pen, new Point(ScenePane.GetAbsoluteX(Shift) + position, Height), new Point(ScenePane.GetAbsoluteX(Shift) + position, Height - tickSize));
                    }
                }

                index++;
            }
        }


        private IEnumerable<double> GetTickMarkIntervals(AxisValue axisValue, double interval, bool major)
        {
            if (axisValue.Logarithmic && major == false)
            {
                return GetLogarithmicMinorTickMarkIntervals(axisValue, interval);
            }

            return GetTickMarkIntervals(axisValue, interval);
        }

        private IEnumerable<double> GetTickMarkIntervals(AxisValue axisValue, double interval)
        {
            decimal roundedMin = Convert.ToDecimal(axisValue.RoundedMinimum);
            decimal roundedMax = Convert.ToDecimal(axisValue.RoundedMaximum);
            decimal intervalDecimal = Convert.ToDecimal(interval);
            for (decimal position = roundedMin; position <= roundedMax; position += intervalDecimal)
            {
                yield return axisValue.GetPosition(Convert.ToDouble(position));
            }
        }

        /// <summary>
        /// Gets the minor tick marks intervals.
        /// 
        /// Note: Major TickMark is the one at integral powers, "|"
        /// Minor are the one in-between major, "+"
        ///
        /// |----+---+--+-++|----+---+--+-++|----+---+--+-++|
        /// </summary>
        /// <param name="axisValue">The axis value.</param>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        private IEnumerable<double> GetLogarithmicMinorTickMarkIntervals(AxisValue axisValue, double interval)
        {
            List<double> minorPositions = new List<double>();

            Axis axis = axisValue.Chart.GetContainer().Axes.GetAxis(axisValue.AxisType);

            double majorInterval = axisValue.RoundedInterval;
            if (axis.MajorTickMark != null && axis.MajorTickMark.Unit != 0)
            {
                majorInterval = axis.MajorTickMark.Unit;
            }

            double prevMajorPosition = double.MinValue;

            IEnumerable<double> majorIntervals = GetTickMarkIntervals(axisValue, majorInterval);
            foreach (double majorPosition in majorIntervals)
            {
                if (prevMajorPosition == double.MinValue)
                {
                    prevMajorPosition = majorPosition;
                    continue;
                }

                double value1 = axisValue.GetPixelValue(majorPosition);
                double value2 = axisValue.GetPixelValue(prevMajorPosition);

                value1 = Math.Pow(axisValue.LogarithmicBase, value1);
                value2 = Math.Pow(axisValue.LogarithmicBase, value2);

                double minorDelta = (value1 - value2) * interval;

                double minorPositionValue = value2 + minorDelta;

                while (minorPositionValue < value1)
                {
                    double minorPosition = axisValue.GetPositionLogarithmic(minorPositionValue);

                    minorPositions.Add(minorPosition);
                    minorPositionValue += minorDelta;
                }

                prevMajorPosition = majorPosition;
            }

            return minorPositions;
        }


        private void SetLabelDistance()
        {
            if (Chart == null)
            {
                LabelDistance = 2;
                return;
            }

            Axis axis = Chart.GetContainer().Axes.GetAxis(Axis.AxisType);
            double distance;
            if (Chart.View3D)
            {
                distance = 2;
            }
            else
            {
                distance = 5;
            }

            if (axis != null && axis.Label != null)
            {
                distance *= axis.Label.DistanceFromAxis;
            }

            LabelDistance = distance;
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