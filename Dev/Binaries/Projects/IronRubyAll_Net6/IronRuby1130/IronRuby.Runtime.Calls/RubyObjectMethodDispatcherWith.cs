using System;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	public class RubyObjectMethodDispatcherWithScope : MethodDispatcher<Func<object, Proc, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, object>(Invoke);
			}
			return new Func<CallSite, object, object, object>(Invoke);
		}

		public object Invoke<TScope>(CallSite callSite, TScope scope, object self)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null);
			}
			return ((CallSite<Func<CallSite, TScope, object, object>>)callSite).Update(callSite, scope, self);
		}
	}
	public class RubyObjectMethodDispatcherWithBlock : MethodDispatcher<Func<object, Proc, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, object, Proc, object>(Invoke);
			}
			return new Func<CallSite, object, object, object>(Invoke);
		}

		public object Invoke<TProc>(CallSite callSite, object self, TProc proc)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc);
			}
			return ((CallSite<Func<CallSite, object, TProc, object>>)callSite).Update(callSite, self, proc);
		}
	}
	public class RubyObjectMethodDispatcherWithScopeAndBlock : MethodDispatcher<Func<object, Proc, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, Proc, object>(Invoke);
			}
			return new Func<CallSite, object, object, object, object>(Invoke);
		}

		public object Invoke<TScope, TProc>(CallSite callSite, TScope scope, object self, TProc proc)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc);
			}
			return ((CallSite<Func<CallSite, TScope, object, TProc, object>>)callSite).Update(callSite, scope, self, proc);
		}
	}
	public class RubyObjectMethodDispatcherWithScope<T0> : MethodDispatcher<Func<object, Proc, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, T0, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, object>(Invoke);
		}

		public object Invoke<TScope>(CallSite callSite, TScope scope, object self, T0 arg0)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0);
			}
			return ((CallSite<Func<CallSite, TScope, object, T0, object>>)callSite).Update(callSite, scope, self, arg0);
		}
	}
	public class RubyObjectMethodDispatcherWithBlock<T0> : MethodDispatcher<Func<object, Proc, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, object, Proc, T0, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, object>(Invoke);
		}

		public object Invoke<TProc>(CallSite callSite, object self, TProc proc, T0 arg0)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0);
			}
			return ((CallSite<Func<CallSite, object, TProc, T0, object>>)callSite).Update(callSite, self, proc, arg0);
		}
	}
	public class RubyObjectMethodDispatcherWithScopeAndBlock<T0> : MethodDispatcher<Func<object, Proc, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, Proc, T0, object>(Invoke);
			}
			return new Func<CallSite, object, object, object, T0, object>(Invoke);
		}

		public object Invoke<TScope, TProc>(CallSite callSite, TScope scope, object self, TProc proc, T0 arg0)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0);
			}
			return ((CallSite<Func<CallSite, TScope, object, TProc, T0, object>>)callSite).Update(callSite, scope, self, proc, arg0);
		}
	}
	public class RubyObjectMethodDispatcherWithScope<T0, T1> : MethodDispatcher<Func<object, Proc, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, T0, T1, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, T1, object>(Invoke);
		}

		public object Invoke<TScope>(CallSite callSite, TScope scope, object self, T0 arg0, T1 arg1)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0, arg1);
			}
			return ((CallSite<Func<CallSite, TScope, object, T0, T1, object>>)callSite).Update(callSite, scope, self, arg0, arg1);
		}
	}
	public class RubyObjectMethodDispatcherWithBlock<T0, T1> : MethodDispatcher<Func<object, Proc, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, object, Proc, T0, T1, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, T1, object>(Invoke);
		}

		public object Invoke<TProc>(CallSite callSite, object self, TProc proc, T0 arg0, T1 arg1)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0, arg1);
			}
			return ((CallSite<Func<CallSite, object, TProc, T0, T1, object>>)callSite).Update(callSite, self, proc, arg0, arg1);
		}
	}
	public class RubyObjectMethodDispatcherWithScopeAndBlock<T0, T1> : MethodDispatcher<Func<object, Proc, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, Proc, T0, T1, object>(Invoke);
			}
			return new Func<CallSite, object, object, object, T0, T1, object>(Invoke);
		}

		public object Invoke<TScope, TProc>(CallSite callSite, TScope scope, object self, TProc proc, T0 arg0, T1 arg1)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0, arg1);
			}
			return ((CallSite<Func<CallSite, TScope, object, TProc, T0, T1, object>>)callSite).Update(callSite, scope, self, proc, arg0, arg1);
		}
	}
	public class RubyObjectMethodDispatcherWithScope<T0, T1, T2> : MethodDispatcher<Func<object, Proc, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, T0, T1, T2, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, T1, T2, object>(Invoke);
		}

		public object Invoke<TScope>(CallSite callSite, TScope scope, object self, T0 arg0, T1 arg1, T2 arg2)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0, arg1, arg2);
			}
			return ((CallSite<Func<CallSite, TScope, object, T0, T1, T2, object>>)callSite).Update(callSite, scope, self, arg0, arg1, arg2);
		}
	}
	public class RubyObjectMethodDispatcherWithBlock<T0, T1, T2> : MethodDispatcher<Func<object, Proc, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, object, Proc, T0, T1, T2, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, T1, T2, object>(Invoke);
		}

		public object Invoke<TProc>(CallSite callSite, object self, TProc proc, T0 arg0, T1 arg1, T2 arg2)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0, arg1, arg2);
			}
			return ((CallSite<Func<CallSite, object, TProc, T0, T1, T2, object>>)callSite).Update(callSite, self, proc, arg0, arg1, arg2);
		}
	}
	public class RubyObjectMethodDispatcherWithScopeAndBlock<T0, T1, T2> : MethodDispatcher<Func<object, Proc, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, Proc, T0, T1, T2, object>(Invoke);
			}
			return new Func<CallSite, object, object, object, T0, T1, T2, object>(Invoke);
		}

		public object Invoke<TScope, TProc>(CallSite callSite, TScope scope, object self, TProc proc, T0 arg0, T1 arg1, T2 arg2)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0, arg1, arg2);
			}
			return ((CallSite<Func<CallSite, TScope, object, TProc, T0, T1, T2, object>>)callSite).Update(callSite, scope, self, proc, arg0, arg1, arg2);
		}
	}
	public class RubyObjectMethodDispatcherWithScope<T0, T1, T2, T3> : MethodDispatcher<Func<object, Proc, object, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, T0, T1, T2, T3, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, T1, T2, T3, object>(Invoke);
		}

		public object Invoke<TScope>(CallSite callSite, TScope scope, object self, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0, arg1, arg2, arg3);
			}
			return ((CallSite<Func<CallSite, TScope, object, T0, T1, T2, T3, object>>)callSite).Update(callSite, scope, self, arg0, arg1, arg2, arg3);
		}
	}
	public class RubyObjectMethodDispatcherWithBlock<T0, T1, T2, T3> : MethodDispatcher<Func<object, Proc, object, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, object, Proc, T0, T1, T2, T3, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, T1, T2, T3, object>(Invoke);
		}

		public object Invoke<TProc>(CallSite callSite, object self, TProc proc, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0, arg1, arg2, arg3);
			}
			return ((CallSite<Func<CallSite, object, TProc, T0, T1, T2, T3, object>>)callSite).Update(callSite, self, proc, arg0, arg1, arg2, arg3);
		}
	}
	public class RubyObjectMethodDispatcherWithScopeAndBlock<T0, T1, T2, T3> : MethodDispatcher<Func<object, Proc, object, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, Proc, T0, T1, T2, T3, object>(Invoke);
			}
			return new Func<CallSite, object, object, object, T0, T1, T2, T3, object>(Invoke);
		}

		public object Invoke<TScope, TProc>(CallSite callSite, TScope scope, object self, TProc proc, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0, arg1, arg2, arg3);
			}
			return ((CallSite<Func<CallSite, TScope, object, TProc, T0, T1, T2, T3, object>>)callSite).Update(callSite, scope, self, proc, arg0, arg1, arg2, arg3);
		}
	}
	public class RubyObjectMethodDispatcherWithScope<T0, T1, T2, T3, T4> : MethodDispatcher<Func<object, Proc, object, object, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, T0, T1, T2, T3, T4, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, T1, T2, T3, T4, object>(Invoke);
		}

		public object Invoke<TScope>(CallSite callSite, TScope scope, object self, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, null, arg0, arg1, arg2, arg3, arg4);
			}
			return ((CallSite<Func<CallSite, TScope, object, T0, T1, T2, T3, T4, object>>)callSite).Update(callSite, scope, self, arg0, arg1, arg2, arg3, arg4);
		}
	}
	public class RubyObjectMethodDispatcherWithBlock<T0, T1, T2, T3, T4> : MethodDispatcher<Func<object, Proc, object, object, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, object, Proc, T0, T1, T2, T3, T4, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, T1, T2, T3, T4, object>(Invoke);
		}

		public object Invoke<TProc>(CallSite callSite, object self, TProc proc, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0, arg1, arg2, arg3, arg4);
			}
			return ((CallSite<Func<CallSite, object, TProc, T0, T1, T2, T3, T4, object>>)callSite).Update(callSite, self, proc, arg0, arg1, arg2, arg3, arg4);
		}
	}
	public class RubyObjectMethodDispatcherWithScopeAndBlock<T0, T1, T2, T3, T4> : MethodDispatcher<Func<object, Proc, object, object, object, object, object, object>>
	{
		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, Proc, T0, T1, T2, T3, T4, object>(Invoke);
			}
			return new Func<CallSite, object, object, object, T0, T1, T2, T3, T4, object>(Invoke);
		}

		public object Invoke<TScope, TProc>(CallSite callSite, TScope scope, object self, TProc proc, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				return Method(self, (Proc)(object)proc, arg0, arg1, arg2, arg3, arg4);
			}
			return ((CallSite<Func<CallSite, TScope, object, TProc, T0, T1, T2, T3, T4, object>>)callSite).Update(callSite, scope, self, proc, arg0, arg1, arg2, arg3, arg4);
		}
	}
}
