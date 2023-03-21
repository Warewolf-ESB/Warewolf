using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyModule("Kernel", Extends = typeof(Kernel))]
	public static class KernelOps
	{
		private sealed class ThrowCatchUnwinder : StackUnwinder
		{
			public readonly object Label;

			internal ThrowCatchUnwinder(object label, object returnValue)
				: base(returnValue)
			{
				Label = label;
			}
		}

		[ThreadStatic]
		private static Stack<object> _catchSymbols;

		private static RNGCryptoServiceProvider _RNGCryptoServiceProvider;

		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static object InitializeCopy(RubyContext context, object self, object source)
		{
			RubyClass classOf = context.GetClassOf(self);
			RubyClass classOf2 = context.GetClassOf(source);
			if (classOf2 != classOf)
			{
				throw RubyExceptions.CreateTypeError("initialize_copy should take same class object");
			}
			if (context.IsObjectFrozen(self))
			{
				throw RubyExceptions.CreateTypeError("can't modify frozen {0}", classOf.Name);
			}
			return self;
		}

		[RubyMethod("Array", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("Array", RubyMethodAttributes.PrivateInstance)]
		public static IList ToArray(ConversionStorage<IList> tryToAry, ConversionStorage<IList> tryToA, object self, object obj)
		{
			IList list = Protocols.TryCastToArray(tryToAry, obj);
			if (list != null)
			{
				return list;
			}
			list = Protocols.TryConvertToArray(tryToA, obj);
			if (list != null)
			{
				return list;
			}
			list = new RubyArray();
			if (obj != null)
			{
				list.Add(obj);
			}
			return list;
		}

		[RubyMethod("Float", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("Float", RubyMethodAttributes.PublicSingleton)]
		public static double ToFloat(object self, [DefaultProtocol] double value)
		{
			return value;
		}

		[RubyMethod("Integer", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("Integer", RubyMethodAttributes.PublicSingleton)]
		public static object ToInteger(object self, [NotNull] MutableString value)
		{
			string text = value.ConvertToString();
			int i = 0;
			object result = Tokenizer.ParseInteger(text, 0, ref i).ToObject();
			for (; i < text.Length && Tokenizer.IsWhiteSpace(text[i]); i++)
			{
			}
			if (i < text.Length)
			{
				throw RubyExceptions.CreateArgumentError("invalid value for Integer: \"{0}\"", text);
			}
			return result;
		}

		[RubyMethod("Integer", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("Integer", RubyMethodAttributes.PublicSingleton)]
		public static object ToInteger(ConversionStorage<IntegerValue> integerConversion, object self, object value)
		{
			IntegerValue integerValue = Protocols.ConvertToInteger(integerConversion, value);
			if (!integerValue.IsFixnum)
			{
				return integerValue.Bignum;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(integerValue.Fixnum);
		}

		[RubyMethod("String", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("String", RubyMethodAttributes.PrivateInstance)]
		public static object ToString(ConversionStorage<MutableString> tosConversion, object self, object obj)
		{
			return Protocols.ConvertToString(tosConversion, obj);
		}

		[RubyMethod("Complex", RubyMethodAttributes.PrivateInstance, Compatibility = RubyCompatibility.Default)]
		[RubyMethod("Complex", RubyMethodAttributes.PublicSingleton, Compatibility = RubyCompatibility.Default)]
		public static object ToComplex(CallSiteStorage<Func<CallSite, object, object, object, object>> toComplex, RubyScope scope, object self, object real, object imaginary)
		{
			scope.RubyContext.Loader.LoadFile(scope.GlobalScope.Scope, self, MutableString.CreateAscii("complex18.rb"), LoadFlags.Require);
			CallSite<Func<CallSite, object, object, object, object>> callSite = toComplex.GetCallSite("Complex", 2);
			return callSite.Target(callSite, self, real, imaginary);
		}

		[RubyMethod("Rational", RubyMethodAttributes.PrivateInstance, Compatibility = RubyCompatibility.Default)]
		[RubyMethod("Rational", RubyMethodAttributes.PublicSingleton, Compatibility = RubyCompatibility.Default)]
		public static object ToRational(CallSiteStorage<Func<CallSite, object, object, object, object>> toRational, RubyScope scope, object self, object numerator, object denominator)
		{
			scope.RubyContext.Loader.LoadFile(scope.GlobalScope.Scope, self, MutableString.CreateAscii("rational18.rb"), LoadFlags.Require);
			CallSite<Func<CallSite, object, object, object, object>> callSite = toRational.GetCallSite("Rational", 2);
			return callSite.Target(callSite, self, numerator, denominator);
		}

		[RubyMethod("binding", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("binding", RubyMethodAttributes.PublicSingleton)]
		public static Binding GetLocalScope(RubyScope scope, object self)
		{
			if (scope.RubyContext.RubyOptions.Compatibility < RubyCompatibility.Default)
			{
				return new Binding(scope, self);
			}
			return new Binding(scope);
		}

		[RubyMethod("iterator?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("block_given?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("block_given?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("iterator?", RubyMethodAttributes.PrivateInstance)]
		public static bool HasBlock(RubyScope scope, object self)
		{
			RubyMethodScope innerMostMethodScope = scope.GetInnerMostMethodScope();
			if (innerMostMethodScope != null)
			{
				return innerMostMethodScope.BlockParameter != null;
			}
			return false;
		}

		[RubyMethod("local_variables", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("local_variables", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetLocalVariableNames(RubyScope scope, object self)
		{
			List<string> visibleLocalNames = scope.GetVisibleLocalNames();
			return new RubyArray(visibleLocalNames.Count).AddRange(scope.RubyContext.StringifyIdentifiers(visibleLocalNames));
		}

		[RubyMethod("caller", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("caller", RubyMethodAttributes.PrivateInstance)]
		[RubyStackTraceHidden]
		public static RubyArray GetStackTrace(RubyContext context, object self, int skipFrames)
		{
			if (skipFrames < 0)
			{
				return new RubyArray();
			}
			return RubyExceptionData.CreateBacktrace(context, skipFrames);
		}

		[RubyMethod("catch", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("catch", RubyMethodAttributes.PrivateInstance)]
		public static object Catch(BlockParam block, object self, object label)
		{
			if (block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			try
			{
				if (_catchSymbols == null)
				{
					_catchSymbols = new Stack<object>();
				}
				_catchSymbols.Push(label);
				try
				{
					object blockResult;
					block.Yield(label, out blockResult);
					return blockResult;
				}
				catch (ThrowCatchUnwinder throwCatchUnwinder)
				{
					if (object.ReferenceEquals(throwCatchUnwinder.Label, label))
					{
						return throwCatchUnwinder.ReturnValue;
					}
					throw;
				}
			}
			finally
			{
				_catchSymbols.Pop();
			}
		}

		[RubyMethod("throw", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("throw", RubyMethodAttributes.PublicSingleton)]
		public static void Throw(RubyContext context, object self, object label, object returnValue)
		{
			if (_catchSymbols == null || !_catchSymbols.Contains(label, ReferenceEqualityComparer<object>.Instance))
			{
				throw RubyExceptions.CreateNameError("uncaught throw `{0}'", context.Inspect(label).ToAsciiString());
			}
			throw new ThrowCatchUnwinder(label, returnValue);
		}

		[RubyMethod("loop", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("loop", RubyMethodAttributes.PrivateInstance)]
		public static object Loop(BlockParam block, object self)
		{
			if (block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			object blockResult;
			while (!block.Yield(out blockResult))
			{
			}
			return blockResult;
		}

		[RubyMethod("lambda", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("lambda", RubyMethodAttributes.PublicSingleton)]
		public static Proc CreateLambda(BlockParam block, object self)
		{
			if (block == null)
			{
				throw RubyExceptions.CreateArgumentError("tried to create Proc object without a block");
			}
			return block.Proc.ToLambda(null);
		}

		[RubyMethod("proc", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("proc", RubyMethodAttributes.PrivateInstance)]
		public static Proc CreateProc(BlockParam block, object self)
		{
			if (block == null)
			{
				throw RubyExceptions.CreateArgumentError("tried to create Proc object without a block");
			}
			return block.Proc;
		}

		[RubyMethod("raise", RubyMethodAttributes.PublicSingleton)]
		[RubyStackTraceHidden]
		[RubyMethod("raise", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("fail", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("fail", RubyMethodAttributes.PublicSingleton)]
		public static void RaiseException(RubyContext context, object self)
		{
			Exception ex = context.CurrentException;
			if (ex == null)
			{
				ex = new RuntimeError();
			}
			throw ex;
		}

		[RubyMethod("raise", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("fail", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("fail", RubyMethodAttributes.PublicSingleton)]
		[RubyStackTraceHidden]
		[RubyMethod("raise", RubyMethodAttributes.PublicSingleton)]
		public static void RaiseException(object self, [NotNull] MutableString message)
		{
			Exception ex = RubyExceptionData.InitializeException(new RuntimeError(message.ToString()), message);
			throw ex;
		}

		[RubyMethod("raise", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("raise", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("fail", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("fail", RubyMethodAttributes.PublicSingleton)]
		[RubyStackTraceHidden]
		public static void RaiseException(RespondToStorage respondToStorage, UnaryOpStorage storage0, BinaryOpStorage storage1, CallSiteStorage<Action<CallSite, Exception, RubyArray>> setBackTraceStorage, object self, object obj, [Optional] object arg, [Optional] RubyArray backtrace)
		{
			Exception ex = CreateExceptionToRaise(respondToStorage, storage0, storage1, setBackTraceStorage, obj, arg, backtrace);
			throw ex;
		}

		internal static Exception CreateExceptionToRaise(RespondToStorage respondToStorage, UnaryOpStorage storage0, BinaryOpStorage storage1, CallSiteStorage<Action<CallSite, Exception, RubyArray>> setBackTraceStorage, object obj, object arg, RubyArray backtrace)
		{
			if (Protocols.RespondTo(respondToStorage, obj, "exception"))
			{
				Exception ex = null;
				if (arg != Missing.Value)
				{
					CallSite<Func<CallSite, object, object, object>> callSite = storage1.GetCallSite("exception");
					ex = callSite.Target(callSite, obj, arg) as Exception;
				}
				else
				{
					CallSite<Func<CallSite, object, object>> callSite2 = storage0.GetCallSite("exception");
					ex = callSite2.Target(callSite2, obj) as Exception;
				}
				if (ex != null)
				{
					if (backtrace != null)
					{
						CallSite<Action<CallSite, Exception, RubyArray>> callSite3 = setBackTraceStorage.GetCallSite("set_backtrace", 1);
						callSite3.Target(callSite3, ex, backtrace);
					}
					return ex;
				}
			}
			throw RubyExceptions.CreateTypeError("exception class/object expected");
		}

		[RubyMethod("=~")]
		public static object Match(object self, object other)
		{
			return null;
		}

		[RubyMethod("!~")]
		public static bool NotMatch(BinaryOpStorage match, object self, object other)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = match.GetCallSite("=~", 1);
			return RubyOps.IsFalse(callSite.Target(callSite, self, other));
		}

		[RubyMethod("===")]
		public static bool CaseEquals(BinaryOpStorage equals, object self, object other)
		{
			return Protocols.IsEqual(equals, self, other);
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage equals, object self, object other)
		{
			if (!Protocols.IsEqual(equals, self, other))
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(0);
		}

		[RubyMethod("eql?")]
		public static bool ValueEquals([NotNull] IRubyObject self, object other)
		{
			return self.BaseEquals(other);
		}

		[RubyMethod("eql?")]
		public static bool ValueEquals(object self, object other)
		{
			return object.Equals(self, other);
		}

		[RubyMethod("hash")]
		public static int Hash([NotNull] IRubyObject self)
		{
			return self.BaseGetHashCode();
		}

		[RubyMethod("hash")]
		public static int Hash(object self)
		{
			if (self != null)
			{
				return self.GetHashCode();
			}
			return RubyUtils.NilObjectId;
		}

		[RubyMethod("to_s")]
		public static MutableString ToS([NotNull] IRubyObject self)
		{
			return RubyUtils.ObjectBaseToMutableString(self);
		}

		[RubyMethod("to_s")]
		public static MutableString ToS(object self)
		{
			if (self != null)
			{
				return MutableString.Create(self.ToString(), RubyEncoding.UTF8);
			}
			return MutableString.CreateEmpty();
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(UnaryOpStorage inspectStorage, ConversionStorage<MutableString> tosConversion, object self)
		{
			RubyContext context = tosConversion.Context;
			RubyClass classOf;
			if (context.HasInstanceVariables(self) && ((classOf = context.GetClassOf(self)).IsRubyClass || classOf.IsObjectClass))
			{
				return RubyUtils.InspectObject(inspectStorage, tosConversion, self);
			}
			CallSite<Func<CallSite, object, MutableString>> site = tosConversion.GetSite(ConvertToSAction.Make(context));
			return site.Target(site, self);
		}

		[RubyMethod("nil?")]
		public static bool IsNil(object self)
		{
			return self == null;
		}

		[RubyMethod("id")]
		public static object GetId(RubyContext context, object self)
		{
			context.ReportWarning("Object#id will be deprecated; use Object#object_id");
			return GetObjectId(context, self);
		}

		[RubyMethod("object_id")]
		[RubyMethod("__id__")]
		public static object GetObjectId(RubyContext context, object self)
		{
			return ClrInteger.Narrow(RubyUtils.GetObjectId(context, self));
		}

		[RubyMethod("clone")]
		public static object Clone(CallSiteStorage<Func<CallSite, object, object, object>> initializeCopyStorage, CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, object self)
		{
			return Clone(initializeCopyStorage, allocateStorage, true, self);
		}

		[RubyMethod("dup")]
		public static object Duplicate(CallSiteStorage<Func<CallSite, object, object, object>> initializeCopyStorage, CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, object self)
		{
			return Clone(initializeCopyStorage, allocateStorage, false, self);
		}

		private static object Clone(CallSiteStorage<Func<CallSite, object, object, object>> initializeCopyStorage, CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, bool isClone, object self)
		{
			RubyContext context = allocateStorage.Context;
			object copy;
			if (!RubyUtils.TryDuplicateObject(initializeCopyStorage, allocateStorage, self, isClone, out copy))
			{
				throw RubyExceptions.CreateTypeError("can't {0} {1}", isClone ? "clone" : "dup", context.GetClassDisplayName(self));
			}
			return context.TaintObjectBy(copy, self);
		}

		[RubyMethod("class")]
		public static RubyClass GetClass(RubyContext context, object self)
		{
			return context.GetClassOf(self);
		}

		[RubyMethod("type")]
		public static RubyClass GetClassObsolete(RubyContext context, object self)
		{
			context.ReportWarning("Object#type will be deprecated; use Object#class");
			return context.GetClassOf(self);
		}

		[RubyMethod("kind_of?")]
		[RubyMethod("is_a?")]
		public static bool IsKindOf(object self, RubyModule other)
		{
			ContractUtils.RequiresNotNull(other, "other");
			return other.Context.IsKindOf(self, other);
		}

		[RubyMethod("instance_of?")]
		public static bool IsOfClass(object self, RubyModule other)
		{
			ContractUtils.RequiresNotNull(other, "other");
			return other.Context.GetClassOf(self) == other;
		}

		[RubyMethod("extend")]
		public static object Extend(CallSiteStorage<Func<CallSite, RubyModule, object, object>> extendObjectStorage, CallSiteStorage<Func<CallSite, RubyModule, object, object>> extendedStorage, object self, [NotNull] RubyModule module, [NotNullItems] params RubyModule[] modules)
		{
			RubyUtils.RequireMixins(module.GetOrCreateSingletonClass(), modules);
			CallSite<Func<CallSite, RubyModule, object, object>> callSite = extendObjectStorage.GetCallSite("extend_object", 1);
			CallSite<Func<CallSite, RubyModule, object, object>> callSite2 = extendedStorage.GetCallSite("extended", 1);
			for (int num = modules.Length - 1; num >= 0; num--)
			{
				callSite.Target(callSite, modules[num], self);
				callSite2.Target(callSite2, modules[num], self);
			}
			callSite.Target(callSite, module, self);
			callSite2.Target(callSite2, module, self);
			return self;
		}

		[RubyMethod("frozen?")]
		public static bool Frozen([NotNull] MutableString self)
		{
			return self.IsFrozen;
		}

		[RubyMethod("frozen?")]
		public static bool Frozen(RubyContext context, object self)
		{
			if (!RubyUtils.HasObjectState(self))
			{
				return false;
			}
			return context.IsObjectFrozen(self);
		}

		[RubyMethod("freeze")]
		public static object Freeze(RubyContext context, object self)
		{
			if (!RubyUtils.HasObjectState(self))
			{
				return self;
			}
			context.FreezeObject(self);
			return self;
		}

		[RubyMethod("tainted?")]
		public static bool Tainted(RubyContext context, object self)
		{
			if (!RubyUtils.HasObjectState(self))
			{
				return false;
			}
			return context.IsObjectTainted(self);
		}

		[RubyMethod("taint")]
		public static object Taint(RubyContext context, object self)
		{
			if (!RubyUtils.HasObjectState(self))
			{
				return self;
			}
			context.SetObjectTaint(self, true);
			return self;
		}

		[RubyMethod("untaint")]
		public static object Untaint(RubyContext context, object self)
		{
			if (!RubyUtils.HasObjectState(self))
			{
				return self;
			}
			context.SetObjectTaint(self, false);
			return self;
		}

		[RubyMethod("untrusted?")]
		public static bool Untrusted(RubyContext context, object self)
		{
			if (!RubyUtils.HasObjectState(self))
			{
				return false;
			}
			return context.IsObjectUntrusted(self);
		}

		[RubyMethod("trust")]
		public static object Trust(RubyContext context, object self)
		{
			if (!RubyUtils.HasObjectState(self))
			{
				return self;
			}
			context.SetObjectTrustiness(self, false);
			return self;
		}

		[RubyMethod("untrust")]
		public static object Untrust(RubyContext context, object self)
		{
			if (!RubyUtils.HasObjectState(self))
			{
				return self;
			}
			context.SetObjectTrustiness(self, true);
			return self;
		}

		[RubyMethod("eval", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("eval", RubyMethodAttributes.PublicSingleton)]
		public static object Evaluate(RubyScope scope, object self, [NotNull] MutableString code, [Optional] Binding binding, [Optional][NotNull] MutableString file, int line)
		{
			RubyScope targetScope;
			object self2;
			if (binding != null)
			{
				targetScope = binding.LocalScope;
				self2 = binding.SelfObject;
			}
			else
			{
				targetScope = scope;
				self2 = self;
			}
			return RubyUtils.Evaluate(code, targetScope, self2, null, file, line);
		}

		[RubyMethod("eval", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("eval", RubyMethodAttributes.PublicSingleton)]
		public static object Evaluate(RubyScope scope, object self, [NotNull] MutableString code, [NotNull] Proc procBinding, [Optional][NotNull] MutableString file, int line)
		{
			return RubyUtils.Evaluate(code, procBinding.LocalScope, procBinding.LocalScope.SelfObject, null, file, line);
		}

		[RubyMethod("instance_variables")]
		public static RubyArray GetInstanceVariableNames(RubyContext context, object self)
		{
			return context.StringifyIdentifiers(context.GetInstanceVariableNames(self));
		}

		[RubyMethod("instance_variable_get")]
		public static object InstanceVariableGet(RubyContext context, object self, [NotNull][DefaultProtocol] string name)
		{
			object value;
			if (!context.TryGetInstanceVariable(self, name, out value))
			{
				RubyUtils.CheckInstanceVariableName(name);
				return null;
			}
			return value;
		}

		[RubyMethod("instance_variable_set")]
		public static object InstanceVariableSet(RubyContext context, object self, [NotNull][DefaultProtocol] string name, object value)
		{
			RubyUtils.CheckInstanceVariableName(name);
			context.SetInstanceVariable(self, name, value);
			return value;
		}

		[RubyMethod("instance_variable_defined?")]
		public static bool InstanceVariableDefined(RubyContext context, object self, [DefaultProtocol][NotNull] string name)
		{
			object value;
			if (!context.TryGetInstanceVariable(self, name, out value))
			{
				RubyUtils.CheckInstanceVariableName(name);
				return false;
			}
			return true;
		}

		[RubyMethod("remove_instance_variable", RubyMethodAttributes.PrivateInstance)]
		public static object RemoveInstanceVariable(RubyContext context, object self, [DefaultProtocol][NotNull] string name)
		{
			object value;
			if (!context.TryRemoveInstanceVariable(self, name, out value))
			{
				RubyUtils.CheckInstanceVariableName(name);
				throw RubyExceptions.CreateNameError("instance variable `{0}' not defined", name);
			}
			return value;
		}

		[RubyMethod("global_variables", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("global_variables", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetGlobalVariableNames(RubyContext context, object self)
		{
			RubyArray rubyArray = new RubyArray();
			lock (context.GlobalVariablesLock)
			{
				foreach (KeyValuePair<string, GlobalVariable> globalVariable in context.GlobalVariables)
				{
					if (globalVariable.Value.IsEnumerated)
					{
						rubyArray.Add(context.StringifyIdentifier(globalVariable.Key));
					}
				}
				return rubyArray;
			}
		}

		[RubyMethod("autoload", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("autoload", RubyMethodAttributes.PublicSingleton)]
		public static void SetAutoloadedConstant(RubyScope scope, object self, [DefaultProtocol][NotNull] string constantName, [DefaultProtocol][NotNull] MutableString path)
		{
			ModuleOps.SetAutoloadedConstant(scope.GetInnerMostModuleForConstantLookup(), constantName, path);
		}

		[RubyMethod("autoload?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("autoload?", RubyMethodAttributes.PrivateInstance)]
		public static MutableString GetAutoloadedConstantPath(RubyScope scope, object self, [NotNull][DefaultProtocol] string constantName)
		{
			return ModuleOps.GetAutoloadedConstantPath(scope.GetInnerMostModuleForConstantLookup(), constantName);
		}

		[RubyMethod("respond_to?")]
		public static bool RespondTo(RubyContext context, object self, [NotNull][DefaultProtocol] string methodName, [Optional] bool includePrivate)
		{
			return context.ResolveMethod(self, methodName, includePrivate).Found;
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, object self)
		{
			throw RubyExceptions.CreateArgumentError("no method name given");
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, object self, [DefaultProtocol][NotNull] string methodName)
		{
			CallSite<Func<CallSite, RubyScope, object, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, object>>(methodName, new RubyCallSignature(0, (RubyCallFlags)17));
			return orCreateSendSite.Target(orCreateSendSite, scope, self);
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [NotNull][DefaultProtocol] string methodName)
		{
			CallSite<Func<CallSite, RubyScope, object, Proc, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, Proc, object>>(methodName, new RubyCallSignature(0, (RubyCallFlags)25));
			return orCreateSendSite.Target(orCreateSendSite, scope, self, (block != null) ? block.Proc : null);
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, object self, [NotNull][DefaultProtocol] string methodName, object arg1)
		{
			CallSite<Func<CallSite, RubyScope, object, object, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, object, object>>(methodName, new RubyCallSignature(1, (RubyCallFlags)17));
			return orCreateSendSite.Target(orCreateSendSite, scope, self, arg1);
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [NotNull][DefaultProtocol] string methodName, object arg1)
		{
			CallSite<Func<CallSite, RubyScope, object, Proc, object, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, Proc, object, object>>(methodName, new RubyCallSignature(1, (RubyCallFlags)25));
			return orCreateSendSite.Target(orCreateSendSite, scope, self, (block != null) ? block.Proc : null, arg1);
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, object self, [NotNull][DefaultProtocol] string methodName, object arg1, object arg2)
		{
			CallSite<Func<CallSite, RubyScope, object, object, object, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, object, object, object>>(methodName, new RubyCallSignature(2, (RubyCallFlags)17));
			return orCreateSendSite.Target(orCreateSendSite, scope, self, arg1, arg2);
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [NotNull][DefaultProtocol] string methodName, object arg1, object arg2)
		{
			CallSite<Func<CallSite, RubyScope, object, Proc, object, object, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, Proc, object, object, object>>(methodName, new RubyCallSignature(2, (RubyCallFlags)25));
			return orCreateSendSite.Target(orCreateSendSite, scope, self, (block != null) ? block.Proc : null, arg1, arg2);
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, object self, [DefaultProtocol][NotNull] string methodName, object arg1, object arg2, object arg3)
		{
			CallSite<Func<CallSite, RubyScope, object, object, object, object, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, object, object, object, object>>(methodName, new RubyCallSignature(3, (RubyCallFlags)17));
			return orCreateSendSite.Target(orCreateSendSite, scope, self, arg1, arg2, arg3);
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [DefaultProtocol][NotNull] string methodName, object arg1, object arg2, object arg3)
		{
			CallSite<Func<CallSite, RubyScope, object, Proc, object, object, object, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, Proc, object, object, object, object>>(methodName, new RubyCallSignature(3, (RubyCallFlags)25));
			return orCreateSendSite.Target(orCreateSendSite, scope, self, (block != null) ? block.Proc : null, arg1, arg2, arg3);
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, object self, [DefaultProtocol][NotNull] string methodName, params object[] args)
		{
			CallSite<Func<CallSite, RubyScope, object, RubyArray, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, RubyArray, object>>(methodName, new RubyCallSignature(1, (RubyCallFlags)19));
			return orCreateSendSite.Target(orCreateSendSite, scope, self, RubyOps.MakeArrayN(args));
		}

		[RubyMethod("send")]
		public static object SendMessage(RubyScope scope, BlockParam block, object self, [DefaultProtocol][NotNull] string methodName, params object[] args)
		{
			CallSite<Func<CallSite, RubyScope, object, Proc, RubyArray, object>> orCreateSendSite = scope.RubyContext.GetOrCreateSendSite<Func<CallSite, RubyScope, object, Proc, RubyArray, object>>(methodName, new RubyCallSignature(1, (RubyCallFlags)27));
			return orCreateSendSite.Target(orCreateSendSite, scope, self, (block != null) ? block.Proc : null, RubyOps.MakeArrayN(args));
		}

		internal static object SendMessageOpt(RubyScope scope, BlockParam block, object self, string methodName, object[] args)
		{
			switch ((args != null) ? args.Length : 0)
			{
			case 0:
				return SendMessage(scope, block, self, methodName);
			case 1:
				return SendMessage(scope, block, self, methodName, args[0]);
			case 2:
				return SendMessage(scope, block, self, methodName, args[0], args[1]);
			case 3:
				return SendMessage(scope, block, self, methodName, args[0], args[1], args[2]);
			default:
				return SendMessage(scope, block, self, methodName, args);
			}
		}

		[RubyMethod("tap")]
		public static object Tap(RubyScope scope, [NotNull] BlockParam block, object self)
		{
			object blockResult;
			if (block.Yield(self, out blockResult))
			{
				return blockResult;
			}
			return self;
		}

		[RubyMethod("clr_member")]
		public static RubyMethod GetClrMember(RubyContext context, object self, [NotNull] object asType, [DefaultProtocol][NotNull] string name)
		{
			RubyClass classOf = context.GetClassOf(self);
			Type asType2 = ((asType != null) ? Protocols.ToType(context, asType) : null);
			RubyMemberInfo method;
			if (!classOf.TryGetClrMember(name, asType2, out method))
			{
				throw RubyExceptions.CreateNameError("undefined CLR method `{0}' for class `{1}'", name, classOf.Name);
			}
			return new RubyMethod(self, method, name);
		}

		[RubyMethod("method")]
		public static RubyMethod GetMethod(RubyContext context, object self, [NotNull][DefaultProtocol] string name)
		{
			RubyMemberInfo info = context.ResolveMethod(self, name, VisibilityContext.AllVisible).Info;
			if (info == null)
			{
				throw RubyExceptions.CreateUndefinedMethodError(context.GetClassOf(self), name);
			}
			return new RubyMethod(self, info, name);
		}

		[RubyMethod("define_singleton_method", RubyMethodAttributes.PublicInstance)]
		public static RubyMethod DefineSingletonMethod(RubyScope scope, object self, [NotNull][DefaultProtocol] string methodName, [NotNull] RubyMethod method)
		{
			return ModuleOps.DefineMethod(scope, scope.RubyContext.GetOrCreateSingletonClass(self), methodName, method);
		}

		[RubyMethod("define_singleton_method", RubyMethodAttributes.PublicInstance)]
		public static RubyMethod DefineSingletonMethod(RubyScope scope, object self, [NotNull] ClrName methodName, [NotNull] RubyMethod method)
		{
			return ModuleOps.DefineMethod(scope, scope.RubyContext.GetOrCreateSingletonClass(self), methodName, method);
		}

		[RubyMethod("define_singleton_method", RubyMethodAttributes.PublicInstance)]
		public static UnboundMethod DefineSingletonMethod(RubyScope scope, object self, [DefaultProtocol][NotNull] string methodName, [NotNull] UnboundMethod method)
		{
			return ModuleOps.DefineMethod(scope, scope.RubyContext.GetOrCreateSingletonClass(self), methodName, method);
		}

		[RubyMethod("define_singleton_method", RubyMethodAttributes.PublicInstance)]
		public static UnboundMethod DefineSingletonMethod(RubyScope scope, object self, [NotNull] ClrName methodName, [NotNull] UnboundMethod method)
		{
			return ModuleOps.DefineMethod(scope, scope.RubyContext.GetOrCreateSingletonClass(self), methodName, method);
		}

		[RubyMethod("define_singleton_method", RubyMethodAttributes.PublicInstance)]
		public static Proc DefineSingletonMethod(RubyScope scope, [NotNull] BlockParam block, object self, [NotNull][DefaultProtocol] string methodName)
		{
			return ModuleOps.DefineMethod(scope, block, scope.RubyContext.GetOrCreateSingletonClass(self), methodName);
		}

		[RubyMethod("define_singleton_method", RubyMethodAttributes.PublicInstance)]
		public static Proc DefineSingletonMethod(RubyScope scope, [NotNull] BlockParam block, object self, [NotNull] ClrName methodName)
		{
			return ModuleOps.DefineMethod(scope, block, scope.RubyContext.GetOrCreateSingletonClass(self), methodName);
		}

		[RubyMethod("define_singleton_method", RubyMethodAttributes.PublicInstance)]
		public static Proc DefineSingletonMethod(RubyScope scope, object self, [DefaultProtocol][NotNull] string methodName, [NotNull] Proc block)
		{
			return ModuleOps.DefineMethod(scope, scope.RubyContext.GetOrCreateSingletonClass(self), methodName, block);
		}

		[RubyMethod("define_singleton_method", RubyMethodAttributes.PublicInstance)]
		public static Proc DefineSingletonMethod(RubyScope scope, object self, [NotNull] ClrName methodName, [NotNull] Proc block)
		{
			return ModuleOps.DefineMethod(scope, scope.RubyContext.GetOrCreateSingletonClass(self), methodName, block);
		}

		[RubyMethod("methods")]
		public static RubyArray GetMethods(RubyContext context, object self, bool inherited)
		{
			IList<string> foreignDynamicMemberNames = context.GetForeignDynamicMemberNames(self);
			RubyClass immediateClassOf = context.GetImmediateClassOf(self);
			if (!inherited && !immediateClassOf.IsSingletonClass)
			{
				RubyArray rubyArray = new RubyArray();
				if (foreignDynamicMemberNames.Count > 0)
				{
					foreach (string item in foreignDynamicMemberNames)
					{
						if (Tokenizer.IsMethodName(item) || Tokenizer.IsOperatorName(item))
						{
							rubyArray.Add(new ClrName(item));
						}
					}
					return rubyArray;
				}
				return rubyArray;
			}
			return ModuleOps.GetMethods(immediateClassOf, inherited, RubyMethodAttributes.Public | RubyMethodAttributes.Protected, foreignDynamicMemberNames);
		}

		[RubyMethod("singleton_methods")]
		public static RubyArray GetSingletonMethods(RubyContext context, object self, bool inherited)
		{
			RubyClass immediateClassOf = context.GetImmediateClassOf(self);
			return ModuleOps.GetMethods(immediateClassOf, inherited, RubyMethodAttributes.PublicSingleton | RubyMethodAttributes.Protected);
		}

		[RubyMethod("private_methods")]
		public static RubyArray GetPrivateMethods(RubyContext context, object self, bool inherited)
		{
			return GetMethods(context, self, inherited, RubyMethodAttributes.PrivateInstance);
		}

		[RubyMethod("protected_methods")]
		public static RubyArray GetProtectedMethods(RubyContext context, object self, bool inherited)
		{
			return GetMethods(context, self, inherited, RubyMethodAttributes.ProtectedInstance);
		}

		[RubyMethod("public_methods")]
		public static RubyArray GetPublicMethods(RubyContext context, object self, bool inherited)
		{
			return GetMethods(context, self, inherited, RubyMethodAttributes.PublicInstance);
		}

		private static RubyArray GetMethods(RubyContext context, object self, bool inherited, RubyMethodAttributes attributes)
		{
			RubyClass immediateClassOf = context.GetImmediateClassOf(self);
			return ModuleOps.GetMethods(immediateClassOf, inherited, attributes);
		}

		[RubyMethod("`", RubyMethodAttributes.PrivateInstance, BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("`", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static MutableString ExecuteCommand(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString command)
		{
			Process process = RubyProcess.CreateProcess(context, command, true);
			string text = process.StandardOutput.ReadToEnd();
			if (Environment.NewLine != "\n")
			{
				text = text.Replace(Environment.NewLine, "\n");
			}
			return MutableString.Create(text, RubyEncoding.GetRubyEncoding(process.StandardOutput.CurrentEncoding));
		}

		[RubyMethod("exec", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("exec", RubyMethodAttributes.PrivateInstance, BuildConfig = "!SILVERLIGHT")]
		public static void Execute(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString command)
		{
			Process process = RubyProcess.CreateProcess(context, command, false);
			process.WaitForExit();
			Exit(self, process.ExitCode);
		}

		[RubyMethod("exec", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("exec", RubyMethodAttributes.PrivateInstance, BuildConfig = "!SILVERLIGHT")]
		public static void Execute(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString command, [DefaultProtocol][NotNullItems] params MutableString[] args)
		{
			Process process = RubyProcess.CreateProcess(context, command, args);
			Exit(self, process.ExitCode);
		}

		[RubyMethod("system", RubyMethodAttributes.PrivateInstance, BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("system", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static bool System(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString command)
		{
			try
			{
				Process process = RubyProcess.CreateProcess(context, command, false);
				process.WaitForExit();
				return process.ExitCode == 0;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		[RubyMethod("system", RubyMethodAttributes.PrivateInstance, BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("system", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static bool System(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString command, [NotNullItems][DefaultProtocol] params MutableString[] args)
		{
			try
			{
				Process process = RubyProcess.CreateProcess(context, command, args);
				return process.ExitCode == 0;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		[RubyMethod("select", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("select", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray Select(RubyContext context, object self, RubyArray read, [Optional] RubyArray write, [Optional] RubyArray error)
		{
			return RubyIOOps.Select(context, null, read, write, error);
		}

		[RubyMethod("select", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("select", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray Select(RubyContext context, object self, RubyArray read, [Optional] RubyArray write, [Optional] RubyArray error, int timeoutInSeconds)
		{
			return RubyIOOps.Select(context, null, read, write, error, timeoutInSeconds);
		}

		[RubyMethod("select", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("select", RubyMethodAttributes.PrivateInstance)]
		public static RubyArray Select(RubyContext context, object self, RubyArray read, [Optional] RubyArray write, [Optional] RubyArray error, double timeoutInSeconds)
		{
			return RubyIOOps.Select(context, null, read, write, error, timeoutInSeconds);
		}

		[RubyMethod("sleep", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("sleep", RubyMethodAttributes.PrivateInstance)]
		public static void Sleep(object self)
		{
			ThreadOps.DoSleep();
		}

		[RubyMethod("sleep", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("sleep", RubyMethodAttributes.PublicSingleton)]
		public static int Sleep(object self, int seconds)
		{
			if (seconds < 0)
			{
				throw RubyExceptions.CreateArgumentError("time interval must be positive");
			}
			long num = seconds * 1000;
			Thread.Sleep((int)((num > int.MaxValue) ? (-1) : num));
			return seconds;
		}

		[RubyMethod("sleep", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("sleep", RubyMethodAttributes.PrivateInstance)]
		public static int Sleep(object self, double seconds)
		{
			if (seconds < 0.0)
			{
				throw RubyExceptions.CreateArgumentError("time interval must be positive");
			}
			double num = seconds * 1000.0;
			Thread.Sleep((num > 2147483647.0) ? (-1) : ((int)num));
			return (int)seconds;
		}

		[RubyMethod("test", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("test", RubyMethodAttributes.PublicSingleton)]
		public static object Test(ConversionStorage<MutableString> toPath, object self, [NotNull] MutableString cmd, object path)
		{
			if (cmd.IsEmpty)
			{
				throw RubyExceptions.CreateTypeConversionError("String", "Integer");
			}
			return Test(toPath, self, cmd.GetChar(0), path);
		}

		[RubyMethod("test", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("test", RubyMethodAttributes.PrivateInstance)]
		public static object Test(ConversionStorage<MutableString> toPath, object self, [DefaultProtocol] int cmd, object path)
		{
			RubyContext context = toPath.Context;
			MutableString path2 = Protocols.CastToPath(toPath, path);
			cmd &= 0xFF;
			switch (cmd)
			{
			case 65:
				return RubyFileOps.RubyStatOps.AccessTime(RubyFileOps.RubyStatOps.Create(context, path2));
			case 98:
				return RubyFileOps.RubyStatOps.IsBlockDevice(RubyFileOps.RubyStatOps.Create(context, path2));
			case 67:
				return RubyFileOps.RubyStatOps.CreateTime(RubyFileOps.RubyStatOps.Create(context, path2));
			case 99:
				return RubyFileOps.RubyStatOps.IsCharDevice(RubyFileOps.RubyStatOps.Create(context, path2));
			case 100:
				return FileTest.DirectoryExists(context, path2);
			case 101:
			case 102:
				return FileTest.FileExists(context, path2);
			case 103:
				return RubyFileOps.RubyStatOps.IsSetGid(RubyFileOps.RubyStatOps.Create(context, path2));
			case 71:
				return RubyFileOps.RubyStatOps.IsGroupOwned(RubyFileOps.RubyStatOps.Create(context, path2));
			case 107:
				return RubyFileOps.RubyStatOps.IsSticky(RubyFileOps.RubyStatOps.Create(context, path2));
			case 108:
				return RubyFileOps.RubyStatOps.IsSymLink(RubyFileOps.RubyStatOps.Create(context, path2));
			case 77:
				throw new NotImplementedException();
			case 79:
				throw new NotImplementedException();
			case 111:
				throw new NotImplementedException();
			case 112:
				throw new NotImplementedException();
			case 114:
				throw new NotImplementedException();
			case 82:
				throw new NotImplementedException();
			case 115:
				throw new NotImplementedException();
			case 83:
				throw new NotImplementedException();
			case 117:
				throw new NotImplementedException();
			case 119:
				throw new NotImplementedException();
			case 87:
				throw new NotImplementedException();
			case 120:
				throw new NotImplementedException();
			case 88:
				throw new NotImplementedException();
			case 122:
				throw new NotImplementedException();
			default:
				throw RubyExceptions.CreateArgumentError("unknown command ?{0}", (char)cmd);
			}
		}

		[RubyMethod("test", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("test", RubyMethodAttributes.PublicSingleton)]
		public static object Test(RubyContext context, object self, int cmd, [NotNull][DefaultProtocol] MutableString file1, [DefaultProtocol][NotNull] MutableString file2)
		{
			cmd &= 0xFF;
			switch (cmd)
			{
			case 45:
				throw new NotImplementedException();
			case 61:
				throw new NotImplementedException();
			case 60:
				throw new NotImplementedException();
			case 62:
				throw new NotImplementedException();
			default:
				throw RubyExceptions.CreateArgumentError("unknown command ?{0}", (char)cmd);
			}
		}

		[RubyMethod("trap", RubyMethodAttributes.PrivateInstance, BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("trap", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static object Trap(RubyContext context, object self, object signalId, Proc proc)
		{
			return Signal.Trap(context, self, signalId, proc);
		}

		[RubyMethod("trap", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("trap", RubyMethodAttributes.PrivateInstance, BuildConfig = "!SILVERLIGHT")]
		public static object Trap(RubyContext context, [NotNull] BlockParam block, object self, object signalId)
		{
			return Signal.Trap(context, block, self, signalId);
		}

		[RubyMethod("abort", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("abort", RubyMethodAttributes.PublicSingleton)]
		public static void Abort(object self)
		{
			Exit(self, 1);
		}

		[RubyMethod("abort", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("abort", RubyMethodAttributes.PrivateInstance)]
		public static void Abort(BinaryOpStorage writeStorage, object self, [DefaultProtocol][NotNull] MutableString message)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = writeStorage.GetCallSite("write", 1);
			callSite.Target(callSite, writeStorage.Context.StandardErrorOutput, message);
			Exit(self, 1);
		}

		[RubyMethod("exit", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("exit", RubyMethodAttributes.PrivateInstance)]
		public static void Exit(object self)
		{
			Exit(self, 0);
		}

		[RubyMethod("exit", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("exit", RubyMethodAttributes.PublicSingleton)]
		public static void Exit(object self, [NotNull] bool isSuccessful)
		{
			Exit(self, (!isSuccessful) ? 1 : 0);
		}

		[RubyMethod("exit", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("exit", RubyMethodAttributes.PublicSingleton)]
		public static void Exit(object self, [DefaultProtocol] int exitCode)
		{
			throw new SystemExit(exitCode, "exit");
		}

		[RubyMethod("exit!", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("exit!", RubyMethodAttributes.PrivateInstance)]
		public static void TerminateExecution(RubyContext context, object self)
		{
			TerminateExecution(context, self, 1);
		}

		[RubyMethod("exit!", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("exit!", RubyMethodAttributes.PublicSingleton)]
		public static void TerminateExecution(RubyContext context, object self, bool isSuccessful)
		{
			TerminateExecution(context, self, (!isSuccessful) ? 1 : 0);
		}

		[RubyMethod("exit!", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("exit!", RubyMethodAttributes.PublicSingleton)]
		public static void TerminateExecution(RubyContext context, object self, int exitCode)
		{
			context.DomainManager.Platform.TerminateScriptExecution(exitCode);
		}

		[RubyMethod("at_exit", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("at_exit", RubyMethodAttributes.PublicSingleton)]
		public static Proc AtExit(BlockParam block, object self)
		{
			if (block == null)
			{
				throw RubyExceptions.CreateArgumentError("called without a block");
			}
			block.RubyContext.RegisterShutdownHandler(block.Proc);
			return block.Proc;
		}

		[RubyMethod("load", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
		public static bool Load(ConversionStorage<MutableString> toPath, RubyScope scope, object self, object libraryName, [Optional] bool wrap)
		{
			return scope.RubyContext.Loader.LoadFile(scope.GlobalScope.Scope, self, Protocols.CastToPath(toPath, libraryName), wrap ? LoadFlags.LoadIsolated : LoadFlags.None);
		}

		[RubyMethod("require", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("require", RubyMethodAttributes.PrivateInstance)]
		public static bool Require(ConversionStorage<MutableString> toPath, RubyScope scope, object self, object libraryName)
		{
			return scope.RubyContext.Loader.LoadFile(scope.GlobalScope.Scope, self, Protocols.CastToPath(toPath, libraryName), LoadFlags.Require);
		}

		[RubyMethod("load_assembly", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("load_assembly", RubyMethodAttributes.PrivateInstance)]
		public static bool LoadAssembly(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString assemblyName, [Optional][DefaultProtocol][NotNull] MutableString libraryNamespace)
		{
			string typeName = ((libraryNamespace != null) ? LibraryInitializer.GetFullTypeName(libraryNamespace.ConvertToString()) : null);
			return context.Loader.LoadAssembly(assemblyName.ConvertToString(), typeName, true, true) != null;
		}

		[RubyMethod("using_clr_extensions", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("using_clr_extensions", RubyMethodAttributes.PublicSingleton)]
		public static void UsingClrExtensions(RubyContext context, object self, RubyModule namespaceModule)
		{
			string @namespace;
			if (namespaceModule == null)
			{
				@namespace = "";
			}
			else
			{
				if (namespaceModule.NamespaceTracker == null)
				{
					throw RubyExceptions.CreateNotClrNamespaceError(namespaceModule);
				}
				if (context != namespaceModule.Context)
				{
					throw RubyExceptions.CreateTypeError("Cannot use namespace `{0}' defined in a foreign runtime #{1}", namespaceModule.NamespaceTracker.Name, namespaceModule.Context.RuntimeId);
				}
				@namespace = namespaceModule.NamespaceTracker.Name;
			}
			context.ActivateExtensions(@namespace);
		}

		private static object OpenWithBlock(BlockParam block, RubyIO file)
		{
			try
			{
				object blockResult;
				block.Yield(file, out blockResult);
				return blockResult;
			}
			finally
			{
				file.Close();
			}
		}

		private static void SetPermission(RubyContext context, string fileName, int permission)
		{
			if (!context.DomainManager.Platform.FileExists(fileName))
			{
				RubyFileOps.Chmod(fileName, permission);
			}
		}

		private static RubyIO CheckOpenPipe(RubyContext context, MutableString path, IOMode mode)
		{
			string text = path.ConvertToString();
			if (text.Length > 0 && text[0] == '|')
			{
				if (text.Length > 1 && text[1] == '-')
				{
					throw new NotImplementedError("forking a process is not supported");
				}
				return RubyIOOps.OpenPipe(context, path.GetSlice(1), mode);
			}
			return null;
		}

		[RubyMethod("open", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		public static RubyIO Open(RubyContext context, object self, [NotNull][DefaultProtocol] MutableString path, [Optional][DefaultProtocol] MutableString modeString, [DefaultProtocol] int permission)
		{
			IOMode mode = IOModeEnum.Parse(modeString);
			RubyIO rubyIO = CheckOpenPipe(context, path, mode);
			if (rubyIO != null)
			{
				return rubyIO;
			}
			string text = path.ConvertToString();
			RubyIO result = new RubyFile(context, text, mode);
			SetPermission(context, text, permission);
			return result;
		}

		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("open", RubyMethodAttributes.PrivateInstance)]
		public static object Open(RubyContext context, [NotNull] BlockParam block, object self, [DefaultProtocol][NotNull] MutableString path, [Optional][DefaultProtocol] MutableString mode, [DefaultProtocol] int permission)
		{
			RubyIO file = Open(context, self, path, mode, permission);
			return OpenWithBlock(block, file);
		}

		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("open", RubyMethodAttributes.PrivateInstance)]
		public static RubyIO Open(RubyContext context, object self, [NotNull][DefaultProtocol] MutableString path, int mode, [DefaultProtocol] int permission)
		{
			RubyIO rubyIO = CheckOpenPipe(context, path, (IOMode)mode);
			if (rubyIO != null)
			{
				return rubyIO;
			}
			string text = path.ConvertToString();
			RubyIO result = new RubyFile(context, text, (IOMode)mode);
			SetPermission(context, text, permission);
			return result;
		}

		[RubyMethod("open", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		public static object Open(RubyContext context, [NotNull] BlockParam block, object self, [NotNull][DefaultProtocol] MutableString path, int mode, [DefaultProtocol] int permission)
		{
			RubyIO file = Open(context, self, path, mode, permission);
			return OpenWithBlock(block, file);
		}

		[RubyMethod("p", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("p", RubyMethodAttributes.PublicSingleton)]
		public static object PrintInspect(BinaryOpStorage writeStorage, UnaryOpStorage inspectStorage, ConversionStorage<MutableString> tosConversion, object self, params object[] args)
		{
			CallSite<Func<CallSite, object, object>> callSite = inspectStorage.GetCallSite("inspect");
			MutableString[] array = new MutableString[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				array[i] = Protocols.ConvertToString(tosConversion, callSite.Target(callSite, args[i]));
			}
			MutableString[] array2 = array;
			foreach (MutableString str in array2)
			{
				PrintOps.Puts(writeStorage, writeStorage.Context.StandardOutput, str);
			}
			if (args.Length == 0)
			{
				return null;
			}
			if (args.Length == 1)
			{
				return args[0];
			}
			return new RubyArray(args);
		}

		[RubyMethod("print", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("print", RubyMethodAttributes.PublicSingleton)]
		public static void Print(BinaryOpStorage writeStorage, RubyScope scope, object self)
		{
			PrintOps.Print(writeStorage, scope, scope.RubyContext.StandardOutput);
		}

		[RubyMethod("print", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("print", RubyMethodAttributes.PublicSingleton)]
		public static void Print(BinaryOpStorage writeStorage, object self, object val)
		{
			PrintOps.Print(writeStorage, writeStorage.Context.StandardOutput, val);
		}

		[RubyMethod("print", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("print", RubyMethodAttributes.PublicSingleton)]
		public static void Print(BinaryOpStorage writeStorage, object self, params object[] args)
		{
			PrintOps.Print(writeStorage, writeStorage.Context.StandardOutput, args);
		}

		[RubyMethod("printf", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("printf", RubyMethodAttributes.PublicSingleton)]
		public static void PrintFormatted(StringFormatterSiteStorage storage, ConversionStorage<MutableString> stringCast, BinaryOpStorage writeStorage, object self, [NotNull] MutableString format, params object[] args)
		{
			PrintFormatted(storage, stringCast, writeStorage, self, storage.Context.StandardOutput, format, args);
		}

		[RubyMethod("printf", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("printf", RubyMethodAttributes.PrivateInstance)]
		public static void PrintFormatted(StringFormatterSiteStorage storage, ConversionStorage<MutableString> stringCast, BinaryOpStorage writeStorage, object self, object io, [NotNull] object format, params object[] args)
		{
			Protocols.Write(writeStorage, io, Sprintf(storage, self, Protocols.CastToString(stringCast, format), args));
		}

		[RubyMethod("putc", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("putc", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Putc(BinaryOpStorage writeStorage, object self, [NotNull] MutableString arg)
		{
			return PrintOps.Putc(writeStorage, writeStorage.Context.StandardOutput, arg);
		}

		[RubyMethod("putc", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("putc", RubyMethodAttributes.PrivateInstance)]
		public static int Putc(BinaryOpStorage writeStorage, object self, [DefaultProtocol] int arg)
		{
			return PrintOps.Putc(writeStorage, writeStorage.Context.StandardOutput, arg);
		}

		[RubyMethod("puts", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("puts", RubyMethodAttributes.PrivateInstance)]
		public static void PutsEmptyLine(BinaryOpStorage writeStorage, object self)
		{
			PrintOps.PutsEmptyLine(writeStorage, writeStorage.Context.StandardOutput);
		}

		[RubyMethod("puts", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("puts", RubyMethodAttributes.PublicSingleton)]
		public static void PutString(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, ConversionStorage<IList> tryToAry, object self, object arg)
		{
			PrintOps.Puts(writeStorage, tosConversion, tryToAry, writeStorage.Context.StandardOutput, arg);
		}

		[RubyMethod("puts", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("puts", RubyMethodAttributes.PublicSingleton)]
		public static void PutString(BinaryOpStorage writeStorage, object self, [NotNull] MutableString arg)
		{
			PrintOps.Puts(writeStorage, writeStorage.Context.StandardOutput, arg);
		}

		[RubyMethod("puts", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("puts", RubyMethodAttributes.PublicSingleton)]
		public static void PutString(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, ConversionStorage<IList> tryToAry, object self, params object[] args)
		{
			PrintOps.Puts(writeStorage, tosConversion, tryToAry, writeStorage.Context.StandardOutput, args);
		}

		[RubyMethod("display")]
		public static void Display(BinaryOpStorage writeStorage, object self)
		{
			Protocols.Write(writeStorage, writeStorage.Context.StandardOutput, self);
		}

		[RubyMethod("warn", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("warn", RubyMethodAttributes.PublicSingleton)]
		public static void ReportWarning(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, object self, object message)
		{
			PrintOps.ReportWarning(writeStorage, tosConversion, message);
		}

		[RubyMethod("gets", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("gets", RubyMethodAttributes.PrivateInstance)]
		public static object ReadInputLine(CallSiteStorage<Func<CallSite, object, object>> storage, object self)
		{
			CallSite<Func<CallSite, object, object>> callSite = storage.GetCallSite("gets", 0);
			return callSite.Target(callSite, storage.Context.StandardInput);
		}

		[RubyMethod("gets", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("gets", RubyMethodAttributes.PublicSingleton)]
		public static object ReadInputLine(CallSiteStorage<Func<CallSite, object, object, object>> storage, object self, [NotNull] MutableString separator)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = storage.GetCallSite("gets", 1);
			return callSite.Target(callSite, storage.Context.StandardInput, separator);
		}

		[RubyMethod("gets", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("gets", RubyMethodAttributes.PrivateInstance)]
		public static object ReadInputLine(CallSiteStorage<Func<CallSite, object, object, object, object>> storage, object self, [NotNull] MutableString separator, [DefaultProtocol] int limit)
		{
			CallSite<Func<CallSite, object, object, object, object>> callSite = storage.GetCallSite("gets", 2);
			return callSite.Target(callSite, storage.Context.StandardInput, separator, limit);
		}

		[RubyMethod("sprintf", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("format", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("format", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("sprintf", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Sprintf(StringFormatterSiteStorage storage, object self, [DefaultProtocol][NotNull] MutableString format, params object[] args)
		{
			return new StringFormatter(storage, format.ConvertToString(), format.Encoding, args).Format();
		}

		[RubyMethod("srand", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("srand", RubyMethodAttributes.PrivateInstance)]
		public static object SeedRandomNumberGenerator(RubyContext context, object self)
		{
			if (_RNGCryptoServiceProvider == null)
			{
				_RNGCryptoServiceProvider = new RNGCryptoServiceProvider();
			}
			int num = 0;
			do
			{
				byte[] array = new byte[4];
				_RNGCryptoServiceProvider.GetBytes(array);
				num = (array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3];
			}
			while (num == 0);
			return SeedRandomNumberGenerator(context, self, num);
		}

		[RubyMethod("srand", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("srand", RubyMethodAttributes.PrivateInstance)]
		public static object SeedRandomNumberGenerator(RubyContext context, object self, [DefaultProtocol] IntegerValue seed)
		{
			object randomNumberGeneratorSeed = context.RandomNumberGeneratorSeed;
			context.SeedRandomNumberGenerator(seed);
			return randomNumberGeneratorSeed;
		}

		[RubyMethod("rand", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("rand", RubyMethodAttributes.PublicSingleton)]
		public static double Random(RubyContext context, object self)
		{
			return context.RandomNumberGenerator.NextDouble();
		}

		[RubyMethod("rand", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("rand", RubyMethodAttributes.PrivateInstance)]
		public static object Random(RubyContext context, object self, int limit)
		{
			Random randomNumberGenerator = context.RandomNumberGenerator;
			if (limit == int.MinValue)
			{
				return randomNumberGenerator.Random(-(BigInteger)limit);
			}
			return ScriptingRuntimeHelpers.Int32ToObject(randomNumberGenerator.Next(limit));
		}

		[RubyMethod("rand", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("rand", RubyMethodAttributes.PublicSingleton)]
		public static object Random(ConversionStorage<IntegerValue> conversion, RubyContext context, object self, object limit)
		{
			IntegerValue integerValue = Protocols.ConvertToInteger(conversion, limit);
			Random randomNumberGenerator = context.RandomNumberGenerator;
			int ret = 0;
			BigInteger limit2 = null;
			bool flag;
			if (integerValue.IsFixnum)
			{
				if (integerValue.Fixnum == int.MinValue)
				{
					limit2 = -(BigInteger)integerValue.Fixnum;
					flag = false;
				}
				else
				{
					ret = Math.Abs(integerValue.Fixnum);
					flag = true;
				}
			}
			else
			{
				limit2 = integerValue.Bignum.Abs();
				flag = integerValue.Bignum.AsInt32(out ret);
			}
			if (flag)
			{
				if (ret == 0)
				{
					return randomNumberGenerator.NextDouble();
				}
				return ScriptingRuntimeHelpers.Int32ToObject(randomNumberGenerator.Next(ret));
			}
			return randomNumberGenerator.Random(limit2);
		}

		[RubyMethod("set_trace_func", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("set_trace_func", RubyMethodAttributes.PrivateInstance)]
		public static Proc SetTraceListener(RubyContext context, object self, Proc listener)
		{
			if (listener != null && !context.RubyOptions.EnableTracing)
			{
				throw new NotSupportedException("Tracing is not supported unless -trace option is specified.");
			}
			return context.TraceListener = listener;
		}

		[RubyMethod("to_enum")]
		[RubyMethod("enum_for")]
		public static Enumerator Create(object self, [NotNull][DefaultProtocol] string enumeratorName, params object[] targetParameters)
		{
			return new Enumerator(self, enumeratorName, targetParameters);
		}
	}
}
