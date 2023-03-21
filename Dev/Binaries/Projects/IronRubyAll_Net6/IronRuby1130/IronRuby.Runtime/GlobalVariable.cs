using System;

namespace IronRuby.Runtime
{
	public abstract class GlobalVariable
	{
		public virtual bool IsEnumerated
		{
			get
			{
				return true;
			}
		}

		public virtual bool IsDefined
		{
			get
			{
				return true;
			}
		}

		public abstract object GetValue(RubyContext context, RubyScope scope);

		public abstract void SetValue(RubyContext context, RubyScope scope, string name, object value);

		internal Exception ReadOnlyError(string name)
		{
			return RubyExceptions.CreateNameError(string.Format("${0} is a read-only variable", name));
		}

		internal T RequireType<T>(object value, string variableName, string typeName)
		{
			if (!(value is T))
			{
				throw RubyExceptions.CreateTypeError(string.Format("Value of ${0} must be {1}", variableName, typeName));
			}
			return (T)value;
		}
	}
}
