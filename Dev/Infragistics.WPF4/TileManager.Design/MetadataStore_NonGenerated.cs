




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;
using Infragistics.Controls.Layouts;
using System.ComponentModel;
using Infragistics.Collections;
using Infragistics;
using System.Windows.Markup;
using System.Windows;
using Infragistics.Windows.Helpers;




namespace InfragisticsWPF4.Controls.Layouts.XamTileManager.Design

{

	internal partial class MetadataStore : IProvideAttributeTable
	{
		#region Private Members

		private int _loadsedAssemblyCount;

		private static string[] _inheritedUIPropsToHide =
		{
			"AllowDrop",
			"Background",
			"BindingGroup",
			"BorderBrush",
			"BorderThickness",
			"Clip",
			"ClipToBounds",
			"ContextMenu",
			"Cursor",
			"Effect",
			"FlowDirection",
			"Focusable",
			"FocusVisualStyle",
			"FontFamily",
			"FontSize",
			"FontStretch",
			"FontStyle",
			"FontWeight",
			"ForceCursor",
			"Foreground",
			"Height",
			"HorizontalAlignment",
			"HorizontalContentAlignment",
			"IsEnabled",
			"IsHitTestVisible",
			"IsManipulationEnabled",
			"IsTabStop",
			"LayoutTransform",
			"Margin",
			"MaxHeight",
			"MaxWidth",
			"MinHeight",
			"MinWidth",
			"Opacity",
			"OpacityMask",
			"OverridesDefaultStyle",
			"Padding",
			"Projection",
			"RenderTransform",
			"RenderTransformOrigin",
			"SnapsToDevicePixels",
			"TabIndex",
			"TabNavigation",
			"Template",
			"ToolTip",
			"Uid",
			"UseLayoutRounding",
			"VerticalAlignment",
			"VerticalContentAlignment",
			"Width",
			"Visibility",
			"ZIndex"
		};

		#endregion //Private Members

		#region AddCustomAttributes

		private void AddCustomAttributes(AttributeTableBuilder builder)
		{

			builder.AddCallback(typeof(Infragistics.Controls.Layouts.XamTileManager), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(new ComplexBindingPropertiesAttribute("ItemSource"), new DefaultPropertyAttribute("Items"));
				callbackBuilder.AddCustomAttributes("GetColumn", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")));
				callbackBuilder.AddCustomAttributes("GetColumnSpan", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")));
				callbackBuilder.AddCustomAttributes("GetColumnWeight", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")));
				callbackBuilder.AddCustomAttributes("GetConstraints", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")), new RefreshPropertiesAttribute(RefreshProperties.Repaint));
				callbackBuilder.AddCustomAttributes("GetConstraintsMaximized", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")), new RefreshPropertiesAttribute(RefreshProperties.Repaint));
				callbackBuilder.AddCustomAttributes("GetConstraintsMinimized", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")), new RefreshPropertiesAttribute(RefreshProperties.Repaint));
				callbackBuilder.AddCustomAttributes("GetConstraintsMinimizedExpanded", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")), new RefreshPropertiesAttribute(RefreshProperties.Repaint));
				callbackBuilder.AddCustomAttributes("MaximizedModeSettings", new RefreshPropertiesAttribute(RefreshProperties.Repaint));
				callbackBuilder.AddCustomAttributes("NormalModeSettings", new RefreshPropertiesAttribute(RefreshProperties.Repaint));
				callbackBuilder.AddCustomAttributes("GetRow", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")));
				callbackBuilder.AddCustomAttributes("GetRowSpan", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")));
				callbackBuilder.AddCustomAttributes("GetRowWeight", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")));
				callbackBuilder.AddCustomAttributes("GetSerializationId", new AttachedPropertyBrowsableForChildrenAttribute(), new CategoryAttribute(SR.GetString("XamTile_Properties")));
			});

			builder.AddCallback(typeof(Infragistics.Controls.Layouts.NormalModeSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(new TypeConverterAttribute(typeof(ExpandableObjectConverter)));
				callbackBuilder.AddCustomAttributes("TileConstraints", new RefreshPropertiesAttribute(RefreshProperties.Repaint));
			});

			builder.AddCallback(typeof(Infragistics.Controls.Layouts.MaximizedModeSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(new TypeConverterAttribute(typeof(ExpandableObjectConverter)));
				callbackBuilder.AddCustomAttributes("MaximizedTileConstraints", new RefreshPropertiesAttribute(RefreshProperties.Repaint));
				callbackBuilder.AddCustomAttributes("MinimizedTileConstraints", new RefreshPropertiesAttribute(RefreshProperties.Repaint));
				callbackBuilder.AddCustomAttributes("MinimizedExpandedTileConstraints", new RefreshPropertiesAttribute(RefreshProperties.Repaint));
			});

			// JJD 9/27/11 - TFS86621/TFS88451
			// Add nullable type converter attributes to supply values in property grid for both SL and WPF
			builder.AddCallback(typeof(Infragistics.Controls.Layouts.XamTile), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowMaximize", new TypeConverterAttribute(typeof(NullableBoolConverter)));
				callbackBuilder.AddCustomAttributes("IsExpandedWhenMinimized", new TypeConverterAttribute(typeof(NullableBoolConverter)));

				callbackBuilder.AddCustomAttributes("CloseButtonVisibility", new TypeConverterAttribute(typeof(NullableConverter<System.Windows.Visibility>)));
				callbackBuilder.AddCustomAttributes("ExpandButtonVisibility", new TypeConverterAttribute(typeof(NullableConverter<System.Windows.Visibility>)));
				callbackBuilder.AddCustomAttributes("MaximizeButtonVisibility", new TypeConverterAttribute(typeof(NullableConverter<System.Windows.Visibility>)));





			});

			// JJD 9/27/11 - TFS86621/TFS88451
			// Add nullable type converter attributes to supply values in property grid for both SL and WPF
			builder.AddCallback(typeof(Infragistics.Controls.Layouts.TileConstraints), delegate(AttributeCallbackBuilder callbackBuilder)
			{

				callbackBuilder.AddCustomAttributes("HorizontalAlignment", new TypeConverterAttribute(typeof(NullableConverter<System.Windows.HorizontalAlignment>)));
				callbackBuilder.AddCustomAttributes("VerticalAlignment", new TypeConverterAttribute(typeof(NullableConverter<System.Windows.VerticalAlignment>)));




			});

		}

		#endregion //AddCustomAttributes

		#region HideInheritedUIProps

		private void HideInheritedUIProps(AttributeTableBuilder builder, Type type)
		{
			foreach (string propName in _inheritedUIPropsToHide)
				builder.AddCustomAttributes(type, propName, new BrowsableAttribute(false));
		}

		#endregion //HideInheritedUIProps

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