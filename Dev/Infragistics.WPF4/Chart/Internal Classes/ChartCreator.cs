
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// The ChartCreator is the internal class which is used to create 2D and 3D charts for the chart control.
    /// </summary>
    internal class ChartCreator
    {
        #region Fields

        // private fields
        private Chart _chart;
        private ScenePane _scenePane;
        private Scene _scene;
        private CaptionPane _captionPane;
        private LegendPane _legendPane;
        private AxisValue _axisX;
        private AxisValue _axisY;
        private AxisValue _axisZ;
        private AxisValue _axisX2;
        private AxisValue _axisY2;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the chart control children
        /// </summary>
        internal UIElementCollection Children
        {
            get { return _chart.Children; }
        }

        /// <summary>
        /// Gets the scene
        /// </summary>
        internal Scene Scene
        {
            get { return _scene; }
        }

        /// <summary>
        /// Gets the actual height from the chart control.
        /// </summary>
        internal double ActualHeight
        {
            get { return _chart.ActualHeight; }
        }

        /// <summary>
        /// Gets or sets primary X axis
        /// </summary>
        internal AxisValue AxisX
        {
            get { return _axisX; }
            set { _axisX = value; }
        }

        /// <summary>
        /// Gets or sets primary Y axis
        /// </summary>
        internal AxisValue AxisY
        {
            get { return _axisY; }
            set { _axisY = value; }
        }

        /// <summary>
        /// Gets or sets primary Z axis
        /// </summary>
        internal AxisValue AxisZ
        {
            get { return _axisZ; }
            set { _axisZ = value; }
        }

        /// <summary>
        /// Gets or sets secondary X axis
        /// </summary>
        internal AxisValue AxisX2
        {
            get { return _axisX2; }
            set { _axisX2 = value; }
        }

        /// <summary>
        /// Gets or sets secondary Y axis
        /// </summary>
        internal AxisValue AxisY2
        {
            get { return _axisY2; }
            set { _axisY2 = value; }
        }

        /// <summary>
        /// Gets Legend Pane
        /// </summary>
        internal ScenePane ScenePane
        {
            get { return _scenePane; }
        }

        /// <summary>
        /// Gets Caption Pane
        /// </summary>
        internal CaptionPane CaptionPane
        {
            get { return _captionPane; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="chart">Parent chart</param>
        internal ChartCreator(Chart chart)
        {
            _chart = chart;
        }

        /// <summary>
        /// Creates 2D charts
        /// </summary>
        internal void Create2DChart()
        {
            // Create Caption
            double relativeCaptionHeight = 0;
            if (GetContainer().Caption != null && !String.IsNullOrEmpty(GetContainer().Caption.Text))
            {
                Caption caption = GetContainer().Caption;

                _captionPane = new CaptionPane(caption);
                _captionPane.ChartParent = _chart;

                // this line initializes "somewhat appropriate" size for the captionPane because otherwise the calculated text size does not fit the chart
                _captionPane.Size = new Size(_chart.GetAbsoluteX(_captionPane.RelativePosition.Width), _chart.GetAbsoluteY(_captionPane.RelativePosition.Height));

                Size captionSize = _captionPane.GetTextSize();

                if (this.ActualHeight != 0)
                {
                    relativeCaptionHeight = captionSize.Height / this.ActualHeight * 100.0 + 2;
                }
                else
                {
                    relativeCaptionHeight = 20;
                }

                if (relativeCaptionHeight > 30)
                {
                    relativeCaptionHeight = 30;
                }

                if (GetContainer().Caption.MarginType != MarginType.Auto)
                {
                    Thickness margin = GetContainer().Caption.Margin;
                    ChartCreator.CheckMargin(margin);
                    _captionPane.RelativePosition = new Rect(margin.Left, margin.Top, 100 - margin.Left - margin.Right, 100 - margin.Top - margin.Bottom);
                }
                else
                {
                    _captionPane.RelativePosition = new Rect(0, 0, 100, relativeCaptionHeight);
                }
                _captionPane.Size = new Size(_chart.GetAbsoluteX(_captionPane.RelativePosition.Width), _chart.GetAbsoluteY(_captionPane.RelativePosition.Height));
                _captionPane.Draw();
            }

            // Create Legend
            bool legendEnabled = false;
            bool legendVisible = this.IsLegendVisible();
            if (legendVisible)
            {
                this.CreateLegend(ref legendEnabled, relativeCaptionHeight);
            }
            // Draw Scene
            _scene = GetContainer().Scene;
            if (_scene == null)
            {
                _scene = new Scene();
            }

            // Add the scene to the chart control
            AddVisualChild(_scene);

            if (this._scenePane == null)
            {
                this._scenePane = new ScenePane(_chart, false);
            }
            else
            {
                this.ScenePane.ClearTooltip();
                this.ScenePane.Draw();
            }

            if (_scene.MarginType != MarginType.Auto)
            {
                Thickness margin = _scene.Margin;
                ChartCreator.CheckMargin(margin);
                _scene.RelativePosition = new Rect(margin.Left, margin.Top, 100 - margin.Left - margin.Right, 100 - margin.Top - margin.Bottom);
            }
            else
            {
                double sceneWidth = 70;
                if (!legendEnabled)
                {
                    sceneWidth = 96;
                }
                _scene.RelativePosition = new Rect(2, relativeCaptionHeight, sceneWidth, 100 - relativeCaptionHeight - 2);
            }

            // clean the old scene content
            Scene oldScene = _scenePane.Parent as Scene;
            if (oldScene != null && _scene != oldScene)
            {
                oldScene.Content = null;
            }

            _scene.Content = _scenePane;
            _scene.ScenePane = _scenePane;

            // Add the legend to the chart control
            if (legendVisible)
            {
                AddVisualChild(GetContainer().Legend);
            }

            // Add the caption to the chart control
            if (GetContainer().Caption != null && !String.IsNullOrEmpty(GetContainer().Caption.Text))
            {
                AddVisualChild(_captionPane);
            }
        }

        /// <summary>
        /// Creates 3D Chart
        /// </summary>
        internal void Create3DChart()
        {
            // Create Caption
            double relativeCaptionHeight = 0;
            if (GetContainer().Caption != null && !String.IsNullOrEmpty(GetContainer().Caption.Text))
            {
                Caption caption = GetContainer().Caption;

                _captionPane = new CaptionPane(caption);
                _captionPane.ChartParent = _chart;
                Size captionSize = _captionPane.GetTextSize();

                if (this.ActualHeight != 0)
                {
                    relativeCaptionHeight = captionSize.Height / this.ActualHeight * 100.0 + 2;
                }
                else
                {
                    relativeCaptionHeight = 20;
                }

                if (relativeCaptionHeight > 30)
                {
                    relativeCaptionHeight = 30;
                }

                if (GetContainer().Caption.MarginType != MarginType.Auto)
                {
                    Thickness margin = GetContainer().Caption.Margin;
                    ChartCreator.CheckMargin(margin);
                    _captionPane.RelativePosition = new Rect(margin.Left, margin.Top, 100 - margin.Left - margin.Right, 100 - margin.Top - margin.Bottom);
                }
                else
                {
                    _captionPane.RelativePosition = new Rect(0, 0, 100, relativeCaptionHeight);
                }
                _captionPane.Size = new Size(_chart.GetAbsoluteX(_captionPane.RelativePosition.Width), _chart.GetAbsoluteY(_captionPane.RelativePosition.Height));
                _captionPane.Draw();
            }

            // Create Legend
            bool legendEnabled = false;
            CreateLegend(ref legendEnabled, relativeCaptionHeight);

            // Draw Scene
            _scene = GetContainer().Scene;
            if (_scene == null)
            {
                _scene = new Scene();
            }
            if (this._scenePane != null)
            {
                this._scenePane.ClearTooltip();
            }
            this._scenePane = new ScenePane(_chart, true);

            if (_scene.MarginType != MarginType.Auto)
            {
                Thickness margin = _scene.Margin;
                ChartCreator.CheckMargin(margin);
                _scene.RelativePosition = new Rect(margin.Left, margin.Top, 100 - margin.Left - margin.Right, 100 - margin.Top - margin.Bottom);
            }
            else
            {
                double sceneWidth = 70;
                if (!legendEnabled)
                {
                    sceneWidth = 96;
                }
                // Fix for Bug - BR28963: yAxis get cut off in a 3D chart
                XamChart control = XamChart.GetControl(_chart);
                if (control != null && !double.IsNaN(control.ActualHeight) && !double.IsNaN(control.ActualWidth) && control.ActualWidth > 0 && control.ActualHeight > 0)
                {
                    double optimalWidth = sceneWidth;
                    double diffLeft = 2;
                    if (control.ActualWidth > control.ActualHeight * 1.5)
                    {
                        optimalWidth = control.ActualHeight / (control.ActualWidth / 1.5) * 100;
                        diffLeft = 2 + (sceneWidth - optimalWidth) / 2;
                        if (diffLeft < 0)
                        {
                            diffLeft = 2;
                        }
                        // [DN 8/6/2008] avoid creating a rectangle with right coordinate > 100
                        optimalWidth = Math.Min(100.0 - diffLeft, optimalWidth);
                    }
                    _scene.RelativePosition = new Rect(diffLeft, relativeCaptionHeight, optimalWidth, 100 - relativeCaptionHeight - 2);
                }
                else
                {
                    _scene.RelativePosition = new Rect(2, relativeCaptionHeight, sceneWidth, 100 - relativeCaptionHeight - 2);
                }
            }
            _scene.ScenePane = _scenePane;
            _scene.Content = _scenePane;
            if (!_scenePane.Draw())
            {
                return;
            }

            // Add the scene to the chart control
            AddVisualChild(_scene);

            // Add the legend to the chart control
            if (GetContainer().Legend != null && GetContainer().Legend.Visible)
            {
                AddVisualChild(GetContainer().Legend);
            }

            // Add the caption to the chart control
            if (GetContainer().Caption != null && !String.IsNullOrEmpty(GetContainer().Caption.Text))
            {
                AddVisualChild(_captionPane);
            }
        }

        /// <summary>
        /// Gets the XamChart or the Chart
        /// </summary>
        /// <returns>The IChart Interface</returns>
        private IChart GetContainer()
        {
            return _chart.GetContainer();
        }

        private void CreateLegend(ref bool legendEnabled, double relativeCaptionHeight)
        {
            if (GetContainer().Legend != null && GetContainer().Legend.Visible)
            {
                legendEnabled = true;

                _legendPane = new LegendPane(GetContainer().Legend);
                _legendPane.ChartParent = _chart;

                // Estimate Legend height
                double height = _legendPane.GetNumOfItems() * _legendPane.MaxItemSize;

                double top = 0;

                height = height / _chart.FinalSize.Height * 100.0;
                if (height > 100 - relativeCaptionHeight)
                {
                    height = 100 - relativeCaptionHeight;
                }

                if (double.IsNaN(height))
                {
                    height = 50;
                }

                top = (100 - height) / 2;

                if (GetContainer().Legend.MarginType == MarginType.Auto)
                {
                    _legendPane.RelativePosition = new Rect(75, top, 20, height);
                }
                else
                {
                    Thickness margin = GetContainer().Legend.Margin;
                    CheckMargin(margin);
                    _legendPane.RelativePosition = new Rect(margin.Left, margin.Top, 100 - margin.Left - margin.Right, 100 - margin.Top - margin.Bottom);
                }

                _legendPane.Size = new Size(_chart.GetAbsoluteX(_legendPane.RelativePosition.Width), _chart.GetAbsoluteY(_legendPane.RelativePosition.Height));
                _legendPane.SizeProportion = _legendPane.Size.Width / _legendPane.Size.Height;
                _legendPane.Draw();
                GetContainer().Legend.RelativePosition = _legendPane.RelativePosition;
                GetContainer().Legend.LegendPane = _legendPane;
                GetContainer().Legend.Content = _legendPane;
            }
        }

        private bool IsLegendVisible()
        {
            Legend legend = this.GetContainer().Legend;
            if (legend == null || legend.Visible == false)
            {
                return false;
            }
            if (legend.ReadLocalValue(Legend.VisibleProperty) == DependencyProperty.UnsetValue && (legend.ReadLocalValue(Legend.StyleProperty) == DependencyProperty.UnsetValue || legend.StyleSetFromControl))
            {
                // Take style from visual tree.
                Legend newLegend = new Legend();
                this.Children.Add(newLegend);
                // [DN 12/9/2009:25533] looks like the legend's Visible property could have been set by a style.  let's see if it is visible.
                bool result = newLegend.Visible;
                this.Children.Remove(newLegend);
                return result;
            }
            return legend.Visible;
        }

        internal static void CheckMargin(Thickness margin)
        {
            if (margin.Left < 0 || margin.Left > 100 || margin.Right < 0 || margin.Right > 100 || margin.Top < 0 || margin.Top > 100 || margin.Bottom < 0 || margin.Bottom > 100)
            {
                // Left, Right, Top and Bottom Margin values have to be between 0 and 100 if MarginType is set to Percent.
                throw new InvalidOperationException(ErrorString.Exc51);
            }

            if (100 - margin.Left - margin.Right < 0 || 100 - margin.Top - margin.Bottom < 0)
            {
                // Margin values cause negative Width or Height.
                throw new InvalidOperationException(ErrorString.Exc52);
            }
        }

        internal bool IsSceneType(SceneType type)
        {
            return _scenePane.IsSceneType(type);
        }

        internal void AddVisualChild(UIElement child)
        {
            Chart.AddVisualChild(Children, child);
        }

        internal void RemoveChildren()
        {
            _chart.Children.Remove(_captionPane);
            _chart.Children.Remove(_chart.GetContainer().Legend);
            _chart.Children.Remove(_chart.GetContainer().Scene);
        }

        internal static Size GetTextSize(TextBlock textBlock)
        {
            Typeface typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);

            CultureInfo cultureToUse = CultureInformation.CultureToUse;

            FormattedText formattedText = new FormattedText(textBlock.Text, cultureToUse, FlowDirection.LeftToRight, typeface, textBlock.FontSize, Brushes.Black);
            Size size = new Size(formattedText.Width, formattedText.Height);

            return size;
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