using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Shared;
using Infragistics.Windows.Internal;
using System.Collections;

namespace Infragistics.Windows.Controls
{
	internal class SpecialFilterOperandFactory
	{
		#region Methods

		#region Private/Internal Methods

		#region GetBuiltInOperands

		internal static List<IGOperand> GetBuiltInOperands( )
		{
			List<IGOperand> list = new List<IGOperand>( 50 );

			list.Add( new BlanksOperand( false ) );
			list.Add( new BlanksOperand( true ) );

			list.Add( new AverageOperand( true ) );
			list.Add( new AverageOperand( false ) );

			list.Add( new TopValuesOperand( "Top10", 10, false, true ) );
			list.Add( new TopValuesOperand( "Top10Percentage", 10, true, true ) );
			list.Add( new TopValuesOperand( "Bottom10", 10, false, false ) );
			list.Add( new TopValuesOperand( "Bottom10Percentage", 10, true, false ) );

			list.Add( new RelativeDateOperand( "Tomorrow", RelativeDateOperand.RelativeDate.Tomorrow ) );
			list.Add( new RelativeDateOperand( "Today", RelativeDateOperand.RelativeDate.Today ) );
			list.Add( new RelativeDateOperand( "Yesterday", RelativeDateOperand.RelativeDate.Yesterday ) );
			list.Add( new RelativeDateOperand( "NextWeek", RelativeDateOperand.RelativeDate.NextWeek ) );
			list.Add( new RelativeDateOperand( "ThisWeek", RelativeDateOperand.RelativeDate.ThisWeek ) );
			list.Add( new RelativeDateOperand( "LastWeek", RelativeDateOperand.RelativeDate.LastWeek ) );
			list.Add( new RelativeDateOperand( "NextMonth", RelativeDateOperand.RelativeDate.NextMonth ) );
			list.Add( new RelativeDateOperand( "ThisMonth", RelativeDateOperand.RelativeDate.ThisMonth ) );
			list.Add( new RelativeDateOperand( "LastMonth", RelativeDateOperand.RelativeDate.LastMonth ) );
			list.Add( new RelativeDateOperand( "NextQuarter", RelativeDateOperand.RelativeDate.NextQuarter ) );
			list.Add( new RelativeDateOperand( "ThisQuarter", RelativeDateOperand.RelativeDate.ThisQuarter ) );
			list.Add( new RelativeDateOperand( "LastQuarter", RelativeDateOperand.RelativeDate.LastQuarter ) );
			list.Add( new RelativeDateOperand( "NextYear", RelativeDateOperand.RelativeDate.NextYear ) );
			list.Add( new RelativeDateOperand( "ThisYear", RelativeDateOperand.RelativeDate.ThisYear ) );
			list.Add( new RelativeDateOperand( "LastYear", RelativeDateOperand.RelativeDate.LastYear ) );
			list.Add( new RelativeDateOperand( "YearToDate", RelativeDateOperand.RelativeDate.YearToDate ) );
			list.Add( new QuarterOperand( "Quarter1", 1 ) );
			list.Add( new QuarterOperand( "Quarter2", 2 ) );
			list.Add( new QuarterOperand( "Quarter3", 3 ) );
			list.Add( new QuarterOperand( "Quarter4", 4 ) );
			list.Add( new MonthOperand( "January", 1 ) );
			list.Add( new MonthOperand( "February", 2 ) );
			list.Add( new MonthOperand( "March", 3 ) );
			list.Add( new MonthOperand( "April", 4 ) );
			list.Add( new MonthOperand( "May", 5 ) );
			list.Add( new MonthOperand( "June", 6 ) );
			list.Add( new MonthOperand( "July", 7 ) );
			list.Add( new MonthOperand( "August", 8 ) );
			list.Add( new MonthOperand( "September", 9 ) );
			list.Add( new MonthOperand( "October", 10 ) );
			list.Add( new MonthOperand( "November", 11 ) );
			list.Add( new MonthOperand( "December", 12 ) );

			return list;
		}

		#endregion // GetBuiltInOperands

		#region RegisterSerializationInfos

		/// <summary>
		/// Registers serialization information for special operands defined in this factory.
		/// </summary>
		/// <param name="serializer"></param>
		internal static void RegisterSerializationInfos( ObjectSerializerBase serializer )
		{
			serializer.RegisterInfo( typeof( BlanksOperand ), new BlanksOperand.SerializationInfo( ) );
			serializer.RegisterInfo( typeof( AverageOperand ), new AverageOperand.SerializationInfo( ) );
			serializer.RegisterInfo( typeof( TopValuesOperand ), new TopValuesOperand.SerializationInfo( ) );
			serializer.RegisterInfo( typeof( RelativeDateOperand ), new RelativeDateOperand.SerializationInfo( ) );
			serializer.RegisterInfo( typeof( MonthOperand ), new MonthOperand.SerializationInfo( ) );
			serializer.RegisterInfo( typeof( QuarterOperand ), new QuarterOperand.SerializationInfo( ) );
			serializer.RegisterInfo( typeof( DateRangeOperand ), new DateRangeOperand.SerializationInfo( ) ); // AS 1/10/12 TFS99082
		}

		#endregion // RegisterSerializationInfos

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region IGOperand Class

		internal abstract class IGOperand : SpecialFilterOperandBase
		{
			#region Nested Data Structures

			#region FormattedSRValue Class

			// SSP 3/23/10 TFS29800
			// 
			internal class FormattedSRValue : IFormattable
			{
				private string _sr;
				private object[] _args;
				private CultureInfo _defaultCultureInfo;

				internal FormattedSRValue( string sr, object[] args, CultureInfo defaultCultureInfo )
				{
					Utilities.ThrowIfNull( sr, "sr" );

					_sr = sr;
					_args = args;
					_defaultCultureInfo = defaultCultureInfo;
				}

				public override string ToString( )
				{
					return this.ToString( null, null );
				}

				public string ToString( string format, IFormatProvider formatProvider )
				{
					CultureInfo cultureInfo = _defaultCultureInfo;
					if ( formatProvider is CultureInfo )
						cultureInfo = formatProvider as CultureInfo;

					DynamicResourceString drs = SR.GetDynamicResourceString( _sr, _args, cultureInfo );
					Debug.Assert( null != drs, "No dynamic resource by this name." );

					return null != drs ? drs.Value : string.Empty;
				}
			}

			#endregion // FormattedSRValue Class

			#endregion // Nested Data Structures

			#region Member Vars

			private string _name;
			private string _sr_displayContent;
			private string _sr_description;
			private DynamicResourceString _drsDescription;

			// SSP 3/23/10 TFS29800
			// 
			//private DynamicResourceString _drsDisplayContent;
			private FormattedSRValue _fsrDisplayContent;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor. Initializes a new instance of <see cref="IGOperand"/>.
			/// </summary>
			public IGOperand( string name, string resourceName )
			{
				_name = name;
                _sr_displayContent = "SpecialFilterOperand_" + resourceName + "_DisplayContent";
                _sr_description = "SpecialFilterOperand_" + resourceName + "_Description";
			}

			/// <summary>
			/// Constructor. Initializes a new instance of <see cref="IGOperand"/>.
			/// </summary>
			public IGOperand( string name ) : this(name, name)
			{
			}

			#endregion // Constructor

			#region Base Overrides

			#region EqualsOverride

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Checks to see if the specified operand is equal to this operand. This method is called
			/// by the Equals method.
			/// </summary>
			/// <param name="operand">Operand to check for equality.</param>
			/// <returns>True if the specified operand is equal to this, false otherwise.</returns>
			protected override bool EqualsOverride( SpecialFilterOperandBase operand )
			{
				IGOperand x = operand as IGOperand;

				return null != x
					&& _name == x.Name
					&& _sr_displayContent == x._sr_displayContent
					&& _sr_description == x._sr_description;
			}

			#endregion // EqualsOverride

			#region GetHashCode

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Overridden. Returns the hash code.
			/// </summary>
			/// <returns>Hash code of the object.</returns>
			/// <remarks>
			/// <para class="body">
			/// <b>Note:</b> If you override <see cref="EqualsOverride"/> then also override this method
			/// and return an appropriate hash code.
			/// </para>
			/// </remarks>
			public override int GetHashCode( )
			{
				return null != _name ? _name.GetHashCode( ) : 0;
			}

			#endregion // GetHashCode

			#endregion // Base Overrides

			#region Properties

			#region Public Properties

			#region Description

			public override object Description
			{
				get
				{
                    if (null == _drsDescription)
                    {
                        // JJD 1/07/09 - added ValueArgumentForStrings
                        _drsDescription = SR.GetDynamicResourceString(_sr_description, this.ValueArgumentsForStrings, null);
                    }

					return _drsDescription.Value;
				}
			}

			#endregion // Description

			#region DisplayContent

			public override object DisplayContent
			{
				get
				{
					// SSP 3/23/10 TFS29800
					// 
					// ------------------------------------------------------------------------------------
					if ( null == _fsrDisplayContent )
						_fsrDisplayContent = new FormattedSRValue( _sr_displayContent, this.ValueArgumentsForStrings, null );

					return _fsrDisplayContent;

					//if (null == _drsDisplayContent)
					//{
					//    // JJD 1/07/09 - added ValueArgumentForStrings
					//    _drsDisplayContent = SR.GetDynamicResourceString(_sr_displayContent, this.ValueArgumentsForStrings, null);
					//}

					//return _drsDisplayContent.Value;
					// ------------------------------------------------------------------------------------
				}
			}

