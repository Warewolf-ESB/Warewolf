using System.Text;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Design.SmartTagFramework;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design.Services;
using Infragistics.Shared;
using System.Collections.Generic;
using Infragistics.Windows.DataPresenter;

namespace Infragistics.Windows.Design.DataPresenter
{
    /// <summary>
    /// A predefined DesignerActionList class. Here you can specify the smart tag items 
    /// and/or methods which will be executed from the smart tag.
    /// </summary>
    public class DALFieldLayoutSettings : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALFieldLayoutSettings(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_FieldLayoutSettings") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;
			
			int groupSequence = 0;

			#region Allow Group

			if (itemsToShow == null || itemsToShow.Contains("Allow"))
			{
				DesignerActionItemGroup groupAllow = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Allow"), groupSequence++);
				groupAllow.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldLayoutSettings), groupAllow))
				{
					this.Items.Add(pic.GetPropertyActionItem("AllowClipboardOperations"));
					this.Items.Add(pic.GetPropertyActionItem("AllowFieldMoving"));
					this.Items.Add(pic.GetPropertyActionItem("AllowRecordFixing"));
				}
			}

			#endregion //Allow Group	
    
			#region RecordAddDelete Group

			if (itemsToShow == null || itemsToShow.Contains("RecordAddDelete"))
			{
				DesignerActionItemGroup groupRecordAddDelete = new DesignerActionItemGroup(SR.GetString("SmartTag_G_RecordAddDelete"), groupSequence++);
				groupRecordAddDelete.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldLayoutSettings), groupRecordAddDelete))
				{
					this.Items.Add(pic.GetPropertyActionItem("AddNewRecordLocation"));
					this.Items.Add(pic.GetPropertyActionItem("AllowAddNew"));
					this.Items.Add(pic.GetPropertyActionItem("AllowDelete"));
				}
			}

			#endregion //RecordAddDelete Group	
    
			#region Appearance Group

			if (itemsToShow == null || itemsToShow.Contains("Appearance"))
			{
				DesignerActionItemGroup groupAppearance = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Appearance"), groupSequence++);
				groupAppearance.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldLayoutSettings), groupAppearance))
				{
					this.Items.Add(pic.GetPropertyActionItem("AddNewRecordLocation"));
					this.Items.Add(pic.GetPropertyActionItem("AutoArrangeCells"));
					this.Items.Add(pic.GetPropertyActionItem("AutoArrangeMaxColumns"));
					this.Items.Add(pic.GetPropertyActionItem("AutoArrangeMaxRows"));
					this.Items.Add(pic.GetPropertyActionItem("AutoArrangePrimaryFieldReservation"));
					this.Items.Add(pic.GetPropertyActionItem("AutoFitMode"));
					this.Items.Add(pic.GetPropertyActionItem("AutoGenerateFields"));
					this.Items.Add(pic.GetPropertyActionItem("ExpansionIndicatorDisplayMode"));
					this.Items.Add(pic.GetPropertyActionItem("GroupBySummaryDisplayMode"));
					this.Items.Add(pic.GetPropertyActionItem("HeaderPlacement"));
					this.Items.Add(pic.GetPropertyActionItem("HeaderPlacementInGroupBy"));
					this.Items.Add(pic.GetPropertyActionItem("HeaderPrefixAreaDisplayMode"));
					this.Items.Add(pic.GetPropertyActionItem("HighlightAlternateRecords"));
					this.Items.Add(pic.GetPropertyActionItem("HighlightPrimaryField"));
					this.Items.Add(pic.GetPropertyActionItem("LabelLocation"));
					this.Items.Add(pic.GetPropertyActionItem("MaxFieldsToAutoGenerate"));
					this.Items.Add(pic.GetPropertyActionItem("RecordSelectorLocation"));
					this.Items.Add(pic.GetPropertyActionItem("RecordSeparatorLocation"));
				}
			}

			#endregion //Appearance Group

			#region Filtering Group

			if (itemsToShow == null || itemsToShow.Contains("RecordFiltering"))
			{
				DesignerActionItemGroup groupFiltering = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Filtering"), groupSequence++);
				groupFiltering.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldLayoutSettings), groupFiltering))
				{
					this.Items.Add(pic.GetPropertyActionItem("FilterAction"));
					this.Items.Add(pic.GetPropertyActionItem("FilterClearButtonLocation"));
					this.Items.Add(pic.GetPropertyActionItem("FilterRecordLocation"));
					this.Items.Add(pic.GetPropertyActionItem("FilterUIType"));
					this.Items.Add(pic.GetPropertyActionItem("RecordFilterScope"));
					this.Items.Add(pic.GetPropertyActionItem("RecordFiltersLogicalOperator"));
					this.Items.Add(pic.GetPropertyActionItem("ReevaluateFiltersOnDataChange"));
				}
			}

			#endregion //Filtering Group	
    
			#region Summaries Group

			if (itemsToShow == null || itemsToShow.Contains("Summaries"))
			{
				DesignerActionItemGroup groupSummaries = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Summaries"), groupSequence++);
				groupSummaries.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldLayoutSettings), groupSummaries))
				{
					this.Items.Add(pic.GetPropertyActionItem("CalculationScope"));
					this.Items.Add(pic.GetPropertyActionItem("GroupBySummaryDisplayMode"));
					this.Items.Add(pic.GetPropertyActionItem("SummaryDescriptionVisibility"));
				}
			}

			#endregion //Summaries Group	
    
			#region Selection Group

			if (itemsToShow == null || itemsToShow.Contains("Selection"))
			{
				DesignerActionItemGroup groupSelection = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Selection"), groupSequence++);
				groupSelection.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldLayoutSettings), groupSelection))
				{
					this.Items.Add(pic.GetPropertyActionItem("MaxSelectedCells"));
					this.Items.Add(pic.GetPropertyActionItem("MaxSelectedRecords"));
					this.Items.Add(pic.GetPropertyActionItem("RecordSelectorExtent"));
					this.Items.Add(pic.GetPropertyActionItem("RecordSelectorLocation"));
					this.Items.Add(pic.GetPropertyActionItem("SelectionTypeCell"));
					this.Items.Add(pic.GetPropertyActionItem("SelectionTypeField"));
					this.Items.Add(pic.GetPropertyActionItem("SelectionTypeRecord"));
				}
			}

			#endregion //Selection Group	
    
			#region RecordFixing Group

			if (itemsToShow == null || itemsToShow.Contains("RecordFixing"))
			{
				DesignerActionItemGroup groupRecordFixing = new DesignerActionItemGroup(SR.GetString("SmartTag_G_RecordFixing"), groupSequence++);
				groupRecordFixing.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldLayoutSettings), groupRecordFixing))
				{
					this.Items.Add(pic.GetPropertyActionItem("FixedFieldUIType"));
					this.Items.Add(pic.GetPropertyActionItem("FixedRecordLimit"));
					this.Items.Add(pic.GetPropertyActionItem("FixedRecordSortOrder"));
					this.Items.Add(pic.GetPropertyActionItem("FixedRecordUIType"));
				}
			}

			#endregion //RecordFixing Group
		}

        #endregion //Constructors
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