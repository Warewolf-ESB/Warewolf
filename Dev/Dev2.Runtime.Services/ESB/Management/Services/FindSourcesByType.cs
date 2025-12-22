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
                        
                        // Cast IEnumerable to specific type based on sourceType and create definition instances
                        switch (sourceType)
                        {
                            case enSourceType.SqlDatabase:
                            case enSourceType.MySqlDatabase:
                            case enSourceType.PostgreSQL:
                            case enSourceType.Oracle:
                            case enSourceType.ODBC:
                            {
                                var list = result.Cast<DbSource>().Select(res =>
                                {
                                    return new DbSourceDefinition
                                    {
                                        AuthenticationType = res.AuthenticationType,
                                        DbName = res.DatabaseName,
                                        Id = res.ResourceID,
                                        Name = res.ResourceName,
                                        Path = res.GetSavePath(),
                                        Password = removePassword ? "" : res.Password,
                                        ConnectionTimeout = res.ConnectionTimeout,
                                        ServerName = res.Server,
                                        Type = res.ServerType,
                                        UserName = res.UserID
                                    };
                                }).ToList();

                                return serializer.SerializeToBuilder(new ExecuteMessage { HasError = false, Message = serializer.SerializeToBuilder(list) });
                            }

                            case enSourceType.EmailSource:
                            {
                                var list = result.Cast<EmailSource>().Select(res =>
                                {
                                    return new EmailSource
                                    {
                                        ResourceID = res.ResourceID,
                                        ResourceName = res.ResourceName,
                                        ResourceType = res.ResourceType,
                                        Host = res.Host,
                                        UserName = res.UserName,
                                        Password = removePassword ? "" : res.Password,
                                        Port = res.Port,
                                        EnableSsl = res.EnableSsl,
                                        Timeout = res.Timeout
                                    };
                                }).ToList();

                                return serializer.SerializeToBuilder(list);
                            }

                            case enSourceType.WebSource:
                            {
                                var list = result.Cast<WebSource>().Select(res =>
                                {
                                    return new WebSource
                                    {
                                        ResourceID = res.ResourceID,
                                        ResourceName = res.ResourceName,
                                        ResourceType = res.ResourceType,
                                        Address = res.Address,
                                        DefaultQuery = res.DefaultQuery,
                                        AuthenticationType = res.AuthenticationType,
                                        UserName = res.UserName,
                                        Password = removePassword ? "" : res.Password
                                    };
                                }).ToList();

                                return serializer.SerializeToBuilder(list);
                            }

                            case enSourceType.RedisSource:
                            {
                                var list = result.Cast<RedisSource>().Select(res =>
                                {
                                    return new RedisSource
                                    {
                                        ResourceID = res.ResourceID,
                                        ResourceName = res.ResourceName,
                                        ResourceType = res.ResourceType,
                                        HostName = res.HostName,
                                        Port = res.Port,
                                        AuthenticationType = res.AuthenticationType,
                                        Password = removePassword ? "" : res.Password
                                    };
                                }).ToList();

                                return serializer.SerializeToBuilder(list);
                            }

                            case enSourceType.RabbitMQSource:
                            {
                                var list = result.Cast<RabbitMQSource>().Select(res =>
                                {
                                    return new RabbitMQSource
                                    {
                                        ResourceID = res.ResourceID,
                                        ResourceName = res.ResourceName,
                                        ResourceType = res.ResourceType,
                                        HostName = res.HostName,
                                        Port = res.Port,
                                        UserName = res.UserName,
                                        Password = removePassword ? "" : res.Password,
                                        VirtualHost = res.VirtualHost
                                    };
                                }).ToList();

                                return serializer.SerializeToBuilder(list);
                            }

                            case enSourceType.ElasticsearchSource:
                            {
                                var list = result.Cast<ElasticsearchSource>().Select(res =>
                                {
                                    return new ElasticsearchSource
                                    {
                                        ResourceID = res.ResourceID,
                                        ResourceName = res.ResourceName,
                                        ResourceType = res.ResourceType,
                                        HostName = res.HostName,
                                        Port = res.Port,
                                        SearchIndex = res.SearchIndex,
                                        AuthenticationType = res.AuthenticationType,
                                        Username = res.Username,
                                        Password = removePassword ? "" : res.Password
                                    };
                                }).ToList();

                                return serializer.SerializeToBuilder(list);
                            }

                            case enSourceType.ExchangeSource:
                            {
                                var list = result.Cast<ExchangeSource>().Select(res =>
                                {
                                    return new ExchangeSource
                                    {
                                        ResourceID = res.ResourceID,
                                        ResourceName = res.ResourceName,
                                        ResourceType = res.ResourceType,
                                        AutoDiscoverUrl = res.AutoDiscoverUrl,
                                        UserName = res.UserName,
                                        Password = removePassword ? "" : res.Password,
                                        Timeout = res.Timeout
                                    };
                                }).ToList();

                                return serializer.SerializeToBuilder(list);
                            }

                            case enSourceType.SharepointServerSource:
                            {
                                var list = result.Cast<SharepointSource>().Select(res =>
                                {
                                    return new SharepointSource
                                    {
                                        ResourceID = res.ResourceID,
                                        ResourceName = res.ResourceName,
                                        ResourceType = res.ResourceType,
                                        Server = res.Server,
                                        AuthenticationType = res.AuthenticationType,
                                        UserName = res.UserName,
                                        Password = removePassword ? "" : res.Password,
                                        IsSharepointOnline = res.IsSharepointOnline
                                    };
                                }).ToList();

                                return serializer.SerializeToBuilder(list);
                            }

                            default:
                            {
                                // For other source types without Password property (e.g., OauthSource, Dev2Server, PluginSource)
                                return serializer.SerializeToBuilder(result);
                            }
                        }
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

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FindSourcesByType";
    }
}
