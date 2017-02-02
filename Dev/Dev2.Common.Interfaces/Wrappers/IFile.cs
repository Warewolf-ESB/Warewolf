/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;

namespace Dev2.Common.Interfaces.Wrappers
{
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
    }
}