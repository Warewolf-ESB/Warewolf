using System;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	public class RubyObjectMethodDispatcher : MethodDispatcher<Func<object, Proc, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			return new Func<CallSite, object, object>(Invoke);
		}

		public object Invoke(CallSite callSite, object self)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null);
			}
			return ((CallSite<Func<CallSite, object, object>>)callSite).Update(callSite, self);
		}
	}
	public class RubyObjectMethodDispatcher<T0> : MethodDispatcher<Func<object, Proc, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			return new Func<CallSite, object, T0, object>(Invoke);
		}

		public object Invoke(CallSite callSite, object self, T0 arg0)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0);
			}
			return ((CallSite<Func<CallSite, object, T0, object>>)callSite).Update(callSite, self, arg0);
		}
	}
	public class RubyObjectMethodDispatcher<T0, T1> : MethodDispatcher<Func<object, Proc, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			return new Func<CallSite, object, T0, T1, object>(Invoke);
		}

		public object Invoke(CallSite callSite, object self, T0 arg0, T1 arg1)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0, arg1);
			}
			return ((CallSite<Func<CallSite, object, T0, T1, object>>)callSite).Update(callSite, self, arg0, arg1);
		}
	}
	public class RubyObjectMethodDispatcher<T0, T1, T2> : MethodDispatcher<Func<object, Proc, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			return new Func<CallSite, object, T0, T1, T2, object>(Invoke);
		}

		public object Invoke(CallSite callSite, object self, T0 arg0, T1 arg1, T2 arg2)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0, arg1, arg2);
			}
			return ((CallSite<Func<CallSite, object, T0, T1, T2, object>>)callSite).Update(callSite, self, arg0, arg1, arg2);
		}
	}
	public class RubyObjectMethodDispatcher<T0, T1, T2, T3> : MethodDispatcher<Func<object, Proc, object, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			return new Func<CallSite, object, T0, T1, T2, T3, object>(Invoke);
		}

		public object Invoke(CallSite callSite, object self, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0, arg1, arg2, arg3);
			}
			return ((CallSite<Func<CallSite, object, T0, T1, T2, T3, object>>)callSite).Update(callSite, self, arg0, arg1, arg2, arg3);
		}
	}
	public class RubyObjectMethodDispatcher<T0, T1, T2, T3, T4> : MethodDispatcher<Func<object, Proc, object, object, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			return new Func<CallSite, object, T0, T1, T2, T3, T4, object>(Invoke);
		}

		public object Invoke(CallSite callSite, object self, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0, arg1, arg2, arg3, arg4);
			}
			return ((CallSite<Func<CallSite, object, T0, T1, T2, T3, T4, object>>)callSite).Update(callSite, self, arg0, arg1, arg2, arg3, arg4);
		}
	}
}
