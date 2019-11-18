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
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace Dev2.Runtime.ServiceModel
{

    public class RedisSources : ExceptionManager
    {
        public RedisSources()
            : this(ResourceCatalog.Instance)
        {
        }

        public RedisSources(IResourceCatalog resourceCatalog)
        {
            if (resourceCatalog == null)
            {
                throw new ArgumentNullException(nameof(resourceCatalog));
            }
        }
        public RedisSource Get(string resourceId, Guid workspaceId)
        {
            var result = new RedisSource();
            try
            {
                var xmlStr = ResourceCatalog.Instance.GetResourceContents(workspaceId, Guid.Parse(resourceId)).ToString();
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new RedisSource(xml);
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
                var source = JsonConvert.DeserializeObject<RedisSource>(args);
                return CanConnectServer(source);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message };
            }
        }

        public ValidationResult Test(RedisSource source)
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

        static ValidationResult CanConnectServer(RedisSource redisSource)
        {
            try
            {
                RedisClient clientAnon;
                if (redisSource.AuthenticationType == AuthenticationType.Anonymous)
                {
                    clientAnon = new RedisClient(redisSource.HostName, int.Parse(redisSource.Port));
                }
                else
                {
                    clientAnon = new RedisClient(redisSource.HostName, int.Parse(redisSource.Port), redisSource.Password);
                }
                var isValid = false;
                var errorMessage = "";
                if (clientAnon.ServerVersion != null)
                {
                    isValid = true;
                }
                else
                {
                    errorMessage = "No such host can be found.";
                }

                return new ValidationResult
                {
                    IsValid = isValid,
                    ErrorMessage = errorMessage
                };
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