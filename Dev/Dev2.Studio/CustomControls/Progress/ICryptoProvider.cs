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
