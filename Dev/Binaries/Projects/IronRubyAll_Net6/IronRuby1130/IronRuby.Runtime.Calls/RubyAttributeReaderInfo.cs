using System;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyAttributeReaderInfo : RubyAttributeAccessorInfo
	{
		public RubyAttributeReaderInfo(RubyMemberFlags flags, RubyModule declaringModule, string variableName)
			: base(flags, declaringModule, variableName)
		{
		}

		internal override MemberDispatcher GetDispatcher(Type delegateType, RubyCallSignature signature, object target, int version)
		{
			if (!(target is IRubyObject))
			{
				return null;
			}
			if (signature.ArgumentCount != 0 || signature.HasRhsArgument || signature.HasBlock || !signature.HasScope)
			{
				return null;
			}
			RubyObjectAttributeReaderDispatcherWithScope rubyObjectAttributeReaderDispatcherWithScope = new RubyObjectAttributeReaderDispatcherWithScope();
			rubyObjectAttributeReaderDispatcherWithScope.Initialize(base.InstanceVariableName, version);
			return rubyObjectAttributeReaderDispatcherWithScope;
		}

		internal override void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
			if (!metaBuilder.Error)
			{
				metaBuilder.Result = Methods.GetInstanceVariable.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.MetaScope.Expression, typeof(RubyScope)), Microsoft.Scripting.Ast.Utils.Box(args.TargetExpression), Microsoft.Scripting.Ast.Utils.Constant(base.InstanceVariableName));
			}
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyAttributeReaderInfo(flags, module, base.InstanceVariableName);
		}

		public override RubyMemberInfo TrySelectOverload(Type[] parameterTypes)
		{
			if (parameterTypes.Length != 0)
			{
				return null;
			}
			return this;
		}
	}
}
