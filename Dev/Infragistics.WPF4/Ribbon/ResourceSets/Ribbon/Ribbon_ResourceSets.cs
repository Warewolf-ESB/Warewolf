using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Infragistics.Windows;

namespace Infragistics.Windows.Themes
{
	#region RibbonResourceSet<T> base class

	/// <summary>
	/// Abstract base class used to supply style resources for a specific look for Ribbons.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class RibbonResourceSet<T> : ResourceSet where T : ResourceSetLocator, new()
	{
		#region Constants

		// AS 11/6/07 ThemeGroupingName
		//static internal readonly string GroupingName = "Ribbon";
		internal const string GroupingName = "Ribbon";

		#endregion //Constants

		#region Base class overrides

		#region Grouping

		/// <summary>
		/// Returns the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: RibbonBase, Editors, RibbonBase, WPF etc.
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

		#region Location

		private static ResourceSetLocator g_Location;

		// AS 5/7/08
		/// <summary>
		/// Returns the <see cref="ResourceSetLocator"/> that describes the theme information for the resource set.
		/// </summary>
		//private static ResourceSetLocator Location
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

		#region ApplicationMenu

		private static Style g_ApplicationMenu;

		/// <summary>
		/// The style for a ApplicationMenu in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ApplicationMenu
		{
			get
			{
				if (g_ApplicationMenu == null)
					g_ApplicationMenu = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.ApplicationMenu)) as Style;

				return g_ApplicationMenu;
			}
		}

		#endregion //ApplicationMenu

		#region ApplicationMenuFooterToolbar

		private static Style g_ApplicationMenuFooterToolbar;

		/// <summary>
		/// The style for a ApplicationMenuFooterToolbar in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ApplicationMenuFooterToolbar
		{
			get
			{
				if (g_ApplicationMenuFooterToolbar == null)
					g_ApplicationMenuFooterToolbar = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.ApplicationMenuFooterToolbar)) as Style;

				return g_ApplicationMenuFooterToolbar;
			}
		}

		#endregion //ApplicationMenuFooterToolbar

		#region ApplicationMenuPresenter

		private static Style g_ApplicationMenuPresenter;

		/// <summary>
		/// The style for a ApplicationMenuPresenter in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ApplicationMenuPresenter
		{
			get
			{
				if (g_ApplicationMenuPresenter == null)
					g_ApplicationMenuPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.ApplicationMenuPresenter)) as Style;

				return g_ApplicationMenuPresenter;
			}
		}

		#endregion //ApplicationMenuPresenter

		#region ButtonTool

		private static Style g_ButtonTool;

		/// <summary>
		/// The style for a ButtonTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ButtonTool
		{
			get
			{
				if (g_ButtonTool == null)
					g_ButtonTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.ButtonTool)) as Style;

				return g_ButtonTool;
			}
		}

		#endregion //ButtonTool

		#region CheckBoxTool

		private static Style g_CheckBoxTool;

		/// <summary>
		/// The style for a CheckBoxTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style CheckBoxTool
		{
			get
			{
				if (g_CheckBoxTool == null)
					g_CheckBoxTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.CheckBoxTool)) as Style;

				return g_CheckBoxTool;
			}
		}

		#endregion //CheckBoxTool

        #region ComboEditorTool

        private static Style g_ComboEditorTool;

		/// <summary>
		/// The style for a ComboEditorTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ComboEditorTool
		{
			get
			{
				if (g_ComboEditorTool == null)
					g_ComboEditorTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.ComboEditorTool)) as Style;

				return g_ComboEditorTool;
			}
		}

		#endregion //ComboEditorTool

        #region ComboEditorToolComboBox

        private static Style g_ComboEditorToolComboBox;

		/// <summary>
		/// The style for a ComboEditorTool.ComboEditorToolComboBox in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ComboEditorToolComboBox
		{
			get
			{
				if (g_ComboEditorToolComboBox == null)
                    g_ComboEditorToolComboBox = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.ComboEditorTool.ComboEditorToolComboBoxStyleKey) as Style;

				return g_ComboEditorToolComboBox;
			}
		}

		#endregion //ComboEditorToolComboBox

        #region ComboEditorToolDropDownButton

        private static Style g_ComboEditorToolDropDownButton;

		/// <summary>
		/// The style for a ComboEditorTool.DropDownButtonStyle in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ComboEditorToolDropDownButton
		{
			get
			{
				if (g_ComboEditorToolDropDownButton == null)
                    g_ComboEditorToolDropDownButton = ResourceSet.GetSealedResource(ResourcesInternal, "ComboEditor_DropDownButtonStyle") as Style;

				return g_ComboEditorToolDropDownButton;
			}
		}

		#endregion //ComboEditorToolDropDownButton

		#region GalleryItemGroup

		private static Style g_GalleryItemGroup;

		/// <summary>
		/// The style for a GalleryItemGroup in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style GalleryItemGroup
		{
			get
			{
				if (g_GalleryItemGroup == null)
					g_GalleryItemGroup = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.GalleryItemGroup)) as Style;

				return g_GalleryItemGroup;
			}
		}

		#endregion //GalleryItemGroup

		#region GalleryItemPresenter

		private static Style g_GalleryItemPresenter;

		/// <summary>
		/// The style for a GalleryItemPresenter in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style GalleryItemPresenter
		{
			get
			{
				if (g_GalleryItemPresenter == null)
					g_GalleryItemPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.GalleryItemPresenter)) as Style;

				return g_GalleryItemPresenter;
			}
		}

		#endregion //GalleryItemPresenter

		#region GalleryTool

		private static Style g_GalleryTool;

		/// <summary>
		/// The style for a GalleryTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style GalleryTool
		{
			get
			{
				if (g_GalleryTool == null)
					g_GalleryTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.GalleryTool)) as Style;

				return g_GalleryTool;
			}
		}

		#endregion //GalleryTool

		#region GalleryToolDropDownPresenter

		private static Style g_GalleryToolDropDownPresenter;

		/// <summary>
		/// The style for a GalleryToolDropDownPresenter in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style GalleryToolDropDownPresenter
		{
			get
			{
				if (g_GalleryToolDropDownPresenter == null)
					g_GalleryToolDropDownPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.GalleryToolDropDownPresenter)) as Style;

				return g_GalleryToolDropDownPresenter;
			}
		}

		#endregion //GalleryToolDropDownPresenter

		#region GalleryToolPreviewPresenter

		private static Style g_GalleryToolPreviewPresenter;

		/// <summary>
		/// The style for a GalleryToolPreviewPresenter in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style GalleryToolPreviewPresenter
		{
			get
			{
				if (g_GalleryToolPreviewPresenter == null)
					g_GalleryToolPreviewPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.GalleryToolPreviewPresenter)) as Style;

				return g_GalleryToolPreviewPresenter;
			}
		}

		#endregion //GalleryToolPreviewPresenter

		#region KeyTip

		private static Style g_KeyTip;

		/// <summary>
		/// The style for a KeyTip in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style KeyTip
		{
			get
			{
				if (g_KeyTip == null)
					g_KeyTip = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.KeyTip)) as Style;

				return g_KeyTip;
			}
		}

		#endregion //KeyTip

		#region LabelTool

		private static Style g_LabelTool;

		/// <summary>
		/// The style for a LabelTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style LabelTool
		{
			get
			{
				if (g_LabelTool == null)
					g_LabelTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.LabelTool)) as Style;

				return g_LabelTool;
			}
		}

		#endregion //LabelTool

		#region LargeToolCaptionPresenter

		private static Style g_LargeToolCaptionPresenter;

		/// <summary>
		/// The style for a LargeToolCaptionPresenter in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style LargeToolCaptionPresenter
		{
			get
			{
				if (g_LargeToolCaptionPresenter == null)
					g_LargeToolCaptionPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.LargeToolCaptionPresenter)) as Style;

				return g_LargeToolCaptionPresenter;
			}
		}

		#endregion //LargeToolCaptionPresenter

        #region MaskedEditorTool

        private static Style g_MaskedEditorTool;

		/// <summary>
		/// The style for a MaskedEditorTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style MaskedEditorTool
		{
			get
			{
				if (g_MaskedEditorTool == null)
					g_MaskedEditorTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.MaskedEditorTool)) as Style;

				return g_MaskedEditorTool;
			}
		}

		#endregion //MaskedEditorTool

		#region MenuButtonArea

		private static Style g_MenuButtonArea;

		/// <summary>
		/// The style for a MenuButtonArea in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style MenuButtonArea
		{
			get
			{
				if (g_MenuButtonArea == null)
					g_MenuButtonArea = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.MenuButtonArea)) as Style;

				return g_MenuButtonArea;
			}
		}

		#endregion //MenuButtonArea

		#region MenuTool

		private static Style g_MenuTool;

		/// <summary>
		/// The style for a MenuTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style MenuTool
		{
			get
			{
				if (g_MenuTool == null)
					g_MenuTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.MenuTool)) as Style;

				return g_MenuTool;
			}
		}

		#endregion //MenuTool

        #region MenuTool_MenuItemDropDownArrow

        private static Style g_MenuToolMenuItemDropDownArrow;

		/// <summary>
        /// The style for a MenuTool.MenuItemDropDownArrow in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style MenuToolMenuItemDropDownArrow
		{
			get
			{
                if (g_MenuToolMenuItemDropDownArrow == null)
                    g_MenuToolMenuItemDropDownArrow = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.MenuTool.MenuItemDropDownArrowStyleKey) as Style;

                return g_MenuToolMenuItemDropDownArrow;
			}
        }

        #endregion //MenuToolMenuItemDropDownArrow

        #region MenuToolMenuToolDropDownArrow

        private static Style g_MenuToolMenuToolDropDownArrow;

		/// <summary>
        /// The style for a MenuTool.MenuToolDropDownArrow in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style MenuToolMenuToolDropDownArrow
		{
			get
			{
                if (g_MenuToolMenuToolDropDownArrow == null)
                    g_MenuToolMenuToolDropDownArrow = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.MenuTool.MenuToolDropDownArrowStyleKey) as Style;

                return g_MenuToolMenuToolDropDownArrow;
			}
        }

        #endregion //MenuToolMenuToolDropDownArrow

        #region MenuTool_QuickCustomizeMenuDropDownArrow

        private static Style g_MenuToolQuickCustomizeMenuDropDownArrow;

		/// <summary>
        /// The style for a MenuTool.QuickCustomizeMenuDropDownArrow in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style MenuToolQuickCustomizeMenuDropDownArrow
		{
			get
			{
                if (g_MenuToolQuickCustomizeMenuDropDownArrow == null)
                    g_MenuToolQuickCustomizeMenuDropDownArrow = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.MenuTool.QuickCustomizeMenuDropDownArrowStyleKey) as Style;

                return g_MenuToolQuickCustomizeMenuDropDownArrow;
			}
        }

        #endregion //MenuToolQuickCustomizeMenuDropDownArrow

		#region MenuToolPresenter

		private static Style g_MenuToolPresenter;

		/// <summary>
		/// The style for a MenuToolPresenter in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style MenuToolPresenter
		{
			get
			{
				if (g_MenuToolPresenter == null)
					g_MenuToolPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.MenuToolPresenter)) as Style;

				return g_MenuToolPresenter;
			}
		}

		#endregion //MenuToolPresenter

		#region QatPlaceholderTool

		private static Style g_QatPlaceholderTool;

		/// <summary>
		/// The style for a QatPlaceholderTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style QatPlaceholderTool
		{
			get
			{
				if (g_QatPlaceholderTool == null)
					g_QatPlaceholderTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.QatPlaceholderTool)) as Style;

				return g_QatPlaceholderTool;
			}
		}

		#endregion //QatPlaceholderTool

		#region QuickAccessToolbar

		private static Style g_QuickAccessToolbar;

		/// <summary>
		/// The style for a QuickAccessToolbar in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style QuickAccessToolbar
		{
			get
			{
				if (g_QuickAccessToolbar == null)
					g_QuickAccessToolbar = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.QuickAccessToolbar)) as Style;

				return g_QuickAccessToolbar;
			}
		}

		#endregion //QuickAccessToolbar

		#region RadioButtonTool

		private static Style g_RadioButtonTool;

		/// <summary>
		/// The style for a RadioButtonTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style RadioButtonTool
		{
			get
			{
				if (g_RadioButtonTool == null)
					g_RadioButtonTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.RadioButtonTool)) as Style;

				return g_RadioButtonTool;
			}
		}

		#endregion //RadioButtonTool

        #region RibbonCaptionPanelCaption

        private static Style g_RibbonCaptionPanelCaption;

		/// <summary>
        /// The style for a RibbonCaptionPanel.Caption in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style RibbonCaptionPanelCaption
		{
			get
			{
                if (g_RibbonCaptionPanelCaption == null)
                    g_RibbonCaptionPanelCaption = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.RibbonCaptionPanel.CaptionStyleKey) as Style;

                return g_RibbonCaptionPanelCaption;
			}
        }

        #endregion //RibbonCaptionPanelCaption

		#region RibbonContextMenu

		private static Style g_RibbonContextMenu;

		/// <summary>
		/// The style for a RibbonContextMenu in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style RibbonContextMenu
		{
			get
			{
				if (g_RibbonContextMenu == null)
					g_RibbonContextMenu = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.RibbonContextMenu)) as Style;

				return g_RibbonContextMenu;
			}
		}

		#endregion //RibbonContextMenu

		#region RibbonGroup

		private static Style g_RibbonGroup;

		/// <summary>
		/// The style for a RibbonGroup in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style RibbonGroup
		{
			get
			{
				if (g_RibbonGroup == null)
					g_RibbonGroup = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.RibbonGroup)) as Style;

				return g_RibbonGroup;
			}
		}

		#endregion //RibbonGroup

        #region RibbonGroupCollapsedGroupButton

        private static Style g_RibbonGroupCollapsedGroupButton;

		/// <summary>
        /// The style for a RibbonGroup.CollapsedGroupButton in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style RibbonGroupCollapsedGroupButton
		{
			get
			{
                if (g_RibbonGroupCollapsedGroupButton == null)
                    g_RibbonGroupCollapsedGroupButton = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.RibbonGroup.CollapsedGroupButtonStyleKey) as Style;

                return g_RibbonGroupCollapsedGroupButton;
			}
        }

        #endregion //RibbonGroupCollapsedGroupButton

        #region RibbonGroup_QuickAccessToolbarGroupButton

        private static Style g_RibbonGroupQuickAccessToolbarGroupButton;

		/// <summary>
        /// The style for a RibbonGroup.QuickAccessToolbarGroupButton in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style RibbonGroupQuickAccessToolbarGroupButton
		{
			get
			{
                if (g_RibbonGroupQuickAccessToolbarGroupButton == null)
                    g_RibbonGroupQuickAccessToolbarGroupButton = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.RibbonGroup.QuickAccessToolbarGroupButtonStyleKey) as Style;

                return g_RibbonGroupQuickAccessToolbarGroupButton;
			}
        }

        #endregion //RibbonGroupQuickAccessToolbarGroupButton

        #region RibbonGroupCollection_PagerScrollLeftButton

        private static Style g_RibbonGroupCollectionPagerScrollLeftButton;

		/// <summary>
        /// The style for a RibbonGroupCollection.PagerScrollLeftButtonStyle in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style RibbonGroupCollectionPagerScrollLeftButton
		{
			get
			{
                if (g_RibbonGroupCollectionPagerScrollLeftButton == null)
                    g_RibbonGroupCollectionPagerScrollLeftButton = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.RibbonGroupCollection.PagerScrollLeftButtonStyleKey) as Style;

                return g_RibbonGroupCollectionPagerScrollLeftButton;
			}
        }

        #endregion //RibbonGroupCollectionPagerScrollLeftButton

        #region RibbonGroupCollection_PagerScrollRightButton

        private static Style g_RibbonGroupCollectionPagerScrollRightButton;

		/// <summary>
        /// The style for a RibbonGroupCollection.PagerScrollRightButtonStyle in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style RibbonGroupCollectionPagerScrollRightButton
		{
			get
			{
                if (g_RibbonGroupCollectionPagerScrollRightButton == null)
                    g_RibbonGroupCollectionPagerScrollRightButton = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.RibbonGroupCollection.PagerScrollRightButtonStyleKey) as Style;

                return g_RibbonGroupCollectionPagerScrollRightButton;
			}
        }

        #endregion //RibbonGroupCollectionPagerScrollRightButton

		#region RibbonTabItem

		private static Style g_RibbonTabItem;

		/// <summary>
		/// The style for a RibbonTabItem in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style RibbonTabItem
		{
			get
			{
				if (g_RibbonTabItem == null)
					g_RibbonTabItem = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.RibbonTabItem)) as Style;

				return g_RibbonTabItem;
			}
		}

		#endregion //RibbonTabItem

        #region RibbonWindowContentHost

		private static Style g_RibbonWindowContentHost;

		/// <summary>
		/// The style for a RibbonWindowContentHost in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style RibbonWindowContentHost
		{
			get
			{
				if (g_RibbonWindowContentHost == null)
					g_RibbonWindowContentHost = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.RibbonWindowContentHost)) as Style;

				return g_RibbonWindowContentHost;
			}
		}

		#endregion //RibbonWindowContentHost

        #region RibbonWindowContentHostStatusBar

        private static Style g_RibbonWindowContentHostStatusBar;

		/// <summary>
        /// The style for a RibbonWindowContentHost.StatusBar in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style RibbonWindowContentHostStatusBar
		{
			get
			{
                if (g_RibbonWindowContentHostStatusBar == null)
                    g_RibbonWindowContentHostStatusBar = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.RibbonWindowContentHost.StatusBarStyleKey) as Style;

                return g_RibbonWindowContentHostStatusBar;
			}
        }

        #endregion //RibbonWindowContentHostStatusBar

        #region RibbonWindowContentHostStatusBarItem

        private static Style g_RibbonWindowContentHostStatusBarItem;

		/// <summary>
        /// The style for a RibbonWindowContentHost.StatusBarItem in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style RibbonWindowContentHostStatusBarItem
		{
			get
			{
                if (g_RibbonWindowContentHostStatusBarItem == null)
                    g_RibbonWindowContentHostStatusBarItem = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.RibbonWindowContentHost.StatusBarItemStyleKey) as Style;

                return g_RibbonWindowContentHostStatusBarItem;
			}
        }

        #endregion //RibbonWindowContentHostStatusBarItem

        #region RibbonWindowContentHostStatusBarSeparator

        private static Style g_RibbonWindowContentHostStatusBarSeparator;

		/// <summary>
        /// The style for a RibbonWindowContentHost.StatusBarSeparator in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style RibbonWindowContentHostStatusBarSeparator
		{
			get
			{
                if (g_RibbonWindowContentHostStatusBarSeparator == null)
                    g_RibbonWindowContentHostStatusBarSeparator = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.RibbonWindowContentHost.StatusBarSeparatorStyleKey) as Style;

                return g_RibbonWindowContentHostStatusBarSeparator;
			}
        }

        #endregion //RibbonWindowContentHostStatusBarSeparator

        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
        #region ScenicRibbonCaptionArea

		private static Style g_ScenicRibbonCaptionArea;

		/// <summary>
		/// The style for a ScenicRibbonCaptionArea in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ScenicRibbonCaptionArea
		{
			get
			{
				if (g_ScenicRibbonCaptionArea == null)
					g_ScenicRibbonCaptionArea = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.ScenicRibbonCaptionArea)) as Style;

				return g_ScenicRibbonCaptionArea;
			}
		}

		#endregion //ScenicRibbonCaptionArea

		#region SeparatorTool

		private static Style g_SeparatorTool;

		/// <summary>
		/// The style for a SeparatorTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style SeparatorTool
		{
			get
			{
				if (g_SeparatorTool == null)
					g_SeparatorTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.SeparatorTool)) as Style;

				return g_SeparatorTool;
			}
		}

		#endregion //SeparatorTool

		#region TextEditorTool

		private static Style g_TextEditorTool;

		/// <summary>
		/// The style for a TextEditorTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style TextEditorTool
		{
			get
			{
				if (g_TextEditorTool == null)
					g_TextEditorTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.TextEditorTool)) as Style;

				return g_TextEditorTool;
			}
		}

		#endregion //TextEditorTool

		#region ToggleButtonTool

		private static Style g_ToggleButtonTool;

		/// <summary>
		/// The style for a ToggleButtonTool in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ToggleButtonTool
		{
			get
			{
				if (g_ToggleButtonTool == null)
					g_ToggleButtonTool = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.ToggleButtonTool)) as Style;

				return g_ToggleButtonTool;
			}
		}

		#endregion //ToggleButtonTool

		#region ToolMenuItem

		private static Style g_ToolMenuItem;

		/// <summary>
		/// The style for a ToolMenuItem in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style ToolMenuItem
		{
			get
			{
				if (g_ToolMenuItem == null)
					g_ToolMenuItem = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.ToolMenuItem)) as Style;

				return g_ToolMenuItem;
			}
		}

		#endregion //ToolMenuItem

		#region XamRibbon

		private static Style g_XamRibbon;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style XamRibbon
		{
			get
			{
				if (g_XamRibbon == null)
					g_XamRibbon = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.XamRibbon)) as Style;

				return g_XamRibbon;
			}
		}

		#endregion //XamRibbon

        #region XamRibbon_PopupResizerBar

        private static Style g_XamRibbonPopupResizerBar;

		/// <summary>
        /// The style for a XamRibbon.PopupResizerBar in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style XamRibbonPopupResizerBar
		{
			get
			{
                if (g_XamRibbonPopupResizerBar == null)
                    g_XamRibbonPopupResizerBar = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.XamRibbon.PopupResizerBarStyleKey) as Style;

                return g_XamRibbonPopupResizerBar;
			}
        }

        #endregion //XamRibbonPopupResizerBar

        #region XamRibbonRibbonTabControl

        private static Style g_XamRibbonRibbonTabControl;

		/// <summary>
        /// The style for a XamRibbon.RibbonTabControl in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style XamRibbonRibbonTabControl
		{
			get
			{
                if (g_XamRibbonRibbonTabControl == null)
                    g_XamRibbonRibbonTabControl = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.XamRibbon.RibbonTabControlStyleKey) as Style;

                return g_XamRibbonRibbonTabControl;
			}
        }

        #endregion //XamRibbonRibbonTabControl

        #region XamRibbonToolTip

        private static Style g_XamRibbonToolTip;

		/// <summary>
        /// The style for a XamRibbon.ToolTip in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
        public static Style XamRibbonToolTip
		{
			get
			{
                if (g_XamRibbonToolTip == null)
                    g_XamRibbonToolTip = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.XamRibbon.ToolTipStyleKey) as Style;

                return g_XamRibbonToolTip;
			}
        }

        #endregion //XamRibbonToolTip

		#region XamRibbonScreenTip

		private static Style g_XamRibbonScreenTip;

		/// <summary>
        /// The style for a XamRibbonScreenTip in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style XamRibbonScreenTip
		{
			get
			{
				if (g_XamRibbonScreenTip == null)
					g_XamRibbonScreenTip = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.XamRibbonScreenTip)) as Style;

				return g_XamRibbonScreenTip;
			}
		}

		#endregion //XamRibbonScreenTip

		#region XamRibbonWindow

		private static Style g_XamRibbonWindow;

		/// <summary>
		/// The style for a XamRibbonWindow in a <see cref="Infragistics.Windows.Ribbon.XamRibbon"/> 
		/// </summary>
		public static Style XamRibbonWindow
		{
			get
			{
				if (g_XamRibbonWindow == null)
					g_XamRibbonWindow = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Ribbon.XamRibbonWindow)) as Style;

				return g_XamRibbonWindow;
			}
		}

		#endregion //XamRibbonWindow

        #region XamRibbonWindowResizeGrip

        private static Style g_XamRibbonWindowResizeGrip;

		/// <summary>
        /// The style for a XamRibbonWindow.ResizeGrip in a <see cref="Infragistics.Windows.Ribbon.XamRibbonWindow"/> 
		/// </summary>
        public static Style XamRibbonWindowResizeGrip
		{
			get
			{
                if (g_XamRibbonWindowResizeGrip == null)
                    g_XamRibbonWindowResizeGrip = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Ribbon.XamRibbonWindow.ResizeGripStyleKey) as Style;

                return g_XamRibbonWindowResizeGrip;
			}
        }

        #endregion //XamRibbonWindowResizeGrip

		#endregion //Public Properties

		#endregion //Static Properties
	}

	#endregion //RibbonResourceSet<T> base class

	#region RibbonGeneric

	/// <summary>
	/// Class used to supply style resources for the Generic look for a Ribbons
	/// </summary>
	public class RibbonGeneric : RibbonResourceSet<RibbonGeneric.Locator>
	{

		#region Instance static property

		private static RibbonGeneric g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonGeneric Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonGeneric();

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
			public override string Grouping { get { return RibbonGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonGeneric.xaml"; } }
		}
	}

	#endregion //RibbonGeneric

	#region RibbonOnyx

	/// <summary>
	/// Class used to supply style resources for the Onyx look for a Ribbons
	/// </summary>
	public class RibbonOnyx : RibbonResourceSet<RibbonOnyx.Locator>
	{

		#region Instance static property

		private static RibbonOnyx g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonOnyx Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonOnyx();

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
			public override string Theme { get { return ThemeManager.ThemeNameOnyx; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonOnyx.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonOnyx.xaml"; } }
		}
	}

	#endregion //RibbonOnyx

	#region RibbonAero

	/// <summary>
	/// Class used to supply style resources for the Aero look for a Ribbons
	/// </summary>
	public class RibbonAero : RibbonResourceSet<RibbonAero.Locator>
	{

		#region Instance static property

		private static RibbonAero g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonAero Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonAero();

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
			public override string Grouping { get { return RibbonAero.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonAero.xaml"; } }
		}
	}

	#endregion //RibbonAero

	#region RibbonRoyale

	/// <summary>
	/// Class used to supply style resources for the Royale look for a Ribbons
	/// </summary>
	public class RibbonRoyale : RibbonResourceSet<RibbonRoyale.Locator>
	{

		#region Instance static property

		private static RibbonRoyale g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonRoyale Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonRoyale();

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
			public override string Grouping { get { return RibbonRoyale.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonRoyale.xaml"; } }
		}
	}

	#endregion //RibbonRoyale

	#region RibbonRoyaleStrong

	/// <summary>
	/// Class used to supply style resources for the RoyaleStrong look for a Ribbons
	/// </summary>
	public class RibbonRoyaleStrong : RibbonResourceSet<RibbonRoyaleStrong.Locator>
	{

		#region Instance static property

		private static RibbonRoyaleStrong g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonRoyaleStrong Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonRoyaleStrong();

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
			public override string Theme { get { return ThemeManager.ThemeNameRoyaleStrong; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonRoyaleStrong.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonRoyaleStrong.xaml"; } }
		}
	}

	#endregion //RibbonRoyaleStrong

	#region RibbonLunaNormal

	/// <summary>
	/// Class used to supply style resources for the LunaNormal look for a Ribbons
	/// </summary>
	public class RibbonLunaNormal : RibbonResourceSet<RibbonLunaNormal.Locator>
	{

		#region Instance static property

		private static RibbonLunaNormal g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonLunaNormal Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonLunaNormal();

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
			public override string Grouping { get { return RibbonLunaNormal.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonLunaNormal.xaml"; } }
		}
	}

	#endregion //RibbonLunaNormal

	#region RibbonLunaOlive

	/// <summary>
	/// Class used to supply style resources for the LunaOlive look for a Ribbons
	/// </summary>
	public class RibbonLunaOlive : RibbonResourceSet<RibbonLunaOlive.Locator>
	{

		#region Instance static property

		private static RibbonLunaOlive g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonLunaOlive Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonLunaOlive();

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
			public override string Grouping { get { return RibbonLunaOlive.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonLunaOlive.xaml"; } }
		}
	}

	#endregion //RibbonLunaOlive

	#region RibbonLunaSilver

	/// <summary>
	/// Class used to supply style resources for the LunaSilver look for a Ribbons
	/// </summary>
	public class RibbonLunaSilver : RibbonResourceSet<RibbonLunaSilver.Locator>
	{

		#region Instance static property

		private static RibbonLunaSilver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonLunaSilver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonLunaSilver();

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
			public override string Grouping { get { return RibbonLunaSilver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonLunaSilver.xaml"; } }
		}
	}

	#endregion //RibbonLunaSilver

	#region RibbonOffice2k7Blue

	/// <summary>
	/// Class used to supply style resources for the Lite look for a Ribbons
	/// </summary>
	public class RibbonOffice2k7Blue : RibbonResourceSet<RibbonOffice2k7Blue.Locator>
	{

		#region Instance static property

		private static RibbonOffice2k7Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonOffice2k7Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonOffice2k7Blue();

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
			public override string Grouping { get { return RibbonOffice2k7Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonOffice2k7Blue.xaml"; } }
		}
	}

	#endregion //RibbonOffice2k7Blue

	#region RibbonOffice2k7Black

	/// <summary>
	/// Class used to supply style resources for the Lite look for a Ribbons
	/// </summary>
	public class RibbonOffice2k7Black : RibbonResourceSet<RibbonOffice2k7Black.Locator>
	{

		#region Instance static property

		private static RibbonOffice2k7Black g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonOffice2k7Black Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonOffice2k7Black();

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
			public override string Grouping { get { return RibbonOffice2k7Black.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonOffice2k7Black.xaml"; } }
		}
	}

	#endregion //RibbonOffice2k7Black

	#region RibbonOffice2k7Silver

	/// <summary>
	/// Class used to supply style resources for the Lite look for a Ribbons
	/// </summary>
	public class RibbonOffice2k7Silver : RibbonResourceSet<RibbonOffice2k7Silver.Locator>
	{

		#region Instance static property

		private static RibbonOffice2k7Silver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonOffice2k7Silver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonOffice2k7Silver();

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
			public override string Grouping { get { return RibbonOffice2k7Silver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonOffice2k7Silver.xaml"; } }
		}
	}

	#endregion //RibbonOffice2k7Silver

	// JJD 8/30/10 - Added Office 2010 Blue theme
	#region RibbonOffice2010Blue

	/// <summary>
	/// Class used to supply style resources for the Lite look for a Ribbons
	/// </summary>
	public class RibbonOffice2010Blue : RibbonResourceSet<RibbonOffice2010Blue.Locator>
	{

		#region Instance static property

		private static RibbonOffice2010Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonOffice2010Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonOffice2010Blue();

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
			public override string Grouping { get { return RibbonOffice2010Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonOffice2010Blue.xaml"; } }
		}
	}

	#endregion //RibbonOffice2010Blue

	#region RibbonHighContrast

	/// <summary>
	/// Class used to supply style resources for the Lite look for a Ribbons
	/// </summary>
	public class RibbonHighContrast : RibbonResourceSet<RibbonHighContrast.Locator>
	{

		#region Instance static property

		// AS 10/16/09 TFS23586
		// Added ThreadStatic so we have 1 instance per thread since the contained
		// resources cannot be shared between threads.
		//
		[ThreadStatic()]
		private static RibbonHighContrast g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonHighContrast Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonHighContrast();

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
			public override string Theme { get { return ThemeManager.ThemeNameHighContrast; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonHighContrast.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonHighContrast.xaml"; } }
		}
	}

	#endregion //RibbonHighContrast
	
	#region RibbonWashBaseDark

	/// <summary>
	/// Class used to supply style resources for the Lite look for a Ribbons
	/// </summary>
	public class RibbonWashBaseDark : RibbonResourceSet<RibbonWashBaseDark.Locator>
	{

		#region Instance static property

		private static RibbonWashBaseDark g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonWashBaseDark Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonWashBaseDark();

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
			public override string Theme { get { return ThemeManager.ThemeNameWashBaseDark; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonWashBaseDark.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonWashBaseDark.xaml"; } }
		}
	}

	#endregion //RibbonWashBaseDark

	#region RibbonWashBaseLight

	/// <summary>
	/// Class used to supply style resources for the Lite look for a Ribbons
	/// </summary>
	public class RibbonWashBaseLight : RibbonResourceSet<RibbonWashBaseLight.Locator>
	{

		#region Instance static property

		private static RibbonWashBaseLight g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonWashBaseLight Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonWashBaseLight();

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
			public override string Theme { get { return ThemeManager.ThemeNameWashBaseLight; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonWashBaseLight.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonWashBaseLight.xaml"; } }
		}
	}

	#endregion //RibbonWashBaseLight

    // JJD 4/9/10 - NA 2010 - volume 2 - Scenic Ribbon
	#region RibbonScenic

	/// <summary>
	/// Class used to supply style resources for the Scenic look for a Ribbons
	/// </summary>
	public class RibbonScenic : RibbonResourceSet<RibbonScenic.Locator>
	{

		#region Instance static property

		private static RibbonScenic g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonScenic Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonScenic();

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
			public override string Theme { get { return "Scenic"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonScenic.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonScenic.xaml"; } }
		}
	}

	#endregion //RibbonScenic

	// JJD 10/29/10 - NA 2011 - volume 1 - IGTheme
	#region RibbonIGTheme

	/// <summary>
	/// Class used to supply style resources for the IGTheme look for a Ribbons
	/// </summary>
	public class RibbonIGTheme : RibbonResourceSet<RibbonIGTheme.Locator>
	{

		#region Instance static property

		private static RibbonIGTheme g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonIGTheme Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonIGTheme();

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
			public override string Grouping { get { return RibbonIGTheme.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonIGTheme.xaml"; } }
		}
	}

	#endregion //RibbonIGTheme

	// JJD 02/16/12 - NA 2012 - volume 1 - Metro theme
	#region RibbonMetro

	/// <summary>
	/// Class used to supply style resources for the Metro look for a Ribbons
	/// </summary>
	public class RibbonMetro : RibbonResourceSet<RibbonMetro.Locator>
	{

		#region Instance static property

		private static RibbonMetro g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonMetro Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonMetro();

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
			public override string Grouping { get { return RibbonMetro.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Ribbon\RibbonMetro.xaml"; } }
		}
	}

	#endregion //RibbonMetro


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