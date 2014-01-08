using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs.Callbacks
{
    public class DbServiceCallbackHandler : ServiceCallbackHandler
    {

        public DbServiceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public DbServiceCallbackHandler(IEnvironmentRepository currentEnvironmentRepository)
            : base(currentEnvironmentRepository)
        {
        }
    }
}
