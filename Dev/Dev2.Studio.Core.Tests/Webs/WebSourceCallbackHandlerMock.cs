using Dev2.Studio.Core.Interfaces;
using Dev2.Webs.Callbacks;

namespace Dev2.Core.Tests.Webs
{
    public class WebSourceCallbackHandlerMock : WebSourceCallbackHandler
    {
        public int StartUriProcessHitCount { get; set; }

        public WebSourceCallbackHandlerMock(IEnvironmentRepository environmentRepository)
            : base(environmentRepository)
        {
        }

        protected override void StartUriProcess(string uri)
        {
            StartUriProcessHitCount++;
        }
    }
}
