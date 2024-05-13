#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Dev2.Common.Interfaces.Infrastructure.Communication;

namespace Dev2.Communication
{
    public class ExecuteMessage : IExecuteMessage
    {
        public bool HasError { get; set; }

        public StringBuilder Message { get; set; }

        public ExecuteMessage()
        {
            Message = new StringBuilder();
        }

        public void SetMessage(string message)
        {
            Message.Append(message);
        }

        public string GetDecompressedMessage() => Message.ToString();
    }

    public class CompressedExecuteMessage : IExecuteMessage
    {
        StringBuilder _message;
        public bool HasError { get; set; }

        public StringBuilder Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
               
            }
        }
        public bool IsCompressed { get; set; }

        public CompressedExecuteMessage()
        {
            IsCompressed = false;
            _message = new StringBuilder();
        }

        public void SetMessage(string message)
        {
            _message.Append(Compress( message));
            IsCompressed = true;
        }


        public static string Compress(string text)
        {
            var buffer = Encoding.UTF8.GetBytes(text);
            using (var memoryStream = new MemoryStream())
            {
                using (var zip = new DeflateStream(memoryStream, CompressionLevel.Optimal))
                {
                    zip.Write(buffer, 0, buffer.Length);
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static string Decompress(string compressedText)
		{
			var gzBuffer = Convert.FromBase64String(compressedText);
            byte[] buffer;
			try
            {
                using (var compressedStream = new MemoryStream(gzBuffer))
                using (var decompressedStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                using (MemoryStream ms = new MemoryStream())
                {
                    decompressedStream.CopyTo(ms);
                    buffer = ms.ToArray();
                }
            }
            catch (InvalidDataException exception)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					var msgLength = BitConverter.ToInt32(gzBuffer, 0);
					ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

					buffer = new byte[msgLength];

					ms.Position = 0;
					using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
					{
						int totalRead = 0;
						while (totalRead < buffer.Length)
						{
							int bytesRead = zip.Read(buffer, totalRead, buffer.Length - totalRead);
							if (bytesRead == 0) break;
							totalRead += bytesRead;
						}
					}
				}
			}
			return Encoding.UTF8.GetString(buffer);
		}

        public string GetDecompressedMessage() => Decompress(_message.ToString());
    }
}
