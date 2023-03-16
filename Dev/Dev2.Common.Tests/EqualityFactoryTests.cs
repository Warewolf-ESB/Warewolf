/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class EqualityFactoryTests
    {
        class Example
        {
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(EqualityFactory))]
        public void EqualityFactory_GetComparer_Compare()
        {
            var expected = 1234;
            var cmp = EqualityFactory.GetComparer<Example>((a, b) => expected);
            var actual = cmp.Compare(new Example(), new Example());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(EqualityFactory))]
        public void EqualityFactory_GetEqualityComparer_Equals_GetHashCode()
        {
            var expectedHash = 4321;
            var expectedBool = true;
            var cmp = EqualityFactory.GetEqualityComparer<Example>((a, b) => expectedBool, (a) => expectedHash);
            var actualBool = cmp.Equals(new Example(), new Example());
            var actual = (cmp as IEqualityComparer<Example>).GetHashCode(new Example());

            Assert.AreEqual(expectedBool, actualBool);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(EqualityFactory))]
        public void EqualityFactory_GetComparable_CompareTo()
        {
            var expectedHash = 4321;
            var cmp = EqualityFactory.GetComparable<Example>((a) => expectedHash);
            var actual = cmp.CompareTo(new Example());

            Assert.AreEqual(expectedHash, actual);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(EqualityFactory))]
        public void EqualityFactory_GetEquitable_GetHashCode_NoHashCodeCallback()
        {
            var expected = true;
            var cmp = EqualityFactory.GetEquitable<Example>((a) => expected);

            try
            {
                var actual = (cmp as IEqualityComparer<Example>).GetHashCode(new Example());
                Assert.Fail("expected exception This method is not implemented for this instance.");
            } catch (Exception e)
            {
                Assert.AreEqual("This method is not implemented for this instance.", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(EqualityFactory))]
        public void EqualityFactory_GetEquitable_IEqualityComparer_Equals()
        {
            var expected = true;
            var cmp = EqualityFactory.GetEquitable<Example>((a) => expected);
            var actual = (cmp as IEqualityComparer<Example>).Equals(new Example());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(EqualityFactory))]
        public void EqualityFactory_GetEquitable_IEquatable_Equals()
        {
            var expected = true;
            var cmp = EqualityFactory.GetEquitable<Example>((a) => expected);
            var actual = (cmp as IEquatable<Example>).Equals(new Example());

            Assert.IsTrue(actual);
        }
    }
}
