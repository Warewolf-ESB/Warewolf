
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class PlottingGridsArea
    {
        #region Fields

        private PlottingPane _plottingPane;
        private double _width;
        private double _height;
        private Brush _borderBrush;

        #endregion Fields

        #region Properties


        /// <summary>
        /// Gets Chart Creator
        /// </summary>
        internal ChartCreator ChartCreator
        {
            get
            {
                return _plottingPane.Chart.ChartCreator;
            }
        }

        internal double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        internal double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// The color of border
        /// </summary>
        internal Brush BorderBrush
        {
            get
            {
                return _borderBrush;
            }
            set
            {
                _borderBrush = value;
            }
        }

        private AxisValue AxisX
        {
            get { return _plottingPane.AxisX; }
        }

        private AxisValue AxisY
        {
            get { return _plottingPane.AxisY; }
        }

        private AxisValue AxisX2
        {
            get { return _plottingPane.AxisX2; }
        }

        private AxisValue AxisY2
        {
            get { return _plottingPane.AxisY2; }
        }

        #endregion Properties

        #region Methods

        internal PlottingGridsArea(PlottingPane plottingPane)
        {
            _plottingPane = plottingPane;
        }


        /// <summary>
        /// Render Axis lines and grid lines
        /// </summary>
        /// <param name="dc">Drawing Context</param>
        internal void OnRender(DrawingContext dc)
        {
            // Draw scene border
            dc.DrawRectangle(Brushes.Transparent, new Pen(_borderBrush, 2), new Rect(0, 0, this._width, this._height));

            // *****************************************************
            // Primary
            // *****************************************************
            // X Stripes
            DrawStripes(dc, AxisType.PrimaryX);

            // Y Stripes
            DrawStripes(dc, AxisType.PrimaryY);

            // X major gridlines
            DrawGridlines(dc, AxisType.PrimaryX, true);

            // X minor gridlines
            DrawGridlines(dc, AxisType.PrimaryX, false);

            // Y minor gridlines
            DrawGridlines(dc, AxisType.PrimaryY, false);

            // Y major gridlines
            DrawGridlines(dc, AxisType.PrimaryY, true);

            // *****************************************************
            // Secondary
            // *****************************************************

            // Y2 Stripes
            DrawStripes(dc, AxisType.SecondaryY);

            // X2 Stripes
            DrawStripes(dc, AxisType.SecondaryX);

            // X2 major gridlines
            DrawGridlines(dc, AxisType.SecondaryX, true);

            // X2 minor gridlines
            DrawGridlines(dc, AxisType.SecondaryX, false);

            // Y2 minor gridlines
            DrawGridlines(dc, AxisType.SecondaryY, false);

            // Y2 major gridlines
            DrawGridlines(dc, AxisType.SecondaryY, true);

            // Render Axes
            RenderAxisLine(dc, AxisType.PrimaryX);
            RenderAxisLine(dc, AxisType.PrimaryY);
            RenderAxisLine(dc, AxisType.SecondaryX);
            RenderAxisLine(dc, AxisType.SecondaryY);
        }

        /// <summary>
        /// Render Axis lines and grid lines using GDI+ Drawing
        /// </summary>
        /// <param name="graphics">Gdi Graphics</param>
        internal void OnRender(GdiGraphics graphics)
        {
            // Smoothing mode is used because gridline position has to match the position of 
            // the thick marks which are drawn using WPF.
            graphics.SetSmoothingMode(true);

            // *****************************************************
            // Primary
            // *****************************************************
            // X Stripes
            DrawStripes(graphics, AxisType.PrimaryX);

            // Y Stripes
            DrawStripes(graphics, AxisType.PrimaryY);

            // X major gridlines
            DrawGridlines(graphics, AxisType.PrimaryX, true);

            // X minor gridlines
            DrawGridlines(graphics, AxisType.PrimaryX, false);

            // Y minor gridlines
            DrawGridlines(graphics, AxisType.PrimaryY, false);

            // Y major gridlines
            DrawGridlines(graphics, AxisType.PrimaryY, true);

            // *****************************************************
            // Secondary
            // *****************************************************

            // Y2 Stripes
            DrawStripes(graphics, AxisType.SecondaryY);

            // X2 Stripes
            DrawStripes(graphics, AxisType.SecondaryX);

            // X2 major gridlines
            DrawGridlines(graphics, AxisType.SecondaryX, true);

            // X2 minor gridlines
            DrawGridlines(graphics, AxisType.SecondaryX, false);

            // Y2 minor gridlines
            DrawGridlines(graphics, AxisType.SecondaryY, false);

            // Y2 major gridlines
            DrawGridlines(graphics, AxisType.SecondaryY, true);

            // Render Axes
            RenderAxisLine(graphics, AxisType.PrimaryX);
            RenderAxisLine(graphics, AxisType.PrimaryY);
            RenderAxisLine(graphics, AxisType.SecondaryX);
            RenderAxisLine(graphics, AxisType.SecondaryY);

            graphics.SetSmoothingMode(false);

        }

        /// <summary>
        /// Draw stripes without animation using GDI+.
        /// </summary>
        /// <param name="graphics">Gdi Graphics</param>
        /// <param name="axisType">Axis Type; X and Y only (2D charts)</param>
        private void DrawStripes(GdiGraphics graphics, AxisType axisType)
        {
            Chart chart = _plottingPane.Chart;
            Axis axis = chart.GetContainer().Axes.GetAxis(axisType);

            if (axis != null)
            {
                foreach (Stripe stripe in axis.Stripes)
                {
                    DrawStripe(graphics, axisType, stripe);
                }
            }
        }

        /// <summary>
        /// Draw stripes with or without animation.
        /// </summary>
        /// <param name="dc">Drawing Context</param>
        /// <param name="axisType">Axis Type; X and Y only (2D charts)</param>
        private void DrawStripes(DrawingContext dc, AxisType axisType)
        {
            Chart chart = _plottingPane.Chart;
            Axis axis = chart.GetContainer().Axes.GetAxis(axisType);

            if (axis != null)
            {
                foreach (Stripe stripe in axis.Stripes)
                {
                    DrawStripe(dc, axisType, stripe);
                }
            }
        }

        /// <summary>
        /// Draw stripe without animation using GDI+.
        /// </summary>
        /// <param name="graphics">Gdi Graphics</param>
        /// <param name="axisType">Axis Type; X and Y only (2D charts)</param>
        /// <param name="stripe">Stripe to draw</param>
        private void DrawStripe(GdiGraphics graphics, AxisType axisType, Stripe stripe)
        {
            AxisValue axisValue = null;
            Pen pen = new Pen(Brushes.Gray, 1);
            Brush fill = Brushes.LightGray;

            Chart chart = _plottingPane.Chart;
            Axis axis = chart.GetContainer().Axes.GetAxis(axisType);

            if (axis == null || axis != null && !axis.Visible)
            {
                return;
            }

            // Set Stroke 
            if (stripe.Stroke != null)
            {
                pen.Brush = stripe.Stroke;
            }

            // Set Fill
            if (stripe.Fill != null)
            {
                fill = stripe.Fill;
            }

            pen.Thickness = stripe.StrokeThickness;

            bool xType = true;
            if (axisType == AxisType.PrimaryX)
            {
                axisValue = AxisX;
            }
            else if (axisType == AxisType.PrimaryY)
            {
                axisValue = AxisY;
                xType = false;
            }
            else if (axisType == AxisType.SecondaryX)
            {
                axisValue = AxisX2;
            }
            else if (axisType == AxisType.SecondaryY)
            {
                axisValue = AxisY2;
                xType = false;
            }

            double interval = axisValue.RoundedInterval * 2;
            if (stripe != null && stripe.Unit != 0)
            {
                interval = stripe.Unit;
            }

            double stripeWidth = axisValue.RoundedInterval;
            if (stripe != null && stripe.Width != 0)
            {
                stripeWidth = stripe.Width;
            }

            int index = 0;

            // Interval validation
            if ((axisValue.RoundedMaximum - axisValue.RoundedMinimum) / interval > 50)
            {
                // Mark Unit value too small.
                throw new InvalidOperationException(ErrorString.Exc12);
            }

            for (double position = axisValue.RoundedMinimum; position + stripeWidth <= axisValue.RoundedMaximum; position += interval)
            {
                if (xType && ChartCreator.IsSceneType(SceneType.Bar) || !xType && !ChartCreator.IsSceneType(SceneType.Bar))
                {
                    Rect rect = new Rect(new Point(0, axisValue.GetPosition(position)), new Point(_width, axisValue.GetPosition(position + stripeWidth)));
                    graphics.DrawRectangle(fill, pen, rect);
                }
                else
                {
                    Rect rect = new Rect(new Point(axisValue.GetPosition(position), 0), new Point(axisValue.GetPosition(position + stripeWidth), _height));
                    graphics.DrawRectangle(fill, pen, rect);
                }

                index++;

            }
        }

        /// <summary>
        /// Draw stripe with or without animation.
        /// </summary>
        /// <param name="dc">Drawing Context</param>
        /// <param name="axisType">Axis Type; X and Y only (2D charts)</param>
        /// <param name="stripe">Stripe to draw</param>
        private void DrawStripe(DrawingContext dc, AxisType axisType, Stripe stripe)
        {
            RectAnimation rectAnimation = null;
            long tickDuration = 0;
            AxisValue axisValue = null;
            Animation animation = null;
            Pen pen = new Pen(Brushes.Gray, 1);
            Brush fill = Brushes.LightGray;

            Chart chart = _plottingPane.Chart;
            Axis axis = chart.GetContainer().Axes.GetAxis(axisType);

            if (axis == null || axis != null && !axis.Visible)
            {
                return;
            }

            // Set Stroke 
            if (stripe.Stroke != null)
            {
                pen.Brush = stripe.Stroke;
            }

            // Set Fill
            if (stripe.Fill != null)
            {
                fill = stripe.Fill;
            }

            pen.Thickness = stripe.StrokeThickness;
            animation = stripe.Animation;

            bool xType = true;
            if (axisType == AxisType.PrimaryX)
            {
                axisValue = AxisX;
            }
            else if (axisType == AxisType.PrimaryY)
            {
                axisValue = AxisY;
                xType = false;
            }
            else if (axisType == AxisType.SecondaryX)
            {
                axisValue = AxisX2;
            }
            else if (axisType == AxisType.SecondaryY)
            {
                axisValue = AxisY2;
                xType = false;
            }

            double interval = axisValue.RoundedInterval * 2;
            if (stripe != null && stripe.Unit != 0)
            {
                interval = stripe.Unit;
            }

            double stripeWidth = axisValue.RoundedInterval;
            if (stripe != null && stripe.Width != 0)
            {
                stripeWidth = stripe.Width;
            }

            if (animation != null)
            {
                rectAnimation = new RectAnimation();
                rectAnimation.BeginTime = animation.BeginTime;
                rectAnimation.Duration = animation.Duration;
                rectAnimation.AccelerationRatio = animation.AccelerationRatio;
                rectAnimation.DecelerationRatio = animation.DecelerationRatio;
                rectAnimation.RepeatBehavior = animation.RepeatBehavior;

                // Calculate begin time and duration for sequential animation.
                if (animation.Sequential)
                {
                    Duration duration = Duration.Automatic;

                    int numOfLines = GetNumOfTicks(axisValue, interval);

                    if (animation.Duration.HasTimeSpan)
                    {
                        tickDuration = animation.Duration.TimeSpan.Ticks / numOfLines;
                        duration = new Duration(new TimeSpan(tickDuration));
                    }

                    rectAnimation.Duration = duration;
                }
            }

            int index = 0;

            // Interval validation
            if ((axisValue.RoundedMaximum - axisValue.RoundedMinimum) / interval > 50)
            {
                // Mark Unit value too small.
                throw new InvalidOperationException(ErrorString.Exc12);
            }

            for (double position = axisValue.RoundedMinimum; position + stripeWidth <= axisValue.RoundedMaximum; position += interval)
            {
                if (rectAnimation != null)
                {
                    if (xType && ChartCreator.IsSceneType(SceneType.Bar) || !xType && !ChartCreator.IsSceneType(SceneType.Bar))
                    {
                        rectAnimation.From = new Rect(0, axisValue.GetPosition(position + stripeWidth), 0, axisValue.GetSize(stripeWidth));
                        rectAnimation.To = new Rect(0, axisValue.GetPosition(position + stripeWidth), _width, axisValue.GetSize(stripeWidth));
                    }
                    else
                    {
                        rectAnimation.From = new Rect(axisValue.GetPosition(position), _height, axisValue.GetSize(stripeWidth), 0);
                        rectAnimation.To = new Rect(axisValue.GetPosition(position), 0, axisValue.GetSize(stripeWidth), _height);
                    }

                    long beginTime = animation.BeginTime.Ticks + tickDuration * index;
                    rectAnimation.BeginTime = new TimeSpan(beginTime);

                    // Create a clock the for the animation.
                    AnimationClock clock = rectAnimation.CreateClock();

                    if (xType && ChartCreator.IsSceneType(SceneType.Bar) || !xType && !ChartCreator.IsSceneType(SceneType.Bar))
                    {
                        Rect rect = new Rect(new Point(0, axisValue.GetPosition(position)), new Point(0, axisValue.GetPosition(position + stripeWidth)));
                        dc.DrawRectangle(fill, pen, rect, clock);
                    }
                    else
                    {
                        Rect rect = new Rect(new Point(axisValue.GetPosition(position), 0), new Point(axisValue.GetPosition(position + stripeWidth), 0));
                        dc.DrawRectangle(fill, pen, rect, clock);
                    }
                }
                else
                {
                    if (xType && ChartCreator.IsSceneType(SceneType.Bar) || !xType && !ChartCreator.IsSceneType(SceneType.Bar))
                    {
                        Rect rect = new Rect(new Point(0, axisValue.GetPosition(position)), new Point(_width, axisValue.GetPosition(position + stripeWidth)));
                        dc.DrawRectangle(fill, pen, rect);
                    }
                    else
                    {
                        Rect rect = new Rect(new Point(axisValue.GetPosition(position), 0), new Point(axisValue.GetPosition(position + stripeWidth), _height));
                        dc.DrawRectangle(fill, pen, rect);
                    }
                }

                index++;

            }
        }

        /// <summary>
        /// Draw grid lines with or without animation.
        /// </summary>
        /// <param name="dc">Drawing Context</param>
        /// <param name="axisType">Axis Type; X and Y only (2D charts)</param>
        /// <param name="major">True if major gridlines are drawn.</param>
        private void DrawGridlines(DrawingContext dc, AxisType axisType, bool major)
        {
            // Fill point animation from chart animation
            PointAnimation pointAnimation = null;
            long tickDuration = 0;
            AxisValue axisValue = null;
            Animation animation = null;
            Pen pen = new Pen(Brushes.Gray, 1);
            Chart chart = _plottingPane.Chart;
            Mark mark = chart.GetContainer().Axes.GetMark(axisType, true, major);

            // Minor Gridlines disabled
            if (mark == null || mark != null && !major && mark.Unit == 0)
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
                animation = mark.Animation;
            }

            bool xType = true;
            if (axisType == AxisType.PrimaryX)
            {
                axisValue = AxisX;
            }
            else if (axisType == AxisType.PrimaryY)
            {
                axisValue = AxisY;
                xType = false;
            }
            else if (axisType == AxisType.SecondaryX)
            {
                axisValue = AxisX2;
            }
            else if (axisType == AxisType.SecondaryY)
            {
                axisValue = AxisY2;
                xType = false;
            }

            double interval = axisValue.RoundedInterval;
            if (mark != null && mark.Unit != 0)
            {
                interval = mark.Unit;
            }

            if (animation != null)
            {
                pointAnimation = new PointAnimation();
                pointAnimation.BeginTime = animation.BeginTime;
                pointAnimation.Duration = animation.Duration;
                pointAnimation.AccelerationRatio = animation.AccelerationRatio;
                pointAnimation.DecelerationRatio = animation.DecelerationRatio;
                pointAnimation.RepeatBehavior = animation.RepeatBehavior;

                // Calculate begin time and duration for sequential animation.
                if (animation.Sequential)
                {
                    Duration duration = Duration.Automatic;

                    int numOfLines = GetNumOfTicks(axisValue, interval);

                    if (animation.Duration.HasTimeSpan)
                    {
                        tickDuration = animation.Duration.TimeSpan.Ticks / numOfLines;
                        duration = new Duration(new TimeSpan(tickDuration));
                    }

                    pointAnimation.Duration = duration;
                }
            }

            int index = 0;

            // Interval validation
            if ((axisValue.RoundedMaximum - axisValue.RoundedMinimum) / interval > 50)
            {
                // Mark Unit value too small.
                //throw new InvalidOperationException(ErrorString.Exc12);
            }

            foreach (double position in GetGridLineIntervals(axisValue, interval, major))
            {
                if (pointAnimation != null)
                {
                    if (xType && ChartCreator.IsSceneType(SceneType.Bar) || !xType && !ChartCreator.IsSceneType(SceneType.Bar))
                    {
                        pointAnimation.From = new Point(0, position);
                        pointAnimation.To = new Point(_width, position);
                    }
                    else
                    {
                        pointAnimation.From = new Point(position, _height);
                        pointAnimation.To = new Point(position, 0);
                    }

                    long beginTime = animation.BeginTime.Ticks + tickDuration * index;
                    pointAnimation.BeginTime = new TimeSpan(beginTime);

                    // Create a clock the for the animation.
                    AnimationClock clock = pointAnimation.CreateClock();
                    AnimationClock emptyClock = new PointAnimation().CreateClock();
                    if (xType && ChartCreator.IsSceneType(SceneType.Bar) || !xType && !ChartCreator.IsSceneType(SceneType.Bar))
                    {
                        dc.DrawLine(pen, new Point(0, position), emptyClock, new Point(0, position), clock);
                    }
                    else
                    {
                        dc.DrawLine(pen, new Point(position, _height), emptyClock, new Point(position, _height), clock);
                    }
                }
                else
                {
                    if (xType && ChartCreator.IsSceneType(SceneType.Bar) || !xType && !ChartCreator.IsSceneType(SceneType.Bar))
                    {
                        dc.DrawLine(pen, new Point(0, position), new Point(_width, position));
                    }
                    else
                    {
                        dc.DrawLine(pen, new Point(position, 0), new Point(position, _height));
                    }
                }

                index++;

            }
        }

        /// <summary>
        /// Draw grid lines without animation using GDI+.
        /// </summary>
        /// <param name="graphics">Gdi Graphics</param>
        /// <param name="axisType">Axis Type; X and Y only (2D charts)</param>
        /// <param name="major">True if major gridlines are drawn.</param>
        private void DrawGridlines(GdiGraphics graphics, AxisType axisType, bool major)
        {
            AxisValue axisValue = null;
            Pen pen = new Pen(Brushes.Gray, 1);
            Chart chart = _plottingPane.Chart;
            Mark mark = chart.GetContainer().Axes.GetMark(axisType, true, major);

            // Minor Gridlines disabled
            if (mark == null || mark != null && !major && mark.Unit == 0)
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

            bool xType = true;
            if (axisType == AxisType.PrimaryX)
            {
                axisValue = AxisX;
            }
            else if (axisType == AxisType.PrimaryY)
            {
                axisValue = AxisY;
                xType = false;
            }
            else if (axisType == AxisType.SecondaryX)
            {
                axisValue = AxisX2;
            }
            else if (axisType == AxisType.SecondaryY)
            {
                axisValue = AxisY2;
                xType = false;
            }

            double interval = axisValue.RoundedInterval;
            if (mark != null && mark.Unit != 0)
            {
                interval = mark.Unit;
            }

            int index = 0;

            // Interval validation
            





            foreach (double position in GetGridLineIntervals(axisValue, interval, major))
            {
                if (xType && ChartCreator.IsSceneType(SceneType.Bar) || !xType && !ChartCreator.IsSceneType(SceneType.Bar))
                {
                    graphics.DrawLine(pen, new Point(0, position), new Point(_width, position));
                }
                else
                {
                    graphics.DrawLine(pen, new Point(position, 0), new Point(position, _height));
                }

                index++;
            }
        }

        private IEnumerable<double> GetGridLineIntervals(AxisValue axisValue, double interval, bool major)
        {
            if (axisValue.Logarithmic && major == false)
            {
                return GetLogarithmicMinorGridLineIntervals(axisValue, interval);
            }

            return GetGridLineIntervals(axisValue, interval);
        }

        private IEnumerable<double> GetGridLineIntervals(AxisValue axisValue, double interval)
        {
            decimal intervalDecimal = Convert.ToDecimal(interval);
            decimal roundedMin = Convert.ToDecimal(axisValue.RoundedMinimum);
            decimal roundedMax = Convert.ToDecimal(axisValue.RoundedMaximum);
            for (decimal position = roundedMin; position <= roundedMax; position += intervalDecimal)
            {
                yield return axisValue.GetPosition(Convert.ToDouble(position));
            }
        }

        /// <summary>
        /// Gets the logarithmic grid line intervals.
        /// 
        ///  Note: Major gridline is the one at integral powers, "|"
        ///  Minor are the one in-between major, "+"
        ///
        /// |----+---+--+-++|----+---+--+-++|----+---+--+-++|
        /// </summary>
        /// <param name="axisValue">The axis value.</param>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        private IEnumerable<double> GetLogarithmicMinorGridLineIntervals(AxisValue axisValue, double interval)
        {
            List<double> minorPositions = new List<double>();

            Axis axis = axisValue.Chart.GetContainer().Axes.GetAxis(axisValue.AxisType);

            double majorInterval = axisValue.RoundedInterval;
            if (axis.MajorGridline != null && axis.MajorGridline.Unit != 0)
            {
                majorInterval = axis.MajorGridline.Unit;
            }

            double prevMajorPosition = double.MinValue;

            IEnumerable<double> majorIntervals = GetGridLineIntervals(axisValue, majorInterval);
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


        /// <summary>
        /// Find the number of gridlines
        /// </summary>
        /// <param name="axis">Axis which contains gridlines</param>
        /// <param name="interval">The axis interval</param>
        /// <returns>The number of gridlines</returns>
        internal static int GetNumOfTicks(AxisValue axis, double interval)
        {
            int numOfTicks = 0;
            for (double position = axis.RoundedMinimum; position <= axis.RoundedMaximum; position += interval)
            {
                numOfTicks++;
            }

            return numOfTicks;
        }

        /// <summary>
        /// Draw axis line with or without animation.
        /// </summary>
        /// <param name="dc">Drawing Context</param>
        /// <param name="axisType">Axis Type; X and Y only (2D charts)</param>
        private void RenderAxisLine(DrawingContext dc, AxisType axisType)
        {
            Chart chart = _plottingPane.Chart;

            Axis axis = chart.GetContainer().Axes.GetAxis(axisType);

            if (axis == null || axis != null && !axis.Visible)
            {
                return;
            }

            AxisValue axisValue = null;

            // Take reference of an axis value.
            if (axisType == AxisType.PrimaryX)
            {
                axisValue = AxisY;
            }
            else if (axisType == AxisType.SecondaryX)
            {
                axisValue = AxisY2;
            }
            else if (axisType == AxisType.PrimaryY)
            {
                axisValue = AxisX;
            }
            else if (axisType == AxisType.SecondaryY)
            {
                axisValue = AxisX2;
            }

            double axisPositon;
            if (axisValue.IsPrimary())
            {
                axisPositon = axisValue.GetPosition(axisValue.Crossing);
            }
            else
            {
                axisPositon = axisValue.GetPosition(axisValue.RoundedMaximum);
            }

            bool xType = false;

            if (axisType == AxisType.PrimaryX || axisType == AxisType.SecondaryX)
            {
                xType = true;
            }

            Pen pen;
            PointAnimation animation = null;
            AnimationClock clock = null;
            if (axis != null)
            {
                // Create pen for axis line
                if (axis.Stroke != null)
                {
                    pen = new Pen(axis.Stroke, axis.StrokeThickness);
                }
                else
                {
                    pen = new Pen(Brushes.Black, axis.StrokeThickness);
                }

                if (axis.Animation != null)
                {
                    if (ChartCreator.IsSceneType(SceneType.Bar))
                    {
                        if (xType)
                        {
                            // Create point animation for Y axis.
                            animation = new PointAnimation(
                            new Point(axisPositon, _height),
                            new Point(axisPositon, 0),
                            new Duration(TimeSpan.FromSeconds(0)));
                        }
                        else
                        {
                            // Create point animation for X axis.
                            animation = new PointAnimation(
                            new Point(0, axisPositon),
                            new Point(_width, axisPositon),
                            new Duration(TimeSpan.FromSeconds(0)));
                        }
                    }
                    else
                    {
                        if (xType)
                        {
                            // Create point animation for X axis.
                            animation = new PointAnimation(
                            new Point(0, axisPositon),
                            new Point(_width, axisPositon),
                            new Duration(TimeSpan.FromSeconds(0)));
                        }
                        else
                        {
                            // Create point animation for Y axis.
                            animation = new PointAnimation(
                            new Point(axisPositon, _height),
                            new Point(axisPositon, 0),
                            new Duration(TimeSpan.FromSeconds(0)));
                        }
                    }

                    // Create start and end time for animation.
                    animation.BeginTime = axis.Animation.BeginTime;
                    animation.Duration = axis.Animation.Duration;
                    animation.AccelerationRatio = axis.Animation.AccelerationRatio;
                    animation.DecelerationRatio = axis.Animation.DecelerationRatio;
                    animation.RepeatBehavior = axis.Animation.RepeatBehavior;

                    // Create a clock the for the animation.
                    clock = animation.CreateClock();
                }
            }
            else // Create default pen.
            {
                pen = new Pen(Brushes.Black, 2);
            }
            // Draw line with or without animation
            if (ChartCreator.IsSceneType(SceneType.Bar))
            {
                if (clock != null)
                {                    
                    AnimationClock emptyClock = new PointAnimation().CreateClock();
                    if (xType)
                    {
                        dc.DrawLine(pen, new Point(axisPositon, _height), clock, new Point(axisPositon, _height), emptyClock);
                    }
                    else
                    {
                        dc.DrawLine(pen, new Point(0, axisPositon), emptyClock, new Point(0, axisPositon), clock);
                    }
                }
                else
                {
                    if (xType)
                    {
                        dc.DrawLine(pen, new Point(axisPositon, 0), new Point(axisPositon, _height));
                    }
                    else
                    {
                        dc.DrawLine(pen, new Point(0, axisPositon), new Point(_width, axisPositon));
                    }
                }
            }
            else
            {
                if (clock != null)
                {
                    AnimationClock emptyClock = new PointAnimation().CreateClock();

                    if (xType)
                    {
                        dc.DrawLine(pen, new Point(0, axisPositon), emptyClock, new Point(0, axisPositon), clock);
                    }
                    else
                    {
                        dc.DrawLine(pen, new Point(axisPositon, _height), clock, new Point(axisPositon, _height), emptyClock);
                    }
                }
                else
                {
                    if (xType)
                    {
                        dc.DrawLine(pen, new Point(0, axisPositon), new Point(_width, axisPositon));
                    }
                    else
                    {
                        dc.DrawLine(pen, new Point(axisPositon, 0), new Point(axisPositon, _height));
                    }
                }
            }
        }

        /// <summary>
        /// Draw axis line without animation using GDI+.
        /// </summary>
        /// <param name="graphics">Gdi Graphics</param>
        /// <param name="axisType">Axis Type; X and Y only (2D charts)</param>
        private void RenderAxisLine(GdiGraphics graphics, AxisType axisType)
        {
            Chart chart = _plottingPane.Chart;

            Axis axis = chart.GetContainer().Axes.GetAxis(axisType);

            if (axis == null || axis != null && !axis.Visible)
            {
                return;
            }

            AxisValue axisValue = null;

            // Take reference of an axis value.
            if (axisType == AxisType.PrimaryX)
            {
                axisValue = AxisY;
            }
            else if (axisType == AxisType.SecondaryX)
            {
                axisValue = AxisY2;
            }
            else if (axisType == AxisType.PrimaryY)
            {
                axisValue = AxisX;
            }
            else if (axisType == AxisType.SecondaryY)
            {
                axisValue = AxisX2;
            }

            double axisPositon;
            if (axisValue.IsPrimary())
            {
                axisPositon = axisValue.GetPosition(axisValue.Crossing);
            }
            else
            {
                axisPositon = axisValue.GetPosition(axisValue.RoundedMaximum);
            }

            bool xType = false;

            if (axisType == AxisType.PrimaryX || axisType == AxisType.SecondaryX)
            {
                xType = true;
            }

            Pen pen;
            if (axis != null)
            {
                // Create pen for axis line
                if (axis.Stroke != null)
                {
                    pen = new Pen(axis.Stroke, axis.StrokeThickness);
                }
                else
                {
                    pen = new Pen(Brushes.Black, axis.StrokeThickness);
                }
            }
            else // Create default pen.
            {
                pen = new Pen(Brushes.Black, 2);
            }

            // Draw line with or without animation
            if (ChartCreator.IsSceneType(SceneType.Bar))
            {
                if (xType)
                {
                    graphics.DrawLine(pen, new Point(axisPositon, 0), new Point(axisPositon, _height));
                }
                else
                {
                    graphics.DrawLine(pen, new Point(0, axisPositon), new Point(_width, axisPositon));
                }
            }
            else
            {
                if (xType)
                {
                    graphics.DrawLine(pen, new Point(0, axisPositon), new Point(_width, axisPositon));
                }
                else
                {
                    graphics.DrawLine(pen, new Point(axisPositon, 0), new Point(axisPositon, _height));
                }
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