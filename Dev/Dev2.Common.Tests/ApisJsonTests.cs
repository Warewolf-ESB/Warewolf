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

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Dev2.Common.Tests
{
    [TestClass]
    public class ApisJsonTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ApisJson))]
        public void ApisJson_Construct()
        {
            var p = GetDefaultInstance();

            Assert.AreEqual(null, p.Apis);
            Assert.AreEqual(0, p.Created.Ticks);
            Assert.AreEqual(null, p.Description);
            Assert.AreEqual(null, p.Image);
            Assert.AreEqual(null, p.Include);
            Assert.AreEqual(null, p.Maintainers);
            Assert.AreEqual(0, p.Modified.Ticks);
            Assert.AreEqual(null, p.Name);
            Assert.AreEqual(null, p.SpecificationVersion);
            Assert.AreEqual(null, p.Tags);
            Assert.AreEqual(null, p.Url);
            Assert.AreEqual(0, p.GetHashCode());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ApisJson))]
        public void ApisJson_ConstructWithValues()
        {
            var singleApis = new List<SingleApi>();
            var includeApis = new List<IncludeApi>();
            var maintainerApis = new List<MaintainerApi>();
            var p = GetInstance(singleApis, includeApis, maintainerApis);

            Assert.AreEqual(singleApis, p.Apis);
            Assert.AreEqual(10000000, p.Created.Ticks);
            Assert.AreEqual("Some description", p.Description);
            Assert.AreEqual("Some image", p.Image);
            Assert.AreEqual(includeApis, p.Include);
            Assert.AreEqual(maintainerApis, p.Maintainers);
            Assert.AreEqual(20000000, p.Modified.Ticks);
            Assert.AreEqual("Some Name", p.Name);
            Assert.AreEqual("spec version", p.SpecificationVersion);
            Assert.AreEqual("s1", p.Tags[0]);
            Assert.AreEqual("s2", p.Tags[1]);
            Assert.AreEqual("some url", p.Url);

            Assert.AreNotEqual(0, p.GetHashCode());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ApisJson))]
        public void ApisJson_Equals_IsFalse()
        {
            var id = new SingleApi();

            var ob1 = GetDefaultInstance();
            var ob2 = GetInstance(new List<SingleApi>(), new List<IncludeApi>(), new List<MaintainerApi>());

            Assert.IsFalse(ob1.Equals(ob2));
            Assert.IsFalse(ob1.Equals((object)ob2));
            Assert.IsFalse(ob1 == ob2);
            Assert.IsTrue(ob1 != ob2);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ApisJson))]
        public void ApisJson_Equals_IsTrue()
        {
            var id = new SingleApi();

            var ob1 = GetInstance(new List<SingleApi>(), new List<IncludeApi>(), new List<MaintainerApi>());
            var ob2 = GetInstance(new List<SingleApi>(), new List<IncludeApi>(), new List<MaintainerApi>());

            Assert.IsTrue(ob1.Equals(ob2));
            Assert.IsTrue(ob1.Equals((object)ob2));
            Assert.IsTrue(ob1 == ob2);
            Assert.IsFalse(ob1 != ob2);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ApisJson))]
        public void ApisJson_SameEquals_IsTrue()
        {
            var id = new SingleApi();

            var ob1 = GetInstance(new List<SingleApi>(), new List<IncludeApi>(), new List<MaintainerApi>());

            Assert.IsTrue(ob1.Equals(ob1));
            Assert.IsTrue(ob1.Equals((object)ob1));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ApisJson))]
        public void ApisJson_Equals_Null_IsFalse()
        {
            var id = new SingleApi();

            var ob1 = GetInstance(new List<SingleApi>(), new List<IncludeApi>(), new List<MaintainerApi>());

            Assert.IsFalse(ob1.Equals(null));
            Assert.IsFalse(ob1.Equals((object)null));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ApisJson))]
        public void ApisJson_HashCode()
        {
            var id = new SingleApi();

            var ob1 = GetInstance(new List<SingleApi>(), new List<IncludeApi>(), new List<MaintainerApi>());

            Assert.AreNotEqual(0, ob1.GetHashCode());
        }

       [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ApisJson))]
        public void ApisJson_Equals_DifferentType_IsFalse()
        {
            var ob1 = GetInstance(new List<SingleApi>(), new List<IncludeApi>(), new List<MaintainerApi>());

            Assert.IsFalse(ob1.Equals((object)1234));
        }

        private static ApisJson GetDefaultInstance() => new ApisJson();
        private static ApisJson GetInstance(List<SingleApi> singleApis, List<IncludeApi> includeApis, List<MaintainerApi> maintainerApis)
        {
            var p = new ApisJson
            {
                Apis = singleApis,
                Created = new DateTime(10000000),
                Description = "Some description",
                Image = "Some image",
                Include = includeApis,
                Maintainers = maintainerApis,
                Modified = new DateTime(20000000),
                Name = "Some Name",
                SpecificationVersion = "spec version",
                Tags = new List<string> { "s1", "s2" },
                Url = "some url",

            };
            return p;
        }
    }
}
