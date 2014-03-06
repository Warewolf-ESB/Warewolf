
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
    /// This class creates 2D bar chart. This class is also 
    /// responsible for 2D bar chart animation.
    /// </summary>
    internal class BarChart2D : ChartSeries
    {
        #region Fields

        // Private fields
        protected double _width = 0.8;

        #endregion fields

        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type has Bar type axis
        /// </summary>
        override internal bool IsBar { get { return true; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart 
        /// type is clustered
        /// </summary>
        override internal bool IsClustered { get { return true; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        internal BarChart2D()
        {
        }

        /// <summary>
        /// Draws data points for different 2D chart types using Shapes. Draws data points for 
        /// all series which have selected chart type. Creates hit test functionality, 
        /// mouse events and tooltips for data points.
        /// </summary>
        protected override void Draw2D()
        {
            int numOfBarSeries = GetNumberOfBarSeries(SeriesList);

            int seriesNum = SeriesList.Count;

            double xVal;

            // Search series for point width. Because of cluster and stacked chart types 
            // all series have to have same point width.
            double width = ColumnChart2D.FindClusterPointWidth(ChartType.Bar, ChartType.CylinderBar, SeriesList, _width);

            // Data series loop
            int columnIndex = 0;
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {
                if (SeriesList[seriesIndx].ChartType != ChartType.Bar && SeriesList[seriesIndx].ChartType != ChartType.CylinderBar)
                {
                    continue;
                }

                // The number of data points
                int pointNum = SeriesList[seriesIndx].DataPoints.Count;

                // Data points loop
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = SeriesList[seriesIndx].DataPoints[pointIndx];

                    // Data point X and y position
                    xVal = pointIndx + 1;

                    if (IsXOutOfRange(xVal))
                    {
                        continue;
                    }

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }

                    double columnWidth = width / numOfBarSeries;

                    Rect rect = new Rect(
                        AxisY.GetColumnValue(point.Value),
                        AxisX.GetPosition(xVal + columnIndex * columnWidth - width / 2 + columnWidth),
                        AxisY.GetColumnHeight(point.Value),
                        AxisX.GetPosition(0) - AxisX.GetPosition(columnWidth)
                        );

                    // Draw or hit test a data point
                    AddShape(rect, point.Value > AxisY.Crossing, point, pointIndx, seriesIndx);
                }
                columnIndex++;
            }
        }

        /// <summary>
        /// Returns the number of bar series
        /// </summary>
        /// <param name="seriesList">The list of all series</param>
        /// <returns>The number of bar series</returns>
        static internal int GetNumberOfBarSeries(List<Series> seriesList)
        {
            int numOfBarSeries = 0;
            foreach (Series series in seriesList)
            {
                if (series.ChartType == ChartType.Bar || series.ChartType == ChartType.CylinderBar)
                {
                    numOfBarSeries++;
                }
            }

            return numOfBarSeries;
        }


        /// <summary>
        /// This method draws data points as shapes using data templates.
        /// </summary>
        /// <param name="rect">Column rectangle which represent a data point.</param>
        /// <param name="positive">True if a data point value is positive.</param>
        /// <param name="point">Coresponding data point.</param>
        /// <param name="pointIndex">Current data point index.</param>
        /// <param name="seriesIndex">Current series index.</param>
        protected void AddShape(Rect rect, bool positive, DataPoint point, int pointIndex, int seriesIndex)
        {
            // If fast rendering mode is active use GDI+ rendering
            if (Graphics != null)
            {
                Graphics.SetGdiPen(seriesIndex, pointIndex, point);
                Graphics.SetGdiBrush(seriesIndex, pointIndex, point);
                Graphics.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);
                Graphics.AddHitTestRegionRectangle(rect.X, rect.Y, rect.Width, rect.Height, seriesIndex, pointIndex);
            }
            else if (point.GetSeries().UseDataTemplate)
            {
                ContentControl content = this.AddTemplate(rect, positive, point, pointIndex, seriesIndex);
                this.SetHitTest2D(content, point);
            }
            else
            {
                DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);

                Rectangle rectangle = new Rectangle();
                Canvas.SetTop(rectangle, rect.Top);
                if (positive)
                {
                    Canvas.SetLeft(rectangle, rect.Left);
                }
                else
                {
                    Canvas.SetRight(rectangle, Size.Width - rect.Right);
                }

                rectangle.Width = rect.Width;
                rectangle.Height = rect.Height;

                SetShapeparameters(rectangle, point, seriesIndex, pointIndex);

                if (IsAnimationEnabled(animation))
                {
                    rectangle.Width = 0;
                    animation.To = rect.Width;
                    rectangle.BeginAnimation(Rectangle.WidthProperty, animation);
                }

                SetHitTest2D(rectangle, point);
                _elements.Add(rectangle);
            }
        }

        /// <summary>
        /// This method draws data points as shapes.
        /// </summary>
        /// <param name="rect">Column rectangle which represent a data point.</param>
        /// <param name="positive">True if a data point value is positive.</param>
        /// <param name="point">Coresponding data point.</param>
        /// <param name="pointIndex">Current data point index.</param>
        /// <param name="seriesIndex">Current series index.</param>
        protected ContentControl AddTemplate(Rect rect, bool positive, DataPoint point, int pointIndex, int seriesIndex)
        {
            BarChartTemplate template = new BarChartTemplate();
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
            template.RectangleRounding = point.GetParameterValueDouble(ChartParameterType.RectangleRounding);
            template.BorderThickness = new Thickness(point.GetStrokeThickness(seriesIndex, pointIndex));
            template.CornerRadius = new CornerRadius(point.GetParameterValueDouble(ChartParameterType.RectangleRounding));
            if (point.Value < AxisY.Crossing)
            {
                template.IsNegative = true;
            }

            Canvas.SetTop(content, rect.Top);
            if (positive)
            {
                Canvas.SetLeft(content, rect.Left);
            }
            else
            {
                Canvas.SetRight(content, Size.Width - rect.Right);
            }

            content.Width = rect.Width;
            content.Height = rect.Height;

            SetContentEvents(content, point);

            if (IsAnimationEnabled(animation))
            {
                content.Width = 0;
                animation.To = rect.Width;
                content.BeginAnimation(ContentControl.WidthProperty, animation);
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