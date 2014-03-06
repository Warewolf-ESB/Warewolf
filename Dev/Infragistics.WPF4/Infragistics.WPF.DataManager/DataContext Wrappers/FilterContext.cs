using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Infragistics
{
	#region FilterContext

	/// <summary>
	/// A nongeneric abstract class representing a filter on an object.
	/// Cast up to CustomFilter<![CDATA[<T>]]> to get an expression representing the filter.
	/// </summary>
	public abstract class FilterContext
	{
		#region Properties
		#region Pubic
		#region CaseSensitive
		/// <summary>
		/// Gets if the filters being built will be case sensitive or not.
		/// </summary>
		public bool CaseSensitive
		{
			get;
			protected set;
		}
		#endregion // CaseSensitive

		#region FieldDataType
		/// <summary>
		/// The <see cref="Type"/> of the field being filtered on.
		/// </summary>
		public Type FieldDataType
		{
			get;
			protected set;
		}
		#endregion // FieldDataType

		#region FromDateColumn
		/// <summary>
		/// Gets if the filter being build is for the Date Column.  If so then we will use some ranging logic when
		/// building filters since the Date column does not support time.
		/// </summary>
		protected bool FromDateColumn
		{
			get;
			set;
		}
		#endregion // FromDateColumn

        #region CachedTypedInfo
        /// <summary>
        /// The CachedTypedInfo for the opeartion
        /// </summary>
        protected CachedTypedInfo CachedTypedInfo
        {
            get;
            set;
        }
        #endregion // CachedTypedInfo

		#endregion //Pubic
		#endregion // Properties

		#region CreateGenericFilter
		/// <summary>
		/// Creates a FilterContext instanced typed to the object type of the data being processed.
		/// </summary>
        /// <param name="cachedTypeInfo">The data object type that will be processed over.</param>
		/// <param name="fieldDataType">The field data type that will be processed on.</param>
		/// <param name="caseSensitive">True if case sensitivity should be applied.  Only used for string fieldDataTypes.</param>
		/// <param name="fromDateColumn">True if this filter was created by a DateColumn and will perform some extra actions to filter out time aspect.</param>
		/// <returns></returns>
		public static FilterContext CreateGenericFilter(CachedTypedInfo cachedTypeInfo, Type fieldDataType, bool caseSensitive, bool fromDateColumn)
		{
            Type objectType = cachedTypeInfo.CachedType;
			System.Type specificFilterType = typeof(FilterContext<>).MakeGenericType(new System.Type[] { objectType });
            return (FilterContext)Activator.CreateInstance(specificFilterType, new object[] { fieldDataType, caseSensitive, fromDateColumn, cachedTypeInfo });
		}
		#endregion //CreateGenericFilter

		#region CreateExpression

		/// <summary>
		/// Creates a new <see cref="Expression"/>
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="op"></param>
		/// <param name="value"></param>
		/// <param name="caseSensitive"></param>
		/// <returns></returns>
		public virtual Expression CreateExpression(string fieldName, ComparisonOperator op, object value, bool caseSensitive)
		{
			return null;
		}

		/// <summary>
		/// Creates a new <see cref="Expression"/>
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="op"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual Expression CreateExpression(string fieldName, ComparisonOperator op, object value)
		{
			return null;
		}

		/// <summary>
		/// Creates a new <see cref="Expression"/>
		/// </summary>
		/// <param name="conditionGroup"></param>
		/// <returns></returns>
		public virtual Expression CreateExpression(ConditionCollection conditionGroup)
		{
			return null;
		}

		/// <summary>
		/// Creates a new <see cref="Expression"/>
		/// </summary>
		/// <param name="conditionGroup"></param>
		/// <returns></returns>
		public virtual Expression CreateExpression(RecordFilterCollection conditionGroup)
		{
			return null;
		}
		#endregion // CreateExpression

        internal abstract Expression And(Expression left, Expression right);
        internal abstract Expression Or(Expression left, Expression right);     
	}

	#endregion // FilterContext

	#region FilterContext<TDataObject>
	/// <summary>
	/// A <see cref="FilterContext"/> object typed to a particular object.
	/// </summary>
	/// <typeparam name="TDataObject">The type of the object that will be processed against.</typeparam>
	public class FilterContext<TDataObject> : FilterContext
	{
        protected readonly static MethodInfo EqualsMethod = typeof(object).GetMethod("Equals", new Type[] { typeof(System.Object) });

		#region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterContext"/> class.
        /// </summary>
		/// <param name="fieldDataType"></param>
		/// <param name="caseSensitive"></param>
		/// <param name="fromDateColumn"></param>
		/// <param name="cti"></param>
		public FilterContext(Type fieldDataType, bool caseSensitive, bool fromDateColumn, CachedTypedInfo cti)
		{
			this.CaseSensitive = caseSensitive;
			this.FieldDataType = fieldDataType;
			this.FromDateColumn = fromDateColumn;
            this.CachedTypedInfo = cti;
		}
		#endregion // Constructor

        internal override Expression And(Expression left, Expression right)
        {
            Expression<Func<TDataObject, bool>> castedLeft = (Expression<Func<TDataObject, bool>>)left;
            Expression<Func<TDataObject, bool>> castedright = (Expression<Func<TDataObject, bool>>)right;
            return this.AndAlsoExpression<TDataObject>(castedLeft, castedright);
        }

        internal override Expression Or(Expression left, Expression right)
        {
            Expression<Func<TDataObject, bool>> castedLeft = (Expression<Func<TDataObject, bool>>)left;
            Expression<Func<TDataObject, bool>> castedright = (Expression<Func<TDataObject, bool>>)right;
            return this.OrElseExpression<TDataObject>(castedLeft, castedright);
        }

		#region Logical Operators
		/// <summary>
		/// Combines two <see cref="Expression"/> objects with a OR Expression.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>An <see cref="Expression"/> which is the combination of the two inputs.</returns>
		protected internal Expression<Func<TDataObject, bool>> OrElseExpression<TDataObjectType>(Expression<Func<TDataObject, bool>> left, Expression<Func<TDataObject, bool>> right)
		{
			InvocationExpression invokeExpression = Expression.Invoke(right, left.Parameters.Cast<Expression>());
            Expression body = Expression.OrElse(left.Body, invokeExpression);

            return Expression.Lambda<Func<TDataObject, bool>>(body, left.Parameters);
		}

		/// <summary>
		/// Combines two <see cref="Expression"/> objects with a AND Expression.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>An <see cref="Expression"/> which is the combination of the two inputs.</returns>
		protected internal Expression<Func<TDataObject, bool>> AndAlsoExpression<TDataObjectType>(Expression<Func<TDataObject, bool>> left, Expression<Func<TDataObject, bool>> right)
		{
			InvocationExpression invokeExpression = Expression.Invoke(right, left.Parameters.Cast<Expression>());
            Expression body = Expression.AndAlso(left.Body, invokeExpression);

            return Expression.Lambda<Func<TDataObject, bool>>(body, left.Parameters);
		}

		/// <summary>
		/// Combines two <see cref="Expression"/> objects with a XOR Expression.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>An <see cref="Expression"/> which is the combination of the two inputs.</returns>
		protected internal Expression<Func<TDataObject, bool>> ExclusiveOrExpression<TDataObjectType>(Expression<Func<TDataObject, bool>> left, Expression<Func<TDataObject, bool>> right)
		{
			InvocationExpression invokeExpression = Expression.Invoke(right, left.Parameters.Cast<Expression>());
            Expression body = Expression.ExclusiveOr(left.Body, invokeExpression);

			return Expression.Lambda<Func<TDataObject, bool>>(body, left.Parameters);
		}
		#endregion  // Logical Operators

		#region Equals / NotEquals
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for equality.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <param name="value">The value that will be analyzed against.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateDateTimeTimeExcludedEqualsExpression<TDataObjectType>(string fieldName, object value)
        {
            ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TDataObjectType), "parameter");
            Expression body, left, right = null;
            left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
            right = Expression.Constant(value, this.FieldDataType);

            if (value != null)
            {
                Expression greaterThanEqual = Expression.GreaterThanOrEqual(left, right);
                DateTime dateValue = (DateTime)value;
                dateValue = dateValue.AddDays(1);
                right = Expression.Constant(dateValue, this.FieldDataType);
                Expression lessThanEqual = Expression.LessThan(left, right);
                body = Expression.AndAlso(greaterThanEqual, lessThanEqual);
            }
            else
            {
                body = Expression.Equal(left, right);
            }

            if (!typeof(TDataObject).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
        }


		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for equality.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateEqualsExpression<TDataObjectType>(string fieldName, object value)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TDataObjectType), "parameter");
			Expression body, left, right = null;
			left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
			right = Expression.Constant(value, this.FieldDataType);

			if (this.FromDateColumn && value != null)
			{
				Expression greaterThanEqual = Expression.GreaterThanOrEqual(left, right);
				DateTime dateValue = (DateTime)value;
				dateValue = dateValue.AddDays(1);
				right = Expression.Constant(dateValue, this.FieldDataType);
				Expression lessThanEqual = Expression.LessThan(left, right);
				body = Expression.AndAlso(greaterThanEqual,lessThanEqual);
			}
			else
			{
                if (!left.Type.IsValueType)
                {
                    Expression eql = Expression.Equal(left, Expression.Constant(null, left.Type));
                    body = Expression.Condition(eql, Expression.Equal(left, right), Expression.Call(left, EqualsMethod, new Expression[] { right }));
                }
                else
                {
                    body = Expression.Equal(left, right);
                }
			}

            if (!typeof(TDataObject).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for inequality.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateNotEqualsExpression<TDataObjectType>(string fieldName, object value)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TDataObjectType), "parameter");
			Expression body, left, right = null;
                       
            left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
			right = Expression.Constant(value, this.FieldDataType);

			if (this.FromDateColumn && value != null)
			{				
				Expression greaterThanEqual = Expression.GreaterThanOrEqual(left, right);
				DateTime dateValue = (DateTime)value;
				dateValue = dateValue.AddDays(1);
				right = Expression.Constant(dateValue, this.FieldDataType);
				Expression lessThanEqual = Expression.LessThan(left, right);
				body = Expression.AndAlso(greaterThanEqual, lessThanEqual);
				body = Expression.Not(body);
			}
			else
			{
                if (!left.Type.IsValueType)
                {
                    Expression eql = Expression.Equal(left, Expression.Constant(null, left.Type));
                    body = Expression.Condition(eql, Expression.NotEqual(left, right), Expression.Not(Expression.Call(left, EqualsMethod, new Expression[] { right })));
                }
                else
                {
                    body = Expression.NotEqual(left, right);
                }
			}

            if (!typeof(TDataObject).IsValueType)
			{
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
		}
		#endregion // Equals / NotEquals

		#region Simple GreaterThan / LessThan Numerics
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for greater than the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateGreaterThanExpression<TDataObjectType>(string fieldName, object value)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TDataObject), "parameter");
			Expression body, left, right = null;
            left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
			right = Expression.Constant(value, this.FieldDataType);

			if (this.FromDateColumn && value != null)
			{
				DateTime dateValue = (DateTime)value;
				dateValue = dateValue.AddDays(1);
				right = Expression.Constant(dateValue, this.FieldDataType);
				Expression greaterThanEqual = Expression.GreaterThanOrEqual(left, right);
				body = greaterThanEqual;				
			}
			else
			{
				body = Expression.GreaterThan(left, right);
			}

            if (!typeof(TDataObject).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for greater than or equal the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateGreaterThanOrEqualsExpression<TDataObjectType>(string fieldName, object value)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TDataObject), "parameter");
			Expression body, left, right = null;
            left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
			right = Expression.Constant(value, this.FieldDataType);
			body = Expression.GreaterThanOrEqual(left, right);

            if (!typeof(TDataObject).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for less than the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateLessThanExpression<TDataObjectType>(string fieldName, object value)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TDataObject), "parameter");
			Expression body, left, right = null;
			left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
			right = Expression.Constant(value, this.FieldDataType);
			body = Expression.LessThan(left, right);

            if (!typeof(TDataObject).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
		}


		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for less than or equal the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateLessThanOrEqualsExpression<TDataObjectType>(string fieldName, object value)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TDataObject), "parameter");
			Expression body, left, right = null;
			left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
			right = Expression.Constant(value, this.FieldDataType);

			if (this.FromDateColumn && value != null)
			{
				DateTime dateValue = (DateTime)value;
				dateValue = dateValue.AddDays(1);
				right = Expression.Constant(dateValue, this.FieldDataType);
				Expression lessThanEqual = Expression.LessThan(left, right);
				body = lessThanEqual;
			}
			else
			{
				body = Expression.LessThanOrEqual(left, right);
			}

            if (!typeof(TDataObject).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
		}
		#endregion //  Simple GreaterThan / LessThan Numerics



