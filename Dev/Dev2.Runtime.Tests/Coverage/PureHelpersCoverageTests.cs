/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Auditing;
using Warewolf.Data;
using Warewolf.Data.Decisions.Operations;
using Warewolf.Security.Encryption;
using Warewolf.Triggers;

namespace Dev2.Runtime.Tests.Coverage
{
    [TestClass]
    public class AESThenHMACCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "AESThenHMAC";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NewKey_ReturnsKeyOfCorrectByteLength()
        {
            var key = AESThenHMAC.NewKey();
            Assert.AreEqual(AESThenHMAC.KeyBitSize / 8, key.Length);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncryptDecrypt_String_RoundTrips()
        {
            var crypt = AESThenHMAC.NewKey();
            var auth = AESThenHMAC.NewKey();
            var msg = "Hello, Warewolf!";
            var enc = AESThenHMAC.SimpleEncrypt(msg, crypt, auth);
            var dec = AESThenHMAC.SimpleDecrypt(enc, crypt, auth);
            Assert.AreEqual(msg, dec);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncryptDecrypt_String_WithNonSecretPayload_RoundTrips()
        {
            var crypt = AESThenHMAC.NewKey();
            var auth = AESThenHMAC.NewKey();
            var payload = new byte[] { 1, 2, 3, 4 };
            var enc = AESThenHMAC.SimpleEncrypt("payload-test", crypt, auth, payload);
            var dec = AESThenHMAC.SimpleDecrypt(enc, crypt, auth, payload.Length);
            Assert.AreEqual("payload-test", dec);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncryptDecrypt_Bytes_RoundTrips()
        {
            var crypt = AESThenHMAC.NewKey();
            var auth = AESThenHMAC.NewKey();
            var msg = Encoding.UTF8.GetBytes("binary message");
            var enc = AESThenHMAC.SimpleEncrypt(msg, crypt, auth);
            var dec = AESThenHMAC.SimpleDecrypt(enc, crypt, auth);
            CollectionAssert.AreEqual(msg, dec);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncryptWithPassword_String_RoundTrips()
        {
            var enc = AESThenHMAC.SimpleEncryptWithPassword("secret", "longenoughpassword");
            var dec = AESThenHMAC.SimpleDecryptWithPassword(enc, "longenoughpassword");
            Assert.AreEqual("secret", dec);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncryptWithPassword_Bytes_RoundTrips()
        {
            var msg = Encoding.UTF8.GetBytes("binary");
            var enc = AESThenHMAC.SimpleEncryptWithPassword(msg, "longenoughpassword");
            var dec = AESThenHMAC.SimpleDecryptWithPassword(enc, "longenoughpassword");
            CollectionAssert.AreEqual(msg, dec);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncrypt_NullOrEmpty_String_Throws()
        {
            var crypt = AESThenHMAC.NewKey();
            var auth = AESThenHMAC.NewKey();
            Assert.ThrowsException<ArgumentException>(() => AESThenHMAC.SimpleEncrypt("", crypt, auth));
            Assert.ThrowsException<ArgumentException>(() => AESThenHMAC.SimpleEncrypt((string)null, crypt, auth));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleDecrypt_NullOrEmpty_String_Throws()
        {
            var crypt = AESThenHMAC.NewKey();
            var auth = AESThenHMAC.NewKey();
            Assert.ThrowsException<ArgumentException>(() => AESThenHMAC.SimpleDecrypt("", crypt, auth));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncrypt_WrongKeySize_Throws()
        {
            var bad = new byte[3];
            var good = AESThenHMAC.NewKey();
            Assert.ThrowsException<ArgumentException>(() => AESThenHMAC.SimpleEncrypt(new byte[] { 1 }, bad, good));
            Assert.ThrowsException<ArgumentException>(() => AESThenHMAC.SimpleEncrypt(new byte[] { 1 }, good, bad));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncrypt_NullBytes_Throws()
        {
            var k = AESThenHMAC.NewKey();
            Assert.ThrowsException<ArgumentException>(() => AESThenHMAC.SimpleEncrypt(new byte[0], k, k));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleDecrypt_TamperedCiphertext_ReturnsNull()
        {
            var crypt = AESThenHMAC.NewKey();
            var auth = AESThenHMAC.NewKey();
            var enc = AESThenHMAC.SimpleEncrypt("foo", crypt, auth);
            var sb = new StringBuilder(enc);
            int mid = sb.Length / 2;
            sb[mid] = sb[mid] == 'A' ? 'B' : 'A';
            var dec = AESThenHMAC.SimpleDecrypt(sb.ToString(), crypt, auth);
            Assert.IsNull(dec);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncryptWithPassword_ShortPassword_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => AESThenHMAC.SimpleEncryptWithPassword("msg", "short"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleEncryptWithPassword_NullMessage_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => AESThenHMAC.SimpleEncryptWithPassword((string)null, "longenoughpassword"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleDecryptWithPassword_NullEncrypted_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => AESThenHMAC.SimpleDecryptWithPassword((string)null, "longenoughpassword"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SimpleDecryptWithPassword_WrongPassword_ReturnsNull()
        {
            var enc = AESThenHMAC.SimpleEncryptWithPassword("secret", "longenoughpassword");
            var dec = AESThenHMAC.SimpleDecryptWithPassword(enc, "anotherlongpassword");
            Assert.IsNull(dec);
        }
    }

    [TestClass]
    public class EqualityFactoryCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "EqualityFactory";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GetComparer_UsesProvidedFunc()
        {
            var c = EqualityFactory.GetComparer<int>((a, b) => a - b);
            Assert.IsTrue(c.Compare(2, 5) < 0);
            Assert.IsTrue(c.Compare(5, 2) > 0);
            Assert.AreEqual(0, c.Compare(3, 3));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GetEqualityComparer_UsesProvidedFuncs()
        {
            var c = EqualityFactory.GetEqualityComparer<string>(string.Equals, s => s.Length);
            Assert.IsTrue(c.Equals("a", "a"));
            Assert.IsFalse(c.Equals("a", "b"));
            Assert.AreEqual(4, c.GetHashCode("abcd"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GetComparable_UsesProvidedFunc()
        {
            var c = EqualityFactory.GetComparable<int>(x => 7 - x);
            Assert.AreEqual(2, c.CompareTo(5));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GetEquitable_UsesProvidedFunc()
        {
            var c = EqualityFactory.GetEquitable<int>(x => x == 42);
            Assert.IsTrue(c.Equals(42));
            Assert.IsFalse(c.Equals(7));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GetComparer_WrongMethodNotImplemented_Throws()
        {
            var comparer = EqualityFactory.GetComparer<int>((a, b) => 0) as IEqualityComparer<int>;
            Assert.ThrowsException<NotImplementedException>(() => comparer.Equals(1, 1));
        }
    }

    [TestClass]
    public class HttpClientExceptionCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "HttpClientException";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_Default()
        {
            var ex = new HttpClientException();
            Assert.IsNull(ex.Response);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_Message()
        {
            var ex = new HttpClientException("hi");
            Assert.AreEqual("hi", ex.Message);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_MessageAndInner()
        {
            var inner = new InvalidOperationException("inner");
            var ex = new HttpClientException("outer", inner);
            Assert.AreEqual("outer", ex.Message);
            Assert.AreSame(inner, ex.InnerException);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_HttpResponseMessage_StoresResponse()
        {
            var resp = new HttpResponseMessage(HttpStatusCode.BadGateway);
            var ex = new HttpClientException(resp);
            Assert.AreSame(resp, ex.Response);
            Assert.IsFalse(string.IsNullOrEmpty(ex.Message));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_NullHttpResponseMessage_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new HttpClientException((HttpResponseMessage)null));
        }
    }

    [TestClass]
    public class WorkflowWithInputsCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "WorkflowWithInputs";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void DefaultCtor_ProducesEmptyDefaults()
        {
            var w = new WorkflowWithInputs();
            Assert.AreEqual("", w.Name);
            Assert.AreEqual(Guid.Empty, w.Value);
            Assert.IsNull(w.Inputs);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_Assigns()
        {
            var g = Guid.NewGuid();
            var w = new WorkflowWithInputs("n", g, null);
            Assert.AreEqual("n", w.Name);
            Assert.AreEqual(g, w.Value);
            Assert.AreEqual("n", w.ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Equals_NullAndSelf_Cases()
        {
            var w = new WorkflowWithInputs("a", Guid.Empty, null);
            Assert.IsFalse(w.Equals((WorkflowWithInputs)null));
            Assert.IsFalse(w.Equals((object)null));
            Assert.IsTrue(w.Equals(w));
            Assert.IsTrue(w.Equals((object)w));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Equals_DifferentType_ReturnsFalse()
        {
            var w = new WorkflowWithInputs("a", Guid.Empty, null);
            Assert.IsFalse(w.Equals("string"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Equals_SameValues_True()
        {
            var g = Guid.NewGuid();
            var a = new WorkflowWithInputs("n", g, null);
            var b = new WorkflowWithInputs("n", g, null);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Equals_DifferentValues_False()
        {
            var a = new WorkflowWithInputs("a", Guid.NewGuid(), null);
            var b = new WorkflowWithInputs("b", Guid.NewGuid(), null);
            Assert.IsFalse(a.Equals(b));
            Assert.IsTrue(a != b);
        }
    }

    [TestClass]
    public class NotBetweenCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "NotBetween";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void HandlesType_IsNotBetween()
        {
            var op = new NotBetween();
            Assert.AreEqual("NotBetween", op.HandlesType().ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Invoke_NumericValueBetween_ReturnsFalse()
        {
            var op = new NotBetween();
            Assert.IsFalse(op.Invoke(new[] { "5", "1", "10" }));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Invoke_NumericValueOutside_ReturnsTrue()
        {
            var op = new NotBetween();
            Assert.IsTrue(op.Invoke(new[] { "20", "1", "10" }));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Invoke_DateTimeBetween_ReturnsFalse()
        {
            var op = new NotBetween();
            Assert.IsFalse(op.Invoke(new[] { "2020-06-15", "2020-01-01", "2020-12-31" }));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Invoke_DateTimeOutside_ReturnsTrue()
        {
            var op = new NotBetween();
            Assert.IsTrue(op.Invoke(new[] { "2021-06-15", "2020-01-01", "2020-12-31" }));
        }
    }

    [TestClass]
    public class ExecutionInfoCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "ExecutionInfo";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Defaults_AllNullOrDefault()
        {
            var info = new ExecutionInfo();
            Assert.AreEqual(default(DateTime), info.StartDate);
            Assert.AreEqual(default(TimeSpan), info.Duration);
            Assert.AreEqual(default(DateTime), info.EndDate);
            Assert.AreEqual(default(Guid), info.ExecutionId);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FullCtor_AssignsAll()
        {
            var id = Guid.NewGuid();
            var info = new ExecutionInfo(
                new DateTime(2025, 1, 1),
                TimeSpan.FromSeconds(2),
                new DateTime(2025, 1, 2),
                QueueRunStatus.Success,
                id,
                "fail",
                "tx-1");
            Assert.AreEqual(new DateTime(2025, 1, 1), info.StartDate);
            Assert.AreEqual(TimeSpan.FromSeconds(2), info.Duration);
            Assert.AreEqual(new DateTime(2025, 1, 2), info.EndDate);
            Assert.AreEqual(QueueRunStatus.Success, info.Success);
            Assert.AreEqual(id, info.ExecutionId);
            Assert.AreEqual("fail", info.FailureReason);
            Assert.AreEqual("tx-1", info.CustomTransactionID);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void JsonCtor_FailureReason_EmptyByDefault()
        {
            var id = Guid.NewGuid();
            var info = new ExecutionInfo(
                new DateTime(2025, 1, 1),
                TimeSpan.FromSeconds(2),
                new DateTime(2025, 1, 2),
                QueueRunStatus.Success,
                id,
                "tx-2");
            Assert.AreEqual("", info.FailureReason);
            Assert.AreEqual("tx-2", info.CustomTransactionID);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Setters_RoundTrip()
        {
            var info = new ExecutionInfo();
            info.StartDate = new DateTime(2024, 5, 1);
            info.Duration = TimeSpan.FromMinutes(1);
            info.EndDate = new DateTime(2024, 5, 2);
            info.Success = QueueRunStatus.Error;
            info.ExecutionId = Guid.Empty;
            info.FailureReason = "oops";
            info.CustomTransactionID = "ctx";
            Assert.AreEqual("oops", info.FailureReason);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ExecutionHistory_DefaultsAndCtor()
        {
            var hist = new ExecutionHistory();
            Assert.AreEqual("ExecutionLog", hist.AuditType);
            var info = new ExecutionInfo();
            var rid = Guid.NewGuid();
            var hist2 = new ExecutionHistory(rid, "out", info, "user");
            Assert.AreEqual(rid, hist2.ResourceId);
            Assert.AreEqual("out", hist2.WorkflowOutput);
            Assert.AreSame(info, hist2.ExecutionInfo);
            Assert.AreEqual("user", hist2.UserName);
        }
    }
}
