using System.Collections.Generic;
using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class TryConvertToHashAction : TryConvertToReferenceTypeAction<TryConvertToHashAction, IDictionary<object, object>>
	{
		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToHash;
			}
		}

		protected override string TargetTypeName
		{
			get
			{
				return "Hash";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToHashValidator;
			}
		}
	}
}
