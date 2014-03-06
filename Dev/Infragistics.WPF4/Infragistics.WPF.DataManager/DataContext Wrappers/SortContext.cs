using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Windows.Data;

namespace Infragistics
{
	#region CurrentSort
	/// <summary>
	/// A nongeneric abstract class representing a sort on a property.
	/// Cast up to CurrentSort<![CDATA[<T>]]> to get an expression representing the sort.
	/// </summary>
	public abstract class SortContext
	{
		/// <summary>
		/// A MethodInfo object containging a method that will convert text to a globalized string for case insensitive sorting.
		/// </summary>
		protected readonly static MethodInfo ToUpperMethod = typeof(string).GetMethod("ToUpper", new Type[] { typeof(System.Globalization.CultureInfo) });

		#region Properties

		#region Public
		/// <summary>
		/// Gets a string describing the column which is sorted.
		/// </summary>
		public String SortPropertyName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets if the current sort is sorted ascending or descending.
		/// </summary>
		public bool SortAscending
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets the type of the property that is being sorted on.
		/// </summary>
		public Type PropertyType
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets the type of the object that is being sorted.
		/// </summary>
		public Type DataType
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets if the sort will be case sensitive.  Only applies to string columns.
		/// </summary>
		public bool CaseSensitiveSort
		{
			get;
			protected set;
		}

		#endregion //Public

		#endregion //Properties

		#region Static Methods

		#region CreateGenericSort
		/// <summary>
		/// Initalizes a new CurrentSort<![CDATA[<T>]]> and returns it as type CurrentSort.
		/// </summary>
		/// <param name="cachedTypeInfo">The type of the CurrentSort<![CDATA[<T>]]> to create.</param>
		/// <param name="propertyName">The name of the property that the created sort should be sorting.</param>
		/// <param name="sortAscending">True if the sort should sort the property in ascending order.</param>
		/// <param name="isCaseSensitiveSort">True if the sort should be considered case sensitive.  Only used for string fields.</param>
		/// <param name="comparer">An IComparer<![CDATA[<T>]]> which will be used instead of the default sorting.</param>
		/// <returns>A new CustomFilter which is also of type CurrentSort<![CDATA[<T>]]>.</returns>
		public static SortContext CreateGenericSort(CachedTypedInfo cachedTypeInfo, string propertyName, bool sortAscending, bool isCaseSensitiveSort, object comparer)
		{            
            Type type = cachedTypeInfo.CachedType;
			Type specificSortType = typeof(SortContext<,>).MakeGenericType(new System.Type[] { type, DataManagerBase.ResolvePropertyTypeFromPropertyName(propertyName, cachedTypeInfo) });
			return (SortContext)Activator.CreateInstance(specificSortType, new object[] { propertyName, sortAscending, isCaseSensitiveSort, comparer, cachedTypeInfo });
		}

		/// <summary>
		/// Initalizes a new CurrentSort<![CDATA[<T>]]> and returns it as type CurrentSort.
		/// </summary>
		/// <param name="cachedTypeInfo">The type of the CurrentSort<![CDATA[<T>]]> to create.</param>
		/// <param name="sortAscending">True if the sort should sort the property in ascending order.</param>
		/// <param name="comparer">An IComparer<![CDATA[<T>]]> which will be used instead of the default sorting.</param>
		/// <param name="converter"></param>
		/// <param name="converterParam"></param>
		/// <returns>A new CustomFilter which is also of type CurrentSort<![CDATA[<T>]]>.</returns>
        public static SortContext CreateGenericSort(CachedTypedInfo cachedTypeInfo, bool sortAscending, object comparer, IValueConverter converter, object converterParam)
		{
            Type type = cachedTypeInfo.CachedType;
			Type specificSortType = typeof(SortContext<,>).MakeGenericType(new System.Type[] { type, type });
			return (SortContext)Activator.CreateInstance(specificSortType, new object[] { sortAscending, comparer, converter, converterParam, cachedTypeInfo });
		}

		#endregion

		#endregion

		#region Methods

		/// <summary>
		/// Sorts a given IQueryable based on the currently sorted column.
		/// </summary>
		/// <param name="query">The IQueryable that should be sorted.</param>		
		/// <returns>A sorted IOrderedQueryable, sorted on the currently sorted property.</returns>
		public abstract IOrderedQueryable<T> Sort<T>(IQueryable<T> query);

