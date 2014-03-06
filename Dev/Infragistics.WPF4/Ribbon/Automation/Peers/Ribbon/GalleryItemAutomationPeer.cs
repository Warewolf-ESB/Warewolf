using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Ribbon;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using Infragistics.Windows.Ribbon.Events;
using System.Windows.Controls;
using System.Diagnostics;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
    /// <summary>
    /// Exposes <see cref="GalleryItem"/> types to UI Automation
    /// </summary>
    public class GalleryItemAutomationPeer : ItemAutomationPeer, ISelectionItemProvider
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="GalleryItemAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="GalleryItem"/> for which the peer is being created</param>
        /// <param name="container">The ItemsControlAutomationPeer that is associated with the ItemsControl that holds the <see cref="GalleryItem"/>  collection.</param>
        public GalleryItemAutomationPeer(GalleryItem owner, ItemsControlAutomationPeer container)
            : base(owner, container)
        {
        }
        #endregion //Constructor

        #region Base class overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>Custom</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ListItem;
        }

        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="GalleryItem"/>
        /// </summary>
        /// <returns>A string that contains 'GalleryItem'</returns>
        protected override string GetClassNameCore()
        {
            return "GalleryItem";
        }

        #endregion //GetClassNameCore

        #region GetNameCore
        /// <summary>
        /// Gets a human readable name for <see cref="GalleryItem"/>. 
        /// </summary>
        /// <returns>The string in <see cref="GalleryItem.Text"/>.</returns>
        protected override string GetNameCore()
        {
            return ((GalleryItem)this.Item).Text;
        }
        #endregion

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the associated <see cref="GalleryItemPresenter"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        /// <returns>Object that implement ISelectionItemProvider pattern</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.SelectionItem)
                return this;

            return null;// base.GetPattern(patternInterface);
        }
        #endregion // GetPattern

        #endregion //Base class overrides

        #region ISelectionItemProvider Members

        void ISelectionItemProvider.AddToSelection()
        {
			// JJD 12/9/11 
			// Check if enabled
			if (!IsEnabled())
				throw new ElementNotEnabledException();

			if (((ISelectionItemProvider)this).IsSelected)
				return;

			GalleryTool gallery;
			GalleryItemGroup group;
			GetGalleryInfo(out gallery, out group);

			Debug.Assert(gallery != null, "We should have a gallery tool at this point");
			
			// JJD 12/9/11 
			// Throw invalid operation exception
			if (gallery == null ||
				gallery.ItemBehavior == GalleryToolItemBehavior.Button ||
				gallery.SelectedItem != null)
			{
				throw new InvalidOperationException();
			}

			((ISelectionItemProvider)this).Select();

        }

        bool ISelectionItemProvider.IsSelected
        {
            get
            {
                return (this.Item as GalleryItem).IsSelected;
            }
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
			// JJD 12/9/11 
			// Check if enabled
            if (!IsEnabled())
                throw new ElementNotEnabledException();

			// JJD 12/9/11 
			// un-select the item
           (this.Item as GalleryItem).IsSelected = false;
          
        }

        void ISelectionItemProvider.Select()
        {
            if (!IsEnabled())
                throw new ElementNotEnabledException();

            GalleryTool gallery;
            GalleryItemGroup group;
            GetGalleryInfo(out gallery, out group);

			// JJD 12/9/11 
			// make sure gallery is not null
            //if (gallery.ItemBehavior == GalleryToolItemBehavior.StateButton)
			if (gallery != null)
			{
				if (gallery.ItemBehavior == GalleryToolItemBehavior.StateButton)
				{
					(this.Item as GalleryItem).IsSelected = true;
				}
				// JJD 12/9/11 
				// For state button behavior seting IsSlected above will cause the
				// ItemSelected event to be raised so only call RaiseItemClickedEvent
				// when the behavior is 'Button"
				//RaiseItemSelectedOrClickEvent();
				else
				{
					this.RaiseItemClickedEvent(gallery, group);
				}
			}
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get
            {
                return ProviderFromPeer(this.ItemsControlAutomationPeer); 
            }
        }

        #endregion

        #region Methods
		// JJD 12/9/11 
		// Pass the gallery and group in since the caller has this info
		// Also renamed method since it is only called to raise the ItemClicked event
		//private void RaiseItemSelectedOrClickEvent()
        private void RaiseItemClickedEvent(GalleryTool gallery, GalleryItemGroup group)
        {
			//GalleryTool gallery;
			//GalleryItemGroup group;
			//GetGalleryInfo(out gallery, out group);

			Debug.Assert(gallery != null, "We should have a gallery tool at this point");
			if (gallery == null)
				return;

			// JJD 12/9/11 
			// Use the associated ItemsControlAutomationPeer to get the presenter for this item
			GalleryItemPresenter presenter = null;

			ItemsControlAutomationPeer	itemsControlPeer = this.ItemsControlAutomationPeer;
			if (itemsControlPeer != null)
				presenter = ((ItemsControl)(itemsControlPeer.Owner)).ItemContainerGenerator.ContainerFromItem(this.Item) as GalleryItemPresenter;

					// raise the appropriated event
			// JJD 12/9/11 - added internal ctor that takes GalleryItemPresenter so we can expose it for TestAdvantage use
			//GalleryItemEventArgs args = new GalleryItemEventArgs(gallery, group, this.Item as GalleryItem);
            GalleryItemEventArgs args = new GalleryItemEventArgs(gallery, group, this.Item as GalleryItem, presenter);
			//if (gallery.ItemBehavior == GalleryToolItemBehavior.Button)
			//{
				// JJD 12/9/11 
				// Call the 
				// args.RoutedEvent = GalleryTool.ItemClickedEvent;
				gallery.RaiseItemClicked(args);
			//}
			//else
			//{
			//   // args.RoutedEvent = GalleryTool.ItemSelectedEvent;
			//}
            //gallery.RaiseEvent(args);
        }

        private void GetGalleryInfo(out GalleryTool gallery, out GalleryItemGroup group)
        {
            gallery = null;
            group = null;
            if (this.ItemsControlAutomationPeer is GalleryItemGroupAutomationPeer)
            {
                group = (this.ItemsControlAutomationPeer as GalleryItemGroupAutomationPeer).Owner as GalleryItemGroup;
                gallery = group.GalleryTool;

            }
				
			else if (this.ItemsControlAutomationPeer is GalleryToolPreviewPresenterAutomationPeer)
			{
				GalleryToolPreviewPresenter previewPresenter = (this.ItemsControlAutomationPeer as GalleryToolPreviewPresenterAutomationPeer).Owner as GalleryToolPreviewPresenter;
				gallery = previewPresenter.GalleryTool;
			}
            else if (this.ItemsControlAutomationPeer is GalleryToolDropDownPresenterAutomationPeer)
            {
                GalleryToolDropDownPresenter dropDownPresenter = (this.ItemsControlAutomationPeer as GalleryToolDropDownPresenterAutomationPeer).Owner as GalleryToolDropDownPresenter;
                gallery = Utilities.GetAncestorFromType(dropDownPresenter, typeof(GalleryTool), false) as GalleryTool;
            }
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