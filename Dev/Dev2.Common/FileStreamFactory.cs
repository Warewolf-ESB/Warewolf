/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Factories;
using System.IO;

namespace Dev2.Common
{
    //Note: these can be merged later, by setting the appropriate defaults on FileStreamArgs initialization
    public class FileStreamFactory : IFileStreamFactory
    {
        public Stream New(string path, FileMode append) => new FileStream(path, append);
        public Stream New(FileStreamArgs fileStreamArgs) => new FileStream(fileStreamArgs.FilePath, fileStreamArgs.FileMode, fileStreamArgs.FileAccess, fileStreamArgs.FileShare, fileStreamArgs.BufferSize, fileStreamArgs.IsAsync);
    }
}
