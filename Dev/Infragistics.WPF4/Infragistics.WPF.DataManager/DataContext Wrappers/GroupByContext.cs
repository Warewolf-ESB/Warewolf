using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Windows.Data;

namespace Infragistics
{
	#region GroupByContext

	/// <summary>
	/// An object that encapsulates the GroupBy functionality used by the <see cref="DataManagerBase"/>
	/// </summary>
	public abstract class GroupByContext
	{
		/// <summary>
		/// Gets the name of the property that data should be grouped by.
		/// </summary>
		public string PropertyName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Creates a Generic instance of the <see cref="GroupByContext"/>.
		/// </summary>
        /// <param name="cti">The type of data to create the GroupByContext from.</param>
		/// <param name="propertyName">The name of the property, that the data should be grouped by.</param>
		/// <param name="comparer">The IEqualityComparer that will be used to perform the grouping</param>
		/// <returns></returns>
		public static GroupByContext CreateGenericCustomGroup(CachedTypedInfo cti, string propertyName, object comparer)
		{
            Type type = cti.CachedType;
			Type customGroupType = typeof(GroupByContext<>).MakeGenericType(new System.Type[] { DataManagerBase.ResolvePropertyTypeFromPropertyName(propertyName, cti) });
			return (GroupByContext)Activator.CreateInstance(customGroupType, new object[] { propertyName, comparer, cti });
		}

		/// <summary>
		/// Creates a Generic instance of the <see cref="GroupByContext"/>.
		/// </summary>
        /// <param name="cti">The type of data to create the GroupByContext from.</param>
		/// <param name="comparer" ></param>
		/// <param name="converter" ></param>
		/// <param name="converterParam"></param>
		/// <returns></returns>
        public static GroupByContext CreateGenericCustomGroup(CachedTypedInfo cti, object comparer, IValueConverter converter, object converterParam)
		{
            Type type = cti.CachedType;
			Type customGroupType = typeof(GroupByContext<>).MakeGenericType(new System.Type[] { type });
			return (GroupByContext)Activator.CreateInstance(customGroupType, new object[] { comparer, converter, converterParam, cti });
		}

		/// <summary>
		/// Groups the specified <see cref="IQueryable"/> by the property this data represents.
		/// </summary>
		/// <typeparam name="T">The typeof data that needs to be grouped.</typeparam>
		/// <param name="query">A colleciton of data to group by.</param>
		/// <returns>Collection of <see cref="GroupByDataContext"/> objects.</returns>
		public abstract IList Group<T>(IQueryable<T> query);

        #region Properties

        #region Protected

        #region Summaries

        /// <summary>
        /// Gets a list of the summaries that should be applied to Children.
        /// </summary>
        protected internal SummaryDefinitionCollection Summaries
        {
            get;
            set;
        }
        #endregion // Protected

        #endregion // Protected

        #endregion // Properties

    }
	#endregion // GroupByContext

	#region GroupByContext<TColumnType>

	/// <summary>
	/// An object that encapsulates the GroupBy functionality used by the <see cref="DataManagerBase"/>
	/// </summary>
	/// <typeparam name="TColumnType">The type of data that should be grouped by.</typeparam>
	public class GroupByContext<TColumnType> : GroupByContext
	{

		private bool _unboundColumn;

		/// <summary>
		/// Gets the Comparer that will be used to perform the grouping.
		/// </summary>
		public IEqualityComparer<TColumnType> Comparer
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets the <see cref="IValueConverter"/> which will be used to evaluate the GroupBy.
		/// </summary>
		protected IValueConverter Converter
		{
			get;
			private set;
		}

		/// <summary>
		/// The parameter applied to the <see cref="Converter"/>.
		/// </summary>
		protected object ConverterParameter
		{
			get;
			private set;
		}

        /// <summary>
        /// The CachedTypedInfo for the operation.
        /// </summary>
        protected CachedTypedInfo CachedTypedInfo
        {
            get;
            private set;
        }