			#endregion // DisplayContent

			#region Name

			public override string Name
			{
				get
				{
					return _name;
				}
			}

			#endregion // Name

			#region UsesAllValues

			/// <summary>
			/// Overridden. Implementation returns false.
			/// </summary>
			public override bool UsesAllValues
			{
				get
				{
					return false;
				}
			}

			#endregion // UsesAllValues

			#endregion // Public Properties

            #region Protected Properties

            // JJD 1/07/09 - added
            #region ValueArgumentForStrings






            protected virtual object[] ValueArgumentsForStrings { get { return null; } }

            #endregion //ValueArgumentForStrings

            #endregion //Protected Properties	
        
			#endregion // Properties

			#region Methods

			#region Public Methods

			#region IsMatch

			public override bool IsMatch( ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context )
			{
				// If a non-supported operator is passed in or non-supported data, return false.
				// 
				if ( !this.SupportsOperator( comparisonOperator )
					|| null != value && DBNull.Value != value && !this.SupportsDataType( value.GetType( ) ) )
					return false;
				
				return true;
			}

			#endregion // IsMatch

			#endregion // Public Methods

			#region Private/Internal Methods

			#region ConvertValue

			internal static object ConvertValue( object value, Type targetType, ConditionEvaluationContext context )
			{
				return ComparisonCondition.ConvertDataTypeHelper( value, targetType, context );
			}

			#endregion // ConvertValue

			#region GetCurrentValueAsDouble

			internal static double GetCurrentValueAsDouble( object value, ConditionEvaluationContext context )
			{
				object r = ComparisonCondition.ConvertDataTypeHelper( value, typeof( double ), context );

				return r is double ? (double)r : double.NaN;
			}

			#endregion // GetCurrentValueAsDouble

			#endregion // Private/Internal Methods

			#endregion // Methods
		}

		#endregion // IGOperand Class

		#region BlanksOperand Class

		/// <summary>
		/// Special operand for "Blanks" and "NonBlanks" filter operations.
		/// </summary>
		internal class BlanksOperand : IGOperand
		{
			#region Nested Data Structures

			#region SerializationInfo Class

			/// <summary>
			/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
			/// </summary>
			internal class SerializationInfo : ObjectSerializationInfo
			{
				private PropertySerializationInfo[] _props;
				private const string NonBlanksProp = "MatchNonBlanks";

				public override IEnumerable<PropertySerializationInfo> SerializedProperties
				{
					get
					{
						if ( null == _props )
						{
							_props = new PropertySerializationInfo[]
							{
								new PropertySerializationInfo( typeof( bool ), NonBlanksProp )
							};
						}

						return _props;
					}
				}

				public override Dictionary<string, object> Serialize( object obj )
				{
					BlanksOperand x = (BlanksOperand)obj;

					Dictionary<string, object> values = new Dictionary<string, object>( );

					values[NonBlanksProp] = x._nonBlanks;

					return values;
				}

				public override object Deserialize( Dictionary<string, object> values )
				{
					bool matchNonBlanks = false;

					object v;
					if ( values.TryGetValue( NonBlanksProp, out v ) )
						matchNonBlanks = (bool)v;

					return new BlanksOperand( matchNonBlanks );
				}
			}

			#endregion // SerializationInfo Class

			#endregion // Nested Data Structures

			#region Member Vars

			internal const string NAME_Blanks = "Blanks";
			internal const string NAME_NonBlanks = "NonBlanks";

			private bool _nonBlanks;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor. Initializes a new instance of <see cref="BlanksOperand"/>.
			/// </summary>
			/// <param name="nonBlanks">Specifies whether to match blanks or non-blanks.</param>
			public BlanksOperand( bool nonBlanks )
				: base( !nonBlanks ? NAME_Blanks : NAME_NonBlanks )
			{
				_nonBlanks = nonBlanks;
			}

			#endregion // Constructor

			#region Base Overrides

			#region EqualsOverride

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Checks to see if the specified operand is equal to this operand. This method is called
			/// by the Equals method.
			/// </summary>
			/// <param name="operand">Operand to check for equality.</param>
			/// <returns>True if the specified operand is equal to this, false otherwise.</returns>
			protected override bool EqualsOverride( SpecialFilterOperandBase operand )
			{
				BlanksOperand x = operand as BlanksOperand;

				return null != x
					&& base.EqualsOverride( x )
					&& _nonBlanks == x._nonBlanks;
			}

			#endregion // EqualsOverride

			#region GetHashCode

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Overridden. Returns the hash code.
			/// </summary>
			/// <returns>Hash code of the object.</returns>
			/// <remarks>
			/// <para class="body">
			/// <b>Note:</b> If you override <see cref="EqualsOverride"/> then also override this method
			/// and return an appropriate hash code.
			/// </para>
			/// </remarks>
			public override int GetHashCode( )
			{
				return base.GetHashCode( )
					^ _nonBlanks.GetHashCode( );
			}

			#endregion // GetHashCode

			#region SupportsDataType

			public override bool SupportsDataType( Type type )
			{
				return true;
			}

			#endregion // SupportsDataType

			#region SupportsOperator

			public override bool SupportsOperator( ComparisonOperator comparisonOperator )
			{
				return ComparisonOperator.Equals == comparisonOperator
					|| ComparisonOperator.NotEquals == comparisonOperator;
			}

			#endregion // SupportsOperator

			#region IsMatch

			public override bool IsMatch( ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context )
			{
				if ( ComparisonOperator.Equals == comparisonOperator )
					return _nonBlanks != IsBlank( value, context );

				if ( ComparisonOperator.NotEquals == comparisonOperator )
					return !this.IsMatch( ComparisonOperator.Equals, value, context );

				// If a non-supported operator is passed in, return false.
				// 
				return false;
			}

			#endregion // IsMatch

			#endregion // Base Overrides

			#region Methods

			#region Private/Internal Methods

			#region IsBlank

			private static bool IsBlank( object value, ConditionEvaluationContext context )
			{
				// AS 6/8/11 TFS78448 - Optimization
				// Don't call ToString on known types.
				//
				//return null == value || value is DBNull
				//        || value is string && 0 == ( (string)value ).Length
				//        || string.IsNullOrEmpty( value.ToString( ) );
				if (null == value || value is DBNull)
					return true;
				else if (value is string)
					return 0 == ( (string)value ).Length;

				Type valueType = value.GetType();

				if (valueType.IsPrimitive 
					|| value is DateTime 
					|| value is Decimal
					|| valueType.IsEnum
					|| value is TimeSpan
					)
					return false;

				return string.IsNullOrEmpty(value.ToString());
			}

			#endregion // IsBlank

			#endregion // Private/Internal Methods

			#endregion // Methods
		}

		#endregion // BlanksOperand Class

		#region AverageOperand Class

		/// <summary>
		/// Special operand for filtering values that are above or below average (but not equal).
		/// </summary>
		internal class AverageOperand : IGOperand
		{
			#region Nested Data Structures

			#region SerializationInfo Class

			/// <summary>
			/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
			/// </summary>
			internal class SerializationInfo : ObjectSerializationInfo
			{
				private PropertySerializationInfo[] _props;
				private const string AboveAverageProp = "AboveAverage";

				public override IEnumerable<PropertySerializationInfo> SerializedProperties
				{
					get
					{
						if ( null == _props )
						{
							_props = new PropertySerializationInfo[]
							{
								new PropertySerializationInfo( typeof( bool ), AboveAverageProp )
							};
						}

						return _props;
					}
				}

				public override Dictionary<string, object> Serialize( object obj )
				{
					AverageOperand x = (AverageOperand)obj;

					Dictionary<string, object> values = new Dictionary<string, object>( );

					values[AboveAverageProp] = x._aboveAverage;

					return values;
				}

				public override object Deserialize( Dictionary<string, object> values )
				{
					bool aboveAverage = false;

					object v;
					if ( values.TryGetValue( AboveAverageProp, out v ) )
						aboveAverage = (bool)v;

					return new AverageOperand( aboveAverage );
				}
			}

			#endregion // SerializationInfo Class

			#endregion // Nested Data Structures

			#region Member Vars

			internal const string NAME_AboveAverage = "AboveAverage";
			internal const string NAME_BelowAverage = "BelowAverage";

			private bool _aboveAverage;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor. Initializes a new instance of <see cref="AverageOperand"/>.
			/// </summary>
			/// <param name="aboveAverage">If true then matches values that are above average, 
			/// otherwise matches values that are below average.</param>
			public AverageOperand( bool aboveAverage )
				: base( aboveAverage ? NAME_AboveAverage : NAME_BelowAverage )
			{
				_aboveAverage = aboveAverage;
			}

			#endregion // Constructor

			#region Base Overrides

			#region SupportsDataType

			public override bool SupportsDataType( Type type )
			{
				return Utilities.IsNumericType( Utilities.GetUnderlyingType( type ) );
			}

			#endregion // SupportsDataType

			#region SupportsOperator

			public override bool SupportsOperator( ComparisonOperator comparisonOperator )
			{
				return ComparisonOperator.Equals == comparisonOperator
					|| ComparisonOperator.NotEquals == comparisonOperator;
			}

