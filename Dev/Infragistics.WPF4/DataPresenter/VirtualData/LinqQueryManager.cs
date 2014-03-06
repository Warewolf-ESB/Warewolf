using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;
using Infragistics.Collections;
using Infragistics.Controls.Schedules;

namespace Infragistics.Windows.DataPresenter
{
	#region LinqQueryManager Class

	internal class LinqQueryManager
	{
		#region Nested Data Structures

		#region CreateExpressionInfo Class

		public class CreateExpressionInfo
		{
			internal LinqQueryManager _manager;
			internal Expression _lastQueriableExpression;
			internal ParameterExpression _parameterExpression;
			internal Dictionary<string, MemberExpression> _cachedMembers = new Dictionary<string, MemberExpression>( );

			public CreateExpressionInfo( LinqQueryManager manager, Expression queriableExpression, ParameterExpression parameterExpression )
			{
				CoreUtilities.ValidateNotNull( manager );
				CoreUtilities.ValidateNotNull( queriableExpression );
				CoreUtilities.ValidateNotNull( parameterExpression );

				_manager = manager;
				_lastQueriableExpression = queriableExpression;
				_parameterExpression = parameterExpression;
			}

			internal Expression QueriableExpression
			{
				get
				{
					return _lastQueriableExpression;
				}
			}

			internal void UpdateQueriableExpression( Expression newQueriableExpression )
			{
				_lastQueriableExpression = newQueriableExpression;
			}

			// SSP 2/15/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
			// 
			internal void UpdateParameterExpression( ParameterExpression newParameterExpression )
			{
				_parameterExpression = newParameterExpression;
			}

			internal void ApplyFilterConditions( Expression lambdaBody )
			{
				LambdaExpression lambda = Expression.Lambda( lambdaBody, _parameterExpression );

				MethodInfo whereMethod = _manager.GetMethod( "Where", ToNonNullItems( _parameterExpression.Type ), 2 );

				_lastQueriableExpression = Expression.Call( whereMethod, _lastQueriableExpression, lambda );
			}
		}

		#endregion // CreateExpressionInfo Class

		#region ILinqExpression Interface

		public interface ILinqExpression
		{
			Expression CreateExpression( CreateExpressionInfo info );
		}

		#endregion // ILinqExpression Interface

		#region ILinqStatement Interface

		public interface ILinqStatement : ILinqExpression
		{
		}

		#endregion // ILinqStatement Interface

		#region ILinqInstruction Interface

		public interface ILinqInstruction : ILinqStatement
		{
		}

		#endregion // ILinqInstruction Interface

		#region ILinqCondition Interface

		public interface ILinqCondition : ILinqStatement
		{
		}

		#endregion // ILinqCondition Interface

		#region LinqOperator Enum

		public enum LinqOperator
		{
			Equal,
			NotEqual,
			LessThan,
			LessThanOrEqual,
			GreaterThan,
			GreaterThanOrEqual,
			AnyOf
		}

		#endregion // LinqOperator Enum

		#region LinqLogicalOperator Enum

		public enum LinqLogicalOperator
		{
			And,
			Or
		}

		#endregion // LinqLogicalOperator Enum

		#region LinqCondition Class

		public class LinqCondition : ILinqCondition
		{
			#region Member Vars

			private LinqOperator _linqOperator;
			private string _fieldName;
			private object _operand;
			private bool _isOperandFieldName = false;

			private ILinqExpression _lhs;
			private ILinqExpression _rhs;

			#endregion // Member Vars

			#region Constructor

			public LinqCondition( string lhsFieldName, string rhsFieldName, LinqOperator linqOperator )
				: this( lhsFieldName, linqOperator, rhsFieldName, true )
			{
			}

			public LinqCondition( string fieldName, LinqOperator linqOperator, object operand )
				: this( fieldName, linqOperator, operand, false )
			{
			}

			private LinqCondition( string fieldName, LinqOperator linqOperator, object operand, bool isOperandFieldName )
			{
				if ( isOperandFieldName && CoreUtilities.IsValueEmpty( operand ) )
					throw new ArgumentOutOfRangeException( "operand" );

				_linqOperator = linqOperator;
				_fieldName = fieldName;
				_operand = operand;
				_isOperandFieldName = isOperandFieldName;
			}

