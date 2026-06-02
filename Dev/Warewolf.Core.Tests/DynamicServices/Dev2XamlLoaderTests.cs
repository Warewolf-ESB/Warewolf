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
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.DynamicServices.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class Dev2XamlLoaderTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2XamlLoader))]
        public void Dev2XamlLoader_Load_NullDefinition_ThrowsArgumentNull()
        {
            var loader = new Dev2XamlLoader();
            Stream xamlStream = null;
            var pool = new Queue<PooledServiceActivity>();
            Activity workflow = null;

            Assert.ThrowsException<ArgumentNullException>(() =>
                loader.Load(null, ref xamlStream, ref pool, ref workflow));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2XamlLoader))]
        public void Dev2XamlLoader_Load_EmptyDefinition_ThrowsArgumentNull()
        {
            var loader = new Dev2XamlLoader();
            Stream xamlStream = null;
            var pool = new Queue<PooledServiceActivity>();
            Activity workflow = null;

            Assert.ThrowsException<ArgumentNullException>(() =>
                loader.Load(new StringBuilder(), ref xamlStream, ref pool, ref workflow));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2XamlLoader))]
        public void Dev2XamlLoader_RemoveWindowsElements_StripsVbSettingsAndSapElementsAndAttributes()
        {
            // Combined fixture exercises all three loops in RemoveWindowsElements:
            //   1. mva:VisualBasic.Settings element removal
            //   2. sap:* element removal
            //   3. sap-namespaced attribute removal
            var input = new StringBuilder(@"<root xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"">
  <mva:VisualBasic.Settings>vb</mva:VisualBasic.Settings>
  <sap:VirtualizedContainerService.HintSize>200,200</sap:VirtualizedContainerService.HintSize>
  <child sap:Something=""value"" Other=""keep"" />
</root>");

            Dev2XamlLoader.RemoveWindowsElements(ref input);
            var result = input.ToString();

            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex("VisualBasic\\.Settings"));
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex("VirtualizedContainerService\\.HintSize"));
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex("sap:Something"));
            StringAssert.Contains(result, "Other=\"keep\"");
        }
    }
}
