namespace IronRuby.Compiler
{
	public static class Errors
	{
		private const int Tokenizer = 4096;

		private const int WarningLevel1 = 8192;

		internal const int RuntimeWarning = 8192;

		private const int WarningLevel2 = 12288;

		internal const int RuntimeVerboseWarning = 12288;

		public static readonly ErrorInfo IllegalOctalDigit = new ErrorInfo(4096, "Illegal octal digit");

		public static readonly ErrorInfo NumericLiteralWithoutDigits = new ErrorInfo(4097, "Numeric literal without digits");

		public static readonly ErrorInfo TrailingUnderscoreInNumber = new ErrorInfo(4098, "Trailing `_' in number");

		public static readonly ErrorInfo TrailingEInNumber = new ErrorInfo(4099, "Trailing `e' in number");

		public static readonly ErrorInfo TrailingPlusInNumber = new ErrorInfo(4100, "Trailing `+' in number");

		public static readonly ErrorInfo TrailingMinusInNumber = new ErrorInfo(4101, "Trailing `-' in number");

		public static readonly ErrorInfo FloatOutOfRange = new ErrorInfo(4102, "Float {0} out of range");

		public static readonly ErrorInfo NoFloatingLiteral = new ErrorInfo(4103, "No .<digit> floating literal anymore; put 0 before dot");

		public static readonly ErrorInfo InvalidGlobalVariableName = new ErrorInfo(4104, "Identifier {0} is not valid");

		public static readonly ErrorInfo CannotAssignTo = new ErrorInfo(4105, "Can't assign to {0}");

		public static readonly ErrorInfo FormalArgumentIsConstantVariable = new ErrorInfo(4106, "Formal argument cannot be a constant");

		public static readonly ErrorInfo FormalArgumentIsInstanceVariable = new ErrorInfo(4107, "Formal argument cannot be an instance variable");

		public static readonly ErrorInfo FormalArgumentIsGlobalVariable = new ErrorInfo(4108, "Formal argument cannot be a global variable");

		public static readonly ErrorInfo FormalArgumentIsClassVariable = new ErrorInfo(4109, "Formal argument cannot be a class variable");

		public static readonly ErrorInfo InvalidInstanceVariableName = new ErrorInfo(4110, "`@{0}' is not allowed as an instance variable name");

		public static readonly ErrorInfo InvalidClassVariableName = new ErrorInfo(4111, "`@@{0}' is not allowed as a class variable name");

		public static readonly ErrorInfo InvalidCharacterInExpression = new ErrorInfo(4112, "Invalid character '{0}' in expression");

		public static readonly ErrorInfo InvalidMultibyteCharacter = new ErrorInfo(4113, "Invalid multibyte character: {0} ({1})");

		public static readonly ErrorInfo ForLoopVariableIsConstantVariable = new ErrorInfo(4114, "For loop variable cannot be a constant");

		public static readonly ErrorInfo ForLoopVariableIsInstanceVariable = new ErrorInfo(4115, "For loop variable cannot be an instance variable");

		public static readonly ErrorInfo ForLoopVariableIsGlobalVariable = new ErrorInfo(4116, "For loop variable cannot be a global variable");

		public static readonly ErrorInfo ForLoopVariableIsClassVariable = new ErrorInfo(4117, "For loop variable cannot be a class variable");

		public static readonly ErrorInfo MatchGroupReferenceOverflow = new ErrorInfo(4126, "Match group reference ${0} doesn't fit into Fixnum");

		public static readonly ErrorInfo MatchGroupReferenceReadOnly = new ErrorInfo(4127, "Can't set variable ${0}");

		public static readonly ErrorInfo CannotAliasGroupMatchVariable = new ErrorInfo(4128, "Can't make alias for number variable");

		public static readonly ErrorInfo DuplicateParameterName = new ErrorInfo(4129, "duplicate parameter name");

		public static readonly ErrorInfo IncompleteCharacter = new ErrorInfo(4133, "Incomplete character syntax");

		public static readonly ErrorInfo UnterminatedEmbeddedDocument = new ErrorInfo(4134, "Embedded document meets end of file");

		public static readonly ErrorInfo UnterminatedString = new ErrorInfo(4135, "Unterminated string meets end of file");

		public static readonly ErrorInfo UnterminatedHereDocIdentifier = new ErrorInfo(4137, "Unterminated here document identifier");

		public static readonly ErrorInfo UnterminatedHereDoc = new ErrorInfo(4138, "can't find string \"{0}\" anywhere before end-of-file");

		public static readonly ErrorInfo FileInitializerInMethod = new ErrorInfo(4139, "BEGIN in method");

		public static readonly ErrorInfo UnknownQuotedStringType = new ErrorInfo(4146, "Unknown type of quoted string");

		public static readonly ErrorInfo UnknownRegexOption = new ErrorInfo(4147, "Unknown RegEx option '{0}'");

		public static readonly ErrorInfo TooLargeUnicodeCodePoint = new ErrorInfo(4148, "Invalid Unicode codepoint (too large)");

		public static readonly ErrorInfo InvalidEscapeCharacter = new ErrorInfo(4149, "Invalid escape character syntax");

		public static readonly ErrorInfo EmptySymbolLiteral = new ErrorInfo(4151, "empty symbol literal");

		public static readonly ErrorInfo EncodingsMixed = new ErrorInfo(4152, "{0} mixed within {1} source");

		public static readonly ErrorInfo UntermintedUnicodeEscape = new ErrorInfo(4153, "Unterminated Unicode escape");

		public static readonly ErrorInfo InvalidUnicodeEscape = new ErrorInfo(4154, "Invalid Unicode escape");

		public static readonly ErrorInfo ModuleNameNotConstant = new ErrorInfo(4162, "Class/module name must be a constant");

		public static readonly ErrorInfo ConstantReassigned = new ErrorInfo(4163, "Constant re-assignment");

		public static readonly ErrorInfo BothBlockDefAndBlockRefGiven = new ErrorInfo(4164, "both block arg and actual block given");

		public static readonly ErrorInfo BlockGivenToYield = new ErrorInfo(4165, "block given to yield");

		public static readonly ErrorInfo ParenthesizeArguments = new ErrorInfo(8193, "parenthesize argument(s) for future version");

		public static readonly ErrorInfo WhitespaceBeforeArgumentParentheses = new ErrorInfo(8194, "don't put space before argument parentheses");

		public static readonly ErrorInfo InvalidCharacterSyntax = new ErrorInfo(8195, "invalid character syntax; use ?\\{0}");

		public static readonly ErrorInfo ShutdownHandlerInMethod = new ErrorInfo(8196, "END in method; use at_exit");

		public static readonly ErrorInfo InterpretedAsGroupedExpression = new ErrorInfo(12289, "(...) interpreted as grouped expression");

		public static readonly ErrorInfo AmbiguousFirstArgument = new ErrorInfo(12290, "Ambiguous first argument; put parentheses or even spaces");

		public static readonly ErrorInfo AmpersandInterpretedAsProcArgument = new ErrorInfo(12291, "`&' interpreted as argument prefix");

		public static readonly ErrorInfo AmpersandInVoidContext = new ErrorInfo(12292, "Useless use of & in void context");

		public static readonly ErrorInfo StarInterpretedAsSplatArgument = new ErrorInfo(12293, "`*' interpreted as argument prefix");

		public static readonly ErrorInfo ShadowingOuterLocalVariable = new ErrorInfo(12295, "shadowing outer local variable - {0}.");

		internal static bool IsVerboseWarning(int errorCode)
		{
			return errorCode >= 12288;
		}
	}
}
