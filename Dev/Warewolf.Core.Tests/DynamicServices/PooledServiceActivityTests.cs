/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.DynamicServices.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class PooledServiceActivityTests
    {
        // The internal ctor is reachable from Warewolf.Core.Tests via InternalsVisibleTo;
        // Activity is reference-typed and never null-checked, so null is fine for Value.
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(PooledServiceActivity))]
        public void PooledServiceActivity_Constructor_ExposesGenerationAndValue()
        {
            var pooled = new PooledServiceActivity(7, null);

            Assert.AreEqual(7, pooled.Generation);
            Assert.IsNull(pooled.Value);
        }
    }
}