			public LinqCondition( ILinqExpression lhs, LinqOperator linqOperator, ILinqExpression rhs )
			{
				_linqOperator = linqOperator;
				_lhs = lhs;
				_rhs = rhs;
			}

			#endregion // Constructor

			#region Properties

			#region Public Properties

			#region FieldName

			public string FieldName
			{
				get
				{
					return _fieldName;
				}
			}

			#endregion // FieldName

			#region IsOperandFieldName

			public bool IsOperandFieldName
			{
				get
				{
					return _isOperandFieldName;
				}
			}

			#endregion // IsOperandFieldName

			#region Operator

			public LinqOperator Operator
			{
				get
				{
					return _linqOperator;
				}
			}

			#endregion // Operator

			#region Operand

			public object Operand
			{
				get
				{
					return _operand;
				}
			}

			#endregion // Operand

			#endregion // lPublic Properties

			#endregion // Properties

			#region Methods

			#region Public Methods

			#region CreateExpression

			public Expression CreateExpression( CreateExpressionInfo info )
			{
				Expression left = null != _lhs ? _lhs.CreateExpression( info ) : GetCachedMemberExpression( info, _fieldName );
				Expression right = null;

				if ( null != _rhs )
				{
					right = _rhs.CreateExpression( info );
				}
				else if ( _isOperandFieldName )
				{
					right = GetCachedMemberExpression( info, _operand.ToString( ) );
				}
				else
				{
					Type propertyType = left.Type;
					Type operandType = null != _operand ? _operand.GetType( ) : null;

					// If the property is nullable then type cast the operand to nullable type otherwise
					// operator expression creation will throw type mismatch exception.
					// 
					if ( propertyType != operandType && null != operandType )
					{
						if ( CoreUtilities.GetUnderlyingType( propertyType ) == operandType )
							right = Expression.Constant( _operand, propertyType );
					}

					if ( null == right )
						right = Expression.Constant( _operand );
				}

				switch ( _linqOperator )
				{
					case LinqOperator.Equal:
						return Expression.Equal( left, right );

					case LinqOperator.NotEqual:
						return Expression.NotEqual( left, right );

					case LinqOperator.GreaterThan:
						return Expression.GreaterThan( left, right );

					case LinqOperator.GreaterThanOrEqual:
						return Expression.GreaterThanOrEqual( left, right );

					case LinqOperator.LessThan:
						return Expression.LessThan( left, right );

					case LinqOperator.LessThanOrEqual:
						return Expression.LessThanOrEqual( left, right );

					case LinqOperator.AnyOf:
						{
							IEnumerable operandList = _operand as IEnumerable;
							if ( null != operandList )
							{
								LinqConditionGroup cg = new LinqConditionGroup( LinqLogicalOperator.Or );
								foreach ( object ii in operandList )
									cg.Add( new LinqCondition( _fieldName, LinqOperator.Equal, ii ) );

								return cg.CreateExpression( info );
							}
							else
							{
								Debug.Assert( false, "List expected for operand for AnyOf operator." );
								return new LinqCondition( _fieldName, LinqOperator.Equal, _operand ).CreateExpression( info );
							}
						}
				}

				Debug.Assert( false, "Unknown type of comparison operator." );
				return null;
			}

			#endregion // CreateExpression

			#endregion // Public Methods

			#region Internal Methods

			#endregion // Internal Methods

			#endregion // Methods
		}

		#endregion // LinqCondition Class

		#region LinqInstructionBase Class

		/// <summary>
		/// Base class for linq instructions like OrderBy.
		/// </summary>
		public abstract class LinqInstructionBase : ILinqInstruction
		{
			#region Member Vars

			protected ILinqStatement _innerStatement;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="innerStatement">Inner statement.</param>
			public LinqInstructionBase( ILinqStatement innerStatement )
			{
				_innerStatement = innerStatement;
			}

			#endregion // Constructor

			#region CreateExpression

			public abstract Expression CreateExpression( CreateExpressionInfo info );

			#endregion // CreateExpression

			#region InnerStatement

			public ILinqStatement InnerStatement
			{
				get { return _innerStatement; }
			}

