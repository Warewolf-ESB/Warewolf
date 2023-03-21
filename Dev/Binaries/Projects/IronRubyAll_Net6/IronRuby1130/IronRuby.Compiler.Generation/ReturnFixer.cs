using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Generation;

namespace IronRuby.Compiler.Generation
{
	internal sealed class ReturnFixer
	{
		private readonly ParameterInfo _parameter;

		private readonly LocalBuilder _reference;

		private readonly int _index;

		private ReturnFixer(LocalBuilder reference, ParameterInfo parameter, int index)
		{
			_parameter = parameter;
			_reference = reference;
			_index = index;
		}

		public void FixReturn(ILGen il)
		{
			il.EmitLoadArg(_index);
			il.Emit(OpCodes.Ldloc, _reference);
			il.EmitFieldGet(_reference.LocalType.GetField("Value"));
			il.EmitStoreValueIndirect(_parameter.ParameterType.GetElementType());
		}

		public static ReturnFixer EmitArgument(ILGen il, ParameterInfo parameter, int index)
		{
			il.EmitLoadArg(index);
			if (parameter.ParameterType.IsByRef)
			{
				Type elementType = parameter.ParameterType.GetElementType();
				Type type = typeof(StrongBox<>).MakeGenericType(elementType);
				LocalBuilder localBuilder = il.DeclareLocal(type);
				il.EmitLoadValueIndirect(elementType);
				il.EmitNew(type, new Type[1] { elementType });
				il.Emit(OpCodes.Stloc, localBuilder);
				il.Emit(OpCodes.Ldloc, localBuilder);
				return new ReturnFixer(localBuilder, parameter, index);
			}
			il.EmitBoxing(parameter.ParameterType);
			return null;
		}
	}
}
