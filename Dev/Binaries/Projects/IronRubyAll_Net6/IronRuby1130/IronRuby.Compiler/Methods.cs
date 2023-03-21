using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Range = IronRuby.Builtins.Range;

namespace IronRuby.Compiler
{
	public static class Methods
	{
		private static ConstructorInfo _RubyCallSignatureCtor;

		private static MethodInfo _Stopwatch_GetTimestamp;

		private static MethodInfo _WeakReference_get_Target;

		private static MethodInfo _IList_get_Item;

		private static MethodInfo _AddItem;

		private static MethodInfo _AddRange;

		private static MethodInfo _AddSubRange;

		private static MethodInfo _AliasGlobalVariable;

		private static MethodInfo _AliasMethod;

		private static MethodInfo _AllocateStructInstance;

		private static MethodInfo _BlockBreak;

		private static MethodInfo _BlockPropagateReturn;

		private static MethodInfo _BlockRetry;

		private static MethodInfo _BlockReturn;

		private static MethodInfo _BlockYield;

		private static MethodInfo _CanRescue;

		private static MethodInfo _CompareDefaultException;

		private static MethodInfo _CompareException;

		private static MethodInfo _CompareSplattedExceptions;

		private static MethodInfo _ConvertBignumToFixnum;

		private static MethodInfo _ConvertBignumToFloat;

		private static MethodInfo _ConvertDoubleToFixnum;

		private static MethodInfo _ConvertMutableStringToClrString;

		private static MethodInfo _ConvertMutableStringToFloat;

		private static MethodInfo _ConvertRubySymbolToClrString;

		private static MethodInfo _ConvertStringToFloat;

		private static MethodInfo _ConvertSymbolToClrString;

		private static MethodInfo _ConvertSymbolToMutableString;

		private static MethodInfo _CreateArgumentsError;

		private static MethodInfo _CreateArgumentsErrorForMissingBlock;

		private static MethodInfo _CreateArgumentsErrorForProc;

		private static MethodInfo _CreateBfcForLibraryMethod;

		private static MethodInfo _CreateBfcForProcCall;

		private static MethodInfo _CreateBfcForYield;

		private static MethodInfo _CreateBlockScope;

		private static MethodInfo _CreateBoundMember;

		private static MethodInfo _CreateBoundMissingMember;

		private static MethodInfo _CreateDefaultInstance;

		private static MethodInfo _CreateDelegateFromMethod;

		private static MethodInfo _CreateDelegateFromProc;

		private static MethodInfo _CreateEncoding;

		private static MethodInfo _CreateEvent;

		private static MethodInfo _CreateExclusiveIntegerRange;

		private static MethodInfo _CreateExclusiveRange;

		private static MethodInfo _CreateFileInitializerScope;

		private static MethodInfo _CreateInclusiveIntegerRange;

		private static MethodInfo _CreateInclusiveRange;

		private static MethodInfo _CreateMethodScope;

		private static MethodInfo _CreateModuleScope;

		private static MethodInfo _CreateMutableStringB;

		private static MethodInfo _CreateMutableStringL;

		private static MethodInfo _CreateMutableStringLM;

		private static MethodInfo _CreateMutableStringM;

		private static MethodInfo _CreateMutableStringML;

		private static MethodInfo _CreateMutableStringMM;

		private static MethodInfo _CreateMutableStringN;

		private static MethodInfo _CreateRegexL;

		private static MethodInfo _CreateRegexLM;

		private static MethodInfo _CreateRegexM;

		private static MethodInfo _CreateRegexML;

		private static MethodInfo _CreateRegexMM;

		private static MethodInfo _CreateRegexN;

		private static MethodInfo _CreateRfcForMethod;

		private static MethodInfo _CreateStructInstance;

		private static MethodInfo _CreateSymbolLM;

		private static MethodInfo _CreateSymbolM;

		private static MethodInfo _CreateSymbolML;

		private static MethodInfo _CreateSymbolMM;

		private static MethodInfo _CreateSymbolN;

		private static MethodInfo _CreateTypeConversionError;

		private static MethodInfo _CreateVector;

		private static MethodInfo _CreateVectorWithValues;

		private static MethodInfo _DefineBlock;

		private static MethodInfo _DefineClass;

		private static MethodInfo _DefineGlobalClass;

		private static MethodInfo _DefineGlobalModule;

		private static MethodInfo _DefineLambda;

		private static MethodInfo _DefineMethod;

		private static MethodInfo _DefineModule;

		private static MethodInfo _DefineNestedClass;

		private static MethodInfo _DefineNestedModule;

		private static MethodInfo _DefineSingletonClass;

		private static MethodInfo _DeserializeObject;

		private static MethodInfo _EnterLoop;

		private static MethodInfo _EnterRescue;

		private static MethodInfo _EvalBreak;

		private static MethodInfo _EvalNext;

		private static MethodInfo _EvalPropagateReturn;

		private static MethodInfo _EvalRedo;

		private static MethodInfo _EvalRetry;

		private static MethodInfo _EvalReturn;

		private static MethodInfo _EvalYield;

		private static MethodInfo _ExistsUnsplat;

		private static MethodInfo _ExistsUnsplatCompare;

		private static MethodInfo _FilterBlockException;

		private static MethodInfo _FreezeObject;

		private static MethodInfo _GetArrayItem;

		private static MethodInfo _GetArrayRange;

		private static MethodInfo _GetClassVariable;

		private static MethodInfo _GetContextFromBlockParam;

		private static MethodInfo _GetContextFromIRubyObject;

		private static MethodInfo _GetContextFromMethod;

		private static MethodInfo _GetContextFromModule;

		private static MethodInfo _GetContextFromProc;

		private static MethodInfo _GetContextFromScope;

		private static MethodInfo _GetCurrentException;

		private static MethodInfo _GetCurrentMatchData;

		private static MethodInfo _GetCurrentMatchGroup;

		private static MethodInfo _GetCurrentMatchLastGroup;

		private static MethodInfo _GetCurrentPostMatch;

		private static MethodInfo _GetCurrentPreMatch;

		private static MethodInfo _GetDefaultExceptionMessage;

		private static MethodInfo _GetEmptyScope;

		private static MethodInfo _GetExpressionQualifiedConstant;

		private static MethodInfo _GetGlobalMissingConstant;

		private static MethodInfo _GetGlobalScopeFromScope;

		private static MethodInfo _GetGlobalVariable;

		private static MethodInfo _GetInstanceData;

		private static MethodInfo _GetInstanceVariable;

		private static MethodInfo _GetLocals;

		private static MethodInfo _GetLocalVariable;

		private static MethodInfo _GetMetaObject;

		private static MethodInfo _GetMethodBlockParameter;

		private static MethodInfo _GetMethodBlockParameterSelf;

		private static MethodInfo _GetMethodUnwinderReturnValue;

		private static MethodInfo _GetMissingConstant;

		private static MethodInfo _GetMutableStringBytes;

		private static MethodInfo _GetParentLocals;

		private static MethodInfo _GetParentScope;

		private static MethodInfo _GetProcArity;

		private static MethodInfo _GetProcSelf;

		private static MethodInfo _GetQualifiedConstant;

		private static MethodInfo _GetSelfClassVersionHandle;

		private static MethodInfo _GetSuperCallTarget;

		private static MethodInfo _GetTrailingArrayItem;

		private static MethodInfo _GetUnqualifiedConstant;

		private static MethodInfo _HookupEvent;

		private static MethodInfo _InitializeBlock;

		private static MethodInfo _InitializeScope;

		private static MethodInfo _InitializeScopeNoLocals;

		private static MethodInfo _InstantiateBlock;

		private static MethodInfo _InstantiateLambda;

		private static MethodInfo _IntegerValue_ToUInt32Unchecked;

		private static MethodInfo _IRubyObject_BaseEquals;

		private static MethodInfo _IRubyObject_BaseGetHashCode;

		private static MethodInfo _IRubyObject_BaseToString;

		private static MethodInfo _IRubyObject_get_ImmediateClass;

		private static MethodInfo _IRubyObject_GetInstanceData;

		private static MethodInfo _IRubyObject_set_ImmediateClass;

		private static MethodInfo _IRubyObject_TryGetInstanceData;

		private static MethodInfo _IRubyObjectState_Freeze;

		private static MethodInfo _IRubyObjectState_get_IsFrozen;

		private static MethodInfo _IRubyObjectState_get_IsTainted;

		private static MethodInfo _IRubyObjectState_get_IsUntrusted;

		private static MethodInfo _IRubyObjectState_set_IsTainted;

		private static MethodInfo _IRubyObjectState_set_IsUntrusted;

		private static MethodInfo _IsClrNonSingletonRuleValid;

		private static MethodInfo _IsClrSingletonRuleValid;

		private static MethodInfo _IsDefinedClassVariable;

		private static MethodInfo _IsDefinedExpressionQualifiedConstant;

		private static MethodInfo _IsDefinedGlobalConstant;

		private static MethodInfo _IsDefinedGlobalVariable;

		private static MethodInfo _IsDefinedInstanceVariable;

		private static MethodInfo _IsDefinedQualifiedConstant;

		private static MethodInfo _IsDefinedUnqualifiedConstant;

		private static MethodInfo _IsFalse;

		private static MethodInfo _IsMethodUnwinderTargetFrame;

		private static MethodInfo _IsObjectFrozen;

		private static MethodInfo _IsObjectTainted;

		private static MethodInfo _IsObjectUntrusted;

		private static MethodInfo _IsProcConverterTarget;

		private static MethodInfo _IsRetrySingleton;

		private static MethodInfo _IsSuperOutOfMethodScope;

		private static MethodInfo _IsTrue;

		private static MethodInfo _LeaveLoop;

		private static MethodInfo _LeaveMethodFrame;

		private static MethodInfo _LeaveProcConverter;

		private static MethodInfo _LeaveRescue;

		private static MethodInfo _MakeAbstractMethodCalledError;

		private static MethodInfo _MakeAllocatorUndefinedError;

		private static MethodInfo _MakeAmbiguousMatchError;

		private static MethodInfo _MakeArray0;

		private static MethodInfo _MakeArray1;

		private static MethodInfo _MakeArray2;

		private static MethodInfo _MakeArray3;

		private static MethodInfo _MakeArray4;

		private static MethodInfo _MakeArray5;

		private static MethodInfo _MakeArrayN;

		private static MethodInfo _MakeClrProtectedMethodCalledError;

		private static MethodInfo _MakeClrVirtualMethodCalledError;

		private static MethodInfo _MakeConstructorUndefinedError;

		private static MethodInfo _MakeHash;

		private static MethodInfo _MakeHash0;

		private static MethodInfo _MakeImplicitSuperInBlockMethodError;

		private static MethodInfo _MakeInvalidArgumentTypesError;

		private static MethodInfo _MakeMissingDefaultConstructorError;

		private static MethodInfo _MakeMissingMemberError;

		private static MethodInfo _MakeMissingMethodError;

		private static MethodInfo _MakeMissingSuperException;

		private static MethodInfo _MakeNotClrTypeError;

		private static MethodInfo _MakePrivateMethodCalledError;

		private static MethodInfo _MakeProtectedMethodCalledError;

		private static MethodInfo _MakeTopLevelSuperException;

		private static MethodInfo _MakeTypeConversionError;

		private static MethodInfo _MakeVirtualClassInstantiatedError;

		private static MethodInfo _MakeWrongNumberOfArgumentsError;

		private static MethodInfo _MarkException;

		private static MethodInfo _MatchLastInputLine;

		private static MethodInfo _MatchString;

		private static MethodInfo _MethodBreak;

		private static MethodInfo _MethodNext;

		private static MethodInfo _MethodProcCall;

		private static MethodInfo _MethodPropagateReturn;

		private static MethodInfo _MethodRedo;

