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
using System;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.Design.DataPresenter
{
    /// <summary>
    /// A predefined DesignerActionList class. Here you can specify the smart tag items 
    /// and/or methods which will be executed from the smart tag.
    /// </summary>
    public class DALXamDataPresenter : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALXamDataPresenter(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_XamDataPresenter") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;

			int groupSequence = 0;

			#region View Group

			DesignerActionItemGroup groupView = new DesignerActionItemGroup(SR.GetString("SmartTag_G_View"), groupSequence++);
			groupView.IsExpanded = true;
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_SetGridView", SR.GetString("SmartTag_D_GridView"), SR.GetString("SmartTag_N_GridView"), groupView, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_SetCardView", SR.GetString("SmartTag_D_CardView"), SR.GetString("SmartTag_N_CardView"), groupView, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_SetCarouselView", SR.GetString("SmartTag_D_CarouselView"), SR.GetString("SmartTag_N_CarouselView"), groupView, groupSequence++));

			this.Items.Add(new DesignerActionObjectPropertyItemView("View", "View", typeof(DALGridView), modelItem, typeof(XamDataPresenter), typeof(Infragistics.Windows.DataPresenter.GridView), "View", string.Empty, groupView, null, groupSequence++));

			#endregion //View Group	

			#region Basic Items

			DALHelpers.AddBasicItems(typeof(XamDataPresenter), this, this.Items, context, modelItem, groupSequence);

			#endregion //BasicItems
		}

        #endregion //Constructors

        #region DesignerActionMethodItem Callbacks

			#region PerformAction_SetGridView

		private static void PerformAction_SetGridView(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			ModelItem viewModelItem = adornedControlModel.Properties["View"].Value;
			if (viewModelItem == null || viewModelItem.ItemType != typeof(Infragistics.Windows.DataPresenter.GridView))
			{
				// Create a ModelItem & object instance for the View property.
				viewModelItem = ModelFactory.CreateItem(context, typeof(Infragistics.Windows.DataPresenter.GridView), null);

				// Set the value of the property to the newly create ModelItem
				adornedControlModel.Properties["View"].SetValue(viewModelItem);
			}

		}

			#endregion //PerformAction_SetGridView

			#region PerformAction_SetCardView

		private static void PerformAction_SetCardView(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			ModelItem viewModelItem = adornedControlModel.Properties["View"].Value;
			if (viewModelItem == null || viewModelItem.ItemType != typeof(Infragistics.Windows.DataPresenter.CardView))
			{
				// Create a ModelItem & object instance for the View property.
				viewModelItem = ModelFactory.CreateItem(context, typeof(Infragistics.Windows.DataPresenter.CardView), null);

				// Set the value of the property to the newly create ModelItem
				adornedControlModel.Properties["View"].SetValue(viewModelItem);
			}

		}

			#endregion //PerformAction_SetCardView

			#region PerformAction_SetCarouselView

		private static void PerformAction_SetCarouselView(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			ModelItem viewModelItem = adornedControlModel.Properties["View"].Value;
			if (viewModelItem == null || viewModelItem.ItemType != typeof(Infragistics.Windows.DataPresenter.CarouselView))
			{
				// Create a ModelItem & object instance for the View property.
				viewModelItem = ModelFactory.CreateItem(context, typeof(Infragistics.Windows.DataPresenter.CarouselView), null);

				// Set the value of the property to the newly create ModelItem
				adornedControlModel.Properties["View"].SetValue(viewModelItem);
			}

		}

			#endregion //PerformAction_SetCarouselView

		#endregion //DesignerActionMethodItem Callbacks

		#region DesignerActionObjectPropertyItemView Nested Class

		/// <summary>
		/// A derived DesignerActionObjectPropertyItem class that dynamically supplies the ActionList type and property
		/// Type for the View property
		/// </summary>
		public class DesignerActionObjectPropertyItemView : DesignerActionObjectPropertyItem
		{
			internal DesignerActionObjectPropertyItemView(string propertyName, string displayName, Type actionListType, ModelItem parentModelItem, Type owningType, Type propertyType, string id, string description, DesignerActionItemGroup designerActionItemGroup, List<string> itemsToShow, int orderNumber)
				: base(propertyName, displayName, actionListType, parentModelItem, owningType, propertyType, id, designerActionItemGroup, itemsToShow, orderNumber)
			{
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			protected override System.Type GetActionListTypeOverride()
			{
				if (this.ParentModelItem.Properties["View"].ComputedValue is CardView)
					return typeof(DALCardView);
				else
				if (this.ParentModelItem.Properties["View"].ComputedValue is CarouselView)
					return typeof(DALCarouselView);
				else
					return typeof(DALGridView);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			protected override System.Type GetPropertyTypeOverride()
			{
				if (this.ParentModelItem.Properties["View"].ComputedValue is CardView)
					return typeof(CardView);
				else
				if (this.ParentModelItem.Properties["View"].ComputedValue is CarouselView)
					return typeof(CarouselView);
				else
					return typeof(Infragistics.Windows.DataPresenter.GridView);
			}
		}

		#endregion //#region DesignerActionObjectPropertyItemView Nested Class
	}

	#region DALGridView, DALGridViewSettings Classes

	/// <summary>
	/// A predefined DesignerActionList class. Here you can specify the smart tag items 
	/// and/or methods which will be executed from the smart tag.
	/// </summary>
	public class DALGridView : DesignerActionList
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALGridView(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
		{
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_GridView") : alternateAdornerTitle;

			DesignerActionItemGroup groupViewSettings = new DesignerActionItemGroup(SR.GetString("SmartTag_G_ViewSettings"), 0);
			groupViewSettings.IsExpanded = true;
			this.Items.Add(new DesignerActionObjectPropertyItem("ViewSettings", "ViewSettings", typeof(DALGridViewSettings), modelItem, typeof(Infragistics.Windows.DataPresenter.GridView), typeof(GridViewSettings), "GridViewSettings", groupViewSettings, null));
		}

		#endregion //Constructors
	}

	/// <summary>
	/// A predefined DesignerActionList class. Here you can specify the smart tag items 
	/// and/or methods which will be executed from the smart tag.
	/// </summary>
	public class DALGridViewSettings : DesignerActionList
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALGridViewSettings(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
		{
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_GridViewSettings") : alternateAdornerTitle;

			int groupSequence = 0;

			DesignerActionItemGroup groupSettings = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Settings"), groupSequence++);
			groupSettings.IsExpanded = true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(GridViewSettings), groupSettings))
			{
				this.Items.Add(pic.GetPropertyActionItem("HeightInInfiniteContainers"));
				this.Items.Add(pic.GetPropertyActionItem("Orientation"));
				this.Items.Add(pic.GetPropertyActionItem("UseNestedPanels"));
				this.Items.Add(pic.GetPropertyActionItem("WidthInInfiniteContainers"));
			}
		}

		#endregion //Constructors
	}

	#endregion //DALGridView, DALGridViewSettings Classes

	#region DALCardView, DALCardViewSettings Classes

	/// <summary>
	/// A predefined DesignerActionList class. Here you can specify the smart tag items 
	/// and/or methods which will be executed from the smart tag.
	/// </summary>
	public class DALCardView : DesignerActionList
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALCardView(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
		{
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_CardView") : alternateAdornerTitle;


			DesignerActionItemGroup groupViewSettings = new DesignerActionItemGroup(SR.GetString("SmartTag_G_ViewSettings"), 0);
			groupViewSettings.IsExpanded = true;
			this.Items.Add(new DesignerActionObjectPropertyItem("ViewSettings", "ViewSettings", typeof(DALCardViewSettings), modelItem, typeof(CardView), typeof(CardViewSettings), "CardViewSettings", groupViewSettings, null));
		}

		#endregion //Constructors
	}

	/// <summary>
	/// A predefined DesignerActionList class. Here you can specify the smart tag items 
	/// and/or methods which will be executed from the smart tag.
	/// </summary>
	public class DALCardViewSettings : DesignerActionList
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALCardViewSettings(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
		{
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_CardViewSettings") : alternateAdornerTitle;

			int groupSequence = 0;

			#region Sizing Group

			DesignerActionItemGroup groupSizing= new DesignerActionItemGroup(SR.GetString("SmartTag_G_CardSizing"), groupSequence++);
			groupSizing.IsExpanded = true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(CardViewSettings), groupSizing))
			{
				this.Items.Add(pic.GetPropertyActionItem("AllowCardWidthResizing"));
				this.Items.Add(pic.GetPropertyActionItem("AllowCardHeightResizing"));
				this.Items.Add(pic.GetPropertyActionItem("CardHeight"));
				this.Items.Add(pic.GetPropertyActionItem("CardWidth"));
			}

			#endregion //Sizing group

			#region Layout Group

			DesignerActionItemGroup groupLayout = new DesignerActionItemGroup(SR.GetString("SmartTag_G_CardLayout"), groupSequence++);
			groupLayout.IsExpanded = false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(CardViewSettings), groupLayout))
			{
				this.Items.Add(pic.GetPropertyActionItem("AutoFitCards"));
				this.Items.Add(pic.GetPropertyActionItem("InterCardSpacingX"));
				this.Items.Add(pic.GetPropertyActionItem("InterCardSpacingY"));
				this.Items.Add(pic.GetPropertyActionItem("MaxCardCols"));
				this.Items.Add(pic.GetPropertyActionItem("MaxCardRows"));
				this.Items.Add(pic.GetPropertyActionItem("Orientation"));
				this.Items.Add(pic.GetPropertyActionItem("Padding"));
			}

			#endregion //Layout group

			#region Header Group

			DesignerActionItemGroup groupHeader = new DesignerActionItemGroup(SR.GetString("SmartTag_G_CardHeader"), groupSequence++);
			groupHeader.IsExpanded = false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(CardViewSettings), groupHeader))
			{
				this.Items.Add(pic.GetPropertyActionItem("CollapseCardButtonVisibility"));
				this.Items.Add(pic.GetPropertyActionItem("CollapseEmptyCellsButtonVisibility"));
				this.Items.Add(pic.GetPropertyActionItem("HeaderPath"));
				this.Items.Add(pic.GetPropertyActionItem("HeaderVisibility"));
			}

			#endregion //Header group

			#region Options Group

			DesignerActionItemGroup groupOptions = new DesignerActionItemGroup(SR.GetString("SmartTag_G_CardOptions"), groupSequence++);
			groupOptions.IsExpanded = false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(CardViewSettings), groupOptions))
			{
				this.Items.Add(pic.GetPropertyActionItem("ShouldAnimateCardPositioning"));
				this.Items.Add(pic.GetPropertyActionItem("ShouldCollapseCards"));
				this.Items.Add(pic.GetPropertyActionItem("ShouldCollapseEmptyCells"));
			}

			#endregion //Options group
		}

		#endregion //Constructors
	}

	#endregion //DALCardView, DALCardViewSettings Classes

	#region DALCarouselView, DALCarouselViewSettings Classes

	/// <summary>
	/// A predefined DesignerActionList class. Here you can specify the smart tag items 
	/// and/or methods which will be executed from the smart tag.
	/// </summary>
	public class DALCarouselView : DesignerActionList
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALCarouselView(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
		{
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_CarouselView") : alternateAdornerTitle;


			DesignerActionItemGroup groupViewSettings = new DesignerActionItemGroup(SR.GetString("SmartTag_G_ViewSettings"), 0);
			groupViewSettings.IsExpanded = true;
			this.Items.Add(new DesignerActionObjectPropertyItem("ViewSettings", "ViewSettings", typeof(DALCarouselViewSettings), modelItem, typeof(CarouselView), typeof(CarouselViewSettings), "CarouselViewSettings", groupViewSettings, null));
		}

		#endregion //Constructors
	}

	/// <summary>
	/// A predefined DesignerActionList class. Here you can specify the smart tag items 
	/// and/or methods which will be executed from the smart tag.
	/// </summary>
	public class DALCarouselViewSettings : DesignerActionList
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALCarouselViewSettings(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
		{
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_CarouselViewSettings") : alternateAdornerTitle;

			int groupSequence = 0;

			#region Sizing Group

			DesignerActionItemGroup groupSizing = new DesignerActionItemGroup(SR.GetString("SmartTag_G_CarouselItemSizing"), groupSequence++);
			groupSizing.IsExpanded = true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(CarouselViewSettings), groupSizing))
			{
				this.Items.Add(pic.GetPropertyActionItem("AutoScaleItemContentsToFit"));
				this.Items.Add(pic.GetPropertyActionItem("ItemSize"));
				this.Items.Add(pic.GetPropertyActionItem("HeightInInfiniteContainers"));
				this.Items.Add(pic.GetPropertyActionItem("WidthInInfiniteContainers"));
			}

			#endregion //Sizing group

			#region ItemPath Group

			DesignerActionItemGroup groupItemPath = new DesignerActionItemGroup(SR.GetString("SmartTag_G_CarouselItemPath"), groupSequence++);
			groupItemPath.IsExpanded = false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(CarouselViewSettings), groupItemPath))
			{
				this.Items.Add(pic.GetPropertyActionItem("ItemPathAutoPad"));
				this.Items.Add(pic.GetPropertyActionItem("ItemPathHorizontalAlignment"));
				this.Items.Add(pic.GetPropertyActionItem("ItemPathVerticalAlignment"));
				this.Items.Add(pic.GetPropertyActionItem("ItemPathPadding"));
				this.Items.Add(pic.GetPropertyActionItem("ItemPathPrefixPercent"));
				this.Items.Add(pic.GetPropertyActionItem("ItemPathSuffixPercent"));
				this.Items.Add(pic.GetPropertyActionItem("ItemPathRenderBrush"));
				this.Items.Add(pic.GetPropertyActionItem("ItemPathStretch"));
			}

			#endregion //ItemPath group

			#region Options Group

			DesignerActionItemGroup groupOptions = new DesignerActionItemGroup(SR.GetString("SmartTag_G_CarouselOptions"), groupSequence++);
			groupOptions.IsExpanded = false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(CarouselViewSettings), groupOptions))
			{
				this.Items.Add(pic.GetPropertyActionItem("IsNavigatorVisible"));
				this.Items.Add(pic.GetPropertyActionItem("IsListContinuous"));
				this.Items.Add(pic.GetPropertyActionItem("ItemHorizontalScrollBarVisibility"));
				this.Items.Add(pic.GetPropertyActionItem("ItemVerticalScrollBarVisibility"));
				this.Items.Add(pic.GetPropertyActionItem("ItemTransitionStyle"));
				this.Items.Add(pic.GetPropertyActionItem("ReserveSpaceForReflections"));
				this.Items.Add(pic.GetPropertyActionItem("RotateItemsWithPathTangent"));
				this.Items.Add(pic.GetPropertyActionItem("ShouldAnimateItemsOnListChange"));
			}

			#endregion //Options group

			#region Effects Group

			DesignerActionItemGroup groupEffects = new DesignerActionItemGroup(SR.GetString("SmartTag_G_CarouselEffects"), groupSequence++);
			groupEffects.IsExpanded = false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(CarouselViewSettings), groupEffects))
			{
				this.Items.Add(pic.GetPropertyActionItem("OpacityEffectStopDirection"));
				this.Items.Add(pic.GetPropertyActionItem("UseOpacity"));
				this.Items.Add(pic.GetPropertyActionItem("SkewAngleXEffectStopDirection"));
				this.Items.Add(pic.GetPropertyActionItem("SkewAngleYEffectStopDirection"));
				this.Items.Add(pic.GetPropertyActionItem("UseSkewing"));
				this.Items.Add(pic.GetPropertyActionItem("ZOrderEffectStopDirection"));
				this.Items.Add(pic.GetPropertyActionItem("UseZOrder"));
			}

			#endregion //Options group
		}

		#endregion //Constructors
	}

	#endregion //DALCarouselView, DALCarouselViewSettings Classes

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