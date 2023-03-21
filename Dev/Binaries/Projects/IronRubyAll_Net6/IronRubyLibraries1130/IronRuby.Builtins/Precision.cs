using System;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("Precision")]
	public class Precision
	{
		[RubyMethod("prec")]
		public static object Prec(CallSiteStorage<Func<CallSite, RubyClass, object, object>> inducedFromStorage, object self, [NotNull] RubyClass klass)
		{
			CallSite<Func<CallSite, RubyClass, object, object>> callSite = inducedFromStorage.GetCallSite("induced_from", 1);
			return callSite.Target(callSite, klass, self);
		}

		[RubyMethod("prec_i")]
		public static object PrecInteger(CallSiteStorage<Func<CallSite, object, RubyClass, object>> precStorage, object self)
		{
			CallSite<Func<CallSite, object, RubyClass, object>> callSite = precStorage.GetCallSite("prec", 1);
			return callSite.Target(callSite, self, precStorage.Context.GetClass(typeof(Integer)));
		}

		[RubyMethod("prec_f")]
		public static object PrecFloat(CallSiteStorage<Func<CallSite, object, RubyClass, object>> precStorage, object self)
		{
			CallSite<Func<CallSite, object, RubyClass, object>> callSite = precStorage.GetCallSite("prec", 1);
			return callSite.Target(callSite, self, precStorage.Context.GetClass(typeof(double)));
		}

		[RubyMethod("included", RubyMethodAttributes.PublicSingleton)]
		public static object Included(RubyContext context, RubyModule self, RubyModule includedIn)
		{
			RubyClass orCreateSingletonClass = includedIn.GetOrCreateSingletonClass();
			orCreateSingletonClass.AddMethod(context, "induced_from", new RubyLibraryMethodInfo(new LibraryOverload[1] { LibraryOverload.Create(new Func<RubyModule, object, object>(InducedFrom), false, 0, 0) }, RubyMethodVisibility.Public, orCreateSingletonClass));
			return self;
		}

		private static object InducedFrom(RubyModule rubyClass, object other)
		{
			throw RubyExceptions.CreateTypeError("undefined conversion from {0} into {1}", rubyClass.Context.GetClassOf(other).Name, rubyClass.Name);
		}
	}
}