			#endregion // InnerStatement

			#region ProcessInnerStatement

			protected void ProcessInnerStatement( CreateExpressionInfo info )
			{
				ProcessInnerStatementHelper( _innerStatement, info );
			}

			internal static void ProcessInnerStatementHelper( ILinqStatement innerStatement, CreateExpressionInfo info )
			{
				if ( null != innerStatement )
				{
					Expression innerExpression = innerStatement.CreateExpression( info );
					if ( innerStatement is ILinqCondition )
						info.ApplyFilterConditions( innerExpression );
					else
						info.UpdateQueriableExpression( innerExpression );
				}
			}

			#endregion // ProcessInnerStatement
		}

		#endregion // LinqInstructionBase Class

		#region LinqInstructionOrderBy Class

		public class LinqInstructionOrderBy : LinqInstructionBase
		{
			#region Member Vars

			private string _fieldName;
			private bool _descending;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="fieldName">Name of the field that's to be ordered by.</param>
			/// <param name="innerStatement">Inner statement.</param>
			/// <param name="descending">Whether to order descending or ascending.</param>
			public LinqInstructionOrderBy( string fieldName, ILinqStatement innerStatement, bool descending )
				: base( innerStatement )
			{
				_fieldName = fieldName;
				_descending = descending;
			}

			#endregion // Constructor

			#region CreateExpression

			public override Expression CreateExpression( CreateExpressionInfo info )
			{
				this.ProcessInnerStatement( info );

				Expression field = null;
				if ( !string.IsNullOrEmpty( _fieldName ) )
					field = GetCachedMemberExpression( info, _fieldName );

				// SSP 2/15/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// 
				//Expression selector = Expression.Lambda( field, info._parameterExpression );
				Expression selector = null != field 
					? Expression.Lambda( field, info._parameterExpression ) 
					: Expression.Lambda( info._parameterExpression, info._parameterExpression );

				Type keyType = null != field ? field.Type : info._parameterExpression.Type;

				
				MethodInfo method = info._manager.GetMethod( _descending ? "OrderByDescending" : "OrderBy", 
					ToNonNullItems( info._parameterExpression.Type, keyType ), 2 );

				return Expression.Call( method, info.QueriableExpression, selector );
			}

			#endregion // CreateExpression

			#region Descending

			public bool Descending
			{
				get { return _descending; }
			}

			#endregion // Descending

			#region FieldName

			public string FieldName
			{
				get { return _fieldName; }
			}

			#endregion // FieldName
		}

		#endregion // LinqInstructionOrderBy Class

		#region LinqInstructionSelect Class

		
		
		public class LinqInstructionSelect : LinqInstructionBase
		{
			#region Member Vars

			private string _fieldName;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="fieldName">Name of the field that's to be select.</param>
			/// <param name="innerStatement">Inner statement.</param>
			public LinqInstructionSelect( string fieldName, ILinqStatement innerStatement )
				: base( innerStatement )
			{
				_fieldName = fieldName;
			}

			#endregion // Constructor

			#region CreateExpression

			public override Expression CreateExpression( CreateExpressionInfo info )
			{
				this.ProcessInnerStatement( info );

				Expression field = GetCachedMemberExpression( info, _fieldName );
				Expression selector = Expression.Lambda( field, info._parameterExpression );

				MethodInfo method = info._manager.GetMethod( "Select", 
					ToNonNullItems( info._parameterExpression.Type, field.Type ), 2 );

				MethodCallExpression expression = Expression.Call( method, info.QueriableExpression, selector );

				ParameterExpression newParameterExpression = Expression.Parameter( field.Type );
				info.UpdateParameterExpression( newParameterExpression );

				return expression;
			}

			#endregion // CreateExpression

			#region FieldName

			public string FieldName
			{
				get { return _fieldName; }
			}

			#endregion // FieldName
		}

		#endregion // LinqInstructionSelect Class

		#region LinqInstructionDistinct Class

		
		
		public class LinqInstructionDistinct : LinqInstructionBase
		{
			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="innerStatement">Inner statement.</param>
			public LinqInstructionDistinct( ILinqStatement innerStatement )
				: base( innerStatement )
			{
			}

