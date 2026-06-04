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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class DeletedFileMetadataTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DeletedFileMetadata))]
        public void DeletedFileMetadata_Properties_GetSet_RoundTrip()
        {
            var resourceId = Guid.NewGuid();
            var metadata = new DeletedFileMetadata
            {
                IsDeleted = true,
                ResourceId = resourceId,
                ShowDependencies = true,
                ApplyToAll = true,
                DeleteAnyway = true
            };

            Assert.IsTrue(metadata.IsDeleted);
            Assert.AreEqual(resourceId, metadata.ResourceId);
            Assert.IsTrue(metadata.ShowDependencies);
            Assert.IsTrue(metadata.ApplyToAll);
            Assert.IsTrue(metadata.DeleteAnyway);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DeletedFileMetadata))]
        public void DeletedFileMetadata_Defaults_AreFalseAndEmpty()
        {
            var metadata = new DeletedFileMetadata();

            Assert.IsFalse(metadata.IsDeleted);
            Assert.AreEqual(Guid.Empty, metadata.ResourceId);
            Assert.IsFalse(metadata.ShowDependencies);
            Assert.IsFalse(metadata.ApplyToAll);
            Assert.IsFalse(metadata.DeleteAnyway);
        }
    }
}
