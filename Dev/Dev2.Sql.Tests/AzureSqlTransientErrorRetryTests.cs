/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Services.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Sql.Tests
{
    [TestClass]
    public class AzureSqlTransientErrorRetryTests
    {
        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("AzureSqlTransientErrorRetry")]
        public void IsTransientErrorNumber_GivenKnownTransientAzureSqlErrorNumbers_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var transientErrorNumbers = new[] { 40613, 40197, 40501, 40540, 49918, 49919, 49920, 4060, 10928, 10929, 10053, 10054, 10060 };

            //------------Execute Test---------------------------
            foreach (var errorNumber in transientErrorNumbers)
            {
                //------------Assert Results-------------------------
                Assert.IsTrue(AzureSqlTransientErrorRetry.IsTransientErrorNumber(errorNumber), $"Error number {errorNumber} should be classified as transient.");
            }
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("AzureSqlTransientErrorRetry")]
        public void IsTransientErrorNumber_GivenNonTransientAzureSqlErrorNumbers_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var nonTransientErrorNumbers = new[] { 18456, 208, 2812, 547 };

            //------------Execute Test---------------------------
            foreach (var errorNumber in nonTransientErrorNumbers)
            {
                //------------Assert Results-------------------------
                Assert.IsFalse(AzureSqlTransientErrorRetry.IsTransientErrorNumber(errorNumber), $"Error number {errorNumber} (login failed / invalid object / constraint violation) should not be classified as transient.");
            }
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("AzureSqlTransientErrorRetry")]
        public void IsTransientErrorNumber_GivenProcedureTextUnavailableError_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            // 15197 ("There is no text for object '%s'.") was previously (incorrectly) treated as
            // transient. It is raised whenever the caller cannot read a module's definition -
            // missing VIEW DEFINITION, or an encrypted procedure - which is deterministic and
            // per-principal. Retrying can never clear it and only holds a pooled connection open
            // for the entire backoff budget, exhausting the pool under concurrent load.
            const int procedureTextUnavailable = 15197;

            //------------Execute Test---------------------------
            var result = AzureSqlTransientErrorRetry.IsTransientErrorNumber(procedureTextUnavailable);

            //------------Assert Results-------------------------
            Assert.IsFalse(result, "Error 15197 is a permanent VIEW DEFINITION/encryption condition and must never be retried.");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("AzureSqlTransientErrorRetry")]
        public void IsTransient_GivenNonSqlException_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var ex = new InvalidOperationException("not a SQL error");

            //------------Execute Test---------------------------
            var result = AzureSqlTransientErrorRetry.IsTransient(ex);

            //------------Assert Results-------------------------
            Assert.IsFalse(result, "A non-SqlException must never be classified as transient.");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("AzureSqlTransientErrorRetry")]
        public void Retry_GivenActionSucceedsFirstTry_DoesNotRetryOrSleep()
        {
            //------------Setup for test--------------------------
            var callCount = 0;
            var sleepCalls = new List<TimeSpan>();
            var onRetryCalls = new List<(int attempt, int maxAttempts)>();

            //------------Execute Test---------------------------
            AzureSqlTransientErrorRetry.Retry(
                openConnection: () => { callCount++; },
                isTransient: ex => true,
                maxAttempts: 3,
                baseDelay: TimeSpan.FromSeconds(5),
                onRetry: (attempt, maxAttempts, ex) => onRetryCalls.Add((attempt, maxAttempts)),
                delay: sleepCalls.Add);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, callCount, "The action should only be invoked once when it succeeds immediately.");
            Assert.AreEqual(0, sleepCalls.Count, "No backoff delay should occur when the first attempt succeeds.");
            Assert.AreEqual(0, onRetryCalls.Count, "onRetry must not be invoked when the first attempt succeeds.");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("AzureSqlTransientErrorRetry")]
        public void Retry_GivenTransientFailureThenSuccess_RetriesAndSucceeds()
        {
            //------------Setup for test--------------------------
            var callCount = 0;
            var onRetryCalls = new List<(int attempt, int maxAttempts)>();

            //------------Execute Test---------------------------
            AzureSqlTransientErrorRetry.Retry(
                openConnection: () =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        throw new InvalidOperationException("transient");
                    }
                },
                isTransient: ex => true,
                maxAttempts: 3,
                baseDelay: TimeSpan.FromMilliseconds(1),
                onRetry: (attempt, maxAttempts, ex) => onRetryCalls.Add((attempt, maxAttempts)),
                delay: _ => { });

            //------------Assert Results-------------------------
            Assert.AreEqual(2, callCount, "The action should be invoked twice: the failing attempt then the succeeding retry.");
            CollectionAssert.AreEqual(new[] { (1, 3) }, onRetryCalls, "onRetry should be invoked once, reporting attempt 1 of 3.");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("AzureSqlTransientErrorRetry")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Retry_GivenNonTransientFailure_ThrowsImmediatelyWithoutRetry()
        {
            //------------Setup for test--------------------------
            var callCount = 0;

            try
            {
                //------------Execute Test---------------------------
                AzureSqlTransientErrorRetry.Retry(
                    openConnection: () =>
                    {
                        callCount++;
                        throw new InvalidOperationException("not transient");
                    },
                    isTransient: ex => false,
                    maxAttempts: 3,
                    baseDelay: TimeSpan.FromMilliseconds(1),
                    delay: _ => Assert.Fail("A non-transient failure must not sleep/retry."));
            }
            finally
            {
                //------------Assert Results-------------------------
                Assert.AreEqual(1, callCount, "A non-transient failure must fail fast after a single attempt.");
            }
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("AzureSqlTransientErrorRetry")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Retry_GivenAlwaysTransientFailure_ThrowsAfterMaxAttempts()
        {
            //------------Setup for test--------------------------
            var callCount = 0;

            try
            {
                //------------Execute Test---------------------------
                AzureSqlTransientErrorRetry.Retry(
                    openConnection: () =>
                    {
                        callCount++;
                        throw new InvalidOperationException("always transient");
                    },
                    isTransient: ex => true,
                    maxAttempts: 3,
                    baseDelay: TimeSpan.FromMilliseconds(1),
                    delay: _ => { });
            }
            finally
            {
                //------------Assert Results-------------------------
                Assert.AreEqual(3, callCount, "All 3 attempts must be exhausted before the final failure is rethrown.");
            }
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("AzureSqlTransientErrorRetry")]
        public void Retry_GivenTransientFailure_AppliesExponentialBackoffDelay()
        {
            //------------Setup for test--------------------------
            var sleepCalls = new List<TimeSpan>();
            var callCount = 0;

            try
            {
                //------------Execute Test---------------------------
                AzureSqlTransientErrorRetry.Retry(
                    openConnection: () =>
                    {
                        callCount++;
                        throw new InvalidOperationException("always transient");
                    },
                    isTransient: ex => true,
                    maxAttempts: 3,
                    baseDelay: TimeSpan.FromSeconds(5),
                    delay: sleepCalls.Add);
            }
            catch (InvalidOperationException)
            {
                // Expected once maxAttempts is exhausted; backoff values are asserted below.
            }

            //------------Assert Results-------------------------
            Assert.AreEqual(2, sleepCalls.Count, "A backoff delay should occur after each retryable failure except the final attempt.");
            Assert.AreEqual(TimeSpan.FromSeconds(5), sleepCalls[0], "The first backoff should equal baseDelay.");
            Assert.AreEqual(TimeSpan.FromSeconds(10), sleepCalls[1], "The second backoff should double baseDelay.");
        }
    }
}
