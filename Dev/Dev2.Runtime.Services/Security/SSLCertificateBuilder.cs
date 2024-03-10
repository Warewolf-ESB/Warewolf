#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Dev2.Common;
using System.Configuration;
using ServiceStack.Host;

namespace Dev2.Runtime.Security
{
    /// <summary>
    /// Build a self-signed SSL cert
    /// </summary>
    public class SslCertificateBuilder
    {
        static string _location;
        static string Location => _location ?? (_location = Assembly.GetExecutingAssembly().Location);

        const string BaseCertificatePath = @"SSL Generation";
        const string TrustCertBatFile = @"TrustCertificate.bat";

        public bool CreateCertificateForServerAuthentication(string certificateName, string certificatePFXName)
        {
            try
            {
                //local variables
                string certificatePFXPassword = GlobalConstants.WarewolfSSLCertificatePassword;
                
                int certificateExpiryInYrs = 10;

                // Generate private-public key pair
                var rsaKey = RSA.Create(2048);

                // Describe certificate
                string subject = "CN=localhost";

                // Create certificate request
                var certificateRequest = new CertificateRequest(subject, rsaKey, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);

                //certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: false, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));

                //certificateRequest.CertificateExtensions.Add(new X509KeyUsageExtension(keyUsages: X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, critical: false));

                //certificateRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(key: certificateRequest.PublicKey, critical: false));

                certificateRequest.CertificateExtensions.Add(new X509Extension(new AsnEncodedData("Subject Alternative Name", new byte[] { 48, 11, 130, 9, 108, 111, 99, 97, 108, 104, 111, 115, 116 }), false));

                var certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(certificateExpiryInYrs));

                // Export certificate with private key
                var exportableCertificate = new X509Certificate2(certificate.Export(X509ContentType.Cert), (string?)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet).CopyWithPrivateKey(rsaKey);

                exportableCertificate.FriendlyName = "Warewolf.local Certificate For Server Authorization";

                // Create password for certificate protection
                var passwordForCertificateProtection = new SecureString();
                foreach (var @char in certificatePFXPassword)
                {
                    passwordForCertificateProtection.AppendChar(@char);
                }

                // Export certificate to a file.
                File.WriteAllBytes(certificatePFXName, exportableCertificate.Export(X509ContentType.Pfx, passwordForCertificateProtection));

                // Export certificate to a file.
                File.WriteAllBytes(certificateName, exportableCertificate.Export(X509ContentType.Cert, passwordForCertificateProtection));

                // Test correctness of export
                //var loadedCertificate = new X509Certificate2(certificatePFXName, passwordForCertificateProtection);
                //Console.WriteLine("Certifcate created successfully");

                return true;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                return false;
            }
        }

        public bool TrustCertificateToRoot(string sslCertificateName)
        {
            bool isenableSSLCertificateTrust = false;
            bool.TryParse(ConfigurationManager.AppSettings["enableSSLCertificateTrust"], out isenableSSLCertificateTrust);
            if (!isenableSSLCertificateTrust)
            {
                return true;
            }

            var result = false;
            var asmLoc = Location;
            var exeBase = string.Empty;
            var masterData = string.Empty;
            var workingDir = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(asmLoc))
                {
                    asmLoc = Path.GetDirectoryName(asmLoc);
                    workingDir = Path.Combine(asmLoc, BaseCertificatePath);
                    exeBase = Path.Combine(asmLoc, BaseCertificatePath, TrustCertBatFile);
                    masterData = File.ReadAllText(exeBase);
                    var writeBack = string.Format(masterData, Path.Combine(asmLoc, sslCertificateName));

                    File.WriteAllText(exeBase, writeBack);
                }

                return ProcessHost.Invoke(workingDir, TrustCertBatFile, null, true);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
            finally
            {
                if (!string.IsNullOrEmpty(masterData))
                {
                    File.WriteAllText(exeBase, masterData);
                }
            }

            return result;
        }
    }
}
