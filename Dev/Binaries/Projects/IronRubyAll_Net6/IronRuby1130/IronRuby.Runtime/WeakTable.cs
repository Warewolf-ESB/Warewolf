using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime
{
	internal class WeakTable<TKey, TValue>
	{
		private sealed class Comparer : IEqualityComparer<object>
		{
			bool IEqualityComparer<object>.Equals(object x, object y)
			{
				WeakReference weakReference = x as WeakReference;
				WeakReference weakReference2 = y as WeakReference;
				if (weakReference != null)
				{
					x = weakReference.Target;
					if (x == null)
					{
						return weakReference == weakReference2;
					}
				}
				if (weakReference2 != null)
				{
					y = weakReference2.Target;
					if (y == null)
					{
						return weakReference == weakReference2;
					}
				}
				return x == y;
			}

			int IEqualityComparer<object>.GetHashCode(object obj)
			{
				WeakReference weakReference = obj as WeakReference;
				if (weakReference != null)
				{
					obj = weakReference.Target;
					if (obj == null)
					{
						return weakReference.GetHashCode();
					}
				}
				return RuntimeHelpers.GetHashCode(obj);
			}
		}

		private Dictionary<object, TValue> _dict;

		private static readonly IEqualityComparer<object> _Comparer = new Comparer();

		private int _version;

		private int _cleanupVersion;

		private int _cleanupGC;

		private bool GarbageCollected()
		{
			int num = GC.CollectionCount(0);
			bool flag = num != _cleanupGC;
			if (flag)
			{
				_cleanupGC = num;
			}
			return flag;
		}

		private void CheckCleanup()
		{
			_version++;
			long num = _version - _cleanupVersion;
			if (num > 1234 + _dict.Count / 2)
			{
				if (GarbageCollected())
				{
					Cleanup();
					_cleanupVersion = _version;
				}
				else
				{
					_cleanupVersion += 1234;
				}
			}
		}

		private void Cleanup()
		{
			int num = 0;
			int num2 = 0;
			foreach (WeakReference key in _dict.Keys)
			{
				if (key.Target != null)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
			if (num2 <= num / 4)
			{
				return;
			}
			Dictionary<object, TValue> dictionary = new Dictionary<object, TValue>(num + num / 4, _Comparer);
			foreach (WeakReference key2 in _dict.Keys)
			{
				object target = key2.Target;
				if (target != null)
				{
					dictionary[key2] = _dict[key2];
					GC.KeepAlive(target);
				}
			}
			_dict = dictionary;
		}

		public WeakTable()
		{
			_dict = new Dictionary<object, TValue>(_Comparer);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dict.TryGetValue(key, out value);
		}

		public void Add(TKey key, TValue value)
		{
			Cleanup();
			_dict.Add(new WeakReference(key, true), value);
		}
	}
}
