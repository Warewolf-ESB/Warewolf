using System;

namespace Technical_Assesment.Sorting
{
    public class PersonAgeSort : ISortable<Person>
    {
        public int CompareTo(Person left, Person right)
        {
            int lname = String.Compare(left.LName, right.LName, StringComparison.Ordinal);
            int fname = String.Compare(left.FName, right.FName, StringComparison.Ordinal);
            int age = left.Age - right.Age;


            if (lname == 0)
            {
                if (fname == 0)
                {
                    return age;
                }

                return fname;
            }

            return lname;
        }
    }
}
