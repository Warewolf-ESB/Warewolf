using System.Threading;
using IronRuby.Builtins;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime
{
	public sealed class RubyGlobalScope : ScopeExtension
	{
		private readonly RubyContext _context;

		private readonly RubyObject _mainObject;

		private readonly bool _isHosted;

		private RubyTopLevelScope _topLocalScope;

		public RubyContext Context
		{
			get
			{
				return _context;
			}
		}

		public RubyClass MainSingleton
		{
			get
			{
				return _mainObject.ImmediateClass;
			}
		}

		public RubyObject MainObject
		{
			get
			{
				return _mainObject;
			}
		}

		public bool IsHosted
		{
			get
			{
				return _isHosted;
			}
		}

		public RubyTopLevelScope TopLocalScope
		{
			get
			{
				return _topLocalScope;
			}
		}

		internal RubyGlobalScope(RubyContext context, Scope scope, RubyObject mainObject, bool isHosted)
			: base(scope)
		{
			_context = context;
			_mainObject = mainObject;
			_isHosted = isHosted;
		}

		internal RubyTopLevelScope SetTopLocalScope(RubyTopLevelScope scope)
		{
			return Interlocked.CompareExchange(ref _topLocalScope, scope, null);
		}
	}
}
