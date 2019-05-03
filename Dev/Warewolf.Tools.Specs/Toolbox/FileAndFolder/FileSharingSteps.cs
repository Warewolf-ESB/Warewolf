using Dev2.Core.Tests;
using System.IO;
using TechTalk.SpecFlow;

namespace Warewolf.Tools.Specs.Toolbox.FileAndFolder
{
    [Binding]
    public sealed class FileSharingSteps
    {
        [Given(@"I authenticate for share at ""(.*)"" as user ""(.*)"" with saved password")]
        public void GivenIAuthenticateForShareAtAsUserWithPassword(string p0, string p1)
        {
            const string passwordsPath = @"\\rsaklfsvrdev.dev2.local\Git-Repositories\Warewolf\.passwords";
            if (File.Exists(passwordsPath))
            {
                var usernamesAndPasswords = File.ReadAllLines(passwordsPath);
                foreach(var usernameAndPassword in usernamesAndPasswords)
                {
                    var password = usernameAndPassword.Split('=');
                    if (password.Length > 1 && password[0] == p1)
                    {
                        FileSystemQueryTest.AuthenticateForSharedFolder(p0, p1, password[1]);
                    }
                }
            }
        }
    }
}
