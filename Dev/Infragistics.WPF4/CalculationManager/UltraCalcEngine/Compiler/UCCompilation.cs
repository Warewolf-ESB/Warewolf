using System;
using System.IO;
using System.Diagnostics;

using PerCederberg.Grammatica.Parser;


using UltraCalcFunction = Infragistics.Calculations.Engine.CalculationFunction;
using UltraCalcNumberStack = Infragistics.Calculations.Engine.CalculationNumberStack;
using UltraCalcValue = Infragistics.Calculations.Engine.CalculationValue;
using IUltraCalcReference = Infragistics.Calculations.Engine.ICalculationReference;
using IUltraCalcFormula = Infragistics.Calculations.Engine.ICalculationFormula;
using UltraCalcErrorValue = Infragistics.Calculations.Engine.CalculationErrorValue;
using UltraCalcReferenceError = Infragistics.Calculations.Engine.CalculationReferenceError;
using UltraCalcException = Infragistics.Calculations.Engine.CalculationException;
using UltraCalcErrorException = Infragistics.Calculations.Engine.CalcErrorException;
using UltraCalcNumberException = Infragistics.Calculations.Engine.CalcNumberException;
using UltraCalcValueException = Infragistics.Calculations.Engine.CalculationValueException;
//using ChildReferenceType = Infragistics.Calculations.Engine.ChildReferenceType;


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


#pragma warning disable 1058









namespace Infragistics.Calculations.Engine







{
	/// <summary>
	/// UCCompliation represents a compiled formula.  It encapsulates the formula parser and is responsible
	/// for formula parsing.
	/// </summary>
	internal class UCCompilation
	{
		#region Member Variables
		/// <summary>
		/// Storage for the context reference where the formula is stored.  Used to create references found in the
		/// formula.
		/// </summary>
		private IUltraCalcReference baseRef;

		/// <summary>
		/// Storage for the string representation of the formula.
		/// </summary>
		private string formula;

		/// <summary>
		/// Storage for the last compilation error message.
		/// </summary>
		private string error;

		/// <summary>
		/// Storage for the root of the parse tree.
		/// </summary>
		private PerCederberg.Grammatica.Parser.Node parsedNode = null;

		/// <summary>
		/// Storage for the formula tokens.
		/// </summary>
		UltraCalcFormulaTokenCollection tokens = null;

		/// <summary>
		/// Storage for the <see cref="UltraCalcFunctionFactory"/> instance used to create function calls.
		/// </summary>
		private UltraCalcFunctionFactory functionFactory = null;

		// MD 7/28/11
		private Exception parseException;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Constructor with default function factory.
		/// </summary>
		public UCCompilation() : this(null)
		{
		}

