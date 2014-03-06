using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows;
using Infragistics.Windows.Virtualization;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers
{
	/// <summary>
	/// Exposes an item of the <see cref="RecyclingItemsControlAutomationPeer"/> to UI Automation
	/// </summary>
	public class RecycleableItemAutomationPeer : AutomationPeerProxy
	{
		#region Member Variables

		private object _item;

		// JM 08-20-09 NA 9.2 EnhancedGridView
		//private RecyclingItemsControlAutomationPeer _listAutomationPeer;
		private IListAutomationPeer _listAutomationPeer;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="RecycleableItemAutomationPeer"/> class
		/// </summary>
		/// <param name="item">The item for which the peer is being created</param>
		/// <param name="listAutomationPeer">The containing automation peer for the item being created</param>
		// JM 08-20-09 NA 9.2 EnhancedGridView
		//public RecycleableItemAutomationPeer(object item, RecyclingItemsControlAutomationPeer listAutomationPeer)
		public RecycleableItemAutomationPeer(object item, IListAutomationPeer listAutomationPeer)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (listAutomationPeer == null)
				throw new ArgumentNullException("listAutomationPeer");

			this._item = item;
			this._listAutomationPeer = listAutomationPeer;
		}

		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>ListItem</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.ListItem;
		}

				#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the item
		/// </summary>
		/// <returns>A string that contains 'Item'</returns>
		protected override string GetClassNameCore()
		{
			return "Item";
		}

		#endregion //GetClassNameCore

		#region GetUnderlyingPeer
		/// <summary>
		/// Returns the automation peer for which this proxy is associated.
		/// </summary>
		/// <returns>An <see cref="AutomationPeer"/> that represents the element for the item</returns>
		protected override AutomationPeer GetUnderlyingPeer()
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView
			//UIElement element = this._listAutomationPeer.ContainerFromItem(this._item) as UIElement;

			//return null != element
			//    ? UIElementAutomationPeer.CreatePeerForElement(element)
			//    : null;
			return this._listAutomationPeer.GetUnderlyingPeer(this._item);
		}
		#endregion //GetUnderlyingPeer

		#endregion //Base class overrides

		#region Properties
		/// <summary>
		/// Returns the item from the containing <see cref="RecyclingItemsControl"/> with which the automation peer is associated.
		/// </summary>
		public object Item
		{
			get { return this._item; }
		}

		// JM 08-20-09 NA 9.2 EnhancedGridView
		///// <summary>
		///// Returns the owning <see cref="RecyclingItemsControlAutomationPeer"/>
		///// </summary>
		//public RecyclingItemsControlAutomationPeer ItemsControlAutomationPeer
		//{
		//    get { return this._listAutomationPeer; }
		//}
		/// <summary>
		/// Returns the owning <see cref="IListAutomationPeer"/>
		/// </summary>
		public IListAutomationPeer ListAutomationPeer
		{
			get { return this._listAutomationPeer; }
		}

		#endregion //Properties

		#region Methods

		#region GetWrapperPeer
		internal AutomationPeer GetWrapperPeer()
		{
			return this.GetUnderlyingPeer();
		}
				#endregion //GetWrapperPeer

		#endregion //Methods
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