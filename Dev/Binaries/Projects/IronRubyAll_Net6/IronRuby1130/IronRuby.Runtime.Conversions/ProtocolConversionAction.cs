using System;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Conversions
{
	public abstract class ProtocolConversionAction : RubyConversionAction
	{
		protected abstract string ToMethodName { get; }

		protected abstract MethodInfo ConversionResultValidator { get; }

		protected abstract string TargetTypeName { get; }

		public override Type ReturnType
		{
			get
			{
				if (!(ConversionResultValidator != null))
				{
					return typeof(object);
				}
				return ConversionResultValidator.ReturnType;
			}
		}

		protected override bool Build(MetaObjectBuilder metaBuilder, CallArguments args, bool defaultFallback)
		{
			BuildConversion(metaBuilder, args, ReturnType, this);
			return true;
		}

		internal static void BuildConversion(MetaObjectBuilder metaBuilder, CallArguments args, Type resultType, params ProtocolConversionAction[] conversions)
		{
			foreach (ProtocolConversionAction protocolConversionAction in conversions)
			{
				if (protocolConversionAction.TryImplicitConversion(metaBuilder, args))
				{
					metaBuilder.AddObjectTypeRestriction(args.Target, args.TargetExpression);
					if (!metaBuilder.Error)
					{
						metaBuilder.Result = ConvertResult(metaBuilder.Result, resultType);
					}
					return;
				}
			}
			RubyClass immediateClassOf = args.RubyContext.GetImmediateClassOf(args.Target);
			Expression targetClassNameConstant = Microsoft.Scripting.Ast.Utils.Constant(immediateClassOf.GetNonSingletonClass().Name, typeof(string));
			MethodResolutionResult methodResolutionResult = MethodResolutionResult.NotFound;
			ProtocolConversionAction protocolConversionAction2 = null;
			RubyMemberInfo rubyMemberInfo = null;
			MethodResolutionResult methodResolutionResult2;
			using (immediateClassOf.Context.ClassHierarchyLocker())
			{
				metaBuilder.AddTargetTypeTest(args.Target, immediateClassOf, args.TargetExpression, args.MetaContext, ArrayUtils.Insert(Symbols.RespondTo, Symbols.MethodMissing, ArrayUtils.ConvertAll(conversions, (ProtocolConversionAction c) => c.ToMethodName)));
				methodResolutionResult2 = immediateClassOf.ResolveMethodForSiteNoLock(Symbols.RespondTo, VisibilityContext.AllVisible);
				if (methodResolutionResult2.Found && methodResolutionResult2.Info.DeclaringModule == immediateClassOf.Context.KernelModule && methodResolutionResult2.Info is RubyLibraryMethodInfo)
				{
					methodResolutionResult2 = MethodResolutionResult.NotFound;
					foreach (ProtocolConversionAction protocolConversionAction3 in conversions)
					{
						protocolConversionAction2 = protocolConversionAction3;
						rubyMemberInfo = immediateClassOf.ResolveMethodForSiteNoLock(protocolConversionAction3.ToMethodName, VisibilityContext.AllVisible).Info;
						if (rubyMemberInfo == null)
						{
							if (!methodResolutionResult.Found)
							{
								methodResolutionResult = immediateClassOf.ResolveMethodNoLock(Symbols.MethodMissing, VisibilityContext.AllVisible);
							}
							methodResolutionResult.InvalidateSitesOnMissingMethodAddition(protocolConversionAction3.ToMethodName, immediateClassOf.Context);
							continue;
						}
						break;
					}
				}
			}
			if (!methodResolutionResult2.Found)
			{
				if (rubyMemberInfo == null)
				{
					protocolConversionAction2.SetError(metaBuilder, args, targetClassNameConstant, resultType);
					return;
				}
				rubyMemberInfo.BuildCall(metaBuilder, args, protocolConversionAction2.ToMethodName);
				if (!metaBuilder.Error)
				{
					metaBuilder.Result = ConvertResult(protocolConversionAction2.MakeValidatorCall(args, targetClassNameConstant, metaBuilder.Result), resultType);
				}
			}
			else
			{
				for (int num = conversions.Length - 1; num >= 0; num--)
				{
					string toMethodName = conversions[num].ToMethodName;
					LightDynamicExpression result = Microsoft.Scripting.Ast.Utils.LightDynamic(RubyCallAction.Make(args.RubyContext, toMethodName, RubyCallSignature.WithImplicitSelf(0)), args.TargetExpression);
					metaBuilder.Result = Expression.Condition(Methods.IsTrue.OpCall(Microsoft.Scripting.Ast.Utils.LightDynamic(RubyCallAction.Make(args.RubyContext, Symbols.RespondTo, RubyCallSignature.WithImplicitSelf(1)), args.TargetExpression, Expression.Constant(args.RubyContext.CreateSymbol(toMethodName, RubyEncoding.Binary)))), ConvertResult(conversions[num].MakeValidatorCall(args, targetClassNameConstant, result), resultType), (num < conversions.Length - 1) ? metaBuilder.Result : conversions[num].MakeErrorExpression(args, targetClassNameConstant, resultType));
				}
			}
		}

		protected internal abstract bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args);

		protected virtual Expression MakeErrorExpression(CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			return Expression.Throw(Methods.CreateTypeConversionError.OpCall(targetClassNameConstant, Microsoft.Scripting.Ast.Utils.Constant(TargetTypeName)), resultType);
		}

		protected virtual void SetError(MetaObjectBuilder metaBuilder, CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(targetClassNameConstant, Microsoft.Scripting.Ast.Utils.Constant(TargetTypeName)));
		}

		protected virtual Expression MakeValidatorCall(CallArguments args, Expression targetClassNameConstant, Expression result)
		{
			MethodInfo conversionResultValidator = ConversionResultValidator;
			if (!(conversionResultValidator != null))
			{
				return result;
			}
			return conversionResultValidator.OpCall(targetClassNameConstant, Microsoft.Scripting.Ast.Utils.Box(result));
		}

		private static Expression ConvertResult(Expression expression, Type resultType)
		{
			if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Union<, >))
			{
				Type[] genericArguments = resultType.GetGenericArguments();
				ConstructorInfo constructor = resultType.GetConstructor(genericArguments);
				if (genericArguments[0].IsAssignableFrom(expression.Type))
				{
					return Expression.New(constructor, expression, Microsoft.Scripting.Ast.Utils.Default(genericArguments[1]));
				}
				return Expression.New(constructor, Microsoft.Scripting.Ast.Utils.Default(genericArguments[0]), expression);
			}
			return Microsoft.Scripting.Ast.Utils.Convert(expression, resultType);
		}
	}
	public abstract class ProtocolConversionAction<TSelf> : ProtocolConversionAction where TSelf : ProtocolConversionAction<TSelf>, new()
	{
		public static TSelf Make(RubyContext context)
		{
			return context.MetaBinderFactory.Conversion<TSelf>();
		}

		public static TSelf MakeShared()
		{
			return RubyMetaBinderFactory.Shared.Conversion<TSelf>();
		}

		public override Expression CreateExpression()
		{
			return Methods.GetMethod(typeof(ProtocolConversionAction<TSelf>), "MakeShared").OpCall();
		}
	}
}
