using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Windows.Data;

namespace Infragistics
{
    #region MergedDataContext

    /// <summary>
    /// An object that encapsulates the Merged Data functionality used by the <see cref="DataManagerBase"/>
    /// </summary>
    public abstract class MergedDataContext
    {





        #region Properties

        #region Public 

        #region PropertyName
        /// <summary>
        /// Gets the name of the property that data should be merged by.
        /// </summary>
        public string PropertyName
        {
            get;
            protected set;
        }
        #endregion // PropertyName

        #region SortContext

        /// <summary>
        /// Gets/Sets the CurrentSort that will be applied when the data is merged by a particular field.
        /// </summary>
        public SortContext SortContext
        {
            get;
            set;
        }

        #endregion // SortContext

        #region SortAscending

        /// <summary>
        /// Gets/Sets the sort direction that should be applied to the field that the underlying data has been merged by.
        /// </summary>
        public bool SortAscending
        {
            get;
            set;
        }

        #endregion // SortAscending

        #region MergedObject

        /// <summary>
        /// The Object the merged operation is performed on.
        /// </summary>
        public object MergedObject
        {
            get;
            set;
        }

        #endregion // MergedObject

        #endregion // Public

        #endregion // Properties

        #region Methods

        #region Static

        #region CreateGenericCustomMerge

        /// <summary>
        /// Creates a Generic instance of the <see cref="MergedDataContext"/>.
        /// </summary>
        /// <param name="cti">The type of data to create the MergedDataContext from.</param>
        /// <param name="propertyName">The name of the property, that the data should be merged by.</param>
        /// <param name="comparer">The IEqualityComparer that will be used to perform the mergin</param>
        /// <returns></returns>
        public static MergedDataContext CreateGenericCustomMerge(CachedTypedInfo cti, string propertyName, object comparer)
        {
            Type type = cti.CachedType;
            Type customGroupType = typeof(MergedDataContext<,>).MakeGenericType(new System.Type[] {type,  DataManagerBase.ResolvePropertyTypeFromPropertyName(propertyName, cti) });
            return (MergedDataContext)Activator.CreateInstance(customGroupType, new object[] { propertyName, comparer, cti });
        }

        /// <summary>
        /// Creates a Generic instance of the <see cref="MergedDataContext"/>.
        /// </summary>
        /// <param name="cti">The type of data to create the MergedDataContext from.</param>
        /// <param name="comparer" ></param>
        /// <param name="converter" ></param>
        /// <param name="converterParam"></param>
        /// <returns></returns>
        public static MergedDataContext CreateGenericCustomMerge(CachedTypedInfo cti, object comparer, IValueConverter converter, object converterParam)
        {
            Type type = cti.CachedType;
            Type customGroupType = typeof(MergedDataContext<,>).MakeGenericType(new System.Type[] { type, type });
            return (MergedDataContext)Activator.CreateInstance(customGroupType, new object[] { comparer, converter, converterParam, cti });
        }

        #endregion // CreateGenericCustomMerge

        #endregion // Static

        #region Abstract

        #region Merge

        /// <summary>
        /// Merges the specified <see cref="IQueryable"/> by the property this data represents.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="mdcs"></param>
        /// <param name="summaries"></param>
        /// <returns></returns>
        public abstract IList Merge(IQueryable query, List<MergedDataContext> mdcs, SummaryDefinitionCollection summaries);

        /// <summary>
        /// Merges the specified <see cref="IQueryable"/> by the property this data represents.
        /// </summary>
        /// <param name="q"></param>
        /// <param name="mdcs"></param>
        /// <param name="method"></param>
        /// <param name="parentMCI"></param>
        /// <returns></returns>
        protected internal abstract IList Merge(IQueryable q, List<MergedDataContext> mdcs, MergeDelegate<object, IEnumerable, MergedDataContext, object, MergedColumnInfo, object> method, MergedColumnInfo parentMCI);

        #endregion // Merge

        #endregion // Abstract

        #endregion // Methods
    }
    #endregion // MergedDataContext

    #region MergedDataContext<T, TColumnType>

