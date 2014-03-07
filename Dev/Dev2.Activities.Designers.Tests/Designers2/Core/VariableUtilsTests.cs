using Dev2.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;

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

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VariableUtils_TryParseRecordsetVariables")]
        public void VariableUtils_TryParseRecordsetVariables_InputValueIsValidRecordsetExpression_HasNoErrors()
        {
            //------------Setup for test--------------------------
            string variableValue = "[[rec().set]]";

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseRecordsetVariables("[[rec().set]]", () => { }, variableValue: variableValue);

            //------------Assert Results-------------------------
            Assert.IsNull(error);
        }
          

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VariableUtils_TryParseRecordsetVariables")]
        public void VariableUtils_TryParseComplexRecordsetVariables_InputValueIsValidRecordsetExpression_HasNoErrors()
        {
            //------------Setup for test--------------------------
            string variableValue = "[[rec([[res().col]]).set]]";

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseRecordsetVariables("[[rec([[res().col]]).set]]", () => { }, variableValue: variableValue);

            //------------Assert Results-------------------------
            Assert.IsNull(error);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VariableUtils_TryParseRecordsetVariables")]
        public void VariableUtils_TryParseRecordsetVariables_RecordsetExpressionWithNegativeIndex_HasErrors()
        {
            //------------Setup for test--------------------------
            string variableValue = "[[rec(-1).set]]";

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseRecordsetVariables("[[rec(-1).set]]", () => { }, variableValue: variableValue);

            //------------Assert Results-------------------------
            Assert.IsNotNull(error);
            Assert.AreEqual("Invalid expression: Recordset index is invalid.", error.Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VariableUtils_TryParseRecordsetVariables")]
        public void VariableUtils_TryParseRecordsetVariables_RecordsetExpressionWithSpecialCharIndex_HasErrors()
        {
            //------------Setup for test--------------------------
            string variableValue = "[[rec(#).set]]";

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseRecordsetVariables("[[rec(#).set]]", () => { }, variableValue: variableValue);

            //------------Assert Results-------------------------
            Assert.IsNotNull(error);
            Assert.AreEqual("Invalid expression: Recordset index is invalid.", error.Message);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VariableUtils_TryParseRecordsetVariables")]
        public void VariableUtils_TryParseComplexRecordsetVariables_RecordsetExpressionWithNegativeIndex_HasErrors()
        {
            //------------Setup for test--------------------------
            string variableValue = "[[rec([[res(-1).set]]).set]]";

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseRecordsetVariables("[[rec([[res(-1).set]]).set]]", () => { }, variableValue: variableValue);

            //------------Assert Results-------------------------
            Assert.IsNotNull(error);
            Assert.AreEqual("Invalid expression: Recordset index is invalid.", error.Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VariableUtils_TryParseVariableSpecialChars")]
        public void VariableUtils_TryParseVariableSpecialChars_RecordsetExpressionWithSpecialChar_HasErrors()
        {
            //------------Setup for test--------------------------
            string variableValue = "[[rec([[res().$et]]).set]]";

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseVariableSpecialChars("[[rec([[res().$et]]).set]]", () => { }, variableValue: variableValue);

            //------------Assert Results-------------------------
            Assert.IsNotNull(error);
            Assert.AreEqual("Invalid expression: Variable has special characters.", error.Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VariableUtils_TryParseVariableSpecialChars")]
        public void VariableUtils_TryParseVariableSpecialChars_VariableExpressionWithSpecialChar_HasErrors()
        {
            //------------Setup for test--------------------------
            string variableValue = "[[$et]]";

            //------------Execute Test---------------------------
            var error = VariableUtils.TryParseVariableSpecialChars("[[$et]]", () => { }, variableValue: variableValue);

            //------------Assert Results-------------------------
            Assert.IsNotNull(error);
            Assert.AreEqual("Invalid expression: Variable has special characters.", error.Message);
        }
    }
}