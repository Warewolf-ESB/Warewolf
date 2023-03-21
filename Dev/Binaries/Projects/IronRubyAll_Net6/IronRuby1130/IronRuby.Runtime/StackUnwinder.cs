using System;
using System.Reflection;

namespace IronRuby.Runtime
{
	public abstract class StackUnwinder : Exception
	{
		public readonly object ReturnValue;

		internal static int InstanceCount;

		internal static FieldInfo ReturnValueField
		{
			get
			{
				return typeof(StackUnwinder).GetField("ReturnValue");
			}
		}

		public StackUnwinder(object returnValue)
		{
			InstanceCount++;
			ReturnValue = returnValue;
		}
	}
}
