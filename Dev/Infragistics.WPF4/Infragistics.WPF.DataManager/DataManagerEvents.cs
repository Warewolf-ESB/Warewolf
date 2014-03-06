using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace Infragistics
{

    #region HandleableEventArgs
    /// <summary>
    /// Event argument for events in which the user wants to mark the event handled.
    /// </summary>
    public class HandleableEventArgs : EventArgs
    {
        /// <summary>
        /// Gets / sets if the event is to be considered handled.
        /// </summary>
        public bool Handled { get; set; }
    }
    #endregion // HandleableEventArgs

    #region HandleableObjectGenerationEventArgs
    /// <summary>
    /// Event argument used when the DataManager has a request for a new data object.
    /// </summary>
    public class HandleableObjectGenerationEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// Gets / sets the instance of the object that will be used by the DataManager rather then attempting to create a new instance of the object using the default constructor.
        /// </summary>
        public object NewObject { get; set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of object that the DataManager is handling. 
        /// </summary>
        public Type ObjectType { get; protected internal set; }

        /// <summary>
        /// Gets the <see cref="Type"/> which is contained in the underlying collection.
        /// </summary>
        public Type CollectionType { get; protected internal set; }
    }
    #endregion // HandleableObjectGenerationEventArgs

    #region DataAcquisitionEventArgs

    /// <summary>
    /// Event argument used when the DataManager is processing its data 
    /// </summary>
    public class DataAcquisitionEventArgs : HandleableEventArgs
    {
        #region DataSource
        /// <summary>
        /// Gets / sets the IList that will be applied to the data manager.
        /// </summary>
        public IList DataSource { get; set; }
        #endregion // DataSource

        #region EnablePaging
        /// <summary>
        /// Gets if the DataManager expects paged data.
        /// </summary>
        public bool EnablePaging { get; protected internal set; }
        #endregion // EnablePaging

        #region PageSize
        /// <summary>
        /// Gets the maximum number of rows expected by the DataManager.  		
        /// </summary>
        /// <remarks>
        /// Used primarily when EnablePaging is true.
        /// </remarks>
        public int PageSize { get; protected internal set; }
        #endregion // PageSize

        #region CurrentPage
        /// <summary>
        /// Gets the current page index
        /// </summary>
        public int CurrentPage { get; protected internal set; }
        #endregion // CurrentPage

        #region CurrentSort
        /// <summary>
        /// Gets a collection <see cref="SortContext"/> which will be applied.
        /// </summary>
        public ObservableCollection<SortContext> CurrentSort { get; protected internal set; }
        #endregion // CurrentSort

        #region GroupByContext
        /// <summary>
        /// Gets the GroupBy that will be applied to the data.
        /// </summary>
        public GroupByContext GroupByContext { get; protected internal set; }
        #endregion // GroupByContext

        #region Filters
        /// <summary>
        /// Gets a collection that lists what filters will be applied.
        /// </summary>
        public RecordFilterCollection Filters { get; protected internal set; }
        #endregion // Filters
    }

    #endregion // DataAcquisitionEventArgs

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