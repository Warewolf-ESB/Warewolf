using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using System.ComponentModel;
using Infragistics.Windows.Tiles;
using Microsoft.Windows.Design.PropertyEditing;

namespace Infragistics.Windows.Design.OutlookBar
{
	// JM 01-06-10 VS2010 Designer Support
	#region DesignMetadataHelper Static Class

	internal static class DesignMetadataHelper
	{
		internal static AttributeTableBuilder GetAttributeTableBuilder()
		{
			AttributeTableBuilder builder = new AttributeTableBuilder();

			#region Description/Category

			// Infragistics.Windows.OutlookBar.XamOutlookBar
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.OutlookBar.XamOutlookBar), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowMinimized", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_AllowMinimized"));
				callbackBuilder.AddCustomAttributes("ContextMenuGroups", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_ContextMenuGroups"));
				callbackBuilder.AddCustomAttributes("ExpandStoryboard", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_ExpandStoryboard"));
				callbackBuilder.AddCustomAttributes("Groups", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_Groups"));
				callbackBuilder.AddCustomAttributes("GroupsSource", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_GroupsSource"));
				callbackBuilder.AddCustomAttributes("IsMinimized", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_IsMinimized"));
				callbackBuilder.AddCustomAttributes("IsVerticalSplitterVisible", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_IsVerticalSplitterVisible"));
				callbackBuilder.AddCustomAttributes("MinimizedStateThreshold", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_MinimizedStateThreshold"));
				callbackBuilder.AddCustomAttributes("MinimizedWidth", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_MinimizedWidth"));
				callbackBuilder.AddCustomAttributes("MinimizeStoryboard", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_MinimizeStoryboard"));
				callbackBuilder.AddCustomAttributes("NavigationAreaGroups", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_NavigationAreaGroups"));
				callbackBuilder.AddCustomAttributes("NavigationAreaMaxGroups", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_NavigationAreaMaxGroups"));
				callbackBuilder.AddCustomAttributes("NavigationPaneOptionsControlStyle", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_NavigationPaneOptionsControlStyle"));
				callbackBuilder.AddCustomAttributes("OverflowAreaGroups", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_OverflowAreaGroups"));
				callbackBuilder.AddCustomAttributes("ReserveSpaceForLargeImage", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_ReserveSpaceForLargeImage"));
				callbackBuilder.AddCustomAttributes("SelectedAreaMinHeight", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_SelectedAreaMinHeight"));
				callbackBuilder.AddCustomAttributes("SelectedGroup", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_SelectedGroup"));
				callbackBuilder.AddCustomAttributes("ShowGroupHeaderAsToolTip", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_ShowGroupHeaderAsToolTip"));
				callbackBuilder.AddCustomAttributes("ShowToolTips", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_ShowToolTips"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_Theme"));
				callbackBuilder.AddCustomAttributes("VerticalSplitterLocation", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_VerticalSplitterLocation"));
				callbackBuilder.AddCustomAttributes("VerticalSplitterResizeMode", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_VerticalSplitterResizeMode"));
				callbackBuilder.AddCustomAttributes("VerticalSplitterWidth", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_P_VerticalSplitterWidth"));
				callbackBuilder.AddCustomAttributes("ExecutingCommand", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_ExecutingCommand"));
				callbackBuilder.AddCustomAttributes("ExecutedCommand", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_ExecutedCommand"));
				callbackBuilder.AddCustomAttributes("GroupsReset", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_GroupsReset"));
				callbackBuilder.AddCustomAttributes("NavigationPaneExpanding", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_NavigationPaneExpanding"));
				callbackBuilder.AddCustomAttributes("NavigationPaneExpanded", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_NavigationPaneExpanded"));
				callbackBuilder.AddCustomAttributes("NavigationPaneMinimizing", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_NavigationPaneMinimizing"));
				callbackBuilder.AddCustomAttributes("NavigationPaneMinimized", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_NavigationPaneMinimized"));
				callbackBuilder.AddCustomAttributes("SelectedGroupChanging", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_SelectedGroupChanging"));
				callbackBuilder.AddCustomAttributes("SelectedGroupChanged", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_SelectedGroupChanged"));
				callbackBuilder.AddCustomAttributes("SelectedGroupPopupClosed", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_SelectedGroupPopupClosed"));
				callbackBuilder.AddCustomAttributes("SelectedGroupPopupOpened", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_SelectedGroupPopupOpened"));
				callbackBuilder.AddCustomAttributes("SelectedGroupPopupOpening", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_SelectedGroupPopupOpening"));
				callbackBuilder.AddCustomAttributes("ThemeChanged", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_XamOutlookBar_E_ThemeChanged"));
			});

			// Infragistics.Windows.OutlookBar.OutlookBarGroup
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.OutlookBar.OutlookBarGroup), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("HasLargeImage", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_OutlookBarGroup_P_HasLargeImage"));
				callbackBuilder.AddCustomAttributes("IsSelected", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_OutlookBarGroup_P_IsSelected"));
				callbackBuilder.AddCustomAttributes("IsMouseOverGroup", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_OutlookBarGroup_P_IsMouseOverGroup"));
				callbackBuilder.AddCustomAttributes("Key", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_OutlookBarGroup_P_Key"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_OutlookBarGroup_P_Location"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_OutlookBarGroup_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("OutlookBar", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_OutlookBarGroup_P_OutlookBar"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_OutlookBarGroup_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("SmallImageResolved", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_OutlookBarGroup_P_SmallImageResolved"));
			});

			// Infragistics.Windows.OutlookBar.GroupsPresenter
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.OutlookBar.GroupsPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_OutlookBar Properties"), CreateDescription("LD_GroupsPresenter_P_Orientation"));
			});

			#endregion //Description/Category

			#region ToolboxBrowsableAttribute

			builder.AddCustomAttributes(typeof(Infragistics.Windows.OutlookBar.GroupAreaSplitter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.OutlookBar.GroupOverflowArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.OutlookBar.GroupsPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.OutlookBar.NavigationPaneOptionsControl), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.OutlookBar.OutlookBarGroup), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.OutlookBar.SelectedGroupContent), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.OutlookBar.SelectedGroupHeader), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.OutlookBar.VerticalSplitterPreview), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.OutlookBar.Internal.GroupsPanel), ToolboxBrowsableAttribute.No);

			#endregion //ToolboxBrowsableAttribute

			// Infragistics.Windows.Controls.XamOutlookBar
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.OutlookBar.XamOutlookBar), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamOutlookBar"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamOutlookBarAssetLibrary")));

			});

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