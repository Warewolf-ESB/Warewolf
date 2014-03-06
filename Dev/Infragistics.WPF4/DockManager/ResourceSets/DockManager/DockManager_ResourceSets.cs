using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using Infragistics.Windows.DockManager;
using System.Windows.Controls;

namespace Infragistics.Windows.Themes
{
	#region DockManagerResourceSet<T> base class

	/// <summary>
	/// Abstract base class used to supply style resources for a specific look for XamDockManager.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class DockManagerResourceSet<T> : ResourceSet where T : ResourceSetLocator, new()
	{
		#region Constants

		internal const string GroupingName = "DockManager";

		#endregion //Constants

		#region Base class overrides

		#region Grouping

		/// <summary>
		/// Returns the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: DockManager, Editors, Primitives, etc.
		/// </remarks>
		public override string Grouping
		{
			get { return Location.Grouping; }
		}

		#endregion //Grouping

		#region Theme

		/// <summary>
		/// Returns the name of the look (read-only)
		/// </summary>
		public override string Theme
		{
			get
			{
				return Location.Theme;
			}
		}

		#endregion //Theme

		#region Resources

		/// <summary>
		/// Returns the ResourceDictionary containing the associated styles (read-only)
		/// </summary>
		public override ResourceDictionary Resources
		{
			get
			{
				//return ResourcesInternal;
				ResourceDictionary rd = ResourcesInternal;

				// JJD 7/25/07 - ResourceWasher support
				// Call VerifyResources after the initial load so that we can delay the hydrating
				// of the resources by a ResourceWasher until this theme is actually used
				this.VerifyResources();

				return rd;
			}
		}

		#endregion //Resources

		#endregion //Base class overrides

		#region Static Properties

		#region Private Propeties

		#region ResourcesInternal

		private static ResourceDictionary g_ResourcesInternal;

		private static ResourceDictionary ResourcesInternal
		{
			get
			{
				if (g_ResourcesInternal == null)
				{
					g_ResourcesInternal = Utilities.CreateResourceSetDictionary(Location.Assembly, Location.ResourcePath);
				}

				return g_ResourcesInternal;
			}
		}

		#endregion //ResourcesInternal

		#endregion //Private Propeties

		#region Public Properties

		#region Location

		private static ResourceSetLocator g_Location;

		/// <summary>
		/// Returns the <see cref="ResourceSetLocator"/> that describes the theme information for the resource set.
		/// </summary>
		public static ResourceSetLocator Location
		{
			get
			{
				if (g_Location == null)
					g_Location = new T();

				return g_Location;
			}
		}

		#endregion //Location

		#region Style Properties

		#region ContentPane

		private static Style g_ContentPane;

		/// <summary>
		/// The style for the <see cref="ContentPane"/> class
		/// </summary>
		/// <seealso cref="ContentPane"/>
		public static Style ContentPane
		{
			get
			{
				if ( g_ContentPane == null )
					g_ContentPane = ResourceSet.GetSealedResource( ResourcesInternal, typeof(ContentPane) ) as Style;

				return g_ContentPane;
			}
		}

		#endregion //ContentPane

		#region DockingIndicator

		private static Style g_DockingIndicator;

		/// <summary>
		/// The style for the <see cref="DockingIndicator"/> class
		/// </summary>
		/// <seealso cref="DockingIndicator"/>
		public static Style DockingIndicator
		{
			get
			{
				if ( g_DockingIndicator == null )
					g_DockingIndicator = ResourceSet.GetSealedResource( ResourcesInternal, typeof(DockingIndicator) ) as Style;

				return g_DockingIndicator;
			}
		}

		#endregion //DockingIndicator

		#region DocumentContentHost

		private static Style g_DocumentContentHost;

		/// <summary>
		/// The style for the <see cref="DocumentContentHost"/> class
		/// </summary>
		/// <seealso cref="DocumentContentHost"/>
		public static Style DocumentContentHost
		{
			get
			{
				if ( g_DocumentContentHost == null )
					g_DocumentContentHost = ResourceSet.GetSealedResource( ResourcesInternal, typeof(DocumentContentHost) ) as Style;

				return g_DocumentContentHost;
			}
		}

		#endregion //DocumentContentHost

		#region PaneHeaderPresenter

		private static Style g_PaneHeaderPresenter;

		/// <summary>
		/// The style for the <see cref="PaneHeaderPresenter"/> class
		/// </summary>
		/// <seealso cref="PaneHeaderPresenter"/>
		public static Style PaneHeaderPresenter
		{
			get
			{
				if ( g_PaneHeaderPresenter == null )
					g_PaneHeaderPresenter = ResourceSet.GetSealedResource( ResourcesInternal, typeof(PaneHeaderPresenter) ) as Style;

				return g_PaneHeaderPresenter;
			}
		}

		#endregion //PaneHeaderPresenter

		#region PaneNavigator

		private static Style g_PaneNavigator;

		/// <summary>
		/// The style for the <see cref="PaneNavigator"/> class
		/// </summary>
		/// <seealso cref="PaneNavigator"/>
		public static Style PaneNavigator
		{
			get
			{
				if ( g_PaneNavigator == null )
					g_PaneNavigator = ResourceSet.GetSealedResource( ResourcesInternal, typeof(PaneNavigator) ) as Style;

				return g_PaneNavigator;
			}
		}

		#endregion //PaneNavigator

		#region PaneSplitter

		private static Style g_PaneSplitter;

		/// <summary>
		/// The style for the <see cref="PaneSplitter"/>
		/// </summary>
		/// <seealso cref="PaneSplitter"/>
		public static Style PaneSplitter
		{
			get
			{
				if ( g_PaneSplitter == null )
					g_PaneSplitter = ResourceSet.GetSealedResource( ResourcesInternal, typeof(PaneSplitter) ) as Style;

				return g_PaneSplitter;
			}
		}

		#endregion //PaneSplitter

		#region PaneTabItem

		private static Style g_PaneTabItem;

		/// <summary>
		/// The style for the <see cref="PaneTabItem"/>
		/// </summary>
		/// <seealso cref="PaneTabItem"/>
		public static Style PaneTabItem
		{
			get
			{
				if ( g_PaneTabItem == null )
					g_PaneTabItem = ResourceSet.GetSealedResource( ResourcesInternal, typeof(PaneTabItem) ) as Style;

				return g_PaneTabItem;
			}
		}

		#endregion //PaneTabItem

		#region PaneToolWindow

		private static Style g_PaneToolWindow;

		/// <summary>
		/// The style for the <see cref="PaneToolWindow"/> class
		/// </summary>
		/// <seealso cref="PaneToolWindow"/>
		public static Style PaneToolWindow
		{
			get
			{
				if ( g_PaneToolWindow == null )
					g_PaneToolWindow = ResourceSet.GetSealedResource( ResourcesInternal, typeof(PaneToolWindow) ) as Style;

				return g_PaneToolWindow;
			}
		}

		#endregion //PaneToolWindow

		#region SplitPane

		private static Style g_SplitPane;

		/// <summary>
		/// The style for the <see cref="SplitPane"/> class
		/// </summary>
		public static Style SplitPane
		{
			get
			{
				if ( g_SplitPane == null )
					g_SplitPane = ResourceSet.GetSealedResource( ResourcesInternal, typeof(SplitPane) ) as Style;

				return g_SplitPane;
			}
		}

		#endregion //SplitPane
		
		#region TabGroupPane

		private static Style g_TabGroupPane;

		/// <summary>
		/// The style for the <see cref="TabGroupPane"/>
		/// </summary>
		/// <seealso cref="TabGroupPane"/>
		public static Style TabGroupPane
		{
			get
			{
				if ( g_TabGroupPane == null )
					g_TabGroupPane = ResourceSet.GetSealedResource( ResourcesInternal, typeof(TabGroupPane) ) as Style;

				return g_TabGroupPane;
			}
		}

		#endregion //TabGroupPane

		#region UnpinnedTabArea

		private static Style g_UnpinnedTabArea;

		/// <summary>
		/// The style for the <see cref="UnpinnedTabArea"/> class
		/// </summary>
		/// <seealso cref="UnpinnedTabArea"/>
		public static Style UnpinnedTabArea
		{
			get
			{
				if ( g_UnpinnedTabArea == null )
					g_UnpinnedTabArea = ResourceSet.GetSealedResource( ResourcesInternal, typeof(UnpinnedTabArea) ) as Style;

				return g_UnpinnedTabArea;
			}
		}

		#endregion //UnpinnedTabArea

		#region UnpinnedTabFlyout

		private static Style g_UnpinnedTabFlyout;

		/// <summary>
		/// The style for the <see cref="UnpinnedTabFlyout"/> class
		/// </summary>
		/// <seealso cref="UnpinnedTabFlyout"/>
		public static Style UnpinnedTabFlyout
		{
			get
			{
				if ( g_UnpinnedTabFlyout == null )
					g_UnpinnedTabFlyout = ResourceSet.GetSealedResource( ResourcesInternal, typeof(UnpinnedTabFlyout) ) as Style;

				return g_UnpinnedTabFlyout;
			}
		}

		#endregion //UnpinnedTabFlyout

		#region XamDockManager

		private static Style g_XamDockManager;

		/// <summary>
		/// The style for the <see cref="XamDockManager"/> control
		/// </summary>
		/// <seealso cref="XamDockManager"/>
		public static Style XamDockManager
		{
			get
			{
				if ( g_XamDockManager == null )
					g_XamDockManager = ResourceSet.GetSealedResource( ResourcesInternal, typeof(XamDockManager) ) as Style;

				return g_XamDockManager;
			}
		}

		#endregion //XamDockManager

		#region PaneHeaderPresenterCloseButton

		private static Style g_PaneHeaderPresenterCloseButton;

		/// <summary>
		/// The style for the Close button within the <see cref="PaneHeaderPresenter"/>
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.PaneHeaderPresenter.CloseButtonStyleKey"/>
		public static Style PaneHeaderPresenterCloseButton
		{
			get
			{
				if ( g_PaneHeaderPresenterCloseButton == null )
					g_PaneHeaderPresenterCloseButton = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.PaneHeaderPresenter.CloseButtonStyleKey ) as Style;

				return g_PaneHeaderPresenterCloseButton;
			}
		}