		/// <summary>
		/// Constructor with a specified function factory.
		/// </summary>
		/// <param name="functionFactory">The function factory used to construct function calls.</param>
		public UCCompilation(UltraCalcFunctionFactory functionFactory)
		{
			if (functionFactory == null)
				this.functionFactory = new UltraCalcFunctionFactory();
			else
				this.functionFactory = functionFactory;
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Return the string representation of the formula.  Read Only.
		/// </summary>
		public string FormulaString
		{
			get 
			{
				return this.formula;
			}
		}

		/// <summary>
		/// Denotes whether the formula contains an always dirty function
		/// </summary>
		public bool HasAlwaysDirty 
		{ 
			get 
			{ 
				// SSP 2/4/05
				// Check for tokens being null. Enclosed the existing code into the if block.
				//
				if ( null != this.Tokens )
				{
					foreach (UltraCalcFormulaToken token in this.Tokens) 
					{
						if (token.Type == UltraCalcFormulaTokenType.Function) 
						{
                            // MRS 12/13/2010 - TFS61341
                            // Don't just return the IsAlwaysDirty of the first function. We need to 
                            // loop through all of the functions until we find one where IsAlwaysDirty
                            // is true (or not). 
                            //
							//return ((UltraCalcFunctionToken)token).Function.IsAlwaysDirty;
                            UltraCalcFunctionToken functionToken = ((UltraCalcFunctionToken)token);
                            UltraCalcFunction function = functionToken.Function;
                            if (function.IsAlwaysDirty)
                                return true;
						}
					}
				}

				return false;
			} 
		}

		/// <summary>
		/// Return the collection of tokens for the parsed formula. Read Only.
		/// </summary>
		public UltraCalcFormulaTokenCollection Tokens
		{
			get 
			{
				return this.tokens;
			}
		}

		/// <summary>
		/// Return the last compilation error message.  Read Only.
		/// </summary>
		public string Error
		{
			get 
			{
				return this.error;
			}
		}

		/// <summary>
		/// Return the root of the parse tree.  Read Only.
		/// </summary>
		public PerCederberg.Grammatica.Parser.Node RootNode
		{
			get
			{
				return this.parsedNode;
			}
		}

		// MD 7/28/11
		/// <summary>
		/// Return the exception which occurred while parsing the formula.
		/// </summary>
		public Exception ParseException
		{
			get
			{
				return this.parseException;
			}
		}

		#endregion //Properties

		#region Methods
		/// <summary>
		/// Parse the formula into a parse tree.  Initializes the FormulaString and RootNode properties.
		/// </summary>
		/// <param name="baseReference">The context reference where the formula is stored.</param>
		/// <param name="formula">The formula to parse.</param>
		/// <returns>true if successful, otherwise Error contains compilation error message</returns>
		public bool Parse( IUltraCalcReference baseReference, string formula )
		{
			try 
			{
				this.tokens = null;
				this.error = null;
				this.baseRef = baseReference;
				this.formula = formula;

				StringReader s = new StringReader( this.formula );
				UCparserParser parser = new UCparserParser( s );
				this.parsedNode = parser.Parse();
			}
			catch( 	PerCederberg.Grammatica.Parser.ParserLogException pe ) 
			{
				this.parsedNode = null;
				this.error = pe.Message;

				// MD 7/28/11
				this.parseException = pe;

				return false;
			}
			catch( Exception ee ) 
			{
				this.parsedNode = null;
				this.error = ee.Message;

				// MD 7/28/11
				this.parseException = ee;

				return false;
			}
			catch
			{
				this.parsedNode = null;
				this.error = "Unknown Exception";
				return false;
			}

			return true;

		}

		/// <summary>
		/// Converts the parsed formula into tokenized form.  Parse must be called first.
		/// <see cref="Parse"/>
		/// </summary>
		/// <returns>true if successful, otherwise Error contains compilation error message</returns>
		public bool Tokenize()
		{
			try 
			{
				this.error = null;

				if( this.parsedNode == null ) 
				{
					this.tokens = null;
					this.error = "Must call Parse First";
					return false;
				}

				this.tokens = new UltraCalcFormulaTokenCollection();

				bool ok = DoPostfixNode( this.parsedNode,this.tokens );
				if( ok == false ) return false;

				this.tokens.TrimToSize();
			}
			catch( Exception ee ) 
			{
				this.tokens = null;
				this.error = ee.Message;

				// MD 8/3/11 - XamFormulaEditor
				this.parseException = ee;

				return false;
			}
			catch
			{
				this.tokens = null;
				this.error = "Unknown Exception";
				return false;
			}

			return true;

		}

		/// <summary>
		/// Recursive function used by Tokenize to walk the parse tree and generate the formula tokens.
		/// </summary>
		/// <param name="n">The current node being evaluated.</param>
		/// <param name="tokens">The formula token collection to append to.</param>
		/// <returns>true if successful, otherwise Error contains compilation error message</returns>
		private bool DoPostfixNode( PerCederberg.Grammatica.Parser.Node n,UltraCalcFormulaTokenCollection tokens ) 
		{
			bool retVal = true;

			try 
			{
				int childCount = n.GetChildCount();
				if( childCount == 1 ) 
				{
					return DoPostfixNode( n.GetChildAt(0),tokens );
				}

				switch( (UCparserConstants)n.GetId() ) 
				{
					case UCparserConstants.NUMBER:
					{
						// AS 9/14/04 Removed operator overloads
						//UltraCalcValue val = ((PerCederberg.Grammatica.Parser.Token)n).GetImage();
						//UltraCalcValue pushVal = (double)val;
						string numberString = ((PerCederberg.Grammatica.Parser.Token)n).GetImage();
						UltraCalcValue pushVal = new UltraCalcValue( ((IConvertible)numberString).ToDouble(System.Globalization.CultureInfo.InvariantCulture) );
						tokens.Add( new UltraCalcValueToken(pushVal) );
					}
						break;

					case UCparserConstants.TEXT:
					{
						UltraCalcValue val = new UltraCalcValue( ((PerCederberg.Grammatica.Parser.Token)n).GetImage() );
						tokens.Add( new UltraCalcValueToken(val) );
					}
						break;

					case UCparserConstants.QUOTED_STRING:
					{
						// SSP 7/10/06 BR13815
						// The previous code trimmed off all " and ' characters so you couldn't have a value 
						// for a double quote ('"' or "\"") or a single quote ("'" or '\''). We should trim 
						// off only one set of quotes.
						// 
						// ------------------------------------------------------------------
						



						string str = ((PerCederberg.Grammatica.Parser.Token)n).GetImage();
						if ( str.Length >= 2 )
						{
							char firstChar = str[0];
							char lastChar = str[ str.Length - 1 ];
							if ( '"' == firstChar && '"' == lastChar
								|| '\'' == firstChar && '\'' == lastChar )
								str = str.Substring( 1, str.Length - 2 );
						}

						// SSP 9/2/11 TFS85766
						// Unescape the string if there are any escaped characters.
						// 
						str = RefParser.UnEscapeString( str );

						UltraCalcValue val = new UltraCalcValue( str );
						// ------------------------------------------------------------------
						tokens.Add( new UltraCalcValueToken(val) );
					}
						break;

					case UCparserConstants.CONSTANT:
					{
						string sval = null;

						int constChildCount = n.GetChildCount();
						for( int i=0;i<constChildCount;++i ) 
						{
							PerCederberg.Grammatica.Parser.Node child = n.GetChildAt(i);
							switch( (UCparserConstants)child.GetId() ) 
							{
								case UCparserConstants.NUMBER:
									sval += ((PerCederberg.Grammatica.Parser.Token)child).GetImage();
									break;

								case UCparserConstants.OP_DOT:
									sval += ".";
									break;

							}
						}

						// AS 9/14/04 Removed operator overloads
						//UltraCalcValue val = sval;
						//UltraCalcValue pushVal = (double)val;
						UltraCalcValue pushVal = new UltraCalcValue( ((IConvertible)sval).ToDouble(System.Globalization.CultureInfo.InvariantCulture) );
						tokens.Add( new UltraCalcValueToken(pushVal) );
						break;
					}

					case UCparserConstants.REFERENCE:
					{
						string sref = ((PerCederberg.Grammatica.Parser.Token)n).GetImage();
						if( sref.Length <= 2 ) 
						{
							// MD 8/3/11 - XamFormulaEditor
							// Instead of passing in the full message directly, give the exception the information and it will construct it.
							//throw new UltraCalcException( SR.GetString( "Error_ParseRef",new Object[] {n.GetStartLine(),n.GetStartColumn()}) );
							throw new UltraCalcException(
								SR.GetString("Error_ParseRef"), 
								n.GetStartLine(), 
								n.GetStartColumn(), 
								new object[] { UltraCalcException.LocationPlaceholder });
						}

						//IUltraCalcReference refVal = this.m_baseRef.CreateReference( sref.Substring(1,sref.Length-2)  );
						UCReference refVal = new UCReference(sref.Substring(1,sref.Length-2));
						refVal.Connect(this.baseRef);
						tokens.Add( new UltraCalcValueToken(new UltraCalcValue( refVal )) );
					}
						break;

					case UCparserConstants.TERM:
					{
						PerCederberg.Grammatica.Parser.Node child1 = n.GetChildAt(1);
						retVal = DoPostfixNode( child1,tokens );
					}
						break;

					case UCparserConstants.FORMULA:
						retVal = DoTermLoop( UCparserConstants.COMPARISON_TERM,n,tokens );
						break;

					case UCparserConstants.COMPARISON_TERM:
						retVal = DoTermLoop( UCparserConstants.CONCAT_TERM,n,tokens );
						break;

					case UCparserConstants.CONCAT_TERM:
						retVal = DoTermLoop( UCparserConstants.ADDITIVE_TERM,n,tokens );
						break;

					case UCparserConstants.ADDITIVE_TERM:
						retVal = DoTermLoop( UCparserConstants.MULT_TERM,n,tokens );
						break;

					case UCparserConstants.MULT_TERM:
						retVal = DoTermLoop( UCparserConstants.EXPON_TERM,n,tokens );
						break;

					case UCparserConstants.EXPON_TERM:
					{
						if( DoPostfixNode(n.GetChildAt(0),tokens) == false ) return false;
						retVal = DoOpNode( n.GetChildAt(1).GetChildAt(0),tokens );
					}
						break;

					case UCparserConstants.POSTFIX_TERM:
					{
						if( DoPostfixNode(n.GetChildAt(1),tokens) == false ) return false;
						PerCederberg.Grammatica.Parser.Node prefixop = n.GetChildAt(0).GetChildAt(0);
						if( (UCparserConstants)prefixop.GetId() == UCparserConstants.OP_MINUS )
						{
							UltraCalcFunction function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.UnaryMinus);
							int arity = UltraCalcFunctionFactory.GetOperatorArity(UltraCalcOperatorFunction.UnaryMinus);

                            // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
							//tokens.Add( new UltraCalcFunctionToken(function, arity) );
                            tokens.Add(new UltraCalcFunctionToken(function, arity, UltraCalcOperatorFunction.UnaryMinus));
						} 
						else if( (UCparserConstants)prefixop.GetId() == UCparserConstants.OP_PLUS )
						{
							UltraCalcFunction function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.UnaryPlus);
							int arity = UltraCalcFunctionFactory.GetOperatorArity(UltraCalcOperatorFunction.UnaryPlus);

                            // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
							//tokens.Add( new UltraCalcFunctionToken(function, arity) );
                            tokens.Add(new UltraCalcFunctionToken(function, arity, UltraCalcOperatorFunction.UnaryPlus));
						}
					}
						break;

					case UCparserConstants.FUNC_ARGS:
					{
						for( int i=0;i<childCount;++i ) 
						{
							PerCederberg.Grammatica.Parser.Node child = n.GetChildAt(i);
							if( (UCparserConstants)child.GetId() == UCparserConstants.FUNC_ARG ) 
							{
								if( DoPostfixNode(child,tokens) == false ) return false;
							}
						}
					}
						break;

					case UCparserConstants.FUNCTION:
					{
						PerCederberg.Grammatica.Parser.Node func_id = n.GetChildAt(0);
						string func_name = ConcatChildren( func_id );

						PerCederberg.Grammatica.Parser.Node maybe_args = n.GetChildAt(2);
						int arg_count = 0;
						if( (UCparserConstants)maybe_args.GetId() == UCparserConstants.FUNC_ARGS ) 
						{
							if( DoPostfixNode(maybe_args,tokens) == false ) return false;
							arg_count = CountFuncArgs(maybe_args);
						} 

						UltraCalcFunction function = this.functionFactory[func_name];

						if (function == null)
						{
							// MD 8/3/11 - XamFormulaEditor
							// Instead of passing in the full message directly, give the exception the information and it will construct it.
							//throw new UltraCalcException( SR.GetString( "Error_InvalidFunction",new Object[] {func_name, n.GetStartLine(),n.GetStartColumn()}) );
							throw new UltraCalcException(
								SR.GetString("Error_InvalidFunction"),
								n.GetStartLine(),
								n.GetStartColumn(),
								new Object[] { func_name, UltraCalcException.LocationPlaceholder });
						}
						else if (arg_count < function.MinArgs)
						{
							// MD 8/3/11 - XamFormulaEditor
							// Instead of passing in the full message directly, give the exception the information and it will construct it.
							//throw new UltraCalcException( SR.GetString( "Error_InvalidFunctionArgCountTooFew",new Object[] {func_name, n.GetStartLine(),n.GetStartColumn(), function.MinArgs}) );
							throw new UltraCalcException(
								SR.GetString("Error_InvalidFunctionArgCountTooFew"),
								n.GetStartLine(),
								n.GetStartColumn(),
								new Object[] { func_name, UltraCalcException.LocationPlaceholder, function.MinArgs });
						}
						else if (arg_count > function.MaxArgs)
						{
							// MD 8/3/11 - XamFormulaEditor
							// Instead of passing in the full message directly, give the exception the information and it will construct it.
							//throw new UltraCalcException( SR.GetString( "Error_InvalidFunctionArgCountTooMany",new Object[] {func_name, n.GetStartLine(),n.GetStartColumn(), function.MaxArgs}) );
							throw new UltraCalcException(
								SR.GetString("Error_InvalidFunctionArgCountTooMany"), 
								n.GetStartLine(), 
								n.GetStartColumn(), 
								new Object[] { func_name, UltraCalcException.LocationPlaceholder, function.MaxArgs });
						}
						else                    
        				{
							tokens.Add( new UltraCalcFunctionToken(function, arg_count) );
						}
					}
						break;

					case UCparserConstants.RANGE:
					{
						PerCederberg.Grammatica.Parser.Node ref0 = n.GetChildAt(0);
						PerCederberg.Grammatica.Parser.Node ref1 = n.GetChildAt(2);
						string from = ((PerCederberg.Grammatica.Parser.Token)ref0).GetImage();
						if( from.Length <= 2 ) 
						{
							// MD 8/3/11 - XamFormulaEditor
							// Instead of passing in the full message directly, give the exception the information and it will construct it.
							//throw new UltraCalcException( SR.GetString( "Error_ParseRef",new Object[] {n.GetStartLine(),n.GetStartColumn()}) );
							throw new UltraCalcException(
								SR.GetString("Error_ParseRef"), 
								n.GetStartLine(), 
								n.GetStartColumn(),
								new Object[] { UltraCalcException.LocationPlaceholder });
						}

						string to = ((PerCederberg.Grammatica.Parser.Token)ref1).GetImage();
						if( to.Length <= 2 ) 
						{
							// MD 8/3/11 - XamFormulaEditor
							// Instead of passing in the full message directly, give the exception the information and it will construct it.
							//throw new UltraCalcException( SR.GetString( "Error_ParseRef",new Object[] {n.GetStartLine(),n.GetStartColumn()}) );
							throw new UltraCalcException(
								SR.GetString("Error_ParseRef"),
								n.GetStartLine(),
								n.GetStartColumn(),
								new Object[] { UltraCalcException.LocationPlaceholder });
						}

						UCReference refVal = new UCReference(from.Substring(1,from.Length-2),to.Substring(1,to.Length-2));
						refVal.Connect(this.baseRef);
						tokens.Add( new UltraCalcValueToken(new UltraCalcValue( refVal )) );

						//						IUltraCalcReference refVal = this.baseRef.CreateRange( from.Substring(1,from.Length-2),to.Substring(1,to.Length-2)  );
						//						UltraCalcValue val = new UltraCalcValue( refVal );
						//						tokens.Add( val );
					}
						break;
				}
			} 
			catch( Exception ee ) 
			{
				this.error = ee.Message;

				// MD 8/3/11 - XamFormulaEditor
				this.parseException = ee;

				return false;
			}
			catch
			{
				this.error = "Unknown Exception";
				return false;
			}

