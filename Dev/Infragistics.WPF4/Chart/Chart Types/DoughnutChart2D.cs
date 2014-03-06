
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

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 2D doughnut chart. This class is also 
    /// responsible for 2D doughnut chart animation.
    /// </summary>
    class DoughnutChart2D : ChartSeries
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
        internal DoughnutChart2D()
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

                //Skip Null values
                if (point.NullValue == true)
                {
                    continue;
                }

                sumPoints += Math.Abs(point.Value);
            }

            double angleCoeff = 360 / sumPoints;

            double startAngle = 0;
            double endAngle = 0;

            if (SeriesList[0].ChartType != ChartType.Doughnut)
            {
                return;
            }

            // Doughnut slices loop
            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = SeriesList[0].DataPoints[pointIndx];

                //Skip Null values
                if (point.NullValue == true)
                {
                    continue;
                }

                endAngle += Math.Abs(point.Value) * angleCoeff;

                double radius = Math.Min(Size.Width, Size.Height) / 2;

                bool exploded = point.GetParameterValueBool(ChartParameterType.Exploded);
                double explodedRadius = point.GetParameterValueDouble(ChartParameterType.ExplodedRadius);

                // Draw or hit test a data point
                AddShape(exploded, explodedRadius, radius, radius * 0.7, startAngle, endAngle - startAngle, point, pointIndx);
                
                startAngle = endAngle;
            }

            startAngle = 0;
            endAngle = 0;

            // Labels loop
            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = SeriesList[0].DataPoints[pointIndx];

                //Skip Null values
                if (point.NullValue == true)
                {
                    continue;
                }

                endAngle += Math.Abs(point.Value) * angleCoeff;

                double radius = Math.Min(Size.Width, Size.Height) / 2;

                bool exploded = point.GetParameterValueBool(ChartParameterType.Exploded);
                double explodedRadius = point.GetParameterValueDouble(ChartParameterType.ExplodedRadius);

                PieChart2D.AddLabel(exploded, this, startAngle, endAngle - startAngle, radius * 0.85, radius * explodedRadius + radius * 0.85, point, _elements, pointIndx);

                startAngle = endAngle;
            }
        }

        private void AddShape(bool exploded, double explodedRadius, double radius, double innerRadius, double startAngle, double sweepAngle, DataPoint point, int pointIndex)
        {
            //Add the Path Element
            Path path = new Path();

            Point explodedCenter = PieChart2D.FindCenter(this, exploded, startAngle + sweepAngle / 2.0, radius * explodedRadius);
            Point centerPosition = PieChart2D.FindCenter(this, false, startAngle + sweepAngle / 2.0, radius);

            Point center = new Point(radius, radius);

            PathFigure pathFigure = new PathFigure();

            // Avoid the problem with one data point.
            if (sweepAngle == 360)
            {
                sweepAngle = 359.99;
            }

            Point startArcPoint = PieChart2D.FindRadialPoint(center, startAngle, radius);
            Point endArcPoint = PieChart2D.FindRadialPoint(center, startAngle + sweepAngle, radius);
            Point startInnerArcPoint = PieChart2D.FindRadialPoint(center, startAngle, innerRadius);
            Point endInnerArcPoint = PieChart2D.FindRadialPoint(center, startAngle + sweepAngle, innerRadius);

            bool isLargeArc = sweepAngle > 180 ? true : false;

            pathFigure.StartPoint = startInnerArcPoint;
            pathFigure.Segments.Add(new LineSegment(startArcPoint,true));
            pathFigure.Segments.Add(new ArcSegment(endArcPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true));
            pathFigure.Segments.Add(new LineSegment(endInnerArcPoint, true));
            pathFigure.Segments.Add(new ArcSegment(startInnerArcPoint, new Size(innerRadius, innerRadius), 0, isLargeArc, SweepDirection.Counterclockwise, true));

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add( pathFigure );
            path.Data = pathGeometry; 
            //ArcSegment arcSegment = new ArcSegment( center, rect.Size, 
            Canvas.SetLeft(path, centerPosition.X - radius);
            Canvas.SetTop(path, centerPosition.Y - radius);
            path.Width = radius * 2 + point.GetStrokeThickness(0, pointIndex);
            path.Height = radius * 2 + point.GetStrokeThickness(0, pointIndex);

            TranslateTransform explodedTransform = PieChart2D.ExplodedAnimation(centerPosition, explodedCenter, this, point);

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

            SetShapeparameters(path, point, 0, pointIndex);
            SetHitTest2D(path, point);
                      
            _elements.Add(path);
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