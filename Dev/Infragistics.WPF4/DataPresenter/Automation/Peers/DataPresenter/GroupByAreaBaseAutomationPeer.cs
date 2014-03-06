using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation;
using Infragistics.Windows.DataPresenter;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Peers;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
    /// <summary>
    /// Exposes <see cref="GroupByAreaBase"/> types to UI Automation
    /// </summary>
    public class GroupByAreaBaseAutomationPeer : FrameworkElementAutomationPeer,
        IExpandCollapseProvider
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="GroupByAreaAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="GroupByAreaBase"/> for which the peer is being created</param>
        public GroupByAreaBaseAutomationPeer(GroupByAreaBase owner)
            : base(owner)
        {
        }
        #endregion //Constructor

        #region Base class overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>Group</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }

        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="GroupByAreaBase"/>
        /// </summary>
        /// <returns>A string that contains 'GroupByAreaBase'</returns>
        protected override string GetClassNameCore()
        {
            return "GroupByAreaBase";
        }

        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="GroupByArea"/> that corresponds with this <see cref="GroupByAreaAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse)
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

            GroupByAreaBase groupByArea = (GroupByAreaBase)this.Owner;
            groupByArea.IsExpanded = false;
        }

        void IExpandCollapseProvider.Expand()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
                throw new InvalidOperationException();

            GroupByAreaBase groupByArea = (GroupByAreaBase)this.Owner;
            groupByArea.IsExpanded = true;
        }

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get
            {
                GroupByAreaBase groupByArea = (GroupByAreaBase)this.Owner;

                if (groupByArea.IsExpanded)
                    return ExpandCollapseState.Expanded;
                else
                    return ExpandCollapseState.Collapsed;
            }
        }

        #endregion //IExpandCollapseProvider
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