		/// <summary>
		/// Appends to this sort to the IOrderedQueryable sort
		/// </summary>
		/// <param name="query">The existing query that already has a sort on it.</param>		
		/// <returns>A sorted IOrderedQueryable, sorted on the currently sorted property.</returns>
		public abstract IOrderedQueryable<T> AppendSort<T>(IOrderedQueryable<T> query);

		#endregion // Methods
	}
	#endregion

	#region CurrentSort<T, TColumnType>
	/// <summary>
	/// A generic class to describe a custom sort on a property given the type of object and column type.
	/// </summary>
	/// <typeparam name="T">The type of object that is sorted by this sort.</typeparam>
	/// <typeparam name="TColumnType">The column type that is sorted by this sort.</typeparam>
	public class SortContext<T, TColumnType> : SortContext
	{
		#region Members
		LambdaExpression _lambda;
		#endregion

		#region Constructor

		/// <summary>
		/// Initalizes a new instance of the CurrentSort class.
		/// </summary>
		/// <param name="propertyName">The name of the property to be sorted.</param>
		/// <param name="sortAscending">True if the property should be sorted ascending.</param>
		/// <param name="caseSensitiveSort">True if the sort should be case sensitive.  Only applies to string columns.</param>
		/// <param name="comparer">A custom IComparer<![CDATA[<T>]]> object, generic typed to the data type of the data in the column.  If non null this will take precedence over case sensitivity.</param>
        /// <param name="cachedTypeInfo"/>
		public SortContext(String propertyName, bool sortAscending, bool caseSensitiveSort, object comparer, CachedTypedInfo cachedTypeInfo)
		{
			this.SortPropertyName = propertyName;
			this.SortAscending = sortAscending;
			this.CaseSensitiveSort = caseSensitiveSort;
			this.Comparer = (IComparer<TColumnType>)comparer;

			PropertyType = typeof(TColumnType);
			DataType = typeof(T);


            ParameterExpression parameterexpression = Expression.Parameter(typeof(T), "param");
			Expression body = DataManagerBase.BuildPropertyExpressionFromPropertyName(propertyName, parameterexpression, cachedTypeInfo, PropertyType, default(TColumnType));

			if (!caseSensitiveSort && PropertyType == typeof(string) && comparer == null)
			{
				// AS 3/16/12
				// Don't allocate a string - just use the case insensitive comparer.
				//
				//body = Expression.Call(body, ToUpperMethod, Expression.Constant(System.Globalization.CultureInfo.CurrentCulture));
				this.Comparer = (IComparer<TColumnType>)StringComparer.CurrentCultureIgnoreCase;
			}

			// AS 3/16/12
			// This doesn't account for interfaces in which case the value could be null. Since 
			// we just want to avoid value type compared to null we can check IsValueType which 
			// is part of what IsClass checks anyway.
			//
			//if (parameterexpression.Type.IsClass)
			if (!parameterexpression.Type.IsValueType)
			{
				Expression equalExpression = Expression.Equal(parameterexpression, Expression.Constant(null, this.DataType));
				System.Linq.Expressions.Expression constExpression = System.Linq.Expressions.Expression.Constant(default(TColumnType), this.PropertyType);
				body = Expression.Condition(equalExpression, constExpression, body);
			}
			_lambda = Expression.Lambda(body, parameterexpression);
		}
        

		/// <summary>
		/// Initalizes a new instance of the CurrentSort class.
		/// </summary>
		/// <param name="propertyName">The name of the property to be sorted.</param>
		/// <param name="sortAscending">True if the property should be sorted ascending.</param>
		/// <param name="caseSensitiveSort">True if the sort should be case sensitive.  Only applies to string columns.</param>
        /// <param name="cachedTypeInfo"/>
		public SortContext(String propertyName, bool sortAscending, bool caseSensitiveSort, CachedTypedInfo cachedTypeInfo)
			: this(propertyName, sortAscending, caseSensitiveSort, null, cachedTypeInfo)
		{
		}

