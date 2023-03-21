using System;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Threading
{
	public sealed class ThreadingLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyClass @class = GetClass(typeof(object));
			DefineGlobalClass("ConditionVariable", typeof(RubyConditionVariable), 8, @class, LoadConditionVariable_Instance, null, null, RubyModule.EmptyArray);
			DefineGlobalClass("Mutex", typeof(RubyMutex), 8, @class, LoadMutex_Instance, null, null, RubyModule.EmptyArray);
			RubyClass super = DefineGlobalClass("Queue", typeof(RubyQueue), 8, @class, LoadQueue_Instance, null, null, RubyModule.EmptyArray);
			ExtendClass(typeof(Thread), 0, @class, null, LoadSystem__Threading__Thread_Class, null, RubyModule.EmptyArray);
			DefineGlobalClass("SizedQueue", typeof(SizedQueue), 8, super, LoadSizedQueue_Instance, null, null, RubyModule.EmptyArray);
		}

		private static void LoadConditionVariable_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "broadcast", 17, 0u, new Func<RubyConditionVariable, RubyConditionVariable>(RubyConditionVariable.Broadcast));
			LibraryInitializer.DefineLibraryMethod(module, "signal", 17, 0u, new Func<RubyConditionVariable, RubyConditionVariable>(RubyConditionVariable.Signal));
			LibraryInitializer.DefineLibraryMethod(module, "wait", 17, 2u, new Func<RubyConditionVariable, RubyMutex, RubyConditionVariable>(RubyConditionVariable.Wait));
		}

		private static void LoadMutex_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "exclusive_unlock", 17, 0u, new Func<BlockParam, RubyMutex, bool>(RubyMutex.ExclusiveUnlock));
			LibraryInitializer.DefineLibraryMethod(module, "lock", 17, 0u, new Func<RubyMutex, RubyMutex>(RubyMutex.Lock));
			LibraryInitializer.DefineLibraryMethod(module, "locked?", 17, 0u, new Func<RubyMutex, bool>(RubyMutex.IsLocked));
			LibraryInitializer.DefineLibraryMethod(module, "synchronize", 17, 0u, new Func<BlockParam, RubyMutex, object>(RubyMutex.Synchronize));
			LibraryInitializer.DefineLibraryMethod(module, "try_lock", 17, 0u, new Func<RubyMutex, bool>(RubyMutex.TryLock));
			LibraryInitializer.DefineLibraryMethod(module, "unlock", 17, 0u, new Func<RubyMutex, RubyMutex>(RubyMutex.Unlock));
		}

		private static void LoadQueue_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<<", 17, 0u, new Func<RubyQueue, object, RubyQueue>(RubyQueue.Enqueue));
			LibraryInitializer.DefineLibraryMethod(module, "clear", 17, 0u, new Func<RubyQueue, RubyQueue>(RubyQueue.Clear));
			LibraryInitializer.DefineLibraryMethod(module, "deq", 17, 0u, new Func<RubyQueue, bool, object>(RubyQueue.Dequeue));
			LibraryInitializer.DefineLibraryMethod(module, "empty?", 17, 0u, new Func<RubyQueue, bool>(RubyQueue.IsEmpty));
			LibraryInitializer.DefineLibraryMethod(module, "enq", 17, 0u, new Func<RubyQueue, object, RubyQueue>(RubyQueue.Enqueue));
			LibraryInitializer.DefineLibraryMethod(module, "length", 17, 0u, new Func<RubyQueue, int>(RubyQueue.GetCount));
			LibraryInitializer.DefineLibraryMethod(module, "num_waiting", 17, 0u, new Func<RubyQueue, int>(RubyQueue.GetNumberOfWaitingThreads));
			LibraryInitializer.DefineLibraryMethod(module, "pop", 17, 0u, new Func<RubyQueue, bool, object>(RubyQueue.Dequeue));
			LibraryInitializer.DefineLibraryMethod(module, "push", 17, 0u, new Func<RubyQueue, object, RubyQueue>(RubyQueue.Enqueue));
			LibraryInitializer.DefineLibraryMethod(module, "shift", 17, 0u, new Func<RubyQueue, bool, object>(RubyQueue.Dequeue));
			LibraryInitializer.DefineLibraryMethod(module, "size", 17, 0u, new Func<RubyQueue, int>(RubyQueue.GetCount));
		}

		private static void LoadSizedQueue_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<<", 17, 0u, new Func<SizedQueue, object, SizedQueue>(SizedQueue.Enqueue));
			LibraryInitializer.DefineLibraryMethod(module, "deq", 17, 2147483648u, new Func<SizedQueue, object[], object>(SizedQueue.Dequeue));
			LibraryInitializer.DefineLibraryMethod(module, "enq", 17, 0u, new Func<SizedQueue, object, SizedQueue>(SizedQueue.Enqueue));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 65536u, new Func<SizedQueue, int, SizedQueue>(SizedQueue.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "max", 17, 0u, new Func<SizedQueue, int>(SizedQueue.GetLimit));
			LibraryInitializer.DefineLibraryMethod(module, "max=", 17, 65536u, new Action<SizedQueue, int>(SizedQueue.SetLimit));
			LibraryInitializer.DefineLibraryMethod(module, "pop", 17, 2147483648u, new Func<SizedQueue, object[], object>(SizedQueue.Dequeue));
			LibraryInitializer.DefineLibraryMethod(module, "push", 17, 0u, new Func<SizedQueue, object, SizedQueue>(SizedQueue.Enqueue));
			LibraryInitializer.DefineLibraryMethod(module, "shift", 17, 2147483648u, new Func<SizedQueue, object[], object>(SizedQueue.Dequeue));
		}

		private static void LoadSystem__Threading__Thread_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "exclusive", 33, 2u, new Func<RubyContext, BlockParam, object, object>(ThreadOps.Exclusive));
		}
	}
}
