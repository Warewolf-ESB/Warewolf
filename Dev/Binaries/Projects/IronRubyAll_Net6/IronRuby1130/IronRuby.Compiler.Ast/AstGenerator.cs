using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;

namespace IronRuby.Compiler.Ast
{
	internal sealed class AstGenerator
	{
		public abstract class LexicalScope
		{
			public LexicalScope Parent;

			public virtual bool IsLambda
			{
				get
				{
					return false;
				}
			}
		}

		public sealed class LoopScope : LexicalScope
		{
			private readonly System.Linq.Expressions.Expression _redoVariable;

			private readonly System.Linq.Expressions.Expression _resultVariable;

			private readonly LabelTarget _breakLabel;

			private readonly LabelTarget _continueLabel;

			public LoopScope _parentLoop;

			public System.Linq.Expressions.Expression RedoVariable
			{
				get
				{
					return _redoVariable;
				}
			}

			public System.Linq.Expressions.Expression ResultVariable
			{
				get
				{
					return _resultVariable;
				}
			}

			public LabelTarget BreakLabel
			{
				get
				{
					return _breakLabel;
				}
			}

			public LabelTarget ContinueLabel
			{
				get
				{
					return _continueLabel;
				}
			}

			public LoopScope ParentLoop
			{
				get
				{
					return _parentLoop;
				}
				set
				{
					_parentLoop = value;
				}
			}

			public LoopScope(System.Linq.Expressions.Expression redoVariable, System.Linq.Expressions.Expression resultVariable, LabelTarget breakLabel, LabelTarget continueLabel)
			{
				_redoVariable = redoVariable;
				_resultVariable = resultVariable;
				_breakLabel = breakLabel;
				_continueLabel = continueLabel;
			}
		}

		public sealed class RescueScope : LexicalScope
		{
			private readonly System.Linq.Expressions.Expression _retryingVariable;

			private readonly LabelTarget _retryLabel;

			public RescueScope _parentRescue;

			public System.Linq.Expressions.Expression RetryingVariable
			{
				get
				{
					return _retryingVariable;
				}
			}

			public RescueScope ParentRescue
			{
				get
				{
					return _parentRescue;
				}
				set
				{
					_parentRescue = value;
				}
			}

			public LabelTarget RetryLabel
			{
				get
				{
					return _retryLabel;
				}
			}

			public RescueScope(System.Linq.Expressions.Expression retryingVariable, LabelTarget retryLabel)
			{
				_retryingVariable = retryingVariable;
				_retryLabel = retryLabel;
			}
		}

		public class VariableScope : LexicalScope
		{
			private readonly ScopeBuilder _builder;

			private readonly System.Linq.Expressions.Expression _selfVariable;

			private readonly ParameterExpression _runtimeScopeVariable;

			private VariableScope _parentVariableScope;

			public ScopeBuilder Builder
			{
				get
				{
					return _builder;
				}
			}

			public System.Linq.Expressions.Expression SelfVariable
			{
				get
				{
					return _selfVariable;
				}
			}

			public ParameterExpression RuntimeScopeVariable
			{
				get
				{
					return _runtimeScopeVariable;
				}
			}

			public VariableScope ParentVariableScope
			{
				get
				{
					return _parentVariableScope;
				}
				set
				{
					_parentVariableScope = value;
				}
			}

			public VariableScope(ScopeBuilder locals, System.Linq.Expressions.Expression selfVariable, ParameterExpression runtimeScopeVariable)
			{
				_builder = locals;
				_runtimeScopeVariable = runtimeScopeVariable;
				_selfVariable = selfVariable;
			}
		}

		public abstract class FrameScope : VariableScope
		{
			private readonly int _uniqueId;

			private BlockScope _parentBlock;

			private RescueScope _parentRescue;

			private LoopScope _parentLoop;

			private LabelTarget _returnLabel;

			public int UniqueId
			{
				get
				{
					return _uniqueId;
				}
			}