		private static MethodInfo _MethodRetry;

		private static MethodInfo _MethodYield;

		private static MethodInfo _NullIfFalse;

		private static MethodInfo _NullIfTrue;

		private static MethodInfo _ObjectToMutableString;

		private static MethodInfo _ObjectToString;

		private static MethodInfo _PrintInteractiveResult;

		private static MethodInfo _PropagateRetrySingleton;

		private static MethodInfo _RegisterShutdownHandler;

		private static MethodInfo _RubyStruct_GetValue;

		private static MethodInfo _RubyStruct_SetValue;

		private static MethodInfo _SerializeObject;

		private static MethodInfo _SetClassVariable;

		private static MethodInfo _SetCurrentException;

		private static MethodInfo _SetDataConstant;

		private static MethodInfo _SetGlobalConstant;

		private static MethodInfo _SetGlobalVariable;

		private static MethodInfo _SetInstanceVariable;

		private static MethodInfo _SetLocalVariable;

		private static MethodInfo _SetObjectTaint;

		private static MethodInfo _SetObjectTrustiness;

		private static MethodInfo _SetQualifiedConstant;

		private static MethodInfo _SetUnqualifiedConstant;

		private static MethodInfo _Splat;

		private static MethodInfo _SplatAppend;

		private static MethodInfo _SplatPair;

		private static MethodInfo _StringToMutableString;

		private static MethodInfo _ToArrayValidator;

		private static MethodInfo _ToAValidator;

		private static MethodInfo _ToBignumValidator;

		private static MethodInfo _ToByteValidator;

		private static MethodInfo _ToDoubleValidator;

		private static MethodInfo _ToFixnumValidator;

		private static MethodInfo _ToHashValidator;

		private static MethodInfo _ToInt16Validator;

		private static MethodInfo _ToInt64Validator;

		private static MethodInfo _ToIntegerValidator;

		private static MethodInfo _ToProcValidator;

		private static MethodInfo _ToRegexValidator;

		private static MethodInfo _ToSByteValidator;

		private static MethodInfo _ToSDefaultConversion;

		private static MethodInfo _ToSingleValidator;

		private static MethodInfo _ToStringValidator;

		private static MethodInfo _ToSymbolValidator;

		private static MethodInfo _ToUInt16Validator;

		private static MethodInfo _ToUInt32Validator;

		private static MethodInfo _ToUInt64Validator;

		private static MethodInfo _TraceBlockCall;

		private static MethodInfo _TraceBlockReturn;

		private static MethodInfo _TraceMethodCall;

		private static MethodInfo _TraceMethodReturn;

		private static MethodInfo _TraceTopLevelCodeFrame;

		private static MethodInfo _TryGetClassVariable;

		private static MethodInfo _UndefineMethod;

		private static MethodInfo _Unsplat;

		private static MethodInfo _UpdateProfileTicks;

		private static MethodInfo _X;

		private static MethodInfo _Yield0;

		private static MethodInfo _Yield1;

		private static MethodInfo _Yield2;

		private static MethodInfo _Yield3;

		private static MethodInfo _Yield4;

		private static MethodInfo _YieldN;

		private static MethodInfo _YieldSplat0;

		private static MethodInfo _YieldSplat1;

		private static MethodInfo _YieldSplat2;

		private static MethodInfo _YieldSplat3;

		private static MethodInfo _YieldSplat4;

		private static MethodInfo _YieldSplatN;

		private static MethodInfo _YieldSplatNRhs;

		public static ConstructorInfo RubyCallSignatureCtor
		{
			get
			{
				return _RubyCallSignatureCtor ?? (_RubyCallSignatureCtor = GetConstructor(typeof(RubyCallSignature), typeof(uint)));
			}
		}

		public static MethodInfo Stopwatch_GetTimestamp
		{
			get
			{
				return _Stopwatch_GetTimestamp ?? (_Stopwatch_GetTimestamp = GetMethod(typeof(Stopwatch), "GetTimestamp"));
			}
		}

		public static MethodInfo IList_get_Item
		{
			get
			{
				return _IList_get_Item ?? (_IList_get_Item = GetMethod(typeof(IList), "get_Item"));
			}
		}

		public static MethodInfo WeakReference_get_Target
		{
			get
			{
				return _WeakReference_get_Target ?? (_WeakReference_get_Target = GetMethod(typeof(WeakReference), "get_Target", BindingFlags.Instance, Type.EmptyTypes));
			}
		}

		public static MethodInfo AddItem
		{
			get
			{
				return _AddItem ?? (_AddItem = CallInstruction.CacheFunc<RubyArray, object, RubyArray>(RubyOps.AddItem));
			}
		}

		public static MethodInfo AddRange
		{
			get
			{
				return _AddRange ?? (_AddRange = CallInstruction.CacheFunc<RubyArray, IList, RubyArray>(RubyOps.AddRange));
			}
		}

		public static MethodInfo AddSubRange
		{
			get
			{
				return _AddSubRange ?? (_AddSubRange = CallInstruction.CacheFunc<RubyArray, IList, int, int, RubyArray>(RubyOps.AddSubRange));
			}
		}

		public static MethodInfo AliasGlobalVariable
		{
			get
			{
				return _AliasGlobalVariable ?? (_AliasGlobalVariable = CallInstruction.CacheAction<RubyScope, string, string>(RubyOps.AliasGlobalVariable));
			}
		}

		public static MethodInfo AliasMethod
		{
			get
			{
				return _AliasMethod ?? (_AliasMethod = CallInstruction.CacheAction<RubyScope, string, string>(RubyOps.AliasMethod));
			}
		}

		public static MethodInfo AllocateStructInstance
		{
			get
			{
				return _AllocateStructInstance ?? (_AllocateStructInstance = CallInstruction.CacheFunc<RubyClass, RubyStruct>(RubyOps.AllocateStructInstance));
			}
		}

		public static MethodInfo BlockBreak
		{
			get
			{
				return _BlockBreak ?? (_BlockBreak = CallInstruction.CacheFunc<BlockParam, object, object>(RubyOps.BlockBreak));
			}
		}

		public static MethodInfo BlockPropagateReturn
		{
			get
			{
				return _BlockPropagateReturn ?? (_BlockPropagateReturn = CallInstruction.CacheFunc<BlockParam, object, object>(RubyOps.BlockPropagateReturn));
			}
		}

		public static MethodInfo BlockRetry
		{
			get
			{
				return _BlockRetry ?? (_BlockRetry = CallInstruction.CacheFunc<BlockParam, object>(RubyOps.BlockRetry));
			}
		}

		public static MethodInfo BlockReturn
		{
			get
			{
				return _BlockReturn ?? (_BlockReturn = CallInstruction.CacheFunc<BlockParam, object, object>(RubyOps.BlockReturn));
			}
		}

		public static MethodInfo BlockYield
		{
			get
			{
				return _BlockYield ?? (_BlockYield = CallInstruction.CacheFunc<RubyScope, BlockParam, BlockParam, object, bool>(RubyOps.BlockYield));
			}
		}

		public static MethodInfo CanRescue
		{
			get
			{
				return _CanRescue ?? (_CanRescue = CallInstruction.CacheFunc<RubyScope, Exception, bool>(RubyOps.CanRescue));
			}
		}

		public static MethodInfo CompareDefaultException
		{
			get
			{
				return _CompareDefaultException ?? (_CompareDefaultException = CallInstruction.CacheFunc<RubyScope, bool>(RubyOps.CompareDefaultException));
			}
		}

		public static MethodInfo CompareException
		{
			get
			{
				return _CompareException ?? (_CompareException = CallInstruction.CacheFunc<BinaryOpStorage, RubyScope, object, bool>(RubyOps.CompareException));
			}
		}

		public static MethodInfo CompareSplattedExceptions
		{
			get
			{
				return _CompareSplattedExceptions ?? (_CompareSplattedExceptions = CallInstruction.CacheFunc<BinaryOpStorage, RubyScope, IList, bool>(RubyOps.CompareSplattedExceptions));
			}
		}

		public static MethodInfo ConvertBignumToFixnum
		{
			get
			{
				return _ConvertBignumToFixnum ?? (_ConvertBignumToFixnum = CallInstruction.CacheFunc<BigInteger, int>(RubyOps.ConvertBignumToFixnum));
			}
		}

		public static MethodInfo ConvertBignumToFloat
		{
			get
			{
				return _ConvertBignumToFloat ?? (_ConvertBignumToFloat = CallInstruction.CacheFunc<BigInteger, double>(RubyOps.ConvertBignumToFloat));
			}
		}

		public static MethodInfo ConvertDoubleToFixnum
		{
			get
			{
				return _ConvertDoubleToFixnum ?? (_ConvertDoubleToFixnum = CallInstruction.CacheFunc<double, int>(RubyOps.ConvertDoubleToFixnum));
			}
		}

		public static MethodInfo ConvertMutableStringToClrString
		{
			get
			{
				return _ConvertMutableStringToClrString ?? (_ConvertMutableStringToClrString = CallInstruction.CacheFunc<MutableString, string>(RubyOps.ConvertMutableStringToClrString));
			}
		}

		public static MethodInfo ConvertMutableStringToFloat
		{
			get
			{
				return _ConvertMutableStringToFloat ?? (_ConvertMutableStringToFloat = CallInstruction.CacheFunc<RubyContext, MutableString, double>(RubyOps.ConvertMutableStringToFloat));
			}
		}

		public static MethodInfo ConvertRubySymbolToClrString
		{
			get
			{
				return _ConvertRubySymbolToClrString ?? (_ConvertRubySymbolToClrString = CallInstruction.CacheFunc<RubyContext, int, string>(RubyOps.ConvertRubySymbolToClrString));
			}
		}

		public static MethodInfo ConvertStringToFloat
		{
			get
			{
				return _ConvertStringToFloat ?? (_ConvertStringToFloat = CallInstruction.CacheFunc<RubyContext, string, double>(RubyOps.ConvertStringToFloat));
			}
		}

		public static MethodInfo ConvertSymbolToClrString
		{
			get
			{
				return _ConvertSymbolToClrString ?? (_ConvertSymbolToClrString = CallInstruction.CacheFunc<RubySymbol, string>(RubyOps.ConvertSymbolToClrString));
			}
		}

		public static MethodInfo ConvertSymbolToMutableString
		{
			get
			{
				return _ConvertSymbolToMutableString ?? (_ConvertSymbolToMutableString = CallInstruction.CacheFunc<RubySymbol, MutableString>(RubyOps.ConvertSymbolToMutableString));
			}
		}

		public static MethodInfo CreateArgumentsError
		{
			get
			{
				return _CreateArgumentsError ?? (_CreateArgumentsError = CallInstruction.CacheFunc<string, ArgumentException>(RubyOps.CreateArgumentsError));
			}
		}

		public static MethodInfo CreateArgumentsErrorForMissingBlock
		{
			get
			{
				return _CreateArgumentsErrorForMissingBlock ?? (_CreateArgumentsErrorForMissingBlock = CallInstruction.CacheFunc(RubyOps.CreateArgumentsErrorForMissingBlock));
			}
		}

		public static MethodInfo CreateArgumentsErrorForProc
		{
			get
			{
				return _CreateArgumentsErrorForProc ?? (_CreateArgumentsErrorForProc = CallInstruction.CacheFunc<string, ArgumentException>(RubyOps.CreateArgumentsErrorForProc));
			}
		}

		public static MethodInfo CreateBfcForLibraryMethod
		{
			get
			{
				return _CreateBfcForLibraryMethod ?? (_CreateBfcForLibraryMethod = CallInstruction.CacheFunc<Proc, BlockParam>(RubyOps.CreateBfcForLibraryMethod));
			}
		}

