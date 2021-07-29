/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;

namespace Dev2.Common.Interfaces.Runtime.Services
{
    public interface IStreamReaderFactory
    {
        StreamReader New(string file);
        IStreamReaderWrapper New();
    }

    public interface IStreamReaderWrapper
    {
        StreamReader GetStream(Stream stream);
    }

    public class StreamReaderWrapper : IStreamReaderWrapper
    {
        public StreamReader GetStream(Stream stream) => new StreamReader(stream);
    }

    public class StreamReaderFactory : IStreamReaderFactory
    {
        public StreamReader New(string file) => new StreamReader(file);
        public IStreamReaderWrapper New() => new StreamReaderWrapper();
    }
}
