using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using System;

namespace Dev2.Studio.Core.Factories
{
    public static class EnvironmentModelFactory
    {
        public static IEnvironmentModel CreateEnvironmentModel(Guid id, Uri applicationServerUri, string alias, int webServerPort)
        {
            IEnvironmentModel environment = new EnvironmentModel();
            ImportService.SatisfyImports(environment);

            environment.ID = id;
            environment.DsfAddress = applicationServerUri;
            environment.Name = alias;
            environment.WebServerPort = webServerPort;

            return environment;
        }

        public static IEnvironmentModel CreateEnvironmentModel(IServer server)
        {
            IEnvironmentModel environment = null;

            if(server != null)
            {
                environment = new EnvironmentModel();
                ImportService.SatisfyImports(environment);

                Guid id;
                if(!Guid.TryParse(server.ID, out id))
                {
                    id = Guid.NewGuid();
                }

                environment.ID = id;
                environment.DsfAddress = server.AppUri;
                environment.Name = server.Alias;
                environment.WebServerPort = server.WebUri.Port;
                server.Environment = environment;
            }

            return environment;
        }

        public static IEnvironmentModel CreateEnvironmentModel(IEnvironmentModel sourceEnvironment)
        {
            IEnvironmentModel environment = new EnvironmentModel();

            if(sourceEnvironment != null)
            {
                ImportService.SatisfyImports(environment);

                environment.ID = Guid.NewGuid();
                environment.DsfAddress = sourceEnvironment.DsfAddress;
                environment.Name = sourceEnvironment.Name;
                environment.WebServerPort = sourceEnvironment.WebServerPort;
            }

            return environment;
        }
    }
}
