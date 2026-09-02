/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for TokenValidationFailureClassifier — WOLF-8512's classification of a
 *  Service Bus trigger token-validation failure as transient (retry via redelivery) or
 *  terminal (a positively-decided bad token, dead-letter). Deliberately exercised with
 *  hand-built exceptions, never a live OIDC round-trip — see the classifier's own doc
 *  comment and ServiceBusSecureTrigger-Architecture.md's "Failure classification" section.
 */

using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Parsers;

namespace Warewolf.Execution.Lightweight.Tests.Auth.Parsers;

[TestClass]
public class TokenValidationFailureClassifierTests
{
    // ── IsTransient — recognised transient shapes ───────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_HttpRequestException_ReturnsTrue()
    {
        Assert.IsTrue(TokenValidationFailureClassifier.IsTransient(new HttpRequestException("connection reset")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_IOException_ReturnsTrue()
    {
        Assert.IsTrue(TokenValidationFailureClassifier.IsTransient(new IOException("stream closed")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_SocketException_ReturnsTrue()
    {
        Assert.IsTrue(TokenValidationFailureClassifier.IsTransient(new SocketException()));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_SecurityTokenSignatureKeyNotFoundException_ReturnsTrue()
    {
        // Despite deriving from SecurityTokenInvalidSignatureException (a terminal type below),
        // "no matching key in the cached JWKS" is a metadata-resolution problem, not a positive
        // bad-signature verdict — see the classifier's own doc comment on ordering.
        Assert.IsTrue(TokenValidationFailureClassifier.IsTransient(new SecurityTokenSignatureKeyNotFoundException("kid not found")));
    }

    [DataTestMethod]
    [DataRow("IDX20803: Unable to obtain configuration from: 'https://login.microsoftonline.com/...'")]
    [DataRow("IDX20804: Unable to retrieve document from: 'https://login.microsoftonline.com/...'")]
    [TestCategory("UnitTest")]
    public void IsTransient_IdentityModelMetadataFailureCode_ReturnsTrue(string message)
    {
        Assert.IsTrue(TokenValidationFailureClassifier.IsTransient(new SecurityTokenException(message)));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_MetadataFailureCodeOnInnerException_ReturnsTrue()
    {
        var inner = new InvalidOperationException("IDX20803: Unable to obtain configuration");
        var outer = new SecurityTokenException("wrapped", inner);

        Assert.IsTrue(TokenValidationFailureClassifier.IsTransient(outer));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_UnrecognisedExceptionType_ReturnsTrue_FailsOpenToRetry()
    {
        Assert.IsTrue(TokenValidationFailureClassifier.IsTransient(new NotSupportedException("never seen this before")));
    }

    // ── IsTransient — terminal (positively-decided bad token) shapes ────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_SecurityTokenInvalidSignatureException_ReturnsFalse()
    {
        Assert.IsFalse(TokenValidationFailureClassifier.IsTransient(new SecurityTokenInvalidSignatureException("bad signature")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_SecurityTokenInvalidAudienceException_ReturnsFalse()
    {
        Assert.IsFalse(TokenValidationFailureClassifier.IsTransient(new SecurityTokenInvalidAudienceException("wrong audience")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_SecurityTokenInvalidIssuerException_ReturnsFalse()
    {
        Assert.IsFalse(TokenValidationFailureClassifier.IsTransient(new SecurityTokenInvalidIssuerException("wrong issuer")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_SecurityTokenExpiredException_ReturnsFalse()
    {
        Assert.IsFalse(TokenValidationFailureClassifier.IsTransient(new SecurityTokenExpiredException("expired")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_SecurityTokenMalformedException_ReturnsFalse()
    {
        Assert.IsFalse(TokenValidationFailureClassifier.IsTransient(new SecurityTokenMalformedException("not a JWT")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_ArgumentException_ReturnsFalse()
    {
        Assert.IsFalse(TokenValidationFailureClassifier.IsTransient(new ArgumentException("token could not be parsed")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_TerminalExceptionWrappedAsInner_ReturnsFalse()
    {
        // The outer type is unrecognised (fail-open) but the ACTUAL cause, one level in, is a
        // positive bad-token verdict — walking InnerException must find it and stay terminal.
        var inner = new SecurityTokenExpiredException("expired");
        var outer = new AggregateException("validation failed", inner);

        Assert.IsFalse(TokenValidationFailureClassifier.IsTransient(outer));
    }

    // ── IsTransient — deliberately NOT this classifier's job ────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsTransient_PlainOperationCanceledException_IsNotClaimedHere_FailsOpenToTransient()
    {
        // Classifying "our own invocation was cancelled" needs the invocation's
        // CancellationToken, which this classifier does not take - that's exclusively
        // ServiceBusWorkflowTriggerFunction.IsOwnInvocationCancellation's job, evaluated in an
        // earlier catch clause. A bare cancellation exception reaching this classifier (i.e. one
        // IsOwnInvocationCancellation did not already claim) falls through to the same fail-open
        // default as any other unrecognised type - it is not given special (terminal) treatment.
        Assert.IsTrue(TokenValidationFailureClassifier.IsTransient(new OperationCanceledException()));
        Assert.IsTrue(TokenValidationFailureClassifier.IsTransient(new TaskCanceledException()));
    }

    // ── ResolveSubReason ─────────────────────────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveSubReason_InvalidSignature_ReturnsSignatureInvalid()
    {
        Assert.AreEqual("SignatureInvalid", TokenValidationFailureClassifier.ResolveSubReason(new SecurityTokenInvalidSignatureException()));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveSubReason_InvalidAudience_ReturnsAudienceInvalid()
    {
        Assert.AreEqual("AudienceInvalid", TokenValidationFailureClassifier.ResolveSubReason(new SecurityTokenInvalidAudienceException()));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveSubReason_InvalidIssuer_ReturnsIssuerInvalid()
    {
        Assert.AreEqual("IssuerInvalid", TokenValidationFailureClassifier.ResolveSubReason(new SecurityTokenInvalidIssuerException()));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveSubReason_Expired_ReturnsExpired()
    {
        Assert.AreEqual("Expired", TokenValidationFailureClassifier.ResolveSubReason(new SecurityTokenExpiredException()));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveSubReason_Malformed_ReturnsMalformed()
    {
        Assert.AreEqual("Malformed", TokenValidationFailureClassifier.ResolveSubReason(new SecurityTokenMalformedException()));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveSubReason_ArgumentException_ReturnsMalformed()
    {
        Assert.AreEqual("Malformed", TokenValidationFailureClassifier.ResolveSubReason(new ArgumentException("bad token")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveSubReason_UnrecognisedType_ReturnsUnclassified()
    {
        Assert.AreEqual("Unclassified", TokenValidationFailureClassifier.ResolveSubReason(new InvalidOperationException("something else")));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveSubReason_WalksInnerExceptionChain()
    {
        var inner = new SecurityTokenExpiredException("expired");
        var outer = new AggregateException("validation failed", inner);

        Assert.AreEqual("Expired", TokenValidationFailureClassifier.ResolveSubReason(outer));
    }
}