			#endregion // SupportsOperator

			#region IsMatch

			public override bool IsMatch( ComparisonOperator comparisonOperator, object valueObj, ConditionEvaluationContext context )
			{
				if ( ComparisonOperator.Equals == comparisonOperator )
				{
					double value = GetCurrentValueAsDouble( valueObj, context );
					if ( !double.IsNaN( value ) )
					{
						double average = this.GetAverage( context );
						if ( double.IsNaN( average ) )
							return false;

						return _aboveAverage 
							? value > average
							: value < average;
					}
				}
				else if ( ComparisonOperator.NotEquals == comparisonOperator )
				{
					return ! this.IsMatch( ComparisonOperator.Equals, valueObj, context );
				}

				return false;
			}

			#endregion // IsMatch

			#region UsesAllValues

			// SSP 5/24/11 TFS76271
			// 
			/// <summary>
			/// Overridden. Returns true.
			/// </summary>
			public override bool UsesAllValues
			{
				get
				{
					return true;
				}
			}

			#endregion // UsesAllValues

			#endregion // Base Overrides

			#region Methods

			#region Private/Internal Methods

			private double GetAverage( ConditionEvaluationContext context )
			{
				// SSP 7/21/11 TFS81583
				// 
				//object cachedValue = context.UserCache;
				object CACHE_ID = this.GetType( );
				object cachedValue = context.GetUserCache( CACHE_ID );

				if ( null != cachedValue )
					return (double)cachedValue;

				int count = 0;
				double sum = 0;

				foreach ( ValueEntry ii in context.AllValues )
				{
					double d = ii.ValueAsDouble;
					if ( !double.IsNaN( d ) && !double.IsInfinity( d ) )
					{
						sum += d;
						count++;
					}
				}

				double average = count > 0 ? sum / count : double.NaN;
				// SSP 7/21/11 TFS81583
				// 
				//context.UserCache = average;
				context.SetUserCache( CACHE_ID, average );

				return average;
			}

			#endregion // Private/Internal Methods

			#endregion // Methods
		}

		#endregion // AverageOperand Class

		#region TopValuesOperand Class

		/// <summary>
		/// Special operand for filtering values that are among top specified number of values
		/// or top specified percentage of values.
		/// </summary>
		internal class TopValuesOperand : IGOperand
		{
			#region Nested Data Structures

			#region SerializationInfo Class

			/// <summary>
			/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
			/// </summary>
			internal class SerializationInfo : ObjectSerializationInfo
			{
				private PropertySerializationInfo[] _props;

				public override IEnumerable<PropertySerializationInfo> SerializedProperties
				{
					get
					{
						if ( null == _props )
						{
							_props = new PropertySerializationInfo[]
							{
								new PropertySerializationInfo( typeof( string ), "Name" ),
								new PropertySerializationInfo( typeof( double ), "TopNumber" ),
								new PropertySerializationInfo( typeof( bool ), "Percentage" ),
								new PropertySerializationInfo( typeof( bool ), "AscendingOrder" ),
							};
						}

						return _props;
					}
				}

				public override Dictionary<string, object> Serialize( object obj )
				{
					TopValuesOperand x = (TopValuesOperand)obj;

					Dictionary<string, object> values = new Dictionary<string, object>( );

					values["Name"] = x.Name;
					values["TopNumber"] = x._topNumber;
					values["Percentage"] = x._percentage;
					values["AscendingOrder"] = x._ascendingOrder;

					return values;
				}

				public override object Deserialize( Dictionary<string, object> values )
				{
					object name;
					object topNumber;
					object percentage;
					object ascendingOrder;

					if ( values.TryGetValue( "Name", out name )
						&& values.TryGetValue( "TopNumber", out topNumber )
						&& values.TryGetValue( "Percentage", out percentage )
						&& values.TryGetValue( "AscendingOrder", out ascendingOrder ) )
					{
						return new TopValuesOperand( (string)name, (double)topNumber, (bool)percentage, (bool)ascendingOrder );
					}

					return null;
				}
			}

			#endregion // SerializationInfo Class

			#endregion // Nested Data Structures

			#region Member Vars

			private double _topNumber;
			private bool _percentage;
			private bool _ascendingOrder;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor. Initializes a new instance of <see cref="TopValuesOperand"/>.
			/// </summary>
			/// <param name="name">Name of the operand.</param>
			/// <param name="topNumber">Specifies either the top absolute number of values or top percentage of values depending on 'percentage' parameter.</param>
			/// <param name="percentage">If true then values that are among specified top percentage are filtered. Otherwise 
			/// the 'topNumber' parameter specifies the absoulte number of values.</param>
			/// <param name="ascendingOrder">If true then top-most (largest) values will be returned, otherwise bottom-most (smallest) values will be returned.</param>
			public TopValuesOperand( string name, double topNumber, bool percentage, bool ascendingOrder )
				: base( name, percentage ? (ascendingOrder ? "TopPercentile" : "BottomPercentile") : (ascendingOrder ? "Top" : "Bottom") )
			{
				Exception error;
				if ( ! ValidateParams( topNumber, percentage, ascendingOrder, out error ) )
					throw error;

				_topNumber = topNumber;
				_percentage = percentage;
				_ascendingOrder = ascendingOrder;
			}

			#endregion // Constructor

			#region Base Overrides

			#region EqualsOverride

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Checks to see if the specified operand is equal to this operand. This method is called
			/// by the Equals method.
			/// </summary>
			/// <param name="operand">Operand to check for equality.</param>
			/// <returns>True if the specified operand is equal to this, false otherwise.</returns>
			protected override bool EqualsOverride( SpecialFilterOperandBase operand )
			{
				TopValuesOperand x = operand as TopValuesOperand;

				return null != x
					&& base.EqualsOverride( x )
					&& this._ascendingOrder == x._ascendingOrder
					&& this._percentage == x._percentage
					&& this._topNumber == x._topNumber;
			}

			#endregion // EqualsOverride

			#region GetHashCode

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Overridden. Returns the hash code.
			/// </summary>
			/// <returns>Hash code of the object.</returns>
			/// <remarks>
			/// <para class="body">
			/// <b>Note:</b> If you override <see cref="EqualsOverride"/> then also override this method
			/// and return an appropriate hash code.
			/// </para>
			/// </remarks>
			public override int GetHashCode( )
			{
				return base.GetHashCode( ) 
					^ _ascendingOrder.GetHashCode( )
					^ _percentage.GetHashCode( )
					^ _topNumber.GetHashCode( );
			}

			#endregion // GetHashCode

			#region SupportsDataType

			public override bool SupportsDataType( Type type )
			{
				type = Utilities.GetUnderlyingType( type );
				return Utilities.IsNumericType( type )
					// Also support DateTime values. Top means latest dates and Bottom 
					// means earliest dates.
					// 
					|| typeof( DateTime ) == type;
			}

			#endregion // SupportsDataType

			#region SupportsOperator

			public override bool SupportsOperator( ComparisonOperator comparisonOperator )
			{
				return ComparisonOperator.Equals == comparisonOperator
					|| ComparisonOperator.NotEquals == comparisonOperator;
			}

			#endregion // SupportsOperator

			#region IsMatch

			public override bool IsMatch( ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context )
			{
				if ( ComparisonOperator.Equals == comparisonOperator )
				{
					// SSP 8/6/12 TFS113405
					// Modified the operand to support types other than numeric and date-time.
					// Enclosed the existing code into the if block and added the else block.
					// 
					//double max = this.GetMaxValidValue( context );
					object maxObj = this.GetMaxValidValue( context );
					if ( maxObj is double )
					{
						double max = (double)maxObj;

						double val = GetDoubleHelper( value, context );
						if ( !double.IsNaN( max ) && !double.IsNaN( val ) )
							return _ascendingOrder ? val >= max : val <= max;
						else
							return false;
					}
					else
					{
						IComparer comparer = context.Comparer ?? Comparer<object>.Default;

						if ( null != maxObj && null != value )
						{
							int r = comparer.Compare( value, maxObj );
							return _ascendingOrder ? r >= 0 : r <= 0;
						}
						else
							return false;
					}
				}
				else if ( ComparisonOperator.NotEquals == comparisonOperator )
					return ! this.IsMatch( ComparisonOperator.Equals, value, context );

				return false;
			}

			#endregion // IsMatch

			#region UsesAllValues

			// SSP 5/24/11 TFS76271
			// 
			/// <summary>
			/// Overridden. Returns true.
			/// </summary>
			public override bool UsesAllValues
			{
				get
				{
					return true;
				}
			}

			#endregion // UsesAllValues

            // JJD 1/07/09 - added
            #region ValueArgumentForStrings






            protected override object[] ValueArgumentsForStrings { get { return new object[] { this._topNumber }; } }

            #endregion //ValueArgumentForStrings

			#endregion // Base Overrides

			#region Methods

			#region Private/Internal Methods

			#region ClearUserCache

			// SSP 7/21/11 TFS81583
			// 
			internal static void ClearUserCacheHelper( SpecialFilterOperandBase topValuesOperand, ConditionEvaluationContext context )
			{
				TopValuesOperand ii = topValuesOperand as TopValuesOperand;
				if ( null != ii )
					context.SetUserCache( ii, null );
			} 

			#endregion // ClearUserCache

			#region CreateHelper

