using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;


using Infragistics.Controls.Editors.Primitives;
using Infragistics.Controls.Editors;
using Infragistics.Controls.Primitives;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes the <see cref="CalendarItemGroup"/> to UI Automation
    /// </summary>
    public class CalendarItemGroupAutomationPeer : FrameworkElementAutomationPeer, 
        ISelectionProvider
    {
        #region member Variables
        private CalendarBase _XamCalendar;
        #endregion //Member Variables

        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="CalendarItemGroup"/> class
        /// </summary>
        /// <param name="owner">The <see cref="CalendarItemGroup"/> for which the peer is being created</param>
        public CalendarItemGroupAutomationPeer(CalendarItemGroup owner)
            : base(owner)
        {
            this._XamCalendar = CalendarBase.GetCalendar(owner);
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
        /// Returns the name of the <see cref="CalendarItemGroup"/>
        /// </summary>
        /// <returns>A string that contains 'CalendarItemGroup'</returns>
        protected override string GetClassNameCore()
        {
            return "CalendarItemGroup";
        }

        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="CalendarItemGroup"/> that corresponds with this <see cref="CalendarItemGroupAutomationPeer"/>.
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

        bool ISelectionProvider.CanSelectMultiple
        {
            get
            {
                return SelectionStrategyBase.IsMultiSelectStrategy(this._XamCalendar.CurrentSelectionTypeResolved);

            }
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            CalendarItemGroup calendarItemGroup = this.Owner as CalendarItemGroup;
            if (calendarItemGroup.Items != null)
            {
                List<IRawElementProviderSimple> selectedItemPeers = new List<IRawElementProviderSimple>();

                
                for (int i = 0; i < calendarItemGroup.Items.Count; i++)
                {
                    if (calendarItemGroup.Items[i].IsSelected == true)
                    {
                        CalendarItem calendarItem = calendarItemGroup.Items[i]; 
                        AutomationPeer peer = FrameworkElementAutomationPeer.CreatePeerForElement(calendarItem);
                        selectedItemPeers.Add(this.ProviderFromPeer(peer));
                    }
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