using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using System.ComponentModel;
using Infragistics.Windows.Tiles;
using Microsoft.Windows.Design.PropertyEditing;

namespace Infragistics.Windows.Design.Tiles
{
	// JM 01-06-10 VS2010 Designer Support
	#region DesignMetadataHelper Static Class

	internal static class DesignMetadataHelper
	{
		internal static AttributeTableBuilder GetAttributeTableBuilder()
		{
			AttributeTableBuilder builder = new AttributeTableBuilder();

			#region Description, Category, etc.

			// Infragistics.Windows.Tiles.XamTilesControl
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.XamTilesControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				//callbackBuilder.AddCustomAttributes("ActivePane", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_ActivePane"));
			});

            // Infragistics.Windows.Tiles.TileHeaderPresenter
			// =====================================================
            builder.AddCallback(typeof(Infragistics.Windows.Tiles.TileHeaderPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				//callbackBuilder.AddCustomAttributes("TabStripPlacement", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_UnpinnedTabItemPanel_P_TabStripPlacement"));
			});

			// Infragistics.Windows.Tiles.TilesPanel
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.TilesPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

            // Infragistics.Windows.Tiles.TileAreaSplitter
			// ========================================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.TileAreaSplitter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.Tiles.ModeSettingsBase
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.ModeSettingsBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowTileDragging", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_ModeSettingsBase_P_AllowTileDragging"));
				callbackBuilder.AddCustomAttributes("HorizontalTileAreaAlignment", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_ModeSettingsBase_P_HorizontalTileAreaAlignment"));
				callbackBuilder.AddCustomAttributes("RepositionAnimation", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_ModeSettingsBase_P_RepositionAnimation"));
				callbackBuilder.AddCustomAttributes("ResizeAnimation", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_ModeSettingsBase_P_ResizeAnimation"));
				callbackBuilder.AddCustomAttributes("ShouldAnimate", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_ModeSettingsBase_P_ShouldAnimate"));
				callbackBuilder.AddCustomAttributes("VerticalTileAreaAlignment", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_ModeSettingsBase_P_VerticalTileAreaAlignment"));
			});

			// Infragistics.Windows.Tiles.MaximizedModeSettings
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.MaximizedModeSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("InterTileAreaSpacing", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_InterTileAreaSpacing"));
				callbackBuilder.AddCustomAttributes("InterTileSpacingXMaximized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_InterTileSpacingXMaximized"));
				callbackBuilder.AddCustomAttributes("InterTileSpacingXMinimized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_InterTileSpacingXMinimized"));
				callbackBuilder.AddCustomAttributes("InterTileSpacingYMaximized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_InterTileSpacingYMaximized"));
				callbackBuilder.AddCustomAttributes("InterTileSpacingYMinimized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_InterTileSpacingYMinimized"));
				callbackBuilder.AddCustomAttributes("MaximizedTileLocation", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_MaximizedTileLocation"));
				callbackBuilder.AddCustomAttributes("MaximizedTileLayoutOrder", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_MaximizedTileLayoutOrder"));
				callbackBuilder.AddCustomAttributes("MaximizedTileConstraints", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_MaximizedTileConstraints"));
				callbackBuilder.AddCustomAttributes("MinimizedTileConstraints", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_MinimizedTileConstraints"));
				callbackBuilder.AddCustomAttributes("MinimizedTileExpandButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_MaximizedModeSettings_P_MinimizedTileExpandButtonVisibility"));
				callbackBuilder.AddCustomAttributes("MinimizedExpandedTileConstraints", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_MinimizedExpandedTileConstraints"));
				callbackBuilder.AddCustomAttributes("MinimizedTileExpansionMode", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_MinimizedTileExpansionMode"));
				callbackBuilder.AddCustomAttributes("ShowAllMinimizedTiles", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_MaximizedModeSettings_P_ShowAllMinimizedTiles"));
				callbackBuilder.AddCustomAttributes("ShowTileAreaSplitter", CreateCategory("LC_Behavior"), CreateDescription("LD_MaximizedModeSettings_P_ShowTileAreaSplitter"));
			});

			// Infragistics.Windows.Tiles.NormalModeSettings
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.NormalModeSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowTileSizing", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_NormalModeSettings_P_AllowTileSizing"));
				
				// JJD 5/4/11 - TFS74206 - added ExplicitLayoutTileSizeBehavior
				callbackBuilder.AddCustomAttributes("ExplicitLayoutTileSizeBehavior", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_NormalModeSettings_P_ExplicitLayoutTileSizeBehavior"));
				
				callbackBuilder.AddCustomAttributes("MaxColumns", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_NormalModeSettings_P_MaxColumns"));
				callbackBuilder.AddCustomAttributes("MaxRows", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_NormalModeSettings_P_MaxRows"));
				callbackBuilder.AddCustomAttributes("MinColumns", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_NormalModeSettings_P_MinColumns"));
				callbackBuilder.AddCustomAttributes("MinRows", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_NormalModeSettings_P_MinRows"));
				callbackBuilder.AddCustomAttributes("ShowAllTiles", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_NormalModeSettings_P_ShowAllTiles"));
				callbackBuilder.AddCustomAttributes("TileConstraints", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_NormalModeSettings_P_TileConstraints"));
				callbackBuilder.AddCustomAttributes("TileLayoutOrder", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_NormalModeSettings_P_TileLayoutOrder"));
			});

			// Infragistics.Windows.Tiles.Tile
			// ===============================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.Tile), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695 
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_Tile"));

				callbackBuilder.AddCustomAttributes("AllowMaximize", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_AllowMaximize"));
				callbackBuilder.AddCustomAttributes("Column", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_Column"));
				callbackBuilder.AddCustomAttributes("ColumnSpan", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_ColumnSpan"));
				callbackBuilder.AddCustomAttributes("ColumnWeight", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_ColumnWeight"));
				callbackBuilder.AddCustomAttributes("Constraints", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_Constraints"));
				callbackBuilder.AddCustomAttributes("ConstraintsMaximized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_ConstraintsMaximized"));
				callbackBuilder.AddCustomAttributes("ConstraintsMinimized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_ConstraintsMinimized"));
				callbackBuilder.AddCustomAttributes("ConstraintsMinimizedExpanded", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_ConstraintsMinimizedExpanded"));
				callbackBuilder.AddCustomAttributes("ContentTemplateMaximized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_ContentTemplateMaximized"));
				callbackBuilder.AddCustomAttributes("ContentTemplateMinimized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_ContentTemplateMinimized"));
				callbackBuilder.AddCustomAttributes("ContentTemplateMinimizedExpanded", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_ContentTemplateMinimizedExpanded"));
				callbackBuilder.AddCustomAttributes("CloseAction", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_CloseAction"));
				callbackBuilder.AddCustomAttributes("CloseButtonVisibility", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_CloseButtonVisibility"));
				callbackBuilder.AddCustomAttributes("ExpandButtonVisibility", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_ExpandButtonVisibility"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_HasImage"));
				callbackBuilder.AddCustomAttributes("HasHeader", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_HasHeader"));
				callbackBuilder.AddCustomAttributes("Header", CreateCategory("LC_Content"), CreateDescription("LD_Tile_P_Header"));
				callbackBuilder.AddCustomAttributes("HeaderStringFormat", CreateCategory("LC_Control"), CreateDescription("LD_Tile_P_HeaderStringFormat"));
				callbackBuilder.AddCustomAttributes("HeaderTemplate", CreateCategory("LC_Content"), CreateDescription("LD_Tile_P_HeaderTemplate"));
				callbackBuilder.AddCustomAttributes("HeaderTemplateSelector", CreateCategory("LC_Content"), CreateDescription("LD_Tile_P_HeaderTemplateSelector"));
				callbackBuilder.AddCustomAttributes("Image", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_Image"));
				callbackBuilder.AddCustomAttributes("IsClosed", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_IsClosed"));
				callbackBuilder.AddCustomAttributes("IsExpandedWhenMinimized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_IsExpandedWhenMinimized"));
				callbackBuilder.AddCustomAttributes("MaximizeButtonVisibility", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_MaximizeButtonVisibility"));
				callbackBuilder.AddCustomAttributes("Row", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_Row"));
				callbackBuilder.AddCustomAttributes("RowSpan", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_RowSpan"));
				callbackBuilder.AddCustomAttributes("RowWeight", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_RowWeight"));
				callbackBuilder.AddCustomAttributes("SaveInLayout", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_SaveInLayout"));
				callbackBuilder.AddCustomAttributes("SerializationId", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_SerializationId"));
				callbackBuilder.AddCustomAttributes("State", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_Tile_P_State"));
                
                // JJD 4/08/10 - TFS30618
                callbackBuilder.AddCustomAttributes("Header", new TypeConverterAttribute(typeof(StringConverter)));
            });

			// Infragistics.Windows.Tiles.TileConstraints
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.TileConstraints), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("HorizontalAlignment", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TileConstraints_P_HorizontalAlignment"));
				callbackBuilder.AddCustomAttributes("Margin", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TileConstraints_P_Margin"));
				callbackBuilder.AddCustomAttributes("MaxHeight", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TileConstraints_P_MaxHeight"));
				callbackBuilder.AddCustomAttributes("MaxWidth", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TileConstraints_P_MaxWidth"));
				callbackBuilder.AddCustomAttributes("MinHeight", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TileConstraints_P_MinHeight"));
				callbackBuilder.AddCustomAttributes("MinWidth", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TileConstraints_P_MinWidth"));
				callbackBuilder.AddCustomAttributes("PreferredHeight", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TileConstraints_P_PreferredHeight"));
				callbackBuilder.AddCustomAttributes("PreferredWidth", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TileConstraints_P_PreferredWidth"));
				callbackBuilder.AddCustomAttributes("VerticalAlignment", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TileConstraints_P_VerticalAlignment"));
			});

			// Infragistics.Windows.Tiles.TileHeaderPresenter
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.TileHeaderPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_TileHeaderPresenter"));
			});

			// Infragistics.Windows.Tiles.XamTilesControl
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.XamTilesControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_XamTilesControl"));

				// JJD 4/19/11 - TFS73129  added animation events
				callbackBuilder.AddCustomAttributes("AnimationEnded", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_AnimationEnded"));
				callbackBuilder.AddCustomAttributes("AnimationStarted", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_AnimationStarted"));
				
				callbackBuilder.AddCustomAttributes("HeaderPath", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_HeaderPath"));
				callbackBuilder.AddCustomAttributes("InterTileSpacingX", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_InterTileSpacingX"));
				callbackBuilder.AddCustomAttributes("InterTileSpacingY", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_InterTileSpacingY"));
				callbackBuilder.AddCustomAttributes("ItemHeaderTemplate", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_ItemHeaderTemplate"));
				callbackBuilder.AddCustomAttributes("ItemHeaderTemplateSelector", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_ItemHeaderTemplateSelector"));
				callbackBuilder.AddCustomAttributes("ItemTemplateMaximized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_ItemTemplateMaximized"));
				callbackBuilder.AddCustomAttributes("ItemTemplateMinimized", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_ItemTemplateMinimized"));
				callbackBuilder.AddCustomAttributes("ItemTemplateMinimizedExpanded", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_ItemTemplateMinimizedExpanded"));
				callbackBuilder.AddCustomAttributes("MaximizedModeSettings", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_MaximizedModeSettings"));
				callbackBuilder.AddCustomAttributes("MaximizedTileLimit", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_MaximizedTileLimit"));
				callbackBuilder.AddCustomAttributes("NormalModeSettings", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_NormalModeSettings"));
				callbackBuilder.AddCustomAttributes("SerializationIdPath", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_SerializationIdPath"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_Theme"));
				callbackBuilder.AddCustomAttributes("TileAreaPadding", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_TileAreaPadding"));
				callbackBuilder.AddCustomAttributes("TileCloseAction", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_XamTilesControl_P_TileCloseAction"));
				callbackBuilder.AddCustomAttributes("LoadingItemMapping", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_LoadingItemMapping"));
				callbackBuilder.AddCustomAttributes("SavingItemMapping", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_SavingItemMapping"));
				callbackBuilder.AddCustomAttributes("TileClosed", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_TileClosed"));
				callbackBuilder.AddCustomAttributes("TileClosing", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_TileClosing"));
				callbackBuilder.AddCustomAttributes("TileDragging", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_TileDragging"));
				callbackBuilder.AddCustomAttributes("TileStateChanged", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_TileStateChanged"));
				callbackBuilder.AddCustomAttributes("TileStateChanging", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_TileStateChanging"));
				callbackBuilder.AddCustomAttributes("TileSwapping", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_TileSwapping"));
				callbackBuilder.AddCustomAttributes("TileSwapped", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_TileSwapped"));
				callbackBuilder.AddCustomAttributes("ThemeChanged", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_XamTilesControl_E_ThemeChanged"));
			});

			// Infragistics.Windows.Tiles.TilesPanel
			// =====================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.TilesPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_TilesPanel"));

				callbackBuilder.AddCustomAttributes("InterTileSpacingX", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TilesPanel_P_InterTileSpacingX"));
				callbackBuilder.AddCustomAttributes("InterTileSpacingY", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TilesPanel_P_InterTileSpacingY"));
				callbackBuilder.AddCustomAttributes("MaximizedModeSettings", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TilesPanel_P_MaximizedModeSettings"));
				callbackBuilder.AddCustomAttributes("MaximizedTileLimit", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TilesPanel_P_MaximizedTileLimit"));
				callbackBuilder.AddCustomAttributes("NormalModeSettings", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TilesPanel_P_NormalModeSettings"));
				callbackBuilder.AddCustomAttributes("TileAreaPadding", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TilesPanel_P_TileAreaPadding"));
				callbackBuilder.AddCustomAttributes("TileCloseAction", CreateCategory("LC_TilesControl Properties"), CreateDescription("LD_TilesPanel_P_TileCloseAction"));
				callbackBuilder.AddCustomAttributes("TileClosed", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_TilesPanel_E_TileClosed"));
				callbackBuilder.AddCustomAttributes("TileClosing", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_TilesPanel_E_TileClosing"));
				callbackBuilder.AddCustomAttributes("TileDragging", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_TilesPanel_E_TileDragging"));
				callbackBuilder.AddCustomAttributes("TileStateChanged", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_TilesPanel_E_TileStateChanged"));
				callbackBuilder.AddCustomAttributes("TileStateChanging", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_TilesPanel_E_TileStateChanging"));
				callbackBuilder.AddCustomAttributes("TileSwapping", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_TilesPanel_E_TileSwapping"));
				callbackBuilder.AddCustomAttributes("TileSwapped", CreateCategory("LC_TilesControl Events"), CreateDescription("LD_TilesPanel_E_TileSwapped"));
			});

			#endregion //Description, Category, etc.

			#region NewItemTypesAttribute

            //// New ItemTypes Attributes
            //builder.AddCustomAttributes(typeof(SplitPane), "Panes",
            //    new NewItemTypesAttribute(new Type[] {typeof(SplitPane), 
            //                                        typeof(TabGroupPane), 
            //                                        typeof(ContentPane) }));

            //builder.AddCustomAttributes(typeof(TabGroupPane), "Items",
            //    new NewItemTypesAttribute(new Type[] { typeof(ContentPane) }));

			#endregion //NewItemTypesAttribute

			#region TypeConverterAttribute

            //builder.AddCallback(typeof(ContentPane), delegate(AttributeCallbackBuilder callbackBuilder)
            //{
            //    //callbackBuilder.AddCustomAttributes("TabHeader", new TypeConverterAttribute(typeof(StringConverter)));
            //});

			#endregion //TypeConverterAttribute

			#region BrowsableAttribute

			builder.AddCallback(typeof(XamTilesControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamTilesControl"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamTilesControlAssetLibrary")));


				callbackBuilder.AddCustomAttributes("InterTileAreaSpacingResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("InterTileSpacingXMaximizedResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("InterTileSpacingYMaximizedResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("InterTileSpacingXMinimizedResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("InterTileSpacingYMinimizedResolved", BrowsableAttribute.No);
				
				// JJD 4/19/11 - TFS73129  added IsAnimationInProgress
				callbackBuilder.AddCustomAttributes("IsAnimationInProgress", BrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("IsInMaximizedMode", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("MaximizedItems", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ItemHeaderTemplate", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ItemHeaderTemplateSelector", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ItemTemplateMaximized", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ItemTemplateMinimized", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ItemTemplateMinimizedExpanded", BrowsableAttribute.No);
			});

			builder.AddCallback(typeof(Tile), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                callbackBuilder.AddCustomAttributes("AllowMaximizeResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ContentTemplateMaximized", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ContentTemplateMinimized", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ContentTemplateMinimizedExpanded", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ContentVisibility", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("CloseActionResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("CloseButtonVisibilityResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("ExpandButtonVisibilityResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("HasImage", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("HasHeader", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("HeaderTemplate", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("HeaderTemplateSelector", BrowsableAttribute.No);
				callbackBuilder.AddCustomAttributes("IsClosed", BrowsableAttribute.No);
				callbackBuilder.AddCustomAttributes("IsDragging", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("IsSwapTarget", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("MaximizeButtonVisibilityResolved", BrowsableAttribute.No);
			});

			builder.AddCallback(typeof(TilesPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                callbackBuilder.AddCustomAttributes("InterTileAreaSpacingResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("InterTileSpacingXMaximizedResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("InterTileSpacingYMaximizedResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("InterTileSpacingXMinimizedResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("InterTileSpacingYMinimizedResolved", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("IsInMaximizedMode", BrowsableAttribute.No);
                callbackBuilder.AddCustomAttributes("MaximizedItems", BrowsableAttribute.No);
			});

			builder.AddCallback(typeof(TileHeaderPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                callbackBuilder.AddCustomAttributes("Tile", BrowsableAttribute.No);
			});

			#endregion //BrowsableAttribute

			return builder;
		}

		#region Methods

		#region CreateDescription
		private static DescriptionAttribute CreateDescription(string resourceName)
		{
			return new System.ComponentModel.DescriptionAttribute(SR.GetString(resourceName));
		}
		#endregion //CreateDescription

		#region CreateCategory
		[ThreadStatic]
		private static Dictionary<string, CategoryAttribute> _categories;

		private static CategoryAttribute CreateCategory(string resourceName)
		{
			if (_categories == null)
				_categories = new Dictionary<string, CategoryAttribute>();

			CategoryAttribute category;

			if (!_categories.TryGetValue(resourceName, out category))
			{
				category = new System.ComponentModel.CategoryAttribute(SR.GetString(resourceName));
				_categories.Add(resourceName, category);
			}

			return category;
		}
		#endregion //CreateCategory

		#endregion //Methods
	}

	#endregion //DesignMetadataHelper Static Class
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