        /// <summary>
        /// Initalizes a new instance of the CurrentSort class.
        /// </summary>
        /// <param name="sortAscending">True if the property should be sorted ascending.</param>
        /// <param name="comparer"></param>
        /// <param name="converter"></param>
        /// <param name="converterParam"></param>
		/// <param name="cachedTypeInfo"></param>
        public SortContext(bool sortAscending, object comparer, IValueConverter converter, object converterParam, CachedTypedInfo cachedTypeInfo)
		{
			this.SortAscending = sortAscending;
			this.Converter = converter;
			this.ConverterParameter = converterParam;

			this.PropertyType = DataType = typeof(T);
			this.Comparer = comparer as IComparer<TColumnType>;

			if (comparer == null && converter != null)
			{
				ValObjComparer<TColumnType> valObjComparer = new ValObjComparer<TColumnType>(converter, converterParam);
				this.Comparer = valObjComparer;
			}

			ParameterExpression parameterexpression = Expression.Parameter(typeof(T), "param");
			Expression body = parameterexpression;

			// AS 3/16/12
			// This doesn't account for interfaces in which case the value could be null. Since 
			// we just want to avoid value type compared to null we can check IsValueType which 
			// is part of what IsClass checks anyway.
			//
			//if (parameterexpression.Type.IsClass)
			if (!parameterexpression.Type.IsValueType)
			{
				Expression equalExpression = Expression.Equal(parameterexpression, Expression.Constant(null, this.DataType));
				System.Linq.Expressions.Expression constExpression = System.Linq.Expressions.Expression.Constant(default(TColumnType), this.PropertyType);
				body = Expression.Condition(equalExpression, constExpression, body);
			}

			_lambda = Expression.Lambda(body, parameterexpression);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current LambdaExpression describing this sort.
		/// </summary>
		protected LambdaExpression Lambda
		{
			get
			{
				return this._lambda;
			}
		}

		/// <summary>
		/// A IComparer<![CDATA[<T>]]> object with the generic type set to the DataType of the field being sorted on.
		/// </summary>
		protected IComparer<TColumnType> Comparer
		{
			get;
			private set;
		}

		/// <summary>
		/// A IValueConverter<![CDATA[<T>]]> object which can be used to derive values from the data.
		/// </summary>
		protected IValueConverter Converter
		{
			get;
			private set;
		}

		/// <summary>
		/// The object to be used as the ConverterParameter for the <see cref="Converter"/>.
		/// </summary>
		protected object ConverterParameter
		{
			get;
			private set;
		}

		#endregion

		#region Methods

		#region Public

		/// <summary>
		/// Sorts a given IQueryable based on the currently sorted column.
		/// </summary>
		/// <param name="query">The IQueryable that should be sorted.</param>		
		/// <returns>A sorted IQueryable, sorted on the currently sorted property.</returns>
		public override IOrderedQueryable<TDataType> Sort<TDataType>(IQueryable<TDataType> query)
		{
			Expression<Func<TDataType, TColumnType>> expr = (Expression<Func<TDataType, TColumnType>>)_lambda;

			if (this.Comparer != null)
			{
				if (SortAscending)
				{
					return query.OrderBy<TDataType, TColumnType>(expr, ((IComparer<TColumnType>)this.Comparer));
				}
				else
				{
					return query.OrderByDescending<TDataType, TColumnType>(expr, this.Comparer);
				}
			}
			else
			{
				if (SortAscending)
				{
					return query.OrderBy<TDataType, TColumnType>(expr);
				}
				else
				{
					return query.OrderByDescending<TDataType, TColumnType>(expr);
				}
			}

		}

		/// <summary>
		/// Appends to this sort to the IOrderedQueryable sort
		/// </summary>
		/// <param name="query">The existing query that already has a sort on it.</param>		
		/// <returns>A sorted IOrderedQueryable, sorted on the currently sorted property.</returns>
		public override IOrderedQueryable<TDataType> AppendSort<TDataType>(IOrderedQueryable<TDataType> query)
		{
			Expression<Func<TDataType, TColumnType>> expr = (Expression<Func<TDataType, TColumnType>>)_lambda;
			if (this.Comparer != null)
			{
				if (SortAscending)
				{
					return query.ThenBy<TDataType, TColumnType>(expr, this.Comparer);
				}
				else
				{
					return query.ThenByDescending<TDataType, TColumnType>(expr, this.Comparer);
				}
			}
			else
			{
				if (SortAscending)
				{
					return query.ThenBy<TDataType, TColumnType>(expr);
				}
				else
				{
					return query.ThenByDescending<TDataType, TColumnType>(expr);
				}
			}
		}

		#endregion //Public

		#endregion //Methods
	}
	#endregion

