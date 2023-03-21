using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
    public static class RubyEncoder
    {
        private struct FormatDirective
        {
            internal readonly char Directive;

            internal readonly int? Count;

            internal FormatDirective(char directive, int? count)
            {
                Directive = directive;
                Count = count;
            }

            private static char MapNative(char c, char modifier)
            {
                switch (c)
                {
                    case 'I':
                    case 'S':
                    case 'i':
                    case 's':
                        return c;
                    case 'l':
                        if (IntPtr.Size != 4)
                        {
                            return 'q';
                        }
                        return 'i';
                    case 'L':
                        if (IntPtr.Size != 4)
                        {
                            return 'Q';
                        }
                        return 'I';
                    default:
                        throw RubyExceptions.CreateArgumentError("'{0}' allowed only after types sSiIlL", modifier);
                }
            }

            internal static IEnumerable<FormatDirective>/*!*/ Enumerate(string/*!*/ format)
            {
                for (int i = 0; i < format.Length; i++)
                {
                    char c = format[i];
                    if (c == '%')
                    {
                        throw RubyExceptions.CreateArgumentError("% is not supported");
                    }

                    if (c == '#')
                    {
                        do { i++; } while (i < format.Length && format[i] != '\n');
                        continue;
                    }

                    if (!Tokenizer.IsLetter(c) && c != '@')
                    {
                        continue;
                    }

                    i++;
                    int? count = 1;
                    char c2 = (i < format.Length) ? format[i] : '\0';
                    if (c2 == '_' || c2 == '!')
                    {
                        char mapped = MapNative(c, c2);

                        // ignore !
                        if (c2 == '_')
                        {
                            c = mapped;
                        }
                        i++;
                        c2 = (i < format.Length) ? format[i] : '\0';
                    }

                    if (Tokenizer.IsDecimalDigit(c2))
                    {
                        int pos1 = i;
                        i++;
                        while (i < format.Length && Tokenizer.IsDecimalDigit(format[i]))
                        {
                            i++;
                        }
                        count = Int32.Parse(format.Substring(pos1, (i - pos1)));
                        i--;
                    }
                    else if (c == '@' && c2 == '-')
                    {
                        int pos1 = i;
                        i += 2;
                        while (i < format.Length && Tokenizer.IsDecimalDigit(format[i]))
                        {
                            i++;
                        }
                        count = Int32.Parse(format.Substring(pos1, (i - pos1)));
                        i--;
                    }
                    else if (c2 == '*')
                    {
                        count = null;
                    }
                    else
                    {
                        i--;
                        if (c == '@')
                        {
                            count = 0;
                        }
                    }

                    yield return new FormatDirective(c, count);
                }
            }
        }

        private const char UUEncodeZero = '`';

        private static byte[] _Base64Table = new byte[64]
        {
            65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
            75, 76, 77, 78, 79, 80, 81, 82, 83, 84,
            85, 86, 87, 88, 89, 90, 97, 98, 99, 100,
            101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
            111, 112, 113, 114, 115, 116, 117, 118, 119, 120,
            121, 122, 48, 49, 50, 51, 52, 53, 54, 55,
            56, 57, 43, 47
        };

        private static void GetCodePointByteCount(int codepoint, out int count, out int mark)
        {
            if (codepoint < 128)
            {
                count = 1;
                mark = 0;
            }
            else if (codepoint < 2048)
            {
                count = 2;
                mark = 192;
            }
            else if (codepoint < 65536)
            {
                count = 3;
                mark = 224;
            }
            else if (codepoint < 2097152)
            {
                count = 4;
                mark = 240;
            }
            else if (codepoint < 67108864)
            {
                count = 5;
                mark = 248;
            }
            else
            {
                count = 6;
                mark = 252;
            }
        }

        private static void WriteUtf8CodePoint(Stream stream, int codepoint)
        {
            if (codepoint < 0)
            {
                throw RubyExceptions.CreateRangeError("pack(U): value out of range");
            }
            int count;
            int mark;
            GetCodePointByteCount(codepoint, out count, out mark);
            stream.WriteByte((byte)((codepoint >> 6 * (count - 1)) | mark));
            switch (count)
            {
                default:
                    return;
                case 6:
                    stream.WriteByte((byte)(((uint)(codepoint >> 24) & 0x3Fu) | 0x80u));
                    goto case 5;
                case 5:
                    stream.WriteByte((byte)(((uint)(codepoint >> 18) & 0x3Fu) | 0x80u));
                    goto case 4;
                case 4:
                    stream.WriteByte((byte)(((uint)(codepoint >> 12) & 0x3Fu) | 0x80u));
                    goto case 3;
                case 3:
                    stream.WriteByte((byte)(((uint)(codepoint >> 6) & 0x3Fu) | 0x80u));
                    break;
                case 2:
                    break;
            }
            stream.WriteByte((byte)(((uint)codepoint & 0x3Fu) | 0x80u));
        }

        private static int ReadUtf8CodePoint(MutableString data, ref int index)
        {
            int byteCount = data.GetByteCount();
            if (index >= byteCount)
            {
                return -1;
            }
            int @byte = data.GetByte(index++);
            int num;
            int num2;
            if ((@byte & 0x80) == 0)
            {
                num = 1;
                num2 = 255;
            }
            else if ((@byte & 0xE0) == 192)
            {
                num = 2;
                num2 = 31;
            }
            else if ((@byte & 0xF0) == 224)
            {
                num = 3;
                num2 = 15;
            }
            else if ((@byte & 0xF8) == 240)
            {
                num = 4;
                num2 = 7;
            }
            else if ((@byte & 0xFC) == 248)
            {
                num = 5;
                num2 = 3;
            }
            else
            {
                if ((@byte & 0xFE) != 252)
                {
                    throw RubyExceptions.CreateArgumentError("malformed UTF-8 character");
                }
                num = 6;
                num2 = 1;
            }
            int num3 = @byte & num2;
            for (int i = 1; i < num; i++)
            {
                if (index >= byteCount)
                {
                    throw RubyExceptions.CreateArgumentError("malformed UTF-8 character (expected {0} bytes, given {1} bytes)", num, i);
                }
                @byte = data.GetByte(index++);
                if ((@byte & 0xC0) != 128)
                {
                    throw RubyExceptions.CreateArgumentError("malformed UTF-8 character");
                }
                num3 = (num3 << 6) | (@byte & 0x3F);
            }
            int count;
            int mark;
            GetCodePointByteCount(num3, out count, out mark);
            if (count != num)
            {
                throw RubyExceptions.CreateArgumentError("redundant UTF-8 sequence");
            }
            return num3;
        }

        private static void WriteBer(Stream stream, IntegerValue value)
        {
            if (value.IsFixnum)
            {
                if (value.Fixnum < 0)
                {
                    throw RubyExceptions.CreateArgumentError("pack(w): value out of range");
                }
                int fixnum = value.Fixnum;
                bool flag = false;
                for (int num = 28; num > 0; num -= 7)
                {
                    int num2 = (fixnum >> num) & 0x7F;
                    if (num2 != 0 || flag)
                    {
                        stream.WriteByte((byte)((uint)num2 | 0x80u));
                        flag = true;
                    }
                }
                stream.WriteByte((byte)((uint)fixnum & 0x7Fu));
                return;
            }
            BigInteger bignum = value.Bignum;
            if (bignum.Sign < 0)
            {
                throw RubyExceptions.CreateArgumentError("pack(w): value out of range");
            }
            uint[] words = bignum.GetWords();
            int num3 = words.Length;
            uint num4 = 0u;
            bool flag2 = false;
            int num5 = (Ceil(32 * num3, 7) - 7) % 32;
            uint num6 = words[--num3];
            while (num5 != 0 || num3 != 0)
            {
                uint num7 = num4 | ((num6 >> num5) & 0x7Fu);
                num4 = 0u;
                flag2 = flag2 || num7 != 0;
                if (flag2)
                {
                    stream.WriteByte((byte)(num7 | 0x80u));
                }
                if (num5 < 7)
                {
                    num4 = (num6 & (uint)((1 << num5) - 1)) << 7 - num5;
                    num6 = words[--num3];
                }
                num5 = (num5 + 32 - 7) % 32;
            }
            stream.WriteByte((byte)(num6 & 0x7Fu));
        }

        private static object ReadBer(MutableString data, ref int index)
        {
            int i = index;
            try
            {
                for (; data.GetByte(i) == 128; i++)
                {
                }
                index = i;
                for (; (data.GetByte(i) & 0x80u) != 0; i++)
                {
                }
            }
            catch (IndexOutOfRangeException)
            {
                index = i;
                return null;
            }
            int num = i - index + 1;
            if (num <= 9)
            {
                long num2 = 0L;
                i = index;
                int num3 = 0;
                while (num3 < num)
                {
                    num2 = (num2 << 7) | ((long)data.GetByte(i) & 0x7FL);
                    num3++;
                    i++;
                }
                index = i;
                return Protocols.Normalize(num2);
            }
            uint[] array = new uint[CeilDiv(num * 7, 32)];
            int num4 = 0;
            int num5 = 0;
            long num6 = 0L;
            int num7 = 0;
            while (num7 < num)
            {
                num6 |= (long)(((ulong)data.GetByte(i) & 0x7FuL) << num4);
                if (num4 >= 25)
                {
                    array[num5++] = (uint)(num6 & 0xFFFFFFFFu);
                    num6 >>= 32;
                }
                num4 = (num4 + 7) % 32;
                num7++;
                i--;
            }
            if (num6 > 0)
            {
                array[num5] |= (uint)(int)num6;
            }
            index += num;
            return new BigInteger(1, array);
        }

        public static void WriteBase64(Stream stream, MutableString str, int bytesPerLine)
        {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(str, "str");
            ContractUtils.Requires(bytesPerLine > 2, "bytesPerLine");
            bytesPerLine -= bytesPerLine % 3;
            int byteCount = str.GetByteCount();
            int num = byteCount % 3;
            int num2 = byteCount - num;
            byte[] base64Table = _Base64Table;
            int num3 = 0;
            int @byte;
            for (int i = 0; i < num2; i += 3)
            {
                @byte = str.GetByte(i);
                int byte2 = str.GetByte(i + 1);
                int byte3 = str.GetByte(i + 2);
                stream.WriteByte(base64Table[(@byte & 0xFC) >> 2]);
                stream.WriteByte(base64Table[((@byte & 3) << 4) | ((byte2 & 0xF0) >> 4)]);
                stream.WriteByte(base64Table[((byte2 & 0xF) << 2) | ((byte3 & 0xC0) >> 6)]);
                stream.WriteByte(base64Table[byte3 & 0x3F]);
                num3 += 3;
                if (num3 == bytesPerLine)
                {
                    stream.WriteByte(10);
                    num3 = 0;
                }
            }
            if (num == 0)
            {
                if (num3 != 0)
                {
                    stream.WriteByte(10);
                }
                return;
            }
            @byte = str.GetByte(num2);
            stream.WriteByte(base64Table[(@byte & 0xFC) >> 2]);
            switch (num)
            {
                case 1:
                    stream.WriteByte(base64Table[(@byte & 3) << 4]);
                    stream.WriteByte(61);
                    break;
                case 2:
                    {
                        int byte2 = str.GetByte(num2 + 1);
                        stream.WriteByte(base64Table[((@byte & 3) << 4) | ((byte2 & 0xF0) >> 4)]);
                        stream.WriteByte(base64Table[(byte2 & 0xF) << 2]);
                        break;
                    }
            }
            stream.WriteByte(61);
            stream.WriteByte(10);
        }

        private static MutableString ReadBase64(MutableString data, ref int offset)
        {
            int byteCount = data.GetByteCount();
            MutableString mutableString = MutableString.CreateBinary();
            while (true)
            {
                int num = DecodeBase64Byte(data, byteCount, true, ref offset);
                int num2 = DecodeBase64Byte(data, byteCount, true, ref offset);
                int num3 = DecodeBase64Byte(data, byteCount, false, ref offset);
                int num4 = ((num3 != -2) ? DecodeBase64Byte(data, byteCount, false, ref offset) : (-2));
                if (num == -1 || num2 == -1 || num3 == -1 || num4 == -1)
                {
                    break;
                }
                int num5 = (num << 18) | (num2 << 12);
                mutableString.Append((byte)((uint)(num5 >> 16) & 0xFFu));
                if (num3 == -2)
                {
                    break;
                }
                num5 |= num3 << 6;
                mutableString.Append((byte)((uint)(num5 >> 8) & 0xFFu));
                if (num4 == -2)
                {
                    break;
                }
                num5 |= num4;
                mutableString.Append((byte)((uint)num5 & 0xFFu));
            }
            return mutableString;
        }

        private static int DecodeBase64Byte(MutableString data, int length, bool skipEquals, ref int offset)
        {
            while (offset < length)
            {
                int @byte = data.GetByte(offset++);
                switch (@byte)
                {
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                    case 71:
                    case 72:
                    case 73:
                    case 74:
                    case 75:
                    case 76:
                    case 77:
                    case 78:
                    case 79:
                    case 80:
                    case 81:
                    case 82:
                    case 83:
                    case 84:
                    case 85:
                    case 86:
                    case 87:
                    case 88:
                    case 89:
                    case 90:
                        return @byte - 65;
                    case 97:
                    case 98:
                    case 99:
                    case 100:
                    case 101:
                    case 102:
                    case 103:
                    case 104:
                    case 105:
                    case 106:
                    case 107:
                    case 108:
                    case 109:
                    case 110:
                    case 111:
                    case 112:
                    case 113:
                    case 114:
                    case 115:
                    case 116:
                    case 117:
                    case 118:
                    case 119:
                    case 120:
                    case 121:
                    case 122:
                        return @byte - 97 + 90 - 65 + 1;
                    case 48:
                    case 49:
                    case 50:
                    case 51:
                    case 52:
                    case 53:
                    case 54:
                    case 55:
                    case 56:
                    case 57:
                        return @byte - 48 + 52;
                }
                switch (@byte)
                {
                    case 43:
                        return 62;
                    case 47:
                        return 63;
                    case 61:
                        if (!skipEquals)
                        {
                            offset--;
                            return -2;
                        }
                        break;
                }
            }
            return -1;
        }

        private static void WritePrintedQuotable(Stream stream, MutableString str, int bytesPerLine)
        {
            bytesPerLine++;
            int num = 0;
            int byteCount = str.GetByteCount();
            for (int i = 0; i < byteCount; i++)
            {
                byte @byte = str.GetByte(i);
                if ((@byte >= 33 && @byte <= 60) || (@byte >= 62 && @byte <= 126) || @byte == 9 || @byte == 32)
                {
                    stream.WriteByte(@byte);
                    num++;
                }
                else if (@byte == 10)
                {
                    stream.WriteByte(@byte);
                    num = 0;
                }
                else
                {
                    stream.WriteByte(61);
                    stream.WriteByte((byte)(@byte >> 4).ToUpperHexDigit());
                    stream.WriteByte((byte)(@byte & 0xF).ToUpperHexDigit());
                    num += 3;
                }
                if (num >= bytesPerLine)
                {
                    stream.WriteByte(61);
                    stream.WriteByte(10);
                    num = 0;
                }
            }
            if (num > 0)
            {
                stream.WriteByte(61);
                stream.WriteByte(10);
            }
        }

        private static MutableString ReadQuotedPrintable(MutableString data, ref int index)
        {
            MutableString mutableString = MutableString.CreateBinary();
            int byteCount = data.GetByteCount();
            int num = index;
            while (num < byteCount)
            {
                byte @byte = data.GetByte(num++);
                if (@byte == 61)
                {
                    if (num >= byteCount)
                    {
                        break;
                    }
                    @byte = data.GetByte(num);
                    switch (@byte)
                    {
                        case 10:
                            num++;
                            continue;
                        case 13:
                            if (num + 1 < byteCount && data.GetByte(num + 1) == 10)
                            {
                                num += 2;
                                continue;
                            }
                            break;
                    }
                    int num2 = Tokenizer.ToDigit(@byte);
                    if (num2 >= 16)
                    {
                        break;
                    }
                    num++;
                    if (num >= byteCount)
                    {
                        break;
                    }
                    int num3 = Tokenizer.ToDigit(data.GetByte(num));
                    if (num3 >= 16)
                    {
                        break;
                    }
                    num++;
                    mutableString.Append((byte)((num2 << 4) | num3));
                }
                else
                {
                    mutableString.Append(@byte);
                }
            }
            index = num;
            return mutableString;
        }

        private static void EncodeUU(byte[] input, int bytesPerLine, Stream output)
        {
            ContractUtils.RequiresNotNull(input, "input");
            ContractUtils.RequiresNotNull(output, "output");
            if (input.Length == 0)
            {
                return;
            }
            bytesPerLine -= bytesPerLine % 3;
            int num = input.Length % bytesPerLine;
            int num2 = input.Length / bytesPerLine;
            int num3 = 0;
            for (int i = 0; i < num2; i++)
            {
                output.WriteByte(EncodeUUByte(bytesPerLine));
                for (int j = 0; j < bytesPerLine / 3; j++)
                {
                    EncodeUUTriple(output, input[num3], input[num3 + 1], input[num3 + 2]);
                    num3 += 3;
                }
                output.WriteByte(10);
            }
            if (num > 0)
            {
                output.WriteByte(EncodeUUByte(num));
                int num4 = num / 3;
                num %= 3;
                for (int k = 0; k < num4; k++)
                {
                    EncodeUUTriple(output, input[num3], input[num3 + 1], input[num3 + 2]);
                    num3 += 3;
                }
                switch (num)
                {
                    case 1:
                        EncodeUUTriple(output, input[num3], 0, 0);
                        break;
                    case 2:
                        EncodeUUTriple(output, input[num3], input[num3 + 1], 0);
                        break;
                }
                output.WriteByte(10);
            }
        }

        private static MutableString ReadUU(MutableString data, ref int position)
        {
            MutableStringStream mutableStringStream = new MutableStringStream(data);
            MutableStringStream mutableStringStream2 = new MutableStringStream();
            mutableStringStream.Position = position;
            ReadUU(mutableStringStream, mutableStringStream2);
            position = (int)mutableStringStream.Position;
            mutableStringStream2.Close();
            return mutableStringStream2.String;
        }

        private static bool ReadUU(Stream input, Stream output)
        {
            do
            {
                int num = input.ReadByte();
                if (num == -1)
                {
                    return true;
                }
                num = DecodeUUByte(num);
                int num2 = num % 3;
                int num3 = num / 3;
                for (int i = 0; i < num3; i++)
                {
                    int num4 = DecodeUUByte(input.ReadByte());
                    int num5 = DecodeUUByte(input.ReadByte());
                    int num6 = DecodeUUByte(input.ReadByte());
                    int num7 = input.ReadByte();
                    if (num7 == -1)
                    {
                        return false;
                    }
                    num7 = DecodeUUByte(num7);
                    output.WriteByte((byte)((uint)((num4 << 2) | (num5 >> 4)) & 0xFFu));
                    output.WriteByte((byte)((uint)((num5 << 4) | (num6 >> 2)) & 0xFFu));
                    output.WriteByte((byte)((uint)((num6 << 6) | num7) & 0xFFu));
                }
                if (num2 > 0)
                {
                    int num8 = DecodeUUByte(input.ReadByte());
                    int num9 = DecodeUUByte(input.ReadByte());
                    int num10 = DecodeUUByte(input.ReadByte());
                    int num11 = input.ReadByte();
                    if (num11 == -1)
                    {
                        return false;
                    }
                    num11 = DecodeUUByte(num11);
                    output.WriteByte((byte)((num8 << 2) | (num9 >> 4)));
                    if (num2 == 2)
                    {
                        output.WriteByte((byte)((num9 << 4) | (num10 >> 2)));
                    }
                }
            }
            while (input.ReadByte() == 10);
            return false;
        }

        private static void EncodeUUTriple(Stream output, int a, int b, int c)
        {
            output.WriteByte(EncodeUUByte(a >> 2));
            output.WriteByte(EncodeUUByte(((a << 4) | (b >> 4)) & 0x3F));
            output.WriteByte(EncodeUUByte(((b << 2) | (c >> 6)) & 0x3F));
            output.WriteByte(EncodeUUByte(c & 0x3F));
        }

        private static byte EncodeUUByte(int b)
        {
            return (byte)((b == 0) ? 96u : ((uint)(32 + b)));
        }

        private static byte DecodeUUByte(int c)
        {
            return (byte)((uint)(c - 32) & 0x3Fu);
        }

        private static void WriteBits(Stream stream, int? countDef, bool reverse, MutableString str)
        {
            int byteCount = str.GetByteCount();
            int num = countDef ?? byteCount;
            int num2 = Math.Min(num, byteCount);
            int num3 = (8 - num2 % 8) % 8;
            long num4 = stream.Length + (num2 + num3) / 8 + (num - num2 + (num - num2) % 2) / 2;
            stream.SetLength(num4);
            int num5 = 0;
            int num6 = 0;
            int num7 = ((!reverse) ? 7 : 0);
            int num8 = (reverse ? 1 : (-1));
            while (num6 < num2)
            {
                num5 |= (str.GetByte(num6++) & 1) << num7;
                num7 += num8;
                if (num6 % 8 == 0)
                {
                    stream.WriteByte((byte)num5);
                    num5 = 0;
                    num7 = ((!reverse) ? 7 : 0);
                }
            }
            if (num3 > 0)
            {
                stream.WriteByte((byte)num5);
            }
            stream.Position = num4;
        }

        private static MutableString ReadBits(MutableString data, int? bitCount, ref int offset, bool lowestFirst)
        {
            int num = data.GetByteCount() - offset;
            int num2 = num * 8;
            if (bitCount.HasValue)
            {
                int num3 = CeilDiv(bitCount.Value, 8);
                if (num3 <= num)
                {
                    num = num3;
                    num2 = bitCount.Value;
                }
            }
            MutableString mutableString = MutableString.CreateBinary(num2);
            if (num2 == 0)
            {
                return mutableString;
            }
            while (true)
            {
                int @byte = data.GetByte(offset++);
                for (int i = 0; i < 8; i++)
                {
                    mutableString.Append((byte)(48 + ((@byte >> (lowestFirst ? i : (7 - i))) & 1)));
                    if (--num2 == 0)
                    {
                        return mutableString;
                    }
                }
            }
        }

        private static int Ceil(int n, int d)
        {
            return CeilDiv(n, d) * d;
        }

        private static int CeilDiv(int n, int d)
        {
            return (n + d - 1) / d;
        }

        private static ulong ReadUInt64(MutableString data, ref int index, bool swap)
        {
            int num = index;
            index += 8;
            if (swap)
            {
                return ((ulong)data.GetByte(num) << 56) | ((ulong)data.GetByte(num + 1) << 48) | ((ulong)data.GetByte(num + 2) << 40) | ((ulong)data.GetByte(num + 3) << 32) | ((ulong)data.GetByte(num + 4) << 24) | ((ulong)data.GetByte(num + 5) << 16) | ((ulong)data.GetByte(num + 6) << 8) | data.GetByte(num + 7);
            }
            return ((ulong)data.GetByte(num + 7) << 56) | ((ulong)data.GetByte(num + 6) << 48) | ((ulong)data.GetByte(num + 5) << 40) | ((ulong)data.GetByte(num + 4) << 32) | ((ulong)data.GetByte(num + 3) << 24) | ((ulong)data.GetByte(num + 2) << 16) | ((ulong)data.GetByte(num + 1) << 8) | data.GetByte(num);
        }

        internal static void Write(Stream stream, ulong n, bool swap)
        {
            if (swap)
            {
                stream.WriteByte((byte)(n >> 56));
                stream.WriteByte((byte)((n >> 48) & 0xFF));
                stream.WriteByte((byte)((n >> 40) & 0xFF));
                stream.WriteByte((byte)((n >> 32) & 0xFF));
                stream.WriteByte((byte)((n >> 24) & 0xFF));
                stream.WriteByte((byte)((n >> 16) & 0xFF));
                stream.WriteByte((byte)((n >> 8) & 0xFF));
                stream.WriteByte((byte)(n & 0xFF));
            }
            else
            {
                stream.WriteByte((byte)(n & 0xFF));
                stream.WriteByte((byte)((n >> 8) & 0xFF));
                stream.WriteByte((byte)((n >> 16) & 0xFF));
                stream.WriteByte((byte)((n >> 24) & 0xFF));
                stream.WriteByte((byte)((n >> 32) & 0xFF));
                stream.WriteByte((byte)((n >> 40) & 0xFF));
                stream.WriteByte((byte)((n >> 48) & 0xFF));
                stream.WriteByte((byte)(n >> 56));
            }
        }

        private static uint ReadUInt32(MutableString data, ref int index, bool swap)
        {
            int num = index;
            index += 4;
            if (swap)
            {
                return (uint)((data.GetByte(num) << 24) | (data.GetByte(num + 1) << 16) | (data.GetByte(num + 2) << 8) | data.GetByte(num + 3));
            }
            return (uint)((data.GetByte(num + 3) << 24) | (data.GetByte(num + 2) << 16) | (data.GetByte(num + 1) << 8) | data.GetByte(num));
        }

        internal static void Write(Stream stream, uint n, bool swap)
        {
            if (swap)
            {
                stream.WriteByte((byte)(n >> 24));
                stream.WriteByte((byte)((n >> 16) & 0xFFu));
                stream.WriteByte((byte)((n >> 8) & 0xFFu));
                stream.WriteByte((byte)(n & 0xFFu));
            }
            else
            {
                stream.WriteByte((byte)(n & 0xFFu));
                stream.WriteByte((byte)((n >> 8) & 0xFFu));
                stream.WriteByte((byte)((n >> 16) & 0xFFu));
                stream.WriteByte((byte)(n >> 24));
            }
        }

        private static ushort ReadUInt16(MutableString data, ref int index, bool swap)
        {
            int num = index;
            index += 2;
            if (swap)
            {
                return (ushort)((data.GetByte(num) << 8) | data.GetByte(num + 1));
            }
            return (ushort)((data.GetByte(num + 1) << 8) | data.GetByte(num));
        }

        private static void Write(Stream stream, ushort n, bool swap)
        {
            if (swap)
            {
                stream.WriteByte((byte)((uint)(n >> 8) & 0xFFu));
                stream.WriteByte((byte)(n & 0xFFu));
            }
            else
            {
                stream.WriteByte((byte)(n & 0xFFu));
                stream.WriteByte((byte)((uint)(n >> 8) & 0xFFu));
            }
        }

        private static double Int64BitsToDouble(long value)
        {
            return BitConverter.Int64BitsToDouble(value);
        }

        private static long DoubleToInt64Bits(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        private static double ReadDouble(MutableString data, ref int index, bool swap)
        {
            return Int64BitsToDouble((long)ReadUInt64(data, ref index, swap));
        }

        private static void WriteDouble(ConversionStorage<double> floatConversion, Stream stream, RubyArray self, int i, int count, bool swap)
        {
            for (int j = 0; j < count; j++)
            {
                Write(stream, (ulong)DoubleToInt64Bits(Protocols.CastToFloat(floatConversion, GetPackArg(self, i + j))), swap);
            }
        }

        private static float ReadSingle(MutableString data, ref int index, bool swap)
        {
            byte[] binarySlice = data.GetBinarySlice(index, 4);
            if (swap)
            {
                byte b = binarySlice[0];
                binarySlice[0] = binarySlice[3];
                binarySlice[3] = b;
                b = binarySlice[1];
                binarySlice[1] = binarySlice[2];
                binarySlice[2] = b;
            }
            index += 4;
            return BitConverter.ToSingle(binarySlice, 0);
        }

        private static void WriteSingle(ConversionStorage<double> floatConversion, Stream stream, RubyArray self, int i, int count, bool swap)
        {
            for (int j = 0; j < count; j++)
            {
                byte[] bytes = BitConverter.GetBytes((float)Protocols.CastToFloat(floatConversion, GetPackArg(self, i + j)));
                if (swap)
                {
                    stream.WriteByte(bytes[3]);
                    stream.WriteByte(bytes[2]);
                    stream.WriteByte(bytes[1]);
                    stream.WriteByte(bytes[0]);
                }
                else
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private static void WriteUInt64(ConversionStorage<IntegerValue> integerConversion, Stream stream, RubyArray self, int i, int count, bool swap)
        {
            for (int j = 0; j < count; j++)
            {
                object packArg = GetPackArg(self, i + j);
                if (packArg == null)
                {
                    throw RubyExceptions.CreateTypeError("no implicit conversion from nil to integer");
                }
                IntegerValue integerValue = Protocols.CastToInteger(integerConversion, packArg);
                if (integerValue.IsFixnum)
                {
                    Write(stream, (ulong)integerValue.Fixnum, swap);
                    continue;
                }
                ulong ret;
                if (integerValue.Bignum.Abs().AsUInt64(out ret))
                {
                    if (integerValue.Bignum.Sign < 0)
                    {
                        ret = ~ret + 1;
                    }
                    Write(stream, ret, swap);
                    continue;
                }
                throw RubyExceptions.CreateRangeError("bignum out of range (-2**64, 2**64)");
            }
        }

        private static void WriteUInt32(ConversionStorage<IntegerValue> integerConversion, Stream stream, RubyArray self, int i, int count, bool swap)
        {
            for (int j = 0; j < count; j++)
            {
                long num = Protocols.CastToInt64Unchecked(integerConversion, GetPackArg(self, i + j));
                if (num <= -4294967296L || num >= 4294967296L)
                {
                    throw RubyExceptions.CreateRangeError("bignum out of range (-2**32, 2**32)");
                }
                Write(stream, (uint)num, swap);
            }
        }

        private static void WriteUInt16(ConversionStorage<IntegerValue> integerConversion, Stream stream, RubyArray self, int i, int count, bool swap)
        {
            for (int j = 0; j < count; j++)
            {
                long num = Protocols.CastToInt64Unchecked(integerConversion, GetPackArg(self, i + j));
                if (num <= -4294967296L || num >= 4294967296L)
                {
                    throw RubyExceptions.CreateRangeError("bignum out of range (-2**32, 2**32)");
                }
                Write(stream, (ushort)num, swap);
            }
        }

        private static void FromHex(Stream stream, MutableString str, int nibbleCount, bool swap)
        {
            int num = Math.Min(nibbleCount, str.GetByteCount());
            int num2 = 0;
            int num3 = 0;
            while (num2 < (nibbleCount + 1) / 2)
            {
                int num4 = ((num3 < num) ? FromHexDigit(str.GetByte(num3)) : 0);
                int num5 = ((num3 + 1 < num) ? FromHexDigit(str.GetByte(num3 + 1)) : 0);
                int num6 = (swap ? ((num5 << 4) | num4) : ((num4 << 4) | num5));
                stream.WriteByte((byte)num6);
                num2++;
                num3 += 2;
            }
        }

        private static int FromHexDigit(int c)
        {
            c = Tokenizer.ToDigit(c);
            if (c < 16)
            {
                return c;
            }
            throw new NotSupportedException("directives `H' and `h' expect hexadecimal digits in input string");
        }

        public static MutableString Pack(ConversionStorage<IntegerValue> integerConversion, ConversionStorage<double> floatConversion, ConversionStorage<MutableString> stringCast, ConversionStorage<MutableString> tosConversion, RubyArray self, [DefaultProtocol][NotNull] MutableString format)
        {
            using (MutableStringStream mutableStringStream = new MutableStringStream())
            {
                int num = 0;
                foreach (FormatDirective item in FormatDirective.Enumerate(format.ConvertToString()))
                {
                    int num2 = item.Count ?? (self.Count - num);
                    switch (item.Directive)
                    {
                        case '@':
                            {
                                num2 = 0;
                                long length = (mutableStringStream.Position = ((!item.Count.HasValue) ? 1 : item.Count.Value));
                                mutableStringStream.SetLength(length);
                                break;
                            }
                        case 'A':
                        case 'Z':
                        case 'a':
                            num2 = 1;
                            WriteString(mutableStringStream, item, ToMutableString(stringCast, mutableStringStream, GetPackArg(self, num)));
                            break;
                        case 'B':
                        case 'b':
                            {
                                num2 = 1;
                                MutableString mutableString;
                                WriteBits(mutableStringStream, item.Count, item.Directive == 'b', mutableString = ((GetPackArg(self, num) != null) ? ToMutableString(stringCast, mutableStringStream, GetPackArg(self, num)) : MutableString.FrozenEmpty));
                                break;
                            }
                        case 'C':
                        case 'c':
                            {
                                for (int i = 0; i < num2; i++)
                                {
                                    mutableStringStream.WriteByte((byte)Protocols.CastToUInt32Unchecked(integerConversion, GetPackArg(self, num + i)));
                                }
                                break;
                            }
                        case 'D':
                        case 'd':
                            WriteDouble(floatConversion, mutableStringStream, self, num, num2, false);
                            break;
                        case 'e':
                            WriteSingle(floatConversion, mutableStringStream, self, num, num2, !BitConverter.IsLittleEndian);
                            break;
                        case 'E':
                            WriteDouble(floatConversion, mutableStringStream, self, num, num2, !BitConverter.IsLittleEndian);
                            break;
                        case 'F':
                        case 'f':
                            WriteSingle(floatConversion, mutableStringStream, self, num, num2, false);
                            break;
                        case 'g':
                            WriteSingle(floatConversion, mutableStringStream, self, num, num2, BitConverter.IsLittleEndian);
                            break;
                        case 'G':
                            WriteDouble(floatConversion, mutableStringStream, self, num, num2, BitConverter.IsLittleEndian);
                            break;
                        case 'H':
                        case 'h':
                            if (GetPackArg(self, num) != null)
                            {
                                MutableString mutableString = ToMutableString(stringCast, mutableStringStream, GetPackArg(self, num));
                                FromHex(mutableStringStream, mutableString, item.Count ?? mutableString.GetByteCount(), item.Directive == 'h');
                            }
                            num2 = 1;
                            break;
                        case 'Q':
                        case 'q':
                            WriteUInt64(integerConversion, mutableStringStream, self, num, num2, false);
                            break;
                        case 'I':
                        case 'L':
                        case 'i':
                        case 'l':
                            WriteUInt32(integerConversion, mutableStringStream, self, num, num2, false);
                            break;
                        case 'N':
                            WriteUInt32(integerConversion, mutableStringStream, self, num, num2, BitConverter.IsLittleEndian);
                            break;
                        case 'n':
                            WriteUInt16(integerConversion, mutableStringStream, self, num, num2, BitConverter.IsLittleEndian);
                            break;
                        case 'V':
                            WriteUInt32(integerConversion, mutableStringStream, self, num, num2, !BitConverter.IsLittleEndian);
                            break;
                        case 'v':
                            WriteUInt16(integerConversion, mutableStringStream, self, num, num2, !BitConverter.IsLittleEndian);
                            break;
                        case 'S':
                        case 's':
                            WriteUInt16(integerConversion, mutableStringStream, self, num, num2, false);
                            break;
                        case 'm':
                            if (GetPackArg(self, num) == null)
                            {
                                throw RubyExceptions.CreateTypeConversionError("nil", "String");
                            }
                            WriteBase64(mutableStringStream, ToMutableString(stringCast, mutableStringStream, GetPackArg(self, num)), (item.Count.HasValue && item.Count.Value > 2) ? item.Count.Value : 45);
                            num2 = 1;
                            break;
                        case 'M':
                            num2 = 1;
                            if (GetPackArg(self, num) != null)
                            {
                                CallSite<Func<CallSite, object, MutableString>> site = tosConversion.GetSite(ConvertToSAction.Make(tosConversion.Context));
                                MutableString str = site.Target(site, GetPackArg(self, num));
                                mutableStringStream.String.TaintBy(str);
                                WritePrintedQuotable(mutableStringStream, str, (item.Count.HasValue && item.Count.Value >= 2) ? item.Count.Value : 72);
                            }
                            break;
                        case 'u':
                            if (GetPackArg(self, num) == null)
                            {
                                throw RubyExceptions.CreateTypeConversionError("nil", "String");
                            }
                            EncodeUU(ToMutableString(stringCast, mutableStringStream, GetPackArg(self, num)).ToByteArray(), (item.Count.HasValue && item.Count.Value > 2) ? item.Count.Value : 45, mutableStringStream);
                            num2 = 1;
                            break;
                        case 'w':
                            {
                                for (int k = 0; k < num2; k++)
                                {
                                    WriteBer(mutableStringStream, Protocols.CastToInteger(integerConversion, GetPackArg(self, num + k)));
                                }
                                break;
                            }
                        case 'U':
                            {
                                for (int j = 0; j < num2; j++)
                                {
                                    WriteUtf8CodePoint(mutableStringStream, Protocols.CastToInteger(integerConversion, GetPackArg(self, num + j)).ToInt32());
                                }
                                break;
                            }
                        case 'X':
                            {
                                num2 = 0;
                                int num4 = (item.Count.HasValue ? item.Count.Value : 0);
                                if (num4 > mutableStringStream.Position)
                                {
                                    throw RubyExceptions.CreateArgumentError("X outside of string");
                                }
                                mutableStringStream.String.Write((int)mutableStringStream.Position - num4, 0, num4);
                                mutableStringStream.Position -= num4;
                                break;
                            }
                        case 'x':
                            {
                                num2 = 0;
                                int num3 = (item.Count.HasValue ? item.Count.Value : 0);
                                mutableStringStream.String.Write((int)mutableStringStream.Position, 0, num3);
                                mutableStringStream.Position += num3;
                                break;
                            }
                        default:
                            num2 = 0;
                            break;
                    }
                    num += num2;
                }
                mutableStringStream.SetLength(mutableStringStream.Position);
                return mutableStringStream.String.TaintBy(format);
            }
        }

        private static MutableString ToMutableString(ConversionStorage<MutableString> stringCast, MutableStringStream stream, object value)
        {
            CallSite<Func<CallSite, object, MutableString>> site = stringCast.GetSite(ProtocolConversionAction<ConvertToStrAction>.Make(stringCast.Context));
            MutableString mutableString = site.Target(site, value);
            if (mutableString != null)
            {
                stream.String.TaintBy(mutableString);
            }
            return mutableString;
        }

        private static object GetPackArg(RubyArray array, int index)
        {
            if (index >= array.Count)
            {
                throw RubyExceptions.CreateArgumentError("too few arguments");
            }
            return array[index];
        }

        private static void WriteString(Stream stream, FormatDirective directive, MutableString str)
        {
            byte[] array = ((str != null) ? str.ToByteArray() : Utils.EmptyBytes);
            int num;
            int num2;
            if (directive.Count.HasValue)
            {
                num = directive.Count.Value;
                num2 = Math.Min(array.Length, num);
            }
            else
            {
                num = array.Length;
                num2 = array.Length;
            }
            stream.Write(array, 0, num2);
            if (num > num2)
            {
                byte value = (byte)((directive.Directive == 'A') ? 32 : 0);
                for (int i = 0; i < num - num2; i++)
                {
                    stream.WriteByte(value);
                }
            }
            if (directive.Directive == 'Z' && !directive.Count.HasValue)
            {
                stream.WriteByte(0);
            }
        }

        public static RubyArray Unpack(MutableString self, [NotNull][DefaultProtocol] MutableString format)
        {
            RubyArray rubyArray = new RubyArray(1 + self.Length / 2);
            int index = 0;
            int byteCount = self.GetByteCount();
            foreach (FormatDirective item in FormatDirective.Enumerate(format.ToString()))
            {
                int leftover = 0;
                switch (item.Directive)
                {
                    case '@':
                        if (item.Count.HasValue)
                        {
                            if (item.Count.Value > byteCount)
                            {
                                throw RubyExceptions.CreateArgumentError("@ outside of string");
                            }
                            index = ((item.Count.Value > 0) ? item.Count.Value : 0);
                        }
                        else
                        {
                            index = byteCount;
                        }
                        break;
                    case 'A':
                    case 'Z':
                    case 'a':
                        rubyArray.Add(ReadString(self, item.Directive, item.Count, ref index));
                        break;
                    case 'B':
                    case 'b':
                        rubyArray.Add(ReadBits(self, item.Count, ref index, item.Directive == 'b'));
                        break;
                    case 'c':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 1, out leftover);
                            for (int num12 = 0; num12 < num2; num12++)
                            {
                                rubyArray.Add((int)(sbyte)self.GetByte(index++));
                            }
                            break;
                        }
                    case 'C':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 1, out leftover);
                            for (int l = 0; l < num2; l++)
                            {
                                rubyArray.Add((int)self.GetByte(index++));
                            }
                            break;
                        }
                    case 'D':
                    case 'd':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 8, out leftover);
                            for (int num17 = 0; num17 < num2; num17++)
                            {
                                rubyArray.Add(ReadDouble(self, ref index, false));
                            }
                            break;
                        }
                    case 'e':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 4, out leftover);
                            for (int num11 = 0; num11 < num2; num11++)
                            {
                                rubyArray.Add((double)ReadSingle(self, ref index, !BitConverter.IsLittleEndian));
                            }
                            break;
                        }
                    case 'E':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 8, out leftover);
                            for (int num7 = 0; num7 < num2; num7++)
                            {
                                rubyArray.Add(ReadDouble(self, ref index, !BitConverter.IsLittleEndian));
                            }
                            break;
                        }
                    case 'F':
                    case 'f':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 4, out leftover);
                            for (int n = 0; n < num2; n++)
                            {
                                rubyArray.Add((double)ReadSingle(self, ref index, false));
                            }
                            break;
                        }
                    case 'g':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 4, out leftover);
                            for (int i = 0; i < num2; i++)
                            {
                                rubyArray.Add((double)ReadSingle(self, ref index, BitConverter.IsLittleEndian));
                            }
                            break;
                        }
                    case 'G':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 8, out leftover);
                            for (int num18 = 0; num18 < num2; num18++)
                            {
                                rubyArray.Add(ReadDouble(self, ref index, BitConverter.IsLittleEndian));
                            }
                            break;
                        }
                    case 'i':
                    case 'l':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 4, out leftover);
                            for (int num16 = 0; num16 < num2; num16++)
                            {
                                rubyArray.Add((int)ReadUInt32(self, ref index, false));
                            }
                            break;
                        }
                    case 'I':
                    case 'L':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 4, out leftover);
                            for (int num13 = 0; num13 < num2; num13++)
                            {
                                rubyArray.Add(Protocols.Normalize(ReadUInt32(self, ref index, false)));
                            }
                            break;
                        }
                    case 'N':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 4, out leftover);
                            for (int num10 = 0; num10 < num2; num10++)
                            {
                                rubyArray.Add(Protocols.Normalize(ReadUInt32(self, ref index, BitConverter.IsLittleEndian)));
                            }
                            break;
                        }
                    case 'n':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 2, out leftover);
                            for (int num8 = 0; num8 < num2; num8++)
                            {
                                rubyArray.Add(ScriptingRuntimeHelpers.Int32ToObject(ReadUInt16(self, ref index, BitConverter.IsLittleEndian)));
                            }
                            break;
                        }
                    case 'v':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 2, out leftover);
                            for (int num5 = 0; num5 < num2; num5++)
                            {
                                rubyArray.Add(ScriptingRuntimeHelpers.Int32ToObject(ReadUInt16(self, ref index, !BitConverter.IsLittleEndian)));
                            }
                            break;
                        }
                    case 'V':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 4, out leftover);
                            for (int num6 = 0; num6 < num2; num6++)
                            {
                                rubyArray.Add(Protocols.Normalize(ReadUInt32(self, ref index, !BitConverter.IsLittleEndian)));
                            }
                            break;
                        }
                    case 'q':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 8, out leftover);
                            for (int m = 0; m < num2; m++)
                            {
                                rubyArray.Add(Protocols.Normalize((long)ReadUInt64(self, ref index, false)));
                            }
                            break;
                        }
                    case 'Q':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 8, out leftover);
                            leftover = 0;
                            for (int k = 0; k < num2; k++)
                            {
                                rubyArray.Add(Protocols.Normalize(ReadUInt64(self, ref index, false)));
                            }
                            break;
                        }
                    case 'm':
                        rubyArray.Add(ReadBase64(self, ref index));
                        break;
                    case 'M':
                        rubyArray.Add(ReadQuotedPrintable(self, ref index));
                        break;
                    case 's':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 2, out leftover);
                            for (int j = 0; j < num2; j++)
                            {
                                rubyArray.Add(ScriptingRuntimeHelpers.Int32ToObject((short)ReadUInt16(self, ref index, !BitConverter.IsLittleEndian)));
                            }
                            break;
                        }
                    case 'S':
                        {
                            int num2 = CalculateCounts(byteCount - index, item.Count, 2, out leftover);
                            for (int num19 = 0; num19 < num2; num19++)
                            {
                                rubyArray.Add(ScriptingRuntimeHelpers.Int32ToObject(ReadUInt16(self, ref index, !BitConverter.IsLittleEndian)));
                            }
                            break;
                        }
                    case 'U':
                        {
                            int num2 = item.Count ?? int.MaxValue;
                            for (int num14 = 0; num14 < num2; num14++)
                            {
                                int num15 = ReadUtf8CodePoint(self, ref index);
                                if (num15 == -1)
                                {
                                    break;
                                }
                                rubyArray.Add(num15);
                            }
                            break;
                        }
                    case 'w':
                        {
                            int num2 = item.Count ?? int.MaxValue;
                            for (int num9 = 0; num9 < num2; num9++)
                            {
                                object obj = ReadBer(self, ref index);
                                if (obj == null)
                                {
                                    break;
                                }
                                rubyArray.Add(obj);
                            }
                            break;
                        }
                    case 'u':
                        rubyArray.Add(ReadUU(self, ref index));
                        break;
                    case 'X':
                        {
                            int num4 = (item.Count.HasValue ? item.Count.Value : (byteCount - index));
                            if (num4 > index)
                            {
                                throw RubyExceptions.CreateArgumentError("X outside of string");
                            }
                            index -= num4;
                            break;
                        }
                    case 'x':
                        {
                            int num3 = (item.Count.HasValue ? (index + item.Count.Value) : byteCount);
                            if (num3 > byteCount)
                            {
                                throw RubyExceptions.CreateArgumentError("X outside of string");
                            }
                            index = num3;
                            break;
                        }
                    case 'H':
                    case 'h':
                        {
                            int num = (byteCount - index) * 2;
                            rubyArray.Add(ToHex(self, ref index, Math.Min(item.Count ?? num, num), item.Directive == 'h'));
                            break;
                        }
                }
                for (int num20 = 0; num20 < leftover; num20++)
                {
                    rubyArray.Add(null);
                }
            }
            return rubyArray;
        }

        private static int CalculateCounts(int remaining, int? count, int size, out int leftover)
        {
            int num = remaining / size;
            if (!count.HasValue)
            {
                leftover = 0;
                return num;
            }
            if (count.Value <= num)
            {
                leftover = 0;
                return count.Value;
            }
            leftover = count.Value - num;
            return num;
        }

        private static MutableString ToHex(MutableString data, ref int index, int nibbleCount, bool swap)
        {
            int num = nibbleCount / 2;
            MutableString mutableString = MutableString.CreateMutable(nibbleCount, RubyEncoding.Binary);
            for (int i = 0; i < num; i++)
            {
                byte @byte = data.GetByte(index++);
                char value = (@byte & 0xF).ToLowerHexDigit();
                char value2 = ((@byte & 0xF0) >> 4).ToLowerHexDigit();
                if (swap)
                {
                    mutableString.Append(value);
                    mutableString.Append(value2);
                }
                else
                {
                    mutableString.Append(value2);
                    mutableString.Append(value);
                }
            }
            if (((uint)nibbleCount & (true ? 1u : 0u)) != 0)
            {
                int byte2 = data.GetByte(index++);
                if (swap)
                {
                    mutableString.Append((byte2 & 0xF).ToLowerHexDigit());
                }
                else
                {
                    mutableString.Append(((byte2 & 0xF0) >> 4).ToLowerHexDigit());
                }
            }
            return mutableString;
        }

        private static MutableString ReadString(MutableString data, int trimMode, int? count, ref int offset)
        {
            int num = offset;
            int num2 = data.GetByteCount();
            if (count.HasValue)
            {
                int num3 = num + count.Value;
                if (num3 < num2)
                {
                    num2 = num3;
                }
            }
            offset = num2;
            if (trimMode != 65)
            {
                if (trimMode == 90)
                {
                    int i;
                    for (i = num; i < num2 && data.GetByte(i) != 0; i++)
                    {
                    }
                    if (!count.HasValue)
                    {
                        offset = i + 1;
                    }
                    num2 = i;
                }
            }
            else
            {
                byte @byte;
                while (--num2 >= num && ((@byte = data.GetByte(num2)) == 0 || @byte == 32))
                {
                }
                num2++;
            }
            return data.GetSlice(num, num2 - num);
        }
    }
}
