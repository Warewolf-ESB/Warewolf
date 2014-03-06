using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Infragistics.Controls.Menus.Primitives
{
    /// <summary>
    /// A class which arranges the <see cref="XamDataTreeNode"/> objects in a tree layout.
    /// </summary>
    public class NodesPanel : Panel
    {
        #region Members

        Collection<XamDataTreeNode> _visibleNodes;
        Collection<XamDataTreeNode> _nonReleasedNodes;

        double _overrideVerticalMax, _overflowAdjustment, _invalidateNodeHeight, _measureScrollBarValue, _previousHeight, _previousWidth, _maximumNodeWidth;
        bool _measureCalled, _reverseMeasure, _onNextMeasureReleaseVisibleNodes, _ensuerVertSBValueUpdated, _measureCalledInfinite, _recalcHorizSBVis, _redoScrollProgress;
        int _reverseNodeStartIndex;

        RectangleGeometry _clipRG;
        Rect _hiddenRect = new Rect(-1000, -1000, 0, 0);

        List<FrameworkElement> _notVisibleAndNotArrangedNodes;

        XamDataTree _tree;

        XamDataTreeNode _scrollIntoViewNode;

        bool _resetVertSBViewportSize = false;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NodesPanel"/> class.
        /// </summary>
        public NodesPanel()
        {
            this._clipRG = new RectangleGeometry();
            this.Clip = this._clipRG;
            this._visibleNodes = new Collection<XamDataTreeNode>();
            this._overrideVerticalMax = -1;
            this._nonReleasedNodes = new Collection<XamDataTreeNode>();
            this._notVisibleAndNotArrangedNodes = new List<FrameworkElement>();


            this.IsManipulationEnabled = true;

        }

        #endregion // Constructor

        #region Properties

        #region Public

        /// <summary>
        /// Gets / sets the <see cref="XamDataTree"/> which this <see cref="NodeLayout"/> will apply to.
        /// </summary>
        public XamDataTree Tree
        {
            get { return this._tree; }
            protected internal set
            {
                if (this._tree != value)
                {
                    this._tree = value;
                    this.ScrollInfo = this._tree as IProvideScrollInfo;
                }
            }
        }

        #region VisibleNodes

        /// <summary>
        /// Gets the nodes that are currently visible in the Viewport.
        /// </summary>
        public Collection<XamDataTreeNode> VisibleNodes
        {
            get { return this._visibleNodes; }
        }

        #endregion // VisibleNodes

        #endregion // Public

        #region Internal

        internal bool MeasureCalled
        {
            get { return this._measureCalled; }
            set { this._measureCalled = value; }
        }

        internal bool ScrollNodeIntoViewInProgress
        {
            get;
            private set;
        }

        #endregion // Internal

        #region Protected

        #region ScrollInfo

        /// <summary>
        /// A reference to the ScrollInfo object that relates to the <see cref="NodesPanel"/>
        /// </summary>
        protected IProvideScrollInfo ScrollInfo
        {
            get;
            private set;
        }
        #endregion // Protected

        #endregion // Proteced

        #endregion // Properties

        #region Methods

        #region Public

        #region ResetNodes

        /// <summary>
        /// Releases all Nodes from the VisualTree. 
        /// </summary>
        public void ResetNodes()
        {
            this.ResetNodes(false);
        }

        /// <summary>
        /// Releases all Nodes from the VisualTree. 
        /// </summary>
        /// <param name="releaseAll">True if the NodesPanel should be released by the RecyclingManager.</param>
        public void ResetNodes(bool releaseAll)
        {
            foreach (XamDataTreeNode node in this.VisibleNodes)
            {
                this.ReleaseNode(node);
            }

            if (releaseAll)
            {
                foreach (NodeLayout layout in this.Tree.GlobalNodeLayouts)
                    RecyclingManager.Manager.ReleaseAll(this, layout.Key);

                RecyclingManager.Manager.ReleaseAll(this);

                this._previousHeight = -1;
                this._previousWidth = -1;
            }

            this._overrideVerticalMax = -1;
            this._visibleNodes.Clear();

        }

        #endregion // ResetNodes

        #region ResetCachedScrollInfo

        /// <summary>
        /// Resets any information that is being cached to stop the scrollbar from jumping around while scrolling. 
        /// Reasons for reseting can be the addition/removal of new nodes, or the expansion/collapsing of child nodes. 
        /// </summary>
        public void ResetCachedScrollInfo(bool resetVisibleNodes)
        {
            if (!this.MeasureCalled)
            {
                this._overflowAdjustment = 0;
                this._overrideVerticalMax = -1;
                this._recalcHorizSBVis = true;
                if (resetVisibleNodes)
                    this._onNextMeasureReleaseVisibleNodes = true;
            }
        }

        #endregion // ResetCachedScrollInfo

        #endregion // Public

        #region Protected

        #region RenderNode

        /// <summary>
        /// Creates a control for the node, and adds it as child of the panel. 
        /// </summary>
        /// <param propertyName="node">The node which is now in View. </param>
        /// <param propertyName="availableWidth">The amount of width the node has to work with.</param>
        /// <returns></returns>
        protected internal virtual Size RenderNode(XamDataTreeNode node, double availableWidth)
        {
            if (node.Control == null)
            {
                RecyclingManager.Manager.AttachElement(node, this);

                node.Control.MeasureRaised = false;
                node.Control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (!node.Control.MeasureRaised)
                {
                    node.Control.Measure(new Size(1, 1));
                    node.Control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                }

            }
            else
            {
                node.ApplyStyle();

                node.Control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            node.EnsureCurrentState();

            this._maximumNodeWidth = Math.Max(this._maximumNodeWidth, node.Control.DesiredSize.Width + node.Manager.Level * node.NodeLayout.IndentationResolved);

            return node.Control.DesiredSize;
        }

        #endregion // RenderNode

        #region UpdateScrollInfo

        /// <summary>
        /// Updates the ScrollInfo of the <see cref="NodesPanel"/>.
        /// Such as changing the horizontal/vertical scrollbar visibility, or their viewport size.
        /// </summary>
        protected virtual void UpdateScrollInfo(int totalNodeCount, double finalWidth)
        {
            ScrollBar vertBar = this.Tree.VerticalScrollBar;
            if (vertBar != null)
            {
                double val = vertBar.Value;

                vertBar.Maximum = this._overrideVerticalMax;

                // So, the scrollbar has this weird bug, where sometimes
                // if you change the max, and the value is still within the max and min, it'll still change the 
                // value, even though it shouldn't have touched it. 
                if (vertBar.Value != val && val < vertBar.Maximum && !this._ensuerVertSBValueUpdated)
                    vertBar.Value = val;

                this._ensuerVertSBValueUpdated = false;

                // So we should only set the Viewportsize once
                // As we don't want it to change while we're scrolling. 
                if (this._resetVertSBViewportSize)
                {
                    vertBar.ViewportSize = this._visibleNodes.Count;
                    this._resetVertSBViewportSize = false;
                }
                
                double largeChange = this._visibleNodes.Count - 1;
                if (largeChange < 0)
                    largeChange = 0;

                // Limit the large change to one less row, so that partial rows don't get jumped over when changing "pages"
                vertBar.LargeChange = largeChange;
                vertBar.SmallChange = (double)vertBar.ViewportSize / 10;

                Visibility previous = vertBar.Visibility;
                vertBar.Visibility = ((this._overrideVerticalMax <= 0) || totalNodeCount == 0) ? Visibility.Collapsed : Visibility.Visible;

                if (vertBar.Visibility != previous && vertBar.Visibility == Visibility.Collapsed)
                    vertBar.Value = 0;
            }

            ScrollBar horizBar = this.Tree.HorizontalScrollBar;
            if (horizBar != null)
            {
                // If we're in a container that is giving us Infinite width, we have no need for a horizontal scrollbar...so hide it.
                if (double.IsPositiveInfinity(this._previousWidth))
                    horizBar.Visibility = System.Windows.Visibility.Collapsed;
                else
                {
                    if (this._maximumNodeWidth > finalWidth)
                    {
                        horizBar.Visibility = System.Windows.Visibility.Visible;
                        horizBar.LargeChange = horizBar.ViewportSize;
                        horizBar.SmallChange = horizBar.ViewportSize / 10.0;

                        horizBar.ViewportSize = finalWidth;

                        double max = this._maximumNodeWidth - finalWidth;
                        if (double.IsInfinity(max) || double.IsNaN(max) || max < 0)
                            max = 0;
                        horizBar.Maximum = max;
                        this._recalcHorizSBVis = false;

                        // Re-enable the scrollbar if its visible and was disabled.
                        if (!horizBar.IsEnabled)
                            horizBar.IsEnabled = true;
                    }
                    else
                    {
                        if (this._recalcHorizSBVis)
                        {
                            horizBar.Visibility = System.Windows.Visibility.Collapsed;
                            horizBar.Value = 0;
                        }
                        // If its visible, but it shouldn'type be enable, disable it. 
                        else if (horizBar.IsEnabled)
                        {
                            horizBar.Value = 0;
                            horizBar.Maximum = 0;
                            horizBar.IsEnabled = false;
                        }
                    }
                }
            }
        }
        #endregion // UpdateScrollInfo

        #region ArrangeNode

        /// <summary>
        /// Calls Arrange on the specified node. 
        /// </summary>
        /// <param propertyName="node">The node that should be arranged.</param>
        /// <param propertyName="left">The left value the node should be positioned at.</param>
        /// <param propertyName="top">The top value the node should be positioned at.</param>
        /// <param propertyName="width">The width the node should be.</param>
        /// <param propertyName="height">The height the node should be.</param>
        protected static bool ArrangeNode(XamDataTreeNode node, double left, double top, double width, double height)
        {
            node.Control.ArrangeRaised = false;

            node.Control.Arrange(new Rect(left, top, width, height));

            // If Arrange isn't triggered right away, it means there is probably another Measure cycle that is waiting to fire. 
            // Since we now know that, we can short circuit our ArrangeOverride, which should even increase perf. 
            return node.Control.ArrangeRaised;
        }

        #endregion // ArrangeNode

        #endregion // Protected

        #region Private

        #region ReleaseNode

        internal void ReleaseNode(XamDataTreeNode node)
        {
            if (!RecyclingManager.Manager.ReleaseElement(node, this))
            {
                this._nonReleasedNodes.Add(node);
            }
        }

        #endregion // ReleaseNode

        #region ThrowoutUnusedNodes
        private void ThrowoutUnusedNodes(List<XamDataTreeNode> previousVisibleNodes)
        {
            foreach (XamDataTreeNode node in previousVisibleNodes)
            {
                if (!this._visibleNodes.Contains(node))
                {
                    if (node.Control != null)
                    {
                        this.ReleaseNode(node);
                    }
                }
            }
        }
        #endregion // ThrowoutUnusedNodes

        #region ResolveBaseSize

        private Size ResolveBaseSize(Size availableSize)
        {
            Size alternateReturnSize = base.MeasureOverride(availableSize);
            if (alternateReturnSize.Height == 0 && !double.IsInfinity(availableSize.Height) && !double.IsNaN(availableSize.Height))
                alternateReturnSize.Height = availableSize.Height;

            if (alternateReturnSize.Width == 0 && !double.IsInfinity(availableSize.Width) && !double.IsNaN(availableSize.Width))
                alternateReturnSize.Width = availableSize.Width;

            return alternateReturnSize;
        }

        #endregion // ResolveBaseSize

        #endregion // Private

        #region Internal

        internal void ScrollNodeIntoView(XamDataTreeNode node)
        {
            if (this._measureCalled)
                return;

            #region Expanding nodes so that we know that the node will be visible
            // check that all the parent nodes are expanded
            if (node.Manager.ParentNode != null)
            {
                XamDataTreeNode parentNode = node.Manager.ParentNode;
                while (parentNode != null)
                {
                    if (!parentNode.IsExpanded)
                    {
                        parentNode.IsExpanded = true;
                    }
                    parentNode = parentNode.Manager.ParentNode;
                }
            }
            #endregion // Expanding nodes so that we know that the node will be visible

            int index = this.Tree.InternalNodes.IndexOf(node);

            if (node.Control != null)
            {
                Rect panelLayout = LayoutInformation.GetLayoutSlot(this);
                Rect rowLayout = LayoutInformation.GetLayoutSlot(node.Control);
                Rect nodeLineControl = LayoutInformation.GetLayoutSlot(node.Control.NodeLineControl);
                Rect contentPresetner = LayoutInformation.GetLayoutSlot(node.Control.ContentPresenter);

                if ((rowLayout.Bottom > panelLayout.Height) || (rowLayout.X == _hiddenRect.X && rowLayout.Y == _hiddenRect.Y))
                {
                    this._reverseNodeStartIndex = index;
                    this._reverseMeasure = true;
                }
                else if (rowLayout.Height <= 0 || rowLayout.Top < 0)
                {
                    this.ScrollInfo.VerticalScrollBar.Value = index;
                }
                if (contentPresetner.X + rowLayout.X < 0)
                    this.ScrollInfo.HorizontalScrollBar.Value = contentPresetner.Left;
            }
            else
            {
                if (this.VisibleNodes.Count > 0)
                {
                    int lastIndex;
                    lastIndex = this.Tree.InternalNodes.IndexOf(this.VisibleNodes[this.VisibleNodes.Count - 1]);
                    if (index > lastIndex)
                    {
                        this._reverseNodeStartIndex = index;
                        this._reverseMeasure = true;
                    }
                    else
                    {
                        if (index > this.ScrollInfo.VerticalScrollBar.Maximum)
                            this.ScrollInfo.VerticalScrollBar.Maximum = index;

                        this.ScrollInfo.VerticalScrollBar.Value = index;
                    }
                }
            }

            this._scrollIntoViewNode = node;
            this.ScrollNodeIntoViewInProgress = true;
            this.InvalidateMeasure();
        }

        #endregion // Internal

        #endregion // Methods

        #region Overrides

        #region MeasureOverride

        /// <summary>
        /// Determines how many nodes can fit given the available size and scroll information.
        /// </summary>
        /// <param name="availableSize">
        ///	The available size that this object can give to child objects. Infinity can be 
        ///	specified as a value to indicate that All nodes should be displayed.
        ///	</param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            


            if (double.IsInfinity(availableSize.Height) && !double.IsInfinity(this._previousHeight))
            {
                Size size = new Size(this.DesiredSize.Width, this._previousHeight);
                this._previousHeight = availableSize.Height;
                this._recalcHorizSBVis = true;
                this._resetVertSBViewportSize = true;

                return size;
            }

            // Measure Gets triggered by lots of things during it's own cycle. Since we don't want to keep evaluating the same thing
            // over and over again, we'll set a flag so that it only does this once. Note: the flag will be reset in ArrageOverride 
            if (this._measureCalled && this._previousWidth == availableSize.Width && this._previousHeight == availableSize.Height)
                return this.DesiredSize;
            else
                this._measureCalled = true;

            // Store currently Visible Nodes, and clear out the global collection of VisibleNodes
            List<XamDataTreeNode> previousVisibleNodes = new List<XamDataTreeNode>();

            if (this._previousHeight != availableSize.Height)
            {
                this._resetVertSBViewportSize = true;
                this._overrideVerticalMax = -1;

            }

            // Something changed, so we can't rely on our advanced logic later on to, figure out which nodes
            // are going to be available on the next go around, so lets just release them all and go again.
            if (this._onNextMeasureReleaseVisibleNodes)
            {
                foreach (XamDataTreeNode node in this.VisibleNodes)
                    this.ReleaseNode(node);

                this.VisibleNodes.Clear();

                this._onNextMeasureReleaseVisibleNodes = false;
            }

            if (this.VisibleNodes.Count == 0)
            {
                // This is our first load.
                // So, make sure that we remove the horizontal scrollbar if it isn't neccessary, 
                this._recalcHorizSBVis = true;
                this._resetVertSBViewportSize = true;
            }

            this._previousHeight = availableSize.Height;

            // Store the width, so that if we need to Render a node at some point outside of the MesureOverride (such as in ScrollCellIntoView
            // we can make sure that the width is adhered to. 
            this._previousWidth = availableSize.Width;

            // See comments in RenderNode for a complete description of this. 
            this._invalidateNodeHeight = (this._invalidateNodeHeight == 10000) ? 10001 : 10000;

            double currentHeight = 0;
            double availableHeight = availableSize.Height;
            double maxNodeWidth = 0;

            ScrollBar vertBar = this.Tree.VerticalScrollBar;
            double scrollTop = (vertBar != null) ? vertBar.Value : 0;

            int nodeCount = this.Tree.InternalNodes.Count;

            this._maximumNodeWidth = 0.0;

            if (nodeCount == 0)
            {
                previousVisibleNodes.AddRange(this.VisibleNodes);
                this.VisibleNodes.Clear();
                this.ThrowoutUnusedNodes(previousVisibleNodes);

                return this.ResolveBaseSize(availableSize);
            }

            int startNodeIndex = 0, currentNode = 0;
            double percentScroll = 0;
            double nodeHeight = 0;
            if (!this._reverseMeasure)
            {
                // Add First Node
                startNodeIndex = (int)scrollTop;

                // Something must have been collapsed. So, now the scrollTop is greater than the amount of scrollable nodes
                // So, make, the last scrollable node, the top. 
                if (startNodeIndex >= nodeCount)
                    startNodeIndex = nodeCount - 1;

                XamDataTreeNode firstNode = this.Tree.InternalNodes[startNodeIndex];

                if (this.VisibleNodes.Count > 0)
                {
                    int prevStartIndex = this.VisibleNodes.IndexOf(firstNode);

                    if (prevStartIndex != 0)
                    {
                        if (prevStartIndex != -1)
                        {
                            for (int i = prevStartIndex; i > 0; i--)
                            {
                                XamDataTreeNode zeroNode = this.VisibleNodes[0];
                                this.ReleaseNode(zeroNode);
                                this.VisibleNodes.RemoveAt(0);
                            }

                            previousVisibleNodes.AddRange(this.VisibleNodes);
                        }
                        else
                        {
                            int total = this.VisibleNodes.Count;
                            int first = this.Tree.InternalNodes.IndexOf(this.VisibleNodes[0]);
                            int last = first + total - 1;

                            if (last < startNodeIndex)
                            {
                                foreach (XamDataTreeNode node in this.VisibleNodes)
                                    this.ReleaseNode(node);

                                this.VisibleNodes.Clear();
                            }
                            else
                            {
                                total--;
                                int diff = first - startNodeIndex;

                                for (int i = 0; i < diff && i < total; i++)
                                {
                                    int index = total - i;
                                    XamDataTreeNode zeroNode = this.VisibleNodes[index];
                                    this.ReleaseNode(zeroNode);
                                    this.VisibleNodes.RemoveAt(index);
                                }
                            }

                            previousVisibleNodes.AddRange(this.VisibleNodes);
                        }
                    }
                    else
                    {
                        previousVisibleNodes.AddRange(this.VisibleNodes);
                    }

                    this.VisibleNodes.Clear();
                }

                // Ok, So in some specific scenarios, a refresh may be triggered on the Tree, and the first node that was visible
                // maybe have been a child node, which hasn't been touched since, and so might not be registered in the InternalNodes
                // Collection. When this happens, we actually start out on the wrong first node. The problem is that b/c the node
                // above it is never touched, the child nodes which were the top nodes, are never registered and thus never come into view
                // Thus this code here, simply just accesses the previous node.  The fact that it does this, ensures that all of it's child nodes
                // have a chance to come into view. 
                if (startNodeIndex > 0)
                {
                    XamDataTreeNode xNode = this.Tree.InternalNodes[startNodeIndex - 1];
                }

                Size firstNodeSize = this.RenderNode(firstNode, availableSize.Width);
                nodeHeight = firstNodeSize.Height;
                maxNodeWidth = Math.Max(maxNodeWidth, firstNodeSize.Width);
                currentHeight += nodeHeight;
                this._visibleNodes.Add(firstNode);

                // Calculate PercentScroll
                if (vertBar != null)
                {
                    double percent = vertBar.Value - (int)vertBar.Value;
                    percentScroll = nodeHeight * percent;
                    currentHeight -= percentScroll;
                    this._measureScrollBarValue = vertBar.Value;
                }

                // Add Nodes untill there is no more height left
                for (currentNode = startNodeIndex + 1; currentNode < nodeCount; currentNode++)
                {
                    if (currentHeight >= availableHeight)
                        break;

                    XamDataTreeNode n = this.Tree.InternalNodes[currentNode];
                    Size nodeSize = this.RenderNode(n, availableSize.Width);
                    nodeHeight = Math.Max(nodeHeight, nodeSize.Height);
                    currentHeight += nodeSize.Height;
                    maxNodeWidth = Math.Max(maxNodeWidth, nodeSize.Width);
                    this._visibleNodes.Add(n);
                }

                // Add the percent scroll back, so that we can truly validate if we've scrolled past the last item.
                currentHeight += percentScroll;

                startNodeIndex--;
            }
            else
            {
                previousVisibleNodes.AddRange(this.VisibleNodes);
                this.VisibleNodes.Clear();
                startNodeIndex = this._reverseNodeStartIndex;
            }

            // If the height of all the visible nodes is less then whats available in the viewport, and there are more nodes in the 
            // collection, it means we've scrolled further than we needed to. Since we don't want whitespace to appear under 
            // the last XamDataTreeNode, lets add more nodes and update the maximum scroll value.
            if (currentHeight < availableHeight && this._visibleNodes.Count < nodeCount)
            {
                for (currentNode = startNodeIndex; currentNode >= 0; currentNode--)
                {
                    XamDataTreeNode n = this.Tree.InternalNodes[currentNode];
                    Size nodeSize = this.RenderNode(n, availableSize.Width);
                    nodeHeight = Math.Max(nodeHeight, nodeSize.Height);
                    maxNodeWidth = Math.Max(maxNodeWidth, nodeSize.Width);
                    currentHeight += nodeHeight;
                    this._visibleNodes.Insert(0, n);

                    if (currentHeight >= availableHeight)
                    {
                        if (this._reverseMeasure)
                        {
                            percentScroll = ((currentHeight - availableHeight) / nodeHeight);
                            double val = currentNode + percentScroll;
                            this.Tree.VerticalScrollBar.Value = val;
                            this._measureScrollBarValue = vertBar.Value;

                            if (this.Tree.VerticalScrollBar.Value != val)
                            {
                                this._ensuerVertSBValueUpdated = true;
                            }
                        }
                        break;
                    }
                }
            }


            // If we're not given Infinite room to work with, then we need to figure out the Maximum value for the Vertical Scrollbar.
            if (!double.IsPositiveInfinity(availableSize.Height))
            {
                // I was doing a LOT of extra work in calculating this, which was very expensive on the first load. 
                // However, most trees aren't going to need that extra work, b/c they generally all have nodes that are all the same height. 
                // So, since thats the most common case, don't waste performance on an edge case. 
                int currentLastNodeIndex = nodeCount - this.VisibleNodes.Count;
                double sp = (currentHeight - availableHeight) / nodeHeight;

                // Whoops, becareful to not divide by zero, as it is possible that a row could have a height of zero.
                if (nodeHeight == 0 || double.IsNaN(sp) || double.IsInfinity(sp))
                    sp = 0;

                this._overrideVerticalMax = currentLastNodeIndex + sp;

                if (this._overrideVerticalMax < 0)
                    this._overrideVerticalMax = -1;
            }


            bool exitEarly = false;
            // If the nodes count doesn'type match up, it means we've probably loaded 
            // a cached node that was expanded. In this case, lets re-measure the panel. 
            if (this.Tree.InternalNodes.Count != nodeCount)
            {
                if (this.ScrollNodeIntoViewInProgress)
                    this._redoScrollProgress = true;
                this._overrideVerticalMax = -1;
                this._measureCalled = false;
                this.Measure(availableSize);
                exitEarly = true;
            }

            // Throw out unused nodes. 
            this.ThrowoutUnusedNodes(previousVisibleNodes);

            if (exitEarly)
                return this.DesiredSize;

            this._overflowAdjustment = (currentHeight - availableHeight);

            double width = availableSize.Width;
            if (double.IsPositiveInfinity(width))
            {
                width = maxNodeWidth;
                if (!this._measureCalledInfinite)
                {
                    // So, if the width is infinite, we might not re-measure,
                    // which means some nodes aren't going to be the full max width. 
                    // So, lets re-measure to make sure this occurs correctly. 
                    this._measureCalled = false;
                    this._measureCalledInfinite = true;
                    this.Measure(availableSize);
                }
            }

            if (!double.IsPositiveInfinity(availableSize.Height))
                currentHeight = availableSize.Height;

            if (currentHeight < 0)
                currentHeight = 0;

            return new Size(width, currentHeight);
        }
        #endregion // MeasureOverride

        #region ArrangeOverride

        /// <summary>
        /// Arranges each node that should be visible, one on top of the other, similar to a 
        /// Vertical <see cref="StackPanel"/>.
        /// </summary>
        /// <param propertyName="finalSize">
        /// The final area within the parent that this object 
        /// should use to arrange itself and its children.
        /// </param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Since we're in arrange now, its ok to reset the measure flag, so that if measure is called again, it will execute. 
            this._measureCalled = false;
            this._measureCalledInfinite = false;
            double scrollBarLeft = 0.0;
            bool arrangeNotWorking = false;

            if (this.ScrollInfo.HorizontalScrollBar != null)
            {
                scrollBarLeft = -this.ScrollInfo.HorizontalScrollBar.Value;
            }

            this._clipRG.Rect = new Rect(0, 0, finalSize.Width, finalSize.Height);

            // So, we get rid of elements that were made recently available. But, its still possible for arrange to not be honored
            // which means, the could still techiniaclly wind up in view. So, if they aren't arranged, we store them off for the next
            // arrange cycle, and arrange them then. 
            if (this._notVisibleAndNotArrangedNodes.Count > 0)
            {
                List<FrameworkElement> elementsStillNotArranged = new List<FrameworkElement>();
                foreach (XamDataTreeNodeControl node in this._notVisibleAndNotArrangedNodes)
                {
                    if (LayoutInformation.GetLayoutSlot(node) != this._hiddenRect)
                    {
                        node.ArrangeRaised = false;
                        node.Arrange(this._hiddenRect);
                        if (!node.ArrangeRaised)
                        {
                            elementsStillNotArranged.Add(node);
                        }
                    }
                }

                this._notVisibleAndNotArrangedNodes.Clear();
                this._notVisibleAndNotArrangedNodes.AddRange(elementsStillNotArranged);
            }

            // Move all Nodes that aren'type being used, out of view. 
            List<FrameworkElement> unusedNodes = RecyclingManager.Manager.GetRecentlyAvailableElements(this, true);
            foreach (XamDataTreeNodeControl node in unusedNodes)
            {
                node.ArrangeRaised = false;
                node.Arrange(this._hiddenRect);
                if (!node.ArrangeRaised)
                {
                    this._notVisibleAndNotArrangedNodes.Add(node);
                }
            }

            // Some nodes, like nodes that are in edit mode, may choose to not get released. 
            // If thats the case, then we need to make sure that they get placed out of view. 
            foreach (XamDataTreeNode node in this._nonReleasedNodes)
            {
                if (node.Control != null)
                    node.Control.Arrange(this._hiddenRect);
            }

            this._nonReleasedNodes.Clear();

            int nodeCount = this.Tree.InternalNodes.Count;

            this.UpdateScrollInfo(nodeCount, finalSize.Width);

            double top = 0;

            if (nodeCount != 0)
            {
                // Calculate the offset TopValue, for the first node in this normal nodes collection. 
                ScrollBar vertSB = this.Tree.VerticalScrollBar;
                if (vertSB != null && vertSB.Visibility == Visibility.Visible && this.VisibleNodes.Count > 0)
                {
                    if (this._measureScrollBarValue != vertSB.Maximum)
                    {
                        double percent = this._measureScrollBarValue - (int)this._measureScrollBarValue;
                        double firstRealHeight = 0.0;
                        int count = this._visibleNodes.Count;
                        




                        for (int i = 0; i < count; i++)
                        {
                            double h = this._visibleNodes[i].Control.DesiredSize.Height;
                            if (h > 0.0)
                            {
                                firstRealHeight = h;
                                break;
                            }
                        }
                        double topVal = firstRealHeight  * percent;

                        double scrollTop = this._measureScrollBarValue;
                        double max = this._overrideVerticalMax;
                        if (scrollTop >= max && max > 0)
                            top -= this._overflowAdjustment;
                        else
                            top += -topVal;
                    }
                    else
                    {
                        // We've reached the last child, so lets make sure its visible. 
                        top -= this._overflowAdjustment;
                    }

                    // For a cleaner scrolling experience, update the small change of the scrollbar to account for the currentIndex visible children.
                    vertSB.SmallChange = (double)this._visibleNodes.Count / 10;
                }


                // Render Normal Nodes
                foreach (XamDataTreeNode node in this._visibleNodes)
                {
                    if (node.Control == null)
                        RenderNode(node, double.PositiveInfinity);

                    double height = node.Control.DesiredSize.Height;
                    double left = 0;

                    if (!NodesPanel.ArrangeNode(node, left + scrollBarLeft, top, node.Control.DesiredSize.Width, height))
                        arrangeNotWorking = true;
                    Canvas.SetZIndex(node.Control, 0);
                    top += height;
                }
            }

            this._reverseMeasure = false;

            if (this.ScrollNodeIntoViewInProgress)
            {
                if (this._scrollIntoViewNode.Control == null || this._redoScrollProgress)
                    this.ScrollNodeIntoView(this._scrollIntoViewNode);

                this.ScrollNodeIntoViewInProgress = false;
                this._redoScrollProgress = false;
                this._scrollIntoViewNode = null;

                // This Invalidate is here to make sure that everything has a second chance to re-measure
                this.InvalidateMeasure();
            }

            return finalSize;
        }
        #endregion // ArrangeOverride


        #region OnChildDesiredSizeChanged

        ///<summary>        
        ///     Supports layout behavior when a child element is resized.
        ///</summary>
        ///<param name="child">        
        ///     The child element that is being resized.
        ///</param>
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
            if (this._maximumNodeWidth < child.DesiredSize.Width)
            {
                this._maximumNodeWidth = child.DesiredSize.Width;
            }
        }

        #endregion // OnChildDesiredSizeChanged


        #endregion // Overrides
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