using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToProcAction : ConvertToReferenceTypeAction<ConvertToProcAction, Proc>
	{
		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToProc;
			}
		}

		protected override string TargetTypeName
		{
			get
			{
				return "Proc";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToProcValidator;
			}
		}
	}
}
