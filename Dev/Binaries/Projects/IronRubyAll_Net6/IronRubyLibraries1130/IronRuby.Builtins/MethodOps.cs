using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Method", Extends = typeof(RubyMethod))]
	public static class MethodOps
	{
		[RubyMethod("==")]
		public static bool Equal(RubyMethod self, [NotNull] RubyMethod other)
		{
			if (object.ReferenceEquals(self.Target, other.Target))
			{
				return object.ReferenceEquals(self.Info, other.Info);
			}
			return false;
		}

		[RubyMethod("==")]
		public static bool Equal(RubyMethod self, object other)
		{
			return false;
		}

		[RubyMethod("arity")]
		public static int GetArity(RubyMethod self)
		{
			return self.Info.GetArity();
		}

		[RubyMethod("clone")]
		public static RubyMethod Clone(RubyMethod self)
		{
			return new RubyMethod(self.Target, self.Info, self.Name);
		}

		[RubyMethod("[]")]
		[RubyMethod("call")]
		public static RuleGenerator Call()
		{
			return RuleGenerators.MethodCall;
		}

		[RubyMethod("to_s")]
		public static MutableString ToS(RubyContext context, RubyMethod self)
		{
			return UnboundMethod.ToS(context, self.Name, self.Info.DeclaringModule, self.GetTargetClass(), "Method");
		}

		[RubyMethod("to_proc")]
		public static Proc ToProc(RubyScope scope, RubyMethod self)
		{
			return self.ToProc(scope);
		}

		[RubyMethod("unbind")]
		public static UnboundMethod Unbind(RubyMethod self)
		{
			return new UnboundMethod(self.GetTargetClass(), self.Name, self.Info);
		}

		internal static RubyMemberInfo BindGenericParameters(RubyContext context, RubyMemberInfo info, string name, object[] typeArgs)
		{
			RubyMemberInfo rubyMemberInfo = info.TryBindGenericParameters(Protocols.ToTypes(context, typeArgs));
			if (rubyMemberInfo == null)
			{
				throw RubyExceptions.CreateArgumentError("wrong number of generic arguments for `{0}'", name);
			}
			return rubyMemberInfo;
		}

		internal static RubyMemberInfo SelectOverload(RubyContext context, RubyMemberInfo info, string name, object[] typeArgs)
		{
			RubyMemberInfo rubyMemberInfo = info.TrySelectOverload(Protocols.ToTypes(context, typeArgs));
			if (rubyMemberInfo == null)
			{
				throw RubyExceptions.CreateArgumentError("no overload of `{0}' matches given parameter types", name);
			}
			return rubyMemberInfo;
		}

		[RubyMethod("of")]
		public static RubyMethod BindGenericParameters(RubyContext context, RubyMethod self, [NotNullItems] params object[] typeArgs)
		{
			return new RubyMethod(self.Target, BindGenericParameters(context, self.Info, self.Name, typeArgs), self.Name);
		}

		[RubyMethod("overloads")]
		public static RubyMethod SelectOverload_old(RubyContext context, RubyMethod self, [NotNullItems] params object[] parameterTypes)
		{
			throw RubyExceptions.CreateNameError("Method#overloads is an obsolete name, use Method#overload.");
		}

		[RubyMethod("overload")]
		public static RubyMethod SelectOverload(RubyContext context, RubyMethod self, [NotNullItems] params object[] parameterTypes)
		{
			return new RubyMethod(self.Target, SelectOverload(context, self.Info, self.Name, parameterTypes), self.Name);
		}

		[RubyMethod("clr_members")]
		public static RubyArray GetClrMembers(RubyMethod self)
		{
			return new RubyArray(self.Info.GetMembers());
		}

		[RubyMethod("source_location")]
		public static RubyArray GetSourceLocation(RubyMethod self)
		{
			return UnboundMethod.GetSourceLocation(self.Info);
		}

		[RubyMethod("parameters")]
		public static RubyArray GetParameters(RubyMethod self)
		{
			return self.Info.GetRubyParameterArray();
		}
	}
}
