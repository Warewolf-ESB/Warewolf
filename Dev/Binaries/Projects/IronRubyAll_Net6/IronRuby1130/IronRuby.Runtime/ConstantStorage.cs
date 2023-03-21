using System;
using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	internal struct ConstantStorage
	{
		internal static ConstantStorage Removed = new ConstantStorage(false);

		public readonly object Value;

		public readonly WeakReference WeakValue;

		internal bool IsRemoved
		{
			get
			{
				return Value == Removed.Value;
			}
		}

		internal ConstantStorage(object value)
		{
			Value = value;
			if (value == null)
			{
				WeakValue = ConstantSiteCache.WeakNull;
				return;
			}
			switch (Type.GetTypeCode(value.GetType()))
			{
			case TypeCode.Object:
			{
				RubyModule rubyModule = value as RubyModule;
				if (rubyModule != null)
				{
					WeakValue = rubyModule.WeakSelf;
				}
				else
				{
					WeakValue = new WeakReference(value);
				}
				break;
			}
			case TypeCode.String:
				WeakValue = new WeakReference(value);
				break;
			default:
				WeakValue = null;
				break;
			}
		}

		internal ConstantStorage(object value, WeakReference weakValue)
		{
			Value = value;
			WeakValue = weakValue;
		}

		private ConstantStorage(bool dummy)
		{
			Value = new object();
			WeakValue = null;
		}
	}
}
