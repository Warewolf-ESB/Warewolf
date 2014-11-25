
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;
using System.Security.Cryptography;

namespace Dev2.CustomControls.Progress
{
    public interface ICryptoProvider
    {
        byte[] ComputeHash(Stream inputStream);
    }

    public class CryptoProvider : ICryptoProvider
    {
        private  SHA256CryptoServiceProvider cryptoService;

        public CryptoProvider(SHA256CryptoServiceProvider cryptoService)
        {
            this.cryptoService = cryptoService;
        }

        public byte[] ComputeHash(Stream inputStream)
        {
            return cryptoService.ComputeHash(inputStream);
        }
    }
}
