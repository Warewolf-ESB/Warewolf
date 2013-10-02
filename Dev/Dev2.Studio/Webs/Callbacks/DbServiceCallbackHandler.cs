using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Webs.Callbacks
{
    public class DbServiceCallbackHandler : ServiceCallbackHandler
    {
        bool _isEditingSource;
        string _returnUri;

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
