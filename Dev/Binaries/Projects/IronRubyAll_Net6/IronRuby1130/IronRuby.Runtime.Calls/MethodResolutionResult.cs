using System.Collections.Generic;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	public struct MethodResolutionResult
	{
		public static readonly MethodResolutionResult NotFound = default(MethodResolutionResult);

		private readonly RubyMemberInfo _info;

		private readonly RubyModule _owner;

		private readonly bool _visible;

		public RubyMemberInfo Info
		{
			get
			{
				return _info;
			}
		}

		public RubyModule Owner
		{
			get
			{
				return _owner;
			}
		}

		public bool Found
		{
			get
			{
				if (_info != null)
				{
					return _visible;
				}
				return false;
			}
		}

		public RubyMethodVisibility IncompatibleVisibility
		{
			get
			{
				if (_info != null && !_visible)
				{
					return _info.Visibility;
				}
				return RubyMethodVisibility.None;
			}
		}

		public MethodResolutionResult(RubyMemberInfo info, RubyModule owner, bool visible)
		{
			_info = info;
			_owner = owner;
			_visible = visible;
		}

		internal MethodResolutionResult InvalidateSitesOnOverride()
		{
			if (_info != null)
			{
				_info.SetInvalidateSitesOnOverride();
			}
			return this;
		}

		internal MethodResolutionResult InvalidateSitesOnMissingMethodAddition(string methodName, RubyContext context)
		{
			if (context.MissingMethodsCachedInSites == null)
			{
				context.MissingMethodsCachedInSites = new HashSet<string>();
			}
			context.MissingMethodsCachedInSites.Add(methodName);
			return this;
		}
	}
}
