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
using System.Collections.ObjectModel;
using Infragistics.Windows.DockManager;
using Infragistics.Collections;
using System;

namespace Infragistics.Windows.Design.DockManager
{
    /// <summary>
    /// A predefined DesignerActionList class. Here you can specify the smart tag items 
    /// and/or methods which will be executed from the smart tag.
    /// </summary>
    public class DALContentPane : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALContentPane(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_ContentPane") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;

			int groupSequence = 0;

			#region Tasks Group

			//DesignerActionItemGroup groupTasks	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Tasks"), groupSequence++);
			//groupTasks.IsExpanded				= true;
			//this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddSplitPaneWithContentPanes", SR.GetString("SmartTag_D_AddSplitPaneWithContentPanes"), SR.GetString("SmartTag_N_AddSplitPaneWithContentPanes"), groupTasks, groupSequence++));
			//this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddSplitPaneWithTabGroupPanes", SR.GetString("SmartTag_D_AddSplitPaneWithTabGroupPanes"), SR.GetString("SmartTag_N_AddSplitPaneWithTabGroupPanes"), groupTasks, groupSequence++));
			//this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddDocumentContentHost", SR.GetString("SmartTag_D_AddDocumentContentHost"), SR.GetString("SmartTag_N_AddDocumentContentHost"), groupTasks, groupSequence++));

			#endregion //Tasks Group

			#region Descriptions Group

			DesignerActionItemGroup groupDescriptions = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Descriptions"), groupSequence++);
			groupDescriptions.IsExpanded = true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(ContentPane), groupDescriptions))
			{
				this.Items.Add(pic.GetPropertyActionItem("Header"));
				this.Items.Add(pic.GetPropertyActionItem("NavigatorDescription"));
				this.Items.Add(pic.GetPropertyActionItem("NavigatorTitle"));
				this.Items.Add(pic.GetPropertyActionItem("TabHeader"));
			}

			#endregion //Strings Group

			#region Appearance Group

			DesignerActionItemGroup groupAppearance = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Appearance"), groupSequence++);
			groupAppearance.IsExpanded				= true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(ContentPane), groupAppearance))
			{
				this.Items.Add(pic.GetPropertyActionItem("CloseButtonVisibility"));
				this.Items.Add(pic.GetPropertyActionItem("Image"));
				this.Items.Add(pic.GetPropertyActionItem("PinButtonVisibility"));
				this.Items.Add(pic.GetPropertyActionItem("WindowPositionMenuVisibility"));
			}

			#endregion //Appearance Group

			#region Options Group

			DesignerActionItemGroup groupOptions	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Options"), groupSequence++);
			groupOptions.IsExpanded					= false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(ContentPane), groupOptions))
			{
				this.Items.Add(pic.GetPropertyActionItem("AllowClose"));
				this.Items.Add(pic.GetPropertyActionItem("AllowDocking"));
				this.Items.Add(pic.GetPropertyActionItem("AllowDockingBottom"));
				this.Items.Add(pic.GetPropertyActionItem("AllowDockingFloating"));
				this.Items.Add(pic.GetPropertyActionItem("AllowDockingInTabGroup"));
				this.Items.Add(pic.GetPropertyActionItem("AllowDockingLeft"));
				this.Items.Add(pic.GetPropertyActionItem("AllowDockingRight"));
				this.Items.Add(pic.GetPropertyActionItem("AllowDockingTop"));
				this.Items.Add(pic.GetPropertyActionItem("AllowFloatingOnly"));
				this.Items.Add(pic.GetPropertyActionItem("AllowInDocumentHost"));
				this.Items.Add(pic.GetPropertyActionItem("AllowPinning"));
			}

			#endregion //Options Group

			#region Behavior Group

			DesignerActionItemGroup groupBehavior	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Behavior"), groupSequence++);
			groupBehavior.IsExpanded				= false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(ContentPane), groupBehavior))
			{
				this.Items.Add(pic.GetPropertyActionItem("CloseAction"));
				this.Items.Add(pic.GetPropertyActionItem("IsPinned"));
			}

			#endregion //Behavior Group

			#region Serialization Group

			DesignerActionItemGroup groupSerialization	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Serialization"), groupSequence++);
			groupSerialization.IsExpanded				= false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(ContentPane), groupSerialization))
			{
				this.Items.Add(pic.GetPropertyActionItem("SaveInLayout"));
				this.Items.Add(pic.GetPropertyActionItem("SerializationId"));
			}

			#endregion //Serialization Group
		}

        #endregion //Constructors

		#region DesignerActionMethodItem Callbacks

		#endregion //DesignerActionMethodItem Callbacks
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