
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System
{
    partial class WeaveUtility
    {
        public static FileStream TryOpen(string path)
        {
            if (File.Exists(path)) return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return null;
        }

        public static FileStream Open(string path)
        {
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
        }

        public static FileStream OpenOrCreate(string path)
        {
            FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            stream.Seek(0, SeekOrigin.End);
            return stream;
        }

        public static FileStream Create(string path)
        {
            return File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        }

        public static IByteReaderBase Read(byte[] data)
        {
            return new BinaryFileReader(new MemoryStream(data, true), true);
        }

        public static IByteReaderBase Read(Stream input, bool closeStream = true)
        {
            return new BinaryFileReader(input, closeStream);
        }

        public static IByteReaderBase ReadOpen(string path)
        {
            return new BinaryFileReader(Open(path));
        }

        public static IByteWriterBase WriteOpenOrCreate(string path)
        {
            return new BinaryFileWriter(OpenOrCreate(path));
        }

        public static IByteWriterBase WriteCreate(string path)
        {
            return new BinaryFileWriter(Create(path));
        }
    }
}