		public static MethodInfo CreateBfcForProcCall
		{
			get
			{
				return _CreateBfcForProcCall ?? (_CreateBfcForProcCall = CallInstruction.CacheFunc<Proc, BlockParam>(RubyOps.CreateBfcForProcCall));
			}
		}

		public static MethodInfo CreateBfcForYield
		{
			get
			{
				return _CreateBfcForYield ?? (_CreateBfcForYield = CallInstruction.CacheFunc<Proc, BlockParam>(RubyOps.CreateBfcForYield));
			}
		}

		public static MethodInfo CreateBlockScope
		{
			get
			{
				return _CreateBlockScope ?? (_CreateBlockScope = CallInstruction.CacheFunc<MutableTuple, string[], BlockParam, object, InterpretedFrame, RubyBlockScope>(RubyOps.CreateBlockScope));
			}
		}

		public static MethodInfo CreateBoundMember
		{
			get
			{
				return _CreateBoundMember ?? (_CreateBoundMember = CallInstruction.CacheFunc<object, RubyMemberInfo, string, RubyMethod>(RubyOps.CreateBoundMember));
			}
		}

		public static MethodInfo CreateBoundMissingMember
		{
			get
			{
				return _CreateBoundMissingMember ?? (_CreateBoundMissingMember = CallInstruction.CacheFunc<object, RubyMemberInfo, string, RubyMethod>(RubyOps.CreateBoundMissingMember));
			}
		}

		public static MethodInfo CreateDefaultInstance
		{
			get
			{
				return _CreateDefaultInstance ?? (_CreateDefaultInstance = CallInstruction.CacheFunc(RubyOps.CreateDefaultInstance));
			}
		}

		public static MethodInfo CreateDelegateFromMethod
		{
			get
			{
				return _CreateDelegateFromMethod ?? (_CreateDelegateFromMethod = CallInstruction.CacheFunc<Type, RubyMethod, Delegate>(RubyOps.CreateDelegateFromMethod));
			}
		}

		public static MethodInfo CreateDelegateFromProc
		{
			get
			{
				return _CreateDelegateFromProc ?? (_CreateDelegateFromProc = CallInstruction.CacheFunc<Type, Proc, Delegate>(RubyOps.CreateDelegateFromProc));
			}
		}

		public static MethodInfo CreateEncoding
		{
			get
			{
				return _CreateEncoding ?? (_CreateEncoding = CallInstruction.CacheFunc<int, RubyEncoding>(RubyOps.CreateEncoding));
			}
		}

		public static MethodInfo CreateEvent
		{
			get
			{
				return _CreateEvent ?? (_CreateEvent = CallInstruction.CacheFunc<RubyEventInfo, object, string, RubyEvent>(RubyOps.CreateEvent));
			}
		}

		public static MethodInfo CreateExclusiveIntegerRange
		{
			get
			{
				return _CreateExclusiveIntegerRange ?? (_CreateExclusiveIntegerRange = CallInstruction.CacheFunc<int, int, Range>(RubyOps.CreateExclusiveIntegerRange));
			}
		}

		public static MethodInfo CreateExclusiveRange
		{
			get
			{
				return _CreateExclusiveRange ?? (_CreateExclusiveRange = CallInstruction.CacheFunc<object, object, RubyScope, BinaryOpStorage, Range>(RubyOps.CreateExclusiveRange));
			}
		}

		public static MethodInfo CreateFileInitializerScope
		{
			get
			{
				return _CreateFileInitializerScope ?? (_CreateFileInitializerScope = CallInstruction.CacheFunc<MutableTuple, string[], RubyScope, RubyScope>(RubyOps.CreateFileInitializerScope));
			}
		}

		public static MethodInfo CreateInclusiveIntegerRange
		{
			get
			{
				return _CreateInclusiveIntegerRange ?? (_CreateInclusiveIntegerRange = CallInstruction.CacheFunc<int, int, Range>(RubyOps.CreateInclusiveIntegerRange));
			}
		}

		public static MethodInfo CreateInclusiveRange
		{
			get
			{
				return _CreateInclusiveRange ?? (_CreateInclusiveRange = CallInstruction.CacheFunc<object, object, RubyScope, BinaryOpStorage, Range>(RubyOps.CreateInclusiveRange));
			}
		}

		public static MethodInfo CreateMethodScope
		{
			get
			{
				return _CreateMethodScope ?? (_CreateMethodScope = CallInstruction.CacheFunc<MutableTuple, string[], int, RubyScope, RubyModule, string, object, Proc, InterpretedFrame, RubyMethodScope>(RubyOps.CreateMethodScope));
			}
		}

		public static MethodInfo CreateModuleScope
		{
			get
			{
				return _CreateModuleScope ?? (_CreateModuleScope = CallInstruction.CacheFunc<MutableTuple, string[], RubyScope, RubyModule, RubyModuleScope>(RubyOps.CreateModuleScope));
			}
		}

		public static MethodInfo CreateMutableStringB
		{
			get
			{
				return _CreateMutableStringB ?? (_CreateMutableStringB = CallInstruction.CacheFunc<byte[], RubyEncoding, MutableString>(RubyOps.CreateMutableStringB));
			}
		}

		public static MethodInfo CreateMutableStringL
		{
			get
			{
				return _CreateMutableStringL ?? (_CreateMutableStringL = CallInstruction.CacheFunc<string, RubyEncoding, MutableString>(RubyOps.CreateMutableStringL));
			}
		}

		public static MethodInfo CreateMutableStringLM
		{
			get
			{
				return _CreateMutableStringLM ?? (_CreateMutableStringLM = CallInstruction.CacheFunc<string, MutableString, RubyEncoding, MutableString>(RubyOps.CreateMutableStringLM));
			}
		}

		public static MethodInfo CreateMutableStringM
		{
			get
			{
				return _CreateMutableStringM ?? (_CreateMutableStringM = CallInstruction.CacheFunc<MutableString, RubyEncoding, MutableString>(RubyOps.CreateMutableStringM));
			}
		}

		public static MethodInfo CreateMutableStringML
		{
			get
			{
				return _CreateMutableStringML ?? (_CreateMutableStringML = CallInstruction.CacheFunc<MutableString, string, RubyEncoding, MutableString>(RubyOps.CreateMutableStringML));
			}
		}

		public static MethodInfo CreateMutableStringMM
		{
			get
			{
				return _CreateMutableStringMM ?? (_CreateMutableStringMM = CallInstruction.CacheFunc<MutableString, MutableString, RubyEncoding, MutableString>(RubyOps.CreateMutableStringMM));
			}
		}

		public static MethodInfo CreateMutableStringN
		{
			get
			{
				return _CreateMutableStringN ?? (_CreateMutableStringN = CallInstruction.CacheFunc<MutableString[], MutableString>(RubyOps.CreateMutableStringN));
			}
		}

		public static MethodInfo CreateRegexL
		{
			get
			{
				return _CreateRegexL ?? (_CreateRegexL = CallInstruction.CacheFunc<string, RubyEncoding, RubyRegexOptions, StrongBox<RubyRegex>, RubyRegex>(RubyOps.CreateRegexL));
			}
		}

		public static MethodInfo CreateRegexB
		{
			get
			{
				return _CreateRegexL ?? (_CreateRegexL = CallInstruction.CacheFunc<string, RubyEncoding, RubyRegexOptions, StrongBox<RubyRegex>, RubyRegex>(RubyOps.CreateRegexL));
			}
		}

		public static MethodInfo CreateRegexLM
		{
			get
			{
				return _CreateRegexLM ?? (_CreateRegexLM = CallInstruction.CacheFunc<string, MutableString, RubyEncoding, RubyRegexOptions, StrongBox<RubyRegex>, RubyRegex>(RubyOps.CreateRegexLM));
			}
		}

		public static MethodInfo CreateRegexM
		{
			get
			{
				return _CreateRegexM ?? (_CreateRegexM = CallInstruction.CacheFunc<MutableString, RubyEncoding, RubyRegexOptions, StrongBox<RubyRegex>, RubyRegex>(RubyOps.CreateRegexM));
			}
		}

		public static MethodInfo CreateRegexML
		{
			get
			{
				return _CreateRegexML ?? (_CreateRegexML = CallInstruction.CacheFunc<MutableString, string, RubyEncoding, RubyRegexOptions, StrongBox<RubyRegex>, RubyRegex>(RubyOps.CreateRegexML));
			}
		}

		public static MethodInfo CreateRegexMM
		{
			get
			{
				return _CreateRegexMM ?? (_CreateRegexMM = CallInstruction.CacheFunc<MutableString, MutableString, RubyEncoding, RubyRegexOptions, StrongBox<RubyRegex>, RubyRegex>(RubyOps.CreateRegexMM));
			}
		}

		public static MethodInfo CreateRegexN
		{
			get
			{
				return _CreateRegexN ?? (_CreateRegexN = CallInstruction.CacheFunc<MutableString[], RubyRegexOptions, StrongBox<RubyRegex>, RubyRegex>(RubyOps.CreateRegexN));
			}
		}

		public static MethodInfo CreateRfcForMethod
		{
			get
			{
				return _CreateRfcForMethod ?? (_CreateRfcForMethod = CallInstruction.CacheFunc<Proc, RuntimeFlowControl>(RubyOps.CreateRfcForMethod));
			}
		}

		public static MethodInfo CreateStructInstance
		{
			get
			{
				return _CreateStructInstance ?? (_CreateStructInstance = CallInstruction.CacheFunc<RubyClass, object[], RubyStruct>(RubyOps.CreateStructInstance));
			}
		}

		public static MethodInfo CreateSymbolLM
		{
			get
			{
				return _CreateSymbolLM ?? (_CreateSymbolLM = CallInstruction.CacheFunc<string, MutableString, RubyEncoding, RubyScope, RubySymbol>(RubyOps.CreateSymbolLM));
			}
		}

		public static MethodInfo CreateSymbolM
		{
			get
			{
				return _CreateSymbolM ?? (_CreateSymbolM = CallInstruction.CacheFunc<MutableString, RubyEncoding, RubyScope, RubySymbol>(RubyOps.CreateSymbolM));
			}
		}

		public static MethodInfo CreateSymbolML
		{
			get
			{
				return _CreateSymbolML ?? (_CreateSymbolML = CallInstruction.CacheFunc<MutableString, string, RubyEncoding, RubyScope, RubySymbol>(RubyOps.CreateSymbolML));
			}
		}

		public static MethodInfo CreateSymbolMM
		{
			get
			{
				return _CreateSymbolMM ?? (_CreateSymbolMM = CallInstruction.CacheFunc<MutableString, MutableString, RubyEncoding, RubyScope, RubySymbol>(RubyOps.CreateSymbolMM));
			}
		}

		public static MethodInfo CreateSymbolN
		{
			get
			{
				return _CreateSymbolN ?? (_CreateSymbolN = CallInstruction.CacheFunc<MutableString[], RubyScope, RubySymbol>(RubyOps.CreateSymbolN));
			}
		}

		public static MethodInfo CreateTypeConversionError
		{
			get
			{
				return _CreateTypeConversionError ?? (_CreateTypeConversionError = CallInstruction.CacheFunc<string, string, Exception>(RubyOps.CreateTypeConversionError));
			}
		}

		public static MethodInfo CreateVector
		{
			get
			{
				return _CreateVector ?? (_CreateVector = GetMethod(typeof(RubyOps), "CreateVector"));
			}
		}

		public static MethodInfo CreateVectorWithValues
		{
			get
			{
				return _CreateVectorWithValues ?? (_CreateVectorWithValues = GetMethod(typeof(RubyOps), "CreateVectorWithValues"));
			}
		}

		public static MethodInfo DefineBlock
		{
			get
			{
				return _DefineBlock ?? (_DefineBlock = CallInstruction.CacheFunc<RubyScope, object, BlockDispatcher, object, Proc>(RubyOps.DefineBlock));
			}
		}

