using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Infragistics
{
    // Summary:
    //     Defines methods and properties that a collection view implements to provide
    //     paging capabilities to a collection.
    internal interface IPagedCollectionView
    {
        // Summary:
        //     Gets a value that indicates whether the System.ComponentModel.IPagedCollectionView.PageIndex
        //     value can change.
        //
        // Returns:
        //     true if the System.ComponentModel.IPagedCollectionView.PageIndex value can
        //     change; otherwise, false.
        bool CanChangePage { get; }
        //
        // Summary:
        //     Gets a value that indicates whether the page index is changing.
        //
        // Returns:
        //     true if the page index is changing; otherwise, false.
        bool IsPageChanging { get; }
        //
        // Summary:
        //     Gets the number of known items in the view before paging is applied.
        //
        // Returns:
        //     The number of known items in the view before paging is applied.
        int ItemCount { get; }
        //
        // Summary:
        //     Gets the zero-based index of the current page.
        //
        // Returns:
        //     The zero-based index of the current page.
        int PageIndex { get; }
        //
        // Summary:
        //     Gets or sets the number of items to display on a page.
        //
        // Returns:
        //     The number of items to display on a page.
        int PageSize { get; set; }
        //
        // Summary:
        //     Gets the total number of items in the view before paging is applied.
        //
        // Returns:
        //     The total number of items in the view before paging is applied, or -1 if
        //     the total number is unknown.
        int TotalItemCount { get; }

        // Summary:
        //     When implementing this interface, raise this event after the System.ComponentModel.IPagedCollectionView.PageIndex
        //     has changed.
        event EventHandler<EventArgs> PageChanged;
        //
        // Summary:
        //     When implementing this interface, raise this event before changing the System.ComponentModel.IPagedCollectionView.PageIndex.
        //     The event handler can cancel this event.
        event EventHandler<PageChangingEventArgs> PageChanging;

        // Summary:
        //     Sets the first page as the current page.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToFirstPage();
        //
        // Summary:
        //     Sets the last page as the current page.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToLastPage();
        //
        // Summary:
        //     Moves to the page after the current page.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToNextPage();
        //
        // Summary:
        //     Moves to the page at the specified index.
        //
        // Parameters:
        //   pageIndex:
        //     The index of the page to move to.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToPage(int pageIndex);
        //
        // Summary:
        //     Moves to the page before the current page.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToPreviousPage();
    }

    // Summary:
    //     Provides data for the System.ComponentModel.IPagedCollectionView.PageChanging
    //     event.
    internal class PageChangingEventArgs : CancelEventArgs
    {
        // Summary:
        //     Initializes a new instance of the System.ComponentModel.PageChangingEventArgs
        //     class.
        //
        // Parameters:
        //   newPageIndex:
        //     The index of the requested page.
        public PageChangingEventArgs(int newPageIndex)
        {
        }

        // Summary:
        //     Gets the index of the requested page.
        //
        // Returns:
        //     The index of the requested page.
        public int NewPageIndex
        {
            get
            {
                return -1;
            }
        }
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