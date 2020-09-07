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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Dev2.Common.Interfaces.Runtime.Services
{

    public interface IStreamContentWrapper
    {
        HttpContentHeaders Headers { get; }
        Task<MultipartMemoryStreamProvider> ReadAsMultipartAsync(MultipartMemoryStreamProvider provider);
        Task<byte[]> ReadAsByteArrayAsync();
    }
    public class StreamContentWrapper  : IStreamContentWrapper
    {
        private readonly StreamContent _content;

        public StreamContentWrapper(MemoryStream memoryStream)
        {
            _content = new StreamContent(memoryStream);
        } 

        public HttpContentHeaders Headers => _content.Headers;

        public async Task<byte[]> ReadAsByteArrayAsync()
        {
            return await _content.ReadAsByteArrayAsync();
        }

        public async Task<MultipartMemoryStreamProvider> ReadAsMultipartAsync(MultipartMemoryStreamProvider provider)
        {
            return await _content.ReadAsMultipartAsync(provider);
        }
    }

    public interface IStreamContentFactory
    {
        IStreamContentWrapper New(MemoryStream tempStream);
    }

    public class StreamContentFactory : IStreamContentFactory
    {
        public IStreamContentWrapper New(MemoryStream tempStream)
        {
            return new StreamContentWrapper(tempStream);
        }
    }
}