	internal class ValObj : System.Windows.FrameworkElement
	{
		#region Value

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property. 
		/// </summary>
		public static readonly System.Windows.DependencyProperty ValueProperty = System.Windows.DependencyProperty.Register("Value", typeof(string), typeof(ValObj), new System.Windows.PropertyMetadata(new System.Windows.PropertyChangedCallback(ValueChanged)));

		public string Value
		{
			get { return (string)this.GetValue(ValueProperty); }
			set { this.SetValue(ValueProperty, value); }
		}

		private static void ValueChanged(System.Windows.DependencyObject obj, System.Windows.DependencyPropertyChangedEventArgs e)
		{

		}

		#endregion // Value
	}

	internal class ValObjComparer<T> : IComparer<T>, IEqualityComparer<T>
	{
		ValObj tbx = new ValObj();
		ValObj tby = new ValObj();

		IValueConverter Converter { get; set; }
		object ConverterParameter { get; set; }

		protected internal ValObjComparer(IValueConverter converter, object converterParam)
		{
			this.Converter = converter;
			this.ConverterParameter = converterParam;
		}

		#region IComparer<object> Members

		public int Compare(T x, T y)
		{
			Binding bx = new Binding();
			bx.Converter = this.Converter;
			bx.ConverterParameter = this.ConverterParameter;
			bx.Source = x;
			tbx.SetBinding(ValObj.ValueProperty, bx);

			Binding by = new Binding();
			by.Converter = this.Converter;
			by.ConverterParameter = this.ConverterParameter;
			by.Source = y;
			tby.SetBinding(ValObj.ValueProperty, by);

			return tbx.Value.CompareTo(tby.Value);
		}

		#endregion


		#region IEqualityComparer<T> Members

		bool IEqualityComparer<T>.Equals(T x, T y)
		{
			return this.Compare(x, y) == 0;
		}

		int IEqualityComparer<T>.GetHashCode(T obj)
		{
			Binding bx = new Binding();
			bx.Converter = this.Converter;
			bx.ConverterParameter = this.ConverterParameter;
			bx.Source = obj;
			tbx.SetBinding(ValObj.ValueProperty, bx);

			if (tbx.Value == null)
				return 0;

			return tbx.Value.GetHashCode();
		}

		#endregion
	}

    #region MultiSortComparer

    /// <summary>
    /// Compares items based on list of <see cref="SortContext"/>s. Read remarks.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This comparer is used for binary searching. It doesn't always return a preciese result.
    /// First the items will be compared based on SortContext.(SortProperty and SortAscending).
    /// Then if the items have the same properties they will be compared for equality,
    /// and if they are not equal -1 will be returned.
    /// 
    /// Intended for use with <see cref="List{T}.BinarySearch(T,System.Collections.Generic.IComparer{T})"/>.
    /// </remarks>
    internal class MultiSortComparer<T> : IComparer<T>
    {
        #region Members

        private readonly IList<SortContext> _sortContexts;
        private Func<T, T, int> _compare;
        private bool _isCompiled;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSortComparer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="sortContexts">The sort contexts.</param>
        public MultiSortComparer(IList<SortContext> sortContexts)
        {
            this._sortContexts = sortContexts;
        }

        #endregion // Constructor

        #region IComparer

        public int Compare(T x, T y)
        {
            object x1 = x; // boxing if T is valueType ...
            object y1 = y;

            if (x1 == null && y1 == null)
            {
                return 0;
            }
            else if (x1 == null)
            {
                return -1;
            }
            else if (y1 == null)
            {
                return 1;
            }

            // Defered compilation
            if (!this._isCompiled)
            {
                this.Compile();
            }

            int result = this._compare(x, y);

            if (result == 0)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return 0;
                }

                return -1;
            }

            return result;
        }

        #endregion // IComparer

        #region Compile

        private void Compile()
        {
            ParameterExpression param1 = Expression.Parameter(typeof(T), "p1");
            ParameterExpression param2 = Expression.Parameter(typeof(T), "p2");
            ParameterExpression result = Expression.Variable(typeof(int), "result");

            var comparerBlock = Expression.Block(
                new[] { result },
                BuildCompareExpression(_sortContexts, result, param1, param2),
                result);

            Expression<Func<T, T, int>> lambda = Expression.Lambda<Func<T, T, int>>(comparerBlock, param1, param2);
            this._compare = lambda.Compile();
            this._isCompiled = true;
        }

