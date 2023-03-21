using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Math.Extensions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Range = IronRuby.Builtins.Range;

namespace IronRuby.Runtime
{
	[CLSCompliant(false)]
	public static class RubyOps
	{
		private sealed class _NeedsUpdate
		{
		}

		public const int OptimizedOpCallParamCount = 5;

		public const char SuffixLiteral = 'L';

		public const char SuffixMutable = 'M';

		public const int MakeStringParamCount = 2;

		public static readonly object DefaultArgument = new object();

		public static readonly object ForwardToBase = new object();

		public static readonly object NeedsUpdate = new _NeedsUpdate();

		public static MutableTuple GetLocals(RubyScope scope)
		{
			return scope.Locals;
		}

		public static MutableTuple GetParentLocals(RubyScope scope)
		{
			return scope.Parent.Locals;
		}

		public static RubyScope GetParentScope(RubyScope scope)
		{
			return scope.Parent;
		}

		public static Proc GetMethodBlockParameter(RubyScope scope)
		{
			RubyMethodScope innerMostMethodScope = scope.GetInnerMostMethodScope();
			if (innerMostMethodScope == null)
			{
				return null;
			}
			return innerMostMethodScope.BlockParameter;
		}

		public static object GetMethodBlockParameterSelf(RubyScope scope)
		{
			Proc blockParameter = scope.GetInnerMostMethodScope().BlockParameter;
			return blockParameter.Self;
		}

		public static object GetProcSelf(Proc proc)
		{
			return proc.Self;
		}

		public static int GetProcArity(Proc proc)
		{
			return proc.Dispatcher.Arity;
		}

		public static void InitializeScope(RubyScope scope, MutableTuple locals, string[] variableNames, InterpretedFrame interpretedFrame)
		{
			if (!scope.LocalsInitialized)
			{
				scope.SetLocals(locals, variableNames ?? ArrayUtils.EmptyStrings);
			}
			scope.InterpretedFrame = interpretedFrame;
		}

		public static void InitializeScopeNoLocals(RubyScope scope, InterpretedFrame interpretedFrame)
		{
			scope.InterpretedFrame = interpretedFrame;
		}

		public static void SetDataConstant(RubyScope scope, string dataPath, int dataOffset)
		{
			RubyContext rubyContext = scope.RubyContext;
			RubyFile rubyFile;
			if (rubyContext.DomainManager.Platform.FileExists(dataPath))
			{
				rubyFile = new RubyFile(rubyContext, dataPath, IOMode.ReadOnly);
				rubyFile.Seek(dataOffset, SeekOrigin.Begin);
			}
			else
			{
				rubyFile = null;
			}
			rubyContext.ObjectClass.SetConstant("DATA", rubyFile);
		}

		public static RubyModuleScope CreateModuleScope(MutableTuple locals, string[] variableNames, RubyScope parent, RubyModule module)
		{
			if (parent.RubyContext != module.Context)
			{
				throw RubyExceptions.CreateTypeError("Cannot open a module `{0}' defined in a foreign runtime #{1}", module.Name, module.Context.RuntimeId);
			}
			RubyModuleScope rubyModuleScope = new RubyModuleScope(parent, module);
			rubyModuleScope.SetLocals(locals, variableNames ?? ArrayUtils.EmptyStrings);
			return rubyModuleScope;
		}

		public static RubyMethodScope CreateMethodScope(MutableTuple locals, string[] variableNames, int visibleParameterCount, RubyScope parentScope, RubyModule declaringModule, string definitionName, object selfObject, Proc blockParameter, InterpretedFrame interpretedFrame)
		{
			return new RubyMethodScope(locals, variableNames ?? ArrayUtils.EmptyStrings, visibleParameterCount, parentScope, declaringModule, definitionName, selfObject, blockParameter, interpretedFrame);
		}

		public static RubyScope CreateFileInitializerScope(MutableTuple locals, string[] variableNames, RubyScope parent)
		{
			return new RubyFileInitializerScope(locals, variableNames ?? ArrayUtils.EmptyStrings, parent);
		}

		public static RubyBlockScope CreateBlockScope(MutableTuple locals, string[] variableNames, BlockParam blockParam, object selfObject, InterpretedFrame interpretedFrame)
		{
			return new RubyBlockScope(locals, variableNames ?? ArrayUtils.EmptyStrings, blockParam, selfObject, interpretedFrame);
		}

		public static void TraceMethodCall(RubyMethodScope scope, string fileName, int lineNumber)
		{
			RubyModule declaringModule = scope.DeclaringModule;
			scope.RubyContext.ReportTraceEvent("call", scope, declaringModule, scope.DefinitionName, fileName, lineNumber);
		}

		public static void TraceMethodReturn(RubyMethodScope scope, string fileName, int lineNumber)
		{
			RubyModule declaringModule = scope.DeclaringModule;
			scope.RubyContext.ReportTraceEvent("return", scope, declaringModule, scope.DefinitionName, fileName, lineNumber);
		}

		public static void TraceBlockCall(RubyBlockScope scope, BlockParam block, string fileName, int lineNumber)
		{
			RubyLambdaMethodInfo method = block.Proc.Method;
			if (method != null)
			{
				scope.RubyContext.ReportTraceEvent("call", scope, method.DeclaringModule, method.DefinitionName, fileName, lineNumber);
			}
		}

		public static void TraceBlockReturn(RubyBlockScope scope, BlockParam block, string fileName, int lineNumber)
		{
			RubyLambdaMethodInfo method = block.Proc.Method;
			if (method != null)
			{
				scope.RubyContext.ReportTraceEvent("return", scope, method.DeclaringModule, method.DefinitionName, fileName, lineNumber);
			}
		}

		public static void PrintInteractiveResult(RubyScope scope, MutableString value)
		{
			Stream outputStream = scope.RubyContext.DomainManager.SharedIO.OutputStream;
			outputStream.WriteByte(61);
			outputStream.WriteByte(62);
			outputStream.WriteByte(32);
			byte[] array = value.ToByteArray();
			outputStream.Write(array, 0, array.Length);
			outputStream.WriteByte(13);
			outputStream.WriteByte(10);
		}

		public static object GetLocalVariable(RubyScope scope, string name)
		{
			return scope.ResolveLocalVariable(name);
		}

		public static object SetLocalVariable(object value, RubyScope scope, string name)
		{
			return scope.ResolveAndSetLocalVariable(name, value);
		}

		public static VersionHandle GetSelfClassVersionHandle(RubyScope scope)
		{
			return scope.SelfImmediateClass.Version;
		}

		public static RubyContext GetContextFromModule(RubyModule module)
		{
			return module.Context;
		}

		public static RubyContext GetContextFromIRubyObject(IRubyObject obj)
		{
			return obj.ImmediateClass.Context;
		}

		public static RubyContext GetContextFromScope(RubyScope scope)
		{
			return scope.RubyContext;
		}

		public static RubyContext GetContextFromMethod(RubyMethod method)
		{
			return method.Info.Context;
		}

		public static RubyContext GetContextFromBlockParam(BlockParam block)
		{
			return block.RubyContext;
		}

		public static RubyContext GetContextFromProc(Proc proc)
		{
			return proc.LocalScope.RubyContext;
		}

		public static RubyScope GetEmptyScope(RubyContext context)
		{
			return context.EmptyScope;
		}

		public static Scope GetGlobalScopeFromScope(RubyScope scope)
		{
			return scope.GlobalScope.Scope;
		}

		public static Proc InstantiateBlock(RubyScope scope, object self, BlockDispatcher dispatcher)
		{
			if ((object)dispatcher.Method == null)
			{
				return null;
			}
			return new Proc(ProcKind.Block, self, scope, dispatcher);
		}

		public static Proc InstantiateLambda(RubyScope scope, object self, BlockDispatcher dispatcher)
		{
			if ((object)dispatcher.Method == null)
			{
				return null;
			}
			return new Proc(ProcKind.Lambda, self, scope, dispatcher);
		}

		public static Proc DefineBlock(RubyScope scope, object self, BlockDispatcher dispatcher, object clrMethod)
		{
			return new Proc(ProcKind.Block, self, scope, dispatcher.SetMethod(clrMethod));
		}

		public static Proc DefineLambda(RubyScope scope, object self, BlockDispatcher dispatcher, object clrMethod)
		{
			return new Proc(ProcKind.Lambda, self, scope, dispatcher.SetMethod(clrMethod));
		}

		public static void InitializeBlock(Proc proc)
		{
			proc.Kind = ProcKind.Block;
		}

		public static void RegisterShutdownHandler(Proc proc)
		{
			proc.LocalScope.RubyContext.RegisterShutdownHandler(proc);
		}