			public BlockScope ParentBlock
			{
				get
				{
					return _parentBlock;
				}
				set
				{
					_parentBlock = value;
				}
			}

			public RescueScope ParentRescue
			{
				get
				{
					return _parentRescue;
				}
				set
				{
					_parentRescue = value;
				}
			}

			public LoopScope ParentLoop
			{
				get
				{
					return _parentLoop;
				}
				set
				{
					_parentLoop = value;
				}
			}

			internal LabelTarget ReturnLabel
			{
				get
				{
					if (_returnLabel == null)
					{
						_returnLabel = System.Linq.Expressions.Expression.Label(typeof(object));
					}
					return _returnLabel;
				}
			}

			public FrameScope(ScopeBuilder builder, System.Linq.Expressions.Expression selfVariable, ParameterExpression runtimeScopeVariable)
				: base(builder, selfVariable, runtimeScopeVariable)
			{
				_uniqueId = Interlocked.Increment(ref _UniqueId);
			}

			internal System.Linq.Expressions.Expression AddReturnTarget(System.Linq.Expressions.Expression expression)
			{
				if (expression.Type != typeof(object))
				{
					expression = Microsoft.Scripting.Ast.Utils.Convert(expression, typeof(object));
				}
				if (_returnLabel != null)
				{
					expression = System.Linq.Expressions.Expression.Label(_returnLabel, expression);
					_returnLabel = null;
				}
				return expression;
			}
		}

		public sealed class BlockScope : FrameScope
		{
			private readonly System.Linq.Expressions.Expression _bfcVariable;

			private readonly LabelTarget _redoLabel;

			public override bool IsLambda
			{
				get
				{
					return true;
				}
			}

			public System.Linq.Expressions.Expression BfcVariable
			{
				get
				{
					return _bfcVariable;
				}
			}

			public LabelTarget RedoLabel
			{
				get
				{
					return _redoLabel;
				}
			}

			public BlockScope(ScopeBuilder builder, System.Linq.Expressions.Expression selfVariable, ParameterExpression runtimeScopeVariable, System.Linq.Expressions.Expression bfcVariable, LabelTarget redoLabel)
				: base(builder, selfVariable, runtimeScopeVariable)
			{
				_bfcVariable = bfcVariable;
				_redoLabel = redoLabel;
			}
		}

		public sealed class MethodScope : FrameScope
		{
			private readonly System.Linq.Expressions.Expression _blockVariable;

			private readonly string _methodName;

			private readonly Parameters _parameters;

			private MethodScope _parentMethod;

			public override bool IsLambda
			{
				get
				{
					return true;
				}
			}

			public System.Linq.Expressions.Expression BlockVariable
			{
				get
				{
					return _blockVariable;
				}
			}

			public MethodScope ParentMethod
			{
				get
				{
					return _parentMethod;
				}
				set
				{
					_parentMethod = value;
				}
			}

			public string MethodName
			{
				get
				{
					return _methodName;
				}
			}

			public Parameters Parameters
			{
				get
				{
					return _parameters;
				}
			}

			public MethodScope(ScopeBuilder builder, System.Linq.Expressions.Expression selfVariable, ParameterExpression runtimeScopeVariable, System.Linq.Expressions.Expression blockVariable, string methodName, Parameters parameters)
				: base(builder, selfVariable, runtimeScopeVariable)
			{
				_blockVariable = blockVariable;
				_methodName = methodName;
				_parameters = parameters;
			}
		}

		public sealed class ModuleScope : VariableScope
		{
			private readonly bool _isSingleton;

			private ModuleScope _parentModule;

			public ModuleScope ParentModule
			{
				get
				{
					return _parentModule;
				}
				set
				{
					_parentModule = value;
				}
			}

			public bool IsSingleton
			{
				get
				{
					return _isSingleton;
				}
			}

