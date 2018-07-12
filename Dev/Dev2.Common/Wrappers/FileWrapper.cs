/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Dev2.Common.Wrappers
{ // not required for code coverage this is simply a pass through required for unit testing
    public class FileWrapper : IFile
    {
        public string ReadAllText(string fileName) => File.ReadAllText(fileName);

        public void Move(string source, string destination)
        {
            File.Move(source, destination);
        }

        public bool Exists(string path) => File.Exists(path);

        public void Delete(string tmpFileName)
        {
            File.Delete(tmpFileName);
        }

        public void WriteAllText(string p1, string p2)
        {
            File.WriteAllText(p1, p2);
        }

        public void Copy(string source, string destination)
        {
            File.Copy(source, destination);
        }

        public void WriteAllBytes(string path, byte[] contents)
        {
            File.WriteAllBytes(path, contents);
        }

        public void AppendAllText(string path, string contents)
        {
            File.AppendAllText(path, contents);
        }

        public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);

        public FileAttributes GetAttributes(string path) => File.GetAttributes(path);

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            File.SetAttributes(path, fileAttributes);
        }

        public Stream OpenRead(string path) => File.OpenRead(path);

        readonly static ConcurrentDictionary<string, RefCountedStreamWriter> cache = new ConcurrentDictionary<string, RefCountedStreamWriter>();
        public IDev2StreamWriter AppendText(string filePath)
        {
            RefCountedStreamWriter writer;
            try
            {
                lock (cache)
                {
                    RefCountedStreamWriter result;
                    if (cache.TryGetValue(filePath, out writer))
                    {
                        result = writer.GetReference();
                        lock (result)
                        {
                            if (result.Closed)
                            {
                                result.SetTextWriter(File.AppendText(filePath));
                            }
                        }
                        return result;
                    }

                    var streamWriter = File.AppendText(filePath);
                    result = new RefCountedStreamWriter(streamWriter);
                    if (!cache.TryAdd(filePath, result))
                    {
                        throw new Exception($"failed keeping single reference to {filePath}");
                    }
                    return result.GetReference();
                }
            } catch (Exception e)
            {
                if (cache.TryGetValue(filePath, out writer))
                {
                    return writer.GetReference();
                }
                throw new Exception($"failed opening {filePath} for appending", e);
            }
        }

        public DateTime GetLastWriteTime(string filePath)
        {
            return File.GetLastWriteTime(filePath).Date;
        }
    }
    class RefCountedStreamWriter : IDev2StreamWriter
    {
        public int count;
        public bool Closed { get; private set; } = false;
        public RefCountedStreamWriter GetReference()
        {
            Interlocked.Increment(ref count);
            return this;
        }
        public TextWriter SynchronizedTextWriter { get; private set; }
        public void SetTextWriter(StreamWriter writer)
        {
            this.SynchronizedTextWriter = TextWriter.Synchronized(writer);
            Closed = false;
        }

        public RefCountedStreamWriter(StreamWriter writer)
        {
            SetTextWriter(writer);
        }

        public void WriteLine(string v)
        {
            this.SynchronizedTextWriter.WriteLine(v);
        }

        public void WriteLine()
        {
            this.SynchronizedTextWriter.WriteLine();
        }

        public void Flush()
        {
            this.SynchronizedTextWriter.Flush();
        }

        public void Dispose()
        {
            lock (this.SynchronizedTextWriter)
            {
                Interlocked.Decrement(ref count);
            }
            if (count <= 0)
            {
                Closed = true;
                this.SynchronizedTextWriter.Dispose();
            }
        }
    }
}