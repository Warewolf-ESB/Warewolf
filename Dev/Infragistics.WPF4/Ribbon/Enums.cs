using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Ribbon
{
	#region GalleryItemSelectionDisplayMode

	/// <summary>
	/// Determines which area of the <see cref="GalleryItem"/> is highlighted when the item is selected.
	/// </summary>
	/// <seealso cref="GalleryItem"/>
	/// <seealso cref="GalleryItem.Image"/>
	/// <seealso cref="GalleryItem.Text"/>
	/// <seealso cref="GalleryTool.ItemSelected"/>
	/// <seealso cref="GalleryTool.SelectedItem"/>
	public enum GalleryItemSelectionDisplayMode
	{
		/// <summary>
		/// Default setting.  The ultimate default is <b>HighlightImageOnly</b>.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Selected <see cref="GalleryItem"/>s do not display differently than un-selected <see cref="GalleryItem"/>s.
		/// </summary>
		None = 1,

		/// <summary>
		/// Selected <see cref="GalleryItem"/>s are displayed with a highlight around the item's image.
		/// </summary>
		HighlightImageOnly = 2,

		/// <summary>
		/// Selected <see cref="GalleryItem"/>s are displayed with a highlight around the entire item (image and text).
		/// </summary>
		HighlightEntireItem = 3,
	}

	#endregion //GalleryItemSelectionDisplayMode

	#region GalleryItemTextDisplayMode

	/// <summary>
	/// Determines when <see cref="GalleryItem"/> text is displayed.
	/// </summary>
	/// <seealso cref="GalleryItem"/>
	/// <seealso cref="GalleryItemSettings.TextDisplayMode"/>
	/// <seealso cref="GalleryItem.Text"/>
	public enum GalleryItemTextDisplayMode
	{
		/// <summary>
		/// Default setting.  The ultimate default is <b>OnlyInDropDown</b>.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Always display the text of the <see cref="GalleryItem"/> regardless of location.
		/// </summary>
		Always = 1,

		/// <summary>
		/// Only display the text of the <see cref="GalleryItem"/> when it is displayed in the <see cref="GalleryTool"/> dropdown.
		/// </summary>
		OnlyInDropDown = 2,

		/// <summary>
		/// Never display the text of the <see cref="GalleryItem"/>.
		/// </summary>
		Never = 3,
	}

	#endregion //GalleryItemTextDisplayMode

	#region GalleryToolItemBehavior

	/// <summary>
	/// Determines how a <see cref="GalleryItem"/> behaves when clicked, including the event that is fired in response to the click.
	/// </summary>
	/// <seealso cref="GalleryTool"/>
	/// <seealso cref="GalleryItem"/>
	/// <seealso cref="GalleryTool.ItemClicked"/>
	/// <seealso cref="GalleryTool.ItemSelected"/>
	/// <seealso cref="GalleryTool.SelectedItem"/>
	public enum GalleryToolItemBehavior
	{
		/// <summary>
		/// The <see cref="GalleryItem"/> is not selected - instead a <see cref="GalleryTool.ItemClicked"/> event is raised.
		/// </summary>
		Button = 0,

		/// <summary>
		/// The <see cref="GalleryItem"/> is selected and <see cref="GalleryTool.SelectedItem"/> is set to reflect the selection.  The <see cref="GalleryTool.ItemSelected"/> event is raised.
		/// </summary>
		StateButton = 1,
	}

	#endregion //GalleryToolItemBehavior

	#region GroupVariantResizeAction
	/// <summary>
	/// Determines the size of the <see cref="RibbonGroup"/> when the associated <see cref="GroupVariant"/> is used to resize the element.
	/// </summary>
	/// <seealso cref="GroupVariant"/>
	/// <seealso cref="GroupVariant.ResizeAction"/>
	/// <seealso cref="GroupVariant.Priority"/>
	/// <seealso cref="RibbonGroup.Variants"/>
	/// <seealso cref="RibbonGroup.IsCollapsed"/>
	/// <seealso cref="RibbonGroup.MaximumSizeProperty"/>
	/// <seealso cref="RibbonGroup.MinimumSizeProperty"/>
	public enum GroupVariantResizeAction
	{
		

		/// <summary>
		/// The number of columns displayed in the preview of a <see cref="GalleryTool"/> within a <see cref="MenuTool"/> will be reduced from its <see cref="GalleryTool.MaxPreviewColumns"/> to the <see cref="GalleryTool.MinPreviewColumns"/>.
		/// </summary>
		ReduceGalleryPreviewItems,

		/// <summary>
		/// The number of rows (<see cref="ToolHorizontalWrapPanel.RowCount"/>) displayed within a <see cref="ToolHorizontalWrapPanel"/> will be increased from its <see cref="ToolHorizontalWrapPanel.MinRows"/> towards the <see cref="ToolHorizontalWrapPanel.MaxRows"/>.
		/// </summary>
		IncreaseHorizontalWrapRowCount,

		/// <summary>
		/// A <see cref="MenuTool"/> whose <see cref="MenuTool.ShouldDisplayGalleryPreview"/> is true and contains a <see cref="GalleryTool"/> (and therefore is displaying a preview of that GalleryTool) will be displayed as a <see cref="RibbonToolSizingMode"/>.<b>ImageAndTextLarge</b> sized tool without the gallery preview.
		/// </summary>
		HideGalleryPreview,

		/// <summary>
		/// Consecutive tools whose <see cref="RibbonToolHelper.SizingModeProperty"/> is resolved to <see cref="RibbonToolSizingMode"/>.<b>ImageAndTextLarge</b> will be reduced to vertically stacked tools with a SizingMode of <see cref="RibbonToolSizingMode"/>.<b>ImageAndTextNormal</b>.
		/// </summary>
		ReduceImageAndTextLargeTools,

		/// <summary>
		/// Consecutive tools whose <see cref="RibbonToolHelper.SizingModeProperty"/> is resolved to <see cref="RibbonToolSizingMode"/>.<b>ImageAndTextNormal</b> will be reduced to tools with a SizingMode of <see cref="RibbonToolSizingMode"/>.<b>ImageOnly</b>.
		/// </summary>
		ReduceImageAndTextNormalTools,

		/// <summary>
		/// The <see cref="RibbonGroup"/> will be displayed as a dropdown button that can be opened to display the contents of the RibbonGroup in a popup.
		/// </summary>
		CollapseRibbonGroup,
	} 
	#endregion //GroupVariantResizeAction

	#region KeyTipAlignment
	internal enum KeyTipAlignment
	{
		TopLeft = 0x1,
		TopCenter = 0x2,
		TopRight = 0x4,
		MiddleLeft = 0x10,
		MiddleCenter = 0x20,
		MiddleRight = 0x40,
		BottomLeft = 0x100,
		BottomCenter = 0x200,
		BottomRight = 0x400,
	} 
	#endregion //KeyTipAlignment

	#region KeyTipPlacementType
	/// <summary>
	/// Enumeration used to decorate an element for the purpose of positioning the keytip with respect to the element.
	/// </summary>
	public enum KeyTipPlacementType
	{
		/// <summary>
		/// Identifies the element that represents the small image for a tool for key tip placement purposes.
		/// </summary>
		SmallImage,

		/// <summary>
		/// Identifies the element that represents the check or radio indicator for a tool for key tip placement purposes.
		/// </summary>
		CheckIndicator,

		/// <summary>
		/// Identifies the element that represents the caption for a tool for key tip placement purposes.  This is used in a <see cref="RibbonTabItem"/> to position the Key Tip below the text.
		/// </summary>
		Caption,

		/// <summary>
		/// Identifies the element that causes the tool to dropdown for key tip placement purposes.  This is used for segmented buttons when a key tip is shown for the button portion and dropdown portion.
		/// </summary>
		DropDownButton,
	} 
	#endregion //KeyTipPlacementType

	#region MenuToolButtonType

	/// <summary>
	/// Determines how a <see cref="MenuTool"/> is used. Either as a single drop down or segmented into a dropdown area and a button area.
	/// </summary>
	/// <seealso cref="MenuTool"/>
	/// <seealso cref="MenuTool.ButtonType"/>
	public enum MenuToolButtonType
	{
		/// <summary>
		/// Pressing the left mouse down anywhere on the menu tool will toggle the tool�s 
		/// <see cref="MenuToolBase.IsOpen"/> property raising either the <see cref="MenuToolBase.Closed"/> 
		/// event or the <see cref="MenuToolBase.Opening"/>/<see cref="MenuToolBase.Opened"/> event pair.
		/// </summary>
		DropDown = 0,

		/// <summary>
		/// The tool is segmented into 2 button areas. Pressing the left mouse down on one area will toggle 
		/// the tool�s <see cref="MenuToolBase.IsOpen"/> property raising either the <see cref="MenuToolBase.Closed"/> 
		/// event or the <see cref="MenuToolBase.Opening"/>/<see cref="MenuToolBase.Opened"/> event pair. Clicking on the 
		/// other area with the left mouse button will raise the tool�s <see cref="MenuTool.Click"/> event. Note: 
		/// this event is raised on the mouse up.
		/// </summary>
		Segmented = 1,

		/// <summary>
		/// The tool is segmented into 2 button areas. Pressing the left mouse down on one area will toggle the tool�s 
		/// <see cref="MenuToolBase.IsOpen"/> property raising either the <see cref="MenuToolBase.Closed"/> event or the 
		/// <see cref="MenuToolBase.Opening"/>/<see cref="MenuToolBase.Opened"/> event pair. Clicking on the other area with 
		/// the left mouse button will raise the tool�s <see cref="MenuTool.Checked"/> or <see cref="MenuTool.Unchecked"/> 
		/// event. Note: these events are raised on the mouse up.
		/// </summary>
		SegmentedState = 2,
	}

	#endregion //MenuToolButtonType

	#region QuickAccessToolbarLocation

	/// <summary>
	/// Determines where the <see cref="QuickAccessToolbar"/> is positioned with respect to its owning <see cref="XamRibbon"/>
	/// </summary>
	/// <seealso cref="XamRibbon.QuickAccessToolbarLocation"/>
	/// <seealso cref="QuickAccessToolbar.IsBelowRibbon"/>
	/// <seealso cref="XamRibbon.QuickAccessToolbar"/>
	public enum QuickAccessToolbarLocation
	{
		/// <summary>
		/// The <see cref="QuickAccessToolbar"/> is displayed above the ribbon tabs in the caption area.
		/// </summary>
		AboveRibbon = 0,

		/// <summary>
		/// The <see cref="QuickAccessToolbar"/> is displayed below the ribbon.
		/// </summary>
		BelowRibbon = 1,
	}

	#endregion //QuickAccessToolbarLocation

	#region QatPlaceholderToolType
	/// <summary>
	/// Identifies the type of object that the <see cref="QatPlaceholderTool"/> represents on the <see cref="QuickAccessToolbar"/>
	/// </summary>
	/// <seealso cref="QatPlaceholderTool"/>
	/// <seealso cref="QatPlaceholderTool.TargetType"/>
	/// <seealso cref="QatPlaceholderTool.Target"/>
	public enum QatPlaceholderToolType
	{
		/// <summary>
		/// The <see cref="QatPlaceholderTool"/> represents a placeholder for a tool (<see cref="IRibbonTool"/>).
		/// </summary>
		Tool,

		/// <summary>
		/// The <see cref="QatPlaceholderTool"/> represents a placeholder for a <see cref="RibbonGroup"/>.
		/// </summary>
		RibbonGroup,
	}
	#endregion //QatPlaceholderToolType

	// AS 12/19/07 BR29199
	#region PopupOpeningReason
	internal enum PopupOpeningReason
	{
		/// <summary>
		/// The object is not opening a popup or is not controlling the opening. It could be 
		/// caused by a base class (e.g. MenuItem) handling.
		/// </summary>
		None,

		/// <summary>
		/// The popup is being opened explicitly by using the mouse
		/// </summary>
		Mouse,

		/// <summary>
		/// The popup is being opened using the keyboard.
		/// </summary>
		Keyboard,

		/// <summary>
		/// The popup is being opened using the keytips
		/// </summary>
		KeyTips,
	}
	#endregion //PopupOpeningReason

	// AS 10/24/07 AutoHide
	#region RibbonAutoHideState
	/// <summary>
	/// An enumeration used to indicate whether the content of the ribbon is visible based on the current 
	/// <see cref="XamRibbon.AutoHideHorizontalThreshold"/> and <see cref="XamRibbon.AutoHideVerticalThreshold"/> values.
	/// </summary>
	/// <seealso cref="XamRibbon.AutoHideState"/>
	/// <seealso cref="XamRibbon.AutoHideHorizontalThreshold"/>
	/// <seealso cref="XamRibbon.AutoHideVerticalThreshold"/>
	public enum RibbonAutoHideState
	{
		/// <summary>
		/// The size of the containing window is above the horizontal and vertical threshold so the 
		/// ribbon's content will be visible to the end user.
		/// </summary>
		NotHidden,

		/// <summary>
		/// The size of the containing window is below the horizontal and/or vertical threshold so the 
		/// ribbon's content should not be visible to the end user.
		/// </summary>
		Hidden,
	} 
	#endregion //RibbonAutoHideState

	#region RibbonMode

	internal enum RibbonMode
	{
		/// <summary>
		/// The ribbon is not dealing with the keyboard at all.
		/// </summary>
		Normal,

		/// <summary>
		/// The ribbon is in a state where it may show keytips. This usually happens as a result of the alt key being pressed but before its released.
		/// </summary>
		KeyTipsPending,

		/// <summary>
		/// Keytips are currently displayed and keyboard interaction is being used to navigate the keytips
		/// </summary>
		KeyTipsActive,

		/// <summary>
		/// The user is navigating between items in the ribbon - e.g. tabs, tools, etc.
		/// </summary>
		ActiveItemNavigation
	}

	#endregion //RibbonMode

	#region RibbonPanelVerticalToolAlignment
	/// <summary>
	/// Determines the vertical position of tools within a <see cref="ToolVerticalWrapPanel"/>
	/// </summary>
	/// <seealso cref="ToolVerticalWrapPanel"/>
	/// <seealso cref="ToolVerticalWrapPanel.VerticalToolAlignment"/>
	public enum RibbonPanelVerticalToolAlignment
	{
		/// <summary>
		/// The tools within a column of the <see cref="RibbonGroup"/> will be aligned to the 
		/// center of the panel.
		/// </summary>
		Center,

		/// <summary>
		/// The tools within a column of the <see cref="RibbonGroup"/> will be aligned to the top 
		/// of the panel.
		/// </summary>
		Top,

		/// <summary>
		/// The tools within a column of the <see cref="RibbonGroup"/> will be aligned to the bottom 
		/// of the panel.
		/// </summary>
		Bottom,
	}
	#endregion //RibbonPanelVerticalToolAlignment

	#region RibbonToolSizingMode

	/// <summary>
	/// Determines the sizing mode of a tool which affects the size of a tool and whether it 
	/// displays image, text or both.
	/// </summary>
	/// <seealso cref="RibbonGroup.MaximumSizeProperty"/>
	/// <seealso cref="RibbonGroup.MinimumSizeProperty"/>
	/// <seealso cref="ButtonTool.SizingMode"/>
	/// <seealso cref="MenuToolBase.SizingMode"/>
	public enum RibbonToolSizingMode
	{
		// JJD 9/7/07 - reversed order on enum
		// Note: These are now kept in smallest to largest order!

		/// <summary>
		/// The tool will display its image only.
		/// </summary>
		ImageOnly = 0,

		/// <summary>
		/// The tool will display its image and text in a normal size.
		/// </summary>
		ImageAndTextNormal = 1,

		/// <summary>
		/// The tool will display its image and text and its height will span the height of the items area of the containing <see cref="RibbonGroup"/>.
		/// </summary>
		ImageAndTextLarge = 2,
	}

	#endregion //RibbonToolSizingMode

	#region TextPlacement

	/// <summary>
	/// Determines how text is positioned with respect to an image.
	/// </summary>
	/// <seealso cref="GalleryItemSettings.TextPlacement"/>
	/// <seealso cref="GalleryItemPresenter.TextPlacementResolved"/>
	public enum TextPlacement
	{
		/// <summary>
		/// The enumeration value defined as the ultimate default for the element to which the enumeration appies.
		/// </summary>
		Default= 0,

		/// <summary>
		/// The <see cref="GalleryItem"/> will display its text above the image.
		/// </summary>
		AboveImage = 1,

		/// <summary>
		/// The <see cref="GalleryItem"/> will display its text below the image.
		/// </summary>
		BelowImage = 2,

		/// <summary>
		/// The <see cref="GalleryItem"/> will display its text to the left of the image.
		/// </summary>
		LeftOfImage = 3,

		/// <summary>
		/// The <see cref="GalleryItem"/> will display its text to the right of the image.
		/// </summary>
		RightOfImage = 4,
	}

	#endregion //TextPlacement

	#region ToolLocation

	/// <summary>
	/// Determines where the tool exists.
	/// </summary>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="XamRibbon.LocationProperty"/>
	/// <seealso cref="XamRibbon.GetLocation"/>
	public enum ToolLocation
	{
		/// <summary>
		/// The tool has not been sited.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// The tool is within a <see cref="Infragistics.Windows.Ribbon.RibbonGroup"/>.
		/// </summary>
		Ribbon = 1,

		/// <summary>
		/// The tool is on a menu (<see cref="Infragistics.Windows.Ribbon.MenuToolBase"/>).
		/// </summary>
		Menu = 2,

		/// <summary>
		/// The tool is on the <see cref="Infragistics.Windows.Ribbon.QuickAccessToolbar"/>.
		/// </summary>
		QuickAccessToolbar = 3,

		/// <summary>
		/// The tool is in the <see cref="Infragistics.Windows.Ribbon.ApplicationMenu"/>.
		/// </summary>
		ApplicationMenu = 4,

		/// <summary>
		/// The tool is on the <see cref="Infragistics.Windows.Ribbon.ApplicationMenuFooterToolbar"/>.
		/// </summary>
		ApplicationMenuFooterToolbar = 5,

		/// <summary>
		/// The tool is in the <see cref="Infragistics.Windows.Ribbon.ApplicationMenu.RecentItems"/> collection.
		/// </summary>
		ApplicationMenuRecentItems = 6,

		/// <summary>
		/// The tool is on a <see cref="MenuToolBase"/> that is located within the <see cref="System.Windows.Controls.ItemsControl.Items"/> collection of the <see cref="Infragistics.Windows.Ribbon.ApplicationMenu"/>.
		/// </summary>
		ApplicationMenuSubMenu = 7,
	}

	#endregion //ToolLocation    
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