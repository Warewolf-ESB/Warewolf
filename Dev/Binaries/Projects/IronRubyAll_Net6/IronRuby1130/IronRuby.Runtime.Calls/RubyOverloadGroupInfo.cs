using IronRuby.Builtins;
using Microsoft.Scripting.Actions.Calls;

namespace IronRuby.Runtime.Calls
{
	internal sealed class RubyOverloadGroupInfo : RubyMethodGroupInfo
	{
		private readonly RubyOverloadGroupInfo[] _overloadOwners;

		private int _maxCachedOverloadLevel = -1;

		internal override bool IsRubyMember
		{
			get
			{
				return false;
			}
		}

		internal RubyOverloadGroupInfo[] OverloadOwners
		{
			get
			{
				return _overloadOwners;
			}
		}

		internal int MaxCachedOverloadLevel
		{
			get
			{
				return _maxCachedOverloadLevel;
			}
		}

		internal RubyOverloadGroupInfo(OverloadInfo[] methods, RubyModule declaringModule, RubyOverloadGroupInfo[] overloadOwners, bool isStatic)
			: base(methods, declaringModule, isStatic)
		{
			_overloadOwners = overloadOwners;
		}

		internal void CachedInGroup(RubyMethodGroupInfo group)
		{
			int level = ((RubyClass)group.DeclaringModule).Level;
			if (_maxCachedOverloadLevel < level)
			{
				_maxCachedOverloadLevel = level;
			}
		}

		internal override void SetInvalidateSitesOnOverride()
		{
			RubyMemberInfo.SetInvalidateSitesOnOverride(this);
			if (_overloadOwners == null)
			{
				return;
			}
			RubyOverloadGroupInfo[] overloadOwners = _overloadOwners;
			foreach (RubyOverloadGroupInfo rubyOverloadGroupInfo in overloadOwners)
			{
				if (rubyOverloadGroupInfo != null)
				{
					RubyMemberInfo.SetInvalidateSitesOnOverride(rubyOverloadGroupInfo);
				}
			}
		}
	}
}