		public static MethodInfo DefineClass
		{
			get
			{
				return _DefineClass ?? (_DefineClass = CallInstruction.CacheFunc<RubyScope, object, string, object, RubyModule>(RubyOps.DefineClass));
			}
		}

		public static MethodInfo DefineGlobalClass
		{
			get
			{
				return _DefineGlobalClass ?? (_DefineGlobalClass = CallInstruction.CacheFunc<RubyScope, string, object, RubyModule>(RubyOps.DefineGlobalClass));
			}
		}

		public static MethodInfo DefineGlobalModule
		{
			get
			{
				return _DefineGlobalModule ?? (_DefineGlobalModule = CallInstruction.CacheFunc<RubyScope, string, RubyModule>(RubyOps.DefineGlobalModule));
			}
		}

		public static MethodInfo DefineLambda
		{
			get
			{
				return _DefineLambda ?? (_DefineLambda = CallInstruction.CacheFunc<RubyScope, object, BlockDispatcher, object, Proc>(RubyOps.DefineLambda));
			}
		}

		public static MethodInfo DefineMethod
		{
			get
			{
				return _DefineMethod ?? (_DefineMethod = CallInstruction.CacheFunc<object, RubyScope, RubyMethodBody, object>(RubyOps.DefineMethod));
			}
		}

		public static MethodInfo DefineModule
		{
			get
			{
				return _DefineModule ?? (_DefineModule = CallInstruction.CacheFunc<RubyScope, object, string, RubyModule>(RubyOps.DefineModule));
			}
		}

		public static MethodInfo DefineNestedClass
		{
			get
			{
				return _DefineNestedClass ?? (_DefineNestedClass = CallInstruction.CacheFunc<RubyScope, string, object, RubyModule>(RubyOps.DefineNestedClass));
			}
		}

		public static MethodInfo DefineNestedModule
		{
			get
			{
				return _DefineNestedModule ?? (_DefineNestedModule = CallInstruction.CacheFunc<RubyScope, string, RubyModule>(RubyOps.DefineNestedModule));
			}
		}

		public static MethodInfo DefineSingletonClass
		{
			get
			{
				return _DefineSingletonClass ?? (_DefineSingletonClass = CallInstruction.CacheFunc<RubyScope, object, RubyClass>(RubyOps.DefineSingletonClass));
			}
		}

		public static MethodInfo DeserializeObject
		{
			get
			{
				return _DeserializeObject ?? (_DeserializeObject = GetMethod(typeof(RubyOps), "DeserializeObject"));
			}
		}

		public static MethodInfo EnterLoop
		{
			get
			{
				return _EnterLoop ?? (_EnterLoop = CallInstruction.CacheAction<RubyScope>(RubyOps.EnterLoop));
			}
		}

		public static MethodInfo EnterRescue
		{
			get
			{
				return _EnterRescue ?? (_EnterRescue = CallInstruction.CacheAction<RubyScope>(RubyOps.EnterRescue));
			}
		}

		public static MethodInfo EvalBreak
		{
			get
			{
				return _EvalBreak ?? (_EvalBreak = CallInstruction.CacheAction<RubyScope, object>(RubyOps.EvalBreak));
			}
		}

		public static MethodInfo EvalNext
		{
			get
			{
				return _EvalNext ?? (_EvalNext = CallInstruction.CacheAction<RubyScope, object>(RubyOps.EvalNext));
			}
		}

		public static MethodInfo EvalPropagateReturn
		{
			get
			{
				return _EvalPropagateReturn ?? (_EvalPropagateReturn = CallInstruction.CacheFunc<object, object>(RubyOps.EvalPropagateReturn));
			}
		}

		public static MethodInfo EvalRedo
		{
			get
			{
				return _EvalRedo ?? (_EvalRedo = CallInstruction.CacheAction<RubyScope>(RubyOps.EvalRedo));
			}
		}

		public static MethodInfo EvalRetry
		{
			get
			{
				return _EvalRetry ?? (_EvalRetry = CallInstruction.CacheAction<RubyScope>(RubyOps.EvalRetry));
			}
		}

		public static MethodInfo EvalReturn
		{
			get
			{
				return _EvalReturn ?? (_EvalReturn = CallInstruction.CacheFunc<RubyScope, object, object>(RubyOps.EvalReturn));
			}
		}

		public static MethodInfo EvalYield
		{
			get
			{
				return _EvalYield ?? (_EvalYield = CallInstruction.CacheFunc<RubyScope, BlockParam, object, bool>(RubyOps.EvalYield));
			}
		}

		public static MethodInfo ExistsUnsplat
		{
			get
			{
				return _ExistsUnsplat ?? (_ExistsUnsplat = CallInstruction.CacheFunc<object, bool>(RubyOps.ExistsUnsplat));
			}
		}

		public static MethodInfo ExistsUnsplatCompare
		{
			get
			{
				return _ExistsUnsplatCompare ?? (_ExistsUnsplatCompare = CallInstruction.CacheFunc<CallSite<Func<CallSite, object, object, object>>, object, object, bool>(RubyOps.ExistsUnsplatCompare));
			}
		}

		public static MethodInfo FilterBlockException
		{
			get
			{
				return _FilterBlockException ?? (_FilterBlockException = CallInstruction.CacheFunc<RubyScope, Exception, bool>(RubyOps.FilterBlockException));
			}
		}

		public static MethodInfo FreezeObject
		{
			get
			{
				return _FreezeObject ?? (_FreezeObject = GetMethod(typeof(RubyOps), "FreezeObject"));
			}
		}

		public static MethodInfo GetArrayItem
		{
			get
			{
				return _GetArrayItem ?? (_GetArrayItem = CallInstruction.CacheFunc<IList, int, object>(RubyOps.GetArrayItem));
			}
		}

		public static MethodInfo GetArrayRange
		{
			get
			{
				return _GetArrayRange ?? (_GetArrayRange = CallInstruction.CacheFunc<IList, int, int, RubyArray>(RubyOps.GetArrayRange));
			}
		}

		public static MethodInfo GetClassVariable
		{
			get
			{
				return _GetClassVariable ?? (_GetClassVariable = CallInstruction.CacheFunc<RubyScope, string, object>(RubyOps.GetClassVariable));
			}
		}

		public static MethodInfo GetContextFromBlockParam
		{
			get
			{
				return _GetContextFromBlockParam ?? (_GetContextFromBlockParam = CallInstruction.CacheFunc<BlockParam, RubyContext>(RubyOps.GetContextFromBlockParam));
			}
		}

		public static MethodInfo GetContextFromIRubyObject
		{
			get
			{
				return _GetContextFromIRubyObject ?? (_GetContextFromIRubyObject = CallInstruction.CacheFunc<IRubyObject, RubyContext>(RubyOps.GetContextFromIRubyObject));
			}
		}

		public static MethodInfo GetContextFromMethod
		{
			get
			{
				return _GetContextFromMethod ?? (_GetContextFromMethod = CallInstruction.CacheFunc<RubyMethod, RubyContext>(RubyOps.GetContextFromMethod));
			}
		}

		public static MethodInfo GetContextFromModule
		{
			get
			{
				return _GetContextFromModule ?? (_GetContextFromModule = CallInstruction.CacheFunc<RubyModule, RubyContext>(RubyOps.GetContextFromModule));
			}
		}

		public static MethodInfo GetContextFromProc
		{
			get
			{
				return _GetContextFromProc ?? (_GetContextFromProc = CallInstruction.CacheFunc<Proc, RubyContext>(RubyOps.GetContextFromProc));
			}
		}

		public static MethodInfo GetContextFromScope
		{
			get
			{
				return _GetContextFromScope ?? (_GetContextFromScope = CallInstruction.CacheFunc<RubyScope, RubyContext>(RubyOps.GetContextFromScope));
			}
		}

		public static MethodInfo GetCurrentException
		{
			get
			{
				return _GetCurrentException ?? (_GetCurrentException = CallInstruction.CacheFunc<RubyScope, Exception>(RubyOps.GetCurrentException));
			}
		}

		public static MethodInfo GetCurrentMatchData
		{
			get
			{
				return _GetCurrentMatchData ?? (_GetCurrentMatchData = CallInstruction.CacheFunc<RubyScope, MatchData>(RubyOps.GetCurrentMatchData));
			}
		}

		public static MethodInfo GetCurrentMatchGroup
		{
			get
			{
				return _GetCurrentMatchGroup ?? (_GetCurrentMatchGroup = CallInstruction.CacheFunc<RubyScope, int, MutableString>(RubyOps.GetCurrentMatchGroup));
			}
		}

		public static MethodInfo GetCurrentMatchLastGroup
		{
			get
			{
				return _GetCurrentMatchLastGroup ?? (_GetCurrentMatchLastGroup = CallInstruction.CacheFunc<RubyScope, MutableString>(RubyOps.GetCurrentMatchLastGroup));
			}
		}

		public static MethodInfo GetCurrentPostMatch
		{
			get
			{
				return _GetCurrentPostMatch ?? (_GetCurrentPostMatch = CallInstruction.CacheFunc<RubyScope, MutableString>(RubyOps.GetCurrentPostMatch));
			}
		}

		public static MethodInfo GetCurrentPreMatch
		{
			get
			{
				return _GetCurrentPreMatch ?? (_GetCurrentPreMatch = CallInstruction.CacheFunc<RubyScope, MutableString>(RubyOps.GetCurrentPreMatch));
			}
		}

		public static MethodInfo GetDefaultExceptionMessage
		{
			get
			{
				return _GetDefaultExceptionMessage ?? (_GetDefaultExceptionMessage = CallInstruction.CacheFunc<RubyClass, string>(RubyOps.GetDefaultExceptionMessage));
			}
		}

		public static MethodInfo GetEmptyScope
		{
			get
			{
				return _GetEmptyScope ?? (_GetEmptyScope = CallInstruction.CacheFunc<RubyContext, RubyScope>(RubyOps.GetEmptyScope));
			}
		}

		public static MethodInfo GetExpressionQualifiedConstant
		{
			get
			{
				return _GetExpressionQualifiedConstant ?? (_GetExpressionQualifiedConstant = CallInstruction.CacheFunc<object, RubyScope, ExpressionQualifiedConstantSiteCache, string[], object>(RubyOps.GetExpressionQualifiedConstant));
			}
		}

		public static MethodInfo GetGlobalMissingConstant
		{
			get
			{
				return _GetGlobalMissingConstant ?? (_GetGlobalMissingConstant = CallInstruction.CacheFunc<RubyScope, ConstantSiteCache, string, object>(RubyOps.GetGlobalMissingConstant));
			}
		}

		public static MethodInfo GetGlobalScopeFromScope
		{
			get
			{
				return _GetGlobalScopeFromScope ?? (_GetGlobalScopeFromScope = CallInstruction.CacheFunc<RubyScope, Scope>(RubyOps.GetGlobalScopeFromScope));
			}
		}

		public static MethodInfo GetGlobalVariable
		{
			get
			{
				return _GetGlobalVariable ?? (_GetGlobalVariable = CallInstruction.CacheFunc<RubyScope, string, object>(RubyOps.GetGlobalVariable));
			}
		}

		public static MethodInfo GetInstanceData
		{
			get
			{
				return _GetInstanceData ?? (_GetInstanceData = GetMethod(typeof(RubyOps), "GetInstanceData"));
			}
		}

		public static MethodInfo GetInstanceVariable
		{
			get
			{
				return _GetInstanceVariable ?? (_GetInstanceVariable = CallInstruction.CacheFunc<RubyScope, object, string, object>(RubyOps.GetInstanceVariable));
			}
		}

		public static MethodInfo GetLocals
		{
			get
			{
				return _GetLocals ?? (_GetLocals = CallInstruction.CacheFunc<RubyScope, MutableTuple>(RubyOps.GetLocals));
			}
		}

