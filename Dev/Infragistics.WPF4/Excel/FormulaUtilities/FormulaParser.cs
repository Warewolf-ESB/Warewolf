using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using Infragistics.Documents.Excel.FormulaUtilities.Constants;
using Infragistics.Documents.Excel.Serialization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.FormulaUtilities
{
	internal class FormulaParser
	{
		#region Formula Grammar Rules

		// * - 0 or more repetitions
		// + - 1 or more repetition
		// ? - 0 or 1 repetition
		//
		// Formula					:	"=" Expression
		//
		// Expression				:	Primary ( Binary Primary )*
		//
		// Primary					:	Secondary ( UnaryL )*
		//
		// Secondary				:	Number | ( UnaryR Secondary ) | String | ( '(' Expression ')' ) | Function | Boolean | Reference | Array | Error
		//
		// Boolean					:	"TRUE" | "FALSE"
		//
		// Number					:	( ([-+]?[0-9]+( '.'[0-9]+ )?) | ('.'[0-9]+) ) ([eE][-+]?[0-9]+)?
		// 
		// String					:	'"' ( [^0|'"'] | '""' )* '"'
		//
		// Array					:	'{' Matrix '}'
		// Matrix					:	MatrixRow ( MatrixRowSeparator MatrixRow )*
		// MatrixRowSeparator		: ';'
		// MatrixRow				:	MatrixElement ( Separator MatrixElement)*
		// MatrixElement			:	Number | String | Error | Boolean
		//
		// UnaryR					:	'+' | '-'
		//
		// UnaryL					:	'%'
		//
		// Binary					:	ArithmeticOp | ComparisonOp | '&' | ReferenceOp
		// ArithmeticOp				:	'+' | '-' | '*' | '/' | '^'
		// ComparisonOp				:	'=' | '<>' | '<' | '>' | '<=' | '>='
		// ReferenceOp				:	':' | ' ' | ','  // The ',' is only a binary operator when not directly (non-nested) in a function.  Otherwise, it is a parameter separator
		//
		// Function					:	FunctionName ( ParameterList )? ')'

		// MD 10/9/07 - BR27172
		// Periods are allowed in function named
		//// FunctionName				:	[A-Za-z][A-Za-z0-9]* '('
		// FunctionName				:	[A-Za-z](.?[A-Za-z0-9.])* '('

		// ParameterList			:	EmptyOrParameter ( Separator EmptyOrParameter )*
		// EmptyOrParameter			:	/*empty*/ | Expression
		// Separator				:	','
		//
		// Error					:	#NULL! | #DIV/0! | #VALUE! | #REF! | #NAME? | #NUM! | #N/A
		//
		// Reference				:	( WorksheetReference? ( CellRange | ColumnRange | RowRange | CellAddress | Name | RowAddressR1C1 | ColumnAddressR1C1 ) ) | ( WorkbookReference Name )
		//
		// CellRange				:	CellAddress ':' CellAddress
		// CellAddress				:	( ColumnAddressA1 RowAddressA1 ) | ( RowAddressR1C1 ColumnAddressR1C1 )
		// RowRange					:	RowAddress ':' RowAddress
		// ColumnRange				:	ColumnAddress ':' ColumnAddress
		//
		// ColumnAddress			:	ColumnAddressA1 | ColumnAddressR1C1
		// ColumnAddressA1			:	('$')? [A-Z|a-z]{1,3}
		// ColumnAddressR1C1		:	'C' ( ( '[' [-|+]? [0-9]{1,6} ']' ) | ( [0-9]{1,6} ) )?
		//
		// RowAddress				:	RowAddressA1 | RowAddressR1C1
		// RowAddressA1				:	('$')? [0-9]{1,6}
		// RowAddressR1C1			:	'R' ( ( '[' [-|+]? [0-9]{1,6} ']' ) | ( [0-9]{1,6} ) )?
		//
		// Name						:	[\_����a-ZA-Z][\_����a-zA-Z0-9.]*		// This cannot be CellAddressA1 or start with ColumnAddressR1C1 or RowAddressR1C1
		//
		// WorksheetNameSafe		:	[_A-Za-z����] [_0-9A-Za-z����.]*
		// WorksheetNameQuoted		:	( [^*[]:'/\?] | ''' ''' )+
		//
		// FilePath					:	LocalPath | UrlPath
		// LocalPath				:	[a-zA-z]:\ ( ( [^\/:*?"<>|[]'] | ''' ''' )* \ )*
		// UrlPath					:	(ht|f)tp(s?):// [0-9a-zA-Z] ( [-.\w]* [0-9a-zA-Z] )* (:(0-9)*)* (/?) ( [a-zA-Z0-9-.?,'/\+&%$#_]*)? /
		//
		// FileNameSafe				:	[_A-Za-z����] [_0-9A-Za-z����.]*
		// FileNameBracketedSafe	:	'[' [_0-9A-Za-z����] [_0-9A-Za-z����.]* ']'
		// FileNameQuoted			:	( [^\/:*?"<>|[]'] | ''' ''' )+
		// FileNameBracketedQuoted	:	FilePath? '[' FileNameQuoted ']'
		//
		// WorksheetReference		:	WorksheetReferenceSafe | WorksheetReferenceQuoted
		// WorksheetReferenceSafe	:	FileNameBracketedSafe? WorksheetNameSafe '!'
		// WorksheetReferenceQuoted	:	''' FileNameBracketedQuoted? WorksheetNameQuoted ''' '!'
		//
		// WorkbookReference		:	WorkbookReferenceSafe | WorkbookReferenceQuoted
		// WorkbookReferenceSafe	:	FileNameSafe '!'
		// WorkbookReferenceQuoted	:	''' FilePath? FileNameQuoted ''' '!'

		#endregion Formula Grammar Rules

        // MBS 7/25/08 - Excel 2007 Format
        // Refactored code into a helper method
        #region ParseError

		// MD 4/6/12 - TFS101506
        //public static ErrorValue ParseError(string value)
		public static ErrorValue ParseError(string value, CultureInfo culture)
        {
            ErrorValue errorValue = null;
			// MD 4/6/12 - TFS101506
            //switch (value.ToUpper(CultureInfo.CurrentCulture))
			switch (value.ToUpper(culture))
            {
                case "#NULL!":
                    // MD 7/14/08 - Excel formula solving
                    // Not sure how this happened, but the wrong error codes were used for some of the error text values.
                    //errorValue = ErrorValue.ArgumentOrFunctionNotAvailable;
                    errorValue = ErrorValue.EmptyCellRangeIntersection;
                    break;
                case "#DIV/0!":
                    errorValue = ErrorValue.DivisionByZero;
                    break;
                case "#VALUE!":
                    // MD 7/14/08 - Excel formula solving
                    // Not sure how this happened, but the wrong error codes were used for some of the error text values.
                    //errorValue = ErrorValue.EmptyCellRangeIntersection;
                    errorValue = ErrorValue.WrongOperandType;
                    break;
                case FormulaParser.ReferenceErrorValue:
                    errorValue = ErrorValue.InvalidCellReference;
                    break;
                case "#NAME?":
                    errorValue = ErrorValue.WrongFunctionName;
                    break;
                case "#NUM!":
                    errorValue = ErrorValue.ValueRangeOverflow;
                    break;
                case "#N/A":
                    // MD 7/14/08 - Excel formula solving
                    // Not sure how this happened, but the wrong error codes were used for some of the error text values.
                    //errorValue = ErrorValue.WrongOperandType;
                    errorValue = ErrorValue.ArgumentOrFunctionNotAvailable;
                    break;
                default:
					// MD 5/13/11 - Data Validations / Page Breaks
					// We now use this method to see if we have an error value, so we shouldn't assert when we don't match.
                    //Utilities.DebugFail("Unknown error type");
                    return null;
            }

            return errorValue;
        }
        #endregion //ParseError

        #region ParseFormula

        // MD 7/9/08 - Excel 2007 Format
		//internal static Formula ParseFormula( string formula, CellReferenceMode cellReferenceMode, FormulaType type, out FormulaParseException parseException )
		internal static Formula ParseFormula( 
			string formula, CellReferenceMode cellReferenceMode, FormulaType type, WorkbookFormat currentFormat, CultureInfo culture,
			List<WorkbookReferenceBase> indexedReferencesDuringLoad,	// MD 2/23/12 - TFS101504
			out FormulaParseException parseException )
		{
			// MD 7/9/08 - Excel 2007 Format
			//FormulaParser parser = new FormulaParser( cellReferenceMode, formula, type );
			FormulaParser parser = new FormulaParser( cellReferenceMode, formula, type, currentFormat, culture );

			// MD 2/23/12 - TFS101504
			parser.indexedReferencesDuringLoad = indexedReferencesDuringLoad;

			Formula parsedFormula = parser.ParseFormula();
			parseException = parser.ParseException;

			return parsedFormula;
		}

		#endregion ParseFormula


		#region Constants

        // MBS 7/10/08 - Excel 2007 Format
        //private const int MaxFunctionNesting = 8;

		// MD 9/24/08 - TFS8014
		// The nesting level for 2003 is 8, not 7.
        //private const int MaxFunctionNesting97To2003 = 7;
		internal const int MaxFunctionNesting97To2003 = 8;

		internal const int MaxFunctionNesting2007 = 64;
		internal const int MaxFormulaLength97To2003 = 1024;
        internal const int MaxFormulaLength2007 = 8192;
		internal const int MaxStringConstantLength = 255;  // Note: True for both 97To2003 and 2007

		internal const string ReferenceErrorValue = "#REF!";

		internal const string ConcatOperator = "&";
		internal const string RangeOperator = ":";

		//internal const string UnionOperator = ",";
		private const string UnionOperator = ",";

		// MD 10/22/10 - TFS36696
		private const string UnionOperatorAlternate = ";";

		// MD 7/14/08 - Excel formula solving
		internal const char WorksheetNameSeparator = '!';

		// MD 12/7/11 - 12.1 - Table Support
		public const string KeywordAll = "#All";
		public const string KeywordData = "#Data";
		public const string KeywordHeaders = "#Headers";
		public const string KeywordTotals = "#Totals";
		public const string KeywordThisRow = "#This Row";

		#endregion Constants

		#region Member Variables

		private readonly string cachedDecimalSeparator;
		private readonly CellReferenceMode cellReferenceMode;
		private readonly CultureInfo culture;

		// MD 7/9/08 - Excel 2007 Format
		private readonly WorkbookFormat currentFormat;

		private readonly string formula;
		private readonly Formula formulaInstance;
		private readonly List<FormulaToken> infixTokenList;
		private readonly string matrixValueSeparatorResolved;
		private readonly FormulaType type;
		private readonly string unionOperatorResolved;

		private string nextWhitespace;
		private int startIndex;
		private bool isVolitile;
		private FormulaParseException parseException;
		private int functionNesting;

		// MD 10/22/10 - TFS36696
		// To save space, we will no longer save these on the token. They are only needed when parsing formulas anyway.
		private Dictionary<FormulaToken, int> tokenStartCharIndices;
		private Dictionary<FormulaToken, List<AttrSpaceToken>> tokenPreceedingWhitespace;

		// MD 12/7/11 - 12.1 - Table Support
		private StructuredTableReferenceParser structuredTableReferenceParserHelper;

		// MD 2/23/12 - TFS101504
		private List<WorkbookReferenceBase> indexedReferencesDuringLoad;

		#endregion Member Variables

		#region Constructor

		// MD 7/9/08 - Excel 2007 Format
		//private FormulaParser( CellReferenceMode cellReferenceMode, string formula, FormulaType type )
		private FormulaParser( CellReferenceMode cellReferenceMode, string formula, FormulaType type, WorkbookFormat currentFormat, CultureInfo culture )
		{
			this.cellReferenceMode = cellReferenceMode;

			// MD 7/9/08 - Excel 2007 Format
			this.currentFormat = currentFormat;

			this.infixTokenList = new List<FormulaToken>();
			this.formula = formula;
			this.type = type;

			// MD 5/13/11 - Data Validations / Page Breaks
			// We allow a null CultureInfo.
			//this.culture = culture;
			this.culture = culture ?? CultureInfo.CurrentCulture;

			this.cachedDecimalSeparator = this.culture.NumberFormat.NumberDecimalSeparator;
			this.unionOperatorResolved = FormulaParser.GetUnionOperatorResolved( this.cachedDecimalSeparator );
			this.matrixValueSeparatorResolved = FormulaParser.GetMatrixValueSeparatorResolved( this.cachedDecimalSeparator );

			this.formulaInstance = Formula.CreateFormula( this.cellReferenceMode, this.type );

			// MD 7/23/08 - Excel formula solving
			// The formula needs to have a reference to a workbook with the correct format set when it is being parsed.
			this.formulaInstance.CurrentFormat = this.currentFormat;

			// MD 10/22/10 - TFS36696
			// To save space, we will no longer save these on the token. They are only needed when parsing formulas anyway.
			this.tokenStartCharIndices = new Dictionary<FormulaToken, int>();
			this.tokenPreceedingWhitespace = new Dictionary<FormulaToken, List<AttrSpaceToken>>();
		}

		#endregion Constructor

		#region Parse various grammar types

		#region ParseFormula

		// Formula	:	"=" Expression
		private Formula ParseFormula()
		{
            // MBS 7/11/08 - Excel 2007 Format
            if (this.formula.Length > this.MaxFormulaLength)
            {
                this.ParseException = new FormulaParseException(
                    0,
                    this.formula,
                    String.Format(SR.GetString("LE_FormulaParseException_TooLong"), this.MaxFormulaLength),
                    this.formula);

                return null;
            }

			this.startIndex = 0;
			if ( this.TestCurrentCharAndAdvance( '=' ) == false )
			{
				this.ParseException = new FormulaParseException( 
					0, 
					this.formula, 
					SR.GetString( "LE_FormulaParseException_NoEqualsSign" ), 
					this.formula );

				return null;
			}

			this.ParseNextWhitespace();

			if ( this.ParseExpression( true ) == false )
			{
				if ( this.ParseException == null )
				{
					this.ParseException = new FormulaParseException( 
						0, 
						this.formula, 
						SR.GetString( "LE_FormulaParseException_NoExpressions" ), 
						this.formula.Substring( 1 ) );
				}

				return null;
			}

			if ( this.startIndex != this.formula.Length )
			{
				this.ParseException = new FormulaParseException( 
					this.startIndex, 
					this.formula, 
					SR.GetString( "LE_FormulaParseException_ExtraExpressions" ), 
					this.formula.Substring( this.startIndex ) );

				return null;
			}

			this.ConvertInfixToPostfix();

			if ( this.isVolitile )
			{
				// MD 7/23/08 - Excel formula solving
				// The formula should know if it contains a volitile function call.
				this.formulaInstance.RecalculateAlways = true;

				// The token list should at least have a function operator, so we don't need to check the count is > 0
				AttrSpaceToken spaceToken = this.formulaInstance.PostfixTokenList[ 0 ] as AttrSpaceToken;

				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//if ( spaceToken != null )
				//    this.formulaInstance.PostfixTokenList[ 0 ] = new AttrSpaceVolitileToken( this.formulaInstance, spaceToken );
				//else
				//    this.formulaInstance.PostfixTokenList.Insert( 0, new AttrVolitileToken( this.formulaInstance ) );
				if (spaceToken != null)
					this.formulaInstance.PostfixTokenList[0] = new AttrSpaceVolitileToken(spaceToken);
				else
					this.formulaInstance.PostfixTokenList.Insert(0, new AttrVolitileToken());
			}

			// MD 10/22/10 - TFS36696
			// Actually, this does need to be done so we can cache each token's string portion in the formula.
			//// MD 10/22/10 - TFS36696
			//// This doesn't really seem necessary, especially at runtime.
			//FormulaStringGenerator stringGenerator = new FormulaStringGenerator(this.formulaInstance, this.cellReferenceMode, this.culture);
			//
			//EvaluationResult<string> stringResult = stringGenerator.Evaluate();
			//Debug.Assert(stringResult.Completed);
			Dictionary<FormulaToken, string> tokenPortionsInFormula = new Dictionary<FormulaToken,string>();
			FormulaStringGenerator stringGenerator = new FormulaStringGenerator(this.formulaInstance, this.cellReferenceMode, this.culture, tokenPortionsInFormula);

			EvaluationResult<string> stringResult = stringGenerator.Evaluate();
			Debug.Assert(stringResult.Completed);

			// MD 10/22/10 - TFS36696
			// To save space, we will no longer save the portions in the record stream on the tokens.
			//TokenReferenceResolver referenceResolver = new TokenReferenceResolver( this.formulaInstance );
			Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream = new Dictionary<FormulaToken, TokenPositionInfo>();
			TokenReferenceResolver referenceResolver = new TokenReferenceResolver(this.formulaInstance, tokenPositionsInRecordStream);

			EvaluationResult<FormulaToken> referenceResult = referenceResolver.Evaluate();

			// MD 10/22/10 - TFS36696
			// If there was an error, cache it's portion in the formula and start char index here.
			string portionInFormula = null;
			int startCharIndex = -1;
			if (referenceResult.TokenWithError != null)
			{
				tokenPortionsInFormula.TryGetValue(referenceResult.TokenWithError, out portionInFormula);
				this.tokenStartCharIndices.TryGetValue(referenceResult.TokenWithError, out startCharIndex);
			}

			Debug.Assert( referenceResult.Completed, "There shouldn't have been a problem here." );
			if ( referenceResult.Completed == false )
			{
				this.ParseException = new FormulaParseException(
					// MD 10/22/10 - TFS36696
					// Use the cached char index
					//referenceResult.TokenWithError.StartCharIndex, 
					startCharIndex,
					this.formula, 
					SR.GetString( "LE_FormulaParseException_UnknownError" ),
					// MD 10/22/10 - TFS36696
					// Use the cached portion within the formula
					//referenceResult.TokenWithError.PortionInFormula );
					portionInFormula);

				return null;
			}

			TokenClassResolver tokenClassResolver = new TokenClassResolver( this.formulaInstance, type );
			EvaluationResult<TokenClassResolver.FormulaTokenNode> tokenClassResult = tokenClassResolver.Evaluate();

			if ( tokenClassResult.Completed == false )
			{
				this.ParseException = new FormulaParseException(
					// MD 10/22/10 - TFS36696
					// Use the cached char index
					//referenceResult.TokenWithError.StartCharIndex, 
					startCharIndex,
					this.formula, 
					SR.GetString( "LE_FormulaParseException_InvalidArguments" ),
					// MD 10/22/10 - TFS36696
					// Use the cached portion within the formula
					//referenceResult.TokenWithError.PortionInFormula );
					portionInFormula);

				return null;
			}

			// MD 5/18/07 - BR23022
			// Named reference formulas need to have additional checks done: they cannot have non 3D cell references
			if ( this.type == FormulaType.NamedReferenceFormula || 
				this.type == FormulaType.ExternalNamedReferenceFormula )
			{
				foreach ( FormulaToken token in this.formulaInstance.PostfixTokenList )
				{
					CellReferenceToken refernceToken = token as CellReferenceToken;

					// If a token in the formula is a cell reference token, and it is not a 3D reference,
					// throw an error
					if ( refernceToken != null && refernceToken.Is3DReference == false )
					{
						// MD 10/22/10 - TFS36696
						tokenPortionsInFormula.TryGetValue(refernceToken, out portionInFormula);
						this.tokenStartCharIndices.TryGetValue(refernceToken, out startCharIndex);

						this.ParseException = new FormulaParseException(
							// MD 10/22/10 - TFS36696
							// Use the cached char index
							//refernceToken.StartCharIndex, 
							startCharIndex,
							this.formula, 
							SR.GetString( "LE_FormulaParseException_NamedReferenceRefsNeedSheetName" ),
							// MD 10/22/10 - TFS36696
							//refernceToken.PortionInFormula );
							portionInFormula);

						return null;
					}
				}
			}

			return this.formulaInstance;
		}

		#endregion ParseFormula

		#region ParseExpression

		// Expression	:	Primary ( Binary Primary )*
		private bool ParseExpression( bool allowListOperator )
		{
			int expressionStart = this.startIndex;
			if ( this.ParsePrimary() == false )
			{
				bool binaryAllowsRollback;
				if ( this.ParseBinary( allowListOperator, out binaryAllowsRollback ) )
				{
					if ( binaryAllowsRollback == false )
					{
						// Find the end of the entire binary operation
						this.ParsePrimary();
						int endIndex = this.startIndex;

						this.ParseException = new FormulaParseException( 
							expressionStart, 
							this.formula, 
							SR.GetString( "LE_FormulaParseException_MissingArgumentBeforeBinary" ), 
							this.formula.Substring( expressionStart, endIndex - expressionStart ) );
					}
				}

				return false;
			}

			while ( true )
			{
				PositionInfo position = this.SavePosition();

				int binaryIndex = this.startIndex;
				bool binaryAllowsRollback;
				if ( this.ParseBinary( allowListOperator, out binaryAllowsRollback ) == false )
				{
					this.RestorePosition( position );
					return true;
				}

				// If the next primary is parsed successfully, we want the expression start index to indicate the start 
				// of the new primary. This is because if we report a problem with a binary, we wan the portion to 
				// indicate just the binary and its surrounding primaries, not all tokens in a chain of binaries. If the 
				// next parse works, reassign expressionStart to the value saved here
				int nextExpressionStart = this.startIndex;

				// If we already parsed a binary, we must have a following primary, otherwise, rollback if we can
				if ( this.ParsePrimary() == false )
				{
					if ( this.ParseException != null )
						return false;

					if ( binaryAllowsRollback == false )
					{
						this.ParseException = new FormulaParseException( 
							binaryIndex, 
							this.formula, 
							SR.GetString( "LE_FormulaParseException_MissingArgumentAfterBinary" ), 
							this.formula.Substring( expressionStart, this.startIndex - expressionStart ) );

						return false;
					}

					this.RestorePosition( position );
					return true;
				}

				// The next expression starts at the primary that was just parsed 
				// (see comment above the declaration of nextExpressionStart).
				expressionStart = nextExpressionStart;
			}
		}

		#endregion ParseExpression

		#region ParsePrimary

		// Primary	:	Secondary ( UnaryL )*
		private bool ParsePrimary()
		{
			if ( this.ParseSecondary() == false )
				return false;

			while ( true ) // Parse as many unary operators at the end of the seocndary.
			{
				if ( this.ParseException != null )
					return false;

				PositionInfo position = this.SavePosition();

				if ( this.ParseUnaryL() == false )
				{
					this.RestorePosition( position );
					return true;
				}
			}
		}

		#endregion ParsePrimary

		#region ParseSecondary

		// Secondary	:	 Function | Boolean | Reference | Array | Error| Number | ( UnaryR Secondary ) | String | ( '(' Expression ')' ) | Array
		private bool ParseSecondary()
		{
			// Functions must be parsed before named references so the function name is not 
			// mistaken for a named reference
			if ( this.ParseFunction() )
				return true;

			// Boolean parameters must be parsed before named references so the boolean value is 
			// not mistaken for a named reference
			if ( this.ParseBoolean() )
				return true;

			// Reference (including named references) must be parse before numbers so the first portion of
			// a row range in A1 cell reference mode (for example: SUM( 7:10 ) ) is not mistaken for a number
			if ( this.ParseReference() )
				return true;

			if ( this.ParseError() )
				return true;

			// Numbers must be parsed before the ( UnaryR Secondary ) alternate of a Secondary so the string "-9"
			// can be parsed as a single number instead of a unary minus followed by a number.  In normal formulas,
			// both options are equivalent, but in constant arrays, the single number token is the only acceptable
			// option.
			if ( this.ParseNumber() )
				return true;

			int unaryIndex = this.startIndex;
			if ( this.ParseUnaryR() )
			{
				if ( this.ParseSecondary() )
					return true;

				// MD 7/9/08 - BR34633
				// Wrapped this set of the ParseException in an if statement. We should only set the exception if it
				// hasn't already been set. But if the expection occurred when ParseSecondary was called above, we want
				// to use that exception because it will have better information about the error than the message that
				// would have been used below.
				if ( this.ParseException == null )
				{
					this.ParseException = new FormulaParseException( 
						unaryIndex, 
						this.formula, 
						SR.GetString( "LE_FormulaParseException_MissingArgumentAfterUnary" ), 
						this.formula.Substring( unaryIndex, this.startIndex - unaryIndex ) );
				}

				return false;
			}

			if ( this.ParseString() )
				return true;

			int parenIndex = this.startIndex;
			if ( this.ParseOpenParen() )
			{
				if ( this.ParseExpression( true ) )
				{
					if ( this.ParseCloseParen() )
						return true;

					this.ParseException = new FormulaParseException( 
						parenIndex, 
						this.formula, 
						SR.GetString( "LE_FormulaParseException_UnmatchedOpenParen" ), 
						this.formula.Substring( parenIndex, this.startIndex - parenIndex ) );
				}

				if ( this.ParseException == null )
				{
					this.ParseException = new FormulaParseException( 
						parenIndex, 
						this.formula, 
						SR.GetString( "LE_FormulaParseException_MissingArgumentAfterParen" ), 
						this.formula.Substring( parenIndex, this.startIndex - parenIndex ) );
				}

				return false;
			}

			if ( this.ParseArray() )
				return true;

			return false;
		}

		#endregion ParseSecondary


		#region ParseArray

		// Array	:	'{' Matrix '}
		private bool ParseArray()
		{
			if ( this.ParseException != null )
				return false;

			int originalTokenCount = this.infixTokenList.Count;

			string whitespaceBeforeOpenBracket = this.nextWhitespace;
			int openBracketIndex = this.startIndex;

			if ( this.TestCurrentCharAndAdvance( '{' ) == false )
				return false;

			this.ParseNextWhitespace();

			if ( this.ParseMatrix( openBracketIndex ) == false )
			{
				Debug.Assert( this.ParseException != null, "If there was no matrix, there should have been an exception" );
				return false;
			}

			if ( this.TestCurrentCharAndAdvance( '}' ) == false )
			{
				int closingBracketIndex = this.formula.IndexOf( '}', this.startIndex );

				if ( closingBracketIndex >= 0 )
				{
					this.ParseException = new FormulaParseException(
						openBracketIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_ArrayContainsConstants" ),
						this.formula.Substring( openBracketIndex, closingBracketIndex - openBracketIndex + 1 ) );
				}
				else
				{
					this.ParseException = new FormulaParseException(
						openBracketIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_UnmatchedOpenBracket" ),
						this.formula.Substring( openBracketIndex, this.startIndex - openBracketIndex ) );
				}

				return false;
			}

			// Calculate the number of columns in the first row of the constant array
			int firstRowColumnCount = 0;
			for ( int i = originalTokenCount; i < this.infixTokenList.Count; i++ )
			{
				if ( this.infixTokenList[ i ] is MatrixRowSeparatorToken )
					break;

				if ( ( this.infixTokenList[ i ] is UnionOperator ) == false )
					firstRowColumnCount++;
			}

			if ( firstRowColumnCount == 0 )
			{
				this.ParseException = new FormulaParseException( 
					openBracketIndex, 
					this.formula,
					SR.GetString( "LE_FormulaParseException_ArrayHasEmptyFirstRow" ),
					this.formula.Substring( openBracketIndex, this.startIndex - openBracketIndex ) );

				return false;
			}

			int totalElements = this.infixTokenList.Count - originalTokenCount;
			totalElements -= ( totalElements / 2 );

			FormulaParseException misalignedException = new FormulaParseException( 
				openBracketIndex, 
				this.formula,
				SR.GetString( "LE_FormulaParseException_ArrayHasMisalignedRows" ),
				this.formula.Substring( openBracketIndex, this.startIndex - openBracketIndex ) );

			int rowCount = totalElements / firstRowColumnCount;
			Constant[][] arrayElements = new Constant[ firstRowColumnCount ][];

			for ( int x = 0; x < firstRowColumnCount; x++ )
				arrayElements[ x ] = new Constant[ rowCount ];

			int operatorIndex = originalTokenCount;

			for ( int y = 0; y < rowCount; y++ )
			{
				for ( int x = 0; x < firstRowColumnCount; x++ )
				{
					FormulaToken currentOperator = this.infixTokenList[ operatorIndex ];

					NumberToken numberToken = currentOperator as NumberToken;
					IntToken intToken = currentOperator as IntToken;
					StrToken strToken = currentOperator as StrToken;
					BoolToken boolToken = currentOperator as BoolToken;
					ErrToken errToken = currentOperator as ErrToken;

					if ( numberToken != null )
						arrayElements[ x ][ y ] = new NumberConstant( numberToken.Value );
					else if ( intToken != null )
						arrayElements[ x ][ y ] = new NumberConstant( intToken.Value );
					else if ( strToken != null )
						arrayElements[ x ][ y ] = new StringConstant( strToken.Value );
					else if ( boolToken != null )
						arrayElements[ x ][ y ] = new BooleanConstant( boolToken.Value );
					else if ( errToken != null )
						arrayElements[ x ][ y ] = new ErrorConstant( errToken.Value );
					else
					{
						Utilities.DebugFail( "I didn't think it was possible to get in here." );
						this.ParseException = misalignedException;
						return false;
					}

					operatorIndex++;

					if ( x == firstRowColumnCount - 1 && y == rowCount - 1 )
						break;

					currentOperator = this.infixTokenList[ operatorIndex ];

					operatorIndex++;

					if ( x < firstRowColumnCount - 1 )
					{
						if ( ( currentOperator is UnionOperator ) == false )
						{
							this.ParseException = misalignedException;
							return false;
						}
					}
					else
					{
						if ( ( currentOperator is MatrixRowSeparatorToken ) == false )
						{
							this.ParseException = misalignedException;
							return false;
						}
					}
				}
			}

			if ( operatorIndex != this.infixTokenList.Count )
			{
				this.ParseException = misalignedException;
				return false;
			}


			this.infixTokenList.RemoveRange( originalTokenCount, this.infixTokenList.Count - originalTokenCount );

			this.AddTokenAndWhitespace(
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//new ArrayToken( this.formulaInstance, arrayElements ), 
				new ArrayToken(arrayElements), 
				openBracketIndex, 
				this.startIndex - openBracketIndex, 
				whitespaceBeforeOpenBracket,
				WhitespaceType.SpacesBeforeNextToken );

			return true;
		}

		#endregion ParseArray

		#region ParseBinary

		// Binary			:	ArithmeticOp | ComparisonOp | '&' | ReferenceOp
		// ArithmeticOp		:	'+' | '-' | '*' | '/' | '^'
		// ComparisonOp		:	'=' | '<>' | '<' | '>' | '<=' | '>='
		// ReferenceOp		:	':' | ' ' | ',' // The ',' is only a binary operator when not directly (non-nested) in a function.  Otherwise, it is a parameter separator
		private bool ParseBinary( bool allowListOperator, out bool binaryAllowsRollback )
		{
			binaryAllowsRollback = false;

			if ( this.ParseException != null )
				return false;

			Match match = BinaryRegex.Match( this.formula, this.startIndex );

			bool foundBinary = match.Success;

			if ( foundBinary == false && this.nextWhitespace.Length == 0 )
				return false;

			string binaryValue = match.Value;

			// MD 9/11/09 - TFS20376
			// The UnionOperator can be different depending on the culture.
			//if ( allowListOperator == false && binaryValue == FormulaParser.UnionOperator )
			if ( allowListOperator == false && binaryValue == this.unionOperatorResolved )
				return false;

			if ( foundBinary == false ) // Its all white space: Intersection operator
			{
				int spaceIndex;
				for ( spaceIndex = this.nextWhitespace.Length - 1; spaceIndex >= 0; spaceIndex-- )
				{
					if ( this.nextWhitespace[ spaceIndex ] == ' ' )
						break;
				}

				if ( spaceIndex == -1 )
					return false;

				binaryAllowsRollback = true;

				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//IsectOperator isectOperator = new IsectOperator( this.formulaInstance );
				IsectOperator isectOperator = new IsectOperator();

				this.startIndex -= this.nextWhitespace.Length;
				this.startIndex += spaceIndex;

				this.nextWhitespace = this.nextWhitespace.Substring( 0, spaceIndex );

				this.AddTokenAndWhitespace( 
					isectOperator, 
					1,
					WhitespaceType.SpacesBeforeNextToken );

				return true;
			}

			OperatorToken binaryOperator;

			switch ( binaryValue )
			{
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore, so I updated all constructor calls below to not take the formula.
				// I also changed to use the singleton instances of any classes where they are defined.

				case "+":
					binaryOperator = new AddOperator();
					break;
				case "-":
					binaryOperator = new SubOperator();
					break;
				case "*":
					binaryOperator = new MulOperator();
					break;
				case "/":
					binaryOperator = new DivOperator();
					break;
				case "^":
					binaryOperator = new PowerOperator();
					break;
				case "=":
					binaryOperator = new EQOperator();
					break;
				case "<>":
					binaryOperator = new NEOperator();
					break;
				case "<":
					binaryOperator = new LTOperator();
					break;
				case ">":
					binaryOperator = new GTOperator();
					break;
				case "<=":
					binaryOperator = new LEOperator();
					break;
				case ">=":
					binaryOperator = new GEOperator();
					break;
				case FormulaParser.ConcatOperator:
					binaryOperator = new ConcatOperator();
					break;
				case FormulaParser.RangeOperator:
					binaryOperator = new RangeOperator();
					break;
				// MD 9/11/09 - TFS20376
				// The UnionOperator can be different depending on the culture.
				//case FormulaParser.UnionOperator:
				//    binaryOperator = new UnionOperator( this.formulaInstance );
				//    break;
				default:
					{
						// MD 9/11/09 - TFS20376
						if ( binaryValue == this.unionOperatorResolved )
						{
							binaryOperator = new UnionOperator();
							break;
						}

						Utilities.DebugFail( "Unknown binary operator" );
						return false;
					}
			}

			this.AddTokenAndWhitespace( 
				binaryOperator, 
				match.Length, 
				WhitespaceType.SpacesBeforeNextToken );

			return true;
		}

		#endregion ParseBinary

		#region ParseBoolean

		// Boolean	:	"TRUE" | "FALSE"
		private bool ParseBoolean()
		{
			if ( this.ParseException != null )
				return false;

			int remainingLength = this.formula.Length - this.startIndex;
			if ( remainingLength < 4 )
				return false;

			bool value;
			int length;

			string subString = this.formula.Substring( this.startIndex, Math.Min( remainingLength, 5 ) );

			if ( subString.StartsWith( "TRUE", StringComparison.InvariantCultureIgnoreCase ) )
			{
				value = true;
				length = 4;
			}
			else if ( subString.StartsWith( "FALSE", StringComparison.InvariantCultureIgnoreCase ) )
			{
				value = false;
				length = 5;
			}
			else
				return false;

			this.AddTokenAndWhitespace(
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//new BoolToken( this.formulaInstance, value ), 
				new BoolToken(value), 
				length, 
				WhitespaceType.SpacesBeforeNextToken );

			return true;
		}

		#endregion ParseBoolean

		#region ParseCellAddress

		// CellAddress		:	( ColumnAddressA1 RowAddressA1 ) | ( RowAddressR1C1 ColumnAddressR1C1 )
		private CellAddress ParseCellAddress()
		{
			if ( this.ParseException != null )
				return null;

			short columnIndex;
			bool isColumnRelative;

			int rowIndex;
			bool isRowRelative;

			PositionInfo position = this.SavePosition();

			if ( this.cellReferenceMode == CellReferenceMode.A1 )
			{
				if ( this.ParseColumnAddressA1( out columnIndex, out isColumnRelative ) == false )
				{
					this.RestorePosition( position );
					return null;
				}

				if ( this.ParseRowAddressA1( out rowIndex, out isRowRelative ) == false )
				{
					this.RestorePosition( position );
					return null;
				}

				return new CellAddress( rowIndex, isRowRelative, columnIndex, isColumnRelative );
			}
			else
			{
				if ( this.ParseRowAddressR1C1( out rowIndex, out isRowRelative ) == false )
				{
					this.RestorePosition( position );
					return null;
				}

				if ( this.ParseColumnAddressR1C1( out columnIndex, out isColumnRelative ) == false )
				{
					this.RestorePosition( position );
					return null;
				}

				return new CellAddress( rowIndex, isRowRelative, columnIndex, isColumnRelative );
			}
		}

		#endregion ParseCellAddress

		#region ParseCellRange

		// CellRange		:	CellRangeA1 | CellRangeR1C1 
		private CellAddressRange ParseCellRange()
		{
			if ( this.ParseException != null )
				return null;

			PositionInfo position = this.SavePosition();

			CellAddress topLeft = this.ParseCellAddress();

			if ( topLeft == null )
			{
				this.RestorePosition( position );
				return null;
			}

			this.ParseNextWhitespace();
			if ( this.ParseException != null )
			{
				this.RestorePosition( position );
				return null;
			}

			if ( this.TestCurrentCharAndAdvance( FormulaParser.RangeOperator ) == false )
			{
				this.RestorePosition( position );
				return null;
			}

			this.ParseNextWhitespace();

			if ( this.ParseException != null )
			{
				this.RestorePosition( position );
				return null;
			}

			CellAddress bottomRight = this.ParseCellAddress();

			if ( bottomRight == null )
			{
				this.RestorePosition( position );
				return null;
			}

			return new CellAddressRange( topLeft, bottomRight );
		}

		#endregion ParseCellRange

		#region ParseCloseParen

		private bool ParseCloseParen()
		{
			if ( this.ParseException != null )
				return false;

			if ( this.TestCurrentChar( ')' ) == false )
				return false;

			// MD 10/22/10 - TFS36696
			// We don't need to store the formula on the token anymore.
			//this.AddTokenAndWhitespace( new ParenToken( this.formulaInstance ), 1, WhitespaceType.SpacesBeforeClosingParens );
			this.AddTokenAndWhitespace(new ParenToken(), 1, WhitespaceType.SpacesBeforeClosingParens);
			return true;
		}

		#endregion ParseCloseParen

		#region ParseColumnAddress

		// ColumnAddress	:	ColumnAddressA1 | ColumnAddressR1C1
		private bool ParseColumnAddress( out short columnIndex, out bool isRelative )
		{
			if ( this.cellReferenceMode == CellReferenceMode.A1 )
				return this.ParseColumnAddressA1( out columnIndex, out isRelative );
			else
				return this.ParseColumnAddressR1C1( out columnIndex, out isRelative );
		}

		#endregion ParseColumnAddress

		#region ParseColumnAddressA1

		// ColumnAddressA1	:	('$')? [A-Z|a-z]{1,3}
		private bool ParseColumnAddressA1( out short columnIndex, out bool isRelative )
		{
			columnIndex = 0;
			isRelative = false;

			if ( this.ParseException != null )
				return false;

			int length;
			// MD 7/9/08 - Excel 2007 Format
			//if ( FormulaParser.ParseColumnAddressA1( this.formula, this.startIndex, ref columnIndex, ref isRelative, out length ) == false )
			// MD 4/6/12 - TFS101506
			//if ( FormulaParser.ParseColumnAddressA1( this.formula, this.startIndex, this.currentFormat, ref columnIndex, ref isRelative, out length ) == false )
			if (FormulaParser.ParseColumnAddressA1(this.formula, this.startIndex, this.currentFormat, this.culture, ref columnIndex, ref isRelative, out length) == false)
				return false;

			this.startIndex += length;
			return true;
		}

		// ColumnAddressA1	:	('$')? [A-Z|a-z]{1,3}
		// MD 7/9/08 - Excel 2007 Format
		// MD 7/14/08 - Excel formula solving
		// Made internal
		//private static bool ParseColumnAddressA1( string formula, int index, ref short columnIndex, ref bool isRelative, out int matchLength )
		// MD 4/6/12 - TFS101506
		//internal static bool ParseColumnAddressA1( string formula, int index, WorkbookFormat currentFormat, ref short columnIndex, ref bool isRelative, out int matchLength )
		internal static bool ParseColumnAddressA1(string formula, int index, WorkbookFormat currentFormat, CultureInfo culture, ref short columnIndex, ref bool isRelative, out int matchLength)
		{
			matchLength = 0;

			Match match = ColumnAddressA1Regex.Match( formula, index );

			if ( match.Success == false )
				return false;

			string value = match.Value;
			isRelative = value[ 0 ] != '$';
			string address = isRelative ? value : value.Substring( 1 );

			// MD 4/6/12 - TFS101506
			//char[] columnLetters = address.ToUpper( CultureInfo.CurrentCulture ).ToCharArray();
			char[] columnLetters = address.ToUpper(culture).ToCharArray();

			int power = 1;

			columnIndex = (short)( columnLetters[ columnLetters.Length - 1 ] - 'A' );

			for ( int i = columnLetters.Length - 2; i >= 0; i-- )
			{
				char columnLetter = columnLetters[ i ];
				int baseNumber = (int)Math.Pow( 26, power );

				columnIndex += (short)( baseNumber * ( ( columnLetter - 'A' ) + 1 ) );

				power++;
			}

			// MD 7/2/08 - Excel 2007 Format
			//if ( columnIndex > CellAddress.MaxColumnIndex )
			if ( columnIndex > Workbook.GetMaxColumnCount( currentFormat ) - 1 )
				return false;

			matchLength = match.Length;
			return true;
		}

		#endregion ParseColumnAddressA1

		// MD 7/9/08 - Excel 2007 Format
		#region ParseColumnAddressIndexR1C1

		private static bool ParseColumnAddressIndexR1C1( string addressIndex, bool isRelative, WorkbookFormat currentFormat, out short columnIndex )
		{
			if ( Int16.TryParse( addressIndex, out columnIndex ) == false )
				return false;

			if ( isRelative == false )
			{
				columnIndex--;

				if ( columnIndex < 0 )
					return false;
			}

			// MD 7/9/08 - Excel 2007 Format
			// We should have always been verifying the upper bounds of the index.
			if ( Math.Abs( columnIndex ) > Workbook.GetMaxColumnCount( currentFormat ) - 1 )
				return false;

			return true;
		} 

		#endregion ParseColumnAddressIndexR1C1

		#region ParseColumnAddressR1C1

		// ColumnAddressR1C1:	'C' ( ( '[' [-|+]? [0-9]{1,6} ']' ) | ( [0-9]{1,6} ) )?
		private bool ParseColumnAddressR1C1( out short columnIndex, out bool isRelative )
		{
			columnIndex = 0;
			isRelative = false;

			if ( this.ParseException != null )
				return false;

			int length;
			// MD 7/9/08 - Excel 2007 Format
			//if ( FormulaParser.ParseColumnAddressR1C1( this.formula, this.startIndex, ref columnIndex, ref isRelative, out length ) == false )
			if ( FormulaParser.ParseColumnAddressR1C1( this.formula, this.startIndex, this.currentFormat, ref columnIndex, ref isRelative, out length ) == false )
				return false;

			this.startIndex += length;
			return true;
		}

		// ColumnAddressR1C1:	'C' ( ( '[' [-|+]? [0-9]{1,6} ']' ) | ( [0-9]{1,6} ) )?
		// MD 7/9/08 - Excel 2007 Format
		//private static bool ParseColumnAddressR1C1( string formula, int index, ref short columnIndex, ref bool isRelative, out int matchLength )
		// MD 8/22/08 - Excel formula solving
		//private static bool ParseColumnAddressR1C1( string formula, int index, WorkbookFormat currentFormat, ref short columnIndex, ref bool isRelative, out int matchLength )
		internal static bool ParseColumnAddressR1C1( string formula, int index, WorkbookFormat currentFormat, ref short columnIndex, ref bool isRelative, out int matchLength )
		{
			matchLength = 0;

			Match match = ColumnAddressR1C1Regex.Match( formula, index );

			if ( match.Success == false )
				return false;

			string value = match.Value;

			if ( value.Length == 1 )
			{
				isRelative = true;
				columnIndex = 0;
			}
			else
			{
				isRelative = value[ 1 ] == '[';
				string address = isRelative ? value.Substring( 2, value.Length - 3 ) : value.Substring( 1 );

				// MD 7/9/08 - Excel 2007 Format
				// Moved to ParseColumnAddressIndexR1C1 and change the logic so if the address parsing fails,
				// we will still return a valid address
				#region Moved

				
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


				#endregion Moved
				if ( FormulaParser.ParseColumnAddressIndexR1C1( address, isRelative, currentFormat, out columnIndex ) == false )
				{
					isRelative = true;
					columnIndex = 0;
					matchLength = 1;
					return true;
				}
			}

			matchLength = match.Length;
			return true;
		}

		#endregion ParseColumnAddressR1C1

		#region ParseColumnRange

		// ColumnRange		:	ColumnRangeA1 | ColumnRangeR1C1 
		private CellAddressRange ParseColumnRange()
		{
			if ( this.ParseException != null )
				return null;

			PositionInfo position = this.SavePosition();

			short columnIndex1;
			bool isRelative1;

			if ( this.ParseColumnAddress( out columnIndex1, out isRelative1 ) == false )
			{
				this.RestorePosition( position );
				return null;
			}

			if ( this.TestCurrentCharAndAdvance( FormulaParser.RangeOperator ) == false )
			{
				this.RestorePosition( position );
				return null;
			}

			short columnIndex2;
			bool isRelative2;

			if ( this.ParseColumnAddress( out columnIndex2, out isRelative2 ) == false )
			{
				this.RestorePosition( position );
				return null;
			}

			CellAddress topLeft;
			CellAddress bottomRight;

			// MD 7/2/08 - Excel 2007 Format
			int maxRowIndex = Workbook.GetMaxRowCount( this.currentFormat ) - 1;

			if ( columnIndex1 <= columnIndex2 )
			{
				topLeft = new CellAddress( 0, false, columnIndex1, isRelative1 );

				// MD 7/2/08 - Excel 2007 Format
				//bottomRight = new CellAddress( CellAddress.MaxRowIndex, false, columnIndex2, isRelative2 );
				bottomRight = new CellAddress( maxRowIndex, false, columnIndex2, isRelative2 );
			}
			else
			{
				topLeft = new CellAddress( 0, false, columnIndex2, isRelative2 );

				// MD 7/2/08 - Excel 2007 Format
				//bottomRight = new CellAddress( CellAddress.MaxRowIndex, false, columnIndex1, isRelative1 );
				bottomRight = new CellAddress( maxRowIndex, false, columnIndex1, isRelative1 );
			}

			return new CellAddressRange( topLeft, bottomRight );
		}

		#endregion ParseColumnRange

		#region ParseEmptyOrParameter

		// EmptyOrParameter	:	/*empty*/ | Expression
		private void ParseEmptyOrParameter()
		{
			if ( this.ParseException != null )
				return;

			if ( this.ParseExpression( false ) )
				return;

			this.AddTokenAndWhitespace(
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//new MissArgToken( this.formulaInstance ), 
				new MissArgToken(), 
				0,
				WhitespaceType.SpacesBeforeNextToken );
		}

		#endregion ParseEmptyOrParameter

		#region ParseError

		// Error	:	#NULL! | #DIV/0! | #VALUE! | #REF! | #NAME? | #NUM! | #N/A
		private bool ParseError()
		{
			// MD 9/17/08
			// Moved all code to the new overload
			return this.ParseError( false );
		}

		// MD 9/17/08
		// Added a parameter to indicate whether all errors are allowed or just reference errors.
		private bool ParseError( bool mustBeReferenceError )
		{
			if ( this.ParseException != null )
				return false;

			if ( this.TestCurrentChar( '#' ) == false )
				return false;

			Match match = ErrorRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
			{
				this.ParseException = new FormulaParseException( 
					this.startIndex, 
					this.formula,
					SR.GetString( "LE_FormulaParseException_InvalidErrorValue" ),
					this.formula.Substring( this.startIndex ) );

				return false;
			}

            // MBS 7/25/08 - Excel 2007 Format
            // Refactored into a helper method
            //
            #region Refactored

            //ErrorValue errorValue = null;

            //switch ( match.Value.ToUpper( CultureInfo.CurrentCulture ) )
            //{
            //    case "#NULL!":
            //        // MD 7/14/08 - Excel formula solving
            //        // Not sure how this happened, but the wrong error codes were used for some of the error text values.
            //        //errorValue = ErrorValue.ArgumentOrFunctionNotAvailable;
            //        errorValue = ErrorValue.EmptyCellRangeIntersection;
            //        break;
            //    case "#DIV/0!":
            //        errorValue = ErrorValue.DivisionByZero;
            //        break;
            //    case "#VALUE!":
            //        // MD 7/14/08 - Excel formula solving
            //        // Not sure how this happened, but the wrong error codes were used for some of the error text values.
            //        //errorValue = ErrorValue.EmptyCellRangeIntersection;
            //        errorValue = ErrorValue.WrongOperandType;
            //        break;
            //    case FormulaParser.ReferenceErrorValue:
            //        errorValue = ErrorValue.InvalidCellReference;
            //        break;
            //    case "#NAME?":
            //        errorValue = ErrorValue.WrongFunctionName;
            //        break;
            //    case "#NUM!":
            //        errorValue = ErrorValue.ValueRangeOverflow;
            //        break;
            //    case "#N/A":
            //        // MD 7/14/08 - Excel formula solving
            //        // Not sure how this happened, but the wrong error codes were used for some of the error text values.
            //        //errorValue = ErrorValue.WrongOperandType;
            //        errorValue = ErrorValue.ArgumentOrFunctionNotAvailable;
            //        break;
            //    default:
            //        Utilities.DebugFail( "Unknown error type" );
            //        return false;
            //}

            #endregion //Refactored
            //
			// MD 4/6/12 - TFS101506
            //ErrorValue errorValue = FormulaParser.ParseError(match.Value);
			ErrorValue errorValue = FormulaParser.ParseError(match.Value, this.culture);
            if (errorValue == null)
                return false;

			// MD 9/17/08
			// If only reference errors are allowed, and the error is not a reference error, return.
			if ( mustBeReferenceError && errorValue != ErrorValue.InvalidCellReference )
				return false;

            this.AddTokenAndWhitespace(
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//new ErrToken( this.formulaInstance, errorValue ),
				new ErrToken(errorValue),
				match.Length,
				WhitespaceType.SpacesBeforeNextToken );

			return true;
		}

		#endregion ParseError

		#region ParseFileNameBracketedQuoted

		// FileNameBracketedQuoted	:	FilePath? '[' FileNameQuoted ']'
		private string ParseFileNameBracketedQuoted()
		{
			int referenceStartIndex = this.startIndex;

			string directoryPath = this.ParseFilePath();

			if ( this.TestCurrentCharAndAdvance( '[' ) == false )
			{
				this.startIndex = referenceStartIndex;
				return null;
			}

			string fileNameQuoted = this.ParseFileNameQuoted();

			if ( fileNameQuoted == null )
			{
				int closingBracketIndex = this.formula.IndexOf( ']', this.startIndex );

				if ( closingBracketIndex >= 0 )
				{
					this.ParseException = new FormulaParseException(
						this.startIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_InvalidFileNameInBrackets" ),
						this.formula.Substring( referenceStartIndex, closingBracketIndex - referenceStartIndex + 1 ) );
				}
				else
				{
					this.ParseException = new FormulaParseException(
						this.startIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_NoFileNameAfterBracket" ),
						this.formula.Substring( referenceStartIndex ) );
				}

				return null;
			}

			if ( this.TestCurrentCharAndAdvance( ']' ) == false )
			{
				int closingBracketIndex = this.formula.IndexOf( ']', this.startIndex );

				if ( closingBracketIndex >= 0 )
				{
					this.ParseException = new FormulaParseException(
						this.startIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_InvalidFileNameInBrackets" ),
						this.formula.Substring( referenceStartIndex, closingBracketIndex - referenceStartIndex + 1 ) );
				}
				else
				{
					this.ParseException = new FormulaParseException(
						this.startIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_UnmatchedOpenSquareBracket" ),
						this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );
				}

				return null;
			}

			string filePath = String.Concat( directoryPath, fileNameQuoted );

			// MD 6/18/12 - TFS102878
			// If the file name is an index into the workbook references collection, get the resolved file name.
			if (this.indexedReferencesDuringLoad != null)
			{
				int index;
				if (Int32.TryParse(filePath, out index) && index < this.indexedReferencesDuringLoad.Count)
					filePath = this.indexedReferencesDuringLoad[index].FileName;
			}

			if ( String.IsNullOrEmpty( filePath ) )
			{
				Utilities.DebugFail( "I didn't think this could happen." );
				filePath = null;
			}

			return filePath;
		}

		#endregion ParseFileNameBracketedQuoted

		#region ParseFileNameBracketedSafe

		// FileNameBracketedSafe	:	'[' [_0-9A-Za-z����] [_0-9A-Za-z����.]* ']'
		// MD 2/23/12 - TFS101504
		// Added an out parameter to indicate whether the bracketed file name was an indexed reference, which is how external
		// links are saved in a formula (such as "=[0]Sheet1!A1").
		//private string ParseFileNameBracketedSafe()
		private string ParseFileNameBracketedSafe(out bool wasIndexedReference)
		{
			// MD 2/23/12 - TFS101504
			wasIndexedReference = false;

			if ( this.ParseException != null )
				return null;

			Match match = FileNameBracketedSafeRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
				return null;

			// MD 9/16/08
			// Found while unit testing
			// We need to strip the brackets off the match value
			//string fileName = match.Value;
			string fileName = match.Value.Substring( 1, match.Length - 2 );

			// MD 2/23/12 - TFS101504
			// If there are indexed references specified, try to parse the bracketed expression into an index
			// and get the reference at that index.
			if (this.indexedReferencesDuringLoad != null)
			{
				int index;
				if (Int32.TryParse(fileName, out index))
				{
					if (index < this.indexedReferencesDuringLoad.Count)
					{
						wasIndexedReference = true;
						fileName = this.indexedReferencesDuringLoad[index].FileName;
					}
					// MD 4/4/12 - TFS100966 
					// This is allowed now.
					//else
					//{
					//    Utilities.DebugFail("This is unexpected.");
					//}
				}
			}

			this.startIndex += match.Length;

			return fileName;
		}

		#endregion ParseFileNameBracketedSafe

		#region ParseFileNameQuoted

		// FileNameQuoted	:	( [^\/:*?"<>|[]'] | ''' ''' )+
		private string ParseFileNameQuoted()
		{
			if ( this.ParseException != null )
				return null;

			Match match = FileNameQuotedRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
				return null;

			string fileName = match.Value;
			fileName = fileName.Replace( "''", "'" );

			this.startIndex += match.Length;

			return fileName;
		}

		#endregion ParseFileNameQuoted

		// MD 9/16/08
		// Found while unit testing
		// It turns out this will never be used
		#region Not used

		
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


		#endregion Not used

		#region ParseFilePath

		// FilePath	:	[a-zA-z]:\ ( ( [^\/:*?"<>|[]'] | ''' ''' )* \ )*
		private string ParseFilePath()
		{
			if ( this.ParseException != null )
				return null;

			string filePath = this.ParseLocalPath();

			if ( filePath != null )
				return filePath;

			return this.ParseUrlPath();
		}

		#endregion ParseFilePath

		#region ParseFunction

		// Function	:	FunctionName ( ParameterList )? ')'
		private bool ParseFunction()
		{
			if ( this.ParseException != null )
				return false;

			int functionNameStart = this.startIndex;

			string whitespaceBeforeFunctionName = this.nextWhitespace;
			string functionName = this.ParseFunctionName();

			if ( functionName != null )
			{
				// MD 4/6/12 - TFS102169
				// Moved most of this code to ParseFunctionHelper so we can also use it from ParseReference when an external 
				// function is parsed.
				#region Moved

				//int functionTokenIndex = this.infixTokenList.Count;

				//this.functionNesting++;

				//int parameterCount = this.ParseParameterList();

				//// MBS 7/10/08 - Excel 2007 Format
				////bool nestingLevelTooDeep = this.functionNesting > FormulaParser.MaxFunctionNesting;
				//bool nestingLevelTooDeep = this.functionNesting > this.MaxFunctionNesting;

				//this.functionNesting--;

				//if ( this.ParseException != null )
				//    return false;

				//if ( this.ParseCloseParen() == false )
				//{
				//    this.ParseException = new FormulaParseException( 
				//        functionNameStart, 
				//        this.formula,
				//        SR.GetString( "LE_FormulaParseException_FunctionMissingClosingParen" ),
				//         this.formula.Substring( functionNameStart, this.startIndex - functionNameStart ) );

				//    return false;
				//}

				//if ( nestingLevelTooDeep )
				//{
				//    this.ParseException = new FormulaParseException(
				//        functionNameStart,
				//        this.formula,
				//        SR.GetString( "LE_FormulaParseException_FunctionNestingTooDeep" ),
				//         this.formula.Substring( functionNameStart, this.startIndex - functionNameStart ) );

				//    return false;
				//}

				//Function function = Function.GetFunction( functionName );

				//if ( function == null )
				//{
				//    // MD 8/4/08
				//    // Found while implementing Excel formula solving.
				//    // They are treated like add in functions.
				//    // It is not an error to use an unknown function name. 
				//    //this.ParseException = new FormulaParseException(
				//    //    functionNameStart,
				//    //    this.formula,
				//    //    SR.GetString( "LE_FormulaParseException_UnknownFunction" ),
				//    //    this.formula.Substring( functionNameStart, this.startIndex - functionNameStart ) );
				//    //
				//    //return false;
				//    function = Function.GetUnknownAddInFunction( functionName );
				//}

				//// MD 10/9/07 - BR27172
				//// Some functions need to mave parameters in multiples or there is an error.
				//// Make sure we have the right multiples.
				////if ( parameterCount < function.MinParams || function.MaxParams < parameterCount )
				//if ( 
				//    parameterCount < function.MinParams || 
				//    // MBS 7/10/08 - Excel 2007 Support
				//    // Changed to use a method to support the current format 
				//    //function.MaxParams < parameterCount ||
				//    function.GetMaxParams(this.currentFormat) < parameterCount ||
				//    ( ( parameterCount - function.TuplesStart ) % function.TuplesDegree ) != 0 )
				//{
				//    this.ParseException = new FormulaParseException(
				//        functionNameStart,
				//        this.formula,
				//        SR.GetString( "LE_FormulaParseException_IncorrectNumberOfArguments" ),
				//         this.formula.Substring( functionNameStart, this.startIndex - functionNameStart ) );

				//    return false;
				//}

				//if ( this.isVolitile == false && function.IsVolatile )
				//    this.isVolitile = true;

				//FunctionOperator functionOperator;

				//// MD 10/9/07 - BR27172
				//// Some functions should still be stored at FunctionVOperators even if the min and max arguments
				//// are equal. Added new conditions and moved the logic to a porperty on Function called IsFuncV.
				////if ( function.MinParams == function.MaxParams )
				////    functionOperator = new FunctionOperator( this.formulaInstance, function );
				////else
				////{
				////    functionOperator = new FunctionVOperator( this.formulaInstance, function, function.MaxParams );
				////    functionOperator.NumberOfArguments = (byte)parameterCount;
				////}
				//if ( function.IsFuncV )
				//{
				//    // MBS 7/10/08 - Excel 2007 Format
				//    //functionOperator = new FunctionVOperator( this.formulaInstance, function, function.MaxParams );
				//    // MD 10/22/10 - TFS36696
				//    // We don't need to store the formula on the token anymore.
				//    //functionOperator = new FunctionVOperator(this.formulaInstance, function, function.GetMaxParams(this.currentFormat));
				//    functionOperator = new FunctionVOperator(function, function.GetMaxParams(this.currentFormat));

				//    functionOperator.NumberOfArguments = (byte)parameterCount;
				//}
				//else
				//{
				//    // MD 10/22/10 - TFS36696
				//    // We don't need to store the formula on the token anymore.
				//    //functionOperator = new FunctionOperator( this.formulaInstance, function );
				//    functionOperator = new FunctionOperator(function);
				//}

				//int index = this.startIndex;

				//// MD 7/17/08
				//// Found while implementing Excel formula solving
				//// If there was whitespace after the function, we were losing it. We were already parsing and storing the whitespace after 
				//// parsing the closing paren. If will be overwritten with blank whitespace by the call to InsertTokenAndWhitespace below,
				//// so we have to cache it here to restore it later. This normally wouldn't be a big deal, but in the case where this came 
				//// up, there was an intersection operator (' ') directly after the function which was getting lost.
				//string whiteSpace = this.nextWhitespace;

				//this.SavePosition();

				//this.InsertTokenAndWhitespace(
				//    functionTokenIndex,
				//    functionOperator,
				//    functionNameStart,
				//    functionName.Length + 1,
				//    whitespaceBeforeFunctionName,
				//    WhitespaceType.SpacesBeforeNextToken );

				//this.startIndex = index;

				//// MD 7/17/08
				//// Found while implementing Excel formula solving
				//// Restore the whitespace cached above.
				//this.nextWhitespace = whiteSpace;

				//return true;

				#endregion // Moved
				Function function = Function.GetFunction(functionName);

				if (function == null)
					function = Function.GetUnknownAddInFunction(functionName);

				if (this.ParseFunctionHelper(function, functionNameStart, whitespaceBeforeFunctionName))
					return true;
			}

			return false;
		}

		// MD 4/6/12 - TFS102169
		// Moved most of this code from ParseFunction so we can also use it from ParseReference when an external function
		// is parsed.
		private bool ParseFunctionHelper(Function function, int functionNameStart, string whitespaceBeforeFunctionName)
		{
			int functionNameEnd = this.startIndex;

			int functionTokenIndex = this.infixTokenList.Count;

			this.functionNesting++;

			int parameterCount = this.ParseParameterList();

			// MBS 7/10/08 - Excel 2007 Format
			//bool nestingLevelTooDeep = this.functionNesting > FormulaParser.MaxFunctionNesting;
			bool nestingLevelTooDeep = this.functionNesting > this.MaxFunctionNesting;

			this.functionNesting--;

			if (this.ParseException != null)
				return false;

			if (this.ParseCloseParen() == false)
			{
				this.ParseException = new FormulaParseException(
					functionNameStart,
					this.formula,
					SR.GetString("LE_FormulaParseException_FunctionMissingClosingParen"),
					 this.formula.Substring(functionNameStart, this.startIndex - functionNameStart));

				return false;
			}

			if (nestingLevelTooDeep)
			{
				this.ParseException = new FormulaParseException(
					functionNameStart,
					this.formula,
					SR.GetString("LE_FormulaParseException_FunctionNestingTooDeep"),
					 this.formula.Substring(functionNameStart, this.startIndex - functionNameStart));

				return false;
			}

			// MD 10/9/07 - BR27172
			// Some functions need to mave parameters in multiples or there is an error.
			// Make sure we have the right multiples.
			//if ( parameterCount < function.MinParams || function.MaxParams < parameterCount )
			if (
				parameterCount < function.MinParams ||
				// MBS 7/10/08 - Excel 2007 Support
				// Changed to use a method to support the current format 
				//function.MaxParams < parameterCount ||
				function.GetMaxParams(this.currentFormat) < parameterCount ||
				((parameterCount - function.TuplesStart) % function.TuplesDegree) != 0)
			{
				this.ParseException = new FormulaParseException(
					functionNameStart,
					this.formula,
					SR.GetString("LE_FormulaParseException_IncorrectNumberOfArguments"),
					 this.formula.Substring(functionNameStart, this.startIndex - functionNameStart));

				return false;
			}

			if (this.isVolitile == false && function.IsVolatile)
				this.isVolitile = true;

			FunctionOperator functionOperator;

			// MD 10/9/07 - BR27172
			// Some functions should still be stored at FunctionVOperators even if the min and max arguments
			// are equal. Added new conditions and moved the logic to a porperty on Function called IsFuncV.
			//if ( function.MinParams == function.MaxParams )
			//    functionOperator = new FunctionOperator( this.formulaInstance, function );
			//else
			//{
			//    functionOperator = new FunctionVOperator( this.formulaInstance, function, function.MaxParams );
			//    functionOperator.NumberOfArguments = (byte)parameterCount;
			//}
			if (function.IsFuncV)
			{
				// MBS 7/10/08 - Excel 2007 Format
				//functionOperator = new FunctionVOperator( this.formulaInstance, function, function.MaxParams );
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//functionOperator = new FunctionVOperator(this.formulaInstance, function, function.GetMaxParams(this.currentFormat));
				functionOperator = new FunctionVOperator(function, function.GetMaxParams(this.currentFormat));

				functionOperator.NumberOfArguments = (byte)parameterCount;
			}
			else
			{
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//functionOperator = new FunctionOperator( this.formulaInstance, function );
				functionOperator = new FunctionOperator(function);
			}

			int index = this.startIndex;

			// MD 7/17/08
			// Found while implementing Excel formula solving
			// If there was whitespace after the function, we were losing it. We were already parsing and storing the whitespace after 
			// parsing the closing paren. If will be overwritten with blank whitespace by the call to InsertTokenAndWhitespace below,
			// so we have to cache it here to restore it later. This normally wouldn't be a big deal, but in the case where this came 
			// up, there was an intersection operator (' ') directly after the function which was getting lost.
			string whiteSpace = this.nextWhitespace;

			this.SavePosition();

			this.InsertTokenAndWhitespace(
				functionTokenIndex,
				functionOperator,
				functionNameStart,
				functionNameEnd - functionNameStart + 1,
				whitespaceBeforeFunctionName,
				WhitespaceType.SpacesBeforeNextToken);

			this.startIndex = index;

			// MD 7/17/08
			// Found while implementing Excel formula solving
			// Restore the whitespace cached above.
			this.nextWhitespace = whiteSpace;

			return true;
		}

		#endregion ParseFunction

		#region ParseFunctionName

		// MD 10/9/07 - BR27172
		// Periods are allowed in function named
		//// FunctionName		:	[A-Z|a-z] [A-Z|a-z|0-9]* '('
		// FunctionName		:	[A-Z|a-z] (.?[A-Z|a-z|0-9|.])* '('
		private string ParseFunctionName()
		{
			Match functionNameMatch = FunctionNameRegex.Match( this.formula, this.startIndex );

			if ( functionNameMatch.Success == false )
				return null;

			// MD 2/4/11 - TFS65015
			// In the 2007 format, add-in function names are prefaced by "_xll.", so this now allowed. 
			// However, if we see this in the 2003 format, it is not a valid function name.
			if (Utilities.Is2003Format(this.currentFormat) &&
				functionNameMatch.Value.StartsWith("_xll."))
			{
				return null;
			}

			this.startIndex += functionNameMatch.Length + 1;
			this.ParseNextWhitespace();

			return functionNameMatch.Value;
		}

		#endregion ParseFunctionName

		#region ParseLocalPath

		// ParseLocalPath	:	[a-zA-z]:\ ( ( [^\/:*?"<>|[]'] | ''' ''' )* \ )*
		private string ParseLocalPath()
		{
			if ( this.ParseException != null )
				return null;

			Match match = LocalPathRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
				return null;

			string localPath = match.Value;
			localPath = localPath.Replace( "''", "'" );

			this.startIndex += match.Length;

			return localPath;
		}

		#endregion ParseLocalPath

		#region ParseMatrix

		// Matrix	:	MatrixRow ( MatrixRowSeparator MatrixRow )*
		private bool ParseMatrix( int arrayStartIndex )
		{
			if ( this.ParseMatrixRow( arrayStartIndex ) == false )
				return false;

			while ( true )
			{
				PositionInfo position = this.SavePosition();

				if ( this.ParseMatrixRowSeparator() == false )
				{
					this.RestorePosition( position );
					return true;
				}

				if ( this.ParseMatrixRow( arrayStartIndex ) == false )
					return false;
			}
		}

		#endregion ParseMatrix

		#region ParseMatrixElement

		// MatrixElement	:	Number | String | Error | Boolean
		private bool ParseMatrixElement()
		{
			if ( this.ParseNumber() )
				return true;

			if ( this.ParseString() )
				return true;

			if ( this.ParseError() )
				return true;

			if ( this.ParseBoolean() )
				return true;

			return false;
		}

		#endregion ParseMatrixElement

		#region ParseMatrixRow

		// MatrixRow	:	MatrixElement ( Separator MatrixElement )*
		private bool ParseMatrixRow( int arrayStartIndex )
		{
			if ( this.ParseMatrixElement() == false )
				return true;

			while ( true )
			{
				PositionInfo position = this.SavePosition();
				
				int separatorIndex = this.startIndex;

				// MD 9/11/09 - TFS20376
				// The matrix value separator can be different depending on the culture and it may not be the same 
				// as the list separator.
				//if ( this.ParseSeparator() == false )
				if ( this.ParseSeparator( this.matrixValueSeparatorResolved ) == false )
				{
					this.RestorePosition( position );
					return true;
				}

				if ( this.ParseMatrixElement() == false )
				{
					this.ParseException = new FormulaParseException( 
						separatorIndex, 
						this.formula,
						SR.GetString( "LE_FormulaParseException_NoElementAfterArraySerapator" ),
						this.formula.Substring( arrayStartIndex, this.startIndex - arrayStartIndex ) );

					return false;
				}
			}
		}

		#endregion ParseMatrixRow

		#region ParseMatrixRowSeparator

		// MatrixRowSeparator	:	';'
		private bool ParseMatrixRowSeparator()
		{
			if ( this.ParseException != null )
				return false;

			if ( this.TestCurrentChar( ';' ) == false )
				return false;

			// MD 10/22/10 - TFS36696
			// We don't need to store the formula on the token anymore.
			//this.AddToken( new MatrixRowSeparatorToken( this.formulaInstance ), 1 );
			this.AddToken(new MatrixRowSeparatorToken(), 1);
			return true;
		}

		#endregion ParseMatrixRowSeparator

		#region ParseName

		// Name				:	[\_����a-ZA-Z][\_����a-zA-Z0-9]*		// This cannot be CellAddressA1 or start with ColumnAddressR1C1 or RowAddressR1C1
		private string ParseName()
		{
			if ( this.ParseException != null )
				return null;

			Match match = NameRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
				return null;

			string name = match.Value;

			// MD 7/9/08 - Excel 2007 Format
			//if ( FormulaParser.IsValidNamedReference( name ) == false )
			// MD 4/6/12 - TFS101506
			//if ( FormulaParser.IsValidNamedReference( name, this.currentFormat ) == false )
			if (FormulaParser.IsValidNamedReference(name, this.currentFormat, this.culture) == false)
				return null;

			startIndex += match.Length;

			// MD 5/26/11 - TFS76314
			// Built in named references have a specific prefix in formulas which shouldn't be displayed.
			if (name.StartsWith(NamedReference.BuildInNamePrefixFor2007) && Utilities.Is2007Format(this.currentFormat))
				name = name.Substring(NamedReferenceBase.BuildInNamePrefixFor2007.Length);

			return name;
		}

		#endregion ParseName

		#region ParseNextWhitespace

		private void ParseNextWhitespace()
		{
			this.nextWhitespace = WhitespaceRegex.Match( this.formula, this.startIndex ).Value;
			this.startIndex += this.nextWhitespace.Length;
		}

		#endregion ParseNextWhitespace

		#region ParseNumber

		// Number	:	( ([0-9]+( '.'[0-9]+ )?) | ('.'[0-9]+) ) ([eE][-+]?[0-9]+)?
		private bool ParseNumber()
		{
			if ( this.ParseException != null )
				return false;

			Match match = NumberRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
				return false;

			double number;

			// MD 10/8/10 - TFS44359
			// We need to use the culture passed into the parser when passing the number.
			//if ( Double.TryParse( match.Value, out number ) == false )
			// MD 5/13/11 - Data Validations / Page Breaks
			// Moved this code to a helper method.
			//if (Double.TryParse(match.Value, NumberStyles.Float | NumberStyles.AllowThousands, this.culture, out number) == false)
			// MD 4/9/12 - TFS101506
			//if (FormulaParser.TryParseNumber(match.Value, this.culture, out number) == false)
			if (MathUtilities.DoubleTryParse(match.Value, this.culture, out number) == false)
				return false;

			this.AddTokenAndWhitespace( 
				this.CreateNumberOperator( number ), 
				match.Length,
				WhitespaceType.SpacesBeforeNextToken );

			return true;
		}

		#endregion ParseNumber

		#region ParseOpenParen

		private bool ParseOpenParen()
		{
			if ( this.ParseException != null )
				return false;

			if ( this.TestCurrentChar( '(' ) == false )
				return false;

			// MD 10/22/10 - TFS36696
			// We don't need to store the formula on the token anymore.
			//this.AddTokenAndWhitespace( new OpenParenOperator( this.formulaInstance ), 1, WhitespaceType.SpacesBeforeOpeningParens );
			this.AddTokenAndWhitespace(new OpenParenOperator(), 1, WhitespaceType.SpacesBeforeOpeningParens);
			return true;
		}

		#endregion ParseOpenParen

		#region ParseParameterList

		// ParameterList	:	EmptyOrParameter ( Separator EmptyOrParameter )*
		private int ParseParameterList()
		{
			PositionInfo position = this.SavePosition();

			this.ParseEmptyOrParameter();
			int parameterCount = 1;

			while ( true )
			{
				PositionInfo nextPosition = this.SavePosition();

				// MD 9/11/09 - TFS20376
				// The list separator can be different depending on the culture and it may not be the same 
				// as the matrix value separator.
				//if ( this.ParseSeparator() == false )
				if ( this.ParseSeparator( this.unionOperatorResolved ) == false )
				{
					this.RestorePosition( nextPosition );
					break;
				}

				// If we got a valid separator, we must have a valid parameter after
				this.ParseEmptyOrParameter();
				parameterCount++;
			}

			// If there is only an empty parameter in the list, there are really no parameters
			if ( parameterCount == 1 && this.infixTokenList[ this.infixTokenList.Count - 1 ] is MissArgToken )
			{
				this.RestorePosition( position );
				return 0;
			}

			return parameterCount;
		}

		#endregion ParseParameterList

		#region ParseReference

		// Reference	:	( WorksheetReference? ( CellRange | ColumnRange | RowRange | CellAddress | Name | RowAddressR1C1 | ColumnAddressR1C1 ) ) | ( WorkbookReference Name )
		private bool ParseReference()
		{
			if ( this.ParseException != null )
				return false;

			int referenceStartIndex = this.startIndex;
			string whitespaceBeforeReference = this.nextWhitespace;

			string filePath;
			string firstWorksheetName;
			string lastWorksheetName;
			bool is3dReference = this.ParseWorksheetReference(out filePath, out firstWorksheetName, out lastWorksheetName);

			// MD 4/6/12 - TFS102169
			// This is no longer valid now that we can parsed bracketed external workbook indexes in the function.
			//Debug.Assert( is3dReference == false || worksheetName != null );

			if ( this.ParseException != null )
				return false;

			// MD 4/6/12 - TFS102169
			// Wrapped in an if. We can only parse cell, row, or column references if this is a local reference or we 
			// have a worksheet name.
			CellAddressRange range;

			if (is3dReference == false || firstWorksheetName != null)
			{
			#region Try to parse a cell, row, or column range

			// MD 4/6/12 - TFS102169
			// Moved the definition of range outside of the if block.
			//CellAddressRange range = this.ParseCellRange();
			range = this.ParseCellRange();

			if ( range == null )
				range = this.ParseColumnRange();

			if ( range == null )
				range = this.ParseRowRange();

			if ( range != null )
			{
				bool needsNTypeToken =
					this.cellReferenceMode == CellReferenceMode.R1C1 &&
					range.HasRelativeAddresses;

				AreaToken areaToken;

				if (is3dReference)
				{
					WorksheetReference worksheetReference = this.GetWorksheetReference(filePath, firstWorksheetName, lastWorksheetName);
					if (needsNTypeToken)
						areaToken = new Area3DNToken(worksheetReference, range);
					else
						areaToken = new Area3DToken(worksheetReference, range);
				}
				else
				{
					if (needsNTypeToken)
						areaToken = new AreaNToken(range);
					else
						areaToken = new AreaToken(range);
				}

				this.AddTokenAndWhitespace(
					areaToken,
					referenceStartIndex,
					this.startIndex - referenceStartIndex,
					whitespaceBeforeReference,
					WhitespaceType.SpacesBeforeNextToken );

				return true;
			}

			#endregion Try to parse a cell, row, or column range

			#region Try to parse a cell address

			CellAddress cellAddress = this.ParseCellAddress();

			if ( cellAddress != null )
			{
				bool needsNTypeToken =
					this.cellReferenceMode == CellReferenceMode.R1C1 &&
					cellAddress.HasRelativeAddresses;

				RefToken refToken;

				if (is3dReference)
				{
					WorksheetReference worksheetReference = this.GetWorksheetReference(filePath, firstWorksheetName, lastWorksheetName);
					if (needsNTypeToken)
						refToken = new Ref3DNToken(worksheetReference, cellAddress);
					else
						refToken = new Ref3DToken(worksheetReference, cellAddress);
				}
				else
				{
					if (needsNTypeToken)
						refToken = new RefNToken(cellAddress);
					else
						refToken = new RefToken(cellAddress);
				}

				this.AddTokenAndWhitespace(
					refToken,
					referenceStartIndex,
					this.startIndex - referenceStartIndex,
					whitespaceBeforeReference,
					WhitespaceType.SpacesBeforeNextToken );

				return true;
			}

			#endregion Try to parse a cell address
			}

			#region Try to parse a local named reference

			string name = this.ParseName();

			if ( name != null )
			{
				// MD 4/6/12 - TFS102169
				// If this is an external named reference and it is followed by a '(', it is actually an external function, so try to
				// parse the function list.
				if (filePath != null && this.TestCurrentCharAndAdvance('('))
				{
					Debug.Assert(firstWorksheetName == null, "We are not expecting a worksheet name here.");
					if (this.ParseFunctionHelper(Function.GetExternalFunction(filePath, name), referenceStartIndex, whitespaceBeforeReference))
						return true;
				}

				// MD 12/6/11 - 12.1 - Table Support
				// Check for structured table references, which may start with a named reference (the name of the table). 
				// Ignore the worksheet name because it is discarded when parsed.
				if (this.StructuredTableReferenceParserHelper.Parse(filePath, name, whitespaceBeforeReference))
					return true;

				NameToken nameToken;

				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//if ( is3dReference )
				//    nameToken = new NameXToken( this.formulaInstance, filePath, worksheetName, name );
				//else
				//    nameToken = new NameToken( this.formulaInstance, name );
				if (is3dReference)
				{
					Debug.Assert(lastWorksheetName == null, "This is unexpected for a named reference."); 
					nameToken = new NameXToken(filePath, firstWorksheetName, name, this.currentFormat);
				}
				else
				{
					nameToken = new NameToken(name, this.currentFormat);
				}

				this.AddTokenAndWhitespace(
					nameToken,
					referenceStartIndex,
					this.startIndex - referenceStartIndex,
					whitespaceBeforeReference,
					WhitespaceType.SpacesBeforeNextToken );

				return true;
			}

			#endregion Try to parse a local named reference

			// MD 12/6/11 - 12.1 - Table Support
			// Check for structured table references which don't start with a table name. 
			// Ignore the worksheet name because it is discarded when parsed.
			if (this.StructuredTableReferenceParserHelper.Parse(null, null, whitespaceBeforeReference))
				return true;

			// MD 4/6/12 - TFS102169
			// We can only parse cell, row, or column references if this is a local reference or we have a worksheet name.
			//if ( this.cellReferenceMode == CellReferenceMode.R1C1 )
			if (this.cellReferenceMode == CellReferenceMode.R1C1 && 
				(is3dReference == false || firstWorksheetName != null))
			{
				// In R1C1 mode, a row or column range consisting of a single row or column can just have the row or column

				#region Try to parse an R1C1 row range

				int rowIndex;
				bool isRowRelative;
				if ( this.ParseRowAddressR1C1( out rowIndex, out isRowRelative ) )
				{
					CellAddress topLeft = new CellAddress( rowIndex, isRowRelative, 0, false );

					// MD 7/2/08 - Excel 2007 Format
					//CellAddress bottomRight = new CellAddress( rowIndex, isRowRelative, CellAddress.MaxColumnIndex, false );
					// MD 4/12/11 - TFS67084
					// Use short instead of int so we don't have to cast.
					//CellAddress bottomRight = new CellAddress( rowIndex, isRowRelative, Workbook.GetMaxColumnCount( this.currentFormat ) - 1, false );
					CellAddress bottomRight = new CellAddress(rowIndex, isRowRelative, (short)(Workbook.GetMaxColumnCountInternal(this.currentFormat) - 1), false);

					range = new CellAddressRange( topLeft, bottomRight );

					AreaToken areaToken;
					if (is3dReference)
					{
						WorksheetReference worksheetReference = this.GetWorksheetReference(filePath, firstWorksheetName, lastWorksheetName);
						if (range.HasRelativeAddresses)
							areaToken = new Area3DNToken(worksheetReference, range);
						else
							areaToken = new Area3DToken(worksheetReference, range);
					}
					else
					{
						if (range.HasRelativeAddresses)
							areaToken = new AreaNToken(range);
						else
							areaToken = new AreaToken(range);
					}

					this.AddTokenAndWhitespace(
						areaToken,
						referenceStartIndex,
						this.startIndex - referenceStartIndex,
						whitespaceBeforeReference,
						WhitespaceType.SpacesBeforeNextToken );

					return true;
				}

				#endregion Try to parse an R1C1 row range

				#region Try to parse an R1C1 column range

				short columnIndex;
				bool isColumnRelative;
				if ( this.ParseColumnAddressR1C1( out columnIndex, out isColumnRelative ) )
				{
					CellAddress topLeft = new CellAddress( 0, false, columnIndex, isColumnRelative );

					// MD 7/2/08 - Excel 2007 Format
					//CellAddress bottomRight = new CellAddress( CellAddress.MaxRowIndex, false, columnIndex, isColumnRelative );
					CellAddress bottomRight = new CellAddress( Workbook.GetMaxRowCount( this.currentFormat ) - 1, false, columnIndex, isColumnRelative );

					range = new CellAddressRange( topLeft, bottomRight );

					AreaToken areaToken;
					if (is3dReference)
					{
						WorksheetReference worksheetReference = this.GetWorksheetReference(filePath, firstWorksheetName, lastWorksheetName);
						if (range.HasRelativeAddresses)
							areaToken = new Area3DNToken(worksheetReference, range);
						else
							areaToken = new Area3DToken(worksheetReference, range);
					}
					else
					{
						if (range.HasRelativeAddresses)
							areaToken = new AreaNToken(range);
						else
							areaToken = new AreaToken(range);
					}

					this.AddTokenAndWhitespace(
						areaToken,
						referenceStartIndex,
						this.startIndex - referenceStartIndex,
						whitespaceBeforeReference,
						WhitespaceType.SpacesBeforeNextToken );

					return true;
				}

				#endregion Try to parse an R1C1 column range
			}

			if ( is3dReference )
			{
				// MD 9/17/08
				// It is possible to have a worksheet reference followed by a #REF! error (=Sheet1!#REF!).
				if ( this.ParseError( true ) )
				{
					// Remove the ErrToken which was added to the list by the ParseError method.
					this.infixTokenList.RemoveAt( this.infixTokenList.Count - 1 );

					WorksheetReference worksheetReference = this.GetWorksheetReference(filePath, firstWorksheetName, lastWorksheetName);
					RefErr3dToken refErr3dToken = new RefErr3dToken(worksheetReference);

					this.AddTokenAndWhitespace(
						refErr3dToken,
						referenceStartIndex,
						this.startIndex - referenceStartIndex,
						whitespaceBeforeReference,
						WhitespaceType.SpacesBeforeNextToken );

					return true;
				}

				this.ParseException = new FormulaParseException(
					referenceStartIndex,
					this.formula,
					SR.GetString( "LE_FormulaParseException_NoValidTermAfterWorksheetName" ),
					this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );

				return false;
			}

			#region Try to parse an external global named reference

			string workbookReference;
			// MD 9/16/08
			// Found while unit testing
			// Only a quoted reference can be used at this point, because a safe workbook reference would have been parsed as the worksheet name above.
			//if ( this.ParseWorkbookReference( out workbookReference ) )
			if ( this.ParseWorkbookReferenceQuoted( out workbookReference ) )
			{
				string globalName = this.ParseName();

				if ( globalName == null )
				{
					CellAddress cellReference = this.ParseCellAddress();

					if ( cellReference != null )
					{
						this.ParseException = new FormulaParseException(
							referenceStartIndex,
							this.formula,
							SR.GetString( "LE_FormulaParseException_CellReferenceAfterWorkbookName" ),
							this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );
					}
					else
					{
						this.ParseException = new FormulaParseException(
							referenceStartIndex,
							this.formula,
							SR.GetString( "LE_FormulaParseException_NoValidTermAfterWorkbookName" ),
							this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );
					}

					return false;
				}

				this.AddTokenAndWhitespace(
					// MD 10/22/10 - TFS36696
					// We don't need to store the formula on the token anymore.
					//new NameXToken( this.formulaInstance, workbookReference, null, globalName ),
					new NameXToken(workbookReference, null, globalName, this.currentFormat),
					referenceStartIndex,
					this.startIndex - referenceStartIndex,
					WhitespaceType.SpacesBeforeNextToken );

				return true;
			}

			#endregion Try to parse an external global named reference

			return false;
		}

		#endregion ParseReference

		#region ParseRowAddress

		// RowAddress	:	RowAddressA1 | RowAddressR1C1
		private bool ParseRowAddress( out int rowIndex, out bool isRelative )
		{
			if ( this.cellReferenceMode == CellReferenceMode.A1 )
				return this.ParseRowAddressA1( out rowIndex, out isRelative );
			else
				return this.ParseRowAddressR1C1( out rowIndex, out isRelative );
		}

		#endregion ParseRowAddress

		#region ParseRowAddressA1

		// RowAddressA1		:	('$')? [0-9]{1,6}
		private bool ParseRowAddressA1( out int rowIndex, out bool isRelative )
		{
			rowIndex = 0;
			isRelative = false;

			if ( this.ParseException != null )
				return false;

			int length;
			// MD 7/9/08 - Excel 2007 Format
			//if ( FormulaParser.ParseRowAddressA1( this.formula, this.startIndex, ref rowIndex, ref isRelative, out length ) == false )
			if ( FormulaParser.ParseRowAddressA1( this.formula, this.startIndex, this.currentFormat, ref rowIndex, ref isRelative, out length ) == false )
				return false;

			this.startIndex += length;
			return true;
		}

		// RowAddressA1		:	('$')? [0-9]{1,6}
		// MD 7/9/08 - Excel 2007 Format
		// MD 7/14/08 - Excel formula solving
		// Made internal
		//private static bool ParseRowAddressA1( string formula, int index, ref int rowIndex, ref bool isRelative, out int matchLength )
		internal static bool ParseRowAddressA1( string formula, int index, WorkbookFormat currentFormat, ref int rowIndex, ref bool isRelative, out int matchLength )
		{
			matchLength = 0;

			Match match = RowAddressA1Regex.Match( formula, index );

			if ( match.Success == false )
				return false;

			string value = match.Value;
			isRelative = value[ 0 ] != '$';
			string address = isRelative ? value : value.Substring( 1 );

			if ( Int32.TryParse( address, out rowIndex ) == false )
				return false;

			rowIndex--;

			if ( rowIndex < 0 )
				return false;

			// MD 7/9/08 - Excel 2007 Format
			// We should have always been verifying the upper bounds of the index.
			if ( rowIndex > Workbook.GetMaxRowCount( currentFormat ) - 1 )
				return false;

			matchLength = match.Length;

			return true;
		}

		#endregion ParseRowAddressA1

		// MD 7/9/08 - Excel 2007 Format
		#region ParseRowAddressIndexR1C1

		private static bool ParseRowAddressIndexR1C1( string addressIndex, bool isRelative, WorkbookFormat currentFormat, out int rowIndex )
		{
			if ( Int32.TryParse( addressIndex, out rowIndex ) == false )
				return false;

			if ( isRelative == false )
			{
				rowIndex--;

				if ( rowIndex < 0 )
					return false;
			}

			// MD 7/9/08 - Excel 2007 Format
			// We should have always been verifying the upper bounds of the index.
			if ( Math.Abs( rowIndex ) > Workbook.GetMaxRowCount( currentFormat ) - 1 )
				return false;

			return true;
		} 

		#endregion ParseRowAddressIndexR1C1

		#region ParseRowAddressR1C1

		// RowAddressR1C1	:	'R' ( ( '[' [-|+]? [0-9]{1,6} ']' ) | ( [0-9]{1,6} ) )? 
		private bool ParseRowAddressR1C1( out int rowIndex, out bool isRelative )
		{
			rowIndex = 0;
			isRelative = false;

			if ( this.ParseException != null )
				return false;

			int length;
			// MD 7/9/08 - Excel 2007 Format
			//if ( FormulaParser.ParseRowAddressR1C1( this.formula, this.startIndex, ref rowIndex, ref isRelative, out length ) == false )
			if ( FormulaParser.ParseRowAddressR1C1( this.formula, this.startIndex, this.currentFormat, ref rowIndex, ref isRelative, out length ) == false )
				return false;

			this.startIndex += length;
			return true;
		}

		// RowAddressR1C1	:	'R' ( ( '[' [-|+]? [0-9]{1,6} ']' ) | ( [0-9]{1,6} ) )? 
		// MD 7/9/08 - Excel 2007 Format
		// MD 7/14/08 - Excel formula solving
		// Made internal
		//private static bool ParseRowAddressR1C1( string formula, int index, ref int rowIndex, ref bool isRelative, out int matchLength )
		internal static bool ParseRowAddressR1C1( string formula, int index, WorkbookFormat currentFormat, ref int rowIndex, ref bool isRelative, out int matchLength )
		{
			matchLength = 0;

			Match match = RowAddressR1C1Regex.Match( formula, index );

			if ( match.Success == false )
				return false;

			string value = match.Value;

			if ( value.Length == 1 )
			{
				isRelative = true;
				rowIndex = 0;
			}
			else
			{
				isRelative = value[ 1 ] == '[';
				string address = isRelative ? value.Substring( 2, value.Length - 3 ) : value.Substring( 1 );

				// MD 7/9/08 - Excel 2007 Format
				// Moved to ParseRowAddressIndexR1C1 and change the logic so if the address parsing fails,
				// we will still return a valid address
				#region Moved

				
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


				#endregion Moved
				if ( FormulaParser.ParseRowAddressIndexR1C1( address, isRelative, currentFormat, out rowIndex ) == false )
				{
					isRelative = true;
					rowIndex = 0;
					matchLength = 1;
					return true;
				}
			}

			matchLength = match.Length;
			return true;
		}

		#endregion ParseRowAddressR1C1

		#region ParseRowRange

		// RowRange			:	RowRangeA1 | RowRangeR1C1
		private CellAddressRange ParseRowRange()
		{
			PositionInfo position = this.SavePosition();

			int rowIndex1;
			bool isRelative1;

			if ( this.ParseRowAddress( out rowIndex1, out isRelative1 ) == false )
			{
				this.RestorePosition( position );
				return null;
			}

			if ( this.ParseException != null )
				return null;

			if ( this.TestCurrentCharAndAdvance( FormulaParser.RangeOperator ) == false )
			{
				this.RestorePosition( position );
				return null;
			}

			int rowIndex2;
			bool isRelative2;

			if ( this.ParseRowAddress( out rowIndex2, out isRelative2 ) == false )
			{
				this.RestorePosition( position );
				return null;
			}

			CellAddress topLeft;
			CellAddress bottomRight;

			// MD 7/2/08 - Excel 2007 Format
			// MD 4/12/11 - TFS67084
			// Use short instead of int so we don't have to cast.
			//int maxColumnIndex = Workbook.GetMaxColumnCount( this.currentFormat ) - 1;
			short maxColumnIndex = (short)(Workbook.GetMaxColumnCountInternal(this.currentFormat) - 1);

			if ( rowIndex1 <= rowIndex2 )
			{
				topLeft = new CellAddress( rowIndex1, isRelative1, 0, false );

				// MD 7/2/08 - Excel 2007 Format
				//bottomRight = new CellAddress( rowIndex2, isRelative2, CellAddress.MaxColumnIndex, false );
				bottomRight = new CellAddress( rowIndex2, isRelative2, maxColumnIndex, false );
			}
			else
			{
				topLeft = new CellAddress( rowIndex2, isRelative2, 0, false );

				// MD 7/2/08 - Excel 2007 Format
				//bottomRight = new CellAddress( rowIndex1, isRelative1, CellAddress.MaxColumnIndex, false );
				bottomRight = new CellAddress( rowIndex1, isRelative1, maxColumnIndex, false );
			}

			return new CellAddressRange( topLeft, bottomRight );
		}

		#endregion ParseRowRange

		#region ParseSeparator

		// Separator	:	','
		// MD 9/11/09 - TFS20376
		// Separators may not be the same depending on the culture, so this need to take the separator.
		//private bool ParseSeparator()
		private bool ParseSeparator( string separator )
		{
			if ( this.ParseException != null )
				return false;

			// MD 9/11/09 - TFS20376
			// Use the passed in separator.
			//if ( this.TestCurrentChar( FormulaParser.UnionOperator ) == false )
			if ( this.TestCurrentChar( separator ) == false )
				return false;

			// MD 10/22/10 - TFS36696
			// We don't need to store the formula on the token anymore.
			//this.AddToken( new UnionOperator( this.formulaInstance ), 1 );
			this.AddToken(new UnionOperator(), 1);
			return true;
		}

		#endregion ParseSeparator

		#region ParseString

		// String	:	'"' ( [^0|'"'] | '""' )* '"'
		private bool ParseString()
		{
			if ( this.ParseException != null )
				return false;

			Match match = StringRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
				return false;

			string value = match.Value;

			// Remove quotes
			value = value.Substring( 1, value.Length - 2 );

            // MBS 7/11/08 - Excel 2007 Format
            // We need to check that the string is not longer than the maximum value.
            // Note that Excel treats an excaped string as two characters, so we have
            // to check this before we do the replacement below
            if (value.Length > FormulaParser.MaxStringConstantLength)
            {
                this.ParseException = new FormulaParseException(
                    this.startIndex,
                    this.formula,
                    String.Format(SR.GetString("LE_FormulaParseException_StringConstantLengthTooLong"), FormulaParser.MaxStringConstantLength),
                    value);

                return false;
            }

			// Convert escaped quotes
			value = value.Replace( "\"\"", "\"" );

			this.AddTokenAndWhitespace(
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//new StrToken( this.formulaInstance, value ),
				new StrToken(value),
				match.Length,
				WhitespaceType.SpacesBeforeNextToken );

			return true;
		}

		#endregion ParseString

		#region ParseUnaryL

		// UnaryL	:	'%'
		private bool ParseUnaryL()
		{
			if ( this.ParseException != null )
				return false;

			if ( this.TestCurrentChar( '%' ) == false )
				return false;

			// MD 10/22/10 - TFS36696
			// We don't need to store the formula on the token anymore.
			//this.AddTokenAndWhitespace( new PercentOperator( this.formulaInstance ), 1, WhitespaceType.SpacesBeforeNextToken );
			this.AddTokenAndWhitespace(new PercentOperator(), 1, WhitespaceType.SpacesBeforeNextToken);
			return true;
		}

		#endregion ParseUnaryL

		#region ParseUnaryR

		// UnaryR	:	'+' | '-'
		private bool ParseUnaryR()
		{
			if ( this.ParseException != null )
				return false;

			// MD 10/22/10 - TFS36696
			// We don't need to store the formula on the token anymore.
			//if ( this.TestCurrentChar( '-' ) )
			//    this.AddTokenAndWhitespace( new UminusOperator( this.formulaInstance ), 1, WhitespaceType.SpacesBeforeNextToken );
			//else if ( this.TestCurrentChar( '+' ) )
			//    this.AddTokenAndWhitespace( new UplusOperator( this.formulaInstance ), 1, WhitespaceType.SpacesBeforeNextToken );
			if (this.TestCurrentChar('-'))
				this.AddTokenAndWhitespace(new UminusOperator(), 1, WhitespaceType.SpacesBeforeNextToken);
			else if (this.TestCurrentChar('+'))
				this.AddTokenAndWhitespace(new UplusOperator(), 1, WhitespaceType.SpacesBeforeNextToken);
			else
				return false;

			return true;
		}

		#endregion ParseUnaryR

		#region ParseUrlPath

		// UrlPath	:	(ht|f)tp(s?):// [0-9a-zA-Z] ( [-.\w]* [0-9a-zA-Z] )* (:(0-9)*)* (/?) ( [a-zA-Z0-9-.?,'/\+&%$#_]*)? /
		private string ParseUrlPath()
		{
			if ( this.ParseException != null )
				return null;

			Match match = UrlPathRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
				return null;

			string urlPath = match.Value;
			urlPath = urlPath.Replace( "''", "'" );

			this.startIndex += match.Length;

			return urlPath;
		}

		#endregion ParseUrlPath

		// MD 9/16/08
		// Found while unit testing
		// It turns out this will never be used
		#region Not Used

		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		#endregion Not Used

		#region ParseWorkbookReferenceQuoted

		// ParseWorkbookReferenceQuoted	:	''' FilePath? FileNameQuoted ''' '!'
		private bool ParseWorkbookReferenceQuoted( out string filePath )
		{
			filePath = null;

			if ( this.ParseException != null )
				return false;

			int referenceStartIndex = this.startIndex;
			if ( this.TestCurrentCharAndAdvance( '\'' ) == false )
				return false;

			string directoryPath = this.ParseFilePath();
			string fileNameQuoted = this.ParseFileNameQuoted();

			if ( fileNameQuoted == null )
			{
				this.startIndex = referenceStartIndex;
				return false;
			}

			filePath = String.Concat( directoryPath, fileNameQuoted );

			if ( this.TestCurrentCharAndAdvance( '\'' ) == false )
			{
				Match endQuoteMatch = FormulaParser.EndQuoteRegex.Match( this.formula, this.startIndex );

				if ( endQuoteMatch.Success )
				{
					this.ParseException = new FormulaParseException(
						this.startIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_InvalidWorkbookName" ),
						this.formula.Substring( referenceStartIndex, endQuoteMatch.Index - referenceStartIndex + 1 ) );
				}
				else
				{
					this.ParseException = new FormulaParseException(
						this.startIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_WorkbookNameMissingEndQuote" ),
						this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );
				}

				return false;
			}

			// MD 7/14/08 - Excel formula solving
			// This is now a constant
			//if ( this.TestCurrentCharAndAdvance( '!' ) == false )
			if ( this.TestCurrentCharAndAdvance( FormulaParser.WorksheetNameSeparator ) == false )
			{
				this.ParseException = new FormulaParseException(
					referenceStartIndex,
					this.formula,
					SR.GetString( "LE_FormulaParseException_NoExclamationAfterWorkbookName" ),
					this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );

				return false;
			}

			return true;
		}

		#endregion ParseWorkbookReferenceQuoted

		// MD 9/16/08
		// Found while unit testing
		// It turns out this will never be used
		#region Not Used

		
#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)


		#endregion Not Used

		#region ParseWorksheetNameQuoted

		// WorksheetNameQuoted	:	( [^*[]:'/\?] | ''' ''' )+
		private string ParseWorksheetNameQuoted()
		{
			if ( this.ParseException != null )
				return null;

			Match match = WorksheetNameQuotedRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
				return null;

			string sheetName = match.Value;
			sheetName = sheetName.Replace( "''", "'" );

			this.startIndex += match.Length;

			return sheetName;
		}

		#endregion ParseWorksheetNameQuoted

		#region ParseWorksheetNameSafe

		// WorksheetNameSafe	:	[_A-Za-z����] [_0-9A-Za-z����.]*
		private string ParseWorksheetNameSafe()
		{
			if ( this.ParseException != null )
				return null;

			Match match = WorksheetNameSafeRegex.Match( this.formula, this.startIndex );

			if ( match.Success == false )
				return null;

			string sheetName = match.Value;
			Debug.Assert( FormulaParser.ShouldWorksheetNameBeQuoted( sheetName ) == false );

			this.startIndex += match.Length;

			return sheetName;
		}

		#endregion ParseWorksheetNameSafe

		#region ParseWorksheetReference

		// WorksheetReference	:	WorksheetReferenceSafe | WorksheetReferenceQuoted
		private bool ParseWorksheetReference(out string filePath, out string firstWorksheetName, out string lastWorksheetName)
		{
			if (this.ParseWorksheetReferenceSafe(out filePath, out firstWorksheetName, out lastWorksheetName))
				return true;

			return this.ParseWorksheetReferenceQuoted(out filePath, out firstWorksheetName, out lastWorksheetName);
		}

		#endregion ParseWorksheetReference

		#region ParseWorksheetReferenceQuoted

		// WorksheetReferenceQuoted	:	''' FileNameBracketedQuoted? WorksheetNameQuoted ''' '!'
		private bool ParseWorksheetReferenceQuoted(out string filePath, out string firstWorksheetName, out string lastWorksheetName)
		{
			filePath = null;
			firstWorksheetName = null;
			lastWorksheetName = null;

			if ( this.ParseException != null )
				return false;

			int referenceStartIndex = this.startIndex;

			if ( this.TestCurrentCharAndAdvance( '\'' ) == false )
				return false;

			filePath = this.ParseFileNameBracketedQuoted();
			firstWorksheetName = this.ParseWorksheetNameQuoted();

			// MD 6/18/12 - TFS102878
			// If the worksheet name is followed by a range operator, there should be another worksheet name after it.
			if (firstWorksheetName != null && this.TestCurrentCharAndAdvance(FormulaParser.RangeOperator))
			{
				int lastWorksheetNameIndex = this.startIndex;
				lastWorksheetName = this.ParseWorksheetNameQuoted();

				if (lastWorksheetName == null && this.TestCurrentCharAndAdvance('\''))
				{
					if (this.ParseException == null)
					{
						this.ParseException = new FormulaParseException(
							lastWorksheetNameIndex,
							this.formula,
							SR.GetString("LE_FormulaParseException_WorksheetRangeMissingEndingName"),
							this.formula.Substring(referenceStartIndex, this.startIndex - referenceStartIndex + 1));
					}

					this.startIndex = referenceStartIndex;
					return false;
				}
			}

			if ( firstWorksheetName == null || this.TestCurrentCharAndAdvance( '\'' ) == false )
			{
				if ( filePath != null )
				{
					Match endQuoteMatch = FormulaParser.EndQuoteRegex.Match( this.formula, this.startIndex );

					if ( endQuoteMatch.Success )
					{
						this.ParseException = new FormulaParseException(
							referenceStartIndex,
							this.formula,
							SR.GetString( "LE_FormulaParseException_InvalidWorksheetName" ),
							this.formula.Substring( referenceStartIndex, endQuoteMatch.Index - referenceStartIndex + 1 ) );
					}
					else
					{
						this.ParseException = new FormulaParseException(
							referenceStartIndex,
							this.formula,
							SR.GetString( "LE_FormulaParseException_InvalidWorksheetName" ),
							this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );
					}
				}

				this.startIndex = referenceStartIndex;
				return false;
			}

			// MD 7/14/08 - Excel formula solving
			// This is now a constant
			//if ( this.TestCurrentCharAndAdvance( '!' ) == false )
			if ( this.TestCurrentCharAndAdvance( FormulaParser.WorksheetNameSeparator ) == false )
			{
				this.ParseException = new FormulaParseException(
					referenceStartIndex,
					this.formula,
					SR.GetString( "LE_FormulaParseException_NoExclamationAfterWorksheetName" ),
					this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );

				return false;
			}

			return true;
		}

		#endregion ParseWorksheetReferenceQuoted

		#region ParseWorksheetReferenceSafe

		// WorksheetReferenceSafe	:	FileNameBracketedSafe? WorksheetNameSafe '!'
		private bool ParseWorksheetReferenceSafe(out string filePath, out string firstWorksheetName, out string lastWorksheetName)
		{
			filePath = null;
			firstWorksheetName = null;
			lastWorksheetName = null;

			if ( this.ParseException != null )
				return false;

			// MD 12/7/11 - 12.1 - Table Support
			// We need to get the position info so we can do a restore.
			//int referenceStartIndex = this.startIndex;
			PositionInfo position = this.SavePosition();
			int referenceStartIndex = position.StartIndex;

			// MD 2/23/12 - TFS101504
			// There is now an out parameter which indicates whether the bracketed expression was an index into a loaded 
			// references collection.
			//filePath = this.ParseFileNameBracketedSafe();
			bool wasIndexedReference;
			filePath = this.ParseFileNameBracketedSafe(out wasIndexedReference);

			firstWorksheetName = this.ParseWorksheetNameSafe();

			// MD 2/23/12 - TFS101504
			// A bracketed expression without a following worksheet name is allowed when it is an indexed reference in the
			// brackets.
			//if ( worksheetName == null )
			if (firstWorksheetName == null && wasIndexedReference == false)
			{
				if ( filePath != null )
				{
					// MD 12/7/11 - 12.1 - Table Support
					// In the 2007 formats and later, you can have structured table references which are in square brackets,
					// so this is not necessarily an error.
					if (Utilities.Is2003Format(this.currentFormat) == false)
					{
						filePath = null;
						this.RestorePosition(position);
						return false;
					}

					this.ParseException = new FormulaParseException(
						referenceStartIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_NoWorksheetAfterWorkbookName" ),
						this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );
				}

				// MD 12/7/11 - 12.1 - Table Support
				//this.startIndex = referenceStartIndex;
				this.RestorePosition(position);

				return false;
			}

			// MD 6/18/12 - TFS102878
			// If the worksheet name is followed by a range operator, there should be another worksheet name after it.
			if (this.TestCurrentCharAndAdvance(FormulaParser.RangeOperator))
			{
				// However, a worksheet name and a cell reference name could be the same, so first check to see if the range is a region
				// address first. If it is, return false here because it should be parsed as an area token.
				if (filePath == null)
				{
					short temp1;
					int temp2;
					if (this.cellReferenceMode == CellReferenceMode.A1)
					{
						if (Utilities.ParseA1CellAddress(firstWorksheetName, this.currentFormat, this.culture, out temp1, out temp2) &&
							this.TestCurrentCharAndAdvance(FormulaParser.WorksheetNameSeparator) == false)
						{
							this.RestorePosition(position);
							return false;
						}
					}
					else
					{
						bool temp3;
						bool temp4;
						if (Utilities.ParseR1C1CellAddress(firstWorksheetName, this.currentFormat, out temp1, out temp3, out temp2, out temp4) &&
							this.TestCurrentCharAndAdvance(FormulaParser.WorksheetNameSeparator) == false)
						{
							this.RestorePosition(position);
							return false;
						}
					}
				}

				int lastWorksheetNameIndex = this.startIndex;
				lastWorksheetName = this.ParseWorksheetNameSafe();

				if (lastWorksheetName == null)
				{
					this.ParseException = new FormulaParseException(
						lastWorksheetNameIndex,
						this.formula,
						SR.GetString("LE_FormulaParseException_WorksheetRangeMissingEndingName"),
						this.formula.Substring(referenceStartIndex, this.startIndex - referenceStartIndex));

					return false;
				}
			}

			// MD 7/14/08 - Excel formula solving
			// This is now a constant
			//if ( this.TestCurrentCharAndAdvance( '!' ) == false )
			if ( this.TestCurrentCharAndAdvance( FormulaParser.WorksheetNameSeparator ) == false )
			{
				// MD 4/4/12 - TFS100966
				// If there is something bracketed at the start of the parsed workbook name, see if it was a named reference.
				// If it was, reset the start index so we can parsed it correctly in the calling function.
				this.startIndex = referenceStartIndex;
				if (this.TestCurrentChar('[') && this.ParseName() != null)
				{
					this.startIndex = referenceStartIndex;
					return false;
				}

				// If the file path is null, this is just a named reference
				if ( filePath != null )
				{
					this.ParseException = new FormulaParseException(
						referenceStartIndex,
						this.formula,
						SR.GetString( "LE_FormulaParseException_NoExclamationAfterWorksheetName" ),
						this.formula.Substring( referenceStartIndex, this.startIndex - referenceStartIndex ) );
				}

				// MD 12/7/11 - 12.1 - Table Support
				//this.startIndex = referenceStartIndex;
				this.RestorePosition(position);

				return false;
			}

			return true;
		}

		#endregion ParseWorksheetReferenceSafe

		// MD 5/13/11 - Data Validations / Page Breaks
		#region TryParseBoolean

		public static bool TryParseBoolean(string value, out bool booleanValue)
		{
			booleanValue = false;

			if (value.Equals("FALSE", StringComparison.InvariantCultureIgnoreCase))
				return true;

			if (value.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
			{
				booleanValue = true;
				return true;
			}

			return false;
		}

		#endregion  // TryParseBoolean

		// MD 4/9/12 - TFS101506
		// This is replaced by MathUtilities.TryParseDouble
		#region Removed

		//// MD 5/13/11 - Data Validations / Page Breaks
		//// Moved this code from ParseNumber()
		//#region TryParseNumber

		//public static bool TryParseNumber(string value, CultureInfo culture, out double number)
		//{
		//    return Double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, culture, out number);
		//}

		//#endregion  // TryParseNumber

		#endregion // Removed

		#endregion Parse various grammar types

		#region Methods

		#region AddOperatorToStack

		private void AddOperatorToStack( Stack<OperatorToken> operatorStack, OperatorToken currentOperator )
		{
			while ( operatorStack.Count > 0 )
			{
				if ( currentOperator.IsRightAssociative )
				{
					// Be careful, this is greater than...
					if ( currentOperator.Precedence > operatorStack.Peek().Precedence )
						this.AddTokenToPostfixList( operatorStack.Pop() );
					else
						break;
				}
				else
				{
					// ... and this is greater than or equal.
					if ( currentOperator.Precedence >= operatorStack.Peek().Precedence )
						this.AddTokenToPostfixList( operatorStack.Pop() );
					else
						break;
				}
			}

			operatorStack.Push( currentOperator );
		}

		#endregion AddOperatorToStack

		#region AddToken

		private void AddToken( FormulaToken token, int tokenLength )
		{
			this.AddToken( token, this.startIndex, tokenLength );
		}

		private void AddToken( FormulaToken token, int tokenStart, int tokenLength )
		{
			this.InsertToken( this.infixTokenList.Count, token, tokenStart, tokenLength );
		}

		private void InsertToken( int index, FormulaToken token, int tokenStart, int tokenLength )
		{
			// MD 10/22/10 - TFS36696
			// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
			//token.StartCharIndex = tokenStart;
			this.tokenStartCharIndices[token] = tokenStart;

			this.startIndex = tokenStart + tokenLength;

			this.infixTokenList.Insert( index, token );
			this.ParseNextWhitespace();
		}

		#endregion AddToken

		#region AddTokenAndWhitespace

		private void AddTokenAndWhitespace( FormulaToken token, int tokenLength, WhitespaceType whitespaceType )
		{
			this.AddTokenAndWhitespace( token, this.startIndex, tokenLength, whitespaceType );
		}

		private void AddTokenAndWhitespace( FormulaToken token, int tokenStart, int tokenLength, WhitespaceType whitespaceType )
		{
			this.AddTokenAndWhitespace( token, tokenStart, tokenLength, this.nextWhitespace, whitespaceType );
		}

		private void AddTokenAndWhitespace( FormulaToken token, int tokenStart, int tokenLength, string whitespace, WhitespaceType whitespaceType )
		{
			this.InsertTokenAndWhitespace( this.infixTokenList.Count, token, tokenStart, tokenLength, whitespace, whitespaceType );
		}

		private void InsertTokenAndWhitespace( int index, FormulaToken token, int tokenStart, int tokenLength, string whitespace, WhitespaceType whitespaceType )
		{
			List<AttrSpaceToken> newWhitespaceTokens = CreateWhitespaceOperators( whitespace, whitespaceType );

			if ( newWhitespaceTokens != null )
			{
				// MD 10/22/10 - TFS36696
				// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
				//if ( token.PreceedingWhitespace == null )
				//    token.PreceedingWhitespace = newWhitespaceTokens;
				//else
				//    token.PreceedingWhitespace.AddRange( newWhitespaceTokens );
				List<AttrSpaceToken> preceedingWhitespace;
				if (this.tokenPreceedingWhitespace.TryGetValue(token, out preceedingWhitespace))
					preceedingWhitespace.AddRange(newWhitespaceTokens);
				else
					this.tokenPreceedingWhitespace[token] = newWhitespaceTokens;
			}

			this.InsertToken( index, token, tokenStart, tokenLength );
		}

		#endregion AddTokenAndWhitespace

		#region AddTokenToPostfixList

		private void AddTokenToPostfixList( FormulaToken token )
		{
			// MD 10/22/10 - TFS36696
			// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
			//if ( token.PreceedingWhitespace != null )
			//{
			//    foreach ( AttrSpaceToken whitespaceToken in token.PreceedingWhitespace )
			//        this.formulaInstance.PostfixTokenList.Add( whitespaceToken );
			//}
			List<AttrSpaceToken> preceedingWhitespace;
			if (this.tokenPreceedingWhitespace.TryGetValue(token, out preceedingWhitespace))
			{
				foreach (AttrSpaceToken whitespaceToken in preceedingWhitespace)
					this.formulaInstance.PostfixTokenList.Add(whitespaceToken);
			}

			this.formulaInstance.PostfixTokenList.Add( token );
		}

		#endregion AddTokenToPostfixList

		#region AddWhitespaceChars

		private void AddWhitespaceChars( int numChars, bool shouldBeSpaces, WhitespaceType whitespaceType, List<AttrSpaceToken> whitespaceTokens )
		{
			whitespaceType = GetCorrectWhitespaceType( whitespaceType, shouldBeSpaces );

			while ( numChars > 0 )
			{
				byte spacesInToken = (byte)Math.Min( numChars, 255 );

				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//whitespaceTokens.Add( new AttrSpaceToken( this.formulaInstance, spacesInToken, whitespaceType ) );
				whitespaceTokens.Add(new AttrSpaceToken(spacesInToken, whitespaceType));

				numChars -= spacesInToken;
			}
		}

		#endregion AddWhitespaceChars

		#region ConvertInfixToPostfix

		// This is a variation of Edsger Dijkstra's shunting yard algorithm
		// http://en.wikipedia.org/wiki/Shunting_yard_algorithm
		private void ConvertInfixToPostfix()
		{
			// MD 7/16/08
			// Found while implementing Excel formula solving
			// See comment below
			bool currentTokenIsFunctionOperator = false;
			bool lastTokenWasFunctionOperator = false;

			Stack<AttrChooseToken> chooseTokens = null;
			Stack<AttrIfToken> ifTokens = null;
			Stack<byte> functionParamIndicies = new Stack<byte>();

			Stack<OperatorToken> operatorStack = new Stack<OperatorToken>();

			foreach ( FormulaToken currentToken in this.infixTokenList )
			{
				// MD 7/16/08
				// Found while implementing Excel formula solving
				// We need to keep track of whether the last token was a function. If so, a closing parenthesis should not increment the paramter
				// count, because it is right after the openning paren of the function. If not, another token occurred after the function operator,
				// which means the closing paren should increment the parameter count.
				lastTokenWasFunctionOperator = currentTokenIsFunctionOperator;
				currentTokenIsFunctionOperator = false;

				#region Process the function token

				FunctionOperator functionOperator = currentToken as FunctionOperator;

				if ( functionOperator != null )
				{
					// MD 7/16/08
					// Found while implementing Excel formula solving
					// See comment above
					currentTokenIsFunctionOperator = true;

					functionParamIndicies.Push( 0 );
					operatorStack.Push( functionOperator );

					// MD 10/8/07 - BR27172
					// If the function is an add-in function, the name of the function must be stored as a named reference
					// before all function parameters, so add the name token to the postfix list now.
					if ( functionOperator.Function.IsAddIn )
					{
						if ( functionOperator.Function.IsInternalAddInFunction )
						{
							this.AddTokenToPostfixList( new NameToken(
								functionOperator.Function.Name,
								this.currentFormat) );
						}
						else
						{
							this.AddTokenToPostfixList( new NameXToken(
								functionOperator.Function.WorkbookPath,
								null,
								functionOperator.Function.Name,
								this.currentFormat) );
						}
					}

					continue;
				}

				#endregion Process the function token

				#region Process the open paren token

				OpenParenOperator openParenOperator = currentToken as OpenParenOperator;

				if ( openParenOperator != null )
				{
					operatorStack.Push( openParenOperator );
					continue;
				}

				#endregion Process the open paren token

				#region Process the function param separator

				UnionOperator listOperator = currentToken as UnionOperator;

				if ( listOperator != null )
				{
					bool isFunctionSeparator = false;

					foreach ( OperatorToken opInStack in operatorStack )
					{
						if ( opInStack is OpenParenOperator )
						{
							// If we reach an open paren, the operator is a list binary operator,
							// break out of the loop so we can add it to the operator stack below
							break;
						}
						else if ( opInStack is FunctionOperator )
						{
							isFunctionSeparator = true;

							while ( ( operatorStack.Peek() is FunctionOperator ) == false )
								this.AddTokenToPostfixList( operatorStack.Pop() );

							FunctionOperator function = (FunctionOperator)operatorStack.Peek();

							#region Perform fixes for IF and CHOOSE functions

							// MD 10/8/07
							// Found while fixing BR27172
							// This is a better way to determine which function we are processing
							//if ( function.Function.Name == "IF" )
							if ( function.Function == Function.IF )
							{
								if ( functionParamIndicies.Peek() == 0 )
								{
									if ( ifTokens == null )
										ifTokens = new Stack<AttrIfToken>();

									// MD 10/22/10 - TFS36696
									// We don't need to store the formula on the token anymore.
									//AttrIfToken ifToken = new AttrIfToken( this.formulaInstance );
									AttrIfToken ifToken = new AttrIfToken();

									ifTokens.Push( ifToken );
									this.formulaInstance.PostfixTokenList.Add( ifToken );
								}
								else
								{
									// MD 10/22/10 - TFS36696
									// We don't need to store the formula on the token anymore.
									//AttrSkipToken skipToken = new AttrSkipToken( this.formulaInstance );
									AttrSkipToken skipToken = new AttrSkipToken();

									ifTokens.Peek().SkipTokens.Add( skipToken );
									this.formulaInstance.PostfixTokenList.Add( skipToken );
								}
							}
							// MD 10/8/07
							// Found while fixing BR27172
							// This is a better way to determine which function we are processing
							//else if ( function.Function.Name == "CHOOSE" )
							else if ( function.Function == Function.CHOOSE )
							{
								if ( functionParamIndicies.Peek() == 0 )
								{
									if ( chooseTokens == null )
										chooseTokens = new Stack<AttrChooseToken>();

									// MD 10/22/10 - TFS36696
									// We don't need to store the formula on the token anymore.
									//AttrChooseToken chooseToken = new AttrChooseToken( this.formulaInstance );
									AttrChooseToken chooseToken = new AttrChooseToken();

									chooseTokens.Push( chooseToken );
									this.formulaInstance.PostfixTokenList.Add( chooseToken );
								}
								else
								{
									// MD 10/22/10 - TFS36696
									// We don't need to store the formula on the token anymore.
									//AttrSkipToken skipToken = new AttrSkipToken( this.formulaInstance );
									AttrSkipToken skipToken = new AttrSkipToken();

									chooseTokens.Peek().SkipTokens.Add( skipToken );
									this.formulaInstance.PostfixTokenList.Add( skipToken );
								}
							}

							#endregion Perform fixes for IF and CHOOSE functions

							byte paramIndex = functionParamIndicies.Pop();

                            // MBS 7/10/08 - Excel 2007 Format
                            // We can't really assert for this anymore since we don't know the current format.
                            // Fortunately, the GetMaxParams on the Function should handle this when parsing the 
                            // formula before we even get here.
							//Debug.Assert( paramIndex < 30, "No function can have more than 30 arguments" );

							paramIndex++;
							functionParamIndicies.Push( paramIndex );

							break;
						}
					}

					// If the comma is a function separator, continue on to the next token, otherwise, let 
					// it fall below where it will be processed like a normal operator token
					if ( isFunctionSeparator )
						continue;
				}

				#endregion Process the list operator or function param separator

				#region Process the closing paren token

				ParenToken parenToken = currentToken as ParenToken;

				if ( parenToken != null )
				{
					while (
						( operatorStack.Peek() is FunctionOperator ) == false &&
						( operatorStack.Peek() is OpenParenOperator ) == false )
					{
						this.AddTokenToPostfixList( operatorStack.Pop() );
					}

					OperatorToken topOperator = operatorStack.Pop();

					FunctionOperator funcOperator = topOperator as FunctionOperator;

					if ( funcOperator != null )
					{
						// MD 10/22/10 - TFS36696
						// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
						//if ( currentToken.PreceedingWhitespace != null )
						//{
						//    if ( funcOperator.PreceedingWhitespace == null )
						//        funcOperator.PreceedingWhitespace = new List<AttrSpaceToken>();
						//
						//    funcOperator.PreceedingWhitespace.InsertRange( 0, currentToken.PreceedingWhitespace );
						//}
						List<AttrSpaceToken> currentTokenPreceedingWhitespace;
						if (this.tokenPreceedingWhitespace.TryGetValue(currentToken, out currentTokenPreceedingWhitespace))
						{
							List<AttrSpaceToken> funcOperatorPreceedingWhitespace;
							if (this.tokenPreceedingWhitespace.TryGetValue(funcOperator, out funcOperatorPreceedingWhitespace))
								funcOperatorPreceedingWhitespace.InsertRange(0, currentTokenPreceedingWhitespace);
							else
								this.tokenPreceedingWhitespace[funcOperator] = new List<AttrSpaceToken>(currentTokenPreceedingWhitespace);
						}

						#region Perform fixes for IF and CHOOSE functions

						// MD 10/8/07
						// Found while fixing BR27172
						// This is a better way to determine which function we are processing
						//if ( funcOperator.Function.Name == "IF" )
						if ( funcOperator.Function == Function.IF )
						{
							AttrIfToken ifToken = ifTokens.Pop();

							// MD 10/22/10 - TFS36696
							// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
							//if ( funcOperator.PreceedingWhitespace != null )
							//{
							//    foreach ( AttrSpaceToken whitespaceToken in funcOperator.PreceedingWhitespace )
							//        this.formulaInstance.PostfixTokenList.Add( whitespaceToken );
							//
							//    funcOperator.PreceedingWhitespace.Clear();
							//}
							List<AttrSpaceToken> funcOperatorPreceedingWhitespace;
							if (this.tokenPreceedingWhitespace.TryGetValue(funcOperator, out funcOperatorPreceedingWhitespace))
							{
								foreach (AttrSpaceToken whitespaceToken in funcOperatorPreceedingWhitespace)
									this.formulaInstance.PostfixTokenList.Add(whitespaceToken);

								funcOperatorPreceedingWhitespace.Clear();
							}

							// MD 10/22/10 - TFS36696
							// We don't need to store the formula on the token anymore.
							//AttrSkipToken skipToken = new AttrSkipToken( this.formulaInstance );
							AttrSkipToken skipToken = new AttrSkipToken();

							ifToken.SkipTokens.Add( skipToken );
							this.formulaInstance.PostfixTokenList.Add( skipToken );
						}
						// MD 10/8/07
						// Found while fixing BR27172
						// This is a better way to determine which function we are processing
						//else if ( funcOperator.Function.Name == "CHOOSE" )
						else if ( funcOperator.Function == Function.CHOOSE )
						{
							AttrChooseToken chooseToken = chooseTokens.Pop();

							// MD 10/22/10 - TFS36696
							// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
							//if ( funcOperator.PreceedingWhitespace != null )
							//{
							//    foreach ( AttrSpaceToken whitespaceToken in funcOperator.PreceedingWhitespace )
							//        this.formulaInstance.PostfixTokenList.Add( whitespaceToken );
							//
							//    funcOperator.PreceedingWhitespace.Clear();
							//}
							List<AttrSpaceToken> funcOperatorPreceedingWhitespace;
							if (this.tokenPreceedingWhitespace.TryGetValue(funcOperator, out funcOperatorPreceedingWhitespace))
							{
								foreach (AttrSpaceToken whitespaceToken in funcOperatorPreceedingWhitespace)
									this.formulaInstance.PostfixTokenList.Add(whitespaceToken);

								funcOperatorPreceedingWhitespace.Clear();
							}

							// MD 10/22/10 - TFS36696
							// We don't need to store the formula on the token anymore.
							//AttrSkipToken skipToken = new AttrSkipToken( this.formulaInstance );
							AttrSkipToken skipToken = new AttrSkipToken();

							chooseToken.SkipTokens.Add( skipToken );
							this.formulaInstance.PostfixTokenList.Add( skipToken );
						}

						#endregion Perform fixes for IF and CHOOSE functions

						// MD 7/16/08
						// Found while implementing Excel formula solving
						// We don't always want to add one here. This should have been commented before, but the reason 1 is added is because
						// we increment the parameter index/count for each ',' in the argument list, but there is always one more agument than
						// argument separator. However, this is not the case with an empty argument list: there are 0 separators and 0 arguments.
						// If this closing parenthesis occurred right after the openning parenthesis for the function, don't increment the 
						// parameter count.
						//byte parameters = (byte)( 1 + functionParamIndicies.Pop() );
						byte parameters = functionParamIndicies.Pop();
						if ( lastTokenWasFunctionOperator == false )
							parameters++;
						else
							Debug.Assert( parameters == 0, "If the ')' occurred right after the function token, there should be no parameters." );

						// MD 10/8/07
						// Found while fixing BR27172
						// This is a better way to determine which function we are processing
						//if ( parameters == 1 && funcOperator.Function.Name == "SUM" )
						if ( parameters == 1 && funcOperator.Function == Function.SUM )
						{
							// MD 10/22/10 - TFS36696
							// We don't need to store the formula on the token anymore.
							//AttrSumToken sumToken = new AttrSumToken( this.formulaInstance );
							AttrSumToken sumToken = new AttrSumToken();

							// MD 10/22/10 - TFS36696
							// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
							//sumToken.PreceedingWhitespace = funcOperator.PreceedingWhitespace;
							List<AttrSpaceToken> funcOperatorPreceedingWhitespace;
							if (this.tokenPreceedingWhitespace.TryGetValue(funcOperator, out funcOperatorPreceedingWhitespace))
								this.tokenPreceedingWhitespace[sumToken] = funcOperatorPreceedingWhitespace;

							this.AddTokenToPostfixList( sumToken );
						}
						else
						{
							this.AddTokenToPostfixList( funcOperator );

							FunctionVOperator funcVOperator = funcOperator as FunctionVOperator;
							if ( funcVOperator != null )
								funcVOperator.NumberOfArguments = parameters;
						}
					}
					else
					{
						// MD 10/22/10 - TFS36696
						// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
						//currentToken.StartCharIndex = topOperator.StartCharIndex;
						int startCharIndex;
						if (this.tokenStartCharIndices.TryGetValue(topOperator, out startCharIndex))
							this.tokenStartCharIndices[currentToken] = startCharIndex;

						// MD 10/22/10 - TFS36696
						// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
						//if ( topOperator.PreceedingWhitespace != null )
						//{
						//    if ( currentToken.PreceedingWhitespace == null )
						//        currentToken.PreceedingWhitespace = new List<AttrSpaceToken>();
						//
						//    currentToken.PreceedingWhitespace.AddRange( topOperator.PreceedingWhitespace );
						//}
						List<AttrSpaceToken> topOperatorPreceedingWhitespace;
						if (this.tokenPreceedingWhitespace.TryGetValue(topOperator, out topOperatorPreceedingWhitespace))
						{
							List<AttrSpaceToken> currentTokenPreceedingWhitespace;
							if (this.tokenPreceedingWhitespace.TryGetValue(currentToken, out currentTokenPreceedingWhitespace))
								currentTokenPreceedingWhitespace.AddRange(topOperatorPreceedingWhitespace);
							else
								this.tokenPreceedingWhitespace[currentToken] = new List<AttrSpaceToken>(topOperatorPreceedingWhitespace);
						}

						this.AddTokenToPostfixList( currentToken );
					}

					continue;
				}

				#endregion Process the closing paren token

				#region Process operator tokens

				OperatorToken operatorToken = currentToken as OperatorToken;

				if ( operatorToken != null )
				{
					this.AddOperatorToStack( operatorStack, operatorToken );
					continue;
				}

				#endregion Process operator tokens
				
				// Process all other tokens
				this.AddTokenToPostfixList( currentToken );
			}

			// If any operators are lef ton the stack, append them to the postfix token list
			while ( operatorStack.Count > 0 )
				this.AddTokenToPostfixList( operatorStack.Pop() );
		}

		#endregion ConvertInfixToPostfix

		#region CreateNumberOperator

		private FormulaToken CreateNumberOperator( double number )
		{
			// MD 10/22/10 - TFS36696
			// We don't need to store the formula on the token anymore.
			//if ( number % 1 == 0 && UInt16.MinValue <= number && number <= UInt16.MaxValue )
			//    return new IntToken( this.formulaInstance, (ushort)number );
			//
			//return new NumberToken( this.formulaInstance, number );
			if (number % 1 == 0 && UInt16.MinValue <= number && number <= UInt16.MaxValue)
				return new IntToken((ushort)number);

			return new NumberToken(number);
		}

		#endregion CreateNumberOperator

		#region CreateWhitespaceOperators

		private List<AttrSpaceToken> CreateWhitespaceOperators( string whitespace, WhitespaceType whitespaceType )
		{
			if ( whitespace == null || whitespace.Length == 0 )
				return null;

			List<AttrSpaceToken> whitespaceTokens = new List<AttrSpaceToken>();

			MatchCollection matches = WhitespaceSectionedRegex.Matches( whitespace );

			// MD 10/22/10 - TFS36696
			// This is called often, so don't use a foreach, which is slower than a for loop.
			//foreach ( Match match in matches )
			//{
			for (int i = 0, count = matches.Count; i < count; i++)
			{
				Match match = matches[i];

				string value = match.Value;

				switch ( value[ 0 ] )
				{
					case ' ':
						this.AddWhitespaceChars( value.Length, true, whitespaceType, whitespaceTokens );
						break;
					case '\t':
						this.AddWhitespaceChars( value.Length * 4, true, whitespaceType, whitespaceTokens );
						break;
					case '\n':
						this.AddWhitespaceChars( value.Length, false, whitespaceType, whitespaceTokens );
						break;
					case '\r':
						this.AddWhitespaceChars( value.Length / 2, false, whitespaceType, whitespaceTokens );
						break;
					default:
						Utilities.DebugFail( "Unrecognized white space." );
						break;
				}
			}

			return whitespaceTokens;
		}

		#endregion CreateWhitespaceOperators

		#region GetCorrectWhitespaceType

		private static WhitespaceType GetCorrectWhitespaceType( WhitespaceType whitespaceType, bool shouldBeSpaces )
		{
			switch ( whitespaceType )
			{
				case WhitespaceType.SpacesBeforeNextToken:
				case WhitespaceType.CarriageReturnsBeforeNextToken:
					if ( shouldBeSpaces )
						return WhitespaceType.SpacesBeforeNextToken;
					else
						return WhitespaceType.CarriageReturnsBeforeNextToken;

				case WhitespaceType.SpacesBeforeOpeningParens:
				case WhitespaceType.CarriageReturnsBeforeOpeningParens:
					if ( shouldBeSpaces )
						return WhitespaceType.SpacesBeforeOpeningParens;
					else
						return WhitespaceType.CarriageReturnsBeforeOpeningParens;

				case WhitespaceType.SpacesBeforeClosingParens:
				case WhitespaceType.CarriageReturnsBeforeClosingParens:
					if ( shouldBeSpaces )
						return WhitespaceType.SpacesBeforeClosingParens;
					else
						return WhitespaceType.CarriageReturnsBeforeClosingParens;

				case WhitespaceType.SpacesFollowEqualitySign:
					return WhitespaceType.SpacesFollowEqualitySign;

				default:
					Utilities.DebugFail( "Unknown white space type" );
					return WhitespaceType.SpacesBeforeNextToken;
			}
		}

		#endregion GetCorrectWhitespaceType

		// MD 2/24/12 - 12.1 - Table Support
		#region GetKeywordText

		public static string GetKeywordText(StructuredTableReferenceKeywordType keywordType)
		{
			switch (keywordType)
			{
				case StructuredTableReferenceKeywordType.All: return FormulaParser.KeywordAll;
				case StructuredTableReferenceKeywordType.Data: return FormulaParser.KeywordData;
				case StructuredTableReferenceKeywordType.Headers: return FormulaParser.KeywordHeaders;
				case StructuredTableReferenceKeywordType.Totals: return FormulaParser.KeywordTotals;
				case StructuredTableReferenceKeywordType.ThisRow: return FormulaParser.KeywordThisRow;

				default:
					Utilities.DebugFail("Unknown StructuredTableReferenceKeywordType: " + keywordType);
					return FormulaParser.KeywordData;
			}
		}

		#endregion // GetKeywordText

		// MD 9/11/09 - TFS20376
		#region GetMatrixValueSeparatorResolved

		internal static string GetMatrixValueSeparatorResolved( string decimalSeparator )
		{
			if ( decimalSeparator == FormulaParser.UnionOperator )
				return "\\";

			return FormulaParser.UnionOperator;
		}

		#endregion GetMatrixValueSeparatorResolved

		// MD 9/11/09 - TFS20376
		#region GetUnionOperatorResolved

		// MD 5/25/11 - Data Validations / Page Breaks
		// MD 4/9/12 - TFS101506
		//internal static string GetUnionOperatorResolved()
		//{
		//    return FormulaParser.GetUnionOperatorResolved(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		//}
		internal static string GetUnionOperatorResolved(CultureInfo culture)
		{
			return FormulaParser.GetUnionOperatorResolved(culture.NumberFormat.NumberDecimalSeparator);
		}

		internal static string GetUnionOperatorResolved( string decimalSeparator )
		{
			if ( decimalSeparator == FormulaParser.UnionOperator )
			{
				// MD 10/22/10 - TFS36696
				// Moved this to a constant.
				//return ";";
				return FormulaParser.UnionOperatorAlternate;
			}

			return FormulaParser.UnionOperator;
		}

		#endregion GetUnionOperatorResolved

		// MD 6/18/12 - TFS102878
		#region GetWorksheetReference

		private WorkbookReferenceBase nullFilePathWorkbook;
		private Dictionary<string, WorkbookReferenceBase> workbookReferences;
		private WorksheetReference GetWorksheetReference(string fileName, string firstWorksheetName, string lastWorksheetName)
		{
			WorkbookReferenceBase workbookReference;
			if (fileName == null)
			{
				if (this.nullFilePathWorkbook == null)
					this.nullFilePathWorkbook = new WorkbookReferenceUnconnected(null);

				workbookReference = this.nullFilePathWorkbook;
			}
			else
			{
				if (this.workbookReferences == null)
					this.workbookReferences = new Dictionary<string, WorkbookReferenceBase>();

				if (this.workbookReferences.TryGetValue(fileName, out workbookReference) == false)
				{
					workbookReference = new WorkbookReferenceUnconnected(fileName);
					this.workbookReferences.Add(fileName, workbookReference);
				}
			}

			return workbookReference.GetWorksheetReference(firstWorksheetName, lastWorksheetName);
		}

		#endregion // GetWorksheetReference

		// MD 2/24/12 - 12.1 - Table Support
		#region IsEscapeTableColumnCharacter

		// escape_column_character = tick | "[" | "]" | "#"
		public static bool IsEscapeTableColumnCharacter(char character)
		{
			switch (character)
			{
				case '\'':
				case '[':
				case ']':
				case '#':
				case '@': // This also needs to be escaped in Excel 2010.
					return true;

				default:
					return false;
			}
		}

		#endregion // IsEscapeTableColumnCharacter

		#region IsValidNamedReference

		// MD 7/9/08 - Excel 2007 Format
		//internal static bool IsValidNamedReference( string name )
		// MD 4/6/12 - TFS101506
		//internal static bool IsValidNamedReference( string name, WorkbookFormat currentFormat )
		internal static bool IsValidNamedReference(string name, WorkbookFormat currentFormat, CultureInfo culture)
		{
			Match match = NameRegex.Match( name );

			if ( match.Success == false || match.Length != name.Length )
				return false;

			bool isColRelative = false;
			short colIndex = 0;
			int colLength;

			bool isRowRelative = false;
			int rowIndex = 0;
			int rowLength;

			// MD 7/9/08 - Excel 2007 Format
			//if ( FormulaParser.ParseColumnAddressA1( name, 0, ref colIndex, ref isColRelative, out colLength ) )
			// MD 4/6/12 - TFS101506
			//if ( FormulaParser.ParseColumnAddressA1( name, 0, currentFormat, ref colIndex, ref isColRelative, out colLength ) )
			if (FormulaParser.ParseColumnAddressA1(name, 0, currentFormat, culture, ref colIndex, ref isColRelative, out colLength))
			{
				// MD 7/9/08 - Excel 2007 Format
				//if ( FormulaParser.ParseRowAddressA1( name, colLength, ref rowIndex, ref isRowRelative, out rowLength ) )
				if ( FormulaParser.ParseRowAddressA1( name, colLength, currentFormat, ref rowIndex, ref isRowRelative, out rowLength ) )
				{
					if ( colLength + rowLength == name.Length )
						return false;
				}
			}

			// MD 7/9/08 - Excel 2007 Format
			//if ( FormulaParser.ParseRowAddressR1C1( name, 0, ref rowIndex, ref isRowRelative, out rowLength ) )
			if ( FormulaParser.ParseRowAddressR1C1( name, 0, currentFormat, ref rowIndex, ref isRowRelative, out rowLength ) )
				return false;

			// MD 7/9/08 - Excel 2007 Format
			//if ( FormulaParser.ParseColumnAddressR1C1( name, 0, ref colIndex, ref isColRelative, out colLength ) )
			if ( FormulaParser.ParseColumnAddressR1C1( name, 0, currentFormat, ref colIndex, ref isColRelative, out colLength ) )
				return false;

			return true;
		}

		#endregion IsValidNamedReference

		#region RestorePosition

		private void RestorePosition( PositionInfo position )
		{
			this.startIndex = position.StartIndex;
			this.nextWhitespace = position.NextWhitespace;
			this.infixTokenList.RemoveRange( position.TokenCount, this.infixTokenList.Count - position.TokenCount );
		}

		#endregion RestorePosition

		#region SavePosition

		private PositionInfo SavePosition()
		{
			return new PositionInfo( this.startIndex, this.infixTokenList.Count, this.nextWhitespace );
		}

		#endregion SavePosition

		// MD 9/16/08
		// Found while unit testing
		// It turns out this will never be used
		#region Not Used

		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		#endregion Not Used

		#region ShouldWorksheetNameBeQuoted

		internal static bool ShouldWorksheetNameBeQuoted( string worksheetName )
		{
			Match match = WorksheetNameSafeRegex.Match( worksheetName );

			return match.Success == false || match.Length != worksheetName.Length;
		}

		#endregion ShouldWorksheetNameBeQuoted

		#region TestCurrentChar

		private bool TestCurrentChar( string expectedChar )
		{
			Debug.Assert( expectedChar.Length == 1 );
			return this.TestCurrentChar( expectedChar[ 0 ] );
		}

		private bool TestCurrentChar( char expectedChar )
		{
			if ( this.startIndex >= this.formula.Length )
				return false;

			return this.formula[ this.startIndex ] == expectedChar;
		}

		#endregion TestCurrentChar

		#region TestCurrentCharAndAdvance

		private bool TestCurrentCharAndAdvance( string expectedChar )
		{
			Debug.Assert( expectedChar.Length == 1 );
			return TestCurrentCharAndAdvance( expectedChar[ 0 ] );
		}

		private bool TestCurrentCharAndAdvance( char expectedChar )
		{
			if ( this.startIndex >= this.formula.Length )
				return false;

			if ( this.formula[ this.startIndex ] != expectedChar )
				return false;

			this.startIndex++;
			return true;
		}

		#endregion TestCurrentCharAndAdvance

		// MD 12/7/11 - 12.1 - Table Support
		#region TestForStringAndAdvance

		private bool TestForStringAndAdvance(string expectedString)
		{
			return this.TestForStringAndAdvance(expectedString, true);
		}

		private bool TestForStringAndAdvance(string expectedString, bool ignoreCase)
		{
			if ((this.startIndex + expectedString.Length) > this.formula.Length)
				return false;

			for (int i = 0; i < expectedString.Length; i++)
			{
				char currentChar = this.formula[this.startIndex + i];
				char expectedChar = expectedString[i];

				if (ignoreCase)
				{
					currentChar = Char.ToLowerInvariant(currentChar);
					expectedChar = Char.ToLowerInvariant(expectedChar);
				}

				if (currentChar != expectedChar)
					return false;
			}

			this.startIndex += expectedString.Length;
			return true;
		}

		#endregion // TestForStringAndAdvance

		#endregion Methods

		#region Properties

		// MBS 7/11/08 - Excel 2007 Format
        #region MaxFormulaLength

        private int MaxFormulaLength
        {
            get
            {
				// MD 2/4/11
				// Done while fixing TFS65015
				// Use the new Utilities.Is2003Format method so we don't need to switch on the format all over the place.
				//switch (this.currentFormat)
				//{
				//    case WorkbookFormat.Excel97To2003:
				//    // MD 5/7/10 - 10.2 - Excel Templates
				//    case WorkbookFormat.Excel97To2003Template:
				//        return FormulaParser.MaxFormulaLength97To2003;
				//
				//    case WorkbookFormat.Excel2007:
				//    // MD 10/1/08 - TFS8471
				//    case WorkbookFormat.Excel2007MacroEnabled:
				//    // MD 5/7/10 - 10.2 - Excel Templates
				//    case WorkbookFormat.Excel2007MacroEnabledTemplate:
				//    case WorkbookFormat.Excel2007Template:
				//        return FormulaParser.MaxFormulaLength2007;
				//
				//    default:
				//        Utilities.DebugFail("Unkown workbook format: " + this.currentFormat);
				//        goto case WorkbookFormat.Excel97To2003;
				//}
				if (Utilities.Is2003Format(this.currentFormat))
					return FormulaParser.MaxFormulaLength97To2003;

				return FormulaParser.MaxFormulaLength2007;
            }
        }
        #endregion //MaxFormulaLength

        // MBS 7/10/08 - Excel 2007 Format
        #region MaxFunctionNesting

        private int MaxFunctionNesting
        {
            get
            {
				// MD 2/4/11
				// Done while fixing TFS65015
				// Use the new Utilities.Is2003Format method so we don't need to switch on the format all over the place.
				//switch (this.currentFormat)
				//{
				//    case WorkbookFormat.Excel97To2003:
				//    // MD 5/7/10 - 10.2 - Excel Templates
				//    case WorkbookFormat.Excel97To2003Template:
				//        return FormulaParser.MaxFunctionNesting97To2003;
				//
				//    case WorkbookFormat.Excel2007:
				//    // MD 10/1/08 - TFS8471
				//    case WorkbookFormat.Excel2007MacroEnabled:
				//    // MD 5/7/10 - 10.2 - Excel Templates
				//    case WorkbookFormat.Excel2007MacroEnabledTemplate:
				//    case WorkbookFormat.Excel2007Template:
				//        return FormulaParser.MaxFunctionNesting2007;
				//
				//    default:
				//        Utilities.DebugFail("Unkown workbook format: " + this.currentFormat);
				//        goto case WorkbookFormat.Excel97To2003;
				//}
				if (Utilities.Is2003Format(this.currentFormat))
					return FormulaParser.MaxFunctionNesting97To2003;

				return FormulaParser.MaxFunctionNesting2007;
            }
        }
        #endregion //MaxFunctionNesting

        #region ParseException

        private FormulaParseException ParseException
		{
			get { return this.parseException; }
			set
			{
				Debug.Assert( this.parseException == null );
				this.parseException = value;
			}
		}

		#endregion ParseException

		// MD 12/7/11 - 12.1 - Table Support
		#region StructuredTableReferenceParserHelper

		private StructuredTableReferenceParser StructuredTableReferenceParserHelper
		{
			get
			{
				if (this.structuredTableReferenceParserHelper == null)
					this.structuredTableReferenceParserHelper = new StructuredTableReferenceParser(this);

				return this.structuredTableReferenceParserHelper;
			}
		}

		#endregion // StructuredTableReferenceParserHelper

		#endregion Properties

		#region Regex cached values

		// MD 4/18/08 - BR32154
		// Made each static variable thread static so we do not get into a race condition when creating them
		// MD 9/11/09 - TFS20376
		// This cannot be static becasue it is based on the culture.
		//[ThreadStatic]
		//private static Regex binaryRegex;
		private Regex binaryRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex columnAddressA1Regex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex columnAddressR1C1Regex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex errorRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex endQuoteRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex fileNameBracketedSafeRegex;

		// MD 9/16/08
		// Found while unit testing
		// It turns out this will never be used
		//[ThreadStatic] // MD 4/18/08 - BR32154
		//private static Regex fileNameSafeRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex fileNameQuotedRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex functionNameRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex localPathRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex nameRegex;

		// MD 9/11/09 - TFS20376
		// This cannot be static becasue it is based on the culture.
		//[ThreadStatic] // MD 4/18/08 - BR32154
		//private static Regex numberRegex;
		private Regex numberRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex rowAddressA1Regex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex rowAddressR1C1Regex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex stringRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex urlPathRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex whitespaceRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex whitespaceSectionedRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex worksheetNameSafeRegex;

		[ThreadStatic] // MD 4/18/08 - BR32154
		private static Regex worksheetNameQuotedRegex;

		#region BinaryRegex

		// MD 10/22/10 - TFS36696
		[ThreadStatic]
		private static Regex defaultBinaryRegex;

		// MD 10/22/10 - TFS36696
		[ThreadStatic]
		private static Regex alternateBinaryRegex;

		// MD 9/11/09 - TFS20376
		// This cannot be static becasue it is based on the culture.
		//private static Regex BinaryRegex
		private Regex BinaryRegex
		{
			get
			{
				if ( binaryRegex == null )
				{
					// MD 10/22/10 - TFS36696
					// We don't want to create a new Regex for each instance of the formula parser, so will will keep one cached for each union operator that can be used.
					// Create the cached instance if necessary and set it on the binaryRegex member.
					//binaryRegex = new Regex( 
					//    String.Format( 
					//    CultureInfo.InvariantCulture,
					//    // MD 10/23/07 - BR27754
					//    // This regex was ordered incorrectly. We can't match on '<' before matching on '<=', 
					//    // because we would never find the later. It would always consume the '<' and continue 
					//    // parsing the rest of the formula starting at the '='. The same is true for '>' signs.
					//    //@"\G(\+|-|\*|/|\^|=|\<\>|\<|\>|\<=|\>=|{0}|{1}|{2})", 
					//    @"\G(\+|-|\*|/|\^|=|\<\>|\<=|\>=|\<|\>|{0}|{1}|{2})", 
					//    FormulaParser.RangeOperator,
					//
					//    // MD 9/11/09 - TFS20376
					//    //FormulaParser.UnionOperator,
					//    Regex.Escape( this.unionOperatorResolved ),
					//
					//    FormulaParser.ConcatOperator ), 
					//    Utilities.RegexOptionsCompiled );
					if (this.unionOperatorResolved == FormulaParser.UnionOperator)
					{
						binaryRegex = FormulaParser.GetBinaryRegex(ref FormulaParser.defaultBinaryRegex, this.unionOperatorResolved);
					}
					else
					{
						Debug.Assert(this.unionOperatorResolved == FormulaParser.UnionOperatorAlternate, "Unknown union operator.");
						binaryRegex = FormulaParser.GetBinaryRegex(ref FormulaParser.alternateBinaryRegex, this.unionOperatorResolved);
					}
				}

				return binaryRegex;
			}
		}

		// MD 10/22/10 - TFS36696
		private static Regex GetBinaryRegex(ref Regex staticRegexCache, string unionOperator)
		{
			if (staticRegexCache == null)
			{
				staticRegexCache = new Regex(
					String.Format(
					CultureInfo.InvariantCulture,
					// MD 10/23/07 - BR27754
					// This regex was ordered incorrectly. We can't match on '<' before matching on '<=', 
					// because we would never find the later. It would always consume the '<' and continue 
					// parsing the rest of the formula starting at the '='. The same is true for '>' signs.
					//@"\G(\+|-|\*|/|\^|=|\<\>|\<|\>|\<=|\>=|{0}|{1}|{2})", 
					@"\G(\+|-|\*|/|\^|=|\<\>|\<=|\>=|\<|\>|{0}|{1}|{2})",
					FormulaParser.RangeOperator,

					// MD 9/11/09 - TFS20376
					//FormulaParser.UnionOperator,
					Regex.Escape(unionOperator),

					FormulaParser.ConcatOperator),
					Utilities.RegexOptionsCompiled);
			}

			return staticRegexCache;
		}

		#endregion BinaryRegex

		#region ColumnAddressA1Regex

		private static Regex ColumnAddressA1Regex
		{
			get
			{
				if ( columnAddressA1Regex == null )
				{
					// MD 5/4/09 - TFS17197
					// If the letters for the column address are followed by any letters (with or without accents), 
					// it is not considered a column address. The previous check was only looking for non-accented letters.
					//columnAddressA1Regex = new Regex( @"\G\$?[a-zA-Z]{1,3}(?![a-zA-Z])", Utilities.RegexOptionsCompiled );
					// MD 3/29/10 - TFS30075
					// If what would be a column address is followed by an underscore, it should not be considered a column range.
					// This can happen in named reference names or function names.
					//columnAddressA1Regex = new Regex( @"\G\$?[a-zA-Z]{1,3}(?!\p{L})", Utilities.RegexOptionsCompiled );
					columnAddressA1Regex = new Regex(@"\G\$?[a-zA-Z]{1,3}(?![\p{L}_])", Utilities.RegexOptionsCompiled);
				}

				return columnAddressA1Regex;
			}
		}

		#endregion ColumnAddressA1Regex

		#region ColumnAddressR1C1Regex

		private static Regex ColumnAddressR1C1Regex
		{
			get
			{
				if ( columnAddressR1C1Regex == null )
				{
					// MD 8/20/07 - BR25818
					// Changed the regex to make sure if there is no column number after the C, 
					// there are also no other alpha characters
					//columnAddressR1C1Regex = new Regex( @"\GC((\[[-+]?[0-9]{1,6}])|([0-9]{1,6}(?![0-9])))?", Utilities.RegexOptionsCompiled );
					// MD 5/4/09 - TFS17197
					// If the 'C' is followed by any letters (with or without accents), it is not considered a column address.
					// The previous check was only looking for non-accented letters.
					//columnAddressR1C1Regex = new Regex( @"\GC(?![a-zA-Z])((\[[-+]?[0-9]{1,6}])|([0-9]{1,6}(?![0-9])))?", Utilities.RegexOptionsCompiled );
					// MD 3/29/10 - TFS30075
					// If what would be a column address is followed by an underscore, it should not be considered a column range.
					// This can happen in named reference names or function names.
					//columnAddressR1C1Regex = new Regex( @"\GC(?!\p{L})((\[[-+]?[0-9]{1,6}])|([0-9]{1,6}(?![0-9])))?", Utilities.RegexOptionsCompiled );
					columnAddressR1C1Regex = new Regex(@"\GC(?![\p{L}_])((\[[-+]?[0-9]{1,6}])|([0-9]{1,6}(?![0-9])))?", Utilities.RegexOptionsCompiled);
				}

				return columnAddressR1C1Regex;
			}
		}

		#endregion ColumnAddressR1C1Regex

		#region ErrorRegex

		private static Regex ErrorRegex
		{
			get
			{
				if ( errorRegex == null )
				{
					// MD 7/14/08
					// Found while implementing Excel formula solving
					// The '?' in the name character needed to be escaped
					//errorRegex = new Regex( @"\G(#NULL!|#DIV/0!|#VALUE!|#REF!|#NAME?|#NUM!|#N/A)", Utilities.RegexOptionsCompiled | RegexOptions.IgnoreCase );
					errorRegex = new Regex( @"\G(#NULL!|#DIV/0!|#VALUE!|#REF!|#NAME\?|#NUM!|#N/A)", Utilities.RegexOptionsCompiled | RegexOptions.IgnoreCase );
				}

				return errorRegex;
			}
		}

		#endregion ErrorRegex

		#region EndQuoteRegex

		private static Regex EndQuoteRegex
		{
			get
			{
				if ( endQuoteRegex == null )
					endQuoteRegex = new Regex( @"(?<=(?<!')('')*)'(?!')", Utilities.RegexOptionsCompiled );

				return endQuoteRegex;
			}
		}

		#endregion EndQuoteRegex

		#region FileNameBracketedSafeRegex

		private static Regex FileNameBracketedSafeRegex
		{
			get
			{
				if ( fileNameBracketedSafeRegex == null )
					fileNameBracketedSafeRegex = new Regex( @"\G\[[_a-zA-Z0-9����][_a-zA-Z0-9����.]*\]", Utilities.RegexOptionsCompiled );

				return fileNameBracketedSafeRegex;
			}
		}

		#endregion FileNameBracketedSafeRegex

		// MD 9/16/08
		// Found while unit testing
		// It turns out this will never be used
		#region Not Used

		
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


		#endregion Not Used

		#region FileNameQuotedRegex

		private static Regex FileNameQuotedRegex
		{
			get
			{
				if ( fileNameQuotedRegex == null )
					fileNameQuotedRegex = new Regex( @"\G([^\\/:*?""<>|[\]']|(''))+", Utilities.RegexOptionsCompiled );

				return fileNameQuotedRegex;
			}
		}

		#endregion FileNameQuotedRegex

		#region FunctionNameRegex

		private static Regex FunctionNameRegex
		{
			get
			{
				if ( functionNameRegex == null )
				{
					// MD 10/9/07 - BR27172
					// Periods are allowed in function named
					//functionNameRegex = new Regex( @"\G[a-zA-Z][a-zA-Z0-9]*(?=\()", Utilities.RegexOptionsCompiled );
					// MD 3/16/10 - TFS25993
					// Unicode letter characters are allowed in custom functions, so we should use the \p{L} code, which matches any letter character.
					//functionNameRegex = new Regex( @"\G[a-zA-Z](\.?[a-zA-Z0-9])*(?=\()", Utilities.RegexOptionsCompiled );
					// MD 2/4/11 - TFS65015
					// In the 2007 format, add-in function names are prefaced by "_xll.".
					//functionNameRegex = new Regex(@"\G\p{L}(\.?[\p{L}0-9])*(?=\()", Utilities.RegexOptionsCompiled);
					functionNameRegex = new Regex(@"\G(_xll.)?\p{L}(\.?[\p{L}0-9])*(?=\()", Utilities.RegexOptionsCompiled);
				}

				return functionNameRegex;
			}
		}

		#endregion FunctionNameRegex

		#region LocalPathRegex

		private static Regex LocalPathRegex
		{
			get
			{
				if ( localPathRegex == null )
                    // MBS 9/11/08 - Excel 2007
                    // We weren't accounting for a valid path such as "\\somenetworkdrive\Book1.xlsx"
                    //
					//localPathRegex = new Regex( @"\G[a-zA-Z]:\\(([^\\/:*?""<>|[\]']|(''))*\\)*", Utilities.RegexOptionsCompiled );
                    localPathRegex = new Regex(@"\G(([a-zA-Z]:)|\\)\\(([^\\/:*?""<>|[\]']|(''))*\\)*", Utilities.RegexOptionsCompiled);

				return localPathRegex;
			}
		}

		#endregion LocalPathRegex

		#region NameRegex

		private static Regex NameRegex
		{
			get
			{
				if ( nameRegex == null )
				{
					// MD 5/4/09 - TFS17197
					// Accent characters are allowed in the named refernce as well, but the "a-zA-Z" values only cover
					// non-accent characters. "\p{L}" covers any letter character in any language.
					//nameRegex = new Regex( @"\G[\\_����a-zA-Z][\\_����a-zA-Z0-9.]*", Utilities.RegexOptionsCompiled );
					// MD 4/4/12 - TFS100966
					// Named reference names can also start with a bracketed expression.
					//nameRegex = new Regex( @"\G[\\_����\p{L}][\\_����\p{L}0-9.]*", Utilities.RegexOptionsCompiled );
					nameRegex = new Regex(@"\G(\[[\\_����\p{L}0-9.]+\])?[\\_����\p{L}][\\_����\p{L}0-9.]*", Utilities.RegexOptionsCompiled);
				}

				return nameRegex;
			}
		}

		#endregion NameRegex

		#region NumberRegex

		// MD 10/22/10 - TFS36696
		[ThreadStatic]
		private static Regex defaultNumberRegex;

		// MD 10/22/10 - TFS36696
		[ThreadStatic]
		private static Dictionary<string, Regex> numberRegexByDecimalSeparator;

		// MD 9/11/09 - TFS20376
		// This cannot be static becasue it is based on the culture.
		//private static Regex NumberRegex
		private Regex NumberRegex
		{
			get
			{
				// MD 9/11/09 - TFS20376
				// We need to the decimal separator from the culture.
				//if ( numberRegex == null )
				//    numberRegex = new Regex( @"\G(([-+]?[0-9]+(\.[0-9]+)?)|(\.[0-9]+))([eE][-+]?[0-9]+)?", Utilities.RegexOptionsCompiled );
				if ( numberRegex == null )
				{
					// MD 10/22/10 - TFS36696
					// We don't want to create a new Regex for each instance of the formula parser, so will will keep one cached for each union operator that can be used.
					// Create the cached instance if necessary and set it on the binaryRegex member.
					//string decimalSeparator = Regex.Escape( this.cachedDecimalSeparator );
					//numberRegex = new Regex(
					//    @"\G(([-+]?[0-9]+(" + decimalSeparator +
					//    @"[0-9]+)?)|(" + decimalSeparator + 
					//    @"[0-9]+))([eE][-+]?[0-9]+)?", Utilities.RegexOptionsCompiled );
					if (this.cachedDecimalSeparator == ".")
					{
						if (defaultNumberRegex == null)
							defaultNumberRegex = FormulaParser.CreateNumberRegex(this.cachedDecimalSeparator);

						numberRegex = defaultNumberRegex;
					}
					else
					{
						if (numberRegexByDecimalSeparator == null)
						{
							numberRegexByDecimalSeparator = new Dictionary<string, Regex>();
						}
						else
						{
							if (numberRegexByDecimalSeparator.TryGetValue(this.cachedDecimalSeparator, out numberRegex))
								return numberRegex;
						}

						numberRegex = FormulaParser.CreateNumberRegex(this.cachedDecimalSeparator);
						numberRegexByDecimalSeparator[this.cachedDecimalSeparator] = numberRegex;
					}
				}

				return numberRegex;
			}
		}

		// MD 10/22/10 - TFS36696
		private static Regex CreateNumberRegex(string decimalSeparator)
		{
			string escapedDecimalSeparator = Regex.Escape(decimalSeparator);
			return new Regex(
				@"\G(([-+]?[0-9]+(" + escapedDecimalSeparator +
				@"[0-9]+)?)|(" + escapedDecimalSeparator +
				@"[0-9]+))([eE][-+]?[0-9]+)?", Utilities.RegexOptionsCompiled);
		}

		#endregion NumberRegex

		#region RowAddressA1Regex

		private static Regex RowAddressA1Regex
		{
			get
			{
				if ( rowAddressA1Regex == null )
				{
					// MD 7/9/08 - Excel 2007 Format
					// The row address can now have 7 digits
					//rowAddressA1Regex = new Regex( @"\G\$?[0-9]{1,6}(?![0-9])", Utilities.RegexOptionsCompiled );
					// MD 6/30/11 - TFS79626
					// The A1 row address cannot be followed by letter or underscore either.
					//rowAddressA1Regex = new Regex( @"\G\$?[0-9]{1,7}(?![0-9])", Utilities.RegexOptionsCompiled );
					rowAddressA1Regex = new Regex(@"\G\$?[0-9]{1,7}(?![0-9\p{L}_])", Utilities.RegexOptionsCompiled);
				}

				return rowAddressA1Regex;
			}
		}

		#endregion RowAddressA1Regex

		#region RowAddressR1C1Regex

		private static Regex RowAddressR1C1Regex
		{
			get
			{
				if ( rowAddressR1C1Regex == null )
				{
					// MD 8/20/07 - BR25818
					// Changed the regex to make sure if there is no row number after the R, 
					// there are also no other alpha characters
					//rowAddressR1C1Regex = new Regex( @"\GR((\[[-+]?[0-9]{1,6}])|([0-9]{1,6}(?![0-9])))?", Utilities.RegexOptionsCompiled );
					// MD 10/10/07 - BR27276
					// This was caused by BR25818.
					// If the refernce looks something like this "RC1", the 'R' can be followed by another alpha character, 
					// so only allow the 'C' after the 'R'
					//rowAddressR1C1Regex = new Regex( @"\GR(?![a-zA-Z])((\[[-+]?[0-9]{1,6}])|([0-9]{1,6}(?![0-9])))?", Utilities.RegexOptionsCompiled );
					// MD 1/23/08 - BR27172
					// This was caused by the fix for BR27276
					// The regex had the or condition added to the end of it, but because a larger set of parens didn't contain
					// all or conditions, the \G at the beginning of the regex did not apply to the last condition (R(?=C)).
					// Therefore, any name with RC anywhere in it would incorrectly appear ot be a row address.
					// I also noticed that a named reference like 'RCA' should be allowed, but it is viewed as starting with a row
					// address, which is incorrect. Only 'R' followed by 'C' with no more alpha characters after it should be a row
					// address, so (?![a-zA-Z]) was added after the C in the last row address option. This means it must be an 'R'
					// that is immediately followed by a 'C' which is not immediately followed by another alpha character.
					//rowAddressR1C1Regex = new Regex( @"\G(R(?![a-zA-Z])((\[[-+]?[0-9]{1,6}])|([0-9]{1,6}(?![0-9])))?)|(R(?=C))", Utilities.RegexOptionsCompiled );
					// MD 7/9/08 - Excel 2007 Format
					// The row address can now have 7 digits
					//rowAddressR1C1Regex = new Regex( @"\G((R(?![a-zA-Z])((\[[-+]?[0-9]{1,6}])|([0-9]{1,6}(?![0-9])))?)|(R(?=C(?![a-zA-Z]))))", Utilities.RegexOptionsCompiled );
                    // MD 5/4/09 - TFS17197
					// If the 'R' is followed by any letters (with or without accents), it is not considered a column address.
					// Similarly, if the 'RC' is followed by any letters (with or without accents), it is not considered a column address.
					// The previous check was only looking for non-accented letters.
					//rowAddressR1C1Regex = new Regex( @"\G((R(?![a-zA-Z])((\[[-+]?[0-9]{1,7}])|([0-9]{1,7}(?![0-9])))?)|(R(?=C(?![a-zA-Z]))))", Utilities.RegexOptionsCompiled );
					// MD 3/29/10 - TFS30075
					// If what would be a column address is followed by an underscore, it should not be considered a column range.
					// This can happen in named reference names or function names.
					//rowAddressR1C1Regex = new Regex( @"\G((R(?!\p{L})((\[[-+]?[0-9]{1,7}])|([0-9]{1,7}(?![0-9])))?)|(R(?=C(?!\p{L}))))", Utilities.RegexOptionsCompiled );
					rowAddressR1C1Regex = new Regex(@"\G((R(?![\p{L}_])((\[[-+]?[0-9]{1,7}])|([0-9]{1,7}(?![0-9])))?)|(R(?=C(?![\p{L}_]))))", Utilities.RegexOptionsCompiled);
				}

				return rowAddressR1C1Regex;
			}
		}

		#endregion RowAddressR1C1Regex

		#region StringRegex

		private static Regex StringRegex
		{
			get
			{
				if ( stringRegex == null )
					stringRegex = new Regex( @"\G""([^""]|"""")*""", Utilities.RegexOptionsCompiled );

				return stringRegex;
			}
		}

		#endregion StringRegex

		#region UrlPathRegex

		private static Regex UrlPathRegex
		{
			get
			{
				if ( urlPathRegex == null )
				{
					// MD 9/16/08
					// Found while unit testing
					// Both forward and back slashes are allowed by Excel.
					//urlPathRegex = new Regex( @"\G(ht|f)tp(s?)\://[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(/?)([a-zA-Z0-9\-\.\?\,'/\\\+&%\$#_]*)?/", Utilities.RegexOptionsCompiled );
					// MD 2/24/11 - TFS66838
					// We need to allow for sub folders under the main URL.
					//urlPathRegex = new Regex( @"\G(ht|f)tp(s?)\:(\\|/){2}[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(/?)([a-zA-Z0-9\-\.\?\,'/\\\+&%\$#_]*)?(\\|/)", Utilities.RegexOptionsCompiled );
					urlPathRegex = new Regex(@"\G(ht|f)tp(s?)\:(\\|/){2}([0-9a-zA-Z]([-.\w\s]*[0-9a-zA-Z])*(\\|/)?)*(:(0-9)*)*(/?)([a-zA-Z0-9\-\.\?\,'/\\\+&%\$#_]*)?(\\|/)", Utilities.RegexOptionsCompiled);
				}

				return urlPathRegex;
			}
		}

		#endregion UrlPathRegex

		#region WhitespaceRegex

		private static Regex WhitespaceRegex
		{
			get
			{
				if ( whitespaceRegex == null )
					whitespaceRegex = new Regex( @"\G\s*", Utilities.RegexOptionsCompiled );

				return whitespaceRegex;
			}
		}

		#endregion WhitespaceRegex

		#region WhitespaceSectionedRegex

		private static Regex WhitespaceSectionedRegex
		{
			get
			{
				if ( whitespaceSectionedRegex == null )
                    whitespaceSectionedRegex = new Regex(@"(\t(\t)*)|(\r\n(\r\n)*)|(\n(\n)*)|( ( )*)", Utilities.RegexOptionsCompiled);

				return whitespaceSectionedRegex;
			}
		}

		#endregion WhitespaceSectionedRegex

		#region WorksheetNameSafeRegex

		private static Regex WorksheetNameSafeRegex
		{
			get
			{
				if ( worksheetNameSafeRegex == null )
				{
					// MD 3/16/10 - TFS25993
					// Unicode letter characters are allowed in unquoted worksheet names, so we should use the \p{L} code, which matches any letter character.
					//worksheetNameSafeRegex = new Regex( @"\G[_a-zA-Z����][_a-zA-Z0-9����.]*", Utilities.RegexOptionsCompiled );
					// MD 3/31/11 - TFS70647
					// We can use unicode numerical digits as well, so use \p{Nd} instead of 0-9.
					//worksheetNameSafeRegex = new Regex(@"\G[_\p{L}����][_\p{L}0-9����.]*", Utilities.RegexOptionsCompiled);
					// MD 1/24/12 - TFS100044
					// Ohter symbols are valid as well in non-quoted worksheet names. I have chanhed the regex to allow for symbols anywhere in the name.
					//worksheetNameSafeRegex = new Regex(@"\G[_\p{L}����][_\p{L}\p{Nd}����.]*", Utilities.RegexOptionsCompiled);
					// MD 2/15/12 - TFS101885
					// Using the \p{S} regex pattern recognizes the math operators, but these should not be allowed in unquoted worksheet names.
					// Use the \p{So} pattern instead.
					//worksheetNameSafeRegex = new Regex(@"\G[_\p{L}\p{S}][_\p{L}\p{Nd}\p{S}����.]*", Utilities.RegexOptionsCompiled);
					// MD 6/28/12 - TFS115695
					// Some other numeric and symbol characters are allowed in the worksheet name.
					//worksheetNameSafeRegex = new Regex(@"\G[_\p{L}\p{So}����][_\p{L}\p{Nd}\p{So}����.]*", Utilities.RegexOptionsCompiled);
					worksheetNameSafeRegex = new Regex(@"\G[_\p{L}\p{So}\p{No}\p{Nl}\p{Mn}����][_\p{L}\p{So}\p{N}\p{Mn}����.]*", Utilities.RegexOptionsCompiled);
				}

				return worksheetNameSafeRegex;
			}
		}

		#endregion WorksheetNameSafeRegex

		#region WorksheetNameQuotedRegex

		private static Regex WorksheetNameQuotedRegex
		{
			get
			{
				if ( worksheetNameQuotedRegex == null )
					worksheetNameQuotedRegex = new Regex( @"\G([^\*\[\]:'/\\\?]|(''))+", Utilities.RegexOptionsCompiled );

				return worksheetNameQuotedRegex;
			}
		}

		#endregion WorksheetNameQuotedRegex

		#endregion Regex cached values


		#region PositionInfo class

		private class PositionInfo
		{
			private string nextWhitespace;
			private int startIndex;
			private int tokenCount;

			public PositionInfo( int startIndex, int tokenCount, string nextWhitespace )
			{
				this.startIndex = startIndex;
				this.tokenCount = tokenCount;
				this.nextWhitespace = nextWhitespace;
			}

			public string NextWhitespace
			{
				get { return this.nextWhitespace; }
			}

			public int StartIndex
			{
				get { return this.startIndex; }
			}

			public int TokenCount
			{
				get { return this.tokenCount; }
			}
		}

		#endregion PositionInfo class

		// MD 12/7/11 - 12.1 - Table Support
		#region StructuredTableReferenceParser class

		private class StructuredTableReferenceParser
		{
			#region Member Variables

			private FormulaParser _owner;

			#endregion // Member Variables

			#region Constructor

			public StructuredTableReferenceParser(FormulaParser owner)
			{
				_owner = owner;
			}

			#endregion // Constructor

			#region Methods

			#region Parse

			// intra_table_reference = spaced_lbracket inner_reference spaced_rbracket | keyword | ("[" [simple_column_name] "]")
			public bool Parse(string filePath, string tableName, string whitespaceBeforeReference)
			{
				// Structured table references only apply in 2007 and later formats.
				if (Utilities.Is2003Format(_owner.currentFormat))
					return false;

				if(_owner.ParseException != null)
					return false;

				PositionInfo position = _owner.SavePosition();

				StructuredTableReference tokenReferenceToken = null;
				try
				{
					bool hasSpaceBefore;
					if (this.ParseSpacedLBracket(out hasSpaceBefore))
					{
						StructuredTableReference.InnerReference innerReference = this.ParseInnerReference();

						bool hasSpaceAfter;
						if (innerReference != null && this.ParseSpacedRBracket(out hasSpaceAfter))
						{
							innerReference.HasSpaceBefore = hasSpaceBefore;
							innerReference.HasSpaceAfter = hasSpaceAfter;

							if (innerReference.IsSimpleColumnReference)
								tokenReferenceToken = new StructuredTableReference(filePath, tableName, innerReference.FirstColumnName, _owner.currentFormat);
							else
								tokenReferenceToken = new StructuredTableReference(filePath, tableName, innerReference, _owner.currentFormat);

							return true;
						}

						_owner.RestorePosition(position);
					}

					StructuredTableReferenceKeywordType? keyword = this.ParseKeyword();
					if (keyword.HasValue)
					{
						tokenReferenceToken = new StructuredTableReference(filePath, tableName, keyword.Value, _owner.currentFormat);
						return true;
					}

					if (_owner.TestCurrentCharAndAdvance('['))
					{
						int indexAfterBracket = _owner.startIndex;

						string simpleColumnName = this.ParseSimpleColumnName(true);
						if (_owner.TestCurrentCharAndAdvance(']'))
						{
							tokenReferenceToken = new StructuredTableReference(filePath, tableName, simpleColumnName, _owner.currentFormat);
							return true;
						}

						_owner.ParseException = new FormulaParseException(
							position.StartIndex,
							_owner.formula,
							SR.GetString("LE_FormulaParseException_InvalidStructuredTableReference"),
							_owner.formula.Substring(position.StartIndex, indexAfterBracket - position.StartIndex));
					}

					return false;
				}
				finally
				{
					if (tokenReferenceToken != null)
					{
						_owner.AddTokenAndWhitespace(
							tokenReferenceToken,
							position.StartIndex,
							_owner.startIndex - position.StartIndex,
							whitespaceBeforeReference,
							WhitespaceType.SpacesBeforeNextToken);
					}
				}
			}

			#endregion // Parse

			#region ParseAnyColumnCharacter

			// any_column_character = any_nospace_column_character | space
			private bool ParseAnyColumnCharacter(bool isColumnNameBracketed, out char columnCharacter)
			{
				if (this.ParseAnyNospaceColumnCharacter(isColumnNameBracketed, out columnCharacter))
					return true;

				if (_owner.TestCurrentCharAndAdvance(' '))
					return true;

				return false;
			}

			#endregion // ParseAnyColumnCharacter

			#region ParseAnyNospaceColumnCharacter

			// any_nospace_column_character = unescaped_column_character | (tick escape_column_character)
			private bool ParseAnyNospaceColumnCharacter(bool isColumnNameBracketed, out char columnCharacter)
			{
				if (this.ParseUnescapedColumnCharacter(isColumnNameBracketed, out columnCharacter))
					return true;

				// Even though the grammar states that only escape_column_character is valid after the tick mark, in practice it seems like any 
				// character is valid after a tick mark.
				if (_owner.TestCurrentCharAndAdvance('\'') && _owner.startIndex < _owner.formula.Length - 1)
				{
					columnCharacter = _owner.formula[_owner.startIndex];
					_owner.startIndex++;
					return true;
				}

				return false;
			}

			#endregion // ParseAnyNospaceColumnCharacter

			#region ParseColumn

			// In practice, the spaces cannot be between the brackets and the column name
			// column = simple_column_name | ("[" *space simple_column_name *space "]")
			private string ParseColumn()
			{
				string simpleColumnName = this.ParseSimpleColumnName(false);
				if (simpleColumnName != null)
					return simpleColumnName;

				if (_owner.TestCurrentCharAndAdvance('['))
				{
					PositionInfo position = _owner.SavePosition();

					simpleColumnName = this.ParseSimpleColumnName(true);
					if (simpleColumnName != null &&
						_owner.TestCurrentCharAndAdvance(']'))
					{
						return simpleColumnName;
					}

					_owner.RestorePosition(position);
					return null;
				}

				return null;
			}

			#endregion // ParseColumn

			#region ParseColumnRange

			// column_range = column [":" column]
			private bool ParseColumnRange(out string firstColumn, out string lastColumn)
			{
				lastColumn = null;

				firstColumn = this.ParseColumn();
				if (firstColumn == null)
					return false;

				PositionInfo position = _owner.SavePosition();
				if (_owner.TestCurrentCharAndAdvance(':'))
				{
					lastColumn = this.ParseColumn();
					if (lastColumn != null)
						return true;

					_owner.RestorePosition(position);
					return false;
				}

				return true;
			}

			#endregion // ParseColumnRange

			#region ParseInnerReference

			// inner_reference = keyword_list | ([keyword_list spaced_comma] column_range)
			private StructuredTableReference.InnerReference ParseInnerReference()
			{
				StructuredTableReferenceKeywordType? firstKeyword;
				StructuredTableReferenceKeywordType? lastKeyword;
				bool hasSpaceAfterFirstKeyword;
				string firstColumn;
				string lastColumn;

				// In Excel 2010, we can also have the inner reference be of the form @ *space column_range
				PositionInfo position = _owner.SavePosition();
				if (_owner.TestCurrentCharAndAdvance('@'))
				{
					while (_owner.TestCurrentCharAndAdvance(' ')) ;
					if (this.ParseColumnRange(out firstColumn, out lastColumn))
					{
						return new StructuredTableReference.InnerReference(
							StructuredTableReferenceKeywordType.ThisRow, 
							false, 
							null, 
							false, 
							firstColumn, 
							lastColumn);
					}

					_owner.RestorePosition(position);
					return null;
				}

				if (this.ParseKeywordList(out firstKeyword, out lastKeyword, out hasSpaceAfterFirstKeyword))
				{
					position = _owner.SavePosition();

					bool hasSpaceAfterKeywordList;
					if (this.ParseSpacedComma(out hasSpaceAfterKeywordList) == false)
					{
						return new StructuredTableReference.InnerReference(
							firstKeyword.Value,
							hasSpaceAfterFirstKeyword,
							lastKeyword);
					}

					if (this.ParseColumnRange(out firstColumn, out lastColumn))
					{
						return new StructuredTableReference.InnerReference(
							firstKeyword.Value,
							hasSpaceAfterFirstKeyword, 
							lastKeyword, 
							hasSpaceAfterKeywordList,
							firstColumn, 
							lastColumn);
					}

					_owner.RestorePosition(position);
					return null;
				}

				if (this.ParseColumnRange(out firstColumn, out lastColumn))
					return new StructuredTableReference.InnerReference(firstColumn, lastColumn);

				return null;
			}

			#endregion // ParseInnerReference

			#region ParseKeyword

			// keyword = "[#All]" | "[#Data]" | "[#Headers]" | "[#Totals]" | "[#This Row]"
			private StructuredTableReferenceKeywordType? ParseKeyword()
			{
				if (_owner.ParseException != null)
					return null;

				PositionInfo position = _owner.SavePosition();
				if (_owner.TestCurrentCharAndAdvance('[') == false)
					return null;

				// The '@' is only valid in Excel 2010, but we should always parse it.
				if (_owner.TestCurrentCharAndAdvance('@'))
				{
					if (_owner.TestCurrentCharAndAdvance(']'))
						return StructuredTableReferenceKeywordType.ThisRow;
				}
				else if (_owner.TestCurrentChar('#'))
				{
					StructuredTableReferenceKeywordType? type;
					if (_owner.TestForStringAndAdvance(FormulaParser.KeywordAll))
						type = StructuredTableReferenceKeywordType.All;
					else if (_owner.TestForStringAndAdvance(FormulaParser.KeywordData))
						type = StructuredTableReferenceKeywordType.Data;
					else if (_owner.TestForStringAndAdvance(FormulaParser.KeywordHeaders))
						type = StructuredTableReferenceKeywordType.Headers;
					else if (_owner.TestForStringAndAdvance(FormulaParser.KeywordTotals))
						type = StructuredTableReferenceKeywordType.Totals;
					else if (_owner.TestForStringAndAdvance(FormulaParser.KeywordThisRow))
						type = StructuredTableReferenceKeywordType.ThisRow;
					else
						type = null;

					if (type.HasValue && _owner.TestCurrentCharAndAdvance(']'))
						return type;
				}

				_owner.RestorePosition(position);
				return null;
			}

			#endregion // ParseKeyword

			#region ParseKeywordList

			// keyword_list = keyword | ("[#Headers]" spaced_comma "[#Data]") | ("[#Data]" spaced_comma "[#Totals]")
			private bool ParseKeywordList(out StructuredTableReferenceKeywordType? firstKeyword, out StructuredTableReferenceKeywordType? lastKeyword, out bool hasSpaceAfterComma)
			{
				lastKeyword = null;
				hasSpaceAfterComma = false;

				firstKeyword = this.ParseKeyword();
				if (firstKeyword.HasValue == false)
					return false;

				if (firstKeyword == StructuredTableReferenceKeywordType.Headers ||
					firstKeyword == StructuredTableReferenceKeywordType.Data)
				{
					PositionInfo position = _owner.SavePosition();

					if (this.ParseSpacedComma(out hasSpaceAfterComma) == false)
						return true;

					lastKeyword = this.ParseKeyword();
					if (lastKeyword.HasValue)
					{
						if (firstKeyword == StructuredTableReferenceKeywordType.Headers &&
							lastKeyword.Value == StructuredTableReferenceKeywordType.Data)
						{
							return true;
						}

						if (firstKeyword == StructuredTableReferenceKeywordType.Data && 
							lastKeyword.Value == StructuredTableReferenceKeywordType.Totals)
						{
							return true;
						}

						lastKeyword = null;
					}

					_owner.RestorePosition(position);
					return true;
				}

				return true;
			}

			#endregion // ParseKeywordList

			#region ParseSimpleColumnName

			// simple_column_name = [any_nospace_column_character *any_column_character] any_nospace_column_character
			private string ParseSimpleColumnName(bool isColumnNameBracketed)
			{
				PositionInfo position = _owner.SavePosition();

				char currentCharacter;
				if (this.ParseAnyNospaceColumnCharacter(isColumnNameBracketed, out currentCharacter))
				{
					StringBuilder columnNameBuilder = new StringBuilder(currentCharacter.ToString());

					if (this.ParseAnyColumnCharacter(isColumnNameBracketed, out currentCharacter))
					{
						columnNameBuilder.Append(currentCharacter);
						while (this.ParseAnyColumnCharacter(isColumnNameBracketed, out currentCharacter))
							columnNameBuilder.Append(currentCharacter);

						// Back up and make sure the last parsed character was an any_nospace_column_character
						_owner.startIndex--;
						if (this.ParseAnyNospaceColumnCharacter(isColumnNameBracketed, out currentCharacter) == false)
						{
							_owner.RestorePosition(position);
							return null;
						}
					}

					return columnNameBuilder.ToString();
				}

				_owner.RestorePosition(position);
				return null;
			}

			#endregion // ParseSimpleColumnName

			#region ParseSpacedComma

			// The grammar seems to be incorrect in practice. Any number of spaces are allowed.
			// spaced_comma = [space] comma [space]
			private bool ParseSpacedComma(out bool hasSpaceAfterComma)
			{
				hasSpaceAfterComma = false;

				PositionInfo position = _owner.SavePosition();

				while (_owner.TestCurrentCharAndAdvance(' ')) ;

				if (_owner.TestCurrentCharAndAdvance(_owner.unionOperatorResolved) == false)
				{
					_owner.RestorePosition(position);
					return false;
				}

				while (_owner.TestCurrentCharAndAdvance(' '))
					hasSpaceAfterComma = true;

				return true;
			}

			#endregion // ParseSpacedComma

			#region ParseSpacedLBracket

			// The grammar seems to be incorrect in practice. Any number of spaces are allowed.
			// spaced_lbracket = "[" [space]
			private bool ParseSpacedLBracket(out bool hasSpace)
			{
				hasSpace = false;

				if (_owner.TestCurrentCharAndAdvance('['))
				{
					while (_owner.TestCurrentCharAndAdvance(' '))
						hasSpace = true;

					return true;
				}

				return false;
			}

			#endregion // ParseSpacedLBracket

			#region ParseSpacedRBracket

			// The grammar seems to be incorrect in practice. Any number of spaces are allowed.
			// spaced_rbracket = [space] "]"
			private bool ParseSpacedRBracket(out bool hasSpace)
			{
				hasSpace = false;

				PositionInfo position = _owner.SavePosition();
				while (_owner.TestCurrentCharAndAdvance(' '))
					hasSpace = true;

				if (_owner.TestCurrentCharAndAdvance(']'))
					return true;

				_owner.RestorePosition(position);
				return false;
			}

			#endregion // ParseSpacedRBracket

			#region ParseUnescapedColumnCharacter

			// unescaped_column_character = character ; MUST NOT match escape_column_character or space
			private bool ParseUnescapedColumnCharacter(bool isColumnNameBracketed, out char columnCharacter)
			{
				columnCharacter = (char)0;

				char currentChar = _owner.formula[_owner.startIndex];

				if (FormulaParser.IsEscapeTableColumnCharacter(currentChar))
					return false;

				// In practice, the semi-colon is only allowed in the column name when it is bracketed. Otherwise,
				// it is used as a range operator.
				if (isColumnNameBracketed == false && currentChar == ':')
					return false;

				int characterCode = (int)currentChar;

				if (characterCode == 0x0009 |
					characterCode == 0x000A |
					characterCode == 0x000D |
					(0x0020 <= characterCode && characterCode <= 0xD7FF) |
					(0xE000 <= characterCode && characterCode <= 0xFFFD) |
					(0x10000 <= characterCode && characterCode <= 0x10FFFF))
				{
					_owner.startIndex++;
					columnCharacter = currentChar;
					return true;
				}

				return false;
			}

			#endregion // ParseUnescapedColumnCharacter

			#endregion // Methods
		}

		#endregion // StructuredTableReferenceParser class
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