using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.Core;
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
            string outputValue;

            //------------Execute Test---------------------------
            var errors = VariableUtils.TryParseVariables(null, out outputValue, () => { });

            //------------Assert Results-------------------------
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("VariableUtils_TryParseVariables")]
        public void VariableUtils_TryParseVariables_InputValueIsInvalidExpression_HasErrors()
        {
            //------------Setup for test--------------------------
            string outputValue;

            //------------Execute Test---------------------------
            var errors = VariableUtils.TryParseVariables("a]]", out outputValue, () => { });

            //------------Assert Results-------------------------
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Invalid expression: opening and closing brackets don't match.", errors[0].Message);
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
            var errors = VariableUtils.TryParseVariables("[[a]]", out outputValue, () => { }, variableValue);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, errors.Count);
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

            var inputs = new ObservableCollection<ObservablePair<string,string>>();
            inputs.Add(new ObservablePair<string, string>("[[a]]", variableValue));

            //------------Execute Test---------------------------
            var errors = VariableUtils.TryParseVariables("[[a]]", out outputValue, () => { }, "a", inputs);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, errors.Count);
            Assert.AreEqual(variableValue, outputValue);
        }
    }
}
