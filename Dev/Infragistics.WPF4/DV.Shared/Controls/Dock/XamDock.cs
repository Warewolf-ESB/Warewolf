using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using Infragistics.AutomationPeers;


using System;


namespace Infragistics.Controls
{
    /// <summary>
    /// Defines an area within which child controls are placed relative to a central content.
    /// </summary>

    
    


    public class XamDock : Panel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XamDock"/> class.
        /// </summary>
        public XamDock()
            : base()
        {

			Infragistics.Windows.Utilities.ValidateLicense(typeof(XamDock), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion Constructor

        #region Properties

        #region LayoutPriority

        /// <summary>
        /// Identifies the <see cref="LayoutPriority"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LayoutPriorityProperty = DependencyProperty.Register("LayoutPriority", typeof(LayoutPriority), typeof(XamDock), new PropertyMetadata(LayoutPriority.None, null));

        /// <summary>
        /// Gets or sets the layout priority.
        /// </summary>
        /// <value>The layout priority.</value>
        public LayoutPriority LayoutPriority
        {
            get { return (LayoutPriority)this.GetValue(LayoutPriorityProperty); }
            set { this.SetValue(LayoutPriorityProperty, value); }
        }

        #endregion LayoutPriority

        #region Edge Attached Dependency Property

        /// <summary>
        /// Sets the values of the Edge attached property for a given dependency object.
        /// </summary>
        /// <param name="d">The Element to which the property value is written</param>
        /// <param name="dockEdge">The edge for the specified Element</param>
        public static void SetEdge(DependencyObject d, DockEdge dockEdge)
        {
            d.SetValue(EdgeProperty, dockEdge);
        }

        /// <summary>
        /// Gets the value of the Edge attached property for a given dependency object.
        /// </summary>
        /// <param name="d">The Element from which the property value is read.</param>
        /// <returns>The edge for the specified Element.</returns>
        public static DockEdge GetEdge(DependencyObject d)
        {
            return (DockEdge)(d.GetValue(EdgeProperty));
        }

        /// <summary>
        /// Identifies the Edge attached DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty EdgeProperty = DependencyProperty.RegisterAttached("Edge", typeof(DockEdge), typeof(XamDock), new PropertyMetadata(DockEdge.Central, XamDock.HandlePropertyChanged));

        #endregion Edge Attached Dependency Property

        private static void HandlePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            XamDock dock = VisualTreeHelper.GetParent(sender) as XamDock;
            if (dock != null)
            {
                dock.InvalidateMeasure();
            }
        }

        /// <summary>
        /// Identifies the HorizontalDockAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalDockAlignmentProperty = DependencyProperty.RegisterAttached("HorizontalDockAlignment", typeof(HorizontalAlignment), typeof(XamDock), new PropertyMetadata(XamDock.HandlePropertyChanged));
        /// <summary>
        /// Identifies the VerticalDockAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalDockAlignmentProperty = DependencyProperty.RegisterAttached("VerticalDockAlignment", typeof(VerticalAlignment), typeof(XamDock), new PropertyMetadata(XamDock.HandlePropertyChanged));

        #endregion Properties

        #region Enums

        private enum DockDirection
        {
            Horizontal,
            Vertical
        }

        #endregion Enums

        #region Overrides

        private void MeasureToFit(UIElement element, Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            if (XamDock.GetEdge(element) == DockEdge.Central)
            {
                element.Measure(availableSize);
                Size measuredAtAvailable = element.DesiredSize;
                element.Measure(infiniteSize);
                Size measuredAtInfinite = element.DesiredSize;

                // [DN 7/24/2009:19953] just in case the element measured at availableSize is larger than the element measured at infinite... this is true for some elements which use a hard coded default height or width because they can't deal with infinite measurements.
                Size sizeToUse;
                if (measuredAtAvailable.Height >= measuredAtInfinite.Height && measuredAtAvailable.Width >= measuredAtInfinite.Width && measuredAtAvailable.Width <= availableSize.Width && measuredAtAvailable.Height <= availableSize.Height)
                {
                    sizeToUse = availableSize;
                }
                else
                {
                    sizeToUse = infiniteSize;
                }
                element.Measure(sizeToUse);
            }
            else
            {
                element.Measure(infiniteSize);
            }

            if (element.DesiredSize.Width > availableSize.Width && element.DesiredSize.Height < availableSize.Height)
            {
                // [DN 12/31/2008:11962] this thing is too wide.  let's try measuring it with a limited width.
                element.Measure(new Size(availableSize.Width, double.PositiveInfinity));
                if (element.DesiredSize.Width > availableSize.Width || element.DesiredSize.Height > availableSize.Height)
                {
                    // that didn't work.  revert to unconstrained measurement
                    element.Measure(infiniteSize);
                }
            }
            else if (element.DesiredSize.Height > availableSize.Height && element.DesiredSize.Width < availableSize.Width)
            {
                // this thing is too tall.  let's try measuring it with a limited height.
                element.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                if (element.DesiredSize.Width > availableSize.Width || element.DesiredSize.Height > availableSize.Height)
                {
                    // that didn't work.  revert to unconstrained measurement
                    element.Measure(infiniteSize);
                }
            }
            else if (element.DesiredSize.Height > availableSize.Height && element.DesiredSize.Width > availableSize.Width)
            {
                // [DN 12/28/2009:24342] this thing is too wide and too tall.  measure using only availableSize.
                element.Measure(availableSize);
                if (element.DesiredSize.Width > availableSize.Width || element.DesiredSize.Height > availableSize.Height)
                {
                    // that didn't work.  revert to unconstrained measurement
                    element.Measure(infiniteSize);
                }
            }
        }

        #region MeasureOverride

        /// <summary>
        /// Provides the behavior for the Measure pass of Silverlight layout. Classes can override this method to define their own Measure pass behavior.
        /// </summary>
        /// <param name="availableSize">The available size that this object can give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of the allocated sizes for child objects; or based on other considerations, such as a fixed container size.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

            List<UIElement>[] children = ByEdge(Children);
            double[,] width = new double[4, 4];
            double[,] height = new double[4, 4];
            double[] widthedge = new double[4];
            double[] heightedge = new double[4];
            // outside elements
            for (int i = 0; i < 4; i++)
            {
                foreach (UIElement elem in children[i])
                {
                    this.MeasureToFit(elem, availableSize);
                    int alignment = -1;

                    if (i % 2 == 0)
                    {
                        alignment = (int)XamDock.GetVerticalDockAlignment(elem);
                        height[i, alignment] += elem.DesiredSize.Height;

                        alignment = (int)XamDock.GetHorizontalDockAlignment(elem);
                        width[i, alignment] = System.Math.Max(width[i, alignment], elem.DesiredSize.Width);
                    }
                    else
                    {
                        alignment = (int)XamDock.GetVerticalDockAlignment(elem);
                        height[i, alignment] = System.Math.Max(height[i, alignment], elem.DesiredSize.Height);

                        alignment = (int)XamDock.GetHorizontalDockAlignment(elem);
                        width[i, alignment] += elem.DesiredSize.Width;
                    }
                }

                if (i % 2 == 1)
                {
                    heightedge[i] = System.Math.Max(System.Math.Max(height[i, (int)VerticalAlignment.Top], height[i, (int)VerticalAlignment.Center]), height[i, (int)VerticalAlignment.Bottom] + height[i, (int)VerticalAlignment.Stretch]);
                    widthedge[i] = width[i, (int)HorizontalAlignment.Left] + width[i, (int)HorizontalAlignment.Center] + width[i, (int)HorizontalAlignment.Right];

                    if (width[i, (int)HorizontalAlignment.Center] > 0)
                    {
                        widthedge[i] = System.Math.Max(width[i, (int)HorizontalAlignment.Left] * 2 + width[i, (int)HorizontalAlignment.Center], width[i, (int)HorizontalAlignment.Right] * 2 + width[i, (int)HorizontalAlignment.Center]);
                    }
                }
                else
                {
                    widthedge[i] = System.Math.Max(System.Math.Max(width[i, (int)HorizontalAlignment.Left], width[i, (int)HorizontalAlignment.Center]), width[i, (int)HorizontalAlignment.Right]);
                    heightedge[i] = height[i, (int)VerticalAlignment.Top] + height[i, (int)VerticalAlignment.Center] + height[i, (int)VerticalAlignment.Bottom] + height[i, (int)VerticalAlignment.Stretch];

                    if (height[i, (int)VerticalAlignment.Center] > 0)
                    {
                        heightedge[i] = System.Math.Max(height[i, (int)VerticalAlignment.Top] * 2 + height[i, (int)VerticalAlignment.Center], height[i, (int)VerticalAlignment.Bottom] * 2 + height[i, (int)VerticalAlignment.Center]);
                    }
                }
            }

            if (this.LayoutPriority == LayoutPriority.TopAndBottom)
            {
                double topAndBottomEdgeHeight = heightedge[(int)DockEdge.OutsideBottom] + heightedge[(int)DockEdge.OutsideTop];
                heightedge[(int)DockEdge.OutsideLeft] += topAndBottomEdgeHeight;
                heightedge[(int)DockEdge.OutsideRight] += topAndBottomEdgeHeight;
            }
            else if (this.LayoutPriority == LayoutPriority.LeftAndRight)
            {
                double leftAndRightEdgeWidth = widthedge[(int)DockEdge.OutsideLeft] + widthedge[(int)DockEdge.OutsideRight];
                widthedge[(int)DockEdge.OutsideBottom] += leftAndRightEdgeWidth;
                widthedge[(int)DockEdge.OutsideTop] += leftAndRightEdgeWidth;
            }

            Size sizeOfCentralArea = new Size(System.Math.Max(0.0, availableSize.Width - widthedge[(int)DockEdge.OutsideLeft] - widthedge[(int)DockEdge.OutsideRight]), System.Math.Max(0.0, availableSize.Height - heightedge[(int)DockEdge.OutsideTop] - heightedge[(int)DockEdge.OutsideBottom]));
            // inside elements
            for (int i = 4; i < 8; i++)
            {
                foreach (UIElement elem in children[i])
                {
                    this.MeasureToFit(elem, sizeOfCentralArea);
                }
            }
            Size currentSize = new Size(); // = System.Windows.Size.Empty;
            foreach (UIElement elem in children[(int)DockEdge.Central])
            {
                this.MeasureToFit(elem, sizeOfCentralArea);

                currentSize.Height = System.Math.Max(System.Math.Max(System.Math.Max(
                                                           heightedge[(int)DockEdge.OutsideLeft],
                                                           heightedge[(int)DockEdge.OutsideRight]),
                                                           heightedge[(int)DockEdge.OutsideBottom] + heightedge[(int)DockEdge.OutsideTop] + elem.DesiredSize.Height),
                                                           currentSize.Height);

                currentSize.Width = System.Math.Max(System.Math.Max(System.Math.Max(
                                                           widthedge[(int)DockEdge.OutsideBottom],
                                                           widthedge[(int)DockEdge.OutsideTop]),
                                                           widthedge[(int)DockEdge.OutsideLeft] + widthedge[(int)DockEdge.OutsideRight] + elem.DesiredSize.Width),
                                                           currentSize.Width);
            }
            return currentSize;
        }

