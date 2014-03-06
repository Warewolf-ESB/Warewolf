using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Controls;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows;
using System.Windows.Automation;

namespace Infragistics.Windows.Automation.Peers
{
	/// <summary>
	/// Exposes the <see cref="XamCarouselListBox"/> to UI Automation.
	/// </summary>
	public class XamCarouselListBoxAutomationPeer : RecyclingItemsControlAutomationPeer,
		ISelectionProvider
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="XamCarouselListBoxAutomationPeer"/>
		/// </summary>
		/// <param name="listBox">XamCarouselListBox that this automation peer will represent.</param>
		public XamCarouselListBoxAutomationPeer(XamCarouselListBox listBox)
			: base(listBox)
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="XamCarouselListBox"/>
		/// </summary>
		/// <returns>A string that contains 'XamCarouselListBox'</returns>
		protected override string GetClassNameCore()
		{
			return "XamCarouselListBox";
		}
				#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the associated <see cref="XamCarouselListBox"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Scroll)
			{
				XamCarouselListBox list = (XamCarouselListBox)this.Owner;

				if (list != null)
				{
					if (list.ScrollInfo != null && list.ScrollInfo.ScrollOwner != null)
					{
						AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(list.ScrollInfo.ScrollOwner);

						if (null != peer && peer is IScrollProvider)
						{
							peer.EventsSource = this;
							return peer;
						}
					}
				}

				return null;
			}
			else if (patternInterface == PatternInterface.Selection)
			{
				return this;
			}

			return base.GetPattern(patternInterface);
		}
		#endregion //GetPattern

		#region CreateAutomationPeer
		/// <summary>
		/// Returns an automation peer used to represent the specified item
		/// </summary>
		/// <param name="item">The item from the items collection for which an automation peer is being requested</param>
		/// <returns>A <see cref="CarouselListBoxItemAutomationPeer"/></returns>
		protected override RecycleableItemAutomationPeer CreateAutomationPeer(object item)
		{
			return new CarouselListBoxItemAutomationPeer(item, this);
		}
		#endregion //CreateAutomationPeer

		#endregion //Base class overrides

		#region Methods

		#region RaiseAutomationEvent
		private void RaiseAutomationEvent(object item, AutomationEvents eventId)
		{
			AutomationPeer peer = this.GetItemPeer(item);

			if (peer != null)
				peer.RaiseAutomationEvent(eventId);
		} 
		#endregion //RaiseAutomationEvent

		#region RaiseIsSelectedChanged
		internal void RaiseIsSelectedChanged(DependencyObject container, bool isSelected)
		{
			object itemValue = this.GetItemOrContainerFromContainer(container);

			if (null != itemValue)
			{
				CarouselListBoxItemAutomationPeer itemPeer = this.GetItemPeer(itemValue) as CarouselListBoxItemAutomationPeer;

				if (null != itemPeer)
					itemPeer.RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, !isSelected, isSelected);
			}
		} 
		#endregion //RaiseIsSelectedChanged

		#region RaiseSelectionChanged
		internal void RaiseSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs args)
		{
			if (AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated) ||
				AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection) ||
				AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection) ||
				AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected))
			{
				if (args.AddedItems.Count == 1)
				{
					this.RaiseAutomationEvent(args.AddedItems[0], AutomationEvents.SelectionItemPatternOnElementSelected);
				}
				else if (args.AddedItems.Count + args.RemovedItems.Count > 20)
				{
					this.RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
				}
				else
				{
					foreach (object unselectedItem in args.RemovedItems)
						this.RaiseAutomationEvent(unselectedItem, AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);

					foreach (object selectedItem in args.AddedItems)
						this.RaiseAutomationEvent(selectedItem, AutomationEvents.SelectionItemPatternOnElementAddedToSelection);
				}
			}
		} 
		#endregion //RaiseSelectionChanged

		#endregion //Methods

		#region ISelectionProvider Members

		bool ISelectionProvider.CanSelectMultiple
		{
			get { return false; }
		}

		IRawElementProviderSimple[] ISelectionProvider.GetSelection()
		{
			XamCarouselListBox list = (XamCarouselListBox)this.Owner;

			if (list.SelectedItem != null)
			{
				RecycleableItemAutomationPeer peer = this.GetItemPeer(list.SelectedItem);

				if (null != peer)
				{
					IRawElementProviderSimple[] providers = new IRawElementProviderSimple[1];
					providers[0] = this.ProviderFromPeer(peer);
					return providers;
				}
			}

			return null;
		}

		bool ISelectionProvider.IsSelectionRequired
		{
			get { return false; }
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