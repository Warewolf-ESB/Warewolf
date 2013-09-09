using System.Collections.Generic;

namespace Dev2.Tests.Runtime.Poco
{
    // BUG 9626 - 2013.06.11 - TWR: added
    public static class PocoTestFactory
    {
        #region CreateCompany

        public static Company CreateCompany()
        {
            return new Company
            {
                Name = "Dev2",
                Departments = new List<Department>(new[]
                {
                    new Department
                    {
                        Name = "Dev",
                        Employees = new List<Person>(new[]
                        {
                            new Person { Name = "Brendon" },
                            new Person { Name = "Jayd" }
                        })
                    },
                    new Department
                    {
                        Name = "Accounts",
                        Employees = new List<Person>(new[]
                        {
                            new Person { Name = "Bob" },
                            new Person { Name = "Jo" }
                        })
                    }
                })
            };
        }

        #endregion

    }

    #region Classes

    public class Person
    {
        public string Name { get; set; }
    }

    public class Department
    {
        public string Name { get; set; }
        public IList<Person> Employees { get; set; }
    }

    public class Company
    {
        public string Name { get; set; }
        public IList<Department> Departments { get; set; }
    }

    #endregion
}