		public static MethodInfo GetLocalVariable
		{
			get
			{
				return _GetLocalVariable ?? (_GetLocalVariable = CallInstruction.CacheFunc<RubyScope, string, object>(RubyOps.GetLocalVariable));
			}
		}

		public static MethodInfo GetMetaObject
		{
			get
			{
				return _GetMetaObject ?? (_GetMetaObject = CallInstruction.CacheFunc<IRubyObject, Expression, DynamicMetaObject>(RubyOps.GetMetaObject));
			}
		}

		public static MethodInfo GetMethodBlockParameter
		{
			get
			{
				return _GetMethodBlockParameter ?? (_GetMethodBlockParameter = CallInstruction.CacheFunc<RubyScope, Proc>(RubyOps.GetMethodBlockParameter));
			}
		}

		public static MethodInfo GetMethodBlockParameterSelf
		{
			get
			{
				return _GetMethodBlockParameterSelf ?? (_GetMethodBlockParameterSelf = CallInstruction.CacheFunc<RubyScope, object>(RubyOps.GetMethodBlockParameterSelf));
			}
		}

		public static MethodInfo GetMethodUnwinderReturnValue
		{
			get
			{
				return _GetMethodUnwinderReturnValue ?? (_GetMethodUnwinderReturnValue = CallInstruction.CacheFunc<Exception, object>(RubyOps.GetMethodUnwinderReturnValue));
			}
		}

		public static MethodInfo GetMissingConstant
		{
			get
			{
				return _GetMissingConstant ?? (_GetMissingConstant = CallInstruction.CacheFunc<RubyScope, ConstantSiteCache, string, object>(RubyOps.GetMissingConstant));
			}
		}

		public static MethodInfo GetMutableStringBytes
		{
			get
			{
				return _GetMutableStringBytes ?? (_GetMutableStringBytes = CallInstruction.CacheFunc<MutableString, byte[]>(RubyOps.GetMutableStringBytes));
			}
		}

		public static MethodInfo GetParentLocals
		{
			get
			{
				return _GetParentLocals ?? (_GetParentLocals = CallInstruction.CacheFunc<RubyScope, MutableTuple>(RubyOps.GetParentLocals));
			}
		}

		public static MethodInfo GetParentScope
		{
			get
			{
				return _GetParentScope ?? (_GetParentScope = CallInstruction.CacheFunc<RubyScope, RubyScope>(RubyOps.GetParentScope));
			}
		}

		public static MethodInfo GetProcArity
		{
			get
			{
				return _GetProcArity ?? (_GetProcArity = CallInstruction.CacheFunc<Proc, int>(RubyOps.GetProcArity));
			}
		}

		public static MethodInfo GetProcSelf
		{
			get
			{
				return _GetProcSelf ?? (_GetProcSelf = CallInstruction.CacheFunc<Proc, object>(RubyOps.GetProcSelf));
			}
		}

		public static MethodInfo GetQualifiedConstant
		{
			get
			{
				return _GetQualifiedConstant ?? (_GetQualifiedConstant = CallInstruction.CacheFunc<RubyScope, ConstantSiteCache, string[], bool, object>(RubyOps.GetQualifiedConstant));
			}
		}

		public static MethodInfo GetSelfClassVersionHandle
		{
			get
			{
				return _GetSelfClassVersionHandle ?? (_GetSelfClassVersionHandle = CallInstruction.CacheFunc<RubyScope, VersionHandle>(RubyOps.GetSelfClassVersionHandle));
			}
		}

		public static MethodInfo GetSuperCallTarget
		{
			get
			{
				return _GetSuperCallTarget ?? (_GetSuperCallTarget = CallInstruction.CacheFunc<RubyScope, int, object>(RubyOps.GetSuperCallTarget));
			}
		}

		public static MethodInfo GetTrailingArrayItem
		{
			get
			{
				return _GetTrailingArrayItem ?? (_GetTrailingArrayItem = CallInstruction.CacheFunc<IList, int, int, object>(RubyOps.GetTrailingArrayItem));
			}
		}

		public static MethodInfo GetUnqualifiedConstant
		{
			get
			{
				return _GetUnqualifiedConstant ?? (_GetUnqualifiedConstant = CallInstruction.CacheFunc<RubyScope, ConstantSiteCache, string, bool, object>(RubyOps.GetUnqualifiedConstant));
			}
		}

		public static MethodInfo HookupEvent
		{
			get
			{
				return _HookupEvent ?? (_HookupEvent = CallInstruction.CacheFunc<RubyEventInfo, object, Proc, Proc>(RubyOps.HookupEvent));
			}
		}

		public static MethodInfo InitializeBlock
		{
			get
			{
				return _InitializeBlock ?? (_InitializeBlock = CallInstruction.CacheAction<Proc>(RubyOps.InitializeBlock));
			}
		}

		public static MethodInfo InitializeScope
		{
			get
			{
				return _InitializeScope ?? (_InitializeScope = CallInstruction.CacheAction<RubyScope, MutableTuple, string[], InterpretedFrame>(RubyOps.InitializeScope));
			}
		}

		public static MethodInfo InitializeScopeNoLocals
		{
			get
			{
				return _InitializeScopeNoLocals ?? (_InitializeScopeNoLocals = CallInstruction.CacheAction<RubyScope, InterpretedFrame>(RubyOps.InitializeScopeNoLocals));
			}
		}

		public static MethodInfo InstantiateBlock
		{
			get
			{
				return _InstantiateBlock ?? (_InstantiateBlock = CallInstruction.CacheFunc<RubyScope, object, BlockDispatcher, Proc>(RubyOps.InstantiateBlock));
			}
		}

		public static MethodInfo InstantiateLambda
		{
			get
			{
				return _InstantiateLambda ?? (_InstantiateLambda = CallInstruction.CacheFunc<RubyScope, object, BlockDispatcher, Proc>(RubyOps.InstantiateLambda));
			}
		}

		public static MethodInfo IntegerValue_ToUInt32Unchecked
		{
			get
			{
				return _IntegerValue_ToUInt32Unchecked ?? (_IntegerValue_ToUInt32Unchecked = GetMethod(typeof(IntegerValue), "ToUInt32Unchecked"));
			}
		}

		public static MethodInfo IRubyObject_BaseEquals
		{
			get
			{
				return _IRubyObject_BaseEquals ?? (_IRubyObject_BaseEquals = GetMethod(typeof(IRubyObject), "BaseEquals"));
			}
		}

		public static MethodInfo IRubyObject_BaseGetHashCode
		{
			get
			{
				return _IRubyObject_BaseGetHashCode ?? (_IRubyObject_BaseGetHashCode = GetMethod(typeof(IRubyObject), "BaseGetHashCode"));
			}
		}

		public static MethodInfo IRubyObject_BaseToString
		{
			get
			{
				return _IRubyObject_BaseToString ?? (_IRubyObject_BaseToString = GetMethod(typeof(IRubyObject), "BaseToString"));
			}
		}

		public static MethodInfo IRubyObject_get_ImmediateClass
		{
			get
			{
				return _IRubyObject_get_ImmediateClass ?? (_IRubyObject_get_ImmediateClass = GetMethod(typeof(IRubyObject), "get_ImmediateClass"));
			}
		}

		public static MethodInfo IRubyObject_GetInstanceData
		{
			get
			{
				return _IRubyObject_GetInstanceData ?? (_IRubyObject_GetInstanceData = GetMethod(typeof(IRubyObject), "GetInstanceData"));
			}
		}

		public static MethodInfo IRubyObject_set_ImmediateClass
		{
			get
			{
				return _IRubyObject_set_ImmediateClass ?? (_IRubyObject_set_ImmediateClass = GetMethod(typeof(IRubyObject), "set_ImmediateClass"));
			}
		}

		public static MethodInfo IRubyObject_TryGetInstanceData
		{
			get
			{
				return _IRubyObject_TryGetInstanceData ?? (_IRubyObject_TryGetInstanceData = GetMethod(typeof(IRubyObject), "TryGetInstanceData"));
			}
		}

		public static MethodInfo IRubyObjectState_Freeze
		{
			get
			{
				return _IRubyObjectState_Freeze ?? (_IRubyObjectState_Freeze = GetMethod(typeof(IRubyObjectState), "Freeze"));
			}
		}

		public static MethodInfo IRubyObjectState_get_IsFrozen
		{
			get
			{
				return _IRubyObjectState_get_IsFrozen ?? (_IRubyObjectState_get_IsFrozen = GetMethod(typeof(IRubyObjectState), "get_IsFrozen"));
			}
		}

		public static MethodInfo IRubyObjectState_get_IsTainted
		{
			get
			{
				return _IRubyObjectState_get_IsTainted ?? (_IRubyObjectState_get_IsTainted = GetMethod(typeof(IRubyObjectState), "get_IsTainted"));
			}
		}

		public static MethodInfo IRubyObjectState_get_IsUntrusted
		{
			get
			{
				return _IRubyObjectState_get_IsUntrusted ?? (_IRubyObjectState_get_IsUntrusted = GetMethod(typeof(IRubyObjectState), "get_IsUntrusted"));
			}
		}

		public static MethodInfo IRubyObjectState_set_IsTainted
		{
			get
			{
				return _IRubyObjectState_set_IsTainted ?? (_IRubyObjectState_set_IsTainted = GetMethod(typeof(IRubyObjectState), "set_IsTainted"));
			}
		}

		public static MethodInfo IRubyObjectState_set_IsUntrusted
		{
			get
			{
				return _IRubyObjectState_set_IsUntrusted ?? (_IRubyObjectState_set_IsUntrusted = GetMethod(typeof(IRubyObjectState), "set_IsUntrusted"));
			}
		}

		public static MethodInfo IsClrNonSingletonRuleValid
		{
			get
			{
				return _IsClrNonSingletonRuleValid ?? (_IsClrNonSingletonRuleValid = CallInstruction.CacheFunc<RubyContext, object, VersionHandle, int, bool>(RubyOps.IsClrNonSingletonRuleValid));
			}
		}

		public static MethodInfo IsClrSingletonRuleValid
		{
			get
			{
				return _IsClrSingletonRuleValid ?? (_IsClrSingletonRuleValid = CallInstruction.CacheFunc<RubyContext, object, int, bool>(RubyOps.IsClrSingletonRuleValid));
			}
		}

		public static MethodInfo IsDefinedClassVariable
		{
			get
			{
				return _IsDefinedClassVariable ?? (_IsDefinedClassVariable = CallInstruction.CacheFunc<RubyScope, string, bool>(RubyOps.IsDefinedClassVariable));
			}
		}

		public static MethodInfo IsDefinedExpressionQualifiedConstant
		{
			get
			{
				return _IsDefinedExpressionQualifiedConstant ?? (_IsDefinedExpressionQualifiedConstant = CallInstruction.CacheFunc<object, RubyScope, ExpressionQualifiedIsDefinedConstantSiteCache, string[], bool>(RubyOps.IsDefinedExpressionQualifiedConstant));
			}
		}

		public static MethodInfo IsDefinedGlobalConstant
		{
			get
			{
				return _IsDefinedGlobalConstant ?? (_IsDefinedGlobalConstant = CallInstruction.CacheFunc<RubyScope, IsDefinedConstantSiteCache, string, bool>(RubyOps.IsDefinedGlobalConstant));
			}
		}

		public static MethodInfo IsDefinedGlobalVariable
		{
			get
			{
				return _IsDefinedGlobalVariable ?? (_IsDefinedGlobalVariable = CallInstruction.CacheFunc<RubyScope, string, bool>(RubyOps.IsDefinedGlobalVariable));
			}
		}

