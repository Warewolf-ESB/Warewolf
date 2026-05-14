/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Warewolf.Security.Encryption;
using Warewolf.Sharepoint;

namespace Dev2.Data.ServiceModel
{
    public class SharepointSource : Resource, ISharepointSource, IResourceSource
    {
        private readonly ISharepointHelperFactory _sharepointHelperFactory;

        public string Server { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public SharepointSource()
            :this(new SharepointHelperFactory())
        {

        }

        public SharepointSource(ISharepointHelperFactory sharepointHelperFactory)
        {
            _sharepointHelperFactory = sharepointHelperFactory;
            ResourceID = Guid.Empty;
            ResourceType = "SharepointServerSource";
            AuthenticationType = AuthenticationType.Windows;
        }

        public SharepointSource(XElement xml)
            : this(xml, new SharepointHelperFactory())
        {

        }

        public SharepointSource(XElement xml, ISharepointHelperFactory sharepointHelperFactory)
            : base(xml)
        {
            _sharepointHelperFactory = sharepointHelperFactory;
            ResourceType = "SharepointServerSource";
            AuthenticationType = AuthenticationType.Windows;

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Server", string.Empty },
                { "AuthenticationType", string.Empty },
                { "UserName", string.Empty },
                { "Password", string.Empty }
            };

            var conString = xml.AttributeSafe("ConnectionString");

            // Detect WFAES:: values that cannot be decrypted because the AES hook was
            // never registered (Key Vault init failed or AZURE_KEYVAULT_NAME not set).
            // Throw early so the caller gets a clear error instead of a silent empty Server.
            if (!string.IsNullOrEmpty(conString)
                && conString.StartsWith("WFAES::", StringComparison.Ordinal)
                && DpapiWrapper.AesDecryptHook == null)
            {
                throw new InvalidOperationException(
                    "SharepointSource ConnectionString has a WFAES:: prefix (AES-256-GCM encryption) " +
                    "but DpapiWrapper.AesDecryptHook is not registered — the AES key was not loaded from Key Vault. " +
                    "Ensure AZURE_KEYVAULT_NAME is set and Key Vault is reachable at startup. " +
                    "If SkipFailureToRetrieveSecret=true, the host started in degraded mode without the decryption key.");
            }

            // If CanBeDecrypted returns false and the value looks like a base64 blob (DPAPI),
            // the file was encrypted by the full Warewolf server using Windows DPAPI and cannot
            // be decrypted on Linux. Throw immediately with a clear message instead of silently
            // using the raw ciphertext as the connection string (which produces an empty Server).
            if (!string.IsNullOrEmpty(conString)
                && !conString.StartsWith("WFAES::", StringComparison.Ordinal)
                && !conString.CanBeDecrypted()
                && conString.IsBase64())
            {
                var hookStatus = DpapiWrapper.AesDecryptHook != null
                    ? "registered (Key Vault key was loaded)"
                    : "NOT registered";
                throw new InvalidOperationException(
                    "SharepointSource ConnectionString is DPAPI-encrypted (Windows-only) and cannot be decrypted " +
                    $"in the current environment. DpapiWrapper.AesDecryptHook is {hookStatus}. " +
                    "Re-encrypt the .bite file using Encrypt-Config.ps1 so the ConnectionString carries the WFAES:: prefix, " +
                    "then rebuild the Docker image (run run.ps1 choosing Y to re-publish).");
            }

            string connectionString;
            try
            {
                connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            }
            catch (Exception ex)
            {
                var prefix = conString.StartsWith("WFAES::", StringComparison.Ordinal) ? "WFAES::" : "(non-WFAES)";
                throw new InvalidOperationException(
                    $"Failed to decrypt SharepointSource ConnectionString (prefix={prefix}): " +
                    $"{ex.GetType().Name}: {ex.Message}. " +
                    "Ensure the Key Vault AES key used by Encrypt-Config.ps1 matches the one configured in the container " +
                    "(AZURE_KEYVAULT_NAME / KEYVAULT_SECRET_NAME env vars).", ex);
            }

            ParseProperties(connectionString, properties);
            Server = properties["Server"];
            UserName = properties["UserName"];
            Password = properties["Password"];
            var isSharepointSourceValue = xml.AttributeSafe("IsSharepointOnline");
            if (bool.TryParse(isSharepointSourceValue, out bool isSharepointSource))
            {
                IsSharepointOnline = isSharepointSource;
            }
            AuthenticationType = Enum.TryParse(properties["AuthenticationType"], true, out AuthenticationType authType) ? authType : AuthenticationType.Windows;
        }

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                $"Server={Server}",
                $"AuthenticationType={AuthenticationType}"
                );

            if (AuthenticationType == AuthenticationType.User)
            {
                connectionString = string.Join(";",
                    connectionString,
                    $"UserName={UserName}",
                    $"Password={Password}"
                    );
            }

            result.Add(
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString)),
				new XAttribute("IsSharepointOnline", IsSharepointOnline),
                new XAttribute("Type", GetType().Name),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        public override bool IsSource => true;

        public override bool IsService => false;

        public override bool IsFolder => false;

        public override bool IsReservedService => false;

        public override bool IsServer => false;

        public override bool IsResourceVersion => false;

        public List<ISharepointListTo> LoadLists()
        {
            var sharepointHelper = CreateSharepointHelper();
            return sharepointHelper.LoadLists();
        }

        public List<ISharepointFieldTo> LoadFieldsForList(string listName) => LoadFieldsForList(listName, false);

        public List<ISharepointFieldTo> LoadFieldsForList(string listName, bool editableFieldsOnly)
        {
            var sharepointHelper = CreateSharepointHelper();
            return sharepointHelper.LoadFieldsForList(listName, editableFieldsOnly);
        }

        public virtual ISharepointHelper CreateSharepointHelper()
        {
            string userName = null;
            string password = null;

            if (AuthenticationType == AuthenticationType.User)
            {
                userName = UserName;
                password = Password;
            }
            var sharepointHelper = _sharepointHelperFactory.New(Server, userName, password, IsSharepointOnline);
            return sharepointHelper;
        }

        public string TestConnection()
        {
            var helper = CreateSharepointHelper();
            var testConnection = helper.TestConnection(out bool isSharepointOnline);
            IsSharepointOnline = isSharepointOnline;
            return testConnection;
        }

        public bool IsSharepointOnline { get; set; }
    }
}