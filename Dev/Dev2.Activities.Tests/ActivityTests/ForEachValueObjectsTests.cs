/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects;

namespace Dev2.Activities.Tests.ActivityTests
{
    /// <summary>
    /// Track-C T1 batch: the two small value-objects used by the ForEach activity —
    /// <see cref="DsfForEachItem"/> POCO and <see cref="ForEachInnerActivityTO"/> whose
    /// ctor branches on whether the inner activity (and each of its mapping strings) is null/empty.
    /// </summary>
    [TestClass]
    public class ForEachValueObjectsTests
    {
        const string Owner = "Ashley Lewis";

        // ---- DsfForEachItem ----------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(DsfForEachItem))]
        public void DsfForEachItem_Properties_Roundtrip()
        {
            var item = new DsfForEachItem
            {
                Name = "n",
                Value = "v",
                RowIndex = 7,
                GroupID = 42,
            };
            Assert.AreEqual("n", item.Name);
            Assert.AreEqual("v", item.Value);
            Assert.AreEqual(7, item.RowIndex);
            Assert.AreEqual(42, item.GroupID);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(DsfForEachItem))]
        public void DsfForEachItem_EmptyList_Get_DefaultsToEmptyArray_AndIsSettable()
        {
            // Static state — capture and restore so test order is irrelevant.
            var original = DsfForEachItem.EmptyList;
            try
            {
                DsfForEachItem.EmptyList = original; // hit the setter explicitly with a known value
                Assert.IsNotNull(DsfForEachItem.EmptyList);

                var replacement = new[] { new DsfForEachItem { Name = "x" } };
                DsfForEachItem.EmptyList = replacement;
                Assert.AreSame(replacement, DsfForEachItem.EmptyList);
            }
            finally
            {
                DsfForEachItem.EmptyList = original;
            }
        }

        // ---- ForEachInnerActivityTO --------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(ForEachInnerActivityTO))]
        public void ForEachInnerActivityTO_NullActivity_NoMappingsStored()
        {
            var to = new ForEachInnerActivityTO(null);
            Assert.IsNull(to.InnerActivity);
            Assert.IsNull(to.OrigInnerInputMapping);
            Assert.IsNull(to.OrigInnerOutputMapping);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(ForEachInnerActivityTO))]
        public void ForEachInnerActivityTO_NonEmptyMappings_AreStored()
        {
            var act = new Mock<IDev2ActivityIOMapping>();
            act.SetupGet(a => a.InputMapping).Returns("[[in]]");
            act.SetupGet(a => a.OutputMapping).Returns("[[out]]");

            var to = new ForEachInnerActivityTO(act.Object);

            Assert.AreSame(act.Object, to.InnerActivity);
            Assert.AreEqual("[[in]]", to.OrigInnerInputMapping);
            Assert.AreEqual("[[out]]", to.OrigInnerOutputMapping);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(ForEachInnerActivityTO))]
        public void ForEachInnerActivityTO_EmptyMappings_StoredAsNull()
        {
            // Both empty strings should hit the `null` ternary branch.
            var act = new Mock<IDev2ActivityIOMapping>();
            act.SetupGet(a => a.InputMapping).Returns(string.Empty);
            act.SetupGet(a => a.OutputMapping).Returns(string.Empty);

            var to = new ForEachInnerActivityTO(act.Object);

            Assert.IsNull(to.OrigInnerInputMapping);
            Assert.IsNull(to.OrigInnerOutputMapping);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(ForEachInnerActivityTO))]
        public void ForEachInnerActivityTO_OtherCollectionProperties_AreSettable()
        {
            // These four IList<Tuple<,>> properties are auto-property get/set — round-trip them
            // so the setters aren't dead-code.
            var to = new ForEachInnerActivityTO(null);
            var inputs = new System.Collections.Generic.List<System.Tuple<string, string>>();
            var outputs = new System.Collections.Generic.List<System.Tuple<string, string>>();

            to.OrigCodedInputs = inputs;
            to.OrigCodedOutputs = outputs;
            to.CurCodedInputs = inputs;
            to.CurCodedOutputs = outputs;

            Assert.AreSame(inputs, to.OrigCodedInputs);
            Assert.AreSame(outputs, to.OrigCodedOutputs);
            Assert.AreSame(inputs, to.CurCodedInputs);
            Assert.AreSame(outputs, to.CurCodedOutputs);
        }
    }
}
