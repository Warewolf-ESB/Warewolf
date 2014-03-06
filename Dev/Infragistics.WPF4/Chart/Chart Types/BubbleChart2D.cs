
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
    /// This class creates 2D bubble chart. This class is also 
    /// responsible for 2D bubble chart animation.
    /// </summary>
    internal class BubbleChart2D : ChartSeries
    {
        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type 
        /// uses all value axes (scatter chart).
        /// </summary>
        override internal bool IsScatter { get { return true; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type 
        /// is Bubble.
        /// </summary>
        override internal bool IsBubble { get { return true; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        internal BubbleChart2D()
        {
        }

        /// <summary>
        /// Draws data points for different 2D chart types using Shapes. Draws data points for 
        /// all series which have selected chart type. Creates hit test functionality, 
        /// mouse events and tooltips for data points.
        /// </summary>
        protected override void Draw2D()
        {            
            int seriesNum = SeriesList.Count;

            double xVal;
            double yVal;
            double radius;

            // Bubble max radius in pixels
            double pixelRadius = Math.Min(
                AxisX.GetSize(Math.Abs(AxisX.RoundedMaximum - AxisX.RoundedMinimum) / 3),
                AxisY.GetSize(Math.Abs(AxisY.RoundedMaximum - AxisY.RoundedMinimum) / 3)
            );
            
            // Calculates Max radius value from all data points.
            double maxRadius = FindMaxRadius(SeriesList);
           
            // Data series loop
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {                
                if (SeriesList[seriesIndx].ChartType != ChartType.Bubble)
                {
                    continue;
                }

                // The number of data points
                int pointNum = SeriesList[seriesIndx].DataPoints.Count;

                // Data points loop
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = SeriesList[seriesIndx].DataPoints[pointIndx];

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }
                    
                    // Data point X and Y position
                    xVal = AxisX.GetPositionLogarithmic(point.GetParameterValueDouble(ChartParameterType.ValueX));
                    yVal = AxisY.GetPositionLogarithmic(point.GetParameterValueDouble(ChartParameterType.ValueY));
                    if (maxRadius == 0)
                    {
                        radius = 0;
                    }
                    else
                    {
                        radius = pixelRadius / maxRadius * point.GetParameterValueDouble(ChartParameterType.Radius);
                        if (radius < 0)
                        {
                            radius = 0;
                        }
                    }                    
                                        
                    // Draw a data point
                    AddShape(new Point(xVal, yVal), radius, point, pointIndx, seriesIndx);
                }                
            }
        }

        /// <summary>
        /// This method draws data points as shapes.
        /// </summary>
        /// <param name="center">Bubble center.</param>
        /// <param name="radius">The radius of bubble chart.</param>
        /// <param name="point">Coresponding data point.</param>
        /// <param name="pointIndex">Current data point index.</param>
        /// <param name="seriesIndex">Current series index.</param>
        protected void AddShape(Point center, double radius, DataPoint point, int pointIndex, int seriesIndex)
        {
            if (point.GetSeries().UseDataTemplate)
            {
                ContentControl content = this.AddTemplate(center, radius, point, pointIndex, seriesIndex);
                this.SetHitTest2D(content, point);
            }
            else
            {
                DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);

                Ellipse ellipse = new Ellipse();
                ellipse.Width = radius;
                ellipse.Height = radius;

                Canvas.SetLeft(ellipse, center.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, center.Y - ellipse.Height / 2);

                SetShapeparameters(ellipse, point, seriesIndex, pointIndex);

                if (IsAnimationEnabled(animation))
                {
                    ScaleTransform scaleTransform = new ScaleTransform(0, 0);
                    animation.From = 0;
                    animation.To = 1;

                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

                    ellipse.RenderTransform = scaleTransform;
                    ellipse.RenderTransformOrigin = new Point(0.5, 0.5);
                }

                SetHitTest2D(ellipse, point);
                _elements.Add(ellipse);
            }
        }

        /// <summary>
        /// This method draws data points as shapes.
        /// </summary>
        /// <param name="center">Bubble center.</param>
        /// <param name="radius">The radius of bubble chart.</param>
        /// <param name="point">Coresponding data point.</param>
        /// <param name="pointIndex">Current data point index.</param>
        /// <param name="seriesIndex">Current series index.</param>
        protected ContentControl AddTemplate(Point center, double radius, DataPoint point, int pointIndex, int seriesIndex)
        {
            BubbleChartTemplate template = new BubbleChartTemplate();
            ContentControl content = new ContentControl();

            point.DataPointTemplate = template;
            
            content.Content = template;

            DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);

            point.UpdateActualFill(seriesIndex, pointIndex);
            template.Fill = point.ActualFill;

            point.UpdateActualStroke(seriesIndex, pointIndex);
            template.Stroke = point.ActualStroke;

            template.StrokeThickness = point.GetStrokeThickness(seriesIndex, pointIndex);
            template.ToolTip = point.GetToolTip();

            Canvas.SetLeft(content, center.X - radius / 2);
            Canvas.SetTop(content, center.Y - radius / 2);

            content.Width = radius;
            content.Height = radius;

            SetContentEvents(content, point);

            if (IsAnimationEnabled(animation))
            {
                ScaleTransform scaleTransform = new ScaleTransform(0, 0);
                animation.From = 0;
                animation.To = 1;

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

                content.RenderTransform = scaleTransform;
                content.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            _elements.Add(content);
            return content;
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