		public static object Yield0(Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.Invoke(blockParam, self, procArg);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object Yield1(object arg1, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.Invoke(blockParam, self, procArg, arg1);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		internal static object YieldNoAutoSplat1(object arg1, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.InvokeNoAutoSplat(blockParam, self, procArg, arg1);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object Yield2(object arg1, object arg2, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.Invoke(blockParam, self, procArg, arg1, arg2);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object Yield3(object arg1, object arg2, object arg3, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.Invoke(blockParam, self, procArg, arg1, arg2, arg3);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object Yield4(object arg1, object arg2, object arg3, object arg4, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.Invoke(blockParam, self, procArg, arg1, arg2, arg3, arg4);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object YieldN(object[] args, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.Invoke(blockParam, self, procArg, args);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		internal static object Yield(object[] args, Proc procArg, object self, BlockParam blockParam)
		{
			switch (args.Length)
			{
			case 0:
				return Yield0(procArg, self, blockParam);
			case 1:
				return Yield1(args[0], procArg, self, blockParam);
			case 2:
				return Yield2(args[0], args[1], procArg, self, blockParam);
			case 3:
				return Yield3(args[0], args[1], args[2], procArg, self, blockParam);
			case 4:
				return Yield4(args[0], args[1], args[2], args[3], procArg, self, blockParam);
			default:
				return YieldN(args, procArg, self, blockParam);
			}
		}

		public static object YieldSplat0(IList splattee, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.InvokeSplat(blockParam, self, procArg, splattee);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object YieldSplat1(object arg1, IList splattee, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.InvokeSplat(blockParam, self, procArg, arg1, splattee);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object YieldSplat2(object arg1, object arg2, IList splattee, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.InvokeSplat(blockParam, self, procArg, arg1, arg2, splattee);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object YieldSplat3(object arg1, object arg2, object arg3, IList splattee, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.InvokeSplat(blockParam, self, procArg, arg1, arg2, arg3, splattee);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object YieldSplat4(object arg1, object arg2, object arg3, object arg4, IList splattee, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.InvokeSplat(blockParam, self, procArg, arg1, arg2, arg3, arg4, splattee);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object YieldSplatN(object[] args, IList splattee, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.InvokeSplat(blockParam, self, procArg, args, splattee);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object YieldSplatNRhs(object[] args, IList splattee, object rhs, Proc procArg, object self, BlockParam blockParam)
		{
			Proc proc = blockParam.Proc;
			try
			{
				return proc.Dispatcher.InvokeSplatRhs(blockParam, self, procArg, args, splattee, rhs);
			}
			catch (EvalUnwinder unwinder)
			{
				return blockParam.GetUnwinderResult(unwinder);
			}
		}

		public static object DefineMethod(object target, RubyScope scope, RubyMethodBody body)
		{
			bool flag = false;
			RubyModule rubyModule;
			RubyMemberFlags flags;
			RubyModule rubyModule2;
			RubyMemberFlags flags2;
			if (body.HasTarget)
			{
				if (!RubyUtils.CanDefineSingletonMethod(target))
				{
					throw RubyExceptions.CreateTypeError("can't define singleton method for literals");
				}
				rubyModule = null;
				flags = RubyMemberFlags.Invalid;
				rubyModule2 = scope.RubyContext.GetOrCreateSingletonClass(target);
				flags2 = RubyMemberFlags.Public;
			}
			else
			{
				RubyScope methodAttributesDefinitionScope = scope.GetMethodAttributesDefinitionScope();
				if ((methodAttributesDefinitionScope.MethodAttributes & RubyMethodAttributes.ModuleFunction) == RubyMethodAttributes.ModuleFunction)
				{
					rubyModule = scope.GetMethodDefinitionOwner();
					if (rubyModule.IsClass)
					{
						throw RubyExceptions.CreateTypeError("A module function cannot be defined on a class.");
					}
					flags = RubyMemberFlags.Private;
					rubyModule2 = rubyModule.GetOrCreateSingletonClass();
					flags2 = RubyMemberFlags.Public;
					flag = true;
				}
				else
				{
					rubyModule = scope.GetMethodDefinitionOwner();
					flags = (RubyMemberFlags)RubyUtils.GetSpecialMethodVisibility(methodAttributesDefinitionScope.Visibility, body.Name);
					rubyModule2 = null;
					flags2 = RubyMemberFlags.Invalid;
				}
			}
			RubyMethodInfo rubyMethodInfo = null;
			RubyMethodInfo rubyMethodInfo2 = null;
			if (rubyModule != null)
			{
				SetMethod(scope.RubyContext, rubyMethodInfo = new RubyMethodInfo(body, scope, rubyModule, flags));
			}
			if (rubyModule2 != null)
			{
				SetMethod(scope.RubyContext, rubyMethodInfo2 = new RubyMethodInfo(body, scope, rubyModule2, flags2));
			}
			RubyMethodInfo rubyMethodInfo3 = rubyMethodInfo ?? rubyMethodInfo2;
			rubyMethodInfo3.DeclaringModule.MethodAdded(body.Name);
			if (flag)
			{
				rubyMethodInfo3.DeclaringModule.GetOrCreateSingletonClass().MethodAdded(body.Name);
			}
			return null;
		}

		private static void SetMethod(RubyContext callerContext, RubyMethodInfo method)
		{
			RubyModule declaringModule = method.DeclaringModule;
			declaringModule.SetMethodNoEvent(callerContext, method.DefinitionName, method);
			if (declaringModule.GlobalScope != null)
			{
				ScopeSetMember(declaringModule.GlobalScope.Scope, method.DefinitionName, new RubyMethod(declaringModule.GlobalScope.MainObject, method, method.DefinitionName));
			}
		}

		public static void AliasMethod(RubyScope scope, string newName, string oldName)
		{
			scope.GetMethodDefinitionOwner().AddMethodAlias(newName, oldName);
		}

		public static void UndefineMethod(RubyScope scope, string name)
		{
			RubyModule methodDefinitionOwner = scope.GetMethodDefinitionOwner();
			if (!methodDefinitionOwner.ResolveMethod(name, VisibilityContext.AllVisible).Found)
			{
				throw RubyExceptions.CreateUndefinedMethodError(methodDefinitionOwner, name);
			}
			methodDefinitionOwner.UndefineMethod(name);
		}

		public static RubyModule DefineGlobalModule(RubyScope scope, string name)
		{
			return DefineModule(scope, scope.Top.TopModuleOrObject, name);
		}

		public static RubyModule DefineNestedModule(RubyScope scope, string name)
		{
			return DefineModule(scope, scope.GetInnerMostModuleForConstantLookup(), name);
		}

		public static RubyModule DefineModule(RubyScope scope, object target, string name)
		{
			return DefineModule(scope, RubyUtils.GetModuleFromObject(scope, target), name);
		}

		private static RubyModule DefineModule(RubyScope scope, RubyModule owner, string name)
		{
			ConstantStorage value;
			if (owner.TryGetConstant(scope.GlobalScope, name, out value))
			{
				RubyModule rubyModule = value.Value as RubyModule;
				if (rubyModule == null || rubyModule.IsClass)
				{
					throw RubyExceptions.CreateTypeError(string.Format("{0} is not a module", name));
				}
				return rubyModule;
			}
			return owner.Context.DefineModule(owner, name);
		}

		public static RubyClass DefineSingletonClass(RubyScope scope, object obj)
		{
			if (!RubyUtils.HasSingletonClass(obj))
			{
				throw RubyExceptions.CreateTypeError(string.Format("no virtual class for {0}", scope.RubyContext.GetClassOf(obj).Name));
			}
			return scope.RubyContext.GetOrCreateSingletonClass(obj);
		}

		public static RubyModule DefineGlobalClass(RubyScope scope, string name, object superClassObject)
		{
			return DefineClass(scope, scope.Top.TopModuleOrObject, name, superClassObject);
		}

		public static RubyModule DefineNestedClass(RubyScope scope, string name, object superClassObject)
		{
			return DefineClass(scope, scope.GetInnerMostModuleForConstantLookup(), name, superClassObject);
		}

		public static RubyModule DefineClass(RubyScope scope, object target, string name, object superClassObject)
		{
			return DefineClass(scope, RubyUtils.GetModuleFromObject(scope, target), name, superClassObject);
		}

		private static RubyClass DefineClass(RubyScope scope, RubyModule owner, string name, object superClassObject)
		{
			RubyClass rubyClass = ToSuperClass(owner.Context, superClassObject);
			ConstantStorage value;
			if (owner.IsObjectClass ? owner.TryResolveConstant(scope.GlobalScope, name, out value) : owner.TryGetConstant(scope.GlobalScope, name, out value))
			{
				RubyClass rubyClass2 = value.Value as RubyClass;
				if (rubyClass2 == null || !rubyClass2.IsClass)
				{
					throw RubyExceptions.CreateTypeError("{0} is not a class", name);
				}
				if (superClassObject != null && !object.ReferenceEquals(rubyClass2.SuperClass, rubyClass))
				{
					throw RubyExceptions.CreateTypeError("superclass mismatch for class {0}", name);
				}
				return rubyClass2;
			}
			return owner.Context.DefineClass(owner, name, rubyClass, null);
		}

		private static RubyClass ToSuperClass(RubyContext ec, object superClassObject)
		{
			if (superClassObject != null)
			{
				RubyClass rubyClass = superClassObject as RubyClass;
				if (rubyClass == null)
				{
					throw RubyExceptions.CreateTypeError("superclass must be a Class ({0} given)", ec.GetClassOf(superClassObject).Name);
				}
				if (rubyClass.IsSingletonClass)
				{
					throw RubyExceptions.CreateTypeError("can't make subclass of virtual class");
				}
				return rubyClass;
			}
			return ec.ObjectClass;
		}

		public static object GetUnqualifiedConstant(RubyScope scope, ConstantSiteCache cache, string name, bool isGlobal)
		{
			object result = null;
			RubyContext rubyContext = scope.RubyContext;
			RubyModule rubyModule;
			using (rubyContext.ClassHierarchyLocker())
			{
				int constantAccessVersion = rubyContext.ConstantAccessVersion;
				ConstantStorage result2;
				rubyModule = ((!isGlobal) ? scope.TryResolveConstantNoLock(scope.GlobalScope, name, out result2) : ((!rubyContext.ObjectClass.TryResolveConstantNoLock(scope.GlobalScope, name, out result2)) ? rubyContext.ObjectClass : null));
				object newValue;
				if (rubyModule == null)
				{
					if (result2.WeakValue != null)
					{
						result = result2.Value;
						newValue = result2.WeakValue;
					}
					else
					{
						result = (newValue = result2.Value);
					}
				}
				else
				{
					newValue = ConstantSiteCache.WeakMissingConstant;
				}
				cache.Update(newValue, constantAccessVersion);
			}
			if (rubyModule != null)
			{
				result = rubyModule.ConstantMissing(name);
			}
			return result;
		}

		public static object GetQualifiedConstant(RubyScope scope, ConstantSiteCache cache, string[] qualifiedName, bool isGlobal)
		{
			RubyGlobalScope globalScope = scope.GlobalScope;
			RubyContext context = globalScope.Context;
			using (context.ClassHierarchyLocker())
			{
				int constantAccessVersion = context.ConstantAccessVersion;
				RubyModule topModule = (isGlobal ? context.ObjectClass : null);
				ConstantStorage storage;
				bool anyMissing;
				object obj = ResolveQualifiedConstant(scope, qualifiedName, topModule, true, out storage, out anyMissing);
				if (!anyMissing)
				{
					cache.Update(storage.WeakValue ?? obj, constantAccessVersion);
				}
				return obj;
			}
		}

		public static object GetExpressionQualifiedConstant(object target, RubyScope scope, ExpressionQualifiedConstantSiteCache cache, string[] qualifiedName)
		{
			RubyModule rubyModule = target as RubyModule;
			if (rubyModule == null)
			{
				throw RubyUtils.CreateNotModuleException(scope, target);
			}
			VersionAndModule condition = cache.Condition;
			RubyContext context = rubyModule.Context;
			if (rubyModule.Id == condition.ModuleId && context.ConstantAccessVersion == condition.Version)
			{
				object value = cache.Value;
				if (value.GetType() == typeof(WeakReference))
				{
					return ((WeakReference)value).Target;
				}
				return value;
			}
			using (context.ClassHierarchyLocker())
			{
				int constantAccessVersion = context.ConstantAccessVersion;
				ConstantStorage storage;
				bool anyMissing;
				object obj = ResolveQualifiedConstant(scope, qualifiedName, rubyModule, true, out storage, out anyMissing);
				if (!anyMissing)
				{
					cache.Update(storage.WeakValue ?? obj, constantAccessVersion, rubyModule);
				}
				return obj;
			}
		}

		public static bool IsDefinedUnqualifiedConstant(RubyScope scope, IsDefinedConstantSiteCache cache, string name)
		{
			RubyContext rubyContext = scope.RubyContext;
			using (rubyContext.ClassHierarchyLocker())
			{
				int constantAccessVersion = rubyContext.ConstantAccessVersion;
				ConstantStorage result;
				bool flag = scope.TryResolveConstantNoLock(null, name, out result) == null;
				cache.Update(flag, constantAccessVersion);
				return flag;
			}
		}

		public static bool IsDefinedGlobalConstant(RubyScope scope, IsDefinedConstantSiteCache cache, string name)
		{
			RubyContext rubyContext = scope.RubyContext;
			using (rubyContext.ClassHierarchyLocker())
			{
				int constantAccessVersion = rubyContext.ConstantAccessVersion;
				ConstantStorage value;
				bool flag = rubyContext.ObjectClass.TryResolveConstantNoLock(null, name, out value);
				cache.Update(flag, constantAccessVersion);
				return flag;
			}
		}

		public static bool IsDefinedQualifiedConstant(RubyScope scope, IsDefinedConstantSiteCache cache, string[] qualifiedName, bool isGlobal)
		{
			RubyContext rubyContext = scope.RubyContext;
			using (rubyContext.ClassHierarchyLocker())
			{
				int constantAccessVersion = rubyContext.ConstantAccessVersion;
				RubyModule topModule = (isGlobal ? rubyContext.ObjectClass : null);
				RubyModule rubyModule;
				ConstantStorage storage;
				bool anyMissing;
				try
				{
					rubyModule = ResolveQualifiedConstant(scope, qualifiedName, topModule, false, out storage, out anyMissing) as RubyModule;
				}
				catch
				{
					scope.RubyContext.SetCurrentException(null);
					return false;
				}
				bool flag = rubyModule != null && rubyModule.TryResolveConstant(rubyContext, null, qualifiedName[qualifiedName.Length - 1], out storage);
				if (!anyMissing)
				{
					cache.Update(flag, constantAccessVersion);
				}
				return flag;
			}
		}

		public static bool IsDefinedExpressionQualifiedConstant(object target, RubyScope scope, ExpressionQualifiedIsDefinedConstantSiteCache cache, string[] qualifiedName)
		{
			RubyModule rubyModule = target as RubyModule;
			if (rubyModule == null)
			{
				return false;
			}
			VersionAndModule condition = cache.Condition;
			RubyContext context = rubyModule.Context;
			if (rubyModule.Id == condition.ModuleId && context.ConstantAccessVersion == condition.Version)
			{
				return cache.Value;
			}
			using (context.ClassHierarchyLocker())
			{
				int constantAccessVersion = context.ConstantAccessVersion;
				bool flag;
				ConstantStorage value;
				if (qualifiedName.Length == 1)
				{
					flag = rubyModule.TryResolveConstant(context, null, qualifiedName[0], out value);
				}
				else
				{
					RubyModule rubyModule2;
					bool anyMissing;
					try
					{
						rubyModule2 = ResolveQualifiedConstant(scope, qualifiedName, rubyModule, false, out value, out anyMissing) as RubyModule;
					}
					catch
					{
						return false;
					}
					flag = rubyModule2 != null && rubyModule2.TryResolveConstant(context, null, qualifiedName[qualifiedName.Length - 1], out value);
					if (anyMissing)
					{
						return flag;
					}
				}
				cache.Update(flag, constantAccessVersion, rubyModule);
				return flag;
			}
		}

		private static object ResolveQualifiedConstant(RubyScope scope, string[] qualifiedName, RubyModule topModule, bool isGet, out ConstantStorage storage, out bool anyMissing)
		{
			RubyContext rubyContext = scope.RubyContext;
			RubyGlobalScope globalScope = scope.GlobalScope;
			int num = (isGet ? qualifiedName.Length : (qualifiedName.Length - 1));
			string name = qualifiedName[0];
			RubyModule rubyModule = ((topModule == null) ? scope.TryResolveConstantNoLock(globalScope, name, out storage) : ((!topModule.TryResolveConstant(rubyContext, globalScope, name, out storage)) ? topModule : null));
			object obj;
			if (rubyModule == null)
			{
				obj = storage.Value;
				anyMissing = false;
			}
			else
			{
				anyMissing = true;
				using (rubyContext.ClassHierarchyUnlocker())
				{
					obj = rubyModule.ConstantMissing(name);
				}
			}
			for (int i = 1; i < num; i++)
			{
				RubyModule moduleFromObject = RubyUtils.GetModuleFromObject(scope, obj);
				name = qualifiedName[i];
				if (moduleFromObject.TryResolveConstant(rubyContext, globalScope, name, out storage))
				{
					if (moduleFromObject.Context != rubyContext)
					{
						anyMissing = true;
					}
					obj = storage.Value;
				}
				else
				{
					anyMissing = true;
					using (rubyContext.ClassHierarchyUnlocker())
					{
						obj = moduleFromObject.ConstantMissing(name);
					}
				}
			}
			return obj;
		}

		public static object GetMissingConstant(RubyScope scope, ConstantSiteCache cache, string name)
		{
			return scope.GetInnerMostModuleForConstantLookup().ConstantMissing(name);
		}

		public static object GetGlobalMissingConstant(RubyScope scope, ConstantSiteCache cache, string name)
		{
			return scope.RubyContext.ObjectClass.ConstantMissing(name);
		}

		public static object SetGlobalConstant(object value, RubyScope scope, string name)
		{
			RubyUtils.SetConstant(scope.RubyContext.ObjectClass, name, value);
			return value;
		}

		public static object SetUnqualifiedConstant(object value, RubyScope scope, string name)
		{
			RubyUtils.SetConstant(scope.GetInnerMostModuleForConstantLookup(), name, value);
			return value;
		}

		public static object SetQualifiedConstant(object value, object target, RubyScope scope, string name)
		{
			RubyUtils.SetConstant(RubyUtils.GetModuleFromObject(scope, target), name, value);
			return value;
		}

		public static RubyArray MakeArray0()
		{
			return new RubyArray(0);
		}

		public static RubyArray MakeArray1(object item1)
		{
			RubyArray rubyArray = new RubyArray(1);
			rubyArray.Add(item1);
			return rubyArray;
		}

		public static RubyArray MakeArray2(object item1, object item2)
		{
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(item1);
			rubyArray.Add(item2);
			return rubyArray;
		}

		public static RubyArray MakeArray3(object item1, object item2, object item3)
		{
			RubyArray rubyArray = new RubyArray(3);
			rubyArray.Add(item1);
			rubyArray.Add(item2);
			rubyArray.Add(item3);
			return rubyArray;
		}

		public static RubyArray MakeArray4(object item1, object item2, object item3, object item4)
		{
			RubyArray rubyArray = new RubyArray(4);
			rubyArray.Add(item1);
			rubyArray.Add(item2);
			rubyArray.Add(item3);
			rubyArray.Add(item4);
			return rubyArray;
		}

		public static RubyArray MakeArray5(object item1, object item2, object item3, object item4, object item5)
		{
			RubyArray rubyArray = new RubyArray(5);
			rubyArray.Add(item1);
			rubyArray.Add(item2);
			rubyArray.Add(item3);
			rubyArray.Add(item4);
			rubyArray.Add(item5);
			return rubyArray;
		}

		public static RubyArray MakeArrayN(object[] items)
		{
			RubyArray rubyArray = new RubyArray(items.Length);
			rubyArray.AddVector(items, 0, items.Length);
			return rubyArray;
		}

		public static Hash MakeHash0(RubyScope scope)
		{
			return new Hash(scope.RubyContext.EqualityComparer, 0);
		}

		public static Hash MakeHash(RubyScope scope, object[] items)
		{
			return RubyUtils.SetHashElements(scope.RubyContext, new Hash(scope.RubyContext.EqualityComparer, items.Length / 2), items);
		}

		public static RubyArray AddRange(RubyArray array, IList list)
		{
			return array.AddRange(list);
		}

		public static RubyArray AddSubRange(RubyArray result, IList array, int start, int count)
		{
			return result.AddRange(array, start, count);
		}

		public static RubyArray AddItem(RubyArray array, object item)
		{
			array.Add(item);
			return array;
		}

		public static IList SplatAppend(IList array, IList list)
		{
			Utils.AddRange(array, list);
			return array;
		}

		public static object Splat(IList list)
		{
			if (list.Count <= 1)
			{
				if (list.Count <= 0)
				{
					return null;
				}
				return list[0];
			}
			return list;
		}

		public static object SplatPair(object value, IList list)
		{
			if (list.Count == 0)
			{
				return value;
			}
			RubyArray rubyArray = new RubyArray(list.Count + 1);
			rubyArray.Add(value);
			rubyArray.AddRange(list);
			return rubyArray;
		}

		public static IList Unsplat(object splattee)
		{
			IList list = splattee as IList;
			if (list == null)
			{
				list = new RubyArray(1);
				list.Add(splattee);
			}
			return list;
		}

		public static bool ExistsUnsplatCompare(CallSite<Func<CallSite, object, object, object>> comparisonSite, object splattee, object value)
		{
			IList list = splattee as IList;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (IsTrue(comparisonSite.Target(comparisonSite, list[i], value)))
					{
						return true;
					}
				}
				return false;
			}
			return IsTrue(comparisonSite.Target(comparisonSite, splattee, value));
		}

		public static bool ExistsUnsplat(object splattee)
		{
			IList list = splattee as IList;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (IsTrue(list[i]))
					{
						return true;
					}
				}
				return false;
			}
			return IsTrue(splattee);
		}

		public static object GetArrayItem(IList array, int index)
		{
			if (index >= array.Count)
			{
				return null;
			}
			return array[index];
		}

		public static object GetTrailingArrayItem(IList array, int index, int explicitCount)
		{
			int num = Math.Max(array.Count, explicitCount) - index;
			if (num < 0)
			{
				return null;
			}
			return array[num];
		}

		public static RubyArray GetArrayRange(IList array, int startIndex, int explicitCount)
		{
			int num = array.Count - explicitCount;
			if (num > 0)
			{
				RubyArray rubyArray = new RubyArray(num);
				for (int i = 0; i < num; i++)
				{
					rubyArray.Add(array[startIndex + i]);
				}
				return rubyArray;
			}
			return new RubyArray();
		}

		[RubyConstructor]
		public static object CreateVector<TElement>(ConversionStorage<TElement> elementConversion, ConversionStorage<Union<IList, int>> toAryToInt, BlockParam block, RubyClass self, [NotNull] object arrayOrSize)
		{
			CallSite<Func<CallSite, object, Union<IList, int>>> site = toAryToInt.GetSite(CompositeConversionAction.Make(self.Context, CompositeConversion.ToAryToInt));
			Union<IList, int> union = site.Target(site, arrayOrSize);
			if (union.First != null)
			{
				return CreateVectorInternal(elementConversion, union.First);
			}
			if (block != null)
			{
				return PopulateVector(elementConversion, CreateVectorInternal<TElement>(union.Second), block);
			}
			return CreateVectorInternal<TElement>(union.Second);
		}

		[RubyConstructor]
		public static Array CreateVectorWithValues<TElement>(ConversionStorage<TElement> elementConversion, RubyClass self, [DefaultProtocol] int size, [DefaultProtocol] TElement value)
		{
			TElement[] array = CreateVectorInternal<TElement>(size);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = value;
			}
			return array;
		}

		private static TElement[] CreateVectorInternal<TElement>(int size)
		{
			if (size < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative array size");
			}
			return new TElement[size];
		}

		private static Array CreateVectorInternal<TElement>(ConversionStorage<TElement> elementConversion, IList list)
		{
			CallSite<Func<CallSite, object, TElement>> defaultConversionSite = elementConversion.GetDefaultConversionSite();
			TElement[] array = new TElement[list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				object obj = list[i];
				array[i] = ((obj is TElement) ? ((TElement)obj) : defaultConversionSite.Target(defaultConversionSite, obj));
			}
			return array;
		}

		private static object PopulateVector<TElement>(ConversionStorage<TElement> elementConversion, TElement[] array, BlockParam block)
		{
			CallSite<Func<CallSite, object, TElement>> defaultConversionSite = elementConversion.GetDefaultConversionSite();
			for (int i = 0; i < array.Length; i++)
			{
				object blockResult;
				if (block.Yield(i, out blockResult))
				{
					return blockResult;
				}
				array[i] = defaultConversionSite.Target(defaultConversionSite, blockResult);
			}
			return array;
		}

		public static object GetGlobalVariable(RubyScope scope, string name)
		{
			object value;
			scope.RubyContext.TryGetGlobalVariable(scope, name, out value);
			return value;
		}

		public static bool IsDefinedGlobalVariable(RubyScope scope, string name)
		{
			GlobalVariable variable;
			if (scope.RubyContext.TryGetGlobalVariable(name, out variable))
			{
				return variable.IsDefined;
			}
			return false;
		}

		public static object SetGlobalVariable(object value, RubyScope scope, string name)
		{
			scope.RubyContext.SetGlobalVariable(scope, name, value);
			return value;
		}

		public static void AliasGlobalVariable(RubyScope scope, string newName, string oldName)
		{
			scope.RubyContext.AliasGlobalVariable(newName, oldName);
		}

		internal static bool TryGetGlobalScopeConstant(RubyContext context, Scope scope, string name, out object value)
		{
			ScopeStorage scopeStorage = scope.Storage as ScopeStorage;
			if (scopeStorage != null)
			{
				if (!scopeStorage.TryGetValue(name, false, out value))
				{
					string name2;
					if ((name2 = RubyUtils.TryMangleName(name)) != null)
					{
						return scopeStorage.TryGetValue(name2, false, out value);
					}
					return false;
				}
				return true;
			}
			if (!context.Operations.TryGetMember(scope, name, out value))
			{
				string name2;
				if ((name2 = RubyUtils.TryMangleName(name)) != null)
				{
					return context.Operations.TryGetMember(scope, name2, out value);
				}
				return false;
			}
			return true;
		}

		internal static void ScopeSetMember(Scope scope, string name, object value)
		{
			object storage = scope.Storage;
			ScopeStorage scopeStorage = storage as ScopeStorage;
			if (scopeStorage != null)
			{
				scopeStorage.SetValue(name, false, value);
				return;
			}
			StringDictionaryExpando stringDictionaryExpando = storage as StringDictionaryExpando;
			if (stringDictionaryExpando != null)
			{
				stringDictionaryExpando.Dictionary[name] = value;
				return;
			}
			throw new NotImplementedException();
		}

		internal static bool ScopeContainsMember(Scope scope, string name)
		{
			object storage = scope.Storage;
			ScopeStorage scopeStorage = storage as ScopeStorage;
			if (scopeStorage != null)
			{
				return scopeStorage.HasValue(name, false);
			}
			StringDictionaryExpando stringDictionaryExpando = storage as StringDictionaryExpando;
			if (stringDictionaryExpando != null)
			{
				return stringDictionaryExpando.Dictionary.ContainsKey(name);
			}
			throw new NotImplementedException();
		}

		internal static bool ScopeDeleteMember(Scope scope, string name)
		{
			object storage = scope.Storage;
			ScopeStorage scopeStorage = storage as ScopeStorage;
			if (scopeStorage != null)
			{
				return scopeStorage.DeleteValue(name, false);
			}
			StringDictionaryExpando stringDictionaryExpando = storage as StringDictionaryExpando;
			if (stringDictionaryExpando != null)
			{
				return stringDictionaryExpando.Dictionary.Remove(name);
			}
			throw new NotImplementedException();
		}

		internal static IList<KeyValuePair<string, object>> ScopeGetItems(Scope scope)
		{
			object storage = scope.Storage;
			ScopeStorage scopeStorage = storage as ScopeStorage;
			if (scopeStorage != null)
			{
				return scopeStorage.GetItems();
			}
			StringDictionaryExpando stringDictionaryExpando = storage as StringDictionaryExpando;
			if (stringDictionaryExpando != null)
			{
				KeyValuePair<string, object>[] array = new KeyValuePair<string, object>[stringDictionaryExpando.Dictionary.Count];
				int num = 0;
				{
					foreach (KeyValuePair<string, object> item in stringDictionaryExpando.Dictionary)
					{
						array[num++] = item;
					}
					return array;
				}
			}
			throw new NotImplementedException();
		}

		public static MutableString GetCurrentMatchGroup(RubyScope scope, int index)
		{
			return scope.GetInnerMostClosureScope().GetCurrentMatchGroup(index);
		}

		public static MatchData GetCurrentMatchData(RubyScope scope)
		{
			return scope.GetInnerMostClosureScope().CurrentMatch;
		}

		public static MutableString GetCurrentMatchLastGroup(RubyScope scope)
		{
			return scope.GetInnerMostClosureScope().GetCurrentMatchLastGroup();
		}

		public static MutableString GetCurrentPreMatch(RubyScope scope)
		{
			return scope.GetInnerMostClosureScope().GetCurrentPreMatch();
		}

		public static MutableString GetCurrentPostMatch(RubyScope scope)
		{
			return scope.GetInnerMostClosureScope().GetCurrentPostMatch();
		}

		public static bool MatchLastInputLine(RubyRegex regex, RubyScope scope)
		{
			MutableString mutableString = scope.GetInnerMostClosureScope().LastInputLine as MutableString;
			if (mutableString == null)
			{
				return false;
			}
			return RubyRegex.SetCurrentMatchData(scope, regex, mutableString) != null;
		}

		public static object MatchString(MutableString str, RubyRegex regex, RubyScope scope)
		{
			MatchData matchData = RubyRegex.SetCurrentMatchData(scope, regex, str);
			if (matchData == null)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(matchData.Index);
		}

		private static RubyRegex CreateRegexWorker(RubyRegexOptions options, StrongBox<RubyRegex> regexpCache, bool isLiteralWithoutSubstitutions, Func<RubyRegex> createRegex)
		{
			try
			{
				if ((options & RubyRegexOptions.Once) == RubyRegexOptions.Once || isLiteralWithoutSubstitutions)
				{
					if (regexpCache.Value == null)
					{
						regexpCache.Value = createRegex();
					}
					return regexpCache.Value;
				}
				return createRegex();
			}
			catch (RegexpError regexpError)
			{
				if (isLiteralWithoutSubstitutions)
				{
					throw new SyntaxError(regexpError.Message);
				}
				throw;
			}
		}

		public static RubyRegex CreateRegexB(byte[] bytes, RubyEncoding encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache)
		{
			Func<RubyRegex> createRegex = () => new RubyRegex(CreateMutableStringB(bytes, encoding), options);
			return CreateRegexWorker(options, regexpCache, true, createRegex);
		}

		public static RubyRegex CreateRegexL(string str1, RubyEncoding encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache)
		{
			Func<RubyRegex> createRegex = () => new RubyRegex(CreateMutableStringL(str1, encoding), options);
			return CreateRegexWorker(options, regexpCache, true, createRegex);
		}

		public static RubyRegex CreateRegexM(MutableString str1, RubyEncoding encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache)
		{
			Func<RubyRegex> createRegex = () => new RubyRegex(CreateMutableStringM(str1, encoding), options);
			return CreateRegexWorker(options, regexpCache, false, createRegex);
		}

		public static RubyRegex CreateRegexLM(string str1, MutableString str2, RubyEncoding encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache)
		{
			Func<RubyRegex> createRegex = () => new RubyRegex(CreateMutableStringLM(str1, str2, encoding), options);
			return CreateRegexWorker(options, regexpCache, false, createRegex);
		}

		public static RubyRegex CreateRegexML(MutableString str1, string str2, RubyEncoding encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache)
		{
			Func<RubyRegex> createRegex = () => new RubyRegex(CreateMutableStringML(str1, str2, encoding), options);
			return CreateRegexWorker(options, regexpCache, false, createRegex);
		}

		public static RubyRegex CreateRegexMM(MutableString str1, MutableString str2, RubyEncoding encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache)
		{
			Func<RubyRegex> createRegex = () => new RubyRegex(CreateMutableStringMM(str1, str2, encoding), options);
			return CreateRegexWorker(options, regexpCache, false, createRegex);
		}

		public static RubyRegex CreateRegexN(MutableString[] strings, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache)
		{
			Func<RubyRegex> createRegex = () => new RubyRegex(CreateMutableStringN(strings), options);
			return CreateRegexWorker(options, regexpCache, false, createRegex);
		}

		public static MutableString CreateMutableStringB(byte[] bytes, RubyEncoding encoding)
		{
			return MutableString.CreateBinary(bytes, encoding);
		}

		public static MutableString CreateMutableStringL(string str1, RubyEncoding encoding)
		{
			return MutableString.Create(str1, encoding);
		}

		public static MutableString CreateMutableStringM(MutableString str1, RubyEncoding encoding)
		{
			return MutableString.CreateInternal(str1, encoding);
		}

		public static MutableString CreateMutableStringLM(string str1, MutableString str2, RubyEncoding encoding)
		{
			return MutableString.CreateMutable(str1, encoding).Append(str2);
		}

		public static MutableString CreateMutableStringML(MutableString str1, string str2, RubyEncoding encoding)
		{
			return MutableString.CreateInternal(str1, encoding).Append(str2);
		}

		public static MutableString CreateMutableStringMM(MutableString str1, MutableString str2, RubyEncoding encoding)
		{
			return MutableString.CreateInternal(str1, encoding).Append(str2);
		}

		public static MutableString CreateMutableStringN(MutableString[] parts)
		{
			MutableString mutableString = MutableString.CreateMutable(RubyEncoding.Ascii);
			for (int i = 0; i < parts.Length; i++)
			{
				mutableString.Append(parts[i]);
			}
			return mutableString;
		}

		public static RubySymbol CreateSymbolM(MutableString str1, RubyEncoding encoding, RubyScope scope)
		{
			return scope.RubyContext.CreateSymbol(CreateMutableStringM(str1, encoding), false);
		}

		public static RubySymbol CreateSymbolLM(string str1, MutableString str2, RubyEncoding encoding, RubyScope scope)
		{
			return scope.RubyContext.CreateSymbol(CreateMutableStringLM(str1, str2, encoding), false);
		}

		public static RubySymbol CreateSymbolML(MutableString str1, string str2, RubyEncoding encoding, RubyScope scope)
		{
			return scope.RubyContext.CreateSymbol(CreateMutableStringML(str1, str2, encoding), false);
		}

		public static RubySymbol CreateSymbolMM(MutableString str1, MutableString str2, RubyEncoding encoding, RubyScope scope)
		{
			return scope.RubyContext.CreateSymbol(CreateMutableStringMM(str1, str2, encoding), false);
		}

		public static RubySymbol CreateSymbolN(MutableString[] strings, RubyScope scope)
		{
			return scope.RubyContext.CreateSymbol(CreateMutableStringN(strings), false);
		}

		public static RubyEncoding CreateEncoding(int codepage)
		{
			return RubyEncoding.GetRubyEncoding(codepage);
		}

		[Obsolete("Internal only")]
		public static byte[] GetMutableStringBytes(MutableString str)
		{
			int count;
			return str.GetByteArray(out count);
		}

		public static bool IsTrue(object obj)
		{
			if (!(obj is bool))
			{
				return obj != null;
			}
			return (bool)obj;
		}

		public static bool IsFalse(object obj)
		{
			if (!(obj is bool))
			{
				return obj == null;
			}
			return !(bool)obj;
		}

		public static object NullIfFalse(object obj)
		{
			if (!(obj is bool) || (bool)obj)
			{
				return obj;
			}
			return null;
		}

		public static object NullIfTrue(object obj)
		{
			if ((!(obj is bool) || (bool)obj) && obj != null)
			{
				return null;
			}
			return DefaultArgument;
		}

		public static bool FilterBlockException(RubyScope scope, Exception exception)
		{
			RubyExceptionData.GetInstance(exception).CaptureExceptionTrace(scope);
			return false;
		}

		public static bool TraceTopLevelCodeFrame(RubyScope scope, Exception exception)
		{
			RubyExceptionData.GetInstance(exception).CaptureExceptionTrace(scope);
			return false;
		}

		public static bool IsMethodUnwinderTargetFrame(RubyScope scope, Exception exception)
		{
			MethodUnwinder methodUnwinder = exception as MethodUnwinder;
			if (methodUnwinder == null)
			{
				RubyExceptionData.GetInstance(exception).CaptureExceptionTrace(scope);
				return false;
			}
			return methodUnwinder.TargetFrame == scope.FlowControlScope;
		}

		public static object GetMethodUnwinderReturnValue(Exception exception)
		{
			return ((MethodUnwinder)exception).ReturnValue;
		}

		public static void LeaveMethodFrame(RuntimeFlowControl rfc)
		{
			rfc.LeaveMethod();
		}

		public static bool CanRescue(RubyScope scope, Exception exception)
		{
			if (exception is StackUnwinder)
			{
				return false;
			}
			LocalJumpError localJumpError = exception as LocalJumpError;
			if (localJumpError != null && localJumpError.SkipFrame == scope.FlowControlScope)
			{
				return false;
			}
			exception = RubyExceptionData.HandleException(scope.RubyContext, exception);
			scope.RubyContext.CurrentException = exception;
			RubyExceptionData.GetInstance(exception).CaptureExceptionTrace(scope);
			return true;
		}

		public static Exception MarkException(Exception exception)
		{
			RubyExceptionData.GetInstance(exception).Handled = true;
			return exception;
		}

		public static Exception GetCurrentException(RubyScope scope)
		{
			return scope.RubyContext.CurrentException;
		}

		public static void SetCurrentException(RubyScope scope, Exception exception)
		{
			scope.RubyContext.CurrentException = exception;
		}

		public static bool CompareException(BinaryOpStorage comparisonStorage, RubyScope scope, object classObject)
		{
			RubyContext rubyContext = scope.RubyContext;
			CallSite<Func<CallSite, object, object, object>> callSite = comparisonStorage.GetCallSite("===");
			bool flag = IsTrue(callSite.Target(callSite, classObject, rubyContext.CurrentException));
			if (flag)
			{
				RubyExceptionData.ActiveExceptionHandled(rubyContext.CurrentException);
			}
			return flag;
		}

		public static bool CompareSplattedExceptions(BinaryOpStorage comparisonStorage, RubyScope scope, IList classObjects)
		{
			for (int i = 0; i < classObjects.Count; i++)
			{
				if (CompareException(comparisonStorage, scope, classObjects[i]))
				{
					return true;
				}
			}
			return false;
		}

		public static bool CompareDefaultException(RubyScope scope)
		{
			RubyContext rubyContext = scope.RubyContext;
			bool flag = rubyContext.IsInstanceOf(rubyContext.CurrentException, rubyContext.StandardErrorClass);
			if (flag)
			{
				RubyExceptionData.ActiveExceptionHandled(rubyContext.CurrentException);
			}
			return flag;
		}

		public static string GetDefaultExceptionMessage(RubyClass exceptionClass)
		{
			return exceptionClass.Name;
		}

		public static ArgumentException CreateArgumentsError(string message)
		{
			return (ArgumentException)RubyExceptions.CreateArgumentError(message);
		}

		public static ArgumentException CreateArgumentsErrorForMissingBlock()
		{
			return (ArgumentException)RubyExceptions.CreateArgumentError("block not supplied");
		}

		public static ArgumentException CreateArgumentsErrorForProc(string className)
		{
			return (ArgumentException)RubyExceptions.CreateArgumentError(string.Format("wrong type argument {0} (should be callable)", className));
		}

		public static ArgumentException MakeWrongNumberOfArgumentsError(int actual, int expected)
		{
			return new ArgumentException(string.Format("wrong number of arguments ({0} for {1})", actual, expected));
		}

		public static Exception MakeTopLevelSuperException()
		{
			return new MissingMethodException("super called outside of method");
		}

		public static Exception MakeMissingSuperException(string name)
		{
			return new MissingMethodException(string.Format("super: no superclass method `{0}'", name));
		}

		public static Exception MakeVirtualClassInstantiatedError()
		{
			return RubyExceptions.CreateTypeError("can't create instance of virtual class");
		}

		public static Exception MakeAbstractMethodCalledError(RuntimeMethodHandle method)
		{
			return new NotImplementedException(string.Format("Abstract method `{0}' not implemented", MethodBase.GetMethodFromHandle(method)));
		}

		public static Exception MakeInvalidArgumentTypesError(string methodName)
		{
			return new ArgumentException(string.Format("wrong number or type of arguments for `{0}'", methodName));
		}

		public static Exception MakeTypeConversionError(RubyContext context, object value, Type type)
		{
			return RubyExceptions.CreateTypeConversionError(context.GetClassDisplayName(value), context.GetTypeName(type, true));
		}

		public static Exception MakeAmbiguousMatchError(string message)
		{
			return new AmbiguousMatchException(message);
		}

		public static Exception MakeAllocatorUndefinedError(RubyClass classObj)
		{
			return RubyExceptions.CreateAllocatorUndefinedError(classObj);
		}

		public static Exception MakeNotClrTypeError(RubyClass classObj)
		{
			return RubyExceptions.CreateNotClrTypeError(classObj);
		}

		public static Exception MakeConstructorUndefinedError(RubyClass classObj)
		{
			return RubyExceptions.CreateTypeError(string.Format("`{0}' doesn't have a visible CLR constructor", classObj.Context.GetTypeName(classObj.TypeTracker.Type, true)));
		}

		public static Exception MakeMissingDefaultConstructorError(RubyClass classObj, string initializerOwnerName)
		{
			return RubyExceptions.CreateMissingDefaultConstructorError(classObj, initializerOwnerName);
		}

		public static Exception MakePrivateMethodCalledError(RubyContext context, object target, string methodName)
		{
			return RubyExceptions.CreatePrivateMethodCalled(context, target, methodName);
		}

		public static Exception MakeProtectedMethodCalledError(RubyContext context, object target, string methodName)
		{
			return RubyExceptions.CreateProtectedMethodCalled(context, target, methodName);
		}

		public static Exception MakeClrProtectedMethodCalledError(RubyContext context, object target, string methodName)
		{
			return new MissingMethodException(RubyExceptions.FormatMethodMissingMessage(context, target, methodName, "CLR protected method `{0}' called for {1}; CLR protected methods can only be called with a receiver whose class is a Ruby subclass of the class declaring the method"));
		}

		public static Exception MakeClrVirtualMethodCalledError(RubyContext context, object target, string methodName)
		{
			return new MissingMethodException(RubyExceptions.FormatMethodMissingMessage(context, target, methodName, "Virtual CLR method `{0}' called via super from {1}; Super calls to virtual CLR methods can only be used in a Ruby subclass of the class declaring the method"));
		}

		public static Exception MakeImplicitSuperInBlockMethodError()
		{
			return RubyExceptions.CreateRuntimeError("implicit argument passing of super from method defined by define_method() is not supported. Specify all arguments explicitly.");
		}

		public static Exception MakeMissingMethodError(RubyContext context, object self, string methodName)
		{
			return RubyExceptions.CreateMethodMissing(context, self, methodName);
		}

		public static Exception MakeMissingMemberError(string memberName)
		{
			return new MissingMemberException(string.Format(CultureInfo.InvariantCulture, "undefined member: `{0}'", new object[1] { memberName }));
		}

		public static Range CreateInclusiveRange(object begin, object end, RubyScope scope, BinaryOpStorage comparisonStorage)
		{
			return new Range(comparisonStorage, scope.RubyContext, begin, end, false);
		}

		public static Range CreateExclusiveRange(object begin, object end, RubyScope scope, BinaryOpStorage comparisonStorage)
		{
			return new Range(comparisonStorage, scope.RubyContext, begin, end, true);
		}

		public static Range CreateInclusiveIntegerRange(int begin, int end)
		{
			return new Range(begin, end, false);
		}

		public static Range CreateExclusiveIntegerRange(int begin, int end)
		{
			return new Range(begin, end, true);
		}

		public static RubyStruct AllocateStructInstance(RubyClass self)
		{
			return RubyStruct.Create(self);
		}

		public static RubyStruct CreateStructInstance(RubyClass self, [NotNull] params object[] items)
		{
			RubyStruct rubyStruct = RubyStruct.Create(self);
			rubyStruct.SetValues(items);
			return rubyStruct;
		}

		public static DynamicMetaObject GetMetaObject(IRubyObject obj, Expression parameter)
		{
			return new RubyObject.Meta(parameter, BindingRestrictions.Empty, obj);
		}

		public static RubyMethod CreateBoundMember(object target, RubyMemberInfo info, string name)
		{
			return new RubyMethod(target, info, name);
		}

		public static RubyMethod CreateBoundMissingMember(object target, RubyMemberInfo info, string name)
		{
			return new RubyMethod.Curried(target, info, name);
		}

		public static bool IsClrSingletonRuleValid(RubyContext context, object target, int expectedVersion)
		{
			RubyInstanceData result;
			RubyClass immediateClass;
			if (context.TryGetClrTypeInstanceData(target, out result) && (immediateClass = result.ImmediateClass) != null && immediateClass.IsSingletonClass)
			{
				return immediateClass.Version.Method == expectedVersion;
			}
			return false;
		}

		public static bool IsClrNonSingletonRuleValid(RubyContext context, object target, VersionHandle versionHandle, int expectedVersion)
		{
			if (versionHandle.Method == expectedVersion)
			{
				RubyInstanceData result;
				RubyClass immediateClass;
				if (context.TryGetClrTypeInstanceData(target, out result) && (immediateClass = result.ImmediateClass) != null)
				{
					return !immediateClass.IsSingletonClass;
				}
				return true;
			}
			return false;
		}

		public static object GetSuperCallTarget(RubyScope scope, int targetId)
		{
			while (true)
			{
				switch (scope.Kind)
				{
				case ScopeKind.Method:
					if (targetId != 0)
					{
						return NeedsUpdate;
					}
					return scope.SelfObject;
				case ScopeKind.BlockMethod:
					if (targetId != ((RubyBlockScope)scope).BlockFlowControl.Proc.Method.Id)
					{
						return NeedsUpdate;
					}
					return scope.SelfObject;
				case ScopeKind.TopLevel:
					throw Assert.Unreachable;
				}
				scope = scope.Parent;
			}
		}

		public static bool IsSuperOutOfMethodScope(RubyScope scope)
		{
			while (true)
			{
				switch (scope.Kind)
				{
				case ScopeKind.Method:
				case ScopeKind.BlockMethod:
					return false;
				case ScopeKind.TopLevel:
					return true;
				}
				scope = scope.Parent;
			}
		}

		public static Proc ToProcValidator(string className, object obj)
		{
			Proc proc = obj as Proc;
			if (proc == null)
			{
				throw RubyExceptions.CreateReturnTypeError(className, "to_proc", "Proc");
			}
			return proc;
		}

		public static MutableString StringToMutableString(string str)
		{
			return MutableString.Create(str, RubyEncoding.UTF8);
		}

		public static MutableString ObjectToMutableString(object value)
		{
			if (value == null)
			{
				return MutableString.FrozenEmpty;
			}
			return MutableString.Create(value.ToString(), RubyEncoding.UTF8);
		}

		public static MutableString ToStringValidator(string className, object obj)
		{
			MutableString mutableString = obj as MutableString;
			if (mutableString == null)
			{
				throw RubyExceptions.CreateReturnTypeError(className, "to_str", "String");
			}
			return mutableString;
		}

		public static string ToSymbolValidator(string className, object obj)
		{
			MutableString mutableString = obj as MutableString;
			if (mutableString == null)
			{
				throw RubyExceptions.CreateReturnTypeError(className, "to_str", "String");
			}
			return mutableString.ConvertToString();
		}

		public static string ConvertSymbolToClrString(RubySymbol value)
		{
			return value.ToString();
		}

		public static string ConvertRubySymbolToClrString(RubyContext context, int value)
		{
			context.ReportWarning("do not use Fixnums as Symbols");
			RubySymbol rubySymbol = context.FindSymbol(value);
			if (rubySymbol != null)
			{
				return rubySymbol.ToString();
			}
			throw RubyExceptions.CreateArgumentError(string.Format("{0} is not a symbol", value));
		}

		public static string ConvertMutableStringToClrString(MutableString value)
		{
			return value.ConvertToString();
		}

		public static MutableString ConvertSymbolToMutableString(RubySymbol value)
		{
			return value.String.Clone();
		}

		public static RubyRegex ToRegexValidator(string className, object obj)
		{
			return new RubyRegex(RubyRegex.Escape(ToStringValidator(className, obj)), RubyRegexOptions.NONE);
		}

		public static IList ToArrayValidator(string className, object obj)
		{
			IList list = obj as IList;
			if (list == null)
			{
				throw RubyExceptions.CreateReturnTypeError(className, "to_ary", "Array");
			}
			return list;
		}

		public static IList ToAValidator(string className, object obj)
		{
			IList list = obj as IList;
			if (list == null)
			{
				throw RubyExceptions.CreateReturnTypeError(className, "to_a", "Array");
			}
			return list;
		}

		public static IDictionary<object, object> ToHashValidator(string className, object obj)
		{
			IDictionary<object, object> dictionary = obj as IDictionary<object, object>;
			if (dictionary == null)
			{
				throw RubyExceptions.CreateReturnTypeError(className, "to_hash", "Hash");
			}
			return dictionary;
		}

		private static int ToIntValidator(string className, string targetType, object obj)
		{
			if (obj is int)
			{
				return (int)obj;
			}
			BigInteger bigInteger = obj as BigInteger;
			if ((object)bigInteger != null)
			{
				int ret;
				if (bigInteger.AsInt32(out ret))
				{
					return ret;
				}
				throw RubyExceptions.CreateRangeError("bignum too big to convert into {0}", targetType);
			}
			throw RubyExceptions.CreateReturnTypeError(className, "to_int", "Integer");
		}

		public static int ToFixnumValidator(string className, object obj)
		{
			return ToIntValidator(className, "Fixnum", obj);
		}

		public static byte ToByteValidator(string className, object obj)
		{
			return Converter.ToByte(ToIntValidator(className, "System::Byte", obj));
		}

		public static sbyte ToSByteValidator(string className, object obj)
		{
			return Converter.ToSByte(ToIntValidator(className, "System::SByte", obj));
		}

		public static short ToInt16Validator(string className, object obj)
		{
			return Converter.ToInt16(ToIntValidator(className, "System::Int16", obj));
		}

		public static ushort ToUInt16Validator(string className, object obj)
		{
			return Converter.ToUInt16(ToIntValidator(className, "System::UInt16", obj));
		}

		public static uint ToUInt32Validator(string className, object obj)
		{
			if (obj is int)
			{
				return Converter.ToUInt32((int)obj);
			}
			BigInteger bigInteger = obj as BigInteger;
			if ((object)bigInteger != null)
			{
				return Converter.ToUInt32(bigInteger);
			}
			throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
		}

		public static long ToInt64Validator(string className, object obj)
		{
			if (obj is int)
			{
				return (int)obj;
			}
			BigInteger bigInteger = obj as BigInteger;
			if ((object)bigInteger != null)
			{
				return Converter.ToInt64(bigInteger);
			}
			throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
		}

		public static ulong ToUInt64Validator(string className, object obj)
		{
			if (obj is int)
			{
				return Converter.ToUInt64((int)obj);
			}
			BigInteger bigInteger = obj as BigInteger;
			if ((object)bigInteger != null)
			{
				return Converter.ToUInt64(bigInteger);
			}
			throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
		}

		public static BigInteger ToBignumValidator(string className, object obj)
		{
			if (obj is int)
			{
				return (int)obj;
			}
			BigInteger bigInteger = obj as BigInteger;
			if ((object)bigInteger != null)
			{
				return bigInteger;
			}
			throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
		}

		public static IntegerValue ToIntegerValidator(string className, object obj)
		{
			if (obj is int)
			{
				return new IntegerValue((int)obj);
			}
			BigInteger bigInteger = obj as BigInteger;
			if ((object)bigInteger != null)
			{
				return new IntegerValue(bigInteger);
			}
			throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
		}

		public static double ToDoubleValidator(string className, object obj)
		{
			if (obj is double)
			{
				return (double)obj;
			}
			if (obj is float)
			{
				return (float)obj;
			}
			throw RubyExceptions.CreateReturnTypeError(className, "to_f", "Float");
		}

		public static float ToSingleValidator(string className, object obj)
		{
			if (obj is double)
			{
				return (float)(double)obj;
			}
			if (obj is float)
			{
				return (float)obj;
			}
			throw RubyExceptions.CreateReturnTypeError(className, "to_f", "System::Single");
		}

		public static double ConvertBignumToFloat(BigInteger value)
		{
			double result;
			//if (!value.TryToFloat64(out result))
			if (!BigIntegerExtensions.TryToFloat64(value, out result))
			{
				if (!value.IsNegative())
				{
					return double.PositiveInfinity;
				}
				return double.NegativeInfinity;
			}
			return result;
		}

		public static double ConvertMutableStringToFloat(RubyContext context, MutableString value)
		{
			return ConvertStringToFloat(context, value.ConvertToString());
		}

		public static double ConvertStringToFloat(RubyContext context, string value)
		{
			double result;
			bool complete;
			if (Tokenizer.TryParseDouble(value, out result, out complete) && complete)
			{
				return result;
			}
			throw RubyExceptions.InvalidValueForType(context, value, "Float");
		}

		public static Exception CreateTypeConversionError(string fromType, string toType)
		{
			return RubyExceptions.CreateTypeConversionError(fromType, toType);
		}

		public static int ConvertBignumToFixnum(BigInteger bignum)
		{
			int ret;
			if (bignum.AsInt32(out ret))
			{
				return ret;
			}
			throw RubyExceptions.CreateRangeError("bignum too big to convert into Fixnum");
		}

		public static int ConvertDoubleToFixnum(double value)
		{
			try
			{
				return checked((int)value);
			}
			catch (OverflowException)
			{
				throw RubyExceptions.CreateRangeError(string.Format("float {0} out of range of Fixnum", value));
			}
		}

		public static MutableString ToSDefaultConversion(RubyContext context, object target, object converted)
		{
			return (converted as MutableString) ?? RubyUtils.ObjectToMutableString(context, target);
		}

		public static object GetInstanceVariable(RubyScope scope, object self, string name)
		{
			RubyInstanceData rubyInstanceData = scope.RubyContext.TryGetInstanceData(self);
			if (rubyInstanceData == null)
			{
				return null;
			}
			return rubyInstanceData.GetInstanceVariable(name);
		}

		public static bool IsDefinedInstanceVariable(RubyScope scope, object self, string name)
		{
			RubyInstanceData rubyInstanceData = scope.RubyContext.TryGetInstanceData(self);
			if (rubyInstanceData == null)
			{
				return false;
			}
			object value;
			return rubyInstanceData.TryGetInstanceVariable(name, out value);
		}

		public static object SetInstanceVariable(object self, object value, RubyScope scope, string name)
		{
			scope.RubyContext.SetInstanceVariable(self, name, value);
			return value;
		}

		public static object GetClassVariable(RubyScope scope, string name)
		{
			RubyModule innerMostModuleForClassVariableLookup = scope.GetInnerMostModuleForClassVariableLookup();
			return GetClassVariableInternal(innerMostModuleForClassVariableLookup, name);
		}

		private static object GetClassVariableInternal(RubyModule module, string name)
		{
			object value;
			if (module.TryResolveClassVariable(name, out value) == null)
			{
				throw RubyExceptions.CreateNameError(string.Format("uninitialized class variable {0} in {1}", name, module.Name));
			}
			return value;
		}

		public static object TryGetClassVariable(RubyScope scope, string name)
		{
			object value;
			scope.GetInnerMostModuleForClassVariableLookup().TryResolveClassVariable(name, out value);
			return value;
		}

		public static bool IsDefinedClassVariable(RubyScope scope, string name)
		{
			RubyModule innerMostModuleForClassVariableLookup = scope.GetInnerMostModuleForClassVariableLookup();
			object value;
			return innerMostModuleForClassVariableLookup.TryResolveClassVariable(name, out value) != null;
		}

		public static object SetClassVariable(object value, RubyScope scope, string name)
		{
			return SetClassVariableInternal(scope.GetInnerMostModuleForClassVariableLookup(), name, value);
		}

		private static object SetClassVariableInternal(RubyModule lexicalOwner, string name, object value)
		{
			object value2;
			RubyModule rubyModule = lexicalOwner.TryResolveClassVariable(name, out value2);
			(rubyModule ?? lexicalOwner).SetClassVariable(name, value);
			return value;
		}

		public static string ObjectToString(IRubyObject obj)
		{
			return RubyUtils.ObjectToMutableString(obj).ToString();
		}

		public static RubyInstanceData GetInstanceData(ref RubyInstanceData instanceData)
		{
			if (instanceData == null)
			{
				Interlocked.CompareExchange(ref instanceData, new RubyInstanceData(), null);
			}
			return instanceData;
		}

		public static bool IsObjectFrozen(RubyInstanceData instanceData)
		{
			if (instanceData != null)
			{
				return instanceData.IsFrozen;
			}
			return false;
		}

		public static bool IsObjectTainted(RubyInstanceData instanceData)
		{
			if (instanceData != null)
			{
				return instanceData.IsTainted;
			}
			return false;
		}

		public static bool IsObjectUntrusted(RubyInstanceData instanceData)
		{
			if (instanceData != null)
			{
				return instanceData.IsUntrusted;
			}
			return false;
		}

		public static void FreezeObject(ref RubyInstanceData instanceData)
		{
			GetInstanceData(ref instanceData).Freeze();
		}

		public static void SetObjectTaint(ref RubyInstanceData instanceData, bool value)
		{
			GetInstanceData(ref instanceData).IsTainted = value;
		}

		public static void SetObjectTrustiness(ref RubyInstanceData instanceData, bool untrusted)
		{
			GetInstanceData(ref instanceData).IsUntrusted = untrusted;
		}

		public static void DeserializeObject(out RubyInstanceData instanceData, out RubyClass immediateClass, SerializationInfo info)
		{
			immediateClass = (RubyClass)info.GetValue(RubyUtils.SerializationInfoClassKey, typeof(RubyClass));
			RubyInstanceData rubyInstanceData = null;
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				if (current.Name.StartsWith("@", StringComparison.Ordinal))
				{
					if (rubyInstanceData == null)
					{
						rubyInstanceData = new RubyInstanceData();
					}
					rubyInstanceData.SetInstanceVariable(current.Name, current.Value);
				}
			}
			instanceData = rubyInstanceData;
		}

		public static void SerializeObject(RubyInstanceData instanceData, RubyClass immediateClass, SerializationInfo info)
		{
			info.AddValue(RubyUtils.SerializationInfoClassKey, immediateClass, typeof(RubyClass));
			if (instanceData == null)
			{
				return;
			}
			string[] instanceVariableNames = instanceData.GetInstanceVariableNames();
			string[] array = instanceVariableNames;
			foreach (string name in array)
			{
				object value;
				if (!instanceData.TryGetInstanceVariable(name, out value))
				{
					value = null;
				}
				info.AddValue(name, value, typeof(object));
			}
		}

		public static Proc HookupEvent(RubyEventInfo eventInfo, object target, Proc proc)
		{
			eventInfo.Tracker.AddHandler(target, proc, eventInfo.Context.DelegateCreator);
			return proc;
		}

		public static RubyEvent CreateEvent(RubyEventInfo eventInfo, object target, string name)
		{
			return new RubyEvent(target, eventInfo, name);
		}

		public static Delegate CreateDelegateFromProc(Type type, Proc proc)
		{
			if (proc == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			BlockParam callableObject = CreateBfcForProcCall(proc);
			return proc.LocalScope.RubyContext.DelegateCreator.GetDelegate(callableObject, type);
		}

		public static Delegate CreateDelegateFromMethod(Type type, RubyMethod method)
		{
			return method.Info.Context.DelegateCreator.GetDelegate(method, type);
		}

		internal static Type MakeObjectTupleType(int fieldCount)
		{
			if (fieldCount <= 128)
			{
				if (fieldCount <= 1)
				{
					return typeof(MutableTuple<object>);
				}
				if (fieldCount <= 2)
				{
					return typeof(MutableTuple<object, object>);
				}
				if (fieldCount <= 4)
				{
					return typeof(MutableTuple<object, object, object, object>);
				}
				if (fieldCount <= 8)
				{
					return typeof(MutableTuple<object, object, object, object, object, object, object, object>);
				}
				if (fieldCount <= 16)
				{
					return typeof(MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
				}
				if (fieldCount <= 32)
				{
					return typeof(MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
				}
				if (fieldCount <= 64)
				{
					return typeof(MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
				}
				return MakeObjectTupleType128();
			}
			Type[] array = new Type[fieldCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = typeof(object);
			}
			return MutableTuple.MakeTupleType(array);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static Type MakeObjectTupleType128()
		{
			return typeof(MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
		}

		internal static MutableTuple CreateObjectTuple(int fieldCount)
		{
			if (fieldCount <= 1)
			{
				return new MutableTuple<object>();
			}
			if (fieldCount <= 2)
			{
				return new MutableTuple<object, object>();
			}
			if (fieldCount <= 4)
			{
				return new MutableTuple<object, object, object, object>();
			}
			if (fieldCount <= 8)
			{
				return new MutableTuple<object, object, object, object, object, object, object, object>();
			}
			if (fieldCount <= 16)
			{
				return new MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>();
			}
			if (fieldCount <= 32)
			{
				return new MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>();
			}
			if (fieldCount <= 64)
			{
				return new MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>();
			}
			return CreateObjectTuple128();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static MutableTuple CreateObjectTuple128()
		{
			return new MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>();
		}

		public static void X(string marker)
		{
		}

		public static object CreateDefaultInstance()
		{
			return null;
		}

		public static void UpdateProfileTicks(int index, long entryStamp)
		{
			Interlocked.Add(ref Profiler._ProfileTicks[index], Stopwatch.GetTimestamp() - entryStamp);
		}

		public static RuntimeFlowControl CreateRfcForMethod(Proc proc)
		{
			RuntimeFlowControl runtimeFlowControl = new RuntimeFlowControl();
			runtimeFlowControl._activeFlowControlScope = runtimeFlowControl;
			runtimeFlowControl.InitializeRfc(proc);
			return runtimeFlowControl;
		}

		public static void EnterLoop(RubyScope scope)
		{
			scope.InLoop = true;
		}

		public static void LeaveLoop(RubyScope scope)
		{
			scope.InLoop = false;
		}

		public static void EnterRescue(RubyScope scope)
		{
			scope.InRescue = true;
		}

		public static void LeaveRescue(RubyScope scope)
		{
			scope.InRescue = false;
		}

		public static object BlockRetry(BlockParam blockFlowControl)
		{
			if (blockFlowControl.CallerKind == BlockCallerKind.Yield)
			{
				blockFlowControl.SetFlowControl(BlockReturnReason.Retry, null, blockFlowControl.Proc.Kind);
				return BlockReturnResult.Retry;
			}
			throw new LocalJumpError("retry from proc-closure");
		}

		public static object MethodRetry(RubyScope scope, Proc proc)
		{
			if (proc != null)
			{
				return BlockReturnResult.Retry;
			}
			throw new LocalJumpError("retry used out of rescue", scope.FlowControlScope);
		}

		public static void EvalRetry(RubyScope scope)
		{
			if (scope.InRescue)
			{
				throw new EvalUnwinder(BlockReturnReason.Retry, BlockReturnResult.Retry);
			}
			RubyBlockScope blockScope;
			RubyMethodScope methodScope;
			scope.GetInnerMostBlockOrMethodScope(out blockScope, out methodScope);
			if (methodScope != null && methodScope.BlockParameter != null)
			{
				throw new EvalUnwinder(BlockReturnReason.Retry, BlockReturnResult.Retry);
			}
			if (blockScope != null)
			{
				if (blockScope.BlockFlowControl.CallerKind == BlockCallerKind.Yield)
				{
					throw new EvalUnwinder(BlockReturnReason.Retry, null, blockScope.BlockFlowControl.Proc.Kind, BlockReturnResult.Retry);
				}
				throw new LocalJumpError("retry from proc-closure");
			}
			throw new LocalJumpError("retry used out of rescue", scope.FlowControlScope);
		}

		public static object BlockBreak(BlockParam blockFlowControl, object returnValue)
		{
			return blockFlowControl.Break(returnValue);
		}

		public static void MethodBreak(object returnValue)
		{
			throw new LocalJumpError("unexpected break");
		}

		public static void EvalBreak(RubyScope scope, object returnValue)
		{
			if (scope.InLoop)
			{
				throw new EvalUnwinder(BlockReturnReason.Break, returnValue);
			}
			RubyBlockScope blockScope;
			RubyMethodScope methodScope;
			scope.GetInnerMostBlockOrMethodScope(out blockScope, out methodScope);
			if (blockScope != null)
			{
				Proc proc = blockScope.BlockFlowControl.Proc;
				throw new EvalUnwinder(BlockReturnReason.Break, proc.Converter, proc.Kind, returnValue);
			}
			throw new LocalJumpError("unexpected break");
		}

		public static void MethodNext(RubyScope scope, object returnValue)
		{
			throw new LocalJumpError("unexpected next", scope.FlowControlScope);
		}

		public static void MethodRedo(RubyScope scope)
		{
			throw new LocalJumpError("unexpected redo", scope.FlowControlScope);
		}

		public static void EvalNext(RubyScope scope, object returnValue)
		{
			EvalNextOrRedo(scope, returnValue, false);
		}

		public static void EvalRedo(RubyScope scope)
		{
			EvalNextOrRedo(scope, null, true);
		}

		private static void EvalNextOrRedo(RubyScope scope, object returnValue, bool isRedo)
		{
			if (scope.InLoop)
			{
				throw new BlockUnwinder(returnValue, isRedo);
			}
			RubyBlockScope blockScope;
			RubyMethodScope methodScope;
			scope.GetInnerMostBlockOrMethodScope(out blockScope, out methodScope);
			if (blockScope != null)
			{
				throw new BlockUnwinder(returnValue, isRedo);
			}
			throw new LocalJumpError(string.Format("unexpected {0}", isRedo ? "redo" : "next"));
		}

		public static object BlockReturn(BlockParam blockFlowControl, object returnValue)
		{
			Proc proc = blockFlowControl.Proc;
			if (blockFlowControl.CallerKind == BlockCallerKind.Call && proc.Kind == ProcKind.Lambda)
			{
				return returnValue;
			}
			RuntimeFlowControl flowControlScope = proc.LocalScope.FlowControlScope;
			if (flowControlScope.IsActiveMethod)
			{
				blockFlowControl.ReturnReason = BlockReturnReason.Return;
				return new BlockReturnResult(flowControlScope, returnValue);
			}
			throw new LocalJumpError("unexpected return");
		}

		public static object EvalReturn(RubyScope scope, object returnValue)
		{
			RubyBlockScope blockScope;
			RubyMethodScope methodScope;
			scope.GetInnerMostBlockOrMethodScope(out blockScope, out methodScope);
			if (blockScope != null)
			{
				Proc proc = blockScope.BlockFlowControl.Proc;
				if (blockScope.BlockFlowControl.CallerKind == BlockCallerKind.Call && proc.Kind == ProcKind.Lambda)
				{
					throw new BlockUnwinder(returnValue, false);
				}
				RuntimeFlowControl flowControlScope = proc.LocalScope.FlowControlScope;
				if (flowControlScope.IsActiveMethod)
				{
					throw new MethodUnwinder(flowControlScope, returnValue);
				}
				throw new LocalJumpError("unexpected return");
			}
			throw new MethodUnwinder(scope.FlowControlScope, returnValue);
		}

		private static void YieldBlockReturn(BlockParam blockFlowControl, object returnValue)
		{
			if (blockFlowControl.CallerKind == BlockCallerKind.Yield)
			{
				blockFlowControl.SetFlowControl(BlockReturnReason.Return, null, blockFlowControl.Proc.Kind);
				return;
			}
			throw ((BlockReturnResult)returnValue).ToUnwinder();
		}

		public static bool BlockYield(RubyScope scope, BlockParam ownerBlockFlowControl, BlockParam yieldedBlockFlowControl, object returnValue)
		{
			switch (yieldedBlockFlowControl.ReturnReason)
			{
			case BlockReturnReason.Retry:
				BlockRetry(ownerBlockFlowControl);
				return true;
			case BlockReturnReason.Return:
				YieldBlockReturn(ownerBlockFlowControl, returnValue);
				return true;
			case BlockReturnReason.Break:
				YieldBlockBreak(scope, ownerBlockFlowControl, yieldedBlockFlowControl, returnValue);
				return true;
			default:
				return false;
			}
		}

		public static bool MethodYield(RubyScope scope, BlockParam yieldedBlockFlowControl, object returnValue)
		{
			return MethodYieldRfc(scope.FlowControlScope, yieldedBlockFlowControl, returnValue);
		}

		public static bool MethodYieldRfc(RuntimeFlowControl rfc, BlockParam yieldedBlockFlowControl, object returnValue)
		{
			switch (yieldedBlockFlowControl.ReturnReason)
			{
			case BlockReturnReason.Retry:
			case BlockReturnReason.Return:
				return true;
			case BlockReturnReason.Break:
				YieldMethodBreak(rfc, yieldedBlockFlowControl, returnValue);
				return true;
			default:
				return false;
			}
		}

		public static bool EvalYield(RubyScope scope, BlockParam yieldedBlockFlowControl, object returnValue)
		{
			switch (yieldedBlockFlowControl.ReturnReason)
			{
			case BlockReturnReason.Retry:
				EvalRetry(scope);
				throw Assert.Unreachable;
			case BlockReturnReason.Return:
				throw ((BlockReturnResult)returnValue).ToUnwinder();
			case BlockReturnReason.Break:
				YieldEvalBreak(yieldedBlockFlowControl, returnValue);
				throw Assert.Unreachable;
			default:
				return false;
			}
		}

		private static void YieldMethodBreak(RuntimeFlowControl rfc, BlockParam yieldedBlockFlowControl, object returnValue)
		{
			RuntimeFlowControl targetFrame = yieldedBlockFlowControl.TargetFrame;
			if (targetFrame.IsActiveMethod)
			{
				if (targetFrame == rfc)
				{
					return;
				}
				throw new MethodUnwinder(targetFrame, returnValue);
			}
			throw new LocalJumpError("break from proc-closure");
		}

		private static void YieldBlockBreak(RubyScope scope, BlockParam ownerBlockFlowControl, BlockParam yieldedBlockFlowControl, object returnValue)
		{
			RuntimeFlowControl targetFrame = yieldedBlockFlowControl.TargetFrame;
			if (targetFrame.IsActiveMethod)
			{
				throw new MethodUnwinder(targetFrame, returnValue);
			}
			throw new LocalJumpError("break from proc-closure");
		}

		private static void YieldEvalBreak(BlockParam blockFlowControl, object returnValue)
		{
			if (blockFlowControl.TargetFrame.IsActiveMethod)
			{
				throw new MethodUnwinder(blockFlowControl.TargetFrame, returnValue);
			}
			throw new LocalJumpError("break from proc-closure");
		}

		public static object MethodProcCall(BlockParam blockFlowControl, object returnValue)
		{
			switch (blockFlowControl.ReturnReason)
			{
			case BlockReturnReason.Break:
				if (blockFlowControl.SourceProcKind != ProcKind.Lambda)
				{
					YieldMethodBreak(null, blockFlowControl, returnValue);
				}
				return returnValue;
			case BlockReturnReason.Return:
				throw ((BlockReturnResult)returnValue).ToUnwinder();
			case BlockReturnReason.Retry:
				throw Assert.Unreachable;
			default:
				return returnValue;
			}
		}

		public static bool IsRetrySingleton(object value)
		{
			return value == BlockReturnResult.Retry;
		}

		public static object PropagateRetrySingleton(object other, object possibleRetrySingleton)
		{
			if (!IsRetrySingleton(possibleRetrySingleton))
			{
				return other;
			}
			return possibleRetrySingleton;
		}

		public static object MethodPropagateReturn(RubyScope scope, Proc block, BlockReturnResult unwinder)
		{
			if (unwinder.TargetFrame == scope)
			{
				return unwinder.ReturnValue;
			}
			if (block != null)
			{
				return unwinder;
			}
			throw unwinder.ToUnwinder();
		}

		public static object BlockPropagateReturn(BlockParam blockFlowControl, object returnValue)
		{
			blockFlowControl.ReturnReason = BlockReturnReason.Return;
			return returnValue;
		}

		public static object EvalPropagateReturn(object returnValue)
		{
			throw ((BlockReturnResult)returnValue).ToUnwinder();
		}

		public static bool IsProcConverterTarget(BlockParam bfc, MethodUnwinder unwinder)
		{
			if (bfc.IsLibProcConverter)
			{
				return unwinder.TargetFrame == bfc.Proc.Converter;
			}
			return false;
		}

		public static BlockParam CreateBfcForYield(Proc proc)
		{
			if (proc != null)
			{
				return new BlockParam(proc, BlockCallerKind.Yield, false);
			}
			throw RubyExceptions.NoBlockGiven();
		}

		public static BlockParam CreateBfcForProcCall(Proc proc)
		{
			return new BlockParam(proc, BlockCallerKind.Call, false);
		}

		public static BlockParam CreateBfcForLibraryMethod(Proc proc)
		{
			bool isLibProcConverter;
			if (proc.Kind == ProcKind.Block)
			{
				RuntimeFlowControl runtimeFlowControl = new RuntimeFlowControl();
				runtimeFlowControl._activeFlowControlScope = runtimeFlowControl;
				proc.Converter = runtimeFlowControl;
				proc.Kind = ProcKind.Proc;
				isLibProcConverter = true;
			}
			else
			{
				isLibProcConverter = false;
			}
			return new BlockParam(proc, BlockCallerKind.Yield, isLibProcConverter);
		}

		public static void LeaveProcConverter(BlockParam bfc)
		{
			if (bfc._isLibProcConverter)
			{
				bfc.Proc.Converter.LeaveMethod();
			}
		}
	}
}
