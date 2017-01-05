/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using System.IO;

namespace Dev2.Common.Wrappers
{ // not required for code coverage this is simply a pass through required for unit testing
    public class FileWrapper : IFile
    {
        public string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        public void Move(string source, string destination)
        {
            File.Move(source, destination);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void Delete(string tmpFileName)
        {
            File.Delete(tmpFileName);
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
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

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(path);
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            File.SetAttributes(path, fileAttributes);
        }
    }
}