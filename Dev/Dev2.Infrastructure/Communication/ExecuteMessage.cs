/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

        public string GetDecompressedMessage()
        {
            return Message.ToString();
        }
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
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
   

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        public static string Decompress(string compressedText)
        {
            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        public string GetDecompressedMessage()
        {
            return Decompress(_message.ToString());
        }
    }
}
