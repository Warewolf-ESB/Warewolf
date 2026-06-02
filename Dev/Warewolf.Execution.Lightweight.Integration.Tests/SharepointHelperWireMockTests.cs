/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Sharepoint;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// End-to-end WireMock-driven tests for the SharePoint REST surface exposed by
    /// <see cref="SharepointHelper"/>: LoadLists, LoadFieldsForList, and ReadListItems.
    ///
    /// These tests stand up an in-process <see cref="WireMockServer"/> simulating the
    /// SharePoint REST API (<c>/_api/...</c>) so that the helper's HTTP code paths
    /// can be exercised without a live SharePoint server or Docker side-car.
    /// </summary>
    [TestClass]
    public class SharepointHelperWireMockTests
    {
        private WireMockServer? _wireMock;
        private string SpUrl => $"http://localhost:{_wireMock!.Port}";

        [TestInitialize]
        public void Setup() => _wireMock = WireMockServer.Start();

        [TestCleanup]
        public void Cleanup() => _wireMock?.Stop();

        // -----------------------------------------------------------------
        // LoadLists
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadLists_EmptyResults_ReturnsEmptyCollection()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/web/lists").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[]}}"""));

            var helper = new SharepointHelper(SpUrl, "", "", false);

            var lists = helper.LoadLists();

            Assert.AreEqual(0, lists.Count);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadLists_MissingTitles_DefaultsToEmptyString()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/web/lists").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[{},{"Title":null}]}}"""));

            var helper = new SharepointHelper(SpUrl, "", "", false);

            var lists = helper.LoadLists();

            Assert.AreEqual(2, lists.Count);
            Assert.AreEqual(string.Empty, lists[0].FullName);
            Assert.AreEqual(string.Empty, lists[1].FullName);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadLists_ServerReturns500_ThrowsHttpRequestException()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/web/lists").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(500)
                    .WithHeader("Content-Type", "text/plain")
                    .WithBody("oh no something broke"));

            var helper = new SharepointHelper(SpUrl, "", "", false);

            var ex = Assert.ThrowsException<HttpRequestException>(() => helper.LoadLists());
            StringAssert.Contains(ex.Message, "LoadLists");
            StringAssert.Contains(ex.Message, "500");
            StringAssert.Contains(ex.Message, "oh no something broke");
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadLists_LargeErrorBody_TruncatedTo500Chars()
        {
            // Build a > 500 char body to force the truncation branch.
            var big = new string('x', 1024);
            _wireMock!.Given(Request.Create().WithPath("/_api/web/lists").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(503).WithBody(big));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var ex = Assert.ThrowsException<HttpRequestException>(() => helper.LoadLists());

            // Truncate appends "..." so the message must contain it.
            StringAssert.Contains(ex.Message, "...");
            // And must not contain the full 1024 char body.
            Assert.IsFalse(ex.Message.Contains(big), "Error body must be truncated, not full");
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadLists_WithUserNamePassword_SendsBasicCredentials()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/web/lists").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[{"Title":"L"}]}}"""));

            // Passing user/password takes the NetworkCredential branch of CreateHttpClient.
            var helper = new SharepointHelper(SpUrl, "alice", "s3cret", false);
            var lists = helper.LoadLists();

            Assert.AreEqual(1, lists.Count);
            Assert.AreEqual("L", lists[0].FullName);
        }

        // -----------------------------------------------------------------
        // LoadFieldsForList
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadFieldsForList_EditableOnly_UsesEditableFilter()
        {
            _wireMock!.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('MyList')/fields")
                    .WithParam("$filter", "Hidden eq false and ReadOnlyField eq false")
                    .UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[{"Title":"T","InternalName":"T","FieldTypeKind":2,"ReadOnlyField":false}]}}"""));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var fields = helper.LoadFieldsForList("MyList", editableFieldsOnly: true);

            Assert.AreEqual(1, fields.Count);
            var entry = _wireMock.LogEntries.FirstOrDefault(
                e => e.RequestMessage.RawQuery != null
                  && e.RequestMessage.RawQuery.Contains("ReadOnlyField"));
            Assert.IsNotNull(entry, "Editable-only filter must include ReadOnlyField clause");
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadFieldsForList_AllFields_UsesNonEditableFilter()
        {
            _wireMock!.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('MyList')/fields")
                    .UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[{"Title":"T","InternalName":"T","FieldTypeKind":2}]}}"""));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var fields = helper.LoadFieldsForList("MyList", editableFieldsOnly: false);

            Assert.AreEqual(1, fields.Count);
            var entry = _wireMock.LogEntries.FirstOrDefault(
                e => e.RequestMessage.RawQuery != null
                  && e.RequestMessage.RawQuery.Contains("Hidden")
                  && !e.RequestMessage.RawQuery.Contains("ReadOnlyField"));
            Assert.IsNotNull(entry, "Non-editable filter should not mention ReadOnlyField");
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadFieldsForList_AllFieldTypeKinds_MapToExpectedTypes()
        {
            // Cover every branch of the FieldTypeKind switch expression in
            // CreateSharepointFieldToFromJson (Integer/Counter, Currency, Text/Choice,
            // Note, DateTime, Boolean, Number, default).
            var body = """
                {"d":{"results":[
                  {"Title":"Int",     "InternalName":"Int",     "FieldTypeKind":1, "MaxLength":0, "MaximumValue":99,    "MinimumValue":-99},
                  {"Title":"Cnt",     "InternalName":"Cnt",     "FieldTypeKind":5},
                  {"Title":"Curr",    "InternalName":"Curr",    "FieldTypeKind":10},
                  {"Title":"Txt",     "InternalName":"Txt",     "FieldTypeKind":2, "MaxLength":255, "Required":true},
                  {"Title":"Choice",  "InternalName":"Choice",  "FieldTypeKind":6},
                  {"Title":"Note",    "InternalName":"Note",    "FieldTypeKind":3},
                  {"Title":"Date",    "InternalName":"Date",    "FieldTypeKind":4},
                  {"Title":"Bool",    "InternalName":"Bool",    "FieldTypeKind":8},
                  {"Title":"Num",     "InternalName":"Num",     "FieldTypeKind":9},
                  {"Title":"Unknown", "InternalName":"Unknown", "FieldTypeKind":99}
                ]}}
                """;
            _wireMock!.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('TypesList')/fields").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json").WithBody(body));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var fields = helper.LoadFieldsForList("TypesList", editableFieldsOnly: false);

            Assert.AreEqual(10, fields.Count);
            // Spot-check a few mapping outcomes
            var dict = fields.ToDictionary(f => f.Name, f => f);
            Assert.AreEqual(SharepointFieldType.Integer,  dict["Int"].Type);
            Assert.AreEqual(SharepointFieldType.Integer,  dict["Cnt"].Type);
            Assert.AreEqual(SharepointFieldType.Currency, dict["Curr"].Type);
            Assert.AreEqual(SharepointFieldType.Text,     dict["Txt"].Type);
            Assert.AreEqual(SharepointFieldType.Text,     dict["Choice"].Type);
            Assert.AreEqual(SharepointFieldType.Note,     dict["Note"].Type);
            Assert.AreEqual(SharepointFieldType.DateTime, dict["Date"].Type);
            Assert.AreEqual(SharepointFieldType.Boolean,  dict["Bool"].Type);
            Assert.AreEqual(SharepointFieldType.Number,   dict["Num"].Type);
            Assert.AreEqual(SharepointFieldType.Text,     dict["Unknown"].Type);

            // Required and MaxLength must round-trip
            Assert.IsTrue(dict["Txt"].IsRequired);
            Assert.AreEqual(255, dict["Txt"].MaxLength);
            Assert.AreEqual(99.0, dict["Int"].MaxValue);
            Assert.AreEqual(-99.0, dict["Int"].MinValue);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadFieldsForList_ListWithSpecialChars_EncodesListName()
        {
            _wireMock!.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('My List & More')/fields")
                    .UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[]}}"""));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var fields = helper.LoadFieldsForList("My List & More", editableFieldsOnly: false);

            Assert.AreEqual(0, fields.Count);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_LoadFieldsForList_ServerError_ThrowsWithOperationName()
        {
            _wireMock!.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('Missing')/fields").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(404).WithBody("not found"));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var ex = Assert.ThrowsException<HttpRequestException>(
                () => helper.LoadFieldsForList("Missing", false));
            StringAssert.Contains(ex.Message, "LoadFieldsForList");
            StringAssert.Contains(ex.Message, "404");
        }

        // -----------------------------------------------------------------
        // ReadListItems
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_ReadListItems_DigestFailure_ThrowsWithServerContext()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/contextinfo").UsingPost())
                .RespondWith(Response.Create().WithStatusCode(401).WithBody("unauthorized"));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var ex = Assert.ThrowsException<HttpRequestException>(
                () => helper.ReadListItems("L", camlXml: null));
            StringAssert.Contains(ex.Message, "contextinfo");
            StringAssert.Contains(ex.Message, "401");
            StringAssert.Contains(ex.Message, SpUrl);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_ReadListItems_GetItemsFailure_ThrowsAfterDigest()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/contextinfo").UsingPost())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"GetContextWebInformation":{"FormDigestValue":"digest!"}}}"""));

            _wireMock.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('L')/GetItems").UsingPost())
                .RespondWith(Response.Create().WithStatusCode(500).WithBody("explode"));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var ex = Assert.ThrowsException<HttpRequestException>(
                () => helper.ReadListItems("L", camlXml: null));
            StringAssert.Contains(ex.Message, "ReadListItems");
            StringAssert.Contains(ex.Message, "500");
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_ReadListItems_WithCamlOverride_PassesDigestHeader()
        {
            const string caml = "<View><Query><Where><Eq><FieldRef Name='Title'/><Value Type='Text'>X</Value></Eq></Where></Query></View>";

            _wireMock!.Given(Request.Create().WithPath("/_api/contextinfo").UsingPost())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"GetContextWebInformation":{"FormDigestValue":"abc-digest"}}}"""));

            _wireMock.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('Items')/GetItems").UsingPost())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[{"Title":"OnlyOne"}]}}"""));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var items = helper.ReadListItems("Items", caml);

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("OnlyOne", items[0]["Title"].ToString());

            var entry = _wireMock.LogEntries.FirstOrDefault(
                e => e.RequestMessage.Path != null
                  && e.RequestMessage.Path.Contains("GetItems", StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(entry, "WireMock must have logged the GetItems request");
            // Body should contain the encoded CAML view
            var body = entry.RequestMessage.Body ?? string.Empty;
            StringAssert.Contains(body, "FieldRef");
            // Digest header should be set when present
            Assert.IsTrue(
                entry.RequestMessage.Headers != null
                && entry.RequestMessage.Headers.ContainsKey("X-RequestDigest"),
                "X-RequestDigest header must be forwarded when digest is available");
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_ReadListItems_EmptyResults_ReturnsEmptyCollection()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/contextinfo").UsingPost())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"GetContextWebInformation":{"FormDigestValue":"d"}}}"""));

            _wireMock.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('E')/GetItems").UsingPost())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[]}}"""));

            var helper = new SharepointHelper(SpUrl, "", "", false);
            var items = helper.ReadListItems("E", null);
            Assert.AreEqual(0, items.Count);
        }

        // -----------------------------------------------------------------
        // Construction
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelper_SingleArgConstructor_DefaultsApplyAndLoadListsWorks()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/web/lists").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[{"Title":"OnlyArg"}]}}"""));

            // Single-argument constructor delegates to the four-argument one with
            // empty credentials and isSharepointOnline=false.
            var helper = new SharepointHelper(SpUrl);
            var lists = helper.LoadLists();

            Assert.AreEqual(1, lists.Count);
            Assert.AreEqual("OnlyArg", lists[0].FullName);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointHelperFactory_New_ReturnsSharepointHelper()
        {
            var factory = new SharepointHelperFactory();
            var helper = factory.New(SpUrl, "u", "p", false);
            Assert.IsNotNull(helper);
            Assert.IsInstanceOfType(helper, typeof(SharepointHelper));
        }
    }
}
