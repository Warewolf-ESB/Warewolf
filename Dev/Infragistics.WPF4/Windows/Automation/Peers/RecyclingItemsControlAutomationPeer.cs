using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Virtualization;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections;

namespace Infragistics.Windows.Automation.Peers
{
	/// <summary>
	/// Exposes a <see cref="RecyclingItemsControl"/> to UI Automation.
	/// </summary>
	public abstract class RecyclingItemsControlAutomationPeer : FrameworkElementAutomationPeer,
																IListAutomationPeer	// JM 08-20-09 NA 9.2 EnhancedGridView - Added.
	{
		#region Member Variables

		private Dictionary<object, RecycleableItemAutomationPeer> _itemPeers = new Dictionary<object, RecycleableItemAutomationPeer>();
		private Panel _itemsControlPanel;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="RecyclingItemsControlAutomationPeer"/>
		/// </summary>
		/// <param name="control">The <see cref="RecyclingItemsControl"/> for which the peer is being created</param>
		protected RecyclingItemsControlAutomationPeer(RecyclingItemsControl control)
			: base(control)
		{
		}

		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>List</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.List;
		}
				#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="RecyclingItemsControl"/>
		/// </summary>
		/// <returns>A string that contains 'RecyclingItemsControl'</returns>
		protected override string GetClassNameCore()
		{
			return "RecyclingItemsControl";
		}
		#endregion //GetClassNameCore

		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of automation peers that represents the items with the associated <see cref="RecyclingItemsControl"/>
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			ItemsControl itemsControl = (ItemsControl)base.Owner;

			List<AutomationPeer> baseItems = this.GetChildrenCoreBase(true);

			List<AutomationPeer> children = new List<AutomationPeer>();

			Debug.Assert(itemsControl.IsGrouping == false, "The automation object is not currently set up to handle grouping!");

			// store the peers and reuse them
			Dictionary<object, RecycleableItemAutomationPeer> oldItemPeers = this._itemPeers;
			this._itemPeers = new Dictionary<object, RecycleableItemAutomationPeer>();

			foreach (object item in itemsControl.Items)
			{
				if (null != item)
				{
					RecycleableItemAutomationPeer peer;

					// try to reuse the peers and if we don't have one then create a new one
					if (false == oldItemPeers.TryGetValue(item, out peer))
					{
						peer = this.CreateAutomationPeer(item);
					}

					if (peer != null)
					{
						AutomationPeer wrapperPeer = peer.GetWrapperPeer();

						// any events raised for the wrapped peer should raise
						// the events for the record automation peer since that is
						// what the clients will have a reference to
						if (null != wrapperPeer)
							wrapperPeer.EventsSource = peer;
					}

					children.Add(peer);
					this._itemPeers[item] = peer;
				}
			}

			if (null != baseItems)
				children.AddRange(baseItems);

