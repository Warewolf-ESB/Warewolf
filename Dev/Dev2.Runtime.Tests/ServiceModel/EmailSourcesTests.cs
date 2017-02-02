/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel
{
    // PBI 953 - 2013.05.16 - TWR - Created
    [TestClass]
    public class EmailSourcesTests
    {
        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailSourcesConstructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
            // ReSharper disable once UnusedVariable
            var handler = new EmailSources(null);
        }

        #endregion

        #region Test

        [TestMethod]
        public void EmailSourcesTestWithInValidArgsExpectedInvalidValidationResult()
        {
            var handler = new EmailSources();
            var result = handler.Test("root:'hello'", Guid.Empty, Guid.Empty);
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void EmailSourcesTestWithInvalidHostExpectedInvalidValidationResult()
        {
            var source = new EmailSource { Host = "smtp.foobar.com", Port = 25 }.ToString();

            var handler = new EmailSources();
            var result = handler.Test(source, Guid.Empty, Guid.Empty);
            Assert.IsFalse(result.IsValid, result.ErrorMessage);
        }

        #endregion

        #region Get

        [TestMethod]
        public void EmailSourcesGetWithNullArgsExpectedReturnsNewSource()
        {
            var handler = new EmailSources();
            var result = handler.Get(null, Guid.Empty, Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        public void EmailSourcesGetWithInvalidArgsExpectedReturnsNewSource()
        {
            var handler = new EmailSources();
            var result = handler.Get("xxxxx", Guid.Empty, Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        #endregion
    }
}
