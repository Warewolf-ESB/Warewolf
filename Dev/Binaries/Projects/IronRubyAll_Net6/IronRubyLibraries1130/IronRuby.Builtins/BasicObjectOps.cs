using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("BasicObject", Extends = typeof(BasicObject), Restrictions = (ModuleRestrictions.NoNameMapping | ModuleRestrictions.NotPublished | ModuleRestrictions.NoUnderlyingType))]
	public static class BasicObjectOps
	{
		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
		public static object Reinitialize(object self, params object[] args)
		{
			return self;
		}

		[RubyMethod("singleton_method_added", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
		public static void MethodAdded(object self, object methodName)
		{
		}

		[RubyMethod("singleton_method_removed", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
		public static void MethodRemoved(object self, object methodName)
		{
		}

		[RubyMethod("singleton_method_undefined", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
		public static void MethodUndefined(object self, object methodName)
		{
		}

		[RubyMethod("method_missing", RubyMethodAttributes.PrivateInstance)]
		[RubyStackTraceHidden]
		public static object MethodMissing(RubyContext context, object self, [NotNull] RubySymbol name, params object[] args)
		{
			throw RubyExceptions.CreateMethodMissing(context, self, name.ToString());
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, object self)
		{
			return KernelOps.SendMessage(scope, self);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, object self, [NotNull][DefaultProtocol] string methodName)
		{
			return KernelOps.SendMessage(scope, self, methodName);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [NotNull][DefaultProtocol] string methodName)
		{
			return KernelOps.SendMessage(scope, block, self, methodName);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, object self, [NotNull][DefaultProtocol] string methodName, object arg1)
		{
			return KernelOps.SendMessage(scope, self, methodName, arg1);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [DefaultProtocol][NotNull] string methodName, object arg1)
		{
			return KernelOps.SendMessage(scope, block, self, methodName, arg1);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, object self, [NotNull][DefaultProtocol] string methodName, object arg1, object arg2)
		{
			return KernelOps.SendMessage(scope, self, methodName, arg1, arg2);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [DefaultProtocol][NotNull] string methodName, object arg1, object arg2)
		{
			return KernelOps.SendMessage(scope, block, self, methodName, arg1, arg2);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, object self, [DefaultProtocol][NotNull] string methodName, object arg1, object arg2, object arg3)
		{
			return KernelOps.SendMessage(scope, self, methodName, arg1, arg2, arg3);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [NotNull][DefaultProtocol] string methodName, object arg1, object arg2, object arg3)
		{
			return KernelOps.SendMessage(scope, block, self, methodName, arg1, arg2, arg3);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, object self, [DefaultProtocol][NotNull] string methodName, params object[] args)
		{
			return KernelOps.SendMessage(scope, self, methodName, args);
		}

		[RubyMethod("__send__")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [DefaultProtocol][NotNull] string methodName, params object[] args)
		{
			return KernelOps.SendMessage(scope, block, self, methodName, args);
		}

		[RubyMethod("==")]
		public static bool ValueEquals([NotNull] IRubyObject self, object other)
		{
			return self.BaseEquals(other);
		}

		[RubyMethod("==")]
		public static bool ValueEquals(object self, object other)
		{
			return object.Equals(self, other);
		}

		[RubyMethod("!")]
		public static bool Not(object self)
		{
			return RubyOps.IsFalse(self);
		}

		[RubyMethod("!=")]
		public static bool ValueNotEquals(BinaryOpStorage eql, object self, object other)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = eql.GetCallSite("==", 1);
			return RubyOps.IsFalse(callSite.Target(callSite, self, other));
		}

		[RubyMethod("equal?")]
		public static bool IsEqual(object self, object other)
		{
			if (self == other)
			{
				return true;
			}
			if (RubyUtils.IsRubyValueType(self) && RubyUtils.IsRubyValueType(other))
			{
				return object.Equals(self, other);
			}
			return false;
		}

		[RubyMethod("instance_eval")]
		public static object Evaluate(RubyScope scope, object self, [NotNull] MutableString code, [Optional][NotNull] MutableString file, int line)
		{
			RubyClass orCreateSingletonClass = scope.RubyContext.GetOrCreateSingletonClass(self);
			return RubyUtils.Evaluate(code, scope, self, orCreateSingletonClass, file, line);
		}

		[RubyMethod("instance_eval")]
		public static object InstanceEval([NotNull] BlockParam block, object self)
		{
			return RubyUtils.EvaluateInSingleton(self, block, null);
		}

		[RubyMethod("instance_exec")]
		public static object InstanceExec([NotNull] BlockParam block, object self, params object[] args)
		{
			return RubyUtils.EvaluateInSingleton(self, block, args);
		}
	}
}
