/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.DynamicServices
{
    [TestClass]
    public class ValidatorTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Validator))]
        public void Validator_Constructor_SetsObjectTypeToValidator()
        {
            var validator = new Validator();

            Assert.AreEqual(enDynamicServiceObjectType.Validator, validator.ObjectType);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Validator))]
        public void Validator_ValidatorType_GetSet_RoundTrip()
        {
            var validator = new Validator { ValidatorType = enValidationType.RequiredAndRegex };

            Assert.AreEqual(enValidationType.RequiredAndRegex, validator.ValidatorType);
        }
    }
}
