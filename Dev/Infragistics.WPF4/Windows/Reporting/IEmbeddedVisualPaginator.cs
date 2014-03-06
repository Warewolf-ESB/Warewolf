using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Reporting 
{
	/// <summary>
	/// Interface implemented by embedded visual elements within a <see cref="ReportSection"/> that need to alter 
    /// their visual tree in response to pagination in the containing <see cref="ReportSection"/>.
	/// </summary>
    /// <remarks>
    /// <para class="note"><b>Note:</b> This interface, or the <see cref="IEmbeddedVisualPaginatorFactory"/> interface, is implemented by controls that may represent multiple pages of data, e.g. XamDateGrid. The interface is used 
    /// by the EmbeddedVisualReportSection, found in the Infragistic.Wpf.Reporting assembly, to generate all of the appropriate pages.</para>
    /// </remarks>
    /// <seealso cref="ReportSection"/>
    /// <seealso cref="IEmbeddedVisualPaginatorFactory"/>
    public interface IEmbeddedVisualPaginator
    {
		#region Properties

		/// <summary>
		/// Returns a piece of opaque data that will be set as the DataContext on the root visual of a Report's current page.
		/// </summary>
        /// <value>The current page's data context or null if there is no data context for the current page.</value>
		object CurrentPageDataContext
		{
			get;
		}

		/// <summary>
		/// Returns a PagePosition instance that represents the current page position in the embedded visual (read-only).
		/// </summary>
        /// <seealso cref="PagePosition"/>
		PagePosition CurrentPagePosition
		{
			get;
		}

        /// <summary>
        /// Returns the estimated count of page that will be printed (read-only).
        /// </summary>
        int EstimatedPageCount
        {
            get;
        }

        /// <summary>
        /// Returns an integer that indicates the logical row of the current page (read-only).
        /// </summary>
        int LogicalPageNumber
        {
            get;
        }

        #region LogicalPagePartNumber
        /// <summary>
        /// Returns an integer that indicates the logical column of the current page(read-only).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> when the <see cref="ReportSettings"/> <see cref="ReportSettings.HorizontalPaginationMode"/> is set to 'Scale', this property will return 1.</para></remarks>
        int LogicalPagePartNumber
        {
            get;
        }

        #endregion //LogicalPagePartNumber

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Called when a pagination process is about to begin.
		/// </summary>
        /// <param name="section">The section of the report.</param>
		void BeginPagination(ReportSection section);

		/// <summary>
		/// Called when the pagination has ended.
		/// </summary>
		void EndPagination();

		/// <summary>
		/// Requests that the implementor move to the next page and update its visual tree if necessary..
		/// </summary>
        /// <returns>True if the implementor was able to move to the next page, or false if there was no next page.</returns>
        bool MoveToNextPage();

		/// <summary>
		/// Requests that the implementor move to a specific page represented by the supplied <see cref="PagePosition"/> and update its visual tree if necessary.
		/// </summary>
		/// <param name="pagePosition">A <see cref="PagePosition"/> instance that represents the page to move to.</param>
		/// <returns>True if the implementor was able to move to the requested page, or false if the move failed.</returns>
		bool MoveToPosition(PagePosition pagePosition);

		#endregion //Methods
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