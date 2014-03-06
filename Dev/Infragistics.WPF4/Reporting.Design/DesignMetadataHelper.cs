using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using System.ComponentModel;
using Microsoft.Windows.Design.PropertyEditing;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.Design.Reporting
{

	// JM 01-06-10 VS2010 Designer Support
	#region DesignMetadataHelper Static Class

	internal static class DesignMetadataHelper
	{
		internal static AttributeTableBuilder GetAttributeTableBuilder()
		{
			AttributeTableBuilder builder = new AttributeTableBuilder();

			#region Description/Category

			// Infragistics.Windows.Reporting.EmbeddedVisualReportSection
			// ==========================================================
			builder.AddCallback(typeof(Infragistics.Windows.Reporting.EmbeddedVisualReportSection), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("SourceVisual", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_EmbeddedVisualReportSection_P_SourceVisual"));
				callbackBuilder.AddCustomAttributes("VisualPaginator", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_EmbeddedVisualReportSection_P_VisualPaginator"));
				callbackBuilder.AddCustomAttributes("IsEndReached", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_EmbeddedVisualReportSection_P_IsEndReached"));
				callbackBuilder.AddCustomAttributes("PaginationEnded", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_EmbeddedVisualReportSection_E_PaginationEnded"));
				callbackBuilder.AddCustomAttributes("PaginationStarted", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_EmbeddedVisualReportSection_E_PaginationStarted"));
				callbackBuilder.AddCustomAttributes("PaginationStarting", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_EmbeddedVisualReportSection_E_PaginationStarting"));
			});

			// Infragistics.Windows.Reporting.Report
			// =====================================
			builder.AddCallback(typeof(Infragistics.Windows.Reporting.Report), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("PhysicalPageNumber", CreateCategory("LC_Data"), CreateDescription("LD_Report_P_PhysicalPageNumber"));
				callbackBuilder.AddCustomAttributes("LogicalPageNumber", CreateCategory("LC_Data"), CreateDescription("LD_Report_P_LogicalPageNumber"));
				callbackBuilder.AddCustomAttributes("LogicalPagePartNumber", CreateCategory("LC_Data"), CreateDescription("LD_Report_P_LogicalPagePartNumber"));
				callbackBuilder.AddCustomAttributes("SectionPhysicalPageNumber", CreateCategory("LC_Data"), CreateDescription("LD_Report_P_SectionPhysicalPageNumber"));
				callbackBuilder.AddCustomAttributes("SectionLogicalPageNumber", CreateCategory("LC_Data"), CreateDescription("LD_Report_P_SectionLogicalPageNumber"));
				callbackBuilder.AddCustomAttributes("SectionLogicalPagePartNumber", CreateCategory("LC_Data"), CreateDescription("LD_Report_P_SectionLogicalPagePartNumber"));
				callbackBuilder.AddCustomAttributes("Sections", CreateCategory("LC_Data"), CreateDescription("LD_Report_P_Sections"));
				callbackBuilder.AddCustomAttributes("PrintProgress", CreateCategory("LC_Behavior"), CreateDescription("LD_Report_E_PrintProgress"));
				callbackBuilder.AddCustomAttributes("PrintStart", CreateCategory("LC_Behavior"), CreateDescription("LD_Report_E_PrintStart"));
				callbackBuilder.AddCustomAttributes("PrintEnded", CreateCategory("LC_Behavior"), CreateDescription("LD_Report_E_PrintEnded"));
			});

			// Infragistics.Windows.Reporting.ReportPagePresenter
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.Reporting.ReportPagePresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("Footer", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_Footer"));
				callbackBuilder.AddCustomAttributes("FooterStringFormat", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_FooterStringFormat"));
				callbackBuilder.AddCustomAttributes("FooterTemplate", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_FooterTemplate"));
				callbackBuilder.AddCustomAttributes("FooterTemplateSelector", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_FooterTemplateSelector"));
				callbackBuilder.AddCustomAttributes("Section", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_Section"));
				callbackBuilder.AddCustomAttributes("LogicalPageNumber", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_LogicalPageNumber"));
				callbackBuilder.AddCustomAttributes("LogicalPagePartNumber", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_LogicalPagePartNumber"));
				callbackBuilder.AddCustomAttributes("PhysicalPageNumber", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_PhysicalPageNumber"));
				callbackBuilder.AddCustomAttributes("SectionLogicalPageNumber", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_SectionLogicalPageNumber"));
				callbackBuilder.AddCustomAttributes("SectionLogicalPagePartNumber", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_SectionLogicalPagePartNumber"));
				callbackBuilder.AddCustomAttributes("SectionPhysicalPageNumber", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportPagePresenter_P_SectionPhysicalPageNumber"));
			});

			// Infragistics.Windows.Reporting.ReportProgressControl
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.Reporting.ReportProgressControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("Report", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_ReportProgressControl_P_Report"));
			});

			// Infragistics.Windows.Reporting.XamReportPreview
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.Reporting.XamReportPreview), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940 Add ToolboxCategoryAttribute and ToolboxBrowsableAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes);

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamReportPreviewAssetLibrary")));


				callbackBuilder.AddCustomAttributes("DocumentViewer", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_XamReportPreview_P_DocumentViewer"));
				callbackBuilder.AddCustomAttributes("IsContentVisible", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_XamReportPreview_P_IsContentVisible"));
				callbackBuilder.AddCustomAttributes("Report", CreateCategory("LC_Reporting Properties"), CreateDescription("LD_XamReportPreview_P_Report"));
			});

			#endregion //Description/Category

			#region ToolboxBrowsableAttribute

			builder.AddCustomAttributes(typeof(Infragistics.Windows.Reporting.ReportPagePresenter), ToolboxBrowsableAttribute.No);

			// JM 10-14-08 TFS 9019 - Allow the ReportProgressControl to be placed in the toolbox.
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Reporting.ReportProgressControl), ToolboxBrowsableAttribute.No);

			#endregion //ToolboxBrowsableAttribute

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