			#endregion // Constructor

			#region CreateExpression

			public override Expression CreateExpression( CreateExpressionInfo info )
			{
				this.ProcessInnerStatement( info );

				MethodInfo method = info._manager.GetMethod( "Distinct", ToNonNullItems( info._parameterExpression.Type ), 1 );
				return Expression.Call( method, info.QueriableExpression );
			}

			#endregion // CreateExpression
		}

		#endregion // LinqInstructionDistinct Class

		#region LinqInstructionFirstOrLast Class

		public class LinqInstructionFirstOrLast : LinqInstructionBase
		{
			#region Member Vars

			private bool _first;
			private bool _orDefault;
			private ILinqCondition _condition;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="first">Whether to get the first element or the last element.</param>
			/// <param name="orDefault">If true then null is returned if there are no elements. If false, an exception is raised
			/// if there are no elements.</param>
			/// <param name="condition">Filter condition.</param>
			/// <param name="innerStatement">Inner statement.</param>
			public LinqInstructionFirstOrLast( bool first, bool orDefault, ILinqCondition condition, ILinqStatement innerStatement )
				: base( innerStatement )
			{
				_first = first;
				_orDefault = orDefault;
				_condition = condition;
			}

			#endregion // Constructor

			#region Condition

			public ILinqCondition Condition
			{
				get { return _condition; }
			}

			#endregion // Condition

			#region CreateExpression

			public override Expression CreateExpression( CreateExpressionInfo info )
			{
				this.ProcessInnerStatement( info );

				Expression conditionExpression = null;
				if ( null != _condition )
				{
					Expression lambdaBody = _condition.CreateExpression( info );
					conditionExpression = Expression.Lambda( lambdaBody, info._parameterExpression );
				}

				string methodName = _first
					? ( _orDefault ? "FirstOrDefault" : "First" )
					: ( _orDefault ? "LastOrDefault" : "Last" );

				MethodInfo method = info._manager.GetMethod( methodName, ToNonNullItems( info._parameterExpression.Type ), null != conditionExpression ? 2 : 1 );

				return null != conditionExpression
					? Expression.Call( method, info.QueriableExpression, conditionExpression )
					: Expression.Call( method, info.QueriableExpression );
			}

			#endregion // CreateExpression

			#region First

			public bool First
			{
				get { return _first; }
			}

			#endregion // First

			#region OrDefault

			public bool OrDefault
			{
				get { return _orDefault; }
			}

			#endregion // OrDefault
		}

		#endregion // LinqInstructionFirstOrLast Class

		#region LinqInstructionSummary Class

		// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		public class LinqInstructionSummary : LinqInstructionBase
		{
			#region Member Vars

			private string _summary;
			private string _fieldName;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="summary">One of Sum, Average, Count, Min or Max. The value is case sensitive.</param>
			/// <param name="fieldName">Field name to summarize.</param>
			/// <param name="innerStatement">Inner statement.</param>
			public LinqInstructionSummary( string summary, string fieldName, ILinqStatement innerStatement )
				: base( innerStatement )
			{
				_summary = summary;
				_fieldName = fieldName;
			}

			#endregion // Constructor

			#region CreateExpression

			public override Expression CreateExpression( CreateExpressionInfo info )
			{
				this.ProcessInnerStatement( info );

				bool isMinMax = "Min" == _summary || "Max" == _summary;
				bool isCount = "Count" == _summary;

				Expression field = null;
				if ( ! isCount && !string.IsNullOrEmpty( _fieldName ) )
					field = GetCachedMemberExpression( info, _fieldName );

				
				if ( "Sum" == _summary && typeof( int ) == field.Type )
					field = Expression.Convert( field, typeof( decimal ) );

				Expression selector = null != field
					? Expression.Lambda( field, info._parameterExpression )
					: null;

				MethodInfo method = info._manager.GetMethod( _summary, 
					ToNonNullItems( info._parameterExpression.Type, isMinMax ? field.Type : null ), 
					null != selector ? 2 : 1, 
					ToNonNullItems( info.QueriableExpression.Type, null != selector ? selector.GetType( ) : null ) 
				);

				return null != selector
					? Expression.Call( method, info.QueriableExpression, selector )
					: Expression.Call( method, info.QueriableExpression );
			}

			#endregion // CreateExpression
		}

