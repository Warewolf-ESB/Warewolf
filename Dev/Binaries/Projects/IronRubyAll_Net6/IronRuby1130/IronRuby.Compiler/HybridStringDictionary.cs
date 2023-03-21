using System.Collections;
using System.Collections.Generic;

namespace IronRuby.Compiler
{
	public class HybridStringDictionary<TValue> : IEnumerable<KeyValuePair<string, TValue>>, IEnumerable
	{
		private const int ListLength = 4;

		private Dictionary<string, TValue> _dict;

		private KeyValuePair<string, TValue>[] _list;

		private int _listSize;

		public int Count
		{
			get
			{
				return _listSize + ((_dict != null) ? _dict.Count : 0);
			}
		}

		public bool TryGetValue(string key, out TValue value)
		{
			for (int i = 0; i < _listSize; i++)
			{
				KeyValuePair<string, TValue> keyValuePair = _list[i];
				if (keyValuePair.Key == key)
				{
					value = keyValuePair.Value;
					return true;
				}
			}
			if (_dict != null)
			{
				return _dict.TryGetValue(key, out value);
			}
			value = default(TValue);
			return false;
		}

		public void Add(string key, TValue value)
		{
			if (_listSize > 0)
			{
				if (_listSize < _list.Length)
				{
					_list[_listSize++] = new KeyValuePair<string, TValue>(key, value);
					return;
				}
				_dict = new Dictionary<string, TValue>();
				for (int i = 0; i < _list.Length; i++)
				{
					KeyValuePair<string, TValue> keyValuePair = _list[i];
					_dict.Add(keyValuePair.Key, keyValuePair.Value);
				}
				_dict.Add(key, value);
				_list = null;
				_listSize = -1;
			}
			else if (_listSize == 0)
			{
				_list = new KeyValuePair<string, TValue>[4];
				_list[0] = new KeyValuePair<string, TValue>(key, value);
				_listSize = 1;
			}
			else
			{
				_dict.Add(key, value);
			}
		}

		IEnumerator<KeyValuePair<string, TValue>> IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator()
		{
			for (int i = 0; i < _listSize; i++)
			{
				yield return _list[i];
			}
			if (_dict == null)
			{
				yield break;
			}
			foreach (KeyValuePair<string, TValue> item in _dict)
			{
				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, TValue>>)this).GetEnumerator();
		}
	}
}
