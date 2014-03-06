using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Collections;
using Infragistics.Shared;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DockManager;
using Infragistics.Collections;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// A pane used in a <see cref="XamDockManager"/> that can display one or more panes stacked horizontally or vertically.
	/// </summary>
	[ContentProperty("Panes")]
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class SplitPane : FrameworkElement
		, ISplitElementCollectionOwner
		, IContentPaneContainer
		, IPaneContainer
	{
		#region Member Variables

		private ObservableSplitElementCollection<FrameworkElement> _panes;
		private List<PaneInfo> _paneInfos;
		private bool _isMeasuringChild = false;
		private static readonly Size DefaultSplitterSize = new Size(4d, 4d);

		// AS 10/5/09 NA 2010.1 - LayoutMode
		private static readonly object NaN = double.NaN;
		private object _uncoercedWidth;
		private object _uncoercedHeight;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="SplitPane"/>
		/// </summary>
		public SplitPane()
		{
			this._panes = new ObservableSplitElementCollection<FrameworkElement>(this, PaneSplitterMode.BetweenPanes);
			this._panes.CollectionChanged += new NotifyCollectionChangedEventHandler(OnChildrenCollectionChanged);
		}

		static SplitPane()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitPane), new FrameworkPropertyMetadata(typeof(SplitPane)));

			XamDockManager.PaneLocationPropertyKey.OverrideMetadata(typeof(SplitPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPaneLocationChanged)));

			// we need to change the visible to collapsed if all children are collapsed
			UIElement.VisibilityProperty.OverrideMetadata(typeof(SplitPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisibilityChanged), new CoerceValueCallback(CoerceVisibility)));
			EventManager.RegisterClassHandler(typeof(SplitPane), DockManagerUtilities.VisibilityChangedEvent, new RoutedEventHandler(OnChildVisibilityChanged));

			// AS 10/5/09 NA 2010.1 - LayoutMode
			FrameworkElement.WidthProperty.OverrideMetadata(typeof(SplitPane), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceWidth)));
			FrameworkElement.HeightProperty.OverrideMetadata(typeof(SplitPane), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceHeight)));
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			// AS 4/25/08
			// Do not store the size of the split group. Instead we should be storing the size of
			// the pane itself within the group.
			//
			//DockManagerUtilities.InitializePaneFloatingSize(this, this.Panes, finalSize);

			List<PaneInfo> panes = this._paneInfos;
			PrepareForArrange(panes, finalSize, this.SplitterOrientation);

			bool isVerticallyArranged = this.SplitterOrientation == Orientation.Horizontal;
			bool isFloating = DockManagerUtilities.IsFloating(XamDockManager.GetPaneLocation(this));

			// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
			PaneToolWindow tw = ToolWindow.GetToolWindow(this) as PaneToolWindow;
			WindowState windowState = tw == null ? WindowState.Normal : tw.WindowState;

			for (int i = 0, count = (panes.Count * 2 - 1); i < count; i++)
			{
				bool isSplitter = i % 2 == 1;
				int elementIndex = i / 2;
				PaneInfo pane = panes[elementIndex];
				FrameworkElement element;
				Rect arrangeRect;

				if (isSplitter)
				{
					element = pane.Splitter;
					arrangeRect = pane.SplitterRect;
				}
				else
				{
					element = pane.Pane;
					arrangeRect = pane.PaneRect;
				}

				if (null == element)
					continue;

				bool measure = false;

				// AS 10/13/10 TFS37022
				// This isn't specific to this issue but we shouldn't measure if the values are close.
				//
				if (isVerticallyArranged)
					measure = arrangeRect.Height < element.DesiredSize.Height && !DockManagerUtilities.AreClose(arrangeRect.Height, element.DesiredSize.Height);
				else
					measure = arrangeRect.Width < element.DesiredSize.Width && !DockManagerUtilities.AreClose(arrangeRect.Width, element.DesiredSize.Width);

				if (measure)
				{
					this._isMeasuringChild = true;
					element.Measure(arrangeRect.Size);
					this._isMeasuringChild = false;
				}

				// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//
				//// AS 4/25/08
				//// Store the size of the element itself.
				////
				//if (isFloating && element is ContentPane)
				if (isFloating && windowState == WindowState.Normal && element is ContentPane)
				{
					// AS 6/24/11 FloatingWindowCaptionSource
					// Like we do for tab groups we should add in the non client size.
					//
					//((ContentPane)element).LastFloatingSize = arrangeRect.Size;
					Size floatingSize = arrangeRect.Size;

					if (tw != null && tw.IsUsingOSNonClientArea == false)
						floatingSize = tw.AddNonClientSize(floatingSize); 
					
					((ContentPane)element).LastFloatingSize = floatingSize;
				}

				element.Arrange(arrangeRect);
			}

			return finalSize;
		}
		#endregion //ArrangeOverride

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns <see cref="SplitPane"/> Automation Peer Class <see cref="SplitPaneAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SplitPaneAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer

		#region GetVisualChild
		/// <summary>
		/// Returns the visual child at the specified index.
		/// </summary>
		/// <param name="index">Integer position of the child to return.</param>
		/// <returns>The child element at the specified position.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is greater than the <see cref="VisualChildrenCount"/></exception>
		protected override Visual GetVisualChild(int index)
		{
			Visual child = this._panes.GetVisualChild(index);

			Debug.Assert(null != child && this == VisualTreeHelper.GetParent(child));

			return child;
		}
		#endregion //GetVisualChild

		#region LogicalChildren
		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				// AS 5/27/08 BR33298
				// The root document host split pane can no longer include the root 
				// splits as logical children since they are logical children of the 
				// documentcontenthost itself.
				//
				if (this.IncludePanesAsLogicalChildren)
				{
					return new MultiSourceEnumerator(this._panes.GetSplitterEnumerator(),
						this._panes.GetEnumerator());
				}

				return this._panes.GetSplitterEnumerator();
			}
		} 
		#endregion //LogicalChildren

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			List<PaneInfo> panes = new List<PaneInfo>();

			bool isVerticallyArranged = this.SplitterOrientation == Orientation.Horizontal;
			Size desiredSize = new Size();
			double totalWeight = 0d;

			#region Splitters
			// first measure the splitters so we can take away the extent from the
			for (int i = 0, count = this._panes.Count; i < count; i++)
			{
				FrameworkElement element = this._panes[i];

				if (element.Visibility != Visibility.Collapsed)
				{
					PaneInfo pane = new PaneInfo();
					pane.Pane = element;
					Size relativeSize = SplitPane.GetRelativeSize(element);

					// AS 4/7/08 BR31677
					//pane.Weight = Math.Min(1, isVerticallyArranged ? relativeSize.Height : relativeSize.Width);
					pane.Weight = Math.Max(isVerticallyArranged ? relativeSize.Height : relativeSize.Width, 1);
					panes.Add(pane);

					// track the sum of the weights
					totalWeight += pane.Weight;

					PaneSplitter splitter = PaneSplitter.GetSplitter(element);

					if (splitter != null)
					{
						splitter.Measure(availableSize);

						pane.Splitter = splitter;
						pane.SplitterSize = splitter.DesiredSize;

						Size elementDesiredSize = pane.SplitterSize;

						if (isVerticallyArranged)
						{
							desiredSize.Height += elementDesiredSize.Height;

							if (desiredSize.Width < elementDesiredSize.Width)
								desiredSize.Width = elementDesiredSize.Width;
						}
						else
						{
							desiredSize.Width += elementDesiredSize.Width;

							if (desiredSize.Height < elementDesiredSize.Height)
								desiredSize.Height = elementDesiredSize.Height;
						}
					}
				}
			} 
			#endregion //Splitters

			#region Remove Splitter Extent From Available

			bool isInfiniteWidth = double.IsPositiveInfinity(availableSize.Width);
			bool isInfiniteHeight = double.IsPositiveInfinity(availableSize.Height);

			// reduce the available size by the splitter size
			if (isVerticallyArranged && isInfiniteHeight == false)
				availableSize.Height = Math.Max(availableSize.Height - desiredSize.Height, 0);
			else if (false == isVerticallyArranged && isInfiniteWidth == false)
				availableSize.Width = Math.Max(availableSize.Width - desiredSize.Width, 0);

			#endregion //Remove Splitter Extent From Available

			bool adjustHeight = isVerticallyArranged && isInfiniteHeight == false;
			bool adjustWidth = isVerticallyArranged == false && isInfiniteWidth == false;

			// measure with weight based sizes
			for (int i = 0, count = panes.Count; i < count; i++)
			{
				PaneInfo pane = panes[i];
				FrameworkElement element = pane.Pane;

				Size elementSize = availableSize;

				double percent = pane.Weight / totalWeight;

				if (adjustHeight)
					elementSize.Height *= percent;
				else if (adjustWidth)
					elementSize.Width *= percent;

				element.Measure(elementSize);

				Size elementDesiredSize = element.DesiredSize;

				if (isVerticallyArranged)
				{
					desiredSize.Height += elementDesiredSize.Height;
					desiredSize.Width = Math.Max(desiredSize.Width, elementDesiredSize.Width);
				}
				else
				{
					desiredSize.Width += elementDesiredSize.Width;
					desiredSize.Height = Math.Max(desiredSize.Height, elementDesiredSize.Height);
				}

				pane.PaneSize = elementDesiredSize;
			}
			
			this._paneInfos = panes;

			return desiredSize;
		}
		#endregion //MeasureOverride

		#region OnChildDesiredSizeChanged
		/// <summary>
		/// Invoked when the desired size of a child has been changed.
		/// </summary>
		/// <param name="child">The child whose size is being changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			if (this._isMeasuringChild)
				return;

			base.OnChildDesiredSizeChanged(child);
		} 
		#endregion //OnChildDesiredSizeChanged

		#region OnInitialized
		/// <summary>
		/// Invoked after the element has been initialized.
		/// </summary>
		/// <param name="e">Provides information for the event.</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			// hide the element if it doesn't have any visible children
			this.CoerceValue(FrameworkElement.VisibilityProperty);

			// make sure a containing toolwindow knows it has new children
			DockManagerUtilities.VerifyOwningToolWindow(this);
		}
		#endregion //OnInitialized

		#region VisualChildrenCount
		/// <summary>
		/// Returns the number of visual children for the element.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				return this._panes.VisualChildrenCount;
			}
		}
		#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region IsInFillPane

		/// <summary>
		/// IsInFillPane Read-Only Dependency Property
		/// </summary>
		internal static readonly DependencyPropertyKey IsInFillPanePropertyKey
			= DependencyProperty.RegisterAttachedReadOnly("IsInFillPane", typeof(bool), typeof(SplitPane),
				new FrameworkPropertyMetadata((bool)KnownBoxes.FalseBox,
					FrameworkPropertyMetadataOptions.Inherits));

		/// <summary>
		/// Identifies the <see cref="GetIsInFillPane"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public static readonly DependencyProperty IsInFillPaneProperty
			= IsInFillPanePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the specified element is within the fill pane of the xamDockManager.
		/// </summary>
		/// <remarks>
		/// <p class="body">The FillPane is the inner most visible root docked <see cref="SplitPane"/> that fills the available area 
		/// when the <see cref="XamDockManager.LayoutMode"/> is set to <b>FillContainer</b>.</p>
		/// </remarks>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = true)]
		[AttachedPropertyBrowsableForType(typeof(ContentPane))]
		[AttachedPropertyBrowsableForType(typeof(TabGroupPane))]
		public static bool GetIsInFillPane(DependencyObject d)
		{
			return (bool)d.GetValue(IsInFillPaneProperty);
		}

		/// <summary>
		/// Provides a secure method for setting the IsInFillPane property.  
		/// This dependency property indicates ....
		/// </summary>
		private static void SetIsInFillPane(DependencyObject d, bool value)
		{
			d.SetValue(IsInFillPanePropertyKey, value);
		}

		#endregion // IsInFillPane

		#region Panes
		/// <summary>
		/// The collection of panes displayed within the split pane. An exception will be thrown if 
		/// the item is not a ContentPane or TabGroupPane.
		/// </summary>
		//[Description("The collection of panes displayed within the split pane. These must be either a ContentPane or a TabGroupPane.")]
		//[Category("DockManager Properties")] // Data
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ObservableCollectionExtended<FrameworkElement> Panes
		{
			get { return this._panes; }
		}
		#endregion //Panes

		#region RelativeSize

		/// <summary>
		/// Identifies the RelativeSize attached dependency property
		/// </summary>
		/// <seealso cref="GetRelativeSize"/>
		/// <seealso cref="SetRelativeSize"/>
		public static readonly DependencyProperty RelativeSizeProperty = DependencyProperty.RegisterAttached("RelativeSize",
			typeof(Size), typeof(SplitPane), new FrameworkPropertyMetadata(new Size(100d,100d), FrameworkPropertyMetadataOptions.AffectsParentMeasure), new ValidateValueCallback(ValidateRelativeSize));

		private static bool ValidateRelativeSize(object value)
		{
			Size size = (Size)value;

			return size.Width > 0 && size.Height > 0 && double.IsInfinity(size.Width) == false && double.IsInfinity(size.Height) == false;
		}

		/// <summary>
		/// Returns the RelativeSize of a child of a SplitPane that is used to determine the percentage of space that the child will be provided within the SplitPane. 
		/// </summary>
		/// <remarks>
		/// <p class="body">The SplitPane uses the RelativeSize of each visible child in its <see cref="Panes"/> collection to 
		/// determine how to distribute the space between the children. Depending upon the <see cref="SplitterOrientation"/>, 
		/// either the Width or Height of the RelativeSize will be used. When the SplitterOrientation of the SplitPane is 
		/// Vertical, the children are arranged horizontally and therefore only the Width of the RelativeSize is evaluated. 
		/// When the SplitterOrientation is Horizontal, the children are arranged vertically and therefore only the Height of 
		/// the RelativeSize is evaluated. The ratio of RelativeSize is used to determine the percentange that a given child 
		/// is provided. For example, consider a SplitPane that has 2 visible children - item 1 has a RelativeSize of 100x200 
		/// and item 2 has a RelativeSize of 200x50. If the SplitterOrientation is Horizontal, the Height of relative sizes are used. 
		/// In this case that means there is a total relative height of 250 (item 1 has a relative height of 200 and item 2 has a 
		/// relative height of 50) and so item 1 would get ~80% (i.e. 200/250) and item 2 would get ~20% (50/250). So if the SplitPane 
		/// is 500 pixels tall, item 1 will have a height of 400 (.80 * 500) and item 2 will have a height of 100 (.20 * 500). Note, 
		/// this value won't be exact since space must be provided to the Splitter displayed between the items.</p>
		/// <p class="note"><b>Note:</b> The minimum Width/Height for the RelativeSize is 1,1 so it is recommended that you 
		/// not use values that are too small as that could leads to problems when resizing the element smaller.</p>
		/// </remarks>
		/// <seealso cref="RelativeSizeProperty"/>
		/// <seealso cref="SetRelativeSize"/>
		//[Description("Returns/sets a value used to determine the ratio of how the size available to the split container should be allocated to the item.")]
		//[Category("DockManager Properties")] // Layout
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		public static Size GetRelativeSize(DependencyObject d)
		{
			return (Size)d.GetValue(SplitPane.RelativeSizeProperty);
		}

		/// <summary>
		/// Sets the RelativeSize of a child of a SplitPane that is used to determine the percentage of space that the child will be provided within the SplitPane. 
		/// </summary>
		/// <remarks>
		/// <p class="body">The SplitPane uses the RelativeSize of each visible child in its <see cref="Panes"/> collection to 
		/// determine how to distribute the space between the children. Depending upon the <see cref="SplitterOrientation"/>, 
		/// either the Width or Height of the RelativeSize will be used. When the SplitterOrientation of the SplitPane is 
		/// Vertical, the children are arranged horizontally and therefore only the Width of the RelativeSize is evaluated. 
		/// When the SplitterOrientation is Horizontal, the children are arranged vertically and therefore only the Height of 
		/// the RelativeSize is evaluated. The ratio of RelativeSize is used to determine the percentange that a given child 
		/// is provided. For example, consider a SplitPane that has 2 visible children - item 1 has a RelativeSize of 100x200 
		/// and item 2 has a RelativeSize of 200x50. If the SplitterOrientation is Horizontal, the Height of relative sizes are used. 
		/// In this case that means there is a total relative height of 250 (item 1 has a relative height of 200 and item 2 has a 
		/// relative height of 50) and so item 1 would get ~80% (i.e. 200/250) and item 2 would get ~20% (50/250). So if the SplitPane 
		/// is 500 pixels tall, item 1 will have a height of 400 (.80 * 500) and item 2 will have a height of 100 (.20 * 500). Note, 
		/// this value won't be exact since space must be provided to the Splitter displayed between the items.</p>
		/// <p class="note"><b>Note:</b> The minimum Width/Height for the RelativeSize is 1,1 so it is recommended that you 
		/// not use values that are too small as that could leads to problems when resizing the element smaller.</p>
		/// </remarks>
		/// <seealso cref="RelativeSizeProperty"/>
		/// <seealso cref="GetRelativeSize"/>
		public static void SetRelativeSize(DependencyObject d, Size value)
		{
			d.SetValue(SplitPane.RelativeSizeProperty, value);
		}

		#endregion //RelativeSize

		#region SplitterOrientation

		/// <summary>
		/// Identifies the <see cref="SplitterOrientation"/> dependency property
		/// </summary>
		/// <seealso cref="SplitterOrientation"/>
		public static readonly DependencyProperty SplitterOrientationProperty = DependencyProperty.Register("SplitterOrientation",
			typeof(Orientation), typeof(SplitPane), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

		/// <summary>
		/// Returns/set the orientation of the splitter bar. If Vertical, the panes are stacked horizontally with a vertical splitter between each; if horizontal, the panes are stacked vertically with a horizontal splitter between each.
		/// </summary>
		/// <seealso cref="SplitterOrientationProperty"/>
		//[Description("Returns/set the orientation of the splitter bar. If Vertical, the panes are stacked horizontally with a vertical splitter between each; if horizontal, the panes are stacked vertically with a horizontal splitter between each.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public Orientation SplitterOrientation
		{
			get
			{
				return (Orientation)this.GetValue(SplitPane.SplitterOrientationProperty);
			}
			set
			{
				this.SetValue(SplitPane.SplitterOrientationProperty, value);
			}
		}

		#endregion //SplitterOrientation

		#endregion //Public Properties

		#region Internal Properties

		// AS 5/27/08 BR33298
		#region IncludePanesAsLogicalChildren
		internal virtual bool IncludePanesAsLogicalChildren
		{
			get { return true; }
		} 
		#endregion //IncludePanesAsLogicalChildren

		#region IsDockedLeftRight
		internal bool IsDockedLeftRight
		{
			get
			{
				PaneLocation location = XamDockManager.GetPaneLocation(this);

				return location == PaneLocation.DockedLeft ||
					location == PaneLocation.DockedRight;
			}
		}
		#endregion //IsDockedLeftRight

		#region IsDockedTopBottom
		internal bool IsDockedTopBottom
		{
			get
			{
				PaneLocation location = XamDockManager.GetPaneLocation(this);

				return location == PaneLocation.DockedTop ||
					location == PaneLocation.DockedBottom;
			}
		}
		#endregion //IsDockedTopBottom

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region IsFillPane

		internal static readonly DependencyPropertyKey IsFillPanePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsFillPane",
			typeof(bool), typeof(SplitPane), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsFillPaneChanged)));

		private static void OnIsFillPaneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(FrameworkElement.WidthProperty);
			d.CoerceValue(FrameworkElement.HeightProperty);

			d.CoerceValue(FrameworkElement.VisibilityProperty);

			d.SetValue(IsInFillPanePropertyKey, true.Equals(e.NewValue) ? e.NewValue : DependencyProperty.UnsetValue);
		}

		internal static readonly DependencyProperty IsFillPaneProperty =
			IsFillPanePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the pane is a root docked SplitPane that fills the available area of the owning XamDockManager when the LayoutMode is FillContainer.
		/// </summary>
		/// <seealso cref="IsFillPaneProperty"/>
		internal bool IsFillPane
		{
			get
			{
				return (bool)this.GetValue(SplitPane.IsFillPaneProperty);
			}
		}

		#endregion //IsFillPane

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region HeightToSerialize
		internal double HeightToSerialize
		{
			get
			{
				if (this._uncoercedHeight is double)
				{
					ValueSource vs = DependencyPropertyHelper.GetValueSource(this, FrameworkElement.HeightProperty);

					if (vs.IsCoerced)
						return (double)this._uncoercedHeight;
				}

				return this.Height;
			}
		}
		#endregion //HeightToSerialize

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region WidthToSerialize
		internal double WidthToSerialize
		{
			get
			{
				if (this._uncoercedWidth is double)
				{
					ValueSource vs = DependencyPropertyHelper.GetValueSource(this, FrameworkElement.WidthProperty);

					if (vs.IsCoerced)
						return (double)this._uncoercedWidth;
				}

				return this.Width;
			}
		}
		#endregion //WidthToSerialize

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region CoerceVisibility
		private static object CoerceVisibility(DependencyObject d, object newValue)
		{
			return DockManagerUtilities.ProcessCoerceVisibility(d, newValue);
		}
		#endregion //CoerceVisibility

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region CoerceHeight
		private static object CoerceHeight(DependencyObject d, object newValue)
		{
			SplitPane split = (SplitPane)d;
			return split.CoerceWidthHeight(newValue, ref split._uncoercedHeight);
		}
		#endregion //CoerceHeight

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region CoerceWidth
		private static object CoerceWidth(DependencyObject d, object newValue)
		{
			SplitPane split = (SplitPane)d;
			return split.CoerceWidthHeight(newValue, ref split._uncoercedWidth);
		}
		#endregion //CoerceWidth

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region CoerceWidthHeight
		private object CoerceWidthHeight(object newValue, ref object _uncoercedValue)
		{
			if (this.IsFillPane)
			{
				_uncoercedValue = newValue;
				return NaN;
			}

			_uncoercedValue = null;
			return newValue;
		}
		#endregion //CoerceWidthHeight

		#region OnChildrenCollectionChanged
		private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// make sure all items have their current container set to this element
			DockManagerUtilities.InitializeCurrentContainer(this.Panes, this, e);

			// hide the element if it doesn't have any visible children
			this.CoerceValue(FrameworkElement.VisibilityProperty);

			// let the tool window know in case it needs to show/hide its caption
			DockManagerUtilities.VerifyOwningToolWindow(this);

			// dirty the measurement
			this.InvalidateMeasure();
		}
		#endregion //OnChildrenCollectionChanged

		#region OnChildVisibilityChanged
		private static void OnChildVisibilityChanged(object sender, RoutedEventArgs e)
		{
			SplitPane group = (SplitPane)sender;

			if (e.OriginalSource != sender)
			{
				group._panes.RefreshSplitterVisibility();

				group.CoerceValue(UIElement.VisibilityProperty);
				e.Handled = true;
			}

			DockManagerUtilities.VerifyOwningToolWindow(group);

		}
		#endregion // OnChildVisibilityChanged

		#region OnPaneLocationChanged
		private static void OnPaneLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SplitPane pane = (SplitPane)d;

			// when put into a document area, ensure that we do not allow anything except tab group panes
			if ((PaneLocation)e.NewValue == PaneLocation.Document)
			{
				foreach (FrameworkElement child in pane.Panes)
				{
					if (child is TabGroupPane == false && child is SplitPane == false)
						throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidDocumentSplitPaneChild"));
				}
			}
		}
		#endregion //OnPaneLocationChanged

		#region OnVisibilityChanged
		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DockManagerUtilities.RaiseVisibilityChanged(d, e);

			// AS 10/5/09 NA 2010.1 - LayoutMode
			// Hiding/showing a root docked split pane could change the fill pane.
			//
			SplitPane split = (SplitPane)d;
			if (split.Parent is XamDockManager)
				((XamDockManager)split.Parent).VerifyFillPane();
		}
		#endregion //OnVisibilityChanged

		#region PredictDrop

		/// <summary>
		/// Returns the rect within the element that a new element would be positioned given a relative size and index.
		/// </summary>
		/// <param name="relativeSize">The relative size for the new pane</param>
		/// <param name="paneIndex">The index in the <see cref="Panes"/> collection where the pane would be</param>
		/// <returns>The relative rect where the pane would be displayed</returns>
		internal Rect PredictDrop(Size relativeSize, int paneIndex)
		{
			List<PaneInfo> panes = new List<PaneInfo>();
			bool isVerticallyArranged = this.SplitterOrientation == Orientation.Horizontal;

			PaneInfo newPane = new PaneInfo();
			newPane.Weight = isVerticallyArranged ? relativeSize.Height : relativeSize.Width;
			Size splitterSize = Size.Empty;

			for (int i = 0, count = this._panes.Count; i < count; i++)
			{
				if (i == paneIndex)
					panes.Add(newPane);

				FrameworkElement child = this._panes[i];

				// use the size of a splitter if we have one
				if (splitterSize.IsEmpty)
				{
					PaneSplitter splitter = PaneSplitter.GetSplitter(child);

					if (null != splitter)
						splitterSize = new Size(splitter.ActualWidth, splitter.ActualHeight);
				}

				if (child != null && child.Visibility != Visibility.Collapsed)
				{
					PaneInfo childPane = new PaneInfo();
					Size relativeChildSize = SplitPane.GetRelativeSize(child);
					childPane.Weight = isVerticallyArranged ? relativeChildSize.Height : relativeChildSize.Width;
					panes.Add(childPane);
				}
			}

			if (paneIndex >= this._panes.Count)
				panes.Add(newPane);

			if (splitterSize.IsEmpty)
				splitterSize = DefaultSplitterSize;

			// now we need to know the size of the splitters
			for (int i = 0, count = panes.Count - 1; i < count; i++)
			{
				panes[i].SplitterSize = splitterSize;
			}

			PrepareForArrange(panes, new Size(this.ActualWidth, this.ActualHeight), this.SplitterOrientation);

			return newPane.PaneRect;
		} 
		#endregion //PredictDrop

		#region PredictDropNewSplit
		internal static Rect PredictDropNewSplit(Dock side, FrameworkElement siblingElement)
		{
			List<PaneInfo> panes = new List<PaneInfo>();
			bool isVerticallyArranged = side == Dock.Left || side == Dock.Right;

			const double DefaultWeight = 100d;
			PaneInfo sibling = new PaneInfo();
			sibling.Weight = DefaultWeight;
			panes.Add(sibling);

			PaneInfo newSplit = new PaneInfo();
			newSplit.Weight = DefaultWeight;

			int newSplitIndex = side == Dock.Left || side == Dock.Top ? 0 : 1;
			panes.Insert(newSplitIndex, newSplit);

			panes[0].SplitterSize = DefaultSplitterSize;

			PrepareForArrange(panes, new Size(siblingElement.ActualWidth, siblingElement.ActualHeight), isVerticallyArranged ? Orientation.Vertical : Orientation.Horizontal);

			return newSplit.PaneRect;
		}
		#endregion //PredictDropNewSplit

		#region PrepareForArrange
		private static void PrepareForArrange(IList<PaneInfo> panes, Size finalSize, Orientation splitterOrientation)
		{
			Rect finalRect = new Rect(finalSize);
			bool isVerticallyArranged = splitterOrientation == Orientation.Horizontal;
			double totalWeight = 0;
			double sizableExtent = isVerticallyArranged ? finalSize.Height : finalSize.Width;

			Debug.Assert(panes != null);

			// if we need to distribute the size then we to remove the extents of the
			// splitters so we know the amount to distribute
			for (int i = 0, count = panes.Count; i < count; i++)
			{
				PaneInfo pane = panes[i];
				sizableExtent = Math.Max(sizableExtent - (isVerticallyArranged ? pane.SplitterSize.Height : pane.SplitterSize.Width), 0);
				totalWeight += pane.Weight;
			}

			for (int i = 0, count = (panes.Count * 2 - 1); i < count; i++)
			{
				bool isSplitter = i % 2 == 1;
				int elementIndex = i / 2;
				PaneInfo pane = panes[elementIndex];
				Size elementSize;

				if (isSplitter)
				{
					elementSize = pane.SplitterSize;
				}
				else
				{
					double extent = sizableExtent * (pane.Weight / totalWeight);

					// only the sizable extent is used so we can set both width
					// and height regardless of the arrangement
					elementSize = new Size(extent, extent);
				}

				Size childSize = finalSize;

				if (isVerticallyArranged)
				{
					childSize.Height = elementSize.Height;
				}
				else
				{
					childSize.Width = elementSize.Width;
				}

				Rect arrangeRect = new Rect(finalRect.Location, childSize);

				if (isSplitter)
					pane.SplitterRect = arrangeRect;
				else
					pane.PaneRect = arrangeRect;

				if (isVerticallyArranged)
				{
					finalRect.Height = Math.Max(finalRect.Height - childSize.Height, 0);
					finalRect.Y += childSize.Height;
				}
				else
				{
					finalRect.Width -= Math.Max(finalRect.Width - childSize.Width, 0);
					finalRect.X += childSize.Width;
				}
			}
		} 
		#endregion //PrepareForArrange

		#endregion //Private Methods 

		#endregion //Methods

		#region ISplitElementCollectionOwner

		void ISplitElementCollectionOwner.OnElementAdding(FrameworkElement newElement)
		{
			if (newElement is SplitPane == false && 
				newElement is TabGroupPane == false &&
				newElement is ContentPanePlaceholder == false &&
				newElement is ContentPane == false)
				throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidSplitPaneChild"));

			if (XamDockManager.GetPaneLocation(this) == PaneLocation.Document &&
				newElement is TabGroupPane == false &&
				newElement is SplitPane == false)
			{
				throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidDocumentSplitPaneChild"));
			}
		}

		void ISplitElementCollectionOwner.OnElementAdded(FrameworkElement newElement)
		{
            // AS 3/5/09 TFS14951
            // Moved this down below the AddLogicalChild call.
            //
			//this.AddVisualChild(newElement);

			// AS 5/27/08 BR33298
			// The root document host split pane can no longer include the root 
			// splits as logical children since they are logical children of the 
			// documentcontenthost itself.
			//
			if (this.IncludePanesAsLogicalChildren)
				this.AddLogicalChild(newElement);

            // AS 3/5/09 TFS14951
            // The AddVisualChild will cause an OnVisualParentChanged which uses the 
            // LogicalChildren (of which the newElement is one) but its not had its 
            // parent property set yet. We need to make sure that is set first.
            //
            this.AddVisualChild(newElement);

			DockManagerUtilities.InitializeCurrentContainer(newElement, this);
		}

		void ISplitElementCollectionOwner.OnElementRemoved(FrameworkElement oldElement)
		{
			// AS 1/14/10
			// We need to remove any references to the element that was removed so 
			// it can be collected.
			//
			if (null != _paneInfos)
			{
				for (int i = 0; i < _paneInfos.Count; i++)
				{
					if (_paneInfos[i].Pane == oldElement)
					{
						_paneInfos.RemoveAt(i);
						break;
					}
				}
			}

			this.RemoveVisualChild(oldElement);

			// AS 5/27/08 BR33298
			// The root document host split pane can no longer include the root 
			// splits as logical children since they are logical children of the 
			// documentcontenthost itself.
			//
			if (this.IncludePanesAsLogicalChildren)
				this.RemoveLogicalChild(oldElement);

			DockManagerUtilities.InitializeCurrentContainer(oldElement, null);
		}

		PaneSplitter ISplitElementCollectionOwner.CreateSplitter()
		{
			return new SplitPaneSplitter();
		}

		void ISplitElementCollectionOwner.OnSplitterAdded(PaneSplitter splitter)
		{
			splitter.SetBinding(PaneSplitter.OrientationProperty, Utilities.CreateBindingObject(SplitPane.SplitterOrientationProperty, System.Windows.Data.BindingMode.OneWay, this));

			this.AddVisualChild(splitter);
			this.AddLogicalChild(splitter);
		}

		void ISplitElementCollectionOwner.OnSplitterRemoved(PaneSplitter splitter)
		{
			// AS 1/14/10
			// We need to remove any references to the element that was removed so 
			// it can be collected.
			//
			if (null != _paneInfos)
			{
				for (int i = 0; i < _paneInfos.Count; i++)
				{
					if (_paneInfos[i].Splitter == splitter)
					{
						_paneInfos[i].Splitter = null;
						break;
					}
				}
			}

			BindingOperations.ClearBinding(splitter, PaneSplitter.OrientationProperty);

			this.RemoveLogicalChild(splitter);
			this.RemoveVisualChild(splitter);
		}

		#endregion //ISplitElementCollectionOwner Members

		#region PaneInfo class
		private class PaneInfo
		{
			internal FrameworkElement Pane;
			internal PaneSplitter Splitter;
			internal double Weight;
			internal Size SplitterSize;
			internal Size PaneSize;
			internal Rect PaneRect;
			internal Rect SplitterRect;
		}
		#endregion //PaneInfo class

		#region IContentPaneContainer Members

		FrameworkElement IContentPaneContainer.ContainerElement
		{
			get { return this; }
		}

		PaneLocation IContentPaneContainer.PaneLocation
		{
			get { return XamDockManager.GetPaneLocation(this); }
		}

		void IContentPaneContainer.RemoveContentPane(ContentPane pane, bool replaceWithPlaceholder)
		{
			int index = DockManagerUtilities.IndexOf(this.Panes, pane, true);

			Debug.Assert(index >= 0, "The pane does not exist within this pane!");

			if (index >= 0)
			{
				FrameworkElement element = this.Panes[index];

				// if we already have a placeholder for the element...
				if (element is ContentPanePlaceholder && replaceWithPlaceholder)
				{
					Debug.Fail("Why are we getting here when we already have a placeholder?");
					return;
				}

				this.Panes.RemoveAt(index);

				if (replaceWithPlaceholder)
				{
					Debug.Assert(DockManagerUtilities.NeedsPlaceholder(XamDockManager.GetPaneLocation(this), PaneLocation.Unknown));

					ContentPanePlaceholder placeholder = new ContentPanePlaceholder();
					placeholder.Initialize(pane);
					this.Panes.Insert(index, placeholder);

					// cache the current relative size on the placeholder so we can maintain the old
					// relative size when we restore the pane
					if (pane.ReadLocalValue(SplitPane.RelativeSizeProperty) != DependencyProperty.UnsetValue)
						placeholder.SetValue(SplitPane.RelativeSizeProperty, pane.ReadLocalValue(SplitPane.RelativeSizeProperty));

					pane.PlacementInfo.StorePlaceholder(placeholder);
				}
				else if (element is ContentPanePlaceholder)
				{
					// if we're removing the placeholder then remove it from the placement info
					pane.PlacementInfo.RemovePlaceholder((ContentPanePlaceholder)element);
				}
			}
		}

		void IContentPaneContainer.InsertContentPane(int? newIndex, ContentPane pane)
		{
			// find the placeholder
			int placeholderIndex = DockManagerUtilities.IndexOf(this.Panes, pane, true);

			if (placeholderIndex >= 0)
			{
				Debug.Assert(this.Panes[placeholderIndex] is ContentPanePlaceholder);

				if (this.Panes[placeholderIndex] is ContentPane)
					return;

				ContentPanePlaceholder placeholder = this.Panes[placeholderIndex] as ContentPanePlaceholder;

				// remove the placeholder
				this.Panes.RemoveAt(placeholderIndex);

				// update the placement info
				pane.PlacementInfo.RemovePlaceholder(placeholder);

				// restore the relative size for the pane when it was in this pane
				pane.SetValue(SplitPane.RelativeSizeProperty, placeholder.ReadLocalValue(SplitPane.RelativeSizeProperty));
			}
			else // add it to the end
				placeholderIndex = this.Panes.Count;

			if (newIndex != null)
				placeholderIndex = Math.Max(0, Math.Min(this.Panes.Count, newIndex.Value));

			this.Panes.Insert(placeholderIndex, pane);
		}

		IList<ContentPane> IContentPaneContainer.GetVisiblePanes()
		{
			return DockManagerUtilities.CreateVisiblePaneList(this.Panes);
		}

		IList<ContentPane> IContentPaneContainer.GetAllPanesForPaneAction(ContentPane pane)
		{
			Debug.Assert(pane != null && this.Panes.IndexOf(pane) >= 0);
			return new ContentPane[] { pane };
		}
		#endregion //IContentPaneContainer

		#region IPaneContainer Members

		IList IPaneContainer.Panes
		{
			get { return this.Panes; }
		}
		bool IPaneContainer.RemovePane(object pane)
		{
			FrameworkElement element = pane as FrameworkElement;
			Debug.Assert(null != element);

			int index = null != element ? this.Panes.IndexOf(element) : -1;

			if (index >= 0)
			{
				this.Panes.RemoveAt(index);
			}

			return index >= 0;
		}

		bool IPaneContainer.CanBeRemoved
		{
			// AS 5/17/08 BR32346
			//get { return this.Panes.Count == 0; }
			get { return this.Panes.Count == 0 && DockManagerUtilities.ShouldPreventPaneRemoval(this) == false; }
		}

		#endregion //IPaneContainer
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