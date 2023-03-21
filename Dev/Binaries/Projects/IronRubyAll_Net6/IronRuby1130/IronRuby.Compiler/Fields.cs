using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Compiler
{
	public static class Fields
	{
		private static FieldInfo _StrongBox_Value;

		private static FieldInfo _ConstantSiteCache_Value;

		private static FieldInfo _ConstantSiteCache_Version;

		private static FieldInfo _ConstantSiteCache_WeakMissingConstant;

		private static FieldInfo _ConstantSiteCache_WeakNull;

		private static FieldInfo _DefaultArgument;

		private static FieldInfo _ForwardToBase;

		private static FieldInfo _IsDefinedConstantSiteCache_Value;

		private static FieldInfo _IsDefinedConstantSiteCache_Version;

		private static FieldInfo _NeedsUpdate;

		private static FieldInfo _RubyContext_ConstantAccessVersion;

		private static FieldInfo _RubyModule_Version;

		private static FieldInfo _VersionHandle_Method;

		public static FieldInfo StrongBox_Value
		{
			get
			{
				return _StrongBox_Value ?? (_StrongBox_Value = GetField(typeof(StrongBox<object>), "Value"));
			}
		}

		public static FieldInfo ConstantSiteCache_Value
		{
			get
			{
				return _ConstantSiteCache_Value ?? (_ConstantSiteCache_Value = GetField(typeof(ConstantSiteCache), "Value"));
			}
		}

		public static FieldInfo ConstantSiteCache_Version
		{
			get
			{
				return _ConstantSiteCache_Version ?? (_ConstantSiteCache_Version = GetField(typeof(ConstantSiteCache), "Version"));
			}
		}

		public static FieldInfo ConstantSiteCache_WeakMissingConstant
		{
			get
			{
				return _ConstantSiteCache_WeakMissingConstant ?? (_ConstantSiteCache_WeakMissingConstant = GetField(typeof(ConstantSiteCache), "WeakMissingConstant"));
			}
		}

		public static FieldInfo ConstantSiteCache_WeakNull
		{
			get
			{
				return _ConstantSiteCache_WeakNull ?? (_ConstantSiteCache_WeakNull = GetField(typeof(ConstantSiteCache), "WeakNull"));
			}
		}

		public static FieldInfo DefaultArgument
		{
			get
			{
				return _DefaultArgument ?? (_DefaultArgument = GetField(typeof(RubyOps), "DefaultArgument"));
			}
		}

		public static FieldInfo ForwardToBase
		{
			get
			{
				return _ForwardToBase ?? (_ForwardToBase = GetField(typeof(RubyOps), "ForwardToBase"));
			}
		}

		public static FieldInfo IsDefinedConstantSiteCache_Value
		{
			get
			{
				return _IsDefinedConstantSiteCache_Value ?? (_IsDefinedConstantSiteCache_Value = GetField(typeof(IsDefinedConstantSiteCache), "Value"));
			}
		}

		public static FieldInfo IsDefinedConstantSiteCache_Version
		{
			get
			{
				return _IsDefinedConstantSiteCache_Version ?? (_IsDefinedConstantSiteCache_Version = GetField(typeof(IsDefinedConstantSiteCache), "Version"));
			}
		}

		public static FieldInfo NeedsUpdate
		{
			get
			{
				return _NeedsUpdate ?? (_NeedsUpdate = GetField(typeof(RubyOps), "NeedsUpdate"));
			}
		}

		public static FieldInfo RubyContext_ConstantAccessVersion
		{
			get
			{
				return _RubyContext_ConstantAccessVersion ?? (_RubyContext_ConstantAccessVersion = GetField(typeof(RubyContext), "ConstantAccessVersion"));
			}
		}

		public static FieldInfo RubyModule_Version
		{
			get
			{
				return _RubyModule_Version ?? (_RubyModule_Version = GetField(typeof(RubyModule), "Version"));
			}
		}

		public static FieldInfo VersionHandle_Method
		{
			get
			{
				return _VersionHandle_Method ?? (_VersionHandle_Method = GetField(typeof(VersionHandle), "Method"));
			}
		}

		internal static FieldInfo GetField(Type type, string name)
		{
			return type.GetField(name);
		}
	}
}