			/// <summary>
			/// Creates a new instance of TopValuesOperand if the specified parameters are valid. Otherwise
			/// returns null and error is set to appropriate error message.
			/// </summary>
			internal static TopValuesOperand CreateHelper( string name, double topNumber, bool percentage, bool ascendingOrder, out Exception error )
			{
				if ( !ValidateParams( topNumber, percentage, ascendingOrder, out error ) )
					return null;

				return new TopValuesOperand( name, topNumber, percentage, ascendingOrder );
			}

			#endregion // CreateHelper

			#region DateTimeToDouble

			private static double DateTimeToDouble( DateTime value )
			{
				return ( (DateTime)value ).Ticks;
			}

			#endregion // DateTimeToDouble

			#region GetAllValues

			// SSP 8/6/12 TFS113405
			// Modified the operand to support types other than numeric and date-time.
			// 
			private IList GetAllValues( ConditionEvaluationContext context )
			{
				Type dataType = context.PreferredComparisonDataType;
				bool numericDataType = IsNumericOrDateTime( dataType );

				List<double> listDouble = new List<double>( );
				foreach ( ValueEntry ii in context.AllValues )
				{
					double d = GetDoubleHelper( ii, context );
					if ( !double.IsNaN( d ) && !double.IsInfinity( d ) )
					{
						listDouble.Add( d );
					}
					// If we failed to convert the value to a number then we should fallback to performing
					// generic object comparison via context.Comparer or Comparer<object>.Default.
					// 
					else if ( !numericDataType && !CoreUtilities.IsValueEmpty( ii.Value ) 
						&& ( null == dataType || ! IsNumericOrDateTime( ii.Value.GetType( ) ) ) )
					{
						listDouble = null;
						break;
					}
				}

				List<object> listObj = null;
				if ( null == listDouble )
				{
					listObj = new List<object>( );
					foreach ( ValueEntry ii in context.AllValues )
						listObj.Add( ii.Value );
				}

				return listDouble ?? (IList)listObj;
			}

			#endregion // GetAllValues

			#region GetDoubleHelper

			private static double GetDoubleHelper( object value, ConditionEvaluationContext context )
			{
				if ( value is DateTime )
					return DateTimeToDouble( (DateTime)value );

				return GetCurrentValueAsDouble( value, context );
			}

			private static double GetDoubleHelper( ValueEntry ii, ConditionEvaluationContext context )
			{
				double d;
				object val = ii.Value;
				if ( val is DateTime )
					d = DateTimeToDouble( (DateTime)val );
				else
					d = ii.ValueAsDouble;

				return d;
			}

			#endregion // GetDoubleHelper

			#region GetMaxValidValue

			// SSP 8/6/12 TFS113405
			// Modified the operand to support types other than numeric and date-time.
			// 
			//private double GetMaxValidValue( ConditionEvaluationContext context )
			private object GetMaxValidValue( ConditionEvaluationContext context )
			{
				// SSP 7/21/11 TFS81583
				// 
				//object cachedValue = context.UserCache;
				object cachedValue = context.GetUserCache( this );
				if ( null == cachedValue )
				{
					// SSP 8/6/12 TFS113405
					// Modified the operand to support types other than numeric and date-time.
					// 
					
					IList list = this.GetAllValues( context );
					//List<double> list = new List<double>( );

					//foreach ( ValueEntry ii in context.AllValues )
					//{
					//    double d = GetDoubleHelper( ii, context );
					//    if ( !double.IsNaN( d ) && !double.IsInfinity( d ) )
					//        list.Add( d );
					//}
					

					cachedValue = double.NaN;
					Debug.Assert( list.Count > 0, "AllValues is returning no values but it's IsMatch is getting called on a value !" );
					if ( list.Count > 0 && _topNumber > 0 )
					{
						// SSP 8/6/12 TFS113405
						// Modified the operand to support types other than numeric and date-time.
						// 
						
						if ( list is List<double> )
							Utilities.SortMergeGeneric<double>( (List<double>)list, Comparer<double>.Default );
						else
							Utilities.SortMergeGeneric<object>( (List<object>)list, CoreUtilities.CreateComparer<object>( context.Comparer ?? Comparer<object>.Default ) );

						int delta;
						if ( _percentage )
							delta = Math.Max( 1, (int)( ( _topNumber / 100.0 ) * ( list.Count ) ) ) - 1;
						else
							delta = (int)_topNumber - 1;

						if ( delta >= 0 )
						{
							delta = Math.Min( delta, list.Count - 1 );
							cachedValue = !_ascendingOrder ? list[delta] : list[list.Count - 1 - delta];
						}
					}

					// SSP 7/21/11 TFS81583
					// 
					//context.UserCache = cachedValue;
					context.SetUserCache( this, cachedValue );
				}

				return cachedValue;
			}

			#endregion // GetMaxValidValue

			#region IsNumericOrDateTime

			// SSP 8/6/12 TFS113405
			// 
			private static bool IsNumericOrDateTime( Type type )
			{
				return null != type && ( Utilities.IsNumericType( type ) || typeof( DateTime ) == type );
			} 

			#endregion // IsNumericOrDateTime

			#region ValidateParams

			/// <summary>
			/// Validates arguements to the constructor.
			/// </summary>
			internal static bool ValidateParams( double topNumber, bool percentage, bool ascendingOrder, out Exception error )
			{
				error = null;
				if ( double.IsNaN( topNumber ) )
					error = new ArgumentOutOfRangeException( "topNumber", SR.GetString( "Filter_TopOperand_InvalidValue_NaN" ) );
				else if ( topNumber < 0 )
					error = new ArgumentOutOfRangeException( "topNumber", SR.GetString( "Filter_TopOperand_InvalidValue_LessThanZero" ) );
				else if ( percentage && topNumber > 100 )
					error = new ArgumentOutOfRangeException( "topNumber", SR.GetString( "Filter_TopOperand_InvalidValue_GreaterThan100Percent" ) );

				return null == error;
			}

			#endregion // ValidateParams

			#endregion // Private/Internal Methods

			#endregion // Methods
		}

		#endregion // TopValuesOperand Class

		
		
		
		
		
		

		#region Date Operands

		#region DateOperand Class







		internal class DateOperand : IGOperand
		{
			#region Constructor

			public DateOperand( string name, string resourceName ) : base( name, resourceName )
			{
			}

			#endregion //Constructor

			#region Base Class Overrides

			#region IsMatch

			public override bool IsMatch( ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context )
			{
				if ( base.IsMatch( comparisonOperator, value, context ) == false )
					return false;

				// Excel doesn't consider null values to match dates
				if ( value == null || value == DBNull.Value || Utilities.GetUnderlyingType( value.GetType( ) ) != typeof( DateTime ) )
					return false;

				return true;
			}

			#endregion // IsMatch

			#region SupportsDataType



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			public override bool SupportsDataType( Type dataType )
			{
				return Utilities.GetUnderlyingType( dataType ) == typeof( DateTime );
			}

			#endregion //SupportsDataType

			#region SupportsOperator



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			public override bool SupportsOperator( ComparisonOperator comparisonOperator )
			{
				switch ( comparisonOperator )
				{
					case ComparisonOperator.Equals:
					case ComparisonOperator.NotEquals:
					case ComparisonOperator.LessThan:
					case ComparisonOperator.LessThanOrEqualTo:
					case ComparisonOperator.GreaterThan:
					case ComparisonOperator.GreaterThanOrEqualTo:
						return true;
				}

				return false;
			}
			#endregion //SupportsOperator

			#endregion //Base Class Overrides
		}

		#endregion // DateOperand Class

		#region RelativeDateOperand Class







		internal class RelativeDateOperand : DateOperand
		{
			#region Nested Data Structures

			#region RelativeDate - Enum

			internal enum RelativeDate
			{
				Tomorrow = 0,
				Today,
				Yesterday,
				NextWeek,
				ThisWeek,
				LastWeek,
				NextMonth,
				ThisMonth,
				LastMonth,
				NextQuarter,
				ThisQuarter,
				LastQuarter,
				NextYear,
				ThisYear,
				LastYear,
				YearToDate,
			}
			#endregion //RelativeDate - Enum

			#region SerializationInfo Class

			/// <summary>
			/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
			/// </summary>
			internal class SerializationInfo : ObjectSerializationInfo
			{
				private PropertySerializationInfo[] _props;

				public override IEnumerable<PropertySerializationInfo> SerializedProperties
				{
					get
					{
						if ( null == _props )
						{
							_props = new PropertySerializationInfo[]
							{
								new PropertySerializationInfo( typeof( string ), "Name" ),
								
								
								
								
								new PropertySerializationInfo( typeof( RelativeDate ), "RelativeDate" )
							};
						}

						return _props;
					}
				}

				public override Dictionary<string, object> Serialize( object obj )
				{
					RelativeDateOperand x = (RelativeDateOperand)obj;

					Dictionary<string, object> values = new Dictionary<string, object>( );

					values["Name"] = x.Name;
					values["RelativeDate"] = x.relativeDate;

					return values;
				}

				public override object Deserialize( Dictionary<string, object> values )
				{
					object name;
					object relativeDate;

					if ( values.TryGetValue( "Name", out name )
						&& values.TryGetValue( "RelativeDate", out relativeDate ) )
					{
						return new RelativeDateOperand( (string)name, (RelativeDate)relativeDate );
					}

					return null;
				}
			}

