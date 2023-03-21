using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using IronRuby.Compiler;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[Serializable]
	[DebuggerDisplay("{GetDebugValue()}", Type = "{GetDebugType()}")]
	public class MutableString : IEquatable<MutableString>, IComparable<MutableString>, IComparable, IRubyObjectState, IDuplicable
	{
		[Serializable]
		internal abstract class Content
		{
			protected MutableString _owner;

			public abstract bool IsEmpty { get; }

			public abstract int Count { get; set; }

			internal void SetOwner(MutableString owner)
			{
				_owner = owner;
			}

			protected Content(MutableString owner)
			{
				_owner = owner;
			}

			protected BinaryContent WrapContent(byte[] bytes, int count)
			{
				BinaryContent binaryContent = new BinaryContent(bytes, count, _owner);
				_owner.SetContent(binaryContent);
				return binaryContent;
			}

			protected CharArrayContent WrapContent(char[] chars, int count)
			{
				CharArrayContent charArrayContent = new CharArrayContent(chars, count, _owner);
				_owner.SetContent(charArrayContent);
				return charArrayContent;
			}

			internal static uint UpdateAsciiAndSurrogatesFlags(string str, uint flags)
			{
				int num = 0;
				foreach (int num2 in str)
				{
					if (Tokenizer.IsSurrogate(num2))
					{
						return flags & 0xFFFFFF87u;
					}
					num |= num2;
				}
				if (num >= 128)
				{
					return (flags & 0xFFFFFFA7u) | 0x20u;
				}
				return (flags & 0xFFFFFFAFu) | 8u | 0x20u;
			}

			internal static uint UpdateAsciiAndSurrogatesFlags(char[] str, int itemCount, uint flags)
			{
				int num = 0;
				for (int i = 0; i < itemCount; i++)
				{
					int num2 = str[i];
					if (Tokenizer.IsSurrogate(num2))
					{
						return flags & 0xFFFFFF87u;
					}
					num |= num2;
				}
				if (num >= 128)
				{
					return (flags & 0xFFFFFFA7u) | 0x20u;
				}
				return (flags & 0xFFFFFFAFu) | 8u | 0x20u;
			}

			public abstract string ConvertToString();

			public abstract byte[] ConvertToBytes();

			public abstract Content SwitchToBinaryContent();

			public abstract Content SwitchToStringContent();

			public abstract Content SwitchToMutableContent();

			public abstract void CheckEncoding();

			public abstract bool ContainsInvalidCharacters();

			public abstract byte[] ToByteArray();

			internal abstract byte[] GetByteArray(out int count);

			public abstract Content EscapeRegularExpression();

			public abstract int CalculateHashCode();

			public abstract uint UpdateCharacterFlags(uint flags);

			public abstract int GetCharCount();

			public abstract int GetCharacterCount();

			public abstract int GetByteCount();

			public abstract void TrimExcess();

			public abstract int GetCapacity();

			public abstract void SetCapacity(int capacity);

			public abstract Content Clone();

			public abstract char GetChar(int index);

			public abstract byte GetByte(int index);

			public abstract CharacterEnumerator GetCharacters();

			public abstract IEnumerable<byte> GetBytes();

			public abstract int OrdinalCompareTo(string str);

			public abstract int OrdinalCompareTo(Content content);

			public abstract int ReverseOrdinalCompareTo(BinaryContent content);

			public abstract int ReverseOrdinalCompareTo(CharArrayContent content);

			public abstract int ReverseOrdinalCompareTo(StringContent content);

			public abstract Content GetSlice(int start, int count);

			public abstract string GetStringSlice(int start, int count);

			public abstract byte[] GetBinarySlice(int start, int count);

			public abstract bool StartsWith(char c);

			public abstract int IndexOf(char c, int start, int count);

			public abstract int IndexOf(byte b, int start, int count);

			public abstract int IndexOf(string str, int start, int count);

			public abstract int IndexOf(byte[] bytes, int start, int count);

			public abstract int IndexIn(Content str, int start, int count);

			public abstract int LastIndexOf(char c, int start, int count);

			public abstract int LastIndexOf(byte b, int start, int count);

			public abstract int LastIndexOf(string str, int start, int count);

			public abstract int LastIndexOf(byte[] bytes, int start, int count);

			public abstract int LastIndexIn(Content str, int start, int count);

			public abstract Content Concat(Content content);

			public abstract Content ConcatTo(BinaryContent content);

			public abstract Content ConcatTo(CharArrayContent content);

			public abstract Content ConcatTo(StringContent content);

			public abstract void Append(char c, int repeatCount);

			public abstract void Append(byte b, int repeatCount);

			public abstract void Append(string str, int start, int count);

			public abstract void Append(char[] chars, int start, int count);

			public abstract void Append(byte[] bytes, int start, int count);

			public abstract void Append(Stream stream, int count);

			public abstract void Append(Content content, int start, int count);

			public abstract void AppendTo(BinaryContent content, int start, int count);

			public abstract void AppendTo(CharArrayContent content, int start, int count);

			public abstract void AppendTo(StringContent content, int start, int count);

			public abstract void AppendFormat(IFormatProvider provider, string format, object[] args);

			public abstract void Insert(int index, char c);

			public abstract void Insert(int index, byte b);

			public abstract void Insert(int index, string str, int start, int count);

			public abstract void Insert(int index, char[] chars, int start, int count);

			public abstract void Insert(int index, byte[] bytes, int start, int count);

			public abstract void InsertTo(Content str, int index, int start, int count);

			public abstract void SetByte(int index, byte b);

			public abstract void SetChar(int index, char c);

			public abstract void Remove(int start, int count);

			public abstract void Write(int offset, byte[] value, int start, int count);

			public abstract void Write(int offset, byte value, int repeatCount);
		}

		[Serializable]
		internal sealed class CharArrayContent : Content
		{
			private char[] _data;

			private int _count;

			private string _immutableSnapshot;

			public int DataLength
			{
				get
				{
					return _count;
				}
			}

			public override int Count
			{
				get
				{
					return _count;
				}
				set
				{
					if (_data.Length < value)
					{
						Array.Resize(ref _data, Utils.GetExpandedSize(_data, value));
					}
					else
					{
						Utils.Fill(_data, _count, '\0', value - _count);
					}
					_count = value;
				}
			}

			public override bool IsEmpty
			{
				get
				{
					return _count == 0;
				}
			}

			internal CharArrayContent(char[] data, MutableString owner)
				: this(data, data.Length, owner)
			{
			}

			internal CharArrayContent(char[] data, int count, MutableString owner)
				: base(owner)
			{
				_data = data;
				_count = count;
			}

			internal BinaryContent SwitchToBinary()
			{
				return SwitchToBinary(0);
			}

			private BinaryContent SwitchToBinary(int additionalCapacity)
			{
				byte[] array = DataToBytes(additionalCapacity);
				return WrapContent(array, array.Length - additionalCapacity);
			}

			private byte[] DataToBytes(int additionalCapacity)
			{
				if (_count == 0)
				{
					if (additionalCapacity != 0)
					{
						return new byte[additionalCapacity];
					}
					return Utils.EmptyBytes;
				}
				if (additionalCapacity == 0)
				{
					return _owner._encoding.StrictEncoding.GetBytes(_data, 0, _count);
				}
				byte[] array = new byte[GetDataByteCount() + additionalCapacity];
				GetDataBytes(array, 0);
				return array;
			}

			internal int GetDataByteCount()
			{
				return _owner._encoding.StrictEncoding.GetByteCount(_data, 0, _count);
			}

			internal void GetDataBytes(byte[] bytes, int start)
			{
				_owner._encoding.StrictEncoding.GetBytes(_data, 0, _count, bytes, start);
			}

			public char DataGetChar(int index)
			{
				return _data[index];
			}

			public void DataSetChar(int index, char c)
			{
				_data[index] = c;
			}

			public override uint UpdateCharacterFlags(uint flags)
			{
				return Content.UpdateAsciiAndSurrogatesFlags(_data, _count, flags);
			}

			public override int CalculateHashCode()
			{
				return ConvertToString().GetHashCode();
			}

			public override int GetCharCount()
			{
				return _count;
			}

			public override int GetCharacterCount()
			{
				if (!_owner.HasSurrogates())
				{
					return _count;
				}
				return _data.GetCharacterCount(_count);
			}

			public override int GetByteCount()
			{
				if (!_owner.HasSingleByteCharacters && _count != 0)
				{
					return SwitchToBinary().GetByteCount();
				}
				return _count;
			}

			public override Content SwitchToBinaryContent()
			{
				return SwitchToBinary();
			}

			public override Content SwitchToStringContent()
			{
				return this;
			}

			public override Content SwitchToMutableContent()
			{
				return this;
			}

			public override Content Clone()
			{
				return new CharArrayContent(_data.GetSlice(0, _count), _owner);
			}

			public override void TrimExcess()
			{
				Utils.TrimExcess(ref _data, _count);
			}

			public override int GetCapacity()
			{
				return _data.Length;
			}

			public override void SetCapacity(int capacity)
			{
				if (capacity < _count)
				{
					throw new InvalidOperationException();
				}
				Array.Resize(ref _data, capacity);
			}

			public override string ConvertToString()
			{
				if (_immutableSnapshot == null || _owner.IsFlagSet(4u))
				{
					_immutableSnapshot = GetStringSlice(0, _count);
					_owner.ClearFlag(4u);
				}
				return _immutableSnapshot;
			}

			public override byte[] ConvertToBytes()
			{
				BinaryContent binaryContent = SwitchToBinary();
				return binaryContent.GetBinarySlice(0, binaryContent.GetByteCount());
			}

			public override string ToString()
			{
				return new string(_data, 0, _count);
			}

			public override byte[] ToByteArray()
			{
				return DataToBytes(0);
			}

			internal override byte[] GetByteArray(out int count)
			{
				return SwitchToBinary().GetByteArray(out count);
			}

			public override Content EscapeRegularExpression()
			{
				StringBuilder stringBuilder = RubyRegex.EscapeToStringBuilder(ToString());
				if (stringBuilder == null)
				{
					return this;
				}
				return new CharArrayContent(stringBuilder.ToString().ToCharArray(), _owner);
			}

			public override void CheckEncoding()
			{
				_owner._encoding.StrictEncoding.GetByteCount(_data, 0, _count);
			}

			public override bool ContainsInvalidCharacters()
			{
				return Utils.ContainsInvalidCharacters(_data, 0, _count, _owner._encoding.StrictEncoding);
			}

			public override int OrdinalCompareTo(string str)
			{
				return _data.ValueCompareTo(_count, str);
			}

			internal int OrdinalCompareTo(char[] chars, int count)
			{
				return _data.ValueCompareTo(_count, chars, count);
			}

			public override int OrdinalCompareTo(Content content)
			{
				return content.ReverseOrdinalCompareTo(this);
			}

			public override int ReverseOrdinalCompareTo(BinaryContent content)
			{
				return SwitchToBinary().ReverseOrdinalCompareTo(content);
			}

			public override int ReverseOrdinalCompareTo(CharArrayContent content)
			{
				return content.OrdinalCompareTo(_data, _count);
			}

			public override int ReverseOrdinalCompareTo(StringContent content)
			{
				return content.OrdinalCompareTo(_data, _count);
			}

			public override char GetChar(int index)
			{
				if (index >= _count)
				{
					throw new IndexOutOfRangeException();
				}
				return _data[index];
			}

			public override byte GetByte(int index)
			{
				if (index == 0 || (_owner.HasSingleByteCharacters && !_owner.HasSurrogates()))
				{
					if (index >= _count)
					{
						throw new IndexOutOfRangeException();
					}
					char c = _data[index];
					if (c < '\u0080' || _owner.HasByteCharacters)
					{
						return (byte)_data[index];
					}
				}
				return SwitchToBinary().GetByte(index);
			}

			public override string GetStringSlice(int start, int count)
			{
				return _data.GetStringSlice(_count, start, count);
			}

			public override byte[] GetBinarySlice(int start, int count)
			{
				return SwitchToBinary().GetBinarySlice(start, count);
			}

			public override Content GetSlice(int start, int count)
			{
				return new CharArrayContent(_data.GetSlice(_count, start, count), _owner);
			}

			public override CharacterEnumerator GetCharacters()
			{
				return new CompositeCharacterEnumerator(_owner.Encoding, _data, _count, null);
			}

			public override IEnumerable<byte> GetBytes()
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.EnumerateAsBytes(_data, _count);
				}
				return SwitchToBinary().GetBytes();
			}

			public override bool StartsWith(char c)
			{
				if (_count != 0)
				{
					return _data[0] == c;
				}
				return false;
			}

			public override int IndexOf(char c, int start, int count)
			{
				count = Utils.NormalizeCount(_count, start, count);
				if (count <= 0)
				{
					return -1;
				}
				return Array.IndexOf(_data, c, start, count);
			}

			public override int IndexOf(byte b, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.IndexOf(_data, _count, b, start, count);
				}
				return SwitchToBinary().IndexOf(b, start, count);
			}

			public override int IndexOf(string str, int start, int count)
			{
				return Utils.IndexOf(_data, _count, str, start, count);
			}

			public override int IndexOf(byte[] bytes, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.IndexOf(_data, _count, bytes, start, count);
				}
				return SwitchToBinary().IndexOf(bytes, start, count);
			}

			public override int IndexIn(Content str, int start, int count)
			{
				return str.IndexOf(ToString(), start, count);
			}

			public override int LastIndexOf(char c, int start, int count)
			{
				Utils.NormalizeLastIndexOfIndices(_count, ref start, ref count);
				return Array.LastIndexOf(_data, c, start, count);
			}

			public override int LastIndexOf(byte b, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.LastIndexOf(_data, _count, b, start, count);
				}
				return SwitchToBinary().LastIndexOf(b, start, count);
			}

			public override int LastIndexOf(string str, int start, int count)
			{
				return Utils.LastIndexOf(_data, _count, str, start, count);
			}

			public override int LastIndexOf(byte[] bytes, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.LastIndexOf(_data, _count, bytes, start, count);
				}
				return SwitchToBinary().LastIndexOf(bytes, start, count);
			}

			public override int LastIndexIn(Content str, int start, int count)
			{
				return str.LastIndexOf(ToString(), start, count);
			}

			public override Content Concat(Content content)
			{
				return content.ConcatTo(this);
			}

			internal CharArrayContent Concatenate(StringContent content)
			{
				int length = content.Data.Length;
				char[] array = new char[_count + length];
				Array.Copy(_data, 0, array, 0, _count);
				content.Data.CopyTo(0, array, _count, length);
				return new CharArrayContent(array, null);
			}

			public override Content ConcatTo(BinaryContent content)
			{
				return content.Concatenate(this);
			}

			public override Content ConcatTo(CharArrayContent content)
			{
				return new CharArrayContent(Utils.Concatenate(content._data, content._count, _data, _count), null);
			}

			public override Content ConcatTo(StringContent content)
			{
				int length = content.Data.Length;
				char[] array = new char[length + _count];
				content.Data.CopyTo(0, array, 0, length);
				Array.Copy(_data, 0, array, length, _count);
				return new CharArrayContent(array, null);
			}

			public override void Append(char c, int repeatCount)
			{
				_count = Utils.Append(ref _data, _count, c, repeatCount);
			}

			public override void Append(byte b, int repeatCount)
			{
				SwitchToBinary(repeatCount).Append(b, repeatCount);
			}

			public override void Append(string str, int start, int count)
			{
				_count = Utils.Append(ref _data, _count, str, start, count);
			}

			public override void Append(char[] chars, int start, int count)
			{
				_count = Utils.Append(ref _data, _count, chars, start, count);
			}

			public override void Append(byte[] bytes, int start, int count)
			{
				SwitchToBinary(count).Append(bytes, start, count);
			}

			public override void Append(Stream stream, int count)
			{
				SwitchToBinary(count).Append(stream, count);
			}

			public override void AppendFormat(IFormatProvider provider, string format, object[] args)
			{
				string text = string.Format(provider, format, args);
				Append(text, 0, text.Length);
			}

			public override void Append(Content content, int start, int count)
			{
				content.AppendTo(this, start, count);
			}

			public override void AppendTo(BinaryContent content, int start, int count)
			{
				if (start > _count - count)
				{
					throw new ArgumentOutOfRangeException("start");
				}
				content.AppendBytes(_data, start, count);
			}

			public override void AppendTo(CharArrayContent content, int start, int count)
			{
				if (start > _count - count)
				{
					throw new ArgumentOutOfRangeException("start");
				}
				content.Append(_data, start, count);
			}

			public override void AppendTo(StringContent content, int start, int count)
			{
				if (start > _count - count)
				{
					throw new ArgumentOutOfRangeException("start");
				}
				content.Append(_data, start, count);
			}

			public override void Insert(int index, char c)
			{
				_count = Utils.InsertAt(ref _data, _count, index, c, 1);
			}

			public override void Insert(int index, byte b)
			{
				if (b < 128 && _owner.HasByteCharacters)
				{
					Insert(index, (char)b);
				}
				else
				{
					SwitchToBinary(1).Insert(index, b);
				}
			}

			public override void Insert(int index, string str, int start, int count)
			{
				_count = Utils.InsertAt(ref _data, _count, index, str, start, count);
			}

			public override void Insert(int index, char[] chars, int start, int count)
			{
				_count = Utils.InsertAt(ref _data, _count, index, chars, start, count);
			}

			public override void Insert(int index, byte[] bytes, int start, int count)
			{
				SwitchToBinary(count).Insert(index, bytes, start, count);
			}

			public override void InsertTo(Content str, int index, int start, int count)
			{
				str.Insert(index, _data, start, count);
			}

			public override void SetByte(int index, byte b)
			{
				if (b < 128 && _owner.HasByteCharacters)
				{
					DataSetChar(index, (char)b);
				}
				else
				{
					SwitchToBinary().SetByte(index, b);
				}
			}

			public override void SetChar(int index, char c)
			{
				DataSetChar(index, c);
			}

			public override void Remove(int start, int count)
			{
				_count = Utils.Remove(ref _data, _count, start, count);
			}

			public override void Write(int offset, byte[] value, int start, int count)
			{
				SwitchToBinary().Write(offset, value, start, count);
			}

			public override void Write(int offset, byte value, int repeatCount)
			{
				SwitchToBinary().Write(offset, value, repeatCount);
			}
		}

		[Serializable]
		internal sealed class StringContent : Content
		{
			private readonly string _data;

			internal string Data
			{
				get
				{
					return _data;
				}
			}

			public override int Count
			{
				get
				{
					return _data.Length;
				}
				set
				{
					SwitchToMutable(value - _data.Length).Count = value;
				}
			}

			public override bool IsEmpty
			{
				get
				{
					return _data.Length == 0;
				}
			}

			internal StringContent(string data, MutableString owner)
				: base(owner)
			{
				_data = data;
			}

			internal BinaryContent SwitchToBinary()
			{
				byte[] array = DataToBytes();
				return WrapContent(array, array.Length);
			}

			internal BinaryContent SwitchToBinary(int additionalCapacity)
			{
				return SwitchToBinary();
			}

			private CharArrayContent SwitchToMutable()
			{
				return WrapContent(_data.ToCharArray(), _data.Length);
			}

			private CharArrayContent SwitchToMutable(int additionalCapacity)
			{
				return SwitchToMutable();
			}

			internal byte[] DataToBytes()
			{
				if (_data.Length <= 0)
				{
					return Utils.EmptyBytes;
				}
				return _owner._encoding.StrictEncoding.GetBytes(_data);
			}

			internal int GetDataByteCount()
			{
				return _owner._encoding.StrictEncoding.GetByteCount(_data);
			}

			internal void GetDataBytes(byte[] bytes, int start)
			{
				_owner._encoding.StrictEncoding.GetBytes(_data, 0, _data.Length, bytes, start);
			}

			public override uint UpdateCharacterFlags(uint flags)
			{
				return Content.UpdateAsciiAndSurrogatesFlags(_data, flags);
			}

			public override int CalculateHashCode()
			{
				return _data.GetHashCode();
			}

			public override int GetCharCount()
			{
				return _data.Length;
			}

			public override int GetCharacterCount()
			{
				if (!_owner.HasSurrogates())
				{
					return _data.Length;
				}
				return _data.GetCharacterCount();
			}

			public override int GetByteCount()
			{
				if (_data.Length == 0 || (_owner.HasSingleByteCharacters && !_owner.HasSurrogates()))
				{
					return _data.Length;
				}
				return SwitchToBinary().GetByteCount();
			}

			public override Content Clone()
			{
				return new StringContent(_data, _owner);
			}

			public override void TrimExcess()
			{
			}

			public override int GetCapacity()
			{
				return _data.Length;
			}

			public override void SetCapacity(int capacity)
			{
				if (capacity < _data.Length)
				{
					throw new InvalidOperationException();
				}
				SwitchToMutable(capacity - _data.Length);
			}

			public override string ConvertToString()
			{
				return _data;
			}

			public override byte[] ConvertToBytes()
			{
				BinaryContent binaryContent = SwitchToBinary();
				return binaryContent.GetBinarySlice(0, binaryContent.GetByteCount());
			}

			public override string ToString()
			{
				return _data;
			}

			public override byte[] ToByteArray()
			{
				return DataToBytes();
			}

			internal override byte[] GetByteArray(out int count)
			{
				return SwitchToBinary().GetByteArray(out count);
			}

			public override Content SwitchToBinaryContent()
			{
				return SwitchToBinary();
			}

			public override Content SwitchToStringContent()
			{
				return this;
			}

			public override Content SwitchToMutableContent()
			{
				return SwitchToMutable();
			}

			public override Content EscapeRegularExpression()
			{
				StringBuilder stringBuilder = RubyRegex.EscapeToStringBuilder(_data);
				if (stringBuilder == null)
				{
					return this;
				}
				return new StringContent(stringBuilder.ToString(), _owner);
			}

			public override void CheckEncoding()
			{
				GetDataByteCount();
			}

			public override bool ContainsInvalidCharacters()
			{
				return Utils.ContainsInvalidCharacters(_data.ToCharArray(), 0, _data.Length, _owner._encoding.StrictEncoding);
			}

			public override int OrdinalCompareTo(string str)
			{
				return _data.ValueCompareTo(str);
			}

			internal int OrdinalCompareTo(char[] chars, int count)
			{
				return -chars.ValueCompareTo(count, _data);
			}

			public override int OrdinalCompareTo(Content content)
			{
				return content.ReverseOrdinalCompareTo(this);
			}

			public override int ReverseOrdinalCompareTo(BinaryContent content)
			{
				return SwitchToBinary().ReverseOrdinalCompareTo(content);
			}

			public override int ReverseOrdinalCompareTo(CharArrayContent content)
			{
				return content.OrdinalCompareTo(_data);
			}

			public override int ReverseOrdinalCompareTo(StringContent content)
			{
				return content.OrdinalCompareTo(_data);
			}

			public override char GetChar(int index)
			{
				return _data[index];
			}

			public override byte GetByte(int index)
			{
				if (index == 0 || (_owner.HasSingleByteCharacters && !_owner.HasSurrogates()))
				{
					if (index >= _data.Length)
					{
						throw new IndexOutOfRangeException();
					}
					char c = _data[index];
					if (c < '\u0080' || _owner.HasByteCharacters)
					{
						return (byte)_data[index];
					}
				}
				return SwitchToBinary().GetByte(index);
			}

			public override string GetStringSlice(int start, int count)
			{
				return _data.GetSlice(start, count);
			}

			public override byte[] GetBinarySlice(int start, int count)
			{
				return SwitchToBinary().GetBinarySlice(start, count);
			}

			public override Content GetSlice(int start, int count)
			{
				return new StringContent(_data.GetSlice(start, count), _owner);
			}

			public override CharacterEnumerator GetCharacters()
			{
				return new StringCharacterEnumerator(_owner.Encoding, _data);
			}

			public override IEnumerable<byte> GetBytes()
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.EnumerateAsBytes(_data);
				}
				return SwitchToBinary().GetBytes();
			}

			public override bool StartsWith(char c)
			{
				if (_data.Length != 0)
				{
					return _data[0] == c;
				}
				return false;
			}

			public override int IndexOf(char c, int start, int count)
			{
				count = Utils.NormalizeCount(_data.Length, start, count);
				if (count <= 0)
				{
					return -1;
				}
				return _data.IndexOf(c, start, count);
			}

			public override int IndexOf(byte b, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.IndexOf(_data, b, start, count);
				}
				return SwitchToBinary().IndexOf(b, start, count);
			}

			public override int IndexOf(string str, int start, int count)
			{
				if (str.Length == 0)
				{
					return start;
				}
				count = Utils.NormalizeCount(_data.Length, start, count);
				if (count <= 0)
				{
					return -1;
				}
				return _data.IndexOf(str, start, count, StringComparison.Ordinal);
			}

			public override int IndexOf(byte[] bytes, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.IndexOf(_data, bytes, start, count);
				}
				return SwitchToBinary().IndexOf(bytes, start, count);
			}

			public override int IndexIn(Content str, int start, int count)
			{
				return str.IndexOf(_data, start, count);
			}

			public override int LastIndexOf(char c, int start, int count)
			{
				Utils.NormalizeLastIndexOfIndices(_data.Length, ref start, ref count);
				return _data.LastIndexOf(c, start, count);
			}

			public override int LastIndexOf(byte b, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.LastIndexOf(_data, b, start, count);
				}
				return SwitchToBinary().LastIndexOf(b, start, count);
			}

			public override int LastIndexOf(string str, int start, int count)
			{
				Utils.NormalizeLastIndexOfIndices(_data.Length, ref start, ref count);
				return _data.LastIndexOf(str, start, count, StringComparison.Ordinal);
			}

			public override int LastIndexOf(byte[] bytes, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.LastIndexOf(_data, bytes, start, count);
				}
				return SwitchToBinary().LastIndexOf(bytes, start, count);
			}

			public override int LastIndexIn(Content str, int start, int count)
			{
				return str.LastIndexOf(_data, start, count);
			}

			public override Content Concat(Content content)
			{
				return content.ConcatTo(this);
			}

			public override Content ConcatTo(BinaryContent content)
			{
				return content.Concatenate(this);
			}

			public override Content ConcatTo(CharArrayContent content)
			{
				return content.Concatenate(this);
			}

			public override Content ConcatTo(StringContent content)
			{
				return new StringContent(content.Data + _data, null);
			}

			public override void Append(char c, int repeatCount)
			{
				SwitchToMutable(repeatCount).Append(c, repeatCount);
			}

			public override void Append(byte b, int repeatCount)
			{
				SwitchToBinary(repeatCount).Append(b, repeatCount);
			}

			public override void Append(string str, int start, int count)
			{
				SwitchToMutable(count).Append(str, start, count);
			}

			public override void Append(char[] chars, int start, int count)
			{
				SwitchToMutable(count).Append(chars, start, count);
			}

			public override void Append(byte[] bytes, int start, int count)
			{
				SwitchToBinary(count).Append(bytes, start, count);
			}

			public override void Append(Stream stream, int count)
			{
				SwitchToBinary(count).Append(stream, count);
			}

			public override void AppendFormat(IFormatProvider provider, string format, object[] args)
			{
				SwitchToMutable().AppendFormat(provider, format, args);
			}

			public override void Append(Content content, int start, int count)
			{
				content.AppendTo(this, start, count);
			}

			public override void AppendTo(BinaryContent content, int start, int count)
			{
				content.AppendBytes(_data, start, count);
			}

			public override void AppendTo(CharArrayContent content, int start, int count)
			{
				content.Append(_data, start, count);
			}

			public override void AppendTo(StringContent content, int start, int count)
			{
				content.Append(_data, start, count);
			}

			public override void Insert(int index, char c)
			{
				SwitchToMutable().Insert(index, c);
			}

			public override void Insert(int index, byte b)
			{
				SwitchToBinary().Insert(index, b);
			}

			public override void Insert(int index, string str, int start, int count)
			{
				SwitchToMutable().Insert(index, str, start, count);
			}

			public override void Insert(int index, char[] chars, int start, int count)
			{
				SwitchToMutable().Insert(index, chars, start, count);
			}

			public override void Insert(int index, byte[] bytes, int start, int count)
			{
				SwitchToBinary().Insert(index, bytes, start, count);
			}

			public override void InsertTo(Content str, int index, int start, int count)
			{
				str.Insert(index, _data, start, count);
			}

			public override void SetByte(int index, byte b)
			{
				SwitchToBinary().SetByte(index, b);
			}

			public override void SetChar(int index, char c)
			{
				SwitchToMutable().DataSetChar(index, c);
			}

			public override void Remove(int start, int count)
			{
				SwitchToMutable().Remove(start, count);
			}

			public override void Write(int offset, byte[] value, int start, int count)
			{
				SwitchToBinary().Write(offset, value, start, count);
			}

			public override void Write(int offset, byte value, int repeatCount)
			{
				SwitchToBinary().Write(offset, value, repeatCount);
			}
		}

		[Serializable]
		internal class BinaryContent : Content
		{
			protected byte[] _data;

			protected int _count;

			public override int Count
			{
				get
				{
					return _count;
				}
				set
				{
					if (_data.Length < value)
					{
						Array.Resize(ref _data, Utils.GetExpandedSize(_data, value));
					}
					else
					{
						Utils.Fill(_data, _count, (byte)0, value - _count);
					}
					_count = value;
				}
			}

			public override bool IsEmpty
			{
				get
				{
					return _count == 0;
				}
			}

			internal BinaryContent(byte[] data, MutableString owner)
				: this(data, data.Length, owner)
			{
			}

			internal BinaryContent(byte[] data, int count, MutableString owner)
				: base(owner)
			{
				_data = data;
				_count = count;
			}

			protected virtual BinaryContent Create(byte[] data, MutableString owner)
			{
				return new BinaryContent(data, owner);
			}

			private void Decode(out char[] chars, out List<byte[]> invalidCharacters)
			{
				Decoder decoder = _owner.Encoding.Encoding.GetDecoder();
				LosslessDecoderFallback losslessDecoderFallback = (LosslessDecoderFallback)(decoder.Fallback = new LosslessDecoderFallback());
				losslessDecoderFallback.Track = true;
				chars = new char[decoder.GetCharCount(_data, 0, _count, true)];
				losslessDecoderFallback.Track = false;
				decoder.GetChars(_data, 0, _count, chars, 0, true);
				invalidCharacters = losslessDecoderFallback.InvalidCharacters;
			}

			private CharArrayContent SwitchToChars()
			{
				return SwitchToChars(0);
			}

			private CharArrayContent SwitchToChars(int additionalCapacity)
			{
				char[] array = DataToChars(additionalCapacity, _owner._encoding.StrictEncoding);
				return WrapContent(array, array.Length - additionalCapacity);
			}

			private char[] DataToChars(int additionalCapacity, Encoding encoding)
			{
				if (_count == 0)
				{
					if (additionalCapacity != 0)
					{
						return new char[additionalCapacity];
					}
					return Utils.EmptyChars;
				}
				if (additionalCapacity == 0)
				{
					return encoding.GetChars(_data, 0, _count);
				}
				char[] array = new char[encoding.GetCharCount(_data, 0, _count) + additionalCapacity];
				encoding.GetChars(_data, 0, _count, array, 0);
				return array;
			}

			private string DataToString()
			{
				if (_count == 0)
				{
					return string.Empty;
				}
				return _owner._encoding.StrictEncoding.GetString(_data, 0, _count);
			}

			internal void AppendBytes(string str, int start, int count)
			{
				_count = Utils.Append(ref _data, _count, str, start, count, _owner._encoding.StrictEncoding);
			}

			internal void AppendBytes(char[] chars, int start, int count)
			{
				_count = Utils.Append(ref _data, _count, chars, start, count, _owner._encoding.StrictEncoding);
			}

			public override uint UpdateCharacterFlags(uint flags)
			{
				if (_data.IsAscii(_count))
				{
					return (flags & 0xFFFFFFEFu) | 8u;
				}
				return flags & 0xFFFFFFE7u;
			}

			public override int CalculateHashCode()
			{
				if (_count == 0)
				{
					return string.Empty.GetHashCode();
				}
				char[] chars;
				List<byte[]> invalidCharacters;
				Decode(out chars, out invalidCharacters);
				if (invalidCharacters == null)
				{
					return WrapContent(chars, chars.Length).CalculateHashCode();
				}
				return new string(chars).GetHashCode();
			}

			public override int GetCharCount()
			{
				if (!_owner.HasSingleByteCharacters && _count != 0)
				{
					return SwitchToChars().GetCharCount();
				}
				return _count;
			}

			public override int GetCharacterCount()
			{
				if (_owner.HasSingleByteCharacters || _count == 0)
				{
					return _count;
				}
				char[] chars;
				List<byte[]> invalidCharacters;
				Decode(out chars, out invalidCharacters);
				if (invalidCharacters == null)
				{
					return WrapContent(chars, chars.Length).GetCharacterCount();
				}
				return chars.GetCharacterCount(chars.Length);
			}

			public override int GetByteCount()
			{
				return _count;
			}

			public override Content Clone()
			{
				return Create(ToByteArray(), _owner);
			}

			public override void TrimExcess()
			{
				Utils.TrimExcess(ref _data, _count);
			}

			public override int GetCapacity()
			{
				return _data.Length;
			}

			public override void SetCapacity(int capacity)
			{
				if (capacity < _count)
				{
					throw new InvalidOperationException();
				}
				Array.Resize(ref _data, capacity);
			}

			public override string ConvertToString()
			{
				return SwitchToChars().ConvertToString();
			}

			public override byte[] ConvertToBytes()
			{
				return _data.GetSlice(0, _count);
			}

			public override string ToString()
			{
				return DataToString();
			}

			public override byte[] ToByteArray()
			{
				return _data.GetSlice(0, _count);
			}

			internal override byte[] GetByteArray(out int count)
			{
				count = _count;
				return _data;
			}

			public override Content SwitchToBinaryContent()
			{
				return this;
			}

			public override Content SwitchToStringContent()
			{
				return SwitchToChars();
			}

			public override Content SwitchToMutableContent()
			{
				return this;
			}

			public override Content EscapeRegularExpression()
			{
				byte[] array = ToByteArray();
				return Create(BinaryEncoding.Instance.GetBytes(RubyRegex.Escape(BinaryEncoding.Instance.GetString(array, 0, array.Length))), _owner);
			}

			public override void CheckEncoding()
			{
				_owner._encoding.StrictEncoding.GetCharCount(_data, 0, _count);
			}

			public override bool ContainsInvalidCharacters()
			{
				return Utils.ContainsInvalidCharacters(_data, 0, _count, _owner._encoding.StrictEncoding);
			}

			public override int OrdinalCompareTo(string str)
			{
				if (_owner.HasByteCharacters)
				{
					return _data.ValueCompareTo(_count, str);
				}
				return SwitchToChars().OrdinalCompareTo(str);
			}

			internal int OrdinalCompareTo(byte[] bytes, int count)
			{
				return _data.ValueCompareTo(_count, bytes, count);
			}

			public override int OrdinalCompareTo(Content content)
			{
				return content.ReverseOrdinalCompareTo(this);
			}

			public override int ReverseOrdinalCompareTo(BinaryContent content)
			{
				return content.OrdinalCompareTo(_data, _count);
			}

			public override int ReverseOrdinalCompareTo(CharArrayContent content)
			{
				return content.SwitchToBinary().OrdinalCompareTo(_data, _count);
			}

			public override int ReverseOrdinalCompareTo(StringContent content)
			{
				return content.SwitchToBinary().OrdinalCompareTo(_data, _count);
			}

			public override char GetChar(int index)
			{
				if (_owner.HasSingleByteCharacters || index == 0)
				{
					if (index >= _count)
					{
						throw new IndexOutOfRangeException();
					}
					byte b = _data[index];
					if (b < 128 || _owner.HasByteCharacters)
					{
						return (char)b;
					}
				}
				return SwitchToChars().GetChar(index);
			}

			public override byte GetByte(int index)
			{
				if (index >= _count)
				{
					throw new IndexOutOfRangeException();
				}
				return _data[index];
			}

			public override string GetStringSlice(int start, int count)
			{
				return SwitchToChars().GetStringSlice(start, count);
			}

			public override byte[] GetBinarySlice(int start, int count)
			{
				return _data.GetSlice(_count, start, count);
			}

			public override Content GetSlice(int start, int count)
			{
				return Create(_data.GetSlice(_count, start, count), _owner);
			}

			public override CharacterEnumerator GetCharacters()
			{
				if (_owner.HasByteCharacters)
				{
					return new BinaryCharacterEnumerator(_owner.Encoding, _data, _count);
				}
				char[] allValid;
				CharacterEnumerator result = EnumerateAsCharacters(_data, _count, _owner.Encoding, out allValid);
				if (allValid != null)
				{
					WrapContent(allValid, allValid.Length);
				}
				return result;
			}

			public override IEnumerable<byte> GetBytes()
			{
				return Utils.Enumerate(_data, _count);
			}

			public override bool StartsWith(char c)
			{
				if (_count == 0)
				{
					return false;
				}
				if (_owner.HasByteCharacters || (c < '\u0080' && _owner._encoding.IsAsciiIdentity))
				{
					return _data[0] == c;
				}
				byte[] array = new byte[_owner.Encoding.MaxBytesPerChar];
				int bytes = _owner.Encoding.StrictEncoding.GetBytes(new char[1] { c }, 0, 1, array, 0);
				if (bytes > _count)
				{
					return false;
				}
				return _data.ValueCompareTo(bytes, array, bytes) == 0;
			}

			public override int IndexOf(char c, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.IndexOf(_data, _count, c, start, count);
				}
				return SwitchToChars().IndexOf(c, start, count);
			}

			public override int IndexOf(byte b, int start, int count)
			{
				count = Utils.NormalizeCount(_count, start, count);
				if (count <= 0)
				{
					return -1;
				}
				return Array.IndexOf(_data, b, start, count);
			}

			public override int IndexOf(string str, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.IndexOf(_data, _count, str, start, count);
				}
				return SwitchToChars().IndexOf(str, start, count);
			}

			public override int IndexOf(byte[] bytes, int start, int count)
			{
				return Utils.IndexOf(_data, _count, bytes, start, count);
			}

			public override int IndexIn(Content str, int start, int count)
			{
				return str.IndexOf(_data, start, count);
			}

			public override int LastIndexOf(char c, int start, int count)
			{
				return SwitchToChars().LastIndexOf(c, start, count);
			}

			public override int LastIndexOf(byte b, int start, int count)
			{
				Utils.NormalizeLastIndexOfIndices(_count, ref start, ref count);
				return Array.LastIndexOf(_data, b, start, count);
			}

			public override int LastIndexOf(string str, int start, int count)
			{
				if (_owner.HasByteCharacters)
				{
					return Utils.LastIndexOf(_data, _count, str, start, count);
				}
				return SwitchToChars().LastIndexOf(str, start, count);
			}

			public override int LastIndexOf(byte[] bytes, int start, int count)
			{
				return Utils.LastIndexOf(_data, _count, bytes, start, count);
			}

			public override int LastIndexIn(Content str, int start, int count)
			{
				return str.LastIndexOf(_data, start, count);
			}

			public override Content Concat(Content content)
			{
				return content.ConcatTo(this);
			}

			internal BinaryContent Concatenate(CharArrayContent content)
			{
				int dataByteCount = content.GetDataByteCount();
				byte[] array = new byte[_count + dataByteCount];
				Array.Copy(_data, 0, array, 0, _count);
				content.GetDataBytes(array, _count);
				return Create(array, null);
			}

			internal BinaryContent Concatenate(StringContent content)
			{
				int dataByteCount = content.GetDataByteCount();
				byte[] array = new byte[_count + dataByteCount];
				Array.Copy(_data, 0, array, 0, _count);
				content.GetDataBytes(array, _count);
				return Create(array, null);
			}

			public override Content ConcatTo(BinaryContent content)
			{
				return Create(Utils.Concatenate(content._data, content._count, _data, _count), null);
			}

			public override Content ConcatTo(CharArrayContent content)
			{
				int dataByteCount = content.GetDataByteCount();
				byte[] array = new byte[dataByteCount + _count];
				content.GetDataBytes(array, 0);
				Array.Copy(_data, 0, array, dataByteCount, _count);
				return Create(array, null);
			}

			public override Content ConcatTo(StringContent content)
			{
				int dataByteCount = content.GetDataByteCount();
				byte[] array = new byte[dataByteCount + _count];
				content.GetDataBytes(array, 0);
				Array.Copy(_data, 0, array, dataByteCount, _count);
				return Create(array, null);
			}

			public override void Append(char c, int repeatCount)
			{
				if (_owner.HasByteCharacters || (c < '\u0080' && _owner._encoding.IsAsciiIdentity))
				{
					Append((byte)c, repeatCount);
				}
				else
				{
					_count = Utils.Append(ref _data, _count, c, repeatCount, _owner._encoding.StrictEncoding);
				}
			}

			public override void Append(byte b, int repeatCount)
			{
				_count = Utils.Append(ref _data, _count, b, repeatCount);
			}

			public override void Append(string str, int start, int count)
			{
				AppendBytes(str, start, count);
			}

			public override void Append(char[] chars, int start, int count)
			{
				AppendBytes(chars, start, count);
			}

			public override void Append(byte[] bytes, int start, int count)
			{
				_count = Utils.Append(ref _data, _count, bytes, start, count);
			}

			public override void Append(Stream stream, int count)
			{
				Utils.Resize(ref _data, _count + count);
				_count += stream.Read(_data, _count, count);
			}

			public override void AppendFormat(IFormatProvider provider, string format, object[] args)
			{
				string text = string.Format(provider, format, args);
				AppendBytes(text, 0, text.Length);
			}

			public override void Append(Content content, int start, int count)
			{
				content.AppendTo(this, start, count);
			}

			public override void AppendTo(BinaryContent content, int start, int count)
			{
				if (start > _count - count)
				{
					throw new ArgumentOutOfRangeException("start");
				}
				content.Append(_data, start, count);
			}

			public override void AppendTo(CharArrayContent content, int start, int count)
			{
				if (start > _count - count)
				{
					throw new ArgumentOutOfRangeException("start");
				}
				content.SwitchToBinary().Append(_data, start, count);
			}

			public override void AppendTo(StringContent content, int start, int count)
			{
				if (start > _count - count)
				{
					throw new ArgumentOutOfRangeException("start");
				}
				content.SwitchToBinary().Append(_data, start, count);
			}

			public override void Insert(int index, char c)
			{
				if (_owner.HasByteCharacters || (c < '\u0080' && _owner._encoding.IsAsciiIdentity))
				{
					_count = Utils.InsertAt(ref _data, _count, index, (byte)c, 1);
				}
				else
				{
					SwitchToChars(1).Insert(index, c);
				}
			}

			public override void Insert(int index, byte b)
			{
				_count = Utils.InsertAt(ref _data, _count, index, b, 1);
			}

			public override void Insert(int index, string str, int start, int count)
			{
				SwitchToChars(count).Insert(index, str, start, count);
			}

			public override void Insert(int index, char[] chars, int start, int count)
			{
				SwitchToChars(count).Insert(index, chars, start, count);
			}

			public override void Insert(int index, byte[] bytes, int start, int count)
			{
				_count = Utils.InsertAt(ref _data, _count, index, bytes, start, count);
			}

			public override void InsertTo(Content str, int index, int start, int count)
			{
				str.Insert(index, _data, start, count);
			}

			public override void SetByte(int index, byte b)
			{
				if (index >= _count)
				{
					throw new IndexOutOfRangeException();
				}
				_data[index] = b;
			}

			public override void SetChar(int index, char c)
			{
				if (_owner.HasByteCharacters || (c < '\u0080' && _owner.HasSingleByteCharacters))
				{
					SetByte(index, (byte)c);
				}
				else
				{
					SwitchToChars().DataSetChar(index, c);
				}
			}

			public override void Remove(int start, int count)
			{
				_count = Utils.Remove(ref _data, _count, start, count);
			}

			public override void Write(int offset, byte[] value, int start, int count)
			{
				Utils.Resize(ref _data, offset + count);
				_count = Math.Max(_count, offset + count);
				Buffer.BlockCopy(value, start, _data, offset, count);
			}

			public override void Write(int offset, byte value, int repeatCount)
			{
				int num = offset + repeatCount;
				Utils.Resize(ref _data, num);
				if (num > _count)
				{
					_count = num;
				}
				for (int i = offset; i < num; i++)
				{
					_data[i] = value;
				}
			}
		}

		[DebuggerDisplay("{_immediateClass.GetDebuggerDisplayValue(this),nq}", Type = "{_immediateClass.GetDebuggerDisplayType(),nq}")]
		[DebuggerTypeProxy(typeof(RubyObjectDebugView))]
		public sealed class Subclass : MutableString, IRubyObject, IRubyObjectState
		{
			private RubyInstanceData _instanceData;

			private RubyClass _immediateClass;

			public RubyClass ImmediateClass
			{
				get
				{
					return _immediateClass;
				}
				set
				{
					_immediateClass = value;
				}
			}

			public Subclass(RubyClass rubyClass)
				: this(rubyClass, RubyEncoding.Binary)
			{
			}

			public Subclass(RubyClass rubyClass, RubyEncoding encoding)
				: base(encoding)
			{
				ImmediateClass = rubyClass;
			}

			private Subclass(RubyClass rubyClass, Content content, RubyEncoding encoding)
				: base(content, encoding)
			{
				ImmediateClass = rubyClass;
			}

			private Subclass(Subclass str)
				: base(str)
			{
				ImmediateClass = str.ImmediateClass;
			}

			internal override MutableString CreateInstance(Content content, RubyEncoding encoding)
			{
				return new Subclass(ImmediateClass, content, encoding);
			}

			public override MutableString CreateInstance()
			{
				return new Subclass(ImmediateClass, _encoding);
			}

			public override MutableString Clone()
			{
				return new Subclass(this);
			}

			public RubyInstanceData GetInstanceData()
			{
				return RubyOps.GetInstanceData(ref _instanceData);
			}

			public RubyInstanceData TryGetInstanceData()
			{
				return _instanceData;
			}

			public int BaseGetHashCode()
			{
				return GetHashCode();
			}

			public bool BaseEquals(object other)
			{
				return Equals(other);
			}

			public string BaseToString()
			{
				return ToString();
			}
		}

		public struct Character : IEquatable<Character>
		{
			public readonly byte[] Invalid;

			public readonly char Value;

			public readonly char LowSurrogate;

			public bool IsValid
			{
				get
				{
					return Invalid == null;
				}
			}

			public bool IsSurrogate
			{
				get
				{
					return LowSurrogate != '\0';
				}
			}

			public int Codepoint
			{
				get
				{
					if (!IsSurrogate)
					{
						return Value;
					}
					return Tokenizer.ToCodePoint(Value, LowSurrogate);
				}
			}

			internal Character(byte[] invalid)
			{
				Invalid = invalid;
				Value = '\0';
				LowSurrogate = '\0';
			}

			internal Character(char value)
			{
				Invalid = null;
				Value = value;
				LowSurrogate = '\0';
			}

			internal Character(char highSurrogate, char lowSurrogate)
			{
				Invalid = null;
				Value = highSurrogate;
				LowSurrogate = lowSurrogate;
			}

			public bool Equals(Character other)
			{
				if (IsValid)
				{
					if (other.IsValid && Value == other.Value)
					{
						return LowSurrogate == other.LowSurrogate;
					}
					return false;
				}
				if (!other.IsValid)
				{
					return Invalid.ValueEquals(other.Invalid);
				}
				return false;
			}

			public MutableString ToMutableString(RubyEncoding encoding)
			{
				if (IsValid)
				{
					if (!IsSurrogate)
					{
						return new MutableString(new char[1] { Value }, encoding);
					}
					return new MutableString(new char[2] { Value, LowSurrogate }, encoding);
				}
				return new MutableString(ArrayUtils.Copy(Invalid), encoding);
			}
		}

		public abstract class CharacterEnumerator : IEnumerator<Character>, IDisposable, IEnumerator
		{
			private readonly RubyEncoding _encoding;

			internal int _index;

			internal Character _current;

			public Character Current
			{
				get
				{
					if (_index < 0)
					{
						throw new InvalidOperationException();
					}
					return _current;
				}
			}

			public abstract bool HasMore { get; }

			object IEnumerator.Current
			{
				get
				{
					return _current;
				}
			}

			protected CharacterEnumerator(RubyEncoding encoding)
			{
				_encoding = encoding;
				_index = -1;
			}

			public virtual void Reset()
			{
				_index = -1;
				_current = default(Character);
			}

			internal void AppendTo(MutableString str)
			{
				ContractUtils.Requires(_encoding == str.Encoding);
				if (_index < 0)
				{
					_index = 0;
				}
				AppendDataTo(str);
				Reset();
			}

			internal abstract void AppendDataTo(MutableString str);

			public abstract bool MoveNext();

			void IDisposable.Dispose()
			{
			}
		}

		internal sealed class StringCharacterEnumerator : CharacterEnumerator
		{
			private readonly string _data;

			public override bool HasMore
			{
				get
				{
					return _index < _data.Length;
				}
			}

			internal StringCharacterEnumerator(RubyEncoding encoding, string data)
				: base(encoding)
			{
				_data = data;
			}

			public override bool MoveNext()
			{
				int num = _index;
				if (num < 0)
				{
					num = 0;
				}
				if (num == _data.Length)
				{
					_index = num;
					return false;
				}
				char c;
				char lowSurrogate;
				if (Tokenizer.IsHighSurrogate(c = _data[num]) && num + 1 < _data.Length && Tokenizer.IsLowSurrogate(lowSurrogate = _data[num + 1]))
				{
					_current = new Character(c, lowSurrogate);
					_index = num + 2;
				}
				else
				{
					_current = new Character(c);
					_index = num + 1;
				}
				return true;
			}

			internal override void AppendDataTo(MutableString str)
			{
				str.Append(_data, _index, _data.Length - _index);
			}
		}

		internal sealed class BinaryCharacterEnumerator : CharacterEnumerator
		{
			private readonly byte[] _data;

			private readonly int _count;

			public override bool HasMore
			{
				get
				{
					return _index < _count;
				}
			}

			internal BinaryCharacterEnumerator(RubyEncoding encoding, byte[] data, int count)
				: base(encoding)
			{
				_data = data;
				_count = count;
			}

			public override bool MoveNext()
			{
				if (_index < 0)
				{
					_index = 0;
				}
				if (!HasMore)
				{
					return false;
				}
				_current = new Character((char)_data[_index++]);
				return true;
			}

			internal override void AppendDataTo(MutableString str)
			{
				str.Append(_data, _index, _count - _index);
			}
		}

		internal sealed class CompositeCharacterEnumerator : CharacterEnumerator
		{
			private readonly char[] _data;

			private readonly int _count;

			private readonly List<byte[]> _invalid;

			private int _invalidIndex;

			private int InvalidCount
			{
				get
				{
					if (_invalid == null)
					{
						return 0;
					}
					return _invalid.Count;
				}
			}

			public override bool HasMore
			{
				get
				{
					return _index < _count;
				}
			}

			internal CompositeCharacterEnumerator(RubyEncoding encoding, char[] data, int count, List<byte[]> invalid)
				: base(encoding)
			{
				_data = data;
				_count = count;
				_invalid = invalid;
			}

			internal override void AppendDataTo(MutableString str)
			{
				while (_index < _count && _invalidIndex < InvalidCount)
				{
					int num = Array.IndexOf(_data, '\uffff', _index);
					str.Append(_data, _index, num - _index);
					_index = num + 1;
					str.Append(_invalid[_invalidIndex++]);
				}
				str.Append(_data, _index, _count - _index);
			}

			public override bool MoveNext()
			{
				int num = _index;
				if (num < 0)
				{
					num = 0;
				}
				if (num == _count)
				{
					_index = num;
					return false;
				}
				char c = _data[num];
				if (c != '\uffff')
				{
					char lowSurrogate;
					if (Tokenizer.IsHighSurrogate(c) && num + 1 < _data.Length && Tokenizer.IsLowSurrogate(lowSurrogate = _data[num + 1]))
					{
						_current = new Character(c, lowSurrogate);
						_index = num + 2;
					}
					else
					{
						_current = new Character(c);
						_index = num + 1;
					}
				}
				else
				{
					if (_invalidIndex >= InvalidCount)
					{
						throw new InvalidOperationException("Decoder produced an invalid chracter \uffff.");
					}
					_current = new Character(_invalid[_invalidIndex++]);
					_index = num + 1;
				}
				return true;
			}

			public override void Reset()
			{
				base.Reset();
				_invalidIndex = -1;
			}
		}

		private sealed class DumpDecoderFallback : DecoderFallback
		{
			internal sealed class Buffer : DecoderFallbackBuffer
			{
				private readonly DumpDecoderFallback _fallback;

				private int _index;

				private byte[] _bytes;

				public bool HasInvalidCharacters
				{
					get
					{
						return _bytes != null;
					}
				}

				public override int Remaining
				{
					get
					{
						return _bytes.Length * 4 - _index;
					}
				}

				public Buffer(DumpDecoderFallback fallback)
				{
					_fallback = fallback;
				}

				public override bool Fallback(byte[] bytesUnknown, int index)
				{
					_bytes = bytesUnknown;
					_index = 0;
					return true;
				}

				public override char GetNextChar()
				{
					if (Remaining == 0)
					{
						return '\0';
					}
					int num = _index % 4;
					int num2 = _bytes[_index / 4];
					_index++;
					if (_fallback._octalEscapes)
					{
						switch (num)
						{
						case 0:
							return '\uffff';
						case 1:
							return (char)(48 + (num2 >> 6));
						case 2:
							return (char)(48 + ((num2 >> 3) & 7));
						case 3:
							return (char)(48 + (num2 & 7));
						}
					}
					else
					{
						switch (num)
						{
						case 0:
							return '\uffff';
						case 1:
							return 'x';
						case 2:
							return (num2 >> 4).ToUpperHexDigit();
						case 3:
							return (num2 & 0xF).ToUpperHexDigit();
						}
					}
					throw Assert.Unreachable;
				}

				public override bool MovePrevious()
				{
					if (_index == 0)
					{
						return false;
					}
					_index--;
					return true;
				}

				public override void Reset()
				{
					_index = 0;
				}
			}

			private const int ReplacementLength = 4;

			internal const char EscapePlaceholder = '\uffff';

			private readonly bool _octalEscapes;

			public override int MaxCharCount
			{
				get
				{
					return 4;
				}
			}

			public DumpDecoderFallback(bool octalEscapes)
			{
				_octalEscapes = octalEscapes;
			}

			public override DecoderFallbackBuffer CreateFallbackBuffer()
			{
				return new Buffer(this);
			}
		}

		[Flags]
		public enum Escape
		{
			Default = 0,
			NonAscii = 1,
			Special = 2,
			Octal = 4
		}

		private const uint IsFrozenFlag = 1u;

		private const uint HasChangedFlag = 2u;

		private const uint HasChangedCharArrayToStringFlag = 4u;

		private const uint HasChangedFlags = 6u;

		private const uint IsAsciiFlag = 8u;

		private const uint AsciiUnknownFlag = 16u;

		private const uint NoSurrogatesFlag = 32u;

		private const uint SurrogatesUnknownFlag = 64u;

		private const uint IsTaintedFlag = 256u;

		private const uint IsUntrustedFlag = 512u;

		private const uint CopyOnWriteFlag = 1024u;

		private Content _content;

		private RubyEncoding _encoding;

		private uint _flags = 80u;

		public static readonly MutableString FrozenEmpty = CreateEmpty().Freeze();

		public bool HasByteCharacters
		{
			get
			{
				if ((_flags & 0x18) != 8)
				{
					return _encoding == RubyEncoding.Binary;
				}
				return true;
			}
		}

		public bool HasSingleByteCharacters
		{
			get
			{
				if ((_flags & 0x18) != 8)
				{
					return _encoding.IsSingleByteCharacterSet;
				}
				return true;
			}
		}

		public bool KnowsAscii
		{
			get
			{
				if ((_flags & 0x10u) != 0)
				{
					return !_encoding.IsAsciiIdentity;
				}
				return true;
			}
		}

		public bool KnowsSurrogates
		{
			get
			{
				if ((_flags & 0x40u) != 0)
				{
					return _encoding.InUnicodeBasicPlane;
				}
				return true;
			}
		}

		public bool IsBinary
		{
			get
			{
				return _content.GetType() == typeof(BinaryContent);
			}
		}

		public RubyEncoding Encoding
		{
			get
			{
				return _encoding;
			}
		}

		public bool IsTainted
		{
			get
			{
				return (_flags & 0x100) != 0;
			}
			set
			{
				uint flags = _flags;
				if ((flags & (true ? 1u : 0u)) != 0)
				{
					throw RubyExceptions.CreateObjectFrozenError();
				}
				_flags = (flags & 0xFFFFFEFFu) | (value ? 256u : 0u);
			}
		}

		public bool IsUntrusted
		{
			get
			{
				return (_flags & 0x200) != 0;
			}
			set
			{
				uint flags = _flags;
				if ((flags & (true ? 1u : 0u)) != 0)
				{
					throw RubyExceptions.CreateObjectFrozenError();
				}
				_flags = (flags & 0xFFFFFDFFu) | (value ? 512u : 0u);
			}
		}

		public bool IsFrozen
		{
			get
			{
				return (_flags & 1) != 0;
			}
		}

		public bool HasChanged
		{
			get
			{
				return (_flags & 2) != 0;
			}
		}

		internal string Dump
		{
			get
			{
				return ToString();
			}
		}

		public bool IsEmpty
		{
			get
			{
				return _content.IsEmpty;
			}
		}

		public int Length
		{
			get
			{
				return _content.Count;
			}
		}

		public int Capacity
		{
			get
			{
				return _content.GetCapacity();
			}
			set
			{
				_content.SetCapacity(value);
			}
		}

		private void SetContent(Content content)
		{
			content.SetOwner(this);
			_content = content;
		}

		private void SetEncoding(RubyEncoding encoding)
		{
			uint num = _flags;
			if (!encoding.IsAsciiIdentity)
			{
				num &= 0xFFFFFFE7u;
			}
			num = ((!encoding.InUnicodeBasicPlane) ? (num | 0x40u) : ((num & 0xFFFFFFBFu) | 0x20u));
			_flags = num | 2u;
			_encoding = encoding;
		}

		internal MutableString(Content content, RubyEncoding encoding)
		{
			SetEncoding(encoding);
			SetContent(content);
		}

		protected MutableString(MutableString str)
			: this(str._content.Clone(), str._encoding)
		{
			IsTainted = str.IsTainted;
			IsUntrusted = str.IsUntrusted;
		}

		private MutableString(char[] chars, RubyEncoding encoding)
			: this(new CharArrayContent(chars, null), encoding)
		{
		}

		private MutableString(char[] chars, int count, RubyEncoding encoding)
			: this(new CharArrayContent(chars, count, null), encoding)
		{
		}

		private MutableString(byte[] bytes, RubyEncoding encoding)
			: this(new BinaryContent(bytes, null), encoding)
		{
		}

		internal MutableString(byte[] bytes, int count, RubyEncoding encoding)
			: this(new BinaryContent(bytes, count, null), encoding)
		{
		}

		private MutableString(string str, RubyEncoding encoding)
			: this(new StringContent(str, null), encoding)
		{
		}

		protected MutableString(RubyEncoding encoding)
			: this(new CharArrayContent(Utils.EmptyChars, 0, null), encoding)
		{
		}

		public MutableString()
			: this(string.Empty, RubyEncoding.Binary)
		{
		}

		public static MutableString CreateMutable(RubyEncoding encoding)
		{
			return new MutableString(encoding);
		}

		public static MutableString CreateMutable(int capacity, RubyEncoding encoding)
		{
			ContractUtils.Requires(capacity >= 0, "Capacity must be greater or equal to zero.");
			ContractUtils.RequiresNotNull(encoding, "encoding");
			return new MutableString(new char[capacity], 0, encoding);
		}

		public static MutableString CreateMutable(string str, RubyEncoding encoding)
		{
			ContractUtils.RequiresNotNull(str, "str");
			ContractUtils.RequiresNotNull(encoding, "encoding");
			return new MutableString(str, encoding);
		}

		public static MutableString CreateAscii(string str)
		{
			ContractUtils.RequiresNotNull(str, "str");
			MutableString mutableString = Create(str, RubyEncoding.Ascii);
			mutableString._flags = 40u;
			return mutableString;
		}

		public static MutableString Create(string str)
		{
			ContractUtils.RequiresNotNull(str, "str");
			if (!str.IsAscii())
			{
				return Create(str, RubyEncoding.UTF8);
			}
			return CreateAscii(str);
		}

		public static MutableString Create(string str, RubyEncoding encoding)
		{
			ContractUtils.RequiresNotNull(str, "str");
			ContractUtils.RequiresNotNull(encoding, "encoding");
			return new MutableString(str, encoding);
		}

		public static MutableString CreateBinary()
		{
			return new MutableString(Utils.EmptyBytes, 0, RubyEncoding.Binary);
		}

		public static MutableString CreateBinary(RubyEncoding encoding)
		{
			return new MutableString(Utils.EmptyBytes, 0, encoding);
		}

		public static MutableString CreateBinary(int capacity)
		{
			return CreateBinary(capacity, RubyEncoding.Binary);
		}

		public static MutableString CreateBinary(int capacity, RubyEncoding encoding)
		{
			ContractUtils.Requires(capacity >= 0, "Capacity must be greater or equal to zero.");
			ContractUtils.RequiresNotNull(encoding, "encoding");
			return new MutableString(new byte[capacity], 0, encoding);
		}

		public static MutableString CreateBinary(byte[] bytes)
		{
			return CreateBinary(bytes, RubyEncoding.Binary);
		}

		public static MutableString CreateBinary(byte[] bytes, RubyEncoding encoding)
		{
			ContractUtils.RequiresNotNull(bytes, "bytes");
			ContractUtils.RequiresNotNull(encoding, "encoding");
			return new MutableString(ArrayUtils.Copy(bytes), encoding);
		}

		public static MutableString CreateBinary(List<byte> bytes, RubyEncoding encoding)
		{
			ContractUtils.RequiresNotNull(bytes, "bytes");
			ContractUtils.RequiresNotNull(encoding, "encoding");
			return new MutableString(bytes.ToArray(), encoding);
		}

		public static MutableString Create(MutableString str)
		{
			ContractUtils.RequiresNotNull(str, "str");
			return new MutableString(str);
		}

		internal static MutableString CreateInternal(MutableString str, RubyEncoding encoding)
		{
			if (str != null)
			{
				return new MutableString(str);
			}
			return CreateMutable(string.Empty, encoding);
		}

		public virtual MutableString CreateInstance()
		{
			return new MutableString(_encoding);
		}

		internal virtual MutableString CreateInstance(Content content, RubyEncoding encoding)
		{
			return new MutableString(content, encoding);
		}

		public static MutableString CreateEmpty()
		{
			return Create(string.Empty, RubyEncoding.Binary);
		}

		public virtual MutableString Clone()
		{
			return new MutableString(this);
		}

		public MutableString Duplicate(RubyContext context, bool copySingletonMembers, MutableString result)
		{
			context.CopyInstanceData(this, result, copySingletonMembers);
			return result;
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			return Duplicate(context, copySingletonMembers, CreateInstance());
		}

		public static MutableString[] MakeArray(ICollection<string> stringCollection, RubyEncoding encoding)
		{
			ContractUtils.RequiresNotNull(stringCollection, "stringCollection");
			ContractUtils.RequiresNotNull(encoding, "encoding");
			MutableString[] array = new MutableString[stringCollection.Count];
			int num = 0;
			foreach (string item in stringCollection)
			{
				array[num++] = Create(item, encoding);
			}
			return array;
		}

		public bool DetectByteCharacters()
		{
			if (_encoding != RubyEncoding.Binary)
			{
				return IsAscii();
			}
			return true;
		}

		public bool DetectSingleByteCharacters()
		{
			if (!_encoding.IsSingleByteCharacterSet)
			{
				return IsAscii();
			}
			return true;
		}

		private void FrozenOrCopyOnWrite(uint flags)
		{
			if ((flags & (true ? 1u : 0u)) != 0)
			{
				throw RubyExceptions.CreateObjectFrozenError();
			}
			_content = _content.Clone();
			_flags = flags & 0xFFFFFBFFu;
		}

		private void MutateContent(uint setFlags)
		{
			uint flags = _flags;
			if ((flags & 0x401u) != 0)
			{
				FrozenOrCopyOnWrite(flags);
			}
			_flags = flags | setFlags;
		}

		private void Mutate()
		{
			MutateContent(86u);
		}

		private void MutateOne(char c)
		{
			uint num = _flags;
			if ((num & 0x401u) != 0)
			{
				FrozenOrCopyOnWrite(num);
			}
			if (c >= '\u0080')
			{
				num = ((!Tokenizer.IsSurrogate(c)) ? (num & 0xFFFFFFE7u) : (num & 0xFFFFFF87u));
			}
			_flags = num | 6u;
		}

		private void MutateOne(byte b)
		{
			uint flags = _flags;
			if ((flags & 0x401u) != 0)
			{
				FrozenOrCopyOnWrite(flags);
			}
			flags = ((b < 128) ? (flags | 0x10u) : (flags & 0xFFFFFFE7u));
			_flags = flags | 0x40u | 6u;
		}

		private void MutatePreserveAsciiness()
		{
			MutateContent(6u);
		}

		private void MutateRemove()
		{
			MutateContent((((_flags & 8) == 0) ? 16u : 0u) | (((_flags & 0x20) == 0) ? 64u : 0u) | 6u);
		}

		private void Mutate(MutableString other)
		{
			RubyEncoding encoding = RequireCompatibleEncoding(other);
			Mutate();
			SetEncoding(encoding);
		}

		public RubyEncoding GetCompatibleEncoding(MutableString other)
		{
			RubyEncoding rubyEncoding = GetCompatibleEncoding(other.Encoding);
			if (rubyEncoding == null)
			{
				if (!other.IsAscii())
				{
					return null;
				}
				rubyEncoding = _encoding;
			}
			return rubyEncoding;
		}

		public RubyEncoding GetCompatibleEncoding(RubyEncoding encoding)
		{
			RubyEncoding rubyEncoding = GetCompatibleEncoding(_encoding, encoding);
			if (rubyEncoding == null)
			{
				if (!IsAscii())
				{
					return null;
				}
				rubyEncoding = encoding;
			}
			return rubyEncoding;
		}

		public static RubyEncoding GetCompatibleEncoding(RubyEncoding encoding1, RubyEncoding encoding2)
		{
			if (encoding1 == encoding2)
			{
				return encoding1;
			}
			if (encoding1 == RubyEncoding.Ascii)
			{
				return encoding2;
			}
			if (encoding2 == RubyEncoding.Ascii)
			{
				return encoding1;
			}
			return null;
		}

		public RubyEncoding RequireCompatibleEncoding(MutableString other)
		{
			RubyEncoding compatibleEncoding = GetCompatibleEncoding(other);
			if (compatibleEncoding == null)
			{
				throw RubyExceptions.CreateEncodingCompatibilityError(_encoding, other.Encoding);
			}
			return compatibleEncoding;
		}

		public void ForceEncoding(RubyEncoding newEncoding)
		{
			ContractUtils.RequiresNotNull(newEncoding, "newEncoding");
			if (_encoding == newEncoding)
			{
				return;
			}
			if (IsBinary)
			{
				SetEncoding(newEncoding);
				return;
			}
			bool flag = IsAscii();
			Mutate();
			if (flag)
			{
				SetEncoding(newEncoding);
				return;
			}
			SwitchToBytes();
			SetEncoding(newEncoding);
		}

		public void Transcode(RubyEncoding fromEncoding, RubyEncoding toEncoding)
		{
			if (fromEncoding == toEncoding && _encoding == fromEncoding)
			{
				return;
			}
			bool flag = IsAscii();
			Mutate();
			if (flag)
			{
				SetEncoding(toEncoding);
				return;
			}
			bool flag2;
			if (IsBinary)
			{
				if (fromEncoding != _encoding)
				{
					SetEncoding(fromEncoding);
				}
				flag2 = true;
			}
			else if (fromEncoding != _encoding)
			{
				try
				{
					_content = _content.SwitchToBinaryContent();
				}
				catch (EncoderFallbackException e)
				{
					throw RubyExceptions.CreateInvalidByteSequenceError(e, _encoding);
				}
				SetEncoding(fromEncoding);
				flag2 = true;
			}
			else
			{
				flag2 = false;
			}
			if (flag2)
			{
				try
				{
					_content = _content.SwitchToStringContent();
				}
				catch (DecoderFallbackException e2)
				{
					throw RubyExceptions.CreateInvalidByteSequenceError(e2, fromEncoding);
				}
			}
			SetEncoding(toEncoding);
			try
			{
				_content.CheckEncoding();
			}
			catch (EncoderFallbackException e3)
			{
				throw RubyExceptions.CreateTranscodingError(e3, fromEncoding, toEncoding);
			}
		}

		public override int GetHashCode()
		{
			return _content.CalculateHashCode();
		}

		public bool IsAscii()
		{
			uint num = _flags;
			if ((num & 0x10u) != 0)
			{
				num = (_flags = ((!_encoding.IsAsciiIdentity) ? (num & 0xFFFFFFE7u) : _content.UpdateCharacterFlags(_flags)));
			}
			return (num & 8) != 0;
		}

		public bool HasSurrogates()
		{
			uint num = _flags;
			if ((num & 0x40u) != 0)
			{
				num = (_flags = ((!_encoding.InUnicodeBasicPlane) ? _content.UpdateCharacterFlags(_flags) : ((num & 0xFFFFFFBFu) | 0x20u)));
			}
			return (num & 0x20) == 0;
		}

		public MutableString CheckEncoding()
		{
			_content.CheckEncoding();
			return this;
		}

		public bool ContainsInvalidCharacters()
		{
			return _content.ContainsInvalidCharacters();
		}

		internal void ClearFlag(uint flag)
		{
			_flags &= ~flag;
		}

		internal bool IsFlagSet(uint flag)
		{
			return (_flags & flag) != 0;
		}

		public void TrackChanges()
		{
			_flags &= 4294967293u;
		}

		void IRubyObjectState.Freeze()
		{
			Freeze();
		}

		public MutableString Freeze()
		{
			_flags |= 1u;
			return this;
		}

		public void RequireNotFrozen()
		{
			if (IsFrozen)
			{
				throw RubyExceptions.CreateObjectFrozenError();
			}
		}

		public MutableString TaintBy(MutableString str)
		{
			IsTainted |= str.IsTainted;
			IsUntrusted |= str.IsUntrusted;
			return this;
		}

		public MutableString TaintBy(IRubyObjectState obj)
		{
			IsTainted |= obj.IsTainted;
			IsUntrusted |= obj.IsUntrusted;
			return this;
		}

		public MutableString TaintBy(object obj, RubyContext context)
		{
			bool tainted;
			bool untrusted;
			context.GetObjectTrust(obj, out tainted, out untrusted);
			IsTainted |= tainted;
			IsUntrusted |= untrusted;
			return this;
		}

		public MutableString TaintBy(object obj, RubyScope scope)
		{
			return TaintBy(obj, scope.RubyContext);
		}

		internal MutableString EscapeRegularExpression()
		{
			return CreateInstance(_content.EscapeRegularExpression(), _encoding);
		}

		public override string ToString()
		{
			return _content.ToString();
		}

		public string ToString(Encoding encoding)
		{
			int count;
			byte[] byteArray = _content.GetByteArray(out count);
			return encoding.GetString(byteArray, 0, count);
		}

		public string ToString(Encoding encoding, int start, int count)
		{
			byte[] byteArrayChecked = GetByteArrayChecked(start, count);
			return encoding.GetString(byteArrayChecked, start, count);
		}

		public byte[] ToByteArray()
		{
			return _content.ToByteArray();
		}

		public string ConvertToString()
		{
			return _content.ConvertToString();
		}

		public byte[] ConvertToBytes()
		{
			return _content.ConvertToBytes();
		}

		public MutableString SwitchToBytes()
		{
			try
			{
				_content = _content.SwitchToBinaryContent();
				return this;
			}
			catch (EncoderFallbackException e)
			{
				throw RubyExceptions.CreateInvalidByteSequenceError(e, _encoding);
			}
		}

		public MutableString SwitchToCharacters()
		{
			try
			{
				_content = _content.SwitchToStringContent();
				return this;
			}
			catch (DecoderFallbackException e)
			{
				throw RubyExceptions.CreateInvalidByteSequenceError(e, _encoding);
			}
		}

		public MutableString PrepareForCharacterRead()
		{
			if (IsBinary && !DetectByteCharacters())
			{
				SwitchToCharacters();
			}
			return this;
		}

		public MutableString PrepareForCharacterWrite()
		{
			if (IsBinary)
			{
				SwitchToCharacters();
			}
			else
			{
				_content.SwitchToMutableContent();
			}
			return this;
		}

		public static explicit operator string(MutableString self)
		{
			return self._content.ConvertToString();
		}

		public static explicit operator byte[](MutableString self)
		{
			return self._content.ConvertToBytes();
		}

		public static explicit operator char(MutableString self)
		{
			try
			{
				return self.GetChar(0);
			}
			catch (IndexOutOfRangeException)
			{
				throw RubyExceptions.CreateTypeConversionError("String", "System::Char");
			}
		}

		public override bool Equals(object other)
		{
			MutableString mutableString = other as MutableString;
			if (mutableString != null)
			{
				return Equals(mutableString);
			}
			return Equals(other as string);
		}

		public bool Equals(MutableString other)
		{
			if (object.ReferenceEquals(other, null))
			{
				return false;
			}
			if (KnowsAscii && other.KnowsAscii && IsAscii() != other.IsAscii())
			{
				return false;
			}
			return CompareTo(other) == 0;
		}

		public bool Equals(string other)
		{
			return CompareTo(other) == 0;
		}

		public int CompareTo(object other)
		{
			MutableString mutableString = other as MutableString;
			if (mutableString != null)
			{
				return CompareTo(mutableString);
			}
			return CompareTo(other as string);
		}

		public int CompareTo(MutableString other)
		{
			if (object.ReferenceEquals(this, other))
			{
				return 0;
			}
			if (object.ReferenceEquals(other, null))
			{
				return 1;
			}
			if (_encoding != other._encoding)
			{
				bool flag = true;
				if (!IsAscii())
				{
					SwitchToBytes();
					flag = false;
				}
				if (!other.IsAscii())
				{
					other.SwitchToBytes();
					flag = false;
				}
				int num = _content.OrdinalCompareTo(other._content);
				if (flag || num != 0)
				{
					return num;
				}
				return _encoding.CompareTo(other._encoding);
			}
			return _content.OrdinalCompareTo(other._content);
		}

		public int CompareTo(string other)
		{
			if (object.ReferenceEquals(other, null))
			{
				return 1;
			}
			return _content.OrdinalCompareTo(other);
		}

		public static bool IsNullOrEmpty(MutableString str)
		{
			if (!object.ReferenceEquals(str, null))
			{
				return str.IsEmpty;
			}
			return true;
		}

		public int GetLength()
		{
			return _content.Count;
		}

		public void SetLength(int value)
		{
			ContractUtils.Requires(value >= 0, "value");
			if (value < _content.Count)
			{
				_content.Remove(value, _content.Count - value);
			}
			else
			{
				_content.Count = value;
			}
		}

		public int GetCharCount()
		{
			return _content.GetCharCount();
		}

		public int GetCharacterCount()
		{
			return _content.GetCharacterCount();
		}

		public void SetCharCount(int value)
		{
			PrepareForCharacterRead().SetLength(value);
		}

		public int GetByteCount()
		{
			return _content.GetByteCount();
		}

		public void SetByteCount(int value)
		{
			SwitchToBytes().SetLength(value);
		}

		public MutableString TrimExcess()
		{
			_content.TrimExcess();
			return this;
		}

		public void EnsureCapacity(int minCapacity)
		{
			if (_content.GetCapacity() < minCapacity)
			{
				_content.SetCapacity(minCapacity);
			}
		}

		public bool StartsWith(char value)
		{
			return _content.StartsWith(value);
		}

		public bool EndsWith(char value)
		{
			return GetLastChar() == value;
		}

		public bool EndsWith(string value)
		{
			return _content.ConvertToString().EndsWith(value, StringComparison.Ordinal);
		}

		public bool EndsWith(MutableString value)
		{
			ContractUtils.RequiresNotNull(value, "value");
			if (IsBinary || value.IsBinary)
			{
				int byteCount = value.GetByteCount();
				int num = GetByteCount() - byteCount;
				if (num < 0)
				{
					return false;
				}
				for (int i = 0; i < byteCount; i++)
				{
					if (GetByte(num + i) != value.GetByte(i))
					{
						return false;
					}
				}
			}
			else
			{
				int charCount = value.GetCharCount();
				int num2 = GetCharCount() - charCount;
				if (num2 < 0)
				{
					return false;
				}
				for (int j = 0; j < charCount; j++)
				{
					if (GetChar(num2 + j) != value.GetChar(j))
					{
						return false;
					}
				}
			}
			return true;
		}

		internal static CharacterEnumerator EnumerateAsCharacters(byte[] data, int count, RubyEncoding encoding, out char[] allValid)
		{
			Decoder decoder = encoding.Encoding.GetDecoder();
			LosslessDecoderFallback losslessDecoderFallback = (LosslessDecoderFallback)(decoder.Fallback = new LosslessDecoderFallback());
			losslessDecoderFallback.Track = true;
			char[] array = new char[decoder.GetCharCount(data, 0, count, true)];
			decoder.Reset();
			losslessDecoderFallback.Track = false;
			decoder.GetChars(data, 0, count, array, 0, true);
			allValid = ((losslessDecoderFallback.InvalidCharacters == null) ? array : null);
			return new CompositeCharacterEnumerator(encoding, array, array.Length, losslessDecoderFallback.InvalidCharacters);
		}

		public CharacterEnumerator GetCharacters()
		{
			_flags |= 1024u;
			return _content.GetCharacters();
		}

		public IEnumerable<byte> GetBytes()
		{
			_flags |= 1024u;
			return _content.GetBytes();
		}

		public char GetChar(int index)
		{
			return _content.GetChar(index);
		}

		public byte GetByte(int index)
		{
			return _content.GetByte(index);
		}

		public int GetLastChar()
		{
			if (!_content.IsEmpty)
			{
				return _content.GetChar(_content.GetCharCount() - 1);
			}
			return -1;
		}

		public int GetFirstChar()
		{
			if (!_content.IsEmpty)
			{
				return _content.GetChar(0);
			}
			return -1;
		}

		public MutableString GetSlice(int start)
		{
			return GetSlice(start, int.MaxValue);
		}

		public MutableString GetSlice(int start, int count)
		{
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			return CreateInstance(_content.GetSlice(start, count), _encoding);
		}

		public string GetStringSlice(int start)
		{
			return GetStringSlice(start, int.MaxValue);
		}

		public string GetStringSlice(int start, int count)
		{
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			return _content.GetStringSlice(start, count);
		}

		public byte[] GetBinarySlice(int start)
		{
			return GetBinarySlice(start, int.MaxValue);
		}

		public byte[] GetBinarySlice(int start, int count)
		{
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			return _content.GetBinarySlice(start, count);
		}

		public MutableString[] Split(char[] separators, int maxComponents, StringSplitOptions options)
		{
			return MakeArray(StringUtils.Split(_content.ConvertToString(), separators, maxComponents, options), _encoding);
		}

		public int IndexOf(char value)
		{
			return IndexOf(value, 0);
		}

		public int IndexOf(char value, int start)
		{
			return IndexOf(value, start, int.MaxValue);
		}

		public int IndexOf(char value, int start, int count)
		{
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			return _content.IndexOf(value, start, count);
		}

		public int IndexOf(byte value)
		{
			return IndexOf(value, 0);
		}

		public int IndexOf(byte value, int start)
		{
			return IndexOf(value, start, int.MaxValue);
		}

		public int IndexOf(byte value, int start, int count)
		{
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			return _content.IndexOf(value, start, count);
		}

		public int IndexOf(string value)
		{
			return IndexOf(value, 0);
		}

		public int IndexOf(string value, int start)
		{
			return IndexOf(value, start, int.MaxValue);
		}

		public int IndexOf(string value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			return _content.IndexOf(value, start, count);
		}

		public int IndexOf(byte[] value)
		{
			return IndexOf(value, 0);
		}

		public int IndexOf(byte[] value, int start)
		{
			return IndexOf(value, start, int.MaxValue);
		}

		public int IndexOf(byte[] value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			return _content.IndexOf(value, start, count);
		}

		public int IndexOf(MutableString value)
		{
			return IndexOf(value, 0);
		}

		public int IndexOf(MutableString value, int start)
		{
			return IndexOf(value, start, int.MaxValue);
		}

		public int IndexOf(MutableString value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			return value._content.IndexIn(_content, start, count);
		}

		public int LastIndexOf(char value)
		{
			return LastIndexOf(value, 2147483646, int.MaxValue);
		}

		public int LastIndexOf(char value, int start)
		{
			return LastIndexOf(value, start, start + 1);
		}

		public int LastIndexOf(char value, int start, int count)
		{
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0 && count - 1 <= start, "count");
			return _content.LastIndexOf(value, start, count);
		}

		public int LastIndexOf(byte value)
		{
			return LastIndexOf(value, 2147483646, int.MaxValue);
		}

		public int LastIndexOf(byte value, int start)
		{
			return LastIndexOf(value, start, start + 1);
		}

		public int LastIndexOf(byte value, int start, int count)
		{
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0 && count - 1 <= start, "count");
			return _content.LastIndexOf(value, start, count);
		}

		public int LastIndexOf(string value)
		{
			return LastIndexOf(value, 2147483646, int.MaxValue);
		}

		public int LastIndexOf(string value, int start)
		{
			return LastIndexOf(value, start, start + 1);
		}

		public int LastIndexOf(string value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0 && count - 1 <= start, "count");
			return _content.LastIndexOf(value, start, count);
		}

		public int LastIndexOf(byte[] value)
		{
			return LastIndexOf(value, 2147483646, int.MaxValue);
		}

		public int LastIndexOf(byte[] value, int start)
		{
			return LastIndexOf(value, start, start + 1);
		}

		public int LastIndexOf(byte[] value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0 && count - 1 <= start, "count");
			return _content.LastIndexOf(value, start, count);
		}

		public int LastIndexOf(MutableString value)
		{
			return LastIndexOf(value, 2147483646, int.MaxValue);
		}

		public int LastIndexOf(MutableString value, int start)
		{
			return LastIndexOf(value, start, start + 1);
		}

		public int LastIndexOf(MutableString value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0 && count - 1 <= start, "count");
			return value._content.LastIndexIn(_content, start, count);
		}

		public MutableString Concat(MutableString other)
		{
			ContractUtils.RequiresNotNull(other, "other");
			RubyEncoding encoding = RequireCompatibleEncoding(other);
			return new MutableString(_content.Concat(other._content), encoding);
		}

		public MutableString Append(char value)
		{
			MutateOne(value);
			_content.Append(value, 1);
			return this;
		}

		public MutableString Append(char value, int repeatCount)
		{
			MutateOne(value);
			_content.Append(value, repeatCount);
			return this;
		}

		public MutableString Append(byte value)
		{
			MutateOne(value);
			_content.Append(value, 1);
			return this;
		}

		public MutableString Append(byte value, int repeatCount)
		{
			MutateOne(value);
			_content.Append(value, repeatCount);
			return this;
		}

		public MutableString Append(char[] value)
		{
			if (value != null)
			{
				Mutate();
				_content.Append(value, 0, value.Length);
			}
			return this;
		}

		public MutableString Append(char[] value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.RequiresArrayRange(value, start, count, "startIndex", "count");
			Mutate();
			_content.Append(value, start, count);
			return this;
		}

		public MutableString Append(string value)
		{
			if (value != null)
			{
				Mutate();
				_content.Append(value, 0, value.Length);
			}
			return this;
		}

		public MutableString Append(string value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.RequiresArrayRange(value, start, count, "start", "count");
			Mutate();
			_content.Append(value, start, count);
			return this;
		}

		public MutableString Append(byte[] value)
		{
			if (value != null)
			{
				Mutate();
				_content.Append(value, 0, value.Length);
			}
			return this;
		}

		public MutableString Append(byte[] value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.RequiresArrayRange(value, start, count, "start", "count");
			Mutate();
			_content.Append(value, start, count);
			return this;
		}

		public MutableString Append(Stream stream, int count)
		{
			ContractUtils.RequiresNotNull(stream, "stream");
			ContractUtils.Requires(count >= 0, "count");
			Mutate();
			_content.Append(stream, count);
			return this;
		}

		public MutableString Append(MutableString value)
		{
			if (value != null)
			{
				Mutate(value);
				_content.Append(value._content, 0, value._content.Count);
			}
			return this;
		}

		public MutableString Append(MutableString value, int start)
		{
			return Append(value, start, value._content.Count - start);
		}

		public MutableString Append(MutableString value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			Mutate(value);
			_content.Append(value._content, start, count);
			return this;
		}

		public MutableString AppendMultiple(MutableString value, int repeatCount)
		{
			ContractUtils.RequiresNotNull(value, "value");
			Mutate(value);
			Content content = value._content;
			EnsureCapacity(content.Count * repeatCount);
			while (repeatCount-- > 0)
			{
				_content.Append(content, 0, content.Count);
			}
			return this;
		}

		public MutableString AppendFormat(string format, params object[] args)
		{
			ContractUtils.RequiresNotNull(format, "format");
			Mutate();
			_content.AppendFormat(CultureInfo.InvariantCulture, format, args);
			return this;
		}

		public MutableString Append(Character character)
		{
			if (character.IsValid)
			{
				Append(character.Value);
				if (character.IsSurrogate)
				{
					Append(character.LowSurrogate);
				}
				return this;
			}
			return Append(character.Invalid);
		}

		public MutableString AppendRemaining(CharacterEnumerator characters)
		{
			ContractUtils.RequiresNotNull(characters, "characters");
			characters.AppendTo(this);
			return this;
		}

		public void SetChar(int index, char value)
		{
			MutateOne(value);
			_content.SetChar(index, value);
		}

		public void SetByte(int index, byte value)
		{
			MutateOne(value);
			_content.SetByte(index, value);
		}

		public MutableString Insert(int index, char value)
		{
			MutateOne(value);
			_content.Insert(index, value);
			return this;
		}

		public MutableString Insert(int index, byte value)
		{
			MutateOne(value);
			_content.Insert(index, value);
			return this;
		}

		public MutableString Insert(int index, string value)
		{
			if (value != null)
			{
				Mutate();
				_content.Insert(index, value, 0, value.Length);
			}
			return this;
		}

		public MutableString Insert(int index, string value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.RequiresArrayRange(value, start, count, "start", "count");
			Mutate();
			_content.Insert(index, value, start, count);
			return this;
		}

		public MutableString Insert(int index, byte[] value)
		{
			if (value != null)
			{
				Mutate();
				_content.Insert(index, value, 0, value.Length);
			}
			return this;
		}

		public MutableString Insert(int index, byte[] value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.RequiresArrayRange(value, start, count, "start", "count");
			Mutate();
			_content.Insert(index, value, start, count);
			return this;
		}

		public MutableString Insert(int index, MutableString value)
		{
			if (value != null)
			{
				Mutate(value);
				value._content.InsertTo(_content, index, 0, value._content.Count);
			}
			return this;
		}

		public MutableString Insert(int index, MutableString value, int start, int count)
		{
			ContractUtils.RequiresNotNull(value, "value");
			Mutate(value);
			value._content.InsertTo(_content, index, start, count);
			return this;
		}

		public MutableString Reverse()
		{
			MutatePreserveAsciiness();
			PrepareForCharacterWrite();
			Content content = _content;
			int count = content.Count;
			if (count <= 1)
			{
				return this;
			}
			for (int i = 0; i < count / 2; i++)
			{
				char @char = content.GetChar(i);
				char char2 = content.GetChar(count - i - 1);
				content.SetChar(i, char2);
				content.SetChar(count - i - 1, @char);
			}
			return this;
		}

		public MutableString Replace(int start, int count, MutableString value)
		{
			Mutate(value);
			return Remove(start, count).Insert(start, value);
		}

		public MutableString WriteBytes(int offset, MutableString value, int start, int count)
		{
			byte[] byteArrayChecked = value.GetByteArrayChecked(start, count);
			return Write(offset, byteArrayChecked, start, count);
		}

		public MutableString Write(int offset, byte[] value, int start, int count)
		{
			Mutate();
			_content.Write(offset, value, start, count);
			return this;
		}

		public MutableString Write(int offset, byte value, int repeatCount)
		{
			Mutate();
			_content.Write(offset, value, repeatCount);
			return this;
		}

		public MutableString Remove(int start)
		{
			ContractUtils.Requires(start >= 0, "start");
			MutateRemove();
			_content.Remove(start, _content.Count - start);
			return this;
		}

		public MutableString Remove(int start, int count)
		{
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			MutateRemove();
			_content.Remove(start, count);
			return this;
		}

		public MutableString Trim(int start, int count)
		{
			ContractUtils.Requires(start >= 0, "start");
			ContractUtils.Requires(count >= 0, "count");
			MutateRemove();
			_content = _content.GetSlice(start, count);
			return this;
		}

		public MutableString Clear()
		{
			Mutate();
			_content = _content.GetSlice(0, 0);
			return this;
		}

		private static void PrepareTranslation(MutableString src, MutableString dst, CharacterMap map)
		{
			ContractUtils.RequiresNotNull(src, "src");
			ContractUtils.RequiresNotNull(dst, "dst");
			ContractUtils.RequiresNotNull(map, "map");
			ContractUtils.Requires(object.ReferenceEquals(src, dst) || dst.IsEmpty);
			dst.Mutate();
			dst.PrepareForCharacterWrite();
			if (!object.ReferenceEquals(src, dst))
			{
				src.PrepareForCharacterRead();
				dst.SetLength(src.GetLength());
			}
		}

		public static bool Translate(MutableString src, MutableString dst, CharacterMap map)
		{
			PrepareTranslation(src, dst, map);
			ContractUtils.Requires(map.HasFullMap, "map");
			int charCount = src.GetCharCount();
			Content content = dst._content;
			Content content2 = src._content;
			bool result = false;
			bool flag = object.ReferenceEquals(src, dst);
			for (int i = 0; i < charCount; i++)
			{
				char @char = content2.GetChar(i);
				int num = map.TryMap(@char);
				if (num >= 0)
				{
					result = true;
					content.SetChar(i, (char)num);
				}
				else if (!flag)
				{
					content.SetChar(i, @char);
				}
			}
			return result;
		}

		public static bool TranslateSqueeze(MutableString src, MutableString dst, CharacterMap map)
		{
			PrepareTranslation(src, dst, map);
			ContractUtils.Requires(map.HasFullMap, "map");
			int charCount = src.GetCharCount();
			Content content = dst._content;
			Content content2 = src._content;
			bool result = false;
			int num = 0;
			int num2 = -1;
			for (int i = 0; i < charCount; i++)
			{
				char @char = content2.GetChar(i);
				int num3 = map.TryMap(@char);
				if (num3 >= 0)
				{
					result = true;
					if (num3 != num2)
					{
						content.SetChar(num++, (char)num3);
					}
				}
				else
				{
					content.SetChar(num++, @char);
				}
				num2 = num3;
			}
			if (num < charCount)
			{
				dst.Remove(num);
			}
			return result;
		}

		public static bool TranslateRemove(MutableString src, MutableString dst, CharacterMap map)
		{
			PrepareTranslation(src, dst, map);
			ContractUtils.Requires(map.HasBitmap, "map");
			Content content = dst._content;
			Content content2 = src._content;
			int charCount = src.GetCharCount();
			bool flag = !map.IsComplemental;
			bool result = false;
			int num = 0;
			for (int i = 0; i < charCount; i++)
			{
				char @char = content2.GetChar(i);
				if (map.IsMapped(@char) == flag)
				{
					result = true;
				}
				else
				{
					content.SetChar(num++, @char);
				}
			}
			if (num < charCount)
			{
				dst.Remove(num);
			}
			return result;
		}

		private static string ToStringWithEscapedInvalidCharacters(byte[] bytes, Encoding encoding, bool octalEscapes, out int escapePlaceholder)
		{
			Decoder decoder = encoding.GetDecoder();
			decoder.Fallback = new DumpDecoderFallback(octalEscapes);
			char[] array = new char[decoder.GetCharCount(bytes, 0, bytes.Length, true)];
			decoder.GetChars(bytes, 0, bytes.Length, array, 0, true);
			escapePlaceholder = (((DumpDecoderFallback.Buffer)decoder.FallbackBuffer).HasInvalidCharacters ? 65535 : (-1));
			return new string(array);
		}

		private static void AppendBinaryCharRepresentation(StringBuilder result, int currentChar, int nextChar, Escape escape, int quote)
		{
			switch (currentChar)
			{
			case 7:
				result.Append("\\a");
				break;
			case 8:
				result.Append("\\b");
				break;
			case 9:
				result.Append("\\t");
				break;
			case 10:
				result.Append("\\n");
				break;
			case 11:
				result.Append("\\v");
				break;
			case 12:
				result.Append("\\f");
				break;
			case 13:
				result.Append("\\r");
				break;
			case 27:
				result.Append("\\e");
				break;
			case 92:
				if ((escape & Escape.Special) != 0)
				{
					result.Append("\\\\");
				}
				else
				{
					result.Append('\\');
				}
				break;
			case 35:
				if ((escape & Escape.Special) != 0)
				{
					if (nextChar == 36 || nextChar == 64 || nextChar == 123)
					{
						result.Append('\\');
					}
				}
				result.Append('#');
				break;
			default:
				if (currentChar == quote)
				{
					result.Append('\\');
					result.Append((char)quote);
				}
				else if (currentChar < 32 || (currentChar >= 128 && (escape & Escape.NonAscii) != 0))
				{
					AppendHexEscape(result, currentChar);
				}
				else
				{
					result.Append((char)currentChar);
				}
				break;
			}
		}

		public static int AppendUnicodeCharRepresentation(StringBuilder result, int currentChar, int nextChar, Escape escape, int quote, int escapePlaceholder)
		{
			int result2 = 1;
			if (currentChar == escapePlaceholder)
			{
				result.Append('\\');
			}
			else if (currentChar < 128)
			{
				AppendBinaryCharRepresentation(result, currentChar, nextChar, escape, quote);
			}
			else if ((escape & Escape.NonAscii) != 0)
			{
				if (nextChar != -1 && char.IsSurrogatePair((char)currentChar, (char)nextChar))
				{
					currentChar = Tokenizer.ToCodePoint(currentChar, nextChar);
					result2 = 2;
				}
				result.Append("\\u{");
				result.Append(Convert.ToString(currentChar, 16));
				result.Append('}');
			}
			else if (nextChar != -1 && char.IsSurrogatePair((char)currentChar, (char)nextChar))
			{
				result.Append((char)currentChar);
				result.Append((char)nextChar);
				result2 = 2;
			}
			else if (char.IsSurrogate((char)currentChar))
			{
				result.Append("\\u{");
				result.Append(Convert.ToString(currentChar, 16));
				result.Append('}');
			}
			else
			{
				result.Append((char)currentChar);
			}
			return result2;
		}

		public static void AppendCharRepresentation(StringBuilder result, int currentChar, int nextChar, Escape escape, int quote, int escapePlaceholder)
		{
			if (currentChar == escapePlaceholder)
			{
				result.Append('\\');
			}
			else if (currentChar < 256)
			{
				AppendBinaryCharRepresentation(result, currentChar, nextChar, escape, quote);
			}
			else
			{
				result.Append((char)currentChar);
			}
		}

		private static void AppendHexEscape(StringBuilder result, int c)
		{
			result.Append("\\x");
			result.Append((c >> 4).ToUpperHexDigit());
			result.Append((c & 0xF).ToUpperHexDigit());
		}

		private string ToStringWithEscapedInvalidCharacters(RubyEncoding encoding, bool octalEscapes, out int escapePlaceholder)
		{
			if (IsBinary || encoding != _encoding)
			{
				return ToStringWithEscapedInvalidCharacters(ToByteArray(), encoding.Encoding, octalEscapes, out escapePlaceholder);
			}
			escapePlaceholder = -1;
			return ToString();
		}

		public string ToAsciiString()
		{
			return AppendRepresentation(new StringBuilder(), null, Escape.NonAscii, -1).ToString();
		}

		public string ToStringWithEscapedInvalidCharacters(RubyEncoding encoding)
		{
			ContractUtils.RequiresNotNull(encoding, "encoding");
			return AppendRepresentation(new StringBuilder(), encoding, Escape.Default, -1).ToString();
		}

		public StringBuilder AppendRepresentation(StringBuilder result, RubyEncoding forceEncoding, Escape escape, int quote)
		{
			ContractUtils.RequiresNotNull(result, "result");
			RubyEncoding rubyEncoding = forceEncoding ?? _encoding;
			if (rubyEncoding == RubyEncoding.Binary || ((escape & Escape.NonAscii) != 0 && rubyEncoding != RubyEncoding.UTF8))
			{
				escape |= Escape.NonAscii;
				if (IsBinary)
				{
					AppendBinaryRepresentation(result, ToByteArray(), escape, quote);
				}
				else
				{
					AppendStringRepresentation(result, ToString(), escape, quote, -1);
				}
			}
			else
			{
				int escapePlaceholder;
				string str = ToStringWithEscapedInvalidCharacters(rubyEncoding, (escape & Escape.Octal) != 0, out escapePlaceholder);
				if (rubyEncoding == RubyEncoding.UTF8)
				{
					AppendUnicodeRepresentation(result, str, escape, quote, escapePlaceholder);
				}
				else
				{
					AppendStringRepresentation(result, str, escape, quote, escapePlaceholder);
				}
			}
			return result;
		}

		public static StringBuilder AppendUnicodeRepresentation(StringBuilder result, string str, Escape escape, int quote, int escapePlaceholder)
		{
			for (int i = 0; i < str.Length; i += AppendUnicodeCharRepresentation(result, str[i], (i < str.Length - 1) ? str[i + 1] : (-1), escape, quote, escapePlaceholder))
			{
			}
			return result;
		}

		public static StringBuilder AppendStringRepresentation(StringBuilder result, string str, Escape escape, int quote, int escapePlaceholder)
		{
			for (int i = 0; i < str.Length; i++)
			{
				AppendCharRepresentation(result, str[i], (i < str.Length - 1) ? str[i + 1] : (-1), escape, quote, escapePlaceholder);
			}
			return result;
		}

		public static StringBuilder AppendBinaryRepresentation(StringBuilder result, byte[] bytes, Escape escape, int quote)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				AppendCharRepresentation(result, bytes[i], (i < bytes.Length - 1) ? bytes[i + 1] : (-1), escape, quote, -1);
			}
			return result;
		}

		internal string GetDebugValue()
		{
			return AppendRepresentation(new StringBuilder(), null, Escape.Default, 34).ToString();
		}

		internal string GetDebugType()
		{
			if (!IsBinary)
			{
				return "String (" + _encoding.ToString() + ")";
			}
			if (_encoding != RubyEncoding.Binary)
			{
				return "String (binary/" + _encoding.ToString() + ")";
			}
			return "String (binary)";
		}

		public static MutableString FormatMessage(string message, params MutableString[] args)
		{
			return Create(string.Format(message, args), RubyEncoding.UTF8);
		}

		internal byte[] GetByteArray(out int count)
		{
			return _content.GetByteArray(out count);
		}

		internal byte[] GetByteArrayChecked(int start, int count)
		{
			int count2;
			byte[] byteArray = _content.GetByteArray(out count2);
			if (count < 0 || start > count2 - count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			return byteArray;
		}
	}
}