        #endregion MeasureOverride

        #region ArrangeOverride

        /// <summary>
        /// Positions child elements and determines a size for this element.
        /// </summary>
        /// <param name="finalSize">The size available to this element for arranging its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            List<UIElement>[] children = this.ByEdge(this.Children);
            Rect rc = new Rect(0, 0, finalSize.Width, finalSize.Height);

            Rect centralarea = this.Arrange(rc, children[(int)DockEdge.OutsideLeft], children[(int)DockEdge.OutsideBottom], children[(int)DockEdge.OutsideRight], children[(int)DockEdge.OutsideTop]);
            Rect actualcentralarea = new Rect(double.MaxValue, double.MaxValue, 0, 0);
            if (children[(int)DockEdge.Central].Count == 0)
            {
                actualcentralarea = centralarea;
            }
            else
            {
                foreach (UIElement child in children[(int)DockEdge.Central])
                {
                    child.Arrange(centralarea);
                    if (child is FrameworkElement)
                    {
                        actualcentralarea = new Rect(
                            System.Math.Min((centralarea.Width - ((FrameworkElement)child).ActualWidth) / 2.0, actualcentralarea.X),
                            System.Math.Min((centralarea.Height - ((FrameworkElement)child).ActualHeight) / 2.0, actualcentralarea.Y),
                            System.Math.Max(((FrameworkElement)child).ActualWidth, actualcentralarea.Width),
                            System.Math.Max(((FrameworkElement)child).ActualHeight, actualcentralarea.Height));
                    }
                }
                actualcentralarea = new Rect(System.Math.Max(actualcentralarea.X, 0.0) + centralarea.X, System.Math.Max(actualcentralarea.Y, 0.0) + centralarea.Y, System.Math.Min(actualcentralarea.Width, centralarea.Width), System.Math.Min(actualcentralarea.Height, centralarea.Height));
            }
            this.Arrange(actualcentralarea, children[(int)DockEdge.InsideLeft], children[(int)DockEdge.InsideBottom], children[(int)DockEdge.InsideRight], children[(int)DockEdge.InsideTop]);

