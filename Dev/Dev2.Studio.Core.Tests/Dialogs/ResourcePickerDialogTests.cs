using System;
using Caliburn.Micro;
using Dev2.Dialogs;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Enums;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Dialogs
{
    [TestClass]
    public class ResourcePickerDialogTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_NullEnvironmentRepository_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var dialog = new ResourcePickerDialog(enDsfActivityType.All, null, null, null, false);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_NullEventAggregator_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var dialog = new ResourcePickerDialog(enDsfActivityType.All, new Mock<IEnvironmentRepository>().Object, null, null, false);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_NullAsyncWorker_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var dialog = new ResourcePickerDialog(enDsfActivityType.All, new Mock<IEnvironmentRepository>().Object, new Mock<IEventAggregator>().Object, null, false);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_EnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var dialog = new ResourcePickerDialog(enDsfActivityType.All, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [Ignore]
        public void ResourcePickerDialog_Constructor_EnvironmentModelIsNotNull_EnvironmentRepositoryIsNew()
        {
            //------------Setup for test-------------------------
            var dialogWindow = new Mock<IDialog>();

            //------------Execute Test---------------------------
            var dialog = new TestResourcePickerDialog(enDsfActivityType.All, null) { CreateDialogResult = dialogWindow.Object };
            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreNotSame(EnvironmentRepository.Instance, dialog.CreateDialogDataContext.ExplorerViewModel.EnvironmentRepository);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [Ignore]
        public void ResourcePickerDialog_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------
            var dialogWindow = new Mock<IDialog>();
            const enDsfActivityType ActivityType = enDsfActivityType.All;
            const bool IsFromActivityDrop = true;

            var envRepo = EnvironmentRepository.Create(new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            var dialog = new TestResourcePickerDialog(ActivityType, envRepo, new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, IsFromActivityDrop)
            {
                CreateDialogResult = dialogWindow.Object
            };
            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreEqual(ActivityType, dialog.CreateDialogDataContext.ActivityType);
            Assert.AreEqual(IsFromActivityDrop, dialog.CreateDialogDataContext.ExplorerViewModel.NavigationViewModel.IsFromActivityDrop);
        }
    }
}
