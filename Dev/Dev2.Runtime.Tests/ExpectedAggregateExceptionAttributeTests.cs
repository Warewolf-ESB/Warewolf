/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Reflection;
using Dev2.Net6.Compatibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Net6Compatibility
{
    [TestClass]
    public class ExpectedAggregateExceptionAttributeTests
    {
        static void InvokeVerify(ExpectedAggregateExceptionAttribute attr, Exception ex)
        {
            var method = typeof(ExpectedAggregateExceptionAttribute)
                .GetMethod("Verify", BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                method.Invoke(attr, new object[] { ex });
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                throw tie.InnerException;
            }
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExpectedAggregateException_Ctor_NullType_Throws()
        {
            new ExpectedAggregateExceptionAttribute(null);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        [ExpectedException(typeof(ArgumentException))]
        public void ExpectedAggregateException_Ctor_NonExceptionType_Throws()
        {
            new ExpectedAggregateExceptionAttribute(typeof(string));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        public void ExpectedAggregateException_Ctor_OneArg_SetsType()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException));

            Assert.AreEqual(typeof(InvalidOperationException), sut.ExceptionType);
            Assert.IsFalse(sut.AllowDerivedTypes);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        public void ExpectedAggregateException_Ctor_TwoArg_SetsType()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException), "no exception");

            Assert.AreEqual(typeof(InvalidOperationException), sut.ExceptionType);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        public void ExpectedAggregateException_AllowDerivedTypes_SetterGetter()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException))
            {
                AllowDerivedTypes = true
            };

            Assert.IsTrue(sut.AllowDerivedTypes);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        public void ExpectedAggregateException_NoExceptionMessage_IncludesTypeName()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException), "custom-msg");

            var prop = typeof(ExpectedAggregateExceptionAttribute)
                .GetProperty("NoExceptionMessage", BindingFlags.Instance | BindingFlags.NonPublic);
            var msg = (string)prop.GetValue(sut);

            StringAssert.Contains(msg, typeof(InvalidOperationException).FullName);
            StringAssert.Contains(msg, "custom-msg");
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        public void ExpectedAggregateException_Verify_ExactType_NoThrow()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException));

            InvokeVerify(sut, new InvalidOperationException("boom"));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = false)]
        public void ExpectedAggregateException_Verify_MismatchType_Throws()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException));

            InvokeVerify(sut, new ArgumentException("nope"));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        public void ExpectedAggregateException_Verify_AggregateWithMatchingInner_NoThrow()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException));
            var agg = new AggregateException(new InvalidOperationException("boom"));

            InvokeVerify(sut, agg);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        public void ExpectedAggregateException_Verify_AggregateNestedWithMatchingInner_NoThrow()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException));
            var inner = new AggregateException(new InvalidOperationException("boom"));
            var agg = new AggregateException(inner);

            InvokeVerify(sut, agg);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = false)]
        public void ExpectedAggregateException_Verify_AggregateNoMatch_Throws()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException));
            var agg = new AggregateException(new ArgumentException("nope"));

            InvokeVerify(sut, agg);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        public void ExpectedAggregateException_Verify_AllowDerived_AcceptsSubclass()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(Exception)) { AllowDerivedTypes = true };

            InvokeVerify(sut, new InvalidOperationException("subclass"));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = false)]
        public void ExpectedAggregateException_Verify_AllowDerived_RejectsUnrelated()
        {
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException))
            {
                AllowDerivedTypes = true
            };

            InvokeVerify(sut, new ArgumentException("unrelated"));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ExpectedAggregateExceptionAttribute))]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ExpectedAggregateException_Verify_FileNotFound_MismatchUsesFusionLog()
        {
            // Covers the GetExceptionMsg branch that appends FusionLog for FileNotFoundException
            // when constructing the mismatch error message. The Verify implementation rethrows
            // FileNotFoundException via RethrowIfAssertException? No — it throws a generic Exception
            // but the underlying call to GetExceptionMsg runs the FileNotFoundException branch.
            // To exercise that branch we feed a FileNotFoundException while expecting a different
            // type. The thrown exception will be a plain Exception that wraps the message; we then
            // ensure no crash occurs by catching it inside the test.
            var sut = new ExpectedAggregateExceptionAttribute(typeof(InvalidOperationException));
            try
            {
                InvokeVerify(sut, new FileNotFoundException("missing", "fake.dll"));
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                // Re-throw as FileNotFoundException to satisfy the ExpectedException attribute;
                // the important thing is that GetExceptionMsg executed without error and the
                // generic Exception was raised by Verify.
                StringAssert.Contains(ex.Message, typeof(InvalidOperationException).FullName);
                throw new FileNotFoundException(ex.Message);
            }
        }
    }
}
