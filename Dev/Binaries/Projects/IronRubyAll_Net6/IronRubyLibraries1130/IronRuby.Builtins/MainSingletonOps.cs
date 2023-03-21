using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("__MainSingleton")]
	public static class MainSingletonOps
	{
		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static object Initialize(object self)
		{
			throw new NotImplementedException("TODO");
		}

		[RubyMethod("to_s", RubyMethodAttributes.PublicInstance)]
		public static MutableString ToS(object self)
		{
			return MutableString.CreateAscii("main");
		}

		[RubyMethod("public", RubyMethodAttributes.PublicInstance)]
		public static RubyModule SetPublicVisibility(RubyScope scope, object self, [DefaultProtocol][NotNullItems] params string[] methodNames)
		{
			return SetVisibility(scope, self, methodNames, RubyMethodAttributes.PublicInstance);
		}

		[RubyMethod("private", RubyMethodAttributes.PublicInstance)]
		public static RubyModule SetPrivateVisibility(RubyScope scope, object self, [DefaultProtocol][NotNullItems] params string[] methodNames)
		{
			return SetVisibility(scope, self, methodNames, RubyMethodAttributes.PrivateInstance);
		}

		private static RubyModule SetVisibility(RubyScope scope, object self, string[] methodNames, RubyMethodAttributes attributes)
		{
			RubyTopLevelScope topLocalScope = scope.Top.GlobalScope.TopLocalScope;
			RubyModule rubyModule = ((scope != topLocalScope || topLocalScope.MethodLookupModule == null) ? scope.RubyContext.GetClassOf(self) : topLocalScope.MethodLookupModule);
			ModuleOps.SetMethodAttributes(scope, rubyModule, methodNames, attributes);
			return rubyModule;
		}

		[RubyMethod("include", RubyMethodAttributes.PublicInstance)]
		public static RubyClass Include(RubyContext context, object self, params RubyModule[] modules)
		{
			RubyClass classOf = context.GetClassOf(self);
			classOf.IncludeModules(modules);
			return classOf;
		}
	}
}
