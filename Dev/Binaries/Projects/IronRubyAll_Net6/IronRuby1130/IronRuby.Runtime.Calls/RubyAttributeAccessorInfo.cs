using System.Reflection;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	public abstract class RubyAttributeAccessorInfo : RubyMemberInfo
	{
		private readonly string _instanceVariableName;

		protected string InstanceVariableName
		{
			get
			{
				return _instanceVariableName;
			}
		}

		internal override bool IsDataMember
		{
			get
			{
				return true;
			}
		}

		protected RubyAttributeAccessorInfo(RubyMemberFlags flags, RubyModule declaringModule, string variableName)
			: base(flags, declaringModule)
		{
			_instanceVariableName = variableName;
		}

		public override MemberInfo[] GetMembers()
		{
			return Utils.EmptyMemberInfos;
		}
	}
}
