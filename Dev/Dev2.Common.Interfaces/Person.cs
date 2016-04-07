using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public class Person
    {

        public string Name { get; set; }
        public IList<Person> Children { get; set; }
        public Person Spouse { get; set; }

    }
}