            return finalSize;
        }

        #endregion ArrangeOverride

        #region OnCreateAutomationPeer

        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new XamDockAutomationPeer(this);
        }

        #endregion OnCreateAutomationPeer

        #endregion Overrides

        #region Methods

        private Rect Arrange(Rect bounds, List<UIElement> leftElements, List<UIElement> bottomElements, List<UIElement> rightElements, List<UIElement> topElements)
        {
            Size leftSize = this.Size(leftElements, bounds, DockDirection.Vertical);
            Size bottomSize = this.Size(bottomElements, bounds, DockDirection.Horizontal);
            Size rightSize = this.Size(rightElements, bounds, DockDirection.Vertical);
            Size topSize = this.Size(topElements, bounds, DockDirection.Horizontal);

            if (this.LayoutPriority == LayoutPriority.None)
            {
                this.ArrangeEdge(leftElements, new Rect(bounds.Left, bounds.Top, leftSize.Width, bounds.Height), DockDirection.Vertical);
                this.ArrangeEdge(bottomElements, new Rect(bounds.Left, bounds.Bottom - bottomSize.Height, bounds.Width, bottomSize.Height), DockDirection.Horizontal);
                this.ArrangeEdge(rightElements, new Rect(bounds.Right - rightSize.Width, bounds.Top, rightSize.Width, bounds.Height), DockDirection.Vertical);
                this.ArrangeEdge(topElements, new Rect(bounds.Left, bounds.Top, bounds.Width, topSize.Height), DockDirection.Horizontal);
            }
            else if (this.LayoutPriority == LayoutPriority.LeftAndRight)
            {
                this.ArrangeEdge(rightElements, new Rect(bounds.Right - rightSize.Width, bounds.Top, rightSize.Width, bounds.Height), DockDirection.Vertical);
                this.ArrangeEdge(leftElements, new Rect(bounds.Left, bounds.Top, leftSize.Width, bounds.Height), DockDirection.Vertical);
                this.ArrangeEdge(bottomElements, new Rect(bounds.Left + leftSize.Width, bounds.Bottom - bottomSize.Height, System.Math.Max(0, bounds.Width - leftSize.Width - rightSize.Width), bottomSize.Height), DockDirection.Horizontal);
                this.ArrangeEdge(topElements, new Rect(bounds.Left + leftSize.Width, bounds.Top, System.Math.Max(0, bounds.Width - leftSize.Width - rightSize.Width), topSize.Height), DockDirection.Horizontal);
            }
            else
            {
                this.ArrangeEdge(topElements, new Rect(bounds.Left, bounds.Top, bounds.Width, topSize.Height), DockDirection.Horizontal);
                this.ArrangeEdge(bottomElements, new Rect(bounds.Left, bounds.Bottom - bottomSize.Height, bounds.Width, bottomSize.Height), DockDirection.Horizontal);
                this.ArrangeEdge(rightElements, new Rect(bounds.Right - rightSize.Width, bounds.Top + topSize.Height, rightSize.Width, System.Math.Max(0, bounds.Height - topSize.Height - bottomSize.Height)), DockDirection.Vertical);
                this.ArrangeEdge(leftElements, new Rect(bounds.Left, bounds.Top + topSize.Height, leftSize.Width, System.Math.Max(0, bounds.Height - topSize.Height - bottomSize.Height)), DockDirection.Vertical);
            }

            return new Rect(bounds.Left + leftSize.Width, bounds.Top + topSize.Height, System.Math.Max(0, bounds.Width - (leftSize.Width + rightSize.Width)), System.Math.Max(0, bounds.Height - (topSize.Height + bottomSize.Height)));
        }

        #region Private

        private void ArrangeEdge(List<UIElement> elements, Rect bounds, DockDirection edge)
        {
            List<UIElement>[] byPosition = this.ByPosition(elements, edge);
            Size[] sz = new Size[byPosition.Length];

            for (int i = 0; i < byPosition.Length; ++i)
            {
                sz[i] = Size(byPosition[i], bounds, edge);
            }

            if (edge == DockDirection.Horizontal)
            {
                this.Arrange(byPosition[0], new Rect(bounds.Left, bounds.Top, sz[0].Width, bounds.Height), edge, false);
                this.Arrange(byPosition[1], new Rect(bounds.Left + 0.5 * (bounds.Width - sz[1].Width), bounds.Top, sz[1].Width, bounds.Height), edge, false);
                this.Arrange(byPosition[2], new Rect(bounds.Left + (bounds.Width - sz[2].Width), bounds.Top, sz[2].Width, bounds.Height), edge, false);
                this.Arrange(byPosition[3], new Rect(bounds.Left + sz[0].Width, bounds.Top, System.Math.Max(0, bounds.Width - sz[0].Width - sz[2].Width), bounds.Height), edge, true);
            }
            else
            {
                this.Arrange(byPosition[0], new Rect(bounds.Left, bounds.Top, bounds.Width, sz[0].Height), edge, false);
                this.Arrange(byPosition[1], new Rect(bounds.Left, bounds.Top + 0.5 * (bounds.Height - sz[1].Height), bounds.Width, sz[1].Height), edge, false);
                this.Arrange(byPosition[2], new Rect(bounds.Left, bounds.Top + (bounds.Height - sz[2].Height), bounds.Width, sz[2].Height), edge, false);
                this.Arrange(byPosition[3], new Rect(bounds.Left, bounds.Top + sz[0].Width, bounds.Width, System.Math.Max(0, bounds.Height - sz[0].Width - sz[2].Width)), edge, true);
            }
        }

        private Size Size(List<UIElement> elements, Rect bounds, DockDirection direction)
        {
            Size sz = new Size();

            foreach (UIElement element in elements)
            {
                double width = element.DesiredSize.Width;
                double height = element.DesiredSize.Height;

                if (direction == DockDirection.Vertical)
                {
                    sz.Height += height;
                    sz.Width = System.Math.Max(sz.Width, width);
                }
                else
                {
                    sz.Width += width;
                    sz.Height = System.Math.Max(sz.Height, height);
                }
            }

            sz.Width = System.Math.Min(sz.Width, bounds.Width);
            sz.Height = System.Math.Min(sz.Height, bounds.Height);
            return sz;
        }

        private void Arrange(List<UIElement> elements, Rect bounds, DockDirection direction, bool isStrech)
        {
            Rect rc = new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
            if (direction == DockDirection.Horizontal)
            {
                if (isStrech)
                {
                    rc.Width = rc.Width / elements.Count;
                    foreach (UIElement element in elements)
                    {
                        element.Arrange(rc);
                        rc.X += rc.Width;
                    }
                }
                else
                {
                    foreach (UIElement element in elements)
                    {
                        rc.Width = element.DesiredSize.Width;
                        rc.Width = System.Math.Min(rc.Width, bounds.Width);
                        element.Arrange(rc);
                        rc.X += rc.Width;
                    }
                }
            }
            else
            {
                if (isStrech)
                {
                    rc.Height = rc.Height / elements.Count;
                    foreach (UIElement element in elements)
                    {
                        element.Arrange(rc);
                        rc.Y += rc.Height;
                    }
                }
                else
                {
                    foreach (UIElement element in elements)
                    {
                        rc.Height = element.DesiredSize.Height;
                        rc.Height = System.Math.Min(rc.Height, bounds.Height);
                        element.Arrange(rc);
                        rc.Y += rc.Height;
                    }
                }
            }
        }

        #region SortElements

        private List<UIElement>[] ByEdge(UIElementCollection elements)
        {
            List<UIElement>[] byEdge = new List<UIElement>[9];

            for (int i = 0; i < byEdge.Length; ++i)
            {
                byEdge[i] = new List<UIElement>();
            }

            foreach (UIElement element in elements)
            {
                byEdge[(int)GetEdge(element)].Add(element);
            }

            return byEdge;
        }

        private List<UIElement>[] ByPosition(IList<UIElement> elements, DockDirection direction)
        {
            List<UIElement>[] byPosition = new List<UIElement>[4];

            for (int i = 0; i < byPosition.Length; ++i)
            {
                byPosition[i] = new List<UIElement>();
            }

            foreach (UIElement element in elements)
            {
                byPosition[(int)GetPosition(element, direction)].Add(element);
            }

            return byPosition;
        }

        #endregion SortElements

        #region GetAlignment

        /// <summary>
        /// Gets the horizontal alignment.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        public static HorizontalAlignment GetHorizontalDockAlignment(DependencyObject d)
        {
            return (HorizontalAlignment)d.GetValue(XamDock.HorizontalDockAlignmentProperty);
        }

        /// <summary>
        /// Gets the vertical alignment.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        public static VerticalAlignment GetVerticalDockAlignment(DependencyObject d)
        {
            return (VerticalAlignment)d.GetValue(XamDock.VerticalDockAlignmentProperty);
        }

        /// <summary>
        /// Sets the horizontal alignment.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetHorizontalDockAlignment(DependencyObject d, HorizontalAlignment value)
        {
            d.SetValue(XamDock.HorizontalDockAlignmentProperty, value);
        }

        /// <summary>
        /// Sets the vertical alignment.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetVerticalDockAlignment(DependencyObject d, VerticalAlignment value)
        {
            d.SetValue(XamDock.VerticalDockAlignmentProperty, value);
        }

        #endregion GetAlignment

        private int GetPosition(UIElement element, DockDirection direction)
        {
            if (direction == DockDirection.Horizontal)
            {
                return (int)GetHorizontalDockAlignment(element);
            }
            else
            {
                return (int)GetVerticalDockAlignment(element);
            }
        }

        #endregion Private

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