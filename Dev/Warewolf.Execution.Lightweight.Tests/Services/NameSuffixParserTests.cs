/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for NameSuffixParser covering:
 *    • Query-string stripping  (literal '?', encoded '%3F', path-like values)
 *    • URL decoding             (%20, %2E, %5C, %2B, %3F)
 *    • Backslash normalization  (raw '\', mixed '\' and '/', '%5C')
 *  for all three public methods: Parse, IsApisJsonRequest, ExtractApisJsonPath.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight;

namespace Warewolf.Execution.Lightweight.Tests.Services;

[TestClass]
public class NameSuffixParserTests
{
    // ── Normalize (tested indirectly through the public API) ─────────────────
    // A dedicated Normalize test would require the method to be internal-visible;
    // all three public methods call it, so coverage is complete via those paths.

    // ═══════════════════════════════════════════════════════════════════════
    // Parse — suffix detection
    // ═══════════════════════════════════════════════════════════════════════

    // ── Basic suffix recognition ──────────────────────────────────────────

    [TestMethod]
    public void Parse_ApiSuffix_SetsIsApiTrue()
    {
        var (name, isDebug, isXml, isApi) = NameSuffixParser.Parse("MyWorkflow.api");
        Assert.AreEqual("MyWorkflow", name);
        Assert.IsFalse(isDebug);
        Assert.IsFalse(isXml);
        Assert.IsTrue(isApi);
    }

    [TestMethod]
    public void Parse_DebugSuffix_SetsIsDebugTrue()
    {
        var (name, isDebug, isXml, isApi) = NameSuffixParser.Parse("MyWorkflow.debug");
        Assert.AreEqual("MyWorkflow", name);
        Assert.IsTrue(isDebug);
        Assert.IsFalse(isXml);
        Assert.IsFalse(isApi);
    }

    [TestMethod]
    public void Parse_XmlSuffix_SetsIsXmlTrue()
    {
        var (name, isDebug, isXml, isApi) = NameSuffixParser.Parse("MyWorkflow.xml");
        Assert.AreEqual("MyWorkflow", name);
        Assert.IsFalse(isDebug);
        Assert.IsTrue(isXml);
        Assert.IsFalse(isApi);
    }

    [TestMethod]
    public void Parse_JsonSuffix_NoFlagsSet()
    {
        var (name, isDebug, isXml, isApi) = NameSuffixParser.Parse("MyWorkflow.json");
        Assert.AreEqual("MyWorkflow", name);
        Assert.IsFalse(isDebug);
        Assert.IsFalse(isXml);
        Assert.IsFalse(isApi);
    }

    [TestMethod]
    public void Parse_NoSuffix_NoFlagsSet()
    {
        var (name, isDebug, isXml, isApi) = NameSuffixParser.Parse("MyWorkflow");
        Assert.AreEqual("MyWorkflow", name);
        Assert.IsFalse(isDebug); Assert.IsFalse(isXml); Assert.IsFalse(isApi);
    }

    // ── Query string stripping ────────────────────────────────────────────

    [TestMethod]
    public void Parse_ApiSuffix_WithQueryString_DetectedCorrectly()
    {
        // Without normalization, EndsWith(".api") would fail because the string
        // ends with "?param=val", not ".api".
        var (_, _, _, isApi) = NameSuffixParser.Parse("MyWorkflow.api?param=val");
        Assert.IsTrue(isApi);
    }

    [TestMethod]
    public void Parse_JsonSuffix_WithMultipleQueryParams_DetectedCorrectly()
    {
        var (name, _, _, _) = NameSuffixParser.Parse("MyWorkflow.json?a=1&b=2");
        Assert.AreEqual("MyWorkflow", name);
    }

    [TestMethod]
    public void Parse_ApiSuffix_WithEncodedQuerySeparator_DetectedCorrectly()
    {
        // %3F is the percent-encoding of '?'. It must be stripped before decoding
        // so it does not slip through as a literal '?' after Uri.UnescapeDataString.
        var (_, _, _, isApi) = NameSuffixParser.Parse("MyWorkflow.api%3Fparam%3Dval");
        Assert.IsTrue(isApi);
    }

    [TestMethod]
    public void Parse_QueryStringContainsPathLikeValue_DoesNotPolluteName()
    {
        // A query value that looks like a path must not affect the extracted name.
        var (name, _, _, _) = NameSuffixParser.Parse("Flow.json?redirect=/secure/Other.json");
        Assert.AreEqual("Flow", name);
    }

    // ── URL decoding ──────────────────────────────────────────────────────

    [TestMethod]
    public void Parse_EncodedSpace_InName_DecodedBeforeSuffixStrip()
    {
        // %20 in the workflow name must be decoded so the returned name contains
        // a real space rather than the escaped sequence.
        var (name, _, _, _) = NameSuffixParser.Parse("Hello%20World.json");
        Assert.AreEqual("Hello World", name);
    }

    [TestMethod]
    public void Parse_EncodedDot_InSuffix_RecognisedAsExtension()
    {
        // %2E is the encoding of '.'.  After decoding, ".api" must be recognised.
        var (_, _, _, isApi) = NameSuffixParser.Parse("MyWorkflow%2Eapi");
        Assert.IsTrue(isApi);
    }

    [TestMethod]
    public void Parse_EncodedPlus_InName_DecodedLiterally()
    {
        // %2B → '+'; the character is valid in a workflow name.
        var (name, _, _, _) = NameSuffixParser.Parse("A%2BB.json");
        Assert.AreEqual("A+B", name);
    }

    [TestMethod]
    public void Parse_EncodedBackslash_InPath_NormalisedToForwardSlash()
    {
        // %5C (backslash) in the folder separator must become '/' after decoding.
        var (name, _, _, _) = NameSuffixParser.Parse("folder%5CMyWorkflow.json");
        Assert.AreEqual("folder/MyWorkflow", name);
    }

    // ── Backslash normalisation ───────────────────────────────────────────

    [TestMethod]
    public void Parse_RawBackslash_InPath_NormalisedToForwardSlash()
    {
        var (name, _, _, _) = NameSuffixParser.Parse("folder\\MyWorkflow.json");
        Assert.AreEqual("folder/MyWorkflow", name);
    }

    [TestMethod]
    public void Parse_MixedSlashes_AllNormalisedToForwardSlash()
    {
        var (name, _, _, _) = NameSuffixParser.Parse("a\\b/c.json");
        Assert.AreEqual("a/b/c", name);
    }

    [TestMethod]
    public void Parse_RawBackslash_WithQueryString_BothHandledTogether()
    {
        var (name, _, _, isApi) = NameSuffixParser.Parse("folder\\Flow.api?debug=true");
        Assert.AreEqual("folder/Flow", name);
        Assert.IsTrue(isApi);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // IsApisJsonRequest
    // ═══════════════════════════════════════════════════════════════════════

    // ── Basic recognition ─────────────────────────────────────────────────

    [TestMethod]
    public void IsApisJsonRequest_BareApisJson_ReturnsTrue()
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("apis.json"));

    [TestMethod]
    public void IsApisJsonRequest_FolderApisJson_ReturnsTrue()
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("MyFolder/apis.json"));

    [TestMethod]
    public void IsApisJsonRequest_OtherJson_ReturnsFalse()
        => Assert.IsFalse(NameSuffixParser.IsApisJsonRequest("MyWorkflow.json"));

    // ── Query string stripping ────────────────────────────────────────────

    [TestMethod]
    public void IsApisJsonRequest_WithQueryString_ReturnsTrue()
    {
        // Without normalization, EndsWith("apis.json") fails because the string
        // ends with "?x=1", not "apis.json".
        Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("apis.json?x=1"));
    }

    [TestMethod]
    public void IsApisJsonRequest_FolderWithQueryString_ReturnsTrue()
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("Folder/apis.json?param=val"));

    [TestMethod]
    public void IsApisJsonRequest_WithEncodedQuerySeparator_ReturnsTrue()
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("apis.json%3Fparam%3Dval"));

    [TestMethod]
    public void IsApisJsonRequest_QueryStringContainsApisJson_ReturnsFalse()
    {
        // The "apis.json" in the query value must not cause a false positive.
        Assert.IsFalse(NameSuffixParser.IsApisJsonRequest("MyFlow.json?redirect=/Folder/apis.json"));
    }

    // ── URL decoding ──────────────────────────────────────────────────────

    [TestMethod]
    public void IsApisJsonRequest_EncodedDotInExtension_ReturnsTrue()
        // %2E decodes to '.'; "apis%2Ejson" → "apis.json"
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("apis%2Ejson"));

    [TestMethod]
    public void IsApisJsonRequest_EncodedSpaceInFolderName_ReturnsTrue()
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("My%20Folder/apis.json"));

    // ── Backslash normalisation ───────────────────────────────────────────

    [TestMethod]
    public void IsApisJsonRequest_RawBackslashSeparator_ReturnsTrue()
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("Folder\\apis.json"));

    [TestMethod]
    public void IsApisJsonRequest_EncodedBackslashSeparator_ReturnsTrue()
        // %5C → '\' → normalised to '/' → "Folder/apis.json"
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("Folder%5Capis.json"));

    [TestMethod]
    public void IsApisJsonRequest_MixedSlashes_ReturnsTrue()
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("A\\B/apis.json"));

    [TestMethod]
    public void IsApisJsonRequest_RawBackslashWithQueryString_ReturnsTrue()
        => Assert.IsTrue(NameSuffixParser.IsApisJsonRequest("Folder\\apis.json?x=1"));

    // ═══════════════════════════════════════════════════════════════════════
    // ExtractApisJsonPath
    // ═══════════════════════════════════════════════════════════════════════

    // ── Basic extraction ──────────────────────────────────────────────────

    [TestMethod]
    public void ExtractApisJsonPath_BareApisJson_ReturnsNull()
        => Assert.IsNull(NameSuffixParser.ExtractApisJsonPath("apis.json"));

    [TestMethod]
    public void ExtractApisJsonPath_SingleFolder_ReturnsFolderName()
        => Assert.AreEqual("MyFolder", NameSuffixParser.ExtractApisJsonPath("MyFolder/apis.json"));

    [TestMethod]
    public void ExtractApisJsonPath_NestedFolders_ReturnsFullPath()
        => Assert.AreEqual("A/B", NameSuffixParser.ExtractApisJsonPath("A/B/apis.json"));

    // ── Query string stripping ────────────────────────────────────────────

    [TestMethod]
    public void ExtractApisJsonPath_WithQueryString_QueryStripped()
    {
        // Without normalization, Split("/apis.json") would fail because the segment
        // ends with "/apis.json?param=val", not "/apis.json".
        Assert.AreEqual("MyFolder", NameSuffixParser.ExtractApisJsonPath("MyFolder/apis.json?param=val"));
    }

    [TestMethod]
    public void ExtractApisJsonPath_BareApisJsonWithQueryString_ReturnsNull()
        => Assert.IsNull(NameSuffixParser.ExtractApisJsonPath("apis.json?x=1&y=2"));

    [TestMethod]
    public void ExtractApisJsonPath_WithEncodedQuerySeparator_ReturnsFolder()
        => Assert.AreEqual("Folder", NameSuffixParser.ExtractApisJsonPath("Folder/apis.json%3Fx%3D1"));

    // ── URL decoding ──────────────────────────────────────────────────────

    [TestMethod]
    public void ExtractApisJsonPath_EncodedSpaceInFolder_ReturnsDecodedFolderName()
        => Assert.AreEqual("My Folder", NameSuffixParser.ExtractApisJsonPath("My%20Folder/apis.json"));

    [TestMethod]
    public void ExtractApisJsonPath_EncodedDotInExtension_StillReturnsFolder()
        // "Folder/apis%2Ejson" → after decode → "Folder/apis.json"
        => Assert.AreEqual("Folder", NameSuffixParser.ExtractApisJsonPath("Folder/apis%2Ejson"));

    [TestMethod]
    public void ExtractApisJsonPath_NestedEncodedFolders_ReturnsDecodedPath()
        => Assert.AreEqual("My Folder/Sub Path", NameSuffixParser.ExtractApisJsonPath("My%20Folder/Sub%20Path/apis.json"));

    // ── Backslash normalisation ───────────────────────────────────────────

    [TestMethod]
    public void ExtractApisJsonPath_RawBackslashSeparator_ReturnsForwardSlashPath()
        => Assert.AreEqual("Folder", NameSuffixParser.ExtractApisJsonPath("Folder\\apis.json"));

    [TestMethod]
    public void ExtractApisJsonPath_EncodedBackslashSeparator_ReturnsForwardSlashPath()
        => Assert.AreEqual("Folder", NameSuffixParser.ExtractApisJsonPath("Folder%5Capis.json"));

    [TestMethod]
    public void ExtractApisJsonPath_NestedRawBackslashes_ReturnsForwardSlashPath()
        => Assert.AreEqual("A/B", NameSuffixParser.ExtractApisJsonPath("A\\B\\apis.json"));

    [TestMethod]
    public void ExtractApisJsonPath_MixedSlashes_ReturnsNormalisedPath()
        => Assert.AreEqual("A/B", NameSuffixParser.ExtractApisJsonPath("A\\B/apis.json"));

    [TestMethod]
    public void ExtractApisJsonPath_RawBackslashWithQueryString_BothHandledTogether()
        => Assert.AreEqual("Folder", NameSuffixParser.ExtractApisJsonPath("Folder\\apis.json?x=1"));
}
