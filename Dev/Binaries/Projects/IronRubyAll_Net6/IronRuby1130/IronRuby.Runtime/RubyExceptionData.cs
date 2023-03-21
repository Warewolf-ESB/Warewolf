using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[Serializable]
	public sealed class RubyExceptionData
	{
		private static readonly object _DataKey = typeof(RubyExceptionData);

		private Exception _exception;

		private Exception _visibleException;

		private object _message;

		private RubyArray _backtrace;

		[NonSerialized]
		private CallSite<Func<CallSite, RubyContext, Exception, RubyArray, object>> _setBacktraceCallSite;

		internal bool Handled { get; set; }

		public object Message
		{
			get
			{
				if (_message == null)
				{
					_message = MutableString.Create(_visibleException.Message, RubyEncoding.UTF8);
				}
				return _message;
			}
			set
			{
				ContractUtils.RequiresNotNull(value, "value");
				_message = value;
			}
		}

		public RubyArray Backtrace
		{
			get
			{
				return _backtrace;
			}
			set
			{
				_backtrace = value;
			}
		}

		private RubyExceptionData(Exception exception)
		{
			_exception = exception;
			_visibleException = exception;
		}

		public static RubyArray CreateBacktrace(RubyContext context, int skipFrames)
		{
			return new RubyStackTraceBuilder(context, skipFrames).RubyTrace;
		}

		internal void CaptureExceptionTrace(RubyScope scope)
		{
			if (_backtrace == null)
			{
				StackTrace clrStackTrace = RubyStackTraceBuilder.GetClrStackTrace(null);
				_backtrace = new RubyStackTraceBuilder(scope.RubyContext, _exception, clrStackTrace, scope.InterpretedFrame != null).RubyTrace;
				DynamicSetBacktrace(scope.RubyContext, _backtrace);
			}
		}

		private void DynamicSetBacktrace(RubyContext context, RubyArray backtrace)
		{
			if (_setBacktraceCallSite == null)
			{
				Interlocked.CompareExchange(ref _setBacktraceCallSite, CallSite<Func<CallSite, RubyContext, Exception, RubyArray, object>>.Create(RubyCallAction.MakeShared("set_backtrace", RubyCallSignature.WithImplicitSelf(1))), null);
			}
			_setBacktraceCallSite.Target(_setBacktraceCallSite, context, _exception, backtrace);
		}

		public static RubyExceptionData GetInstance(Exception e)
		{
			RubyExceptionData rubyExceptionData = TryGetInstance(e);
			if (rubyExceptionData == null)
			{
				rubyExceptionData = AssociateInstance(e);
			}
			return rubyExceptionData;
		}

		internal static RubyExceptionData AssociateInstance(Exception e)
		{
			Exception visibleException = RubyUtils.GetVisibleException(e);
			RubyExceptionData rubyExceptionData;
			if (e == visibleException || visibleException == null)
			{
				rubyExceptionData = new RubyExceptionData(e);
			}
			else
			{
				rubyExceptionData = GetInstance(visibleException);
				if (rubyExceptionData._exception == visibleException)
				{
					rubyExceptionData._exception = e;
				}
			}
			e.Data[_DataKey] = rubyExceptionData;
			return rubyExceptionData;
		}

		internal static RubyExceptionData TryGetInstance(Exception e)
		{
			return e.Data[_DataKey] as RubyExceptionData;
		}

		public static string GetClrMessage(RubyContext context, object message)
		{
			return Protocols.ToClrStringNoThrow(context, message);
		}

		public static string GetClrMessage(RubyClass exceptionClass, object message)
		{
			return GetClrMessage(exceptionClass.Context, message ?? exceptionClass.Name);
		}

		public static Exception InitializeException(Exception exception, object message)
		{
			RubyExceptionData instance = GetInstance(exception);
			if (message != null)
			{
				instance.Message = message;
			}
			return exception;
		}

		internal static Exception HandleException(RubyContext context, Exception exception)
		{
			RubyExceptionData instance = GetInstance(exception);
			if (instance.Handled)
			{
				return exception;
			}
			RubyClass @class = context.GetClass(exception.GetType());
			MethodResolutionResult methodResolutionResult = @class.ImmediateClass.ResolveMethod("new", VisibilityContext.AllVisible);
			if (methodResolutionResult.Found && methodResolutionResult.Info.DeclaringModule == context.ClassClass && methodResolutionResult.Info is RubyCustomMethodInfo)
			{
				MethodResolutionResult methodResolutionResult2 = @class.ResolveMethod("initialize", VisibilityContext.AllVisible);
				if (methodResolutionResult2.Found && methodResolutionResult2.Info is RubyLibraryMethodInfo)
				{
					instance.Handled = true;
					return exception;
				}
			}
			CallSite<Func<CallSite, object, object, object>> newSite = @class.NewSite;
			Exception ex;
			try
			{
				ex = newSite.Target(newSite, @class, instance.Message) as Exception;
			}
			catch (Exception exception2)
			{
				return HandleException(context, exception2);
			}
			if (ex == null)
			{
				ex = RubyExceptions.CreateTypeError("exception object expected");
			}
			RubyExceptionData instance2 = GetInstance(ex);
			instance2.Handled = true;
			instance2._backtrace = instance._backtrace;
			return ex;
		}

		public static void ActiveExceptionHandled(Exception visibleException)
		{
			RubyExceptionData instance = GetInstance(visibleException);
			if (instance._exception != visibleException && (Thread.CurrentThread.ThreadState & System.Threading.ThreadState.AbortRequested) != 0)
			{
				Thread.ResetAbort();
			}
		}
	}
}
