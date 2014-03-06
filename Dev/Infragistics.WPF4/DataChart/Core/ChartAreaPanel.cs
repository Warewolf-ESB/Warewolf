using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a specialized panel used to layout the contents of Charts.
    /// </summary>
    /// <remarks>
    /// The ChartAreaPanel is used internally by the chart to handle the rather
    /// complicated layout of its central area, which requires collaboration between the owner
    /// chart and the other chart objects.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ChartAreaPanel : Panel
    {
        /// <summary>
        /// Initializes an empty, default ChartAreaPanel
        /// </summary>
        /// <param name="chart">The owner chart object.</param>
        internal ChartAreaPanel(SeriesViewer chart)
        {
            if (chart == null)
            {
                throw new ArgumentException(GetType().Name + " must be created with an owner " + typeof(XamDataChart).Name);
            }

            SeriesViewer = chart;
        }

        /// <summary>
        /// Provides the behavior for the Measure pass of Silverlight layout. Classes can override this method to define their own Measure pass behavior.
        /// </summary>
        /// <param name="availableSize">The available size that this object can give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of the allocated sizes for child objects; or based on other considerations, such as a fixed container size.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            //start out with a minimum desired size for the plot area.
            Size desiredSize = new Size(SeriesViewer.PlotAreaMinWidth, SeriesViewer.PlotAreaMinHeight);

            desiredSize = GetDesiredSize(availableSize, desiredSize);
            var nonPlotDesiredSize = desiredSize;
            UIElement opdEle = null;
            foreach (UIElement element in Children)
            {
                if (element is XamOverviewPlusDetailPane)
                {
                    opdEle = element;
                }
            }
            if (opdEle != null)
            {
                if (opdEle is XamOverviewPlusDetailPane)
                {
                    desiredSize = new Size(
                        opdEle.DesiredSize.Width > desiredSize.Width ? opdEle.DesiredSize.Width : desiredSize.Width,
                        opdEle.DesiredSize.Height > desiredSize.Height ? opdEle.DesiredSize.Height : desiredSize.Height
                        );
                }
            }

            if (SeriesViewer.ChartContentManager.FirstMeasure
                && !double.IsInfinity(availableSize.Width)
                && !double.IsInfinity(availableSize.Height)
                && SeriesViewer.Series.Count > 0)
            {
                SeriesViewer.ChartContentManager.FirstMeasure = false;

                Size nonPlotSize = new Size(
                    nonPlotDesiredSize.Width - SeriesViewer.PlotAreaMinWidth,
                    nonPlotDesiredSize.Height - SeriesViewer.PlotAreaMinHeight);

                double availablePlotWidth = Math.Max(availableSize.Width - nonPlotSize.Width, 0);
                double availablePlotHeight = Math.Max(availableSize.Height - nonPlotSize.Height, 0);
                double plotWidth = SeriesViewer.PlotAreaMinWidth;
                double plotHeight = SeriesViewer.PlotAreaMinHeight;

                if (SeriesViewer.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    plotHeight = Math.Max(plotHeight, availablePlotHeight);
                }
                if (SeriesViewer.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    plotWidth = Math.Max(plotWidth, availablePlotWidth);
                }
                if (SeriesViewer.EffectiveIsSquare())
                {
                    if (plotHeight > plotWidth)
                    {
                        plotHeight = plotWidth;
                    }
                    else if (plotWidth > plotHeight)
                    {
                        plotWidth = plotHeight;
                    }
                }

                Size plotSize = new Size(plotWidth, plotHeight);
                Border b = Children.OfType<Border>()
                    .Where((c) => VisualInformationManager.GetIsMainGeometryVisual(c))
                    .FirstOrDefault();
                if (b != null)
                {
                    b.Measure(new Size(plotWidth, plotHeight));
                }
                SeriesViewer.ChartContentManager.EnsureAxesRendered(plotSize);

                desiredSize = new Size(SeriesViewer.PlotAreaMinWidth, SeriesViewer.PlotAreaMinHeight);
                desiredSize = GetDesiredSize(availableSize, desiredSize);
            }

            if (double.IsInfinity(desiredSize.Width))
            {
                desiredSize.Width = SeriesViewer.MinWidth;
                desiredSize.Height = SeriesViewer.MinHeight;
            }

            return desiredSize;
        }

        private Size GetDesiredSize(Size availableSize, Size desiredSize)
        {
            foreach (UIElement element in Children)
            {



                if (element is XamZoombar)
                {
                    
                    //space than it was told it could have.
                    element.Measure(
                        new Size(double.PositiveInfinity, double.PositiveInfinity));
                }
                else
                {
                    element.Measure(availableSize);
                }

                desiredSize = AccomodateElement(element, desiredSize);
            }

            return desiredSize;
        }

        private Size AccomodateElement(UIElement element, Size currentDesiredSize)
        {

            if (element == SeriesViewer.HorizontalZoombar)
            {
                return new Size(currentDesiredSize.Width,
                    currentDesiredSize.Height + element.DesiredSize.Height);
            }

            if (element == SeriesViewer.VerticalZoombar)
            {
                return new Size(currentDesiredSize.Width + element.DesiredSize.Width,
                    currentDesiredSize.Height);
            }

            if (element is AxisLabelPanelBase)
            {
                var labelSettings = (element as AxisLabelPanelBase).Axis.LabelSettings;
                bool takesSpace = (labelSettings == null || labelSettings.Visibility != System.Windows.Visibility.Collapsed);
                if (takesSpace)
                {
                    AxisLabelsLocation labelLocation = LabelPanelArranger.ResolveLabelLocation(element as AxisLabelPanelBase);
                    switch (labelLocation)
                    {
                        case AxisLabelsLocation.InsideLeft:
                        case AxisLabelsLocation.InsideRight:
                            break;
                        case AxisLabelsLocation.OutsideRight:
                            return new Size(currentDesiredSize.Width + element.DesiredSize.Width, currentDesiredSize.Height);
                        case AxisLabelsLocation.OutsideLeft:
                            return new Size(currentDesiredSize.Width + element.DesiredSize.Width, currentDesiredSize.Height);

                        case AxisLabelsLocation.InsideTop:
                        case AxisLabelsLocation.InsideBottom:
                            break;
                        case AxisLabelsLocation.OutsideTop:
                            return new Size(currentDesiredSize.Width, currentDesiredSize.Height + element.DesiredSize.Height);
                        case AxisLabelsLocation.OutsideBottom:
                        default:
                            return new Size(currentDesiredSize.Width, currentDesiredSize.Height + element.DesiredSize.Height);
                    }
                }
            }
        
            return currentDesiredSize;
        }

        /// <summary>
        /// Positions child elements and determines a size for this element.
        /// </summary>
        /// <param name="finalSize">The size available to this element for arranging its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect gridAreaRect = new Rect(new Point(0, 0), finalSize);
            List<AxisLabelPanelBase> labelPanels = new List<AxisLabelPanelBase>();

            foreach (UIElement element in Children)
            {

                if (element == SeriesViewer.HorizontalZoombar)
                {
                    gridAreaRect.Height = Math.Max(0, gridAreaRect.Height - element.DesiredSize.Height);
                }

                if (element == SeriesViewer.VerticalZoombar)
                {
                    gridAreaRect.Width = Math.Max(0, gridAreaRect.Width - element.DesiredSize.Width);
                }

                AxisLabelPanelBase labelPanel = element as AxisLabelPanelBase;
                if (labelPanel != null && labelPanel.Axis != null && (labelPanel.Axis.LabelSettings == null || labelPanel.Axis.LabelSettings.Visibility == Visibility.Visible))
                {
                    labelPanels.Add(labelPanel);
                    ResetPanelLocation(labelPanel);
                }
                else if (labelPanel != null)
                {
                    //hide the panels if they arent visible.
                    labelPanel.Arrange(new Rect(0, 0, 0, 0));
                }
            }

            gridAreaRect = LabelPanelArranger.PreparePanels(labelPanels,gridAreaRect);

            // place everything
            if (SeriesViewer.EffectiveIsSquare() && gridAreaRect.Width != gridAreaRect.Height)
            {
                if (gridAreaRect.Width < gridAreaRect.Height)
                {
                    finalSize.Height -= (gridAreaRect.Height - gridAreaRect.Width);
                    gridAreaRect.Height = gridAreaRect.Width;
                }
                else
                {
                    finalSize.Width -= (gridAreaRect.Width - gridAreaRect.Height);
                    gridAreaRect.Width = gridAreaRect.Height;
                }
            }

            LabelPanelsArrangeState arrangeState = new LabelPanelsArrangeState()
            {
                Left = 0.0,
                InsideLeft = 0.0,
                Bottom = finalSize.Height,
                InsideBottom = finalSize.Height,
                Right = finalSize.Width,
                InsideRight = finalSize.Width,
                Top = 0.0,
                InsideTop = 0.0,
            };


            if (SeriesViewer.HorizontalZoombar != null)
            {
                UIElement element = SeriesViewer.HorizontalZoombar;
                arrangeState.Bottom -= element.DesiredSize.Height;
                arrangeState.InsideBottom -= element.DesiredSize.Height;
                element.Arrange(new Rect(gridAreaRect.Left, arrangeState.Bottom, gridAreaRect.Width, element.DesiredSize.Height));
            }

            if (SeriesViewer.VerticalZoombar != null)
            {
                UIElement element = SeriesViewer.VerticalZoombar;
                arrangeState.Right -= element.DesiredSize.Width;
                arrangeState.InsideRight -= element.DesiredSize.Width;
                element.Arrange(new Rect(arrangeState.Right, gridAreaRect.Top, element.DesiredSize.Width, gridAreaRect.Height));
            }


            if (SeriesViewer.PlotAreaBorder != null)
            {
                SeriesViewer.PlotAreaBorder.Arrange(gridAreaRect);
            }

            if (SeriesViewer.OverviewPlusDetailPane != null)
            {
                FrameworkElement element = SeriesViewer.OverviewPlusDetailPane;

                double left = 0;
                double top = 0;

                switch (element.HorizontalAlignment)
                {
                    case System.Windows.HorizontalAlignment.Center:                        
                    case System.Windows.HorizontalAlignment.Stretch:
                        left = (gridAreaRect.Right - element.DesiredSize.Width) / 2;
                        break;
                    case System.Windows.HorizontalAlignment.Right:
                        left = gridAreaRect.Right - element.DesiredSize.Width;
                        break;
                }

                switch (element.VerticalAlignment)
                {
                    case System.Windows.VerticalAlignment.Center:
                    case System.Windows.VerticalAlignment.Stretch:
                        top = (gridAreaRect.Bottom - element.DesiredSize.Height) / 2;
                        break;
                    case System.Windows.VerticalAlignment.Bottom:
                        top = gridAreaRect.Bottom - element.DesiredSize.Height;
                        break;
                }

                element.Arrange(new Rect(left, top, element.DesiredSize.Width, element.DesiredSize.Height));
            }


            LabelPanelArranger.ArrangePanels(labelPanels, arrangeState, gridAreaRect, (p, b) => p.Arrange(b));

            return finalSize;
        }

        private void ResetPanelLocation(AxisLabelPanelBase labelPanel)
        {
            if (labelPanel.Axis != null && labelPanel.Axis.LabelSettings != null
                && labelPanel.Axis.LabelSettings.ReadLocalValue(AxisLabelSettings.LocationProperty) !=
                DependencyProperty.UnsetValue)
            {
                labelPanel.Axis.LabelSettings.ActualLocation = labelPanel.Axis.LabelSettings.Location;
            }
        }
       
        /// <summary>
        /// Gets the owner Chart for the current ChartAreaPanel object. 
        /// </summary>
        public SeriesViewer SeriesViewer { get; private set; }
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