/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*/
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Dev2.Collections;
using Dev2.Communication;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Text;

namespace Dev2.Infrastructure.Tests.Coverage
{
    [TestClass]
    public class ObservableReadOnlyListCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "ObservableReadOnlyList";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_Default_EmptyList()
        {
            var l = new ObservableReadOnlyList<int>();
            Assert.AreEqual(0, l.Count);
            Assert.IsTrue(l.IsReadOnly);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_List_PopulatesAll()
        {
            var l = new ObservableReadOnlyList<int>(new List<int> { 1, 2, 3 });
            Assert.AreEqual(3, l.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, l.ToArray());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_NullEnumerable_DoesNotThrow()
        {
            var l = new ObservableReadOnlyList<string>((IEnumerable<string>)null);
            Assert.AreEqual(0, l.Count);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Add_Remove_Clear_WorkAndIndexerWorks()
        {
            var l = new ObservableReadOnlyList<int>();
            l.Add(1); l.Add(2); l.Add(3);
            Assert.AreEqual(3, l.Count);
            Assert.IsTrue(l.Contains(2));
            Assert.AreEqual(1, l.IndexOf(2));
            l.Insert(0, 0);
            Assert.AreEqual(0, l[0]);
            l[0] = 9;
            Assert.AreEqual(9, l[0]);
            l.RemoveAt(0);
            Assert.AreEqual(3, l.Count);
            Assert.IsTrue(l.Remove(2));
            Assert.IsFalse(l.Remove(99));
            var arr = new int[2];
            l.CopyTo(arr, 0);
            Assert.AreEqual(1, arr[0]);
            l.Clear();
            Assert.AreEqual(0, l.Count);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void CollectionChanged_Fires_OnAdd()
        {
            var l = new ObservableReadOnlyList<int>();
            NotifyCollectionChangedEventArgs gotArgs = null;
            l.CollectionChanged += (s, e) => gotArgs = e;
            l.Add(42);
            Assert.IsNotNull(gotArgs);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, gotArgs.Action);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Enumerator_IteratesItems()
        {
            var l = new ObservableReadOnlyList<int>(new List<int> { 5, 6, 7 });
            var sum = 0;
            foreach (var v in l) sum += v;
            Assert.AreEqual(18, sum);
        }
    }

    [TestClass]
    public class HasNoDuplicateEntriesRuleTests
    {
        const string Owner = "Coverage";
        const string Cat = "HasNoDuplicateEntries";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NoDuplicates_ReturnsNull()
        {
            var rule = new HasNoDuplicateEntriesRule(() => "a,b,c");
            Assert.IsNull(rule.Check());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Duplicates_ReturnsError()
        {
            var rule = new HasNoDuplicateEntriesRule(() => "a,b,a");
            var err = rule.Check();
            Assert.IsNotNull(err);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void RecordsetIndexes_StrippedBeforeCompare()
        {
            var rule = new HasNoDuplicateEntriesRule(() => "rs(1).f,rs(2).f");
            var err = rule.Check();
            Assert.IsNotNull(err);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ReplaceRecordsetIndexWithBlank_StripsDigits()
        {
            Assert.AreEqual("rs().f", HasNoDuplicateEntriesRule.ReplaceRecordsetIndexWithBlank("rs(1).f"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ExtractIndexRegion_NoParens_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, HasNoDuplicateEntriesRule.ExtractIndexRegionFromRecordset("rs.f"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ExtractIndexRegion_MissingClose_UsesEndOfString()
        {
            Assert.AreEqual("123", HasNoDuplicateEntriesRule.ExtractIndexRegionFromRecordset("rs(123"));
        }
    }

    [TestClass]
    public class HasNoIndexsInRecordsetsRuleTests
    {
        const string Owner = "Coverage";
        const string Cat = "HasNoIndexsInRecordsets";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NoIndex_ReturnsNull()
        {
            var rule = new HasNoIndexsInRecordsetsRule(() => "rs().f,other");
            Assert.IsNull(rule.Check());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void StarIndex_AllowedReturnsNull()
        {
            var rule = new HasNoIndexsInRecordsetsRule(() => "rs(*).f");
            Assert.IsNull(rule.Check());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NumericIndex_ReturnsError()
        {
            var rule = new HasNoIndexsInRecordsetsRule(() => "rs(3).f");
            Assert.IsNotNull(rule.Check());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ExtractIndexRegion_Static_ReturnsInner()
        {
            Assert.AreEqual("42", HasNoIndexsInRecordsetsRule.ExtractIndexRegionFromRecordset("rs(42).f"));
            Assert.AreEqual(string.Empty, HasNoIndexsInRecordsetsRule.ExtractIndexRegionFromRecordset("plain"));
        }
    }

    public class TypeHolder
    {
        public Type T { get; set; }
    }

    [TestClass]
    public class Dev2JsonSerializerCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "Dev2JsonSerializer";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Serialize_TypeProperty_WritesFullName()
        {
            var s = new Dev2JsonSerializer();
            var holder = new TypeHolder { T = typeof(int) };
            var json = s.Serialize(holder);
            Assert.IsTrue(json.Contains("System.Int32"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Serialize_NullTypeProperty_WritesNull()
        {
            var s = new Dev2JsonSerializer();
            var holder = new TypeHolder { T = null };
            var json = s.Serialize(holder);
            Assert.IsTrue(json.Contains("null"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void RoundTrip_TypeProperty_ResolvesBackToInt()
        {
            var s = new Dev2JsonSerializer();
            var original = new TypeHolder { T = typeof(int) };
            var json = s.Serialize(original);
            var copy = s.Deserialize<TypeHolder>(json);
            Assert.IsNotNull(copy);
            Assert.AreEqual(typeof(int), copy.T);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Deserialize_PrimitiveAliases_Resolve()
        {
            // Manually craft JSON for various aliases; we want to hit the switch arms.
            string Make(string val) => "{\"T\":\"" + val + "\"}";
            var s = new Dev2JsonSerializer();
            foreach (var pair in new (string raw, Type expected)[]
            {
                ("string",   typeof(string)),
                ("bool",     typeof(bool)),
                ("boolean",  typeof(bool)),
                ("byte",     typeof(byte)),
                ("short",    typeof(short)),
                ("Int16",    typeof(short)),
                ("int",      typeof(int)),
                ("Int32",    typeof(int)),
                ("long",     typeof(long)),
                ("Int64",    typeof(long)),
                ("float",    typeof(float)),
                ("Single",   typeof(float)),
                ("double",   typeof(double)),
                ("Double",   typeof(double)),
                ("decimal",  typeof(decimal)),
                ("Guid",     typeof(Guid)),
                ("System.Guid", typeof(Guid)),
                ("DateTime", typeof(DateTime)),
                ("TimeSpan", typeof(TimeSpan)),
            })
            {
                var holder = s.Deserialize<TypeHolder>(Make(pair.raw));
                Assert.AreEqual(pair.expected, holder.T, $"alias '{pair.raw}' should map to {pair.expected}");
            }
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Deserialize_UnknownTypeName_ReturnsObject()
        {
            var s = new Dev2JsonSerializer();
            var holder = s.Deserialize<TypeHolder>("{\"T\":\"NotAType\"}");
            Assert.AreEqual(typeof(object), holder.T);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Deserialize_AssemblyQualified_RoundTripsThroughBinder()
        {
            var s = new Dev2JsonSerializer();
            var holder = s.Deserialize<TypeHolder>("{\"T\":\"System.Int32, mscorlib\"}");
            Assert.AreEqual(typeof(int), holder.T);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Deserialize_Empty_ReturnsNull()
        {
            var s = new Dev2JsonSerializer();
            var holder = s.Deserialize<TypeHolder>("{\"T\":null}");
            Assert.IsNull(holder.T);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Serialize_RoundTrip_StringBuilder()
        {
            var s = new Dev2JsonSerializer();
            var sb = s.SerializeToBuilder(new TypeHolder { T = typeof(string) });
            Assert.IsTrue(sb.ToString().Contains("System.String"));
        }
    }
}
