
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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests.Wizards
{
    /// <summary>
    /// Summary description for HtmlWidgetWizardTest  
    /// </summary>
    [TestClass]
    public class WizardTest
    {

        public WizardTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        /*
         * 25.07.2012 : Travis.Frisinger 
         * General integration test for webpart wizards
         * 
         */

        // PBI 6278 : Error: Does not render at all, so the tests fails (No <table> tags)

        
        [TestMethod]
        public void Html_Wizard_Test()
        { // - OK

            string path = ServerSettings.WebserverURI + "HtmlWidget.wiz?" + wizard_test_strings.HtmlWidgetWizardString;

            string result = TestHelper.PostDataToWebserver(path);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.HtmlWidgetResult);

            Assert.AreEqual(exp, frag);
        }
        

        [TestMethod]
        public void Button_Wizard_Test()
        { // - OK!

            string path = ServerSettings.WebserverURI + "Button.wiz?" + wizard_test_strings.ButtonWizardString;

            string result = TestHelper.PostDataToWebserver(path);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag.Trim());

            string exp = TestHelper.CleanUp(wizard_test_strings.ButtonWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void Checkbox_Wizard_Test()
        { // - OK

            string path = ServerSettings.WebserverURI + "Checkbox.wiz?" + wizard_test_strings.CheckboxWizardString;

            string result = TestHelper.PostDataToWebserver(path);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.CheckboxResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void Datagrid_Wizard_Test()
        {
            string path = ServerSettings.WebserverURI + "Data Grid.wiz?" + wizard_test_strings.DataGridWizardString;

            string result = TestHelper.PostDataToWebserver(path);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.DataGridWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void Datepicker_Wizard_Test()
        { // - OK
            string result = TestHelper.PostDataToWebserver(ServerSettings.WebserverURI + "Date Picker.wiz?" + wizard_test_strings.DatePickerWizardString);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.DatePickerWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void Dropdown_Wizard_Test()
        { // - OK
            string url = ServerSettings.WebserverURI + "Drop Down List.wiz?" + wizard_test_strings.DropDownListWizardString;
            
            string result = TestHelper.PostDataToWebserver(url);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.DropDownListWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void File_Wizard_Test()
        { // - OK

            string url = ServerSettings.WebserverURI + "File.wiz?" + wizard_test_strings.FileWizardString;

            string result = TestHelper.PostDataToWebserver(url);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.FileWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void HelpRegion_Wizard_Test()
        { // - OK

            string result = TestHelper.PostDataToWebserver(ServerSettings.WebserverURI + "HelpRegion.wiz?" + wizard_test_strings.HelpRegionWizardString);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.HelpRegionWizardResult);

            Assert.IsTrue(frag.Contains(exp));
        }

        [TestMethod]
        public void Image_Wizard_Test()
        { // - OK

            string result = TestHelper.PostDataToWebserver(ServerSettings.WebserverURI + "Image.wiz?" + wizard_test_strings.ImageWizardString);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.ImageWizardResult);

            Assert.AreEqual(exp, frag);
        }


        [TestMethod]
        public void Label_Wizard_Test()
        { // - OK
            // [[welcomeMessage]] being parsed instead of displayed
            string path = ServerSettings.WebserverURI + "Label.wiz?" + wizard_test_strings.LabelWizardString;

            string result = TestHelper.PostDataToWebserver(path);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.LabelWizardResult);

            Assert.IsTrue(frag.Contains(exp));
        }

        [TestMethod]
        public void Link_Wizard_Test()
        { // - OK

            string result = TestHelper.PostDataToWebserver(ServerSettings.WebserverURI + "Link.wiz?" + wizard_test_strings.LinkWizardString);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.LinkWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void NameRegion_Wizard_Test()
        { // - OK

            string result = TestHelper.PostDataToWebserver(ServerSettings.WebserverURI + "NameRegion.wiz?" + wizard_test_strings.NamedRegionWizardString);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.NamedRegionWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void RadioButton_Wizard_Test()
        { // - OK

            string path = ServerSettings.WebserverURI + "Radio Button.wiz?" + wizard_test_strings.RadioButtonWizardString;

            string result = TestHelper.PostDataToWebserver(path);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.RadioButtonWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void Textarea_Wizard_Test()
        { // - OK

            string result = TestHelper.PostDataToWebserver(ServerSettings.WebserverURI + "Textarea.wiz?" + wizard_test_strings.TextareaWizardString);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.TextareaWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void Textbox_Wizard_Test()
        { // - OK

            string postData = ServerSettings.WebserverURI + "Textbox.wiz?" + wizard_test_strings.TextboxWizardString;
            
            string result = TestHelper.PostDataToWebserver(postData);

            string frag = TestHelper.ExtractDataBetween(result, TestHelper._tblStart, TestHelper._tblEnd);

            frag = TestHelper.CleanUp(frag);

            string exp = TestHelper.CleanUp(wizard_test_strings.TextboxWizardResult);

            Assert.AreEqual(exp, frag);
        }

        [TestMethod]
        public void Dev2Webserver_Can_Bind()
        {
            string path = ServerSettings.WebserverURI + "Dev2ServiceDetails";

            string result = TestHelper.PostDataToWebserver(path);

            Assert.IsTrue((result.IndexOf("<script>initServiceDetails('http://localhost:1234'") >= 0));
        }

    }
}
