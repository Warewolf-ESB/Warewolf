/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Dev2.Common.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class FileWrapper : IFile
    {
        private static T PerformActionAsServerUser<T>(Func<T> actionToPerform)
        {
            var returnValue = default(T);
            var userPrinciple = Utilities.ServerUser;
            Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
                if (actionToPerform != null)
                {
                    returnValue = actionToPerform();
                }
            });
            return returnValue;
        }

        private static void PerformActionAsServerUser(Action actionToPerform)
        {
            var userPrinciple = Utilities.ServerUser;
            Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
                actionToPerform?.Invoke();
            });
        }

        public string ReadAllText(string fileName) => PerformActionAsServerUser(() => File.ReadAllText(fileName));

        public void Move(string source, string destination)
        {
            PerformActionAsServerUser(() => File.Move(source, destination));
        }

        public bool Exists(string path) => PerformActionAsServerUser(() => File.Exists(path));

        public void Delete(string tmpFileName)
        {
            PerformActionAsServerUser(() => File.Delete(tmpFileName));
        }

        public void WriteAllText(string p1, string p2)
        {
            PerformActionAsServerUser(() => File.WriteAllText(p1, p2));
        }

        public void Copy(string source, string destination)
        {
            PerformActionAsServerUser(() => File.Copy(source, destination));
        }

        public void WriteAllBytes(string path, byte[] contents)
        {
            PerformActionAsServerUser(() => File.WriteAllBytes(path, contents));
        }

        public void AppendAllText(string path, string contents)
        {
            PerformActionAsServerUser(() => File.AppendAllText(path, contents));
        }

        public byte[] ReadAllBytes(string path) => PerformActionAsServerUser(() => File.ReadAllBytes(path));

        public FileAttributes GetAttributes(string path) => PerformActionAsServerUser(() => File.GetAttributes(path));

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            PerformActionAsServerUser(() => File.SetAttributes(path, fileAttributes));
        }

        public Stream OpenRead(string path) => PerformActionAsServerUser(() => File.OpenRead(path));

        readonly static ConcurrentDictionary<string, RefCountedStreamWriter> _cache = new ConcurrentDictionary<string, RefCountedStreamWriter>();
        public IDev2StreamWriter AppendText(string filePath)
        {
            return PerformActionAsServerUser(() =>
            {
                RefCountedStreamWriter writer;
                try
                {
                    lock (_cache)
                    {
                        RefCountedStreamWriter result;
                        if (_cache.TryGetValue(filePath, out writer))
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
                        if (!_cache.TryAdd(filePath, result))
                        {
                            throw new Exception($"failed keeping single reference to {filePath}");
                        }
                        return result.GetReference();
                    }
                }
                catch (Exception e)
                {
                    if (_cache.TryGetValue(filePath, out writer))
                    {
                        return writer.GetReference();
                    }
                    throw new Exception($"failed opening {filePath} for appending", e);
                }
            });
        }

        public DateTime GetLastWriteTime(string filePath)
        {
            return PerformActionAsServerUser(() => File.GetLastWriteTime(filePath).Date);
        }

        public IFileInfo Info(string path)
        {
            return new FileInfoWrapper(new FileInfo(path));
        }


        public void Copy(string src, string dst, bool overwrite)
        {
            File.Copy(src, dst, overwrite);
        }
        public string DirectoryName(string path)
        {
            var f = new FileInfo(path);
            return f.DirectoryName;
        }

        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }
    }

    [ExcludeFromCodeCoverage]
    class RefCountedStreamWriter : IDev2StreamWriter
    {
        public int _count;
        public bool Closed { get; private set; }
        public RefCountedStreamWriter GetReference()
        {
            Interlocked.Increment(ref _count);
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
                Interlocked.Decrement(ref _count);
            }
            if (_count <= 0)
            {
                Closed = true;
                this.SynchronizedTextWriter.Dispose();
            }
        }
    }
}