using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.TOTests
{
    /// <summary>
    /// Summary description for DataSplitDTOTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataSplitDTOTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_Constructor")]
        public void DataSplitDTO_Constructor_Default_PropertiesInitializedCorrectly()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------            
            var dto = new DataSplitDTO();

            //------------Assert Results-------------------------
            Assert.AreEqual(DataSplitDTO.SplitTypeIndex, dto.SplitType);
            Assert.IsNull(dto.At);
            Assert.AreEqual(true, dto.EnableAt);
            Assert.AreEqual(true, dto.IsEscapeCharEnabled);
            Assert.AreEqual(0, dto.IndexNumber);
            Assert.IsNull(dto.EscapeChar);
            Assert.AreEqual(false, dto.Include);
            Assert.IsNotNull(dto.Errors);
            Assert.AreEqual(false, dto.IsAtFocused);
            Assert.AreEqual(false, dto.IsOutputVariableFocused);
            Assert.AreEqual(false, dto.IsEscapeCharFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_Constructor")]
        public void DataSplitDTO_Constructor_FullConstructor_PropertiesInitializedCorrectly()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var dto = new DataSplitDTO(string.Empty, null, null, 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(DataSplitDTO.SplitTypeIndex, dto.SplitType);
            Assert.AreEqual(string.Empty, dto.At);
            Assert.AreEqual(true, dto.EnableAt);
            Assert.AreEqual(true, dto.IsEscapeCharEnabled);
            Assert.AreEqual(1, dto.IndexNumber);
            Assert.IsNull(dto.EscapeChar);
            Assert.AreEqual(false, dto.Include);
            Assert.IsNotNull(dto.Errors);
            Assert.AreEqual(false, dto.IsAtFocused);
            Assert.AreEqual(false, dto.IsOutputVariableFocused);
            Assert.AreEqual(false, dto.IsEscapeCharFocused);
        }

        #region CanAdd Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanAdd")]
        public void DataSplitDTO_CanAdd_WithNewLineSplitTypeAndNoOtherValues_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, "NewLine", null, 1);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataSplitDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanAdd")]
        public void DataSplitDTO_CanAdd_WithNoInputVarButValueForAt_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, null, "|", 1);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataSplitDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanAdd")]
        public void DataSplitDTO_CanAdd_WithNoInputVarAndNoAt_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, null, null, 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanAdd")]
        public void DataSplitDTO_CanAdd_WithIndexSplitTypeAndNoOtherValues_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, "Index", null, 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanAdd());
        }

        #endregion

        #region CanRemove Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanRemove")]
        public void DataSplitDTO_CanRemove_WithNoInputVarButValueForAt_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, null, "|", 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanRemove")]
        public void DataSplitDTO_CanRemove_WithNoInputVarAndNoAt_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, null, null, 1);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataSplitDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanRemove")]
        public void DataSplitDTO_CanRemove_WithNewLineTypeAndNoInputVar_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, "NewLine", null, 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanRemove")]
        public void DataSplitDTO_CanRemove_WithNewLineInputTypeAndVar_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO("s", "NewLine", null, 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanRemove());
        }

        #endregion

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_IsEmpty")]
        public void DataSplitDTO_IsEmpty_PropertiesAreEmpty_True()
        {
            Verify_IsEmpty(DataSplitDTO.SplitTypeIndex);
            Verify_IsEmpty(DataSplitDTO.SplitTypeChars);
            Verify_IsEmpty(DataSplitDTO.SplitTypeNone);
        }

        void Verify_IsEmpty(string splitType)
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { SplitType = splitType };

            //------------Execute Test---------------------------
            var actual = dto.IsEmpty();

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_IsEmpty")]
        public void DataSplitDTO_IsEmpty_PropertiesAreNotEmpty_False()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "xxx" };

            //------------Execute Test---------------------------
            var actual = dto.IsEmpty();

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_ClearRow")]
        public void DataSplitDTO_ClearRow_PropertiesAreEmpty()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "xxx", SplitType = DataSplitDTO.SplitTypeNone, Include = true, EscapeChar = "'" };

            Assert.IsFalse(dto.IsEmpty());

            //------------Execute Test---------------------------
            dto.ClearRow();

            //------------Assert Results-------------------------
            Assert.IsTrue(dto.IsEmpty());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_ConvertToOutputTO")]
        public void DataSplitDTO_ConvertToOutputTO_OutputTOPropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[h]]", OutList = new List<string>(new[] { "hello" }) };

            //------------Execute Test---------------------------
            var outputTO = dto.ConvertToOutputTO();

            //------------Assert Results-------------------------
            Assert.IsNotNull(outputTO);
            Assert.AreEqual("[[h]]", outputTO.OutPutDescription);
            Assert.AreSame(dto.OutList, outputTO.OutputStrings);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSet_IsEmptyIsTrue_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO();

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "OutputVariable", null);
            Verify_RuleSet(dto, "At", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetOutputVariable_ExpressionIsInvalid_ValidateRulesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "h]]" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "OutputVariable", "Invalid expression: opening and closing brackets don't match");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetOutputVariable_ExpressionIsValid_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[h]]" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "OutputVariable", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetOutputVariable_IsNotNullOrEmpty_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[h]]", At = "1" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "OutputVariable", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsIndexAndExpressionIsInvalid_ValidateRulesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = "h]]", SplitType = DataSplitDTO.SplitTypeIndex };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", "Invalid expression: opening and closing brackets don't match");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsIndexAndExpressionIsValid_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = "[[h]]", SplitType = DataSplitDTO.SplitTypeIndex };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsIndexAndIsNumeric_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = "1", SplitType = DataSplitDTO.SplitTypeIndex };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsCharsAndExpressionIsInvalid_ValidateRulesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = "h]]", SplitType = DataSplitDTO.SplitTypeChars };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", "Invalid expression: opening and closing brackets don't match");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsCharsAndExpressionIsValid_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = "[[h]]", SplitType = DataSplitDTO.SplitTypeChars };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsCharsAndIsEmpty_ValidateRulesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = "", SplitType = DataSplitDTO.SplitTypeChars };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", "cannot be empty");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsCharsAndIsNotEmpty_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = ",", SplitType = DataSplitDTO.SplitTypeChars };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsIndexAndIsLessThan0_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = "-1", SplitType = DataSplitDTO.SplitTypeIndex };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", " must be a positive whole number");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsIndexAndIsGreaterThan0_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = "5", SplitType = DataSplitDTO.SplitTypeIndex };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataSplitDTO_GetRuleSet")]
        public void DataSplitDTO_GetRuleSetAt_SplitTypeIsIndexAndIs0_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataSplitDTO { OutputVariable = "[[rs]]", At = "0", SplitType = DataSplitDTO.SplitTypeIndex };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", null);
        }

        static void Verify_RuleSet(DataSplitDTO dto, string propertyName, string expectedErrorMessage)
        {

            //------------Execute Test---------------------------
            var ruleSet = dto.GetRuleSet(propertyName);
            var errors = ruleSet.ValidateRules(null, null);

            //------------Assert Results-------------------------
            if(expectedErrorMessage == null)
            {
                Assert.AreEqual(0, errors.Count);
            }
            else
            {
                var err = errors.FirstOrDefault(e => e.Message.Contains(expectedErrorMessage));
                Assert.IsNotNull(err);
            }
        }
    }
}