			#endregion // SerializationInfo Class

			#endregion // Nested Data Structures

			#region Members

			private bool useCustomCurrentDate;
			private DateTime customCurrentDate;
			private RelativeDate relativeDate;
			private DateTime cachedCurrentDate;
			// AS 8/15/11 TFS84145
			// This isn't necessary for this fix but there is no need to 
			// have different members for all of these when all we use is 
			// one set of start/end members.
			//
			//private DateTime cachedLastWeekStart;
			//private DateTime cachedLastWeekEnd;
			//private DateTime cachedLastMonthStart;
			//private DateTime cachedLastMonthEnd;
			//private DateTime cachedLastQuarterStart;
			//private DateTime cachedLastQuarterEnd;
			//private DateTime cachedLastYearStart;
			//private DateTime cachedLastYearEnd;
			//private DateTime cachedThisWeekStart;
			//private DateTime cachedThisWeekEnd;
			//private DateTime cachedThisMonthStart;
			//private DateTime cachedThisMonthEnd;
			//private DateTime cachedThisQuarterStart;
			//private DateTime cachedThisQuarterEnd;
			//private DateTime cachedThisYearStart;
			//private DateTime cachedThisYearEnd;
			//private DateTime cachedNextWeekStart;
			//private DateTime cachedNextWeekEnd;
			//private DateTime cachedNextMonthStart;
			//private DateTime cachedNextMonthEnd;
			//private DateTime cachedNextQuarterStart;
			//private DateTime cachedNextQuarterEnd;
			//private DateTime cachedNextYearStart;
			//private DateTime cachedNextYearEnd;
			//private DateTime cachedYearToDateStart;
			private DateTime cachedStart;
			private DateTime cachedEnd;

			// AS 8/15/11 TFS84145
			// Since we're using the FirstDayOfWeek we need to cache the current culture.
			//
			private CultureInfo cachedCurrentCulture; 

			#endregion //Members

			#region Constuctor

			public RelativeDateOperand( string name, RelativeDate relativeDate ) : base( name, name )
			{
				this.relativeDate = relativeDate;
			}
			#endregion //Constructor

			#region Methods

			#region CalculateQuarterRange

			private static void CalculateQuarterRange( DateTime dateInQuarter, ref DateTime cachedStart, ref DateTime cachedEnd )
			{
				if ( dateInQuarter.Month < 4 )
				{
					cachedStart = new DateTime( dateInQuarter.Year, 1, 1 );
					cachedEnd = new DateTime( dateInQuarter.Year, 3, DateTime.DaysInMonth( dateInQuarter.Year, 3 ) );
				}
				else if ( dateInQuarter.Month < 7 )
				{
					cachedStart = new DateTime( dateInQuarter.Year, 4, 1 );
					cachedEnd = new DateTime( dateInQuarter.Year, 6, DateTime.DaysInMonth( dateInQuarter.Year, 6 ) );
				}
				else if ( dateInQuarter.Month < 10 )
				{
					cachedStart = new DateTime( dateInQuarter.Year, 7, 1 );
					cachedEnd = new DateTime( dateInQuarter.Year, 9, DateTime.DaysInMonth( dateInQuarter.Year, 9 ) );
				}
				else
				{
					cachedStart = new DateTime( dateInQuarter.Year, 10, 1 );
					cachedEnd = new DateTime( dateInQuarter.Year, 12, DateTime.DaysInMonth( dateInQuarter.Year, 12 ) );
				}
			}
			#endregion //CalculateQuarterRange

			#region MatchHelper

			private static bool MatchHelper( ComparisonOperator comparisonOperator, DateTime rangeStart, DateTime rangeEnd, DateTime comparisonValue )
			{
				// AS 8/15/11 TFS84145 - Optimization
				// This is not necessary since we are caching the start/end.
				//
				//// Sanity check to make sure that the range is valid
				//if ( rangeStart > rangeEnd )
				//{
				//    DateTime temp = rangeEnd;
				//    rangeEnd = rangeStart;
				//    rangeStart = temp;
				//}
				Debug.Assert(rangeStart <= rangeEnd, "We should be passed a normalized start/end");

				switch ( comparisonOperator )
				{
					case ComparisonOperator.Equals:
						return rangeStart <= comparisonValue && rangeEnd >= comparisonValue;

					case ComparisonOperator.NotEquals:
						return false == MatchHelper( ComparisonOperator.Equals, rangeStart, rangeEnd, comparisonValue );

					case ComparisonOperator.LessThan:
						return comparisonValue < rangeStart;

					case ComparisonOperator.LessThanOrEqualTo:
						// When a value is equal to a relative date/month/etc, it can span
						// a range of dates, so we need to check both start and end
						return comparisonValue < rangeStart ||
							MatchHelper( ComparisonOperator.Equals, rangeStart, rangeEnd, comparisonValue );

					case ComparisonOperator.GreaterThan:
						return comparisonValue > rangeEnd;

					case ComparisonOperator.GreaterThanOrEqualTo:
						// When a value is equal to a relative date/month/etc, it can span
						// a range of dates, so we need to check both start and end
						return comparisonValue > rangeEnd ||
							MatchHelper( ComparisonOperator.Equals, rangeStart, rangeEnd, comparisonValue );
				}

				Debug.Fail( "We should not be trying to match an unsupported FilterComparisonOperator" );
				return false;
			}
			#endregion //MatchHelper

			#region ResetCurrentDate






			private void ResetCurrentDate( )
			{
				this.useCustomCurrentDate = false;
				this.cachedCurrentDate = DateTime.MinValue;
			}
			#endregion //ResetCurrentDate

			#region SetCurrentDate







			private void SetCurrentDate( DateTime currentDate )
			{
				this.useCustomCurrentDate = true;
				this.customCurrentDate = currentDate;
			}
			#endregion //SetCurrentDate

			#region VerifyCache