			public ModuleScope(ScopeBuilder builder, System.Linq.Expressions.Expression selfVariable, ParameterExpression runtimeScopeVariable, bool isSingleton)
				: base(builder, selfVariable, runtimeScopeVariable)
			{
				_isSingleton = isSingleton;
			}
		}

		private static int _UniqueId;

		private readonly RubyContext _context;

		private readonly RubyCompilerOptions _compilerOptions;

		private readonly SymbolDocumentInfo _document;

		private readonly System.Linq.Expressions.Expression _sequencePointClearance;

		private readonly RubyEncoding _encoding;

		private readonly Profiler _profiler;

		private readonly bool _printInteractiveResult;

		private readonly bool _debugCompiler;

		private readonly bool _debugMode;

		private readonly bool _traceEnabled;

		private readonly bool _savingToDisk;

		private IList<System.Linq.Expressions.Expression> _fileInitializers;

		private System.Linq.Expressions.Expression _sourcePathConstant;

		private LexicalScope _currentElement;

		private MethodScope _currentMethod;

		private BlockScope _currentBlock;

		private LoopScope _currentLoop;

		private RescueScope _currentRescue;

		private VariableScope _currentVariableScope;

		private MethodScope _topLevelScope;

		private ModuleScope _currentModule;

		public RubyCompilerOptions CompilerOptions
		{
			get
			{
				return _compilerOptions;
			}
		}

		public bool DebugMode
		{
			get
			{
				return _debugMode;
			}
		}

		public Profiler Profiler
		{
			get
			{
				return _profiler;
			}
		}

		public bool DebugCompiler
		{
			get
			{
				return _debugCompiler;
			}
		}

		public bool TraceEnabled
		{
			get
			{
				return _traceEnabled;
			}
		}

		public bool SavingToDisk
		{
			get
			{
				return _savingToDisk;
			}
		}

		public string SourcePath
		{
			get
			{
				if (_document == null)
				{
					return "(eval)";
				}
				return _document.FileName;
			}
		}

		public SymbolDocumentInfo Document
		{
			get
			{
				return _document;
			}
		}

		public bool PrintInteractiveResult
		{
			get
			{
				return _printInteractiveResult;
			}
		}

		public System.Linq.Expressions.Expression SourcePathConstant
		{
			get
			{
				if (_sourcePathConstant == null)
				{
					_sourcePathConstant = System.Linq.Expressions.Expression.Constant(SourcePath, typeof(string));
				}
				return _sourcePathConstant;
			}
		}

		public RubyEncoding Encoding
		{
			get
			{
				return _encoding;
			}
		}

		internal RubyContext Context
		{
			get
			{
				return _context;
			}
		}

		internal IList<System.Linq.Expressions.Expression> FileInitializers
		{
			get
			{
				return _fileInitializers ?? ((IList<System.Linq.Expressions.Expression>)AstFactory.EmptyExpressions);
			}
		}

		public MethodScope CurrentMethod
		{
			get
			{
				return _currentMethod;
			}
		}

		public FrameScope CurrentFrame
		{
			get
			{
				return (FrameScope)(((object)CurrentBlock) ?? ((object)CurrentMethod));
			}
		}

		public BlockScope CurrentBlock
		{
			get
			{
				return _currentBlock;
			}
		}

		public LoopScope CurrentLoop
		{
			get
			{
				return _currentLoop;
			}
		}

		public RescueScope CurrentRescue
		{
			get
			{
				return _currentRescue;
			}
		}

		public System.Linq.Expressions.Expression CurrentSelfVariable
		{
			get
			{
				return _currentVariableScope.SelfVariable;
			}
		}

		public ParameterExpression CurrentScopeVariable
		{
			get
			{
				return _currentVariableScope.RuntimeScopeVariable;
			}
		}

		public ScopeBuilder CurrentScope
		{
			get
			{
				return _currentVariableScope.Builder;
			}
		}

		public MethodScope TopLevelScope
		{
			get
			{
				return _topLevelScope;
			}
		}

		internal LabelTarget ReturnLabel
		{
			get
			{
				return CurrentFrame.ReturnLabel;
			}
		}

