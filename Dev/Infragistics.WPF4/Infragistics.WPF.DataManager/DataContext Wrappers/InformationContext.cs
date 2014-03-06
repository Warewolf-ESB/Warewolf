using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;


namespace Infragistics
{
	/// <summary>
	/// A context object used to get data from the data for secondary features.
	/// </summary>
	public abstract class InformationContext
	{
		#region StaticMethods
		/// <summary>
        /// A <see cref="MethodInfo"/> object containing a method that will convert text to a globalized string for case insensitive sorting.
		/// </summary>
		protected readonly static MethodInfo ToUpperMethod = typeof(string).GetMethod("ToUpper", new Type[] { typeof(System.Globalization.CultureInfo) });

        /// <summary>
        /// A <see cref="MethodInfo"/> object containing a method that will validate if an inputted string is null or empty. 
        /// </summary>
        protected readonly static MethodInfo IsNullOrEmptyMethod = typeof(string).GetMethod("IsNullOrEmpty", new Type[] {typeof(string) });
		
		#endregion // StaticMethods

		#region Properties

		#region PropertyName
		/// <summary>
		/// Gets the name of the property that will be processed for information.
		/// </summary>
		public String PropertyName
		{
			get;
			protected set;
		}

		#endregion // PropertyName

		#region DataType

		/// <summary>
		/// Gets the Type of the data object being processed.
		/// </summary>
		public Type DataType
		{
			get;
			protected set;
		}

		#endregion // DataType

		#region PropertyType

		/// <summary>
		/// Gets the Type of the objects provided by the property.
		/// </summary>
		public Type PropertyType
		{
			get;
			protected set;
		}

		#endregion // PropertyType

		#region CaseSensitive

		/// <summary>
		/// Gets if case sensitivity should be used when generating the list.
		/// </summary>
		public bool CaseSensitive
		{
			get;
			protected set;
		}

		#endregion // CaseSensitive

		#region SortAscending

		/// <summary>
		/// Gets if the generated list should be sorted.
		/// </summary>
		public bool SortAscending
		{
			get;
			protected set;
		}

		#endregion // SortAscending

		#region Comparer

		/// <summary>
		/// Gets the Comparer that should be used for sorting.
		/// </summary>
		public object Comparer
		{
			get;
			protected set;
		}

		#endregion // Comparer

		#region Lambda

		/// <summary>
		/// Gets the <see cref="LambdaExpression"/> which is used to generate the list.
		/// </summary>
		public LambdaExpression Lambda
		{
			get;
			protected set;
		}

		#endregion // Lambda

		#region FromDateColumn
		/// <summary>
		/// Gets if the unique list being build is for the Date Column.  If so then we will use some ranging logic when
		/// building unique list since the Date column does not support time.
		/// </summary>
		protected bool FromDateColumn
		{
			get;
			set;
		}
		#endregion // FromDateColumn

		#endregion // Properties

		#region Methods

		#region Public

		#region CreateGenericInformationContext
		/// <summary>
		/// Creates a typed <see cref="InformationContext"/> object.
		/// </summary>
        /// <param name="cti"></param>
		/// <param name="propertyName"></param>
		/// <param name="sortAscending"></param>
		/// <param name="caseSensitive"></param>
		/// <param name="comparer"></param>
		/// <param name="fromDateColumn"></param>
		/// <returns></returns>
		public static InformationContext CreateGenericInformationContext(CachedTypedInfo cti, string propertyName, bool sortAscending, bool caseSensitive, object comparer, bool fromDateColumn)
		{
            Type type = cti.CachedType;
			Type specificICType = typeof(InformationContext<,>).MakeGenericType(new System.Type[] { type, DataManagerBase.ResolvePropertyTypeFromPropertyName(propertyName, cti) });
			return (InformationContext)Activator.CreateInstance(specificICType, new object[] { propertyName, sortAscending, caseSensitive, comparer, fromDateColumn, cti });
		}
		#endregion // CreateGenericInformationContext

