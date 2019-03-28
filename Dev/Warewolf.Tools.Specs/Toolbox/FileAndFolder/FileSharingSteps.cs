using Dev2.Core.Tests;
using TechTalk.SpecFlow;

namespace Warewolf.Tools.Specs.Toolbox.FileAndFolder
{
    [Binding]
    public sealed class FileSharingSteps
    {
        [Given(@"I authenticate for share at ""(.*)"" as user ""(.*)"" with password ""(.*)""")]
        public void GivenIAuthenticateForShareAtAsUserWithPassword(string p0, string p1, string p2)
        {
            FileSystemQueryTest.AuthenticateForSharedFolder(p0, p1, p2);
        }
    }
}
