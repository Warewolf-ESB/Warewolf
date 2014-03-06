using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Infragistics
{
	/// <summary>
	/// A nongeneric abstract class representing a summary on an object.
	/// </summary>
	public abstract class SummaryContext
	{
		#region Statics

		#region CreateGenericSummary

		/// <summary>
		/// Creates a SummaryContext instanced typed to the object type of the data being processed.
		/// </summary>
        /// <param name="cachedTypeInfo">The <see cref="CachedTypedInfo"/> object which has the type info for this method.</param>
		/// <param name="propertyName">The field data type that will be processed on.</param>
		/// <param name="linqSummary">The LINQ statement which will be used.</param>
		/// <returns></returns>		
		public static SummaryContext CreateGenericSummary(CachedTypedInfo cachedTypeInfo, string propertyName, LinqSummaryOperator linqSummary)
		{
            Type objectType = cachedTypeInfo.CachedType;
			Type columnType = DataManagerBase.ResolvePropertyTypeFromPropertyName(propertyName, cachedTypeInfo);
			Type specificSummaryType = typeof(SummaryContext<,>).MakeGenericType(new System.Type[] { objectType, columnType });
			return (SummaryContext)Activator.CreateInstance(specificSummaryType, new object[] { propertyName, linqSummary, cachedTypeInfo });
		}

		#endregion // CreateGenericSummary

		#endregion // Statics

		#region Members

		#region FieldName

		/// <summary>
		/// Gets the name of the property on the data object that will be summed on.
		/// </summary>
		public string FieldName
		{
			get;
			protected set;
		}

		#endregion // FieldName

		#region LinqSummary

		/// <summary>
		/// Gets the LinqSummaryOperator associated with this <see cref="SummaryContext"/>.
		/// </summary>
		public LinqSummaryOperator LinqSummary
		{
			get;
			protected set;
		}

		#endregion // LinqSummary

		#endregion // Members

		#region Methods

		#region Public

		#region Execute
		/// <summary>
		/// Performs a LINQ based summary on the inputted query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public object Execute(IQueryable query)
		{
			object returnValue = null;
			switch (this.LinqSummary)
			{
				case (LinqSummaryOperator.Maximum):
					{
						returnValue = this.Maximum(query);
						break;
					}
				case (LinqSummaryOperator.Minimum):
					{
						returnValue = this.Minimum(query);
						break;
					}
				case (LinqSummaryOperator.Count):
					{
						returnValue = this.Count(query);
						break;
					}
				case (LinqSummaryOperator.Average):
					{
						returnValue = this.Average(query);
						break;
					}
				case (LinqSummaryOperator.Sum):
					{
						returnValue = this.Sum(query);
						break;
					}

			}
			return returnValue;
		}
		#endregion // Execute

		#endregion // Public

		#region Protected

		#region Maximum
		/// <summary>
		/// Executes a LINQ based Maximum summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected abstract object Maximum(IQueryable query);
		#endregion // Maximum

		#region Minimum
		/// <summary>
		/// Executes a LINQ based Minimum summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected abstract object Minimum(IQueryable query);
		#endregion // Minimum

		#region Sum
		/// <summary>
		/// Executes a LINQ based Sum summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected abstract object Sum(IQueryable query);
		#endregion // Sum

		#region Count
		/// <summary>
		/// Executes a LINQ based Count summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected abstract int Count(IQueryable query);
		#endregion // Count

		#region Average
		/// <summary>
		/// Executes a LINQ based Average summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected abstract object Average(IQueryable query);
		#endregion // Average

		#endregion // Protected

		#endregion // Methods
	}

	/// <summary>
	/// A generic class representing a summary on an object.
	/// </summary>
	/// <typeparam name="TObjectType">The type of the object which will be summed on.</typeparam>
	/// <typeparam name="TColumnType">The type of the field that will be summed on.</typeparam>
	public class SummaryContext<TObjectType, TColumnType> : SummaryContext
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryContext"/>
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="linqSummary"></param>
        /// <param name="cti"></param>
		public SummaryContext(string propertyName, LinqSummaryOperator linqSummary, CachedTypedInfo cti)
		{
			this.FieldName = propertyName;
			this.LinqSummary = linqSummary;
            this.CachedTypedInfo = cti;
		}

		#endregion // Constructor

        #region Properties

        #region Protected

        /// <summary>
        /// The CachedTypedInfo for the opeartion
        /// </summary>
        protected CachedTypedInfo CachedTypedInfo
        {
            get;
            private set;
        }

        #endregion // Protected

        #endregion // Properties

        #region Maximum
        /// <summary>
		/// Executes a LINQ based Maximum summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected override object Maximum(IQueryable query)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TObjectType),
																								   "parameter");

			Expression left = DataManagerBase.BuildPropertyExpressionFromPropertyName(this.FieldName, parameterExpression, this.CachedTypedInfo, typeof(TColumnType), default(TColumnType));

			var expr3 = Expression.Lambda<Func<TObjectType, TColumnType>>(left, parameterExpression);

			try
			{
				return query.Cast<TObjectType>().Max(expr3);
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}
		#endregion // Maximum

		#region Minimum
		/// <summary>
		/// Executes a LINQ based Minimum summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected override object Minimum(IQueryable query)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TObjectType),
																						   "parameter");
            Expression left = DataManagerBase.BuildPropertyExpressionFromPropertyName(this.FieldName, parameterExpression, this.CachedTypedInfo, typeof(TColumnType), default(TColumnType));

			var expr3 = Expression.Lambda<Func<TObjectType, TColumnType>>(left, parameterExpression);

			try
			{
				return query.Cast<TObjectType>().Min(expr3);
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}
		#endregion // Minimum

		#region Sum
		/// <summary>
		/// Executes a LINQ based Sum summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected override object Sum(IQueryable query)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TObjectType),
																						   "parameter");
            Expression left = DataManagerBase.BuildPropertyExpressionFromPropertyName(this.FieldName, parameterExpression, this.CachedTypedInfo, typeof(TColumnType), default(TColumnType));

			Type columnType = typeof(TColumnType);

			try
			{
				#region wholes
				if (columnType == typeof(int))
				{
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, double>>(Expression.Convert(left, typeof(double)), parameterExpression));
				}
				if (columnType == typeof(int?))
				{
					return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, double?>>(Expression.Convert(left, typeof(double?)), parameterExpression));
				}
				if (columnType == typeof(long))
				{
					return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, double>>(Expression.Convert(left, typeof(double)), parameterExpression));
				}
				if (columnType == typeof(long?))
				{
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, double?>>(Expression.Convert(left, typeof(double?)), parameterExpression));
				}
				#endregion // wholes

				#region reals
				if (columnType == typeof(double))
				{
					return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, double>>(left, parameterExpression));
				}
				if (columnType == typeof(double?))
				{
					return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, double?>>(left, parameterExpression));
				}

				if (columnType == typeof(decimal))
				{
					return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, decimal>>(left, parameterExpression));
				}
				if (columnType == typeof(decimal?))
				{
					return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, decimal?>>(left, parameterExpression));
				}
				if (columnType == typeof(float))
				{
					return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, float>>(left, parameterExpression));
				}
				if (columnType == typeof(float?))
				{
					return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, float?>>(left, parameterExpression));
				}
				#endregion // reals

                #region Converted values

                #region Byte, NByte
                if (columnType == typeof(byte))
                {
                    left = MethodCallExpression.Convert(left, typeof(int));                    
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, int>>(left, parameterExpression));
                }

                if (columnType == typeof(byte?))
                {
                    left = MethodCallExpression.Convert(left, typeof(int?));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, int?>>(left, parameterExpression));
                }
                #endregion // Byte, NByte

                #region SByte, NSByte
                if (columnType == typeof(sbyte))
                {
                    left = MethodCallExpression.Convert(left, typeof(int));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, int>>(left, parameterExpression));
                }

                if (columnType == typeof(sbyte?))
                {
                    left = MethodCallExpression.Convert(left, typeof(int?));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, int?>>(left, parameterExpression));
                }
                #endregion  // SByte, NSByte

                #region Uint, NUint
                if (columnType == typeof(uint))
                {
                    left = MethodCallExpression.Convert(left, typeof(long));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, long>>(left, parameterExpression));
                }

                if (columnType == typeof(uint?))
                {
                    left = MethodCallExpression.Convert(left, typeof(long?));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, long?>>(left, parameterExpression));
                }
                #endregion // Uint, NUint

                #region Short, NShort, UShort, NUShort
                if (columnType == typeof(short))
                {
                    left = MethodCallExpression.Convert(left, typeof(int));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, int>>(left, parameterExpression));
                }
                if (columnType == typeof(short?))
                {
                    left = MethodCallExpression.Convert(left, typeof(int?));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, int?>>(left, parameterExpression));
                }
                if (columnType == typeof(ushort))
                {
                    left = MethodCallExpression.Convert(left, typeof(int));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, int>>(left, parameterExpression));
                }
                if (columnType == typeof(ushort?))
                {
                    left = MethodCallExpression.Convert(left, typeof(int?));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, int?>>(left, parameterExpression));
                }
                #endregion // Short, NShort, UShort, NUShort

                #region ULong, ULong?

                if (columnType == typeof(ulong))
                {
                    left = MethodCallExpression.Convert(left, typeof(decimal));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, decimal>>(left, parameterExpression));
                }
                if (columnType == typeof(ulong?))
                {
                    left = MethodCallExpression.Convert(left, typeof(decimal?));
                    return query.Cast<TObjectType>().Sum(Expression.Lambda<Func<TObjectType, decimal?>>(left, parameterExpression));
                }

                #endregion // ULong, ULong?

                #endregion // Converted values
			}
			catch (InvalidOperationException)
			{
				return null;
			}

			return null;
		}
		#endregion // Sum

		#region Average
		/// <summary>
		/// Executes a LINQ based Average summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected override object Average(IQueryable query)
		{
			Type objectType = typeof(TObjectType);
			Type columnType = typeof(TColumnType);

			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(objectType, "parameter");
            Expression left = DataManagerBase.BuildPropertyExpressionFromPropertyName(this.FieldName, parameterExpression, this.CachedTypedInfo, typeof(TColumnType), default(TColumnType));
		
			try
			{
				#region wholes

				if (columnType == typeof(int))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int>>(left, parameterExpression));
				}
				if (columnType == typeof(int?))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int?>>(left, parameterExpression));
				}
				if (columnType == typeof(long))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, long>>(left, parameterExpression));
				}
				if (columnType == typeof(long?))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, long?>>(left, parameterExpression));
				}
				#endregion // wholes

				#region reals
				if (columnType == typeof(double))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, double>>(left, parameterExpression));
				}
				if (columnType == typeof(double?))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, double?>>(left, parameterExpression));
				}

				if (columnType == typeof(decimal))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, decimal>>(left, parameterExpression));
				}
				if (columnType == typeof(decimal?))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, decimal?>>(left, parameterExpression));
				}
				if (columnType == typeof(float))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, float>>(left, parameterExpression));
				}
				if (columnType == typeof(float?))
				{
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, float?>>(left, parameterExpression));
				}
				#endregion // reals

				#region Converted values

				#region Byte, NByte
				if (columnType == typeof(byte))
				{
					left = MethodCallExpression.Convert(left, typeof(int));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int>>(left, parameterExpression));
				}

				if (columnType == typeof(byte?))
				{
					left = MethodCallExpression.Convert(left, typeof(int?));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int?>>(left, parameterExpression));
				}
				#endregion // Byte, NByte

				#region SByte, NSByte
				if (columnType == typeof(sbyte))
				{
					left = MethodCallExpression.Convert(left, typeof(int));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int>>(left, parameterExpression));
				}

				if (columnType == typeof(sbyte?))
				{
					left = MethodCallExpression.Convert(left, typeof(int?));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int?>>(left, parameterExpression));
				}
				#endregion  // SByte, NSByte

				#region Uint, NUint
				if (columnType == typeof(uint))
				{
					left = MethodCallExpression.Convert(left, typeof(long));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, long>>(left, parameterExpression));
				}

				if (columnType == typeof(uint?))
				{
					left = MethodCallExpression.Convert(left, typeof(long?));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, long?>>(left, parameterExpression));
				}
				#endregion // Uint, NUint

				#region Short, NShort, UShort, NUShort
				if (columnType == typeof(short))
				{
					left = MethodCallExpression.Convert(left, typeof(int));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int>>(left, parameterExpression));
				}
				if (columnType == typeof(short?))
				{
					left = MethodCallExpression.Convert(left, typeof(int?));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int?>>(left, parameterExpression));
				}
				if (columnType == typeof(ushort))
				{
					left = MethodCallExpression.Convert(left, typeof(int));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int>>(left, parameterExpression));
				}
				if (columnType == typeof(ushort?))
				{
					left = MethodCallExpression.Convert(left, typeof(int?));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, int?>>(left, parameterExpression));
				}
				#endregion // Short, NShort, UShort, NUShort

				#region ULong, ULong?

				if (columnType == typeof(ulong))
				{
					left = MethodCallExpression.Convert(left, typeof(decimal));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, decimal>>(left, parameterExpression));
				}
				if (columnType == typeof(ulong?))
				{
					left = MethodCallExpression.Convert(left, typeof(decimal?));
					return query.Cast<TObjectType>().Average(Expression.Lambda<Func<TObjectType, decimal?>>(left, parameterExpression));
				}

				#endregion // ULong, ULong?

				#endregion // Converted values
			}
			catch (InvalidOperationException)
			{
				return null;
			}

			return null;
		}
		#endregion // Average

		#region Count
		/// <summary>
		/// Executes a LINQ based Count summary.
		/// </summary>
		/// <param name="query">The IQueryable to execute the summary against.</param>
		/// <returns>The value of the summation.</returns>
		protected override int Count(IQueryable query)
		{
			return query.Cast<TObjectType>().Count();
		}
		#endregion // Count
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