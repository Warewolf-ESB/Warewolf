using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Ribbon;
using System.Windows.Automation.Provider;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
    /// <summary>
    /// Exposes <see cref="ContextualTabGroup"/> types to UI Automation
    /// </summary>
    public class ContextualTabGroupAutomationPeer : AutomationPeerProxy, IInvokeProvider
    {
        #region Member Variables
        ContextualTabGroup _group;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="ContextualTabGroupAutomationPeer"/> class
        /// </summary>
        /// <param name="group">The <see cref="ContextualTabGroup"/> for which the peer is being created</param>
        /// <exception cref="ArgumentNullException">The owner is null</exception>
        public ContextualTabGroupAutomationPeer(ContextualTabGroup group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            _group = group;
        }
        #endregion //Constructor

        #region Base class overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>TabItem</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.TabItem;
        }

        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="ContextualTabGroup"/>
        /// </summary>
        /// <returns>A string that contains 'ContextualTabGroup'</returns>
        protected override string GetClassNameCore()
        {
            return "ContextualTabGroup";
        }

        #endregion //GetClassNameCore

        #region GetUnderlyingPeer
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override AutomationPeer GetUnderlyingPeer()
        {
            return null != _group.CaptionElement
                ? UIElementAutomationPeer.CreatePeerForElement(_group.CaptionElement)
                : null;
        }
        #endregion //GetUnderlyingPeer

        #endregion //Base class overrides

        #region IInvokeProvider Members

        void IInvokeProvider.Invoke()
        {
            _group.SelectFirstTab();
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