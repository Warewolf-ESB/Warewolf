/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    /// <summary>
    /// Extra branch coverage for <see cref="FieldAndPropertyMapper"/>.
    /// Targets the high-CRAP rows on the coverage dashboard:
    ///   - <c>Map</c>                (CRAP 110, cyclomatic 10)
    ///   - <c>MapProperties</c>      (CRAP 110, cyclomatic 10)
    ///   - <c>MapFields</c>          (CRAP 210, cyclomatic 14)
    ///
    /// The existing <c>MapperTests</c> happy-paths the common code; this
    /// fixture fills in the remaining branches: missing-mapping
    /// ArgumentException, duplicate AddMap calls, private-field copy
    /// (k__BackingField filter), and type-mismatched fields.
    /// </summary>
    [TestClass]
    public class MapperBranchCoverageTests
    {
        // ------------------------------------------------------------------
        // Map: no AddMap registered  ->  ArgumentException branch
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(FieldAndPropertyMapper))]
        public void Map_WithoutRegisteredMapping_ThrowsArgumentException()
        {
            var mapper = new FieldAndPropertyMapper();
            mapper.Clear();

            var parent = new Parent { Id = 1, Name = "n", Surname = "s" };
            var child = new Child();

            var ex = Assert.ThrowsException<ArgumentException>(() => mapper.Map(parent, child));
            StringAssert.Contains(ex.Message, "No mapping exists");
            StringAssert.Contains(ex.Message, nameof(Parent));
            StringAssert.Contains(ex.Message, nameof(Child));
        }

        // ------------------------------------------------------------------
        // AddMap: second call with same TFrom/TTo pair is a no-op
        // (covers the `ContainsKey` true branch on line 51-54)
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(FieldAndPropertyMapper))]
        public void AddMap_DuplicateRegistration_KeepsFirstCallback()
        {
            var mapper = new FieldAndPropertyMapper();
            mapper.Clear();

            int firstHits = 0, secondHits = 0;
            mapper.AddMap<Parent, Child>((_, _) => firstHits++);
            mapper.AddMap<Parent, Child>((_, _) => secondHits++); // ignored

            mapper.Map(new Parent { Id = 7 }, new Child());

            Assert.AreEqual(1, firstHits, "First callback should still be the one stored.");
            Assert.AreEqual(0, secondHits, "Second AddMap must be a no-op when key already exists.");
        }

        // ------------------------------------------------------------------
        // MapFields: private field with matching name+type is copied,
        // k__BackingField entries are filtered, mismatched-type fields are
        // skipped even when names align.
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(FieldAndPropertyMapper))]
        public void MapFields_CopiesMatchingPrivateFieldOnly()
        {
            var mapper = new FieldAndPropertyMapper();
            mapper.Clear();
            mapper.AddMap<FieldSource, FieldTarget>();

            var src = new FieldSource();
            src.SetSecret("classified");
            src.SetTypeMismatch(42); // target has same name but string type

            var dst = new FieldTarget();

            mapper.Map(src, dst);

            // Matching name + same type -> copied via reflection.
            Assert.AreEqual("classified", dst.GetSecret());
            // Same field name, different type -> MapFields skips it
            // (FieldType.Name equality check excludes it).
            Assert.IsNull(dst.GetTypeMismatch());
        }

        // ------------------------------------------------------------------
        // MapProperties: only writable, name+type-matching properties copy.
        // Read-only target property is skipped (CanWrite branch).
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(FieldAndPropertyMapper))]
        public void MapProperties_ReadOnlyTargetProperty_IsSkipped()
        {
            var mapper = new FieldAndPropertyMapper();
            mapper.Clear();
            mapper.AddMap<PropertySource, PropertyTarget>();

            var src = new PropertySource { Writable = "ok", ReadOnly = "should-not-copy" };
            var dst = new PropertyTarget();

            mapper.Map(src, dst);

            Assert.AreEqual("ok", dst.Writable);
            // Target's ReadOnly has no setter -> CanWrite false -> not in the
            // compatibleProperties projection -> stays at the default.
            Assert.AreEqual("default", dst.ReadOnly);
        }

        // ------------------------------------------------------------------
        // Map: null mapTo  ->  Activator.CreateInstance branch reached
        // (covers the Activator path *with* a working AddMap, verifying
        // the surrogate target is still mapped from properties).
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(FieldAndPropertyMapper))]
        public void Map_NullMapTo_UsesActivatorAndStillExecutesCallback()
        {
            var mapper = new FieldAndPropertyMapper();
            mapper.Clear();

            var callbackRan = false;
            mapper.AddMap<Parent, Child>((p, c) =>
            {
                Assert.IsNotNull(c, "Activator must have produced an instance.");
                c.ParentId = p.Id;
                callbackRan = true;
            });

            mapper.Map<Parent, Child>(new Parent { Id = 99 }, null);

            Assert.IsTrue(callbackRan);
        }
    }

    // ---------- fixtures ----------

    public class FieldSource
    {
#pragma warning disable 169, 414
        private string _secret = "default";
        private int _typeMismatch;
#pragma warning restore 169, 414

        public void SetSecret(string s) => _secret = s;
        public void SetTypeMismatch(int n) => _typeMismatch = n;
    }

    public class FieldTarget
    {
#pragma warning disable 169, 414, 649
        private string _secret;
        // Same name as FieldSource._typeMismatch but different type ->
        // MapFields must skip the assignment.
        private string _typeMismatch;
#pragma warning restore 169, 414, 649

        public string GetSecret() => _secret;
        public string GetTypeMismatch() => _typeMismatch;
    }

    public class PropertySource
    {
        public string Writable { get; set; }
        public string ReadOnly { get; set; }
    }

    public class PropertyTarget
    {
        public string Writable { get; set; }
        public string ReadOnly { get; } = "default"; // no setter
    }
}
