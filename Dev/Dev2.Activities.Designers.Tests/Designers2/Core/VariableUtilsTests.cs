/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
    
    public class VariableUtilsTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("VariableUtils_TryParseVariables")]
        public void VariableUtils_TryParseVariables_InputValueIsNullOrEmpty_NoErrors()
        {
            //------------Setup for test--------------------------
            var util = new VariableUtils();
            //------------Execute Test---------------------------
            var error = util.TryParseVariables(null, out string outputValue, () => { });

            //------------Assert Results-------------------------
            Assert.IsNull(error);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("VariableUtils_TryParseVariables")]
        public void VariableUtils_TryParseVariables_InputValueIsInvalidExpression_HasErrors()
        {
            //------------Setup for test--------------------------
            var util = new VariableUtils();
            //------------Execute Test---------------------------
            var error = util.TryParseVariables("a]]",out string outputValue, () => { });

            //------------Assert Results-------------------------
            Assert.IsNotNull(error);
            Assert.AreEqual("Result - Invalid expression: opening and closing brackets don't match", error.Message);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("VariableUtils_TryParseVariables")]
        public void VariableUtils_TryParseVariables_InputValueIsValidExpressionAndNoInputs_UsesVariableValueAndHasNoErrors()
        {
            //------------Setup for test--------------------------
            var variableValue = "xxx";
            var util = new VariableUtils();
            //------------Execute Test---------------------------
            var error = util.TryParseVariables("[[a]]",out string outputValue, () => { }, variableValue: variableValue);

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
            var variableValue = "xxx";

            var inputs = new ObservableCollection<ObservablePair<string, string>>();
            inputs.Add(new ObservablePair<string, string>("[[a]]", variableValue));
            var util = new VariableUtils();
            //------------Execute Test---------------------------
            var error = util.TryParseVariables("[[a]]",out string outputValue, () => { }, "a", null, inputs);

            //------------Assert Results-------------------------
            Assert.IsNull(error);
            Assert.AreEqual(variableValue, outputValue);
        }
    }
}
