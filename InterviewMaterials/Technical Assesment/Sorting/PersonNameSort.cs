using System;

namespace Technical_Assesment.Sorting
{
    public class PersonNameSort : ISortable<Person>
    {
        public int CompareTo(Person left, Person right)
        {
            int lname = String.Compare(left.LName, right.LName, StringComparison.Ordinal);
            int fname = String.Compare(left.FName, right.FName, StringComparison.Ordinal);
            
            if (lname == 0)
            {
                return fname;
            }

            return lname;
        }
    }
}