		/// <summary>
		/// Creates a new instance of the <see cref="GroupByContext"/>.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="comparer"></param>
        /// <param name="cti"></param>
		public GroupByContext(String propertyName, object comparer, CachedTypedInfo cti)
		{
			this.PropertyName = propertyName;

			this.Comparer = comparer as IEqualityComparer<TColumnType>;

			this._unboundColumn = false;
            this.CachedTypedInfo = cti;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="GroupByContext"/>.
		/// </summary>
		/// <param name="comparer"></param>
		/// <param name="converter"></param>
		/// <param name="converterParam"></param>
        /// <param name="cti"></param>
		public GroupByContext(object comparer, IValueConverter converter, object converterParam, CachedTypedInfo cti)
		{
			this._unboundColumn = true;
			this.Comparer = comparer as IEqualityComparer<TColumnType>;
			this.Converter = converter as IValueConverter;
			this.ConverterParameter = converterParam;
            this.CachedTypedInfo = cti;
		}

		/// <summary>
		/// Groups the specified <see cref="IQueryable"/> by the property this data represents.
		/// </summary>
		/// <typeparam name="T">Type typeof data that needs to be grouped.</typeparam>
		/// <param name="query">A colleciton of data to group by.</param>
		/// <returns>Collection of <see cref="GroupByDataContext"/> objects.</returns>
		public override IList Group<T>(IQueryable<T> query)
		{
			ParameterExpression parameterexpression = Expression.Parameter(typeof(T), "param");

			Expression body = null;

			if (this._unboundColumn )
			{
				body = parameterexpression;			
			}
			else
			{
				body = DataManagerBase.BuildPropertyExpressionFromPropertyName(this.PropertyName, parameterexpression, this.CachedTypedInfo, typeof(TColumnType), default(TColumnType));
			}

            if (!typeof(T).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterexpression, Expression.Constant(null, typeof(T)));
                System.Linq.Expressions.Expression constExpression = System.Linq.Expressions.Expression.Constant(default(TColumnType), typeof(TColumnType));
                body = Expression.Condition(equalExpression, constExpression, body);
            }
      
			Expression<Func<T, TColumnType>> expr = Expression.Lambda(body, parameterexpression) as Expression<Func<T, TColumnType>>;

			List<GroupByDataContext> list = new List<GroupByDataContext>();


			IEqualityComparer<TColumnType> comparer = this.Comparer;
			if (comparer == null && this._unboundColumn)
			{
				comparer = new ValObjComparer<TColumnType>(this.Converter, this.ConverterParameter);
			}

			IQueryable<IGrouping<TColumnType, T>> q = query.GroupBy<T, TColumnType>(expr, comparer);

            if (this._unboundColumn)
            {
                foreach (IGrouping<TColumnType, T> grouping in q)
                {
                    ValObj tbx = new ValObj();
                    Binding bx = new Binding();
                    bx.Converter = this.Converter;
                    bx.ConverterParameter = this.ConverterParameter;
                    bx.Source = grouping.Key;
                    tbx.SetBinding(ValObj.ValueProperty, bx);

                    list.Add(new GroupByDataContext() { Records = grouping.ToList<T>(), Value = tbx.Value, TypedInfo = CachedTypedInfo, Summaries = this.Summaries });
                }
            }
            else
            {
                foreach (IGrouping<TColumnType, T> grouping in q)
                {
                    list.Add(new GroupByDataContext() { Records = grouping.ToList<T>(), Value = grouping.Key, TypedInfo = CachedTypedInfo, Summaries = this.Summaries });
                }
            }

			return list;
		}
	}
	#endregion // GroupByContext<T, TColumnType>

	#region GroupByDataContext

	/// <summary>
	/// An object that contains the informaton of data that has been grouped by the <see cref="GroupByContext"/>
	/// </summary>
	public class GroupByDataContext
    {
        #region Members

        SummaryResultCollection _summaryResultCollection, _groupBySummaryResultCollection;
        Dictionary<string, SummaryResultCollection> _groupBySummariesLookup, _summaryLookup;

        #endregion // Members

        #region Properties
        
        #region Internal

        internal string DisplayValueStringFormat
        {
            get { return "{0}: ({1})"; }
        }

        #endregion // Internal

        #region Public

        #region Value

        /// <summary>
		/// Gets the value of the that the data has been grouped by.
		/// </summary>
		public object Value
		{
			get;
			internal set;
		}

        #endregion // Value

        #region Records
        /// <summary>
		/// Gets a collection of data that belongs to this particular grouping.
		/// </summary>
		public IList Records
		{
			get;
			internal set;
		}
        #endregion  // Records

        #region Count
        /// <summary>
		/// Gets the total amount of records in this particular grouping.
		/// </summary>
		public int Count
		{
			get
			{
				if (this.Records == null)
					return 0;
				else
					return this.Records.Count;
			}
		}

        #endregion // Count        

        #region DisplayValue


        /// <summary>
        /// Gets the string representation of the value, with the Count appended to it. 
        /// </summary>
        public string DisplayValue
        {
            get
            {
                string value = string.Format(this.DisplayValueStringFormat, this.Value ?? string.Empty, this.Count);

                return value;
            }
        }

        #endregion // DisplayValue

        #region SummaryLookupResults

        /// <summary>
        /// Gets a lookup table of the SummaryResults based on a column key.
        /// </summary>
        public Dictionary<string, SummaryResultCollection> SummaryLookupResults
        {
            get
            {
                if (this.SummaryResults != null)
                {
                    return this._summaryLookup;
                }

                return new Dictionary<string, SummaryResultCollection>();
            }
        }

        #endregion // SummaryLookupResults

        #region SummaryResults