			return children;
		}
		#endregion //GetChildrenCore

		#endregion //Base class overrides

		#region Properties

		#region ItemsControlPanel
		internal Panel ItemsControlPanel
		{
			get
			{
				if (this._itemsControlPanel == null)
				{
					this._itemsControlPanel = this.FindPanel();
				}

				return this._itemsControlPanel;
			}
		}
		#endregion //ItemsControlPanel

		#endregion //Properties

		#region Methods

		#region Protected

		#region ContainerFromItem
		/// <summary>
		/// Returns the generated uielement associated with the specified item from the list.
		/// </summary>
		/// <param name="item">The item in the list whose container is to be returned.</param>
		/// <returns>Returns the generated uielement associated with the specified item from the list.</returns>
		internal protected DependencyObject ContainerFromItem(object item)
		{
			RecyclingItemsControl list = (RecyclingItemsControl)this.Owner;
			RecyclingItemsPanel recyclingPanel = this.ItemsControlPanel as RecyclingItemsPanel;

			if (null != recyclingPanel && recyclingPanel.ItemContainerGenerationModeResolved == Infragistics.Windows.Controls.ItemContainerGenerationMode.Recycle)
				return list.RecyclingItemContainerGenerator.ContainerFromItem(item);
			else
				return list.ItemContainerGenerator.ContainerFromItem(item);
		}
		#endregion //ContainerFromItem

		#region CreateAutomationPeer

		/// <summary>
		/// Used to create an automation peer that will represent an item from the associated <see cref="RecyclingItemsControl"/>
		/// </summary>
		/// <param name="item">Item for which the automation peer is to be created</param>
		/// <returns>An <see cref="RecycleableItemAutomationPeer"/> that represents the specified item.</returns>
		protected abstract RecycleableItemAutomationPeer CreateAutomationPeer(object item);

		#endregion //CreateAutomationPeer

		#region GetChildrenCoreBase
		/// <summary>
		/// Returns the base implementation of <see cref="GetChildrenCore"/>.
		/// </summary>
		/// <param name="excludeItemElements">True to remove any peers that represents the item elements for the items within the <see cref="RecyclingItemsControl"/></param>
		/// <returns>The collection of child elements from the base implementation of GetChildrenCore.</returns>
		protected List<AutomationPeer> GetChildrenCoreBase(bool excludeItemElements)
		{
			// we need to get the base automation objects since 
			// there could be things other than items
			List<AutomationPeer> baseItems = base.GetChildrenCore();

			if (null != baseItems && excludeItemElements)
			{
				for (int i = baseItems.Count - 1; i >= 0; i--)
				{
					FrameworkElementAutomationPeer frameworkBaseItemPeer = baseItems[i] as FrameworkElementAutomationPeer;

					if (null != frameworkBaseItemPeer &&
						frameworkBaseItemPeer.Owner is ScrollViewer)
					{
						List<AutomationPeer> baseItemChildren = baseItems[i].GetChildren();

						if (null != baseItemChildren)
						{
							for (int j = baseItemChildren.Count - 1; j >= 0; j--)
							{
								FrameworkElementAutomationPeer frameworkPeer = baseItemChildren[j] as FrameworkElementAutomationPeer;

								// remove any records since we will add our wrappers
								if (null != frameworkPeer)
								{
									if (frameworkPeer.Owner is System.Windows.Controls.Primitives.ScrollBar)
										baseItemChildren.RemoveAt(j);
									else if (this.ItemFromContainer(frameworkPeer.Owner) != DependencyProperty.UnsetValue)
										baseItemChildren.RemoveAt(j);
								}
							}
						}

						// remove the scroll viewer
						baseItems.RemoveAt(i);

						// promote the children
						if (null != baseItemChildren)
							baseItems.AddRange(baseItemChildren);
					}
					// AS 6/5/07
					//else if (rlc.ItemContainerGenerator.ItemFromContainer(frameworkBaseItemPeer.Owner) != DependencyProperty.UnsetValue)
					else if (this.ItemFromContainer(frameworkBaseItemPeer.Owner) != DependencyProperty.UnsetValue)
						baseItems.RemoveAt(i);
				}
			}

			return baseItems;
		}
		#endregion //GetChildrenCoreBase

		#region GetItemOrContainerFromContainer
		/// <summary>
		/// Returns the item represented by the specified element container.
		/// </summary>
		/// <param name="container">The element for which the item will be returned.</param>
		/// <returns>The item represented by the element, the element itself if it is the item or <see cref="DependencyProperty.UnsetValue"/> is the container is not recognized.</returns>
		internal protected object GetItemOrContainerFromContainer(DependencyObject container)
		{
			object item = this.ItemFromContainer(container);

			if (item == DependencyProperty.UnsetValue &&
				ItemsControl.ItemsControlFromItemContainer(container) == this.Owner &&
				((RecyclingItemsControl)this.Owner).IsItemItsOwnContainerInternal(container))
			{
				item = container;
			}

			return item;
		} 
		#endregion //GetItemOrContainerFromContainer

		#region GetItemPeer
		/// <summary>
		/// Returns the automation peer that represents the specified item.
		/// </summary>
		/// <param name="item">Item whose automation peer is to be obtained.</param>
		/// <returns>A <see cref="RecycleableItemAutomationPeer"/></returns>
		internal protected virtual RecycleableItemAutomationPeer GetItemPeer(object item)
		{
			RecycleableItemAutomationPeer peer = null;

			if (null != this._itemPeers)
				this._itemPeers.TryGetValue(item, out peer);

			return peer;
		}
		#endregion //GetItemPeer

		#region ItemFromContainer
		/// <summary>
		/// Returns the item from the list that corresponds to the specified generated UIElement.
		/// </summary>
		/// <param name="container">The <see cref="DependencyObject"/> that corresponds with the item to be returned.</param>
		/// <returns>Returns the item from the list that corresponds to the specified generated UIElement.</returns>
		internal protected object ItemFromContainer(DependencyObject container)
		{
			RecyclingItemsControl list = (RecyclingItemsControl)this.Owner;
			RecyclingItemsPanel recyclingPanel = this.ItemsControlPanel as RecyclingItemsPanel;

			if (null != recyclingPanel && recyclingPanel.ItemContainerGenerationModeResolved == Infragistics.Windows.Controls.ItemContainerGenerationMode.Recycle)
				return list.RecyclingItemContainerGenerator.ItemFromContainer(container);
			else
				return list.ItemContainerGenerator.ItemFromContainer(container);
		}
		#endregion //ItemFromContainer

		#region OnItemsChanged

		/// <summary>
		/// Invoked when the contents of the items collection of the associated <see cref="RecyclingItemsControl"/> changed.
		/// </summary>
		/// <param name="e">Event arguments indicating the change that occurred.</param>
		internal protected virtual void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
		}
		#endregion //OnItemsChanged

		// AS 9/12/07 - See RecyclingItemsControl.OnItemsSourceChanged
		#region OnItemsSourceChanged
		/// <summary>
		/// Invoked when the ItemsSource of the associated <see cref="RecyclingItemsControl"/> has changed.
		/// </summary>
		/// <param name="oldValue">Old item source</param>
		/// <param name="newValue">New item source</param>
		internal protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
		} 
		#endregion //OnItemsSourceChanged

		#endregion //Protected

		#region Private

		#region FindPanel
		private Panel FindPanel()
		{
			Utilities.DependencyObjectSearchCallback<Panel> callBack = delegate(Panel panel)
			{
				return ItemsControl.GetItemsOwner(panel) == this.Owner;
			};

			return Utilities.GetDescendantFromType<Panel>(this.Owner, true, callBack, new Type[] { typeof(ItemsControl) });
		}
				#endregion //FindPanel

		#endregion //Private

		#region Internal

		#region OnItemsPanelChanged
		internal void OnItemsPanelChanged()
		{
			this._itemsControlPanel = null;
		}
				#endregion //OnItemsPanelChanged

		#endregion //Internal

		#endregion //Methods

		// JM 08-20-09 NA 9.2 EnhancedGridView - Added.
		#region IListAutomationPeer Members

		AutomationPeer IListAutomationPeer.GetUnderlyingPeer(object item)
		{
			UIElement element = this.ContainerFromItem(item) as UIElement;

			return null != element
				? UIElementAutomationPeer.CreatePeerForElement(element)
				: null;
		}

		DependencyObject IListAutomationPeer.ContainerFromItem(object item)
		{
			return this.ContainerFromItem(item);
		}

		object IListAutomationPeer.GetPattern(PatternInterface patternInterface)
		{
			return this.GetPattern(patternInterface);
		}

		UIElement IListAutomationPeer.Owner
		{
			get { return this.Owner; }
		}

		Panel IListAutomationPeer.ItemsControlPanel
		{
			get { return this.ItemsControlPanel; }
		}

	#endregion
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