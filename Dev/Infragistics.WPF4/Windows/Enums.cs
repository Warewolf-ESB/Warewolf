using System;
using Infragistics.Windows.Virtualization;

namespace Infragistics.Windows
{


    // MBS 8/19/09 - NA9.2 Excel Exporting
    #region DeviceUnitType

    /// <summary>
    /// Indicates the type of the value that is represented by the <see cref="DeviceUnitLength"/>.
    /// </summary>
    public enum DeviceUnitType
    {
        /// <summary>
        /// The units are measured in Device Independent Units (DIUs) where 96 units equals 1 inch.
        /// </summary>
        DeviceIndependentUnit,

        /// <summary>
        /// The units are measured in Pixels.
        /// </summary>
        Pixel,

		/// <summary>
		/// Units are expressed as DTPs (desktop publishing points), a unit
		/// equal to 1/72 of an inch. One point is equal to 20 twips.
		/// </summary>
		Point,

		/// <summary>
		/// Units are expressed as one-twentieths of a point, or 1/1440 of an inch.
		/// </summary>
		Twip,

		/// <summary>
		/// Units are expressed as centimeters. One centimeter is equal to
		/// approximately 567 twips.
		/// </summary>
		Centimeter,

		/// <summary>
		/// Units are expressed as inches (U.S. customary units).
		/// One inch is equal to 1,440 twips.
		/// </summary>
		Inch
    }
    #endregion //DeviceUnitType


}

namespace Infragistics.Windows.Controls
{



    #region EffectStopDirection

    /// <summary>
	/// Determines the direction in which <see cref="EffectStop"/>s are evaluated in the <see cref="XamCarouselPanel"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">The enumeration contains values for evaluating <see cref="EffectStop"/>s based on an item's position along the <see cref="CarouselViewSettings.ItemPath"/> or based on the item's vertical or horizontal position within the <see cref="XamCarouselPanel"/>.</p>
	/// </remarks>
	/// <seealso cref="CarouselViewSettings"/>
	/// <seealso cref="EffectStop"/>
	/// <seealso cref="CarouselViewSettings.OpacityEffectStopDirection"/>
	/// <seealso cref="CarouselViewSettings.ScalingEffectStopDirection"/>
	/// <seealso cref="CarouselViewSettings.SkewAngleXEffectStopDirection"/>
	/// <seealso cref="CarouselViewSettings.SkewAngleYEffectStopDirection"/>
	/// <seealso cref="CarouselViewSettings.ZOrderEffectStopDirection"/>
	public enum EffectStopDirection
	{
		/// <summary>
		/// The direction should default to a value that makes sense for the effect being applied.
		/// </summary>
		Default,

		/// <summary>
		/// <see cref="EffectStop"/>s should be evaluated by treating their <see cref="EffectStop.Offset"/>s as a percentage into a range of values that describe a horizontal extent.
		/// </summary>
		Horizontal,

		/// <summary>
		/// <see cref="EffectStop"/>s should be evaluated by treating their <see cref="EffectStop.Offset"/>s as a percentage into a range of values that describe a vertical extent.
		/// </summary>
		Vertical,

		/// <summary>
		/// <see cref="EffectStop"/>s should be evaluated by treating their <see cref="EffectStop.Offset"/>s as a percentage along the <see cref="CarouselViewSettings.ItemPath"/>.
		/// </summary>
		UseItemPath
	}

	#endregion //EffectStopDirection	

	#region PathItemTransitionStyle