    /// <summary>
    /// An object that encapsulates the Merging functionality used by the <see cref="DataManagerBase"/>
    /// </summary>
    /// <typeparam name="T">The data object that the field belongs to.</typeparam>
    /// <typeparam name="TColumnType">The type of data that should be merged by.</typeparam>
    public class MergedDataContext<T, TColumnType> : MergedDataContext
    {
        #region Members

        private bool _unboundColumn;
        MergedDataContext _nextMergeDataContext = null;
        List<MergedDataContext> _mdcs;
        Dictionary<T, MergedRowInfo> _rowsDictionary = new Dictionary<T, MergedRowInfo>();
        List<MergedRowInfo> _rows = new List<MergedRowInfo>();

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="MergedDataContext"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="comparer"></param>
        /// <param name="typedInfo"></param>
        public MergedDataContext(String propertyName, object comparer, CachedTypedInfo typedInfo)
        {
            this.PropertyName = propertyName;
            this.Comparer = comparer as IEqualityComparer<TColumnType>;
            this._unboundColumn = false;
            this.TypedInfo = typedInfo;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MergedDataContext"/>.
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="converter"></param>
        /// <param name="converterParam"></param>
        /// <param name="typedInfo"></param>
        public MergedDataContext(object comparer, IValueConverter converter, object converterParam, CachedTypedInfo typedInfo)
        {
            this._unboundColumn = true;
            this.Comparer = comparer as IEqualityComparer<TColumnType>;
            this.Converter = converter as IValueConverter;
            this.ConverterParameter = converterParam;
            this.TypedInfo = typedInfo;
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region Comparer
        /// <summary>
        /// Gets the Comparer that will be used to perform the merging.
        /// </summary>
        public IEqualityComparer<TColumnType> Comparer
        {
            get;
            protected set;
        }
        #endregion // Comparer

        #endregion // Public

        #region Protected

        #region Converter
        /// <summary>
        /// Gets the <see cref="IValueConverter"/> which will be used to evaluate the merge.
        /// </summary>
        protected IValueConverter Converter
        {
            get;
            private set;
        }
        #endregion // Converter

        #region ConverterParameter
        /// <summary>
        /// The parameter applied to the <see cref="Converter"/>.
        /// </summary>
        protected object ConverterParameter
        {
            get;
            private set;
        }
        #endregion // ConverterParameter

        #region Summaries

        /// <summary>
        /// A collection of summaries that should be applied to each subset of merged groupings.
        /// </summary>
        protected SummaryDefinitionCollection Summaries
        {
            get;
            set;
        }
        #endregion // Summaries

        #region TypedInfo

        /// <summary>
        /// Gets/Sets the TypedInfo for the object.
        /// </summary>
        protected CachedTypedInfo TypedInfo
        {
            get;
            set;
        }

        #endregion // TypedInfo

        #endregion // Protected

        #endregion // Properties

        #region Overrides

        #region Merge

        /// <summary>
        /// Merges the specified <see cref="IQueryable"/> by the property this data represents.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="mdcs"></param>
        /// <param name="summaries"></param>
        /// <returns></returns>
        public override IList Merge(IQueryable query, List<MergedDataContext> mdcs, SummaryDefinitionCollection summaries)
        {
            // this Merge method is only invoked by the DataManager, so the root level of the merging hierachy. 
            // So we need to invalidate all previous calls, by clearing out our rows collections.
            this._rows.Clear();
            this._rowsDictionary.Clear();

            this.Summaries = summaries;
            return this.Merge(query, mdcs, new MergeDelegate<object, IEnumerable, MergedDataContext, object, MergedColumnInfo, object>(this.OnGrouping), null);
        }

       /// <summary>
       /// Merges the specified <see cref="IQueryable"/> by the property this data represents.
       /// </summary>
       /// <param name="iquery"></param>
       /// <param name="mdcs"></param>
       /// <param name="method"></param>
       /// <param name="parentMCI"></param>
       /// <returns></returns>
        protected internal override IList Merge(IQueryable iquery, List<MergedDataContext> mdcs, MergeDelegate<object, IEnumerable, MergedDataContext, object, MergedColumnInfo, object> method, MergedColumnInfo parentMCI)
        {
            IQueryable<T> query = iquery as IQueryable<T>;

            if (query != null)
            {
                this._mdcs = mdcs;

                // Find out where in the hierarchy of MergedDataContext we fall into, as this method is called recursively.
                int index = mdcs.IndexOf(this);

                // Then determine if we have a child MDC, and sset it has our next one.
                if (index != mdcs.Count - 1)
                    this._nextMergeDataContext = mdcs[index + 1];
                else
                    this._nextMergeDataContext = null;

                
                // Build the expression similarly to what we do for GroupBy.
                ParameterExpression parameterexpression = Expression.Parameter(typeof(T), "param");
                Expression body = null;
                
                if (this._unboundColumn)
                {
                    body = parameterexpression;
                }
                else
                {
                    body = DataManagerBase.BuildPropertyExpressionFromPropertyName(this.PropertyName, parameterexpression, this.TypedInfo, typeof(TColumnType), default(TColumnType));
                }

                if (!typeof(T).IsValueType)
                {
                    Expression equalExpression = Expression.Equal(parameterexpression, Expression.Constant(null, typeof(T)));
                    System.Linq.Expressions.Expression constExpression = System.Linq.Expressions.Expression.Constant(default(TColumnType), typeof(TColumnType));
                    body = Expression.Condition(equalExpression, constExpression, body);
                }

                Expression<Func<T, TColumnType>> expr = Expression.Lambda(body, parameterexpression) as Expression<Func<T, TColumnType>>;

                IEqualityComparer<TColumnType> comparer = this.Comparer;
                if (comparer == null && this._unboundColumn)
                {
                    comparer = new ValObjComparer<TColumnType>(this.Converter, this.ConverterParameter);
                }

                // Now, b/c we invoke a method for each Group that we're grouping by, we need to convert the query to an Enumerable first
                // We call a method, so that we can flatten out the hierachy that would otherwise be built by calling nested GroupBy operations
                // The method we're Invoking is "OnGrouping"
                var mergedData = query.AsEnumerable().GroupBy(expr.Compile(), (key, list) => method(key, list, this._nextMergeDataContext, this.MergedObject, parentMCI), comparer);

                // Execute the query. 



                mergedData.ToList();

            }


            return _rows;
        }

        #endregion // Merge

        #endregion // Overrides

        #region Methods

        #region Public

        #region OnGrouping

        /// <summary>
        /// Invoked by the GroupBy query, this will recurse through all MergeDataContexts and Invoke their merge method.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="nextMergeDataContext"></param>
        /// <param name="mergedObject"></param>
        /// <param name="parentMCI"></param>
        /// <returns></returns>
        public object OnGrouping(object key, IEnumerable list, MergedDataContext nextMergeDataContext, object mergedObject, MergedColumnInfo parentMCI)
        {
            IEnumerable<T> dataList = list as IEnumerable<T>;

            // Create a MergedColumnInfo, and fill it with all the information it will need to be properly consumed
            MergedColumnInfo mci = new MergedColumnInfo() 
            { 
                Key = key, 
                Children = dataList.ToList(), 
                DataType= typeof(T), 
                Summaries = this.Summaries, 
                ParentMergedColumnInfo = parentMCI,
                MergingObject = mergedObject,
                TypedInfo = this.TypedInfo
                
            };

            int count = dataList.Count() - 1;

            // Now, create another query that will build out MergedRowInfo objects and add the MergedColumnInfo we just created to it
            // This method will add each Row into a collection, so that we'll have a flattened list, which is why we call 
            // ToList at the end of it, so that its invoked immediately.
            dataList.Select((comic, index) => this.AppendColumnInfoToRowInfo(comic, mci, index, count, mergedObject)).ToList();

            // if we have another MergedDataContext, invoke it, but pass in this specific OnGrouping method
            // This is important so that we add all the rows, to this specific  collection.
            if (nextMergeDataContext != null)
                nextMergeDataContext.Merge(list.AsQueryable(), this._mdcs, new MergeDelegate<object, IEnumerable, MergedDataContext, object, MergedColumnInfo, object>(this.OnGrouping), mci);

            return mci;
        }

        #endregion // OnGrouping

        #region AppendColumnInfoToRowInfo

        /// <summary>
        /// Takes the given MergedColumnInfo, and determines if needs to create a new MergedRowInfo or use and existing one, then appends itself to that RowInfo
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mci"></param>
        /// <param name="index"></param>
        /// <param name="lastIndex"></param>
        /// <param name="mergedObject"></param>
        /// <returns></returns>
        public MergedRowInfo AppendColumnInfoToRowInfo(T data, MergedColumnInfo mci, int index, int lastIndex, object mergedObject)
        {
            MergedRowInfo row = null;

            // See if we've already careated an instance of this MergedRowInfo for this specific piece of data. 
            if (_rowsDictionary.ContainsKey(data))
            {
                row = _rowsDictionary[data];
            }
            else
            {
                // If we haven, then create a new one
                row = new MergedRowInfo() { Data = data, MergedGroups = new List<MergedColumnInfo>() };

                // Add an entry so it can be looked up later
                this._rowsDictionary.Add(data, row);

                // This is the master collectiont hat will be returned by the Merge call, so add the row, to our flattened list.
                this._rows.Add(row);
            }

            // For later consumption, we need to know if the row is the last one in the grouping, so do the check here.
            if (index == lastIndex)
            {
                if (index == 0)
                {
                    row.IsLastRowInGroup.Add(mergedObject, true);
                    row.IsFirstRowInGroup.Add(mergedObject, true);
                }
                else
                {
                    row.IsLastRowInGroup.Add(mergedObject, true);
                    row.IsFirstRowInGroup.Add(mergedObject, false);
                }
            }
            else if (index == 0)
            {
                row.IsFirstRowInGroup.Add(mergedObject, true);
                row.IsLastRowInGroup.Add(mergedObject, false);
            }
            else
            {
                row.IsFirstRowInGroup.Add(mergedObject, false);
                row.IsLastRowInGroup.Add(mergedObject, false);
            }

            row.MergedGroups.Add(mci);

            return row;
        }

        #endregion // AppendColumnInfoToRowInfo

        #endregion // Public

        #endregion // Methods
    }
    #endregion // MergedDataContext<T, TColumnType>

    #region MergedRowInfo
    
    /// <summary>
    /// An object that stores the merge information for a particualr data row from an items source.
    /// </summary>
    public class MergedRowInfo 
    {
        #region Members

        Dictionary<object, bool> _lastRowInGroup = new Dictionary<object, bool>();
        Dictionary<object, bool> _firstRowInGroup = new Dictionary<object, bool>();

        #endregion // Members

        #region MergedGroups

        /// <summary>
        /// A collection of MergeColumnInfo that this row has been merged by.
        /// </summary>
        public List<MergedColumnInfo> MergedGroups
        {
            get;
            set;
        }

        #endregion // MergedGroups

        #region Data

        /// <summary>
        /// The underlying data object that this row object represents
        /// </summary>
        public object Data
        {
            get;
            set;
        }

        #endregion // Data

        #region IsLastRowInGroup

        /// <summary>
        /// A lookup table for whether a specific key of a MergedColumnInfo, that this row is the last one in its grouping.
        /// </summary>
        public Dictionary<object, bool> IsLastRowInGroup
        {
            get { return this._lastRowInGroup; }
        }

        #endregion // IsLastRowInGroup

        #region IsFirstRowInGroup

        /// <summary>
        /// A lookup table for whether a specific key of a MergedColumnInfo, that this row is the first one in its grouping.
        /// </summary>
        public Dictionary<object, bool> IsFirstRowInGroup
        {
            get { return this._firstRowInGroup; }
        }

        #endregion // IsFirstRowInGroup

        #region Equals

        /// <summary>
        /// Does an equals comparison on the underlying data, not the MergedRowInfo object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            MergedRowInfo mri = obj as MergedRowInfo;
            if (this.Data != null && mri != null)
            {
                return this.Data.Equals(mri.Data);
            }

            return base.Equals(obj);
        }

        #endregion // Equals

        #region GetHashCode

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (this.Data != null)
            {
                return this.Data.GetHashCode();
            }

            return base.GetHashCode();
        }