		internal AstGenerator(RubyContext context, RubyCompilerOptions options, SymbolDocumentInfo document, RubyEncoding encoding, bool printInteractiveResult)
		{
			_context = context;
			_compilerOptions = options;
			_debugMode = context.DomainManager.Configuration.DebugMode;
			_traceEnabled = context.RubyOptions.EnableTracing;
			_document = document;
			_sequencePointClearance = ((document != null) ? System.Linq.Expressions.Expression.ClearDebugInfo(document) : null);
			_encoding = encoding;
			_profiler = (context.RubyOptions.Profile ? Profiler.Instance : null);
			_savingToDisk = context.RubyOptions.SavePath != null;
			_printInteractiveResult = printInteractiveResult;
			_debugCompiler = Snippets.Shared.SaveSnippets;
		}

		internal void AddFileInitializer(System.Linq.Expressions.Expression expression)
		{
			if (_fileInitializers == null)
			{
				_fileInitializers = new List<System.Linq.Expressions.Expression>();
			}
			_fileInitializers.Add(expression);
		}

		public void EnterLoop(System.Linq.Expressions.Expression redoVariable, System.Linq.Expressions.Expression resultVariable, LabelTarget breakLabel, LabelTarget continueLabel)
		{
			LoopScope loopScope = new LoopScope(redoVariable, resultVariable, breakLabel, continueLabel);
			loopScope.Parent = _currentElement;
			loopScope.ParentLoop = _currentLoop;
			_currentElement = (_currentLoop = loopScope);
		}

		public void LeaveLoop()
		{
			_currentElement = _currentLoop.Parent;
			_currentLoop = _currentLoop.ParentLoop;
		}

		public void EnterRescueClause(System.Linq.Expressions.Expression retryingVariable, LabelTarget retryLabel)
		{
			RescueScope rescueScope = new RescueScope(retryingVariable, retryLabel);
			rescueScope.Parent = _currentElement;
			rescueScope.ParentRescue = _currentRescue;
			_currentElement = (_currentRescue = rescueScope);
		}

		public void LeaveRescueClause()
		{
			_currentElement = _currentRescue.Parent;
			_currentRescue = _currentRescue.ParentRescue;
		}

		public void EnterBlockDefinition(ScopeBuilder locals, System.Linq.Expressions.Expression bfcVariable, System.Linq.Expressions.Expression selfVariable, ParameterExpression runtimeScopeVariable, LabelTarget redoLabel)
		{
			BlockScope blockScope = new BlockScope(locals, selfVariable, runtimeScopeVariable, bfcVariable, redoLabel);
			blockScope.Parent = _currentElement;
			blockScope.ParentRescue = _currentRescue;
			blockScope.ParentLoop = _currentLoop;
			blockScope.ParentBlock = _currentBlock;
			blockScope.ParentVariableScope = _currentVariableScope;
			_currentElement = blockScope;
			_currentRescue = null;
			_currentLoop = null;
			_currentBlock = blockScope;
			_currentVariableScope = blockScope;
		}

		public void LeaveBlockDefinition()
		{
			BlockScope currentBlock = _currentBlock;
			_currentElement = currentBlock.Parent;
			_currentRescue = currentBlock.ParentRescue;
			_currentLoop = currentBlock.ParentLoop;
			_currentVariableScope = currentBlock.ParentVariableScope;
			_currentBlock = currentBlock.ParentBlock;
		}

		public void EnterMethodDefinition(ScopeBuilder locals, System.Linq.Expressions.Expression selfParameter, ParameterExpression runtimeScopeVariable, System.Linq.Expressions.Expression blockParameter, string methodName, Parameters parameters)
		{
			MethodScope methodScope = new MethodScope(locals, selfParameter, runtimeScopeVariable, blockParameter, methodName, parameters);
			methodScope.Parent = _currentElement;
			methodScope.ParentRescue = _currentRescue;
			methodScope.ParentLoop = _currentLoop;
			methodScope.ParentBlock = _currentBlock;
			methodScope.ParentVariableScope = _currentVariableScope;
			methodScope.ParentMethod = _currentMethod;
			_currentElement = methodScope;
			_currentRescue = null;
			_currentLoop = null;
			_currentBlock = null;
			_currentVariableScope = methodScope;
			_currentMethod = methodScope;
		}