		public static MethodInfo IsDefinedInstanceVariable
		{
			get
			{
				return _IsDefinedInstanceVariable ?? (_IsDefinedInstanceVariable = CallInstruction.CacheFunc<RubyScope, object, string, bool>(RubyOps.IsDefinedInstanceVariable));
			}
		}

		public static MethodInfo IsDefinedQualifiedConstant
		{
			get
			{
				return _IsDefinedQualifiedConstant ?? (_IsDefinedQualifiedConstant = CallInstruction.CacheFunc<RubyScope, IsDefinedConstantSiteCache, string[], bool, bool>(RubyOps.IsDefinedQualifiedConstant));
			}
		}

		public static MethodInfo IsDefinedUnqualifiedConstant
		{
			get
			{
				return _IsDefinedUnqualifiedConstant ?? (_IsDefinedUnqualifiedConstant = CallInstruction.CacheFunc<RubyScope, IsDefinedConstantSiteCache, string, bool>(RubyOps.IsDefinedUnqualifiedConstant));
			}
		}

		public static MethodInfo IsFalse
		{
			get
			{
				return _IsFalse ?? (_IsFalse = CallInstruction.CacheFunc<object, bool>(RubyOps.IsFalse));
			}
		}

		public static MethodInfo IsMethodUnwinderTargetFrame
		{
			get
			{
				return _IsMethodUnwinderTargetFrame ?? (_IsMethodUnwinderTargetFrame = CallInstruction.CacheFunc<RubyScope, Exception, bool>(RubyOps.IsMethodUnwinderTargetFrame));
			}
		}

		public static MethodInfo IsObjectFrozen
		{
			get
			{
				return _IsObjectFrozen ?? (_IsObjectFrozen = CallInstruction.CacheFunc<RubyInstanceData, bool>(RubyOps.IsObjectFrozen));
			}
		}

		public static MethodInfo IsObjectTainted
		{
			get
			{
				return _IsObjectTainted ?? (_IsObjectTainted = CallInstruction.CacheFunc<RubyInstanceData, bool>(RubyOps.IsObjectTainted));
			}
		}

		public static MethodInfo IsObjectUntrusted
		{
			get
			{
				return _IsObjectUntrusted ?? (_IsObjectUntrusted = CallInstruction.CacheFunc<RubyInstanceData, bool>(RubyOps.IsObjectUntrusted));
			}
		}

		public static MethodInfo IsProcConverterTarget
		{
			get
			{
				return _IsProcConverterTarget ?? (_IsProcConverterTarget = CallInstruction.CacheFunc<BlockParam, MethodUnwinder, bool>(RubyOps.IsProcConverterTarget));
			}
		}

		public static MethodInfo IsRetrySingleton
		{
			get
			{
				return _IsRetrySingleton ?? (_IsRetrySingleton = CallInstruction.CacheFunc<object, bool>(RubyOps.IsRetrySingleton));
			}
		}

		public static MethodInfo IsSuperOutOfMethodScope
		{
			get
			{
				return _IsSuperOutOfMethodScope ?? (_IsSuperOutOfMethodScope = CallInstruction.CacheFunc<RubyScope, bool>(RubyOps.IsSuperOutOfMethodScope));
			}
		}

		public static MethodInfo IsTrue
		{
			get
			{
				return _IsTrue ?? (_IsTrue = CallInstruction.CacheFunc<object, bool>(RubyOps.IsTrue));
			}
		}

		public static MethodInfo LeaveLoop
		{
			get
			{
				return _LeaveLoop ?? (_LeaveLoop = CallInstruction.CacheAction<RubyScope>(RubyOps.LeaveLoop));
			}
		}

		public static MethodInfo LeaveMethodFrame
		{
			get
			{
				return _LeaveMethodFrame ?? (_LeaveMethodFrame = CallInstruction.CacheAction<RuntimeFlowControl>(RubyOps.LeaveMethodFrame));
			}
		}

		public static MethodInfo LeaveProcConverter
		{
			get
			{
				return _LeaveProcConverter ?? (_LeaveProcConverter = CallInstruction.CacheAction<BlockParam>(RubyOps.LeaveProcConverter));
			}
		}

		public static MethodInfo LeaveRescue
		{
			get
			{
				return _LeaveRescue ?? (_LeaveRescue = CallInstruction.CacheAction<RubyScope>(RubyOps.LeaveRescue));
			}
		}

		public static MethodInfo MakeAbstractMethodCalledError
		{
			get
			{
				return _MakeAbstractMethodCalledError ?? (_MakeAbstractMethodCalledError = CallInstruction.CacheFunc<RuntimeMethodHandle, Exception>(RubyOps.MakeAbstractMethodCalledError));
			}
		}

		public static MethodInfo MakeAllocatorUndefinedError
		{
			get
			{
				return _MakeAllocatorUndefinedError ?? (_MakeAllocatorUndefinedError = CallInstruction.CacheFunc<RubyClass, Exception>(RubyOps.MakeAllocatorUndefinedError));
			}
		}

		public static MethodInfo MakeAmbiguousMatchError
		{
			get
			{
				return _MakeAmbiguousMatchError ?? (_MakeAmbiguousMatchError = CallInstruction.CacheFunc<string, Exception>(RubyOps.MakeAmbiguousMatchError));
			}
		}

		public static MethodInfo MakeArray0
		{
			get
			{
				return _MakeArray0 ?? (_MakeArray0 = CallInstruction.CacheFunc(RubyOps.MakeArray0));
			}
		}

		public static MethodInfo MakeArray1
		{
			get
			{
				return _MakeArray1 ?? (_MakeArray1 = CallInstruction.CacheFunc<object, RubyArray>(RubyOps.MakeArray1));
			}
		}

		public static MethodInfo MakeArray2
		{
			get
			{
				return _MakeArray2 ?? (_MakeArray2 = CallInstruction.CacheFunc<object, object, RubyArray>(RubyOps.MakeArray2));
			}
		}

		public static MethodInfo MakeArray3
		{
			get
			{
				return _MakeArray3 ?? (_MakeArray3 = CallInstruction.CacheFunc<object, object, object, RubyArray>(RubyOps.MakeArray3));
			}
		}

		public static MethodInfo MakeArray4
		{
			get
			{
				return _MakeArray4 ?? (_MakeArray4 = CallInstruction.CacheFunc<object, object, object, object, RubyArray>(RubyOps.MakeArray4));
			}
		}

		public static MethodInfo MakeArray5
		{
			get
			{
				return _MakeArray5 ?? (_MakeArray5 = CallInstruction.CacheFunc<object, object, object, object, object, RubyArray>(RubyOps.MakeArray5));
			}
		}

		public static MethodInfo MakeArrayN
		{
			get
			{
				return _MakeArrayN ?? (_MakeArrayN = CallInstruction.CacheFunc<object[], RubyArray>(RubyOps.MakeArrayN));
			}
		}

		public static MethodInfo MakeClrProtectedMethodCalledError
		{
			get
			{
				return _MakeClrProtectedMethodCalledError ?? (_MakeClrProtectedMethodCalledError = CallInstruction.CacheFunc<RubyContext, object, string, Exception>(RubyOps.MakeClrProtectedMethodCalledError));
			}
		}

		public static MethodInfo MakeClrVirtualMethodCalledError
		{
			get
			{
				return _MakeClrVirtualMethodCalledError ?? (_MakeClrVirtualMethodCalledError = CallInstruction.CacheFunc<RubyContext, object, string, Exception>(RubyOps.MakeClrVirtualMethodCalledError));
			}
		}

		public static MethodInfo MakeConstructorUndefinedError
		{
			get
			{
				return _MakeConstructorUndefinedError ?? (_MakeConstructorUndefinedError = CallInstruction.CacheFunc<RubyClass, Exception>(RubyOps.MakeConstructorUndefinedError));
			}
		}

		public static MethodInfo MakeHash
		{
			get
			{
				return _MakeHash ?? (_MakeHash = CallInstruction.CacheFunc<RubyScope, object[], Hash>(RubyOps.MakeHash));
			}
		}

		public static MethodInfo MakeHash0
		{
			get
			{
				return _MakeHash0 ?? (_MakeHash0 = CallInstruction.CacheFunc<RubyScope, Hash>(RubyOps.MakeHash0));
			}
		}

		public static MethodInfo MakeImplicitSuperInBlockMethodError
		{
			get
			{
				return _MakeImplicitSuperInBlockMethodError ?? (_MakeImplicitSuperInBlockMethodError = CallInstruction.CacheFunc(RubyOps.MakeImplicitSuperInBlockMethodError));
			}
		}

		public static MethodInfo MakeInvalidArgumentTypesError
		{
			get
			{
				return _MakeInvalidArgumentTypesError ?? (_MakeInvalidArgumentTypesError = CallInstruction.CacheFunc<string, Exception>(RubyOps.MakeInvalidArgumentTypesError));
			}
		}

		public static MethodInfo MakeMissingDefaultConstructorError
		{
			get
			{
				return _MakeMissingDefaultConstructorError ?? (_MakeMissingDefaultConstructorError = CallInstruction.CacheFunc<RubyClass, string, Exception>(RubyOps.MakeMissingDefaultConstructorError));
			}
		}

		public static MethodInfo MakeMissingMemberError
		{
			get
			{
				return _MakeMissingMemberError ?? (_MakeMissingMemberError = CallInstruction.CacheFunc<string, Exception>(RubyOps.MakeMissingMemberError));
			}
		}

		public static MethodInfo MakeMissingMethodError
		{
			get
			{
				return _MakeMissingMethodError ?? (_MakeMissingMethodError = CallInstruction.CacheFunc<RubyContext, object, string, Exception>(RubyOps.MakeMissingMethodError));
			}
		}

		public static MethodInfo MakeMissingSuperException
		{
			get
			{
				return _MakeMissingSuperException ?? (_MakeMissingSuperException = CallInstruction.CacheFunc<string, Exception>(RubyOps.MakeMissingSuperException));
			}
		}

		public static MethodInfo MakeNotClrTypeError
		{
			get
			{
				return _MakeNotClrTypeError ?? (_MakeNotClrTypeError = CallInstruction.CacheFunc<RubyClass, Exception>(RubyOps.MakeNotClrTypeError));
			}
		}

		public static MethodInfo MakePrivateMethodCalledError
		{
			get
			{
				return _MakePrivateMethodCalledError ?? (_MakePrivateMethodCalledError = CallInstruction.CacheFunc<RubyContext, object, string, Exception>(RubyOps.MakePrivateMethodCalledError));
			}
		}

		public static MethodInfo MakeProtectedMethodCalledError
		{
			get
			{
				return _MakeProtectedMethodCalledError ?? (_MakeProtectedMethodCalledError = CallInstruction.CacheFunc<RubyContext, object, string, Exception>(RubyOps.MakeProtectedMethodCalledError));
			}
		}

		public static MethodInfo MakeTopLevelSuperException
		{
			get
			{
				return _MakeTopLevelSuperException ?? (_MakeTopLevelSuperException = CallInstruction.CacheFunc(RubyOps.MakeTopLevelSuperException));
			}
		}

		public static MethodInfo MakeTypeConversionError
		{
			get
			{
				return _MakeTypeConversionError ?? (_MakeTypeConversionError = CallInstruction.CacheFunc<RubyContext, object, Type, Exception>(RubyOps.MakeTypeConversionError));
			}
		}

		public static MethodInfo MakeVirtualClassInstantiatedError
		{
			get
			{
				return _MakeVirtualClassInstantiatedError ?? (_MakeVirtualClassInstantiatedError = CallInstruction.CacheFunc(RubyOps.MakeVirtualClassInstantiatedError));
			}
		}

		public static MethodInfo MakeWrongNumberOfArgumentsError
		{
			get
			{
				return _MakeWrongNumberOfArgumentsError ?? (_MakeWrongNumberOfArgumentsError = CallInstruction.CacheFunc<int, int, ArgumentException>(RubyOps.MakeWrongNumberOfArgumentsError));
			}
		}