		#region GetDistinctValues

		/// <summary>
		/// Gets an <see cref="IList"/> of distinct values from the inputted <see cref="IEnumerable"/>.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public abstract IList GetDistinctValues(IEnumerable list);


	    /// <summary>
	    /// Gets an <see cref="IList"/> of distinct values from the inputted <see cref="IEnumerable"/>.
	    /// </summary>
	    /// <param name="list"></param>
	    /// <param name="token"></param>
	    /// <param name="runAsync"></param>
	    /// <returns></returns>
	    internal abstract Task<IList> GetDistinctValuesAsync(IEnumerable list, CancellationToken token, bool runAsync);


		#endregion // GetDistinctValues

		#region GetCompleteValuesList

		/// <summary>
		/// Returns an <see cref="IList"/> of objects from this <see cref="InformationContext"/>.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public abstract IList GetCompleteValuesList(IEnumerable list);

		#endregion // GetCompleteValuesList

		#endregion // Public

		#endregion // Methods
	}

	#region InformationContext<T, TColumnType>

	/// <summary>
	/// A class of <see cref="InformationContext"/> objects generically typed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TColumnType"></typeparam>
	public class InformationContext<T, TColumnType> : InformationContext
    {
        #region Members

        private IEqualityComparer<TColumnType> _filterComparer = null;

        #endregion // Members

        #region Constructor
        /// <summary>
		/// Initializes a new instance of the <see cref="InformationContext"/> class.
		/// </summary>
		public InformationContext(String propertyName, bool sortAscending, bool caseSensitiveSort, object comparer, bool fromDateColumn, CachedTypedInfo cti)
		{
			this.PropertyName = propertyName;
			this.SortAscending = sortAscending;
			this.CaseSensitive = caseSensitiveSort;
			this.Comparer = (IComparer<TColumnType>)comparer;
			this.FromDateColumn = fromDateColumn;

			this.PropertyType = typeof(TColumnType);
			this.DataType = typeof(T);

			ParameterExpression parameterexpression = Expression.Parameter(typeof(T), "param");

			Expression body = DataManagerBase.BuildPropertyExpressionFromPropertyName(propertyName, parameterexpression, cti, this.PropertyType, default(TColumnType));

			if (!caseSensitiveSort && PropertyType == typeof(string) && comparer == null)
			{
				// AS 3/16/12
				// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
				//
                // NZ 17 May 2012 - TFS111849 - Restored IsNullOrEmpty check required for the filtering of string.Empty in filter menu.
				Expression equalExpression = Expression.Call(null, IsNullOrEmptyMethod, body);
				////Expression equalExpression = Expression.Equal(body, Expression.Constant(null, this.PropertyType));                
				//body = Expression.Call(body, ToUpperMethod, Expression.Constant(System.Globalization.CultureInfo.CurrentCulture));
				System.Linq.Expressions.Expression constExpression = System.Linq.Expressions.Expression.Constant(default(TColumnType), this.PropertyType);
				body = Expression.Condition(equalExpression, constExpression, body);
				this.Comparer = StringComparer.CurrentCultureIgnoreCase;
                this._filterComparer = (IEqualityComparer<TColumnType>)StringComparer.CurrentCultureIgnoreCase;
			}

			if (!parameterexpression.Type.IsValueType)
			{
				Expression equalExpression = Expression.Equal(parameterexpression, Expression.Constant(null, this.DataType));
				System.Linq.Expressions.Expression constExpression = System.Linq.Expressions.Expression.Constant(default(TColumnType), this.PropertyType);
				body = Expression.Condition(equalExpression, constExpression, body);
			}

			this.Lambda = Expression.Lambda(body, parameterexpression);
		}

		#endregion // InformationContext

		#region GetDistinctValues

