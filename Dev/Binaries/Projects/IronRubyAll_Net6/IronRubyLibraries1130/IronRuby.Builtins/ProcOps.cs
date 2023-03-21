using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Proc", Extends = typeof(Proc), Inherits = typeof(object))]
	public static class ProcOps
	{
		[RubyConstructor]
		public static void Error(RubyClass self, params object[] args)
		{
			throw RubyExceptions.CreateAllocatorUndefinedError(self);
		}

		[RubyMethod("==")]
		[RubyMethod("eql?")]
		public static bool Equal(Proc self, [NotNull] Proc other)
		{
			if (self.Dispatcher == other.Dispatcher)
			{
				return self.LocalScope == other.LocalScope;
			}
			return false;
		}

		[RubyMethod("eql?")]
		[RubyMethod("==")]
		public static bool Equal(Proc self, object other)
		{
			return false;
		}

		[RubyMethod("hash")]
		public static int GetHash(Proc self)
		{
			return self.Dispatcher.GetHashCode() ^ self.LocalScope.GetHashCode();
		}

		[RubyMethod("dup")]
		[RubyMethod("clone")]
		public static Proc Clone(Proc self)
		{
			return self.Copy();
		}

		[RubyMethod("arity")]
		public static int GetArity(Proc self)
		{
			return self.Dispatcher.Arity;
		}

		[RubyMethod("lambda?")]
		public static bool IsLambda(Proc self)
		{
			return self.Kind == ProcKind.Lambda;
		}

		[RubyMethod("binding")]
		public static Binding GetLocalScope(Proc self)
		{
			return new Binding(self.LocalScope);
		}

		[RubyMethod("source_location")]
		public static RubyArray GetSourceLocation(Proc self)
		{
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(self.LocalScope.RubyContext.EncodePath(self.Dispatcher.SourcePath));
			rubyArray.Add(self.Dispatcher.SourceLine);
			return rubyArray;
		}

		[RubyMethod("to_s")]
		public static MutableString ToS(Proc self)
		{
			RubyContext rubyContext = self.LocalScope.RubyContext;
			MutableString mutableString = RubyUtils.ObjectToMutableStringPrefix(rubyContext, self);
			if (self.SourcePath != null || self.SourceLine != 0)
			{
				mutableString.Append('@');
				mutableString.Append(self.SourcePath ?? "(unknown)");
				mutableString.Append(':');
				mutableString.Append(self.SourceLine.ToString(CultureInfo.InvariantCulture));
			}
			if (self.Kind == ProcKind.Lambda)
			{
				mutableString.Append(" (lambda)");
			}
			mutableString.Append('>');
			return mutableString;
		}

		[RubyMethod("to_proc")]
		public static Proc ToProc(Proc self)
		{
			return self;
		}

		[RubyMethod("yield")]
		[RubyMethod("[]")]
		[RubyMethod("call")]
		[RubyMethod("===")]
		public static object Call(BlockParam block, Proc self)
		{
			RequireParameterCount(self, 0);
			return self.Call((block != null) ? block.Proc : null);
		}

		[RubyMethod("yield")]
		[RubyMethod("call")]
		[RubyMethod("[]")]
		[RubyMethod("===")]
		public static object Call(BlockParam block, Proc self, object arg1)
		{
			RequireParameterCount(self, 1);
			return self.Call((block != null) ? block.Proc : null, arg1);
		}

		[RubyMethod("call")]
		[RubyMethod("[]")]
		[RubyMethod("yield")]
		[RubyMethod("===")]
		public static object Call(BlockParam block, Proc self, object arg1, object arg2)
		{
			RequireParameterCount(self, 2);
			return self.Call((block != null) ? block.Proc : null, arg1, arg2);
		}

		[RubyMethod("===")]
		[RubyMethod("call")]
		[RubyMethod("[]")]
		[RubyMethod("yield")]
		public static object Call(BlockParam block, Proc self, object arg1, object arg2, object arg3)
		{
			RequireParameterCount(self, 3);
			return self.Call((block != null) ? block.Proc : null, arg1, arg2, arg3);
		}

		[RubyMethod("call")]
		[RubyMethod("yield")]
		[RubyMethod("===")]
		[RubyMethod("[]")]
		public static object Call(BlockParam block, Proc self, object arg1, object arg2, object arg3, object arg4)
		{
			RequireParameterCount(self, 4);
			return self.Call((block != null) ? block.Proc : null, arg1, arg2, arg3, arg4);
		}

		[RubyMethod("[]")]
		[RubyMethod("call")]
		[RubyMethod("yield")]
		[RubyMethod("===")]
		public static object Call(BlockParam block, Proc self, params object[] args)
		{
			RequireParameterCount(self, args.Length);
			return self.CallN((block != null) ? block.Proc : null, args);
		}

		private static void RequireParameterCount(Proc proc, int argCount)
		{
			int arity;
			if (proc.Kind == ProcKind.Lambda && argCount != (arity = proc.Dispatcher.Arity))
			{
				if (arity >= 0)
				{
					throw RubyOps.MakeWrongNumberOfArgumentsError(argCount, arity);
				}
				if (argCount < -arity - 1)
				{
					throw RubyOps.MakeWrongNumberOfArgumentsError(argCount, -arity - 1);
				}
			}
		}

		[RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
		public static Proc CreateNew(CallSiteStorage<Func<CallSite, object, object>> storage, RubyScope scope, RubyClass self)
		{
			RubyMethodScope innerMostMethodScope = scope.GetInnerMostMethodScope();
			if (innerMostMethodScope == null || innerMostMethodScope.BlockParameter == null)
			{
				throw RubyExceptions.CreateArgumentError("tried to create Proc object without a block");
			}
			Proc blockParameter = innerMostMethodScope.BlockParameter;
			if (self.GetUnderlyingSystemType() == typeof(Proc))
			{
				return blockParameter;
			}
			Proc.Subclass subclass = new Proc.Subclass(self, blockParameter);
			CallSite<Func<CallSite, object, object>> callSite = storage.GetCallSite("initialize", new RubyCallSignature(0, RubyCallFlags.HasImplicitSelf));
			callSite.Target(callSite, subclass);
			return subclass;
		}

		[RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
		public static object CreateNew(CallSiteStorage<Func<CallSite, object, object, object>> storage, BlockParam block, RubyClass self)
		{
			if (block == null)
			{
				throw RubyExceptions.CreateArgumentError("tried to create Proc object without a block");
			}
			Proc proc = block.Proc;
			if (self.GetUnderlyingSystemType() == typeof(Proc))
			{
				return proc;
			}
			Proc.Subclass subclass = new Proc.Subclass(self, proc);
			CallSite<Func<CallSite, object, object, object>> callSite = storage.GetCallSite("initialize", new RubyCallSignature(0, (RubyCallFlags)24));
			object obj = callSite.Target(callSite, subclass, block.Proc);
			if (obj is BlockReturnResult)
			{
				return obj;
			}
			return subclass;
		}
	}
}
