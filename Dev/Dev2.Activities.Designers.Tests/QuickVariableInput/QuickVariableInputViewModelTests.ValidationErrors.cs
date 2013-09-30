using Dev2.Activities.Designers2.Core.QuickVariableInput;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.QuickVariableInput
{
    public partial class QuickVariableInputViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_ErrorInPrefix_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Custo@mer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Prefix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsPrefixFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_InvalidCharsInPrefixTwoDots_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer()..",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Prefix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsPrefixFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_InvalidCharsInPrefix_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Cust<>omer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Prefix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsPrefixFocused);

            qviViewModel.Prefix = "Cust(";
            qviViewModel.IsPrefixFocused = false;
            VerifyValidationErrors(qviViewModel, "Prefix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsPrefixFocused);

            qviViewModel.Prefix = "Cust()";
            qviViewModel.IsPrefixFocused = false;
            VerifyValidationErrors(qviViewModel, "Prefix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsPrefixFocused);

            qviViewModel.Prefix = "Customer(x).";
            qviViewModel.IsPrefixFocused = false;
            VerifyValidationErrors(qviViewModel, "Prefix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsPrefixFocused);

            qviViewModel.Prefix = "Customer(-1).";
            qviViewModel.IsPrefixFocused = false;
            VerifyValidationErrors(qviViewModel, "Prefix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsPrefixFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_InvalidCharsInSuffix_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "@",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Suffix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsSuffixFocused);

            qviViewModel.Suffix = ".";
            qviViewModel.IsSuffixFocused = false;
            VerifyValidationErrors(qviViewModel, "Suffix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsSuffixFocused);

            qviViewModel.Suffix = " ";
            qviViewModel.IsSuffixFocused = false;
            VerifyValidationErrors(qviViewModel, "Suffix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsSuffixFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_NegativeNumberForIndexSplit_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Index",
                SplitToken = "-1",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Please supply a whole positive number for an Index split");
            Assert.IsTrue(qviViewModel.IsSplitOnFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_DecimalNumberForIndexSplit_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Index",
                SplitToken = "100.3000",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Please supply a whole positive number for an Index split");
            Assert.IsTrue(qviViewModel.IsSplitOnFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_TextForIndexSplit_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Index",
                SplitToken = "text",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Please supply a whole positive number for an Index split");
            Assert.IsTrue(qviViewModel.IsSplitOnFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_BlankValueForCharSplit_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = "",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Please supply a value for a Character split");
            Assert.IsTrue(qviViewModel.IsSplitOnFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_BlankValueInVariableList_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Variable List String can not be blank/empty");
            Assert.IsTrue(qviViewModel.IsVariableListFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_FunnyRecordsetNotationInPrefix_ContainsError()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().Other<>text",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            VerifyValidationErrors(qviViewModel, "Prefix contains invalid characters");
            Assert.IsTrue(qviViewModel.IsPrefixFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ValidationErrors")]
        public void QuickVariableInputViewModel_ValidationErrors_SetsHelpErrors()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().Other<>text",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            qviViewModel.Validate();
            Assert.IsNotNull(qviViewModel.Errors);
        }


        static void VerifyValidationErrors(QuickVariableInputViewModel qviViewModel, string message)
        {
            qviViewModel.Validate();
            var errors = qviViewModel.Errors;
            Assert.AreEqual(1, errors.Count);

            var error = errors[0];
            Assert.AreEqual(message, error.Message);
            error.Do();
        }
    }
}
