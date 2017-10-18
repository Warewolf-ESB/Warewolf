using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dev2.Integration.Tests.Merge
{
    [TestClass]
    public class CustomGitOpsTests
    {
        [TestMethod]
        public void CustomGitOps_ExecutesThreeCommands()
        {
            //------------Setup for test--------------------------
            CustomGitOps.SetCustomGitTool(new ExternalProcessExecutor());
            var userProfile = Environment.SpecialFolder.UserProfile;
            var currentUserProfileRoot = Environment.GetFolderPath(userProfile);
            var globalConfig = Directory.GetFiles(currentUserProfileRoot, ".gitconfig")[0];                     
                        
            using (var a = File.OpenRead(globalConfig))
            {
                StreamReader b = new StreamReader(a);
                var gitText = b.ReadToEnd();
                
                StringAssert.Contains(gitText, "tool = DiffMerge");
                StringAssert.Contains(gitText, "cmd = C:/Program Files (x86)/Warewolf/Studio/customMerge.sh -merge $REMOTE");
                StringAssert.Contains(gitText, "trustExitCode = false");
                StringAssert.Contains(gitText, "[difftool \"DiffMerge\"]");
                StringAssert.Contains(gitText, "[mergetool \"DiffMerge\"]");
            }
        }

        [TestMethod]
        public void GetGitExePath_GetsTheGitexe()
        {
            //------------Setup for test--------------------------
            IExternalProcessExecutor executor = new ExternalProcessExecutor();
            PrivateType privateType = new PrivateType(typeof(CustomGitOps));
            var gitPath = privateType.InvokeStatic("GetGitExePath", executor);
            StringAssert.EndsWith(gitPath.ToString(), "git.exe");

        }


    }
}