		public void LeaveMethodDefinition()
		{
			MethodScope currentMethod = _currentMethod;
			_currentElement = currentMethod.Parent;
			_currentRescue = currentMethod.ParentRescue;
			_currentLoop = currentMethod.ParentLoop;
			_currentBlock = currentMethod.ParentBlock;
			_currentVariableScope = currentMethod.ParentVariableScope;
			_currentMethod = currentMethod.ParentMethod;
		}

		public void EnterModuleDefinition(ScopeBuilder locals, System.Linq.Expressions.Expression selfVariable, ParameterExpression runtimeScopeVariable, bool isSingleton)
		{
			ModuleScope moduleScope = new ModuleScope(locals, selfVariable, runtimeScopeVariable, isSingleton);
			moduleScope.Parent = _currentElement;
			moduleScope.ParentVariableScope = _currentVariableScope;
			moduleScope.ParentModule = _currentModule;
			_currentElement = moduleScope;
			_currentVariableScope = moduleScope;
			_currentModule = moduleScope;
		}

		public void LeaveModuleDefinition()
		{
			ModuleScope currentModule = _currentModule;
			_currentElement = currentModule.Parent;
			_currentVariableScope = currentModule.ParentVariableScope;
			_currentModule = currentModule.ParentModule;
		}

		public void EnterFileInitializer(ScopeBuilder locals, System.Linq.Expressions.Expression selfVariable, ParameterExpression runtimeScopeVariable)
		{
			VariableScope variableScope = new VariableScope(locals, selfVariable, runtimeScopeVariable);
			variableScope.Parent = _currentElement;
			variableScope.ParentVariableScope = _currentVariableScope;
			_currentElement = variableScope;
			_currentVariableScope = variableScope;
		}

		public void LeaveFileInitializer()
		{
			VariableScope currentVariableScope = _currentVariableScope;
			_currentElement = currentVariableScope.Parent;
			_currentVariableScope = currentVariableScope.ParentVariableScope;
		}

		public void EnterSourceUnit(ScopeBuilder locals, System.Linq.Expressions.Expression selfParameter, ParameterExpression runtimeScopeVariable, System.Linq.Expressions.Expression blockParameter, string methodName, Parameters parameters)
		{
			EnterMethodDefinition(locals, selfParameter, runtimeScopeVariable, blockParameter, methodName, parameters);
			_topLevelScope = _currentMethod;
		}

		public void LeaveSourceUnit()
		{
			_currentElement = null;
			_currentMethod = null;
			_currentVariableScope = null;
			_topLevelScope = null;
		}

		private VariableScope GetCurrentLambdaScope()
		{
			LexicalScope lexicalScope = _currentVariableScope;
			while (!lexicalScope.IsLambda)
			{
				lexicalScope = lexicalScope.Parent;
			}
			return (VariableScope)lexicalScope;
		}

		internal System.Linq.Expressions.Expression MakeMethodBlockParameterRead()
		{
			VariableScope currentLambdaScope = GetCurrentLambdaScope();
			if (currentLambdaScope == CurrentMethod && CurrentMethod.BlockVariable != null)
			{
				return CurrentMethod.BlockVariable;
			}
			return Methods.GetMethodBlockParameter.OpCall(CurrentScopeVariable);
		}

		internal System.Linq.Expressions.Expression MakeMethodBlockParameterSelfRead()
		{
			VariableScope currentLambdaScope = GetCurrentLambdaScope();
			if (currentLambdaScope == CurrentMethod && CurrentMethod.BlockVariable != null)
			{
				return Methods.GetProcSelf.OpCall(CurrentMethod.BlockVariable);
			}
			return Methods.GetMethodBlockParameterSelf.OpCall(CurrentScopeVariable);
		}

