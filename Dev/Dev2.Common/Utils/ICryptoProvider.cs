using System.IO;
using System.Security.Cryptography;

namespace Dev2.Common.Utils
{
    public interface ICryptoProvider
    {
        byte[] ComputeHash(Stream inputStream);
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
