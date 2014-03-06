using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

using Infragistics.Shared;
using Infragistics.Windows.OutlookBar;

namespace Infragistics.Windows.Automation.Peers.OutlookBar
{
    /// <summary>
    /// Exposes <see cref="OutlookBarGroup"/> types to UI Automation
    /// </summary>
    public class OutlookBarGroupAutomationPeer : FrameworkElementAutomationPeer,
        ISelectionItemProvider
    {
        #region Member Variables
        #endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Initializes a new instance of <see cref="OutlookBarGroupAutomationPeer"/>
        /// </summary>
        /// <param name="owner">An <see cref="XamOutlookBar"/> instance</param>
        public OutlookBarGroupAutomationPeer(OutlookBarGroup owner)
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
        /// Returns the name of the <see cref="OutlookBarGroup"/>
        /// </summary>
        /// <returns>A string that contains 'OutlookBarGoup'</returns>
        protected override string GetClassNameCore()
        {
            return "OutlookBarGroup";
        }
        #endregion //GetClassNameCore

        #region GetNameCore
        /// <summary>
        /// Returns the text label for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
        /// </summary>
        /// <returns>The text label</returns>
        protected override string GetNameCore()
        {
            string name = base.GetNameCore();

            if (string.IsNullOrEmpty(name))
            {
                OutlookBarGroup outlookBarGroup = (OutlookBarGroup)this.Owner;

                if (outlookBarGroup.Name != null)
                {
                    name = outlookBarGroup.Name;
                }
            }

            return name;
        }
        #endregion //GetNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="OutlookBarGroup"/> that corresponds with this <see cref="OutlookBarGroupAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.SelectionItem)
                return this;

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #endregion //Base class overrides

        #region Properties


        #endregion //Properties

        #region ISelectionItemProvider

        void ISelectionItemProvider.AddToSelection()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            OutlookBarGroup outlookBarGroup = (OutlookBarGroup)this.Owner;

            XamOutlookBarAutomationPeer peer = new XamOutlookBarAutomationPeer(outlookBarGroup.OutlookBar);
            ISelectionProvider provider = peer != null
                ? peer.GetPattern(PatternInterface.Selection) as ISelectionProvider
                : null;


            outlookBarGroup.IsSelected = true;
        }

        bool ISelectionItemProvider.IsSelected
        {
            get 
            {
                OutlookBarGroup outlookBarGroup = (OutlookBarGroup)this.Owner;
                return outlookBarGroup.IsSelected; 
            }
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            OutlookBarGroup outlookBarGroup = (OutlookBarGroup)this.Owner;
            outlookBarGroup.IsSelected = false;
        }


        void ISelectionItemProvider.Select()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            OutlookBarGroup outlookBarGroup = (OutlookBarGroup)this.Owner;
            XamOutlookBar outlookBar = outlookBarGroup.OutlookBar;

            if (null != outlookBar)
                outlookBar.SelectGroup(outlookBarGroup, true);
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get
            {
                OutlookBarGroup outlookBarGroup = (OutlookBarGroup)this.Owner;
                XamOutlookBarAutomationPeer peer = new XamOutlookBarAutomationPeer(outlookBarGroup.OutlookBar);
                return peer != null
                    ? this.ProviderFromPeer(peer)
                    : null;
            }
        }


        #endregion //ISelectionItemProvider


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