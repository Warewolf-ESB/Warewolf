using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Infragistics.Controls.Interactions
{





	internal class FormulaParser
	{
		#region Static Variables

		private static readonly FormulaTokenType[] AdditiveOpTokenTypes = new FormulaTokenType[] { 
			FormulaTokenType.OpPlus, 
			FormulaTokenType.OpMinus };

		private static readonly FormulaTokenType[] ComparisonOpTokenTypes = new FormulaTokenType[] { 
			FormulaTokenType.OpGt, 
			FormulaTokenType.OpLt, 
			FormulaTokenType.OpGe, 
			FormulaTokenType.OpLe, 
			FormulaTokenType.OpNe, 
			FormulaTokenType.OpAltNe };

		private static readonly FormulaTokenType[] ConcatOpTokenTypes = new FormulaTokenType[] { 
			FormulaTokenType.OpConcat };

		private static readonly FormulaTokenType[] ExponOpTokenTypes = new FormulaTokenType[] { 
			FormulaTokenType.OpExpon };

		private static readonly FormulaTokenType[] MultOpTokenTypes = new FormulaTokenType[] { 
			FormulaTokenType.OpTimes, 
			FormulaTokenType.OpDiv };

		private static readonly FormulaTokenType[] PostfixOpTokenTypes = new FormulaTokenType[] { 
			FormulaTokenType.OpPercent };

		private static readonly FormulaTokenType[] PrefixOpTokenTypes = new FormulaTokenType[] { 
			FormulaTokenType.OpPlus, 
			FormulaTokenType.OpMinus };

		#endregion  // Static Variables

		#region Member Variables

		private string _formula;
		private int _currentIndex;

		#endregion  // Member Variables

		#region Constructor

		private FormulaParser(string formula)
		{
			_formula = formula;
			_currentIndex = 0;
		}

		#endregion  // Constructor

		#region Methods

		#region Public Methods

		#region FindToken

		public static FormulaElement FindToken(List<FormulaElement> elements, int index)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				FormulaElement element = elements[i];

				if (element.EndIndex < index)
					continue;

				if (index < element.StartIndex)
					break;

				FormulaToken token = element as FormulaToken;
				if (token != null)
					return token;

				FormulaProduction production = element as FormulaProduction;
				if (production != null)
				{
					FormulaElement retVal = FormulaParser.FindToken(production.Children, index);
					if (retVal != null)
						return retVal;

					return production;
				}
			}

			return null;
		}

		#endregion  // FindToken

		#region Parse

		public static List<FormulaElement> Parse(string formula)
		{
			if (formula == null)
				return new List<FormulaElement>();

			FormulaParser parser = new FormulaParser(formula);
			return parser.Parse();
		}

		#endregion  // Parse

		#endregion  // Public Methods

		#region Private Methods

		#region AdvancePastErroneousFuncArg

		private void AdvancePastErroneousFuncArg()
		{
			for (; _currentIndex < _formula.Length; _currentIndex++)
			{
				char currentChar = _formula[_currentIndex];

				switch (currentChar)
				{
					case ')':
					case ',':
						return;

					case '(':
						this.AdvancePastErroneousParenthesizedTerm(true);
						break;

					case '[':
						this.AdvancePastErroneousReference();
						break;

					case '\'':
					case '\"':
						this.AdvancePastQuotedString(currentChar);
						break;

					default:
						break;
				}
			}
		}

		#endregion  // AdvancePastErroneousFuncArg

		#region AdvancePastErroneousParenthesizedTerm

		private void AdvancePastErroneousParenthesizedTerm(bool isAtStartOfTerm = false)
		{
			if (isAtStartOfTerm)
			{
				Debug.Assert(_formula[_currentIndex] == '(', "We should be at the start of the parenthesized term here.");
				_currentIndex++;
			}

			for (; _currentIndex < _formula.Length; _currentIndex++)
			{
				char currentChar = _formula[_currentIndex];

				switch (currentChar)
				{
					case ')':
						return;

					case '(':
						this.AdvancePastErroneousParenthesizedTerm(true);
						break;

					case '[':
						this.AdvancePastErroneousReference();
						break;

					case '\'':
					case '\"':
						this.AdvancePastQuotedString(currentChar);
						break;

					default:
						break;
				}
			}
		}

		#endregion  // AdvancePastErroneousParenthesizedTerm

		#region AdvancePastErroneousReference

		private bool AdvancePastErroneousReference()
		{
			Debug.Assert(_formula[_currentIndex] == '[', "We should be at the start of the reference here.");
			_currentIndex++;

			for (; _currentIndex < _formula.Length; _currentIndex++)
			{
				char currentChar = _formula[_currentIndex];

				switch (currentChar)
				{
					case ']':
						return true;

					case '\"':
						this.AdvancePastQuotedString(currentChar);
						break;

					case '\\':
						_currentIndex++;
						break;

					default:
						break;
				}
			}

			_currentIndex = Math.Min(_currentIndex, _formula.Length);
			return false;
		}

		#endregion  // AdvancePastErroneousReference

		#region AdvancePastQuotedString

		private void AdvancePastQuotedString(char startingQuoteChar)
		{
			Debug.Assert(_formula[_currentIndex] == startingQuoteChar, "We should be at the start of the string here.");
			_currentIndex++;

			while (_currentIndex < _formula.Length)
			{
				char nextChar = _formula[_currentIndex++];

				// Strings started with a double-quote end with a double-quote and those started with a single-quote 
				// end with a single-quote.
				if (nextChar == startingQuoteChar)
					break;

				// Escape character, skip the next character.
				if (nextChar == '\\')
					_currentIndex++;
			}

			_currentIndex = Math.Min(_currentIndex, _formula.Length);
		}

		#endregion  // AdvancePastQuotedString

		#region AdvancePastWhitespace

		private bool AdvancePastWhitespace()
		{
			while (_currentIndex < _formula.Length)
			{
				char currentChar = _formula[_currentIndex];
				switch (currentChar)
				{
					case ' ':
					case '\t':
					case '\r':
					case '\n':
						_currentIndex++;
						continue;

					default:
						break;
				}

				return _currentIndex < _formula.Length;
			}

			return false;
		}

		#endregion  // AdvancePastWhitespace

		#region ContainsToken

		private static bool ContainsToken(FormulaTokenType[] tokensToParse, params FormulaTokenType[] tokensToSearchFor)
		{
			if (tokensToParse == null)
				return false;

			for (int i = 0; i < tokensToParse.Length; i++)
			{
				FormulaTokenType tokenToParse = tokensToParse[i];

				for (int j = 0; j < tokensToSearchFor.Length; j++)
				{
					FormulaTokenType tokenToSearchFor = tokensToSearchFor[j];

					if (tokenToParse == tokensToSearchFor[j])
						return true;

					// When we found an ErroneousReference, but were looking for a Reference, we should consider it a match.
					if (tokenToParse == FormulaTokenType.Reference && 
						tokenToSearchFor == FormulaTokenType.ErroneousReference)
						return true;

					// When we found an ErroneousQuotedString, but were looking for a QuotedString, we should consider it a match.
					if (tokenToParse == FormulaTokenType.QuotedString &&
						tokenToSearchFor == FormulaTokenType.ErroneousQuotedString)
						return true;
				}
			}

			return false;
		}

		#endregion  // ContainsToken

		#region GetPossibleOpTokenTypes

		private static FormulaTokenType[] GetPossibleOpTokenTypes(FormulaProductionType opProductionType)
		{
			switch (opProductionType)
			{
				case FormulaProductionType.AdditiveOp:
					return FormulaParser.AdditiveOpTokenTypes;

				case FormulaProductionType.ComparisonOp:
					return FormulaParser.ComparisonOpTokenTypes;

				case FormulaProductionType.ConcatOp:
					return FormulaParser.ConcatOpTokenTypes;

				case FormulaProductionType.ExponOp:
					return FormulaParser.ExponOpTokenTypes;

				case FormulaProductionType.MultOp:
					return FormulaParser.MultOpTokenTypes;

				case FormulaProductionType.PostfixOp:
					return FormulaParser.PostfixOpTokenTypes;

				case FormulaProductionType.PrefixOp:
					return FormulaParser.PrefixOpTokenTypes;

				default:
					Debug.Assert(false, "This is not a operator production type:" + opProductionType);
					return new FormulaTokenType[0];
			}
		}

		#endregion  // GetPossibleOpTokenTypes

		#region IsNumberCharacter

		private static bool IsNumberCharacter(char character)
		{
			return '0' <= character && character <= '9';
		}

		#endregion  // IsNumberCharacter

		#region IsTextCharacter

		private static bool IsTextCharacter(char character)
		{
			return
				('a' <= character && character <= 'z') ||
				('A' <= character && character <= 'Z') ||
				character == '_';
		}

		#endregion  // IsTextCharacter

		#region Parse

		private List<FormulaElement> Parse()
		{
			List<FormulaElement> productions = new List<FormulaElement>();

			while (_currentIndex < _formula.Length)
			{
				int startIndex = _currentIndex;

				FormulaProduction formula = this.ParseFormula();
				if (formula == null || startIndex == _currentIndex)
				{
					FormulaProduction erroneousFuncId = this.ParseFuncId(true);
					if (erroneousFuncId != null)
					{
						productions.Add(erroneousFuncId);
					}
					else
					{
						_currentIndex++;
					}
					continue;
				}

				productions.Add(formula);
			}

			return productions;
		}

		#endregion  // Parse

		#region ParseAdditiveTerm

		private FormulaProduction ParseAdditiveTerm()
		{
			return this.ParseOneOrMoreBinaryTermProduction(
				FormulaProductionType.AdditiveTerm,
				this.ParseMultTerm,
				FormulaProductionType.MultOp);
		}

		#endregion  // ParseAdditiveTerm

		#region ParseComparisonTerm

		private FormulaProduction ParseComparisonTerm()
		{
			return this.ParseOneOrMoreBinaryTermProduction(
				FormulaProductionType.ComparisonTerm,
				this.ParseConcatTerm,
				FormulaProductionType.ConcatOp);
		}

		#endregion  // ParseComparisonTerm

		#region ParseConcatTerm

		private FormulaProduction ParseConcatTerm()
		{
			return this.ParseOneOrMoreBinaryTermProduction(
				FormulaProductionType.ConcatTerm,
				this.ParseAdditiveTerm,
				FormulaProductionType.AdditiveOp);
		}

		#endregion  // ParseConcatTerm

		#region ParseConstant

		private FormulaProduction ParseConstant()
		{
			int startIndex = _currentIndex;

			FormulaToken opDot = this.ParseToken(FormulaTokenType.OpDot);
			if (opDot != null)
			{
				FormulaProduction constantProduction = new FormulaProduction(FormulaProductionType.Constant, opDot);

				FormulaToken number = this.ParseToken(FormulaTokenType.Number);
				if (number != null)
				{
					constantProduction.AddChild(number);
					return constantProduction;
				}
			}
			else
			{
				FormulaToken integerNumber = this.ParseToken(FormulaTokenType.Number);
				if (integerNumber != null)
				{
					FormulaProduction constantProduction = new FormulaProduction(FormulaProductionType.Constant, integerNumber);

					opDot = this.ParseToken(FormulaTokenType.OpDot);
					if (opDot != null)
					{
						constantProduction.AddChild(opDot);

						FormulaToken fractionNumber = this.ParseToken(FormulaTokenType.Number);
						if (fractionNumber != null)
							constantProduction.AddChild(fractionNumber);
					}

					return constantProduction;
				}
			}

			// Restore the old position.
			_currentIndex = startIndex;
			return null;
		}

		#endregion  // ParseConstant

		#region ParseExponTerm

		private FormulaProduction ParseExponTerm()
		{
			FormulaProduction postfixTerm = this.ParsePostfixTerm();
			if (postfixTerm == null)
				return null;

			FormulaProduction exponTermProduction = new FormulaProduction(FormulaProductionType.ExponTerm, postfixTerm);

			FormulaProduction postfixOp = this.ParseOpProduction(FormulaProductionType.PostfixOp);
			if (postfixOp != null)
				exponTermProduction.AddChild(postfixOp);

			return exponTermProduction;
		}

		#endregion  // ParseExponTerm

		#region ParseFormula

		private FormulaProduction ParseFormula()
		{
			return this.ParseOneOrMoreBinaryTermProduction(
				FormulaProductionType.Formula,
				this.ParseComparisonTerm,
				FormulaProductionType.ComparisonOp);
		}

		#endregion  // ParseFormula

		#region ParseFuncArg

		private FormulaProduction ParseFuncArg()
		{
			FormulaProduction range = this.ParseRange();
			if (range != null)
				return new FormulaProduction(FormulaProductionType.FuncArg, range);

			FormulaProduction formula = this.ParseFormula();
			if (formula != null)
				return new FormulaProduction(FormulaProductionType.FuncArg, formula);

			return null;
		}

		#endregion  // ParseFuncArg

		#region ParseFuncArgs

		private FormulaProduction ParseFuncArgs()
		{
			FormulaProduction funcArg = this.ParseFuncArg();
			FormulaToken argSep;
			FormulaProduction funcArgsProduction;

			bool requiresAnArgSep = false;
			if (funcArg == null)
			{
				funcArgsProduction = new FormulaProduction(FormulaProductionType.FuncArgs,
					new FormulaProduction(FormulaProductionType.ErroneousFuncArg,
						new FormulaToken("", FormulaTokenType.ErroneousPortion, _currentIndex, _currentIndex - 1)));

				// If we found no argument, assume that there is a blank argument here, but require that an argument separator is found next. 
				// If it isn't, the blank argument was an incorrect assumption.
				requiresAnArgSep = true;
			}
			else
			{
				funcArgsProduction = new FormulaProduction(FormulaProductionType.FuncArgs, funcArg);
			}

			while (true)
			{
				argSep = this.ParseToken(FormulaTokenType.ArgSep);
				if (argSep == null)
				{
					if (requiresAnArgSep)
						return null;

					break;
				}

				// Once the first argument separator is found, we don't have to require one anymore.
				requiresAnArgSep = false;

				funcArgsProduction.AddChild(argSep);

				funcArg = this.ParseFuncArg();
				if (funcArg != null)
				{
					funcArgsProduction.AddChild(funcArg);
				}
				else
				{
					int startOfError = _currentIndex;
					this.AdvancePastErroneousFuncArg();

					string errorText = _formula.Substring(startOfError, _currentIndex - startOfError);
					funcArgsProduction.AddChild(new FormulaProduction(
						FormulaProductionType.ErroneousFuncArg,
						new FormulaToken(errorText, FormulaTokenType.ErroneousPortion, startOfError, _currentIndex - 1)
						));
				}
			}

			return funcArgsProduction;
		}

		#endregion  // ParseFuncArgs

		#region ParseFuncId

		private FormulaProduction ParseFuncId(bool isError = false)
		{
			FormulaToken text = this.ParseToken(FormulaTokenType.Text);

			if (text == null)
				return null;

			FormulaProductionType productionType = isError
				? FormulaProductionType.ErroneousFuncId 
				: FormulaProductionType.FuncId;

			FormulaProduction funcIdProduction = new FormulaProduction(productionType, text);

			FormulaToken number;
			while (true)
			{
				number = this.ParseToken(FormulaTokenType.Number);
				if (number != null)
				{
					funcIdProduction.AddChild(number);
					continue;
				}

				text = this.ParseToken(FormulaTokenType.Text);
				if (text != null)
				{
					funcIdProduction.AddChild(text);
					continue;
				}

				break;
			}

			return funcIdProduction;
		}

		#endregion  // ParseFuncId

		#region ParseFunction

		private FormulaProduction ParseFunction()
		{
			int startIndex = _currentIndex;

			FormulaProduction funcId = this.ParseFuncId();
			if (funcId != null)
			{
				FormulaProduction functionProduction = new FormulaProduction(FormulaProductionType.Function, funcId);

				FormulaToken leftParen = this.ParseToken(FormulaTokenType.LeftParen);
				if (leftParen != null)
				{
					functionProduction.AddChild(leftParen);

					FormulaProduction funcArgs = this.ParseFuncArgs();
					if (funcArgs != null)
						functionProduction.AddChild(funcArgs);

					FormulaToken rightParen = this.ParseToken(FormulaTokenType.RightParen);
					if (rightParen != null)
					{
						functionProduction.AddChild(rightParen);
					}
					else
					{
						functionProduction = new FormulaProduction(FormulaProductionType.ErroneousFunction, funcId, leftParen);
						if (funcArgs != null)
							functionProduction.AddChild(funcArgs);
					}

					return functionProduction;
				}
				else
				{
					return new FormulaProduction(FormulaProductionType.ErroneousFunction, funcId);
				}
			}

			// Restore the old position.
			_currentIndex = startIndex;
			return null;
		}

		#endregion  // ParseFunction

		#region ParseMultTerm

		private FormulaProduction ParseMultTerm()
		{
			return this.ParseOneOrMoreBinaryTermProduction(
				FormulaProductionType.MultTerm, 
				this.ParseExponTerm, 
				FormulaProductionType.ExponOp);
		}

		#endregion  // ParseMultTerm

		#region ParseOneOrMoreBinaryTermProduction

		private FormulaProduction ParseOneOrMoreBinaryTermProduction(
			FormulaProductionType productionType,
			Func<FormulaProduction> parseTermCallback,
			FormulaProductionType binaryOpProductionType)
		{
			FormulaProduction term = parseTermCallback();
			if (term == null)
				return null;

			FormulaProduction production = new FormulaProduction(productionType, term);

			FormulaProduction binaryOp;
			while (true)
			{
				int startPosition = _currentIndex;

				binaryOp = this.ParseOpProduction(binaryOpProductionType);

				if (binaryOp == null)
					break;

				term = parseTermCallback();

				if (term == null)
				{
					// If we got a binary operator but no term after it, restore the old position.
					_currentIndex = startPosition;
					break;
				}

				production.AddChild(binaryOp);
				production.AddChild(term);
			}

			return production;
		}

		#endregion  // ParseOneOrMoreBinaryTermProduction

		#region ParseOpProduction

		private FormulaProduction ParseOpProduction(FormulaProductionType opProductionType)
		{
			FormulaToken opToken = this.ParseToken(FormulaParser.GetPossibleOpTokenTypes(opProductionType));
			if (opToken == null)
				return null;

			return new FormulaProduction(opProductionType, opToken);
		}

		#endregion  // ParseOpProduction

		#region ParsePostfixTerm

		private FormulaProduction ParsePostfixTerm()
		{
			int startIndex = _currentIndex;

			FormulaProduction prefixOp = this.ParseOpProduction(FormulaProductionType.PrefixOp);
			FormulaProduction term = this.ParseTerm();

			if (term == null)
			{
				// Restore the old position.
				_currentIndex = startIndex;
				return null;
			}

			if (prefixOp != null)
				return new FormulaProduction(FormulaProductionType.PostfixTerm, prefixOp, term);

			return new FormulaProduction(FormulaProductionType.PostfixTerm, term);
		}

		#endregion  // ParsePostfixTerm

		#region ParseRange

		private FormulaProduction ParseRange()
		{
			int startIndex = _currentIndex;

			FormulaToken reference1 = this.ParseToken(FormulaTokenType.Reference);
			if (reference1 != null)
			{
				FormulaToken rangeSep = this.ParseToken(FormulaTokenType.RangeSep);
				if (rangeSep != null)
				{
					FormulaToken reference2 = this.ParseToken(FormulaTokenType.Reference);
					if (reference2 != null)
						return new FormulaProduction(FormulaProductionType.Range, reference1, rangeSep, reference2);
				}
			}

			// Restore the old position.
			_currentIndex = startIndex;
			return null;
		}

		#endregion  // ParseRange

		#region ParseTerm

		private FormulaProduction ParseTerm()
		{
			int startIndex = _currentIndex;

			FormulaProduction constant = this.ParseConstant();
			if (constant != null)
				return new FormulaProduction(FormulaProductionType.Term, constant);

			FormulaToken quotedString = this.ParseToken(FormulaTokenType.QuotedString);
			if (quotedString != null)
				return new FormulaProduction(FormulaProductionType.Term, quotedString);

			FormulaProduction function = this.ParseFunction();
			if (function != null)
				return new FormulaProduction(FormulaProductionType.Term, function);

			FormulaToken reference = this.ParseToken(FormulaTokenType.Reference);
			if (reference != null)
				return new FormulaProduction(FormulaProductionType.Term, reference);

			FormulaToken leftParen = this.ParseToken(FormulaTokenType.LeftParen);
			if (leftParen != null)
			{
				FormulaProduction formula = this.ParseFormula();
				FormulaToken rightParen;
				if (formula != null)
				{
					rightParen = this.ParseToken(FormulaTokenType.RightParen);
					if (rightParen != null)
						return new FormulaProduction(FormulaProductionType.Term, leftParen, formula, rightParen);
				}

				int startOfError = _currentIndex;
				this.AdvancePastErroneousParenthesizedTerm();

				rightParen = this.ParseToken(FormulaTokenType.RightParen);

				FormulaProduction erroneousTerm = new FormulaProduction(FormulaProductionType.ErroneousTerm, leftParen);
				if (formula != null)
					erroneousTerm.AddChild(formula);

				int endOfError = rightParen != null 
					? rightParen.StartIndex 
					: _currentIndex;

				if (startOfError != endOfError)
				{
					string errorText = _formula.Substring(startOfError, endOfError - startOfError);
					erroneousTerm.AddChild(new FormulaToken(errorText, FormulaTokenType.ErroneousPortion, startOfError, endOfError - 1));
				}

				if (rightParen != null)
					erroneousTerm.AddChild(rightParen);

				return erroneousTerm;
			}

			// Restore the old position.
			_currentIndex = startIndex;

			return null;
		}

		#endregion  // ParseTerm

		#region ParseToken

		private FormulaToken ParseToken(params FormulaTokenType[] tokensToParse)
		{
			if (this.AdvancePastWhitespace() == false)
				return null;

			int startIndex = _currentIndex;
			char currentChar = _formula[_currentIndex];

			FormulaTokenType tokenType;
			int tokenLength = 1;

			switch (currentChar)
			{
				case '*':
					tokenType = FormulaTokenType.OpTimes;
					break;

				case '/':
					tokenType = FormulaTokenType.OpDiv;
					break;

				case '+':
					tokenType = FormulaTokenType.OpPlus;
					break;

				case '-':
					tokenType = FormulaTokenType.OpMinus;
					break;

				case '&':
					tokenType = FormulaTokenType.OpConcat;
					break;

				case '^':
					tokenType = FormulaTokenType.OpExpon;
					break;

				case '%':
					tokenType = FormulaTokenType.OpPercent;
					break;

				case ',':
					tokenType = FormulaTokenType.ArgSep;
					break;

				case '.':
					if (FormulaParser.ContainsToken(tokensToParse, FormulaTokenType.RangeSep))
					{
						int nextIndex = _currentIndex + 1;
						if (nextIndex < _formula.Length && _formula[nextIndex] == '.')
						{
							tokenLength = 2;
							tokenType = FormulaTokenType.RangeSep;
							break;
						}
					}

					tokenType = FormulaTokenType.OpDot;
					break;

				case '(':
					tokenType = FormulaTokenType.LeftParen;
					break;

				case ')':
					tokenType = FormulaTokenType.RightParen;
					break;

				case ':':
					tokenType = FormulaTokenType.RangeSep;
					break;

				case '=':
					tokenType = FormulaTokenType.OpEqual;
					break;

				case '<':
					if (FormulaParser.ContainsToken(tokensToParse, FormulaTokenType.OpLe, FormulaTokenType.OpAltNe))
					{
						int nextIndex = _currentIndex + 1;
						if (nextIndex < _formula.Length)
						{
							char nextChar = _formula[nextIndex];

							if (nextChar == '=')
							{
								tokenLength = 2;
								tokenType = FormulaTokenType.OpLe;
								break;
							}
							else if (nextChar == '>')
							{
								tokenLength = 2;
								tokenType = FormulaTokenType.OpAltNe;
								break;
							}
						}
					}

					tokenType = FormulaTokenType.OpLt;
					break;

				case '>':
					if (FormulaParser.ContainsToken(tokensToParse, FormulaTokenType.OpGe))
					{
						int nextIndex = _currentIndex + 1;
						if (nextIndex < _formula.Length && _formula[nextIndex] == '=')
						{
							tokenLength = 2;
							tokenType = FormulaTokenType.OpGe;
							break;
						}
					}

					tokenType = FormulaTokenType.OpGt;
					break;

				case '!':
					{
						int nextIndex = _currentIndex + 1;
						if (nextIndex < _formula.Length && _formula[nextIndex] == '=')
						{
							tokenLength = 2;
							tokenType = FormulaTokenType.OpNe;
						}
						else
						{
							return null;
						}
					}
					break;

				case '\"':
				case '\'':
					if (FormulaParser.ContainsToken(tokensToParse, FormulaTokenType.QuotedString))
					{
						tokenType = FormulaTokenType.ErroneousQuotedString;
						while ((_currentIndex + tokenLength) < _formula.Length)
						{
							char nextChar = _formula[_currentIndex + tokenLength];
							tokenLength++;

							// Strings started with a double-quote end with a double-quote and those started with a single-quote 
							// end with a single-quote.
							if (nextChar == currentChar)
							{
								tokenType = FormulaTokenType.QuotedString;
								break;
							}

							// Escape character, skip the next character.
							if (nextChar == '\\')
								tokenLength++;
						}

						break;
					}
					else
					{
						return null;
					}

				case '[':
					if (FormulaParser.ContainsToken(tokensToParse, FormulaTokenType.Reference))
					{
						Regex referenceRegex = new Regex(@"\G\[([^\[\]\\""]|\\.|""([^""]|\\.)*"")+\]");
						Match match = referenceRegex.Match(_formula, startIndex);

						if (match.Success)
						{
							tokenLength = match.Length;
							tokenType = FormulaTokenType.Reference;
						}
						else
						{
							if (this.AdvancePastErroneousReference())
								tokenLength = _currentIndex - startIndex + 1;
							else
								tokenLength = _currentIndex - startIndex;

							tokenType = FormulaTokenType.ErroneousReference;
						}
						break;
					}
					else
					{
						return null;
					}

				default:
					if (FormulaParser.IsNumberCharacter(currentChar))
					{
						tokenType = FormulaTokenType.Number;

						while ((_currentIndex + tokenLength) < _formula.Length)
						{
							char nextChar = _formula[_currentIndex + tokenLength];
							if (FormulaParser.IsNumberCharacter(nextChar) == false)
								break;

							tokenLength++;
						}

						break;
					}
					else if (FormulaParser.IsTextCharacter(currentChar))
					{
						tokenType = FormulaTokenType.Text;

						while ((_currentIndex + tokenLength) < _formula.Length)
						{
							char nextChar = _formula[_currentIndex + tokenLength];
							if (FormulaParser.IsTextCharacter(nextChar) == false)
								break;

							tokenLength++;
						}

						break;
					}
					else
					{
						return null;
					}
			}

			if (FormulaParser.ContainsToken(tokensToParse, tokenType) == false)
				return null;

			_currentIndex += tokenLength;
			string tokenText = _formula.Substring(startIndex, tokenLength);
			return new FormulaToken(tokenText, tokenType, startIndex, _currentIndex - 1);
		}

		#endregion  // ParseToken

		#endregion  // Private Methods

		#endregion  // Methods
	}

	#region FormulaElement class

	internal abstract class FormulaElement
	{
		private int _endIndex;
		private FormulaProduction _parent;
		private int _startIndex;

		public FormulaElement(int startIndex, int endIndex)
		{
			_startIndex = startIndex;
			_endIndex = endIndex;
		}

		public abstract string GetText();

		public int EndIndex
		{
			get { return _endIndex; }
			protected set { _endIndex = value; }
		}

		public virtual bool IsArgSep
		{
			get { return false; }
		}

		public virtual bool IsFuncId
		{
			get { return false; }
		}

		public virtual bool IsFunction
		{
			get { return false; }
		}

		public virtual bool IsLeftParen
		{
			get { return false; }
		}

		public virtual bool IsReference
		{
			get { return false; }
		}

		public virtual bool IsRightParen
		{
			get { return false; }
		}

		public FormulaProduction Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		public int StartIndex
		{
			get { return _startIndex; }
		}
	}

	#endregion  // FormulaElement class

	#region FormulaProduction class

	[DebuggerDisplay("{_type}: {GetText()}")]
	internal class FormulaProduction : FormulaElement
	{
		private List<FormulaElement> _children;
		private string _text;
		private FormulaProductionType _type;

		public FormulaProduction(FormulaProductionType type, int startIndex)
			: base(startIndex, startIndex - 1)
		{
			_children = new List<FormulaElement>();
			_type = type;
		}

		public FormulaProduction(FormulaProductionType type, params FormulaElement[] children)
			: this(type, children != null && children.Length > 0 ? children[0].StartIndex : 0)
		{
			Debug.Assert(children != null && children.Length > 0, "At least one child should have been passed in here.");

			for (int i = 0; i < children.Length; i++)
				this.AddChild(children[i]);
		}

		public override string GetText()
		{
			if (_text == null)
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < _children.Count; i++)
				{
					sb.Append(_children[i].GetText());
				}

				_text = sb.ToString();
			}

			return _text;
		}

		public override bool IsFuncId
		{
			get { return this.Type == FormulaProductionType.FuncId; }
		}

		public override bool IsFunction
		{
			get { return this.Type == FormulaProductionType.Function || this.Type == FormulaProductionType.ErroneousFunction; }
		}

		public void AddChild(FormulaElement child)
		{
			Debug.Assert(child != this, "This child element should not be this element.");

			child.Parent = this;

			_children.Add(child);
			_text = null;

			Debug.Assert(this.StartIndex <= child.StartIndex, "The child element be within the production.");
			this.EndIndex = Math.Max(this.EndIndex, child.EndIndex);
		}

		public List<FormulaElement> Children
		{
			get { return _children; }
		}

		public FormulaProductionType Type
		{
			get { return _type; }
		}
	}

	#endregion  // FormulaProduction class

	#region FormulaProductionType enum

	internal enum FormulaProductionType
	{
		Formula = 1,		// ComparisonTerm (ComparisonOp ComparisonTerm)*;

		ComparisonOp,		// OpEqual | OpGt | OpLt | OpGe | OpLe | OpNe | OpAltNe;
		ComparisonTerm,		// ConcatTerm (ConcatOp ConcatTerm)*;

		ConcatOp,			// OpConcat;
		ConcatTerm,			// AdditiveTerm (AdditiveOp AdditiveTerm)*;

		AdditiveOp,			// OpPlus | OpMinus;
		AdditiveTerm,		// MultTerm (MultOp MultTerm)*;

		MultOp,				// OpTimes | OpDiv;
		MultTerm,			// ExponTerm (ExponOp ExponTerm)*;

		ExponOp,			// OpExpon;
		PostfixOp,			// OpPercent;
		ExponTerm,			// PostfixTerm (PrefixOp)?;

		PrefixOp,			// OpPlus | OpMinus ;
		PostfixTerm,		// (PrefixOp)? Term;

		Constant,			// ((OpDot Number) |  (Number (OpDot (Number)? )?)) ;

		Term,				// Constant | QuotedString | Function | Reference | LeftParen Formula RightParen;

		Range,				// Reference RangeSep Reference;

		FuncId,				// Text (Text | Number)*;
		FuncArgs,			// FuncArg (ArgSep FuncArg )*;
		FuncArg,			// Range | Formula;

		Function,			// FuncId LeftParen (FuncArgs)? RightParen;

		ErroneousFuncArg,
		ErroneousFuncId,
		ErroneousFunction,
		ErroneousTerm,
	}

	#endregion  // FormulaProductionType enum

	#region FormulaToken class

	[DebuggerDisplay("{_type}: {GetText()}")]
	internal class FormulaToken : FormulaElement
	{
		private string _text;
		private FormulaTokenType _type;

		public FormulaToken(string text, FormulaTokenType type, int startIndex, int endIndex)
			: base(startIndex, endIndex)
		{
			_text = text;
			_type = type;
		}

		public override string GetText()
		{
			return _text;
		}

		public override bool IsArgSep
		{
			get { return this.Type == FormulaTokenType.ArgSep; }
		}

		public override bool IsLeftParen
		{
			get { return this.Type == FormulaTokenType.LeftParen; }
		}

		public override bool IsReference
		{
			get { return this.Type == FormulaTokenType.ErroneousReference || this.Type == FormulaTokenType.Reference; }
		}

		public override bool IsRightParen
		{
			get { return this.Type == FormulaTokenType.RightParen; }
		}

		public string Text
		{
			get { return _text; }
		}

		public FormulaTokenType Type
		{
			get { return _type; }
		}
	}

	#endregion  // FormulaToken class

	#region FormulaTokenType enum

	internal enum FormulaTokenType
	{
		LeftParen = 1,		// "("
		RightParen,			// ")"
		Number,				// <<[0-9]+>>
		Text,				// <<[A-Za-z_]+>>
		Whitespace,			// <<[ \t\n\r]+>> %ignore%
		OpDot,				// "."
		OpEqual,			// "="
		OpGt,				// ">"
		OpLt,				// "<"
		OpGe,				// ">="
		OpLe,				// "<="
		OpNe,				// "!="
		OpAltNe,			// "<>"
		OpConcat,			// "&"
		OpPlus,				// "+"
		OpMinus,			// "-"
		OpTimes,			// "*"
		OpDiv,				// "/"
		OpExpon,			// "^"
		OpPercent,			// "%"
		ArgSep,				// ","
		RangeSep,			// <<\.\.|\:>>
		Reference,			// <<\[([^\[\]\\"]|\\.|"([^"]|\\.)*")+\]>>
		QuotedString,		// <<"([^"]|\\.)*"|'([^']|\\.)*'>>

		ErroneousPortion,
		ErroneousQuotedString,
		ErroneousReference,
	}

	#endregion  // FormulaTokenType enum
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