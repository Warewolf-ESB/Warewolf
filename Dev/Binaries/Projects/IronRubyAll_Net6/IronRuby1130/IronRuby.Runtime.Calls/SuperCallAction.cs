using System;
using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public sealed class SuperCallAction : RubyMetaBinder
	{
		private readonly RubyCallSignature _signature;

		private readonly int _lexicalScopeId;

		public override RubyCallSignature Signature
		{
			get
			{
				return _signature;
			}
		}

		public override Type ReturnType
		{
			get
			{
				if (!_signature.ResolveOnly)
				{
					return typeof(object);
				}
				return typeof(bool);
			}
		}

		internal SuperCallAction(RubyContext context, RubyCallSignature signature, int lexicalScopeId)
			: base(context)
		{
			_signature = signature;
			_lexicalScopeId = lexicalScopeId;
		}

		public static SuperCallAction Make(RubyContext context, RubyCallSignature signature, int lexicalScopeId)
		{
			ContractUtils.RequiresNotNull(context, "context");
			return context.MetaBinderFactory.SuperCall(lexicalScopeId, signature);
		}

		public static SuperCallAction MakeShared(RubyCallSignature signature, int lexicalScopeId)
		{
			return RubyMetaBinderFactory.Shared.SuperCall(lexicalScopeId, signature);
		}

		public override string ToString()
		{
			object[] array = new object[5] { "super", null, null, null, null };
			RubyCallSignature signature = _signature;
			array[1] = signature.ToString();
			array[2] = ":";
			array[3] = _lexicalScopeId;
			array[4] = ((base.Context != null) ? (" @" + base.Context.RuntimeId) : null);
			return string.Concat(array);
		}

		protected override bool Build(MetaObjectBuilder metaBuilder, CallArguments args, bool defaultFallback)
		{
			RubyScope scope = args.Scope;
			Expression arg = Microsoft.Scripting.Ast.Utils.Convert(args.MetaScope.Expression, typeof(RubyScope));
			RubyModule declaringModule;
			string methodName;
			RubyScope targetScope;
			int superCallTarget = scope.GetSuperCallTarget(out declaringModule, out methodName, out targetScope);
			if (superCallTarget == -1)
			{
				metaBuilder.AddCondition(Methods.IsSuperOutOfMethodScope.OpCall(arg));
				metaBuilder.SetError(Methods.MakeTopLevelSuperException.OpCall());
				return true;
			}
			object selfObject = targetScope.SelfObject;
			ParameterExpression temporary = metaBuilder.GetTemporary(typeof(object), "#super-self");
			BinaryExpression binaryExpression = Expression.Assign(temporary, Methods.GetSuperCallTarget.OpCall(arg, Microsoft.Scripting.Ast.Utils.Constant(superCallTarget)));
			if (_signature.HasImplicitArguments && targetScope.Kind == ScopeKind.BlockMethod)
			{
				metaBuilder.AddCondition(Expression.NotEqual(binaryExpression, Expression.Field(null, Fields.NeedsUpdate)));
				metaBuilder.SetError(Methods.MakeImplicitSuperInBlockMethodError.OpCall());
				return true;
			}
			metaBuilder.AddInitialization(binaryExpression);
			args.SetTarget(temporary, selfObject);
			RubyMemberInfo methodMissing = null;
			RubyClass immediateClassOf = scope.RubyContext.GetImmediateClassOf(selfObject);
			RubyMemberInfo info;
			using (immediateClassOf.Context.ClassHierarchyLocker())
			{
				immediateClassOf.InitializeMethodsNoLock();
				metaBuilder.TreatRestrictionsAsConditions = true;
				metaBuilder.AddTargetTypeTest(selfObject, immediateClassOf, temporary, args.MetaContext, new string[1] { Symbols.MethodMissing });
				metaBuilder.TreatRestrictionsAsConditions = false;
				info = immediateClassOf.ResolveSuperMethodNoLock(methodName, declaringModule).InvalidateSitesOnOverride().Info;
				if (_signature.ResolveOnly)
				{
					metaBuilder.Result = Microsoft.Scripting.Ast.Utils.Constant(info != null);
					return true;
				}
				if (info == null)
				{
					methodMissing = immediateClassOf.ResolveMethodMissingForSite(methodName, RubyMethodVisibility.None);
				}
			}
			if (info != null)
			{
				info.BuildSuperCall(metaBuilder, args, methodName, declaringModule);
				return true;
			}
			return RubyCallAction.BuildMethodMissingCall(metaBuilder, args, methodName, methodMissing, RubyMethodVisibility.None, true, defaultFallback);
		}

		public override Expression CreateExpression()
		{
			return Methods.GetMethod(GetType(), "MakeShared").OpCall(_signature.CreateExpression(), Microsoft.Scripting.Ast.Utils.Constant(_lexicalScopeId));
		}
	}
}
