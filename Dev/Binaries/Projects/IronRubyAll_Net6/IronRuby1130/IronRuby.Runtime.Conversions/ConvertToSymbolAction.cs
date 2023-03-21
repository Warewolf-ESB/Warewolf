using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToSymbolAction : ConvertToReferenceTypeAction<ConvertToSymbolAction, string>
	{
		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToStr;
			}
		}

		protected override string TargetTypeName
		{
			get
			{
				return "Symbol";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToSymbolValidator;
			}
		}

		protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			if (base.TryImplicitConversion(metaBuilder, args))
			{
				return true;
			}
			object target = args.Target;
			Expression targetExpression = args.TargetExpression;
			MutableString mutableString = target as MutableString;
			if (mutableString != null)
			{
				metaBuilder.Result = Methods.ConvertMutableStringToClrString.OpCall(Microsoft.Scripting.Ast.Utils.Convert(targetExpression, typeof(MutableString)));
				return true;
			}
			if (target is RubySymbol)
			{
				metaBuilder.Result = Methods.ConvertSymbolToClrString.OpCall(Microsoft.Scripting.Ast.Utils.Convert(targetExpression, typeof(RubySymbol)));
				return true;
			}
			if (target is int)
			{
				metaBuilder.Result = Methods.ConvertRubySymbolToClrString.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.MetaContext.Expression, typeof(RubyContext)), Microsoft.Scripting.Ast.Utils.Convert(targetExpression, typeof(int)));
				return true;
			}
			return false;
		}
	}
}