		public static MethodInfo MarkException
		{
			get
			{
				return _MarkException ?? (_MarkException = CallInstruction.CacheFunc<Exception, Exception>(RubyOps.MarkException));
			}
		}

		public static MethodInfo MatchLastInputLine
		{
			get
			{
				return _MatchLastInputLine ?? (_MatchLastInputLine = CallInstruction.CacheFunc<RubyRegex, RubyScope, bool>(RubyOps.MatchLastInputLine));
			}
		}

		public static MethodInfo MatchString
		{
			get
			{
				return _MatchString ?? (_MatchString = CallInstruction.CacheFunc<MutableString, RubyRegex, RubyScope, object>(RubyOps.MatchString));
			}
		}

		public static MethodInfo MethodBreak
		{
			get
			{
				return _MethodBreak ?? (_MethodBreak = CallInstruction.CacheAction<object>(RubyOps.MethodBreak));
			}
		}

		public static MethodInfo MethodNext
		{
			get
			{
				return _MethodNext ?? (_MethodNext = CallInstruction.CacheAction<RubyScope, object>(RubyOps.MethodNext));
			}
		}

		public static MethodInfo MethodProcCall
		{
			get
			{
				return _MethodProcCall ?? (_MethodProcCall = CallInstruction.CacheFunc<BlockParam, object, object>(RubyOps.MethodProcCall));
			}
		}

		public static MethodInfo MethodPropagateReturn
		{
			get
			{
				return _MethodPropagateReturn ?? (_MethodPropagateReturn = CallInstruction.CacheFunc<RubyScope, Proc, BlockReturnResult, object>(RubyOps.MethodPropagateReturn));
			}
		}

		public static MethodInfo MethodRedo
		{
			get
			{
				return _MethodRedo ?? (_MethodRedo = CallInstruction.CacheAction<RubyScope>(RubyOps.MethodRedo));
			}
		}

		public static MethodInfo MethodRetry
		{
			get
			{
				return _MethodRetry ?? (_MethodRetry = CallInstruction.CacheFunc<RubyScope, Proc, object>(RubyOps.MethodRetry));
			}
		}

		public static MethodInfo MethodYield
		{
			get
			{
				return _MethodYield ?? (_MethodYield = CallInstruction.CacheFunc<RubyScope, BlockParam, object, bool>(RubyOps.MethodYield));
			}
		}

		public static MethodInfo NullIfFalse
		{
			get
			{
				return _NullIfFalse ?? (_NullIfFalse = CallInstruction.CacheFunc<object, object>(RubyOps.NullIfFalse));
			}
		}

		public static MethodInfo NullIfTrue
		{
			get
			{
				return _NullIfTrue ?? (_NullIfTrue = CallInstruction.CacheFunc<object, object>(RubyOps.NullIfTrue));
			}
		}

		public static MethodInfo ObjectToMutableString
		{
			get
			{
				return _ObjectToMutableString ?? (_ObjectToMutableString = CallInstruction.CacheFunc<object, MutableString>(RubyOps.ObjectToMutableString));
			}
		}

		public static MethodInfo ObjectToString
		{
			get
			{
				return _ObjectToString ?? (_ObjectToString = CallInstruction.CacheFunc<IRubyObject, string>(RubyOps.ObjectToString));
			}
		}

		public static MethodInfo PrintInteractiveResult
		{
			get
			{
				return _PrintInteractiveResult ?? (_PrintInteractiveResult = CallInstruction.CacheAction<RubyScope, MutableString>(RubyOps.PrintInteractiveResult));
			}
		}

		public static MethodInfo PropagateRetrySingleton
		{
			get
			{
				return _PropagateRetrySingleton ?? (_PropagateRetrySingleton = CallInstruction.CacheFunc<object, object, object>(RubyOps.PropagateRetrySingleton));
			}
		}

		public static MethodInfo RegisterShutdownHandler
		{
			get
			{
				return _RegisterShutdownHandler ?? (_RegisterShutdownHandler = CallInstruction.CacheAction<Proc>(RubyOps.RegisterShutdownHandler));
			}
		}

		public static MethodInfo RubyStruct_GetValue
		{
			get
			{
				return _RubyStruct_GetValue ?? (_RubyStruct_GetValue = CallInstruction.CacheFunc<RubyStruct, int, object>(RubyStruct.GetValue));
			}
		}

		public static MethodInfo RubyStruct_SetValue
		{
			get
			{
				return _RubyStruct_SetValue ?? (_RubyStruct_SetValue = CallInstruction.CacheFunc<RubyStruct, int, object, object>(RubyStruct.SetValue));
			}
		}

		public static MethodInfo SerializeObject
		{
			get
			{
				return _SerializeObject ?? (_SerializeObject = GetMethod(typeof(RubyOps), "SerializeObject"));
			}
		}

		public static MethodInfo SetClassVariable
		{
			get
			{
				return _SetClassVariable ?? (_SetClassVariable = CallInstruction.CacheFunc<object, RubyScope, string, object>(RubyOps.SetClassVariable));
			}
		}

		public static MethodInfo SetCurrentException
		{
			get
			{
				return _SetCurrentException ?? (_SetCurrentException = CallInstruction.CacheAction<RubyScope, Exception>(RubyOps.SetCurrentException));
			}
		}

		public static MethodInfo SetDataConstant
		{
			get
			{
				return _SetDataConstant ?? (_SetDataConstant = CallInstruction.CacheAction<RubyScope, string, int>(RubyOps.SetDataConstant));
			}
		}

		public static MethodInfo SetGlobalConstant
		{
			get
			{
				return _SetGlobalConstant ?? (_SetGlobalConstant = CallInstruction.CacheFunc<object, RubyScope, string, object>(RubyOps.SetGlobalConstant));
			}
		}

		public static MethodInfo SetGlobalVariable
		{
			get
			{
				return _SetGlobalVariable ?? (_SetGlobalVariable = CallInstruction.CacheFunc<object, RubyScope, string, object>(RubyOps.SetGlobalVariable));
			}
		}

		public static MethodInfo SetInstanceVariable
		{
			get
			{
				return _SetInstanceVariable ?? (_SetInstanceVariable = CallInstruction.CacheFunc<object, object, RubyScope, string, object>(RubyOps.SetInstanceVariable));
			}
		}

		public static MethodInfo SetLocalVariable
		{
			get
			{
				return _SetLocalVariable ?? (_SetLocalVariable = CallInstruction.CacheFunc<object, RubyScope, string, object>(RubyOps.SetLocalVariable));
			}
		}

		public static MethodInfo SetObjectTaint
		{
			get
			{
				return _SetObjectTaint ?? (_SetObjectTaint = GetMethod(typeof(RubyOps), "SetObjectTaint"));
			}
		}

		public static MethodInfo SetObjectTrustiness
		{
			get
			{
				return _SetObjectTrustiness ?? (_SetObjectTrustiness = GetMethod(typeof(RubyOps), "SetObjectTrustiness"));
			}
		}

		public static MethodInfo SetQualifiedConstant
		{
			get
			{
				return _SetQualifiedConstant ?? (_SetQualifiedConstant = CallInstruction.CacheFunc<object, object, RubyScope, string, object>(RubyOps.SetQualifiedConstant));
			}
		}

		public static MethodInfo SetUnqualifiedConstant
		{
			get
			{
				return _SetUnqualifiedConstant ?? (_SetUnqualifiedConstant = CallInstruction.CacheFunc<object, RubyScope, string, object>(RubyOps.SetUnqualifiedConstant));
			}
		}

		public static MethodInfo Splat
		{
			get
			{
				return _Splat ?? (_Splat = CallInstruction.CacheFunc<IList, object>(RubyOps.Splat));
			}
		}

		public static MethodInfo SplatAppend
		{
			get
			{
				return _SplatAppend ?? (_SplatAppend = CallInstruction.CacheFunc<IList, IList, IList>(RubyOps.SplatAppend));
			}
		}

		public static MethodInfo SplatPair
		{
			get
			{
				return _SplatPair ?? (_SplatPair = CallInstruction.CacheFunc<object, IList, object>(RubyOps.SplatPair));
			}
		}

		public static MethodInfo StringToMutableString
		{
			get
			{
				return _StringToMutableString ?? (_StringToMutableString = CallInstruction.CacheFunc<string, MutableString>(RubyOps.StringToMutableString));
			}
		}

		public static MethodInfo ToArrayValidator
		{
			get
			{
				return _ToArrayValidator ?? (_ToArrayValidator = CallInstruction.CacheFunc<string, object, IList>(RubyOps.ToArrayValidator));
			}
		}

		public static MethodInfo ToAValidator
		{
			get
			{
				return _ToAValidator ?? (_ToAValidator = CallInstruction.CacheFunc<string, object, IList>(RubyOps.ToAValidator));
			}
		}

		public static MethodInfo ToBignumValidator
		{
			get
			{
				return _ToBignumValidator ?? (_ToBignumValidator = CallInstruction.CacheFunc<string, object, BigInteger>(RubyOps.ToBignumValidator));
			}
		}

		public static MethodInfo ToByteValidator
		{
			get
			{
				return _ToByteValidator ?? (_ToByteValidator = CallInstruction.CacheFunc<string, object, byte>(RubyOps.ToByteValidator));
			}
		}

		public static MethodInfo ToDoubleValidator
		{
			get
			{
				return _ToDoubleValidator ?? (_ToDoubleValidator = CallInstruction.CacheFunc<string, object, double>(RubyOps.ToDoubleValidator));
			}
		}

		public static MethodInfo ToFixnumValidator
		{
			get
			{
				return _ToFixnumValidator ?? (_ToFixnumValidator = CallInstruction.CacheFunc<string, object, int>(RubyOps.ToFixnumValidator));
			}
		}

		public static MethodInfo ToHashValidator
		{
			get
			{
				return _ToHashValidator ?? (_ToHashValidator = CallInstruction.CacheFunc<string, object, IDictionary<object, object>>(RubyOps.ToHashValidator));
			}
		}

		public static MethodInfo ToInt16Validator
		{
			get
			{
				return _ToInt16Validator ?? (_ToInt16Validator = CallInstruction.CacheFunc<string, object, short>(RubyOps.ToInt16Validator));
			}
		}

		public static MethodInfo ToInt64Validator
		{
			get
			{
				return _ToInt64Validator ?? (_ToInt64Validator = CallInstruction.CacheFunc<string, object, long>(RubyOps.ToInt64Validator));
			}
		}

		public static MethodInfo ToIntegerValidator
		{
			get
			{
				return _ToIntegerValidator ?? (_ToIntegerValidator = CallInstruction.CacheFunc<string, object, IntegerValue>(RubyOps.ToIntegerValidator));
			}
		}

		public static MethodInfo ToProcValidator
		{
			get
			{
				return _ToProcValidator ?? (_ToProcValidator = CallInstruction.CacheFunc<string, object, Proc>(RubyOps.ToProcValidator));
			}
		}

		public static MethodInfo ToRegexValidator
		{
			get
			{
				return _ToRegexValidator ?? (_ToRegexValidator = CallInstruction.CacheFunc<string, object, RubyRegex>(RubyOps.ToRegexValidator));
			}
		}

		public static MethodInfo ToSByteValidator
		{
			get
			{
				return _ToSByteValidator ?? (_ToSByteValidator = CallInstruction.CacheFunc<string, object, sbyte>(RubyOps.ToSByteValidator));
			}
		}

		public static MethodInfo ToSDefaultConversion
		{
			get
			{
				return _ToSDefaultConversion ?? (_ToSDefaultConversion = CallInstruction.CacheFunc<RubyContext, object, object, MutableString>(RubyOps.ToSDefaultConversion));
			}
		}