		#endregion // LinqInstructionSummary Class

		#region LinqExpressionBinary Class

		public class LinqExpressionBinary : ILinqExpression
		{
			#region Member Vars

			private string _lhs;
			private string _rhs;
			private string _binaryOperator;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="lhsField">LHS field.</param>
			/// <param name="rhsField">RHS field.</param>
			/// <param name="binaryOperator">Binary operator.</param>
			public LinqExpressionBinary( string lhsField, string rhsField, string binaryOperator )
			{
				if ( "+" != binaryOperator || "-" != binaryOperator )
					throw new ArgumentOutOfRangeException( );

				_lhs = lhsField;
				_rhs = rhsField;
				_binaryOperator = binaryOperator;
			}

			#endregion // Constructor

			#region CreateExpression

			public Expression CreateExpression( CreateExpressionInfo info )
			{
				Expression lhs = GetCachedMemberExpression( info, _lhs );
				Expression rhs = GetCachedMemberExpression( info, _rhs );

				switch ( _binaryOperator )
				{
					case "+":
						return Expression.Add( lhs, rhs );
					case "-":
						return Expression.Subtract( lhs, rhs );
				}

				Debug.Assert( false, "Unknown binary operator." );
				return null;
			}

			#endregion // CreateExpression
		}

		#endregion // LinqExpressionBinary Class

		#region LinqExpressionConstant Class

		public class LinqExpressionConstant : ILinqExpression
		{
			#region Member Vars

			private object _value;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="value">LHS field.</param>
			public LinqExpressionConstant( object value )
			{
				_value = value;
			}

			#endregion // Constructor

			#region CreateExpression

			public Expression CreateExpression( CreateExpressionInfo info )
			{
				return Expression.Constant( _value );
			}

			#endregion // CreateExpression
		}

		#endregion // LinqExpressionConstant Class

		#region LinqConditionGroup Class

		public class LinqConditionGroup : Collection<ILinqCondition>, ILinqCondition
		{
			private LinqLogicalOperator _logicalOperator = LinqLogicalOperator.And;

			public LinqConditionGroup( )
				: this( LinqLogicalOperator.And )
			{
			}

			public LinqConditionGroup( LinqLogicalOperator logicalOperator )
			{
				_logicalOperator = logicalOperator;
			}

			public LinqConditionGroup( ILinqCondition x, ILinqCondition y )
				: this( x, y, LinqLogicalOperator.And )
			{
			}

			public LinqConditionGroup( ILinqCondition x, ILinqCondition y, LinqLogicalOperator logicalOperator )
			{
				_logicalOperator = logicalOperator;

				this.Add( x );
				this.Add( y );
			}

			protected override void InsertItem( int index, ILinqCondition item )
			{
				if ( null == item )
					throw new InvalidOperationException( ScheduleUtilities.GetString( "LE_CanNotAddANullItem" ) );// "Can not add a null item."

				base.InsertItem( index, item );
			}

			protected override void SetItem( int index, ILinqCondition item )
			{
				if ( null == item )
					throw new InvalidOperationException( ScheduleUtilities.GetString( "LE_CanNotAddANullItem" ) );// "Can not add a null item."

				base.SetItem( index, item );
			}

			/// <summary>
			/// Gets or sets the logical operator with which the conditions are to be combined. Default value is 'And'.
			/// </summary>
			public LinqLogicalOperator LogicalOperator
			{
				get
				{
					return _logicalOperator;
				}
				set
				{
					_logicalOperator = value;
				}
			}

			public Expression CreateExpression( CreateExpressionInfo info )
			{
				Expression result = null;

				for ( int i = 0, count = this.Count; i < count; i++ )
				{
					ILinqCondition condition = this[i];
					Expression tmp = condition.CreateExpression( info );

					if ( null == result )
						result = tmp;
					else
						result = LinqLogicalOperator.Or == _logicalOperator
							? Expression.OrElse( result, tmp )
							: Expression.AndAlso( result, tmp );
				}

				return result;
			}
		}

		#endregion // LinqConditionGroup Class

		#endregion // Nested Data Structures

