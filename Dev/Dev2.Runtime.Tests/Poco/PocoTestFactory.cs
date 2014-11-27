
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
