using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("ObjectSpace")]
	public static class ObjectSpace
	{
		private sealed class FinalizerInvoker
		{
			public const string InstanceVariableName = "<FINALIZER>";

			private CallSite<Func<CallSite, object, object, object>> _callSite;

			private object _finalizer;

			public FinalizerInvoker(CallSite<Func<CallSite, object, object, object>> callSite, object finalizer)
			{
				_callSite = callSite;
				_finalizer = finalizer;
			}

			~FinalizerInvoker()
			{
				if (_callSite != null)
				{
					try
					{
						_callSite.Target(_callSite, _finalizer, 0);
						return;
					}
					catch (Exception)
					{
						return;
					}
				}
			}
		}

		[RubyMethod("define_finalizer", RubyMethodAttributes.PublicSingleton)]
		public static object DefineFinalizer(RespondToStorage respondTo, BinaryOpStorage call, RubyModule self, object obj, object finalizer)
		{
			if (!Protocols.RespondTo(respondTo, finalizer, "call"))
			{
				throw RubyExceptions.CreateArgumentError("finalizer should be callable (respond to :call)");
			}
			respondTo.Context.SetInstanceVariable(obj, "<FINALIZER>", new FinalizerInvoker(call.GetCallSite("call"), finalizer));
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(0);
			rubyArray.Add(finalizer);
			return rubyArray;
		}

		[RubyMethod("undefine_finalizer", RubyMethodAttributes.PublicSingleton)]
		public static object UndefineFinalizer(RubyContext context, RubyModule self, object obj)
		{
			object value;
			if (context.TryRemoveInstanceVariable(obj, "<FINALIZER>", out value))
			{
				GC.SuppressFinalize(value);
			}
			return obj;
		}

		[RubyMethod("each_object", RubyMethodAttributes.PublicSingleton)]
		public static object EachObject(BlockParam block, RubyModule self, [NotNull] RubyClass theClass)
		{
			if (!theClass.HasAncestor(self.Context.ModuleClass))
			{
				throw RubyExceptions.CreateRuntimeError("each_object only supported for objects of type Class or Module");
			}
			if (block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			int num = 0;
			List<RubyModule> visitedModules = new List<RubyModule>();
			Stack<RubyModule> pendingModules = new Stack<RubyModule>();
			pendingModules.Push(theClass.Context.ObjectClass);
			while (pendingModules.Count > 0)
			{
				RubyModule rubyModule = pendingModules.Pop();
				visitedModules.Add(rubyModule);
				if (theClass.Context.IsKindOf(rubyModule, theClass))
				{
					num++;
					object blockResult;
					if (block.Yield(rubyModule, out blockResult))
					{
						return blockResult;
					}
				}
				using (theClass.Context.ClassHierarchyLocker())
				{
					rubyModule.EnumerateConstants(delegate(RubyModule module, string name, object value)
					{
						RubyModule rubyModule2 = value as RubyModule;
						if (rubyModule2 != null && !visitedModules.Contains(rubyModule2))
						{
							pendingModules.Push(rubyModule2);
						}
						return false;
					});
				}
			}
			return num;
		}

		[RubyMethod("garbage_collect", RubyMethodAttributes.PublicSingleton)]
		public static void GarbageCollect(RubyModule self)
		{
			GC.Collect();
		}
	}
}
