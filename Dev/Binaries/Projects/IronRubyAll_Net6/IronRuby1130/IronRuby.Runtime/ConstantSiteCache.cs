using System;

namespace IronRuby.Runtime
{
	[CLSCompliant(false)]
	public sealed class ConstantSiteCache
	{
		public static readonly WeakReference WeakNull = new WeakReference(null);

		public static readonly WeakReference WeakMissingConstant = new WeakReference(StrongMissingConstant);

		private static readonly object StrongMissingConstant = new object();

		public volatile int Version;

		public volatile object Value;

		internal void Update(object newValue, int newVersion)
		{
			Value = newValue;
			Version = newVersion;
		}
	}
}
