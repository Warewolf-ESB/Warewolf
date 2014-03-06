using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

using Infragistics.Shared;
using Infragistics.Windows.DockManager;

namespace Infragistics.Windows.Automation.Peers.DockManager
{
    /// <summary>
    /// Exposes the <see cref="XamDockManager"/> to UI Automation
    /// </summary>
    public class XamDockManagerAutomationPeer : FrameworkElementAutomationPeer,
        ISelectionProvider
    {
        #region Constructors

        /// <summary>
        /// Creates a new istance of the <see cref="XamDockManagerAutomationPeer"/>
        /// </summary>
        /// <param name="owner">The <see cref="XamDockManager"/> for which the peer is being created</param>
        public XamDockManagerAutomationPeer(XamDockManager owner): base(owner)
        {
        }

        #endregion //Constructors

        #region Base class overrides

        #region GetAutomationControlTypeCore

        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>Custom</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="XamDockManager"/>
        /// </summary>
        /// <returns>A string that contains 'XamDockManager'</returns>
        protected override string GetClassNameCore()
        {
            return "XamDockManager";
        }
        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="XamDockManager"/> that corresponds with this <see cref="XamDockManagerAutomationPeer"/>
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Selection)
                return this;

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #endregion //Base class overrides

        #region ISelectionProvider

        /// <summary>
        /// Defines when it is possible to select musliple items using SelectionPattern
        /// </summary>
        bool ISelectionProvider.CanSelectMultiple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the selected item (Content Pane)
        /// </summary>
        /// <returns>IRawElementProviderSimple[] interface array</returns>
        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            XamDockManager owner = (XamDockManager)this.Owner;
            ContentPane selectedItem = owner.ActivePane;

            if (selectedItem != null)
            {
                List<IRawElementProviderSimple> selectedItemPeers = new List<IRawElementProviderSimple>(1);

                if (selectedItem is ContentPane)
                {
                    AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(selectedItem);
                    selectedItemPeers.Add(this.ProviderFromPeer(peer));
                }

                return selectedItemPeers.ToArray();
            }

            return null;
        }

        bool ISelectionProvider.IsSelectionRequired
        {
            get { return false; }
        }

        #endregion //ISelectionProvider
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