		public static MethodInfo ToSingleValidator
		{
			get
			{
				return _ToSingleValidator ?? (_ToSingleValidator = CallInstruction.CacheFunc<string, object, float>(RubyOps.ToSingleValidator));
			}
		}

		public static MethodInfo ToStringValidator
		{
			get
			{
				return _ToStringValidator ?? (_ToStringValidator = CallInstruction.CacheFunc<string, object, MutableString>(RubyOps.ToStringValidator));
			}
		}

		public static MethodInfo ToSymbolValidator
		{
			get
			{
				return _ToSymbolValidator ?? (_ToSymbolValidator = CallInstruction.CacheFunc<string, object, string>(RubyOps.ToSymbolValidator));
			}
		}

		public static MethodInfo ToUInt16Validator
		{
			get
			{
				return _ToUInt16Validator ?? (_ToUInt16Validator = CallInstruction.CacheFunc<string, object, ushort>(RubyOps.ToUInt16Validator));
			}
		}

		public static MethodInfo ToUInt32Validator
		{
			get
			{
				return _ToUInt32Validator ?? (_ToUInt32Validator = CallInstruction.CacheFunc<string, object, uint>(RubyOps.ToUInt32Validator));
			}
		}

		public static MethodInfo ToUInt64Validator
		{
			get
			{
				return _ToUInt64Validator ?? (_ToUInt64Validator = CallInstruction.CacheFunc<string, object, ulong>(RubyOps.ToUInt64Validator));
			}
		}

		public static MethodInfo TraceBlockCall
		{
			get
			{
				return _TraceBlockCall ?? (_TraceBlockCall = CallInstruction.CacheAction<RubyBlockScope, BlockParam, string, int>(RubyOps.TraceBlockCall));
			}
		}

		public static MethodInfo TraceBlockReturn
		{
			get
			{
				return _TraceBlockReturn ?? (_TraceBlockReturn = CallInstruction.CacheAction<RubyBlockScope, BlockParam, string, int>(RubyOps.TraceBlockReturn));
			}
		}

		public static MethodInfo TraceMethodCall
		{
			get
			{
				return _TraceMethodCall ?? (_TraceMethodCall = CallInstruction.CacheAction<RubyMethodScope, string, int>(RubyOps.TraceMethodCall));
			}
		}

		public static MethodInfo TraceMethodReturn
		{
			get
			{
				return _TraceMethodReturn ?? (_TraceMethodReturn = CallInstruction.CacheAction<RubyMethodScope, string, int>(RubyOps.TraceMethodReturn));
			}
		}

		public static MethodInfo TraceTopLevelCodeFrame
		{
			get
			{
				return _TraceTopLevelCodeFrame ?? (_TraceTopLevelCodeFrame = CallInstruction.CacheFunc<RubyScope, Exception, bool>(RubyOps.TraceTopLevelCodeFrame));
			}
		}

		public static MethodInfo TryGetClassVariable
		{
			get
			{
				return _TryGetClassVariable ?? (_TryGetClassVariable = CallInstruction.CacheFunc<RubyScope, string, object>(RubyOps.TryGetClassVariable));
			}
		}

		public static MethodInfo UndefineMethod
		{
			get
			{
				return _UndefineMethod ?? (_UndefineMethod = CallInstruction.CacheAction<RubyScope, string>(RubyOps.UndefineMethod));
			}
		}

		public static MethodInfo Unsplat
		{
			get
			{
				return _Unsplat ?? (_Unsplat = CallInstruction.CacheFunc<object, IList>(RubyOps.Unsplat));
			}
		}

		public static MethodInfo UpdateProfileTicks
		{
			get
			{
				return _UpdateProfileTicks ?? (_UpdateProfileTicks = CallInstruction.CacheAction<int, long>(RubyOps.UpdateProfileTicks));
			}
		}

		public static MethodInfo X
		{
			get
			{
				return _X ?? (_X = CallInstruction.CacheAction<string>(RubyOps.X));
			}
		}

		public static MethodInfo Yield0
		{
			get
			{
				return _Yield0 ?? (_Yield0 = CallInstruction.CacheFunc<Proc, object, BlockParam, object>(RubyOps.Yield0));
			}
		}

		public static MethodInfo Yield1
		{
			get
			{
				return _Yield1 ?? (_Yield1 = CallInstruction.CacheFunc<object, Proc, object, BlockParam, object>(RubyOps.Yield1));
			}
		}

		public static MethodInfo Yield2
		{
			get
			{
				return _Yield2 ?? (_Yield2 = CallInstruction.CacheFunc<object, object, Proc, object, BlockParam, object>(RubyOps.Yield2));
			}
		}

		public static MethodInfo Yield3
		{
			get
			{
				return _Yield3 ?? (_Yield3 = CallInstruction.CacheFunc<object, object, object, Proc, object, BlockParam, object>(RubyOps.Yield3));
			}
		}

		public static MethodInfo Yield4
		{
			get
			{
				return _Yield4 ?? (_Yield4 = CallInstruction.CacheFunc<object, object, object, object, Proc, object, BlockParam, object>(RubyOps.Yield4));
			}
		}

		public static MethodInfo YieldN
		{
			get
			{
				return _YieldN ?? (_YieldN = CallInstruction.CacheFunc<object[], Proc, object, BlockParam, object>(RubyOps.YieldN));
			}
		}

		public static MethodInfo YieldSplat0
		{
			get
			{
				return _YieldSplat0 ?? (_YieldSplat0 = CallInstruction.CacheFunc<IList, Proc, object, BlockParam, object>(RubyOps.YieldSplat0));
			}
		}

		public static MethodInfo YieldSplat1
		{
			get
			{
				return _YieldSplat1 ?? (_YieldSplat1 = CallInstruction.CacheFunc<object, IList, Proc, object, BlockParam, object>(RubyOps.YieldSplat1));
			}
		}

		public static MethodInfo YieldSplat2
		{
			get
			{
				return _YieldSplat2 ?? (_YieldSplat2 = CallInstruction.CacheFunc<object, object, IList, Proc, object, BlockParam, object>(RubyOps.YieldSplat2));
			}
		}

		public static MethodInfo YieldSplat3
		{
			get
			{
				return _YieldSplat3 ?? (_YieldSplat3 = CallInstruction.CacheFunc<object, object, object, IList, Proc, object, BlockParam, object>(RubyOps.YieldSplat3));
			}
		}

		public static MethodInfo YieldSplat4
		{
			get
			{
				return _YieldSplat4 ?? (_YieldSplat4 = CallInstruction.CacheFunc<object, object, object, object, IList, Proc, object, BlockParam, object>(RubyOps.YieldSplat4));
			}
		}

		public static MethodInfo YieldSplatN
		{
			get
			{
				return _YieldSplatN ?? (_YieldSplatN = CallInstruction.CacheFunc<object[], IList, Proc, object, BlockParam, object>(RubyOps.YieldSplatN));
			}
		}

		public static MethodInfo YieldSplatNRhs
		{
			get
			{
				return _YieldSplatNRhs ?? (_YieldSplatNRhs = CallInstruction.CacheFunc<object[], IList, object, Proc, object, BlockParam, object>(RubyOps.YieldSplatNRhs));
			}
		}

		internal static ConstructorInfo GetConstructor(Type type, params Type[] signature)
		{
			return type.GetConstructor(signature);
		}

		internal static MethodInfo GetMethod(Type type, string name)
		{
			return type.GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		internal static MethodInfo GetMethod(Type type, string name, params Type[] signature)
		{
			return GetMethod(type, name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, signature);
		}

		internal static MethodInfo GetMethod(Type type, string name, BindingFlags flags, params Type[] signature)
		{
			return type.GetMethod(name, flags | BindingFlags.Public | BindingFlags.DeclaredOnly, null, signature, null);
		}

		public static Expression MakeArrayOpCall(IList<Expression> args)
		{
			switch (args.Count)
			{
			case 0:
				return MakeArray0.OpCall();
			case 1:
				return MakeArray1.OpCall(Microsoft.Scripting.Ast.Utils.Box(args[0]));
			case 2:
				return MakeArray2.OpCall(Microsoft.Scripting.Ast.Utils.Box(args[0]), Microsoft.Scripting.Ast.Utils.Box(args[1]));
			case 3:
				return MakeArray3.OpCall(Microsoft.Scripting.Ast.Utils.Box(args[0]), Microsoft.Scripting.Ast.Utils.Box(args[1]), Microsoft.Scripting.Ast.Utils.Box(args[2]));
			case 4:
				return MakeArray4.OpCall(Microsoft.Scripting.Ast.Utils.Box(args[0]), Microsoft.Scripting.Ast.Utils.Box(args[1]), Microsoft.Scripting.Ast.Utils.Box(args[2]), Microsoft.Scripting.Ast.Utils.Box(args[3]));
			case 5:
				return MakeArray5.OpCall(new ReadOnlyCollectionBuilder<Expression>
				{
					Microsoft.Scripting.Ast.Utils.Box(args[0]),
					Microsoft.Scripting.Ast.Utils.Box(args[1]),
					Microsoft.Scripting.Ast.Utils.Box(args[2]),
					Microsoft.Scripting.Ast.Utils.Box(args[3]),
					Microsoft.Scripting.Ast.Utils.Box(args[4])
				});
			default:
				return MakeArrayN.OpCall(Microsoft.Scripting.Ast.Utils.NewArrayHelper(typeof(object), args));
			}
		}

		public static MethodInfo Yield(int argumentCount, bool hasSplattedArgument, bool hasRhsArgument, out bool hasArgumentArray)
		{
			if (hasRhsArgument)
			{
				if (hasSplattedArgument)
				{
					hasArgumentArray = true;
					return YieldSplatNRhs;
				}
				argumentCount++;
			}
			hasArgumentArray = argumentCount > 4;
			if (!hasSplattedArgument)
			{
				return Yield(argumentCount);
			}
			return YieldSplat(argumentCount);
		}

		public static MethodInfo CreateRegex(string suffix)
		{
			switch (suffix)
			{
			case "N":
				return CreateRegexN;
			case "M":
				return CreateRegexM;
			case "LM":
				return CreateRegexLM;
			case "ML":
				return CreateRegexML;
			case "MM":
				return CreateRegexMM;
			default:
				throw Assert.Unreachable;
			}
		}

		public static MethodInfo CreateMutableString(string suffix)
		{
			switch (suffix)
			{
			case "N":
				return CreateMutableStringN;
			case "M":
				return CreateMutableStringM;
			case "LM":
				return CreateMutableStringLM;
			case "ML":
				return CreateMutableStringML;
			case "MM":
				return CreateMutableStringMM;
			default:
				throw Assert.Unreachable;
			}
		}

		public static MethodInfo CreateSymbol(string suffix)
		{
			switch (suffix)
			{
			case "N":
				return CreateSymbolN;
			case "M":
				return CreateSymbolM;
			case "LM":
				return CreateSymbolLM;
			case "ML":
				return CreateSymbolML;
			case "MM":
				return CreateSymbolMM;
			default:
				throw Assert.Unreachable;
			}
		}

		public static MethodInfo Yield(int parameterCount)
		{
			switch (parameterCount)
			{
			case 0:
				return Yield0;
			case 1:
				return Yield1;
			case 2:
				return Yield2;
			case 3:
				return Yield3;
			case 4:
				return Yield4;
			default:
				return YieldN;
			}
		}

		public static MethodInfo YieldSplat(int parameterCount)
		{
			switch (parameterCount)
			{
			case 0:
				return YieldSplat0;
			case 1:
				return YieldSplat1;
			case 2:
				return YieldSplat2;
			case 3:
				return YieldSplat3;
			case 4:
				return YieldSplat4;
			default:
				return YieldSplatN;
			}
		}
	}
}