		#region Member Vars

		// SSP 2/15/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		//private IMap<Tuple<string, int, Type>, MethodInfo> _cachedMethods = MapsFactory.CreateMapHelper<Tuple<string, int, Type>, MethodInfo>( );
		private Dictionary<Tuple<string, ListKey<Type>, int, ListKey<Type>>, MethodInfo> _cachedMethods = new Dictionary<Tuple<string, ListKey<Type>, int, ListKey<Type>>, MethodInfo>( );

		private Dictionary<string, PropertyInfo> _cachedProperties;
		private IEnumerable _list;
		private Type _listItemType;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="LinqQueryManager"/>.
		/// </summary>
		public LinqQueryManager( IEnumerable list, Type listItemType )
		{
			Utilities.ValidateNotNull( list );
			Utilities.ValidateNotNull( listItemType );

			_list = list;
			_listItemType = listItemType;
		}

		#endregion // Constructor

		#region Properties

		#region List

		/// <summary>
		/// Gets the associated list against which the queries will be done.
		/// </summary>
		public IEnumerable List
		{
			get
			{
				return _list;
			}
		}

		#endregion // List

		#region ListItemType

		/// <summary>
		/// Gets the type of items in the list.
		/// </summary>
		public Type ListItemType
		{
			get
			{
				return _listItemType;
			}
		}

		#endregion // ListItemType

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region DoesItemMatch

		/// <summary>
		/// Returns true if the specified item matches the specified condition.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="condition"></param>
		/// <returns></returns>
		public bool DoesItemMatch( object item, ILinqCondition condition )
		{
			
			// 
			IEnumerable r = this.PerformQuery( new object[] { item }, condition );
			return CoreUtilities.HasItems( r );
		}

		#endregion // DoesItemMatch

		#region PerformQuery

		/// <summary>
		/// Performs query on the list and returns the results.
		/// </summary>
		/// <param name="linqStatement">Specifies the query criteria.</param>
		/// <returns>Results of the query.</returns>
		public IEnumerable PerformQuery( ILinqStatement linqStatement )
		{
			return this.PerformQuery( _list, linqStatement );
		}

		#endregion // PerformQuery

		#endregion // Public Methods

		#region Internal Methods

		#region AndHelper

		/// <summary>
		/// Creates a new condition that combines the specified two conditions using logical <i>And</i> operator.
		/// If x or y is null then returns the non-null condition without creating a new condition.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		internal static ILinqCondition AndHelper( ILinqCondition x, ILinqCondition y )
		{
			return AndOrHelper( x, y, LinqLogicalOperator.And );
		}

		/// <summary>
		/// Creates a new condition that combines the specified three conditions using logical <i>And</i> operator.
		/// If a condition is null then combines the other non-null conditions.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		internal static ILinqCondition AndHelper( ILinqCondition x, ILinqCondition y, ILinqCondition z )
		{
			return AndHelper( AndHelper( x, y ), z );
		}

		#endregion // AndHelper

		#region GetTypedEnumerable

		/// <summary>
		/// If the specified list is not a generic IEnumerable&lt;T&gt; where T is the same type or a derived type as the this.List element type,
		/// then creates a this.List element type typed enumerable instance that wraps the specified list.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		internal IEnumerable GetTypedEnumerable( IEnumerable list )
		{
			// SSP 1/9/12 TFS89492
			// Pass true for the new useCollectionViewSource parameter.
			// 
			//if ( _listItemType != GetListElementType( list ) )
			if ( _listItemType != GetListElementType( list, false ) )
			{
				Type t = typeof( TypedEnumerable<> );
				Type ts = t.MakeGenericType( _listItemType );

				return (IEnumerable)Activator.CreateInstance( ts, list );
			}

			return list;
		}

		#endregion // GetTypedEnumerable

		#region GetListElementType

