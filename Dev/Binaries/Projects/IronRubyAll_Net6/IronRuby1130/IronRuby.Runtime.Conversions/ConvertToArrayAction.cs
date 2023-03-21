using System.Collections;
using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToArrayAction : ConvertToReferenceTypeAction<ConvertToArrayAction, IList>
	{
		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToAry;
			}
		}

		protected override string TargetTypeName
		{
			get
			{
				return "Array";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToArrayValidator;
			}
		}
	}
}
