
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Globalization;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 2D pie chart. This class is also 
    /// responsible for 2D pie chart animation.
    /// </summary>
    class PieChart2D : ChartSeries
    {
        #region Fields

        #endregion fields

        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return true; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether scene exist 
        /// for this chart type.
        /// </summary>
        override internal bool IsScene { get { return false; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        internal PieChart2D()
        {
        }

        /// <summary>
        /// Draws data points for different 2D chart types using Shapes. Draws data points for 
        /// all series which have selected chart type. Creates hit test functionality, 
        /// mouse events and tooltips for data points.
        /// </summary>
        protected override void Draw2D()
        {
            double sumPoints = 0;
            
            // The number of data points
            int pointNum = SeriesList[0].DataPoints.Count;

            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = SeriesList[0].DataPoints[pointIndx];

                // Skip Null and 0 values
                if (point.NullValue == true || point.Value == 0)
                {
                    continue;
                }

                sumPoints += Math.Abs(point.Value);
            }

            if (SeriesList[0].ChartType != ChartType.Pie)
            {
                return;
            }

            double angleCoeff = 360.0 / sumPoints;

            double startAngle = 0.0;
            double endAngle = 0.0;

            // Pie slices loop
            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = SeriesList[0].DataPoints[pointIndx];

                // Skip Null and 0 values
                if (point.NullValue == true || point.Value == 0)
                {
                    continue;
                }

                endAngle += Math.Abs(point.Value) * angleCoeff;

                bool exploded = point.GetParameterValueBool(ChartParameterType.Exploded);
                double explodedRadius = point.GetParameterValueDouble(ChartParameterType.ExplodedRadius);

                double radius = Math.Min(Size.Width, Size.Height) / 2;

                // Draw or hit test a data point
                AddShape(exploded, explodedRadius, radius, startAngle, endAngle - startAngle, point, pointIndx, 0);
     
                startAngle = endAngle;
            }

            startAngle = 0;
            endAngle = 0;

            // Labels loop
            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = SeriesList[0].DataPoints[pointIndx];

                // Skip Null and 0 values
                if (point.NullValue == true || point.Value == 0)
                {
                    continue;
                }

                endAngle += Math.Abs(point.Value) * angleCoeff;

                bool exploded = point.GetParameterValueBool(ChartParameterType.Exploded);
                double explodedRadius = point.GetParameterValueDouble(ChartParameterType.ExplodedRadius);

                double radius = Math.Min(Size.Width, Size.Height) / 2;

                PieChart2D.AddLabel(exploded, this, startAngle, endAngle - startAngle, radius * 0.60, radius * explodedRadius + radius * 0.60, point, _elements, pointIndx);

                startAngle = endAngle;
            }
        }

        static internal void AddLabel(bool exploded, ChartSeries chartSeries, double startAngle, double sweepAngle, double radius, double explodedRadius, DataPoint point, UIElementCollection elements, int pointIndex)
        {

            if (sweepAngle == 0)
            {
                return;
            }

            TextBlock label = new TextBlock();
			// the hittest is disabled to prevent showing tooltips of the labels
            label.IsHitTestVisible = false;
            Marker marker = GetMarker(point);

            Point explodedCenter = FindCenter(chartSeries, true, startAngle + sweepAngle / 2.0, explodedRadius);
            Point center = FindCenter(chartSeries, true, startAngle + sweepAngle / 2.0, radius);

            if (marker == null)
            {
                return;
            }

            TranslateTransform explodedTransform = new TranslateTransform(0, 0);
            if (exploded)
            {
                explodedTransform = ExplodedAnimation(center, explodedCenter, chartSeries, point);
            }

            double value;
            value = point.Value;

            label.Width = radius;
            
            if (!string.IsNullOrEmpty(marker.Format) && !String.IsNullOrEmpty(marker.Format))
            {
                label.TextWrapping = TextWrapping.Wrap;
                label.TextAlignment = TextAlignment.Center;
                label.Text = MarkerSeries.CreateMarkerText(chartSeries.Chart.ChartCreator, point);
            }
            else
            {
                label.TextWrapping = TextWrapping.Wrap;
                label.TextAlignment = TextAlignment.Center;

                CultureInfo cultureToUse = CultureInformation.CultureToUse;
                label.Text = value.ToString(cultureToUse);
            }

            Size size = MarkerSeries.GetTextSize(label, marker, elements);

            if (size.Width > radius && radius != 0 && label.TextWrapping != TextWrapping.NoWrap)
            {
                size.Height = size.Height * Math.Ceiling(size.Width / radius);
                size.Width = radius;
            }

            DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);
            DoubleAnimation animationTranslateX = point.GetDoubleAnimation(pointIndex);
            DoubleAnimation animationTranslateY = point.GetDoubleAnimation(pointIndex);
            if (chartSeries.IsAnimationEnabled(animation))
            {
                Point centerStart = FindCenter(chartSeries, true, startAngle + sweepAngle / 2.0, 0);
                TransformGroup transformGroup = new TransformGroup();
                TranslateTransform translateFromCenter = new TranslateTransform(0, 0);
                ScaleTransform scaleTransform = new ScaleTransform(0, 0);
                animation.From = 0;
                animation.To = 1;
                animationTranslateX.From = centerStart.X;
                animationTranslateX.To = center.X - radius / 2;
                animationTranslateY.From = centerStart.Y;
                animationTranslateY.To = center.Y - size.Height / 2;

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                translateFromCenter.BeginAnimation(TranslateTransform.XProperty, animationTranslateX);
                translateFromCenter.BeginAnimation(TranslateTransform.YProperty, animationTranslateY);

                transformGroup.Children.Add(scaleTransform);
                transformGroup.Children.Add(explodedTransform);
                transformGroup.Children.Add(translateFromCenter);
                label.RenderTransform = transformGroup;
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, 0);
            }
            else
            {
                label.RenderTransform = explodedTransform;
                Canvas.SetLeft(label, center.X - radius / 2);
                Canvas.SetTop(label, center.Y - size.Height / 2);
            }
            if (marker.UseDataTemplate)
            {
                MarkerSeries.AddTemplateInternal(center, radius, point, pointIndex, 0, chartSeries.Chart, elements);
            }
            SolidColorBrush solidBrush = label.Foreground as SolidColorBrush;
            if (solidBrush != null && solidBrush.Color == Color.FromArgb(0, 0, 0, 0))
            {
                label.Foreground = Brushes.Black;
            }
            elements.Add(label);
        }

        static internal Point FindCenter(ChartSeries chartSeries, bool exploded, double angle, double radius)
        {
            Point center;
            if (exploded)
            {
                center = FindRadialPoint(new Point(chartSeries.Size.Width / 2, chartSeries.Size.Height / 2), angle, radius);
            }
            else
            {
                center = new Point(chartSeries.Size.Width / 2, chartSeries.Size.Height / 2);
            }
            return center;
        }

        private void AddShape(bool exploded, double explodedRadius, double radius, double startAngle, double sweepAngle, DataPoint point, int pointIndex, int seriesIndex)
        {
            //Add the Path Element
            Path path = new Path();

            Point explodedCenter = FindCenter(this,exploded, startAngle + sweepAngle / 2.0, radius * explodedRadius);
            Point centerPosition = FindCenter(this,false, startAngle + sweepAngle / 2.0, radius);

            Point center = new Point(radius, radius);

            PathFigure pathFigure = new PathFigure();

            // Avoid the problem with one data point.
            sweepAngle = Math.Min(sweepAngle, 359.99);

            Point startArcPoint = FindRadialPoint( center, startAngle, radius );
            Point endArcPoint = FindRadialPoint( center, startAngle + sweepAngle, radius );

            pathFigure.StartPoint = center;
            pathFigure.Segments.Add(new LineSegment(startArcPoint,true));
            //pathFigure.Segments.Add(new LineSegment(endArcPoint, true));
            bool isLargeArc = sweepAngle > 180 ? true : false;

            pathFigure.Segments.Add(new ArcSegment(endArcPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true));
            pathFigure.Segments.Add(new LineSegment(center,true));

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add( pathFigure );
            path.Data = pathGeometry; 
            //ArcSegment arcSegment = new ArcSegment( center, rect.Size, 
            Canvas.SetLeft(path, centerPosition.X - radius);
            Canvas.SetTop(path, centerPosition.Y - radius);
            path.Width = radius * 2 + point.GetStrokeThickness(seriesIndex, pointIndex);
            path.Height = radius * 2 + point.GetStrokeThickness(seriesIndex, pointIndex);

            TranslateTransform explodedTransform = ExplodedAnimation(centerPosition, explodedCenter, this, point);

            DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);
            if (IsAnimationEnabled(animation))
            {
                TransformGroup transformGroup = new TransformGroup();
                TranslateTransform translateToCenter = new TranslateTransform(-radius, -radius);
                TranslateTransform translateFromCenter = new TranslateTransform(+radius, +radius);
                ScaleTransform scaleTransform = new ScaleTransform(0, 0);
                animation.From = 0;
                animation.To = 1;

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

                transformGroup.Children.Add(translateToCenter);
                transformGroup.Children.Add(scaleTransform);
                transformGroup.Children.Add(explodedTransform);
                transformGroup.Children.Add(translateFromCenter);
                path.RenderTransform = transformGroup;
            }
            else
            {
                path.RenderTransform = explodedTransform;
            }

            SetShapeparameters(path, point, seriesIndex, pointIndex);
            SetHitTest2D(path, point);
            
            _elements.Add(path);
        }
        
       
        static internal Point FindRadialPoint( Point center, double angle, double radius)
        {
            angle = angle / 180 * Math.PI;
            double y = center.Y + radius * Math.Sin(angle);
            double x = center.X + radius * Math.Cos(angle);
            return new Point(x, y);
        }

        static internal TranslateTransform ExplodedAnimation(Point centerPosition, Point explodedCenter, ChartSeries chartSeries, DataPoint point)
        {
            // Exploded Animation
            TranslateTransform explodedTransform = new TranslateTransform(0, 0);
            Animation exAnimX = point.GetParameterValueAnimation(ChartParameterType.ExplodedAnimation);
            Animation exAnimY = point.GetParameterValueAnimation(ChartParameterType.ExplodedAnimation);
            DoubleAnimation explodedAnimationX = null;
            DoubleAnimation explodedAnimationY = null;

            if (exAnimX != null && exAnimY != null)
            {
                explodedAnimationX = exAnimX.GetDoubleAnimation();
                explodedAnimationY = exAnimY.GetDoubleAnimation();
            }

            if (chartSeries.IsAnimationEnabled(explodedAnimationX) && chartSeries.IsAnimationEnabled(explodedAnimationY))
            {
                explodedAnimationX.From = 0;
                explodedAnimationY.From = 0;

                explodedAnimationX.To = explodedCenter.X - centerPosition.X;
                explodedAnimationY.To = explodedCenter.Y - centerPosition.Y;

                explodedTransform.X = explodedCenter.X - centerPosition.X;
                explodedTransform.Y = explodedCenter.Y - centerPosition.Y;

                explodedTransform.BeginAnimation(TranslateTransform.XProperty, explodedAnimationX);
                explodedTransform.BeginAnimation(TranslateTransform.YProperty, explodedAnimationY);
            }
            else
            {
                explodedTransform.X = explodedCenter.X - centerPosition.X; ;
                explodedTransform.Y = explodedCenter.Y - centerPosition.Y; ;
            }

            return explodedTransform;
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