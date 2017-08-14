using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WarewolfParsingTest
{

    public class Person
    {

        public string Name { get; set; }
        public IList<Person> Children { get; set; }
        public Person Spouse { get; set; }

    }
}