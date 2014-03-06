using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Ribbon;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Windows;
using System.Windows.Threading;
using System.Collections;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
    /// <summary>
    /// Exposes <see cref="GalleryToolDropDownPresenter"/> types to UI Automation
    /// </summary>
    public class GalleryToolDropDownPresenterAutomationPeer : ItemsControlAutomationPeer, ISelectionProvider
    {
        #region Member variables
        Hashtable _itemPeers = new Hashtable();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="GalleryToolDropDownPresenterAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="GalleryToolDropDownPresenter"/> for which the peer is being created</param>
        public GalleryToolDropDownPresenterAutomationPeer(GalleryToolDropDownPresenter owner)
            : base(owner)
        {
        }
        #endregion //Constructor

        #region Base class overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>Pane</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Pane;
        }
        #endregion

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="GalleryToolDropDownPresenter"/>
        /// </summary>
        /// <returns>A string that contains 'GalleryToolDropDownPresenter'</returns>
        protected override string GetClassNameCore()
        {
            return "GalleryToolDropDownPresenter";
        }

        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the associated <see cref="GalleryTool"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        /// <returns>Object that implement ISelectionProvider pattern</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Selection)
                return this;

            return base.GetPattern(patternInterface);
        }
        #endregion // GetPattern

        #region CreateItemAutomationPeer
        /// <summary>
        /// Creates a new instance of the ItemAutomationPeer class.
        /// </summary>
        /// <param name="item">The <see cref="GalleryItem"/> or <see cref="GalleryItemGroup"/> that is associated with this GalleryToolDropDownPresenterAutomationPeer.
        ///</param>
        /// <returns>Automation peer for item</returns>
        protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
        {
            if (item is GalleryItem)
            {
                GalleryItemAutomationPeer peer = new GalleryItemAutomationPeer(item as GalleryItem, this);
                _itemPeers[item] = peer;
                return peer;
            }
            else if (item is GalleryItemGroup)
            {

                GalleryItemGroupItemAutomationPeer peer = new GalleryItemGroupItemAutomationPeer(item as GalleryItemGroup, this);
                _itemPeers[item] = peer;
                return peer;
            }

            return null;
        }
        #endregion //CreateItemAutomationPeer

        #endregion //Base class overrides


        #region ISelectionProvider Members

        bool ISelectionProvider.CanSelectMultiple
        {
            get { return false; }
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            GalleryToolDropDownPresenter owner = Owner as GalleryToolDropDownPresenter;

            GalleryItem selItem = owner.GalleryTool.SelectedItem;

            if (selItem == null)
                return null;

            List<IRawElementProviderSimple> selectedProviders = new List<IRawElementProviderSimple>(1);
            GalleryItemAutomationPeer itemPeer = this._itemPeers[selItem] as GalleryItemAutomationPeer;

            if (itemPeer != null)
            {
                selectedProviders.Add(ProviderFromPeer(itemPeer));
                return selectedProviders.ToArray();
            }
            else
            {
                foreach (DictionaryEntry entry in this._itemPeers)
                {
                    GalleryItemGroupItemAutomationPeer groupItemPeer = entry.Value as GalleryItemGroupItemAutomationPeer;
                    GalleryItemGroupAutomationPeer groupPeer = UIElementAutomationPeer.FromElement(groupItemPeer.Item as UIElement) as GalleryItemGroupAutomationPeer;
                    if (groupPeer == null)
                        continue;

                    itemPeer = groupPeer.GetItemPeer(selItem);
                    if (itemPeer != null)
                    {
                        selectedProviders.Add(ProviderFromPeer(itemPeer));
                        return selectedProviders.ToArray();
                    }
                }
                return null;
            }

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