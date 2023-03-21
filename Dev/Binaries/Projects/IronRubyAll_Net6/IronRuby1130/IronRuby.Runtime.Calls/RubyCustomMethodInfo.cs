using System;
using System.Reflection;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	internal sealed class RubyCustomMethodInfo : RubyMemberInfo
	{
		private readonly RuleGenerator _ruleGenerator;

		public RubyCustomMethodInfo(RuleGenerator ruleGenerator, RubyMemberFlags flags, RubyModule declaringModule)
			: base(flags, declaringModule)
		{
			_ruleGenerator = ruleGenerator;
		}

		internal override void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			_ruleGenerator(metaBuilder, args, name);
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyCustomMethodInfo(_ruleGenerator, flags, module);
		}

		public override MemberInfo[] GetMembers()
		{
			return new MemberInfo[1] { _ruleGenerator.Method };
		}

		public override RubyMemberInfo TrySelectOverload(Type[] parameterTypes)
		{
			return this;
		}
	}
}
