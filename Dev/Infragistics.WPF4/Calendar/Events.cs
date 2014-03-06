using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// Provides information for the <see cref="CalendarBase.SelectedDatesChanged"/> event.
    /// </summary>
    /// <seealso cref="CalendarBase"/>
    /// <seealso cref="CalendarBase.SelectedDate"/>
    /// <seealso cref="CalendarBase.SelectedDates"/>
    /// <seealso cref="CalendarBase.SelectedDatesChanged"/>
    public class SelectedDatesChangedEventArgs : EventArgs
    {
        #region Member Variables

        private DateTime[] _removedDates;
        private DateTime[] _addedDates;

        #endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="SelectedDatesChangedEventArgs"/>
        /// </summary>
        /// <param name="removedDates">A list of the dates that were unselected</param>
        /// <param name="addedDates">A list of the dates that were selected</param>
        public SelectedDatesChangedEventArgs(IList<DateTime> removedDates, IList<DateTime> addedDates)
        {
            CalendarUtilities.ValidateNull("removedDates", removedDates);
            CalendarUtilities.ValidateNull("addedDates", addedDates);

            this._removedDates = new DateTime[removedDates.Count];
            removedDates.CopyTo(this._removedDates, 0);
            this._addedDates = new DateTime[addedDates.Count];
            addedDates.CopyTo(this._addedDates, 0);
        } 
        #endregion //Constructor

        #region Properties
        /// <summary>
        /// Returns a list of the dates that were unselected.
        /// </summary>
        public IList<DateTime> RemovedDates
        {
            get { return this._removedDates; }
        }

        /// <summary>
        /// Returns a list of the dates that were selected.
        /// </summary>
        public IList<DateTime> AddedDates
        {
            get { return this._addedDates; }
        } 
        #endregion //Properties
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