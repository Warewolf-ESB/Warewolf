namespace IronRuby.Runtime
{
	public abstract class RubyCallSiteStorage
	{
		private readonly RubyContext _context;

		public RubyContext Context
		{
			get
			{
				return _context;
			}
		}

		protected RubyCallSiteStorage(RubyContext context)
		{
			_context = context;
		}
	}
}
