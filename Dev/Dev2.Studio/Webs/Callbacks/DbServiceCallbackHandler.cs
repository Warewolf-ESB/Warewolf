using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Webs.Callbacks
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