#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


        #region String Expressions

		#region CreateStringExpression
		/// <summary>
		/// Builds an expression for analyzing string
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="booleanStringExpression">A <![CDATA[Expression<Func<string, bool>>]]> which will analyze a string.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringExpression<TDataObjectType>(string fieldName, Expression<Func<string, bool>> booleanStringExpression)
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(TDataObject), "parameter");
			Expression left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
			Expression body = Expression.Invoke(booleanStringExpression, left);

            if (!typeof(TDataObject).IsValueType)
		    {
		        Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
		        body = Expression.Condition(equalExpression, Expression.Constant(false), body);
		    }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
		}
		
		#endregion // CreateStringExpression

		#region Equals / NotEqual (CaseInsensitive)

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive equals against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateEqualsCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				// AS 3/16/12
				// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
				//
			    //value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
				//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.ToUpper(System.Globalization.CultureInfo.CurrentCulture) == value);
				return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.Equals(value, StringComparison.CurrentCultureIgnoreCase));
			}
			else
			{
			    return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => string.IsNullOrEmpty(x));
			}
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive inequality against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateNotEqualsCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				// AS 3/16/12
				// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
				//
				//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || x.ToUpper(System.Globalization.CultureInfo.CurrentCulture) != value);
				//value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
				return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || !x.Equals(value, StringComparison.CurrentCultureIgnoreCase));
			}
			else
			{
				return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => !string.IsNullOrEmpty(x));
			}
		}
		#endregion

		#region StartsWith
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive starts with against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStartsWithExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.StartsWith(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive starts with against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStartsWithCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).StartsWith(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.StartsWith(value, StringComparison.CurrentCultureIgnoreCase));
		}

		#endregion // StartsWith

		#region EndsWith
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive ends with against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateEndsWithExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.EndsWith(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive ends with against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateEndsWithCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).EndsWith(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.EndsWith(value, StringComparison.CurrentCultureIgnoreCase));
		}
		#endregion //  EndsWith

		#region Contains
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive contains against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateContainsExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.Contains(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive contains against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateContainsCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).Contains(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && x.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) >= 0);
		}
		#endregion // Contains

		#region DoesNotContain
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive does not contain against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateDoesNotContainExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || !x.Contains(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive does not contain against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateDoesNotContainCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || !x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).Contains(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || x.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) == -1);
		}
		#endregion // DoesNotContain

		#region GreaterThan
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive greater than against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringGreaterThanExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && 1 == x.CompareTo(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive greater than against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringGreaterThanCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && 1 == x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).CompareTo(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && 1 == StringComparer.CurrentCultureIgnoreCase.Compare(x, value));
		}
		#endregion // GreaterThan

		#region GreaterThanOrEqual
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive greater than or equal against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringGreaterThanOrEqualExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && -1 != x.CompareTo(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive greater than or equal against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringGreaterThanOrEqualCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && -1 != x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).CompareTo(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x != null && -1 != StringComparer.CurrentCultureIgnoreCase.Compare(x, value));
		}
		#endregion // GreaterThanOrEqual

		#region LessThan
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive less than against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringLessThanExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || -1 == x.CompareTo(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive less than against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringLessThanCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || -1 == x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).CompareTo(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || -1 == StringComparer.CurrentCultureIgnoreCase.Compare(x, value));
		}
		#endregion // LessThan

		#region LessThanOrEqual
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive less than or equal against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringLessThanOrEqualExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || 1 != x.CompareTo(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive less than or equal against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringLessThanOrEqualCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || 1 != x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).CompareTo(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || 1 != StringComparer.CurrentCultureIgnoreCase.Compare(x, value));
		}
		#endregion // LessThanOrEqual

		#region EmptyString / NotEmpty
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for empty strings values.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringEmptyExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => string.IsNullOrEmpty(x));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for nonempty strings values.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateStringNotEmptyExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => !string.IsNullOrEmpty(x));
		}
		#endregion // EmptyString

		#region DoesNotStartsWith
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive does not starts with against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateDoesNotStartWithExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || !x.StartsWith(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive does not starts with against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateDoesNotStartWithCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || !x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).StartsWith(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || !x.StartsWith(value, StringComparison.CurrentCultureIgnoreCase));
		}

		#endregion // DoesNotStartsWith

		#region DoesNotEndsWith
		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case sensitive does not ends with against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateDoesNotEndWithExpression<TDataObjectType>(string fieldName, string value)
		{
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || !x.EndsWith(value));
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> that evaluates for case insensitive does not ends with against the inputted value.
		/// </summary>
		/// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
		/// <param name="fieldName">The property that will be evaluated on.</param>
		/// <param name="value">The value that will be analyzed against.</param>
		/// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
		protected internal virtual Expression<Func<TDataObject, bool>> CreateDoesNotEndWithCaseInsensitiveExpression<TDataObjectType>(string fieldName, string value)
		{
			// AS 3/16/12
			// Avoid ToUpper to avoid unnecessary string allocation for comparison purposes.
			//
			//if (!string.IsNullOrEmpty(value))
			//{
			//    value = value.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
			//}
			//return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || !x.ToUpper(System.Globalization.CultureInfo.CurrentCulture).EndsWith(value));
			return (Expression<Func<TDataObject, bool>>)this.CreateStringExpression<TDataObject>(fieldName, (x) => x == null || !x.EndsWith(value, StringComparison.CurrentCultureIgnoreCase));
		}
		#endregion //  DoesNotEndsWith

		#endregion // String Expressions

		#region CreateExpression
		/// <summary>
		/// Creates an <see cref="Expression"/> based on all the terms in the <see cref="ConditionCollection"/>.
		/// </summary>
		/// <param name="conditionGroup"></param>
		/// <returns></returns>
		public override Expression CreateExpression(ConditionCollection conditionGroup)
		{
			Expression<Func<TDataObject, bool>> left = null, right = null;

			if (conditionGroup.Count > 0)
			{
				left = (Expression<Func<TDataObject, bool>>)conditionGroup[0].GetCurrentExpression(this);

				for (int i = 1; i < conditionGroup.Count; i++)
				{
					right = (Expression<Func<TDataObject, bool>>)conditionGroup[i].GetCurrentExpression(this);
					if (conditionGroup.LogicalOperator == LogicalOperator.Or)
						left = this.OrElseExpression<TDataObject>(left, right);
					else
						left = this.AndAlsoExpression<TDataObject>(left, right);
				}
			}
			return left;
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> from the <see cref="RecordFilterCollection"/>
		/// </summary>
		/// <param name="conditionGroup"></param>
		/// <returns></returns>
		public override Expression CreateExpression(RecordFilterCollection conditionGroup)
		{
			Expression<Func<TDataObject, bool>> left = null, right = null;

			if (conditionGroup.Count > 0)
			{
				left = (Expression<Func<TDataObject, bool>>)conditionGroup[0].GetCurrentExpression();

				for (int i = 1; i < conditionGroup.Count; i++)
				{
					right = (Expression<Func<TDataObject, bool>>)conditionGroup[i].GetCurrentExpression();
					if (left == null)
					{
						left = right;
						continue;
					}
					if (right != null)
					{
						if (conditionGroup.LogicalOperator == LogicalOperator.Or)
							left = this.OrElseExpression<TDataObject>(left, right);
						else
							left = this.AndAlsoExpression<TDataObject>(left, right);
					}
				}
			}
			return left;
		}

		/// <summary>
		/// Creates an expression based on the <see cref="ComparisonOperator"/>.
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="op"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override Expression CreateExpression(string fieldName, ComparisonOperator op, object value)
		{
			if (this.FieldDataType == typeof(string))
			{
				if (this.CaseSensitive)
					return this.CreateCaseSensitiveStringExpression(fieldName, op, value);
				else
					return this.CreateCaseInsensitiveStringExpression(fieldName, op, value);
			}
			return CreateObjectExpression(fieldName, op, value);
		}


		/// <summary>
		/// Creates a new <see cref="Expression"/>
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="op"></param>
		/// <param name="value"></param>
		/// <param name="caseSensitive"></param>
		/// <returns></returns>
		public override Expression CreateExpression(string fieldName, ComparisonOperator op, object value, bool caseSensitive)
		{
			if (this.FieldDataType == typeof(string))
			{
				if (caseSensitive)
					return this.CreateCaseSensitiveStringExpression(fieldName, op, value);
				else
					return this.CreateCaseInsensitiveStringExpression(fieldName, op, value);
			}
			return CreateObjectExpression(fieldName, op, value);
		}

		#endregion // CreateExpression

		#region CreateCaseInsensitiveStringExpression

		/// <summary>
		/// Returns an <see cref="Expression"/> for a case insensitive string operation.
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="op"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected internal Expression CreateCaseInsensitiveStringExpression(string fieldName, ComparisonOperator op, object value)
		{
			string stringConvertedValue = (string)value;

			Expression returnExpression = null;

			switch (op)
			{
				case (ComparisonOperator.Equals):
					{
						returnExpression = CreateEqualsCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.NotEquals):
					{
						returnExpression = CreateNotEqualsCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.StartsWith):
					{
						returnExpression = CreateStartsWithCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.DoesNotStartWith):
					{
						returnExpression = CreateDoesNotStartWithCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.EndsWith):
					{
						returnExpression = CreateEndsWithCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.DoesNotEndWith):
					{
						returnExpression = CreateDoesNotEndWithCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.Contains):
					{
						returnExpression = CreateContainsCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.DoesNotContain):
					{
						returnExpression = CreateDoesNotContainCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.GreaterThan):
					{
						returnExpression = CreateStringGreaterThanCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.GreaterThanOrEqual):
					{
						returnExpression = CreateStringGreaterThanOrEqualCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.LessThan):
					{
						returnExpression = CreateStringLessThanCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.LessThanOrEqual):
					{
						returnExpression = CreateStringLessThanOrEqualCaseInsensitiveExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
			}
			return returnExpression;
		}

		#endregion // CreateCaseInsensitiveStringExpression

		#region CreateCaseSensitiveStringExpression

		/// <summary>
		/// Returns an <see cref="Expression"/> for a case sensitive string operation.
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="op"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected internal Expression CreateCaseSensitiveStringExpression(string fieldName, ComparisonOperator op, object value)
		{
			string stringConvertedValue = (string)value;

			Expression returnExpression = null;

			switch (op)
			{
				case (ComparisonOperator.Equals):
					{
						returnExpression = CreateEqualsExpression<TDataObject>(fieldName, value);
						break;
					}
				case (ComparisonOperator.NotEquals):
					{
						returnExpression = CreateNotEqualsExpression<TDataObject>(fieldName, value);
						break;
					}
				case (ComparisonOperator.StartsWith):
					{
						returnExpression = CreateStartsWithExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.EndsWith):
					{
						returnExpression = CreateEndsWithExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.DoesNotStartWith):
					{
						returnExpression = CreateDoesNotStartWithExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.DoesNotEndWith):
					{
						returnExpression = CreateDoesNotEndWithExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.Contains):
					{
						returnExpression = CreateContainsExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.DoesNotContain):
					{
						returnExpression = CreateDoesNotContainExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.GreaterThan):
					{
						returnExpression = CreateStringGreaterThanExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.GreaterThanOrEqual):
					{
						returnExpression = CreateStringGreaterThanOrEqualExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.LessThan):
					{
						returnExpression = CreateStringLessThanExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
				case (ComparisonOperator.LessThanOrEqual):
					{
						returnExpression = CreateStringLessThanOrEqualExpression<TDataObject>(fieldName, stringConvertedValue);
						break;
					}
			}
			return returnExpression;
		}

		#endregion // CreateCaseSensitiveStringExpression

		#region CreateObjectExpression

		/// <summary>
		/// Returns an <see cref="Expression"/> for a object operation.
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="op"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected internal Expression CreateObjectExpression(string fieldName, ComparisonOperator op, object value)
		{
			Expression returnExpression = null;

            switch (op)
            {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                case (ComparisonOperator.Equals):
                    {
                        returnExpression = CreateEqualsExpression<TDataObject>(fieldName, value);
                        break;
                    }
                case (ComparisonOperator.NotEquals):
                    {
                        returnExpression = CreateNotEqualsExpression<TDataObject>(fieldName, value);
                        break;
                    }
                case (ComparisonOperator.GreaterThan):
                    {
                        returnExpression = CreateGreaterThanExpression<TDataObject>(fieldName, value);
                        break;
                    }
                case (ComparisonOperator.GreaterThanOrEqual):
                    {
                        returnExpression = CreateGreaterThanOrEqualsExpression<TDataObject>(fieldName, value);
                        break;
                    }
                case (ComparisonOperator.LessThan):
                    {
                        returnExpression = CreateLessThanExpression<TDataObject>(fieldName, value);
                        break;
                    }
                case (ComparisonOperator.LessThanOrEqual):
                    {
                        returnExpression = CreateLessThanOrEqualsExpression<TDataObject>(fieldName, value);
                        break;
                    }
                case (ComparisonOperator.DateTimeAfter):
                    {
                        returnExpression = CreateGreaterThanExpression<TDataObject>(fieldName, value);
                        break;
                    }
                case (ComparisonOperator.DateTimeBefore):
                    {
                        returnExpression = CreateLessThanExpression<TDataObject>(fieldName, value);
                        break;
                    }
                case (ComparisonOperator.DateTimeToday):
                    {
                        returnExpression = CreateDateTimeTimeExcludedEqualsExpression<TDataObject>(fieldName, DateTime.Today);
                        break;
                    }
                case (ComparisonOperator.DateTimeTomorrow):
                    {
                        returnExpression = CreateDateTimeTimeExcludedEqualsExpression<TDataObject>(fieldName, DateTime.Today.AddDays(1));
                        break;
                    }
                case (ComparisonOperator.DateTimeYesterday):
                    {
                        returnExpression = CreateDateTimeTimeExcludedEqualsExpression<TDataObject>(fieldName, DateTime.Today.AddDays(-1));
                        break;
                    }
                case (ComparisonOperator.DateTimeThisWeek):
                    {
                        returnExpression = CreateThisWeekExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeLastWeek):
                    {
                        returnExpression = CreateLastWeekExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeNextWeek):
                    {
                        returnExpression = CreateNextWeekExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeThisMonth):
                    {
                        returnExpression = CreateThisMonthExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeLastMonth):
                    {
                        returnExpression = CreateLastMonthExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeNextMonth):
                    {
                        returnExpression = CreateNextMonthExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeThisYear):
                    {
                        returnExpression = CreateThisYearExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeLastYear):
                    {
                        returnExpression = CreateLastYearExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeNextYear):
                    {
                        returnExpression = CreateNextYearExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeThisQuarter):
                    {
                        returnExpression = CreateThisQuarterExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeLastQuarter):
                    {
                        returnExpression = CreateLastQuarterExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeNextQuarter):
                    {
                        returnExpression = CreateNextQuarterExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeYearToDate):
                    {
                        returnExpression = CreateYearToDateExpression<TDataObject>(fieldName);
                        break;
                    }
                case (ComparisonOperator.DateTimeJanuary):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 1);
                        break;
                    }
                case (ComparisonOperator.DateTimeFebruary):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 2);
                        break;
                    }
                case (ComparisonOperator.DateTimeMarch):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 3);
                        break;
                    }
                case (ComparisonOperator.DateTimeApril):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 4);
                        break;
                    }
                case (ComparisonOperator.DateTimeMay):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 5);
                        break;
                    }
                case (ComparisonOperator.DateTimeJune):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 6);
                        break;
                    }
                case (ComparisonOperator.DateTimeJuly):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 7);
                        break;
                    }
                case (ComparisonOperator.DateTimeAugust):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 8);
                        break;
                    }
                case (ComparisonOperator.DateTimeSeptember):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 9);
                        break;
                    }
                case (ComparisonOperator.DateTimeOctober):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 10);
                        break;
                    }
                case (ComparisonOperator.DateTimeNovember):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 11);
                        break;
                    }
                case (ComparisonOperator.DateTimeDecember):
                    {
                        returnExpression = CreateMonthExpression<TDataObject>(fieldName, 12);
                        break;
                    }
                case (ComparisonOperator.DateTimeQuarter1):
                    {                        
                        returnExpression = CreateQuarterYearIndependentExpression<TDataObject>(fieldName, 1);
                        break;
                    }
                case (ComparisonOperator.DateTimeQuarter2):
                    {
                        returnExpression = CreateQuarterYearIndependentExpression<TDataObject>(fieldName, 2);
                        break;
                    }
                case (ComparisonOperator.DateTimeQuarter3):
                    {                        
                        returnExpression = CreateQuarterYearIndependentExpression<TDataObject>(fieldName, 3);
                        break;
                    }
                case (ComparisonOperator.DateTimeQuarter4):
                    {                        
                        returnExpression = CreateQuarterYearIndependentExpression<TDataObject>(fieldName, 4);
                        break;
                    }
            }
			return returnExpression;
		}



