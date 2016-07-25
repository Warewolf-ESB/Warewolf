/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Security.Cryptography;

namespace Dev2.Common.Interfaces.Utils
{
    public interface ICryptoProvider
    {
    }

    public class CryptoProvider : ICryptoProvider
    {
        private readonly SHA256CryptoServiceProvider _cryptoService;

        public CryptoProvider(SHA256CryptoServiceProvider cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public byte[] ComputeHash(Stream inputStream)
        {
            return _cryptoService.ComputeHash(inputStream);
        }
    }
}