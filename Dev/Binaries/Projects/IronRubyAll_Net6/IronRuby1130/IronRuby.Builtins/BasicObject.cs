namespace IronRuby.Builtins
{
	public class BasicObject : RubyObject
	{
		public BasicObject(RubyClass cls)
			: base(cls)
		{
		}

		public BasicObject(RubyClass cls, params object[] args)
			: base(cls, args)
		{
		}
	}
}
