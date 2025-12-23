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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Dev2.Communication;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchResourceDefinition : IEsbManagementEndpoint
    {
        // Regex for JSON password properties: "Password":"value"
        private static readonly Regex JsonPasswordRegex = new Regex(
           @"""(?:Password|PWD)""\s*:\s*""(?:[^""\\]|\\.)*""",
           RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Regex for connection string passwords
        // Pattern matches: Password=<value><terminator>
        // Where <value> can contain any characters including escaped quotes
        // And <terminator> is either:
        //   - ; (semicolon for next param)
        //   - \" (escaped quote - end of XML attribute in JSON context)
        //   - \"; (semicolon after escaped quote - password ends the ConnectionString)
        private static readonly Regex ConnectionStringPasswordRegex = new Regex(
            @"(?:Password|PWD)\s*=\s*(?:[^;\\]|\\[""\\])*?([;]|\\"")",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private IResourceDefinationCleaner _resourceDefinationCleaner;
        private IResourceCatalog _resourceCatalog;

        public IResourceCatalog ResourceCat
        {
            private get
            {
                return _resourceCatalog ?? ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalog = value;
            }
        }

        public IResourceDefinationCleaner Cleaner
        {
            private get
            {
                return _resourceDefinationCleaner ?? new ResourceDefinationCleaner();
            }
            set
            {
                _resourceDefinationCleaner = value;
            }
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("ResourceID", out StringBuilder tmp);
            if (tmp != null && Guid.TryParse(tmp.ToString(), out Guid resourceId))
            {
                return resourceId;
            }

            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.View;

        [ExcludeFromCodeCoverage]
        public FetchResourceDefinition()
        {
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                string serviceId = null;
                var prepairForDeployment = false;
                var removePassword = false;
                values.TryGetValue(@"ResourceID", out StringBuilder tmp);

                if (tmp != null)
                {
                    serviceId = tmp.ToString();
                }

                values.TryGetValue(@"PrepairForDeployment", out tmp);

                if (tmp != null)
                {
                    bool.TryParse(tmp.ToString(), out prepairForDeployment);
                }

                values.TryGetValue(@"RemovePass", out tmp);
                if (tmp != null)
                {
                    bool.TryParse(tmp.ToString(), out removePassword);
                }

                Guid.TryParse(serviceId, out Guid resourceId);

                Dev2Logger.Info($"Fetch Resource definition. ResourceId: {resourceId}", GlobalConstants.WarewolfInfo);
                var result = ResourceCat.GetResourceContents(theWorkspace.ID, resourceId);
                var resourceDefinition = Cleaner.GetResourceDefinition(prepairForDeployment, resourceId, result);


                return removePassword ? RemovePasswordsFromJson(resourceDefinition.ToString()) : resourceDefinition;
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        /// <summary>
        /// Removes passwords from JSON strings including both JSON properties and connection strings.
        /// Handles escaped quotes within password values and multiple password field variations.
        /// Supported variations: Password, password, PASSWORD, PWD, Pwd, pwd
        /// </summary>
        /// <param name="json">The JSON string containing passwords</param>
        /// <returns>JSON StringBuilder with passwords removed</returns>
        public static StringBuilder RemovePasswordsFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new StringBuilder();
            }

            try
            {
                

                // Remove JSON password properties: "Password":"value", "PWD":"value", etc.
                json = JsonPasswordRegex.Replace(json, match =>
                {
                    // Preserve the original key casing in the replacement
                    var key = match.Value.Substring(0, match.Value.IndexOf(':'));
                    return $"{key}:\"\"";
                });

                // Remove connection string passwords
                // This handles:
                // - Password=test;Port=80\" -> Password=;Port=80\",
                // - Password=test\" -> Password=\",
                // - Password=test;\" -> Password=;\",
                // - Password=hello \" world;\" -> Password=;\",
                json = ConnectionStringPasswordRegex.Replace(json, match =>
                {
                    var equalsIndex = match.Value.IndexOf('=');
                    if (equalsIndex == -1)
                    {
                        return match.Value;
                    }

                    var key = match.Value.Substring(0, equalsIndex);
                    var terminator = match.Groups[1].Value; // Gets ; or \"
                    
                    return $"{key}={terminator}";
                });

                return new StringBuilder(json);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                return new StringBuilder(json); // return original json if error occurs
            }
        }

        public StringBuilder DecryptAllPasswords(StringBuilder stringBuilder) => Cleaner.DecryptAllPasswords(stringBuilder);

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => @"FetchResourceDefinitionService";
    }
}
