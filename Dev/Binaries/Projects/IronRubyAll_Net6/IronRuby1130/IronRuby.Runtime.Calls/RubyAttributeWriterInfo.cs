using System;
using System.Collections.Generic;
using System.Dynamic;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyAttributeWriterInfo : RubyAttributeAccessorInfo
	{
		public RubyAttributeWriterInfo(RubyMemberFlags flags, RubyModule declaringModule, string name)
			: base(flags, declaringModule, name)
		{
		}

		internal override MemberDispatcher GetDispatcher(Type delegateType, RubyCallSignature signature, object target, int version)
		{
			if (!(target is IRubyObject))
			{
				return null;
			}
			if (signature.ArgumentCount + (signature.HasRhsArgument ? 1 : 0) != 1 || signature.HasBlock)
			{
				return null;
			}
			return AttributeDispatcher.CreateRubyObjectWriterDispatcher(delegateType, base.InstanceVariableName, version);
		}

		internal override void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			IList<DynamicMetaObject> list = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
			if (!metaBuilder.Error)
			{
				metaBuilder.Result = Methods.SetInstanceVariable.OpCall(Microsoft.Scripting.Ast.Utils.Box(args.TargetExpression), Microsoft.Scripting.Ast.Utils.Box(list[0].Expression), Microsoft.Scripting.Ast.Utils.Convert(args.MetaScope.Expression, typeof(RubyScope)), Microsoft.Scripting.Ast.Utils.Constant(base.InstanceVariableName));
			}
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyAttributeWriterInfo(flags, module, base.InstanceVariableName);
		}

		public override RubyMemberInfo TrySelectOverload(Type[] parameterTypes)
		{
			if (parameterTypes.Length != 1 || !(parameterTypes[0] == typeof(object)))
			{
				return null;
			}
			return this;
		}
	}
}
