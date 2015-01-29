using System;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DialogueViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DialogueViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DialogueViewModel_Ctor_ArgNull_ExpectException()
        {
            //------------Setup for test--------------------------
            var dialogueViewModel = new DialogueViewModel(null);
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DialogueViewModel_Ctor")]
        public void DialogueViewModel_Ctor_Valid_ExpectProperValuesSet()
        {
            //------------Setup for test--------------------------
            var inner = new Mock<IInnerDialogueTemplate>();
            var dialogueViewModel = new DialogueViewModel(inner.Object);
            Assert.IsNotNull(dialogueViewModel.InnerDialogue);
            Assert.IsNotNull(dialogueViewModel.Validate);
            dialogueViewModel.Header = "Bob";
            dialogueViewModel.HeaderDetail = "the builder";
            //------------Execute Test---------------------------
            Assert.AreEqual("Bob",dialogueViewModel.Header);
            Assert.AreEqual("the builder",dialogueViewModel.HeaderDetail);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DialogueViewModel_Ctor")]
        public void DialogueViewModel_Validate_CallsInner()
        {
            //------------Setup for test--------------------------
            var inner = new Mock<IInnerDialogueTemplate>();
            var dialogueViewModel = new DialogueViewModel(inner.Object);
            inner.Setup(a=>a.Validate()).Returns("bob");
            dialogueViewModel.Validate.Execute(null);
            Assert.IsFalse(dialogueViewModel.OkEnabled);
            Assert.AreEqual("bob",dialogueViewModel.ValidationMessage);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

    }
}