			return retVal;
		}

		/// <summary>
		/// Function used by DoPostfixNode to extract a collection of terms from the parse tree.
		/// </summary>
		/// <param name="termID">The term identifier.</param>
		/// <param name="n">The root node of the term collection.</param>
		/// <param name="tokens">The formula token collection to append to.</param>
		/// <returns>true if successful, otherwise Error contains compilation error message</returns>
		private bool DoTermLoop( UCparserConstants termID,PerCederberg.Grammatica.Parser.Node n,UltraCalcFormulaTokenCollection tokens )
		{
			try 
			{
				int childCount = n.GetChildCount();

				for( int i=0;i<childCount;++i ) 
				{
					PerCederberg.Grammatica.Parser.Node child = n.GetChildAt(i);
					if( (UCparserConstants)child.GetId() == termID ) 
					{
						if( DoPostfixNode(child,tokens) == false ) return false;
					}
					else
					{
						PerCederberg.Grammatica.Parser.Node nextTerm = n.GetChildAt(i+1);
						if( DoPostfixNode(nextTerm,tokens) == false ) return false;
						++i;
						PerCederberg.Grammatica.Parser.Node opNode = child.GetChildAt(0);
						DoOpNode( opNode,tokens );
					}
				}
			} 
			catch( Exception ee ) 
			{
				this.error = ee.Message;

				// MD 8/3/11 - XamFormulaEditor
				this.parseException = ee;

				return false;
			}
			catch
			{
				this.error = "Unknown Exception";
				return false;
			}

			return true;
		}


