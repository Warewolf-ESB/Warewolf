/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    public interface IPluginServices
    {
        RecordsetList Test(string args, out string serializedResult);
        NamespaceList Namespaces(PluginSource args, Guid workspaceId, Guid dataListId);
        ServiceMethodList Methods(PluginService args, Guid workspaceId, Guid dataListId);
        ServiceConstructorList Constructors(PluginService args, Guid workspaceId, Guid dataListId);
    }

    public class PluginServices : Services, IPluginServices
    {
        #region CTOR

        public PluginServices()
        {
        }

        public PluginServices(IResourceCatalog resourceCatalog, IAuthorizationService authorizationService)
            : base(resourceCatalog, authorizationService)
        {
        }

        #endregion

        #region DeserializeService

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        protected virtual Service DeserializeService(string args)
        {
            return JsonConvert.DeserializeObject<PluginService>(args);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        protected virtual Service DeserializeService(XElement xml, string resourceType)
        {
            return xml == null ? new PluginService() : new PluginService(xml);
        }

        #endregion

        #region Test

        // POST: Service/PluginServices/Test
        public RecordsetList Test(string args, out string serializedResult)
        {
            try
            {
           
                
                var service = JsonConvert.DeserializeObject<PluginService>(args,new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
                var fetchRecordset = FetchRecordset(service, true);
                serializedResult = service.SerializedResult;
                return fetchRecordset;
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                serializedResult = null;
                return new RecordsetList { new Recordset { HasErrors = true, ErrorMessage = ex.Message } };
            }
        }

        #endregion

        #region Namespaces

        // POST: Service/PluginServices/Namespaces
        public virtual NamespaceList Namespaces(PluginSource pluginSource, Guid workspaceId, Guid dataListId)
        {
            var result = new NamespaceList();
            try
            {

                if (pluginSource != null)
                {
                    var broker = new PluginBroker();
                    return broker.GetNamespaces(pluginSource);
                }
            }
            catch (BadImageFormatException e)
            {
                RaiseError(e);
                throw;
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }
        
        // POST: Service/PluginServices/Namespaces
        public virtual NamespaceList NamespacesWithJsonObjects(PluginSource pluginSource, Guid workspaceId, Guid dataListId)
        {
            var result = new NamespaceList();
            try
            {

                if (pluginSource != null)
                {
                    var broker = new PluginBroker();
                    return broker.GetNamespacesWithJsonObjects(pluginSource);
                }
            }
            catch (BadImageFormatException e)
            {
                RaiseError(e);
                throw;
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Methods

        // POST: Service/PluginServices/Methods
        public ServiceMethodList Methods(PluginService service, Guid workspaceId, Guid dataListId)
        {
            var result = new ServiceMethodList();
            try
            {
                // BUG 9500 - 2013.05.31 - TWR : changed to use PluginService as args 
              
                var broker = new PluginBroker();
                result = broker.GetMethods(((PluginSource)service.Source).AssemblyLocation, ((PluginSource)service.Source).AssemblyName, service.Namespace);
                return result;
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }
        // POST: Service/PluginServices/MethodsWithReturns
        public ServiceMethodList MethodsWithReturns(PluginService service, Guid workspaceId, Guid dataListId)
        {
            var result = new ServiceMethodList();
            try
            {
                // BUG 9500 - 2013.05.31 - TWR : changed to use PluginService as args 
              
                var broker = new PluginBroker();
                result = broker.GetMethodsWithReturns(((PluginSource)service.Source).AssemblyLocation, ((PluginSource)service.Source).AssemblyName, service.Namespace);
                return result;
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        public ServiceConstructorList Constructors(PluginService service, Guid workspaceId, Guid dataListId)
        {
            var result = new ServiceConstructorList();
            try
            {
                // BUG 9500 - 2013.05.31 - TWR : changed to use PluginService as args 

                var broker = new PluginBroker();
                result = broker.GetConstructors(((PluginSource)service.Source).AssemblyLocation, ((PluginSource)service.Source).AssemblyName, service.Namespace);
                return result;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion
    }
   
}
