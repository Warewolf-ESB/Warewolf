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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindSourcesByType : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                string type = null;
                bool removePassword = false;
                values.TryGetValue("Type", out StringBuilder tmp);
                if (tmp != null)
                {
                    type = tmp.ToString();
                }

                if (string.IsNullOrEmpty(type))
                {
                    throw new ArgumentNullException("type");
                }

                values.TryGetValue("RemovePassword", out StringBuilder tmp2);
                if (tmp2 != null)
                {
                    bool.TryParse(tmp2.ToString(), out removePassword);
                }

                Dev2Logger.Info("Find Sources By Type. " + type, GlobalConstants.WarewolfInfo);
                if (Enum.TryParse(type, true, out enSourceType sourceType))
                {
                    var result = ResourceCatalog.Instance.GetModels(theWorkspace.ID, sourceType);
                    if (result != null)
                    {
                        var serializer = new Dev2JsonSerializer();
                        

                        /// <summary>
                        /// Determines the appropriate serialization strategy based on the source type.
                        /// Clones source objects with optional password removal before serialization.
                        /// </summary>
                        /// <returns>A <see cref="StringBuilder"/> containing the serialized result.</returns>
                        return sourceType switch
                        {
                            enSourceType.Dev2Server => CloneAndSerialize<Connection>(result, removePassword, serializer, CloneConnection),
                            enSourceType.SqlDatabase or enSourceType.MySqlDatabase or enSourceType.PostgreSQL or enSourceType.Oracle or enSourceType.ODBC 
                                => CloneAndSerialize<DbSource>(result, removePassword, serializer, CloneDbSource),
                            enSourceType.EmailSource => CloneAndSerialize<EmailSource>(result, removePassword, serializer, CloneEmailSource),
                            enSourceType.WebSource => CloneAndSerialize<WebSource>(result, removePassword, serializer, CloneWebSource),
                            enSourceType.RedisSource => CloneAndSerialize<RedisSource>(result, removePassword, serializer, CloneRedisSource),
                            enSourceType.RabbitMQSource => CloneAndSerialize<RabbitMQSource>(result, removePassword, serializer, CloneRabbitMQSource),
                            enSourceType.ElasticsearchSource => CloneAndSerialize<ElasticsearchSource>(result, removePassword, serializer, CloneElasticsearchSource),
                            enSourceType.ExchangeSource => CloneAndSerialize<ExchangeSource>(result, removePassword, serializer, CloneExchangeSource),
                            enSourceType.SharepointServerSource => CloneAndSerialize<SharepointSource>(result, removePassword, serializer, CloneSharepointSource),
                            _ => serializer.SerializeToBuilder(result)
                        };
                    }
                }
                return new StringBuilder();
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        /// <summary>
        /// Clones and serializes a list of objects using a provided clone function.
        /// </summary>
        /// <typeparam name="T">The type of the objects to clone and serialize.</typeparam>
        /// <param name="result">The enumerable collection of objects to process.</param>
        /// <param name="removePassword">Whether to remove passwords during cloning.</param>
        /// <param name="serializer">The serializer to use for converting to StringBuilder.</param>
        /// <param name="cloneFunc">The function to clone each object.</param>
        /// <returns>A StringBuilder containing the serialized list.</returns>
        private StringBuilder CloneAndSerialize<T>(IEnumerable result, bool removePassword, Dev2JsonSerializer serializer, Func<T, bool, T> cloneFunc) where T : class
        {
            var list = result.Cast<T>().Select(res => cloneFunc(res, removePassword)).ToList();
            return serializer.SerializeToBuilder(list);
        }

        /// <summary>
        /// Clones a Connection object, optionally removing the password.
        /// </summary>
        /// <param name="res">The original Connection object to clone.</param>
        /// <param name="removePassword">If true, the password property is set to an empty string.</param>
        /// <returns>A new Connection object with properties copied from the original.</returns>
        private Connection CloneConnection(Connection res, bool removePassword) => new Connection
        {
            // Connection-specific properties
            Address = res.Address,
            AuthenticationType = res.AuthenticationType,
            UserName = res.UserName,
            Password = removePassword ? "" : res.Password,
            WebServerPort = res.WebServerPort,
            
            // ResourceBase properties
            ResourceID = res.ResourceID,
            ResourceName = res.ResourceName,
            ResourceType = res.ResourceType,
            AuthorRoles = res.AuthorRoles,
            FilePath = res.FilePath,
            IsValid = res.IsValid,
            Errors = res.Errors,
            ReloadActions = res.ReloadActions,
            DataList = res.DataList,
            Inputs = res.Inputs,
            Outputs = res.Outputs,
            UserPermissions = res.UserPermissions,
            IsNewResource = res.IsNewResource,
            VersionInfo = res.VersionInfo,
            Dependencies = res.Dependencies
        };

        /// <summary>
        /// Clones a DbSource object, optionally removing the password.
        /// </summary>
        /// <param name="res">The original DbSource object to clone.</param>
        /// <param name="removePassword">If true, the password property is set to an empty string.</param>
        /// <returns>A new DbSource object with properties copied from the original.</returns>
        private DbSource CloneDbSource(DbSource res, bool removePassword) => new DbSource
        {
            // DbSource-specific properties
            ServerType = res.ServerType,
            Server = res.Server,
            DatabaseName = res.DatabaseName,
            Port = res.Port,
            ConnectionTimeout = res.ConnectionTimeout,
            AuthenticationType = res.AuthenticationType,
            UserID = res.UserID,
            Password = removePassword ? "" : res.Password,
            
            // ResourceBase properties
            ResourceID = res.ResourceID,
            ResourceName = res.ResourceName,
            ResourceType = res.ResourceType,
            AuthorRoles = res.AuthorRoles,
            FilePath = res.FilePath,
            IsValid = res.IsValid,
            Errors = res.Errors,
            ReloadActions = res.ReloadActions,
            DataList = res.DataList,
            Inputs = res.Inputs,
            Outputs = res.Outputs,
            UserPermissions = res.UserPermissions,
            IsNewResource = res.IsNewResource,
            VersionInfo = res.VersionInfo,
            Dependencies = res.Dependencies
        };

        /// <summary>
        /// Clones an EmailSource object, optionally removing the password.
        /// </summary>
        /// <param name="res">The original EmailSource object to clone.</param>
        /// <param name="removePassword">If true, the password property is set to an empty string.</param>
        /// <returns>A new EmailSource object with properties copied from the original.</returns>
        private EmailSource CloneEmailSource(EmailSource res, bool removePassword) => new EmailSource
        {
            // EmailSource-specific properties
            Host = res.Host,
            UserName = res.UserName,
            Password = removePassword ? "" : res.Password,
            Port = res.Port,
            EnableSsl = res.EnableSsl,
            Timeout = res.Timeout,
            TestFromAddress = res.TestFromAddress,
            TestToAddress = res.TestToAddress,
            
            // ResourceBase properties
            ResourceID = res.ResourceID,
            ResourceName = res.ResourceName,
            ResourceType = res.ResourceType,
            AuthorRoles = res.AuthorRoles,
            FilePath = res.FilePath,
            IsValid = res.IsValid,
            Errors = res.Errors,
            ReloadActions = res.ReloadActions,
            DataList = res.DataList,
            Inputs = res.Inputs,
            Outputs = res.Outputs,
            UserPermissions = res.UserPermissions,
            IsNewResource = res.IsNewResource,
            VersionInfo = res.VersionInfo,
            Dependencies = res.Dependencies
        };

        /// <summary>
        /// Clones a WebSource object, optionally removing the password.
        /// </summary>
        /// <param name="res">The original WebSource object to clone.</param>
        /// <param name="removePassword">If true, the password property is set to an empty string.</param>
        /// <returns>A new WebSource object with properties copied from the original.</returns>
        private WebSource CloneWebSource(WebSource res, bool removePassword) => new WebSource
        {
            // WebSource-specific properties
            Address = res.Address,
            DefaultQuery = res.DefaultQuery,
            AuthenticationType = res.AuthenticationType,
            UserName = res.UserName,
            Password = removePassword ? "" : res.Password,
            Response = res.Response,
            
            // ResourceBase properties
            ResourceID = res.ResourceID,
            ResourceName = res.ResourceName,
            ResourceType = res.ResourceType,
            AuthorRoles = res.AuthorRoles,
            FilePath = res.FilePath,
            IsValid = res.IsValid,
            Errors = res.Errors,
            ReloadActions = res.ReloadActions,
            DataList = res.DataList,
            Inputs = res.Inputs,
            Outputs = res.Outputs,
            UserPermissions = res.UserPermissions,
            IsNewResource = res.IsNewResource,
            VersionInfo = res.VersionInfo,
            Dependencies = res.Dependencies
        };

        /// <summary>
        /// Clones a RedisSource object, optionally removing the password.
        /// </summary>
        /// <param name="res">The original RedisSource object to clone.</param>
        /// <param name="removePassword">If true, the password property is set to an empty string.</param>
        /// <returns>A new RedisSource object with properties copied from the original.</returns>
        private RedisSource CloneRedisSource(RedisSource res, bool removePassword) => new RedisSource
        {
            // RedisSource-specific properties
            HostName = res.HostName,
            Port = res.Port,
            AuthenticationType = res.AuthenticationType,
            Password = removePassword ? "" : res.Password,
            
            // ResourceBase properties
            ResourceID = res.ResourceID,
            ResourceName = res.ResourceName,
            ResourceType = res.ResourceType,
            AuthorRoles = res.AuthorRoles,
            FilePath = res.FilePath,
            IsValid = res.IsValid,
            Errors = res.Errors,
            ReloadActions = res.ReloadActions,
            DataList = res.DataList,
            Inputs = res.Inputs,
            Outputs = res.Outputs,
            UserPermissions = res.UserPermissions,
            IsNewResource = res.IsNewResource,
            VersionInfo = res.VersionInfo,
            Dependencies = res.Dependencies
        };

        /// <summary>
        /// Clones a RabbitMQSource object, optionally removing the password.
        /// </summary>
        /// <param name="res">The original RabbitMQSource object to clone.</param>
        /// <param name="removePassword">If true, the password property is set to an empty string.</param>
        /// <returns>A new RabbitMQSource object with properties copied from the original.</returns>
        private RabbitMQSource CloneRabbitMQSource(RabbitMQSource res, bool removePassword) => new RabbitMQSource
        {
            // RabbitMQSource-specific properties
            HostName = res.HostName,
            Port = res.Port,
            UserName = res.UserName,
            Password = removePassword ? "" : res.Password,
            VirtualHost = res.VirtualHost,
            
            // ResourceBase properties
            ResourceID = res.ResourceID,
            ResourceName = res.ResourceName,
            ResourceType = res.ResourceType,
            AuthorRoles = res.AuthorRoles,
            FilePath = res.FilePath,
            IsValid = res.IsValid,
            Errors = res.Errors,
            ReloadActions = res.ReloadActions,
            DataList = res.DataList,
            Inputs = res.Inputs,
            Outputs = res.Outputs,
            UserPermissions = res.UserPermissions,
            IsNewResource = res.IsNewResource,
            VersionInfo = res.VersionInfo,
            Dependencies = res.Dependencies
        };

        /// <summary>
        /// Clones an ElasticsearchSource object, optionally removing the password.
        /// </summary>
        /// <param name="res">The original ElasticsearchSource object to clone.</param>
        /// <param name="removePassword">If true, the password property is set to an empty string.</param>
        /// <returns>A new ElasticsearchSource object with properties copied from the original.</returns>
        private ElasticsearchSource CloneElasticsearchSource(ElasticsearchSource res, bool removePassword) => new ElasticsearchSource
        {
            // ElasticsearchSource-specific properties
            HostName = res.HostName,
            Port = res.Port,
            SearchIndex = res.SearchIndex,
            AuthenticationType = res.AuthenticationType,
            Username = res.Username,
            Password = removePassword ? "" : res.Password,
            CertificateFingerprint = res.CertificateFingerprint,
            
            // ResourceBase properties
            ResourceID = res.ResourceID,
            ResourceName = res.ResourceName,
            ResourceType = res.ResourceType,
            AuthorRoles = res.AuthorRoles,
            FilePath = res.FilePath,
            IsValid = res.IsValid,
            Errors = res.Errors,
            ReloadActions = res.ReloadActions,
            DataList = res.DataList,
            Inputs = res.Inputs,
            Outputs = res.Outputs,
            UserPermissions = res.UserPermissions,
            IsNewResource = res.IsNewResource,
            VersionInfo = res.VersionInfo,
            Dependencies = res.Dependencies
        };

        /// <summary>
        /// Clones an ExchangeSource object, optionally removing the password.
        /// </summary>
        /// <param name="res">The original ExchangeSource object to clone.</param>
        /// <param name="removePassword">If true, the password property is set to an empty string.</param>
        /// <returns>A new ExchangeSource object with properties copied from the original.</returns>
        private ExchangeSource CloneExchangeSource(ExchangeSource res, bool removePassword) => new ExchangeSource
        {
            // ExchangeSource-specific properties
            AutoDiscoverUrl = res.AutoDiscoverUrl,
            UserName = res.UserName,
            Password = removePassword ? "" : res.Password,
            Timeout = res.Timeout,
            EmailFrom = res.EmailFrom,
            EmailTo = res.EmailTo,
            TestFromAddress = res.TestFromAddress,
            TestToAddress = res.TestToAddress,
            
            // ResourceBase properties
            ResourceID = res.ResourceID,
            ResourceName = res.ResourceName,
            ResourceType = res.ResourceType,
            AuthorRoles = res.AuthorRoles,
            FilePath = res.FilePath,
            IsValid = res.IsValid,
            Errors = res.Errors,
            ReloadActions = res.ReloadActions,
            DataList = res.DataList,
            Inputs = res.Inputs,
            Outputs = res.Outputs,
            UserPermissions = res.UserPermissions,
            IsNewResource = res.IsNewResource,
            VersionInfo = res.VersionInfo,
            Dependencies = res.Dependencies
        };

        /// <summary>
        /// Clones a SharepointSource object, optionally removing the password.
        /// </summary>
        /// <param name="res">The original SharepointSource object to clone.</param>
        /// <param name="removePassword">If true, the password property is set to an empty string.</param>
        /// <returns>A new SharepointSource object with properties copied from the original.</returns>
        private SharepointSource CloneSharepointSource(SharepointSource res, bool removePassword) => new SharepointSource
        {
            // SharepointSource-specific properties
            Server = res.Server,
            AuthenticationType = res.AuthenticationType,
            UserName = res.UserName,
            Password = removePassword ? "" : res.Password,
            IsSharepointOnline = res.IsSharepointOnline,
            
            // ResourceBase properties
            ResourceID = res.ResourceID,
            ResourceName = res.ResourceName,
            ResourceType = res.ResourceType,
            AuthorRoles = res.AuthorRoles,
            FilePath = res.FilePath,
            IsValid = res.IsValid,
            Errors = res.Errors,
            ReloadActions = res.ReloadActions,
            DataList = res.DataList,
            Inputs = res.Inputs,
            Outputs = res.Outputs,
            UserPermissions = res.UserPermissions,
            IsNewResource = res.IsNewResource,
            VersionInfo = res.VersionInfo,
            Dependencies = res.Dependencies
        };

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FindSourcesByType";
    }
}
