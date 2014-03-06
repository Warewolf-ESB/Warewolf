using System;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics.Controls.Charts
{
    internal class ChartToolTipManager
    {
        internal Control Owner { get; set; }
        internal object ToolTip { get; set; }
        internal StringFormatter ToolTipFormatter { get; set; }
        internal Popup ToolTipPopup { get; set; }

        public ChartToolTipManager(Control owner)
        {
            Owner = owner;
        }

        internal Action<FrameworkElement> GetChartInfo { get; set; }

        protected virtual void UpdateToolTipDataContext(Popup toolTipPopup, FrameworkElement item)
        {
            if (toolTipPopup != null)
            {
                Brush itemBrush = null;
                if (item is Shape) itemBrush = (item as Shape).Fill;
                if (item is Control) itemBrush = (item as Control).Background;
                toolTipPopup.DataContext = new DataContext { Series = Owner, Item = item.DataContext, ItemBrush = itemBrush};
            }
        }

        protected virtual void UpdateToolTipContent(ContentControl toolTipControl, DataContext dc)
        {
            object tooltip = ToolTip;
            if (tooltip is UIElement && toolTipControl.Content != tooltip)
            {
                toolTipControl.Content = tooltip as UIElement;
            }
            else if (tooltip is string)
            {
                if (ToolTipFormatter != null && dc.Item != null)
                {
                    toolTipControl.Content = ToolTipFormatter.Format(dc.Item, null);
                }
            }
        }

        private void UpdateToolTipVisibility(Popup toolTipPopup, FrameworkElement item)
        {
            if (item.DataContext != null)
            {
                toolTipPopup.Visibility = Visibility.Visible;
            }
            else
            {
                toolTipPopup.Visibility = Visibility.Collapsed;
            }
        }

        private void EnsureTooltipMeasured(ContentControl toolTipControl)
        {
            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            toolTipControl.Measure(availableSize);
        }

        /// <summary>
        /// Updates the tooltip
        /// </summary>
        /// <param name="e">Mouse position</param>
        /// <param name="item">Framework Element that shows the tooltip</param>
        public void UpdateToolTip(MouseEventArgs e, FrameworkElement item)
        {
            if (item == null) return;

            GetChartInfo(item);

            Popup toolTipPopup = ToolTipPopup;
            ContentControl toolTipControl = toolTipPopup != null ? toolTipPopup.Child as ContentControl : null;

            if (toolTipControl != null && toolTipPopup != null)
            {
                DataContext dc = toolTipPopup.DataContext as DataContext;

                if (ToolTip != null)
                {
                    UpdateToolTipDataContext(toolTipPopup, item);
                    //[GT Mar-13-2012] Commenting the following line didn't change bug 61154 behavior. Having an update of the layout here creates issues when there are changes in the visual tree during the measure phase
                    //toolTipPopup.UpdateLayout(); // [DN Jan-12-2011:61154] forcing updatelayout is necessary here in WPF in order to show the datacontext on the screen the 1st time
                    UpdateToolTipContent(toolTipControl, dc);

                    UpdateToolTipVisibility(toolTipPopup, item);

                }
                else
                {
                    dc.Item = null;
                }

                UIElement relativeSource;



                relativeSource = e.Source as UIElement;
                toolTipPopup.PlacementTarget = e.Source as UIElement;
                toolTipPopup.Placement = PlacementMode.Relative;
                // [GT Mar-18-2012] In WPF we should allow transparancy, so the popup style to be able to benefit from it.
                if (SafeSetter.IsSafe) // [DN Mar-20-2012] AllowsTransparency can throw a SecurityException in partial trust.  this is how we check if trust levels are adequate to set that property.
                {
                    toolTipPopup.AllowsTransparency = true;
                }


                bool wasClosed = !toolTipPopup.IsOpen;
                toolTipPopup.IsOpen = ToolTip != null && item != null;

                if (wasClosed && toolTipPopup.IsOpen)
                {
                    EnsureTooltipMeasured(toolTipControl);
                }

                #region prevent tooltip popup from being clipped by the edge of the control.

                double x = e.GetPosition(relativeSource).X + 4;
                double y = e.GetPosition(relativeSource).Y - toolTipControl.DesiredSize.Height - 4;
                double verticalOffset, horizontalOffset;

                try
                {



                    Size totalSize = Application.Current.MainWindow.RenderSize;

                    horizontalOffset = Math.Max(0, x + toolTipControl.DesiredSize.Width - totalSize.Width);
                    verticalOffset = Math.Max(0, y);
                }
                catch
                {
                    horizontalOffset = 0;
                    verticalOffset = y;
                }

                #endregion

                toolTipPopup.HorizontalOffset = x - horizontalOffset;
                toolTipPopup.VerticalOffset = verticalOffset;
            }
        }
    }

    internal class PieChartToolTipManager:ChartToolTipManager
    {
        internal PieChartToolTipManager(Control owner) : base(owner)
        {
        }

        protected override void UpdateToolTipDataContext(Popup toolTipPopup, FrameworkElement item)
        {
            if (toolTipPopup != null)
            {
                Brush itemBrush = null;
                if (item is Shape)
                {
                    itemBrush = (item as Shape).Fill;
                }
                if (item is Control)
                {
                    itemBrush = (item as Control).Background;
                }
                PieChartBase pieChart = this.Owner as PieChartBase;
                Slice slice = item as Slice;
                object itemLabel = pieChart.GetLabel(slice);
                toolTipPopup.DataContext = new PieSliceDataContext 
                { 
                    Series = Owner, 
                    Slice = item, 
                    Item = item.DataContext, 
                    ItemLabel = itemLabel != null ? itemLabel.ToString() : null,
                    ItemBrush = itemBrush,
                    PercentValue = pieChart != null ? pieChart.GetPercentValue(slice) : double.NaN
                };
            }
        }

        protected override void UpdateToolTipContent(ContentControl toolTipControl, DataContext dc)
        {
            base.UpdateToolTipContent(toolTipControl, dc);

            PieSliceDataContext context = dc as PieSliceDataContext;
            if (context != null && dc.Item != null && ToolTipFormatter != null)
            {
                Slice slice = context.Slice as Slice;
                if (slice != null && slice.IsOthersSlice && !(toolTipControl.Content is UIElement))
                {
                    toolTipControl.Content = ToolTipFormatter.Format(dc.Item, null);
                }
            }
        }
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