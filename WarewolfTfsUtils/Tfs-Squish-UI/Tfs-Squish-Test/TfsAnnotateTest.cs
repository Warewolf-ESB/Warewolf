using System;
using System.IO;
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

            TextWriter tw = new TestingTextWriter();

            TfsAnnotate tfsAnn = new TfsAnnotate("http://rsaklfsvrgendev:8080/tfs");

            tfsAnn.FetchAnnotateInfo(@"C:\Development\Dev\Dev2.Server\WebServer.cs", tw, false);

            var tmp = (tw as TestingTextWriter);

            var data = tmp.FetchContents();

            Console.WriteLine(data);

        }
    }
}
