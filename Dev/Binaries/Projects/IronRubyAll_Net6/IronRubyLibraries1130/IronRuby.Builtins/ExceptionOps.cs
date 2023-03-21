using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyException("Exception", Extends = typeof(Exception))]
	public static class ExceptionOps
	{
		public static string GetClrMessage(RubyClass exceptionClass, object message)
		{
			return RubyExceptionData.GetClrMessage(exceptionClass.Context, message);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static Exception ReinitializeException(RubyContext context, Exception self, object message)
		{
			RubyExceptionData instance = RubyExceptionData.GetInstance(self);
			instance.Backtrace = null;
			instance.Message = message ?? MutableString.Create(context.GetClassOf(self).Name, context.GetIdentifierEncoding());
			return self;
		}

		[RubyMethod("exception", RubyMethodAttributes.PublicSingleton)]
		public static RuleGenerator CreateException()
		{
			return RuleGenerators.InstanceConstructor;
		}

		[RubyMethod("backtrace", RubyMethodAttributes.PublicInstance)]
		public static RubyArray GetBacktrace(Exception self)
		{
			return RubyExceptionData.GetInstance(self).Backtrace;
		}

		[RubyMethod("set_backtrace", RubyMethodAttributes.PublicInstance)]
		public static RubyArray SetBacktrace(Exception self, [NotNull] MutableString backtrace)
		{
			return RubyExceptionData.GetInstance(self).Backtrace = RubyArray.Create(backtrace);
		}

		[RubyMethod("set_backtrace", RubyMethodAttributes.PublicInstance)]
		public static RubyArray SetBacktrace(Exception self, RubyArray backtrace)
		{
			if (backtrace != null && !CollectionUtils.TrueForAll(backtrace, (object item) => item is MutableString))
			{
				throw RubyExceptions.CreateTypeError("backtrace must be Array of String");
			}
			return RubyExceptionData.GetInstance(self).Backtrace = backtrace;
		}

		[RubyMethod("exception", RubyMethodAttributes.PublicInstance)]
		public static RuleGenerator GetException()
		{
			return delegate(MetaObjectBuilder metaBuilder, CallArguments args, string name)
			{
				ArgsBuilder argsBuilder = new ArgsBuilder(0, 0, 0, 1, false);
				argsBuilder.AddCallArguments(metaBuilder, args);
				if (!metaBuilder.Error)
				{
					if (argsBuilder.ActualArgumentCount == 0)
					{
						metaBuilder.Result = args.TargetExpression;
					}
					else
					{
						RubyClass classOf = args.RubyContext.GetClassOf(args.Target);
						Expression classExpression = Microsoft.Scripting.Ast.Utils.Constant(classOf);
						args.SetTarget(classExpression, classOf);
						ParameterExpression messageVariable = null;
						if (classOf.BuildAllocatorCall(metaBuilder, args, () => Expression.Call(null, new Func<RubyClass, object, string>(GetClrMessage).Method, classExpression, Expression.Assign(messageVariable = metaBuilder.GetTemporary(typeof(object), "#message"), Microsoft.Scripting.Ast.Utils.Box(argsBuilder[0])))))
						{
							metaBuilder.Result = Expression.Call(null, new Func<RubyContext, Exception, object, Exception>(ReinitializeException).Method, Microsoft.Scripting.Ast.Utils.Convert(args.MetaContext.Expression, typeof(RubyContext)), metaBuilder.Result, messageVariable ?? Microsoft.Scripting.Ast.Utils.Box(argsBuilder[0]));
						}
						else
						{
							metaBuilder.SetError(Methods.MakeAllocatorUndefinedError.OpCall(Expression.Convert(args.TargetExpression, typeof(RubyClass))));
						}
					}
				}
			};
		}

		[RubyMethod("message")]
		public static object GetMessage(UnaryOpStorage stringReprStorage, Exception self)
		{
			CallSite<Func<CallSite, object, object>> callSite = stringReprStorage.GetCallSite("to_s");
			return callSite.Target(callSite, self);
		}

		[RubyMethod("to_str")]
		[RubyMethod("to_s")]
		public static object StringRepresentation(Exception self)
		{
			return RubyExceptionData.GetInstance(self).Message;
		}

		[RubyMethod("inspect", RubyMethodAttributes.PublicInstance)]
		public static MutableString Inspect(UnaryOpStorage inspectStorage, ConversionStorage<MutableString> tosConversion, Exception self)
		{
			object message = RubyExceptionData.GetInstance(self).Message;
			string classDisplayName = inspectStorage.Context.GetClassDisplayName(self);
			MutableString mutableString = MutableString.CreateMutable(inspectStorage.Context.GetIdentifierEncoding());
			mutableString.Append("#<");
			mutableString.Append(classDisplayName);
			mutableString.Append(": ");
			if (message != null)
			{
				mutableString.Append(KernelOps.Inspect(inspectStorage, tosConversion, message));
			}
			else
			{
				mutableString.Append(classDisplayName);
			}
			mutableString.Append('>');
			return mutableString;
		}
	}
}