        /// <summary>
        /// Summary results for that were specified for all fields.
        /// </summary>
        public SummaryResultCollection SummaryResults
        {
            get
            {
                if (this._summaryResultCollection == null)
                {
                    if (this.Summaries.Count > 0)
                    {
                        this._summaryResultCollection = new SummaryResultCollection();
                        this._summaryLookup = new Dictionary<string, SummaryResultCollection>();

                        IQueryable query = this.Records.AsQueryable();
                        foreach (SummaryDefinition sd in this.Summaries)
                        {
                            if (!this._summaryLookup.ContainsKey(sd.ColumnKey))
                                this._summaryLookup.Add(sd.ColumnKey, new SummaryResultCollection());

                            SummaryResultCollection lookupSRC = this._summaryLookup[sd.ColumnKey];

                            SummaryResult result = null; 

                            ISupportLinqSummaries isls = sd.SummaryOperand.SummaryCalculator as ISupportLinqSummaries;
                            if (isls != null)
                            {
                                SummaryContext sc = SummaryContext.CreateGenericSummary(this.TypedInfo, sd.ColumnKey, isls.SummaryType);
                                isls.SummaryContext = sc;
                                result = new SummaryResult(sd, sc.Execute(query));
                                
                            }
                            else
                            {
                                SynchronousSummaryCalculator ssc = sd.SummaryOperand.SummaryCalculator as SynchronousSummaryCalculator;
                                if (ssc != null)
                                {
                                    result = new SummaryResult(sd, ssc.Summarize(query, sd.ColumnKey));
                                }
                            }

                            if (result != null)
                            {
                                this._summaryResultCollection.Add(result);
                                lookupSRC.Add(result);
                            }
                        }
                    }
                    else
                        return new SummaryResultCollection();

                }
                return this._summaryResultCollection;
            }
        }
        #endregion // SummaryResults

        #region GroupBySummaryLookupResults

        /// <summary>
        /// Gets a lookup table of the GroupBySummaryResults based on a column key.
        /// </summary>
        public Dictionary<string, SummaryResultCollection> GroupBySummaryLookupResults
        {
            get
            {
                if (this.GroupBySummaryResults != null)
                {
                    return this._groupBySummariesLookup;
                }

                return new Dictionary<string, SummaryResultCollection>();
            }
        }

        #endregion // GroupBySummaryLookupResults

        #region GroupBySummaryResults

        /// <summary>
        /// Summary Results specific to the particular field that this GroupByContext represents.
        /// </summary>
        public SummaryResultCollection GroupBySummaryResults
        {
            get
            {
                if (this._groupBySummaryResultCollection == null)
                {
                    this._groupBySummariesLookup = new Dictionary<string, SummaryResultCollection>();
                    if (this.GroupBySummaries.Count > 0)
                    {
                        this._groupBySummaryResultCollection = new SummaryResultCollection();

                        IQueryable query = this.Records.AsQueryable();
                        foreach (SummaryDefinition sd in this.GroupBySummaries)
                        {
                            if (!this._groupBySummariesLookup.ContainsKey(sd.ColumnKey))
                                this._groupBySummariesLookup.Add(sd.ColumnKey, new SummaryResultCollection());

                            SummaryResultCollection lookupSRC = this._groupBySummariesLookup[sd.ColumnKey];

                            SummaryResult result = null; 

                            ISupportLinqSummaries isls = sd.SummaryOperand.SummaryCalculator as ISupportLinqSummaries;
                            if (isls != null)
                            {
                                SummaryContext sc = SummaryContext.CreateGenericSummary(this.TypedInfo, sd.ColumnKey, isls.SummaryType);
                                isls.SummaryContext = sc;
                                result = new SummaryResult(sd, sc.Execute(query));
                            }
                            else
                            {
                                SynchronousSummaryCalculator ssc = sd.SummaryOperand.SummaryCalculator as SynchronousSummaryCalculator;
                                if (ssc != null)
                                {
                                    result = new SummaryResult(sd, ssc.Summarize(query, sd.ColumnKey));
                                }
                            }

                            if (result != null)
                            {
                                this._groupBySummaryResultCollection.Add(result);
                                lookupSRC.Add(result);
                            }
                        }
                    }
                    else
                        return new SummaryResultCollection();

                }
                return this._groupBySummaryResultCollection;
            }
        }
        #endregion // GroupBySummaryResults

        #endregion // Public

        #region Protected

        #region Summaries

        /// <summary>
        /// Gets a list of the summaries that should be applied to Children.
        /// </summary>
        protected internal SummaryDefinitionCollection Summaries
        {
            get;
            set;
        }
        #endregion // Summaries

        #region GroupBySummaries

        /// <summary>
        /// Gets a list of the summaries that should be applied specifically for GroupBy.
        /// </summary>
        protected internal SummaryDefinitionCollection GroupBySummaries
        {
            get;
            set;
        }
        #endregion // GroupBySummaries

        #region TypedInfo

        /// <summary>
        /// Gets/Sets the TypedInfo for the object.
        /// </summary>
        protected internal CachedTypedInfo TypedInfo
        {
            get;
            set;
        }

        #endregion // TypedInfo

        #endregion // Protected

        #endregion // Properties

        #region Methods

        #region Internal

        internal void Reload()
        {
            this._summaryResultCollection = null;
            this._groupBySummaryResultCollection = null;
        }

        #endregion // Internal

        #endregion // Methods
    }

	#endregion // GroupByDataContext
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