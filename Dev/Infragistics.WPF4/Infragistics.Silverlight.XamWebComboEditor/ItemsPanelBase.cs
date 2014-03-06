using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using Infragistics.Collections;
using System.Windows.Input;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A panel containing the ComboEditorItemBase objects.
    /// </summary>
    public class ItemsPanelBase<T, TControl> : Panel
        where T : ComboEditorItemBase<TControl>
        where TControl : FrameworkElement
    {
        #region Members

        private ComboEditorBase<T, TControl> _comboEditor;
        double _overrideVerticalMax, _previousHeight, _measureScrollBarValue, _overflowAdjustment;
        Collection<T> _lastItemsDetached;
        RectangleGeometry _clipRG;
        double _actualWidth = 0;
        int _previousItemsCount;
        bool _reverseMeasure;
        int _reverseItemStartIndex;
        T _scrollIntoViewItem;

        #endregion //Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsPanel"/> class.
        /// </summary>
        public ItemsPanelBase()
        {
            this._clipRG = new RectangleGeometry();
            this.Clip = this._clipRG;

            this.VisibleItems = new Collection<T>();
            this._overrideVerticalMax = -1;
            this._lastItemsDetached = new Collection<T>();

            this.Loaded += new RoutedEventHandler(ItemsPanelBase_Loaded);

            // JM 02-23-12 TFS100053, TFS100158

             this.SetCurrentValue(Stylus.IsPressAndHoldEnabledProperty, false);

        }

        #endregion //Constructor

        #region Overrides

        #region MeasureOverride

        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child elements and determines a size for the FrameworkElement-derived class. 
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes. </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            this.MeasureCalled = true; 

            this.ComboEditor.InvalidateDropDownPosition(false);

            if (double.IsPositiveInfinity(availableSize.Height))
            {
                availableSize.Height = this.ComboEditor.DropDownHeight;

                if (this.ComboEditor.HorizontalScrollBar.Visibility.Equals(Visibility.Visible) &&
                    availableSize.Height - this.ComboEditor.HorizontalScrollBar.Height >= 0)
                {
                    availableSize.Height -= this.ComboEditor.HorizontalScrollBar.Height;
                }
            }

            double availableHeight = availableSize.Height;

            if (this._previousHeight != availableHeight)
                this._overrideVerticalMax = -1;

            this._previousHeight = availableHeight;

            this.InLayoutPhase = true;

            Size panelDesiredSize = new Size();
            double currentHeight = 0;

            BindableItemCollection<T> items = this.ComboEditor.Items;
            int itemsCount = items.Count;

            if (this._previousItemsCount != itemsCount)
                this._overrideVerticalMax = -1;

            this._previousItemsCount = itemsCount;

            // Store currently Visible Items, and clear out the global collection of VisbleItems
            List<T> previousVisibleItems = new List<T>();

            double fixedRowHeight = 0; 

			// Render Top Fixed Rows
            // Always measure top rows even, if there are no data items.
			if (null != this.FixedRowsTop)
			{
                foreach (ComboRowBase fixedRow in this.FixedRowsTop)
                {
                    Size rowSize = this.RenderItem<ComboRowBase, ComboCellsPanel>(fixedRow, availableSize);
                    fixedRowHeight += rowSize.Height;
                    availableHeight -= rowSize.Height;
					panelDesiredSize.Width = Math.Max(panelDesiredSize.Width, rowSize.Width);
                }
			}

            // Checks if the ItemsSource property is not set.
            if (this.ComboEditor.DataManager == null || itemsCount == 0)
            {
                previousVisibleItems.AddRange(this.VisibleItems);
                this.VisibleItems.Clear();
                this.ThrowoutUnusedItems(previousVisibleItems);

                // Be sure to set the _actualWIdth so the HorizontalScrollbar updates properly.
                this._actualWidth = availableSize.Width;
                this.ComboEditor.UpdateDropDownPosition(availableSize.Width, availableSize.Height, 0);
                return new Size();
            }

            int startIndex = 0;
            double itemHeight = 0;
            if (!this._reverseMeasure)
            {
                startIndex = (int)this.ComboEditor.VerticalScrollBar.Value;

                T firstItem = this.ComboEditor.Items[startIndex];

                if (this.VisibleItems.Count > 0)
                {
                    int prevStartIndex = this.VisibleItems.IndexOf(firstItem);

                    if (prevStartIndex != 0)
                    {
                        if (prevStartIndex != -1)
                        {
                            for (int i = prevStartIndex; i > 0; i--)
                            {
                                T zeroNode = this.VisibleItems[0];
                                this.ReleaseItem(zeroNode);
                                this.VisibleItems.RemoveAt(0);
                            }

                            previousVisibleItems.AddRange(this.VisibleItems);
                        }
                        else
                        {
                            int total = this.VisibleItems.Count;
                            int first = this.ComboEditor.Items.IndexOf(this.VisibleItems[0]);
                            int last = first + total - 1;

                            if (last < startIndex)
                            {
                                foreach (T item in this.VisibleItems)
                                    this.ReleaseItem(item);

                                this.VisibleItems.Clear();
                            }
                            else
                            {
                                total--;
                                int diff = first - startIndex;

                                for (int i = 0; i < diff && i < total; i++)
                                {
                                    int index = total - i;
                                    T zeroNode = this.VisibleItems[index];
                                    this.ReleaseItem(zeroNode);
                                    this.VisibleItems.RemoveAt(index);
                                }
                            }

                            previousVisibleItems.AddRange(this.VisibleItems);
                        }
                    }
                    else
                    {
                        previousVisibleItems.AddRange(this.VisibleItems);
                    }

                    this.VisibleItems.Clear();
                }

                Size firstItemSize = this.RenderItem(firstItem, availableSize);
                itemHeight = firstItemSize.Height;
                panelDesiredSize.Width = Math.Max(panelDesiredSize.Width, firstItemSize.Width);
                currentHeight += itemHeight;
                this.VisibleItems.Add(firstItem);

                ScrollBar vertBar = this.ComboEditor.VerticalScrollBar;
                // Calculate PercentScroll
                if (vertBar != null)
                {

                    if (vertBar.Value != vertBar.Maximum)
                    {
                        double percent = vertBar.Value - (int)vertBar.Value;
                        double percentScroll = itemHeight * percent;
                        currentHeight -= percentScroll;
                    }
                    this._measureScrollBarValue = vertBar.Value;
                }


                for (int currentItem = startIndex + 1; currentItem < itemsCount; currentItem++)
                {
                    if (currentHeight >= availableHeight)
                        break;

                    T comboEditorItem = items[currentItem];

                    // Calculates the desired size.
                    Size desiredItemSize = this.RenderItem(comboEditorItem, availableSize);
                    currentHeight += desiredItemSize.Height;
                    panelDesiredSize.Width = Math.Max(panelDesiredSize.Width, desiredItemSize.Width);

                    this.VisibleItems.Add(comboEditorItem);
                }

                startIndex--;
            }
            else
            {
                previousVisibleItems.AddRange(this.VisibleItems);
                this.VisibleItems.Clear();
                startIndex = this._reverseItemStartIndex;
            }


            // If the height of all the visible rows is less then whats available in the viewport, and there are more rows in the 
            // collection, it means we've scrolled further than we needed to. Since we don't want whitespace to appear under 
            // the last DataRow, lets add more rows and update the maximum scroll value.
            if (currentHeight < availableHeight && this.VisibleItems.Count < itemsCount && startIndex < itemsCount)
            {
                for (int currentItem = startIndex; currentItem >= 0; currentItem--)
                {
                    T r = this.ComboEditor.Items[currentItem];
                    Size itemSize = this.RenderItem(r, availableSize);
                    itemHeight = itemSize.Height;
                    panelDesiredSize.Width = Math.Max(panelDesiredSize.Width, itemSize.Width);

                    currentHeight += itemHeight;
                    this.VisibleItems.Insert(0, r);

                    if (currentHeight >= availableHeight)
                    {
                        if (this._reverseMeasure)
                        {
                            double percentScroll = ((currentHeight - availableHeight) / itemHeight);
                            this._measureScrollBarValue = this.ComboEditor.VerticalScrollBar.Value = currentItem + percentScroll;
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
                int currentLastNodeIndex = itemsCount - this.VisibleItems.Count;
                double sp = (currentHeight - availableHeight) / itemHeight;
                this._overrideVerticalMax = currentLastNodeIndex + sp;

				// JM 9-1-11 TFS84956 - Check for Infinity and NaN
                if (this._overrideVerticalMax < 0 ||
					double.IsInfinity(this._overrideVerticalMax) ||
					double.IsNaN(this._overrideVerticalMax))
                    this._overrideVerticalMax = -1;
            }


            if (this._measureScrollBarValue != this.ComboEditor.VerticalScrollBar.Value)
            {
                this.ComboEditor.VerticalScrollBar.Maximum = this._overrideVerticalMax;
                this.ComboEditor.VerticalScrollBar.Value = this._measureScrollBarValue;
            }


            // Throw out unused rows. 
            this.ThrowoutUnusedItems(previousVisibleItems);

            this._overflowAdjustment = (currentHeight - availableHeight);

            currentHeight += fixedRowHeight;
            panelDesiredSize.Height = currentHeight;

            if (!double.IsPositiveInfinity(availableSize.Width))
            {
                if (panelDesiredSize.Width > availableSize.Width)
                {
                    this._actualWidth = panelDesiredSize.Width;
                    panelDesiredSize.Width = availableSize.Width;
                }
                else
                {
                    this._actualWidth = availableSize.Width;
                }
            }

            if (panelDesiredSize.Height < availableHeight)
            {
                double width = panelDesiredSize.Width;
                if (!double.IsPositiveInfinity(availableSize.Width) && width < availableSize.Width)
                    width = availableSize.Width;

                this.UpdateScrollInfo(itemsCount, new Size(width, panelDesiredSize.Height));
                this.ComboEditor.UpdateDropDownPosition(availableSize.Width, availableSize.Height, panelDesiredSize.Height);
            }
            else if(!double.IsPositiveInfinity(availableSize.Height))
            {
                panelDesiredSize.Height = availableSize.Height;
            }

            return panelDesiredSize;
        }

        #endregion // MeasureOverride

        #region ArrangeOverride

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a FrameworkElement derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double left = 0;
            double top = 0;
            this.InLayoutPhase = false;
            this.MeasureCalled = false;

            if (this.ComboEditor.HorizontalScrollBar != null)
            {
                left = -this.ComboEditor.HorizontalScrollBar.Value;
            }

            this._clipRG.Rect = new Rect(0, 0, finalSize.Width, finalSize.Height);

            // Move all Rows that aren'type being used, out of view. 
            List<FrameworkElement> unusedRows = RecyclingManager.Manager.GetAvailableElements(this);
            foreach (FrameworkElement row in unusedRows)
            {
                row.Arrange(new Rect(-1000, -1000, 0, 0));
            }

            // Make sure we hide the last items, that aren't in view.
            foreach (T item in this._lastItemsDetached)
            {
                if (item.Control != null && !this.VisibleItems.Contains(item))
                    item.Control.Arrange(new Rect(-1000, -1000, 0, 0));
            }

            int itemCount = this.ComboEditor.Items.Count;

            this.UpdateScrollInfo(itemCount, finalSize);

			// Render Top FixedRows
			if (null != this.FixedRowsTop)
			{
                foreach (ComboRowBase fixedRow in this.FixedRowsTop)
                {
                    if (fixedRow.Control == null)
                        continue;

                    this.ArrangeItem(fixedRow.Control, left, top, fixedRow.Control.DesiredSize.Width);
                    Canvas.SetZIndex(fixedRow.Control, 10003);
                    top += fixedRow.Control.DesiredSize.Height;
                }
			}

            if (itemCount != 0)
            {
                // Calculate the offset TopValue, for the first row in this normal rows collection. 
                ScrollBar vertSB = this.ComboEditor.VerticalScrollBar;
                if (vertSB != null && vertSB.Visibility == Visibility.Visible && this.VisibleItems.Count > 0)
                {
                    if (this._measureScrollBarValue != vertSB.Maximum)
                    {
                        double percent = this._measureScrollBarValue - (int)this._measureScrollBarValue;
                        double topVal = this.VisibleItems[0].Control.DesiredSize.Height * percent;

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
                    vertSB.SmallChange = (double)this.VisibleItems.Count / 10;
                }

                double offsetScrollPos = (this.ComboEditor.HorizontalScrollBar != null && this.ComboEditor.HorizontalScrollBar.Visibility == Visibility.Visible) ? this.ComboEditor.HorizontalScrollBar.Value : 0;

                double itemWidth = finalSize.Width + offsetScrollPos;
                foreach (T comboEditorItem in this.VisibleItems)
                {
                    this.ArrangeItem(comboEditorItem.Control, left, top, itemWidth);
                    top += comboEditorItem.Control.DesiredSize.Height;
                }

                this._reverseMeasure = false;

                if (this.ScrollItemIntoViewInProgress)
                {
                    if (this._scrollIntoViewItem.Control == null)
                    {
                        this.ScrollItemIntoView(this._scrollIntoViewItem, false);
                    }
                    else if (this._lastItemsDetached.Contains(this._scrollIntoViewItem)) // The Control is not null because it is added to _lastItemsDetached
                    {
                        this.ScrollItemIntoView(this._scrollIntoViewItem, false);
                    }

                    this.ScrollItemIntoViewInProgress = false;
                    this._scrollIntoViewItem = null;

                    // This Invalidate is here to make sure that everything has a second chance to re-measure
                    this.InvalidateMeasure();
                }
            }

            return finalSize; // Returns the final Arranged size 
        }

        #endregion //ArrangeOverride

        #endregion // Overrides

        #region Properties

        #region Public

        #region ComboEditor

        /// <summary>
        /// Gets a reference to the parent ComboEditorBase.
        /// </summary>
        public ComboEditorBase<T, TControl> ComboEditor
        {
            get
            {
                return this._comboEditor;
            }

            internal set
            {
                this._comboEditor = value;
            }
        }

        #endregion //ComboEditor

		#region FixedRowsTop

		/// <summary>
		/// Gets the rows that are currently fixed to the top of the Viewport.
		/// </summary>
		public virtual List<ComboRowBase> FixedRowsTop
		{
			get { return null; }
		}

		#endregion // FixedRowsBottom

        #endregion //Public

        #region Protected

        #region UseAvailableWidthToMeasureItem
        /// <summary>
        /// Gets whether an item should measured with the available width or Infinity
        /// </summary>
        protected virtual bool UseAvailableWidthToMeasureItem
        {
            get
            {
                return false;
            }
        }
        #endregion // UseAvailableWidthToMeasureItem

        #endregion // Protected

        #region Internal

        #region VisibleItems

        /// <summary>
        /// Gets or sets the the current visible ComboEditorItem objects.
        /// </summary>
        internal Collection<T> VisibleItems
        {
            get;
            set;
        }

        #endregion //VisibleItems

        #region ScrollItemIntoViewInProgress

        internal bool ScrollItemIntoViewInProgress
        {
            get;
            private set;
        }

        #endregion // ScrollItemIntoViewInProgress   

        internal bool InLayoutPhase
        {
            get;
            set;
        }
        internal bool MeasureCalled
        {
            get;
            set;
        }

        internal bool HorizontalScrollBarJustMadeVisible
        {
            get;
            set;
        }


        #endregion //Internal

        #endregion //Properties

        #region Methods

        #region Private

        #region ReleaseItem

        /// <summary>
        /// Removes the attached ComboEditorItem
        /// </summary>
        /// <param name="comboEditorItem">The ComboEditorItem</param>
        private void ReleaseItem(ISupportRecycling comboEditorItem)
        {
            RecyclingManager.Manager.ReleaseElement(comboEditorItem, this);
        }

        #endregion // ReleaseItem

        #region ThrowoutUnusedItems

        /// <summary>
        /// Removes the unused items.
        /// </summary>
        /// <param name="previousVisibleItems">List of <see cref="ComboEditorItem"/> objects.</param>
        private void ThrowoutUnusedItems(List<T> previousVisibleItems)
        {
            foreach (T item in previousVisibleItems)
            {
                if (!this.VisibleItems.Contains(item))
                {
                    if (item.Control != null)
                    {
                        this.ReleaseItem(item);
                    }
                }
            }
        }

        #endregion // ThrowoutUnusedItems

        #region UpdateScrollInfo

        /// <summary>
        /// Updates the ScrollInfo of the <see cref="ItemsPanel"/>.
        /// Such as changing the horizontal/vertical scrollbar visibility, or their viewport size.
        /// </summary>
        private void UpdateScrollInfo(int totalRowCount, Size finalSize)
        {
            ScrollBar vertBar = this.ComboEditor.VerticalScrollBar;
            if (vertBar != null)
            {
                double val = vertBar.Value;

                vertBar.Maximum = this._overrideVerticalMax;

                // So, the scrollbar has this weird bug, where sometimes
                // if you change the max, and the value is still within the max and min, it'll still change the 
                // value, even though it shouldn't have touched it. 
                if (vertBar.Value != val && val < vertBar.Maximum)
                    vertBar.Value = val;

                vertBar.ViewportSize = this.VisibleItems.Count;

                vertBar.LargeChange = vertBar.ViewportSize;
                vertBar.SmallChange = (double)vertBar.ViewportSize / 10;

                Visibility previous = vertBar.Visibility;
                vertBar.Visibility = ((vertBar.ViewportSize == totalRowCount && this._overrideVerticalMax < 1) || totalRowCount == 0) ? Visibility.Collapsed : Visibility.Visible;

                if (vertBar.Visibility != previous && vertBar.Visibility == Visibility.Collapsed)
                    vertBar.Value = 0;
            }

            ScrollBar horizBar = this.ComboEditor.HorizontalScrollBar;
            if (horizBar != null)
            {
                Visibility vis = horizBar.Visibility;

                this.UpdateHorizontalScrollBar(horizBar, finalSize);

                this.HorizontalScrollBarJustMadeVisible = (vis != horizBar.Visibility && horizBar.Visibility == Visibility.Visible);
            }

        }
        #endregion // UpdateScrollInfo

        #endregion //Private

        #region Protected

        #region RenderItem

        /// <summary>
        /// Creates a control for the row, and adds it as child of the panel. 
        /// </summary>
        /// <param name="comboEditorItem">The ComboEditorItem that will be rendered.</param>
        /// <param name="availableSize"></param>
        /// <returns>The new desired size.</returns>
		protected virtual Size RenderItem<T2, TControl2>(T2 comboEditorItem, Size availableSize)
			where T2 : ComboEditorItemBase<TControl2>
			where TControl2 : FrameworkElement
		{
			Size desiredSize = new Size();

			if (comboEditorItem.Control == null)
			{                
				RecyclingManager.Manager.AttachElement(comboEditorItem, this);
			}

			comboEditorItem.MeasureRaised = false;

            double width = double.PositiveInfinity;
            if (this.UseAvailableWidthToMeasureItem)
                width = availableSize.Width;

			if (comboEditorItem.Control != null)
			{
                this.OnControlAttachedToItem(comboEditorItem.Control as TControl);
				comboEditorItem.Control.Measure(new Size(width, double.PositiveInfinity));
				comboEditorItem.EnsureVisualStates();
				desiredSize = comboEditorItem.Control.DesiredSize;
			}


			if (!comboEditorItem.MeasureRaised )
			{
				comboEditorItem.Control.Measure(new Size(1, 1));
				comboEditorItem.Control.Measure(new Size(width, double.PositiveInfinity));
				desiredSize = comboEditorItem.Control.DesiredSize;
			}

			return desiredSize;
		}

        internal Size RenderItem(T comboEditorItem, Size availableSize)
        {
			return this.RenderItem<T, TControl>(comboEditorItem, availableSize);
        }

        #endregion // RenderItem

        #region ArrangeItem

        /// <summary>
        /// Arranges the specified item in the panel
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        protected virtual void ArrangeItem(FrameworkElement elem, double left, double top, double width)
        {
            elem.Arrange(new Rect(left, top, Math.Max(elem.DesiredSize.Width, width), elem.DesiredSize.Height));
        }

        #endregion // ArrangeItem

        #region OnControlAttachedToItem

        /// <summary>
        /// Raised when a control is attached to an item, before measure is called.
        /// </summary>
        /// <param name="cntrl"></param>
        protected virtual void OnControlAttachedToItem(TControl cntrl)
        {

        }
        #endregion // OnControlAttachedToItem

        #region UpdateHorizontalScrollBar

        /// <summary>
        /// Updates the horizontal scrollbar, based on how many items it can scroll horizontally
        /// </summary>
        /// <param name="horizBar"></param>
        /// <param name="finalSize"></param>
        protected virtual void UpdateHorizontalScrollBar(ScrollBar horizBar, Size finalSize)
        {
			if ((finalSize.Width < this._actualWidth) | this.ComboEditor.HorizontalScrollBar.Visibility == System.Windows.Visibility.Visible)
			{
                bool update = false;

                if (horizBar.Visibility == System.Windows.Visibility.Collapsed)
                    update = true;

                horizBar.Visibility = Visibility.Visible;

                horizBar.LargeChange = horizBar.ViewportSize;
                horizBar.SmallChange = (double)horizBar.ViewportSize / 10;

                horizBar.ViewportSize = finalSize.Width;
                horizBar.Maximum = this._actualWidth - finalSize.Width;

                // The height has most likely changed if scrollbar has been made visible
                // if thats the case, make sure we re-measure so that the dropdown position is properly calculated.
                if (update)
                    this.InvalidateMeasure();
            }
            else
            {
                horizBar.Visibility = Visibility.Collapsed;
                horizBar.Value = 0;
            }
        }
        
        #endregion // UpdateHorizontalScrollBar

        #endregion //Protected

        #region Internal

        internal void ScrollItemIntoView(T item, bool scrollToTop)
        {
			// JM 02-14-12 TFS101707 - Add null reference check.
			if (this.ComboEditor == null || this.ComboEditor.DataManager == null)
				return;

            int index = this.ComboEditor.DataManager.ResolveIndexForRecord(item.Data);

            bool hasControl = false;

            Rect itemLayout = Rect.Empty;

            // The item could have a control
            // However, it might not have been arranged yet, so if thats the case, then pretend it doesn't have a control.
            if (item.Control != null)
            {
                itemLayout = LayoutInformation.GetLayoutSlot(item.Control);

                if (itemLayout.Top > -1000)
                    hasControl = true;
            }

            // Is the item already in view?
            if (hasControl)
            {
                Rect panelLayout = LayoutInformation.GetLayoutSlot(this);

                if (scrollToTop)
                {
                    this.ComboEditor.VerticalScrollBar.Value = index;
                }
                else
                {
                    if ((itemLayout.Height + itemLayout.Top) > panelLayout.Height)
                    {
                        // If the row is at the bottom and not fully in view, then we need to reverse load the rows.
                        this._reverseItemStartIndex = index;
                        this._reverseMeasure = true;
                    }
                    else if (itemLayout.Top < 0)
                    {
                        // If the row is at the very top, and not fully in view, then simply set the scroll value to it's index
                        // so that it scrolls to the top of that row.
                        this.ComboEditor.VerticalScrollBar.Value = index;
                    }
                }
            }
            else
            {
                if (this.VisibleItems.Count > 0)
                {
                    int lastIndex;

                    if (scrollToTop)
                    {
                        this.ComboEditor.VerticalScrollBar.Value = index;
                    }
                    else
                    {
                        // Find the index of the last visible row. 
                        lastIndex = this.ComboEditor.DataManager.ResolveIndexForRecord(this.VisibleItems[this.VisibleItems.Count - 1].Data);
                        if (index > lastIndex)
                        {
                            // If the index of the last visible row, is less than the current row
                            // lets render backwards
                            this._reverseItemStartIndex = index;
                            this._reverseMeasure = true;
                        }
                        else
                        {
                            // this must mean that the row is towards the top, so lets render normally.
                            this.ComboEditor.VerticalScrollBar.Value = index;
                        }
                    }
                }
                else
                {
                    // So, we haven't loaded any items yet, but somone called scroll into view.
                    // So if the Value doesn't take, update the maximum and reset the value.
                    this.ComboEditor.VerticalScrollBar.Value = index;
                    if (this.ComboEditor.VerticalScrollBar.Value != index)
                    {
                        this.ComboEditor.VerticalScrollBar.Maximum = index + 1;
                        this.ComboEditor.VerticalScrollBar.Value = index;
                    }
                }

            }
            this._scrollIntoViewItem = item;
            this.ScrollItemIntoViewInProgress = true;
            this.InvalidateMeasure();
        }

        internal void ResetItems()
        {
            foreach (T item in this.VisibleItems)
                this.ReleaseItem(item);

            foreach (T item in this._lastItemsDetached)
                this.ReleaseItem(item);

            RecyclingManager.Manager.ReleaseAll(this, typeof(TControl));

            this.VisibleItems.Clear();
            this._lastItemsDetached.Clear();
            this._overrideVerticalMax = -1;
            this._overflowAdjustment = -1;
            this._actualWidth = 0; 
        }

        internal void DropDownClosed()
        {
            this._reverseMeasure = false;
        }

        #endregion // Internal

        #endregion //Methods

        #region EventHandlers

        void ItemsPanelBase_Loaded(object sender, RoutedEventArgs e)
        {
            this.InvalidateMeasure();
        }

        #endregion // EventHandlers
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