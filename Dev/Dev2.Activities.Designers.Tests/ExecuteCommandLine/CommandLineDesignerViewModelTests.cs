using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Activities.Designers2.CommandLine;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.ExecuteCommandLine
{
    [TestClass]
    public class CommandLineDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CommandLineDesignerViewModel_Constructor")]
        public void CommandLineDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new CommandLineDesignerViewModel(CreateModelItem());

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.CommandPriorities);

            // Ensure order is correct
            Assert.AreEqual(ProcessPriorityClass.Normal, viewModel.CommandPriorities[0].Key);
            Assert.AreEqual(ProcessPriorityClass.Idle, viewModel.CommandPriorities[1].Key);
            Assert.AreEqual(ProcessPriorityClass.High, viewModel.CommandPriorities[2].Key);
            Assert.AreEqual(ProcessPriorityClass.RealTime, viewModel.CommandPriorities[3].Key);
            Assert.AreEqual(ProcessPriorityClass.BelowNormal, viewModel.CommandPriorities[4].Key);
            Assert.AreEqual(ProcessPriorityClass.AboveNormal, viewModel.CommandPriorities[5].Key);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CommandLineDesignerViewModel_Validate")]
        public void CommandLineDesignerViewModel_Validate_CommandFileNameIsNull_HasErrors()
        {
            //------------Setup for test--------------------------
            const string CommandFileName = null;
            var viewModel = new CommandLineDesignerViewModel(CreateModelItem(CommandFileName));

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(1, viewModel.Errors.Count);

            var error = viewModel.Errors[0];
            Assert.AreEqual("Command must have a value", error.Message);
            Assert.AreEqual(ErrorType.Critical, error.ErrorType);

            Assert.IsFalse(viewModel.IsCommandFileNameFocused);
            error.Do();
            Assert.IsTrue(viewModel.IsCommandFileNameFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CommandLineDesignerViewModel_Validate")]
        public void CommandLineDesignerViewModel_Validate_CommandFileNameHasInvalidExpression_HasErrors()
        {
            //------------Setup for test--------------------------
            const string CommandFileName = "h]]";
            var viewModel = new CommandLineDesignerViewModel(CreateModelItem(CommandFileName));

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(1, viewModel.Errors.Count);

            var error = viewModel.Errors[0];
            Assert.AreEqual("Invalid expression: opening and closing brackets don't match.", error.Message);
            Assert.AreEqual(ErrorType.Critical, error.ErrorType);

            Assert.IsFalse(viewModel.IsCommandFileNameFocused);
            error.Do();
            Assert.IsTrue(viewModel.IsCommandFileNameFocused);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CommandLineDesignerViewModel_Validate")]
        public void CommandLineDesignerViewModel_Validate_CommandFileNameHasValidExpression_HasNoErrors()
        {
            //------------Setup for test--------------------------
            const string CommandFileName = "[[h]]";
            var viewModel = new CommandLineDesignerViewModel(CreateModelItem(CommandFileName));

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(0, viewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CommandLineDesignerViewModel_Validate")]
        public void CommandLineDesignerViewModel_Validate_ClearsErrorsFirst()
        {
            //------------Setup for test--------------------------
            const string CommandFileName = "h]]";
            var viewModel = new CommandLineDesignerViewModel(CreateModelItem(CommandFileName))
            {
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo() { Message = "Previous Error", ErrorType = ErrorType.Warning}
                }
            };

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(1, viewModel.Errors.Count);

            var error = viewModel.Errors[0];
            Assert.AreEqual("Invalid expression: opening and closing brackets don't match.", error.Message);
            Assert.AreEqual(ErrorType.Critical, error.ErrorType);
        }


        static ModelItem CreateModelItem(string commandFileName = null)
        {
            return ModelItemUtils.CreateModelItem(new DsfExecuteCommandLineActivity { CommandFileName = commandFileName});
        }
    }
}
