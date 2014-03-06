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
    public class DALFieldSettings : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALFieldSettings(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_FieldSettings") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;

			int groupSequence = 0;

			#region Allow Group

			if (itemsToShow == null || itemsToShow.Contains("Allow"))
			{
				DesignerActionItemGroup groupAllow = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Allow"), groupSequence++);
				groupAllow.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldSettings), groupAllow))
				{
					this.Items.Add(pic.GetPropertyActionItem("AllowCellVirtualization"));
					this.Items.Add(pic.GetPropertyActionItem("AllowEdit"));
					this.Items.Add(pic.GetPropertyActionItem("AllowFixing"));
					this.Items.Add(pic.GetPropertyActionItem("AllowGroupBy"));
					this.Items.Add(pic.GetPropertyActionItem("AllowHiding"));
					this.Items.Add(pic.GetPropertyActionItem("AllowLabelVirtualization"));
					this.Items.Add(pic.GetPropertyActionItem("AllowRecordFiltering"));
					this.Items.Add(pic.GetPropertyActionItem("AllowResize"));
					this.Items.Add(pic.GetPropertyActionItem("AllowSummaries"));
				}
			}

			#endregion //Allow Group	
    
			#region Cells Group

			if (itemsToShow == null || itemsToShow.Contains("Cells"))
			{
				DesignerActionItemGroup groupCells = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Cells"), groupSequence++);
				groupCells.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldSettings), groupCells))
				{
					this.Items.Add(pic.GetPropertyActionItem("CellClickAction"));
					this.Items.Add(pic.GetPropertyActionItem("CellContentAlignment"));
					this.Items.Add(pic.GetPropertyActionItem("CellHeight"));
					this.Items.Add(pic.GetPropertyActionItem("CellMaxHeight"));
					this.Items.Add(pic.GetPropertyActionItem("CellMaxWidth"));
					this.Items.Add(pic.GetPropertyActionItem("CellMinHeight"));
					this.Items.Add(pic.GetPropertyActionItem("CellMinWidth"));
					this.Items.Add(pic.GetPropertyActionItem("CellWidth"));
				}
			}

			#endregion //Cells Group	
    
			#region Labels Group

			if (itemsToShow == null || itemsToShow.Contains("Labels"))
			{
				DesignerActionItemGroup groupLabels = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Labels"), groupSequence++);
				groupLabels.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldSettings), groupLabels))
				{
					this.Items.Add(pic.GetPropertyActionItem("LabelClickAction"));
					this.Items.Add(pic.GetPropertyActionItem("LabelHeight"));
					this.Items.Add(pic.GetPropertyActionItem("LabelMaxHeight"));
					this.Items.Add(pic.GetPropertyActionItem("LabelMaxWidth"));
					this.Items.Add(pic.GetPropertyActionItem("LabelMinHeight"));
					this.Items.Add(pic.GetPropertyActionItem("LabelMinWidth"));
					this.Items.Add(pic.GetPropertyActionItem("LabelTextAlignment"));
					this.Items.Add(pic.GetPropertyActionItem("LabelTextTrimming"));
					this.Items.Add(pic.GetPropertyActionItem("LabelTextWrapping"));
					this.Items.Add(pic.GetPropertyActionItem("LabelWidth"));
				}
			}

			#endregion //Labels Group	

			#region Filtering Group

			if (itemsToShow == null || itemsToShow.Contains("RecordFiltering"))
			{
				DesignerActionItemGroup groupFiltering = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Filtering"), groupSequence++);
				groupFiltering.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldSettings), groupFiltering))
				{
					this.Items.Add(pic.GetPropertyActionItem("AllowRecordFiltering"));
					this.Items.Add(pic.GetPropertyActionItem("FilterClearButtonVisibility"));
					this.Items.Add(pic.GetPropertyActionItem("FilterEvaluationTrigger"));
					this.Items.Add(pic.GetPropertyActionItem("FilterOperandUIType"));
					this.Items.Add(pic.GetPropertyActionItem("FilterOperatorDefaultValue"));
					this.Items.Add(pic.GetPropertyActionItem("FilterStringComparisonType"));

					// JM 10-13-11 TFS91820
					this.Items.Add(pic.GetPropertyActionItem("FilterLabelIconDropDownType"));
				}
			}

			#endregion //Filtering Group

			#region Summaries Group

			if (itemsToShow == null || itemsToShow.Contains("Summaries"))
			{
				DesignerActionItemGroup groupSummaries = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Summaries"), groupSequence++);
				groupSummaries.IsExpanded = (groupSequence == 1);
				using (PropertyItemCreator pic = new PropertyItemCreator(typeof(FieldSettings), groupSummaries))
				{
					this.Items.Add(pic.GetPropertyActionItem("AllowSummaries"));
					this.Items.Add(pic.GetPropertyActionItem("SummaryDisplayArea"));
					this.Items.Add(pic.GetPropertyActionItem("SummaryUIType"));
				}
			}

			#endregion //Filtering Group
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