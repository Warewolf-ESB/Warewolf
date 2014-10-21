
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
using System.Collections.Generic;
using System.Linq;
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

        const string WebServerTestSmall = "Integration Test Resources/WebServerTestSmallString";
        const string WebServerTestLarge = "Integration Test Resources/WebServerTestLargeString";

        const string WebServerTest = "Integration Test Resources/WebServerTest";
        const string WebServerTestExpectedXml = "<DataList><Text>Of resolve to gravity thought my prepare chamber so</Text><Args></Args></DataList>";
        const string WebServerTestExpectedJson = "{\"Text\":\"Of resolve to gravity thought my prepare chamber so\",\"Args\":\"\"}";

        readonly static string[] ServicesEndPoints =
        {
            ServerSettings.WebserverURI
        };

        readonly static string[] WebsiteEndPoints =
        {
            ServerSettings.WebsiteServerUri
        };

        static readonly string[] SslEndPoints =
        {
            ServerSettings.WebserverHttpsURI
        };

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_DataIsNotLarge_DownloadHeadersNotAdded()
        {
            foreach(var endPoint in ServicesEndPoints)
            {
                var path = endPoint + WebServerTestSmall;

                var result = TestHelper.GetResponseFromServer(path);

                var allKeys = result.Headers.AllKeys;
                const string ContentType = "Content-Type";
                const string ContentDisposition = "Content-Disposition";
                Assert.IsNotNull(allKeys.FirstOrDefault(k => string.Equals(ContentType, k, StringComparison.InvariantCultureIgnoreCase)));
                Assert.IsNull(allKeys.FirstOrDefault(k => string.Equals(ContentDisposition, k, StringComparison.InvariantCultureIgnoreCase)));

                var contentTypeValue = result.Headers.Get(ContentType);

                Assert.IsFalse(contentTypeValue.Contains("application/force-download"));
            }
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_DataIsLarge_DownloadHeadersAdded()
        {
            foreach(var endPoint in ServicesEndPoints)
            {
                var path = endPoint + WebServerTestLarge;

                var result = TestHelper.GetResponseFromServer(path);

                var allKeys = result.Headers.AllKeys;
                const string ContentType = "Content-Type";
                const string ContentDisposition = "Content-Disposition";

                Assert.IsNotNull(allKeys.FirstOrDefault(k => string.Equals(ContentType, k, StringComparison.InvariantCultureIgnoreCase)));
                Assert.IsNotNull(allKeys.FirstOrDefault(k => string.Equals(ContentDisposition, k, StringComparison.InvariantCultureIgnoreCase)));

                var contentTypeValue = result.Headers.Get(ContentType);
                var contentDispositionValue = result.Headers.Get(ContentDisposition);

                StringAssert.Contains(contentTypeValue, "application/force-download");
                Assert.AreEqual("attachment; filename=Output.xml", contentDispositionValue);
            }
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_Service_CannotExecuteServiceError()
        {
            VerifyRequest(ServicesEndPoints, new List<Tuple<string, string, AssertType>>
            {
                new Tuple<string, string, AssertType>("Integration Test Resources/CaseSP", @"<InnerError>Can only execute workflows from web browser</InnerError>", AssertType.Contains)
            });
        }

        // -- Trav New -- //
        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_ServiceAsJson_CannotExecuteServiceErrorAsJson()
        {
            const string Expected = "{ \"FatalError\": \"An internal error occurred while executing the service request\",\"errors\": [ \"Can only execute workflows from web browser\"]}";
            VerifyRequest(ServicesEndPoints, new List<Tuple<string, string, AssertType>>
            {
                new Tuple<string, string, AssertType>("Integration Test Resources/CaseSP.json", Expected, AssertType.Equals)
            });
        }

        [TestMethod]
        public void WebServer_ServicesGet_NonExistingServiceAsJson_InternalErrorsAsJson()
        {
            const string Expected = "{ \"FatalError\": \"An internal error occurred while executing the service request\",\"errors\": [ \"Service [ BugXXXX ] not found.\"]}";
            VerifyRequest(ServicesEndPoints, new List<Tuple<string, string, AssertType>>
            {
                new Tuple<string, string, AssertType>("BugXXXX.json", Expected, AssertType.Equals)
            });
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_NonExistingServiceAsXml_InternalErrorAsXml()
        {
            const string Expected = "<FatalError> <Message> An internal error occurred while executing the service request </Message><InnerError>Service [ BugXXXX ] not found.</InnerError></FatalError>";
            VerifyRequest(ServicesEndPoints, new List<Tuple<string, string, AssertType>>
            {
                new Tuple<string, string, AssertType>("BugXXXX.xml", Expected, AssertType.Equals)
            });
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_WorkflowAsXml_ResultAsXml()
        {
            VerifyRequest(ServicesEndPoints, new List<Tuple<string, string, AssertType>>
            {
                new Tuple<string, string, AssertType>(WebServerTest + ".xml", WebServerTestExpectedXml, AssertType.Equals)
            });
        }

        [TestMethod]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_WorkflowAsBadExtension_ResultAsXml()
        {
            VerifyRequest(ServicesEndPoints, new List<Tuple<string, string, AssertType>>
            {
                new Tuple<string, string, AssertType>(WebServerTest + ".ml", WebServerTestExpectedXml, AssertType.Equals)
            });
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebServer_ServicesGet")]
        public void WebServer_ServicesGet_OnSSLPort_ValidResultViaSSL()
        {
            VerifyRequest(SslEndPoints, new List<Tuple<string, string, AssertType>>
            {
                new Tuple<string, string, AssertType>(WebServerTest, WebServerTestExpectedXml, AssertType.Equals)
            });
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServer_ServicesPost")]
        public void WebServer_ServicesPost_Workflow_ResultAsXml()
        {
            const string Input = "hello";
            var postData = string.Format("<DataList><Text></Text><Args></Args><Input>{0}</Input></DataList>", Input);
            var expected = WebServerTestExpectedXml.Replace("<Args></Args>", string.Format("<Args>{0}</Args>", Input));

            VerifyRequest(ServicesEndPoints, new List<Tuple<string, string, AssertType>>
            {
                new Tuple<string, string, AssertType>(WebServerTest + "?" + postData + "?", expected, AssertType.Equals)
            });
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
                new Tuple<string, string>("images/folder-closed.png", @"?PNG"),
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

function WebServiceViewModel(saveContainerID, resourceID, sourceID, environment, resourcePath) {"),
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
        <div id=""body"" class=""ui-widget-content"">
            <form id=""mainForm"">
                <div id=""mainFormError"" class=""error"" style=""display: none; text-align: left; margin-top: 1em; margin-left: 1em; float: left;"">
                    <img src=""../images/warning.png"" alt=""Warning!"" width=""24"" height=""24"" class=""icon"" />
                    <span id=""Dev2Msg"">You missed some fields.The have been highlighted below.</span>
                </div>
                <div class=""decision-form"">"),
                new Tuple<string, string>("Dialogs/SaveDialog.htm", "<div id=\"saveForm\" title=\"Save\""),
                new Tuple<string, string>("Services/WebService.htm", "<div id=\"webServiceContainer\""),
                new Tuple<string, string>("Sources/WebSource.htm", "<div id=\"webSourceContainer\""),
                new Tuple<string, string>("Switch/Drag.htm", @"<div id=""container"" style=""height: 111px;"">
    <div class=""float-left"">
       <div id=""body"" class=""ui-widget-content"">
           <form id=""mainForm"">
            <div class=""mainFormError"" style=""display: none; text-align: right;"">
                <img src=""images/warning.gif"" alt=""Warning!"" width=""24"" height=""24"" class=""icon"" />
                <span id=""Dev2Msg"">You missed some fields.The have been highlighted below.</span>
            </div>
            <div class=""switch-form"">"),
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
            const string Expected = @"<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width"" />
    <title>Warewolf</title>";
            var requests = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("services/webservice", Expected),
                new Tuple<string, string>("sources/websource", Expected),
                new Tuple<string, string>("dialogs/savedialog", Expected),
                //new Tuple<string, string>("dialogs/savedialog?type=WorkflowService", Expected),
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
            };

            VerifyWebsiteRequests(requests, new[] { "services", "sources", "dialogs" });
        }

        static void VerifyWebsiteRequests(IEnumerable<Tuple<string, string>> requests, IEnumerable<string> roots)
        {
            var allRequests = (from request in requests
                               from root in roots
                               select new Tuple<string, string, AssertType>(
                                   string.Format("{0}/{1}", root, request.Item1),
                                   request.Item2,
                                   request.Item2 == AssertNotNull ? AssertType.IsNotNull : AssertType.StartsWith)).ToList();
            VerifyRequest(WebsiteEndPoints, allRequests);
        }

        static void VerifyRequest(IEnumerable<string> endpoints, List<Tuple<string, string, AssertType>> requests)
        {
            foreach(var endPoint in endpoints)
            {
#pragma warning disable 219
                var requestPos = 0;
#pragma warning restore 219
                foreach(var request in requests)
                {
                    //------------Setup for test--------------------------
                    var url = String.Format("{0}{1}", endPoint, request.Item1);
                    try
                    {

                        //------------Execute Test---------------------------
                        bool wasSsl;
                        var responseData = TestHelper.PostDataToWebserver(url, out wasSsl);

                        //------------Assert Results-------------------------
                        switch(request.Item3)
                        {
                            case AssertType.StartsWith:
                                if(!request.Item1.EndsWith(".png"))
                                {
                                    StringAssert.StartsWith(responseData, request.Item2, "Expected [ " + request.Item2 + "] but got [ " + responseData + " ]");
                                }
                                else
                                {
                                    try
                                    {
                                        StringAssert.StartsWith(responseData, request.Item2);
                                    }
                                    catch(AssertFailedException)
                                    {
                                        Assert.Fail("The request did not return the correct png image file.");
                                    }
                                }
                                break;
                            case AssertType.Contains:
                                StringAssert.Contains(responseData, request.Item2, "Expected [ " + request.Item2 + "] but got [ " + responseData + " ]");
                                break;
                            case AssertType.Equals:
                                Assert.AreEqual(responseData, request.Item2, "Expected [ " + request.Item2 + "] but got [ " + responseData + " ]");
                                break;
                            case AssertType.IsNotNull:
                                Assert.IsNotNull(responseData);
                                break;
                        }
                    }
                    catch(WebException wex)
                    {
                        Assert.Fail("{0} - {1}", wex.Message, url);
                    }

                    requestPos++;
                }
            }
        }

        enum AssertType
        {
            StartsWith,
            Contains,
            Equals,
            IsNotNull
        }
    }
}
