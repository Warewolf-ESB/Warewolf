/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Coverage for <see cref="VersionInfo"/>'s three constructors + ToString.
    /// </summary>
    [TestClass]
    [TestCategory("Runtime Hosting")]
    [ExcludeFromCodeCoverage]
    public class VersionInfoTests
    {
        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(VersionInfo))]
        public void VersionInfo_DefaultCtor_LeavesAllPropertiesAtDefaults()
        {
            var sut = new VersionInfo();

            Assert.AreEqual(default(DateTime), sut.DateTimeStamp);
            Assert.IsNull(sut.Reason);
            Assert.IsNull(sut.User);
            Assert.IsNull(sut.VersionNumber);
            Assert.AreEqual(Guid.Empty, sut.ResourceId);
            Assert.AreEqual(Guid.Empty, sut.VersionId);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(VersionInfo))]
        public void VersionInfo_FullCtor_AssignsAllFields()
        {
            var ts = new DateTime(2026, 3, 14, 9, 26, 53, DateTimeKind.Utc);
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();

            var sut = new VersionInfo(ts, "Manual save", "alice", "7", resourceId, versionId);

            Assert.AreEqual(ts, sut.DateTimeStamp);
            Assert.AreEqual("Manual save", sut.Reason);
            Assert.AreEqual("alice", sut.User);
            Assert.AreEqual("7", sut.VersionNumber);
            Assert.AreEqual(resourceId, sut.ResourceId);
            Assert.AreEqual(versionId, sut.VersionId);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(VersionInfo))]
        public void VersionInfo_XmlCtor_EmptyXml_PopulatesDefaultsAndUsesProvidedResourceId()
        {
            var resourceId = Guid.NewGuid();
            var beforeUtc = DateTime.Now.AddSeconds(-2);

            var sut = new VersionInfo(string.Empty, resourceId);

            Assert.AreEqual("Save", sut.Reason);
            Assert.AreEqual("Unknown", sut.User);
            Assert.AreEqual("1", sut.VersionNumber);
            Assert.AreEqual(resourceId, sut.ResourceId);
            Assert.AreNotEqual(Guid.Empty, sut.VersionId);
            Assert.IsTrue(sut.DateTimeStamp >= beforeUtc,
                "DateTimeStamp should be set to ~now on the empty-xml branch.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(VersionInfo))]
        public void VersionInfo_XmlCtor_NullXml_TreatedAsEmpty()
        {
            // string.IsNullOrEmpty(null) -> true, so the same defaults-branch fires.
            var resourceId = Guid.NewGuid();

            var sut = new VersionInfo(null, resourceId);

            Assert.AreEqual("Save", sut.Reason);
            Assert.AreEqual("Unknown", sut.User);
            Assert.AreEqual("1", sut.VersionNumber);
            Assert.AreEqual(resourceId, sut.ResourceId);
            Assert.AreNotEqual(Guid.Empty, sut.VersionId);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(VersionInfo))]
        public void VersionInfo_XmlCtor_ValidXml_ParsesAllAttributes()
        {
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var ts = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var xml = $@"<VersionInfo DateTimeStamp=""{ts:O}"" Reason=""Edit"" User=""bob"" VersionNumber=""42"" ResourceId=""{resourceId}"" VersionId=""{versionId}"" />";

            // The provided-resourceId argument is ignored on this branch; the
            // XML's ResourceId attribute takes precedence.
            var sut = new VersionInfo(xml, Guid.NewGuid());

            Assert.AreEqual(ts, sut.DateTimeStamp.ToUniversalTime());
            Assert.AreEqual("Edit", sut.Reason);
            Assert.AreEqual("bob", sut.User);
            Assert.AreEqual("42", sut.VersionNumber);
            Assert.AreEqual(resourceId, sut.ResourceId);
            Assert.AreEqual(versionId, sut.VersionId);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(VersionInfo))]
        public void VersionInfo_ToString_IncludesResourceAndVersionIds()
        {
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var sut = new VersionInfo(DateTime.UtcNow, "r", "u", "1", resourceId, versionId);

            var str = sut.ToString();

            StringAssert.Contains(str, resourceId.ToString());
            StringAssert.Contains(str, versionId.ToString());
            StringAssert.Contains(str, "ResourceId:");
            StringAssert.Contains(str, "Version:");
        }
    }
}
