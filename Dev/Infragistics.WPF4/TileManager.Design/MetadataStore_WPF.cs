using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Layouts.XamTileManager.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Layouts.XamTileManager.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Layouts.XamTileManager);
				Assembly controlAssembly = t.Assembly;

				#region XamTileManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.XamTileManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamTileManagerAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamTileManagerAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("XamTileManager_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderPath",
					new DescriptionAttribute(SR.GetString("XamTileManager_HeaderPath_Property")),
				    new DisplayNameAttribute("HeaderPath"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InterTileAreaSpacingResolved",
					new DescriptionAttribute(SR.GetString("XamTileManager_InterTileAreaSpacingResolved_Property")),
				    new DisplayNameAttribute("InterTileAreaSpacingResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingX",
					new DescriptionAttribute(SR.GetString("XamTileManager_InterTileSpacingX_Property")),
				    new DisplayNameAttribute("InterTileSpacingX"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingXMaximizedResolved",
					new DescriptionAttribute(SR.GetString("XamTileManager_InterTileSpacingXMaximizedResolved_Property")),
				    new DisplayNameAttribute("InterTileSpacingXMaximizedResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingXMinimizedResolved",
					new DescriptionAttribute(SR.GetString("XamTileManager_InterTileSpacingXMinimizedResolved_Property")),
				    new DisplayNameAttribute("InterTileSpacingXMinimizedResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingY",
					new DescriptionAttribute(SR.GetString("XamTileManager_InterTileSpacingY_Property")),
				    new DisplayNameAttribute("InterTileSpacingY"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingYMaximizedResolved",
					new DescriptionAttribute(SR.GetString("XamTileManager_InterTileSpacingYMaximizedResolved_Property")),
				    new DisplayNameAttribute("InterTileSpacingYMaximizedResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingYMinimizedResolved",
					new DescriptionAttribute(SR.GetString("XamTileManager_InterTileSpacingYMinimizedResolved_Property")),
				    new DisplayNameAttribute("InterTileSpacingYMinimizedResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsAnimationInProgress",
					new DescriptionAttribute(SR.GetString("XamTileManager_IsAnimationInProgress_Property")),
				    new DisplayNameAttribute("IsAnimationInProgress"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsInMaximizedMode",
					new DescriptionAttribute(SR.GetString("XamTileManager_IsInMaximizedMode_Property")),
				    new DisplayNameAttribute("IsInMaximizedMode"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemHeaderTemplate",
					new DescriptionAttribute(SR.GetString("XamTileManager_ItemHeaderTemplate_Property")),
				    new DisplayNameAttribute("ItemHeaderTemplate"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplateMaximized",
					new DescriptionAttribute(SR.GetString("XamTileManager_ItemTemplateMaximized_Property")),
				    new DisplayNameAttribute("ItemTemplateMaximized"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplateMinimized",
					new DescriptionAttribute(SR.GetString("XamTileManager_ItemTemplateMinimized_Property")),
				    new DisplayNameAttribute("ItemTemplateMinimized"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplateMinimizedExpanded",
					new DescriptionAttribute(SR.GetString("XamTileManager_ItemTemplateMinimizedExpanded_Property")),
				    new DisplayNameAttribute("ItemTemplateMinimizedExpanded"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaximizedModeSettings",
					new DescriptionAttribute(SR.GetString("XamTileManager_MaximizedModeSettings_Property")),
				    new DisplayNameAttribute("MaximizedModeSettings"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaximizedItems",
					new DescriptionAttribute(SR.GetString("XamTileManager_MaximizedItems_Property")),
				    new DisplayNameAttribute("MaximizedItems"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaximizedTileLimit",
					new DescriptionAttribute(SR.GetString("XamTileManager_MaximizedTileLimit_Property")),
				    new DisplayNameAttribute("MaximizedTileLimit"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NormalModeSettings",
					new DescriptionAttribute(SR.GetString("XamTileManager_NormalModeSettings_Property")),
				    new DisplayNameAttribute("NormalModeSettings"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SerializationIdPath",
					new DescriptionAttribute(SR.GetString("XamTileManager_SerializationIdPath_Property")),
				    new DisplayNameAttribute("SerializationIdPath"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TileAreaPadding",
					new DescriptionAttribute(SR.GetString("XamTileManager_TileAreaPadding_Property")),
				    new DisplayNameAttribute("TileAreaPadding"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TileCloseAction",
					new DescriptionAttribute(SR.GetString("XamTileManager_TileCloseAction_Property")),
				    new DisplayNameAttribute("TileCloseAction"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Items",
					new DescriptionAttribute(SR.GetString("XamTileManager_Items_Property")),
				    new DisplayNameAttribute("Items"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemContainerStyle",
					new DescriptionAttribute(SR.GetString("XamTileManager_ItemContainerStyle_Property")),
				    new DisplayNameAttribute("ItemContainerStyle"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplate",
					new DescriptionAttribute(SR.GetString("XamTileManager_ItemTemplate_Property")),
				    new DisplayNameAttribute("ItemTemplate"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTouchSupportEnabled",
					new DescriptionAttribute(SR.GetString("XamTileManager_IsTouchSupportEnabled_Property")),
				    new DisplayNameAttribute("IsTouchSupportEnabled"),
					new CategoryAttribute(SR.GetString("XamTileManager_Properties"))
				);

				#endregion // XamTileManager Properties

				#region MaximizedModeSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.MaximizedModeSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InterTileAreaSpacing",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_InterTileAreaSpacing_Property")),
				    new DisplayNameAttribute("InterTileAreaSpacing")				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingXMaximized",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_InterTileSpacingXMaximized_Property")),
				    new DisplayNameAttribute("InterTileSpacingXMaximized")				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingXMinimized",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_InterTileSpacingXMinimized_Property")),
				    new DisplayNameAttribute("InterTileSpacingXMinimized")				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingYMaximized",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_InterTileSpacingYMaximized_Property")),
				    new DisplayNameAttribute("InterTileSpacingYMaximized")				);


				tableBuilder.AddCustomAttributes(t, "InterTileSpacingYMinimized",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_InterTileSpacingYMinimized_Property")),
				    new DisplayNameAttribute("InterTileSpacingYMinimized")				);


				tableBuilder.AddCustomAttributes(t, "MaximizedTileLocation",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_MaximizedTileLocation_Property")),
				    new DisplayNameAttribute("MaximizedTileLocation")				);


				tableBuilder.AddCustomAttributes(t, "MaximizedTileLayoutOrder",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_MaximizedTileLayoutOrder_Property")),
				    new DisplayNameAttribute("MaximizedTileLayoutOrder")				);


				tableBuilder.AddCustomAttributes(t, "MaximizedTileConstraints",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_MaximizedTileConstraints_Property")),
				    new DisplayNameAttribute("MaximizedTileConstraints")				);


				tableBuilder.AddCustomAttributes(t, "MinimizedTileConstraints",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_MinimizedTileConstraints_Property")),
				    new DisplayNameAttribute("MinimizedTileConstraints")				);


				tableBuilder.AddCustomAttributes(t, "MinimizedTileExpandButtonVisibility",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_MinimizedTileExpandButtonVisibility_Property")),
				    new DisplayNameAttribute("MinimizedTileExpandButtonVisibility")				);


				tableBuilder.AddCustomAttributes(t, "MinimizedExpandedTileConstraints",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_MinimizedExpandedTileConstraints_Property")),
				    new DisplayNameAttribute("MinimizedExpandedTileConstraints")				);


				tableBuilder.AddCustomAttributes(t, "MinimizedTileExpansionMode",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_MinimizedTileExpansionMode_Property")),
				    new DisplayNameAttribute("MinimizedTileExpansionMode")				);


				tableBuilder.AddCustomAttributes(t, "ShowAllMinimizedTiles",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_ShowAllMinimizedTiles_Property")),
				    new DisplayNameAttribute("ShowAllMinimizedTiles")				);


				tableBuilder.AddCustomAttributes(t, "ShowTileAreaSplitter",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_ShowTileAreaSplitter_Property")),
				    new DisplayNameAttribute("ShowTileAreaSplitter")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalTileAreaAlignment",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_HorizontalTileAreaAlignment_Property")),
				    new DisplayNameAttribute("HorizontalTileAreaAlignment")				);


				tableBuilder.AddCustomAttributes(t, "VerticalTileAreaAlignment",
					new DescriptionAttribute(SR.GetString("MaximizedModeSettings_VerticalTileAreaAlignment_Property")),
				    new DisplayNameAttribute("VerticalTileAreaAlignment")				);

				#endregion // MaximizedModeSettings Properties

				#region ModeSettingsBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.ModeSettingsBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowTileDragging",
					new DescriptionAttribute(SR.GetString("ModeSettingsBase_AllowTileDragging_Property")),
				    new DisplayNameAttribute("AllowTileDragging")				);


				tableBuilder.AddCustomAttributes(t, "RepositionAnimationProperty",
					new DescriptionAttribute(SR.GetString("ModeSettingsBase_RepositionAnimationProperty_Property")),
				    new DisplayNameAttribute("RepositionAnimationProperty")				);


				tableBuilder.AddCustomAttributes(t, "RepositionAnimation",
					new DescriptionAttribute(SR.GetString("ModeSettingsBase_RepositionAnimation_Property")),
				    new DisplayNameAttribute("RepositionAnimation")				);


				tableBuilder.AddCustomAttributes(t, "ResizeAnimationProperty",
					new DescriptionAttribute(SR.GetString("ModeSettingsBase_ResizeAnimationProperty_Property")),
				    new DisplayNameAttribute("ResizeAnimationProperty")				);


				tableBuilder.AddCustomAttributes(t, "ResizeAnimation",
					new DescriptionAttribute(SR.GetString("ModeSettingsBase_ResizeAnimation_Property")),
				    new DisplayNameAttribute("ResizeAnimation")				);


				tableBuilder.AddCustomAttributes(t, "ShouldAnimate",
					new DescriptionAttribute(SR.GetString("ModeSettingsBase_ShouldAnimate_Property")),
				    new DisplayNameAttribute("ShouldAnimate")				);

				#endregion // ModeSettingsBase Properties

				#region NormalModeSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.NormalModeSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowTileSizing",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_AllowTileSizing_Property")),
				    new DisplayNameAttribute("AllowTileSizing")				);


				tableBuilder.AddCustomAttributes(t, "ExplicitLayoutTileSizeBehavior",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_ExplicitLayoutTileSizeBehavior_Property")),
				    new DisplayNameAttribute("ExplicitLayoutTileSizeBehavior")				);


				tableBuilder.AddCustomAttributes(t, "MaxColumns",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_MaxColumns_Property")),
				    new DisplayNameAttribute("MaxColumns")				);


				tableBuilder.AddCustomAttributes(t, "MaxRows",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_MaxRows_Property")),
				    new DisplayNameAttribute("MaxRows")				);


				tableBuilder.AddCustomAttributes(t, "MinColumns",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_MinColumns_Property")),
				    new DisplayNameAttribute("MinColumns")				);


				tableBuilder.AddCustomAttributes(t, "MinRows",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_MinRows_Property")),
				    new DisplayNameAttribute("MinRows")				);


				tableBuilder.AddCustomAttributes(t, "ShowAllTiles",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_ShowAllTiles_Property")),
				    new DisplayNameAttribute("ShowAllTiles")				);


				tableBuilder.AddCustomAttributes(t, "TileConstraints",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_TileConstraints_Property")),
				    new DisplayNameAttribute("TileConstraints")				);


				tableBuilder.AddCustomAttributes(t, "TileLayoutOrder",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_TileLayoutOrder_Property")),
				    new DisplayNameAttribute("TileLayoutOrder")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalTileAreaAlignment",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_HorizontalTileAreaAlignment_Property")),
				    new DisplayNameAttribute("HorizontalTileAreaAlignment")				);


				tableBuilder.AddCustomAttributes(t, "VerticalTileAreaAlignment",
					new DescriptionAttribute(SR.GetString("NormalModeSettings_VerticalTileAreaAlignment_Property")),
				    new DisplayNameAttribute("VerticalTileAreaAlignment")				);

				#endregion // NormalModeSettings Properties

				#region TileConstraints Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.TileConstraints");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalAlignment",
					new DescriptionAttribute(SR.GetString("TileConstraints_HorizontalAlignment_Property")),
				    new DisplayNameAttribute("HorizontalAlignment")				);


				tableBuilder.AddCustomAttributes(t, "Margin",
					new DescriptionAttribute(SR.GetString("TileConstraints_Margin_Property")),
				    new DisplayNameAttribute("Margin")				);


				tableBuilder.AddCustomAttributes(t, "MaxHeight",
					new DescriptionAttribute(SR.GetString("TileConstraints_MaxHeight_Property")),
				    new DisplayNameAttribute("MaxHeight")				);


				tableBuilder.AddCustomAttributes(t, "MaxWidth",
					new DescriptionAttribute(SR.GetString("TileConstraints_MaxWidth_Property")),
				    new DisplayNameAttribute("MaxWidth")				);


				tableBuilder.AddCustomAttributes(t, "MinHeight",
					new DescriptionAttribute(SR.GetString("TileConstraints_MinHeight_Property")),
				    new DisplayNameAttribute("MinHeight")				);


				tableBuilder.AddCustomAttributes(t, "MinWidth",
					new DescriptionAttribute(SR.GetString("TileConstraints_MinWidth_Property")),
				    new DisplayNameAttribute("MinWidth")				);


				tableBuilder.AddCustomAttributes(t, "PreferredHeight",
					new DescriptionAttribute(SR.GetString("TileConstraints_PreferredHeight_Property")),
				    new DisplayNameAttribute("PreferredHeight")				);


				tableBuilder.AddCustomAttributes(t, "PreferredWidth",
					new DescriptionAttribute(SR.GetString("TileConstraints_PreferredWidth_Property")),
				    new DisplayNameAttribute("PreferredWidth")				);


				tableBuilder.AddCustomAttributes(t, "VerticalAlignment",
					new DescriptionAttribute(SR.GetString("TileConstraints_VerticalAlignment_Property")),
				    new DisplayNameAttribute("VerticalAlignment")				);

				#endregion // TileConstraints Properties

				#region LoadingItemMappingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.LoadingItemMappingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("LoadingItemMappingEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item")				);


				tableBuilder.AddCustomAttributes(t, "SerializationId",
					new DescriptionAttribute(SR.GetString("LoadingItemMappingEventArgs_SerializationId_Property")),
				    new DisplayNameAttribute("SerializationId")				);

				#endregion // LoadingItemMappingEventArgs Properties

				#region SavingItemMappingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.SavingItemMappingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("SavingItemMappingEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item")				);


				tableBuilder.AddCustomAttributes(t, "SerializationId",
					new DescriptionAttribute(SR.GetString("SavingItemMappingEventArgs_SerializationId_Property")),
				    new DisplayNameAttribute("SerializationId")				);

				#endregion // SavingItemMappingEventArgs Properties

				#region CancelableTileEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.CancelableTileEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cancel",
					new DescriptionAttribute(SR.GetString("CancelableTileEventArgs_Cancel_Property")),
				    new DisplayNameAttribute("Cancel")				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("CancelableTileEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item")				);


				tableBuilder.AddCustomAttributes(t, "Tile",
					new DescriptionAttribute(SR.GetString("CancelableTileEventArgs_Tile_Property")),
				    new DisplayNameAttribute("Tile")				);

				#endregion // CancelableTileEventArgs Properties

				#region TileEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.TileEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Tile",
					new DescriptionAttribute(SR.GetString("TileEventArgs_Tile_Property")),
				    new DisplayNameAttribute("Tile")				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("TileEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item")				);

				#endregion // TileEventArgs Properties

				#region TileClosingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.TileClosingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TileClosingEventArgs Properties

				#region TileClosedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.TileClosedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TileClosedEventArgs Properties

				#region TileDraggingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.TileDraggingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TileDraggingEventArgs Properties

				#region TileStateChangingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.TileStateChangingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NewState",
					new DescriptionAttribute(SR.GetString("TileStateChangingEventArgs_NewState_Property")),
				    new DisplayNameAttribute("NewState")				);

				#endregion // TileStateChangingEventArgs Properties

				#region TileStateChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.TileStateChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NewState",
					new DescriptionAttribute(SR.GetString("TileStateChangedEventArgs_NewState_Property")),
				    new DisplayNameAttribute("NewState")				);


				tableBuilder.AddCustomAttributes(t, "OldState",
					new DescriptionAttribute(SR.GetString("TileStateChangedEventArgs_OldState_Property")),
				    new DisplayNameAttribute("OldState")				);

				#endregion // TileStateChangedEventArgs Properties

				#region TileSwappingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.TileSwappingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TargetItem",
					new DescriptionAttribute(SR.GetString("TileSwappingEventArgs_TargetItem_Property")),
				    new DisplayNameAttribute("TargetItem")				);


				tableBuilder.AddCustomAttributes(t, "TargetTile",
					new DescriptionAttribute(SR.GetString("TileSwappingEventArgs_TargetTile_Property")),
				    new DisplayNameAttribute("TargetTile")				);


				tableBuilder.AddCustomAttributes(t, "SwapIsExpandedWhenMinimized",
					new DescriptionAttribute(SR.GetString("TileSwappingEventArgs_SwapIsExpandedWhenMinimized_Property")),
				    new DisplayNameAttribute("SwapIsExpandedWhenMinimized")				);

				#endregion // TileSwappingEventArgs Properties

				#region TileSwappedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.TileSwappedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TargetItem",
					new DescriptionAttribute(SR.GetString("TileSwappedEventArgs_TargetItem_Property")),
				    new DisplayNameAttribute("TargetItem")				);


				tableBuilder.AddCustomAttributes(t, "TargetTile",
					new DescriptionAttribute(SR.GetString("TileSwappedEventArgs_TargetTile_Property")),
				    new DisplayNameAttribute("TargetTile")				);

				#endregion // TileSwappedEventArgs Properties

				#region XamTile Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.XamTile");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "State",
					new DescriptionAttribute(SR.GetString("XamTile_State_Property")),
				    new DisplayNameAttribute("State"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowMaximize",
					new DescriptionAttribute(SR.GetString("XamTile_AllowMaximize_Property")),
				    new DisplayNameAttribute("AllowMaximize"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowMaximizeResolved",
					new DescriptionAttribute(SR.GetString("XamTile_AllowMaximizeResolved_Property")),
				    new DisplayNameAttribute("AllowMaximizeResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentTemplateMaximized",
					new DescriptionAttribute(SR.GetString("XamTile_ContentTemplateMaximized_Property")),
				    new DisplayNameAttribute("ContentTemplateMaximized"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentTemplateMinimized",
					new DescriptionAttribute(SR.GetString("XamTile_ContentTemplateMinimized_Property")),
				    new DisplayNameAttribute("ContentTemplateMinimized"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentTemplateMinimizedExpanded",
					new DescriptionAttribute(SR.GetString("XamTile_ContentTemplateMinimizedExpanded_Property")),
				    new DisplayNameAttribute("ContentTemplateMinimizedExpanded"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentTemplateResolved",
					new DescriptionAttribute(SR.GetString("XamTile_ContentTemplateResolved_Property")),
				    new DisplayNameAttribute("ContentTemplateResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentVisibility",
					new DescriptionAttribute(SR.GetString("XamTile_ContentVisibility_Property")),
				    new DisplayNameAttribute("ContentVisibility"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CloseAction",
					new DescriptionAttribute(SR.GetString("XamTile_CloseAction_Property")),
				    new DisplayNameAttribute("CloseAction"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CloseActionResolved",
					new DescriptionAttribute(SR.GetString("XamTile_CloseActionResolved_Property")),
				    new DisplayNameAttribute("CloseActionResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CloseButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamTile_CloseButtonVisibility_Property")),
				    new DisplayNameAttribute("CloseButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CloseButtonVisibilityResolved",
					new DescriptionAttribute(SR.GetString("XamTile_CloseButtonVisibilityResolved_Property")),
				    new DisplayNameAttribute("CloseButtonVisibilityResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExpandButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamTile_ExpandButtonVisibility_Property")),
				    new DisplayNameAttribute("ExpandButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExpandButtonVisibilityResolved",
					new DescriptionAttribute(SR.GetString("XamTile_ExpandButtonVisibilityResolved_Property")),
				    new DisplayNameAttribute("ExpandButtonVisibilityResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasImage",
					new DescriptionAttribute(SR.GetString("XamTile_HasImage_Property")),
				    new DisplayNameAttribute("HasImage"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasHeader",
					new DescriptionAttribute(SR.GetString("XamTile_HasHeader_Property")),
				    new DisplayNameAttribute("HasHeader"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Header",
					new DescriptionAttribute(SR.GetString("XamTile_Header_Property")),
				    new DisplayNameAttribute("Header"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderTemplate",
					new DescriptionAttribute(SR.GetString("XamTile_HeaderTemplate_Property")),
				    new DisplayNameAttribute("HeaderTemplate"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Image",
					new DescriptionAttribute(SR.GetString("XamTile_Image_Property")),
				    new DisplayNameAttribute("Image"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsExpandedWhenMinimized",
					new DescriptionAttribute(SR.GetString("XamTile_IsExpandedWhenMinimized_Property")),
				    new DisplayNameAttribute("IsExpandedWhenMinimized"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaximizeButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamTile_MaximizeButtonVisibility_Property")),
				    new DisplayNameAttribute("MaximizeButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaximizeButtonVisibilityResolved",
					new DescriptionAttribute(SR.GetString("XamTile_MaximizeButtonVisibilityResolved_Property")),
				    new DisplayNameAttribute("MaximizeButtonVisibilityResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SaveInLayout",
					new DescriptionAttribute(SR.GetString("XamTile_SaveInLayout_Property")),
				    new DisplayNameAttribute("SaveInLayout"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsMaximized",
					new DescriptionAttribute(SR.GetString("XamTile_IsMaximized_Property")),
				    new DisplayNameAttribute("IsMaximized"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsClosed",
					new DescriptionAttribute(SR.GetString("XamTile_IsClosed_Property")),
				    new DisplayNameAttribute("IsClosed"),
					new CategoryAttribute(SR.GetString("XamTile_Properties"))
				);

				#endregion // XamTile Properties

				#region TileAreaSplitter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.Primitives.TileAreaSplitter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsDragging",
					new DescriptionAttribute(SR.GetString("TileAreaSplitter_IsDragging_Property")),
				    new DisplayNameAttribute("IsDragging")				);


				tableBuilder.AddCustomAttributes(t, "PreviewStyle",
					new DescriptionAttribute(SR.GetString("TileAreaSplitter_PreviewStyle_Property")),
				    new DisplayNameAttribute("PreviewStyle")				);


				tableBuilder.AddCustomAttributes(t, "ShowsPreview",
					new DescriptionAttribute(SR.GetString("TileAreaSplitter_ShowsPreview_Property")),
				    new DisplayNameAttribute("ShowsPreview")				);

				#endregion // TileAreaSplitter Properties

				#region ItemTileInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.ItemTileInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_Index_Property")),
				    new DisplayNameAttribute("Index")				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_Item_Property")),
				    new DisplayNameAttribute("Item")				);


				tableBuilder.AddCustomAttributes(t, "LogicalIndex",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_LogicalIndex_Property")),
				    new DisplayNameAttribute("LogicalIndex")				);


				tableBuilder.AddCustomAttributes(t, "OccupiesScrollPosition",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_OccupiesScrollPosition_Property")),
				    new DisplayNameAttribute("OccupiesScrollPosition")				);


				tableBuilder.AddCustomAttributes(t, "ScrollPosition",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_ScrollPosition_Property")),
				    new DisplayNameAttribute("ScrollPosition")				);


				tableBuilder.AddCustomAttributes(t, "IsClosed",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_IsClosed_Property")),
				    new DisplayNameAttribute("IsClosed")				);


				tableBuilder.AddCustomAttributes(t, "IsExpandedWhenMinimized",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_IsExpandedWhenMinimized_Property")),
				    new DisplayNameAttribute("IsExpandedWhenMinimized")				);


				tableBuilder.AddCustomAttributes(t, "IsExpandedWhenMinimizedResolved",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_IsExpandedWhenMinimizedResolved_Property")),
				    new DisplayNameAttribute("IsExpandedWhenMinimizedResolved"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "IsMaximized",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_IsMaximized_Property")),
				    new DisplayNameAttribute("IsMaximized")				);


				tableBuilder.AddCustomAttributes(t, "SizeOverride",
					new DescriptionAttribute(SR.GetString("ItemTileInfo_SizeOverride_Property")),
				    new DisplayNameAttribute("SizeOverride")				);

				#endregion // ItemTileInfo Properties

				#region XamTileManagerAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamTileManagerAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamTileManagerAutomationPeer Properties

				#region TileCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.Primitives.TileCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TileCommand Properties

				#region TileCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.Primitives.TileCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("TileCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // TileCommandSource Properties

				#region TileManagerResourceString Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.Primitives.TileManagerResourceString");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TileManagerResourceString Properties

				#region TileAreaSplitterAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.TileAreaSplitterAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TileAreaSplitterAutomationPeer Properties

				#region TileHeaderPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.Primitives.TileHeaderPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Tile",
					new DescriptionAttribute(SR.GetString("TileHeaderPresenter_Tile_Property")),
				    new DisplayNameAttribute("Tile")				);

				#endregion // TileHeaderPresenter Properties

				#region TileAreaPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.Primitives.TileAreaPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TileAreaPanel Properties

				#region XamTileAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamTileAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamTileAutomationPeer Properties

				#region ObservableItemCollection Properties
				t = controlAssembly.GetType("Infragistics.Collections.ObservableItemCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsFixedSize",
					new DescriptionAttribute(SR.GetString("ObservableItemCollection_IsFixedSize_Property")),
				    new DisplayNameAttribute("IsFixedSize")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("ObservableItemCollection_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("ObservableItemCollection_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "IsSynchronized",
					new DescriptionAttribute(SR.GetString("ObservableItemCollection_IsSynchronized_Property")),
				    new DisplayNameAttribute("IsSynchronized")				);


				tableBuilder.AddCustomAttributes(t, "SyncRoot",
					new DescriptionAttribute(SR.GetString("ObservableItemCollection_SyncRoot_Property")),
				    new DisplayNameAttribute("SyncRoot")				);

				#endregion // ObservableItemCollection Properties

				#region TileDragIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.Primitives.TileDragIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Container",
					new DescriptionAttribute(SR.GetString("TileDragIndicator_Container_Property")),
				    new DisplayNameAttribute("Container")				);

				#endregion // TileDragIndicator Properties

				#region XamTileManagerPersistenceInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.Primitives.XamTileManagerPersistenceInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsExplicitLayout",
					new DescriptionAttribute(SR.GetString("XamTileManagerPersistenceInfo_IsExplicitLayout_Property")),
				    new DisplayNameAttribute("IsExplicitLayout"),
					new CategoryAttribute(SR.GetString("XamTileManagerPersistenceInfo_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Items",
					new DescriptionAttribute(SR.GetString("XamTileManagerPersistenceInfo_Items_Property")),
				    new DisplayNameAttribute("Items"),
					new CategoryAttribute(SR.GetString("XamTileManagerPersistenceInfo_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinimizedAreaExtentX",
					new DescriptionAttribute(SR.GetString("XamTileManagerPersistenceInfo_MinimizedAreaExtentX_Property")),
				    new DisplayNameAttribute("MinimizedAreaExtentX"),
					new CategoryAttribute(SR.GetString("XamTileManagerPersistenceInfo_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinimizedAreaExtentY",
					new DescriptionAttribute(SR.GetString("XamTileManagerPersistenceInfo_MinimizedAreaExtentY_Property")),
				    new DisplayNameAttribute("MinimizedAreaExtentY"),
					new CategoryAttribute(SR.GetString("XamTileManagerPersistenceInfo_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SynchoronizedTileWidth",
					new DescriptionAttribute(SR.GetString("XamTileManagerPersistenceInfo_SynchoronizedTileWidth_Property")),
				    new DisplayNameAttribute("SynchoronizedTileWidth"),
					new CategoryAttribute(SR.GetString("XamTileManagerPersistenceInfo_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SynchoronizedTileHeight",
					new DescriptionAttribute(SR.GetString("XamTileManagerPersistenceInfo_SynchoronizedTileHeight_Property")),
				    new DisplayNameAttribute("SynchoronizedTileHeight"),
					new CategoryAttribute(SR.GetString("XamTileManagerPersistenceInfo_Properties"))
				);

				#endregion // XamTileManagerPersistenceInfo Properties

				#region TileItemPersistenceInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.Primitives.TileItemPersistenceInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_Column_Property")),
				    new DisplayNameAttribute("Column")				);


				tableBuilder.AddCustomAttributes(t, "ColumnSpan",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_ColumnSpan_Property")),
				    new DisplayNameAttribute("ColumnSpan")				);


				tableBuilder.AddCustomAttributes(t, "ColumnWeight",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_ColumnWeight_Property")),
				    new DisplayNameAttribute("ColumnWeight")				);


				tableBuilder.AddCustomAttributes(t, "IsClosed",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_IsClosed_Property")),
				    new DisplayNameAttribute("IsClosed")				);


				tableBuilder.AddCustomAttributes(t, "IsExpandedWhenMinimized",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_IsExpandedWhenMinimized_Property")),
				    new DisplayNameAttribute("IsExpandedWhenMinimized")				);


				tableBuilder.AddCustomAttributes(t, "IsMaximized",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_IsMaximized_Property")),
				    new DisplayNameAttribute("IsMaximized")				);


				tableBuilder.AddCustomAttributes(t, "LogicalIndex",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_LogicalIndex_Property")),
				    new DisplayNameAttribute("LogicalIndex")				);


				tableBuilder.AddCustomAttributes(t, "MaximizedIndex",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_MaximizedIndex_Property")),
				    new DisplayNameAttribute("MaximizedIndex")				);


				tableBuilder.AddCustomAttributes(t, "PreferredHeightOverride",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_PreferredHeightOverride_Property")),
				    new DisplayNameAttribute("PreferredHeightOverride")				);


				tableBuilder.AddCustomAttributes(t, "PreferredWidthOverride",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_PreferredWidthOverride_Property")),
				    new DisplayNameAttribute("PreferredWidthOverride")				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_Row_Property")),
				    new DisplayNameAttribute("Row")				);


				tableBuilder.AddCustomAttributes(t, "RowSpan",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_RowSpan_Property")),
				    new DisplayNameAttribute("RowSpan")				);


				tableBuilder.AddCustomAttributes(t, "RowWeight",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_RowWeight_Property")),
				    new DisplayNameAttribute("RowWeight")				);


				tableBuilder.AddCustomAttributes(t, "SerializationId",
					new DescriptionAttribute(SR.GetString("TileItemPersistenceInfo_SerializationId_Property")),
				    new DisplayNameAttribute("SerializationId")				);

				#endregion // TileItemPersistenceInfo Properties

				#region TileItemAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.TileItemAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemInfo",
					new DescriptionAttribute(SR.GetString("TileItemAutomationPeer_ItemInfo_Property")),
				    new DisplayNameAttribute("ItemInfo")				);

				#endregion // TileItemAutomationPeer Properties
                this.AddCustomAttributes(tableBuilder);
				return tableBuilder.CreateTable();
			}
		}
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