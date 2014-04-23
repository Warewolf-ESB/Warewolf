using System.Text;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Utils
{
    /// <summary>
    /// Summary description for Dev2XamlLoaderTest
    /// </summary>
    [TestClass]
    public class Dev2XamlCleanerTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2XamlCleaner_StripNaughtyNamespaces")]
        public void Dev2XamlCleaner_StripNaughtyNamespaces_WhenContainsBadNamespace_ExpectNamespaceRemoved()
        {
            //------------Setup for test--------------------------
            var dev2XamlCleaner = new Dev2XamlCleaner();
            var data = new StringBuilder(ParserStrings.XamlLoaderBadNamespace);

            //------------Execute Test---------------------------
            var result = dev2XamlCleaner.StripNaughtyNamespaces(data);

            //------------Assert Results-------------------------
            int indexOf = result.ToString().IndexOf("clr-namespace:clr-namespace:Unlimited.Framework;assembly=Dev2.Core", System.StringComparison.Ordinal);
            int indexOf2 = result.ToString().IndexOf(@"<Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" />", System.StringComparison.Ordinal);

            Assert.IsTrue(indexOf < 0, "Dev2.Core Namespace was left behind");
            Assert.IsTrue(indexOf2 < 0, "UnlimitedObject d was left behind");
        }
    }
}
