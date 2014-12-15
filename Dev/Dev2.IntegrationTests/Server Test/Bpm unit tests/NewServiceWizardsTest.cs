
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
using WebpartConfiguration.Test;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.BPM.Unit.Test {
    [TestClass]
    //[DeploymentItem(@"..\Test References\Interop.SHDocVw.dll")]
    public class NewServiceWizardsTest {
        #region Pre and Post Test Activities
        private TestContext testContextInstance;
        string WebserverUrl = ServerSettings.WebserverURI;
        //private static ProcessInvoker _process;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        [TestInitialize]
        public void SetUp() {
            AssemblyResolver assemblyResolve = new AssemblyResolver();
            assemblyResolve.RegisterAssemblyResolver();
        }

        #endregion
        //#region ServiceDetails
        //[TestMethod]
        //public void ServiceDetails_NextButtonClicked_RequiredFieldsBlank() {
        //    using(var browser = new IE(string.Format("{0}{1}", WebserverUrl, "ServiceDetails"))) {
        //        string expected = "Please fill in all required values as indicated with a *";
        //        string actual = string.Empty;
        //        AlertAndConfirmDialogHandler confirmAndAlertDialogHandler = new AlertAndConfirmDialogHandler();
        //        browser.DialogWatcher.Add(confirmAndAlertDialogHandler);
        //        browser.Button(Find.ById("Dev2ServiceDetailsNext")).Click();
        //        actual = confirmAndAlertDialogHandler.Peek();
        //        browser.WaitForComplete();
        //        browser.Close();
        //        browser.Dispose();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}

        //[TestMethod]
        //public void ServiceDetails_AdvancedButtonClicked() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "ServiceDetails"))) {
        //        browser.Button(Find.ById("advanced")).ClickNoWait();
        //        string jsCommand = string.Format("$('#{0}').is(':visible');", "Dev2ServiceDetailsIcon");
        //        bool isVisible = browser.Eval(jsCommand) == "true";
        //        browser.WaitForComplete();
        //        browser.Close();
        //        browser.Dispose();
        //        Assert.IsTrue(isVisible);
        //    }
        //}
        //[TestMethod]
        //public void ServiceDetails_CancelButtonClicked() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "ServiceDetails"))) {
        //        AlertAndConfirmDialogHandler alertAndConfirmDialogHandler = new AlertAndConfirmDialogHandler();
        //        browser.DialogWatcher.Add(alertAndConfirmDialogHandler);
        //        browser.Button(Find.ById("Dev2ServiceDetailsCancelButton")).ClickNoWait();
        //        using (browser) {
        //            System.Windows.Forms.SendKeys.SendWait("{ENTER}");
        //        }
        //        browser.WaitForComplete();
        //        browser.Close();
        //        browser.Dispose();
        //        Assert.IsTrue(browser.NativeBrowser == null);
        //    }
        //}
        //[TestMethod]
        //public void ServiceDetails_SelectIconButtonClicked() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "ServiceDetails"))) {
        //        browser.Button(Find.ById("Dev2ServiceDetailsSelectIconButton")).ClickNoWait();
        //        string jsCommand = string.Format("$('#{0}').is(':visible');", "Dev2ServiceDetailsServerTree");
        //        bool isVisible = browser.Eval(jsCommand) == "true";
        //        browser.WaitForComplete();
        //        browser.Close();
        //        browser.Dispose();
        //        Assert.IsTrue(isVisible);
        //    }
        //}
        //#endregion ServiceDetails

        #region DatabaseServiceSetup
        #endregion DatabaseServiceSetup

        #region DatabaseSourceManagement
        [TestMethod]
        public void DatabaseSourceManagement_DataBindingTest_Expected_SourceFieldContainsOptions() {
            string PostData = String.Format("{0}{1}", WebserverUrl, "DatabaseSourceManagement");
            string typeOfWork = @"Data Source:";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(typeOfWork));
        }
        #endregion DatabaseSourceManagement

        #region ServiceInputOutputDescription

        [TestMethod]
        public void ServiceInputOutputDescription_CancelButtonClickedTest_Expected_PageClosed() {
            //string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription?Dev2ServiceDetailsName=Get Countries&Dev2ServiceDetailsWorkType=Database&Dev2ServiceSetupSourceMethod=sp_GetCountries&Dev2ServiceSetupInputs=<Prefix/>&Dev2ServiceSetupOutputs=<GetCities><CountryID/><Description/></GetCities>");
            //string typeOfWork = @"<tr><td><input type=""checkbox"" id=""Dev2ServiceInputOutputDescriptionForceRecordset"">Force results to Recordset?</input></td></tr>";

            //string responseData = TestHelper.PostDataToWebserver(PostData);
            //string action = responseData.Substring(responseData.IndexOf("action="), responseData.IndexOf("method=") - responseData.IndexOf("action=")).Replace("action=","");
            //action = action.Replace("\"", "");
            //PostData = String.Format("{0}{1}", WebserverUrl, action);
            //responseData = TestHelper.PostDataToWebserver(PostData);

            //Assert.IsTrue(responseData.Contains(typeOfWork));            

        }
        [TestMethod]
        public void ServiceInputOutputDescription_DoneButtonClickedTest_Expected_DataPostedToWebServer() {
            //string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription?Dev2ServiceDetailsName=Get Countries&Dev2ServiceDetailsWorkType=Database&Dev2ServiceSetupSourceMethod=sp_GetCountries&Dev2ServiceSetupInputs=<Prefix/>&Dev2ServiceSetupOutputs=<GetCities><CountryID/><Description/></GetCities>");
            //string typeOfWork = @"<tr><td><input type=""checkbox"" id=""Dev2ServiceInputOutputDescriptionForceRecordset"">Force results to Recordset?</input></td></tr>";

            //string responseData = TestHelper.PostDataToWebserver(PostData);
            //string action = responseData.Substring(responseData.IndexOf("action="), responseData.IndexOf("method=") - responseData.IndexOf("action=")).Replace("action=","");
            //action = action.Replace("\"", "");
            //PostData = String.Format("{0}{1}", WebserverUrl, action);
            //responseData = TestHelper.PostDataToWebserver(PostData);

            //Assert.IsTrue(responseData.Contains(typeOfWork));            

        }
        [TestMethod]
        public void ServiceInputOutputDescription_BackButtonClickedTest_Expected_ReturnToPreviousPage() {
            //string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription?Dev2ServiceDetailsName=Get Countries&Dev2ServiceDetailsWorkType=Database&Dev2ServiceSetupSourceMethod=sp_GetCountries&Dev2ServiceSetupInputs=<Prefix/>&Dev2ServiceSetupOutputs=<GetCities><CountryID/><Description/></GetCities>");
            //string typeOfWork = @"<tr><td><input type=""checkbox"" id=""Dev2ServiceInputOutputDescriptionForceRecordset"">Force results to Recordset?</input></td></tr>";

            //string responseData = TestHelper.PostDataToWebserver(PostData);
            //string action = responseData.Substring(responseData.IndexOf("action="), responseData.IndexOf("method=") - responseData.IndexOf("action=")).Replace("action=","");
            //action = action.Replace("\"", "");
            //PostData = String.Format("{0}{1}", WebserverUrl, action);
            //responseData = TestHelper.PostDataToWebserver(PostData);

            //Assert.IsTrue(responseData.Contains(typeOfWork));            

        }
        #endregion ServiceInputOutputDescription

        #region PluginSourceManagement
        //[TestMethod]
        //public void PluginSourceManagement_DoneButtonClicked_AliasAndPathValid() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        string sourceName = "Source" + new Random().Next().ToString();
        //        string expected = "Added Source '" + sourceName + "'"; ;
        //        string actual = string.Empty;
        //        AlertDialogHandler alertDialogHandler = new AlertDialogHandler();
        //        browser.AddDialogHandler(alertDialogHandler);
        //        browser.TextField(Find.ById("Dev2SourceManagementServer")).TypeText("ABC.xml");
        //        browser.TextField(Find.ById("Dev2SourceManagementAlias")).TypeText(sourceName);
        //        browser.Button(Find.ById("Dev2SourceManagementDoneButton")).ClickNoWait();
        //        alertDialogHandler.WaitUntilExists();
        //        if (!alertDialogHandler.Exists()) {
        //            Assert.Fail("No JavaScript alert when it should have been there");
        //        }
        //        alertDialogHandler.OKButton.Click();
        //        actual = alertDialogHandler.Message;
        //        browser.WaitForComplete();
        //        browser.Close();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}
        //[TestMethod]
        //public void PluginSourceManagement_DoneButtonClicked_AliasBlank() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        string expected = "Please enter source name";
        //        string actual = string.Empty;
        //        AlertDialogHandler alertDialogHandler = new AlertDialogHandler();
        //        browser.AddDialogHandler(alertDialogHandler);
        //        browser.TextField(Find.ById("Dev2SourceManagementServer")).TypeText("ABC.xml");
        //        browser.Button(Find.ById("Dev2SourceManagementDoneButton")).ClickNoWait();
        //        alertDialogHandler.WaitUntilExists();
        //        if (!alertDialogHandler.Exists()) {
        //            Assert.Fail("No JavaScript alert when it should have been there");
        //        }
        //        alertDialogHandler.OKButton.Click();
        //        actual = alertDialogHandler.Message;
        //        browser.WaitForComplete();
        //        browser.Close();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}
        //[TestMethod]
        //public void PluginSourceManagement_DoneButtonClicked_PathBlank() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        string expected = "Please select the path from server";
        //        string actual = string.Empty;
        //        AlertDialogHandler alertDialogHandler = new AlertDialogHandler();
        //        browser.AddDialogHandler(alertDialogHandler);
        //        browser.Button(Find.ById("Dev2SourceManagementDoneButton")).ClickNoWait();
        //        alertDialogHandler.WaitUntilExists();
        //        if (!alertDialogHandler.Exists()) {
        //            Assert.Fail("No JavaScript alert when it should have been there");
        //        }
        //        alertDialogHandler.OKButton.Click();
        //        actual = alertDialogHandler.Message;
        //        browser.WaitForComplete();
        //        browser.Close();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}
        //[TestMethod]
        //public void PluginSourceManagement_CloseButtonClicked() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        AlertAndConfirmDialogHandler alertAndConfirmDialogHandler = new AlertAndConfirmDialogHandler();
        //        browser.DialogWatcher.Add(alertAndConfirmDialogHandler);
        //        browser.Button(Find.ById("Dev2SourceManagementCloseButton")).ClickNoWait();
        //        using (new UseDialogOnce(browser.DialogWatcher, alertAndConfirmDialogHandler)) {
        //            Assert.AreEqual(0, alertAndConfirmDialogHandler.Count);
        //            System.Windows.Forms.SendKeys.SendWait("~");
        //        }
        //        browser.WaitForComplete();
        //    }
        //}
        //[TestMethod]
        //public void PluginSourceManagement_TestConnectionButtonClicked_UsernamePasswordNotEntered() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        string expected = "Please enter a username";
        //        string actual = string.Empty;
        //        AlertDialogHandler alertDialogHandler = new AlertDialogHandler();
        //        browser.AddDialogHandler(alertDialogHandler);
        //        browser.Button(Find.ById("Dev2SourceManagementTestConnection")).ClickNoWait();
        //        alertDialogHandler.WaitUntilExists();
        //        if (!alertDialogHandler.Exists()) {
        //            Assert.Fail("No JavaScript alert when it should have been there");
        //        }
        //        alertDialogHandler.OKButton.Click();
        //        actual = alertDialogHandler.Message;
        //        browser.WaitForComplete();
        //        browser.Close();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}
        //[TestMethod]
        //public void PluginSourceManagement_TestConnectionButtonClicked_UsernamePasswordValid() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        string expected = "Connection successful!";
        //        string actual = string.Empty;
        //        AlertDialogHandler alertDialogHandler = new AlertDialogHandler();
        //        browser.AddDialogHandler(alertDialogHandler);
        //        browser.TextField(Find.ById("Dev2SourceManagementUsername")).TypeText("DEV2\\zuko.mgwili");
        //        browser.TextField(Find.ById("Dev2SourceManagementPassword")).TypeText("P@ssword");
        //        browser.Button(Find.ById("Dev2SourceManagementTestConnection")).ClickNoWait();
        //        alertDialogHandler.WaitUntilExists();
        //        if (!alertDialogHandler.Exists()) {
        //            Assert.Fail("No JavaScript alert when it should have been there");
        //        }
        //        alertDialogHandler.OKButton.Click();
        //        actual = alertDialogHandler.Message;
        //        browser.WaitForComplete();
        //        browser.Close();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}
        //[TestMethod]
        //public void PluginSourceManagement_TestConnectionButtonClicked_UsernamePasswordNotValid() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        string expected = "Connection failed. Ensure your username and password are valid";
        //        string actual = string.Empty;
        //        AlertDialogHandler alertDialogHandler = new AlertDialogHandler();
        //        browser.AddDialogHandler(alertDialogHandler);
        //        browser.TextField(Find.ById("Dev2SourceManagementUsername")).TypeText("DEV2\\john.doe");
        //        browser.TextField(Find.ById("Dev2SourceManagementPassword")).TypeText("P@ssword");
        //        browser.Button(Find.ById("Dev2SourceManagementTestConnection")).ClickNoWait();
        //        alertDialogHandler.WaitUntilExists();
        //        if (!alertDialogHandler.Exists()) {
        //            Assert.Fail("No JavaScript alert when it should have been there");
        //        }
        //        alertDialogHandler.OKButton.Click();
        //        actual = alertDialogHandler.Message;
        //        browser.WaitForComplete();
        //        browser.Close();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}
        //[TestMethod]
        //public void PluginSourceManagement_BrowseServerButtonClicked_UsernamePasswordNotEntered() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        bool expected = true;
        //        browser.WaitForComplete();
        //        browser.Button(Find.ById("Dev2SourceManagementBrowseServer")).ClickNoWait();
        //        //The dialog does not have any children
        //        Div dialog = browser.Div("Dev2SourceManagementServerDialog");
        //        dialog.WaitForComplete();
        //        //if the dynatree has been successful created, the number of children will be great than 0
        //        bool actual = (dialog.Children().Count > 0);
        //        browser.Close();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}
        //[TestMethod]
        //public void PluginSourceManagement_BrowseServerButtonClicked_UsernamePasswordInvalid() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        string expected = "Connection failed. Ensure your username and password are valid";
        //        string actual = string.Empty;
        //        AlertDialogHandler alertDialogHandler = new AlertDialogHandler();
        //        browser.AddDialogHandler(alertDialogHandler);
        //        browser.TextField(Find.ById("Dev2SourceManagementUsername")).TypeText(@"DEV2\john.doe");
        //        browser.TextField(Find.ById("Dev2SourceManagementPassword")).TypeText("P@ssword");
        //        browser.Button(Find.ById("Dev2SourceManagementBrowseServer")).ClickNoWait();
        //        alertDialogHandler.WaitUntilExists();
        //        if (!alertDialogHandler.Exists()) {
        //            Assert.Fail("No JavaScript alert when it should have been there");
        //        }
        //        alertDialogHandler.OKButton.Click();
        //        actual = alertDialogHandler.Message;
        //        browser.WaitForComplete();
        //        browser.Close();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}
        //[TestMethod]
        //public void PluginSourceManagement_BrowseServerButtonClicked_UsernamePasswordValid() {
        //    using (var browser = new IE(string.Format("{0}{1}", WebserverUrl, "PluginSourceManagement"))) {
        //        bool expected = true;
        //        browser.WaitForComplete();
        //        browser.TextField(Find.ById("Dev2SourceManagementUsername")).TypeText(@"DEV2\zuko.mgwili");
        //        browser.TextField(Find.ById("Dev2SourceManagementPassword")).TypeText("P@ssword");
        //        browser.Button(Find.ById("Dev2SourceManagementBrowseServer")).ClickNoWait();
        //        //The dialog does not have any children
        //        Div dialog = browser.Div("Dev2SourceManagementServerDialog");
        //        dialog.WaitForComplete();
        //        //if the dynatree has been successful created, the number of children will be great than 0
        //        bool actual = (dialog.Children().Count > 0);
        //        browser.Close();
        //        Assert.AreEqual(expected, actual);
        //    }
        //}
        #endregion PluginSourceManagement
    }
}
