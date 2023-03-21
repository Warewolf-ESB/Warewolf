using System;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public class TryConvertToFixnumAction : ProtocolConversionAction<TryConvertToFixnumAction>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "Fixnum";
			}
		}

		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToInt;
			}
		}

		public override Type ReturnType
		{
			get
			{
				return typeof(int?);
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToFixnumValidator;
			}
		}

		protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			object target = args.Target;
			if (target != null)
			{
				Expression expression2 = (metaBuilder.Result = RubyConversionAction.Convert(ReturnType, args));
				return expression2 != null;
			}
			return false;
		}

		protected override Expression MakeErrorExpression(CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			return Microsoft.Scripting.Ast.Utils.Constant(null, resultType);
		}

		protected override void SetError(MetaObjectBuilder metaBuilder, CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			metaBuilder.Result = Microsoft.Scripting.Ast.Utils.Constant(null, resultType);
		}
	}
}
