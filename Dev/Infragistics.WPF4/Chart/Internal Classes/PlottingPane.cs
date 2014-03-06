
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class PlottingPane : ChartCanvas
    {
        #region Fields

        // Fields
        private PlottingGridsArea _grids;
        private Chart _chart;
        private ArrayList _chartSeriesList = new ArrayList();
        private Grid _panel = new Grid();

        #endregion

        #region Properties

        internal Chart Chart
        {
            get { return _chart; }
        }

        internal ArrayList ChartSeriesList
        {
            get { return _chartSeriesList; }
        }

        internal AxisValue AxisX
        {
            get { return _chart.ChartCreator.AxisX; }
        }

        internal AxisValue AxisY
        {
            get { return _chart.ChartCreator.AxisY; }
        }

        internal AxisValue AxisX2
        {
            get { return _chart.ChartCreator.AxisX2; }
        }

        internal AxisValue AxisY2
        {
            get { return _chart.ChartCreator.AxisY2; }
        }

        #endregion

        #region Methods

        internal PlottingPane(Chart chart)
        {
            _grids = new PlottingGridsArea(this);
            _chart = chart;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (this.Parent == null)
            {
                return;
            }

            //this replotting needs to be treated like a full chart refresh.
            //if we allow some of the property updates, etc, to try and modify
            //the logical tree we will get enumerator errors.
            XamChart.GetControl(_chart).IsRefreshingMode = true;
            Draw();
            XamChart.GetControl(_chart).IsRefreshingMode = false;
            base.OnRenderSizeChanged(sizeInfo);
        }

        /// <summary>
        /// Repaints the chart
        /// </summary>
        public bool Draw()
        {
            try
            {
                DrawWithException();
            }
            catch (InvalidOperationException e)
            {
                _chart.InvalidDataReceived(e);

                return false;
            }
            catch (ArgumentException e)
            {
                _chart.InvalidDataReceived(e);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Draws chart elements which belong to grid area and draw 
        /// exception on the control surface if thrown.
        /// </summary>
        private void DrawWithException()
        {
            if (this.ActualWidth == 0 || this.ActualHeight == 0)
            {
                return;
            }

            _chart.CreateFakeData();

            if (IsPerformance())
            {
                this.Children.Clear();
                _chart.HitTestInfoArray.Clear();

                _chart.FastRenderingMode = true;
                FastRenderingMode(this.ActualWidth, this.ActualHeight);
            }
            else
            {
                List<MarkerSeries> markersList = new List<MarkerSeries>();

                this.Children.Clear();
                _chart.HitTestInfoArray.Clear();

                foreach (ChartSeries chartSeries in _chartSeriesList)
                {
                    chartSeries.Size = new Size(this.ActualWidth, this.ActualHeight);
                    chartSeries.Draw(this.Children);

                    // Draw Markers
                    MarkerSeries markers = new MarkerSeries();

                    markers.Size = new Size(this.ActualWidth, this.ActualHeight);
                    markers.Chart = _chart;
                    markers.AxisX = chartSeries.AxisX;
                    markers.AxisY = chartSeries.AxisY;
                    markers.ChartSeries = chartSeries;
                    markers.SeriesList = chartSeries.SeriesList;
                    markersList.Add(markers);
                }

                foreach (MarkerSeries markers in markersList)
                {
                    markers.Draw(this.Children);
                }

                SetBorder();
            }

            XamChart chartControl = XamChart.GetControl(this.Chart);

            if (chartControl.IsCrosshairsSupported())
            {
                chartControl.Crosshairs.SetupCrosshairSizes();
            }

            _chart.RemoveFakeData();
        }
        private Brush ResolveBackground()
        {
            XamChart chartControl = XamChart.GetControl(_chart);
            if (chartControl == null)
            {
                return null;
            }
            if (chartControl.Scene.GridArea.Background != null)
            {
                return chartControl.Scene.GridArea.Background;
            }
            if (chartControl.Scene.Background != null)
            {
                // check if the background is transparent
                SolidColorBrush solidColorBrush = chartControl.Scene.Background as SolidColorBrush;
                if (solidColorBrush != null)
                {
                    if (solidColorBrush.Color == Colors.Transparent)
                    {
                        return chartControl.Background;
                    }
                }

                return chartControl.Scene.Background;
            }
            if (chartControl.Background != null)
            {
                return chartControl.Background;
            }
            return null;
        }
        /// <summary>
        /// This rendering mode use fast GDI+ rendering to improve performance of data points.
        /// </summary>
        /// <param name="width">The width of the grid area</param>
        /// <param name="height">The height of the grid area</param>
        private void FastRenderingMode(double width, double height)
        {
            // Create an image and use GDI+ to draw.
            GdiGraphics graphics = new GdiGraphics(width, height, this.ResolveBackground(), _chart);

            List<MarkerSeries> markersList = new List<MarkerSeries>();

            // Draw Axis, gridlines and striplines
            if (!_chart.ChartCreator.IsSceneType(SceneType.Pie))
            {
                _grids.Width = this.ActualWidth;
                _grids.Height = this.ActualHeight;

                SetBorder();

                _grids.OnRender(graphics);
            }

            // Smoothing mode
            if (XamChart.GetControl(_chart).Scene.GridArea.RenderingOptions.RenderingDetails.AntiAliasing)
            {
                graphics.SetSmoothingMode(true);
            }

            // Allow Brush from Data Points
            graphics.AllowDataPointBrush = XamChart.GetControl(_chart).Scene.GridArea.RenderingOptions.RenderingDetails.AllowDataPointBrush;

            // Allow Brush from Data Points
            graphics.ForceSolidBrush = XamChart.GetControl(_chart).Scene.GridArea.RenderingOptions.RenderingDetails.ForceSolidBrush;


            // Draw all chart series
            foreach (ChartSeries chartSeries in _chartSeriesList)
            {
                chartSeries.Size = new Size(this.ActualWidth, this.ActualHeight);
                chartSeries.Draw(graphics);

                // Draw Markers
                MarkerSeries markers = new MarkerSeries();

                markers.Size = new Size(this.ActualWidth, this.ActualHeight);
                markers.Chart = _chart;
                markers.AxisX = chartSeries.AxisX;
                markers.AxisY = chartSeries.AxisY;
                markers.ChartSeries = chartSeries;
                markers.SeriesList = chartSeries.SeriesList;
                markersList.Add(markers);
            }

            foreach (MarkerSeries markers in markersList)
            {
                markers.Draw(graphics, this.Children);
            }

            ImageBrush brush = graphics.CreateImageBrush();

            graphics.Dispose();

            this.Background = brush;
        }

        /// <summary>
        /// This method is used to create tooltips for 3D charts. Tooltips for 3D charts do not work 
        /// same as 2D charts. 2D charts are drawn using Framework elements and they have implemented 
        /// tooltips. 3D charts cannot use tooltips from Framework elements (model 3D is not Framework element). 
        /// This is implementation for 3D charts tooltips.
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            XamChart chartControl = XamChart.GetControl(this.Chart);

            if (chartControl.IsCrosshairsSupported())
            {
                chartControl.Crosshairs.Visible = true;
                chartControl.Crosshairs.UpdateByCursor(e.GetPosition(this));
            }

            if (!_chart.FastRenderingMode)
            {
                return;
            }

            if (!PlottingPane3D.IsToolTipsEnabled(_chart))
            {
                return;
            }

            // Call Hit test method to check if a mouse is above a data point.
            HitTestArgs args = _chart.HitTest(e);

            this.Children.Remove(_panel);
            _panel.Children.Clear();

            // If the mouse is above a data point, show a tooltip.
            if (args.SelectedObject != null)
            {
                //  Get the tooltip from selected data point.
                object pointToolTip = _chart.GetContainer().Series[args.SeriesIndex].DataPoints[args.PointIndex].ToolTip;

                if (pointToolTip != null)
                {
                    // Get mouse position 
                    Point position = e.GetPosition(this);

                    UIElement element = pointToolTip as UIElement;

                    ToolTip toolTipType = pointToolTip as ToolTip;
                    if (toolTipType != null)
                    {
                        pointToolTip = toolTipType.Content;
                    }

                    if (pointToolTip is Image)              // Tooltip is an image
                    {
                        element = (Image)pointToolTip;
                    }
                    else if (pointToolTip is UIElement)     // Toolitp is a control
                    {
                        element = (UIElement)pointToolTip;
                    }
                    else                                    // Tooltip is a text
                    {
                        string str = pointToolTip.ToString();
                        TextBlock text = new TextBlock();
                        text.Text += " ";
                        text.Text += str;
                        text.Text += " ";
                        element = text;
                    }


                    ToolTip toolTipAppearance = new ToolTip();

                    // Draw tooltip as rectangle.
                    Rectangle rect = new Rectangle();
                    rect.HorizontalAlignment = HorizontalAlignment.Stretch;
                    rect.VerticalAlignment = VerticalAlignment.Stretch;
                    rect.Fill = toolTipAppearance.Background;
                    rect.Stroke = Brushes.Black;

                    _panel.Children.Add(rect);
                    _panel.Children.Add(element);

                    this.Children.Add(_panel);

                    // Set position for the tooltip.
                    if (position.Y + _panel.ActualHeight * 2 > this.ActualHeight)
                    {
                        position.Y = this.ActualHeight - _panel.ActualHeight * 2;
                    }

                    if (position.X + _panel.ActualWidth + 30 > this.ActualWidth)
                    {
                        position.X = this.ActualWidth - _panel.ActualWidth - 30;
                    }

                    ChartCanvas.SetLeft(_panel, position.X + 30);
                    ChartCanvas.SetTop(_panel, position.Y + _panel.ActualHeight);


                }
            }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter"/>�attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            XamChart chartControl = XamChart.GetControl(this.Chart);
            chartControl.Crosshairs.Visible = true;
            if (chartControl.IsCrosshairsSupported())
            {
                chartControl.Crosshairs.Show();
            }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave"/>�attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            XamChart chartControl = XamChart.GetControl(this.Chart);
            chartControl.Crosshairs.Visible = false;
            chartControl.Crosshairs.Hide();
        }

        /// <summary>
        /// Check if a chart type supports fast rendering.
        /// </summary>
        /// <returns>True if fast rendering is supported</returns>
        private bool IsFastChartType()
        {
            XamChart control = XamChart.GetControl(_chart);
            SeriesCollection series = control.Series;
            foreach (Series ser in series)
            {
                if (ser.ChartType != ChartType.Line && ser.ChartType != ChartType.Column && ser.ChartType != ChartType.StackedColumn && ser.ChartType != ChartType.Stacked100Column
                     && ser.ChartType != ChartType.Bar && ser.ChartType != ChartType.StackedBar && ser.ChartType != ChartType.Stacked100Bar
                     && ser.ChartType != ChartType.Area && ser.ChartType != ChartType.StackedArea && ser.ChartType != ChartType.Stacked100Area
                    && ser.ChartType != ChartType.ScatterLine && ser.ChartType != ChartType.Scatter && ser.ChartType != ChartType.Point)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the number of all data points
        /// </summary>
        /// <returns>The number of points</returns>
        private int GetNumberOfPoints()
        {
            XamChart control = XamChart.GetControl(_chart);
            SeriesCollection series = control.Series;
            int number = 0;
            foreach (Series ser in series)
            {
                number += ser.DataPoints.Count;
            }

            return number;
        }

        /// <summary>
        /// Checks which rendering mode is used
        /// </summary>
        /// <returns>Returns true if fast rendering mode is enabled</returns>
        internal bool IsPerformance()
        {
            if (!IsFastChartType())
            {
                return false;
            }

            XamChart control = XamChart.GetControl(_chart);
            if (control == null || control.Scene == null || control.Scene.GridArea == null)
            {
                return false;
            }

            RenderingMode renderingMode = control.Scene.GridArea.RenderingOptions.RenderingMode;
            if (renderingMode == RenderingMode.Auto)
            {
                if (GetNumberOfPoints() > control.Scene.GridArea.RenderingOptions.RenderingDetails.AutoModePointsNumber)
                {
                    return true;
                }
            }
            else if (renderingMode == RenderingMode.Performance)
            {
                return true;
            }

            return false;
        }

        private void SetBorder()
        {
            if (_chart.GetContainer().Scene != null && _chart.GetContainer().Scene.GridArea != null && _chart.GetContainer().Scene.GridArea.BorderBrush != null)
            {
                _grids.BorderBrush = _chart.GetContainer().Scene.GridArea.BorderBrush;
            }
            else
            {
                _grids.BorderBrush = Brushes.Transparent;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {

            base.OnRender(dc);


            // If fast rendering mode draw gridlines using Gdi Graphics
            if (!IsPerformance())
            {
                if (!_chart.ChartCreator.IsSceneType(SceneType.Pie))
                {
                    _grids.Width = this.ActualWidth;
                    _grids.Height = this.ActualHeight;

                    SetBorder();

                    _grids.OnRender(dc);
                }

                RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
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