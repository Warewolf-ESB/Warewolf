
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.ObjectModel;
using Dev2.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.Designers2.Core
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class VariableUtilsTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("VariableUtils_TryParseVariables")]
        public void VariableUtils_TryParseVariables_InputValueIsNullOrEmpty_NoErrors()
        {
            //------------Setup for test--------------------------
            string outputValue;

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseVariables(null, out outputValue, () => { });

            //------------Assert Results-------------------------
            Assert.IsNull(error);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("VariableUtils_TryParseVariables")]
        public void VariableUtils_TryParseVariables_InputValueIsInvalidExpression_HasErrors()
        {
            //------------Setup for test--------------------------
            string outputValue;

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseVariables("a]]", out outputValue, () => { });

            //------------Assert Results-------------------------
            Assert.IsNotNull(error);
            Assert.AreEqual("Invalid expression: opening and closing brackets don't match.", error.Message);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("VariableUtils_TryParseVariables")]
        public void VariableUtils_TryParseVariables_InputValueIsValidExpressionAndNoInputs_UsesVariableValueAndHasNoErrors()
        {
            //------------Setup for test--------------------------
            string outputValue;
            string variableValue = "xxx";

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseVariables("[[a]]", out outputValue, () => { }, variableValue: variableValue);

            //------------Assert Results-------------------------
            Assert.IsNull(error);
            Assert.AreEqual(variableValue, outputValue);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("VariableUtils_TryParseVariables")]
        public void VariableUtils_TryParseVariables_InputValueIsValidExpressionAndHasInputs_UsesInputsValueAndHasNoErrors()
        {
            //------------Setup for test--------------------------
            string outputValue;
            string variableValue = "xxx";

            var inputs = new ObservableCollection<ObservablePair<string, string>>();
            inputs.Add(new ObservablePair<string, string>("[[a]]", variableValue));

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseVariables("[[a]]", out outputValue, () => { }, variableValue: "a", inputs: inputs);

            //------------Assert Results-------------------------
            Assert.IsNull(error);
            Assert.AreEqual(variableValue, outputValue);
        }
    }
}
