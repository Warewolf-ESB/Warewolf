



using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Documents;
using Infragistics.Windows.Themes;
using System.Diagnostics;
using Infragistics.Windows.Helpers;
using System.Windows.Input;
using System.Windows.Threading;
using System.ComponentModel;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Used in a PaneNavigatorControl to provide columnar layout for the Panes and Documents lists, with scrolling.
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class PaneNavigatorItemsPanel : Panel, 
										   IScrollInfo
	{
		#region Member Variables

		private ScrollData							_scrollingData;
		private PaneNavigatorItemsPanelAdorner		_paneNavigatorItemsPanelAdorner;
		private Selector							_selectorControl;
		private bool								_scrollUpButtonIsVisible;
		private bool								_scrollDownButtonIsVisible;
		private int									_lastArrangedChildIndex = -1;
		private UIElement							_lastArrangedChild = null;
		private bool								_inArrange;

		delegate void DeferredFocusDelegate(UIElement element);

		#endregion //Member Variables

		#region Constants

		private const int									MAXCOLUMNS_DEFAULT = 1;
		private const double								COLUMNWIDTH_DEFAULT = 180d;
		private const int									SCROLL_SMALL_CHANGE = 1;

		#endregion //Constants

		#region Resource Keys

			#region ScrollUpButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for the 'scroll up' button in the <see cref="PaneNavigatorItemsPanel"/>.
		/// </summary>
		public static readonly ResourceKey ScrollUpButtonStyleKey = new StaticPropertyResourceKey(typeof(PaneNavigatorItemsPanel), "ScrollUpButtonStyleKey");

			#endregion //ScrollUpButtonStyleKey

			#region ScrollDownButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for the 'scroll Down' button in the <see cref="PaneNavigatorItemsPanel"/>.
		/// </summary>
		public static readonly ResourceKey ScrollDownButtonStyleKey = new StaticPropertyResourceKey(typeof(PaneNavigatorItemsPanel), "ScrollDownButtonStyleKey");

			#endregion //ScrollDownButtonStyleKey

		#endregion //Resource Keys

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			// Set a flag which lets us know we are in the middle of Arrange processing.  We will check this flag in OnChildDesiredSizeChanged
			// on not pass on child size changes that we are causing in ArrangeOverride.
			this._inArrange = true;

			int			totalVisibleItems		= 0;
			int			firstVisibleItemIndex	= (int)this.VerticalOffset;
			int			currentColumn			= 0;
			double		currentHorizontalOffset	= 0d;
			double		currentVerticalOffset	= 0d;
			bool		allSlotsFilled			= false;
			Rect		lastArrangeRect			= Rect.Empty;
			int			totalChildren			= base.Children.Count;
			UIElementCollection children		= base.Children;


			// Initialize the first/last visible item attached properties on all our children.
			for (int i = 0; i < totalChildren; i++)
			{
				children[i].SetValue(PaneNavigatorItemsPanel.IsFirstVisibleItemPropertyKey, false);
				children[i].SetValue(PaneNavigatorItemsPanel.IsLastVisibleItemPropertyKey, false);
			}


			// Process all our children and arrange them.
			for (int i = 0; i < totalChildren; i++)
			{
				UIElement child = children[i];

				// If we are processing our first child, decide whether we need to show the ScrollUpButton in the adorner layer
				// depending on whether our first child is the first visible child.
				if (i == 0)
				{
					if (i != firstVisibleItemIndex)
					{
						this._paneNavigatorItemsPanelAdorner.ScrollUpButton.Arrange(new Rect(new Point(0, 0), new Size(this.ColumnWidth, this._paneNavigatorItemsPanelAdorner.ScrollUpButton.DesiredSize.Height)));
						this._scrollUpButtonIsVisible = true;
						currentVerticalOffset += this._paneNavigatorItemsPanelAdorner.ScrollUpButton.DesiredSize.Height;
					}
					else
					{
						this._paneNavigatorItemsPanelAdorner.ScrollUpButton.Arrange(new Rect(new Point(-5000, -5000), new Size(1, 1)));
						this._scrollUpButtonIsVisible = false;
					}
				}


				// If the child we are processing is before the first visible child or after the last child, set the child's
				// visibility to collapsed.
				if (i < firstVisibleItemIndex || allSlotsFilled)
				{
					if (KnownBoxes.VisibilityCollapsedBox.Equals(child.Visibility) == false)
						child.SetValue(FrameworkElement.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);

					continue;
				}


				// If the current child is the first visible item, set the IsFirstVisibleItem attached property 
				// on the child.
				if (i == firstVisibleItemIndex)
					child.SetValue(PaneNavigatorItemsPanel.IsFirstVisibleItemPropertyKey, true);


				if (KnownBoxes.VisibilityVisibleBox.Equals(child.Visibility) == false)
					child.SetValue(FrameworkElement.VisibilityProperty, KnownBoxes.VisibilityVisibleBox);


				// Try to arrange the child in the current column.  If there is not enough space in the current column, 
				// try the next column
				bool childArranged	= false;
				while (currentColumn < this.MaxColumns)
				{
					// If we can fit the current child in the curren tcolumn, arrange it now.
					if (currentVerticalOffset + child.DesiredSize.Height < finalSize.Height)
					{
						lastArrangeRect = new Rect(currentHorizontalOffset, currentVerticalOffset,
													this.ColumnWidth, child.DesiredSize.Height);
						child.Arrange(lastArrangeRect);
						this._lastArrangedChild			= child;
						this._lastArrangedChildIndex	= i;

						totalVisibleItems++;
						currentVerticalOffset	+= child.DesiredSize.Height;
						childArranged			= true;
						break;
					}
					else
					{
						// Since we can't fit the current child in the current column, bump the column number and
						// update some variables to prepare for trying the next column.
						currentColumn++;
						currentVerticalOffset	= 0;
						currentHorizontalOffset	+= this.ColumnWidth;
					}
				}


				// If we were not able to arrange the current child (i.e., there was not enough room in any of the columns)
				// set a flag that lets us know that all available slots have been used.  Also, arrange the ScrollDownButton in the
				// adorner layer in the same position as the previous child we arranged, and then collapse the previously arranged child.
				if (childArranged == false)
				{
					if (allSlotsFilled == false)
					{
						allSlotsFilled = true;
						this._paneNavigatorItemsPanelAdorner.ScrollDownButton.Arrange(new Rect(new Point(lastArrangeRect.Left, lastArrangeRect.Top), new Size(this.ColumnWidth, this._paneNavigatorItemsPanelAdorner.ScrollDownButton.DesiredSize.Height)));
						this._scrollDownButtonIsVisible = true;

						if (this._lastArrangedChild != null)
						{
							this._lastArrangedChild.SetValue(FrameworkElement.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
							this._lastArrangedChildIndex--;
							this._lastArrangedChild = this.Children[this._lastArrangedChildIndex];
							totalVisibleItems--;
						}
					}

					if (KnownBoxes.VisibilityCollapsedBox.Equals(child.Visibility) == false)
						child.SetValue(FrameworkElement.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
				}
			}


			// If the last child that was arranged is the last child in our children collection, make sure the previously shown
			// ScrollDownButton (if any) does not appear.
			if (totalChildren > 0  &&  this._lastArrangedChildIndex == (totalChildren - 1))
			{
				this._paneNavigatorItemsPanelAdorner.ScrollDownButton.Arrange(new Rect(new Point(-5000, -5000), new Size(1, 1)));
				this._scrollDownButtonIsVisible = false;
			}


			if (totalChildren > 0)
			{
				// Set the IsLastVisibleItem attached property on the last child we arranged.
				this._lastArrangedChild.SetValue(PaneNavigatorItemsPanel.IsLastVisibleItemPropertyKey, true);


				// Refine our scrolldata information now that we have arranged everything.
				this.UpdateScrollData(new Size((double)this.TotalItemCount, (double)this.TotalItemCount),
									  new Size((double)totalVisibleItems, (double)totalVisibleItems),
									  this.ScrollingData._offset);
			}


			this._inArrange = false;

			return finalSize;
		}

			#endregion //ArrangeOverride	
    
			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
		{
			int		firstVisibleItemIndex			= (int)this.VerticalOffset;
			int		currentColumn					= 0;
			double  currentHorizontalOffset			= 0d;
			double	currentVerticalOffset			= 0d;
			double	verticalExtentUsed				= 0d;
			double	remainingHeightCurrentColumn	= availableSize.Height;
			bool	allSlotsFilled					= false;
			int		totalChildren					= base.Children.Count;
			UIElementCollection children			= base.Children;


			// Create our adorner layer if we haven't yet done so.
			if (this._paneNavigatorItemsPanelAdorner== null)
				this.InitializeAdorner();


			for (int i = 0; i < totalChildren; i++)
			{
				UIElement child = children[i];

				if (i < firstVisibleItemIndex || allSlotsFilled)
				{
					child.Measure(new Size(this.ColumnWidth, double.PositiveInfinity));
					continue;
				}

				bool childInView	= false;
				child.Measure(new Size(this.ColumnWidth, double.PositiveInfinity));

				while (currentColumn < this.MaxColumns)
				{
					if (currentVerticalOffset + child.DesiredSize.Height < availableSize.Height)
					{
						currentVerticalOffset	+= child.DesiredSize.Height;
						verticalExtentUsed		= Math.Max(verticalExtentUsed, currentVerticalOffset);
						childInView				= true;
						break;
					}
					else
					{
						if ((currentColumn + 1) < this.MaxColumns)
						{
							currentColumn++;
							currentVerticalOffset = 0;
							currentHorizontalOffset += this.ColumnWidth;
						}
						else
							break;
					}
				}

				if (childInView == false)
					allSlotsFilled	= true;
			}


			double heightUsed = availableSize.Height;
			if (currentColumn == 0  &&  allSlotsFilled == true)
				heightUsed = verticalExtentUsed;
			else if (double.IsPositiveInfinity(availableSize.Height)) // AS 7/17/12 TFS115804
				heightUsed = verticalExtentUsed;
			

			return new Size((currentColumn + 1) * this.ColumnWidth, heightUsed);
		}

			#endregion //MeasureOverride	

			#region OnChildDesiredSizeChanged

		/// <summary>
		/// Invoked when the desired size of a child has been changed.
		/// </summary>
		/// <param name="child">The child whose size is being changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			if (this._inArrange == true)
				return;

			base.OnChildDesiredSizeChanged(child);
		}

			#endregion //OnChildDesiredSizeChanged	
    
			#region OnPreviewKeyDown

		/// <summary>
		/// Invoked when a key is pressed
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Down:
				case Key.Up:
					bool			scrollingEnabled	= e.Key == Key.Down ?	this._scrollDownButtonIsVisible :
																				this._scrollUpButtonIsVisible;
					UIElement		referenceElement	= e.Key == Key.Down ?	this._lastArrangedChild : this.Children[(int)this.VerticalOffset];
					UIElement		focusedElement		= e.OriginalSource as UIElement;
					int				focusedElementIndex = this.Children.IndexOf(focusedElement);

					if (scrollingEnabled)
					{
						if (focusedElement == referenceElement)
						{
							UIElement newFocusedElement;
							if (e.Key == Key.Down)
								newFocusedElement = this.Children[Math.Min(this.Children.Count - 1, focusedElementIndex + 1)];
							else
								newFocusedElement = this.Children[Math.Max(0, focusedElementIndex- 1)];

							this.EnsureItemIsInView(newFocusedElement);
							newFocusedElement.Focus();
							this.SelectorControl.SelectedItem = ((ListBoxItem)newFocusedElement).Content;

							e.Handled = true;
							return;
						}
					}
					else
					{
						if (focusedElement == referenceElement)
						{
							if (e.Key == Key.Down)
								referenceElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
							else
								referenceElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));

							e.Handled = true;
							return;
						}
					}

					break;
			}


			base.OnPreviewKeyDown(e);
		}

			#endregion //OnPreviewKeyDown

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region ColumnWidth

		/// <summary>
		/// Identifies the <see cref="ColumnWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register("ColumnWidth",
			typeof(double), typeof(PaneNavigatorItemsPanel), new FrameworkPropertyMetadata(COLUMNWIDTH_DEFAULT), new ValidateValueCallback(OnValidateColumnWidth));

		private static bool OnValidateColumnWidth(object value)
		{
			return (double)value > 0;
		}

		/// <summary>
		/// Returns/sets the width of each column in the list.
		/// </summary>
		/// <seealso cref="ColumnWidthProperty"/>
		/// <seealso cref="MaxColumns"/>
		public double ColumnWidth
		{
			get
			{
				return (double)this.GetValue(PaneNavigatorItemsPanel.ColumnWidthProperty);
			}
			set
			{
				this.SetValue(PaneNavigatorItemsPanel.ColumnWidthProperty, value);
			}
		}

				#endregion //ColumnWidth

				#region MaxColumns

		/// <summary>
		/// Identifies the <see cref="MaxColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register("MaxColumns",
			typeof(int), typeof(PaneNavigatorItemsPanel), new FrameworkPropertyMetadata(MAXCOLUMNS_DEFAULT), new ValidateValueCallback(OnValidateMaxColumns));

		private static bool OnValidateMaxColumns(object value)
		{
			return (int)value > 0;
		}

		/// <summary>
		/// Returns/sets the maximum number of columns to display in the panel.  Items in the associated list will be arranged in 'at most' this number of columns depending on the number of items in the list and the vertical space available.
		/// </summary>
		/// <seealso cref="MaxColumnsProperty"/>
		/// <seealso cref="ColumnWidth"/>
		public int MaxColumns
		{
			get
			{
				return (int)this.GetValue(PaneNavigatorItemsPanel.MaxColumnsProperty);
			}
			set
			{
				this.SetValue(PaneNavigatorItemsPanel.MaxColumnsProperty, value);
			}
		}

				#endregion //MaxColumns

			#endregion //Public Properties

			#region Private Properties

				#region Selector

		internal Selector SelectorControl
		{
			get
			{
				if (this._selectorControl == null)
					this._selectorControl = ItemsControl.GetItemsOwner(this) as Selector;

				return this._selectorControl;
			}
		}

				#endregion //Selector

				#region ScrollingData

		internal ScrollData ScrollingData
		{
			get
			{
				if (this._scrollingData == null)
					this._scrollingData = new ScrollData();

				return this._scrollingData;
			}
		}

				#endregion //#region ScrollingData

				#region ScrollLargeChange

		internal double ScrollLargeChange
		{
			get
			{
				return this.ScrollingData._viewport.Height;
			}
		}

				#endregion //ScrollLargeChange	

				#region TotalItemCount

		private int TotalItemCount
		{
			get { return base.Children.Count; }
		}

				#endregion //TotalItemCount	
    
			#endregion //Private Properties

			#region Internal Properties

				#region IsFirstVisibleItem (attached)

		private static readonly DependencyPropertyKey IsFirstVisibleItemPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsFirstVisibleItem",
			typeof(bool), typeof(PaneNavigatorItemsPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		internal static readonly DependencyProperty IsFirstVisibleItemProperty =
			IsFirstVisibleItemPropertyKey.DependencyProperty;

		internal static bool GetIsFirstVisibleItem(DependencyObject d)
		{
			return (bool)d.GetValue(PaneNavigatorItemsPanel.IsFirstVisibleItemProperty);
		}

				#endregion //IsFirstVisibleItem (attached)

				#region IsLastVisibleItem (attached)

		private static readonly DependencyPropertyKey IsLastVisibleItemPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsLastVisibleItem",
			typeof(bool), typeof(PaneNavigatorItemsPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		internal static readonly DependencyProperty IsLastVisibleItemProperty =
			IsLastVisibleItemPropertyKey.DependencyProperty;

		internal static bool GetIsLastVisibleItem(DependencyObject d)
		{
			return (bool)d.GetValue(PaneNavigatorItemsPanel.IsLastVisibleItemProperty);
		}

				#endregion //IsLastVisibleItem (attached)

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region InitializeAdorner

		private void InitializeAdorner()
		{
			if (this._paneNavigatorItemsPanelAdorner != null)
				return;

			this._paneNavigatorItemsPanelAdorner = new PaneNavigatorItemsPanelAdorner(this);

			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
			if (adornerLayer != null)
				adornerLayer.Add(this._paneNavigatorItemsPanelAdorner);
		}

				#endregion //InitializeAdorner	

				#region OnScrollInfoChange







		private void OnScrollInfoChange()
		{
			if (this.ScrollingData._scrollOwner != null)
				this.ScrollingData._scrollOwner.InvalidateScrollInfo();
		}

				#endregion OnScrollInfoChange

				#region UpdateScrollData







		private void UpdateScrollData(Size newExtent, Size newViewport, Vector newOffset)
		{
			if (this.ScrollingData._extent		!= newExtent	||
				this.ScrollingData._viewport	!= newViewport	||
				this.ScrollingData._offset		!= newOffset)
			{
				// Update scrolling data since something has changed.
				this.ScrollingData._extent		= newExtent;
				this.ScrollingData._viewport	= newViewport;
				this.ScrollingData._offset		= newOffset;
			}
		}

				#endregion //UpdateScrollData

			#endregion //Private Methods

			#region Internal Methods

				#region AddLogicalChildInternal

		internal void AddLogicalChildInternal(DependencyObject child)
		{
			this.AddLogicalChild(child);
		}

				#endregion //AddLogicalChildInternal	

				#region EnsureItemIsInView

		internal void EnsureItemIsInView(UIElement elementToMakeVisible)
		{
			int indexOfElementToMakeVisible = this.Children.IndexOf(elementToMakeVisible);
			if (indexOfElementToMakeVisible != -1)
			{
				int selectedIndex = this.SelectorControl.SelectedIndex;
				if (selectedIndex != -1)
				{
					if (selectedIndex < indexOfElementToMakeVisible)
					{
						while (indexOfElementToMakeVisible > this._lastArrangedChildIndex)
						{
							this.SetVerticalOffset(Math.Min(this.Children.Count - 1, this.VerticalOffset + (indexOfElementToMakeVisible - this._lastArrangedChildIndex)));
							this.UpdateLayout();
						}
					}
					else
					{
						int firstArrangedChildIndex = (int)this.VerticalOffset;
						if (firstArrangedChildIndex > indexOfElementToMakeVisible)
						{
							this.SetVerticalOffset(Math.Max(this.VerticalOffset - (firstArrangedChildIndex - indexOfElementToMakeVisible), 0));
							this.UpdateLayout();
						}
					}
				}
			}
		}

				#endregion //EnsureItemIsInView

			#endregion //Internal Methods

		#endregion //Methods

		#region IScrollInfo Members

			#region ExtentHeight, ExtentHeight

		/// <summary>
		/// Returns the overall logical height of the scrollable area. (read only)
		/// </summary>
		public double ExtentHeight
		{
			get
			{
				return this.ScrollingData._extent.Height;
			}
		}

		/// <summary>
		/// Returns the overall logical width of the scrollable area. (read only)
		/// </summary>
		public double ExtentWidth
		{
			get
			{
				return this.ScrollingData._extent.Width;
			}
		}

			#endregion //ExtentWidth, ExtentHeight

			#region HorizontalOffset, VerticalOffset

		/// <summary>
		/// Returns the logical horizontal offset of the scrollable area. (read only)
		/// </summary>
		public double HorizontalOffset
		{
			get
			{
				// Since we are only supporting scrolling in one direction always return vertical offsets.
				return this.ScrollingData._offset.Y;
			}
		}

		/// <summary>
		/// Returns the logical vertical offset of the scrollable area. (read only)
		/// </summary>
		public double VerticalOffset
		{
			get
			{
				return this.ScrollingData._offset.Y;
			}
		}

			#endregion //HorizontalOffset, VerticalOffset

			#region SetHorizontalOffset, SetVerticalOffset

		/// <summary>
		/// Sets the horizontal scroll offset.
		/// </summary>
		/// <param name="offset">The new horizontal scroll offset.</param>
		public void SetHorizontalOffset(double offset)
		{
			// Since we are only supporting scrolling in one direction always set the vertical offset.
			this.SetVerticalOffset(offset);
		}

		/// <summary>
		/// Sets the vertical scroll offset.
		/// </summary>
		/// <param name="offset">The new vertical scroll offset.</param>
		public void SetVerticalOffset(double offset)
		{
			ScrollData	scrollingData		= this.ScrollingData;
			double		newOffsetNormalized = Math.Min(Math.Max(offset, 0), scrollingData._viewport.Height - 1);

			scrollingData._offset.Y = offset;
			this.InvalidateMeasure();
		}

			#endregion //SetHorizontalOffset, SetVerticalOffset

			#region LineDown, LineUp, LineLeft, LineRight

		/// <summary>
		/// Scrolls down 1 line.
		/// </summary>
		public void LineDown()
		{
			this.SetVerticalOffset(this.VerticalOffset + SCROLL_SMALL_CHANGE);
		}

		/// <summary>
		/// Scrolls left 1 line.
		/// </summary>
		public void LineLeft()
		{
			this.SetHorizontalOffset(this.HorizontalOffset - SCROLL_SMALL_CHANGE);
		}

		/// <summary>
		/// Scrolls right 1 line.
		/// </summary>
		public void LineRight()
		{
			this.SetHorizontalOffset(this.HorizontalOffset + SCROLL_SMALL_CHANGE);
		}

		/// <summary>
		/// Scrolls up 1 line.
		/// </summary>
		public void LineUp()
		{
			this.SetVerticalOffset(this.VerticalOffset - SCROLL_SMALL_CHANGE);
		}

			#endregion //LineDown, LineUp, LineLeft, LineRight

			#region MouseWheelDown, MouseWheelUp, MouseWheelLeft, MouseWheelRight

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelDown.
		/// </summary>
		public void MouseWheelDown()
		{
			this.SetVerticalOffset(this.VerticalOffset + SCROLL_SMALL_CHANGE);
		}

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelLeft.
		/// </summary>
		public void MouseWheelLeft()
		{
			this.SetVerticalOffset(this.VerticalOffset - SCROLL_SMALL_CHANGE);
		}

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelRight.
		/// </summary>
		public void MouseWheelRight()
		{
			this.SetVerticalOffset(this.VerticalOffset + SCROLL_SMALL_CHANGE);
		}

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelUp.
		/// </summary>
		public void MouseWheelUp()
		{
			this.SetVerticalOffset(this.VerticalOffset - SCROLL_SMALL_CHANGE);
		}

			#endregion //MouseWheelDown, MouseWheelUp, MouseWheelLeft, MouseWheelRight

			#region PageUp, PageDown, PageLeft, PageRight

		/// <summary>
		/// Scrolls down 1 page.
		/// </summary>
		public void PageDown()
		{
			this.SetVerticalOffset(this.VerticalOffset + this.ScrollLargeChange);
		}

		/// <summary>
		/// Scrolls left 1 page.
		/// </summary>
		public void PageLeft()
		{
			this.SetHorizontalOffset(this.HorizontalOffset - this.ScrollLargeChange);
		}

		/// <summary>
		/// Scrolls right 1 page.
		/// </summary>
		public void PageRight()
		{
			this.SetHorizontalOffset(this.HorizontalOffset + this.ScrollLargeChange);
		}

		/// <summary>
		/// Scrolls up 1 page.
		/// </summary>
		public void PageUp()
		{
			this.SetVerticalOffset(this.VerticalOffset - this.ScrollLargeChange);
		}

			#endregion //PageUp, PageDown, PageLeft, PageRight

			#region ScrollOwner

		/// <summary>
		/// Returns/sets the scroll owner.
		/// </summary>
		public ScrollViewer ScrollOwner
		{
			get
			{
				return this.ScrollingData._scrollOwner;
			}
			set
			{
				if (value != this.ScrollingData._scrollOwner)
				{
					this.ScrollingData.Reset();
					this.ScrollingData._scrollOwner = value;
				}
			}
		}

			#endregion //ScrollOwner

			#region ViewportHeight, ViewportWidth

		/// <summary>
		/// Returns the height of the Viewport. (read only)
		/// </summary>
		public double ViewportHeight
		{
			get
			{
				return this.ScrollingData._viewport.Height;
			}
		}

		/// <summary>
		/// Returns the width of the Viewport. (read only)
		/// </summary>
		public double ViewportWidth
		{
			get
			{
				return this.ScrollingData._viewport.Width;
			}
		}

			#endregion //ViewportHeight, ViewportWidth

			#region MakeVisible

		/// <summary>
		/// Ensures that the supplied parent is visible.
		/// </summary>
		/// <param name="visual">The element to make visible.</param>
		/// <param name="rectangle">The rectangle within the parent to make visible.</param>
		/// <returns>The rectangle that was actually made visible.</returns>
		/// <remarks>
		/// <p class="note"><b>Note: </b>When this method is called the specified parent is assumed to be a CarouselPanelItem.
		/// If it is, the entire item is scrolled into view if necessary and the rectangle paramater is ignored.</p>
		/// </remarks>
		public Rect MakeVisible(Visual visual, Rect rectangle)
		{
			return new Rect();
		}

			#endregion //MakeVisible

			#region CanVerticallyScroll, CanHorizontallyScroll

		/// <summary>
		/// Returns/sets whether vertical scrolling can be performed.
		/// </summary>
		bool IScrollInfo.CanVerticallyScroll
		{
			get
			{

				return this.ScrollingData._canVerticallyScroll;
			}
			set
			{
				if (this.ScrollingData._canVerticallyScroll != value)
				{
					this.ScrollingData._canVerticallyScroll = value;
					this.InvalidateMeasure();
				}
			}
		}

		/// <summary>
		/// Returns/sets whether horizontal scrolling can be performed.
		/// </summary>
		bool IScrollInfo.CanHorizontallyScroll
		{
			get
			{

				return this.ScrollingData._canHorizontallyScroll;
			}
			set
			{
				if (this.ScrollingData._canHorizontallyScroll != value)
				{
					this.ScrollingData._canHorizontallyScroll = value;
					this.InvalidateMeasure();
				}
			}
		}

			#endregion //CanVerticallyScroll, CanHorizontallyScroll

		#endregion //IScrollInfo Members

		#region Nested Classes

			#region ScrollData Internal Class

		internal class ScrollData
		{
			#region Member Variables

			internal ScrollViewer						_scrollOwner;
			internal Size								_extent = new Size();
			internal Size								_viewport = new Size();
			internal Vector								_offset = new Vector();
			internal bool								_canHorizontallyScroll;
			internal bool								_canVerticallyScroll;

			#endregion //Member Variables

			#region Methods

				#region Reset

			internal void Reset()
			{
				this._offset		= new Vector();
				this._extent		= new Size();
				this._viewport		= new Size();
			}

				#endregion //Reset

			#endregion //Methods
		}

			#endregion //ScrollData Internal Class

		#endregion //Nested Classes
	}

	#region PaneNavigatorItemsPanelAdorner class

	internal class PaneNavigatorItemsPanelAdorner : Adorner
	{
		#region Member Variables

		private PaneNavigatorItemsPanel		_paneNavigatorItemsPanel;
		private Button						_scrollUpButton;
		private Button						_scrollDownButton;
		private List<UIElement>				_children;
		private Point						_scrollDownButtonLocation = new Point(0, 0);
		private double						_scrollDownButtonWidth = 0d;
		private Point						_scrollUpButtonLocation = new Point(0, 0);
		private double						_scrollUpButtonWidth = 0d;

		#endregion //Member Variables

		#region Constructor

		internal PaneNavigatorItemsPanelAdorner(UIElement adornedElement)
			: base(adornedElement)
		{
			this._paneNavigatorItemsPanel = adornedElement as PaneNavigatorItemsPanel;
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			return finalSize;
		}

			#endregion //ArrangeOverride

			#region GetVisualChild

		/// <summary>
		/// Gets the visual child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child visual.</param>
		/// <returns>The visual child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
			if (index < 0 || index >= this.Children.Count)
				return null;

			return this.Children[index];
		}

			#endregion //GetVisualChild

			#region VisualChildrenCount

		/// <summary>
		/// Returns the total numder of visual children (read-only).
		/// </summary>
		protected override int VisualChildrenCount
		{
			get { return this.Children.Count; }
		}

			#endregion //VisualChildrenCount

		#endregion //Base Class Overrides

		#region Properties

			#region Children

		private List<UIElement> Children
		{
			get
			{
				if (this._children == null)
					this._children = new List<UIElement>(2);

				return this._children;
			}
		}

			#endregion //Children

			#region ScrollDownButton

		internal Button ScrollDownButton
		{
			get
			{
				if (this._scrollDownButton == null)
				{
					this._scrollDownButton = new Button();

					this.AddVisualChild(this._scrollDownButton);
					this.Children.Add(this._scrollDownButton);

					if (this._paneNavigatorItemsPanel != null)
					{
						DependencyObject logicalParent = LogicalTreeHelper.GetParent(this._scrollDownButton);
						if (logicalParent == null)
							this._paneNavigatorItemsPanel.AddLogicalChildInternal(this._scrollDownButton);
					}

					this._scrollDownButton.SetResourceReference(StyleProperty, PaneNavigatorItemsPanel.ScrollDownButtonStyleKey);
					this._scrollDownButton.InvalidateMeasure();
					this._scrollDownButton.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					if (this._scrollDownButton.DesiredSize.Width == 0 || this._scrollDownButton.DesiredSize.Height == 0)
						this._scrollDownButton.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

					this._scrollDownButton.Focusable		= false;
					this._scrollDownButton.IsHitTestVisible	= true;
					this._scrollDownButton.Click			+= new RoutedEventHandler(OnScrollDownButtonClick);
				}

				return this._scrollDownButton;
			}
		}

			#endregion //ScrollDownButton	

			#region ScrollUpButton

		internal Button ScrollUpButton
		{
			get
			{
				if (this._scrollUpButton == null)
				{
					this._scrollUpButton = new Button();

					this.AddVisualChild(this._scrollUpButton);
					this.Children.Add(this._scrollUpButton);

					if (this._paneNavigatorItemsPanel != null)
					{
						DependencyObject logicalParent = LogicalTreeHelper.GetParent(this._scrollUpButton);
						if (logicalParent == null)
							this._paneNavigatorItemsPanel.AddLogicalChildInternal(this._scrollUpButton);
					}


					this._scrollUpButton.SetResourceReference(StyleProperty, PaneNavigatorItemsPanel.ScrollUpButtonStyleKey);
					this._scrollUpButton.InvalidateMeasure();
					this._scrollUpButton.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					if (this._scrollUpButton.DesiredSize.Width == 0 || this._scrollUpButton.DesiredSize.Height == 0)
						this._scrollUpButton.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));


					this._scrollUpButton.Focusable			= false;
					this._scrollUpButton.IsHitTestVisible	= true;
					this._scrollUpButton.Click				+= new RoutedEventHandler(OnScrollUpButtonClick);
				}

				return this._scrollUpButton;
			}
		}

			#endregion //ScrollUpButton	
    
		#endregion //Properties

		#region Methods

			#region OnScrollDownButtonClick

		private void OnScrollDownButtonClick(object sender, RoutedEventArgs e)
		{
			IScrollInfo iScrollInfo = this._paneNavigatorItemsPanel as IScrollInfo;
			iScrollInfo.SetVerticalOffset(iScrollInfo.VerticalOffset + 1);
		}

			#endregion //OnScrollDownButtonClick	
    
			#region OnScrollUpButtonClick

		private void OnScrollUpButtonClick(object sender, RoutedEventArgs e)
		{
			IScrollInfo iScrollInfo = this._paneNavigatorItemsPanel as IScrollInfo;
			iScrollInfo.SetVerticalOffset(iScrollInfo.VerticalOffset - 1);
		}

			#endregion //OnScrollUpButtonClick	
    
			#region PositionScrollDownButton

		internal void PositionScrollDownButton(Point location, double width)
		{
			this._scrollDownButtonLocation	= location;
			this._scrollDownButtonWidth		= width;
		}

			#endregion //PositionScrollDownButton	

			#region PositionScrollUpButton

		internal void PositionScrollUpButton(Point location, double width)
		{
			this._scrollUpButtonLocation	= location;
			this._scrollUpButtonWidth		= width;
		}

			#endregion //PositionScrollUpButton	
    
		#endregion //Methods
	}

	#endregion //PaneNavigatorItemsPanelAdorner class
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