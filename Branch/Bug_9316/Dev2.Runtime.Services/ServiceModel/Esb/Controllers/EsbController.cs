using Dev2.Runtime.ServiceModel.Data;
using System;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Esb.Controllers
{
    public class EsbController : IEsbEndpoint
    {
        #region Methods

        public ServiceMethodList GetServiceMethods(Resource resource)
        {
            throw new NotImplementedException();
        }

        public IOutputDescription TestServiceMethod(Resource resource, ServiceMethod serviceMethod)
        {
            throw new NotImplementedException();
        }

        public Guid ExecuteServiceMethod(Resource resource, ServiceMethod serviceMethod)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        //private DatabaseBroker CreateDataBroker(Resource resource)
        //{
        //    if (resource.ResourceType == enSourceType.SqlDatabase)
        //    {
        //        return new MsSqlDataBroker();
        //    }

        //    throw new Exception(string.Format("Cant create a data broker for the resource type '{0}'.", ResourceType));
        //}

        #endregion Private Methods
    }
}
