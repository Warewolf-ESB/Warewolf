using System;
using System.Configuration;
using System.IO;
using Dev2.Runtime.Security;

namespace Dev2.Runtime.SignerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SignFiles(
                ConfigurationManager.AppSettings["Inbox"],
                ConfigurationManager.AppSettings["Outbox"]);
        }

        static void SignFiles(string srcPath, string dstPath)
        {
            var files = Directory.GetFiles(srcPath);
            foreach(var srcFile in files)
            {
                var dstFile = Path.Combine(dstPath, Path.GetFileName(srcFile));
                SignFile(srcFile, dstFile);
            }
        }

        #region SignFile

        static void SignFile(string srcPath, string dstPath)
        {
            var xml = File.ReadAllText(srcPath);
            var signedXml = HostSecurityProvider.Instance.SignXml(xml);

            File.WriteAllText(dstPath, signedXml);

            //ValidateFile(signedXml);
        }

        #endregion

        #region ValidateFile

        static void ValidateFile(string signedXml)
        {
            var settings = HostSecureConfig.CreateSettings(
                "d53bbcc5-4794-4dfa-b096-3aa815692e66",
                "BwIAAACkAABSU0EyAAQAAAEAAQBBgKRIdzPGpaPt3hJ7Kxm8iVrKpfu4wfsJJf/3gBG5qhiS0rs5j5HqkLazdO5az9oPWnSTmNnww03WvCJhz8nhaJjXHoEK6xtcWL++IY+R3E27xaHaPQJSDvGg3j1Jvm0QKUGmzZX75tGDC4s17kQSCpsVW3vEuZ5gBdMLMi3UqaVW9EO7qOcEvVO9Cym7lxViqUhvq6c0nLzp6C6zrtZGjLtFqo9KDj7PMkq10Xc0JkzE1ptRz/YytMRacIDn8tptbHbxM8AtObeeiZ7V6Tznmi82jcAm2Jugr0D97Da2MXZuqEKLL5uPagL4RUHite3kT/puSNbTtqZLdqMtV5HGqVmn2a64JU3b8TIW8rKd5rKucG4KwoXRNQihJzX1it8vcqt6tjDnJZdJkuyDjdd6BKCYHWeX9mqDwKJ3EY+TRZmsl9RILyV/XviyrpTYBdDDmdQ9YLSLt0LtdpPpcRzciwMsBEfMH5NPFOtqSF/151Sl/DBdEJxOINXDl1qdO5MtgL7rXkfiGwu66n4hokRdVlj6TTcXTCn6YrUbzOts6IZJnQ9cwK693u9yMJ3Di0hp49L6LWnoWmW334ys+iOfob0i4eM+M3XNw7wGN/jd6t2KYemVZEnTcl5Lon5BpdoFlxa7Y1n+kXbaeSAwTJIe9HM6uoXIH61VCIk0ac69oZcG2/FhSeBO/DcGIQQqdFvuFqJY0g2qbt7+hmEZDZBehr3KpoRTgB5xPW/ThVhuaoZxlpEb4hFmKoj900knnQk=",
                "BgIAAACkAABSU0ExAAQAAAEAAQBzb9y6JXoJj70+TVeUgRc7hPjb6tTJR7B/ZHZKFQsTLkhQLHo+93x/f30Lj/FToE2xXqnuZPk9IV94L4ekt+5jgEFcf1ReuJT/G1dVb1POiEC0upGdagwW10T3PcBK+UzfSXz5kD0SiGhXamPnT/zuHiTtVjv87W+5WuvU1vsrsQ=="
                );

            var config = new HostSecureConfig(settings, false);
            var provider = new HostSecurityProviderImpl(config);
            var isValid = provider.VerifyXml(signedXml);
            if(!isValid)
            {
                throw new InvalidOperationException("verification failed");
            }
        }

        #endregion
    }


    #region HostSecurityProviderImpl class

    public class HostSecurityProviderImpl : HostSecurityProvider
    {
        public HostSecurityProviderImpl(ISecureConfig config)
            : base(config)
        {

        }

    }

    #endregion

}
