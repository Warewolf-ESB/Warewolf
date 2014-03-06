using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Ribbon;
using System.Windows.Automation.Provider;
using System.Windows.Automation;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
    /// <summary>
    /// Exposes <see cref="MenuToolBase"/> types to UI Automation
    /// </summary>
    public class MenuToolBaseAutomationPeer : FrameworkElementAutomationPeer,
                                                IExpandCollapseProvider
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="MenuToolBaseAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="MenuToolBase"/> for which the peer is being created</param>
        public MenuToolBaseAutomationPeer(MenuToolBase owner)
            : base(owner)
        {
        }
        #endregion //Constructor

        #region Base class overrides

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
            MenuToolBase owner = this.Owner as MenuToolBase;

			// AS 11/24/09 TFS25020
			if (null == owner.Presenter)
				return base.GetChildrenCore();

			// return the visual children of the presenter.
            AutomationPeer contentHostPeer = new FrameworkElementAutomationPeer(owner.Presenter);
            List<AutomationPeer> contentChildren = contentHostPeer.GetChildren();
            return contentChildren;
        }
            #endregion // GetChildrenCore

            #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="MenuToolBase"/>
        /// </summary>
        /// <returns>A string that contains 'MenuToolBase'</returns>
        protected override string GetClassNameCore()
        {
            return "MenuToolBase";
        }

            #endregion //GetClassNameCore

            #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>Menu</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Menu;
        }

            #endregion //GetAutomationControlTypeCore

            #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the associated <see cref="MenuToolBase"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        /// <returns>Object that implement IExpandCollapseProvider pattern</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse)
                return this;

            return base.GetPattern(patternInterface);
        }
            #endregion // GetPattern

        #endregion //Base class overrides

        #region Methods

            #region VerifyEnabled
        /// <summary>
        /// Verify if element is enable
        /// </summary>
        protected void VerifyEnabled()
        {
            if (!base.IsEnabled())
                throw new ElementNotEnabledException();
        }
        #endregion

            #region RaiseExpandCollapseStateChanged

        internal void RaiseExpandCollapseStateChanged(bool oldValue, bool newValue)
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                RaisePropertyChangedEvent(
                    ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                    oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                    newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
            }
        }
        #endregion //RaiseExpandCollapseStateChanged

        #endregion //Methods


        #region IExpandCollapseProvider Members

        void IExpandCollapseProvider.Collapse()
        {
            this.VerifyEnabled();

            ((MenuToolBase)this.Owner).IsOpen = false;
        }

        void IExpandCollapseProvider.Expand()
        {
            this.VerifyEnabled();

            ((MenuToolBase)this.Owner).IsOpen = true;
        }

        System.Windows.Automation.ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get
            {
                if (((MenuToolBase)this.Owner).IsOpen)
                {
                    return ExpandCollapseState.Expanded;
                }
                else
                {
                    return ExpandCollapseState.Collapsed;
                }
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