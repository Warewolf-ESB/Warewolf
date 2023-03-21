using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	internal sealed class SuperForwarderInfo : RubyMemberInfo
	{
		private readonly string _superName;

		internal override bool IsSuperForwarder
		{
			get
			{
				return true;
			}
		}

		public string SuperName
		{
			get
			{
				return _superName;
			}
		}

		public SuperForwarderInfo(RubyMemberFlags flags, RubyModule declaringModule, string superName)
			: base(flags, declaringModule)
		{
			_superName = superName;
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new SuperForwarderInfo(flags, module, _superName);
		}

		public override string ToString()
		{
			return base.ToString() + ((_superName != null) ? (" forward to: " + _superName) : null);
		}
	}
}
