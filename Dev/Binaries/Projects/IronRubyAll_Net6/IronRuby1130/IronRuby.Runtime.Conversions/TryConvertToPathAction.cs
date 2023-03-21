using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class TryConvertToPathAction : TryConvertToReferenceTypeAction<TryConvertToPathAction, MutableString>
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
				return Methods.ToStringValidator;
			}
		}
	}
}