		#endregion //PaneHeaderPresenterCloseButton

		#region PaneHeaderPresenterPositionMenuItem

		private static Style g_PaneHeaderPresenterPositionMenuItem;

		/// <summary>
		/// The style for the window position menu item within the <see cref="PaneHeaderPresenter"/>
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.PaneHeaderPresenter.PositionMenuItemStyleKey"/>
		public static Style PaneHeaderPresenterPositionMenuItem
		{
			get
			{
				if ( g_PaneHeaderPresenterPositionMenuItem == null )
					g_PaneHeaderPresenterPositionMenuItem = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.PaneHeaderPresenter.PositionMenuItemStyleKey ) as Style;

				return g_PaneHeaderPresenterPositionMenuItem;
			}
		}

		#endregion //PaneHeaderPresenterPositionMenuItem

		#region PaneHeaderPresenterPinButton

		private static Style g_PaneHeaderPresenterPinButton;

		/// <summary>
		/// The style for the pin button within the <see cref="PaneHeaderPresenter"/>
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.PaneHeaderPresenter.PinButtonStyleKey"/>
		public static Style PaneHeaderPresenterPinButton
		{
			get
			{
				if ( g_PaneHeaderPresenterPinButton == null )
					g_PaneHeaderPresenterPinButton = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.PaneHeaderPresenter.PinButtonStyleKey ) as Style;

				return g_PaneHeaderPresenterPinButton;
			}
		}

		#endregion //PaneHeaderPresenterPinButton

		#region PaneNavigatorPanelScrollUpButton

		private static Style g_PaneNavigatorPanelScrollUpButton;

		/// <summary>
		/// The style for the up scroll button used by the <see cref="PaneNavigatorItemsPanel"/>
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.PaneNavigatorItemsPanel.ScrollUpButtonStyleKey"/>
		public static Style PaneNavigatorPanelScrollUpButton
		{
			get
			{
				if ( g_PaneNavigatorPanelScrollUpButton == null )
					g_PaneNavigatorPanelScrollUpButton = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.PaneNavigatorItemsPanel.ScrollUpButtonStyleKey ) as Style;

				return g_PaneNavigatorPanelScrollUpButton;
			}
		}

		#endregion //PaneNavigatorPanelScrollUpButton

		#region PaneNavigatorPanelScrollDownButton

		private static Style g_PaneNavigatorPanelScrollDownButton;

		/// <summary>
		/// The style for the down scroll button within the <see cref="PaneNavigatorItemsPanel"/>
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.PaneNavigatorItemsPanel.ScrollDownButtonStyleKey"/>
		public static Style PaneNavigatorPanelScrollDownButton
		{
			get
			{
				if ( g_PaneNavigatorPanelScrollDownButton == null )
					g_PaneNavigatorPanelScrollDownButton = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.DockManager.PaneNavigatorItemsPanel.ScrollDownButtonStyleKey) as Style;

				return g_PaneNavigatorPanelScrollDownButton;
			}
		}

		#endregion //PaneNavigatorPanelScrollDownButton

		#region PaneSplitterPreview

		private static Style g_PaneSplitterPreview;

		/// <summary>
		/// The style for the preview displayed while dragging a <see cref="PaneSplitter"/>
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.PaneSplitter.PreviewStyleKey"/>
		public static Style PaneSplitterPreview
		{
			get
			{
				if ( g_PaneSplitterPreview == null )
					g_PaneSplitterPreview = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.PaneSplitter.PreviewStyleKey ) as Style;

				return g_PaneSplitterPreview;
			}
		}

		#endregion //PaneSplitterPreview

		#region TabGroupPaneDocumentCloseButton

		private static Style g_TabGroupPaneDocumentCloseButton;

		/// <summary>
		/// The style for the close button for a <see cref="TabGroupPane"/> when used within a <see cref="DocumentContentHost"/>.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.TabGroupPane.DocumentCloseButtonStyleKey"/>
		public static Style TabGroupPaneDocumentCloseButton
		{
			get
			{
				if ( g_TabGroupPaneDocumentCloseButton == null )
					g_TabGroupPaneDocumentCloseButton = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.TabGroupPane.DocumentCloseButtonStyleKey ) as Style;

				return g_TabGroupPaneDocumentCloseButton;
			}
		}

		#endregion //TabGroupPaneDocumentCloseButton

		#region TabGroupPaneDocumentFilesMenuItem

		private static Style g_TabGroupPaneDocumentFilesMenuItem;

		/// <summary>
		/// The style for the file <see cref="MenuItem"/> used within the <see cref="TabGroupPane"/> when used within a <see cref="DocumentContentHost"/>.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.TabGroupPane.DocumentFilesMenuItemStyleKey"/>
		public static Style TabGroupPaneDocumentFilesMenuItem
		{
			get
			{
				if ( g_TabGroupPaneDocumentFilesMenuItem == null )
					g_TabGroupPaneDocumentFilesMenuItem = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.TabGroupPane.DocumentFilesMenuItemStyleKey ) as Style;

				return g_TabGroupPaneDocumentFilesMenuItem;
			}
		}

		#endregion //TabGroupPaneDocumentFilesMenuItem

		#region TabGroupPanelDocumentPaneNavigatorButton

		private static Style g_TabGroupPanelDocumentPaneNavigatorButton;

		/// <summary>
		/// The style for the pane navigator button when the <see cref="TabGroupPane"/> is used within a <see cref="DocumentContentHost"/>.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.TabGroupPane.DocumentPaneNavigatorButtonStyleKey"/>
		public static Style TabGroupPanelDocumentPaneNavigatorButton
		{
			get
			{
				if ( g_TabGroupPanelDocumentPaneNavigatorButton == null )
					g_TabGroupPanelDocumentPaneNavigatorButton = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.TabGroupPane.DocumentPaneNavigatorButtonStyleKey ) as Style;

				return g_TabGroupPanelDocumentPaneNavigatorButton;
			}
		}

		#endregion //TabGroupPanelDocumentPaneNavigatorButton

		#region DockManagerDropPreview

		private static Style g_DockManagerDropPreview;

		/// <summary>
		/// The style for the preview control used to indicate the position at which a pane will be positioned during a drag operation.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.XamDockManager.DropPreviewStyleKey"/>
		public static Style DockManagerDropPreview
		{
			get
			{
				if ( g_DockManagerDropPreview == null )
					g_DockManagerDropPreview = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.XamDockManager.DropPreviewStyleKey ) as Style;

				return g_DockManagerDropPreview;
			}
		}

		#endregion //DockManagerDropPreview

		#region DockManagerContextMenu

		private static Style g_DockManagerContextMenu;

		/// <summary>
		/// The style for the <see cref="ContextMenu"/> instances created by the <see cref="XamDockManager"/> elements.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.XamDockManager.ContextMenuStyleKey"/>
		public static Style DockManagerContextMenu
		{
			get
			{
				if ( g_DockManagerContextMenu == null )
					g_DockManagerContextMenu = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.XamDockManager.ContextMenuStyleKey ) as Style;

				return g_DockManagerContextMenu;
			}
		}

		#endregion //DockManagerContextMenu

		#region DockManagerMenuItemSeparator

		private static Style g_DockManagerMenuItemSeparator;

		/// <summary>
		/// The style for a <see cref="Separator"/> using within a menu of the <see cref="XamDockManager"/>.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.XamDockManager.MenuItemSeparatorStyleKey"/>
		public static Style DockManagerMenuItemSeparator
		{
			get
			{
				if ( g_DockManagerMenuItemSeparator == null )
					g_DockManagerMenuItemSeparator = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.XamDockManager.MenuItemSeparatorStyleKey ) as Style;

				return g_DockManagerMenuItemSeparator;
			}
		}

		#endregion //DockManagerMenuItemSeparator

		#region DockManagerMenuItem

		private static Style g_DockManagerMenuItem;

		/// <summary>
		/// The style for a <see cref="MenuItem"/> used within the <see cref="XamDockManager"/> and its associated elements.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.XamDockManager.MenuItemStyleKey"/>
		public static Style DockManagerMenuItem
		{
			get
			{
				if ( g_DockManagerMenuItem == null )
					g_DockManagerMenuItem = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.XamDockManager.MenuItemStyleKey ) as Style;

				return g_DockManagerMenuItem;
			}
		}

		#endregion //DockManagerMenuItem

		#region DockManagerToolTip

		private static Style g_DockManagerToolTip;

		/// <summary>
		/// The style for a <see cref="ToolTip"/> used within the <see cref="XamDockManager"/>
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DockManager.XamDockManager.ToolTipStyleKey"/>
		public static Style DockManagerToolTip
		{
			get
			{
				if ( g_DockManagerToolTip == null )
					g_DockManagerToolTip = ResourceSet.GetSealedResource( ResourcesInternal, Infragistics.Windows.DockManager.XamDockManager.ToolTipStyleKey ) as Style;

				return g_DockManagerToolTip;
			}
		}

		#endregion //DockManagerToolTip
		
		#endregion //Style Properties

		#endregion //Public Properties

		#endregion //Static Properties
	}

	#endregion //DockManagerResourceSet<T> base class

	#region DockManagerGeneric

	/// <summary>
	/// Class used to supply style resources for the Generic look for a DockManagers
	/// </summary>
	public class DockManagerGeneric : DockManagerResourceSet<DockManagerGeneric.Locator>
	{

		#region Instance static property

		private static DockManagerGeneric g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerGeneric Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerGeneric();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameGeneric; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerGeneric.xaml"; } }
		}
	}

	#endregion //DockManagerGeneric

	#region DockManagerLunaNormal

	/// <summary>
	/// Class used to supply style resources for the LunaNormal look for a DockManagers
	/// </summary>
	public class DockManagerLunaNormal : DockManagerResourceSet<DockManagerLunaNormal.Locator>
	{

		#region Instance static property

		private static DockManagerLunaNormal g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerLunaNormal Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerLunaNormal();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameLunaNormal; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerLunaNormal.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerLunaNormal.xaml"; } }
		}
	}

	#endregion //DockManagerLunaNormal

	#region DockManagerLunaOlive

	/// <summary>
	/// Class used to supply style resources for the LunaOlive look for a DockManagers
	/// </summary>
	public class DockManagerLunaOlive : DockManagerResourceSet<DockManagerLunaOlive.Locator>
	{

		#region Instance static property

		private static DockManagerLunaOlive g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerLunaOlive Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerLunaOlive();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameLunaOlive; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerLunaOlive.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerLunaOlive.xaml"; } }
		}
	}

	#endregion //DockManagerLunaOlive

	#region DockManagerLunaSilver

	/// <summary>
	/// Class used to supply style resources for the LunaSilver look for a DockManagers
	/// </summary>
	public class DockManagerLunaSilver : DockManagerResourceSet<DockManagerLunaSilver.Locator>
	{

		#region Instance static property

		private static DockManagerLunaSilver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerLunaSilver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerLunaSilver();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameLunaSilver; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerLunaSilver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerLunaSilver.xaml"; } }
		}
	}

	#endregion //DockManagerLunaSilver

	#region DockManagerAero

	/// <summary>
	/// Class used to supply style resources for the Aero look for a DockManagers
	/// </summary>
	public class DockManagerAero : DockManagerResourceSet<DockManagerAero.Locator>
	{

		#region Instance static property

		private static DockManagerAero g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerAero Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerAero();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameAero; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerAero.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerAero.xaml"; } }
		}
	}

	#endregion //DockManagerAero

	#region DockManagerOffice2k7Black

	/// <summary>
	/// Class used to supply style resources for the Office2k7Black look for a DockManagers
	/// </summary>
	public class DockManagerOffice2k7Black : DockManagerResourceSet<DockManagerOffice2k7Black.Locator>
	{

		#region Instance static property

		private static DockManagerOffice2k7Black g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerOffice2k7Black Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerOffice2k7Black();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Black; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerOffice2k7Black.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerOffice2k7Black.xaml"; } }
		}
	}

	#endregion //DockManagerOffice2k7Black

	#region DockManagerOffice2k7Blue

	/// <summary>
	/// Class used to supply style resources for the Office2k7Blue look for a DockManagers
	/// </summary>
	public class DockManagerOffice2k7Blue : DockManagerResourceSet<DockManagerOffice2k7Blue.Locator>
	{

		#region Instance static property

		private static DockManagerOffice2k7Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerOffice2k7Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerOffice2k7Blue();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Blue; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerOffice2k7Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerOffice2k7Blue.xaml"; } }
		}
	}

	#endregion //DockManagerOffice2k7Blue

	#region DockManagerOffice2k7Silver

	/// <summary>
	/// Class used to supply style resources for the Office2k7Silver look for a DockManagers
	/// </summary>
	public class DockManagerOffice2k7Silver : DockManagerResourceSet<DockManagerOffice2k7Silver.Locator>
	{

		#region Instance static property

		private static DockManagerOffice2k7Silver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerOffice2k7Silver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerOffice2k7Silver();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Silver; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerOffice2k7Silver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerOffice2k7Silver.xaml"; } }
		}
	}

	#endregion //DockManagerOffice2k7Silver

	// JJD 8/30/10 - Added Office 2010 Blue theme
	#region DockManagerOffice2010Blue

	/// <summary>
	/// Class used to supply style resources for the Office2010Blue look for a DockManagers
	/// </summary>
	public class DockManagerOffice2010Blue : DockManagerResourceSet<DockManagerOffice2010Blue.Locator>
	{

		#region Instance static property

		private static DockManagerOffice2010Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerOffice2010Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerOffice2010Blue();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2010Blue; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerOffice2010Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerOffice2010Blue.xaml"; } }
		}
	}

	#endregion //DockManagerOffice2010Blue

	#region DockManagerRoyale

	/// <summary>
	/// Class used to supply style resources for the Office2k7Silver look for a DockManagers
	/// </summary>
	public class DockManagerRoyale : DockManagerResourceSet<DockManagerRoyale.Locator>
	{

		#region Instance static property

		private static DockManagerRoyale g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerRoyale Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerRoyale();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameRoyale; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerRoyale.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerRoyale.xaml"; } }
		}
	}

	#endregion DockManagerRoyale

	#region DockManagerWashBaseDark

	/// <summary>
	/// Class used to supply style resources for the Office2k7Silver look for a DockManagers
	/// </summary>
	public class DockManagerWashBaseDark : DockManagerResourceSet<DockManagerWashBaseDark.Locator>
	{

		#region Instance static property

		private static DockManagerWashBaseDark g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerWashBaseDark Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerWashBaseDark();

				return g_Instance;
			}
		}

		#endregion Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameWashBaseDark; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerWashBaseDark.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerWashBaseDark.xaml"; } }
		}
	}

	#endregion DockManagerWashBaseDark

	#region DockManagerWashBaseLight

	/// <summary>
	/// Class used to supply style resources for the Office2k7Silver look for a DockManagers
	/// </summary>
	public class DockManagerWashBaseLight : DockManagerResourceSet<DockManagerWashBaseLight.Locator>
	{

		#region Instance static property

		private static DockManagerWashBaseLight g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerWashBaseLight Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerWashBaseLight();

				return g_Instance;
			}
		}

		#endregion Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameWashBaseLight; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerWashBaseLight.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerWashBaseLight.xaml"; } }
		}
	}

	#endregion DockManagerWashBaseLight

	// JJD 10/29/10 - NA 2011 - Volumn 1 - IGTheme
	#region DockManagerIGTheme

	/// <summary>
	/// Class used to supply style resources for the IGTheme look for a DockManagers
	/// </summary>
	public class DockManagerIGTheme : DockManagerResourceSet<DockManagerIGTheme.Locator>
	{

		#region Instance static property

		private static DockManagerIGTheme g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerIGTheme Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerIGTheme();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameIGTheme; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerIGTheme.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerIGTheme.xaml"; } }
		}
	}

	#endregion //DockManagerIGTheme

	// JJD 02/16/12 - NA 2012 - Volumn 1 - Metro theme
	#region DockManagerMetro

	/// <summary>
	/// Class used to supply style resources for the Metro look for a DockManagers
	/// </summary>
	public class DockManagerMetro : DockManagerResourceSet<DockManagerMetro.Locator>
	{

		#region Instance static property

		private static DockManagerMetro g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerMetro Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DockManagerMetro();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameMetro; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerMetro.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManager\DockManagerMetro.xaml"; } }
		}
	}

	#endregion //DockManagerMetro
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