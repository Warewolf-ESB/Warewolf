/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class WebPostInputRegionTest
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostInputRegion))]
        public void WebPostInputRegion_TestInputCtor()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            var webSourceRegion = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), webSourceRegion);
            Assert.AreEqual(region.IsEnabled, false);
            Assert.AreEqual(region.Errors.Count, 0);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostInputRegion))]
        public void WebPostInputRegion_TestInputCtorEmpty()
        {
            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            var region = new WebPostInputRegion();
            Assert.AreEqual(region.IsEnabled, false);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostInputRegion))]
        public void WebPostInputRegion_TestClone()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            var webSourceRegion = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), webSourceRegion) { PostData = "bob" };
            Assert.AreEqual(region.IsEnabled, false);
            Assert.AreEqual(region.Errors.Count, 0);
            if (region.CloneRegion() is WebPostInputRegion clone)
            {
                Assert.AreEqual(clone.IsEnabled, false);
                Assert.AreEqual(clone.Errors.Count, 0);
                Assert.AreEqual(clone.PostData, "bob");
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostInputRegion))]
        public void WebPostInputRegion_RestoreFromPrevious_Restore_ExpectValuesChanged()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            var webSourceRegion = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), webSourceRegion);
            var regionToRestore = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), webSourceRegion)
            {
                IsEnabled = true,
                QueryString = "blob",
                Headers = new ObservableCollection<INameValue> {new NameValue("a", "b")}
            };
            //------------Execute Test---------------------------
            region.RestoreRegion(regionToRestore);
            //------------Assert Results-------------------------

            Assert.AreEqual(region.QueryString, "blob");
            Assert.AreEqual(region.Headers.First().Name, "a");
            Assert.AreEqual(region.Headers.First().Value, "b");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostInputRegion))]
        public void WebPostInputRegion_SrcChanged_UpdateValues()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            var lst = new List<IWebServiceSource> { new WebServiceSourceDefinition() { HostName = "bob", DefaultQuery = "Dave" }, new WebServiceSourceDefinition() { HostName = "f", DefaultQuery = "g" } };
            mod.Setup(a => a.RetrieveSources()).Returns(lst);
            var webSourceRegion = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), webSourceRegion);

            webSourceRegion.SelectedSource = lst[0];
            Assert.AreEqual(region.QueryString, "Dave");
            Assert.AreEqual(region.RequestUrl, "bob");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostInputRegion))]
        public void WebPostInputRegion_Headers_AddEmptyHeaders()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var webPostActivity = new DsfWebPostActivity
            {
                SourceId = id,
                Headers = new ObservableCollection<INameValue> { new NameValue("a", "b") },
            };

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());

            var modelItem = ModelItemUtils.CreateModelItem(webPostActivity);

            var webSourceRegion = new WebSourceRegion(mod.Object, modelItem);
            var region = new WebPostInputRegion(modelItem, webSourceRegion);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------

            Assert.AreEqual(2, region.Headers.Count);
            Assert.AreEqual("a", region.Headers[0].Name);
            Assert.AreEqual("b", region.Headers[0].Value);
            Assert.AreEqual("", region.Headers[1].Name);
            Assert.AreEqual("", region.Headers[1].Value);
        }
    }
}