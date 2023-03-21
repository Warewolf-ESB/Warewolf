using System;

namespace IronRuby.Runtime
{
	public abstract class RubyAttribute : Attribute
	{
		private string _buildConfig;

		private RubyCompatibility _compatibility = RubyCompatibility.Default;

		public string BuildConfig
		{
			get
			{
				return _buildConfig;
			}
			set
			{
				_buildConfig = value;
			}
		}

		public RubyCompatibility Compatibility
		{
			get
			{
				return _compatibility;
			}
			set
			{
				_compatibility = value;
			}
		}
	}
}
