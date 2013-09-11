using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tfs.Squish;

namespace Tfs_Squish_Test
{
    [TestClass]
    public class TfsAnnotateTest
    {
        [TestMethod]
        public void CanFetchFileInfo()
        {

            TfsAnnotate tfsAnn = new TfsAnnotate("http://rsaklfsvrgendev:8080/tfs", "bob");

            tfsAnn.MyInvoke("bob", "bob.cs", string.Empty, string.Empty, string.Empty);

        }
    }
}
