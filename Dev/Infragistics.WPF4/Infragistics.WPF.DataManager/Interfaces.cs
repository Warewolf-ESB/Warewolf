using System.Linq;
using System.Collections.Generic;

namespace Infragistics
{

    #region IRule
    /// <summary>
	/// Defines an interface which will participate the data binding so that it can gather data during the
	/// data processing.
	/// </summary>
	public interface IRule
	{
		#region RuleExecution

		/// <summary>
		/// Designates at what stage during the data binding the GatherData needs to be evaluated.
		/// </summary>		
		EvaluationStage RuleExecution { get; }

		#endregion // RuleExecution

		#region GatherData

		/// <summary>
		/// Allows access to the query at the time so that values can be derived off it for the condition.
		/// </summary>
		void GatherData(IQueryable query);

		#endregion // GatherData
	}
    #endregion // IRule

    #region IBindableItem

    /// <summary>
    /// An interface that should be used to describe whether an object was created from a data source or added adhoc. 
    /// </summary>
    public interface IBindableItem
    {
        /// <summary>
        /// Gets/sets a value that determines if the object was created from a data source or adhoc. 
        /// </summary>
        bool IsDataBound
        {
            get;
            set;
        }
    }

    #endregion // IBindableItem

    #region IProvideDataItems

    /// <summary>
    /// An interface that describes a collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of object that is being provided.</typeparam>
    public interface IProvideDataItems<T>
    {
        /// <summary>
        /// Gets the amount of objects in the collection.
        /// </summary>
        int DataCount
        {
            get;
        }

        /// <summary>
        /// Resolves the item at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T GetDataItem(int index);

        /// <summary>
        /// Creates a new object with a default underlying data object.
        /// </summary>
        /// <returns></returns>
        T CreateItem();

        /// <summary>
        /// Creates a new object using the inputted data object.
        /// </summary>
        /// <param name="dataItem"></param>
        /// <returns></returns>
        T CreateItem(object dataItem);

        /// <summary>
        /// Adds a new object to the collection
        /// </summary>
        /// <param name="addedObject"></param>
        void AddItem(T addedObject);

        /// <summary>
        /// Removes an object from the collection
        /// </summary>
        /// <param name="removedObject"></param>
        /// <returns></returns>
        bool RemoveItem(T removedObject);

        /// <summary>
        /// Removes the specified range of items from the collection.
        /// </summary>
        /// <param name="itemsToRemove"></param>
        void RemoveRange(IList<T> itemsToRemove);

        /// <summary>
        /// Adds an item to the collection at a given index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="insertedObject"></param>
        void InsertItem(int index, T insertedObject);
    }

    #endregion // IProvideDataItems

    #region IFilteredCollectionView

    /// <summary>
    /// An interface which describes how to provide filtering information to a datasource.
    /// </summary>
    public interface IFilteredCollectionView
    {
        /// <summary>
        /// Whether filtering is supported on a particular datasource.
        /// </summary>
        bool CanFilter { get; }

        /// <summary>
        /// A collection of FilterConditions that should be applied to the datasource.
        /// </summary>
        RecordFilterCollection FilterConditions { get; }
    }

    #endregion // IFilteredCollectionView

    #region ISupportLinqSummaries
    /// <summary>
    /// An interface used to designate a summary that can use the LINQ summary structure.
    /// </summary>
    internal interface ISupportLinqSummaries
    {
        /// <summary>
        /// Gets the <see cref="LinqSummaryOperator"/> which designates which LINQ summary to use.
        /// </summary>
        LinqSummaryOperator SummaryType { get; }

        /// <summary>
        /// Gets / sets the <see cref="SummaryContext"/> that will be used by the summary framework to build the summary.
        /// </summary>
        SummaryContext SummaryContext { get; set; }
    }
    #endregion // ISupportLinqSummaries

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