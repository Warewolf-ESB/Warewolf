using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Infragistics.Windows.Reporting.Events
{
    /// <summary>
    /// Event arguments for event <see cref="EmbeddedVisualReportSection"/> pagination events.
    /// </summary>
    /// <seealso cref="EmbeddedVisualReportSection"/>
    /// <seealso cref="EmbeddedVisualReportSection.PaginationStarting"/>
    /// <seealso cref="EmbeddedVisualReportSection.PaginationStarted"/>
    /// <seealso cref="EmbeddedVisualReportSection.PaginationEnded"/>
    public class EmbeddedVisualPaginationEventArgs : EventArgs
    {
        #region Member Variables
        IEmbeddedVisualPaginator _visualPaginator;
        #endregion // Member Variables

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedVisualPaginationEventArgs"/> class 
        /// </summary>
        /// <param name="visualPaginator">The IEmbeddedVisualPaginator used for the pagination</param>
        /// <seealso cref="EmbeddedVisualReportSection"/>
        /// <seealso cref="EmbeddedVisualReportSection.PaginationStarting"/>
        /// <seealso cref="EmbeddedVisualReportSection.PaginationStarted"/>
        /// <seealso cref="EmbeddedVisualReportSection.PaginationEnded"/>
        public EmbeddedVisualPaginationEventArgs(IEmbeddedVisualPaginator visualPaginator)
        {
            this._visualPaginator = visualPaginator;
        }
        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Returns the visual paginator used for the pagination (read-only).
        /// </summary>
        public IEmbeddedVisualPaginator VisualPaginator
        {
            get { return this._visualPaginator; }
        }

        #endregion 
    }

    /// <summary>
    /// Event arguments for event <see cref="Report.PrintProgress"/>
    /// </summary>
    public class PrintProgressEventArgs : CancelEventArgs
    {
        #region Member Variables

        private int _currentPageNumber;
        private string _description;
        private double _percentCompleted;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PrintProgressEventArgs"/> class 
        /// </summary>
        /// <param name="currenPage">Number of page currently being printed</param>
        /// <param name="description">Descriptive information displayed in the progress window when the content of a grid is being printed. </param>
        /// <param name="percentCompleted">Percent of completing of process</param>
        public PrintProgressEventArgs(int currenPage, string description, double percentCompleted)
        {
            this.Cancel = false;
            this._currentPageNumber = currenPage;
            this._description = description;
            this._percentCompleted = percentCompleted;
        }

        #endregion // Constructors

        #region Properties
        /// <summary>
        /// Returns the number of page currently being printed
        /// </summary>
        public int CurrentPageNumber
        {
            get { return _currentPageNumber; }
        }

        /// <summary>
        /// Returns/sets descriptive information displayed in the progress window when the content of a grid is being printed.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Returns a value representing the percentage of the printing process that has been completed
        /// </summary>
        public double PercentCompleted
        {
            get { return _percentCompleted; }
        }

        #endregion

    }

    /// <summary>
    /// Event arguments for event <see cref="Report.PrintEnded"/>
    /// </summary>
    public class PrintEndedEventArgs : EventArgs
    {
        #region Member Variables
        PrintStatus _status;
        PrintCancelationReason _reason;
        int _totalPrintedPages;
        #endregion // Member Variables

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PrintEndedEventArgs"/> class 
        /// </summary>
        /// <param name="status">The status of printing process</param>
        /// <param name="pages">Count of printed pages</param>
        public PrintEndedEventArgs(PrintStatus status, int pages)
        {
            this._totalPrintedPages = pages;
            this._status = status;

            if (status == PrintStatus.Canceled)
                this._reason = PrintCancelationReason.User;
            else
                this._reason = PrintCancelationReason.NotCanceled;
        }

        // JJD 11/10/09 - TFS24546 - added
        /// <summary>
        /// Initializes a new instance of the <see cref="PrintEndedEventArgs"/> class 
        /// </summary>
        /// <param name="status">The status of printing process</param>
        /// <param name="pages">Count of printed pages</param>
        /// <param name="reason">The reason the operation was canceled</param>
        public PrintEndedEventArgs(PrintStatus status, int pages, PrintCancelationReason reason)
        {
            this._totalPrintedPages = pages;
            this._status = status;

            this._reason = reason;
        }
        #endregion // Constructors

        #region Properties
        // JJD 11/10/09 - TFS24546 - added
        /// <summary>
        /// Returns the reason the operation was canceled.
        /// </summary>
        public PrintCancelationReason Reason
        {
            get { return this._reason; }
        }

        /// <summary>
        /// Returns the status of printing.
        /// </summary>
        public PrintStatus Status
        {
            get { return this._status; }
        }
        /// <summary>
        /// Returns the count of printed pages.
        /// </summary>
        public int TotalPrintedPages
        {
            get { return this._totalPrintedPages; }
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