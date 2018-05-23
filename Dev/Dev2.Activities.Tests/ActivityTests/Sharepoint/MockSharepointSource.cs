using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;

namespace Dev2.Tests.Activities.ActivityTests.Sharepoint
{
    public partial class SharepointCopyFileActivityTests
    {
        public class MockSharepointSource : SharepointSource
        {
            public ISharepointHelper MockSharepointHelper { get; set; }
            public override ISharepointHelper CreateSharepointHelper()
            {
                return MockSharepointHelper;
            }
        }
    }
}
