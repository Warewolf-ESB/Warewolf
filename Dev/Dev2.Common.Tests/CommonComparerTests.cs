using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class CommonComparerTests
    {
        [TestMethod]
        public void GetEqualityComparer_GivenEquals_GetHashCode()
        {
            IEqualityComparer<Person> personComparer = EqualityFactory.GetEqualityComparer<Person>(
                                                                                    (p1, p2) => p1.FirstName == p2.FirstName && p1.LastName == p2.LastName,
                                                                                    p => string.Concat(p.FirstName, p.LastName).ToLowerInvariant().GetHashCode());
            var characters = new HashSet<Person>(personComparer)
            {
                new Person {FirstName = "Bette", LastName = "Johnson"},
                new Person {FirstName = "Elmer", LastName = "Pickle"},
                new Person {FirstName = "Fred", LastName = "Smith"},
                new Person {FirstName = "Barney", LastName = "Rhinestone"},
                new Person {FirstName = "Wilma", LastName = "Green"},
                new Person {FirstName = "Billy", LastName = "Thompson"},
                new Person {FirstName = "Daisy", LastName = "Anderson"}
            };

            //Add returns true if an element is added, false if it's already there.
            bool addSamePersonAgain = characters.Add(new Person { FirstName = "Elmer", LastName = "Pickle" });
            Assert.IsFalse(addSamePersonAgain);

        }

        [TestMethod]
        public void GetComparer_StringComparer()
        {
            List<string> characterNames = new List<string>
            {
                "Bette",
                "Elmer",
                "Fred",
                "Barney",
                "Wilma",
                "Billy",
                "Daisy"
            };


            characterNames.Sort(EqualityFactory.GetComparer<string>((v1, v2) => string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase)));

            Assert.AreEqual("Barney", characterNames[0]);
            Assert.AreEqual("Bette", characterNames[1]);
            Assert.AreEqual("Billy", characterNames[2]);
            Assert.AreEqual("Daisy", characterNames[3]);
            Assert.AreEqual("Elmer", characterNames[4]);
            Assert.AreEqual("Fred", characterNames[5]);
            Assert.AreEqual("Wilma", characterNames[6]);

        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetComparer_isEquality()
        {
            var comparer = EqualityFactory.GetComparer<string>((s1, s2) => String.Compare(s1, s2, StringComparison.Ordinal));
            IEqualityComparer<string> otherComparer = comparer as IEqualityComparer<string>;
            Assert.IsNotNull(otherComparer);
            int result = otherComparer.GetHashCode("Test");
            Assert.AreEqual("Test".GetHashCode(), result);

        }


    }
}
