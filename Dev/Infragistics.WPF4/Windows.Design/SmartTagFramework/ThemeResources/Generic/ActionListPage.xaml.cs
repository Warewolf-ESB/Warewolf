using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Design.Model;
using System.ComponentModel;

namespace Infragistics.Windows.Design.SmartTagFramework
{
	/// <summary>
	/// Interaction logic for ActionListPage.xaml
	/// </summary>
	[DesignTimeVisible(false)]
	public partial class ActionListPage : UserControl
	{
		#region Member Variables

		private ModelItem					_modelItem;
		private DesignerActionList			_actionList;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Creates an instance of an ActionListPage.
		/// </summary>
		public ActionListPage()
		{
			InitializeComponent();
		}

		#endregion //Constructor

		#region Methods

			#region AdornedControlModelPropertyChanged

		private void AdornedControlModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("Properties"))
				return;

			foreach (DesignerActionItem item in this._actionList.Items)
			{
				if (item is DesignerActionPropertyItem)
				{
					DesignerActionPropertyItem propertyItem = item as DesignerActionPropertyItem;
					if (propertyItem.Name.Equals(e.PropertyName))
					{
						propertyItem.Value = this._modelItem.Properties[e.PropertyName].ComputedValue;
						break;
					}
				}
			}
		}

			#endregion //AdornedControlModelPropertyChanged

			#region Initialize

		internal void Initialize(ModelItem modelItem, DesignerActionList actionList)
		{
			this._modelItem			= modelItem;
			this._actionList		= actionList;

			this.itemsControl.ItemsSource = actionList.Items;

			CollectionView	cv				= CollectionViewSource.GetDefaultView(this.itemsControl.ItemsSource) as CollectionView;
			cv.SortDescriptions.Clear();
			cv.GroupDescriptions.Clear();

			SortDescription sortDescription = new SortDescription("DesignerActionItemGroup.OrderNumber", ListSortDirection.Ascending);
			cv.SortDescriptions.Add(sortDescription);

			sortDescription					= new SortDescription("OrderNumber", ListSortDirection.Ascending);
			cv.SortDescriptions.Add(sortDescription);

			PropertyGroupDescription groupDescription	= new PropertyGroupDescription();
			groupDescription.PropertyName				= "DesignerActionItemGroup.Name";
			cv.GroupDescriptions.Add(groupDescription);

			this._modelItem.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(AdornedControlModelPropertyChanged);

			foreach (DesignerActionItem daItem in this._actionList.Items)
			{
				if (daItem is DesignerActionPropertyItem)
				{
					DesignerActionPropertyItem propertyItem = daItem as DesignerActionPropertyItem;
					propertyItem.PropertyChanged			+= new PropertyChangedEventHandler(PropertyItemPropertyChanged);
				}
			}
		}

			#endregion //Initialize

			#region PropertyItemPropertyChanged

		private void PropertyItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			DesignerActionPropertyItem	propertyItem	= sender as DesignerActionPropertyItem;
			ModelProperty				modelProperty	= this._modelItem.Properties[propertyItem.Name];

			if (propertyItem.Value == modelProperty.ComputedValue)
				return;

			if ((null != modelProperty.ComputedValue)	&& 
				(null != propertyItem.Value)			&&
				propertyItem.Value.ToString() == modelProperty.ComputedValue.ToString())
				return;

			modelProperty.SetValue(propertyItem.Value);
		}

			#endregion //PropertyItemPropertyChanged

			#region UnInitialize

		internal void UnInitialize()
		{
			this._modelItem.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(AdornedControlModelPropertyChanged);

			foreach (DesignerActionItem daItem in this._actionList.Items)
			{
				if (daItem is DesignerActionPropertyItem)
				{
					DesignerActionPropertyItem propertyItem = daItem as DesignerActionPropertyItem;
					propertyItem.PropertyChanged			-= new PropertyChangedEventHandler(PropertyItemPropertyChanged);
				}
			}
		}

			#endregion //UnInitialize

		#endregion //Methods

		#region Properties

			#region ActionList

		internal DesignerActionList ActionList
		{
			get { return this._actionList; }
		}

			#endregion //ActionList

			#region ModelItem

		internal ModelItem ModelItem
		{
			get { return this._modelItem; }
		}

			#endregion //ModelItem

		#endregion //Properties
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