using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public sealed class GenericConversionAction : RubyConversionAction
	{
		private readonly Type _type;

		public override Type ReturnType
		{
			get
			{
				return _type;
			}
		}

		internal GenericConversionAction(RubyContext context, Type type)
			: base(context)
		{
			_type = type;
		}

		public static GenericConversionAction MakeShared(Type type)
		{
			return RubyMetaBinderFactory.Shared.GenericConversionAction(type);
		}

		public override Expression CreateExpression()
		{
			return Methods.GetMethod(typeof(GenericConversionAction), "MakeShared").OpCall(Expression.Constant(_type, typeof(Type)));
		}

		protected override DynamicMetaObjectBinder GetInteropBinder(RubyContext context, IList<DynamicMetaObject> args, out MethodInfo postProcessor)
		{
			postProcessor = null;
			return context.MetaBinderFactory.InteropConvert(_type, true);
		}

		protected override bool Build(MetaObjectBuilder metaBuilder, CallArguments args, bool defaultFallback)
		{
			if (args.Target is IDynamicMetaObjectProvider)
			{
				metaBuilder.SetMetaResult(args.MetaTarget.BindConvert(args.RubyContext.MetaBinderFactory.InteropConvert(_type, true)), false);
				return true;
			}
			return BuildConversion(metaBuilder, args.MetaTarget, args.MetaContext.Expression, _type, defaultFallback);
		}

		internal static bool BuildConversion(MetaObjectBuilder metaBuilder, DynamicMetaObject target, Expression contextExpression, Type toType, bool defaultFallback)
		{
			Expression expression = TryImplicitConversion(target, toType);
			if (expression != null)
			{
				metaBuilder.Result = expression;
				metaBuilder.AddObjectTypeRestriction(target.Value, target.Expression);
				return true;
			}
			if (defaultFallback)
			{
				metaBuilder.AddObjectTypeRestriction(target.Value, target.Expression);
				metaBuilder.SetError(Methods.MakeTypeConversionError.OpCall(contextExpression, Microsoft.Scripting.Ast.Utils.Convert(target.Expression, typeof(object)), Expression.Constant(toType, typeof(Type))));
				return true;
			}
			return false;
		}

		private static Expression TryImplicitConversion(DynamicMetaObject target, Type toType)
		{
			if (target.Value == null)
			{
				if (!toType.IsValueType || (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>)))
				{
					return Microsoft.Scripting.Ast.Utils.Constant(null, toType);
				}
				return null;
			}
			Type type = target.Value.GetType();
			return Converter.ImplicitConvert(target.Expression, type, toType) ?? Converter.ExplicitConvert(target.Expression, type, toType);
		}
	}
}
