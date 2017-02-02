/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.TO;
using Dev2.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.Validation
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class IsValidJsonCreateMappingInputRuleTests
    {
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("IsValidJsonCreateMappingInputRule_Check")]
        public void IsValidJsonCreateMappingInputRule_RaiseError()
        {
            //------------Setup for test--------------------------
            var validator = new IsValidJsonCreateMappingInputRule(() => new JsonMappingTo { SourceName = "[[rec()]],[[b]]", DestinationName = "a" });
            //------------Execute Test---------------------------
            var errorInfo = validator.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
        }

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("IsValidJsonCreateMappingInputRule_Check")]
        public void IsValidJsonCreateMappingInputRule_RaisesNoError()
        {
            //------------Setup for test--------------------------
            var validator = new IsValidJsonCreateMappingInputRule(() => new JsonMappingTo { SourceName = "[[rec().x]],[[b]]", DestinationName = "a" });
            //------------Execute Test---------------------------
            var result = validator.Check();
            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

    }
}
