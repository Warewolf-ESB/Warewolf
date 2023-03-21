using System;
using System.Runtime.Remoting;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[HideMethod("message")]
	[RubyException("NoMethodError", Extends = typeof(MissingMethodException), Inherits = typeof(MemberAccessException))]
	public static class NoMethodErrorOps
	{
		[RubyConstructor]
		public static MissingMethodException Factory(RubyClass self, object message, object name, object args)
		{
			MissingMethodException ex = new MissingMethodException(RubyExceptionData.GetClrMessage(self, message ?? "NoMethodError"));
			RubyExceptionData.InitializeException(ex, message);
			ex.Data[typeof(NoMethodErrorOps)] = new ObjectHandle[1]
			{
				new ObjectHandle(args)
			};
			return ex;
		}

		[RubyMethod("args")]
		public static object GetArguments(MissingMethodException self)
		{
			ObjectHandle[] array = self.Data[typeof(NoMethodErrorOps)] as ObjectHandle[];
			if (array == null)
			{
				return null;
			}
			return array[0].Unwrap();
		}
	}
}