#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)


        #endregion // CreateObjectExpression

        #region DateTime Filters

        #region CreateThisWeekExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for this week.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateThisWeekExpression<TDataObjectType>(string fieldName)
        {
            DateTime sundayThisWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

            sundayThisWeek = new DateTime(sundayThisWeek.Year, sundayThisWeek.Month, sundayThisWeek.Day);
            
            DateTime sundayNextWeek = sundayThisWeek.AddDays(7);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, sundayThisWeek, sundayNextWeek);            
        }
        #endregion // CreateThisWeekExpression

        #region CreateNextWeekExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for next week.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateNextWeekExpression<TDataObjectType>(string fieldName)
        {
            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            startDate = startDate.AddDays(7);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, startDate, startDate.AddDays(7));
        }
        #endregion // CreateNextWeekExpression

        #region CreateLastWeekExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for last week.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateLastWeekExpression<TDataObjectType>(string fieldName)
        {
            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            startDate = startDate.AddDays(-7);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, startDate, startDate.AddDays(7));
        }
        #endregion // CreateLastWeekExpression

        #region CreateThisMonthExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for this Month.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateThisMonthExpression<TDataObjectType>(string fieldName)
        {
            DateTime startofMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            DateTime nextMonthValue = DateTime.Today.AddMonths(1);

            DateTime startOfAfterMonth = new DateTime(nextMonthValue.Year, nextMonthValue.Month, 1);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, startofMonth, startOfAfterMonth);
        }
        #endregion // CreateThisMonthExpression

        #region CreateNextMonthExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for next Month.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateNextMonthExpression<TDataObjectType>(string fieldName)
        {
            DateTime nextMonthValue = DateTime.Today.AddMonths(1);

            DateTime startofMonth = new DateTime(nextMonthValue.Year, nextMonthValue.Month, 1);

            nextMonthValue = nextMonthValue.AddMonths(1);

            DateTime startOfAfterMonth = new DateTime(nextMonthValue.Year, nextMonthValue.Month, 1);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, startofMonth, startOfAfterMonth);
        }
        #endregion // CreateNextMonthExpression

        #region CreateLastMonthExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for last Month.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateLastMonthExpression<TDataObjectType>(string fieldName)
        {
            DateTime startOfAfterMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
           
            DateTime nextMonthValue = DateTime.Today.AddMonths(-1);

            DateTime startofMonth   = new DateTime(nextMonthValue.Year, nextMonthValue.Month, 1);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, startofMonth, startOfAfterMonth);
        }
        #endregion // CreateLastMonthExpression

        #region CreateThisYearExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for this Year.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateThisYearExpression<TDataObjectType>(string fieldName)
        {
            DateTime startofYear = new DateTime(DateTime.Today.Year, 1, 1);            

            DateTime startOfAfterYear = new DateTime(DateTime.Today.Year + 1, 1, 1);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, startofYear, startOfAfterYear);
        }
        #endregion // CreateThisYearExpression

        #region CreateNextYearExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for next Year.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateNextYearExpression<TDataObjectType>(string fieldName)
        {
            DateTime startofYear = new DateTime(DateTime.Today.Year + 1, 1, 1);

            DateTime startOfAfterYear = new DateTime(DateTime.Today.Year + 2, 1, 1);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, startofYear, startOfAfterYear);
        }
        #endregion // CreateNextYearExpression

        #region CreateLastYearExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for last Year.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateLastYearExpression<TDataObjectType>(string fieldName)
        {
            DateTime startOfAfterYear = new DateTime(DateTime.Today.Year, 1, 1);

            DateTime startofYear = new DateTime(DateTime.Today.Year - 1, 1, 1);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, startofYear, startOfAfterYear);
        }
        #endregion // CreateLastYearExpression

        #region CreateThisQuarterExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for this Quarter.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateThisQuarterExpression<TDataObjectType>(string fieldName)
        {
            QuarterTracking tracker = GetCurrentQuarterInfo(DateTime.Today);
            return this.CreateDateRangeExpression<TDataObject>(fieldName, tracker.StartDate, tracker.EndDate);
        }
        #endregion // CreateThisQuarterExpression

        #region CreateNextQuarterExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for next Quarter.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateNextQuarterExpression<TDataObjectType>(string fieldName)
        {
            QuarterTracking tracker = GetNextQuarterInfo(DateTime.Today);
            return this.CreateDateRangeExpression<TDataObject>(fieldName, tracker.StartDate, tracker.EndDate);
        }
        #endregion // CreateNextQuarterExpression

        #region CreateLastQuarterExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for last Quarter.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateLastQuarterExpression<TDataObjectType>(string fieldName)
        {
            QuarterTracking tracker = GetPreviousQuarterInfo(DateTime.Today);
            return this.CreateDateRangeExpression<TDataObject>(fieldName, tracker.StartDate, tracker.EndDate);
        }
        #endregion // CreateLastQuarterExpression

        #region CreateYearToDateExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for year to date.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateYearToDateExpression<TDataObjectType>(string fieldName)
        {
            DateTime startofYear = new DateTime(DateTime.Today.Year, 1, 1);

            DateTime startOfAfterYear = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);

            startOfAfterYear = startOfAfterYear.AddDays(1);

            return this.CreateDateRangeExpression<TDataObject>(fieldName, startofYear, startOfAfterYear);
        }
        #endregion // CreateLastYearExpression

        #region CreateDateRangeExpression
        /// <summary>
        /// Creates an <see cref="Expression"/> that evaluates for date ranges.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <param name="includeStartDate"></param>
        /// <param name="excludedEndDate"></param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateDateRangeExpression<TDataObjectType>(string fieldName, DateTime includeStartDate, DateTime excludedEndDate)
        {
            ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TDataObjectType), "parameter");
            Expression body, left, right = null;
            left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
            right = Expression.Constant(includeStartDate, this.FieldDataType);

            Expression greaterThanEqual = Expression.GreaterThanOrEqual(left, right);
            right = Expression.Constant(excludedEndDate, this.FieldDataType);
            Expression lessThanEqual = Expression.LessThan(left, right);
            body = Expression.AndAlso(greaterThanEqual, lessThanEqual);

            if (!typeof(TDataObject).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
        }
        #endregion // CreateDateRangeExpression    
 
        #region CreateMonthExpression

        /// <summary>
        /// Creates an <see cref="Expression"/> that will evaluate an object's DateTime field for a particular month value.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName">The property that will be evaluated on.</param>
        /// <param name="month">The int month value which will be filterd for.</param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateMonthExpression<TDataObjectType>(string fieldName, int month)
        {
            Expression dateTimeExpression = null;
            if (this.FieldDataType == typeof(DateTime))
            {
                Expression<Func<DateTime, bool>> datetimeStringExpression = (x) => x.Month == month;
                dateTimeExpression = datetimeStringExpression;
            }
            else
            {
                Expression<Func<DateTime?, bool>> datetimeStringExpression = (x) => x != null && ((DateTime)x).Month == month;
                dateTimeExpression = datetimeStringExpression;
            }
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TDataObject), "parameter");
            Expression left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
            Expression body = Expression.Invoke(dateTimeExpression, left);

            if (!typeof(TDataObject).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
        }
        #endregion // CreateMonthExpression

        #region CreateQuarterYearIndependentExpression

        /// <summary>
        /// Creates an <see cref="Expression"/> that will evaluate an object's DateTime field for a particular quarter.
        /// </summary>
        /// <typeparam name="TDataObjectType">The object type that the <see cref="Expression"/> will be applied over.</typeparam>
        /// <param name="fieldName"></param>
        /// <param name="quarter"></param>
        /// <returns>An <see cref="Expression"/> which will evaluate for the condition.</returns>
        protected internal virtual Expression<Func<TDataObject, bool>> CreateQuarterYearIndependentExpression<TDataObjectType>(string fieldName, int quarter)
        {
            Expression dateTimeExpression = null;
            if (this.FieldDataType == typeof(DateTime))
            {
                Expression<Func<DateTime, bool>> datetimeStringExpression = (x) => IsDateValueInQuarter(x, quarter);
                dateTimeExpression = datetimeStringExpression;
            }
            else
            {
                Expression<Func<DateTime?, bool>> datetimeStringExpression = (x) => x != null && IsDateValueInQuarter((DateTime)x, quarter);
                dateTimeExpression = datetimeStringExpression;
            }
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TDataObject), "parameter");
            Expression left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.CachedTypedInfo, this.FieldDataType, this.GetDefaultValue());
            Expression body = Expression.Invoke(dateTimeExpression, left);

            if (!typeof(TDataObject).IsValueType)
            {
                Expression equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(TDataObject)));
                body = Expression.Condition(equalExpression, Expression.Constant(false), body);
            }

            return Expression.Lambda<Func<TDataObject, bool>>(body, parameterExpression);
        }

        #endregion // CreateQuarterYearIndependentExpression

        internal bool IsDateValueInQuarter(DateTime value, int quarter)
        {
            int startMonth = 1;
            int endMonth = 1;
            int startYear = 1;
            int endYear = 1;

            switch (quarter)
            {
                case (1):
                    {
                        startMonth = 1;
                        endMonth = 4;
                        endYear =  startYear = value.Year;
                        break;
                    }
                case (2):
                    {
                        startMonth = 4;
                        endMonth = 7;
                        endYear = startYear = value.Year;
                        break;
                    }
                case (3):
                    {
                        startMonth = 7;
                        endMonth = 10;
                        endYear = startYear = value.Year;
                        break;
                    }
                case (4):
                    {
                        startMonth = 10;
                        endMonth = 1;
                        endYear = startYear = value.Year;
                        endYear++;
                        break;
                    }
            }

            // first we take the quarter info and convert it with the year value of the date 
            DateTime startDate = new DateTime(startYear, startMonth, 1);

            DateTime endDate = new DateTime(endYear, endMonth, 1);

            return (value >= startDate && value < endDate);
        }

        #endregion // DateTime Filters

        #region GetCurrentQuarterInfo
        private QuarterTracking GetCurrentQuarterInfo(DateTime date)
        {
            int month = date.Month;
            int year = date.Year;
            QuarterTracking tracker = new QuarterTracking();

            if (month <= 3)
            {
                tracker.StartDate = new DateTime(year, 1, 1);
                tracker.EndDate = new DateTime(year, 4, 1);
            }
            else if (month <= 6)
            {
                tracker.StartDate = new DateTime(year, 4, 1);
                tracker.EndDate = new DateTime(year, 7, 1);
            }
            else if (month <= 9)
            {
                tracker.StartDate = new DateTime(year, 7, 1);
                tracker.EndDate = new DateTime(year, 10, 1);
            }
            else
            {
                tracker.StartDate = new DateTime(year, 10, 1);
                tracker.EndDate = new DateTime(year + 1, 1, 1);
            }

            return tracker;
        }
        #endregion // GetCurrentQuarterInfo

        #region GetNextQuarterInfo
        private QuarterTracking GetNextQuarterInfo(DateTime date)
        {
            int month = date.Month;
            int year = date.Year;
            QuarterTracking tracker = new QuarterTracking();

            if (month <= 3)
            {
                tracker.StartDate = new DateTime(year, 4, 1);
                tracker.EndDate = new DateTime(year, 7, 1);
              
            }
            else if (month <= 6)
            {
                tracker.StartDate = new DateTime(year, 7, 1);
                tracker.EndDate = new DateTime(year, 10, 1);
            }
            else if (month <= 9)
            {
                tracker.StartDate = new DateTime(year, 10, 1);
                tracker.EndDate = new DateTime(year + 1, 1, 1);
            }
            else
            {
                tracker.StartDate = new DateTime(year + 1, 1, 1);
                tracker.EndDate = new DateTime(year + 1, 4, 1);
            }

            return tracker;
        }
        #endregion // GetNextQuarterInfo

        #region GetPreviousQuarterInfo
        private QuarterTracking GetPreviousQuarterInfo(DateTime date)
        {
            int month = date.Month;
            int year = date.Year;
            QuarterTracking tracker = new QuarterTracking();

            if (month <= 3)
            {
                tracker.StartDate = new DateTime(year - 1, 10, 1);
                tracker.EndDate = new DateTime(year, 1, 1);
            }
            else if (month <= 6)
            {
                tracker.StartDate = new DateTime(year, 1, 1);
                tracker.EndDate = new DateTime(year, 4, 1);
            }
            else if (month <= 9)
            {
                tracker.StartDate = new DateTime(year, 4, 1);
                tracker.EndDate = new DateTime(year, 7, 1);
            }
            else
            {
                tracker.StartDate = new DateTime(year, 7, 1);
                tracker.EndDate = new DateTime(year, 10, 1);
            }

            return tracker;
        }
        #endregion // GetPreviousQuarterInfo

        #region QuarterTracking

        internal class QuarterTracking
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        #endregion // QuarterTracking

        #region Helper

        private object GetDefaultValue()
        {
            if (!this.FieldDataType.IsValueType)
            {
                return null;
            }
            else
            {
                return Activator.CreateInstance(this.FieldDataType);
            }
        }

        #endregion // Helper
    }	
	#endregion // FilterContext<TDataObject>
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