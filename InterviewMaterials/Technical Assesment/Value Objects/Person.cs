using System;

namespace Technical_Assesment
{

    public static class PersonFactory
    {
        
        public static Person CreatePerson(string fname, string lname, int age )
        {
            return new Person(fname, lname, age);
        }
    }

    public class Person
    {

        public string FName { get; private set; }
        public string LName { get; private set; }
        public int Age { get; private set; }

        internal Person(string fname, string lname, int yearsOnEarth)
        {
            FName = fname;
            LName = lname;
            Age = yearsOnEarth;
        }

        public override string ToString()
        {
            return string.Concat("{ ", LName, ", ", FName, " is ", Age, " }");
        }

        //public int CompareTo(Person with)
        //{

        //    int result = 0;

        //    if (with != null)
        //    {
        //        result = string.Compare(FName, with.FName, StringComparison.Ordinal);

        //        result += String.Compare(LName, with.LName, StringComparison.Ordinal);

        //        result += Age - with.Age;
        //    }
        //    else
        //    {
        //        result = Int32.MinValue;
        //    }

        //    return result;
        //}
    }
}
