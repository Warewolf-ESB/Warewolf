using System;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;

namespace Dev2.Studio.Core.Factories
{
    public static class EnvironmentModelFactory
    {
        //public static IEnvironmentModel CreateEnvironmentModel(Guid id, Uri applicationServerUri, string alias, int webServerPort,
        //    IFrameworkSecurityContext securityContext, IEventAggregator eventAggregator)
        //{
        //    var environmentConnection = new TcpConnection(securityContext, applicationServerUri, webServerPort, eventAggregator);
        //    return new EnvironmentModel(environmentConnection) { ID = id, Name = alias };
        //}

        //public static IEnvironmentModel CreateEnvironmentModel(Guid id, Uri applicationServerUri, string alias, int webServerPort)
        //{
        //    var eventAggregator = ImportService.GetExportValue<IEventAggregator>();
        //    var securityContext = ImportService.GetExportValue<IFrameworkSecurityContext>();

        //    return CreateEnvironmentModel(id, applicationServerUri, alias, webServerPort, securityContext, eventAggregator);
        //}

        //public static IEnvironmentModel CreateEnvironmentModel(IServer server)
        //{
        //    IEnvironmentModel environment = null;

        //    if(server != null)
        //    {
        //        Guid id;
        //        if(!Guid.TryParse(server.ID, out id))
        //        {
        //            id = Guid.NewGuid();
        //        }
        //        environment = CreateEnvironmentModel(id, server.AppUri, server.Alias, server.WebUri.Port);
        //        server.Environment = environment;
        //    }

        //    return environment;
        //}

        //public static IEnvironmentModel CreateEnvironmentModel(IEnvironmentModel sourceEnvironment)
        //{
        //    IEnvironmentModel environment = null;

        //    if(sourceEnvironment != null)
        //    {
        //        environment = CreateEnvironmentModel(Guid.NewGuid(),
        //            sourceEnvironment.Connection.AppServerUri,
        //            sourceEnvironment.Name,
        //            sourceEnvironment.Connection.WebServerUri.Port,
        //            sourceEnvironment.Connection.SecurityContext,
        //            sourceEnvironment.Connection.EventAggregator);
        //    }

        //    return environment;
        //}
    }
}
