using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WarewolfParsingTest
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Person
    {

        public string Name { get; set; }
        public IList<Person> Children { get; set; }
        public Person Spouse { get; set; }

    }
}