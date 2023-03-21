using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	public struct VisibilityContext
	{
		public static readonly VisibilityContext AllVisible = new VisibilityContext(RubyMethodAttributes.VisibilityMask);

		public readonly RubyClass Class;

		public readonly RubyMethodAttributes Visible;

		public VisibilityContext(RubyMethodAttributes mask)
		{
			Class = null;
			Visible = mask;
		}

		public VisibilityContext(RubyClass cls)
		{
			Class = cls;
			Visible = RubyMethodAttributes.VisibilityMask;
		}

		public bool IsVisible(RubyMethodVisibility visibility)
		{
			return ((uint)visibility & (uint)Visible) != 0;
		}
	}
}
