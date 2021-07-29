/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;

namespace Dev2.Common.Interfaces.Factories
{
    public class FileStreamArgs
    {
        public string FilePath { get; set; }
        public FileMode FileMode { get; set; }
        public FileAccess FileAccess { get; set; }
        public FileShare FileShare { get; set; }
        public int BufferSize { get; set; } = 4096;
        public bool IsAsync { get; set; }
    }

    public interface IFileStreamFactory
    {
        //Note: these can be merged later, by setting the appropriate defaults on FileStreamArgs initialization

        Stream New(string path, FileMode append);
        Stream New(FileStreamArgs fileStreamArgs);
    }
}
