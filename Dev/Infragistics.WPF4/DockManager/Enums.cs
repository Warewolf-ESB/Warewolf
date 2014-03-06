using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.DockManager
{
	#region ContentPaneStates
	/// <summary>
	/// Enumeration used to indicate the current state of a <see cref="ContentPane"/>
	/// </summary>
	[Flags()]
	public enum ContentPaneStates : long
	{
		/// <summary>
		/// The <see cref="ContentPane.IsActiveDocument"/> is true
		/// </summary>
		IsActiveDocument = 0x00000001,

		/// <summary>
		/// The <see cref="ContentPane.IsActivePane"/> is true
		/// </summary>
		IsActivePane = 0x00000002,

		/// <summary>
		/// The <see cref="ContentPane"/> can be closed.
		/// </summary>
		AllowClose = 0x00000004,

		/// <summary>
		/// The <see cref="ContentPane"/> is displayed within the <see cref="DocumentContentHost"/>
		/// </summary>
		IsDocument = 0x00000008,

		/// <summary>
		/// The <see cref="ContentPane"/> is displayed within a floating window
		/// </summary>
		IsFloating = 0x00000010,

		/// <summary>
		/// The <see cref="ContentPane"/> is displayed within a floating window that can be docked with other panes
		/// </summary>
		IsFloatingDockable = 0x00000020,

		/// <summary>
		/// The <see cref="ContentPane"/> is displayed within a floating window and cannot be docked within another pane.
		/// </summary>
		IsFloatingOnly = 0x00000040,

		/// <summary>
		/// The <see cref="ContentPane"/> is displayed within the unpinned tab area
		/// </summary>
		IsUnpinned = 0x00000080,

		/// <summary>
		/// The <see cref="ContentPane"/> is docked within one of the sides of the <see cref="XamDockManager"/>
		/// </summary>
		IsDocked = 0x00000100,
	} 
	#endregion //ContentPaneStates

	#region DockableAreas
	/// <summary>
	/// Flagged enumeration used to indicate which areas within the <see cref="XamDockManager"/>, a <see cref="ContentPane"/> may be repositioned.
	/// </summary>
	[Flags()]
	internal enum DockableAreas
	{
		/// <summary>
		/// The pane cannot be made dockable.
		/// </summary>
		None = 0x00,

		/// <summary>
		/// The pane may be displayed within the <see cref="XamDockManager"/> along the left edge.
		/// </summary>
		Left = 0x01,

		/// <summary>
		/// The pane may be displayed within the <see cref="XamDockManager"/> along the top edge.
		/// </summary>
		Top = 0x02,

		/// <summary>
		/// The pane may be displayed within the <see cref="XamDockManager"/> along the right edge.
		/// </summary>
		Right = 0x04,

		/// <summary>
		/// The pane may be displayed within the <see cref="XamDockManager"/> along the bottom edge.
		/// </summary>
		Bottom = 0x08,

		/// <summary>
		/// The pane may be displayed within a floating window and may be docked with other floating dockable windows.
		/// </summary>
		Floating = 0x10,
	}

	#endregion //DockableAreas

	#region DockingIndicatorPosition
	/// <summary>
	/// Enumeration used to identify the type of docking indicator.
	/// </summary>
	public enum DockingIndicatorPosition
	{
		// note the order has some impact on the order in which they're handled in the drag manager. center should be first.

		/// <summary>
		/// The center indicator allows some to dock a pane to the left, top, right, bottom and center.
		/// </summary>
		Center,

		/// <summary>
		/// The indicator used to dock a pane on the left.
		/// </summary>
		Left,

		/// <summary>
		/// The indicator used to dock a pane on the top.
		/// </summary>
		Top,

		/// <summary>
		/// The indicator used to dock a pane on the right.
		/// </summary>
		Right,

		/// <summary>
		/// The indicator used to dock a pane on the bottom.
		/// </summary>
		Bottom,
	}

	#endregion //DockingIndicatorPosition

	#region DockableState
	internal enum DockableState
	{
		Docked,
		Floating,
	} 
	#endregion //DockableState

	// AS 10/5/09 NA 2010.1 - LayoutMode
	#region DockedPaneLayoutMode
	/// <summary>
	/// Enumeration of layout style for root docked <see cref="SplitPane"/> instances in a <see cref="XamDockManager"/>
	/// </summary>
	/// <seealso cref="XamDockManager.LayoutMode"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
	public enum DockedPaneLayoutMode
	{
		/// <summary>
		/// Docked SplitPane instances in the <see cref="XamDockManager.Panes"/> collection are positioned on the edges of the container
		/// </summary>
		Standard,

		/// <summary>
		/// The inner most docked <see cref="SplitPane"/> in the <see cref="XamDockManager.Panes"/> fills the available space.
		/// </summary>
		FillContainer,
	}
	#endregion //DockedPaneLayoutMode

	#region DockManagerStates
	/// <summary>
	/// Enumeration used to indicate the current state of a <see cref="XamDockManager"/>
	/// </summary>
	[Flags()]
	public enum DockManagerStates : long
	{
		/// <summary>
		/// The <see cref="XamDockManager.ActivePane"/> is not null.
		/// </summary>
		ActivePane = 0x00000001,

		/// <summary>
		/// The <see cref="DocumentContentHost.ActiveDocument"/> is not null
		/// </summary>
		ActiveDocument = 0x00000002,

		/// <summary>
		/// The <see cref="DocumentContentHost.ActiveDocument"/> is not null
		/// </summary>
		HasDocumentContentHost = 0x00000004,
	} 
	#endregion //DockManagerStates

	// AS 6/24/11 FloatingWindowCaptionSource
	#region FloatingWindowCaptionSource
	/// <summary>
	/// Enumeration used to determine what is providing the caption for a floating <see cref="PaneToolWindow"/>
	/// </summary>
	/// <seealso cref="XamDockManager.FloatingWindowCaptionSource"/>
	public enum FloatingWindowCaptionSource
	{
		/// <summary>
		/// The <see cref="Infragistics.Windows.Controls.ToolWindow.Title"/> is shown.
		/// </summary>
		UseToolWindowTitle,

		/// <summary>
		/// When the <see cref="PaneToolWindow"/> is only showing a single ContentPane, the title 
		/// of the PaneToolWindow will be hidden and the <see cref="PaneHeaderPresenter"/> of the 
		/// <see cref="ContentPane"/> will be displayed instead. Note, if the tool window 
		/// has multiple ContentPane instances visible at the same time (e.g. 2 ContentPanes in a 
		/// Horizontal/Vertical SplitPane) then the title of the PaneToolWindow will still be displayed. 
		/// Also when using this value the <see cref="Infragistics.Windows.Controls.ToolWindow.UseOSNonClientArea"/> 
		/// will be forced to false since the OS non-client area cannot render WPF content.
		/// </summary>
		UseContentPaneCaption,
	} 
	#endregion //FloatingWindowCaptionSource

	// AS 6/9/11 TFS76337
	#region FloatingWindowDoubleClickAction
	/// <summary>
	/// Enumeration used to determine what happens when double clicking the floating window's caption.
	/// </summary>
	/// <remarks>
	/// <p class="note"><b>Note:</b> This property only impacts the floating ToolWindow's caption. It does not affect the behavior when double clicking on a <see cref="PaneHeaderPresenter"/> for a <see cref="ContentPane"/> or a <see cref="PaneTabItem"/>.</p>
	/// </remarks>
	/// <seealso cref="XamDockManager.FloatingWindowDoubleClickAction"/>
	public enum FloatingWindowDoubleClickAction
	{
		/// <summary>
		/// The ContentPane instances within a floating <see cref="PaneToolWindow"/> whose PaneLocation is DockableFloating will be returned to their previous docked locations.
		/// </summary>
		ToggleDockedState,

		/// <summary>
		/// The <see cref="Infragistics.Windows.Controls.ToolWindow.WindowState"/> of the window will be toggled between 'Normal' and 'Maximized' if the <see cref="Infragistics.Windows.Controls.ToolWindow.AllowMaximize"/> is true.
		/// </summary>
		ToggleWindowState,
	} 
	#endregion //FloatingWindowDoubleClickAction

    // AS 3/13/09 FloatingWindowDragMode
    #region FloatingWindowDragMode
    /// <summary>
    /// Enumeration used to determine when a floating window should be repositioned during a drag operation.
    /// </summary>
    public enum FloatingWindowDragMode
    {
        /// <summary>
        /// The pane(s) are moved during the drag operation.
        /// </summary>
        Immediate,

        /// <summary>
        /// The pane(s) are moved after the drag operation has been completed.
        /// </summary>
        Deferred,

        /// <summary>
        /// The value is determined based on the <see cref="System.Windows.SystemParameters.DragFullWindows"/>. If true, then Immediate will be used otherwise Deferred will be used.
        /// </summary>
        UseDragFullWindowsSystemSetting,

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		/// <summary>
		/// The drag operation is handled by the operating system and therefore the window or an outline will be positioned based on the system's 'Show Window Contents While Dragging' setting. Note, this setting will allow dragging windows into and out of a maximized state in Windows 7 (when the OS' setting allow it) but will mean that functionality such as setting the <see cref="Infragistics.Windows.DockManager.Dragging.MoveWindowAction.NewLocation"/> will not be supported.
		/// </summary>
		UseSystemWindowDrag,
    } 
    #endregion //FloatingWindowDragMode

	// AS 6/23/11 TFS73499
	#region FloatingWindowVisibility
	/// <summary>
	/// Enumeration used to determine whether floating windows should be displayed.
	/// </summary>
	public enum FloatingWindowVisibility
	{
		/// <summary>
		/// The floating windows will be displayed as long as the xamDockManager is in the visual tree of the root visual
		/// </summary>
		Visible,

		/// <summary>
		/// The floating windows will be displayed but hidden as long as the xamDockManager is in the visual tree of the root visual
		/// </summary>
		Hidden,

		/// <summary>
		/// The floating windows will not be shown.
		/// </summary>
		Collapsed,

		/// <summary>
		/// The floating windows will be visible based on the visibility of the root visual containing the xamDockManager
		/// </summary>
		SyncWithDockManagerWindow
	} 
	#endregion //FloatingWindowVisibility

	#region InitialPaneLocation
	/// <summary>
	/// Enumeration used to indicate where within the <see cref="XamDockManager"/> a <see cref="SplitPane"/> in the <see cref="XamDockManager.Panes"/> collection should be positioned.
	/// </summary>
	/// <seealso cref="XamDockManager.InitialLocationProperty"/>
	public enum InitialPaneLocation
	{
		/// <summary>
		/// The pane is displayed within the <see cref="XamDockManager"/> along the left edge.
		/// </summary>
		DockedLeft,

		/// <summary>
		/// The pane is displayed within the <see cref="XamDockManager"/> along the top edge.
		/// </summary>
		DockedTop,

		/// <summary>
		/// The pane is displayed within the <see cref="XamDockManager"/> along the right edge.
		/// </summary>
		DockedRight,

		/// <summary>
		/// The pane is displayed within the <see cref="XamDockManager"/> along the bottom edge.
		/// </summary>
		DockedBottom,

		/// <summary>
		/// The pane is displayed within a floating window and may participate in the docking with the <see cref="XamDockManager"/> and other floating dockable windows.
		/// </summary>
		DockableFloating,

		/// <summary>
		/// The pane is displayed in a floating window and is not allowed to be docked within the XamDockManager and cannot be docked with other floating panes.
		/// </summary>
		FloatingOnly,
	}

	#endregion //InitialPaneLocation

	#region PaneActionBehavior
	/// <summary>
	/// Enumeration used to determine which panes within a dockable <see cref="TabGroupPane"/> are affected by a particular pane action such as closing or unpinning a pane.
	/// </summary>
	public enum PaneActionBehavior
	{
		/// <summary>
		/// Only the selected pane will be affected.
		/// </summary>
		ActivePane,

		/// <summary>
		/// All panes within the <see cref="TabGroupPane"/> will be affected.
		/// </summary>
		AllPanes,
	}

	#endregion //PaneActionBehavior

	#region PaneCloseAction
	/// <summary>
	/// Enumeration used to indicate what should happen to a <see cref="ContentPane"/> when it has been closed.
	/// </summary>
	public enum PaneCloseAction
	{
		/// <summary>
		/// The pane is hidden but remains in the element tree.
		/// </summary>
		HidePane,

		/// <summary>
		/// The pane is removed from the <see cref="XamDockManager"/> and the associated content is no longer referenced.
		/// </summary>
		RemovePane,
	}

	#endregion //PaneCloseAction

	#region PaneFlyoutAnimation
	/// <summary>
	/// Enumeration used to control how the contents of an unpinned pane are brought into and out of view.
	/// </summary>
	public enum PaneFlyoutAnimation
	{
		/// <summary>
		/// No animation is used when showing or hiding the display of the unpinned pane.
		/// </summary>
		None,

		/// <summary>
		/// The flyout gradually fade into/out of view.
		/// </summary>
		Fade,

		/// <summary>
		/// The flyout increases in width/height as it comes into view depending on the edge to which it is unpinned and decreases as it goes out of view.
		/// </summary>
		Resize,

		/// <summary>
		/// The contents of the flyout will slide into and out of view.
		/// </summary>
		Slide,
	}

	#endregion //PaneFlyoutAnimation

	#region PaneLocation
	/// <summary>
	/// Enumeration used to determine where the containing pane is located within the <see cref="XamDockManager"/>
	/// </summary>
	public enum PaneLocation
	{
		/// <summary>
		/// The pane is not located within an identifiable area of a <see cref="XamDockManager"/>
		/// </summary>
		Unknown,

		/// <summary>
		/// The pane is docked along the left edge of the <see cref="XamDockManager"/>
		/// </summary>
		DockedLeft,

		/// <summary>
		/// The pane is docked along the right edge of the <see cref="XamDockManager"/>
		/// </summary>
		DockedRight,

		/// <summary>
		/// The pane is docked along the top edge of the <see cref="XamDockManager"/>
		/// </summary>
		DockedTop,

		/// <summary>
		/// The pane is docked along the bottom edge of the <see cref="XamDockManager"/>
		/// </summary>
		DockedBottom,

		/// <summary>
		/// The pane is floating and dockable.
		/// </summary>
		Floating,

		/// <summary>
		/// The pane is floating and non-dockable.
		/// </summary>
		FloatingOnly,

		/// <summary>
		/// The pane is currently unpinned and displayed within the unpinned tab area
		/// </summary>
		Unpinned,

		/// <summary>
		/// The pane is displayed within the <see cref="DocumentContentHost"/>
		/// </summary>
		Document,

	}

	#endregion //PaneLocation

	#region PaneMenuItem
	internal enum PaneMenuItem
	{
		FloatingOnly,
		Dockable,
		Document,
		AutoHide,
		Hide,
		Close,
		CloseAllButThis,
		NewHorizontalTabGroup,
		NewVerticalTabGroup,
		NextTabGroup,
		PreviousTabGroup,
		Separator,
	} 
	#endregion //PaneMenuItem

	#region PaneNavigationOrder
	/// <summary>
	/// Enumeration used to determine the order in which panes are navigated when using the keyboard or the <see cref="PaneNavigator"/>
	/// </summary>
	public enum PaneNavigationOrder
	{
		/// <summary>
		/// The items are navigated based on the order in which they were activated.
		/// </summary>
		ActivationOrder,

		/// <summary>
		/// The items are navigated based on the order in which they exist on the screen. Navigation occurs 
		/// from outermost to innermost within the <see cref="XamDockManager"/> followed by the floating panes.
		/// </summary>
		VisibleOrder,
	} 
	#endregion //PaneNavigationOrder

	#region PaneNavigatorButtonDisplayMode
	/// <summary>
	/// Enumeration used to determine when the button used to show the <see cref="PaneNavigator"/> should be displayed in the ui.
	/// </summary>
	public enum PaneNavigatorButtonDisplayMode
	{
		/// <summary>
		/// The button should only when the application is run within a browser (i.e. xbap)
		/// </summary>
		WhenHostedInBrowser,

		/// <summary>
		/// The button should always be shown.
		/// </summary>
		Always,

		/// <summary>
		/// The button should never be shown.
		/// </summary>
		Never,
	} 
	#endregion //PaneNavigatorButtonDisplayMode

	#region PaneLocationFilterFlags
	/// <summary>
	/// Used to filter the types of panes to evaluate when searching
	/// </summary>
	[Flags()]
	internal enum PaneFilterFlags
	{
		/// <summary>
		/// Include docked panes
		/// </summary>
		Docked = 0x1,

		/// <summary>
		/// Include panes within the <see cref="DocumentContentHost"/>
		/// </summary>
		Document = 0x2,

		/// <summary>
		/// Include floating only panes
		/// </summary>
		FloatingOnly = 0x4,

		/// <summary>
		/// Include floating dockable panes
		/// </summary>
		FloatingDockable = 0x8,

		/// <summary>
		/// Include unpinned panes
		/// </summary>
		Unpinned = 0x10,

		/// <summary>
		/// Include the pane even if its hidden/collapsed
		/// </summary>
		Hidden = 0x20,

		// AS 6/23/11 TFS73499
		/// <summary>
		/// Include the pane even if the containing window would not be visible
		/// </summary>
		IgnoreFloatingWindowVisibility = 0x40,

		/// <summary>
		/// All locations except documents
		/// </summary>
		AllVisibleExceptDocument = AllVisible & ~Document,

		/// <summary>
		/// Include all panes
		/// </summary>
		AllVisible = All & ~Hidden & ~IgnoreFloatingWindowVisibility ,

		/// <summary>
		/// Include all panes
		/// </summary>
		All = -1,
	}
	#endregion //PaneLocationFilterFlags

	#region PaneSplitterMode
	internal enum PaneSplitterMode
	{
		/// <summary>
		/// The splitter follows the element that it is used to resize
		/// </summary>
		SinglePane,

		/// <summary>
		/// The splitter is between two elements that it uses to resize the panes.
		/// </summary>
		BetweenPanes,
	} 
	#endregion //PaneSplitterMode

	#region PaneSplitterType
	/// <summary>
	/// Enumeration used to identify the type of <see cref="PaneSplitter"/>.
	/// </summary>
	/// <seealso cref="PaneSplitter.SplitterType"/>
	public enum PaneSplitterType
	{
		/// <summary>
		/// The splitter used to resize a root level <see cref="Infragistics.Windows.DockManager.SplitPane"/> within the <see cref="Infragistics.Windows.DockManager.XamDockManager"/>
		/// </summary>
		DockedPane,

		/// <summary>
		/// The splitter used to adjust the size of two siblings panes within a <see cref="Infragistics.Windows.DockManager.SplitPane"/>
		/// </summary>
		SplitPane,

		/// <summary>
		/// The splitter used to adjust the size an unpinned <see cref="Infragistics.Windows.DockManager.ContentPane"/> within the <see cref="Infragistics.Windows.DockManager.UnpinnedTabFlyout"/>
		/// </summary>
		UnpinnedPane,
	} 
	#endregion //PaneSplitterType

	// AS 11/12/09 TFS24789 - TabItemDragBehavior
	#region TabItemDragBehavior
	/// <summary>
	/// Enumeration used to determine the type of drop indication that is displayed when dragging over the tab item area of a <see cref="TabGroupPane"/>
	/// </summary>
	public enum TabItemDragBehavior
	{
		/// <summary>
		/// A drop preview using the <see cref="XamDockManager.DropPreviewStyleKey"/> is displayed.
		/// </summary>
		DisplayTabPreview,

		/// <summary>
		/// An insertion bar using the <see cref="Infragistics.Windows.Controls.DropIndicator"/> is displayed.
		/// </summary>
		DisplayInsertionBar,
	} 
	#endregion //TabItemDragBehavior

	#region UnpinnedFlyoutState
	internal enum UnpinnedFlyoutState
	{
		/// <summary>
		/// The flyout is closed and is not showing. No content pane in the flyout.
		/// </summary>
		Closed,

		/// <summary>
		/// The flyout is animating a close. Content pane still in flyout.
		/// </summary>
		Closing,

		/// <summary>
		/// The flyout is animating a close and the mouse will never be used to keep if from closing.
		/// </summary>
		ClosingIgnoreMouse,

		/// <summary>
		/// The flyout is in the process of animating its opening.
		/// </summary>
		Showing,

		/// <summary>
		/// The flyout is fully opened.
		/// </summary>
		Shown,
	} 
	#endregion //UnpinnedFlyoutState

	// AS 9/29/09 NA 2010.1 - UnpinnedTabHoverAction
	#region UnpinnedTabHoverAction
	/// <summary>
	/// Enumeration used to indicate what should happen when the mouse hovers over an unpinned tab for a <see cref="ContentPane"/>
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
	public enum UnpinnedTabHoverAction
	{
		/// <summary>
		/// The flyout for the unpinned ContentPane should be displayed when the mouse is hovered over the tab item.
		/// </summary>
		Flyout,

		/// <summary>
		/// No action should be taken when the mouse hovers over the unpinned tab item.
		/// </summary>
		None,
	}
	#endregion //UnpinnedTabHoverAction
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