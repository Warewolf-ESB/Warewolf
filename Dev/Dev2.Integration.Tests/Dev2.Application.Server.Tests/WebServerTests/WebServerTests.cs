using System;
using System.Collections.Generic;
using System.Net;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2_Application_Server_Tests.WebServerTests
{
    /// <summary>
    /// Summary description for BasicWebServerTests
    /// </summary>
    [TestClass]
    public class WebServerTests
    {
        const string AssertNotNull = "AssertNotNull";
        readonly static string ServicesUri = ServerSettings.WebserverURI;
        readonly static string ServicesHttpsUri = ServerSettings.WebserverHttpsURI;
        readonly static string WebsiteUri = ServerSettings.WebsiteServerUri;


        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_DataIsNotLarge_DownloadHeadersNotAdded()
        {
            string path = ServicesUri + "ABC";

            HttpWebResponse result = TestHelper.GetResponseFromServer(path);

            var allKeys = result.Headers.AllKeys;
            const string ContentType = "Content-Type";
            const string ContentDisposition = "Content-Disposition";
            CollectionAssert.Contains(allKeys, ContentType);
            CollectionAssert.DoesNotContain(allKeys, ContentDisposition);

            var contentTypeValue = result.Headers.Get(ContentType);

            Assert.AreNotEqual("application/force-download", contentTypeValue);
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_DataIsLarge_DownloadHeadersAdded()
        {

            string path = ServerSettings.WebserverURI + "LargeDataTest";

            HttpWebResponse result = TestHelper.GetResponseFromServer(path);

            var allKeys = result.Headers.AllKeys;
            const string ContentType = "Content-Type";
            const string ContentDisposition = "Content-Disposition";
            CollectionAssert.Contains(allKeys, ContentType);
            CollectionAssert.Contains(allKeys, ContentDisposition);

            var contentTypeValue = result.Headers.Get(ContentType);
            var contentDispositionValue = result.Headers.Get(ContentDisposition);

            Assert.AreEqual("application/force-download", contentTypeValue);
            Assert.AreEqual("attachment; filename=Output.xml", contentDispositionValue);
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_Service_CannotExecuteServiceError()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "CaseSP");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, "<InnerError>Can only execute workflows from web browser</InnerError>");
        }

        // -- Trav New -- //
        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_ServiceAsJson_CannotExecuteServiceErrorAsJson()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "CaseSP.json");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------

            var expected = "{ \"FatalError\": \"An internal error occured while executing the service request\",\"errors\": [ \"Can only execute workflows from web browser\"]}";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServer_ServicesGet_NonExistingServiceAsJson_InternalErrorsAsJson()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "BugXXXX.json");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "{ \"FatalError\": \"An internal error occured while executing the service request\",\"errors\": [ \"Service [ BugXXXX ] not found.\"]}";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_NonExistingServiceAsXml_InternalErrorAsXml()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "BugXXXX.xml");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "<FatalError> <Message> An internal error occured while executing the service request </Message><InnerError>Service [ BugXXXX ] not found.</InnerError></FatalError>";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_WorkflowAsJson_ResultAsJson()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "Bug9139.json");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "{\"result\":\"PASS\"}";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_WorkflowAsXml_ResultAsXml()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "Bug9139.xml");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "<DataList><result>PASS</result></DataList>";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_WorkflowAsBadExtension_ResultAsXml()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "Bug9139.ml");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "<DataList><result>PASS</result></DataList>";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_OnSSLPort_ValidResultViaSSL()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesHttpsUri, "Bug9139");
            //------------Execute Test---------------------------
            bool wasHTTPS;
            string responseData = TestHelper.PostDataToWebserver(postData, out wasHTTPS);

            //------------Assert Results-------------------------
            var expected = "<DataList><result>PASS</result></DataList>";
            Assert.IsTrue(wasHTTPS);
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServer_ServicesPost")]
        public void WebServer_ServicesPost_Workflow_ResultAsXml()
        {
            //------------Setup for test--------------------------
            var message = "hello";

            var postData = string.Format("<DataList><payload>{0}</payload><result></result></DataList>", message);
            var expectedResponse = string.Format("<DataList><result>{0}</result></DataList>", message);

            var requestData = String.Format("{0}{1}?{2}?", ServicesUri, "PBI 10654 - Web Server Test", postData);

            //------------Execute Test---------------------------
            var responseData = TestHelper.PostDataToWebserver(requestData);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedResponse, responseData);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServer_ServicesPost")]
        public void WebServer_ServicesPost_Bookmarks_ResultAsXml()
        {
            //------------Setup for test--------------------------
            var instanceID = Guid.NewGuid();
            var bookmark = "bkmk";
            var message = "hello";

            var postData = string.Format("<DataList><payload>{0}</payload><result></result></DataList>", message);
            var expectedResponse = string.Format("<DataList><result>{0}</result></DataList>", ""); // expect no result because there isn't a bookmark to be resumed!

            var requestData = String.Format("{0}{1}/instances/{2}/bookmarks/{3}?{4}?", ServicesUri, "PBI 10654 - Web Server Test", instanceID, bookmark, postData);

            //------------Execute Test---------------------------
            var responseData = TestHelper.PostDataToWebserver(requestData);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedResponse, responseData);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServer_WebsiteGet")]
        public void WebServer_WebsiteGet_Content_FileContents()
        {
            var requests = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Content/Site.css", @"html {
    margin: 0;
    padding: 0;
    font-size: 100%;
}"),
                new Tuple<string, string>("Content/themes/warewolf/jquery-ui.css", @"/*
 * jQuery UI CSS Framework
 *
 * Copyright 2011, AUTHORS.txt (http://jqueryui.com/about)
 * Dual licensed under the MIT or GPL Version 2 licenses.
 * http://jquery.org/license
 *
 * http://docs.jquery.com/UI/Theming/API
 */
"),
                 new Tuple<string, string>("Content/icons.gif", @"GIF")

            };

            VerifyWebsiteRequests(requests, new[] { "sources", "services", "dialogs" });
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServer_WebsiteGet")]
        public void WebServer_WebsiteGet_Images_FileContents()
        {
            var requests = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("images/folder-closed.png", @"�PNG"),
                new Tuple<string, string>("images/ajax-loader32.gif", @"GIF")
            };

            VerifyWebsiteRequests(requests, new[] { "sources", "services", "dialogs" });
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServer_WebsiteGet")]
        public void WebServer_WebsiteGet_Scripts_FileContents()
        {
            var requests = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("warewolf-studio.js", @"var studio = namespace('Warewolf.Studio');"),
                new Tuple<string, string>("fx/jquery.caret.js", @"(function($) {
  $.fn.caret = function(pos) {"),
                new Tuple<string, string>("Decisions/DecisionModel.js", @"// Make this available to chrome debugger
//@ sourceURL=DecisionModel.js

/* Model for Decision Wizard */
function DecisionViewModel() {"),
                new Tuple<string, string>("Dialogs/SaveViewModel.js", @"// Make this available to chrome debugger
//@ sourceURL=SaveViewModel.js  

function SaveViewModel(saveUri, baseViewModel, saveFormID, environment) {"),
                new Tuple<string, string>("Services/WebServiceViewModel.js", @"// Make this available to chrome debugger
//@ sourceURL=WebServiceViewModel.js  

function WebServiceViewModel(saveContainerID, resourceID, sourceName, environment, resourcePath) {"),
                new Tuple<string, string>("Sources/WebSourceViewModel.js", @"// Make this available to chrome debugger
//@ sourceURL=WebSourceViewModel.js  

function WebSourceViewModel(saveContainerID, environment, resourceID) {"),
                new Tuple<string, string>("Switch/DragModel.js", @"function DragViewModel() {"),
            };

            VerifyWebsiteRequests(requests, new[] { "Scripts", "services/Scripts", "sources/Scripts", "dialogs/Scripts" });
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServer_WebsiteGet")]
        public void WebServer_WebsiteGet_Views_FileContents()
        {
            var requests = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Decisions/Wizard.htm", @"<div id=""container"">
    <div class=""float-left"">

        <div id=""header"" class=""ui-dialog-titlebar ui-widget-header ui-helper-clearfix"" style=""padding: 5px; font-size: 1.2em;"">
            Decision Flow"),
                new Tuple<string, string>("Dialogs/SaveDialog.htm", "<div id=\"saveForm\" title=\"Save\""),
                new Tuple<string, string>("Services/WebService.htm", "<div id=\"webServiceContainer\""),
                new Tuple<string, string>("Sources/WebSource.htm", "<div id=\"webSourceContainer\""),
                new Tuple<string, string>("Switch/Drag.htm", @"<div id=""container"">
    <div class=""float-left"">
        
        <div id=""header"" class=""ui-dialog-titlebar ui-widget-header ui-helper-clearfix"" style=""padding: 5px; font-size: 1.2em;"">
            Switch Case"),
                new Tuple<string, string>("Templates/InputMapping.htm", @"<h4>Inputs</h4>
<div>"),
            };

            VerifyWebsiteRequests(requests, new[] { "views" });
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServer_WebsiteGet")]
        public void WebServer_WebsiteGet_Layout_FileContents()
        {
            var requests = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("layout.htm", @"<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width"" />
    <title>Warewolf</title>"),
                new Tuple<string, string>("favicon.ico", AssertNotNull)
            };

            VerifyWebsiteRequests(requests, new[] { "" });
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServer_WebsitePost")]
        public void WebServer_WebsitePost_ServiceInvoked()
        {
            var requests = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Service/Help/GetDictionary?SaveDialog?", @"{""default"":""Name cannot be blank."",""DuplicateFound"":""Name already exists.""}"),
                new Tuple<string, string>("Service/Resources/PathsAndNames??", @"{""IsValid"":true,""ErrorMessage"":""Exception has been thrown by the target of an invocation."",""ErrorFields"":[],""Result"":""""}"),
                new Tuple<string, string>("Service/Resources/PathsAndNames?WebSource?", AssertNotNull),
            };

            VerifyWebsiteRequests(requests, new[] { "services", "sources", "dialogs" });
        }

        static void VerifyWebsiteRequests(IEnumerable<Tuple<string, string>> requests, string[] roots)
        {
            foreach(var request in requests)
            {
                foreach(var root in roots)
                {
                    var url = String.Format("{0}{1}/{2}", WebsiteUri, root, request.Item1);

                    try
                    {
                        //------------Execute Test---------------------------
                        var responseData = TestHelper.PostDataToWebserver(url);

                        //------------Assert Results-------------------------
                        if(request.Item2 == AssertNotNull)
                        {
                            Assert.IsNotNull(responseData);
                        }
                        else
                        {
                            StringAssert.StartsWith(responseData, request.Item2);
                        }
                    }
                    catch(WebException wex)
                    {
                        Assert.Fail("{0} - {1}", wex.Message, url);
                    }
                }

            }
        }
    }
}