		/// <summary>
		/// Returns the list element type.
		/// </summary>
		/// <param name="list">List whose element type to get.</param>
		/// <param name="useCollectionViewSource">If element type cannot be determined from list and it's a collection view then
		/// determine it from the collection view's source collection.</param>
		/// <returns></returns>
		internal static Type GetListElementType( IEnumerable list
			// SSP 1/9/12 TFS89492
			// Added useCollectionViewSource parameter.
			// 
			, bool useCollectionViewSource = true
			)
		{
			Type listType = list.GetType( );
			Type[] implementedInterfaces = listType.GetInterfaces( );

			if ( null != implementedInterfaces )
			{
				for ( int i = 0; i < implementedInterfaces.Length; i++ )
				{
					Type interfaceType = implementedInterfaces[i];
					if ( interfaceType.IsGenericType && typeof( IEnumerable<> ) == interfaceType.GetGenericTypeDefinition( ) )
					{
						Type[] typeArguments = interfaceType.GetGenericArguments( );
						if ( null != typeArguments && 1 == typeArguments.Length )
						{
							return typeArguments[0];
						}
					}
				}

				// SSP 1/9/12 TFS89492
				// If the list is ICollectionView then find out the item type from the source collection.
				// 
				if ( useCollectionViewSource )
				{
					Type type = GetListElementType_CollectionView( list );
					if ( null != type )
						return type;
				}
			}

			return null;
		}

		// SSP 1/9/12 TFS89492
		// 
		private static Type GetListElementType_CollectionView( IEnumerable list )
		{
			System.ComponentModel.ICollectionView view = list as System.ComponentModel.ICollectionView;
			var sourceColl = null != view ? view.SourceCollection : null;
			return null != sourceColl ? GetListElementType( sourceColl, false ) : null;
		}

		#endregion // GetListElementType

		#region GetPropertyType

		internal Type GetPropertyType( string propName )
		{
			PropertyInfo info = this.GetProperty( propName );

			return null != info ? info.PropertyType : null;
		}

		#endregion // GetPropertyType

		#region HasProperty

		internal bool HasProperty( string propName )
		{
			return null != this.GetProperty( propName );
		}

		#endregion // HasProperty

		#region OrHelper

		/// <summary>
		/// Creates a new condition that combines the specified two conditions using logical <i>Or</i> operator.
		/// If x or y is null then returns the non-null condition without creating a new condition.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		internal static ILinqCondition OrHelper( ILinqCondition x, ILinqCondition y )
		{
			return AndOrHelper( x, y, LinqLogicalOperator.Or );
		}

		/// <summary>
		/// Creates a new condition that combines the specified three conditions using logical <i>Or</i> operator.
		/// If a condition is null then combines the other non-null conditions.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		internal static ILinqCondition OrHelper( ILinqCondition x, ILinqCondition y, ILinqCondition z )
		{
			return OrHelper( OrHelper( z, y ), z );
		}

		#endregion // OrHelper

		#region PerformQuery

		/// <summary>
		/// Performs query on the specified list and returns the results.
		/// </summary>
		/// <param name="list">List to perform the query against. Note that the list
		/// must contain the same types of items as the list associated with this query manager.</param>
		/// <param name="linqQuery">Specifies the query criteria.</param>
		/// <returns>Results of the query.</returns>
		internal IEnumerable PerformQuery( IEnumerable list, ILinqStatement linqQuery )
		{
			IEnumerable listTyped = GetTypedEnumerable( list );
			IQueryable queriable = Queryable.AsQueryable( listTyped );

			CreateExpressionInfo info = new CreateExpressionInfo( this, queriable.Expression, Expression.Parameter( _listItemType, "i" ) );

			LinqInstructionBase.ProcessInnerStatementHelper( linqQuery, info );

			Expression expression = info.QueriableExpression;
			if ( typeof( IQueryable ).IsAssignableFrom( expression.Type ) )
			{
				IQueryable result = queriable.Provider.CreateQuery( expression );
				return result;
			}
			else
			{
				object result = queriable.Provider.Execute( expression );
				if ( null != result && !( result is IEnumerable ) )
					result = new object[] { result };

				return (IEnumerable)result;
			}
		}

		#endregion // PerformQuery

		#endregion // Internal Methods

		#region Private Methods

		#region AndOrHelper

		private static ILinqCondition AndOrHelper( ILinqCondition x, ILinqCondition y, LinqLogicalOperator logicalOperator )
		{
			if ( null != x && null != y )
				return new LinqConditionGroup( x, y, logicalOperator );
			else if ( null != x )
				return x;
			else
				return y;
		}