		internal ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> TransformExpressions(IList<Expression> arguments)
		{
			return TranformExpressions(arguments, new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>(arguments.Count));
		}

		internal ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> TranformExpressions(IList<Expression> arguments, ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> result)
		{
			foreach (Expression argument in arguments)
			{
				result.Add(argument.TransformRead(this));
			}
			return result;
		}

		internal System.Linq.Expressions.Expression TransformStatements(Statements statements, ResultOperation resultOperation)
		{
			return TransformStatements(null, statements, null, resultOperation);
		}

		internal System.Linq.Expressions.Expression TransformStatements(System.Linq.Expressions.Expression prologue, Statements statements, ResultOperation resultOperation)
		{
			return TransformStatements(prologue, statements, null, resultOperation);
		}

		internal System.Linq.Expressions.Expression TransformStatements(System.Linq.Expressions.Expression prologue, Statements statements, System.Linq.Expressions.Expression epilogue, ResultOperation resultOperation)
		{
			switch (statements.Count + ((prologue != null) ? 1 : 0) + ((epilogue != null) ? 1 : 0))
			{
			case 0:
				if (resultOperation.IsIgnore)
				{
					return Microsoft.Scripting.Ast.Utils.Empty();
				}
				if (resultOperation.Variable != null)
				{
					return System.Linq.Expressions.Expression.Assign(resultOperation.Variable, Microsoft.Scripting.Ast.Utils.Constant(null, resultOperation.Variable.Type));
				}
				return System.Linq.Expressions.Expression.Return(CurrentFrame.ReturnLabel, Microsoft.Scripting.Ast.Utils.Constant(null));
			case 1:
				if (prologue != null)
				{
					return prologue;
				}
				if (epilogue != null)
				{
					return epilogue;
				}
				if (resultOperation.IsIgnore)
				{
					return statements.First.Transform(this);
				}
				return statements.First.TransformResult(this, resultOperation);
			default:
			{
				BlockBuilder blockBuilder = new BlockBuilder();
				if (prologue != null)
				{
					blockBuilder.Add(prologue);
				}
				foreach (Expression item in statements.AllButLast)
				{
					blockBuilder.Add(item.Transform(this));
				}
				if (statements.Count > 0)
				{
					if (resultOperation.IsIgnore)
					{
						blockBuilder.Add(statements.Last.Transform(this));
					}
					else
					{
						blockBuilder.Add(statements.Last.TransformResult(this, resultOperation));
					}
				}
				if (epilogue != null)
				{
					blockBuilder.Add(epilogue);
				}
				blockBuilder.Add(Microsoft.Scripting.Ast.Utils.Empty());
				return blockBuilder;
			}
			}
		}

		internal System.Linq.Expressions.Expression TransformStatementsToBooleanExpression(Statements statements, bool positive)
		{
			return TransformStatementsToExpression(statements, true, positive);
		}

		internal System.Linq.Expressions.Expression TransformStatementsToExpression(Statements statements)
		{
			return TransformStatementsToExpression(statements, false, false);
		}

		private System.Linq.Expressions.Expression TransformStatementsToExpression(Statements statements, bool toBoolean, bool positive)
		{
			if (statements == null || statements.Count == 0)
			{
				if (!toBoolean)
				{
					return Microsoft.Scripting.Ast.Utils.Constant(null);
				}
				return Microsoft.Scripting.Ast.Utils.Constant(!positive);
			}
			System.Linq.Expressions.Expression expression = (toBoolean ? statements.Last.TransformCondition(this, positive) : statements.Last.TransformReadStep(this));
			if (statements.Count == 1)
			{
				return expression;
			}
			BlockBuilder blockBuilder = new BlockBuilder();
			foreach (Expression item in statements.AllButLast)
			{
				blockBuilder.Add(item.Transform(this));
			}
			blockBuilder.Add(expression);
			return blockBuilder;
		}

