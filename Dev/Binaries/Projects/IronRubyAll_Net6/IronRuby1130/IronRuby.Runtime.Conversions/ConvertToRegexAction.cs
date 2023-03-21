using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToRegexAction : ConvertToReferenceTypeAction<ConvertToRegexAction, RubyRegex>
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
				return "Regexp";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToRegexValidator;
			}
		}
	}
}
