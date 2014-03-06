using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Menus.XamTagCloud.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Menus.XamTagCloud.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Menus.XamTagCloud);
				Assembly controlAssembly = t.Assembly;

				#region ScaleBreak Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.ScaleBreak");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "StartWeight",
					new DescriptionAttribute(SR.GetString("ScaleBreak_StartWeight_Property")),
				    new DisplayNameAttribute("StartWeight")				);


				tableBuilder.AddCustomAttributes(t, "EndWeight",
					new DescriptionAttribute(SR.GetString("ScaleBreak_EndWeight_Property")),
				    new DisplayNameAttribute("EndWeight")				);


				tableBuilder.AddCustomAttributes(t, "Weight",
					new DescriptionAttribute(SR.GetString("ScaleBreak_Weight_Property")),
				    new DisplayNameAttribute("Weight")				);

				#endregion // ScaleBreak Properties

				#region XamTagCloud Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamTagCloud");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamTagCloudAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamTagCloudAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxScale",
					new DescriptionAttribute(SR.GetString("XamTagCloud_MaxScale_Property")),
				    new DisplayNameAttribute("MaxScale"),
					new CategoryAttribute(SR.GetString("XamTagCloud_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinScale",
					new DescriptionAttribute(SR.GetString("XamTagCloud_MinScale_Property")),
				    new DisplayNameAttribute("MinScale"),
					new CategoryAttribute(SR.GetString("XamTagCloud_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemSpacing",
					new DescriptionAttribute(SR.GetString("XamTagCloud_ItemSpacing_Property")),
				    new DisplayNameAttribute("ItemSpacing"),
					new CategoryAttribute(SR.GetString("XamTagCloud_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsContainerStyle",
					new DescriptionAttribute(SR.GetString("XamTagCloud_ItemsContainerStyle_Property")),
				    new DisplayNameAttribute("ItemsContainerStyle"),
					new CategoryAttribute(SR.GetString("XamTagCloud_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WeightMemberPath",
					new DescriptionAttribute(SR.GetString("XamTagCloud_WeightMemberPath_Property")),
				    new DisplayNameAttribute("WeightMemberPath"),
					new CategoryAttribute(SR.GetString("XamTagCloud_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TargetName",
					new DescriptionAttribute(SR.GetString("XamTagCloud_TargetName_Property")),
				    new DisplayNameAttribute("TargetName"),
					new CategoryAttribute(SR.GetString("XamTagCloud_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UseSmoothScaling",
					new DescriptionAttribute(SR.GetString("XamTagCloud_UseSmoothScaling_Property")),
				    new DisplayNameAttribute("UseSmoothScaling"),
					new CategoryAttribute(SR.GetString("XamTagCloud_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NavigateUriMemberPath",
					new DescriptionAttribute(SR.GetString("XamTagCloud_NavigateUriMemberPath_Property")),
				    new DisplayNameAttribute("NavigateUriMemberPath"),
					new CategoryAttribute(SR.GetString("XamTagCloud_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScaleBreaks",
					new DescriptionAttribute(SR.GetString("XamTagCloud_ScaleBreaks_Property")),
				    new DisplayNameAttribute("ScaleBreaks"),
					new CategoryAttribute(SR.GetString("XamTagCloud_Properties"))
				);

				#endregion // XamTagCloud Properties

				#region XamTagCloudItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamTagCloudItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamTagCloudSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamTagCloudSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "Owner",
					new DescriptionAttribute(SR.GetString("XamTagCloudItem_Owner_Property")),
				    new DisplayNameAttribute("Owner"),
					new CategoryAttribute(SR.GetString("XamTagCloudItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Weight",
					new DescriptionAttribute(SR.GetString("XamTagCloudItem_Weight_Property")),
				    new DisplayNameAttribute("Weight"),
					new CategoryAttribute(SR.GetString("XamTagCloudItem_Properties"))
				);

				#endregion // XamTagCloudItem Properties

				#region XamTagCloudItemEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamTagCloudItemEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "XamTagCloudItem",
					new DescriptionAttribute(SR.GetString("XamTagCloudItemEventArgs_XamTagCloudItem_Property")),
				    new DisplayNameAttribute("XamTagCloudItem"),
					new CategoryAttribute(SR.GetString("XamTagCloudItemEventArgs_Properties"))
				);

				#endregion // XamTagCloudItemEventArgs Properties

				#region XamTagCloudClippedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamTagCloudClippedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CloudClipped",
					new DescriptionAttribute(SR.GetString("XamTagCloudClippedEventArgs_CloudClipped_Property")),
				    new DisplayNameAttribute("CloudClipped"),
					new CategoryAttribute(SR.GetString("XamTagCloudClippedEventArgs_Properties"))
				);

				#endregion // XamTagCloudClippedEventArgs Properties

				#region XamTagCloudPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamTagCloudPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "UseSmoothScaling",
					new DescriptionAttribute(SR.GetString("XamTagCloudPanel_UseSmoothScaling_Property")),
				    new DisplayNameAttribute("UseSmoothScaling"),
					new CategoryAttribute(SR.GetString("XamTagCloudPanel_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxScale",
					new DescriptionAttribute(SR.GetString("XamTagCloudPanel_MaxScale_Property")),
				    new DisplayNameAttribute("MaxScale"),
					new CategoryAttribute(SR.GetString("XamTagCloudPanel_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinScale",
					new DescriptionAttribute(SR.GetString("XamTagCloudPanel_MinScale_Property")),
				    new DisplayNameAttribute("MinScale"),
					new CategoryAttribute(SR.GetString("XamTagCloudPanel_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemSpacing",
					new DescriptionAttribute(SR.GetString("XamTagCloudPanel_ItemSpacing_Property")),
				    new DisplayNameAttribute("ItemSpacing"),
					new CategoryAttribute(SR.GetString("XamTagCloudPanel_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScaleBreaks",
					new DescriptionAttribute(SR.GetString("XamTagCloudPanel_ScaleBreaks_Property")),
				    new DisplayNameAttribute("ScaleBreaks"),
					new CategoryAttribute(SR.GetString("XamTagCloudPanel_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalContentAlignment",
					new DescriptionAttribute(SR.GetString("XamTagCloudPanel_HorizontalContentAlignment_Property")),
				    new DisplayNameAttribute("HorizontalContentAlignment"),
					new CategoryAttribute(SR.GetString("XamTagCloudPanel_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalContentAlignment",
					new DescriptionAttribute(SR.GetString("XamTagCloudPanel_VerticalContentAlignment_Property")),
				    new DisplayNameAttribute("VerticalContentAlignment"),
					new CategoryAttribute(SR.GetString("XamTagCloudPanel_Properties"))
				);

				#endregion // XamTagCloudPanel Properties

				#region XamTagCloudAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamTagCloudAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamTagCloudAutomationPeer Properties

				#region XamTagCloudItemAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamTagCloudItemAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamTagCloudItemAutomationPeer Properties

				#region HyperlinkButton Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.HyperlinkButton");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NavigateUri",
					new DescriptionAttribute(SR.GetString("HyperlinkButton_NavigateUri_Property")),
				    new DisplayNameAttribute("NavigateUri")				);


				tableBuilder.AddCustomAttributes(t, "TargetName",
					new DescriptionAttribute(SR.GetString("HyperlinkButton_TargetName_Property")),
				    new DisplayNameAttribute("TargetName")				);

				#endregion // HyperlinkButton Properties

				#region HyperlinkButtonAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.HyperlinkButtonAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HyperlinkButtonAutomationPeer Properties
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