        #endregion // Compile

        #region BuildCompareExpression

        /// <summary>
        /// Builds compare expression.
        /// </summary>
        /// <param name="sortContexts">The sort contexts.</param>
        /// <param name="resultVar">The result var.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns></returns>
        /// <remarks>
        /// resultVar will contain the result of the comparsion.
        /// -1 if the position of p1 is before p2 in the sorted list.
        /// 0 if p1 and p2 are equal
        /// +1 if the position of p1 is after p2 in the sorted list.
        /// </remarks>
        private Expression BuildCompareExpression(IList<SortContext> sortContexts, Expression resultVar, ParameterExpression p1, ParameterExpression p2)
        {
            int lastSort = sortContexts.Count - 1;

            Expression body = null;

            for (int i = lastSort; i >= 0; i--)
            {
                SortContext sortContext = sortContexts[i];

                if (lastSort == i)
                {
                    body = Expression.Assign(resultVar, BuildCompareCondition(sortContext, p1, p2));
                }
                else
                {
                    // resultVar == 0
                    var test = Expression.Equal(
                        resultVar,
                        Expression.Constant(0, typeof(int)));

                    // resultVar = p1.Prop.ComapreTo(p2.Prop);
                    // if (resultVar == 0) { body }
                    body = Expression.Block(
                        Expression.Assign(resultVar, BuildCompareCondition(sortContext, p1, p2)),
                        Expression.IfThen(
                            test,
                            body));
                }
            }

            return body;
        }

        #endregion // BuildCompareExpression

        #region BuildCompareCondition

        private static Expression BuildCompareCondition(SortContext sortContext, ParameterExpression param1, ParameterExpression param2)
        {
            Expression prop1 = DataManagerBase.BuildPropertyExpressionFromPropertyName(sortContext.SortPropertyName, param1);
            Expression prop2 = DataManagerBase.BuildPropertyExpressionFromPropertyName(sortContext.SortPropertyName, param2);

            Type type = sortContext.PropertyType;

            MethodInfo compreToMethodInfo = type.GetMethod("CompareTo", new[] { type });

            // prop1.CompareTo(prop2)
            Expression then = null;

            if (compreToMethodInfo != null)
            {
                Expression convertedProp2 = prop2;

                if (type.IsEnum)
                {
                    convertedProp2 = Expression.Convert(prop2, typeof(object));
                }

                then = Expression.Call(prop1, compreToMethodInfo, convertedProp2);

                // (prop1 == null) ? -1 : then)
                if (!type.IsValueType)
                {
                    Expression nullProp1Test = Expression.Equal(prop1, Expression.Constant(null, type));
                    System.Linq.Expressions.Expression constExpression = System.Linq.Expressions.Expression.Constant(-1, typeof(int));
                    then = Expression.Condition(nullProp1Test, constExpression, then);
                }                
            }

            // sortContext.Comparer
            MemberExpression comparer = Expression.Property(Expression.Constant(sortContext), "Comparer");

            Type genericIComparerType = typeof(IComparer<>).MakeGenericType(type);
            MethodInfo compareMethodInfo = genericIComparerType.GetMethod("Compare");

            // sortContext.Comparer.Compare(prop1, prop2)
            Expression elze = Expression.Call(comparer, compareMethodInfo, prop1, prop2);

            // sortContext.Comparer == null
            Expression nullComparerTest = Expression.Equal(comparer, Expression.Constant(null, genericIComparerType));
            
            Expression body = null;
            
            // If the type doesn't have CompareTo`1 method, build an expression that uses just the comparer.
            if (then != null)
            {
                body = Expression.Condition(
                    nullComparerTest, // IF: (sortContext.Comparer == null)
                    then,                    //     THEN: (prop1 == null) ? -1 : prop1.CompreTo(prop2)
                    elze                     //     ELSE: sortContext.Comparer.Compare(prop1, prop2)
                );
            }
            else
            {
                body = elze;
            }

            if (!sortContext.SortAscending)
            {
                // If the prop in the list are sorted in descending order then multiply by -1
                body = Expression.Multiply(body, Expression.Constant(-1, typeof(int)));
            }

            return body;
        }

        #endregion // BuildCompareCondition
    }

    #endregion // MultiSortComparer
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