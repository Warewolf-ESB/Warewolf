using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;



using Infragistics.Controls.Editors.Primitives;
using Infragistics.Controls.Editors;
using Infragistics.Collections;
using Infragistics.Controls.Primitives;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="CalendarBase"/> types to UI Automation
    /// </summary>
    public class CalendarBaseAutomationPeer : FrameworkElementAutomationPeer,
        ISelectionProvider, IMultipleViewProvider
    {
        #region member Variables
        #endregion //Member Variables

        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="CalendarBaseAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="CalendarBase"/> for which the peer is being created</param>
        public CalendarBaseAutomationPeer(CalendarBase owner)
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
            return AutomationControlType.Calendar;
        }

        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="CalendarBase"/>
        /// </summary>
        /// <returns>A string that contains 'XamOutlookBar'</returns>
        protected override string GetClassNameCore()
        {
            return "CalendarBase";
        }

        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="CalendarBase"/> that corresponds with this <see cref="CalendarBaseAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if ( patternInterface == PatternInterface.Selection)
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
                CalendarBase CalendarBase = this.Owner as CalendarBase;
                return SelectionStrategyBase.IsMultiSelectStrategy(CalendarBase.CurrentSelectionTypeResolved);
            }
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            CalendarBase CalendarBase = this.Owner as CalendarBase;
            DateCollection selectedItems = CalendarBase.SelectedDates;

            if (selectedItems != null)
            {
                List<IRawElementProviderSimple> selectedItemPeers = new List<IRawElementProviderSimple>(selectedItems.Count);

                
                for (int i = 0; i < selectedItems.Count; i++)
                {
                    DateTime itemDate = selectedItems[i];
                    SelectedDateAutomationPeer calendarItem = new SelectedDateAutomationPeer(CalendarBase, itemDate);
                    selectedItemPeers[i] = this.ProviderFromPeer(calendarItem);
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

        #region IMultipleViewProvider

        private static CalendarZoomMode GetMode(int viewId)
        {
            CalendarZoomMode mode = (CalendarZoomMode)viewId;
            if (false == Enum.IsDefined(typeof(CalendarZoomMode), mode))
                throw new ArgumentException(CalendarBase.GetString("LE_InvalidViewId"));
            return mode;
        }

        int[] IMultipleViewProvider.GetSupportedViews()
        {
            CalendarBase cal = (CalendarBase)this.Owner;
            List<int> views = new List<int>();
            foreach (CalendarZoomMode mode in CalendarUtilities.GetEnumValues<CalendarZoomMode>())
            {
                if (mode >= cal.MinCalendarModeResolved && mode <= cal.MaxCalendarMode)
                    views.Add((int)mode);
            }

            return views.ToArray();

        }

        string IMultipleViewProvider.GetViewName(int viewId)
        {
            return GetMode(viewId).ToString();
        }

        void IMultipleViewProvider.SetCurrentView(int viewId)
        {
            CalendarBase cal = (CalendarBase)this.Owner;
            CalendarZoomMode mode = GetMode(viewId);

            if (mode < cal.MinCalendarModeResolved)
				throw new InvalidOperationException(CalendarBase.GetString("LE_CannotSetMode", mode));

            if (mode > cal.MaxCalendarMode)
                throw new InvalidOperationException();

            cal.CurrentMode = mode;
        }

        int IMultipleViewProvider.CurrentView
        {
            get
            {
                CalendarBase cal = (CalendarBase)this.Owner;
                return (int)cal.CurrentMode;
            }
        }
        #endregion //IMultipleViewProvider
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