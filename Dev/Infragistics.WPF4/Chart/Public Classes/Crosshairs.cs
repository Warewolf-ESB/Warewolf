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

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Enum which determines which of the crosshairs are shown.
    /// </summary>
    public enum CrosshairDisplayMode
    {
		/// <summary>
		/// Vertical
		/// </summary>
        Vertical,

		/// <summary>
		/// Horizontal
		/// </summary>
        Horizontal,

		/// <summary>
		/// Horizontal and Vertical
		/// </summary>
        Both
    };

    /// <summary>
    /// Crosshairs class is the holder of the horizontal and the vertical crosshairs. It manages their drawing, positioning and styling.
    /// </summary>
    public class Crosshairs : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Crosshairs"/> class.
        /// </summary>
        public Crosshairs()
        {
            this.HorizontalCrosshair = new CrosshairLine();
            this.VerticalCrosshair = new CrosshairLine();
        }

        #region properties

        #region HorizontalCrosshair

        /// <summary>
        /// Gets or sets the horizontal crosshair.
        /// </summary>
        /// <value>The horizontal crosshair.</value>
        public CrosshairLine HorizontalCrosshair
        {
            get { return (CrosshairLine)GetValue(HorizontalCrosslineProperty); }
            set { SetValue(HorizontalCrosslineProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalCrosshair"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HorizontalCrosslineProperty =
            DependencyProperty.Register("HorizontalCrosshair", typeof(CrosshairLine), typeof(Crosshairs),
            new PropertyMetadata(null, OnHorizontalCrosshairChanged));

        private static void OnHorizontalCrosshairChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Crosshairs)d).OnHorizontalCrosshairChanged((CrosshairLine)e.OldValue, (CrosshairLine)e.NewValue);
        }

        private void OnHorizontalCrosshairChanged(CrosshairLine oldValue, CrosshairLine newValue)
        {
            newValue.Crosshairs = this;
            if (this.PlottingPane != null)
            {
                this.SetupCrosshairSizes();
                if (this.Visible)
                {
                    HideCrosshair(oldValue);
                    ShowCrosshair(newValue);
                    UpdateByCursor(new Point(this.ScreenX, this.ScreenY));
                }
            }
        }

        #endregion

        #region VerticalCrosshair

        /// <summary>
        /// Gets or sets the vertical crosshair.
        /// </summary>
        /// <value>The vertical crosshair.</value>
        public CrosshairLine VerticalCrosshair
        {
            get { return (CrosshairLine)GetValue(VerticalCrosshairProperty); }
            set { SetValue(VerticalCrosshairProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalCrosshair"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalCrosshairProperty =
            DependencyProperty.Register("VerticalCrosshair", typeof(CrosshairLine), typeof(Crosshairs),
            new PropertyMetadata(null, OnVerticalCrosshairChanged));

        private static void OnVerticalCrosshairChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Crosshairs)d).OnVerticalCrosshairChanged((CrosshairLine)e.OldValue, (CrosshairLine)e.NewValue);
        }

        private void OnVerticalCrosshairChanged(CrosshairLine oldValue, CrosshairLine newValue)
        {
            newValue.Crosshairs = this;
            if (this.PlottingPane != null)
            {
                this.SetupCrosshairSizes();
                if (this.Visible)
                {
                    HideCrosshair(oldValue);
                    ShowCrosshair(newValue);
                    UpdateByCursor(new Point(this.ScreenX, this.ScreenY));
                }
            }
        }

        #endregion


        #region Enabled

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Crosshairs"/> are enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Enabled"/> dependency property
        /// </summary>
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(Crosshairs),
            new PropertyMetadata(false, OnEnabledChanged));

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Crosshairs)d).OnEnabledChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnEnabledChanged(bool oldValue, bool newValue)
        {
            if (this.Visible)
            {
                if (newValue)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        #endregion

        #region DisplayMode



        /// <summary>
        /// Gets or sets the display mode. It determines which ones of the crosshais are shown.
        /// </summary>
        /// <value>The display mode.</value>
        public CrosshairDisplayMode DisplayMode
        {
            get { return (CrosshairDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DisplayMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(CrosshairDisplayMode), typeof(Crosshairs),
            new PropertyMetadata(CrosshairDisplayMode.Both, OnDisplayModeChanged));

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Crosshairs)d).OnDisplayModeChanged((CrosshairDisplayMode)e.OldValue, (CrosshairDisplayMode)e.NewValue);
        }

        private void OnDisplayModeChanged(CrosshairDisplayMode oldValue, CrosshairDisplayMode newValue)
        {
            if (this.Visible)
            {
                Hide();
                Show();
            }
        }

        #endregion

        #region ScreenX

        

        /// <summary>
        /// Gets the screen X. It is the X position of the cross point of the crosshairs in screen coordinates.
        /// </summary>
        /// <value>The screen X.</value>
        public double ScreenX
        {
            get { return (double)GetValue(ScreenXProperty); }
            internal set { SetValue(ScreenXProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ScreenX"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ScreenXProperty =
            DependencyProperty.Register("ScreenX", typeof(double), typeof(Crosshairs),
              new PropertyMetadata(0.0));

        #endregion

        #region ScreenY



        /// <summary>
        /// Gets the screen Y. It is the Y position of the cross point of the crosshairs in screen coordinates.
        /// </summary>
        /// <value>The screen Y.</value>
        public double ScreenY
        {
            get { return (double)GetValue(ScreenYProperty); }
            internal set { SetValue(ScreenYProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ScreenY"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ScreenYProperty =
            DependencyProperty.Register("ScreenY", typeof(double), typeof(Crosshairs),
              new PropertyMetadata(0.0));

        #endregion


        #region GraphX

        

        /// <summary>
        /// Gets the graph X. It is the X axis value corresponding to the X of the cross point of the crosshairs.
        /// </summary>
        /// <value>The graph X.</value>
        public double GraphX
        {
            get { return (double)GetValue(GraphXProperty); }
            internal set { SetValue(GraphXProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="GraphX"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GraphXProperty =
            DependencyProperty.Register("GraphX", typeof(double), typeof(Crosshairs),
              new PropertyMetadata(0.0));

        #endregion

        #region GraphY

        /// <summary>
        /// Gets the graph Y. It is the Y axis value corresponding to the Y of the cross point of the crosshairs.
        /// </summary>
        /// <value>The graph Y.</value>
        public double GraphY
        {
            get { return (double)GetValue(GraphYProperty); }
            internal set { SetValue(GraphYProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="GraphY"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GraphYProperty =
            DependencyProperty.Register("GraphY", typeof(double), typeof(Crosshairs),
              new PropertyMetadata(0.0));

        #endregion

        #region GraphX2

        /// <summary>
        /// Gets the graph X2. It is the X2 axis value corresponding to the X2 of the cross point of the crosshairs.
        /// </summary>
        /// <value>The graph X2.</value>
        public double GraphX2
        {
            get { return (double)GetValue(GraphX2Property); }
            internal set { SetValue(GraphX2Property, value); }
        }

        /// <summary>
        /// Identifies the <see cref="GraphX2"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GraphX2Property =
            DependencyProperty.Register("GraphX2", typeof(double), typeof(Crosshairs),
              new PropertyMetadata(0.0));

        #endregion

        #region GraphY2

        /// <summary>
        /// Gets the graph Y2. The Y2 axis value corresponding to the Y2 of the cross point of the crosshairs.
        /// </summary>
        /// <value>The graph Y2.</value>
        public double GraphY2
        {
            get { return (double)GetValue(GraphY2Property); }
            internal set { SetValue(GraphY2Property, value); }
        }

        /// <summary>
        /// Identifies the <see cref="GraphY2"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GraphY2Property =
            DependencyProperty.Register("GraphY2", typeof(double), typeof(Crosshairs),
              new PropertyMetadata(0.0));

        #endregion


        #region PlottingPane
        private PlottingPane _plottingPane;

        internal PlottingPane PlottingPane
        {
            get { return _plottingPane; }
            set { _plottingPane = value; }
        }
        #endregion

        #region Visible
        private bool _visible;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Crosshairs"/> is visible. They are visible if they are enabled and the mouse is over the grid area.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public bool Visible
        {
            get { return _visible && this.Enabled; }
            internal set { _visible = value; }
        }
        #endregion

        #endregion

        #region internal methods
        internal void UpdateByCursor(Point cursorPosition)
        {
            UpdateCrosshairCoordinates(cursorPosition);

            Canvas.SetLeft(HorizontalCrosshair, 0);
            Canvas.SetTop(HorizontalCrosshair, cursorPosition.Y - HorizontalCrosshair.Height / 2);

            Canvas.SetLeft(VerticalCrosshair, cursorPosition.X - VerticalCrosshair.Width / 2);
            Canvas.SetTop(VerticalCrosshair, 0);
        }



        internal void SetupCrosshairSizes()
        {
            if (this.PlottingPane != null)
            {
                this.HorizontalCrosshair.Width = this.PlottingPane.ActualWidth;
                this.HorizontalCrosshair.Height = this.HorizontalCrosshair.Thickness;

                this.VerticalCrosshair.Width = this.VerticalCrosshair.Thickness;
                this.VerticalCrosshair.Height = this.PlottingPane.ActualHeight;
            }
        }

        internal void Show()
        {
            if (!this.Enabled)
            {
                return;
            }

            switch (DisplayMode)
            {
                case CrosshairDisplayMode.Both:
                    ShowCrosshair(HorizontalCrosshair);
                    ShowCrosshair(VerticalCrosshair);
                    break;

                case CrosshairDisplayMode.Horizontal:
                    ShowCrosshair(HorizontalCrosshair);
                    HideCrosshair(VerticalCrosshair);
                    break;
                case CrosshairDisplayMode.Vertical:
                    HideCrosshair(HorizontalCrosshair);
                    ShowCrosshair(VerticalCrosshair);
                    break;
            }
        }
        internal void Hide()
        {
            HideCrosshair(HorizontalCrosshair);
            HideCrosshair(VerticalCrosshair);
        }
        #endregion

        #region private methods
        private void UpdateCrosshairCoordinates(Point cursorPosition)
        {
            this.ScreenX = cursorPosition.X;
            this.ScreenY = cursorPosition.Y;

            if (IsChartRotated())
            {
                CalculateGraphCoordinatesRotated(cursorPosition);
            }
            else
            {
                CalculateGraphCoordinates(cursorPosition);
            }

        }

        private bool IsChartRotated()
        {
            bool rotatedChart = false;

            XamChart chartControl = XamChart.GetControl(this.PlottingPane.Chart);
            if (chartControl.Series != null && chartControl.Series.Count > 0)
            {
                ChartType type = chartControl.Series[0].ChartType;
                if (type == ChartType.Bar || type == ChartType.StackedBar || 
                    type == ChartType.Stacked100Bar || type == ChartType.CylinderBar ||
                    type == ChartType.StackedCylinderBar || type == ChartType.Stacked100CylinderBar)
                {
                    rotatedChart = true;
                }
            }

            return rotatedChart;
        }

        private void CalculateGraphCoordinates(Point cursorPosition)
        {
            XamChart chartControl = XamChart.GetControl(this.PlottingPane.Chart);
            if (this.PlottingPane == null || chartControl == null)
            {
                return;
            }
            this.GraphX = this.PlottingPane.AxisX != null ? this.PlottingPane.AxisX.GetPixelValueLogarithmic(cursorPosition.X) : double.NaN;
            this.GraphY = this.PlottingPane.AxisY != null ? this.PlottingPane.AxisY.GetPixelValueLogarithmic(cursorPosition.Y) : double.NaN;
            this.GraphX2 = this.PlottingPane.AxisX2 != null ? this.PlottingPane.AxisX2.GetPixelValueLogarithmic(cursorPosition.X) : double.NaN;
            this.GraphY2 = this.PlottingPane.AxisY2 != null ? this.PlottingPane.AxisY2.GetPixelValueLogarithmic(cursorPosition.Y) : double.NaN;
        }

        private void CalculateGraphCoordinatesRotated(Point cursorPosition)
        {
            XamChart chartControl = XamChart.GetControl(this.PlottingPane.Chart);
            if (this.PlottingPane == null || chartControl == null)
            {
                return;
            }
            this.GraphY = this.PlottingPane.AxisX != null ? this.PlottingPane.AxisX.GetPixelValueLogarithmic(cursorPosition.Y) : double.NaN;
            this.GraphX = this.PlottingPane.AxisY != null ? this.PlottingPane.AxisY.GetPixelValueLogarithmic(cursorPosition.X) : double.NaN;
            this.GraphY2 = this.PlottingPane.AxisX2 != null ? this.PlottingPane.AxisX2.GetPixelValueLogarithmic(cursorPosition.Y) : double.NaN;
            this.GraphX2 = this.PlottingPane.AxisY2 != null ? this.PlottingPane.AxisY2.GetPixelValueLogarithmic(cursorPosition.X) : double.NaN;
        }

        private void HideCrosshair(CrosshairLine crosshair)
        {
            crosshair.Visibility = Visibility.Collapsed;

            if (PlottingPane != null && PlottingPane.Children != null)
            {
                if (PlottingPane.Children.Contains(crosshair))
                {
                    PlottingPane.Children.Remove(crosshair);
                }
            }
        }

        private void ShowCrosshair(CrosshairLine crosshair)
        {
            crosshair.Visibility = Visibility.Visible;
            if (PlottingPane != null && PlottingPane.Children != null)
            {
                if (!PlottingPane.Children.Contains(crosshair))
                {
                    PlottingPane oldPlottingPane = crosshair.Parent as PlottingPane;
                    if (oldPlottingPane != null && oldPlottingPane.Children.Contains(crosshair))
                    {
                        oldPlottingPane.Children.Remove(crosshair);
                    }
                    PlottingPane.Children.Add(crosshair);
                    Canvas.SetZIndex(crosshair, 100);
                }
            }
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