	/// <summary>
	/// Determines the effects applied to items as they transition through the prefix and suffix areas of a <see cref="XamCarouselPanel"/>'s <see cref="CarouselViewSettings.ItemPath"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">The prefix area is located at the beginning of the path and the suffix area is located at the end of the path.  These are the areas within which items are transitioned into and out of view during scrolling.
	/// Items appear in the prefix or suffix area only while they are transitioning (scrolling) into or out of view.  When they are 'at rest', items appear in the area between the prefix and suffix areas.</p>
	/// </remarks>
	/// <seealso cref="CarouselViewSettings"/>
	/// <seealso cref="CarouselViewSettings.ItemPath"/>
	/// <seealso cref="CarouselViewSettings.ItemPathPrefixPercent"/>
	/// <seealso cref="CarouselViewSettings.ItemPathSuffixPercent"/>
	/// <seealso cref="CarouselViewSettings.ItemTransitionStyle"/>
	public enum PathItemTransitionStyle
	{
		/// <summary>
		/// The opacity of items are adjusted from transparent to Opaque as the items pass through the prefix and suffix areas of the <see cref="XamCarouselPanel"/>'s <see cref="CarouselViewSettings.ItemPath"/>
		/// </summary>
		AdjustOpacity = 0x00000001,
		/// <summary>
		/// The size of items are adjusted from full size to zero as the items pass through the prefix and suffix areas of the <see cref="XamCarouselPanel"/>'s <see cref="CarouselViewSettings.ItemPath"/>
		/// </summary>
		AdjustSize = 0x00000002,
		/// <summary>
		/// Both the size and opacity of items are adjusted as the items pass through the prefix and suffix areas of the <see cref="XamCarouselPanel"/>'s <see cref="CarouselViewSettings.ItemPath"/>
		/// </summary>
		AdjustSizeAndOpacity = 0x00000003
	}

	#endregion PathItemTransitionStyle


