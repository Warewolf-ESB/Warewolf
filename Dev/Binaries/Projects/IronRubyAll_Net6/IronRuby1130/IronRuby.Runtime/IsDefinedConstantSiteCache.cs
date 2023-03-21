using System;

namespace IronRuby.Runtime
{
	[CLSCompliant(false)]
	public sealed class IsDefinedConstantSiteCache
	{
		public volatile int Version;

		public volatile bool Value;

		internal void Update(bool newValue, int newVersion)
		{
			Value = newValue;
			Version = newVersion;
		}
	}
}