		/// <summary>
		/// Function used by DoPostfixNode to extract an Operator from the parse tree.
		/// </summary>
		/// <param name="n">The operator node.</param>
		/// <param name="tokens">The formula token collection to append to.</param>
		/// <returns>true if successful, otherwise Error contains compilation error message</returns>
		private bool DoOpNode( PerCederberg.Grammatica.Parser.Node n,UltraCalcFormulaTokenCollection tokens ) 
		{
			try 
			{
				UltraCalcFunction uf = null;
				int arity = 2;

                // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
                // Changed this to store the enum
                #region Old Code
                //switch( (UCparserConstants)n.GetId() ) 
                //{
                //    case UCparserConstants.OP_CONCAT:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Concatenate);
                //        break;
                //    case UCparserConstants.OP_DIV:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Divide);
                //        break;
                //    case UCparserConstants.OP_EQUAL:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Equal);
                //        break;
                //    case UCparserConstants.OP_EXPON:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Exponent);
                //        break;
                //    case UCparserConstants.OP_GE:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.GreaterThanOrEqual);
                //        break;
                //    case UCparserConstants.OP_GT:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.GreaterThan);
                //        break;
                //    case UCparserConstants.OP_LE:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.LessThanOrEqual);
                //        break;
                //    case UCparserConstants.OP_LT:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.LessThan);
                //        break;
                //    case UCparserConstants.OP_MINUS:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Subtract);
                //        break;
                //    case UCparserConstants.OP_NE:
                //    case UCparserConstants.OP_ALT_NE:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.NotEqual);
                //        break;
                //    case UCparserConstants.OP_PERCENT:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Percent);
                //        arity = UltraCalcFunctionFactory.GetOperatorArity(UltraCalcOperatorFunction.Percent);
                //        break;
                //    case UCparserConstants.OP_PLUS:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Add);
                //        break;
                //    case UCparserConstants.OP_TIMES:
                //        uf = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Multiply);
                //        break;
                //}

                //if (uf == null)
                //{
                //    this.error = "Unknown OP " + n.GetId().ToString();
                //    return false;
                //}
                //else
                //{
                //    // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
                //    //tokens.Add( new UltraCalcFunctionToken(uf, arity) );
                //    tokens.Add(new UltraCalcFunctionToken(uf, arity, uf));
                //}
                #endregion //Old Code
                UltraCalcOperatorFunction ultraCalcOperatorFunction;
                switch ((UCparserConstants)n.GetId())
                {
                    case UCparserConstants.OP_CONCAT:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.Concatenate;
                        break;
                    case UCparserConstants.OP_DIV:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.Divide;
                        break;
                    case UCparserConstants.OP_EQUAL:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.Equal;
                        break;
                    case UCparserConstants.OP_EXPON:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.Exponent;
                        break;
                    case UCparserConstants.OP_GE:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.GreaterThanOrEqual;
                        break;
                    case UCparserConstants.OP_GT:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.GreaterThan;
                        break;
                    case UCparserConstants.OP_LE:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.LessThanOrEqual;
                        break;
                    case UCparserConstants.OP_LT:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.LessThan;
                        break;
                    case UCparserConstants.OP_MINUS:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.Subtract;
                        break;
                    case UCparserConstants.OP_NE:
                    case UCparserConstants.OP_ALT_NE:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.NotEqual;
                        break;
                    case UCparserConstants.OP_PERCENT:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.Percent;                        
                        arity = UltraCalcFunctionFactory.GetOperatorArity(UltraCalcOperatorFunction.Percent);
                        break;
                    case UCparserConstants.OP_PLUS:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.Add;
                        break;
                    case UCparserConstants.OP_TIMES:
                        ultraCalcOperatorFunction = UltraCalcOperatorFunction.Multiply;
                        break;
                    default:
                        this.error = "Unknown OP " + n.GetId().ToString();
                        return false;
                }

                uf = this.functionFactory.GetOperator(ultraCalcOperatorFunction);
                tokens.Add(new UltraCalcFunctionToken(uf, arity, ultraCalcOperatorFunction));
			} 
			catch( Exception ee ) 
			{
				this.error = ee.Message;

				// MD 8/3/11 - XamFormulaEditor
				this.parseException = ee;

				return false;
			}
			catch
			{
				this.error = "Unknown Exception";
				return false;
			}

			return true;
		}


