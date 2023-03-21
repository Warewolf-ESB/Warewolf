using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToPathAction : ConvertToReferenceTypeAction<ConvertToPathAction, MutableString>
	{
		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToPath;
			}
		}

		protected override string TargetTypeName
		{
			get
			{
				return "String";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return null;
			}
		}

		protected override Expression MakeValidatorCall(CallArguments args, Expression targetClassNameConstant, Expression result)
		{
			return Microsoft.Scripting.Ast.Utils.LightDynamic(ProtocolConversionAction<ConvertToStrAction>.Make(args.RubyContext), Microsoft.Scripting.Ast.Utils.Box(result));
		}
	}
}
