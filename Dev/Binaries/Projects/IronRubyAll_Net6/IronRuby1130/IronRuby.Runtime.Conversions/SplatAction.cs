using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public abstract class SplatAction<TSelf> : ConvertToReferenceTypeAction<TSelf, IList> where TSelf : SplatAction<TSelf>, new()
	{
		protected override string TargetTypeName
		{
			get
			{
				return "Array";
			}
		}

		protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			IList list = args.Target as IList;
			if (list != null)
			{
				metaBuilder.Result = Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(IList));
				return true;
			}
			return false;
		}

		protected override Expression MakeErrorExpression(CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			return Expression.Convert(Methods.MakeArray1.OpCall(Microsoft.Scripting.Ast.Utils.Box(args.TargetExpression)), typeof(IList));
		}

		protected override void SetError(MetaObjectBuilder metaBuilder, CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			metaBuilder.Result = Expression.Convert(Methods.MakeArray1.OpCall(Microsoft.Scripting.Ast.Utils.Box(args.TargetExpression)), typeof(IList));
		}

		protected override DynamicMetaObjectBinder GetInteropBinder(RubyContext context, IList<DynamicMetaObject> args, out MethodInfo postProcessor)
		{
			postProcessor = null;
			return context.MetaBinderFactory.InteropSplat();
		}
	}
}
