using System;
using System.Net;
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
    internal class SeriesToolTipManager
    {
        private Series Owner { get; set; }



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


        public SeriesToolTipManager(Series owner)
        {
            this.Owner = owner;




        }

        private void UpdateToolTipDataContext(Popup toolTipPopup, Series owner, object item)
        {
            if (toolTipPopup != null)
            {
                toolTipPopup.DataContext = new DataContext() { Series = owner, Item = item };
            }
        }

        private void UpdateToolTipContent(ContentControl toolTipControl, Series owner, DataContext dc)
        {
            if (owner.ToolTip is UIElement && toolTipControl.Content != owner.ToolTip)
            {
                toolTipControl.Content = owner.ToolTip as UIElement;
            }
            else if (owner.ToolTip is string)
            {
                if (owner.ToolTipFormatter != null && dc.Item != null)
                {
                    toolTipControl.Content = owner.ToolTipFormatter.Format(dc, null);
                }
            }
        }

        private void UpdateToolTipVisibility(Popup toolTipPopup, object item)
        {
            if (item != null)
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

        public void UpdateToolTip(Point pt, object item, object data)
        {
            Series owner = Owner;

            //to avoid the tooltip being hijacked by the stacked series.
            if (Owner is StackedSeriesBase)
            {
                //The only time we still have to show the tooltip for StackedSeriesBase is when
                //it's a column/bar series and the mouse is over the marker. 
                //Stacked fragments push their markers to the parent's frame.
                
                MouseEventArgs args = data as MouseEventArgs;

                if (args == null || args.OriginalSource is Rectangle)
                {
                    return;
                }

                FrameworkElement contextElement = args.OriginalSource as FrameworkElement;

                if (contextElement == null)
                {
                    return;
                }

                DataContext context = contextElement.DataContext as DataContext;

                if (context == null || !(context.Series is Series))
                {
                    return;
                }

                owner = context.Series as Series;
            }

            Popup toolTipPopup = owner.SeriesViewer != null ? owner.SeriesViewer.ToolTipPopup : null;
            ContentControl toolTipControl = toolTipPopup != null ? toolTipPopup.Child as ContentControl : null;

            if (toolTipControl != null && toolTipPopup != null)
            {
                DataContext dc = toolTipPopup.DataContext as DataContext;

                if (owner.ToolTip != null)
                {
                    UpdateToolTipDataContext(toolTipPopup, owner, item);
                    toolTipPopup.UpdateLayout(); // [DN Jan-12-2011:61154] forcing updatelayout is necessary here in WPF in order to show the datacontext on the screen the 1st time
                    UpdateToolTipContent(toolTipControl, owner, dc);
                    UpdateToolTipVisibility(toolTipPopup, item);
                }
                else
                {
                    dc.Item = null;
                }

                bool wasClosed = !toolTipPopup.IsOpen;
                bool chartInDragOperation = owner.SeriesViewer.IsInDragOperation;
                toolTipPopup.IsOpen = owner.ToolTip != null && item != null && !chartInDragOperation;

                if (wasClosed && toolTipPopup.IsOpen)
                {
                    EnsureTooltipMeasured(toolTipControl);
                }

                UIElement relativeSource;




                MouseEventArgs e = (MouseEventArgs)data;
				relativeSource = e.Source as UIElement;
				toolTipPopup.PlacementTarget = e.Source as UIElement;
				toolTipPopup.Placement = PlacementMode.Relative;


                #region prevent tooltip popup from being clipped by the edge of the control.
                Point mouseLocation = e.GetPosition(relativeSource);
                double x = mouseLocation.X + 4;
                double y = mouseLocation.Y - toolTipControl.DesiredSize.Height - 4;
                double verticalOffset, horizontalOffset;

                try
                {



                    Window parentWindow = Window.GetWindow(e.Source as DependencyObject) ?? Application.Current.MainWindow;
                    Size totalSize = parentWindow.RenderSize;


                    if (mouseLocation.Y < toolTipControl.DesiredSize.Height 
                        && mouseLocation.X > totalSize.Width - toolTipControl.DesiredSize.Width)
                    {
                        y = mouseLocation.Y + 10;
                    }

                    horizontalOffset = Math.Max(0, x + toolTipControl.DesiredSize.Width - totalSize.Width);
                    verticalOffset = Math.Max(0, y);
                }
                catch
                {
                    horizontalOffset = 0;
                    verticalOffset = y;
                }

                #endregion prevent tooltip popup from being clipped by the edge of the control.

                toolTipPopup.HorizontalOffset = x - horizontalOffset;
                toolTipPopup.VerticalOffset = verticalOffset;
            }
        }



#region Infragistics Source Cleanup (Region)




























































#endregion // Infragistics Source Cleanup (Region)

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