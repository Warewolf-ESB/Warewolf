using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class SourceUnitTree : Node
	{
		private readonly LexicalScope _definedScope;

		private readonly List<FileInitializerStatement> _initializers;

		private readonly Statements _statements;

		private readonly RubyEncoding _encoding;

		private readonly int _dataOffset;

		public List<FileInitializerStatement> Initializers
		{
			get
			{
				return _initializers;
			}
		}

		public Statements Statements
		{
			get
			{
				return _statements;
			}
		}

		public RubyEncoding Encoding
		{
			get
			{
				return _encoding;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.SourceUnitTree;
			}
		}

		public SourceUnitTree(LexicalScope definedScope, Statements statements, List<FileInitializerStatement> initializers, RubyEncoding encoding, int dataOffset)
			: base(SourceSpan.None)
		{
			_definedScope = definedScope;
			_statements = statements;
			_initializers = initializers;
			_encoding = encoding;
			_dataOffset = dataOffset;
		}

		private ScopeBuilder DefineLocals()
		{
			return new ScopeBuilder(_definedScope.AllocateClosureSlotsForLocals(0), null, _definedScope);
		}

		internal Expression<T> Transform<T>(AstGenerator gen)
		{
			ScopeBuilder scopeBuilder = DefineLocals();
			ParameterExpression[] array;
			ParameterExpression parameterExpression;
			ParameterExpression selfParameter;
			ParameterExpression blockParameter;
			if (gen.CompilerOptions.FactoryKind == TopScopeFactoryKind.None || gen.CompilerOptions.FactoryKind == TopScopeFactoryKind.ModuleEval)
			{
				array = new ParameterExpression[4];
				parameterExpression = (array[0] = System.Linq.Expressions.Expression.Parameter(typeof(RubyScope), "#scope"));
				selfParameter = (array[1] = System.Linq.Expressions.Expression.Parameter(typeof(object), "#self"));
				array[2] = System.Linq.Expressions.Expression.Parameter(typeof(RubyModule), "#module");
				blockParameter = (array[3] = System.Linq.Expressions.Expression.Parameter(typeof(Proc), "#block"));
			}
			else
			{
				array = new ParameterExpression[2];
				parameterExpression = (array[0] = System.Linq.Expressions.Expression.Parameter(typeof(RubyScope), "#scope"));
				selfParameter = (array[1] = System.Linq.Expressions.Expression.Parameter(typeof(object), "#self"));
				blockParameter = null;
			}
			gen.EnterSourceUnit(scopeBuilder, selfParameter, parameterExpression, blockParameter, gen.CompilerOptions.TopLevelMethodName, null);
			System.Linq.Expressions.Expression body;
			if (_statements.Count > 0)
			{
				if (gen.PrintInteractiveResult)
				{
					ParameterExpression parameterExpression2 = scopeBuilder.DefineHiddenVariable("#result", typeof(object));
					System.Linq.Expressions.Expression epilogue = Methods.PrintInteractiveResult.OpCall(parameterExpression, Microsoft.Scripting.Ast.Utils.LightDynamic(ConvertToSAction.Make(gen.Context), typeof(MutableString), CallSiteBuilder.InvokeMethod(gen.Context, "inspect", RubyCallSignature.WithScope(0), gen.CurrentScopeVariable, parameterExpression2)));
					body = gen.TransformStatements(null, _statements, epilogue, ResultOperation.Store(parameterExpression2));
				}
				else
				{
					body = gen.TransformStatements(_statements, ResultOperation.Return);
				}
				ParameterExpression parameterExpression3 = System.Linq.Expressions.Expression.Parameter(typeof(Exception), "#exception");
				body = Microsoft.Scripting.Ast.Utils.Try(body).Filter(parameterExpression3, Methods.TraceTopLevelCodeFrame.OpCall(parameterExpression, parameterExpression3), System.Linq.Expressions.Expression.Empty());
			}
			else
			{
				body = Microsoft.Scripting.Ast.Utils.Constant(null);
			}
			System.Linq.Expressions.Expression expression;
			switch (gen.CompilerOptions.FactoryKind)
			{
			case TopScopeFactoryKind.None:
			case TopScopeFactoryKind.ModuleEval:
				expression = Methods.InitializeScopeNoLocals.OpCall(parameterExpression, EnterInterpretedFrameExpression.Instance);
				break;
			case TopScopeFactoryKind.Hosted:
			case TopScopeFactoryKind.File:
			case TopScopeFactoryKind.WrappedFile:
				expression = Methods.InitializeScope.OpCall(parameterExpression, scopeBuilder.MakeLocalsStorage(), scopeBuilder.GetVariableNamesExpression(), EnterInterpretedFrameExpression.Instance);
				break;
			case TopScopeFactoryKind.Main:
				expression = Methods.InitializeScope.OpCall(parameterExpression, scopeBuilder.MakeLocalsStorage(), scopeBuilder.GetVariableNamesExpression(), EnterInterpretedFrameExpression.Instance);
				if (_dataOffset >= 0)
				{
					expression = System.Linq.Expressions.Expression.Block(expression, Methods.SetDataConstant.OpCall(parameterExpression, gen.SourcePathConstant, Microsoft.Scripting.Ast.Utils.Constant(_dataOffset)));
				}
				break;
			default:
				throw Assert.Unreachable;
			}
			if (gen.FileInitializers != null)
			{
				BlockBuilder blockBuilder = new BlockBuilder();
				blockBuilder.Add(expression);
				blockBuilder.Add(gen.FileInitializers);
				blockBuilder.Add(body);
				body = blockBuilder;
			}
			body = gen.AddReturnTarget(scopeBuilder.CreateScope(body));
			gen.LeaveSourceUnit();
			return System.Linq.Expressions.Expression.Lambda<T>(body, GetEncodedName(gen), array);
		}

		private static string GetEncodedName(AstGenerator gen)
		{
			return RubyStackTraceBuilder.EncodeMethodName("#", gen.SourcePath, SourceSpan.None, gen.DebugMode);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
