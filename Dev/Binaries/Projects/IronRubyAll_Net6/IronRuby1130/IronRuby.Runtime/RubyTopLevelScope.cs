using System;
using IronRuby.Builtins;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime
{
	public sealed class RubyTopLevelScope : RubyClosureScope
	{
		private readonly RubyGlobalScope _globalScope;

		private readonly RubyContext _context;

		private readonly RubyModule _methodLookupModule;

		private readonly RubyModule _wrappingModule;

		public override ScopeKind Kind
		{
			get
			{
				return ScopeKind.TopLevel;
			}
		}

		public override bool InheritsLocalVariables
		{
			get
			{
				return false;
			}
		}

		public RubyGlobalScope RubyGlobalScope
		{
			get
			{
				if (_globalScope == null)
				{
					throw new InvalidOperationException("Empty scope has no global scope.");
				}
				return _globalScope;
			}
		}

		internal new RubyContext RubyContext
		{
			get
			{
				return _context;
			}
		}

		public override RubyModule Module
		{
			get
			{
				return _wrappingModule;
			}
		}

		public RubyModule MethodLookupModule
		{
			get
			{
				return _methodLookupModule;
			}
		}

		internal RubyModule TopModuleOrObject
		{
			get
			{
				return _wrappingModule ?? _globalScope.Context.ObjectClass;
			}
		}

		internal RubyTopLevelScope(RubyContext context)
		{
			_top = this;
			_methodAttributes = RubyMethodAttributes.PrivateInstance;
			_context = context;
			SetEmptyLocals();
		}

		internal RubyTopLevelScope(RubyGlobalScope globalScope, RubyModule scopeModule, RubyModule methodLookupModule, RubyObject selfObject)
		{
			_top = this;
			_selfObject = selfObject;
			_methodAttributes = RubyMethodAttributes.PrivateInstance;
			_globalScope = globalScope;
			_context = globalScope.Context;
			_wrappingModule = scopeModule;
			_methodLookupModule = methodLookupModule;
		}

		internal static RubyTopLevelScope CreateTopLevelScope(Scope globalScope, RubyContext context, bool isMain)
		{
			RubyGlobalScope rubyGlobalScope = context.InitializeGlobalScope(globalScope, false, false);
			RubyTopLevelScope rubyTopLevelScope = new RubyTopLevelScope(rubyGlobalScope, null, null, rubyGlobalScope.MainObject);
			if (isMain)
			{
				context.ObjectClass.SetConstant("TOPLEVEL_BINDING", new Binding(rubyTopLevelScope));
				if (context.RubyOptions.RequirePaths != null)
				{
					foreach (string requirePath in context.RubyOptions.RequirePaths)
					{
						context.Loader.LoadFile(globalScope, rubyGlobalScope.MainObject, MutableString.Create(requirePath, RubyEncoding.UTF8), LoadFlags.Require);
					}
					return rubyTopLevelScope;
				}
			}
			return rubyTopLevelScope;
		}

		internal static RubyTopLevelScope CreateHostedTopLevelScope(Scope globalScope, RubyContext context, bool bindGlobals)
		{
			RubyGlobalScope rubyGlobalScope = context.InitializeGlobalScope(globalScope, true, bindGlobals);
			RubyTopLevelScope rubyTopLevelScope = rubyGlobalScope.TopLocalScope;
			if (rubyTopLevelScope == null)
			{
				rubyTopLevelScope = new RubyTopLevelScope(rubyGlobalScope, null, bindGlobals ? rubyGlobalScope.MainSingleton : null, rubyGlobalScope.MainObject);
				rubyGlobalScope.SetTopLocalScope(rubyTopLevelScope);
			}
			return rubyTopLevelScope;
		}

		internal static RubyTopLevelScope CreateWrappedTopLevelScope(Scope globalScope, RubyContext context)
		{
			RubyGlobalScope globalScope2 = context.InitializeGlobalScope(globalScope, false, false);
			RubyModule rubyModule = context.CreateModule(null, null, null, null, null, null, null, ModuleRestrictions.None);
			RubyObject rubyObject = new RubyObject(context.ObjectClass);
			context.GetOrCreateMainSingleton(rubyObject, new RubyModule[1] { rubyModule });
			return new RubyTopLevelScope(globalScope2, rubyModule, null, rubyObject);
		}
	}
}