			private void VerifyCache( ConditionEvaluationContext context  )
			{
				// AS 8/15/11 TFS84145
				// We also want to compare the culture.
				//
				//if ( ( this.useCustomCurrentDate && this.cachedCurrentDate == this.customCurrentDate ) ||
				//    ( !this.useCustomCurrentDate && this.cachedCurrentDate == DateTime.Today ) )
				//    return;
				CultureInfo culture = (context != null && context.CurrentValue != null ? context.CurrentValue.Culture : null) ?? CultureInfo.CurrentCulture;

				if (this.cachedCurrentDate == (this.useCustomCurrentDate ? this.customCurrentDate : DateTime.Today))
				{
					if (culture == this.cachedCurrentCulture)
						return;
				}

				// The custom dates are only meant to be used for unit testing
				if ( this.useCustomCurrentDate )
					this.cachedCurrentDate = this.customCurrentDate;
				else
					this.cachedCurrentDate = DateTime.Today;

				this.cachedCurrentCulture = culture; // AS 8/15/11 TFS84145

				// AS 8/15/11 TFS84145 - Optimization
				//int daysInWeek = Enum.GetValues( typeof( DayOfWeek ) ).Length;
				const int daysInWeek = 7;

				switch ( this.relativeDate )
				{
					// AS 8/15/11 TFS84145
					// Take the DateTimeFormatInfo.FirstDayOfWeek into consideration.
					//
					//case RelativeDate.LastWeek:
					//    DateTime dateInLastWeek = this.cachedCurrentDate.AddDays( -1 * daysInWeek );
					//    if ( dateInLastWeek.DayOfWeek > 0 )
					//        dateInLastWeek = dateInLastWeek.AddDays( -1 * Convert.ToInt32( dateInLastWeek.DayOfWeek ) );
					//
					//    this.cachedLastWeekStart = dateInLastWeek;
					//    this.cachedLastWeekEnd = dateInLastWeek.AddDays( daysInWeek - 1 );
					//    break;
					//
					//case RelativeDate.ThisWeek:
					//    DateTime dateInThisWeek = this.cachedCurrentDate;
					//    if ( dateInThisWeek.DayOfWeek > 0 )
					//        dateInThisWeek = dateInThisWeek.AddDays( -1 * Convert.ToInt32( dateInThisWeek.DayOfWeek ) );
					//
					//    this.cachedThisWeekStart = dateInThisWeek;
					//    this.cachedThisWeekEnd = dateInThisWeek.AddDays( daysInWeek - 1 );
					//    break;
					//
					//case RelativeDate.NextWeek:
					//    DateTime dateInNextWeek = this.cachedCurrentDate.AddDays( daysInWeek );
					//    if ( dateInNextWeek.DayOfWeek > 0 )
					//        dateInNextWeek = dateInNextWeek.AddDays( -1 * Convert.ToInt32( dateInNextWeek.DayOfWeek ) );
					//
					//    this.cachedNextWeekStart = dateInNextWeek;
					//    this.cachedNextWeekEnd = dateInNextWeek.AddDays( daysInWeek - 1 );
					//    break;
					case RelativeDate.LastWeek:
					case RelativeDate.ThisWeek:
					case RelativeDate.NextWeek:
						{
							DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
							int dowOffset = ((int)this.cachedCurrentDate.DayOfWeek - (int)firstDayOfWeek + daysInWeek) % daysInWeek;
							DateTime thisWeekStart = this.cachedCurrentDate.AddDays(-dowOffset);

							if (this.relativeDate == RelativeDate.LastWeek)
								this.cachedStart = thisWeekStart.AddDays(-7);
							else if (this.relativeDate == RelativeDate.NextWeek)
								this.cachedStart = thisWeekStart.AddDays(7);
							else
								this.cachedStart = thisWeekStart;

							this.cachedEnd = this.cachedStart.AddDays(daysInWeek - 1);
							break;
						}

					case RelativeDate.LastMonth:
						DateTime lastMonth = this.cachedCurrentDate.AddMonths( -1 );
						this.cachedStart = new DateTime( lastMonth.Year, lastMonth.Month, 1 );
						this.cachedEnd = new DateTime( lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth( lastMonth.Year, lastMonth.Month ) );
						break;

					case RelativeDate.ThisMonth:
						DateTime thisMonth = this.cachedCurrentDate;
						this.cachedStart = new DateTime( thisMonth.Year, thisMonth.Month, 1 );
						this.cachedEnd = new DateTime( thisMonth.Year, thisMonth.Month, DateTime.DaysInMonth( thisMonth.Year, thisMonth.Month ) );
						break;

					case RelativeDate.NextMonth:
						DateTime nextMonth = this.cachedCurrentDate.AddMonths( 1 );
						this.cachedStart = new DateTime( nextMonth.Year, nextMonth.Month, 1 );
						this.cachedEnd = new DateTime( nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth( nextMonth.Year, nextMonth.Month ) );
						break;

					case RelativeDate.LastQuarter:
						DateTime dateInLastQuarter = this.cachedCurrentDate.AddMonths( -3 );
						RelativeDateOperand.CalculateQuarterRange( dateInLastQuarter, ref this.cachedStart, ref this.cachedEnd );
						break;

					case RelativeDate.ThisQuarter:
						RelativeDateOperand.CalculateQuarterRange( this.cachedCurrentDate, ref this.cachedStart, ref this.cachedEnd );
						break;

					case RelativeDate.NextQuarter:
						DateTime dateInNextQuarter = this.cachedCurrentDate.AddMonths( 3 );
						RelativeDateOperand.CalculateQuarterRange( dateInNextQuarter, ref this.cachedStart, ref this.cachedEnd );
						break;

					case RelativeDate.LastYear:
						DateTime dateInLastYear = this.cachedCurrentDate.AddYears( -1 );
						this.cachedStart = new DateTime( dateInLastYear.Year, 1, 1 );
						this.cachedEnd = new DateTime( dateInLastYear.Year, 12, DateTime.DaysInMonth( dateInLastYear.Year, 12 ) );
						break;

					case RelativeDate.ThisYear:
						this.cachedStart = new DateTime( this.cachedCurrentDate.Year, 1, 1 );
						this.cachedEnd = new DateTime( this.cachedCurrentDate.Year, 12, DateTime.DaysInMonth( this.cachedCurrentDate.Year, 12 ) );
						break;

					case RelativeDate.NextYear:
						DateTime dateInNextYear = this.cachedCurrentDate.AddYears( 1 );
						this.cachedStart = new DateTime( dateInNextYear.Year, 1, 1 );
						this.cachedEnd = new DateTime( dateInNextYear.Year, 12, DateTime.DaysInMonth( dateInNextYear.Year, 12 ) );
						break;

					case RelativeDate.YearToDate:
						this.cachedStart = new DateTime( this.cachedCurrentDate.Year, 1, 1 );
						this.cachedEnd = this.cachedCurrentDate;
						break;

					// AS 8/15/11 TFS84145
					// Added caching here for the ones that were handled directly in the IsMatch method.
					//
					case RelativeDate.Tomorrow:
						this.cachedStart = this.cachedEnd = this.cachedCurrentDate.AddDays(1);
						break;
					case RelativeDate.Today:
						this.cachedStart = this.cachedEnd = this.cachedCurrentDate;
						break;
					case RelativeDate.Yesterday:
						this.cachedStart = this.cachedEnd = this.cachedCurrentDate.AddDays(-1);
						break;
					default:
						Debug.Fail("Unexpected RelativeDate value");
						this.cachedStart = DateTime.MaxValue;
						this.cachedEnd = DateTime.MinValue;
						break;

				}
			}
			#endregion //VerifyCache

			#endregion //Methods

			#region Base Class Overrides

			#region EqualsOverride

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Checks to see if the specified operand is equal to this operand. This method is called
			/// by the Equals method.
			/// </summary>
			/// <param name="operand">Operand to check for equality.</param>
			/// <returns>True if the specified operand is equal to this, false otherwise.</returns>
			protected override bool EqualsOverride( SpecialFilterOperandBase operand )
			{
				RelativeDateOperand x = operand as RelativeDateOperand;

				return null != x
					&& base.EqualsOverride( x )
					&& this.relativeDate == x.relativeDate;
			}

			#endregion // EqualsOverride

			#region GetHashCode

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Overridden. Returns the hash code.
			/// </summary>
			/// <returns>Hash code of the object.</returns>
			/// <remarks>
			/// <para class="body">
			/// <b>Note:</b> If you override <see cref="EqualsOverride"/> then also override this method
			/// and return an appropriate hash code.
			/// </para>
			/// </remarks>
			public override int GetHashCode( )
			{
				return base.GetHashCode( )
					^ this.relativeDate.GetHashCode( );
			}

			#endregion // GetHashCode

			#region IsMatch

			public override bool IsMatch( ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context )
			{
				// If the basic conditions fail, then we shouldn't bother doing any more processing.
				if ( false == base.IsMatch( comparisonOperator, value, context ) )
					return false;

				// We don't want to have to do the calculations of parsing what the next week, current week,
				// etc will be every time that we have to compare a value, so we'll cache what we need to
				// perform these comparions, only updating them if the current date has changed.
				this.VerifyCache( context  );

				// Excel does not perform the filtering comparisons with respect to the time of day (i.e.
				// if a YearToDate filter is done, a value that has today's date but a time an hour in
				// advance will still be considered a match).
				DateTime comparisonDate = ( (DateTime)value ).Date;

				// AS 8/15/11 TFS84145
				// Don't use separate members for the start/end. Note we don't need to be worried about the 
				// last parameter being comparisonDate.Date for the Yesterday/Today/Tmomorrow since we were 
				// already setting comparisonDate to the datetime without the time above.
				//
				//switch ( this.relativeDate )
				//{
				//    case RelativeDate.Yesterday:
				//        DateTime yesterday = this.cachedCurrentDate.AddDays( -1 );
				//        return MatchHelper( comparisonOperator, yesterday, yesterday, comparisonDate.Date );
				//
				//    case RelativeDate.Today:
				//        return MatchHelper( comparisonOperator, this.cachedCurrentDate, this.cachedCurrentDate, comparisonDate.Date );
				//
				//    case RelativeDate.Tomorrow:
				//        DateTime tomorrow = this.cachedCurrentDate.AddDays( 1 );
				//        return MatchHelper( comparisonOperator, tomorrow, tomorrow, comparisonDate.Date );
				//
				//    case RelativeDate.LastWeek:
				//        return MatchHelper( comparisonOperator, this.cachedLastWeekStart, this.cachedLastWeekEnd, comparisonDate );
				//
				//    case RelativeDate.ThisWeek:
				//        return MatchHelper( comparisonOperator, this.cachedThisWeekStart, this.cachedThisWeekEnd, comparisonDate );
				//
				//    case RelativeDate.NextWeek:
				//        return MatchHelper( comparisonOperator, this.cachedNextWeekStart, this.cachedNextWeekEnd, comparisonDate );
				//
				//    case RelativeDate.LastMonth:
				//        return MatchHelper( comparisonOperator, this.cachedLastMonthStart, this.cachedLastMonthEnd, comparisonDate );
				//
				//    case RelativeDate.ThisMonth:
				//        return MatchHelper( comparisonOperator, this.cachedThisMonthStart, this.cachedThisMonthEnd, comparisonDate );
				//
				//    case RelativeDate.NextMonth:
				//        return MatchHelper( comparisonOperator, this.cachedNextMonthStart, this.cachedNextMonthEnd, comparisonDate );
				//
				//    case RelativeDate.LastQuarter:
				//        return MatchHelper( comparisonOperator, this.cachedLastQuarterStart, this.cachedLastQuarterEnd, comparisonDate );
				//
				//    case RelativeDate.ThisQuarter:
				//        return MatchHelper( comparisonOperator, this.cachedThisQuarterStart, this.cachedThisQuarterEnd, comparisonDate );
				//
				//    case RelativeDate.NextQuarter:
				//        return MatchHelper( comparisonOperator, this.cachedNextQuarterStart, this.cachedNextQuarterEnd, comparisonDate );
				//
				//    case RelativeDate.LastYear:
				//        return MatchHelper( comparisonOperator, this.cachedLastYearStart, this.cachedLastYearEnd, comparisonDate );
				//
				//    case RelativeDate.ThisYear:
				//        return MatchHelper( comparisonOperator, this.cachedThisYearStart, this.cachedThisYearEnd, comparisonDate );
				//
				//    case RelativeDate.NextYear:
				//        return MatchHelper( comparisonOperator, this.cachedNextYearStart, this.cachedNextYearEnd, comparisonDate );
				//
				//    case RelativeDate.YearToDate:
				//        return MatchHelper( comparisonOperator, this.cachedYearToDateStart, this.cachedCurrentDate, comparisonDate );
				//}
				//
				//Debug.Fail( "Unexpected RelativeDate value" );
				//return false;
				return MatchHelper( comparisonOperator, this.cachedStart, this.cachedEnd, comparisonDate );
			}

