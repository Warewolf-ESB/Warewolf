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
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
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
                //TODO: Connect to Elasticsearch source via serilog??
                //var isValid = false;
               // var errorMessage = "";
                
                return new ValidationResult
                {
                    IsValid = true,
                    ErrorMessage = ""
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