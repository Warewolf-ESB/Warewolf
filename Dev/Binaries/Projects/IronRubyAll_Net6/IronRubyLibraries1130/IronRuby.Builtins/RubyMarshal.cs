using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyModule("Marshal")]
	public class RubyMarshal
	{
		public sealed class WriterSites : RubyCallSiteStorage
		{
			private CallSite<Func<CallSite, object, object>> _marshalDump;

			private CallSite<Func<CallSite, object, int, object>> _dump;

			public CallSite<Func<CallSite, object, object>> MarshalDump
			{
				get
				{
					return RubyUtils.GetCallSite(ref _marshalDump, base.Context, "marshal_dump", 0);
				}
			}

			public CallSite<Func<CallSite, object, int, object>> Dump
			{
				get
				{
					return RubyUtils.GetCallSite(ref _dump, base.Context, "_dump", 1);
				}
			}

			public WriterSites(RubyContext context)
				: base(context)
			{
			}
		}

		public sealed class ReaderSites : RubyCallSiteStorage
		{
			private CallSite<Func<CallSite, object, object, object>> _marshalLoad;

			private CallSite<Func<CallSite, object, MutableString, object>> _load;

			public CallSite<Func<CallSite, Proc, object, object>> _procCall;

			public CallSite<Func<CallSite, object, object, object>> MarshalLoad
			{
				get
				{
					return RubyUtils.GetCallSite(ref _marshalLoad, base.Context, "marshal_load", 1);
				}
			}

			public CallSite<Func<CallSite, object, MutableString, object>> Load
			{
				get
				{
					return RubyUtils.GetCallSite(ref _load, base.Context, "_load", 1);
				}
			}

			public CallSite<Func<CallSite, Proc, object, object>> ProcCall
			{
				get
				{
					return RubyUtils.GetCallSite(ref _procCall, base.Context, "call", 1);
				}
			}

			public ReaderSites(RubyContext context)
				: base(context)
			{
			}
		}

		internal class MarshalWriter
		{
			private readonly BinaryWriter _writer;

			private readonly RubyContext _context;

			private readonly WriterSites _sites;

			private int _recursionLimit;

			private readonly Dictionary<string, int> _symbols;

			private readonly Dictionary<object, int> _objects;

			internal MarshalWriter(WriterSites sites, BinaryWriter writer, RubyContext context, int? limit)
			{
				_sites = sites;
				_writer = writer;
				_context = context;
				_recursionLimit = (limit.HasValue ? limit.Value : (-1));
				_symbols = new Dictionary<string, int>();
				_objects = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Instance);
			}

			private void WritePreamble()
			{
				_writer.Write((byte)4);
				_writer.Write((byte)8);
			}

			private void WriteBignumValue(BigInteger value)
			{
				char c = ((value.Sign > 0) ? '+' : ((value.Sign >= 0) ? '0' : '-'));
				_writer.Write((byte)c);
				uint[] words = value.GetWords();
				int num = words.Length * 2;
				int num2 = words.Length - 1;
				bool flag = false;
				if (words.Length > 0 && words[num2] >> 16 == 0)
				{
					num--;
					flag = true;
				}
				WriteInt32(num);
				for (int i = 0; i < words.Length; i++)
				{
					if (flag && i == num2)
					{
						_writer.Write((ushort)words[i]);
					}
					else
					{
						_writer.Write(words[i]);
					}
				}
			}

			private void WriteBignum(BigInteger value)
			{
				_writer.Write((byte)108);
				WriteBignumValue(value);
			}

			private void WriteInt32(int value)
			{
				if (value == 0)
				{
					_writer.Write((byte)0);
					return;
				}
				if (value > 0 && value < 123)
				{
					_writer.Write((byte)(value + 5));
					return;
				}
				if (value < 0 && value > -124)
				{
					_writer.Write((sbyte)(value - 5));
					return;
				}
				byte[] array = new byte[5]
				{
					0,
					(byte)((uint)value & 0xFFu),
					(byte)((uint)(value >> 8) & 0xFFu),
					(byte)((uint)(value >> 16) & 0xFFu),
					(byte)((uint)(value >> 24) & 0xFFu)
				};
				int num = 4;
				sbyte b;
				if (value < 0)
				{
					while (array[num] == byte.MaxValue)
					{
						num--;
					}
					b = (sbyte)(-num);
				}
				else
				{
					while (array[num] == 0)
					{
						num--;
					}
					b = (sbyte)num;
				}
				array[0] = (byte)b;
				_writer.Write(array, 0, num + 1);
			}

			private void WriteFixnum(int value)
			{
				_writer.Write((byte)105);
				WriteInt32(value);
			}

			private void WriteFloat(double value)
			{
				_writer.Write((byte)102);
				if (double.IsInfinity(value))
				{
					if (double.IsPositiveInfinity(value))
					{
						WriteStringValue(_positiveInfinityString);
					}
					else
					{
						WriteStringValue(_negativeInfinityString);
					}
				}
				else if (double.IsNaN(value))
				{
					WriteStringValue(_nanString);
				}
				else
				{
					StringFormatter stringFormatter = new StringFormatter(_context, "%.15g", RubyEncoding.Binary, new object[1] { value });
					stringFormatter.TrailingZeroAfterWholeFloat = false;
					WriteStringValue(stringFormatter.Format());
				}
			}

			private void WriteSubclassData(object obj, Type type)
			{
				RubyClass @class = _context.GetClass(type);
				RubyClass classOf = _context.GetClassOf(obj);
				if (@class != classOf && !(obj is RubyStruct))
				{
					_writer.Write((byte)67);
					WriteModuleName(classOf);
				}
			}

			private void WriteStringValue(MutableString value)
			{
				byte[] array = value.ToByteArray();
				WriteInt32(array.Length);
				_writer.Write(array);
			}

			private void WriteModuleName(RubyModule module)
			{
				WriteSymbol(module.Name, module.Context.GetIdentifierEncoding());
			}

			private void WriteStringValue(string value, RubyEncoding encoding)
			{
				byte[] bytes = encoding.StrictEncoding.GetBytes(value);
				WriteInt32(bytes.Length);
				_writer.Write(bytes);
			}

			private void WriteString(MutableString value)
			{
				WriteSubclassData(value, typeof(MutableString));
				_writer.Write((byte)34);
				WriteStringValue(value);
			}

			private void WriteRegex(RubyRegex value)
			{
				WriteSubclassData(value, typeof(RubyRegex));
				_writer.Write((byte)47);
				WriteStringValue(value.Pattern);
				_writer.Write((byte)value.Options);
			}

			private void WriteArray(RubyArray value)
			{
				WriteSubclassData(value, typeof(RubyArray));
				_writer.Write((byte)91);
				WriteInt32(value.Count);
				foreach (object item in value)
				{
					WriteAnObject(item);
				}
			}

			private void WriteHash(Hash value)
			{
				if (value.DefaultProc != null)
				{
					throw RubyExceptions.CreateTypeError("can't dump hash with default proc");
				}
				WriteSubclassData(value, typeof(Hash));
				char c = ((value.DefaultValue != null) ? '}' : '{');
				_writer.Write((byte)c);
				WriteInt32(value.Count);
				foreach (KeyValuePair<object, object> item in value)
				{
					WriteAnObject(item.Key);
					WriteAnObject(item.Value);
				}
				if (value.DefaultValue != null)
				{
					WriteAnObject(value.DefaultValue);
				}
			}

			private void WriteSymbol(string value, RubyEncoding encoding)
			{
				int value2;
				if (_symbols.TryGetValue(value, out value2))
				{
					_writer.Write((byte)59);
					WriteInt32(value2);
					return;
				}
				value2 = _symbols.Count;
				_symbols[value] = value2;
				_writer.Write((byte)58);
				WriteStringValue(value, encoding);
			}

			private void TestForAnonymous(RubyModule theModule)
			{
				if (theModule.Name == null)
				{
					throw RubyExceptions.CreateTypeError("can't dump anonymous {0} {1}", theModule.IsClass ? "class" : "module", theModule.GetDisplayName(_context, false));
				}
			}

			private void WriteRange(Range range)
			{
				WriteObject(range);
				WriteInt32(3);
				WriteSymbol("begin", RubyEncoding.Binary);
				WriteAnObject(range.Begin);
				WriteSymbol("end", RubyEncoding.Binary);
				WriteAnObject(range.End);
				WriteSymbol("excl", RubyEncoding.Binary);
				WriteAnObject(range.ExcludeEnd);
			}

			private void WriteObject(object obj)
			{
				_writer.Write((byte)111);
				RubyClass classOf = _context.GetClassOf(obj);
				TestForAnonymous(classOf);
				WriteModuleName(classOf);
			}

			private void WriteUsingDump(object obj)
			{
				_writer.Write((byte)117);
				RubyClass classOf = _context.GetClassOf(obj);
				TestForAnonymous(classOf);
				WriteModuleName(classOf);
				MutableString mutableString = _sites.Dump.Target(_sites.Dump, obj, _recursionLimit) as MutableString;
				if (mutableString == null)
				{
					throw RubyExceptions.CreateTypeError("_dump() must return string");
				}
				WriteStringValue(mutableString);
			}

			private void WriteUsingMarshalDump(object obj)
			{
				_writer.Write((byte)85);
				RubyClass classOf = _context.GetClassOf(obj);
				TestForAnonymous(classOf);
				WriteModuleName(classOf);
				WriteAnObject(_sites.MarshalDump.Target(_sites.MarshalDump, obj));
			}

			private void WriteClass(RubyClass obj)
			{
				_writer.Write((byte)99);
				TestForAnonymous(obj);
				WriteStringValue(obj.Name, _context.GetIdentifierEncoding());
			}

			private void WriteModule(RubyModule obj)
			{
				_writer.Write((byte)109);
				TestForAnonymous(obj);
				WriteStringValue(obj.Name, _context.GetIdentifierEncoding());
			}

			private void WriteStruct(RubyStruct obj)
			{
				WriteSubclassData(obj, typeof(RubyStruct));
				_writer.Write((byte)83);
				RubyClass classOf = _context.GetClassOf(obj);
				TestForAnonymous(classOf);
				WriteModuleName(classOf);
				ReadOnlyCollection<string> names = obj.GetNames();
				WriteInt32(names.Count);
				foreach (string item in names)
				{
					int index = obj.GetIndex(item);
					WriteSymbol(item, _context.GetIdentifierEncoding());
					WriteAnObject(obj[index]);
				}
			}

			private void WriteAnObject(object obj)
			{
				if (_recursionLimit == 0)
				{
					throw RubyExceptions.CreateArgumentError("exceed depth limit");
				}
				if (_recursionLimit > 0)
				{
					_recursionLimit--;
				}
				if (obj is int)
				{
					int num = (int)obj;
					if (num < -1073741824 || num >= 1073741824)
					{
						obj = (BigInteger)num;
					}
				}
				RubySymbol rubySymbol;
				int value;
				if (obj == null)
				{
					_writer.Write((byte)48);
				}
				else if (obj is bool)
				{
					_writer.Write((byte)(((bool)obj) ? 84u : 70u));
				}
				else if (obj is int)
				{
					WriteFixnum((int)obj);
				}
				else if ((rubySymbol = obj as RubySymbol) != null)
				{
					WriteSymbol(rubySymbol.ToString(), rubySymbol.Encoding);
				}
				else if (_objects.TryGetValue(obj, out value))
				{
					_writer.Write((byte)64);
					WriteInt32(value);
				}
				else
				{
					value = _objects.Count;
					_objects[obj] = value;
					bool found = _context.ResolveMethod(obj, "_dump", VisibilityContext.AllVisible).Found;
					bool found2 = _context.ResolveMethod(obj, "marshal_dump", VisibilityContext.AllVisible).Found;
					bool flag = false;
					string[] array = null;
					if (!found && !found2)
					{
						array = _context.GetInstanceVariableNames(obj);
						if (array.Length > 0)
						{
							_writer.Write((byte)73);
							flag = true;
						}
					}
					if (!found || found2)
					{
						RubyClass immediateClassOf = _context.GetImmediateClassOf(obj);
						if (immediateClassOf.IsSingletonClass)
						{
							RubyModule[] mixins = immediateClassOf.GetMixins();
							foreach (RubyModule module in mixins)
							{
								_writer.Write((byte)101);
								WriteModuleName(module);
							}
						}
					}
					if (obj is double)
					{
						WriteFloat((double)obj);
					}
					else if (obj is float)
					{
						WriteFloat((float)obj);
					}
					else if (obj is BigInteger)
					{
						WriteBignum((BigInteger)obj);
					}
					else if (found2)
					{
						WriteUsingMarshalDump(obj);
					}
					else if (found)
					{
						WriteUsingDump(obj);
					}
					else if (obj is MutableString)
					{
						WriteString((MutableString)obj);
					}
					else if (obj is RubyArray)
					{
						WriteArray((RubyArray)obj);
					}
					else if (obj is Hash)
					{
						WriteHash((Hash)obj);
					}
					else if (obj is RubyRegex)
					{
						WriteRegex((RubyRegex)obj);
					}
					else if (obj is RubyClass)
					{
						WriteClass((RubyClass)obj);
					}
					else if (obj is RubyModule)
					{
						WriteModule((RubyModule)obj);
					}
					else if (obj is RubyStruct)
					{
						WriteStruct((RubyStruct)obj);
					}
					else if (obj is Range)
					{
						WriteRange((Range)obj);
					}
					else
					{
						if (flag)
						{
							_writer.BaseStream.Seek(-1L, SeekOrigin.Current);
						}
						else
						{
							flag = true;
						}
						WriteObject(obj);
					}
					if (flag)
					{
						WriteInt32(array.Length);
						RubyEncoding identifierEncoding = _context.GetIdentifierEncoding();
						string[] array2 = array;
						foreach (string text in array2)
						{
							object value2;
							if (!_context.TryGetInstanceVariable(obj, text, out value2))
							{
								value2 = null;
							}
							WriteSymbol(text, identifierEncoding);
							WriteAnObject(value2);
						}
					}
				}
				if (_recursionLimit >= 0)
				{
					_recursionLimit++;
				}
			}

			internal void Dump(object obj)
			{
				WritePreamble();
				WriteAnObject(obj);
				_writer.BaseStream.Flush();
			}
		}

		internal class MarshalReader
		{
			private sealed class Symbol
			{
				private string _string;

				private RubySymbol _symbol;

				public Symbol(string str, RubySymbol sym)
				{
					_string = str;
					_symbol = sym;
				}

				public string GetString()
				{
					return _string ?? (_string = _symbol.ToString());
				}

				public RubySymbol GetSymbol(RubyContext context)
				{
					return _symbol ?? (_symbol = context.EncodeIdentifier(_string));
				}
			}

			private readonly BinaryReader _reader;

			private readonly ReaderSites _sites;

			private readonly RubyGlobalScope _globalScope;

			private readonly Proc _proc;

			private readonly Dictionary<int, Symbol> _symbols;

			private readonly Dictionary<int, object> _objects;

			private RubyContext Context
			{
				get
				{
					return _globalScope.Context;
				}
			}

			internal MarshalReader(ReaderSites sites, BinaryReader reader, RubyGlobalScope globalScope, Proc proc)
			{
				_sites = sites;
				_reader = reader;
				_globalScope = globalScope;
				_proc = proc;
				_symbols = new Dictionary<int, Symbol>();
				_objects = new Dictionary<int, object>();
			}

			private void CheckPreamble()
			{
				int num = _reader.ReadByte();
				int num2 = _reader.ReadByte();
				if (num != 4 || num2 > 8)
				{
					throw RubyExceptions.CreateTypeError("incompatible marshal file format (can't be read)\n\tformat version {0}.{1} required; {2}.{3} given", 4, 8, num, num2);
				}
				if (num2 < 8)
				{
					Context.ReportWarning(string.Format(CultureInfo.InvariantCulture, "incompatible marshal file format (can be read)\n\tformat version {0}.{1} required; {2}.{3} given", 4, 8, num, num2));
				}
			}

			private BigInteger ReadBignum()
			{
				int sign;
				switch (_reader.ReadByte())
				{
				case 43:
					sign = 1;
					break;
				case 45:
					sign = -1;
					break;
				default:
					sign = 0;
					break;
				}
				int num = ReadInt32();
				int num2 = num / 2;
				int num3 = (num + 1) / 2;
				uint[] array = new uint[num3];
				for (int i = 0; i < num2; i++)
				{
					array[i] = _reader.ReadUInt32();
				}
				if (num2 != num3)
				{
					array[num2] = _reader.ReadUInt16();
				}
				return new BigInteger(sign, array);
			}

			private int ReadInt32()
			{
				sbyte b = _reader.ReadSByte();
				if (b == 0)
				{
					return 0;
				}
				if (b > 4)
				{
					return b - 5;
				}
				if (b < -4)
				{
					return b + 5;
				}
				byte b2;
				if (b < 0)
				{
					b2 = byte.MaxValue;
					b = (sbyte)(-b);
				}
				else
				{
					b2 = 0;
				}
				uint num = 0u;
				for (int i = 0; i < 4; i++)
				{
					uint num2 = ((i >= b) ? b2 : _reader.ReadByte());
					num |= num2 << i * 8;
				}
				return (int)num;
			}

			private double ReadFloat()
			{
				MutableString mutableString = ReadString();
				if (mutableString.Equals(_positiveInfinityString))
				{
					return double.PositiveInfinity;
				}
				if (mutableString.Equals(_negativeInfinityString))
				{
					return double.NegativeInfinity;
				}
				if (mutableString.Equals(_nanString))
				{
					return double.NaN;
				}
				int num = mutableString.IndexOf(0);
				if (num >= 0)
				{
					mutableString.Remove(num, mutableString.Length - num);
				}
				return Protocols.ConvertStringToFloat(Context, mutableString);
			}

			private MutableString ReadString()
			{
				int count = ReadInt32();
				byte[] bytes = _reader.ReadBytes(count);
				return MutableString.CreateBinary(bytes, RubyEncoding.Binary);
			}

			private RubyRegex ReadRegex()
			{
				MutableString pattern = ReadString();
				int options = _reader.ReadByte();
				return new RubyRegex(pattern, (RubyRegexOptions)options);
			}

			private RubyArray ReadArray()
			{
				int num = ReadInt32();
				RubyArray rubyArray = new RubyArray(num);
				for (int i = 0; i < num; i++)
				{
					rubyArray.Add(ReadAnObject(false));
				}
				return rubyArray;
			}

			private Hash ReadHash(int typeFlag)
			{
				int num = ReadInt32();
				Hash hash = new Hash(Context);
				for (int i = 0; i < num; i++)
				{
					object key = ReadAnObject(false);
					hash[key] = ReadAnObject(false);
				}
				if (typeFlag == 125)
				{
					hash.DefaultValue = ReadAnObject(false);
				}
				return hash;
			}

			private string ReadIdentifier()
			{
				return ReadSymbolOrIdentifier(_reader.ReadByte(), false).GetString();
			}

			private Symbol ReadSymbolOrIdentifier(int typeFlag, bool symbol)
			{
				Symbol value;
				if (typeFlag == 59)
				{
					int key = ReadInt32();
					if (!_symbols.TryGetValue(key, out value))
					{
						throw RubyExceptions.CreateArgumentError("bad symbol");
					}
				}
				else
				{
					int count = ReadInt32();
					byte[] array = _reader.ReadBytes(count);
					value = ((!symbol) ? new Symbol(Context.GetIdentifierEncoding().Encoding.GetString(array, 0, array.Length), null) : new Symbol(null, Context.CreateSymbol(array, RubyEncoding.Binary)));
					_symbols[_symbols.Count] = value;
				}
				return value;
			}

			private RubyClass ReadType()
			{
				return (RubyClass)ReadClassOrModule(99, ReadIdentifier());
			}

			private object UnmarshalNewObject()
			{
				return RubyUtils.CreateObject(ReadType());
			}

			private object ReadObject()
			{
				RubyClass theclass = ReadType();
				int num = ReadInt32();
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				for (int i = 0; i < num; i++)
				{
					string key = ReadIdentifier();
					dictionary[key] = ReadAnObject(false);
				}
				return RubyUtils.CreateObject(theclass, dictionary);
			}

			private object ReadUsingLoad()
			{
				RubyClass arg = ReadType();
				return _sites.Load.Target(_sites.Load, arg, ReadString());
			}

			private object ReadUsingMarshalLoad()
			{
				object obj = UnmarshalNewObject();
				_sites.MarshalLoad.Target(_sites.MarshalLoad, obj, ReadAnObject(false));
				return obj;
			}

			private object ReadClassOrModule(int typeFlag)
			{
				string name = ReadString().ToString();
				return ReadClassOrModule(typeFlag, name);
			}

			private object ReadClassOrModule(int typeFlag, string name)
			{
				RubyModule result;
				if (!Context.TryGetModule(_globalScope, name, out result))
				{
					throw RubyExceptions.CreateArgumentError("undefined class/module {0}", name);
				}
				bool flag = result is RubyClass;
				if (flag && typeFlag == 109)
				{
					throw RubyExceptions.CreateArgumentError("{0} does not refer module", name);
				}
				if (!flag && typeFlag == 99)
				{
					throw RubyExceptions.CreateArgumentError("{0} does not refer class", name);
				}
				return result;
			}

			private RubyStruct ReadStruct()
			{
				RubyStruct rubyStruct = UnmarshalNewObject() as RubyStruct;
				if (rubyStruct == null)
				{
					throw RubyExceptions.CreateArgumentError("non-initialized struct");
				}
				ReadOnlyCollection<string> names = rubyStruct.GetNames();
				int num = ReadInt32();
				if (num != names.Count)
				{
					throw RubyExceptions.CreateArgumentError("struct size differs");
				}
				for (int i = 0; i < num; i++)
				{
					string text = ReadIdentifier();
					if (text != names[i])
					{
						RubyClass classOf = Context.GetClassOf(rubyStruct);
						throw RubyExceptions.CreateTypeError("struct {0} not compatible ({1} for {2})", classOf.Name, text, names[i]);
					}
					rubyStruct[i] = ReadAnObject(false);
				}
				return rubyStruct;
			}

			private object ReadInstanced()
			{
				object obj = ReadAnObject(true);
				int num = ReadInt32();
				for (int i = 0; i < num; i++)
				{
					string name = ReadIdentifier();
					Context.SetInstanceVariable(obj, name, ReadAnObject(false));
				}
				return obj;
			}

			private object ReadExtended()
			{
				string name = ReadIdentifier();
				RubyModule self = ReadClassOrModule(109, name) as RubyModule;
				object obj = ReadAnObject(true);
				ModuleOps.ExtendObject(self, obj);
				return obj;
			}

			private object ReadUserClass()
			{
				object obj = UnmarshalNewObject();
				bool flag = false;
				int num = _reader.ReadByte();
				switch (num)
				{
				case 34:
				{
					MutableString mutableString = obj as MutableString;
					if (mutableString != null)
					{
						mutableString.Replace(0, mutableString.Length, ReadString());
						flag = true;
					}
					break;
				}
				case 47:
				{
					RubyRegex rubyRegex = obj as RubyRegex;
					if (rubyRegex != null)
					{
						RubyRegex rubyRegex2 = ReadRegex();
						rubyRegex.Set(rubyRegex2.Pattern, rubyRegex2.Options);
						flag = true;
					}
					break;
				}
				case 91:
				{
					RubyArray rubyArray = obj as RubyArray;
					if (rubyArray != null)
					{
						rubyArray.AddRange(ReadArray());
						flag = true;
					}
					break;
				}
				case 123:
				case 125:
				{
					Hash hash = obj as Hash;
					if (hash == null)
					{
						break;
					}
					Hash hash2 = ReadHash(num);
					hash.DefaultProc = hash2.DefaultProc;
					hash.DefaultValue = hash2.DefaultValue;
					foreach (KeyValuePair<object, object> item in hash2)
					{
						hash.Add(item.Key, item.Value);
					}
					flag = true;
					break;
				}
				}
				if (!flag)
				{
					throw RubyExceptions.CreateArgumentError("incompatible base type");
				}
				return obj;
			}

			private object ReadAnObject(bool noCache)
			{
				object obj = null;
				bool flag = !noCache && _proc != null;
				int num = _reader.ReadByte();
				switch (num)
				{
				case 48:
					obj = null;
					break;
				case 84:
					obj = true;
					break;
				case 70:
					obj = false;
					break;
				case 105:
					obj = ReadInt32();
					break;
				case 58:
					obj = ReadSymbolOrIdentifier(num, true).GetSymbol(Context);
					break;
				case 59:
					obj = ReadSymbolOrIdentifier(num, true).GetSymbol(Context);
					flag = false;
					break;
				case 64:
					obj = _objects[ReadInt32()];
					flag = false;
					break;
				default:
				{
					int count = _objects.Count;
					if (!noCache)
					{
						_objects[count] = null;
					}
					switch (num)
					{
					case 102:
						obj = ReadFloat();
						break;
					case 108:
						obj = ReadBignum();
						break;
					case 34:
						obj = ReadString();
						break;
					case 47:
						obj = ReadRegex();
						break;
					case 91:
						obj = ReadArray();
						break;
					case 123:
					case 125:
						obj = ReadHash(num);
						break;
					case 111:
						obj = ReadObject();
						break;
					case 117:
						obj = ReadUsingLoad();
						break;
					case 85:
						obj = ReadUsingMarshalLoad();
						break;
					case 99:
					case 109:
						obj = ReadClassOrModule(num);
						break;
					case 83:
						obj = ReadStruct();
						break;
					case 73:
						obj = ReadInstanced();
						break;
					case 101:
						obj = ReadExtended();
						break;
					case 67:
						obj = ReadUserClass();
						break;
					default:
						throw RubyExceptions.CreateArgumentError("dump format error({0})", num);
					}
					if (!noCache)
					{
						_objects[count] = obj;
					}
					break;
				}
				}
				if (flag)
				{
					_sites.ProcCall.Target(_sites.ProcCall, _proc, obj);
				}
				return obj;
			}

			internal object Load()
			{
				try
				{
					CheckPreamble();
					return ReadAnObject(false);
				}
				catch (IOException ex)
				{
					throw RubyExceptions.CreateArgumentError("marshal data too short", ex);
				}
			}
		}

		[RubyConstant]
		public const int MAJOR_VERSION = 4;

		[RubyConstant]
		public const int MINOR_VERSION = 8;

		private static readonly MutableString _positiveInfinityString = MutableString.CreateAscii("inf").Freeze();

		private static readonly MutableString _negativeInfinityString = MutableString.CreateAscii("-inf").Freeze();

		private static readonly MutableString _nanString = MutableString.CreateAscii("nan").Freeze();

		[RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Dump(WriterSites sites, RubyModule self, object obj)
		{
			return Dump(sites, self, obj, -1);
		}

		[RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Dump(WriterSites sites, RubyModule self, object obj, int limit)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(memoryStream);
			MarshalWriter marshalWriter = new MarshalWriter(sites, writer, self.Context, limit);
			marshalWriter.Dump(obj);
			return MutableString.CreateBinary(memoryStream.ToArray());
		}

		[RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
		public static object Dump(WriterSites sites, RubyModule self, object obj, [NotNull] RubyIO io, [Optional] int? limit)
		{
			BinaryWriter binaryWriter = io.GetBinaryWriter();
			MarshalWriter marshalWriter = new MarshalWriter(sites, binaryWriter, self.Context, limit);
			marshalWriter.Dump(obj);
			return io;
		}

		[RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
		public static object Dump(WriterSites sites, RespondToStorage respondToStorage, RubyModule self, object obj, object io, [Optional] int? limit)
		{
			Stream stream = null;
			if (io != null)
			{
				stream = RubyIOOps.CreateIOWrapper(respondToStorage, io, FileAccess.Write);
			}
			if (stream == null || !stream.CanWrite)
			{
				throw RubyExceptions.CreateTypeError("instance of IO needed");
			}
			BinaryWriter writer = new BinaryWriter(stream);
			MarshalWriter marshalWriter = new MarshalWriter(sites, writer, self.Context, limit);
			marshalWriter.Dump(obj);
			return io;
		}

		[RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("restore", RubyMethodAttributes.PublicSingleton)]
		public static object Load(ReaderSites sites, RubyScope scope, RubyModule self, [NotNull] MutableString source, [Optional] Proc proc)
		{
			BinaryReader reader = new BinaryReader(new MemoryStream(source.ConvertToBytes()));
			MarshalReader marshalReader = new MarshalReader(sites, reader, scope.GlobalScope, proc);
			return marshalReader.Load();
		}

		[RubyMethod("restore", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
		public static object Load(ReaderSites sites, RubyScope scope, RubyModule self, [NotNull] RubyIO source, [Optional] Proc proc)
		{
			BinaryReader binaryReader = source.GetBinaryReader();
			MarshalReader marshalReader = new MarshalReader(sites, binaryReader, scope.GlobalScope, proc);
			return marshalReader.Load();
		}

		[RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("restore", RubyMethodAttributes.PublicSingleton)]
		public static object Load(ReaderSites sites, RespondToStorage respondToStorage, RubyScope scope, RubyModule self, object source, [Optional] Proc proc)
		{
			Stream stream = null;
			if (source != null)
			{
				stream = RubyIOOps.CreateIOWrapper(respondToStorage, source, FileAccess.Read);
			}
			if (stream == null || !stream.CanRead)
			{
				throw RubyExceptions.CreateTypeError("instance of IO needed");
			}
			BinaryReader reader = new BinaryReader(stream);
			MarshalReader marshalReader = new MarshalReader(sites, reader, scope.GlobalScope, proc);
			return marshalReader.Load();
		}
	}
}