		/// <summary>
		/// Gets an <see cref="IList"/> of distinct values from the inputted <see cref="IEnumerable"/>.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public override IList GetDistinctValues(IEnumerable list)
		{
            if (list == null)
            {
                return null;
            }


		    return this.GetDistinctValuesAsync(list, CancellationToken.None, false).Result;


#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

		}


	    /// <summary>
	    /// Gets an <see cref="IList"/> of distinct values from the inputted <see cref="IEnumerable"/>.
	    /// </summary>
	    /// <param name="list"></param>
	    /// <param name="token"></param>
	    /// <param name="runAsync"></param>
	    /// <returns></returns>
        internal override Task<IList> GetDistinctValuesAsync(IEnumerable list, CancellationToken token, bool runAsync)
        {
            if (list == null)
            {
                return null;
            }

            Type itemType = DataManagerBase.ResolveItemType(list);

            if (itemType == typeof(MergedRowInfo))
            {
                return this.GetDistinctValuesAsync(list.Cast<MergedRowInfo>().Select(i => i.Data).Cast<T>().AsQueryable<T>(), token, runAsync);
            }
            else
            {
                IQueryable<T> listCasted = list as IQueryable<T>;

                if (listCasted == null)
                {
                    listCasted = list.Cast<T>().AsQueryable<T>();
                }

                return this.GetDistinctValuesAsync(listCasted, token, runAsync);
            }
        }

	    /// <summary>
		/// Gets an <see cref="IList"/> of distinct values from the inputted <see cref="IEnumerable"/>.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
        public IList<TColumnType> GetDistinctValues(IQueryable<T> list)
	    {
	        return (IList<TColumnType>)this.GetDistinctValuesAsync(list, CancellationToken.None, false).Result;
	    }

	    /// <summary>
	    /// Gets an <see cref="IList"/> of distinct values from the inputted <see cref="IEnumerable"/>.
	    /// </summary>
	    /// <param name="list"></param>
	    /// <param name="token"></param>
	    /// <param name="runAsync"></param>
	    /// <returns></returns>
	    internal Task<IList> GetDistinctValuesAsync(IQueryable<T> list, CancellationToken token, bool runAsync)
        {
            return this.GetDistinctValuesImpl(list, token, runAsync);
        }

	    /// <summary>
	    /// Gets an <see cref="IList"/> of distinct values from the inputted <see cref="IEnumerable"/>.
	    /// </summary>
	    /// <param name="list"></param>
	    /// <param name="token"></param>
	    /// <param name="runAsync"></param>
	    /// <returns></returns>
	    private Task<IList> GetDistinctValuesImpl(IQueryable<T> list, CancellationToken token, bool runAsync)
        {
            Expression<Func<T, TColumnType>> expr = (Expression<Func<T, TColumnType>>)this.Lambda;

	        IQueryable<TColumnType> copy;

	        if (runAsync)
	        {
	            copy = list.Select(expr);
	        }
	        else
	        {
	            copy = new List<TColumnType>(list.Select(expr)).AsQueryable();
	        }

            Task<IList> task = Task<IList>.Factory.StartNew(
                () =>
                    {
                        List<TColumnType> result;
                        IQueryable<TColumnType> distinct = copy.Distinct(this._filterComparer);

                        ParameterExpression parameterexpression = Expression.Parameter(typeof(TColumnType), "param");
                        LambdaExpression lambda = Expression.Lambda(parameterexpression, parameterexpression);
                        Expression<Func<TColumnType, TColumnType>> sortSelector = (Expression<Func<TColumnType, TColumnType>>)lambda;

                        if (this.SortAscending)
                        {
                            if (this.Comparer != null)
                            {
                                distinct = distinct.OrderBy<TColumnType, TColumnType>(sortSelector, (IComparer<TColumnType>)this.Comparer);
                            }
                            else
                            {
                                distinct = distinct.OrderBy<TColumnType, TColumnType>(sortSelector);
                            }
                        }
                        else
                        {
                            if (this.Comparer != null)
                            {
                                distinct = distinct.OrderByDescending<TColumnType, TColumnType>(sortSelector, (IComparer<TColumnType>)this.Comparer);
                            }
                            else
                            {
                                distinct = distinct.OrderByDescending<TColumnType, TColumnType>(sortSelector);
                            }
                        }

                        if (token == CancellationToken.None)
                        {
                            result = distinct.ToList(); //<-- Perf Hit
                        }
                        else
                        {

                            try
                            {
                                result = distinct.AsParallel().AsOrdered().WithCancellation(token).ToList(); //<-- Perf Hit
                            }
                            catch(OperationCanceledException)
                            {
                                result = null;
                            }


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                        }

                        return result;
                    });

	        return task;
        }


#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

