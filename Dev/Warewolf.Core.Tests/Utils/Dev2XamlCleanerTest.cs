/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core.Tests;

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
            var indexOf = result.ToString().IndexOf("clr-namespace:clr-namespace:Unlimited.Framework;assembly=Dev2.Core", StringComparison.Ordinal);
            var indexOf2 = result.ToString().IndexOf(@"<Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" />", StringComparison.Ordinal);

            Assert.IsTrue(indexOf < 0, "Dev2.Core Namespace was left behind");
            Assert.IsTrue(indexOf2 < 0, "UnlimitedObject d was left behind");
        }
    }
}
