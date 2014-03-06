using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Internal;
using System.Collections;
using System.Text.RegularExpressions;
using Infragistics.Shared;

namespace Infragistics.Windows.Controls
{
	#region ComparisonCondition Class

	/// <summary>
	/// Condition that compares two values using the operator specified by the 
	/// <see cref="ComparisonCondition.Operator"/> property.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ComparisonCondition</b> is an ICondition implementation that compares two
	/// values using the operator specified by the <see cref="ComparisonCondition.Operator"/> 
	/// property. The value on the right hand side of the operator is specified by the
	/// <see cref="Value"/> property. The value being matched is the left hand side value, which
	/// is specified via the call to <see cref="IsMatch"/> method.
	/// </para>
	/// <para class="body">
	/// Multiple conditions can be grouped using the <see cref="ConditionGroup"/> class, which itself
	/// implements <see cref="ICondition"/> interface, allowing you to create an arbitrarily nested
	/// conditions. <b>ConditionGroup</b>'s <see cref="ConditionGroup.LogicalOperator"/> property is 
	/// used to specify the boolean operator that will be used for combining multiple conditions in 
	/// that particular condition group.
	/// </para>
	/// </remarks>
	/// <seealso cref="ICondition"/>
	/// <seealso cref="ConditionGroup"/>
	/// <seealso cref="ComplementCondition"/>
	public class ComparisonCondition : ICondition
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
							new PropertySerializationInfo( typeof( ComparisonOperator ), "Operator" ),
							new PropertySerializationInfo( typeof( object ), "Value" )
						};
					}

					return _props;
				}
			}

			public override Dictionary<string, object> Serialize( object obj )
			{
				ComparisonCondition cc = (ComparisonCondition)obj;
				Dictionary<string, object> values = new Dictionary<string, object>( );

				values["Operator"] = cc.Operator;

				object compareValue = cc.Value;

				// If compare value is a custom special filter operand that we don't know how to
				// serialize, then serialize out a wrapper that serializes out the name of the operand.
				// 
				if ( compareValue is SpecialFilterOperandBase )
					compareValue = SpecialFilterOperands.GetSerializationValue( (SpecialFilterOperandBase)compareValue );

				values["Value"] = compareValue;

				// JJD 02/17/12 - TFS101703 
				// Serialize new DisplayText property
				if (!string.IsNullOrEmpty(cc._displayText))
					values["DisplayText"] = cc._displayText;

				return values;
			}

			public override object Deserialize( Dictionary<string, object> values )
			{
				object op, val;

				if (values.TryGetValue("Operator", out op)
					&& values.TryGetValue("Value", out val))
				{

					// JJD 02/17/12 - TFS101703
					// De-serialize new DisplayText property
					object displayText;
					values.TryGetValue("DisplayText", out displayText);

					//return new ComparisonCondition((ComparisonOperator)op, val);
					return new ComparisonCondition((ComparisonOperator)op, val, displayText as string);
				}

				return null;
			}
		}

		#endregion // SerializationInfo Class

		#region WildcardToRegexConverter Class

		// SSP 1/29/09 - NAS9.1 Record Filtering
		// Added WildcardToRegexConverter class.
		// 
		internal class WildcardToRegexConverter
		{
			#region ParseState Enum

			private enum ParseState
			{
				Initial,
				Bracket
			}

			#endregion // ParseState Enum

			#region Member Vars

			private const string REGEX_SPECIAL_CHARS = @"[]{}()\.+*?^$";
			private string _wildcard;
			private StringBuilder _regex;
			private string _error;

			#endregion // Member Vars

			#region Constructor

			private WildcardToRegexConverter( string wildcard )
			{
				_wildcard = wildcard;
				_regex = new StringBuilder( wildcard.Length );
			}

			#endregion // Constructor

			#region Convert

			public static Regex Convert( string wildcard, bool ignoreCase )
			{
				string regex;
				RegexOptions options;
				Exception error;

				Convert( wildcard, out regex, out options, out error );

				if ( null == error )
				{
					if ( ignoreCase )
						options |= RegexOptions.IgnoreCase;

					return new Regex( regex, options );
				}

				return null;
			}

			public static void Convert( string wildcard, out string regex, out RegexOptions options, out Exception error )
			{
                // JJD 1/31/09
                // If wildcard is null or an empty string then return null
                if (wildcard == null ||
                     wildcard.Length == 0)
                {
                    options = RegexOptions.None;
                    regex = null;
                    error = new Exception( SR.GetString( "Filter_WildCardEmpty" ) );
                    return;
                }

                WildcardToRegexConverter w = new WildcardToRegexConverter(wildcard);
				w.Parse( );

				if ( null == w._error && w._regex.Length > 0 )
				{
					regex = "^" + w._regex.ToString( ) + "$";
					options = RegexOptions.Singleline | RegexOptions.ExplicitCapture;
					error = null;
					return;
				}

				error = new Exception( w._error );
				options = RegexOptions.None;
				regex = null;
			}

			#endregion // Convert

			#region Parse

			private void Parse( )
			{
				const char BRACKET_OPEN = '[';
				const char BRACKET_CLOSE = ']';

				ParseState state = ParseState.Initial;
				int bracketOpen = -1;
				StringBuilder regex = _regex;

				string str = _wildcard;
				for ( int i = 0; i < str.Length; i++ )
				{
					char c = str[i];

					if ( ParseState.Bracket == state && BRACKET_CLOSE == c )
					{
						this.ParseSet( 1 + bracketOpen, i - 1 );
						state = ParseState.Initial;
					}
					else if ( ParseState.Initial == state && BRACKET_OPEN == c )
					{
						bracketOpen = i;
						state = ParseState.Bracket;
					}
					else if ( ParseState.Initial == state && '#' == c )
					{
						regex.Append( @"\d" );
					}
					else if ( ParseState.Initial == state && '?' == c )
					{
						regex.Append( '.' );
					}
					else if ( ParseState.Initial == state && '*' == c )
					{
						regex.Append( ".*" );
					}
					else if ( ParseState.Initial == state )
					{
						if ( REGEX_SPECIAL_CHARS.IndexOf( c ) >= 0 )
							regex.Append( '\\' );

						regex.Append( c );
					}
				}
			}

			#endregion // Parse

			#region ParseSet

			private void ParseSet( int si, int ei )
			{
				StringBuilder regex = _regex;
				string str = _wildcard;

				int regexStartIndex = regex.Length;

				regex.Append( '[' );

				// '!' in the begining matches anything but the characters in the set.
				// 
				if ( '!' == str[si] )
				{
					// If there's only !, then it's an invalid wildcard.
					// 
					if ( si == ei )
					{
						_error = SR.GetString( "Filter_WildCard_InvalidCharClass" );
						return;
					}

					regex.Append( '^' );
					si++;
				}

				for ( int i = si; i <= ei; i++ )
				{
					char c = str[i];
					if ( REGEX_SPECIAL_CHARS.IndexOf( c ) >= 0 )
						regex.Append( '\\' );

					regex.Append( c );
				}

				regex.Append( ']' );
			}

			#endregion // ParseSet
		}

		#endregion // WildcardToRegexConverter Class

		#endregion // Nested Data Structures

		#region Member Vars

		private ComparisonOperator _comparisonOperator;
		private object _value;

		// JJD 02/17/12 - TFS101703 - added
		private string _displayText;

		private Regex _cachedRegex;
		private string _cachedRegex_Pattern;		
		private bool _cachedRegex_IgnoreCase;

		private SpecialFilterOperandBase _topValuesOperand;
		private ComparisonOperator _topValuesOperand_Operator;
		private double _topValuesOperand_Number;

		private Type _lastConversionType;
		private object _lastConversionSourceValue;
		private object _lastConvertedValue;

		// JJD 5/26/09 - TFS17564
        // Added separate cached value whn converting to string
        private object _lastConversionToStringSourceValue;
		private object _lastConvertedToStringValue;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ComparisonCondition"/>.
		/// </summary>
		public ComparisonCondition( )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ComparisonCondition"/>.
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator.</param>
		/// <param name="value">Compare value. This is the right hand side value of the comparison operator.</param>
		public ComparisonCondition( ComparisonOperator comparisonOperator, object value ) : this(comparisonOperator, value, null)
		{
		}

		// JJD 02/17/12 - TFS101703 - added overload that takes the new DisplayText parameter
		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ComparisonCondition"/>.
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator.</param>
		/// <param name="value">Compare value. This is the right hand side value of the comparison operator.</param>
		/// <param name="displayText">The text to return from the ToString() method.</param>
		/// <remarks>
		/// <para class="note">
		/// <b>Note:</b> if DisplayText is supplied then the user will see the 'Equals' operator in a filter cell when 
		/// this ComparisonCondition is selected. Also, if the operator is subsequently changed then the condition will be cleared. The reason 
		/// for this behavior is that the display text may include context that is related to its own operator. Therefore, any additional operator
		/// displayed to the user (other than 'Equals') may be incorrect. For eaxmple, if the ComparisonCondition Operator was 'NotEquals', the Value 
		/// was '0' and the DisplayText was 'None Zero'.
		/// </para>
		/// </remarks>
		public ComparisonCondition( ComparisonOperator comparisonOperator, object value, string displayText )
		{
			// Validate if a valid regex/wildcard is specified depending on the operator.
			// 
			ValidateValueHelper( comparisonOperator, value );

			this.Operator = comparisonOperator;
			_value = value;

			// JJD 02/17/12 - TFS101703 - added displayText parameter
			_displayText = displayText;
		}

		#endregion // Constructor

        #region Base class overrides

		#region Equals
		/// <summary>
		/// Returns true if the passed in object is equal
		/// </summary>
		public override bool Equals(object obj)
		{
			ComparisonCondition other = obj as ComparisonCondition;

			return null != other &&
				other._comparisonOperator == _comparisonOperator &&
				object.Equals(other._value, _value);
		} 
		#endregion //Equals

		#region GetHashCode
		/// <summary>
		/// Caclulates a value used for hashing
		/// </summary>
		public override int GetHashCode()
		{
			int hash = _comparisonOperator.GetHashCode();

			if (null != _value)
				hash |= _value.GetHashCode();

			return hash;
		} 
		#endregion //GetHashCode

        #region ToString

        /// <summary>
        /// Returns a string representation of the condition.
        /// </summary>
        public override string ToString()
        {
			// SSP 1/5/10 TFS25670
			// Changed the ToString implementation of the SpecialFilterOperandBase to return its DisplayContent
			// instead of description. If the the operand is an SpecialFilterOperandBase, use its 
			// Description here because this ToString is used for the tool-tip purposes.
			// 
			// --------------------------------------------------------------------------------------------------
            //return SR.GetString("ComparisonCondition_" + this.Operator.ToString(), new object[] { this.Value });

			// JJD 02/17/12 - TFS101703 
			// If the display text is specified then return it
			if (!string.IsNullOrEmpty(_displayText))
				return _displayText;

			// AS 5/17/11 NA 11.2 Excel Style Filtering
			// Discussed this with sandip. The tostring of the condition shouldn't be returning the description 
			// as that was meant to be used for something like a tooltip and not a textual representation of the 
			// expression. Right now the condition expression ends up having something like "A blanks value" in 
			// it instead of "= (Blanks)".
			//
			//object value = this.Value;
			//if ( value is SpecialFilterOperandBase )
			//{
			//    string tmp = ( (SpecialFilterOperandBase)value ).Description as string;
			//    if ( !string.IsNullOrEmpty( tmp ) )
			//        value = tmp;
			//}
			object value = this.Value;

			return SR.GetString( "ComparisonCondition_" + this.Operator.ToString( ), new object[] { value } );
			// --------------------------------------------------------------------------------------------------
        }

        #endregion //ToString

        #endregion //Base class overrides	

		#region Properties

		#region Public Properties

		// JJD 02/17/12 - TFS101703 - added
		#region DisplayText

		/// <summary>
		/// Gets or sets the text to use in the UI for the condition.
		/// </summary>
		/// <value>if specified will be returned from the <see cref="ToString()"/> method. </value>
		/// <remarks>
		/// <para class="note">
		/// <b>Note:</b> if DisplayText is supplied then the user will see the 'Equals' operator in a filter cell when 
		/// this ComparisonCondition is selected. Also, if the operator is subsequently changed then the condition will be cleared. The reason 
		/// for this behavior is that the display text may include context that is related to its own operator. Therefore, any additional operator
		/// displayed to the user (other than 'Equals') may be incorrect. For eaxmple, if the ComparisonCondition Operator was 'NotEquals', the Value 
		/// was '0' and the DisplayText was 'None Zero'.
		/// </para>
		/// </remarks>
		public string DisplayText
		{
			get
			{
				return _displayText;
			}
			set
			{
				_displayText = value;
			}
		}

		#endregion // DisplayText

		#region Operator

		/// <summary>
		/// Gets or sets the comparison operator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Operator</b> property specifies the operator that will be used when evaluating the condition
		/// to match a value. The <see cref="Value"/> property specifies the value on the right hand side of 
		/// the operator. The left hand side value is the value that's being tested to see if it matches
		/// the condition (passed into the <see cref="IsMatch( object, ConditionEvaluationContext )"/> method).
		/// </para>
		/// </remarks>
		/// <see cref="Value"/>
		public ComparisonOperator Operator
		{
			get
			{
				return _comparisonOperator;
			}
			set
			{
				if ( _comparisonOperator != value )
				{
					Utilities.ThrowIfInvalidEnum( value, "Operator" );

					_comparisonOperator = value;

					this.ClearCache( );
				}
			}
		}

		#endregion // Operator

		#region Value

		/// <summary>
		/// Returns the comparison value. This is the value on the right hand side of the comparison operator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Value</b> property specifies the the value on the right hand side of the operator. 
		/// The <see cref="Operator"/> property specifies the operator that will be used when evaluating 
		/// the condition to match a value. The left hand side value is the value that's being tested to see 
		/// if it matches the condition (passed into the <see cref="IsMatch( object, ConditionEvaluationContext )"/> method).
		/// </para>
		/// <para class="body">
		/// <i>Value</i> can also be set to an instance of <see cref="SpecialFilterOperandBase"/> derived class. 
		/// There are several built-in special operands exposed by the <see cref="SpecialFilterOperands"/> class as
		/// static properties. See <see cref="SpecialFilterOperands"/> for more information.
		/// </para>
		/// </remarks>
		/// <seealso cref="Operator"/>
		/// <seealso cref="SpecialFilterOperandBase"/>
		/// <seealso cref="SpecialFilterOperands"/>
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				if ( _value != value )
				{
					// Validate if a valid regex/wildcard is specified depending on the operator.
					// 
					ValidateValueHelper( _comparisonOperator, value );

					_value = value;
				}
			}
		}

		#endregion // Value

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Create

		/// <summary>
		/// Creates a new ComparisonCondition instance based on the specified parameters.
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator</param>
		/// <param name="operand">Comparison operand</param>
		/// <param name="validateOperand">Whether to validate if the operand is valid for the specified comparison 
		/// operator. If true and the operand is invalid, then null will be returned and error will be set to the error message. 
		/// If false, a new ComparisonCondition will be returned with the specified operator and operand, even if the operand
		/// is invalid.</param>
        /// <param name="error">If the value is invalid, this will be set to an error instance that provides information about why the condition could not be created.</param>
        /// <returns>New ComparisonCondition instance.</returns>
		public static ComparisonCondition Create(
			ComparisonOperator comparisonOperator, object operand, bool validateOperand, out Exception error)
		{
			return Create(comparisonOperator, operand, null, validateOperand, out error);
		}

		// JJD 02/17/12 - TFS101703 - added overload that takes the new DisplayText parameter
		/// <summary>
		/// Creates a new ComparisonCondition instance based on the specified parameters.
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator</param>
		/// <param name="operand">Comparison operand</param>
		/// <param name="displayText">The text to return from the ToString() method.</param>
		/// <param name="validateOperand">Whether to validate if the operand is valid for the specified comparison 
		/// operator. If true and the operand is invalid, then null will be returned and error will be set to the error message. 
		/// If false, a new ComparisonCondition will be returned with the specified operator and operand, even if the operand
		/// is invalid.</param>
        /// <param name="error">If the value is invalid, this will be set to an error instance that provides information about why the condition could not be created.</param>
        /// <returns>New ComparisonCondition instance.</returns>
		/// <remarks>
		/// <para class="note">
		/// <b>Note:</b> if DisplayText is supplied then the user will see the 'Equals' operator in a filter cell when 
		/// this ComparisonCondition is selected. Also, if the operator is subsequently changed then the condition will be cleared. The reason 
		/// for this behavior is that the display text may include context that is related to its own operator. Therefore, any additional operator
		/// displayed to the user (other than 'Equals') may be incorrect. For eaxmple, if the ComparisonCondition Operator was 'NotEquals', the Value 
		/// was '0' and the DisplayText was 'None Zero'.
		/// </para>
		/// </remarks>
		public static ComparisonCondition Create( 
			ComparisonOperator comparisonOperator, object operand, string displayText, bool validateOperand, out Exception error )
		{
			error = null;
			if ( validateOperand && !ValidateValueHelper( comparisonOperator, operand, null, null, out error ) )
				return null;

			ComparisonCondition cc = new ComparisonCondition( );
			cc._comparisonOperator = comparisonOperator;
			cc._value = operand;

			// JJD 02/17/12 - TFS101703 - initilaize the new DisplayText property
			cc._displayText = displayText;

			return cc;
		}

		#endregion // Create

		#region ICloneable.Clone

		object ICloneable.Clone( )
		{
			// JJD 02/17/12 - TFS101703 - added DisplayText property
			//return new ComparisonCondition( _comparisonOperator, _value );
			return new ComparisonCondition(_comparisonOperator, _value, _displayText);
		}

		#endregion // ICloneable.Clone

		#region GetComparisonOperatorFlag

		/// <summary>
		/// Returns the ComparisonOperatorFlags value associated with the specified comparison operator.
		/// </summary>
		/// <param name="comparisonOperator">ComparsionOperatorFlags value associated with this comparison operator will be returned.</param>
		/// <returns>ComparsionOperatorFlags value associated the specified comparison operator.</returns>
		public static ComparisonOperatorFlags GetComparisonOperatorFlag( ComparisonOperator comparisonOperator )
		{
			return (ComparisonOperatorFlags)( 1 << (int)comparisonOperator );
		}

		#endregion // GetComparisonOperatorFlag

		#region GetComparisonOperators

		// AS 11/29/10 TFS60418
		[ThreadStatic]
		private static Dictionary<ComparisonOperatorFlags, ComparisonOperator[]> _operatorTable;

		/// <summary>
		/// Returns the list of comparison operators that are included in the specified flags.
		/// </summary>
		/// <param name="flags">Comparison operator flags.</param>
		/// <returns>Array of comparison operators that have the associated flag set in the specified flags.</returns>
		public static ComparisonOperator[] GetComparisonOperators( ComparisonOperatorFlags flags )
		{
			// AS 11/29/10 TFS60418
			if (_operatorTable == null)
				_operatorTable = new Dictionary<ComparisonOperatorFlags, ComparisonOperator[]>();

			ComparisonOperator[] operators;

			if (!_operatorTable.TryGetValue(flags, out operators))
			{
				_operatorTable[flags] = operators = GetComparisonOperatorsImpl(flags);
			}

			return (ComparisonOperator[])operators.Clone();
		}

		// AS 11/29/10 TFS60418
		// Moved to a helper method so we can cache the results.
		//
		private static ComparisonOperator[] GetComparisonOperatorsImpl(ComparisonOperatorFlags flags)
		{
			List<ComparisonOperator> list = new List<ComparisonOperator>( );

			for ( int i = 0; i < 32; i++ )
			{
				int v = 1 << i;

				if ( 0 != ( v & (int)flags ) )
				{
					// JJD 12/20/08
					// Use 'i' which represents the operator not 'v' which is the flag value
					//if (Enum.IsDefined(typeof(ComparisonOperator), v))
					//    list.Add((ComparisonOperator)v);
					if ( Enum.IsDefined( typeof( ComparisonOperator ), i ) )
						list.Add( (ComparisonOperator)i );
				}
			}
			return list.ToArray( );
		}

		#endregion // GetComparisonOperators

		#region GetCompatibleComparisonOperators

		/// <summary>
		/// Gets comparison operators that are compatible with values of specified data type.
		/// </summary>
		/// <param name="dataType">Operators compatible with values of this data type are returned.</param>
		/// <param name="defaultUIOperator">This will be set to an operator that is most suitable as the 
		/// default operator in an operator selection UI part of filtering UI.</param>
		/// <returns>Operators compatible with values of specified data type.</returns>
		public static ComparisonOperatorFlags GetCompatibleComparisonOperators( Type dataType, out ComparisonOperator defaultUIOperator )
		{
			ComparisonOperator[] prioritizedDefaultUIOperators;
			return GetCompatibleComparisonOperators( dataType, out defaultUIOperator, out prioritizedDefaultUIOperators );
		}

		/// <summary>
		/// Gets comparison operators that are compatible with values of specified data type.
		/// </summary>
		/// <param name="dataType">Operators compatible with values of this data type are returned.</param>
		/// <param name="defaultUIOperator">This will be set to an operator that is most suitable as the 
		/// default operator in an operator selection UI part of filtering UI.</param>
		/// <param name="prioritizedDefaultUIOperators">This will be set to a list of default operators that are
		/// compatible with the specified data type in priority order where more preferred is before less preferred.
		/// This is used when filter drop-down items is explicitly specified and doesn't include the default operator.
		/// For example, if FilterDropDownItems in data presenter is set to a value that doesn't include 'StartsWith',
		/// which is the defaultUIOperator for string fields, then we have to select one of the operators from the
		/// explicitly specified FilterDropDownItems list, selecting what makes the most sense. That's where the 
		/// priority comes into play.
		/// </param>
		/// <returns>Operators compatible with values of specified data type.</returns>
		private static ComparisonOperatorFlags GetCompatibleComparisonOperators( Type dataType, out ComparisonOperator defaultUIOperator, out ComparisonOperator[] prioritizedDefaultUIOperators )
		{
			Type typeUnderlying = Utilities.GetUnderlyingType( dataType );
			bool isNumeric = Utilities.IsNumericType( typeUnderlying );

			ComparisonOperatorFlags compatibleOperators = ComparisonOperatorFlags.None;

			if ( isNumeric || typeof( DateTime ) == typeUnderlying )
			{
				defaultUIOperator = ComparisonOperator.Equals;

				compatibleOperators |=
					ComparisonOperatorFlags.Equals
					| ComparisonOperatorFlags.NotEquals
					| ComparisonOperatorFlags.GreaterThan
					| ComparisonOperatorFlags.GreaterThanOrEqualsTo
					| ComparisonOperatorFlags.LessThan
					| ComparisonOperatorFlags.LessThanOrEqualsTo
					| ComparisonOperatorFlags.Top
					| ComparisonOperatorFlags.Bottom;

				if ( isNumeric )
				{
					compatibleOperators |=
						ComparisonOperatorFlags.TopPercentile
						| ComparisonOperatorFlags.BottomPercentile;

					// List of preferred initial operator for numeric fields.
					// 
					prioritizedDefaultUIOperators = new ComparisonOperator[]
					{
						ComparisonOperator.Equals,
						ComparisonOperator.Top,
						ComparisonOperator.TopPercentile,
						ComparisonOperator.Bottom,
						ComparisonOperator.BottomPercentile,
						ComparisonOperator.StartsWith,
						ComparisonOperator.Contains,
						ComparisonOperator.Like,
						ComparisonOperator.Match
					};
				}
				else
				{
					// List of preferred initial operator for date fields.
					// 
					prioritizedDefaultUIOperators = new ComparisonOperator[]
					{
						ComparisonOperator.Equals,
						ComparisonOperator.StartsWith,
						ComparisonOperator.EndsWith,
						ComparisonOperator.Contains,
						ComparisonOperator.Like,
						ComparisonOperator.Match,
						ComparisonOperator.Top,
						ComparisonOperator.TopPercentile,
						ComparisonOperator.Bottom,
						ComparisonOperator.BottomPercentile
					};
				}
			}
			else if ( typeof( bool ) == typeUnderlying )
			{
				defaultUIOperator = ComparisonOperator.Equals;
				compatibleOperators = ComparisonOperatorFlags.Equals
					| ComparisonOperatorFlags.NotEquals;

				// List of preferred initial operator for date fields.
				// 
				prioritizedDefaultUIOperators = new ComparisonOperator[]
					{
						ComparisonOperator.Equals
					};
			}
				// If the data type is not numeric, date time or bool, then treat the values as strings
				// and return operators compatible with string values.
				// 
			else
			{
				defaultUIOperator = ComparisonOperator.StartsWith;

				// All operators are applicable to string values except Top and Bottom which only make
				// sense for numeric values.
				// 
				compatibleOperators = ComparisonOperatorFlags.All
					^ ComparisonOperatorFlags.Top
					^ ComparisonOperatorFlags.TopPercentile
					^ ComparisonOperatorFlags.Bottom
					^ ComparisonOperatorFlags.BottomPercentile;

				// List of preferred initial operator for numeric fields.
				// 
				prioritizedDefaultUIOperators = new ComparisonOperator[]
					{
						ComparisonOperator.StartsWith,
						ComparisonOperator.Contains,
						ComparisonOperator.Like,
						ComparisonOperator.Match,
						ComparisonOperator.Equals
					};
			}

			return compatibleOperators;
		}

		#endregion // GetCompatibleComparisonOperators

		#region GetCompatibleComparisonOperator

		/// <summary>
		/// Gets preferred comparison operator out of availableOperators for the specified data type. If none of the
		/// preferred operators are in the availableOperators flags, then it will return the operator associated with 
		/// the first set bit in the availableOperators. If availableOperators parameter is empty (0), then it will
		/// return the default operator deemed the most appropriate for the specified data type.
		/// </summary>
		/// <param name="dataType">Data type</param>
		/// <param name="availableOperators">Available operators</param>
		/// <returns>Preferred comparison operator for the specified data type</returns>
		public static ComparisonOperator GetCompatibleComparisonOperator( Type dataType, ComparisonOperatorFlags availableOperators )
		{
			ComparisonOperator[] prioritizedDefaultUIOperators;
			ComparisonOperator defaultUIOperator;
			ComparisonOperatorFlags compatibleOperatorsFlags = GetCompatibleComparisonOperators( dataType, out defaultUIOperator, out prioritizedDefaultUIOperators );

			if ( 0 != ( GetComparisonOperatorFlag( defaultUIOperator ) & availableOperators ) )
				return defaultUIOperator;

			Debug.Assert( null != prioritizedDefaultUIOperators );
			if ( null != prioritizedDefaultUIOperators )
			{
				for ( int i = 0; i < prioritizedDefaultUIOperators.Length; i++ )
				{
					ComparisonOperator ii = prioritizedDefaultUIOperators[i];
					if ( 0 != ( GetComparisonOperatorFlag( ii ) & availableOperators ) )
						return ii;
				}
			}

			ComparisonOperator[] availableOperatorList = GetComparisonOperators( availableOperators );
			return availableOperatorList.Length > 0 ? availableOperatorList[0] : defaultUIOperator;
		}

		#endregion // GetCompatibleComparisonOperator

		#region IsCompareValueValid

		/// <summary>
		/// Indicates if the compare value is valid for the specified operator.
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator for which to check if the compare value is valid.</param>
		/// <param name="compareValue">Compare value.</param>
		/// <param name="convertCulture">Culture to use for converter purposes. If the compare value is not
		/// of the type suitable for the operator evaluation, it will be converted to appropriate type.</param>
		/// <param name="convertFormat">Format to use for converter purposes.</param>
		/// <param name="error">If the compar value is invalid, this parameter will be set to an error.</param>
		/// <returns>True if the compare value is valid. False otherwise.</returns>
		public static bool IsCompareValueValid( ComparisonOperator comparisonOperator, object compareValue, CultureInfo convertCulture, string convertFormat, out Exception error )
		{
			return ValidateValueHelper( comparisonOperator, compareValue, convertCulture, convertFormat, out error );
		}

		#endregion // IsCompareValueValid

		#region IsCountingOperator

		/// <summary>
		/// Indicates if the specified operator counts top or bottom N number of 
		/// values (Top, TopPercentile, Bottom, BottomPercentile).
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator to check.</param>
		/// <returns>Returns true if the operator is a counting operator.</returns>
		public static bool IsCountingOperator( ComparisonOperator comparisonOperator )
		{
			switch ( comparisonOperator )
			{
				case ComparisonOperator.Top:
				case ComparisonOperator.TopPercentile:
				case ComparisonOperator.Bottom:
				case ComparisonOperator.BottomPercentile:
					return true;
			}

			return false;
		}

		#endregion // IsCountingOperator

		#region IsMatch

		/// <summary>
		/// Returns true if the specified value matches the condition. False otherwise.
		/// </summary>
		/// <param name="value">Value to test. This is the value on the left hand side of the operator. Right hand side value is specified by the <see cref="Value"/> property.</param>
		/// <param name="context">Context information on where the value came from.</param>
		/// <returns>True if the value passes the condition, false otherwise.</returns>
		public bool IsMatch( object value, ConditionEvaluationContext context )
		{
			object lhs = value;
			object rhs = _value;

			// SSP 2/29/12 TFS89053
			// Added IFilterEvaluator interface to allow the developer to provide custom evaluation logic
			// and/or opportunity to convert values before we evaluate.
			// 
			IFilterEvaluator evaluator = null != context ? context.FilterEvaluator : null;
			if ( null != evaluator )
			{
				bool? ret = evaluator.Evaluate( _comparisonOperator, ref lhs, ref rhs, context );
				if ( ret.HasValue )
					return ret.Value;
			}

			// If the compare value is a special operand then delgate to it.
			// 
			SpecialFilterOperandBase s = rhs as SpecialFilterOperandBase;
			if ( null != s )
			{
				if ( s.SupportsOperator( _comparisonOperator ) )
					return s.IsMatch( _comparisonOperator, lhs, context );

				return false;
			}

			Type lhsConvertType = null;
			Type rhsConvertType = null;
			Type fallbackConvertType = null;
			if ( IsStringComparison( _comparisonOperator ) )
			{
				// If performing string comparison, like StartsWith, Contains etc... then both
				// values have to be converted to string.

				lhsConvertType = rhsConvertType = typeof( string );
			}
			else if ( IsCountingOperator( _comparisonOperator ) )
			{
				// If the operator is a counting operator like Top, Bottom, TopPercentile, 
				// BottomPercentile etc... then then RHS has to be a number and the LHS can 
				// be any type that's IComparable as long as all the LHS values are of the 
				// same type. Appropriate conversions for LHS are done in the Evaluate 
				// method itself.
				// 
				lhsConvertType = null;
				rhsConvertType = typeof( double );
			}
			else if ( IsQuantitativeComparison( _comparisonOperator ) )
			{
				// Otherwise when performing quantitative comparison, like GreaterThan, Equals etc...
				// then both have to be converted to the same type.

				// If one of the value is null/dnull then don't bother converting the other value
				// since the null/dbnull is always taken to be less than anything else.
				// 
				if ( null != lhs && DBNull.Value != lhs && null != rhs && DBNull.Value != rhs )
				{
					lhsConvertType = rhsConvertType = this.GetPreferredComparisonTypeHelper( context, lhs, rhs );

                    // JJD 5/26/09 - TFS17564
                    // don't use a fallback value for bool
                    if (rhsConvertType != typeof(bool))
					    fallbackConvertType = typeof( string );
				}
			}
			else
				Debug.Assert( false, "Unknown type of operator found!" );

			object lhsConverted = lhs;
			object rhsConverted = rhs;

			// Convert LHS and RHS to right types for comparison.
			// 
			if ( ! this.ConvertHelper( ref lhsConverted, lhsConvertType, context )
				|| ! this.ConvertHelper( ref rhsConverted, rhsConvertType, context ) )
			{
				// If above conversion fails then fallback to string for quantitative comparison.
				// 
				if ( null != fallbackConvertType )
				{
					if ( ! this.ConvertHelper( ref lhsConverted, fallbackConvertType, context )
						|| ! this.ConvertHelper( ref rhsConverted, fallbackConvertType, context ) )
						// If conversion fails, return false which means the value is a non-match.
						return false;
				}
				else
					// If conversion fails, return false which means the value is a non-match.
					return false;
			}

			return this.Evaluate( _comparisonOperator, lhsConverted, rhsConverted, context );
		}

		#endregion // IsMatch

		#region IsQuantitativeComparison

		/// <summary>
		/// Indicates if the specified operator performs quantitative comparison.
		/// </summary>
		/// <param name="op">Comparison operator to check.</param>
		/// <returns>Returns true if the operator performs quantitative comparison.</returns>
		public static bool IsQuantitativeComparison( ComparisonOperator op )
		{
			switch ( op )
			{
				case ComparisonOperator.Equals:
				case ComparisonOperator.NotEquals:
				case ComparisonOperator.GreaterThan:
				case ComparisonOperator.GreaterThanOrEqualTo:
				case ComparisonOperator.LessThan:
				case ComparisonOperator.LessThanOrEqualTo:
					return true;
			}

			return false;
		}

		#endregion // IsQuantitativeComparison

		#region IsStringComparison

		/// <summary>
		/// Indicates if the specified operator performs string comparison.
		/// </summary>
		/// <param name="op">Comparison operator to check.</param>
		/// <returns>Returns true if the operator performs string comparison.</returns>
		public static bool IsStringComparison( ComparisonOperator op )
		{
			switch ( op )
			{
				case ComparisonOperator.Contains:
				case ComparisonOperator.DoesNotContain:
				case ComparisonOperator.DoesNotEndWith:
				case ComparisonOperator.DoesNotMatch:
				case ComparisonOperator.DoesNotStartWith:
				case ComparisonOperator.EndsWith:
				case ComparisonOperator.Like:
				case ComparisonOperator.Match:
				case ComparisonOperator.NotLike:
				case ComparisonOperator.StartsWith:
					return true;
			}

			return false;
		}

		#endregion // IsStringComparison

		#endregion // Public Methods

		#region Private/Internal Methods

		#region ClearCache

		private void ClearCache( )
		{
			_cachedRegex = null;
			_cachedRegex_Pattern = null;

			_topValuesOperand = null;
			_topValuesOperand_Number = double.NaN;
		}

		#endregion // ClearCache

		#region CompareHelper

		private static int CompareHelper( object x, object y, ConditionEvaluationContext context )
		{
			// SSP 5/3/10 TFS25788
			// Added FilterValueComparer property on the FieldSettings in data presenter.
			// 
			IComparer comparer = null != context ? context.Comparer : null;
			if ( null != comparer )
				return comparer.Compare( x, y );

			// If both are strings and compare using IgnoreCase and Culture values supplied by the context.
			// 
			if ( x is string && y is string )
			{
				return string.Compare( (string)x, (string)y, context.IgnoreCase, GetCulture( context ) );
			}

			if ( null == x || x is DBNull )
			{
				if ( null == y || y is DBNull )
					return 0;

				return -1;
			}

			if ( null == y || y is DBNull )
				return 1;

			// Conversions are done in IsMatch method.
			Debug.Assert( x.GetType( ) == y.GetType( ) );

			if ( x is IComparable )
			{
				int r = System.Collections.Comparer.Default.Compare( x, y );

				if ( r < 0 )
					return -1;
				else if ( r > 0 )
					return 1;
				else
					return 0;
			}

			return -2;
		}

		#endregion // CompareHelper

		#region ConvertDataType

		private object ConvertDataType( object x, Type targetType, ConditionEvaluationContext context )
		{
		    // JJD 5/26/09 - TFS17564
            // Added separate cached value when converting to string
            // since we might otherwise use a fallback type and blow away our
            // cached value
            if (targetType == typeof(string))
            {
                if (_lastConversionToStringSourceValue == x && x != null)
                    return _lastConvertedToStringValue;
            }
            else
            {
		        // JJD 5/26/09 - TFS17564
                // Check x != null instead of _lastConvertedValue which might have legitimately
                // been converted to null
                //if ( _lastConversionSourceValue == x && _lastConversionType == targetType && null != _lastConvertedValue )
                if (_lastConversionSourceValue == x && _lastConversionType == targetType && x != null)
                    return _lastConvertedValue;
            }

			object tmp = ConvertDataTypeHelper( x, targetType, context );

			// Cache the converted result of the condition's compare value so we can reuse the converted
			// value next time IsMatch is called.
			// 
			if ( x == _value )
			{
                // JJD 5/26/09 - TFS17564
                // Added separate cached value when converting to string
                // since we might otherwise use a fallback type and blow away our
                // cached value
                if (targetType == typeof(string))
                {
                    _lastConversionToStringSourceValue = x;
                    _lastConvertedToStringValue = tmp;
                }
                else
                {
                    _lastConversionSourceValue = x;
                    _lastConversionType = targetType;
                    _lastConvertedValue = tmp;
                }
			}

			return tmp;
		}

		#endregion // ConvertDataType

		#region ConvertDataTypeHelper

		internal static object ConvertDataTypeHelper( object x, Type targetType, ConditionEvaluationContext context )
		{
			if ( null == x || DBNull.Value == x )
				return x;

			Type xType = x.GetType( );
			if ( xType == targetType )
				return x;

			// If value is numeric and the target type is also numeric then conversion doesn't require
			// any culture/format info. Therefore try to convert without it for better efficiency.
			// 
			if ( Utilities.IsNumericType( targetType ) && Utilities.IsNumericType( xType ) )
			{
				try
				{
					return Convert.ChangeType( x, targetType );
				}
				catch
				{
				}
			}
                 
			ValueEntry info = context.CurrentValue;
			return Utilities.ConvertDataValue( x, targetType, info.Culture, info.Format );
		}

		#endregion // ConvertDataTypeHelper

		#region ConvertHelper

		private bool ConvertHelper( ref object val, Type targetType, ConditionEvaluationContext context )
		{
			if ( null != targetType )
			{
				// Attempt to convert to the target type.
				// 
				object convertedValue = this.ConvertDataType( val, targetType, context );

				// If conversion fails, return false.
				// 
				if ( null == convertedValue && null != val )
					return false;

				val = convertedValue;
			}

			return true;
		}

		#endregion // ConvertHelper

		#region ConvertToString

		private string ConvertToString( object x, ConditionEvaluationContext context )
		{
			string s = x as string;

			if ( null == s )
				s = this.ConvertDataType( x, typeof( string ), context ) as string;

			return null != s ? s : string.Empty;
		}

		#endregion // ConvertToString

		#region CreateTopValuesOperandHelper

		private static SpecialFilterOperandBase CreateTopValuesOperandHelper( ComparisonOperator op, double topNValue, out Exception error )
		{
			switch ( op )
			{
				case ComparisonOperator.Top:
					return SpecialFilterOperandFactory.TopValuesOperand.CreateHelper( "Top", topNValue, false, true, out error );
				case ComparisonOperator.Bottom:
					return SpecialFilterOperandFactory.TopValuesOperand.CreateHelper( "Bottom", topNValue, false, false, out error );
				case ComparisonOperator.TopPercentile:
					return SpecialFilterOperandFactory.TopValuesOperand.CreateHelper( "TopPercentile", topNValue, true, true, out error );
				case ComparisonOperator.BottomPercentile:
					return SpecialFilterOperandFactory.TopValuesOperand.CreateHelper( "BottomPercentile", topNValue, true, false, out error );
				default:
					Debug.Assert( false, "Unknown Top Values operator!" );
					error = new InvalidOperationException( );
					return null;
			}
		}

		#endregion // CreateTopValuesOperandHelper

		#region Evaluate

		private bool Evaluate( ComparisonOperator op, object lhs, object rhs, ConditionEvaluationContext context )
		{
			// First do non-string comparison operators. Note that they could still be comparing strings however
			// they are quantitative operators.
			// 
			int r;
			switch ( op )
			{
				case ComparisonOperator.Equals:
					r = CompareHelper( lhs, rhs, context );
					return -2 != r && 0 == r;
				case ComparisonOperator.NotEquals:
					return !this.Evaluate( ComparisonOperator.Equals, lhs, rhs, context );

				case ComparisonOperator.GreaterThan:
					r = CompareHelper( lhs, rhs, context );
					return -2 != r && r > 0;
				case ComparisonOperator.GreaterThanOrEqualTo:
					r = CompareHelper( lhs, rhs, context );
					return -2 != r && r >= 0;

				case ComparisonOperator.LessThan:
					r = CompareHelper( lhs, rhs, context );
					return -2 != r && r < 0;
				case ComparisonOperator.LessThanOrEqualTo:
					r = CompareHelper( lhs, rhs, context );
					return -2 != r && r <= 0;
				case ComparisonOperator.Top:
				case ComparisonOperator.Bottom:
				case ComparisonOperator.TopPercentile:
				case ComparisonOperator.BottomPercentile:
					{
						SpecialFilterOperandBase topValuesOperand = null;

						// RHS is converted to double in the IsMatch method before it calls this method.
						// 
						if ( rhs is double )
						{
							double n = (double)rhs;

							// Get cached top values operand. Cache is based on the comparison operator and
							// the topN value.
							// 
							if ( op == _topValuesOperand_Operator && n == _topValuesOperand_Number )
							{
								topValuesOperand = _topValuesOperand;
							}
							// If the _value or the comparison operator has changed, then re-create the 
							// appropriate comparison operand.
							// 
							else
							{
								// SSP 7/21/11 TFS81583
								// Clear any cache the context may be storing for the old operand.
								// 
								SpecialFilterOperandFactory.TopValuesOperand.ClearUserCacheHelper( _topValuesOperand, context );

								Exception error;
								topValuesOperand = CreateTopValuesOperandHelper( op, n, out error );
								_topValuesOperand = topValuesOperand;
								_topValuesOperand_Number = n;
								_topValuesOperand_Operator = op;
								
								// SSP 7/21/11 TFS81583
								// 
								//context.UserCache = null;
							}
						}

						// TopValuesOperand can be null if the rhs is out of range. For example if the operator
						// is TopPercentile and rhs is less than 0 or over 100.
						// 
						//Debug.Assert( null != topValuesOperand );
						return null != topValuesOperand && topValuesOperand.IsMatch( ComparisonOperator.Equals, lhs, context );
					}
			}


			// Do string comparison operators.
			// 
			string lhsStr = ConvertToString( lhs, context );
			string rhsStr = ConvertToString( rhs, context );
			bool ignoreCase = context.IgnoreCase;

			switch ( op )
			{
				case ComparisonOperator.Contains:
					{
						if ( ignoreCase )
						{
							string xxStrLower = ToLower( lhsStr, context );
							string yyStrLower = ToLower( rhsStr, context );
							return xxStrLower.Contains( yyStrLower );
						}
						else
						{
							return lhsStr.Contains( rhsStr );
						}
					}
				case ComparisonOperator.DoesNotContain:
					return !this.Evaluate( ComparisonOperator.Contains, lhsStr, rhsStr, context );

				case ComparisonOperator.StartsWith:
					return lhsStr.StartsWith( rhsStr, ignoreCase, GetCulture( context ) );
				case ComparisonOperator.DoesNotStartWith:
					return !this.Evaluate( ComparisonOperator.StartsWith, lhsStr, rhsStr, context );

				case ComparisonOperator.EndsWith:
					return lhsStr.EndsWith( rhsStr, ignoreCase, GetCulture( context ) );
				case ComparisonOperator.DoesNotEndWith:
					return !this.Evaluate( ComparisonOperator.EndsWith, lhsStr, rhsStr, context );

				case ComparisonOperator.Like:
					{
						string wildcard = rhsStr;
						Regex rx = this.GetCachedRegex( wildcard, true, ignoreCase );

						return null != rx && rx.IsMatch( lhsStr );
					}
				case ComparisonOperator.NotLike:
					return !this.Evaluate( ComparisonOperator.Like, lhsStr, rhsStr, context );

				case ComparisonOperator.Match:
					{
						Regex rx = rhs as Regex;

						if ( null == rx )
						{
							string regexPattern = rhsStr;
							rx = this.GetCachedRegex( regexPattern, false, ignoreCase );
						}

						return null != rx && rx.IsMatch( lhsStr );
					}

				case ComparisonOperator.DoesNotMatch:
					return !this.Evaluate( ComparisonOperator.Match, lhsStr, rhsStr, context );
			}

			Debug.Assert( false, "Unknown comparison operator." );
			return false;
		}

		#endregion // Evaluate

		#region GetCachedRegex

		private Regex GetCachedRegex( string pattern, bool isPatternWildcard, bool ignoreCase )
		{
			Regex rx = null;

			// Instead of parsing regex every time, cache it. If the last cached regex was created with
			// the same pattern, reuse the cached regex object.
			// 
			if ( _cachedRegex_Pattern == pattern && _cachedRegex_IgnoreCase == ignoreCase )
			{
				rx = _cachedRegex;
			}
			else
			{
				_cachedRegex_Pattern = pattern;
				_cachedRegex_IgnoreCase = ignoreCase;

				try
				{
					if ( isPatternWildcard )
					{
						rx = WildcardToRegexConverter.Convert( pattern, ignoreCase );
					}
					else
					{
						RegexOptions regexOptions = RegexOptions.ExplicitCapture;
						if ( ignoreCase )
							regexOptions |= RegexOptions.IgnoreCase;

						rx = new Regex( pattern, regexOptions );
					}
				}
				catch
				{
					Debug.Assert( false, "Invalid regular expression." );
				}

				_cachedRegex = rx;
			}

			return rx;
		}

		#endregion // GetCachedRegex

		#region GetCulture

		private static CultureInfo GetCulture( ConditionEvaluationContext context )
		{
			CultureInfo culture = null;
			if ( null != context )
			{
				ValueEntry currentValue = context.CurrentValue;
				if ( null != currentValue )
					culture = currentValue.Culture;
			}

			if ( null == culture )
				culture = System.Globalization.CultureInfo.CurrentCulture;

			return culture;
		}

		#endregion // GetCulture

		#region GetPreferredComparisonTypeHelper

		private Type GetPreferredComparisonTypeHelper( ConditionEvaluationContext context, object lhs, object rhs )
		{
			Type prefComparisonType = context.PreferredComparisonDataType;

			// If the preferred comparsion type is not specified (for example if the 
			// field data type is object)
			//
			if ( null == prefComparisonType || typeof( object ) == prefComparisonType )
			{
				// If both values are of same type then leave them as they are.
				// 
				Type lhsType = Utilities.GetUnderlyingType( lhs.GetType( ) );
				Type rhsType = Utilities.GetUnderlyingType( rhs.GetType( ) );
				if ( lhsType == rhsType )
					prefComparisonType = lhsType;
				// If one of the values is number then convert both to decimal and compare.
				// 
				else if ( Utilities.IsNumericType( lhsType ) || Utilities.IsNumericType( rhsType ) )
					prefComparisonType = typeof( decimal );
				// Otherwise convert to string.
				// 
				else
					prefComparisonType = typeof( string );
			}

			return prefComparisonType;
		}

		#endregion // GetPreferredComparisonTypeHelper

		#region IsValidRegex

		private static bool IsValidRegex( string regex, out Exception error )
		{
			Exception innerError = null;

			try
			{
				if ( !string.IsNullOrEmpty( regex ) )
				{
					new Regex( regex );
					error = null;
					return true;
				}
			}
			catch ( Exception e )
			{
				innerError = e;
			}

			error = new ArgumentException( "value", SR.GetString( "Filter_Regex_InvalidPattern" ), innerError );
			return false;
		}

		#endregion // IsValidRegex

		#region IsValidWildcard

		private static bool IsValidWildcard( string wildcard, out Exception error )
		{
			string regex;
			RegexOptions options;
			WildcardToRegexConverter.Convert( wildcard, out regex, out options, out error );

            return null == error;
		}

		#endregion // IsValidWildcard

		#region ToLower

		private string ToLower( string str, ConditionEvaluationContext context )
		{
			return str.ToLower( GetCulture( context ) );
		}

		#endregion // ToLower

		#region ValidateValueHelper

		/// <summary>
		/// Throws an exception if the value is invalid for the specified comparison operator.
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator.</param>
		/// <param name="value">Value to validate.</param>
		private static void ValidateValueHelper( ComparisonOperator comparisonOperator, object value )
		{
			Exception error;
			if ( ! ValidateValueHelper( comparisonOperator, value, null, null, out error ) )
				throw error;
		}

		/// <summary>
		/// Throws an exception if the value is invalid for the specified comparison operator.
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator.</param>
		/// <param name="value">Value to validate.</param>
		/// <param name="convertCulture">Culture to use for conversion purposes.</param>
		/// <param name="convertFormat">Format to use for conversion purposes.</param>
		/// <param name="error">If the value is invalid, this will be set to an error message.</param>
		/// <remarks>True if the value is valid for the specified comparison opeator. False otherwise.</remarks>
		private static bool ValidateValueHelper( ComparisonOperator comparisonOperator, object value, CultureInfo convertCulture, string convertFormat, out Exception error )
		{
			error = null;

			SpecialFilterOperandBase specialOperand = value as SpecialFilterOperandBase;
			if ( null != specialOperand )
			{
				if ( !specialOperand.SupportsOperator( comparisonOperator ) )
				{
					error = new ArgumentException( "value", SR.GetString( "Filter_IncompatibleOperand" ) );
					return false;
				}

				return true;
			}

			if ( null == convertCulture )
				convertCulture = GetCulture( null );

			switch ( comparisonOperator )
			{
				case ComparisonOperator.Match:
				case ComparisonOperator.DoesNotMatch:
					{
						string valueAsString = Utilities.ConvertDataValue( value, typeof( string ), convertCulture, convertFormat ) as string;
						if ( !IsValidRegex( valueAsString, out error ) )
							return false;
					}
					break;

				case ComparisonOperator.Like:
				case ComparisonOperator.NotLike:
					{
						string valueAsString = Utilities.ConvertDataValue( value, typeof( string ), convertCulture, convertFormat ) as string;
						if ( !IsValidWildcard( valueAsString, out error ) )
							return false;
					}
					break;
			}

			if ( IsCountingOperator( comparisonOperator ) )
			{
				object convertedVal = Utilities.ConvertDataValue( value, typeof( double ), convertCulture, convertFormat );
				bool isValidNumber = convertedVal is double
					&& null != CreateTopValuesOperandHelper( comparisonOperator, (double)convertedVal, out error );

				if ( !isValidNumber )
				{
					if ( null == error )
						error = new ArgumentException( SR.GetString( "LE_ArgumentException_26" ) );

					return false;
				}
			}

			return true;
		}

		#endregion // ValidateValueHelper

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // ComparisonCondition Class

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