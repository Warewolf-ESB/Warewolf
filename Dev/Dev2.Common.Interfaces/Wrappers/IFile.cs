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

namespace Dev2.Common.Interfaces.Wrappers
{
    public enum FileOverwrite {
        No,
        Yes
    }

    public interface IFile
    {
        string ReadAllText(string fileName);
        void Move(string source, string destination);

        bool Exists(string path);
        void Delete(string tmpFileName);
        void WriteAllText(string p1, string p2);
        void Copy(string source, string destination);

        void WriteAllBytes(string path, byte[] contents);
        void AppendAllText(string path, string contents);

        byte[] ReadAllBytes(string path);

        FileAttributes GetAttributes(string path);

        void SetAttributes(string path, FileAttributes fileAttributes);
        Stream OpenRead(string path);
        IDev2StreamWriter AppendText(string filePath);

        DateTime GetLastWriteTime(string filePath);
        void Copy(string src, string dst, bool overwrite);
        string DirectoryName(string path);
        IFileInfo Info(string path);
        /// <summary>
        /// returns the fully qualified path to a new temp file
        /// </summary>
        /// <returns></returns>
        string GetTempFileName();
    }

    public interface IDev2StreamWriter : IDisposable
    {
        TextWriter SynchronizedTextWriter { get; }

        void WriteLine(string v);
        void WriteLine();
        void Flush();
    }
}