			#endregion // IsMatch

			#endregion //Base Class Overrides
		}

		#endregion // RelativeDateOperand Class

		#region MonthOperand Class






		internal class MonthOperand : DateOperand
		{
			#region Nested Data Structures

			#region FormattedMonthName Class

			// SSP 3/23/10 TFS29800
			// 
			private class FormattedMonthName : IFormattable
			{
				private MonthOperand _operand;

				internal FormattedMonthName( MonthOperand operand )
				{
					_operand = operand;
				}

				public string ToString( string format, IFormatProvider formatProvider )
				{
					try
					{
						DateTimeFormatInfo dateFormatInfo = DateTimeFormatInfo.GetInstance( formatProvider );

						if ( null == dateFormatInfo )
							dateFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;

						return dateFormatInfo.GetMonthName( _operand.month );
					}
					catch
					{
						Debug.Assert( false );

						return _operand.month.ToString( );
					}
				}
			}

			#endregion // FormattedMonthName Class

			#region SerializationInfo Class

			/// <summary>
			/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
			/// </summary>
			internal class SerializationInfo : ObjectSerializationInfo
			{
				private PropertySerializationInfo[] _props;

				public override IEnumerable<PropertySerializationInfo> SerializedProperties
				{
					get
					{
						if ( null == _props )
						{
							_props = new PropertySerializationInfo[]
							{
								new PropertySerializationInfo( typeof( string ), "Name" ),
								new PropertySerializationInfo( typeof( short ), "Month" )
							};
						}

						return _props;
					}
				}

				public override Dictionary<string, object> Serialize( object obj )
				{
					MonthOperand x = (MonthOperand)obj;

					Dictionary<string, object> values = new Dictionary<string, object>( );

					values["Name"] = x.Name;
					values["Month"] = x.month;

					return values;
				}

				public override object Deserialize( Dictionary<string, object> values )
				{
					object name;
					object month;

					if ( values.TryGetValue( "Name", out name )
						&& values.TryGetValue( "Month", out month ) )
					{
						return new MonthOperand( (string)name, (short)month );
					}

					return null;
				}
			}

			#endregion // SerializationInfo Class

			#endregion // Nested Data Structures

			#region Members

			private short month;

			#endregion //Members

			#region Constructor

			public MonthOperand( string name, short month ) : base( name, "Month" )
			{
				if ( month < 1 || month > 12 )
					throw new ArgumentOutOfRangeException( );

				this.month = month;
			}
			#endregion //Constructor

			#region Base Class Overrides

			#region EqualsOverride

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Checks to see if the specified operand is equal to this operand. This method is called
			/// by the Equals method.
			/// </summary>
			/// <param name="operand">Operand to check for equality.</param>
			/// <returns>True if the specified operand is equal to this, false otherwise.</returns>
			protected override bool EqualsOverride( SpecialFilterOperandBase operand )
			{
				MonthOperand x = operand as MonthOperand;

				return null != x
					&& base.EqualsOverride( x )
					&& this.month == x.month;
			}

			#endregion // EqualsOverride

			#region GetHashCode

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Overridden. Returns the hash code.
			/// </summary>
			/// <returns>Hash code of the object.</returns>
			/// <remarks>
			/// <para class="body">
			/// <b>Note:</b> If you override <see cref="EqualsOverride"/> then also override this method
			/// and return an appropriate hash code.
			/// </para>
			/// </remarks>
			public override int GetHashCode( )
			{
				return base.GetHashCode( )
					^ this.month.GetHashCode( );
			}

			#endregion // GetHashCode

			#region IsMatch

			public override bool IsMatch( ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context )
			{
				if ( base.IsMatch( comparisonOperator, value, context ) == false )
					return false;

				// The base implementation will perform null and type checks
				int monthOfValue = ( (DateTime)value ).Month;
				switch ( comparisonOperator )
				{
					case ComparisonOperator.Equals:
						return this.month == monthOfValue;

					case ComparisonOperator.NotEquals:
						return this.month != monthOfValue;

					case ComparisonOperator.LessThan:
						return monthOfValue < this.month;

					case ComparisonOperator.LessThanOrEqualTo:
						return monthOfValue <= this.month;

					case ComparisonOperator.GreaterThan:
						return monthOfValue > this.month;

					case ComparisonOperator.GreaterThanOrEqualTo:
						return monthOfValue >= this.month;
				}

				Debug.Fail( "We shouldn't be trying to match a month with an unsupported FilterComparisonOperator" );
				return false;
			}

			#endregion // IsMatch

            // JJD 1/07/09 - added
            #region ValueArgumentForStrings






            protected override object[] ValueArgumentsForStrings 
            { 
                get 
                {
					// SSP 3/23/10 TFS29800
					// 
					// ----------------------------------------------------------------------------
					return new object[] { new FormattedMonthName( this ) };

					//try
					//{
					//    return new object[] { CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.month) };
					//}
					//catch
					//{
					//    return new object[] { this.month };
					//}
					// ----------------------------------------------------------------------------
                } 
            }

            #endregion //ValueArgumentForStrings

			#endregion //Base Class Overrides
		}

		#endregion // MonthOperand Class

		#region QuarterOperand Class






		internal class QuarterOperand : DateOperand
		{
			#region Nested Data Structures

			#region SerializationInfo Class

			/// <summary>
			/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
			/// </summary>
			internal class SerializationInfo : ObjectSerializationInfo
			{
				private PropertySerializationInfo[] _props;

				public override IEnumerable<PropertySerializationInfo> SerializedProperties
				{
					get
					{
						if ( null == _props )
						{
							_props = new PropertySerializationInfo[]
							{
								new PropertySerializationInfo( typeof( string ), "Name" ),
								new PropertySerializationInfo( typeof( short ), "Quarter" ),
							};
						}

						return _props;
					}
				}

				public override Dictionary<string, object> Serialize( object obj )
				{
					QuarterOperand x = (QuarterOperand)obj;

					Dictionary<string, object> values = new Dictionary<string, object>( );

					values["Name"] = x.Name;
					values["Quarter"] = x.quarter;

					return values;
				}

				public override object Deserialize( Dictionary<string, object> values )
				{
					object name;
					object quarter;

					if ( values.TryGetValue( "Name", out name )
						&& values.TryGetValue( "Quarter", out quarter ) )
					{
						return new QuarterOperand( (string)name, (short)quarter );
					}

					return null;
				}
			}

			#endregion // SerializationInfo Class

			#endregion // Nested Data Structures

			#region Members

			private short quarter;

			#endregion //Members

			#region Constructor

			public QuarterOperand( string name, short quarter ) : base( name, "Quarter" )
			{
				if ( quarter < 1 || quarter > 4 )
					throw new ArgumentOutOfRangeException( );

				this.quarter = quarter;
			}
			#endregion //Constructor

			#region Base Class Overrides

			#region EqualsOverride

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Checks to see if the specified operand is equal to this operand. This method is called
			/// by the Equals method.
			/// </summary>
			/// <param name="operand">Operand to check for equality.</param>
			/// <returns>True if the specified operand is equal to this, false otherwise.</returns>
			protected override bool EqualsOverride( SpecialFilterOperandBase operand )
			{
				QuarterOperand x = operand as QuarterOperand;

				return null != x
					&& base.EqualsOverride( x )
					&& this.quarter == x.quarter;
			}

			#endregion // EqualsOverride

			#region GetHashCode

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Overridden. Returns the hash code.
			/// </summary>
			/// <returns>Hash code of the object.</returns>
			/// <remarks>
			/// <para class="body">
			/// <b>Note:</b> If you override <see cref="EqualsOverride"/> then also override this method
			/// and return an appropriate hash code.
			/// </para>
			/// </remarks>
			public override int GetHashCode( )
			{
				return base.GetHashCode( )
					^ this.quarter.GetHashCode( );
			}

			#endregion // GetHashCode

			#region IsMatch

			public override bool IsMatch( ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context )
			{
				if ( base.IsMatch( comparisonOperator, value, context ) == false )
					return false;

				// The base implementation will perform null and type checks
				int quarterOfValue = (int)Math.Ceiling( ( (DateTime)value ).Month / 3.0 );

				switch ( comparisonOperator )
				{
					case ComparisonOperator.Equals:
						return quarterOfValue == this.quarter;

					case ComparisonOperator.NotEquals:
						return quarterOfValue != this.quarter;

					case ComparisonOperator.LessThan:
						return quarterOfValue < this.quarter;

					case ComparisonOperator.LessThanOrEqualTo:
						return quarterOfValue <= this.quarter;

					case ComparisonOperator.GreaterThan:
						return quarterOfValue > this.quarter;

					case ComparisonOperator.GreaterThanOrEqualTo:
						return quarterOfValue >= this.quarter;
				}

				Debug.Fail( "We shouldn't be trying to match a quarter with an unsupported FilterComparisonOperator" );
				return false;
			}

			#endregion // IsMatch

            // JJD 1/07/09 - added
            #region ValueArgumentForStrings






            protected override object[] ValueArgumentsForStrings 
            { 
                get 
                {
                    return new object[] { this.quarter };
                } 
            }

