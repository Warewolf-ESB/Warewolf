using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Hosting
{
	public sealed class RubyService : MarshalByRefObject
	{
		private readonly ScriptEngine _engine;

		private readonly RubyContext _context;

		internal RubyService(RubyContext context, ScriptEngine engine)
		{
			_context = context;
			_engine = engine;
		}

		public bool RequireFile(string path)
		{
			ContractUtils.RequiresNotNull(path, "path");
			return RequireFile(path, (Scope)null);
		}

		public bool RequireFile(string path, ScriptScope scope)
		{
			ContractUtils.RequiresNotNull(path, "path");
			ContractUtils.RequiresNotNull(scope, "scope");
			return RequireFile(path, HostingHelpers.GetScope(scope));
		}

		private bool RequireFile(string path, Scope scope)
		{
			return _context.Loader.LoadFile(scope, null, _context.EncodePath(path), LoadFlags.Require);
		}

		public override object InitializeLifetimeService()
		{
			//return _engine.InitializeLifetimeService();
			return null; // _engine..InitializeLifetimeService(); does not exist anymore, in previous version it returns null;
        }
	}
}
