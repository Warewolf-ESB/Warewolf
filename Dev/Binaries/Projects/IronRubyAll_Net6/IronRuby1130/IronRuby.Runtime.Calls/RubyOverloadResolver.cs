using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyOverloadResolver : OverloadResolver
	{
		internal sealed class RubyContextArgBuilder : ArgBuilder
		{
			public override int Priority
			{
				get
				{
					return -1;
				}
			}

			public override int ConsumedArgumentCount
			{
				get
				{
					return 0;
				}
			}

			public RubyContextArgBuilder(ParameterInfo info)
				: base(info)
			{
			}

			protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed)
			{
				return ((RubyOverloadResolver)resolver).ContextExpression;
			}
		}

		internal sealed class RubyCallSiteStorageBuilder : ArgBuilder
		{
			public override int Priority
			{
				get
				{
					return -1;
				}
			}

			public override int ConsumedArgumentCount
			{
				get
				{
					return 0;
				}
			}

			public RubyCallSiteStorageBuilder(ParameterInfo info)
				: base(info)
			{
			}

			protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed)
			{
				return Microsoft.Scripting.Ast.Utils.Constant(Activator.CreateInstance(base.ParameterInfo.ParameterType, ((RubyOverloadResolver)resolver).Context));
			}
		}

		internal sealed class RubyScopeArgBuilder : ArgBuilder
		{
			public override int Priority
			{
				get
				{
					return -1;
				}
			}

			public override int ConsumedArgumentCount
			{
				get
				{
					return 0;
				}
			}

			public RubyScopeArgBuilder(ParameterInfo info)
				: base(info)
			{
			}

			protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed)
			{
				return ((RubyOverloadResolver)resolver).ScopeExpression;
			}
		}

		internal sealed class RubyClassCtorArgBuilder : ArgBuilder
		{
			public override int Priority
			{
				get
				{
					return -1;
				}
			}

			public override int ConsumedArgumentCount
			{
				get
				{
					return 0;
				}
			}

			public RubyClassCtorArgBuilder(ParameterInfo info)
				: base(info)
			{
			}

			protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed)
			{
				return ((RubyOverloadResolver)resolver)._args.TargetExpression;
			}
		}

		internal sealed class ImplicitInstanceBuilder : InstanceBuilder
		{
			public override bool HasValue
			{
				get
				{
					return true;
				}
			}

			public override int ConsumedArgumentCount
			{
				get
				{
					return 0;
				}
			}

			public ImplicitInstanceBuilder()
				: base(-1)
			{
			}

			protected override Expression ToExpression(ref MethodInfo method, OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed)
			{
				return ((RubyOverloadResolver)resolver)._args.TargetExpression;
			}
		}

		internal sealed class MissingBlockArgBuilder : SimpleArgBuilder
		{
			public override int Priority
			{
				get
				{
					return -1;
				}
			}

			public override int ConsumedArgumentCount
			{
				get
				{
					return 1;
				}
			}

			public MissingBlockArgBuilder(int index)
				: base(typeof(MissingBlockParam), index, false, false)
			{
			}

			protected override SimpleArgBuilder Copy(int newIndex)
			{
				return new MissingBlockArgBuilder(newIndex);
			}

			protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed)
			{
				hasBeenUsed[base.Index] = true;
				return null;
			}
		}

		private readonly CallArguments _args;

		private readonly MetaObjectBuilder _metaBuilder;

		private readonly SelfCallConvention _callConvention;

		private readonly bool _implicitProtocolConversions;

		private int _firstRestrictedArg;

		private int _lastSplattedArg;

		private ParameterExpression _listVariable;

		private IList _list;

		private List<Key<int, NarrowingLevel, Expression>> _argumentAssumptions;

		private static readonly DynamicMetaObject NullMetaBlockParam = new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.Constant(null, typeof(BlockParam)), BindingRestrictions.Empty, null);

		internal RubyContext Context
		{
			get
			{
				return _args.RubyContext;
			}
		}

		internal Expression ScopeExpression
		{
			get
			{
				return _args.MetaScope.Expression;
			}
		}

		internal Expression ContextExpression
		{
			get
			{
				return _args.MetaContext.Expression;
			}
		}

		internal RubyOverloadResolver(MetaObjectBuilder metaBuilder, CallArguments args, SelfCallConvention callConvention, bool implicitProtocolConversions)
			: base(args.RubyContext.Binder)
		{
			_args = args;
			_metaBuilder = metaBuilder;
			_callConvention = callConvention;
			_implicitProtocolConversions = implicitProtocolConversions;
		}

		protected override bool AllowMemberInitialization(OverloadInfo method)
		{
			return false;
		}

		protected override bool BindToUnexpandedParams(MethodCandidate candidate)
		{
			return _implicitProtocolConversions;
		}

		protected override BitArray MapSpecialParameters(ParameterMapping mapping)
		{
			OverloadInfo overload = mapping.Overload;
			IList<ParameterInfo> parameters = overload.Parameters;
			BitArray bitArray = new BitArray(parameters.Count);
			int num = 0;
			if (_callConvention == SelfCallConvention.SelfIsInstance)
			{
				if (overload.IsStatic)
				{
					AddSimpleHiddenMapping(mapping, parameters[num], true);
					bitArray[num++] = true;
				}
				else
				{
					mapping.AddParameter(new ParameterWrapper(null, overload.DeclaringType, null, ParameterBindingFlags.ProhibitNull | ParameterBindingFlags.IsHidden));
					mapping.AddInstanceBuilder(new InstanceBuilder(mapping.ArgIndex));
				}
			}
			else if (_callConvention == SelfCallConvention.NoSelf && !overload.IsStatic && overload.DeclaringType == typeof(object))
			{
				mapping.AddInstanceBuilder(new ImplicitInstanceBuilder());
			}
			while (num < parameters.Count && parameters[num].ParameterType.IsSubclassOf(typeof(RubyCallSiteStorage)))
			{
				mapping.AddBuilder(new RubyCallSiteStorageBuilder(parameters[num]));
				bitArray[num++] = true;
			}
			if (num < parameters.Count)
			{
				ParameterInfo parameterInfo = parameters[num];
				if (parameterInfo.ParameterType == typeof(RubyScope))
				{
					mapping.AddBuilder(new RubyScopeArgBuilder(parameterInfo));
					bitArray[num++] = true;
				}
				else if (parameterInfo.ParameterType == typeof(RubyContext))
				{
					mapping.AddBuilder(new RubyContextArgBuilder(parameterInfo));
					bitArray[num++] = true;
				}
				else if (overload.IsConstructor && parameterInfo.ParameterType == typeof(RubyClass))
				{
					mapping.AddBuilder(new RubyClassCtorArgBuilder(parameterInfo));
					bitArray[num++] = true;
				}
			}
			if (num < parameters.Count && parameters[num].ParameterType == typeof(BlockParam))
			{
				AddSimpleHiddenMapping(mapping, parameters[num], mapping.Overload.ProhibitsNull(num));
				bitArray[num++] = true;
			}
			else if (num >= parameters.Count || parameters[num].ParameterType != typeof(BlockParam))
			{
				mapping.AddBuilder(new MissingBlockArgBuilder(mapping.ArgIndex));
				mapping.AddParameter(new ParameterWrapper(null, typeof(MissingBlockParam), null, ParameterBindingFlags.IsHidden));
			}
			if (_callConvention == SelfCallConvention.SelfIsParameter)
			{
				AddSimpleHiddenMapping(mapping, parameters[num], mapping.Overload.ProhibitsNull(num));
				bitArray[num++] = true;
			}
			return bitArray;
		}

		private void AddSimpleHiddenMapping(ParameterMapping mapping, ParameterInfo info, bool prohibitNull)
		{
			mapping.AddBuilder(new SimpleArgBuilder(info, info.ParameterType, mapping.ArgIndex, false, false));
			mapping.AddParameter(new ParameterWrapper(info, info.ParameterType, null, ParameterBindingFlags.IsHidden | (prohibitNull ? ParameterBindingFlags.ProhibitNull : ParameterBindingFlags.None)));
		}

		internal static int GetHiddenParameterCount(OverloadInfo method, SelfCallConvention callConvention)
		{
			int i = 0;
			IList<ParameterInfo> parameters = method.Parameters;
			if (callConvention == SelfCallConvention.SelfIsInstance && method.IsStatic)
			{
				i++;
			}
			for (; i < parameters.Count && parameters[i].ParameterType.IsSubclassOf(typeof(RubyCallSiteStorage)); i++)
			{
			}
			if (i < parameters.Count)
			{
				ParameterInfo parameterInfo = parameters[i];
				if (parameterInfo.ParameterType == typeof(RubyScope))
				{
					i++;
				}
				else if (parameterInfo.ParameterType == typeof(RubyContext))
				{
					i++;
				}
				else if (method.IsConstructor && parameterInfo.ParameterType == typeof(RubyClass))
				{
					i++;
				}
			}
			if (i < parameters.Count && parameters[i].ParameterType == typeof(BlockParam))
			{
				i++;
			}
			if (callConvention == SelfCallConvention.SelfIsParameter)
			{
				i++;
			}
			return i;
		}

		internal static void GetParameterCount(OverloadInfo method, SelfCallConvention callConvention, out int mandatory, out int optional)
		{
			mandatory = 0;
			optional = 0;
			for (int i = GetHiddenParameterCount(method, callConvention); i < method.ParameterCount; i++)
			{
				ParameterInfo pi = method.Parameters[i];
				if (method.IsParamArray(i))
				{
					optional++;
				}
				else if (pi.IsOutParameter())
				{
					optional++;
				}
				else if (pi.IsMandatory())
				{
					mandatory++;
				}
				else
				{
					optional++;
				}
			}
		}

		protected override ActualArguments CreateActualArguments(IList<DynamicMetaObject> namedArgs, IList<string> argNames, int preSplatLimit, int postSplatLimit)
		{
			List<DynamicMetaObject> list = new List<DynamicMetaObject>();
			if (_callConvention == SelfCallConvention.SelfIsInstance)
			{
				list.Add(_args.MetaTarget);
			}
			if (_args.Signature.HasBlock)
			{
				if (_args.GetBlock() == null)
				{
					list.Add(NullMetaBlockParam);
				}
				else
				{
					if (_metaBuilder.BfcVariable == null)
					{
						_metaBuilder.BfcVariable = _metaBuilder.GetTemporary(typeof(BlockParam), "#bfc");
					}
					list.Add(new DynamicMetaObject(_metaBuilder.BfcVariable, BindingRestrictions.Empty));
				}
			}
			else
			{
				list.Add(MissingBlockParam.Meta.Instance);
			}
			if (_callConvention == SelfCallConvention.SelfIsParameter)
			{
				list.Add(_args.MetaTarget);
			}
			_firstRestrictedArg = list.Count;
			int hidden = ((_callConvention == SelfCallConvention.NoSelf) ? 1 : 2);
			return CreateActualArguments(list, _metaBuilder, _args, hidden, preSplatLimit, postSplatLimit, out _lastSplattedArg, out _list, out _listVariable);
		}

		public static IList<DynamicMetaObject> NormalizeArguments(MetaObjectBuilder metaBuilder, CallArguments args, int minCount, int maxCount)
		{
			int lastSplattedArg;
			IList list;
			ParameterExpression listVariable;
			ActualArguments actualArguments = CreateActualArguments(new List<DynamicMetaObject>(), metaBuilder, args, 2, maxCount, maxCount, out lastSplattedArg, out list, out listVariable);
			int num = actualArguments.Count + actualArguments.CollapsedCount;
			if (num < minCount)
			{
				metaBuilder.SetWrongNumberOfArgumentsError(num, minCount);
				return null;
			}
			if (num > maxCount)
			{
				metaBuilder.SetWrongNumberOfArgumentsError(num, maxCount);
				return null;
			}
			return actualArguments.Arguments;
		}

		private static ActualArguments CreateActualArguments(List<DynamicMetaObject> normalized, MetaObjectBuilder metaBuilder, CallArguments args, int hidden, int preSplatLimit, int postSplatLimit, out int lastSplattedArg, out IList list, out ParameterExpression listVariable)
		{
			for (int i = 0; i < args.SimpleArgumentCount; i++)
			{
				normalized.Add(args.GetSimpleMetaArgument(i));
			}
			list = null;
			listVariable = null;
			int num;
			int splatIndex;
			int collapsedCount;
			if (args.Signature.HasSplattedArgument)
			{
				num = normalized.Count;
				DynamicMetaObject splattedMetaArgument = args.GetSplattedMetaArgument();
				list = (IList)splattedMetaArgument.Value;
				int listLength;
				metaBuilder.AddSplattedArgumentTest(list, splattedMetaArgument.Expression, out listLength, out listVariable);
				int j;
				for (j = 0; j < Math.Min(listLength, preSplatLimit - num); j++)
				{
					normalized.Add(MakeSplattedItem(list, listVariable, j));
				}
				splatIndex = normalized.Count;
				for (j = Math.Max(j, listLength - (postSplatLimit - (args.Signature.HasRhsArgument ? 1 : 0))); j < listLength; j++)
				{
					normalized.Add(MakeSplattedItem(list, listVariable, j));
				}
				collapsedCount = listLength - (normalized.Count - num);
				lastSplattedArg = normalized.Count - 1;
			}
			else
			{
				splatIndex = (num = (lastSplattedArg = -1));
				collapsedCount = 0;
			}
			if (args.Signature.HasRhsArgument)
			{
				normalized.Add(args.GetRhsMetaArgument());
			}
			return new ActualArguments(normalized.ToArray(), DynamicMetaObject.EmptyMetaObjects, ArrayUtils.EmptyStrings, hidden, collapsedCount, num, splatIndex);
		}

		internal static DynamicMetaObject MakeSplattedItem(IList list, Expression listVariable, int index)
		{
			return DynamicMetaObject.Create(list[index], Expression.Call(listVariable, typeof(IList).GetMethod("get_Item"), Microsoft.Scripting.Ast.Utils.Constant(index)));
		}

		internal void AddArgumentRestrictions(MetaObjectBuilder metaBuilder, BindingTarget bindingTarget)
		{
			ActualArguments actualArguments = GetActualArguments();
			IList<DynamicMetaObject> list = (bindingTarget.Success ? bindingTarget.RestrictedArguments.GetObjects() : actualArguments.Arguments);
			for (int i = _firstRestrictedArg; i < list.Count; i++)
			{
				DynamicMetaObject dynamicMetaObject = (bindingTarget.Success ? list[i] : list[i].Restrict(list[i].GetLimitType()));
				if (i >= actualArguments.FirstSplattedArg && i <= _lastSplattedArg)
				{
					metaBuilder.AddCondition(dynamicMetaObject.Restrictions.ToExpression());
				}
				else
				{
					metaBuilder.AddRestriction(dynamicMetaObject.Restrictions);
				}
			}
			Expression collapsedArgsCondition = GetCollapsedArgsCondition();
			if (collapsedArgsCondition != null)
			{
				metaBuilder.AddCondition(collapsedArgsCondition);
			}
			if (_argumentAssumptions == null)
			{
				return;
			}
			foreach (Key<int, NarrowingLevel, Expression> argumentAssumption in _argumentAssumptions)
			{
				if (argumentAssumption.Second == bindingTarget.NarrowingLevel)
				{
					metaBuilder.AddCondition(argumentAssumption.Third);
				}
			}
		}

		public override bool CanConvertFrom(Type fromType, DynamicMetaObject fromArg, ParameterWrapper toParameter, NarrowingLevel level)
		{
			Convertibility convertibility = Converter.CanConvertFrom(fromArg, fromType, toParameter.Type, toParameter.ProhibitNull, level, HasExplicitProtocolConversion(toParameter), _implicitProtocolConversions);
			if (convertibility.Assumption != null)
			{
				if (_argumentAssumptions == null)
				{
					_argumentAssumptions = new List<Key<int, NarrowingLevel, Expression>>();
				}
				if (_argumentAssumptions.FindIndex((Key<int, NarrowingLevel, Expression> k) => k.First == toParameter.ParameterInfo.Position && k.Second == level) < 0)
				{
					_argumentAssumptions.Add(Key.Create(toParameter.ParameterInfo.Position, level, convertibility.Assumption));
				}
			}
			return convertibility.IsConvertible;
		}

		public override bool CanConvertFrom(ParameterWrapper fromParameter, ParameterWrapper toParameter)
		{
			return Converter.CanConvertFrom(null, fromParameter.Type, toParameter.Type, toParameter.ProhibitNull, NarrowingLevel.None, false, false).IsConvertible;
		}

		private bool HasExplicitProtocolConversion(ParameterWrapper parameter)
		{
			if (parameter.ParameterInfo != null && parameter.ParameterInfo.IsDefined(typeof(DefaultProtocolAttribute), false))
			{
				return !parameter.IsParamsArray;
			}
			return false;
		}

		public override Candidate SelectBestConversionFor(DynamicMetaObject arg, ParameterWrapper candidateOne, ParameterWrapper candidateTwo, NarrowingLevel level)
		{
			Type type = candidateOne.Type;
			Type type2 = candidateTwo.Type;
			Type limitType = arg.GetLimitType();
			if (limitType == typeof(DynamicNull))
			{
				if (type == typeof(BlockParam) && type2 == typeof(MissingBlockParam))
				{
					return Candidate.One;
				}
				if (type == typeof(MissingBlockParam) && type2 == typeof(BlockParam))
				{
					return Candidate.Two;
				}
			}
			else if (type == limitType)
			{
				if (!(type2 == limitType))
				{
					return Candidate.One;
				}
				if (!limitType.IsValueType)
				{
					if (candidateOne.ProhibitNull)
					{
						return Candidate.One;
					}
					if (candidateTwo.ProhibitNull)
					{
						return Candidate.Two;
					}
				}
			}
			else if (type2 == limitType)
			{
				return Candidate.Two;
			}
			if (type.IsEnum && Enum.GetUnderlyingType(type) == type2)
			{
				return Candidate.Two;
			}
			if (type2.IsEnum && Enum.GetUnderlyingType(type2) == type)
			{
				return Candidate.One;
			}
			return base.SelectBestConversionFor(arg, candidateOne, candidateTwo, level);
		}

		public override Expression Convert(DynamicMetaObject metaObject, Type restrictedType, ParameterInfo info, Type toType)
		{
			Expression expression = metaObject.Expression;
			Type type = restrictedType ?? expression.Type;
			if (type == typeof(MissingBlockParam))
			{
				return Microsoft.Scripting.Ast.Utils.Constant(null);
			}
			if (type == typeof(BlockParam) && toType == typeof(MissingBlockParam))
			{
				return Microsoft.Scripting.Ast.Utils.Constant(null);
			}
			if (info != null && info.IsDefined(typeof(DefaultProtocolAttribute), false))
			{
				RubyConversionAction rubyConversionAction = RubyConversionAction.TryGetDefaultConversionAction(Context, toType);
				if (rubyConversionAction != null)
				{
					return Microsoft.Scripting.Ast.Utils.LightDynamic(rubyConversionAction, toType, expression);
				}
			}
			if (restrictedType != null)
			{
				if (restrictedType == typeof(DynamicNull))
				{
					if (!toType.IsValueType || (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>)))
					{
						return Microsoft.Scripting.Ast.Utils.Constant(null, toType);
					}
					if (toType == typeof(bool))
					{
						return Microsoft.Scripting.Ast.Utils.Constant(false);
					}
				}
				if (toType.IsAssignableFrom(restrictedType))
				{
					return Microsoft.Scripting.Ast.Utils.Convert(expression, CompilerHelpers.GetVisibleType(toType));
				}
				Type visibleType = CompilerHelpers.GetVisibleType(restrictedType);
				if (Converter.CanConvertFrom(metaObject, visibleType, toType, false, NarrowingLevel.None, false, false).IsConvertible)
				{
					expression = Microsoft.Scripting.Ast.Utils.Convert(expression, visibleType);
				}
			}
			return Converter.ConvertExpression(expression, toType, _args.RubyContext, _args.MetaContext.Expression, _implicitProtocolConversions);
		}

		protected override Expression GetSplattedExpression()
		{
			return _listVariable;
		}

		protected override object GetSplattedItem(int index)
		{
			return _list[index];
		}

		public override Microsoft.Scripting.Actions.ErrorInfo MakeInvalidParametersError(BindingTarget target)
		{
			Expression exceptionValue;
			switch (target.Result)
			{
			case BindingResult.AmbiguousMatch:
				exceptionValue = MakeAmbiguousCallError(target);
				break;
			case BindingResult.IncorrectArgumentCount:
				exceptionValue = MakeIncorrectArgumentCountError(target);
				break;
			case BindingResult.CallFailure:
				exceptionValue = MakeCallFailureError(target);
				break;
			case BindingResult.NoCallableMethod:
				exceptionValue = Methods.CreateArgumentsError.OpCall(Microsoft.Scripting.Ast.Utils.Constant(string.Format("Method '{0}' is not callable", target.Name)));
				break;
			default:
				throw new InvalidOperationException();
			}
			return Microsoft.Scripting.Actions.ErrorInfo.FromException(exceptionValue);
		}

		private Expression MakeAmbiguousCallError(BindingTarget target)
		{
			StringBuilder stringBuilder = new StringBuilder(string.Format("Found multiple methods for '{0}': ", target.Name));
			string value = "";
			foreach (MethodCandidate ambiguousMatch in target.AmbiguousMatches)
			{
				IList<ParameterWrapper> parameters = ambiguousMatch.GetParameters();
				string value2 = "";
				stringBuilder.Append(value);
				stringBuilder.Append(target.Name);
				stringBuilder.Append('(');
				foreach (ParameterWrapper item in parameters)
				{
					if (!item.IsHidden)
					{
						stringBuilder.Append(value2);
						stringBuilder.Append(base.Binder.GetTypeName(item.Type));
						if (item.ProhibitNull)
						{
							stringBuilder.Append('!');
						}
						value2 = ", ";
					}
				}
				stringBuilder.Append(')');
				value = ", ";
			}
			return Methods.MakeAmbiguousMatchError.OpCall(Microsoft.Scripting.Ast.Utils.Constant(stringBuilder.ToString()));
		}

		private Expression MakeIncorrectArgumentCountError(BindingTarget target)
		{
			IList<int> expectedArgumentCount = target.ExpectedArgumentCount;
			int num3;
			if (expectedArgumentCount.Count > 0)
			{
				int val = int.MaxValue;
				int num = int.MinValue;
				int num2 = int.MinValue;
				foreach (int item in expectedArgumentCount)
				{
					if (item > target.ActualArgumentCount)
					{
						val = Math.Min(val, item);
					}
					else
					{
						num = Math.Max(num, item);
					}
					num2 = Math.Max(num2, item);
				}
				num3 = ((target.ActualArgumentCount < num) ? num : Math.Min(val, num2));
			}
			else
			{
				num3 = 0;
			}
			return Methods.MakeWrongNumberOfArgumentsError.OpCall(Microsoft.Scripting.Ast.Utils.Constant(target.ActualArgumentCount), Microsoft.Scripting.Ast.Utils.Constant(num3));
		}

		private Expression MakeCallFailureError(BindingTarget target)
		{
			foreach (CallFailure callFailure in target.CallFailures)
			{
				switch (callFailure.Reason)
				{
				case CallFailureReason.ConversionFailure:
					foreach (ConversionResult conversionResult in callFailure.ConversionResults)
					{
						if (conversionResult.Failed)
						{
							if (typeof(Proc).IsAssignableFrom(conversionResult.To))
							{
								return Methods.CreateArgumentsErrorForProc.OpCall(Microsoft.Scripting.Ast.Utils.Constant(conversionResult.GetArgumentTypeName(base.Binder)));
							}
							if (conversionResult.To == typeof(BlockParam))
							{
								return Methods.CreateArgumentsErrorForMissingBlock.OpCall();
							}
							string value;
							if (conversionResult.To.IsGenericType && conversionResult.To.GetGenericTypeDefinition() == typeof(Union<, >))
							{
								Type[] genericArguments = conversionResult.To.GetGenericArguments();
								value = base.Binder.GetTypeName(genericArguments[0]) + " or " + base.Binder.GetTypeName(genericArguments[1]);
							}
							else
							{
								value = base.Binder.GetTypeName(conversionResult.To);
							}
							return Methods.CreateTypeConversionError.OpCall(Microsoft.Scripting.Ast.Utils.Constant(conversionResult.GetArgumentTypeName(base.Binder)), Microsoft.Scripting.Ast.Utils.Constant(value));
						}
					}
					break;
				case CallFailureReason.TypeInference:
					return Methods.CreateArgumentsError.OpCall(Microsoft.Scripting.Ast.Utils.Constant(string.Format("generic arguments could not be infered for method '{0}'", target.Name)));
				default:
					throw new InvalidOperationException();
				}
			}
			throw new InvalidOperationException();
		}
	}
}