		#endregion // AndOrHelper

		#region GetCachedMemberExpression

		internal static MemberExpression GetCachedMemberExpression( CreateExpressionInfo info, string fieldName )
		{
			MemberExpression ret;
			if ( !info._cachedMembers.TryGetValue( fieldName, out ret ) )
			{
				ret = Expression.Property( info._parameterExpression, fieldName );
				info._cachedMembers[fieldName] = ret;
			}

			return ret;
		}

		#endregion // GetCachedMemberExpression

		#region GetProperty

		internal PropertyInfo GetProperty( string propName )
		{
			PropertyInfo info;
			if ( null != _cachedProperties && _cachedProperties.TryGetValue( propName, out info ) )
				return info;

			PropertyInfo[] props = _listItemType.GetProperties( );

			info = props.FirstOrDefault( ii => ii.Name == propName );

			if ( null == _cachedProperties )
				_cachedProperties = new Dictionary<string, PropertyInfo>( );

			_cachedProperties[propName] = info;

			return info;
		}

		#endregion // GetProperty

		#region GetWhereMethod

		private static bool AreParametersCompatible( MethodInfo method, Type[] parameterTypes )
		{
			int i = 0;

			foreach ( ParameterInfo ii in method.GetParameters( ) )
			{
				if ( !ii.ParameterType.IsAssignableFrom( parameterTypes[i++] ) )
					return false;
			}

			return i == parameterTypes.Length;
		}

		private static MethodInfo GetMethodHelper( MethodInfo[] methods, 
			string name, Type[] genericTypeArguments, int parameterCount, params Type[] parameterTypes )
		{
			IEnumerable<MethodInfo> rr = methods.Where( ii => ii.Name == name );

			if ( parameterCount >= 0 )
				rr = rr.Where( ii => ii.GetParameters( ).Length == parameterCount );

			if ( null != genericTypeArguments )
				rr = rr.Where( ii => ii.GetGenericArguments( ).Length == genericTypeArguments.Length );

			foreach ( MethodInfo ii in rr )
			{
				try
				{
					MethodInfo iiMethod = null == genericTypeArguments ? ii : ii.MakeGenericMethod( genericTypeArguments );

					if ( null != iiMethod && ( null == parameterTypes || AreParametersCompatible( iiMethod, parameterTypes ) ) )
						return iiMethod;
				}
				catch
				{
				}
			}

			return null;
		}

		private MethodInfo GetMethod( string name, Type[] genericTypeArguments, int parameterCount, Type[] parameterTypes = null )
		{
			Tuple<string, ListKey<Type>, int, ListKey<Type>> key = new Tuple<string, ListKey<Type>, int, ListKey<Type>>( 
				name, new ListKey<Type>( genericTypeArguments ), parameterCount, new ListKey<Type>( parameterTypes ) );

			MethodInfo method;
			_cachedMethods.TryGetValue( key, out method );

			if ( null == method )
			{
				MethodInfo[] methods = typeof( Queryable ).GetMethods( );
				method = GetMethodHelper( methods, name, genericTypeArguments, parameterCount, parameterTypes );

				if ( null == method )
					throw new ArgumentException( "Unable to find a matching method." );

				_cachedMethods[key] = method;
			}

			return method;
		}

		#endregion // GetWhereMethod

		#region ToNonNullItems

		private static T[] ToNonNullItems<T>( params T[] items ) where T : class
		{
			return items.Where( ii => null != ii ).ToArray( );
		} 

		#endregion // ToNonNullItems

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // LinqQueryManager Class

	internal class ListKey<T>
	{
		internal readonly IList<T> _list;

		internal ListKey( IList<T> list )
		{
			_list = list;
		}

		public override int GetHashCode( )
		{
			int h = 0;

			if ( null != _list )
			{
				foreach ( T ii in _list )
					h ^= ii.GetHashCode( );
			}

			return h;
		}

		public override bool Equals( object obj )
		{
			ListKey<T> lc = obj as ListKey<T>;

			if ( null != lc )
			{
				IList<T> x = _list;
				IList<T> y = lc._list;

				return CoreUtilities.AreEqual( x, y );
			}

			return false;
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