            #endregion //ValueArgumentForStrings

			#endregion //Base Class Override
		}

		#endregion // QuarterOperand Class

		// AS - NA 11.2 Excel Style Filtering
		#region DateRangeOperand Class






		internal class DateRangeOperand : DateOperand
		{
			#region Nested Data Structures

			#region SerializationInfo Class

			/// <summary>
			/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
			/// </summary>
			internal class SerializationInfo : ObjectSerializationInfo
			{
				private PropertySerializationInfo[] _props;

				public override IEnumerable<PropertySerializationInfo> SerializedProperties
				{
					get
					{
						if (null == _props)
						{
							_props = new PropertySerializationInfo[]
							{
								new PropertySerializationInfo( typeof( DateRangeScope ), "Scope" ),
								new PropertySerializationInfo( typeof( DateTime ), "RelativeDate" ),
							};
						}

						return _props;
					}
				}

				public override Dictionary<string, object> Serialize(object obj)
				{
					DateRangeOperand x = (DateRangeOperand)obj;

					Dictionary<string, object> values = new Dictionary<string, object>();

					values["Scope"] = x._scope;
					values["RelativeDate"] = x._relativeDate;

					return values;
				}

				public override object Deserialize(Dictionary<string, object> values)
				{
					object scope;
					object relativeDate;

					if (values.TryGetValue("Scope", out scope)
						&& values.TryGetValue("RelativeDate", out relativeDate))
					{
						return new DateRangeOperand((DateTime)relativeDate, (DateRangeScope)scope);
					}

					return null;
				}
			}

			#endregion // SerializationInfo Class

			#endregion // Nested Data Structures

			#region Members

			private DateTime _relativeDate;
			private DateRangeScope _scope;
			private DateRange _cachedDateRange;

			#endregion //Members

			#region Constructor

			public DateRangeOperand(DateTime relativeDate, DateRangeScope scope)
				: base(GetName(relativeDate, scope), GetResourceName(scope))
			{
				_relativeDate = relativeDate;
				_scope = scope;
				_cachedDateRange = CalculateRange(relativeDate, scope);
			}
			#endregion //Constructor

			#region Properties

			#region Calendar
			private static System.Globalization.Calendar Calendar
			{
				get { return CultureInfo.InvariantCulture.Calendar; }
			}
			#endregion //Calendar

			#endregion //Properties

			#region Methods

			#region CalculateRange
			private static DateRange CalculateRange(DateTime relativeDate, DateRangeScope scope)
			{
				int year = relativeDate.Year;
				int month = relativeDate.Month;
				int day = relativeDate.Day;
				int hour = relativeDate.Hour;
				int minute = relativeDate.Minute;
				int second = relativeDate.Second;

				switch (scope)
				{
					case DateRangeScope.Year:
						month = day = 1;
						hour = minute = second = 0;
						break;
					case DateRangeScope.Month:
						day = 1;
						hour = minute = second = 0;
						break;
					case DateRangeScope.Day:
						hour = minute = second = 0;
						break;
					case DateRangeScope.Hour:
						minute = second = 0;
						break;
					case DateRangeScope.Minute:
						second = 0;
						break;
				}

				DateTime start = GetDate(year, month, day, hour, minute, second, true);
				DateTime end = GetEndDate(scope, year, month, day, hour, minute, second);

				return new DateRange(start, end);
			}
			#endregion //CalculateRange

			#region GetDate
			private static DateTime GetDate(int year, int month, int day, int hour, int minute, int second, bool isStart)
			{
				System.Globalization.Calendar calendar = Calendar;

				try
				{
					return calendar.ToDateTime(year, month, day, hour, minute, second, 0);
				}
				catch (ArgumentOutOfRangeException)
				{
					return isStart ? calendar.MinSupportedDateTime : calendar.MaxSupportedDateTime;
				}
			}
			#endregion //GetDate

			#region GetEndDate
			private static DateTime GetEndDate(DateRangeScope scope, int year, int month, int day, int hour, int minute, int second)
			{
				switch (scope)
				{
					case DateRangeScope.Year:
						year++;
						break;
					case DateRangeScope.Month:
						if (month == 12)
							return GetEndDate(DateRangeScope.Year, year, 1, day, hour, minute, second);
						month++;
						break;
					case DateRangeScope.Day:
						if (day == Calendar.GetDaysInMonth(year, month))
							return GetEndDate(DateRangeScope.Month, year, month, 1, hour, minute, second);
						day++;
						break;
					case DateRangeScope.Hour:
						if (hour == 23)
							return GetEndDate(DateRangeScope.Day, year, month, day, 0, minute, second);
						hour++;
						break;
					case DateRangeScope.Minute:
						if (minute == 59)
							return GetEndDate(DateRangeScope.Hour, year, month, day, hour, 0, second);
						minute++;
						break;
					case DateRangeScope.Second:
						if (second == 59)
							return GetEndDate(DateRangeScope.Minute, year, month, day, hour, minute, 0);
						second++;
						break;
				}

				return GetDate(year, month, day, hour, minute, second, false);
			}
			#endregion //GetEndDate

			#region GetName
			private static string GetName(DateTime relativeDate, DateRangeScope scope)
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				string format;

				switch (scope)
				{
					case DateRangeScope.Year:
						format = "DateRange {0:yyyy}";
						break;
					case DateRangeScope.Month:
						format = "DateRange {0:yyyy}-{0:MM}";
						break;
					case DateRangeScope.Day:
						format = "DateRange {0:yyyy}-{0:MM}-{0:dd}";
						break;
					case DateRangeScope.Hour:
						format = "DateRange {0:yyyy}-{0:MM}-{0:dd} {0:HH}";
						break;
					case DateRangeScope.Minute:
						format = "DateRange {0:yyyy}-{0:MM}-{0:dd} {0:HH}:{0:mm}";
						break;
					case DateRangeScope.Second:
						format = "DateRange {0:yyyy}-{0:MM}-{0:dd} {0:HH}:{0:mm}:{0:ss}";
						break;
					default:
						Debug.Fail("Unrecognized scope:" + scope.ToString());
						return "DateRange";
				}

				return string.Format(cultureInfo, format, relativeDate);
			}
			#endregion //GetName

			#region GetResourceName
			private static string GetResourceName(DateRangeScope scope)
			{
				switch (scope)
				{
					case DateRangeScope.Year:
						return "DateRange_Year";
					case DateRangeScope.Month:
						return "DateRange_Month";
					case DateRangeScope.Day:
						return "DateRange_Day";
					case DateRangeScope.Hour:
						return "DateRange_Hour";
					case DateRangeScope.Minute:
						return "DateRange_Minute";
					case DateRangeScope.Second:
						return "DateRange_Second";
					default:
						Debug.Fail("Unrecognized scope:" + scope.ToString());
						return "DateRange";
				}
			}
			#endregion //GetResourceName

			#endregion //Methods

			#region Base Class Overrides

			#region EqualsOverride

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Checks to see if the specified operand is equal to this operand. This method is called
			/// by the Equals method.
			/// </summary>
			/// <param name="operand">Operand to check for equality.</param>
			/// <returns>True if the specified operand is equal to this, false otherwise.</returns>
			protected override bool EqualsOverride(SpecialFilterOperandBase operand)
			{
				DateRangeOperand x = operand as DateRangeOperand;

				return null != x
					&& _scope == x._scope
					&& base.EqualsOverride(x);
			}

			#endregion // EqualsOverride

			#region GetHashCode

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Overridden. Returns the hash code.
			/// </summary>
			/// <returns>Hash code of the object.</returns>
			/// <remarks>
			/// <para class="body">
			/// <b>Note:</b> If you override <see cref="EqualsOverride"/> then also override this method
			/// and return an appropriate hash code.
			/// </para>
			/// </remarks>
			public override int GetHashCode()
			{
				return base.GetHashCode()
					^ this._scope.GetHashCode();
			}

			#endregion // GetHashCode

			#region IsMatch

			public override bool IsMatch(ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context)
			{
				if (base.IsMatch(comparisonOperator, value, context) == false)
					return false;

				DateTime date = (DateTime)value;

				// note: remember that the end of our range is exclusive...
				switch (comparisonOperator)
				{
					case ComparisonOperator.Equals:
						return _cachedDateRange.ContainsExclusive(date);

					case ComparisonOperator.NotEquals:
						return !_cachedDateRange.ContainsExclusive(date);

					case ComparisonOperator.LessThan:
						return date < _cachedDateRange.Start;

					case ComparisonOperator.LessThanOrEqualTo:
						return date < _cachedDateRange.End;

					case ComparisonOperator.GreaterThan:
						return date >= _cachedDateRange.End;

					case ComparisonOperator.GreaterThanOrEqualTo:
						return date >= _cachedDateRange.Start;
				}

				Debug.Fail("We shouldn't be trying to match a range with an unsupported FilterComparisonOperator");
				return false;
			}

			#endregion // IsMatch

			// JJD 1/07/09 - added
			#region ValueArgumentForStrings






			protected override object[] ValueArgumentsForStrings
			{
				get
				{
					return new object[] { this._relativeDate };
				}
			}

			#endregion //ValueArgumentForStrings

			#endregion //Base Class Override
		}

		#endregion // QuarterOperand Class

		#endregion // Date Operands

		
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