using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Linq;
using System.Reflection; 







using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

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

			internal void ApplyFilterConditions( Expression lambdaBody )
			{
				LambdaExpression lambda = Expression.Lambda( lambdaBody, _parameterExpression );

				MethodInfo whereMethod = _manager.GetWhereMethod( );

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

			#region LinqConditionSerializationInfo class

			internal class LinqConditionSerializationInfo : ObjectSerializationInfo
			{
				public override object Deserialize(Dictionary<string, object> obj)
				{
					object value;

					string fieldName = null;
					if (obj.TryGetValue("FieldName", out value))
						fieldName = (string)value;

					object operand = null;
					if (obj.TryGetValue("Operand", out value))
						operand = value;

					LinqOperator linqOperator = LinqOperator.Equal;
					if (obj.TryGetValue("Operator", out value))
						linqOperator = (LinqOperator)value;

					bool isOperandFieldName = false;
					if ( obj.TryGetValue( "IsOperandFieldName", out value ) )
						isOperandFieldName = (bool)value;

					return new LinqCondition( fieldName, linqOperator, operand, isOperandFieldName );
				}

				protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties()
				{
					return new PropertySerializationInfo[]
					{
						new PropertySerializationInfo(typeof(string), "FieldName"),
						new PropertySerializationInfo(typeof(object), "Operand"),
						new PropertySerializationInfo(typeof(LinqOperator), "Operator"),
						new PropertySerializationInfo( typeof( bool ), "IsOperandFieldName" )
					};
				}

				public override Dictionary<string, object> Serialize(object obj)
				{
					LinqCondition instruction = (LinqCondition)obj;

					Dictionary<string, object> map = null;

					if (instruction._fieldName != null)
						map = ScheduleUtilities.AddEntryHelper(map, "FieldName", instruction._fieldName);

					if (instruction._operand != null)
						map = ScheduleUtilities.AddEntryHelper(map, "Operand", instruction._operand);

					if ( instruction._isOperandFieldName )
						map = ScheduleUtilities.AddEntryHelper( map, "IsOperandFieldName", true );

					map = ScheduleUtilities.AddEntryHelper(map, "Operator", instruction._linqOperator);

					return map;
				}
			} 

			#endregion // LinqConditionSerializationInfo class
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

				Expression selector = Expression.Lambda( field, info._parameterExpression );

				MethodInfo method = info._manager.GetMethod( _descending ? "OrderByDescending" : "OrderBy", 2, _fieldName );
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


			#region LinqInstructionOrderBySerializationInfo class

			internal class LinqInstructionOrderBySerializationInfo : ObjectSerializationInfo
			{
				public override object Deserialize(Dictionary<string, object> obj)
				{
					object value;

					string fieldName = null;
					if (obj.TryGetValue("FieldName", out value))
						fieldName = (string)value;

					bool descending = false;
					if (obj.TryGetValue("Descending", out value))
						descending = (bool)value;

					ILinqStatement innerStatement = null;
					if (obj.TryGetValue("InnerStatement", out value))
						innerStatement = (ILinqStatement)value;

					return new LinqInstructionOrderBy(fieldName, innerStatement, descending);
				}

				protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties()
				{
					return new PropertySerializationInfo[]
					{
						new PropertySerializationInfo(typeof(string), "FieldName"),
						new PropertySerializationInfo(typeof(bool), "Descending"),
						new PropertySerializationInfo(typeof(ILinqStatement), "InnerStatement")
					};
				}

				public override Dictionary<string, object> Serialize(object obj)
				{
					LinqInstructionOrderBy instruction = (LinqInstructionOrderBy)obj;

					Dictionary<string, object> map = null;

					if (instruction._fieldName != null)
						map = ScheduleUtilities.AddEntryHelper(map, "FieldName", instruction._fieldName);

					map = ScheduleUtilities.AddEntryHelper(map, "Descending", instruction._descending);

					if (instruction._innerStatement != null)
						map = ScheduleUtilities.AddEntryHelper(map, "InnerStatement", instruction._innerStatement);

					return map;
				}
			} 

			#endregion // LinqInstructionOrderBySerializationInfo class
		} 

		#endregion // LinqInstructionOrderBy Class

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

				MethodInfo method = info._manager.GetMethod( methodName, null != conditionExpression ? 2 : 1, null );

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


			#region LinqInstructionFirstOrLastSerializationInfo class

			internal class LinqInstructionFirstOrLastSerializationInfo : ObjectSerializationInfo
			{
				public override object Deserialize(Dictionary<string, object> obj)
				{
					object value;

					bool first = false;
					if (obj.TryGetValue("First", out value))
						first = (bool)value;

					bool orDefault = false;
					if (obj.TryGetValue("OrDefault", out value))
						orDefault = (bool)value;

					ILinqCondition condition = null;
					if (obj.TryGetValue("Condition", out value))
						condition = (ILinqCondition)value;

					ILinqStatement innerStatement = null;
					if (obj.TryGetValue("InnerStatement", out value))
						innerStatement = (ILinqStatement)value;

					return new LinqInstructionFirstOrLast(first, orDefault, condition, innerStatement);
				}

				protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties()
				{
					return new PropertySerializationInfo[]
					{
						new PropertySerializationInfo(typeof(bool), "First"),
						new PropertySerializationInfo(typeof(bool), "OrDefault"),
						new PropertySerializationInfo(typeof(ILinqCondition), "Condition"),
						new PropertySerializationInfo(typeof(ILinqStatement), "InnerStatement")
					};
				}

				public override Dictionary<string, object> Serialize(object obj)
				{
					LinqInstructionFirstOrLast instruction = (LinqInstructionFirstOrLast)obj;

					Dictionary<string, object> map = null;

					map = ScheduleUtilities.AddEntryHelper(map, "First", instruction._first);
					map = ScheduleUtilities.AddEntryHelper(map, "OrDefault", instruction._orDefault);

					if (instruction._condition != null)
						map = ScheduleUtilities.AddEntryHelper(map, "Condition", instruction._condition);

					if (instruction._innerStatement != null)
						map = ScheduleUtilities.AddEntryHelper(map, "InnerStatement", instruction._innerStatement);

					return map;
				}
			} 

			#endregion // LinqInstructionFirstOrLastSerializationInfo class
		} 

		#endregion // LinqInstructionFirstOrLast Class

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
					throw new InvalidOperationException(ScheduleUtilities.GetString("LE_CanNotAddANullItem"));// "Can not add a null item."

				base.InsertItem( index, item );
			}

			protected override void SetItem( int index, ILinqCondition item )
			{
				if ( null == item )
					throw new InvalidOperationException(ScheduleUtilities.GetString("LE_CanNotAddANullItem"));// "Can not add a null item."

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


			#region LinqConditionGroupSerializationInfo class

			internal class LinqConditionGroupSerializationInfo : CollectionSerializationInfo<LinqConditionGroup, ILinqCondition>
			{
				public override object Deserialize(Dictionary<string, object> obj)
				{
					LinqConditionGroup group = (LinqConditionGroup)base.Deserialize(obj);

					object value;
					if (obj.TryGetValue("LogicalOperator", out value))
						group.LogicalOperator = (LinqLogicalOperator)value;

					return group;
				}

				protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties()
				{
					List<PropertySerializationInfo> list = new List<PropertySerializationInfo>(base.GetSerializedProperties());
					list.Add(new PropertySerializationInfo(typeof(LinqLogicalOperator), "LogicalOperator"));
					return list.ToArray();
				}

				public override Dictionary<string, object> Serialize(object obj)
				{
					LinqConditionGroup group = (LinqConditionGroup)obj;

					Dictionary<string, object> map = base.Serialize(obj);

					map = ScheduleUtilities.AddEntryHelper(map, "LogicalOperator", group.LogicalOperator);

					return map;
				}
			} 

			#endregion // LinqConditionGroupSerializationInfo class
		}

		#endregion // LinqConditionGroup Class

		#endregion // Nested Data Structures

		#region Member Vars

		private IMap<Tuple<string, int, Type>, MethodInfo> _cachedMethods = MapsFactory.CreateMapHelper<Tuple<string, int, Type>, MethodInfo>( );
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
			ScheduleUtilities.ValidateNotNull( list );
			ScheduleUtilities.ValidateNotNull( listItemType );

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

		#region RegisterSerializerInfos

		public static void RegisterSerializerInfos(ObjectSerializer serializer)
		{
			serializer.RegisterInfo(typeof(LinqCondition), new LinqCondition.LinqConditionSerializationInfo());
			serializer.RegisterInfo(typeof(LinqInstructionOrderBy), new LinqInstructionOrderBy.LinqInstructionOrderBySerializationInfo());
			serializer.RegisterInfo(typeof(LinqInstructionFirstOrLast), new LinqInstructionFirstOrLast.LinqInstructionFirstOrLastSerializationInfo());
			serializer.RegisterInfo(typeof(LinqConditionGroup), new LinqConditionGroup.LinqConditionGroupSerializationInfo());
			serializer.RegisterInfo(typeof(List<string>), new GenericListSerializationInfo());
			serializer.RegisterInfo(typeof(string), new StringSerializationInfo());
		}

		#endregion // RegisterSerializerInfos

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
				if ( null != result && ! ( result is IEnumerable ) )
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

		private static MethodInfo GetMethodHelper( MethodInfo[] methods, string name, int parameterCount )
		{
			IEnumerable<MethodInfo> rr = methods.Where( ii => ii.Name == name );
			
			if ( parameterCount >= 0 )
				rr = rr.Where( ii => ii.GetParameters( ).Length == parameterCount );

			return rr.FirstOrDefault( );
		}

		private MethodInfo GetMethod( string name, int parameterCount, string selectorField )
		{
			Type secondGenericParameter = string.IsNullOrEmpty( selectorField )
				? null : this.GetPropertyType( selectorField );

			Tuple<string, int, Type> key = new Tuple<string, int, Type>( name, parameterCount, secondGenericParameter );
			MethodInfo method = _cachedMethods[key];

			if ( null == method )
			{
				MethodInfo[] methods = typeof( Queryable ).GetMethods( );
				method = GetMethodHelper( methods, name, parameterCount );
				method = method.MakeGenericMethod( ScheduleUtilities.GetNonNullValues( _listItemType, secondGenericParameter ) );
				_cachedMethods[key] = method;
			}

			return method;
		}

		private MethodInfo GetWhereMethod( )
		{
			return this.GetMethod( "Where", 2, null );
		}

		#endregion // GetWhereMethod

		#endregion // Private Methods 

		#endregion // Methods

		
		private class GenericListSerializationInfo : CollectionSerializationInfo<List<string>, string> { }

		private class StringSerializationInfo : ObjectSerializationInfo
		{
			public override object Deserialize(Dictionary<string, object> obj)
			{
				string strValue = null;

				object value;
				if (obj.TryGetValue("V", out value))
					strValue = (string)value;

				return strValue;
			}

			protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties()
			{
				return new PropertySerializationInfo[]
				{
					new PropertySerializationInfo( typeof( string ), "V" )
				};
			}

			public override Dictionary<string, object> Serialize(object obj)
			{
				return ScheduleUtilities.AddEntryHelper( null, "V", obj );
			}
		}
	}

	#endregion // LinqQueryManager Class
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