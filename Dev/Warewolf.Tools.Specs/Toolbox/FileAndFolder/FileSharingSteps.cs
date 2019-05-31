using Dev2.Core.Tests;
using Dev2.Infrastructure.Tests;
using TechTalk.SpecFlow;

namespace Warewolf.Tools.Specs.Toolbox.FileAndFolder
{
    [Binding]
    public sealed class FileSharingSteps
    {
        [Given(@"I authenticate for share at ""(.*)"" as user ""(.*)"" with saved password")]
        public void GivenIAuthenticateForShareAtAsUserWithPassword(string p0, string p1) => FileSystemQueryTest.AuthenticateForSharedFolder(p0, p1, TestEnvironmentVariables.GetVar(p1));
    }
}
