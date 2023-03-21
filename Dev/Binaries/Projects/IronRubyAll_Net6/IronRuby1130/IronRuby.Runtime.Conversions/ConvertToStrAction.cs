using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToStrAction : ConvertToReferenceTypeAction<ConvertToStrAction, MutableString>
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
				return "String";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToStringValidator;
			}
		}

		protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			if (base.TryImplicitConversion(metaBuilder, args))
			{
				return true;
			}
			if (args.Target is RubySymbol)
			{
				metaBuilder.Result = Methods.ConvertSymbolToMutableString.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(RubySymbol)));
				return true;
			}
			return false;
		}

		protected override DynamicMetaObjectBinder GetInteropBinder(RubyContext context, IList<DynamicMetaObject> args, out MethodInfo postConverter)
		{
			postConverter = Methods.StringToMutableString;
			return context.MetaBinderFactory.InteropConvert(typeof(string), true);
		}
	}
}