        #endregion // GetHashCode
    }

    #endregion // MergedRowInfo

    #region MergedColumnInfo

    /// <summary>
    /// An object that represents a particular field that the data has been merged by. 
    /// </summary>
    public class MergedColumnInfo
    {
        #region Members

        SummaryResultCollection _summaryResultCollection;

        #endregion // Members

        #region Propeties

        #region Public

        #region Key

        /// <summary>
        /// Gets the unique key that this particualr field grouping represents.
        /// </summary>
        public object Key
        {
            get;
            internal set;
        }

        #endregion // Key

        #region Children

        /// <summary>
        /// Gets the collection of child rows that belong to this grouping, who all share the same value for the field as the Key
        /// </summary>
        public IList Children
        {
            get;
            internal set;
        }
        #endregion // Children

        #region ParentMergedColumnInfo

        /// <summary>
        /// The MergedColumnInfo who this grouping falls under, null if its the root. 
        /// </summary>
        public MergedColumnInfo ParentMergedColumnInfo
        {
            get;
            internal set;
        }

        #endregion // ParentMergedColumnInfo

        #region DataType

        /// <summary>
        /// Gets the Type of the underling data row.
        /// </summary>
        public Type DataType
        {
            get;
            internal set;
        }
        #endregion // DataType

        #region Summaries

