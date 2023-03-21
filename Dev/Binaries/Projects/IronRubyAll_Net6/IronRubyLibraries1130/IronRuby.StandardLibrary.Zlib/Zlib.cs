using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Zlib
{
	[RubyModule("Zlib")]
	public static class Zlib
	{
		[RubyClass("ZStream")]
		public class ZStream
		{
			protected readonly List<byte> _inputBuffer;

			protected readonly List<byte> _outputBuffer;

			protected int _outPos = -1;

			protected int _inPos = -1;

			protected byte _bitBucket;

			protected byte _bitCount;

			protected bool _closed;

			public ZStream()
			{
				_outPos = -1;
				_inPos = -1;
				_bitBucket = 0;
				_bitCount = 0;
				_inputBuffer = new List<byte>();
				_outputBuffer = new List<byte>();
			}

			public bool Close()
			{
				_closed = true;
				return _closed;
			}

			[RubyMethod("adler")]
			public static int Adler(ZStream self)
			{
				throw new NotImplementedError();
			}

			[RubyMethod("avail_in")]
			public static int AvailIn(ZStream self)
			{
				return self._inputBuffer.Count - self._inPos;
			}

			[RubyMethod("avail_out")]
			public static int GetAvailOut(ZStream self)
			{
				return self._outputBuffer.Count - self._outPos;
			}

			[RubyMethod("avail_out=")]
			public static int SetAvailOut(ZStream self, int size)
			{
				self._outputBuffer.Capacity = size;
				return self._outputBuffer.Count;
			}

			[RubyMethod("finish")]
			[RubyMethod("close")]
			public static bool Close(ZStream self)
			{
				return self.Close();
			}

			[RubyMethod("finished?")]
			[RubyMethod("closed?")]
			[RubyMethod("stream_end?")]
			public static bool IsClosed(ZStream self)
			{
				return self._closed;
			}

			[RubyMethod("data_type")]
			public static void DataType(ZStream self)
			{
				throw new NotImplementedException();
			}

			[RubyMethod("flush_next_in")]
			public static List<byte> FlushNextIn(ZStream self)
			{
				self._inPos = self._inputBuffer.Count;
				return self._inputBuffer;
			}

			[RubyMethod("flush_next_out")]
			public static List<byte> FlushNextOut(ZStream self)
			{
				self._outPos = self._outputBuffer.Count;
				return self._outputBuffer;
			}

			[RubyMethod("reset")]
			public static void Reset(ZStream self)
			{
				self._outPos = -1;
				self._inPos = -1;
				self._inputBuffer.Clear();
				self._outputBuffer.Clear();
			}

			[RubyMethod("total_in")]
			public static int TotalIn(ZStream self)
			{
				return self._inputBuffer.Count;
			}

			[RubyMethod("total_out")]
			public static int TotalOut(ZStream self)
			{
				return self._outputBuffer.Count;
			}

			protected int GetBits(int need)
			{
				int num = _bitBucket;
				while (_bitCount < need)
				{
					num |= _inputBuffer[++_inPos] << (int)_bitCount;
					_bitCount += 8;
				}
				_bitBucket = (byte)(num >> need);
				_bitCount -= (byte)need;
				return num & ((1 << need) - 1);
			}
		}

		[RubyClass("Inflate")]
		public class Inflate : ZStream
		{
			private sealed class HuffmanTree
			{
				internal readonly List<int> Count;

				internal readonly List<int> Symbol;

				internal HuffmanTree()
				{
					Count = new List<int>();
					Symbol = new List<int>();
				}
			}

			private int _wBits;

			private bool _rawDeflate;

			private HuffmanTree _fixedLengthCodes;

			private HuffmanTree _fixedDistanceCodes;

			private HuffmanTree _dynamicLengthCodes;

			private HuffmanTree _dynamicDistanceCodes;

			public Inflate()
				: this(15)
			{
			}

			public Inflate(int windowBits)
			{
				_wBits = windowBits;
				if (_wBits < 0)
				{
					_rawDeflate = true;
					_wBits *= -1;
				}
			}

			private void DynamicCodes()
			{
				byte[] array = new byte[19]
				{
					16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
					11, 4, 12, 3, 13, 2, 14, 1, 15
				};
				int num = GetBits(5) + 257;
				int num2 = GetBits(5) + 1;
				int num3 = GetBits(4) + 4;
				List<int> list = new List<int>();
				_dynamicLengthCodes = new HuffmanTree();
				_dynamicDistanceCodes = new HuffmanTree();
				if (num > 286 || num2 > 30)
				{
					throw new DataError("too many length or distance codes");
				}
				int i;
				for (i = 0; i < num3; i++)
				{
					SetOrExpand(list, array[i], GetBits(3));
				}
				for (; i < 19; i++)
				{
					SetOrExpand(list, array[i], 0);
				}
				if (ConstructTree(_dynamicLengthCodes, list, 18) != 0)
				{
					throw new DataError("code lengths codes incomplete");
				}
				i = 0;
				while (i < num + num2)
				{
					int num4 = Decode(_dynamicLengthCodes);
					if (num4 < 16)
					{
						SetOrExpand(list, i, num4);
						i++;
						continue;
					}
					int item = 0;
					switch (num4)
					{
					case 16:
						if (i == 0)
						{
							throw new DataError("repeat lengths with no first length");
						}
						item = list[i - 1];
						num4 = 3 + GetBits(2);
						break;
					case 17:
						num4 = 3 + GetBits(3);
						break;
					case 18:
						num4 = 11 + GetBits(7);
						break;
					default:
						throw new DataError("invalid repeat length code");
					}
					if (i + num4 > num + num2)
					{
						throw new DataError("repeat more than specified lengths");
					}
					while (num4 != 0)
					{
						SetOrExpand(list, i, item);
						i++;
						num4--;
					}
				}
				int num5 = ConstructTree(_dynamicLengthCodes, list, num - 1);
				if (num5 < 0 || (num5 > 0 && num - _dynamicLengthCodes.Count[0] != 1))
				{
					throw new DataError("invalid literal/length code lengths");
				}
				list.RemoveRange(0, num);
				num5 = ConstructTree(_dynamicDistanceCodes, list, num2 - 1);
				if (num5 < 0 || (num5 > 0 && num2 - _dynamicDistanceCodes.Count[0] != 1))
				{
					throw new DataError("invalid distance code lengths");
				}
				Codes(_dynamicLengthCodes, _dynamicDistanceCodes);
			}

			private void NoCompression()
			{
				_bitBucket = 0;
				_bitCount = 0;
				if (_inPos + 4 > _inputBuffer.Count)
				{
					throw new DataError("not enough input to read length code");
				}
				int num = _inputBuffer[++_inPos] | (_inputBuffer[++_inPos] << 8);
				int num2 = _inputBuffer[++_inPos] | (_inputBuffer[++_inPos] << 8);
				if ((ushort)num != (ushort)(~num2))
				{
					throw new DataError("invalid stored block lengths");
				}
				if (_inPos + num > _inputBuffer.Count)
				{
					throw new DataError("ran out of input");
				}
				_outputBuffer.AddRange(_inputBuffer.GetRange(_inPos + 1, num));
				_inPos += num;
				_outPos += num;
			}

			private void FixedCodes()
			{
				if (_fixedLengthCodes == null && _fixedDistanceCodes == null)
				{
					GenerateHuffmans();
				}
				Codes(_fixedLengthCodes, _fixedDistanceCodes);
			}

			private void GenerateHuffmans()
			{
				List<int> list = new List<int>(300);
				int i;
				for (i = 0; i < 144; i++)
				{
					list.Add(8);
				}
				for (; i < 256; i++)
				{
					list.Add(9);
				}
				for (; i < 280; i++)
				{
					list.Add(7);
				}
				for (; i < 288; i++)
				{
					list.Add(8);
				}
				_fixedLengthCodes = new HuffmanTree();
				ConstructTree(_fixedLengthCodes, list, 287);
				list.Clear();
				for (int j = 0; j < 30; j++)
				{
					list.Add(5);
				}
				_fixedDistanceCodes = new HuffmanTree();
				ConstructTree(_fixedDistanceCodes, list, 29);
			}

			private int ConstructTree(HuffmanTree tree, List<int> lengths, int symbols)
			{
				List<int> list = new List<int>();
				for (int i = 0; i <= 15; i++)
				{
					SetOrExpand(tree.Count, i, 0);
				}
				for (int j = 0; j <= symbols; j++)
				{
					tree.Count[lengths[j]]++;
				}
				if (tree.Count[0] == symbols)
				{
					return 0;
				}
				int num = 1;
				for (int k = 1; k <= 15; k++)
				{
					num <<= 1;
					num -= tree.Count[k];
					if (num < 0)
					{
						return num;
					}
				}
				list.Add(0);
				list.Add(0);
				for (int l = 1; l <= 14; l++)
				{
					list.Add(0);
					list[l + 1] = list[l] + tree.Count[l];
				}
				for (int m = 0; m <= symbols; m++)
				{
					if (lengths[m] != 0)
					{
						SetOrExpand(tree.Symbol, list[lengths[m]], m);
						list[lengths[m]]++;
					}
				}
				return num;
			}

			private void SetOrExpand<T>(List<T> list, int index, T item)
			{
				int num = index + 1;
				for (int num2 = num - list.Count; num2 > 0; num2--)
				{
					list.Add(default(T));
				}
				list[index] = item;
			}

			private int Codes(HuffmanTree lengthCodes, HuffmanTree distanceCodes)
			{
				int[] array = new int[29]
				{
					3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
					15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
					67, 83, 99, 115, 131, 163, 195, 227, 258
				};
				int[] array2 = new int[29]
				{
					0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
					1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
					4, 4, 4, 4, 5, 5, 5, 5, 0
				};
				int[] array3 = new int[30]
				{
					1, 2, 3, 4, 5, 7, 9, 13, 17, 25,
					33, 49, 65, 97, 129, 193, 257, 385, 513, 769,
					1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
				};
				int[] array4 = new int[30]
				{
					0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
					4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
					9, 9, 10, 10, 11, 11, 12, 12, 13, 13
				};
				int num = 0;
				while (num != 256)
				{
					num = Decode(lengthCodes);
					if (num < 0)
					{
						return num;
					}
					if (num < 256)
					{
						SetOrExpand(_outputBuffer, ++_outPos, (byte)num);
					}
					if (num > 256)
					{
						num -= 257;
						if (num >= 29)
						{
							throw new DataError("invalid literal/length or distance code in fixed or dynamic block");
						}
						int num2 = array[num] + GetBits((byte)array2[num]);
						num = Decode(distanceCodes);
						if (num < 0)
						{
							return num;
						}
						int num3 = array3[num] + GetBits((byte)array4[num]);
						if (num3 > _outputBuffer.Count)
						{
							throw new DataError("distance is too far back in fixed or dynamic block");
						}
						while (num2 > 0)
						{
							SetOrExpand(_outputBuffer, ++_outPos, _outputBuffer[_outPos - num3]);
							num2--;
						}
					}
				}
				return 0;
			}

			private int Decode(HuffmanTree tree)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				for (int i = 1; i <= 15; i++)
				{
					num |= GetBits(1);
					int num4 = tree.Count[i];
					if (num < num2 + num4)
					{
						return tree.Symbol[num3 + (num - num2)];
					}
					num3 += num4;
					num2 += num4;
					num2 <<= 1;
					num <<= 1;
				}
				return -9;
			}

			[RubyMethod("inflate")]
			public static MutableString InflateString(Inflate self, [DefaultProtocol][NotNull] MutableString zstring)
			{
				if (zstring.IsEmpty)
				{
					throw new BufError("buffer error");
				}
				if (zstring.GetByteCount() == 6 && zstring.GetByte(0) == 88 && zstring.GetByte(1) == 133 && zstring.GetByte(2) == 0 && zstring.GetByte(3) == 0 && zstring.GetByte(4) == 0 && zstring.GetByte(5) == 0)
				{
					return MutableString.CreateEmpty();
				}
				self._inputBuffer.AddRange(zstring.ConvertToBytes());
				if (!self._rawDeflate)
				{
					byte b = self._inputBuffer[++self._inPos];
					byte b2 = self._inputBuffer[++self._inPos];
					if (((b << 8) + b2) % 31 != 0)
					{
						throw new DataError("incorrect header check");
					}
					byte b3 = (byte)(b & 0xFu);
					if (b3 != 8)
					{
						throw new DataError("unknown compression method");
					}
					byte b4 = (byte)(b >> 4);
					if (b4 + 8 > self._wBits)
					{
						throw new DataError("invalid window size");
					}
					if ((b2 & 0x20) >> 5 == 1)
					{
						self._inPos += 4;
					}
				}
				bool flag = false;
				while (!flag)
				{
					flag = self.GetBits(1) == 1;
					switch ((byte)self.GetBits(2))
					{
					case 0:
						self.NoCompression();
						break;
					case 1:
						self.FixedCodes();
						break;
					case 2:
						self.DynamicCodes();
						break;
					case 3:
						throw new DataError("invalid block type");
					}
				}
				return Close(self);
			}

			[RubyMethod("inflate", RubyMethodAttributes.PublicSingleton)]
			public static MutableString InflateString(RubyClass self, [DefaultProtocol][NotNull] MutableString zstring)
			{
				return InflateString(new Inflate(), zstring);
			}

			[RubyMethod("close")]
			public static MutableString Close(Inflate self)
			{
				return MutableString.CreateBinary(self._outputBuffer, RubyEncoding.Binary);
			}
		}

		[RubyClass("GzipFile")]
		public class GZipFile
		{
			[RubyClass("Error")]
			public class Error : RuntimeError
			{
				public Error(string message)
					: base(message)
				{
				}
			}

			protected IOWrapper _ioWrapper;

			protected List<byte> _inputBuffer;

			protected List<byte> _outputBuffer;

			protected int _outPos;

			protected int _inPos;

			protected bool _isClosed;

			protected MutableString _originalName;

			protected MutableString _comment;

			public GZipFile(IOWrapper ioWrapper)
			{
				_ioWrapper = ioWrapper;
				_inputBuffer = new List<byte>();
				_outputBuffer = new List<byte>();
				_outPos = -1;
				_inPos = -1;
			}

			[RubyMethod("wrap", RubyMethodAttributes.PublicSingleton)]
			public static object Wrap(BinaryOpStorage newStorage, UnaryOpStorage closedStorage, UnaryOpStorage closeStorage, BlockParam block, RubyClass self, object io)
			{
				CallSite<Func<CallSite, object, object, object>> callSite = newStorage.GetCallSite("new");
				GZipFile gZipFile = (GZipFile)callSite.Target(callSite, self, io);
				if (block == null)
				{
					return gZipFile;
				}
				try
				{
					object blockResult;
					block.Yield(gZipFile, out blockResult);
					return blockResult;
				}
				finally
				{
					CloseFile(closedStorage, closeStorage, self, gZipFile);
				}
			}

			private static void CloseFile(UnaryOpStorage closedStorage, UnaryOpStorage closeStorage, RubyClass self, GZipFile gzipFile)
			{
				CallSite<Func<CallSite, object, object>> callSite = closedStorage.GetCallSite("closed?");
				if (!Protocols.IsTrue(callSite.Target(callSite, gzipFile)))
				{
					CallSite<Func<CallSite, object, object>> callSite2 = closeStorage.GetCallSite("close");
					callSite2.Target(callSite2, gzipFile);
				}
			}

			internal static void Close(UnaryOpStorage closeStorage, GZipFile self, bool closeIO)
			{
				if (self._isClosed)
				{
					throw new Error("closed gzip stream");
				}
				if (closeIO && self._ioWrapper.CanBeClosed)
				{
					CallSite<Func<CallSite, object, object>> callSite = closeStorage.GetCallSite("close");
					callSite.Target(callSite, self._ioWrapper.UnderlyingObject);
				}
				self._isClosed = true;
			}

			[RubyMethod("closed?")]
			public static bool IsClosed(GZipFile self)
			{
				return self._isClosed;
			}

			[RubyMethod("comment")]
			public static MutableString Comment(GZipFile self)
			{
				if (self._isClosed)
				{
					throw new Error("closed gzip stream");
				}
				return self._comment;
			}

			[RubyMethod("orig_name")]
			[RubyMethod("original_name")]
			public static MutableString OriginalName(GZipFile self)
			{
				if (self._isClosed)
				{
					throw new Error("closed gzip stream");
				}
				return self._originalName;
			}
		}

		[RubyClass("GzipReader")]
		public class GZipReader : GZipFile
		{
			protected MutableString _xtraField;

			protected MutableString _contents;

			protected ushort _headerCrc;

			[RubyConstant("OSES")]
			public static string[] OSES = new string[15]
			{
				"FAT filesystem", "Amiga", "VMS (or OpenVMS)", "Unix", "VM/CMS", "Atari TOS", "HPFS fileystem (OS/2, NT)", "Macintosh", "Z-System", "CP/M",
				"TOPS-20", "NTFS filesystem (NT)", "QDOS", "Acorn RISCOS", "unknown"
			};

			[RubyMethod("xtra_field")]
			public static MutableString ExtraField(GZipReader self)
			{
				return self._xtraField;
			}

			private bool IsBitSet(byte b, byte bit)
			{
				return (b & (1 << (int)bit)) == 1 << (int)bit;
			}

			[RubyConstructor]
			public static GZipReader Create(RespondToStorage respondToStorage, RubyClass self, object io)
			{
				IOWrapper iOWrapper = null;
				if (io != null)
				{
					iOWrapper = RubyIOOps.CreateIOWrapper(respondToStorage, io, FileAccess.Read);
				}
				if (iOWrapper == null || !iOWrapper.CanRead)
				{
					throw RubyExceptions.CreateMethodMissing(self.Context, io, "read");
				}
				using (BinaryReader reader = new BinaryReader(iOWrapper))
				{
					return new GZipReader(iOWrapper, reader);
				}
			}

			private static ushort ReadUInt16LE(BinaryReader reader)
			{
				return (ushort)(reader.ReadByte() | (reader.ReadByte() << 8));
			}

			private static uint ReadUInt32LE(BinaryReader reader)
			{
				return (uint)(reader.ReadByte() | (reader.ReadByte() << 8) | (reader.ReadByte() << 16) | (reader.ReadByte() << 24));
			}

			private static MutableString ReadStringZ(BinaryReader reader)
			{
				List<byte> list = new List<byte>();
				byte item;
				while ((item = reader.ReadByte()) != 0)
				{
					list.Add(item);
				}
				return MutableString.CreateBinary(list, RubyEncoding.Binary);
			}

			private static MutableString ReadToEnd(BinaryReader reader)
			{
				List<byte> list = new List<byte>();
				try
				{
					while (true)
					{
						list.Add(reader.ReadByte());
					}
				}
				catch (EndOfStreamException)
				{
				}
				return MutableString.CreateBinary(list, RubyEncoding.Binary);
			}

			private GZipReader(IOWrapper ioWrapper, BinaryReader reader)
				: base(ioWrapper)
			{
				if (ReadUInt16LE(reader) != 35615)
				{
					throw new Error("not in gzip format");
				}
				if (reader.ReadByte() != 8)
				{
					throw new Error("unknown compression method");
				}
				byte b = reader.ReadByte();
				IsBitSet(b, 0);
				bool flag = IsBitSet(b, 1);
				bool flag2 = IsBitSet(b, 2);
				bool flag3 = IsBitSet(b, 3);
				bool flag4 = IsBitSet(b, 4);
				uint num = ReadUInt32LE(reader);
				RubyTime.Epoch.AddSeconds(num);
				reader.ReadByte();
				string text = OSES[reader.ReadByte()];
				if (flag2)
				{
					int count = ReadUInt16LE(reader);
					_xtraField = MutableString.CreateBinary(reader.ReadBytes(count));
				}
				if (flag3)
				{
					_originalName = ReadStringZ(reader);
				}
				else
				{
					_originalName = MutableString.CreateBinary();
				}
				if (flag4)
				{
					_comment = ReadStringZ(reader);
				}
				else
				{
					_comment = MutableString.CreateBinary();
				}
				if (flag)
				{
					_headerCrc = ReadUInt16LE(reader);
				}
				_contents = ReadToEnd(reader);
			}

			[RubyMethod("read")]
			public static MutableString Read(GZipReader self)
			{
				Inflate self2 = new Inflate(-15);
				return Inflate.InflateString(self2, self._contents);
			}

			[RubyMethod("open", RubyMethodAttributes.PrivateInstance)]
			public static GZipReader Open(GZipReader self)
			{
				return self;
			}

			[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
			public static GZipReader Open(RespondToStorage respondToStorage, RubyClass self, [DefaultProtocol][NotNull] MutableString path)
			{
				return Create(respondToStorage, self, new RubyFile(self.Context, path.ConvertToString(), IOMode.ReadOnly));
			}

			[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
			public static object Open(RespondToStorage respondToStorage, [NotNull] BlockParam block, RubyClass self, [DefaultProtocol][NotNull] MutableString path)
			{
				GZipReader arg = Open(respondToStorage, self, path);
				object blockResult;
				block.Yield(arg, out blockResult);
				return blockResult;
			}

			[RubyMethod("close")]
			public static object Close(UnaryOpStorage closeStorage, RubyContext context, GZipReader self)
			{
				GZipFile.Close(closeStorage, self, true);
				return self._ioWrapper.UnderlyingObject;
			}

			[RubyMethod("finish")]
			public static object Finish(UnaryOpStorage closeStorage, RubyContext context, GZipReader self)
			{
				GZipFile.Close(closeStorage, self, false);
				return self._ioWrapper.UnderlyingObject;
			}
		}

		[Serializable]
		[RubyException("Error")]
		public class Error : SystemException
		{
			public Error()
				: this(null, null)
			{
			}

			public Error(string message)
				: this(message, null)
			{
			}

			public Error(string message, Exception inner)
				: base(message ?? "Error", inner)
			{
			}

			protected Error(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyException("DataError")]
		public class DataError : Error
		{
			public DataError()
				: this(null, null)
			{
			}

			public DataError(string message)
				: this(message, null)
			{
			}

			public DataError(string message, Exception inner)
				: base(message ?? "DataError", inner)
			{
			}

			protected DataError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyException("BufError")]
		public class BufError : Error
		{
			public BufError()
				: this(null, null)
			{
			}

			public BufError(string message)
				: this(message, null)
			{
			}

			public BufError(string message, Exception inner)
				: base(message ?? "BufError", inner)
			{
			}

			protected BufError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyException("StreamError")]
		public class StreamError : Error
		{
			public StreamError()
				: this(null, null)
			{
			}

			public StreamError(string message)
				: this(message, null)
			{
			}

			public StreamError(string message, Exception inner)
				: base(message ?? "StreamError", inner)
			{
			}

			protected StreamError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[RubyClass("Deflate", BuildConfig = "!SILVERLIGHT")]
		public class Deflate : ZStream
		{
			internal class ZDeflateStream : DeflateStream
			{
				private long _size;

				private uint _crc;

				private bool _leaveOpen;

				private Stream _output;

				private static readonly uint[] crcTable = new uint[256]
				{
					0u, 1996959894u, 3993919788u, 2567524794u, 124634137u, 1886057615u, 3915621685u, 2657392035u, 249268274u, 2044508324u,
					3772115230u, 2547177864u, 162941995u, 2125561021u, 3887607047u, 2428444049u, 498536548u, 1789927666u, 4089016648u, 2227061214u,
					450548861u, 1843258603u, 4107580753u, 2211677639u, 325883990u, 1684777152u, 4251122042u, 2321926636u, 335633487u, 1661365465u,
					4195302755u, 2366115317u, 997073096u, 1281953886u, 3579855332u, 2724688242u, 1006888145u, 1258607687u, 3524101629u, 2768942443u,
					901097722u, 1119000684u, 3686517206u, 2898065728u, 853044451u, 1172266101u, 3705015759u, 2882616665u, 651767980u, 1373503546u,
					3369554304u, 3218104598u, 565507253u, 1454621731u, 3485111705u, 3099436303u, 671266974u, 1594198024u, 3322730930u, 2970347812u,
					795835527u, 1483230225u, 3244367275u, 3060149565u, 1994146192u, 31158534u, 2563907772u, 4023717930u, 1907459465u, 112637215u,
					2680153253u, 3904427059u, 2013776290u, 251722036u, 2517215374u, 3775830040u, 2137656763u, 141376813u, 2439277719u, 3865271297u,
					1802195444u, 476864866u, 2238001368u, 4066508878u, 1812370925u, 453092731u, 2181625025u, 4111451223u, 1706088902u, 314042704u,
					2344532202u, 4240017532u, 1658658271u, 366619977u, 2362670323u, 4224994405u, 1303535960u, 984961486u, 2747007092u, 3569037538u,
					1256170817u, 1037604311u, 2765210733u, 3554079995u, 1131014506u, 879679996u, 2909243462u, 3663771856u, 1141124467u, 855842277u,
					2852801631u, 3708648649u, 1342533948u, 654459306u, 3188396048u, 3373015174u, 1466479909u, 544179635u, 3110523913u, 3462522015u,
					1591671054u, 702138776u, 2966460450u, 3352799412u, 1504918807u, 783551873u, 3082640443u, 3233442989u, 3988292384u, 2596254646u,
					62317068u, 1957810842u, 3939845945u, 2647816111u, 81470997u, 1943803523u, 3814918930u, 2489596804u, 225274430u, 2053790376u,
					3826175755u, 2466906013u, 167816743u, 2097651377u, 4027552580u, 2265490386u, 503444072u, 1762050814u, 4150417245u, 2154129355u,
					426522225u, 1852507879u, 4275313526u, 2312317920u, 282753626u, 1742555852u, 4189708143u, 2394877945u, 397917763u, 1622183637u,
					3604390888u, 2714866558u, 953729732u, 1340076626u, 3518719985u, 2797360999u, 1068828381u, 1219638859u, 3624741850u, 2936675148u,
					906185462u, 1090812512u, 3747672003u, 2825379669u, 829329135u, 1181335161u, 3412177804u, 3160834842u, 628085408u, 1382605366u,
					3423369109u, 3138078467u, 570562233u, 1426400815u, 3317316542u, 2998733608u, 733239954u, 1555261956u, 3268935591u, 3050360625u,
					752459403u, 1541320221u, 2607071920u, 3965973030u, 1969922972u, 40735498u, 2617837225u, 3943577151u, 1913087877u, 83908371u,
					2512341634u, 3803740692u, 2075208622u, 213261112u, 2463272603u, 3855990285u, 2094854071u, 198958881u, 2262029012u, 4057260610u,
					1759359992u, 534414190u, 2176718541u, 4139329115u, 1873836001u, 414664567u, 2282248934u, 4279200368u, 1711684554u, 285281116u,
					2405801727u, 4167216745u, 1634467795u, 376229701u, 2685067896u, 3608007406u, 1308918612u, 956543938u, 2808555105u, 3495958263u,
					1231636301u, 1047427035u, 2932959818u, 3654703836u, 1088359270u, 936918000u, 2847714899u, 3736837829u, 1202900863u, 817233897u,
					3183342108u, 3401237130u, 1404277552u, 615818150u, 3134207493u, 3453421203u, 1423857449u, 601450431u, 3009837614u, 3294710456u,
					1567103746u, 711928724u, 3020668471u, 3272380065u, 1510334235u, 755167117u
				};

				public ZDeflateStream(Stream output, bool leaveOpen)
					: base(output, CompressionMode.Compress, true)
				{
					_output = output;
					_leaveOpen = leaveOpen;
					byte[] array = new byte[2] { 88, 133 };
					_output.Write(array, 0, array.Length);
				}

				public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
				{
					IAsyncResult result = base.BeginWrite(array, offset, count, asyncCallback, asyncState);
					_size += count;
					_crc = UpdateCrc(_crc, array, offset, count);
					return result;
				}

				public override void Write(byte[] array, int offset, int count)
				{
					base.Write(array, offset, count);
					_size += count;
					_crc = UpdateCrc(_crc, array, offset, count);
				}

				protected override void Dispose(bool disposing)
				{
					base.Dispose(disposing);
					if (disposing && _output != null)
					{
						_output.WriteByte((byte)(_crc & 0xFFu));
						_output.WriteByte((byte)((_crc >> 8) & 0xFFu));
						_output.WriteByte((byte)((_crc >> 16) & 0xFFu));
						_output.WriteByte((byte)((_crc >> 24) & 0xFFu));
						if (!_leaveOpen)
						{
							_output.Close();
						}
						_output = null;
					}
				}

				internal static uint UpdateCrc(uint crc, byte[] buffer, int offset, int length)
				{
					crc ^= 0xFFFFFFFFu;
					while (--length >= 0)
					{
						crc = crcTable[(crc ^ buffer[offset++]) & 0xFF] ^ (crc >> 8);
					}
					crc ^= 0xFFFFFFFFu;
					return crc;
				}
			}

			public Deflate()
				: this(-1, -1, -1, -1)
			{
			}

			public Deflate(int level)
				: this(level, -1, -1, -1)
			{
			}

			public Deflate(int level, int windowBits)
				: this(level, windowBits, -1, -1)
			{
			}

			public Deflate(int level, int windowBits, int memlevel)
				: this(level, windowBits, memlevel, -1)
			{
			}

			public Deflate(int level, int windowBits, int memlevel, int strategy)
			{
			}

			[RubyMethod("deflate")]
			public static MutableString DeflateString(Deflate self, [DefaultProtocol][NotNull] MutableString str, int flush)
			{
				if (flush != 4)
				{
					throw new NotImplementedError("flush can only be FINISH");
				}
				MutableStringStream mutableStringStream = new MutableStringStream(str);
				MutableStringStream mutableStringStream2 = new MutableStringStream();
				ZDeflateStream zDeflateStream = new ZDeflateStream(mutableStringStream2, false);
				int num = str.Length;
				byte[] array = new byte[Math.Min(4096, num)];
				while (num > 0)
				{
					int num2 = mutableStringStream.Read(array, 0, array.Length);
					zDeflateStream.Write(array, 0, num2);
					num -= num2;
				}
				zDeflateStream.Close();
				return mutableStringStream2.String;
			}

			[RubyMethod("deflate", RubyMethodAttributes.PublicSingleton)]
			public static MutableString DeflateString(RubyClass self, [NotNull][DefaultProtocol] MutableString str)
			{
				return DeflateString(new Deflate(), str, 4);
			}
		}

		[RubyClass("GzipWriter", BuildConfig = "!SILVERLIGHT")]
		public class GzipWriter : GZipFile
		{
			private readonly GZipStream _gzipStream;

			private int _level;

			private int _strategy;

			private GzipWriter(RespondToStorage respondToStorage, RubyContext context, IOWrapper ioWrapper, int level, int strategy)
				: base(ioWrapper)
			{
				_level = level;
				_strategy = strategy;
				_gzipStream = new GZipStream(ioWrapper, CompressionMode.Compress, true);
			}

			[RubyConstructor]
			public static GzipWriter Create(RespondToStorage respondToStorage, RubyClass self, object io, int level, int strategy)
			{
				IOWrapper iOWrapper = RubyIOOps.CreateIOWrapper(respondToStorage, io, FileAccess.Write);
				if (iOWrapper == null || !iOWrapper.CanWrite)
				{
					throw RubyExceptions.CreateMethodMissing(self.Context, io, "write");
				}
				return new GzipWriter(respondToStorage, self.Context, iOWrapper, level, strategy);
			}

			[RubyMethod("<<")]
			public static GzipWriter Output(ConversionStorage<MutableString> tosConversion, RubyContext context, GzipWriter self, [NotNull][DefaultProtocol] MutableString str)
			{
				Write(tosConversion, context, self, str);
				return self;
			}

			[RubyMethod("close")]
			public static object Close(UnaryOpStorage closeStorage, RubyContext context, GzipWriter self)
			{
				if (self._isClosed)
				{
					throw new Error("closed gzip stream");
				}
				self._gzipStream.Close();
				self._ioWrapper.Flush();
				GZipFile.Close(closeStorage, self, true);
				return self._ioWrapper.UnderlyingObject;
			}

			[RubyMethod("finish")]
			public static object Finish(UnaryOpStorage closeStorage, RubyContext context, GzipWriter self)
			{
				self._gzipStream.Close();
				self._ioWrapper.Flush(closeStorage, context);
				GZipFile.Close(closeStorage, self, false);
				return self._ioWrapper.UnderlyingObject;
			}

			[RubyMethod("comment=")]
			public static MutableString Comment(GzipWriter self, [NotNull] MutableString comment)
			{
				if (self._isClosed)
				{
					throw new Error("closed gzip stream");
				}
				self._comment = comment;
				return comment;
			}

			[RubyMethod("flush")]
			public static GzipWriter Flush(UnaryOpStorage flushStorage, RubyContext context, GzipWriter self, object flush)
			{
				if (flush != null)
				{
					throw RubyExceptions.CreateUnexpectedTypeError(context, flush, "Fixnum");
				}
				return Flush(flushStorage, context, self, 2);
			}

			[RubyMethod("flush")]
			public static GzipWriter Flush(UnaryOpStorage flushStorage, RubyContext context, GzipWriter self, int flush)
			{
				switch (flush)
				{
				case 0:
				case 2:
				case 3:
				case 4:
					self._gzipStream.Flush();
					self._ioWrapper.Flush(flushStorage, context);
					return self;
				default:
					throw new StreamError("stream error");
				}
			}

			[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
			public static object Open(RespondToStorage respondToStorage, UnaryOpStorage closeStorage, BlockParam block, RubyClass self, [NotNull] MutableString filename, int level, int strategy)
			{
				RubyFile io = new RubyFile(self.Context, filename.ConvertToString(), IOMode.WriteOnly | IOMode.CreateIfNotExists | IOMode.Truncate | IOMode.PreserveEndOfLines);
				GzipWriter gzipWriter = Create(respondToStorage, self, io, level, strategy);
				if (block == null)
				{
					return gzipWriter;
				}
				try
				{
					object blockResult;
					block.Yield(gzipWriter, out blockResult);
					return blockResult;
				}
				finally
				{
					Close(closeStorage, self.Context, gzipWriter);
				}
			}

			[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
			public static object Open(RespondToStorage respondToStorage, UnaryOpStorage closeStorage, BlockParam block, RubyClass self, [NotNull] MutableString filename, object level, object strategy)
			{
				if (level != null)
				{
					throw RubyExceptions.CreateUnexpectedTypeError(self.Context, level, "Fixnum");
				}
				if (strategy != null)
				{
					throw RubyExceptions.CreateUnexpectedTypeError(self.Context, strategy, "Fixnum");
				}
				return Open(respondToStorage, closeStorage, block, self, filename, 0, 0);
			}

			[RubyMethod("orig_name=")]
			public static MutableString OriginalName(GzipWriter self, [NotNull] MutableString originalName)
			{
				if (self._isClosed)
				{
					throw new Error("closed gzip stream");
				}
				self._originalName = originalName;
				return originalName;
			}

			[RubyMethod("write")]
			public static int Write(ConversionStorage<MutableString> tosConversion, RubyContext context, GzipWriter self, [NotNull][DefaultProtocol] MutableString str)
			{
				byte[] array = str.ToByteArray();
				self._gzipStream.Write(array, 0, array.Length);
				return array.Length;
			}
		}

		[RubyConstant("NO_FLUSH")]
		public const int NO_FLUSH = 0;

		[RubyConstant("SYNC_FLUSH")]
		public const int SYNC_FLUSH = 2;

		[RubyConstant("FULL_FLUSH")]
		public const int FULL_FLUSH = 3;

		[RubyConstant("FINISH")]
		public const int FINISH = 4;

		[RubyConstant("MAXBITS")]
		public const int MAXBITS = 15;

		[RubyConstant("MAXLCODES")]
		public const int MAXLCODES = 286;

		[RubyConstant("MAXDCODES")]
		public const int MAXDCODES = 30;

		[RubyConstant("MAXCODES")]
		public const int MAXCODES = 316;

		[RubyConstant("FIXLCODES")]
		public const int FIXLCODES = 288;

		[RubyConstant("MAX_WBITS")]
		public const int MAX_WBITS = 15;

		[RubyConstant("Z_DEFLATED")]
		public const int Z_DEFLATED = 8;

		[RubyConstant("BINARY")]
		public const int BINARY = 0;

		[RubyConstant("ASCII")]
		public const int ASCII = 1;

		[RubyConstant("UNKNOWN")]
		public const int UNKNOWN = 2;

		[RubyConstant("NO_COMPRESSION")]
		public const int NO_COMPRESSION = 0;

		[RubyConstant("BEST_SPEED")]
		public const int BEST_SPEED = 1;

		[RubyConstant("BEST_COMPRESSION")]
		public const int BEST_COMPRESSION = 9;

		[RubyConstant("DEFAULT_COMPRESSION")]
		public const int DEFAULT_COMPRESSION = -1;

		[RubyConstant("FILTERED")]
		public const int FILTERED = 1;

		[RubyConstant("HUFFMAN_ONLY")]
		public const int HUFFMAN_ONLY = 2;

		[RubyConstant("DEFAULT_STRATEGY")]
		public const int DEFAULT_STRATEGY = 0;

		[RubyConstant("ZLIB_VERSION")]
		public static string ZLIB_VERSION = "1.2.3";

		[RubyConstant("VERSION")]
		public static string VERSION = "0.6.0";

		[RubyMethod("crc32", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static int GetCrc(RubyModule self)
		{
			return 0;
		}

		[RubyMethod("crc32", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static object GetCrc(RubyModule self, [Optional][DefaultProtocol] MutableString str, [Optional] int initialCrc)
		{
			byte[] array = ((str != null) ? str.ToByteArray() : new byte[0]);
			uint x = Deflate.ZDeflateStream.UpdateCrc((uint)initialCrc, array, 0, array.Length);
			return Protocols.Normalize(x);
		}
	}
}
