/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Common.Interfaces.NetStandard20;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class WebExecuteStringArgsTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebExecuteStringArgs))]
        public void WebExecuteStringArgs_AllProperties_RoundTrip()
        {
            var factory = new Mock<IWebRequestFactory>().Object;
            var formData = new List<IFormDataParameters>();

            var args = new WebExecuteStringArgs
            {
                FormDataParameters = formData,
                WebRequestFactory = factory,
                IsManualChecked = true,
                IsFormDataChecked = true,
                IsUrlEncodedChecked = true,
            };

            Assert.AreSame(formData, args.FormDataParameters);
            Assert.AreSame(factory, args.WebRequestFactory);
            Assert.IsTrue(args.IsManualChecked);
            Assert.IsTrue(args.IsFormDataChecked);
            Assert.IsTrue(args.IsUrlEncodedChecked);
        }
    }
}