	    /// <summary>
		/// Gets an <see cref="IList"/> of distinct values from the inputted <see cref="IEnumerable"/>.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public IList GetDistinctValues(IQueryable list)
		{
			if (list == null)
				return null;

			IQueryable<T> listCasted = list as IQueryable<T>;

			if (listCasted == null)
			{
				listCasted = list.AsQueryable().Cast<T>();
			}

			return (IList)this.GetDistinctValues(listCasted);
		}
		#endregion // GetDistinctValues

		#region GetCompleteValuesList
		/// <summary>
		/// Returns an <see cref="IList"/> of objects from this <see cref="InformationContext"/>.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public override IList GetCompleteValuesList(IEnumerable list)
		{
			if (list == null)
				return null;

			IQueryable<T> listCasted = list as IQueryable<T>;

			if (listCasted == null)
			{
				listCasted = list.AsQueryable().Cast<T>();
			}

			return (IList)this.GetCompleteValuesList(listCasted);
		}

		/// <summary>
		/// Returns an <see cref="IList"/> of objects from this <see cref="InformationContext"/>.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public IList GetCompleteValuesList(IQueryable list)
		{
			if (list == null)
				return null;

			IQueryable<T> listCasted = list as IQueryable<T>;

			if (listCasted == null)
			{
				listCasted = list.AsQueryable().Cast<T>();
			}

			return (IList)this.GetCompleteValuesList(listCasted);
		}

		/// <summary>
		/// Returns an <see cref="IList"/> of objects from this <see cref="InformationContext"/>.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public IList<TColumnType> GetCompleteValuesList(IQueryable<T> list)
		{
			Expression<Func<T, TColumnType>> expr = (Expression<Func<T, TColumnType>>)this.Lambda;

			if (this.SortAscending)
			{
				IEnumerable<TColumnType> needToSortEnumerable = list.Select(expr);

				Type columnType = typeof(TColumnType);

				if (this.FromDateColumn)
				{
					if (columnType == typeof(DateTime))
						needToSortEnumerable = (IEnumerable<TColumnType>)(((IEnumerable<DateTime>)needToSortEnumerable).Select(a => new DateTime(a.Year, a.Month, a.Day)).Distinct());
					else if (columnType == typeof(DateTime?))
						needToSortEnumerable = (IEnumerable<TColumnType>)(((IEnumerable<DateTime?>)needToSortEnumerable).Select(a => (!a.HasValue ? (DateTime?)null : new DateTime(a.Value.Year, a.Value.Month, a.Value.Day))));
				}

				List<TColumnType> needToSortList = (List<TColumnType>)needToSortEnumerable.ToList();
				if (this.Comparer != null)
				{
					needToSortList.Sort((IComparer<TColumnType>)this.Comparer);
				}
				else
				{
					try
					{
						needToSortList.Sort();
					}
					catch
					{

					}
				}
				return needToSortList;
			}

			return list.Select(expr).ToList();
		}

		#endregion // GetCompleteValuesList
	}
	#endregion // InformationContext<T, TColumnType>
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