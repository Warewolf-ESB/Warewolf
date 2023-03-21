using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public abstract class TrySplatAction<TSelf> : ConvertToReferenceTypeAction<TSelf, IList> where TSelf : TrySplatAction<TSelf>, new()
	{
		protected override string TargetTypeName
		{
			get
			{
				return "Array";
			}
		}

		public override Type ReturnType
		{
			get
			{
				return typeof(object);
			}
		}

		protected override Expression MakeErrorExpression(CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			return Microsoft.Scripting.Ast.Utils.Box(args.TargetExpression);
		}

		protected override void SetError(MetaObjectBuilder metaBuilder, CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			metaBuilder.Result = Microsoft.Scripting.Ast.Utils.Box(args.TargetExpression);
		}

		protected override DynamicMetaObjectBinder GetInteropBinder(RubyContext context, IList<DynamicMetaObject> args, out MethodInfo postProcessor)
		{
			postProcessor = null;
			return context.MetaBinderFactory.InteropSplat();
		}
	}
}
