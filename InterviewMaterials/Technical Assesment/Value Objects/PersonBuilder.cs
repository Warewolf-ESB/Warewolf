using System;

namespace Technical_Assesment.Value_Objects
{
    public class PersonBuilder : ImportBuilder<Person>
    {
        public Person FromImportTokens(string[] parts)
        {
            int age;
            Int32.TryParse(parts[2], out age);
            return new Person(parts[1], parts[0], age);
        }

        public int TokenCnt()
        {
            return 3;
        }
    }
}