		internal ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> TransformMapletsToExpressions(IList<Maplet> maplets)
		{
			return TransformMapletsToExpressions(maplets, new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>(maplets.Count * 2));
		}

		internal ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> TransformMapletsToExpressions(IList<Maplet> maplets, ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> result)
		{
			foreach (Maplet maplet in maplets)
			{
				result.Add(maplet.Key.TransformRead(this));
				result.Add(maplet.Value.TransformRead(this));
			}
			return result;
		}

		public System.Linq.Expressions.Expression TransformToHashConstructor(IList<Maplet> maplets)
		{
			return MakeHashOpCall(TransformMapletsToExpressions(maplets));
		}

		internal System.Linq.Expressions.Expression MakeHashOpCall(IEnumerable<System.Linq.Expressions.Expression> expressions)
		{
			return Methods.MakeHash.OpCall(CurrentScopeVariable, Microsoft.Scripting.Ast.Utils.NewArrayHelper(typeof(object), expressions));
		}

		internal static bool CanAssign(Type to, Type from)
		{
			if (to.IsAssignableFrom(from))
			{
				return to.IsValueType == from.IsValueType;
			}
			return false;
		}

		internal System.Linq.Expressions.Expression AddReturnTarget(System.Linq.Expressions.Expression expression)
		{
			return CurrentFrame.AddReturnTarget(expression);
		}

		internal System.Linq.Expressions.Expression Return(System.Linq.Expressions.Expression expression)
		{
			LabelTarget returnLabel = ReturnLabel;
			if (returnLabel.Type != typeof(void) && expression.Type == typeof(void))
			{
				expression = System.Linq.Expressions.Expression.Block(expression, Microsoft.Scripting.Ast.Utils.Constant(null, typeof(object)));
			}
			else if (returnLabel.Type != expression.Type && !CanAssign(returnLabel.Type, expression.Type))
			{
				expression = System.Linq.Expressions.Expression.Convert(expression, returnLabel.Type);
			}
			return System.Linq.Expressions.Expression.Return(returnLabel, expression);
		}

		internal System.Linq.Expressions.Expression ClearDebugInfo()
		{
			return _sequencePointClearance;
		}

		internal System.Linq.Expressions.Expression AddDebugInfo(System.Linq.Expressions.Expression expression, SourceSpan location)
		{
			if (_document != null)
			{
				DebugInfoExpression arg = System.Linq.Expressions.Expression.DebugInfo(_document, location.Start.Line, location.Start.Column, location.End.Line, location.End.Column);
				return System.Linq.Expressions.Expression.Block(arg, expression);
			}
			return expression;
		}

		internal System.Linq.Expressions.Expression DebugMarker(string marker)
		{
			if (!_debugCompiler)
			{
				return null;
			}
			return Methods.X.OpCall(Microsoft.Scripting.Ast.Utils.Constant(marker));
		}

		internal System.Linq.Expressions.Expression DebugMark(System.Linq.Expressions.Expression expression, string marker)
		{
			if (!_debugCompiler)
			{
				return expression;
			}
			return System.Linq.Expressions.Expression.Block(Methods.X.OpCall(Microsoft.Scripting.Ast.Utils.Constant(marker)), expression);
		}

		internal System.Linq.Expressions.Expression TryCatchAny(System.Linq.Expressions.Expression tryBody, System.Linq.Expressions.Expression catchBody)
		{
			ParameterExpression parameterExpression = CurrentScope.DefineHiddenVariable("#value", tryBody.Type);
			return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.TryCatch(System.Linq.Expressions.Expression.Assign(parameterExpression, tryBody), System.Linq.Expressions.Expression.Catch(typeof(Exception), System.Linq.Expressions.Expression.Assign(parameterExpression, catchBody))), parameterExpression);
		}
	}
}
