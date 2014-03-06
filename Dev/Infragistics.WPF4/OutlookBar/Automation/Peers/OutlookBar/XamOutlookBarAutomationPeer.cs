using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

using Infragistics.Shared;
using Infragistics.Windows.OutlookBar;

namespace Infragistics.Windows.Automation.Peers.OutlookBar
{
    /// <summary>
    /// Exposes <see cref="XamOutlookBar"/> types to UI Automation
    /// </summary>
    public class XamOutlookBarAutomationPeer : FrameworkElementAutomationPeer,
        IExpandCollapseProvider, ISelectionProvider
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="XamOutlookBarAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="XamOutlookBar"/> for which the peer is being created</param>
        public XamOutlookBarAutomationPeer(XamOutlookBar owner)
            : base(owner)
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
            return AutomationControlType.Custom;
        }

        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="XamOutlookBar"/>
        /// </summary>
        /// <returns>A string that contains 'XamOutlookBar'</returns>
        protected override string GetClassNameCore()
        {
            return "XamOutlookBar"; 
        }

        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="XamOutlookBar"/> that corresponds with this <see cref="XamOutlookBarAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse || 
                patternInterface == PatternInterface.Selection)
                return this;

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #endregion //Base class overrides

        #region IExpandCollapseProvider

        void IExpandCollapseProvider.Collapse()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
                throw new InvalidOperationException();

            XamOutlookBar owner = (XamOutlookBar)this.Owner;
            owner.IsMinimized = true;
        }

        void IExpandCollapseProvider.Expand()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
                throw new InvalidOperationException();

            XamOutlookBar owner = (XamOutlookBar)this.Owner;
            owner.IsMinimized = false;
        }

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get
            {
                XamOutlookBar owner = (XamOutlookBar)this.Owner;

                if (owner.AllowMinimized == false)
                    return ExpandCollapseState.LeafNode;

                if (owner.IsMinimized)
                    return ExpandCollapseState.Collapsed;
                else
                    return ExpandCollapseState.Expanded;
            }
        }

        #endregion //IExpandCollapseProvider

        #region ISelectionProvider

        bool ISelectionProvider.CanSelectMultiple
        {
            get
            {
                return false;
            }
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            XamOutlookBar owner = (XamOutlookBar)this.Owner;

            OutlookBarGroup selectedItem = owner.SelectedGroup;

            if (selectedItem != null)
            {
                List<IRawElementProviderSimple> selectedItemPeers = new List<IRawElementProviderSimple>(1);

                if (selectedItem is OutlookBarGroup)
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