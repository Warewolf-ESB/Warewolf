/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    public class ElasticsearchSources : ExceptionManager
    { 
        public ElasticsearchSources() : this(ResourceCatalog.Instance)
        {
        }
        
        public ElasticsearchSources(IResourceCatalog resourceCatalog)
        {
            if (resourceCatalog == null)
            {
                throw new ArgumentNullException(nameof(resourceCatalog));
            }
        }

         public ElasticsearchSource Get(string resourceId, Guid workspaceId)
        {
            var result = new ElasticsearchSource();
            try
            {
                var xmlStr = ResourceCatalog.Instance.GetResourceContents(workspaceId, Guid.Parse(resourceId)).ToString();
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new ElasticsearchSource(xml);
                }
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        public ValidationResult Test(string args)
        {
            try
            {
                var source = JsonConvert.DeserializeObject<ElasticsearchSource>(args);
                return CanConnectServer(source);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message };
            }
        }

        public ValidationResult Test(ElasticsearchSource source)
        {
            try
            {
                return CanConnectServer(source);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message };
            }
        }

        static ValidationResult CanConnectServer(ElasticsearchSource source)
        {
            try
            {
                var uri = new Uri(source.HostName + ":" + source.Port);  
                bool isValid; 
                var errorMessage = "";
                using (var connectionSettings = new ElasticsearchClientSettings(uri))
                {
                    var settings = connectionSettings.RequestTimeout(TimeSpan.FromMinutes(2));
                    if (source.AuthenticationType == AuthenticationType.Password)
					{
						settings.Authentication(new BasicAuthentication(source.Username, source.Password));
                    }
                    if (source.AuthenticationType == AuthenticationType.API_Key)
                    {
                        settings.Authentication(new ApiKey(source.Password));
                    }
                    if (source.CertificateFingerprint != null)
                    {
                        settings.CertificateFingerprint(source.CertificateFingerprint);
                    }

					var client = new ElasticsearchClient(settings);
                    var result = client.Ping();
                    isValid = result.IsValidResponse;
                    if (!isValid)
                    {
                        errorMessage = "could not connect to elasticsearch Instance";
                    }
                    return new ValidationResult
                    {
                        IsValid = isValid,
                        ErrorMessage = errorMessage
                    };
                }
            }
            catch (Exception e)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = e.InnerException != null ? string.Join(Environment.NewLine, e.Message, e.InnerException.Message) : e.Message
                };
            }
        }
    }
}