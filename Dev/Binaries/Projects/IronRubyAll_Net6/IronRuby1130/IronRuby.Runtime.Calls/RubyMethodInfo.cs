using System;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyMethodInfo : RubyMemberInfo
	{
		internal static readonly Type ParamsArrayDelegateType = typeof(Func<object, Proc, object[], object>);

		private readonly RubyMethodBody _body;

		private readonly RubyScope _declaringScope;

		public string DefinitionName
		{
			get
			{
				return _body.Name;
			}
		}

		public Parameters Parameters
		{
			get
			{
				return _body.Ast.Parameters;
			}
		}

		public SymbolDocumentInfo Document
		{
			get
			{
				return _body.Document;
			}
		}

		public SourceSpan SourceSpan
		{
			get
			{
				return _body.Ast.Location;
			}
		}

		public RubyScope DeclaringScope
		{
			get
			{
				return _declaringScope;
			}
		}

		internal RubyMethodInfo(RubyMethodBody body, RubyScope declaringScope, RubyModule declaringModule, RubyMemberFlags flags)
			: base(flags, declaringModule)
		{
			_body = body;
			_declaringScope = declaringScope;
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyMethodInfo(_body, _declaringScope, module, flags);
		}

		public override RubyMemberInfo TrySelectOverload(Type[] parameterTypes)
		{
			if (parameterTypes.Length < Parameters.Mandatory.Length || (Parameters.Unsplat == null && parameterTypes.Length > Parameters.Mandatory.Length + Parameters.Optional.Length) || !CollectionUtils.TrueForAll(parameterTypes, (Type type) => type == typeof(object)))
			{
				return null;
			}
			return this;
		}

		public override MemberInfo[] GetMembers()
		{
			return new MemberInfo[1] { GetDelegate().Method };
		}

		public override int GetArity()
		{
			if (Parameters.Unsplat != null || Parameters.Optional.Length > 0)
			{
				return -Parameters.Mandatory.Length - 1;
			}
			return Parameters.Mandatory.Length;
		}

		public override RubyArray GetRubyParameterArray()
		{
			RubyContext rubyContext = _declaringScope.RubyContext;
			RubySymbol item = rubyContext.CreateAsciiSymbol("req");
			RubySymbol item2 = rubyContext.CreateAsciiSymbol("opt");
			Parameters parameters = _body.Ast.Parameters;
			RubyArray rubyArray = new RubyArray();
			for (int i = 0; i < parameters.LeadingMandatoryCount; i++)
			{
				rubyArray.Add(new RubyArray
				{
					item,
					rubyContext.EncodeIdentifier(((LocalVariable)parameters.Mandatory[i]).Name)
				});
			}
			SimpleAssignmentExpression[] optional = parameters.Optional;
			foreach (SimpleAssignmentExpression simpleAssignmentExpression in optional)
			{
				rubyArray.Add(new RubyArray
				{
					item2,
					rubyContext.EncodeIdentifier(((LocalVariable)simpleAssignmentExpression.Left).Name)
				});
			}
			if (parameters.Unsplat != null)
			{
				rubyArray.Add(new RubyArray
				{
					rubyContext.CreateAsciiSymbol("rest"),
					rubyContext.EncodeIdentifier(((LocalVariable)parameters.Unsplat).Name)
				});
			}
			for (int k = parameters.LeadingMandatoryCount; k < parameters.Mandatory.Length; k++)
			{
				rubyArray.Add(new RubyArray
				{
					item,
					rubyContext.EncodeIdentifier(((LocalVariable)parameters.Mandatory[k]).Name)
				});
			}
			if (parameters.Block != null)
			{
				rubyArray.Add(new RubyArray
				{
					rubyContext.CreateAsciiSymbol("block"),
					rubyContext.EncodeIdentifier(parameters.Block.Name)
				});
			}
			return rubyArray;
		}

		public MethodDefinition GetSyntaxTree()
		{
			return _body.Ast;
		}

		internal Delegate GetDelegate()
		{
			return _body.GetDelegate(_declaringScope, base.DeclaringModule);
		}

		internal override MemberDispatcher GetDispatcher(Type delegateType, RubyCallSignature signature, object target, int version)
		{
			if (Parameters.Unsplat != null || Parameters.Optional.Length > 0)
			{
				return null;
			}
			if (!(target is IRubyObject))
			{
				return null;
			}
			return MethodDispatcher.CreateRubyObjectDispatcher(delegateType, GetDelegate(), Parameters.Mandatory.Length, signature.HasScope, signature.HasBlock, version);
		}

		internal override void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			ArgsBuilder argsBuilder = new ArgsBuilder(2, Parameters.Mandatory.Length, Parameters.LeadingMandatoryCount, Parameters.Optional.Length, Parameters.Unsplat != null);
			argsBuilder.SetImplicit(0, Microsoft.Scripting.Ast.Utils.Box(args.TargetExpression));
			argsBuilder.SetImplicit(1, args.Signature.HasBlock ? Microsoft.Scripting.Ast.Utils.Convert(args.GetBlockExpression(), typeof(Proc)) : AstFactory.NullOfProc);
			argsBuilder.AddCallArguments(metaBuilder, args);
			if (!metaBuilder.Error)
			{
				System.Linq.Expressions.Expression[] arguments = argsBuilder.GetArguments();
				for (int i = 2; i < arguments.Length; i++)
				{
					arguments[i] = Microsoft.Scripting.Ast.Utils.Box(arguments[i]);
				}
				Delegate @delegate = GetDelegate();
				if (@delegate.GetType() == ParamsArrayDelegateType)
				{
					metaBuilder.Result = AstFactory.CallDelegate(@delegate, new System.Linq.Expressions.Expression[3]
					{
						arguments[0],
						arguments[1],
						System.Linq.Expressions.Expression.NewArrayInit(typeof(object), ArrayUtils.ShiftLeft(arguments, 2))
					});
				}
				else
				{
					metaBuilder.Result = AstFactory.CallDelegate(@delegate, arguments);
				}
			}
		}

		public static void RuleControlFlowBuilder(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			if (!metaBuilder.Error)
			{
				ParameterExpression temporary = metaBuilder.GetTemporary(typeof(RuntimeFlowControl), "#rfc");
				ParameterExpression temporary2 = metaBuilder.GetTemporary(typeof(object), "#result");
				ParameterExpression expression;
				metaBuilder.Result = System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(temporary, Methods.CreateRfcForMethod.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.GetBlockExpression(), typeof(Proc)))), Microsoft.Scripting.Ast.Utils.Try(System.Linq.Expressions.Expression.Assign(temporary2, metaBuilder.Result)).Filter(expression = System.Linq.Expressions.Expression.Parameter(typeof(MethodUnwinder), "#unwinder"), System.Linq.Expressions.Expression.Equal(System.Linq.Expressions.Expression.Field(expression, MethodUnwinder.TargetFrameField), temporary), System.Linq.Expressions.Expression.Assign(temporary2, System.Linq.Expressions.Expression.Field(expression, StackUnwinder.ReturnValueField))).Finally(Methods.LeaveMethodFrame.OpCall(temporary)), temporary2);
			}
		}
	}
}
