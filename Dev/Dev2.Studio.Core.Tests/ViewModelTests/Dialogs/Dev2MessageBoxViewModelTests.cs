using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests.Dialogs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2MessageBoxViewModelTests
    {

        [TestMethod]
        public void SetAndGetDontShowAgainOptionExpectedSameOptionReturnedOnGetAsWasSet()
        {
            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            ImportService.CurrentContext = CompositionInitializer.InitializeIFilePersistenceProvider(filePersistenceProvider);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Dev2MessageBoxViewModel.SetDontShowAgainOption("1", MessageBoxResult.OK);
            Tuple<bool, MessageBoxResult> result = Dev2MessageBoxViewModel.GetDontShowAgainOption("1");

            Assert.AreEqual(MessageBoxResult.OK, result.Item2, "Value of option different to what was set.");
            Assert.AreEqual(true, result.Item1, "Option wasn't added correctly on set.");
        }

        [TestMethod]
        public void SetAndGetDontShowAgainOptionWhereOptionAlreadyExistsExpectedSameOptionReturnedOnGetAsWasLastSet()
        {
            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            ImportService.CurrentContext = CompositionInitializer.InitializeIFilePersistenceProvider(filePersistenceProvider);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Dev2MessageBoxViewModel.SetDontShowAgainOption("1", MessageBoxResult.OK);
            Dev2MessageBoxViewModel.SetDontShowAgainOption("1", MessageBoxResult.Yes);
            Tuple<bool, MessageBoxResult> result = Dev2MessageBoxViewModel.GetDontShowAgainOption("1");

            Assert.AreEqual(MessageBoxResult.Yes, result.Item2, "Value of option not updated whrn updateing on set.");
            Assert.AreEqual(true, result.Item1, "Option removed when updating on set.");
        }

        [TestMethod]
        public void GetDontShowAgainOptionWhereKeyDoesntExistExpectedFalseReturnedOnGet()
        {
            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            ImportService.CurrentContext = CompositionInitializer.InitializeIFilePersistenceProvider(filePersistenceProvider);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Tuple<bool, MessageBoxResult> result = Dev2MessageBoxViewModel.GetDontShowAgainOption("1");

            Assert.AreEqual(false, result.Item1, "False should be returned if the option doesn't exist.");
        }

        [TestMethod]
        public void ResetDontShowAgainOptionExpectedFalseReturnedOnGetAndOtherOptionsAreIntact()
        {
            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            ImportService.CurrentContext = CompositionInitializer.InitializeIFilePersistenceProvider(filePersistenceProvider);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Dev2MessageBoxViewModel.SetDontShowAgainOption("1", MessageBoxResult.OK);
            Dev2MessageBoxViewModel.SetDontShowAgainOption("2", MessageBoxResult.Cancel);
            Dev2MessageBoxViewModel.ResetDontShowAgainOption("1");
            Tuple<bool, MessageBoxResult> result = Dev2MessageBoxViewModel.GetDontShowAgainOption("1");
            Tuple<bool, MessageBoxResult> result1 = Dev2MessageBoxViewModel.GetDontShowAgainOption("2");

            Assert.AreEqual(false, result.Item1, "Reset didn't clear the correct option.");
            Assert.AreEqual(true, result1.Item1, "Reset clear the incorrect option.");
            Assert.AreEqual(MessageBoxResult.Cancel, result1.Item2, "Value of other options corrupted by reset.");
        }

        [TestMethod]
        public void ResetAllDontShowAgainOptionExpectedAllOptionsCleared()
        {
            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            ImportService.CurrentContext = CompositionInitializer.InitializeIFilePersistenceProvider(filePersistenceProvider);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Dev2MessageBoxViewModel.SetDontShowAgainOption("1", MessageBoxResult.OK);
            Dev2MessageBoxViewModel.SetDontShowAgainOption("2", MessageBoxResult.Cancel);
            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();
            Tuple<bool, MessageBoxResult> result = Dev2MessageBoxViewModel.GetDontShowAgainOption("1");
            Tuple<bool, MessageBoxResult> result1 = Dev2MessageBoxViewModel.GetDontShowAgainOption("2");

            Assert.AreEqual(false, result.Item1, "Reset all didn't clear all options.");
            Assert.AreEqual(false, result1.Item1, "Reset all didn't clear all options.");
        }

        [TestMethod]
        public void SetDontShowAgainOptionExpectedPersistedToXML()
        {
            string expected = @"<root>
  <Option Key=""1"" Value=""OK"" />
</root>";
            string actual = null;

            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            filePersistenceProvider.Setup(p => p.Write(It.IsAny<string>(), It.IsAny<string>())).Callback((string p1, string p2) =>
                {
                    actual = p2;
                });


            ImportService.CurrentContext = CompositionInitializer.InitializeIFilePersistenceProvider(filePersistenceProvider);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Dev2MessageBoxViewModel.SetDontShowAgainOption("1", MessageBoxResult.OK);

            Assert.AreEqual(expected, actual, "Serialization resulted in an unexpected format.");
        }

        [TestMethod]
        public void GetDontShowAgainOptionExpectedPersistedToXML()
        {
            string data = @"<root>
  <Option Key=""1"" Value=""OK"" />
</root>";

            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            filePersistenceProvider.Setup(p => p.Read(It.IsAny<string>())).Returns(() => data);

            ImportService.CurrentContext = CompositionInitializer.InitializeIFilePersistenceProvider(filePersistenceProvider);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Tuple<bool, MessageBoxResult> result = Dev2MessageBoxViewModel.GetDontShowAgainOption("1");

            Assert.AreEqual(true, result.Item1, "Failed to hydrate options from XML data.");
            Assert.AreEqual(MessageBoxResult.OK, result.Item2, "Options incorrectly hydrated from XML data.");
        }
    }
}
