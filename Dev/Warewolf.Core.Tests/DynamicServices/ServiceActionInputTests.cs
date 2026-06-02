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

namespace Dev2.Tests
{
    [TestClass]
    public class ServiceActionInputTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ServiceActionInput))]
        public void ServiceActionInput_Constructor_SetsObjectTypeAndAllocatesValidators()
        {
            var input = new ServiceActionInput();

            Assert.AreEqual(enDynamicServiceObjectType.ServiceActionInput, input.ObjectType);
            Assert.IsNotNull(input.Validators);
            Assert.AreEqual(0, input.Validators.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ServiceActionInput))]
        public void ServiceActionInput_Properties_GetSet_RoundTrip()
        {
            var input = new ServiceActionInput
            {
                NativeType = "System.String",
                EmptyToNull = true,
                Source = "FromCaller",
                Value = 42,
                DefaultValue = "fallback"
            };
            input.Validators.Add(new Validator());

            Assert.AreEqual("System.String", input.NativeType);
            Assert.IsTrue(input.EmptyToNull);
            Assert.AreEqual("FromCaller", input.Source);
            Assert.AreEqual(42, input.Value);
            Assert.AreEqual("fallback", input.DefaultValue);
            Assert.AreEqual(1, input.Validators.Count);
        }
    }
}
