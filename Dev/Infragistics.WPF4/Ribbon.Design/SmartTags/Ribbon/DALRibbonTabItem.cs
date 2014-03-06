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
using Infragistics.Windows.Ribbon;
using System.Collections.ObjectModel;

namespace Infragistics.Windows.Design.Ribbon
{
    /// <summary>
    /// A predefined DesignerActionList class. Here you can specify the smart tag items 
    /// and/or methods which will be executed from the smart tag.
    /// </summary>
    public class DALRibbonTabItem : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALRibbonTabItem(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_RibbonTabItem") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;

			int groupSequence = 0;

			#region Tasks Group

			DesignerActionItemGroup groupTasks	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Tasks"), groupSequence++);
			groupTasks.IsExpanded				= true;
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddGroup", SR.GetString("SmartTag_D_AddGroup"), SR.GetString("SmartTag_N_AddGroup"), groupTasks, groupSequence++));

			#endregion //Tasks Group

			#region Properties Group

			DesignerActionItemGroup groupProperties = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Properties"), groupSequence++);
			groupProperties.IsExpanded				= true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(RibbonTabItem), groupProperties))
			{
				this.Items.Add(pic.GetPropertyActionItem("Header"));
				this.Items.Add(pic.GetPropertyActionItem("KeyTip"));
			}

			#endregion //Appearance Group
		}

        #endregion //Constructors

		#region DesignerActionMethodItem Callbacks

			#region PerformAction_AddGroup

		private static void PerformAction_AddGroup(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			ModelItem groupsModelItem = adornedControlModel.Properties["RibbonGroups"].Value;
			if (groupsModelItem == null || groupsModelItem.ItemType != typeof(RibbonGroupCollection))
			{
				// Create a ModelItem & object instance for the property.
				groupsModelItem = ModelFactory.CreateItem(context, typeof(RibbonGroupCollection), null);

				// Set the value of the property to the newly create ModelItem
				adornedControlModel.Properties["RibbonGroups"].SetValue(groupsModelItem);
			}

			// Create a new RibbonGroup and set it's Caption property to a default value 
			ModelItem		newGroupItem		= ModelFactory.CreateItem(context, typeof(RibbonGroup), null);
			newGroupItem.Properties["Caption"].SetValue(SR.GetString("SmartTag_Default_RibbonGroupCaption"));

			// Add the new RibbonGroup to the RibbonGroups collection
			ModelProperty	groupsModelProperty	= adornedControlModel.Properties["RibbonGroups"];
			groupsModelProperty.Collection.Add(newGroupItem);
		}

			#endregion //PerformAction_AddGroup

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