		/// <summary>
		/// Function used by DoPostfixNode to concatenate a collection of nodes.
		/// <param name="n">The root node to concatenate.</param>
		/// <returns>true if successful, otherwise Error contains compilation error message</returns>
		/// </summary>
		private string ConcatChildren( PerCederberg.Grammatica.Parser.Node n )
		{
			try 
			{
				int childCount = n.GetChildCount();
				string concat = "";

				for( int i=0;i<childCount;++i ) 
				{
					PerCederberg.Grammatica.Parser.Node child = n.GetChildAt(i);
					concat += ((PerCederberg.Grammatica.Parser.Token)child).GetImage();
				}
				
				return concat;
			} 
			catch( Exception ee ) 
			{
				Debug.WriteLine( "ConcatChildren exception: " + ee.Message );
			}
			catch
			{
				Debug.WriteLine( "ConcatChildren exception" );
			}
			return "";
		}

		
		/// <summary>
		/// Function used by DoPostfixNode to determine the number of function arguments for a function.
		/// <param name="n">The function node.</param>
		/// <returns>the argument count.</returns>
		/// </summary>
		private int CountFuncArgs( PerCederberg.Grammatica.Parser.Node n )
		{
			try 
			{
				int childCount = n.GetChildCount();
				int argCount = 0;

				for( int i=0;i<childCount;++i ) 
				{
					PerCederberg.Grammatica.Parser.Node child = n.GetChildAt(i);
					if( (UCparserConstants)child.GetId() == UCparserConstants.FUNC_ARG ) 
					{
						++argCount;
					}
				}
				
				return argCount;
			} 
			catch( Exception ee ) 
			{
				Debug.WriteLine( "count_args exception: " + ee.Message );
			}
			catch
			{
				Debug.WriteLine( "unknown exception" );
			}
			return 0;
		}
		#endregion //Methods
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