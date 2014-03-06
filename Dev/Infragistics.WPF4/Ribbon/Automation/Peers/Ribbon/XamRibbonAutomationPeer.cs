using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Ribbon;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
    /// <summary>
    /// Exposes <see cref="XamRibbon"/> types to UI Automation
    /// </summary>
    public class XamRibbonAutomationPeer : FrameworkElementAutomationPeer
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="XamRibbonAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="XamRibbon"/> for which the peer is being created</param>
        public XamRibbonAutomationPeer(XamRibbon owner)
            : base(owner)
        {
        }
        #endregion //Constructor 

        #region Base class overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>ToolBar</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ToolBar;
        }

        #endregion //GetAutomationControlTypeCore

           #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="XamRibbon"/>
        /// </summary>
        /// <returns>A string that contains 'XamRibbon'</returns>
        protected override string GetClassNameCore()
        {
            return "XamRibbon";
        }

        #endregion //GetClassNameCore

           #region GetChildrenCore
        /// <summary>
        /// Returns the collection of child elements of the <see cref="XamRibbon"/> that is associated with this <see cref="XamRibbonAutomationPeer"/>
        /// </summary>
        /// <returns>The collection of child elements</returns>
        /// <remarks>
        /// <p class="body">The ribbon returns as its children QAT, ApplicationMenu, ContextualTabGroups and RibbonTabItems</p>
        /// </remarks> 
        protected override List<AutomationPeer> GetChildrenCore()
        {
            XamRibbon ribbon = this.Owner as XamRibbon;
            if (ribbon == null)
                return new List<AutomationPeer>();

            // we want empty list for our children
            List<AutomationPeer> list = new List<AutomationPeer>();// base.GetChildrenCore();
            if (list == null)
                list = new List<AutomationPeer>();

            // Create QAT AutomationPeer and add it to returned list
            AutomationPeer qatPeer = UIElementAutomationPeer.CreatePeerForElement(ribbon.QuickAccessToolbar);
            list.Add(qatPeer);

            // Create ApplicationMenu AutomationPeer and add it to returned list
            AutomationPeer appMenuPeer = UIElementAutomationPeer.CreatePeerForElement(ribbon.ApplicationMenu);
            list.Add(appMenuPeer);

            list.Add(UIElementAutomationPeer.CreatePeerForElement(ribbon.RibbonTabControl));
            //// Create RibbonTabItem AutomationPeer and add it to returned list
            //foreach (RibbonTabItem tabItem in ribbon.Tabs)
            //{
            //    AutomationPeer tabItemPeer = UIElementAutomationPeer.CreatePeerForElement(tabItem);
            //    list.Add(tabItemPeer);
            //}

            // Create ContextualTabGroup AutomationPeer and add it to returned list
            foreach (ContextualTabGroup group in ribbon.ContextualTabGroups)
            {
                list.Add(group.GetPeer(true));
            }

            return list;
        }
        #endregion // GetChildrenCore

           #region GetNameCore
        /// <summary>
        /// Gets a human readable name for <see cref="XamRibbon"/>. 
        /// </summary>
        /// <returns>The string that contains 'Infragistics XamRibbon'</returns>
        protected override string GetNameCore()
        {
			string name = base.GetNameCore();

            if (string.IsNullOrEmpty(name))
            {
                name = "Infragistics XamRibbon";
            }

            return name;
        }
        #endregion

        #endregion //Base class overrides
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