        /// <summary>
        /// Gets a list of the summaries that should be applied to Children.
        /// </summary>
        public SummaryDefinitionCollection Summaries
        {
            get;
            internal set;
        }
        #endregion // Summaries

        #region SummaryResultCollection

        /// <summary>
        /// When this collection is acccessed it will lazily perform the summaries for this particular MergedColumnInfo based on the children.
        /// </summary>
        public SummaryResultCollection SummaryResultCollection
        {
            get
            {
                if (this._summaryResultCollection == null)
                {
                    if (this.Summaries.Count > 0)
                    {
                        this._summaryResultCollection = new SummaryResultCollection();

                        IQueryable query = this.Children.AsQueryable();
                        foreach (SummaryDefinition sd in this.Summaries)
                        {
                            ISupportLinqSummaries isls = sd.SummaryOperand.SummaryCalculator as ISupportLinqSummaries;
                            if (isls != null)
                            {
                                SummaryContext sc = SummaryContext.CreateGenericSummary(this.TypedInfo, sd.ColumnKey, isls.SummaryType);
                                isls.SummaryContext = sc;
                                this._summaryResultCollection.Add(new SummaryResult(sd, sc.Execute(query)));
                                continue;
                            }
                            SynchronousSummaryCalculator ssc = sd.SummaryOperand.SummaryCalculator as SynchronousSummaryCalculator;
                            if (ssc != null)
                            {
                                this._summaryResultCollection.Add(new SummaryResult(sd, ssc.Summarize(query, sd.ColumnKey)));
                            }
                        }
                    }
                    else
                        return new SummaryResultCollection();

                }
                return this._summaryResultCollection;
            }
        }
        #endregion // SummaryResultCollection

        #region MergingObject

        /// <summary>
        /// The object used to create this merging.
        /// </summary>
        public object MergingObject
        {
            get;
            set;
        }

        #endregion // MergingObject

        #region TypedInfo

        /// <summary>
        /// Gets/Sets the TypedInfo for the object.
        /// </summary>
        public CachedTypedInfo TypedInfo
        {
            get;
            set;
        }

        #endregion // TypedInfo

        #endregion // Public

        #endregion // Properties
    }

    #endregion // MergedColumnInfo

    #region MergeDelegate

    /// <summary>
    /// Custom deletage used to pass information in the MergedDataContext.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="in1"></param>
    /// <param name="in2"></param>
    /// <param name="in3"></param>
    /// <param name="in4"></param>
    /// <param name="in5"></param>
    /// <returns></returns>
    public delegate TResult MergeDelegate<T1, T2, T3, T4, T5, TResult>(T1 in1, T2 in2, T3 in3, T4 in4, T5 in5);
    
    #endregion // MergeDelegate
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