	#region ExpansionIndicatorToggleMode
	/// <summary>
	/// Enumeration used to determine when the IsChecked value of an <see cref="ExpansionIndicator"/> is toggled.
	/// </summary>
	[InfragisticsFeatureAttribute(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
	public enum ExpansionIndicatorToggleMode
	{
		/// <summary>
		/// The IsChecked value is toggle automatically when clicked.
		/// </summary>
		Automatic,

		/// <summary>
		/// The IsChecked state must be updated programatically.
		/// </summary>
		Manual,
	} 
	#endregion //ExpansionIndicatorToggleMode

	// JJD 5/22/07 - Added
	#region ItemContainerGenerationMode enum

	/// <summary>
	/// Determines how item containers are generated and cached by the RecyclingItemsPanel.
	/// </summary>
	/// <seealso cref="RecyclingItemsPanel"/>
	/// <seealso cref="RecyclingItemsControl"/>
	/// <seealso cref="RecyclingItemContainerGenerator"/>
	public enum ItemContainerGenerationMode
	{
		/// <summary>
		/// Reuses item containers for items that get scrolled out of view for the items that get scrolled into view.
		/// </summary>
		Recycle = 0,
		/// <summary>
		/// Generates item containers for only the items in view and clears item containers for items that subsequently get scrolled out of view.
		/// </summary>
		Virtualize = 1,
		/// <summary>
		/// Generates item containers for items as they are scrolled into view and does not clear them for items that get scrolled out of view.
		/// </summary>
		LazyLoad = 2,
		/// <summary>
		/// Pre-generates and caches an item container for each item.
		/// </summary>
		PreLoad = 3
	}

	#endregion //ItemContainerGenerationMode enum

	// AS 7/24/04 XamPager
	#region PagerScrollDirection
	internal enum PagerScrollDirection
	{
		Up,
		Down,
		Left,
		Right
	}
	#endregion //PagerScrollDirection


	// JJD 8/24/07 PopupResizerDecorator
	#region PopupResizeMode
	/// <summary>
	/// Identifies how a popup can be resized
	/// </summary>
	/// <seealso cref="PopupResizerDecorator"/>
	/// <seealso cref="PopupResizerBar"/>
	/// <seealso cref="PopupResizerBarLocation"/>
	public enum PopupResizeMode	
	{
		/// <summary>
		/// The popup can not be resized
		/// </summary>
		None,

		/// <summary>
		/// Only the popup's height can be resized
		/// </summary>
		VerticalOnly,

		/// <summary>
		/// The popup can be resized in either the vertical or horizontal dimensions
		/// </summary>
		Both,
	}
	#endregion //PopupResizeMode

	// JJD 8/24/07 PopupResizerDecorator
	#region PopupResizerBarLocation
	/// <summary>
	/// Identifies how a popup can be resized
	/// </summary>
	/// <seealso cref="PopupResizerDecorator"/>
	/// <seealso cref="PopupResizerBar"/>
	/// <seealso cref="PopupResizeMode"/>
	public enum PopupResizerBarLocation	
	{
		/// <summary>
		/// The resizer bar is positioned at the bottom of the Popup/>
		/// </summary>
		Bottom,

		/// <summary>
		/// The resizer bar is positioned at the top of the Popup/>
		/// </summary>
		Top,
	}
	#endregion //PopupResizerBarLocation


	#region RelativeContentLocation

	/// <summary>
	/// Determines the relative location of one piece of content with respect to another.
	/// </summary>
	public enum RelativeContentLocation
	{
		/// <summary>
		/// Above content on left
		/// </summary>
		AboveContentLeft		= 0,
		/// <summary>
		/// Above content in center
		/// </summary>
		AboveContentCenter		= 1,
		/// <summary>
		/// Above content on right
		/// </summary>
		AboveContentRight		= 2,
		/// <summary>
		/// Above content - stretch to same width
		/// </summary>
		AboveContentStretch		= 3,
		/// <summary>
		/// Below content on left
		/// </summary>
		BelowContentLeft		= 4,
		/// <summary>
		/// Below content in center
		/// </summary>
		BelowContentCenter		= 5,
		/// <summary>
		/// Below content on right
		/// </summary>
		BelowContentRight		= 6,
		/// <summary>
		/// Below content - stretch to same width
		/// </summary>
		BelowContentStretch		= 7,
		/// <summary>
		/// Left of content on top 
		/// </summary>
		LeftOfContentTop		= 8,
		/// <summary>
		/// Left of content in middle
		/// </summary>
		LeftOfContentMiddle		= 9,
		/// <summary>
		/// Left of content on bottom
		/// </summary>
		LeftOfContentBottom		= 10,
		/// <summary>
		/// Left of content - stretch to same height
		/// </summary>
		LeftOfContentStretch	= 11,
		/// <summary>
		/// Right of content on top 
		/// </summary>
		RightOfContentTop		= 12,
		/// <summary>
		/// Right of content in middle
		/// </summary>
		RightOfContentMiddle	= 13,
		/// <summary>
		/// Right of content on bottom
		/// </summary>
		RightOfContentBottom	= 14,
		/// <summary>
		/// Right of content - stretch to same height
		/// </summary>
		RightOfContentStretch	= 15,
	}

	#endregion RelativeContentLocation

	#region ResizingMode

	/// <summary>
	/// Determines how resizing occurs.
	/// </summary>
	public enum ResizingMode
	{
		/// <summary>
		/// Use the default setting
		/// </summary>
		Default = 0,
		/// <summary>
		/// Resizing occurs after the mouse is released.  While the mouse is down a marker line is displayed where the new boundary will be.
		/// </summary>
		Deferred = 1,
		/// <summary>
		/// Resizing occurs immediately as the cursor is dragged.
		/// </summary>
		Immediate = 2,
	}

	#endregion ResizingMode

	#region RoundedRectCorners
	/// <summary>
	/// Flagged enumeration used to indicate one or more corners of a rectangle.
	/// </summary>
	[Flags()]
	public enum RoundedRectCorners
	{
		/// <summary>
		/// No corners
		/// </summary>
		None = 0,

		/// <summary>
		/// The top left corner
		/// </summary>
		TopLeft = 0x1,

		/// <summary>
		/// The top right corner
		/// </summary>
		TopRight = 0x2,

		/// <summary>
		/// The bottom left corner
		/// </summary>
		BottomLeft = 0x4,

		/// <summary>
		/// The bottom right corner
		/// </summary>
		BottomRight = 0x8,

		/// <summary>
		/// The left and right corners of the top edge.
		/// </summary>
		Top = TopLeft | TopRight,

		/// <summary>
		/// The left and right corners of the bottom edge.
		/// </summary>
		Bottom = BottomLeft | BottomRight,

		/// <summary>
		/// The top and bottom corners of the left edge.
		/// </summary>
		Left = TopLeft | BottomLeft,

		/// <summary>
		/// The top and bottom corners of the right edge.
		/// </summary>
		Right = TopRight | BottomRight,

		/// <summary>
		/// All 4 corners
		/// </summary>
		All = Top | Bottom,
	}
	#endregion //RoundedRectCorners

	#region RoundedRectSide
	/// <summary>
	/// A flagged enumeration used to identify a side of a rectangle.
	/// </summary>
	public enum RoundedRectSide
	{
		

		/// <summary>
		/// The left side
		/// </summary>
		Left = 0,

		/// <summary>
		/// The top side
		/// </summary>
		Top = 1,

		/// <summary>
		/// The right side
		/// </summary>
		Right = 2,

		/// <summary>
		/// The bottom side
		/// </summary>
		Bottom = 3,
	}
	#endregion //RoundedRectSide

    #region SelectionType

    /// <summary>
    /// Used to specify the type of selection that is allowed for an object.
    /// </summary>
    public enum SelectionType
    {
        /// <summary>
        /// Use Default. The setting of the object's parent will be used.
        /// </summary>
        Default = 0,

        /// <summary>
        /// None. Objects may not be selected.
        /// </summary>
        None = 1,

        /// <summary>
        /// Single Select. Only one object may be selected at any time.
        /// </summary>
        Single = 2,

        /// <summary>
        /// Extended Select. Multiple objects may be selected at once.
        /// </summary>
        Extended = 3,

        // AS 7/16/08 NA 2008 Vol 2
        /// <summary>
        /// A single range that could include multiple objects may be selected.
        /// </summary>
        Range = 4,

		// AS 6/4/09
		// Since we support dragging of fields even when using a SelectField click action
		// we should allow single click dragging of the field.
		//
		/// <summary>
		/// Strategy used when multiple items can be selected but pressing the left
		/// button and dragging does not select other items but instead starts dragging
		/// the selected item immediately. 
		/// </summary>
		ExtendedAutoDrag = 5,

		/// <summary>
		/// Strategy used when only a single item can be selected and pressing the left
		/// button and dragging does not select other items but instead starts dragging
		/// the selected item immediately.
		/// </summary>
		SingleAutoDrag = 6
    }

    #endregion SelectionType

    #region SortStatus

    /// <summary>
	/// Determines how items are sorted
	/// </summary>
	public enum SortStatus
	{
        /// <summary>
        /// The items are not sorted
        /// </summary>
        NotSorted                   = 0,
		/// <summary>
        /// The items are sorted ascending
		/// </summary>
		Ascending					= 1,
		/// <summary>
        /// The items are sorted descending
		/// </summary>
		Descending    				= 2,
	}

	#endregion SortStatus


    // JJD 8/4/08 added
    #region TabControlStates
    /// <summary>
    /// Enumeration used to indicate the current state of a <see cref="TabItemEx"/>
    /// </summary>
    [Flags()]
    public enum TabControlStates : long
    {
        /// <summary>
        /// The <see cref="XamTabControl.AllowMinimize"/> property is true.
        /// </summary>
        AllowMinimized = 0x00000001,

        /// <summary>
        /// Has a tab that is selected
        /// </summary>
        HasSelectedTab = 0x00000002,

        /// <summary>
        /// Has at least one tab that is selectable
        /// </summary>
        HasSelectableTab = 0x00000004,

        /// <summary>
        /// The selected tab is the first tab that is not closed.
        /// </summary>
        FirstTabSelected = 0x00000008,

        /// <summary>
        /// The selected tab is the last tab that is not closed.
        /// </summary>
        LastTabSelected = 0x00000010,

        /// <summary>
        /// The <see cref="XamTabControl.IsMinimized"/> property is true.
        /// </summary>
        Minimized = 0x00000020,

        /// <summary>
        /// The selected tab can be closed.
        /// </summary>
        SelectedTabAllowsClosing = 0x00000040,
    }
    #endregion //TabItemStates
    
    // JJD 8/4/08 Added
    #region TabItemExStates
    /// <summary>
    /// Enumeration used to indicate the current state of a <see cref="TabItemEx"/>
    /// </summary>
    [Flags()]
    public enum TabItemExStates : long
    {
        /// <summary>
        /// The <see cref="System.Windows.Controls.TabItem.IsSelected"/> is true
        /// </summary>
        SelectedTab = 0x00000001,

        /// <summary>
        /// The <see cref="TabItemEx"/> can be closed.
        /// </summary>
        AllowsClosing = 0x00000002,
    }
    #endregion //TabItemStates

    // JJD 8/4/08 XamTabControl
    #region TabItemCloseButtonVisibility
    /// <summary>
    /// Indicates when the close button should be displayed within a <see cref="TabItemEx"/>
	/// </summary>
    /// <seealso cref="TabItemEx.CloseButtonVisibility"/>
    /// <seealso cref="XamTabControl.TabItemCloseButtonVisibility"/>
	public enum TabItemCloseButtonVisibility
	{
		/// <summary>
        /// The close button is always visible in the TabItem.
		/// </summary>
		Visible,

		/// <summary>
        /// The close button is only visible when the TabItem is selected.
		/// </summary>
        WhenSelected,

		/// <summary>
        /// The close button is only visible when the TabItem is selected or when it is HotTracked.
		/// </summary>
        WhenSelectedOrHotTracked,

		/// <summary>
        /// The close button is never visible in the tab item.
		/// </summary>
		Hidden,
	}
	#endregion //TabLayoutStyle

	// AS 7/24/07 XamTabControl
	#region TabLayoutStyle
	/// <summary>
	/// Identifies the types of layouts supported by the <see cref="TabItemPanel"/>
	/// </summary>
	public enum TabLayoutStyle
	{
		/// <summary>
		/// The items are sized based on their content and arranged in a single row.
		/// </summary>
		SingleRowAutoSize,

		/// <summary>
		/// The items are sized based on their content size and then reduced towards their minimum size if there is not enough room to fit the items. The items are arranged within a single row.
		/// </summary>
		SingleRowJustified,

		/// <summary>
		/// The items are sized based on their content size and then increased in size if there is more room to display the items than required. The items are arranged within a single row.
		/// </summary>
		SingleRowSizeToFit,

		/// <summary>
		/// The items are sized based on their content and arranged in multiple rows.
		/// </summary>
		MultiRowAutoSize,

		/// <summary>
		/// The items are sized based on their content and then increased in size if there is more room to display the items than required. The items are arranged in multiple rows.
		/// </summary>
		MultiRowSizeToFit,
	}
	#endregion //TabLayoutStyle

	// AS 4/2/08 NA 2008 Vol 1 - XamDockManager
	#region ToolWindowAlignmentMode
	/// <summary>
	/// Enumeration used to control the vertical or horizontal positioning of a <see cref="ToolWindow"/>
	/// </summary>
	public enum ToolWindowAlignmentMode
	{
		/// <summary>
		/// The <see cref="System.Windows.FrameworkElement.HorizontalAlignment"/> or <see cref="System.Windows.FrameworkElement.VerticalAlignment"/>, depending on the property, should be honored. The tool window will be positioned relative to the owning element.
		/// </summary>
		UseAlignment,

		/// <summary>
		/// The <see cref="ToolWindow.Left"/> or <see cref="ToolWindow.Top"/> property should be used to control the horizontal/vertical positioning.
		/// </summary>
		Manual,
	} 
	#endregion //ToolWindowAlignmentMode

	// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	#region ToolWindowPart
	internal enum ToolWindowPart
	{
		




		Caption,
		BorderLeft,
		BorderRight,
		BorderTop,
		BorderBottom,
		BorderTopLeft,
		BorderTopRight,
		BorderBottomLeft,
		BorderBottomRight,
		ResizeGrip,
		Content,
	}
	#endregion //ToolWindowPart

	// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	#region ToolWindowResizeElementLocation
	/// <summary>
	/// Enumeration indicating the location of a resize border.
	/// </summary>
	public enum ToolWindowResizeElementLocation
	{
		/// <summary>
		/// Represents the border edge used to increase the width from the left.
		/// </summary>
		Left,

		/// <summary>
		/// Represents the border edge used to increase the height from the top.
		/// </summary>
		Top,

		/// <summary>
		/// Represents the border edge used to increase the width from the right.
		/// </summary>
		Right,

		/// <summary>
		/// Represents the border edge used to increase the height from the bottom.
		/// </summary>
		Bottom,

		/// <summary>
		/// Represents the border edge used to increase the height and width from the top left corner.
		/// </summary>
		TopLeft,

		/// <summary>
		/// Represents the border edge used to increase the height and width from the top right corner.
		/// </summary>
		TopRight,

		/// <summary>
		/// Represents the border edge used to increase the height and width from the bottom left corner.
		/// </summary>
		BottomLeft,

		/// <summary>
		/// Represents the border edge used to increase the height and width from the top right corner.
		/// </summary>
		BottomRight,
	}
	#endregion //ToolWindowResizeElementLocation

	// AS 11/2/10 TFS49402/TFS49912/TFS51985
	#region ToolWindowStartupLocation
	/// <summary>
	/// Enumeration used to indicate how the <see cref="ToolWindow"/> should be positioned when it initially displayed.
	/// </summary>
	/// <seealso cref="ToolWindow.WindowStartupLocation"/>
	public enum ToolWindowStartupLocation
	{
		/// <summary>
		/// The position is based on the <see cref="ToolWindow.Left"/> and <see cref="ToolWindow.Top"/> or the alignments if the <see cref="ToolWindow.HorizontalAlignmentMode"/> or <see cref="ToolWindow.VerticalAlignmentMode"/> are set to UseAlignments.
		/// </summary>
		Manual,

		/// <summary>
		/// The position is calculated based on the logical screen relative to the owner.
		/// </summary>
		CenterScreen,

		/// <summary>
		/// The position is calculated based on the logical window relative to the owner.
		/// </summary>
		CenterOwnerWindow,
	} 
	#endregion // ToolWindowStartupLocation

	#region XamCarouselPanelStates
	/// <summary>
	/// Enumeration used to identify the navigation state of the <see cref="XamCarouselPanel"/>
	/// </summary>
	/// <remarks>
	/// <p class="body">These states are returned by the XamCarouselPanel in its implementation of ICommandHost.CurrentState
	/// The Infragistics.Windows.Commands.ICommandHost interface is for internal use by Infragistics controls and is used by the Infragistics command infrastructure 
	/// to evaluate command and control state mappings.</p>
	/// </remarks>
	/// <seealso cref="Infragistics.Windows.Commands.IGRoutedCommand"/>
	/// <seealso cref="Infragistics.Windows.Commands.IGRoutedUICommand"/>
	/// <seealso cref="XamCarouselPanelCommands"/>
	public enum XamCarouselPanelStates : long
	{
		/// <summary>
		/// The panel can navigate to the next item
		/// </summary>
		CanNavigateToNextItem = 0x01,

		/// <summary>
		/// The panel can navigate to the previous item
		/// </summary>
		CanNavigateToPreviousItem = 0x02,

		/// <summary>
		/// The panel can navigate to the next page of items
		/// </summary>
		CanNavigateToNextPage = 0x04,

		/// <summary>
		/// The panel can navigate to the previous page of items
		/// </summary>
		CanNavigateToPreviousPage = 0x08,
	}
	#endregion //XamCarouselPanelStates

}

#region Infragistics.Windows.Reporting
namespace Infragistics.Windows.Reporting
{
    #region PageOrientation
    /// <summary>
    /// Identifies the page orientation
    /// </summary>
    /// <seealso cref="ReportSettings.PageOrientation"/>
    public enum PageOrientation
    {
        /// <summary>
        /// The page will be oriented such that its height will be greater than its width.
        /// </summary>
        Portrait = 0,
        /// <summary>
        /// The page will be oriented such that its width will be greater than its height.
        /// </summary>
        Landscape = 1,
    } 
    #endregion

    #region PagePrintOrder
    /// <summary>
    /// Identifies the order pages in a report when the <see cref="HorizontalPaginationMode"/> is set to 'Mosaic' and logical pages are split up onto multiple pages.
    /// </summary>
    /// <remarks>
    /// <p class="body">When a logical page within a report is too wide to fit and <see cref="HorizontalPaginationMode"/> is set to 'Mosaic' 
    /// then it will be will be split up into multiple page parts horizontally. This property determines the order that the page parts are placed in the report.</p>
    /// </remarks>
    /// <seealso cref="ReportSettings.PagePrintOrder"/>
    /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
    /// <seealso cref="ReportSection.LogicalPageNumber"/>
    /// <seealso cref="ReportSection.LogicalPagePartNumber"/>
    public enum PagePrintOrder
    {
        /// <summary>
        /// Each logical page will print all of its multiple page parts, left to right, before the next logical page is printed.
        /// </summary>
        Horizontal = 0,
        /// <summary>
        /// Every logical page will print its 'part 1' first. This will then be follwed by every logical page's 'part 2' etc.  
        /// </summary>
        Vertical = 1,
    } 
    #endregion

    #region HorizontalPaginationMode
    /// <summary>
    /// Determines how to print a logical page when it it too wide to fit on a single page.
    /// </summary>
    /// <remarks>
    /// <p class="note"><b>Note:</b> if 'Scale' is specified then the aspect ratio will be maintained, i.e. the scale factor for the width and height will be the same.</p>
    /// </remarks>
    /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
    /// <seealso cref="ReportSettings.PagePrintOrder"/>
    /// <seealso cref="ReportSection.LogicalPageNumber"/>
    /// <seealso cref="ReportSection.LogicalPagePartNumber"/>
    public enum HorizontalPaginationMode
    {
        /// <summary>
        /// When the logical width of the page too wide to fit on a single page then scale the page down so it does fit while maintaining its aspect ratio. In other words, the sale factor for the width and height will be the same.
        /// </summary>
        Scale = 0,
        /// <summary>
        /// When the logical width of the page too wide to fit on a single page then split it up onto multiple separate pages based on the <see cref="ReportSettings.PagePrintOrder"/> setting. 
        /// </summary>
        Mosaic = 1,
    } 
    #endregion

    #region RepeatType
    /// <summary>
    /// Determines how logical headers within the page content area will be treated within a report section.
    /// </summary>
    /// <remarks>
    /// <para class="note"><b>Note:</b> In the case of a XamDataGrid this setting determines when field headers are displayed.</para>
    /// </remarks>
    /// <seealso cref="ReportSettings.RepeatType"/>
    public enum RepeatType
    {
        /// <summary>
        /// Logical content headers appear only on the page where they occur for the first time. 
        /// </summary>
        FirstOccurrence = 0,
        /// <summary>
        /// Logical content headers appear at the top of the each new page.
        /// </summary>
        PageBreak = 1,
        /// <summary>
        /// Logical content headers appear only when they are required when the context requires it, e.g. when record type changes.
        /// </summary>
        LevelBreak = 2,
    } 
    #endregion
}
#endregion

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