
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests.Dialogs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class Dev2MessageBoxViewModelTests
    {

        [TestMethod]
        public void SetAndGetDontShowAgainOptionExpectedSameOptionReturnedOnGetAsWasSet()
        {
            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            CustomContainer.Register(filePersistenceProvider.Object);

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
            CustomContainer.Register(filePersistenceProvider.Object);

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
            CustomContainer.Register(filePersistenceProvider.Object);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Tuple<bool, MessageBoxResult> result = Dev2MessageBoxViewModel.GetDontShowAgainOption("1");

            Assert.AreEqual(false, result.Item1, "False should be returned if the option doesn't exist.");
        }

        [TestMethod]
        public void ResetDontShowAgainOptionExpectedFalseReturnedOnGetAndOtherOptionsAreIntact()
        {
            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            CustomContainer.Register(filePersistenceProvider.Object);

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
            CustomContainer.Register(filePersistenceProvider.Object);

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
        // ReSharper disable InconsistentNaming
        public void SetDontShowAgainOptionExpectedPersistedToXML()
        {
            const string expected = @"<root>
  <Option Key=""1"" Value=""OK"" />
</root>";
            string actual = null;
            CustomContainer.DeRegister<IFilePersistenceProvider>();
            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            filePersistenceProvider.Setup(p => p.Write(It.IsAny<string>(), It.IsAny<string>())).Callback((string p1, string p2) =>
                {
                    actual = p2;
                });


            CustomContainer.Register(filePersistenceProvider.Object);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Dev2MessageBoxViewModel.SetDontShowAgainOption("1", MessageBoxResult.OK);

            Assert.AreEqual(expected, actual, "Serialization resulted in an unexpected format.");
        }

        [TestMethod]
        public void GetDontShowAgainOptionExpectedPersistedToXML()
        {
            const string data = @"<root>
  <Option Key=""1"" Value=""OK"" />
</root>";
            CustomContainer.DeRegister<IFilePersistenceProvider>();
            Mock<IFilePersistenceProvider> filePersistenceProvider = new Mock<IFilePersistenceProvider>();
            filePersistenceProvider.Setup(p => p.Read(It.IsAny<string>())).Returns(() => data);

            CustomContainer.Register(filePersistenceProvider.Object);

            Dev2MessageBoxViewModel.ResetAllDontShowAgainOptions();

            Tuple<bool, MessageBoxResult> result = Dev2MessageBoxViewModel.GetDontShowAgainOption("1");

            Assert.AreEqual(true, result.Item1, "Failed to hydrate options from XML data.");
            Assert.AreEqual(MessageBoxResult.OK, result.Item2, "Options incorrectly hydrated from XML data.");
        }
    }
}
