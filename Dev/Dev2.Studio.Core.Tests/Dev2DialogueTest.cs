
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
using System.Drawing;
using System.IO;
using Dev2.Common.Interfaces.Studio;
using Dev2.Studio.ViewModels.Administration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for Dev2DialogueTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2DialogueTest
    {

        private TestContext _testContextInstance;
        private readonly string _filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\test.png";
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        // ReSharper disable once ConvertToAutoProperty
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        #region Additional test attributes

        private void createFile(string fileName)
        {
            Bitmap flag = new Bitmap(10, 10);
            for(int x = 0; x < flag.Height; ++x)
            {
                for(int y = 0; y < flag.Width; ++y)
                {
                    flag.SetPixel(x, y, Color.White);
                }
            }
            for(int x = 0; x < flag.Height; ++x)
            {
                flag.SetPixel(x, x, Color.Red);
            }
            FileStream fs = File.Create(fileName);
            fs.Close();
            fs.Dispose();
            flag.Save(fileName);
        }

        #endregion

        #region Positive Test Cases

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void Dev2DialogueSetup_CorrectParameterSet_Test()
        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            string newFileName = _filePath.Replace(".png", "Dev2DialogueSetup_CorrectParameterSet_Test.png");
            createFile(newFileName);
            dev2Dialogue.SetupDialogue("Test Title", "Test Description", newFileName, "SomeTitleText");
            File.Delete(newFileName);
            Assert.IsTrue(dev2Dialogue.ImageSource != null);

            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();
        }

        #endregion Positive Test Cases

        #region Negative Test Cases

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void Dev2Dialogue_NullTitle_ValidDescriptionImgSourceDecsriptionTitle_Expected_DialogueSetup_TitleSetToEmptyStringNotNull()
        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            string newFileName = _filePath.Replace(".png", "Dev2Dialogue_NullTitle_ValidDescriptionImgSourceDecsriptionTitle_Expected_DialogueSetup_TitleSetToEmptyStringNotNull.png");
            createFile(newFileName);
            dev2Dialogue.SetupDialogue(null, "Test Description", newFileName, "SomeTitleText");
            File.Delete(newFileName);
            Assert.AreEqual(string.Empty, dev2Dialogue.Title);

            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();

        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Dev2Dialogue_NullDescription_ValidDescriptionImgSourceDecsriptionTitle_Expected_DialogueSetup_DescriptionSetToEmpty()

        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            string newFileName = _filePath.Replace(".png", "Dev2Dialogue_NullDescription_ValidDescriptionImgSourceDecsriptionTitle_Expected_DialogueSetup_DescriptionSetToEmpty.png");
            createFile(newFileName);
            dev2Dialogue.SetupDialogue("Test Title", null, newFileName, "SomeTitleText");
            File.Delete(newFileName);
            Assert.AreEqual(string.Empty, dev2Dialogue.DescriptionText);
            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();

        }

        [TestMethod]
        public void Dev2Dialogue_NullTitleDescription_ValidgImgSourceDecsriptionTitle_Expected_DialogueSetup_TitleandDescriptionSetToEmpty()
        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            string newFileName = _filePath.Replace(".png", "Dev2Dialogue_NullTitleDescription_ValidgImgSourceDecsriptionTitle_Expected_DialogueSetup_TitleandDescriptionSetToEmpty.png");
            createFile(newFileName);
            dev2Dialogue.SetupDialogue(null, null, newFileName, "SomeTitleText");
            File.Delete(newFileName);
            Assert.IsTrue(dev2Dialogue.Title == string.Empty && dev2Dialogue.DescriptionText == string.Empty);

            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();
        }

        [TestMethod]
        public void Dev2Dialogue_NullImageSource_ValidTitleDescriptionDescriptionTitle_Expected_NullImageReference()
        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            dev2Dialogue.SetupDialogue("Test Title", "Test Description", null, "SomeTitleText");
            Assert.IsTrue(dev2Dialogue.ImageSource == null);
            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();
        }

        [TestMethod]
        public void Dev2Dialogue_NullTitleDesciptionImageSource_ValidDescriptionTitle_Expected_CreatedWithOnlyDescriptionTitleText()
        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            dev2Dialogue.SetupDialogue(null, null, null, "SomeTitleText");
            Assert.IsFalse(string.IsNullOrEmpty(dev2Dialogue.DescriptionTitleText));
            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();
        }


        [TestMethod]
        public void Dev2Dialogue_NullDesciptionTitleText_ValidTitleDescriptionImageSource_Expected_NullImageReference()
        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            dev2Dialogue.SetupDialogue(null, null, null, "SomeTitleText");
            Assert.IsFalse(string.IsNullOrEmpty(dev2Dialogue.DescriptionTitleText));

            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();
        }

        [TestMethod]
        public void Dev2Dialogue_NullTitleDescriptionTitleText_ValidImageSourceDescription_Expected_EmptyTitleAndDescription()
        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            string newFileName = _filePath.Replace(".png", "Dev2Dialogue_NullTitleDescriptionTitleText_ValidImageSourceDescription_Expected_EmptyTitleAndDescription.png");
            createFile(newFileName);
            dev2Dialogue.SetupDialogue(null, "Test Description", newFileName, null);
            File.Delete(newFileName);
            Assert.IsTrue(dev2Dialogue.Title == string.Empty && dev2Dialogue.DescriptionTitleText == string.Empty);

            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();
        }

        [TestMethod]
        public void Dev2Dialogue_NullDescriptionDescriptionTitleText_ValidTitleImageSource_Expected_EmptyDescriptionTitleAndDescription()
        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            string newFileName = _filePath.Replace(".png", "Dev2Dialogue_NullDescriptionDescriptionTitleText_ValidTitleImageSource_Expected_EmptyDescriptionTitleAndDescription.png");
            createFile(newFileName);
            dev2Dialogue.SetupDialogue("Test Title", null, newFileName, null);
            File.Delete(newFileName);
            Assert.IsTrue(dev2Dialogue.DescriptionText == string.Empty && dev2Dialogue.DescriptionTitleText == string.Empty);

            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();
        }

        [TestMethod]
        public void Dev2Dialogue_NullTitleDescriptionDescriptionTitleText_ValidImageSource_Expected_ValidImage()
        {
            IDialogueViewModel dev2Dialogue = new DialogueViewModel();
            string newFileName = _filePath.Replace(".png", "Dev2Dialogue_NullTitleDescriptionDescriptionTitleText_ValidImageSource_Expected_ValidImage.png");
            createFile(newFileName);
            dev2Dialogue.SetupDialogue("Test Title", null, newFileName, null);
            File.Delete(newFileName);
            Assert.IsNotNull(dev2Dialogue.ImageSource);

            // cleanup - Dispose of the dev2Dialogue
            dev2Dialogue.Dispose();
        }

        #endregion Negative Test Cases
        // ReSharper restore InconsistentNaming
    }
}
