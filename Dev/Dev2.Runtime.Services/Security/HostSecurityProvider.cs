/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Wrappers;
using Warewolf.Licensing;

namespace Dev2.Runtime.Security
{
    public class HostSecurityProvider : IHostSecurityProvider
    {
        public static readonly Guid InternalServerID = new Guid("51A58300-7E9D-4927-A57B-E5D700B11B55");
        public static readonly string InternalPublicKey = "BgIAAACkAABSU0ExAAQAAAEAAQBlsJw+ibEmPy3P93PV7a8QjuHqS4QR+yP/+6CVUUpqvUE3hguQUzZ4Fw28hz0LwMLK8Sc1qb0s0FFiH9Ju6O+fIXruGzC3CjzN8wZRGoV2IvfmJ/nMKQ/NVESx9virJA1xTIZa9Za3PQvGbPh1ce0me5YJd3VOHKUqqJCbVeE7pg==";
        public static readonly string SubscriptionTestKey = "wCYcjqzbAiHIneFFib+LCrn73SSkOlRzm4QxP+mkeHsH7e3surKN5liDsrv39JFR";
        public static readonly string SubscriptionTestSiteName = "L8NilnImZ18r8VCMD88AdQ==";
        public static readonly string LiveConfigKey = "ml420y+ZHMiv8CoQJxF1XMsYXXxCcDgNkvFkZSJHQB+m3EdlYIeUAP8oEIl9Z29b";
        public static readonly string LiveConfigSitename = "tWPn5xcpWET9NX3yt+uPHQ==";

        readonly RSACryptoServiceProvider _serverKey;
        readonly RSACryptoServiceProvider _systemKey;

        public Guid ServerID { get; private set; }
        public string SubscriptionKey { get; }
        public string SubscriptionSiteName { get; }
        public string CustomerId { get; }
        public string PlanId { get; }
        public string SubscriptionId { get; }
        static volatile IHostSecurityProvider _theInstance;
        static readonly object SyncRoot = new object();
        static readonly object SyncSubRoot = new object();
        static volatile ISubscriptionData _subscriptionData;

        public static IHostSecurityProvider Instance
        {
            get
            {
                if (_theInstance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_theInstance == null)
                        {
                            var config = new HostSecureConfig();
                            _theInstance = new HostSecurityProvider(config);
                        }
                    }
                }

                return _theInstance;
            }
        }

        public static ISubscriptionData SubscriptionDataInstance
        {
            get
            {
                if (_subscriptionData == null)
                {
                    lock (SyncSubRoot)
                    {
                        if (_subscriptionData == null)
                        {
                            var instance = Instance;
                            //TODO: Add Subscription Status to the Instance
                            _subscriptionData = new SubscriptionData
                            {
                                SubscriptionKey = instance.SubscriptionKey,
                                SubscriptionSiteName = instance.SubscriptionSiteName,
                                SubscriptionId = instance.SubscriptionId,
                                CustomerId = instance.CustomerId,
                                PlanId = instance.PlanId,
                                //Status = instance.Status
                            };
                        }
                    }
                }

                return _subscriptionData;
            }
        }

        public RSACryptoServiceProvider SystemKey => _systemKey;
        public RSACryptoServiceProvider ServerKey => _serverKey;

        protected HostSecurityProvider(ISecureConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            ServerID = config.ServerID;
            _serverKey = config.ServerKey;
            _systemKey = config.SystemKey;
            SubscriptionKey = config.ConfigKey;
            SubscriptionSiteName = config.ConfigSitename;
            CustomerId = config.CustomerId;
            PlanId = config.PlanId;
            SubscriptionId = config.SubscriptionId;
        }

        public bool VerifyXml(StringBuilder xml)
        {
            if (xml == null || xml.Length == 0)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            return true;
        }

        public StringBuilder SignXml(StringBuilder xml)
        {
            if (xml == null || xml.Length == 0)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            // remove the signature element here as it does not pick up correctly futher down ;(
            xml = RemoveSignature(xml);

            using (var s = xml.EncodeForXmlDocument())
            {
                var doc = new XmlDocument();

                doc.Load(s);

                SetServerID(doc);

                var result = new StringBuilder();
                using (var sw = new StringWriter(result))
                {
                    doc.Save(sw);
                }

                // remove the crapy encoding header
                result = result.CleanEncodingHeaderForXmlSave();

                return result;
            }
        }

        static StringBuilder RemoveSignature(StringBuilder sb)
        {
            const string SignatureStart = "<Signature xmlns";
            const string SignatureEnd = "</Signature>";

            var startIdx = sb.IndexOf(SignatureStart, 0, false);
            if (startIdx >= 0)
            {
                var endIdx = sb.IndexOf(SignatureEnd, startIdx, false);

                if (endIdx >= 0)
                {
                    var len = endIdx - startIdx + SignatureEnd.Length;
                    return sb.Remove(startIdx, len);
                }
            }

            return sb;
        }

        void SetServerID(XmlDocument doc)
        {
            doc.DocumentElement?.SetAttribute(nameof(ServerID), ServerID.ToString());
        }

        public bool EnsureSsl(IFile fileWrapper, string certPath, IPEndPoint endPoint)
        {
            var result = false;

            if (!fileWrapper.Exists(certPath))
            {
                try
                {
                    var certificateBuilder = new SslCertificateBuilder();
                    certificateBuilder.EnsureSslCertificate(certPath, endPoint);
                    result = fileWrapper.Exists(certPath);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                }
            }
            else
            {
                result = SslCertificateBuilder.BindSslCertToPorts(endPoint, certPath);
            }

            return result;
        }

        public ISubscriptionData UpdateSubscriptionData(ISubscriptionData subscriptionData)
        {
            //TODO: Save new subscription data to Warewolf Server.exe.secureconfig
            var updateSubscriptionData = new SubscriptionData
            {
                SubscriptionId = subscriptionData.SubscriptionId,
                CustomerId = subscriptionData.CustomerId,
                PlanId = subscriptionData.PlanId,
                Status = subscriptionData.Status,
                IsLicensed = subscriptionData.IsLicensed,
            };

            return updateSubscriptionData;
        }
    }
}