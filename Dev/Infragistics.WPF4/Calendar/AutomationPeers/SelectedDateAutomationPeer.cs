using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Infragistics.Controls.Editors.Primitives;
using Infragistics.Controls.Editors;

namespace Infragistics.AutomationPeers
{
    internal class SelectedDateAutomationPeer: PeerProxy,
        ISelectionItemProvider 
    {
        #region Member Variables

        private CalendarBase _XamCalendar;

        private DateTime _dateTime;

        #endregion //Member Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="SelectedDateAutomationPeer"/>
        /// </summary>
        /// <param name="CalendarBase">An <see cref="CalendarBase"/> instance</param>
        /// <param name="dateTime">An <see cref="DateTime"/> structure</param>
        public SelectedDateAutomationPeer(CalendarBase CalendarBase, DateTime dateTime)
        {
            CalendarUtilities.ValidateNull("CalendarBase", CalendarBase);

            this._XamCalendar = CalendarBase;
            this._dateTime = dateTime;
        }

        #endregion

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
        /// Returns the name of the <see cref="DateTime"/>
        /// </summary>
        /// <returns>A string that contains 'DateTime'</returns>
        protected override string GetClassNameCore()
        {
            return "DateTime";
        }
        #endregion //GetClassNameCore

		#region GetLocalizedControlTypeCore

		/// <summary>
		/// Returns the localized type
		/// </summary>
		/// <returns></returns>
		protected override string GetLocalizedControlTypeCore()
		{
			return CalendarUtilities.GetString("SelectedDate_AutomationType");
		}

		#endregion //GetLocalizedControlTypeCore	
    
        #region GetNameCore

        /// <summary>
        /// Returns the day for the <see cref="DateTime"/> that corresponds with the DateTime struct that is associated with this <see cref="SelectedDateAutomationPeer"/>.
        /// </summary>
        /// <returns></returns>
        protected override string GetNameCore()
        {
            string name = base.GetNameCore();

            if (string.IsNullOrEmpty(name))
            {
                if (this._dateTime != null)
                {
                    name = this._dateTime.ToString("d");
                }
            }

            return name;
        }
        #endregion //GetNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="DateTime"/> that corresponds with this <see cref="SelectedDateAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.SelectionItem)
                return this;

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #region GetUnderlyingPeer
        /// <summary>
        /// Returns the automation peer for which this proxy is associated.
        /// </summary>
        /// <returns>A <see cref="SelectedDateAutomationPeer"/></returns>
        protected override AutomationPeer GetUnderlyingPeer()
        {
            AutomationPeer peer = null;
            if (this._XamCalendar.IsMinCalendarMode)
            {
                CalendarItem item = this._XamCalendar.GetItem(this._dateTime);

                if (null != item)
                    peer = FrameworkElementAutomationPeer.CreatePeerForElement(item);
            }

            return peer;
        }
        #endregion //GetUnderlyingPeer

        #region IsEnabledCore
        /// <summary>
        /// Returns a value indicating whether the <see cref="CalendarItem"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> can receive and send events.
        /// </summary>
        /// <returns><b>True</b> if the <see cref="CalendarItem"/> can send and receive events; otherwise, <b>false</b>.</returns>
        protected override bool IsEnabledCore()
        {
            AutomationPeer peer = this.GetUnderlyingPeer();
            return peer != null
                ? peer.IsEnabled()
                : true;
        }
        #endregion //IsEnabledCore

        #endregion //Base class overrides

        #region ISelectionItemProvider

        void ISelectionItemProvider.AddToSelection()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (this._XamCalendar.SelectedDates.Contains(this._dateTime) == false)
            {
                this._XamCalendar.SelectedDates.Add(this._dateTime);
            }
        }

        bool ISelectionItemProvider.IsSelected
        {
            get 
            {
                return this._XamCalendar.SelectedDates.Contains(this._dateTime); 
            }
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            this._XamCalendar.SelectedDates.Remove(this._dateTime);
        }

        void ISelectionItemProvider.Select()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            this._XamCalendar.SelectedDate = this._dateTime;

        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get
            {
                AutomationPeer peer = FrameworkElementAutomationPeer.CreatePeerForElement(this._XamCalendar);

                return peer != null
                    ? this.ProviderFromPeer(peer)
                    : null;
            }
        }
        #endregion // ISelectionItemProvider
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