/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Json;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.JsonTest
{
    [TestClass]
    public class JsonNavigatorTests
    {
        internal string testData => @"{
    ""Name"": ""Dev2"",
    ""Motto"": ""Eat lots of cake"",
    ""Departments"": [      
        {
          ""Name"": ""Dev"",
          ""Employees"": [
              {
                ""Name"": ""Brendon"",
                ""Surename"": ""Page""
              },
              {
                ""Name"": ""Jayd"",
                ""Surename"": ""Page""
              }
            ]
        },
        {
          ""Name"": ""Accounts"",
          ""Employees"": [
              {
                ""Name"": ""Bob"",
                ""Surename"": ""Soap""
              },
              {
                ""Name"": ""Joe"",
                ""Surename"": ""Pants""
              }
            ]
        }
      ],
    ""Contractors"": [      
        {
          ""Name"": ""Roofs Inc."",
          ""PhoneNumber"": ""123"",
        },
        {
          ""Name"": ""Glass Inc."",
          ""PhoneNumber"": ""1234"",
        },
        {
          ""Name"": ""Doors Inc."",
          ""PhoneNumber"": ""1235"",
        },
        {
          ""Name"": ""Cakes Inc."",
          ""PhoneNumber"": ""1236"",
        }
      ],
    ""PrimitiveRecordset"": [
      ""
        RandomData
    "",
      ""
        RandomData1
    ""
    ],
  }";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SelectScalar_WithNull_ExpectArgumentNullException()
        {
            var JsonNavigator = new JsonNavigator(testData);
            JsonNavigator.SelectScalar(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void SelectScalar_WithoutJsonPath_ExpectException()
        {
            var JsonNavigator = new JsonNavigator(testData);
            JsonNavigator.SelectScalar(new XmlPath());
        }

        [TestMethod]
        public void SelectScalarValue_WithEnumerableSymbolAndSeperatorSymbol_Expected_PrimitiveRecordset()
        {
            IPath namePath = new JsonPath("().", "().");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = JsonNavigator.SelectScalar(namePath).ToString();
            const string expected = @"""PrimitiveRecordset"": [
  ""\r\n        RandomData\r\n    "",
  ""\r\n        RandomData1\r\n    ""
]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectScalarValueUsingSeperatorSymbol_Expected_UnchangedPath()
        {
            IPath namePath = new JsonPath(".", ".");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = JsonNavigator.SelectScalar(namePath).ToString();

            var expected = @"{
  ""Name"": ""Dev2"",
  ""Motto"": ""Eat lots of cake"",
  ""Departments"": [
    {
      ""Name"": ""Dev"",
      ""Employees"": [
        {
          ""Name"": ""Brendon"",
          ""Surename"": ""Page""
        },
        {
          ""Name"": ""Jayd"",
          ""Surename"": ""Page""
        }
      ]
    },
    {
      ""Name"": ""Accounts"",
      ""Employees"": [
        {
          ""Name"": ""Bob"",
          ""Surename"": ""Soap""
        },
        {
          ""Name"": ""Joe"",
          ""Surename"": ""Pants""
        }
      ]
    }
  ],
  ""Contractors"": [
    {
      ""Name"": ""Roofs Inc."",
      ""PhoneNumber"": ""123""
    },
    {
      ""Name"": ""Glass Inc."",
      ""PhoneNumber"": ""1234""
    },
    {
      ""Name"": ""Doors Inc."",
      ""PhoneNumber"": ""1235""
    },
    {
      ""Name"": ""Cakes Inc."",
      ""PhoneNumber"": ""1236""
    }
  ],
  ""PrimitiveRecordset"": [
    ""\r\n        RandomData\r\n    "",
    ""\r\n        RandomData1\r\n    ""
  ]
}";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectScalarValue_WithScalarPathFromJson_Expected_ScalarValue()
        {
            IPath namePath = new JsonPath("Name", "Name");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = JsonNavigator.SelectScalar(namePath).ToString();
            const string expected = "Dev2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectScalarValue_WithEnumerablePathFromJson_Expected_ScalarValue()
        {
            IPath namePath = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = JsonNavigator.SelectScalar(namePath).ToString();
            const string expected = "Joe";

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectScalarValue_WithEnumerablePathFromJson_WherePathMapsToPrimitiveEnumerable_Expected_ScalarValue()
        {
            IPath namePath = new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = JsonNavigator.SelectScalar(namePath).ToString().Trim();
            const string expected = "RandomData1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SelectEnumerable_WithNull_ExpectArgumentNullException()
        {
            var JsonNavigator = new JsonNavigator(testData);
            JsonNavigator.SelectEnumerable(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void SelectEnumerable_WithoutJsonPath_ExpectArgumentNullException()
        {
            var JsonNavigator = new JsonNavigator(testData);
            JsonNavigator.SelectEnumerable(new XmlPath());
        }

        [TestMethod]
        public void SelectEnumerable_WithEnumerableSymbolAndSeperatorSymbol_Expected_PipeDelimited()
        {
            IPath namePath = new JsonPath("().", "().");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(namePath).Select(o => o.ToString().Trim()));
            const string expected = @"""Name"": ""Dev2""|""Motto"": ""Eat lots of cake""|""Departments"": [
  {
    ""Name"": ""Dev"",
    ""Employees"": [
      {
        ""Name"": ""Brendon"",
        ""Surename"": ""Page""
      },
      {
        ""Name"": ""Jayd"",
        ""Surename"": ""Page""
      }
    ]
  },
  {
    ""Name"": ""Accounts"",
    ""Employees"": [
      {
        ""Name"": ""Bob"",
        ""Surename"": ""Soap""
      },
      {
        ""Name"": ""Joe"",
        ""Surename"": ""Pants""
      }
    ]
  }
]|""Contractors"": [
  {
    ""Name"": ""Roofs Inc."",
    ""PhoneNumber"": ""123""
  },
  {
    ""Name"": ""Glass Inc."",
    ""PhoneNumber"": ""1234""
  },
  {
    ""Name"": ""Doors Inc."",
    ""PhoneNumber"": ""1235""
  },
  {
    ""Name"": ""Cakes Inc."",
    ""PhoneNumber"": ""1236""
  }
]|""PrimitiveRecordset"": [
  ""\r\n        RandomData\r\n    "",
  ""\r\n        RandomData1\r\n    ""
]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectEnumerable_WithSeperatorSymbol_Expected_UnchangedPath()
        {
            IPath namePath = new JsonPath(".", ".");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(namePath).Select(o => o.ToString().Trim()));

            var expected = @"{
  ""Name"": ""Dev2"",
  ""Motto"": ""Eat lots of cake"",
  ""Departments"": [
    {
      ""Name"": ""Dev"",
      ""Employees"": [
        {
          ""Name"": ""Brendon"",
          ""Surename"": ""Page""
        },
        {
          ""Name"": ""Jayd"",
          ""Surename"": ""Page""
        }
      ]
    },
    {
      ""Name"": ""Accounts"",
      ""Employees"": [
        {
          ""Name"": ""Bob"",
          ""Surename"": ""Soap""
        },
        {
          ""Name"": ""Joe"",
          ""Surename"": ""Pants""
        }
      ]
    }
  ],
  ""Contractors"": [
    {
      ""Name"": ""Roofs Inc."",
      ""PhoneNumber"": ""123""
    },
    {
      ""Name"": ""Glass Inc."",
      ""PhoneNumber"": ""1234""
    },
    {
      ""Name"": ""Doors Inc."",
      ""PhoneNumber"": ""1235""
    },
    {
      ""Name"": ""Cakes Inc."",
      ""PhoneNumber"": ""1236""
    }
  ],
  ""PrimitiveRecordset"": [
    ""\r\n        RandomData\r\n    "",
    ""\r\n        RandomData1\r\n    ""
  ]
}";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromJson_WherePathMapsToPrimitiveEnumerable_Expected_EnumerableValue()
        {
            IPath path = new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            const string expected = "RandomData|RandomData1";

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromJson_Expected_EnumerableValue()
        {
            IPath path = new JsonPath("Departments().Name", "Departments.Name");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            const string expected = "Dev|Accounts";

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesUsingScalarPathFromJson_Expected_EnumerableValue()
        {
            IPath path = new JsonPath("Motto", "Motto");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            const string expected = "Eat lots of cake";

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromJson_WherePathMapsThroughNestedEnumerables_Expected_EnumerableValue()
        {
            IPath path = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            const string expected = "Brendon|Jayd|Bob|Joe";

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainAScalarPath_Expected_FlattenedDataWithValueFromScalarPathRepeatingForEachEnumeration()
        {
            IPath path = new JsonPath("Name", "Name");
            IPath path1 = new JsonPath("Departments().Name", "Departments.Name");
            var paths = new List<IPath> { path, path1 };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            const string expected = "Dev2|Dev2^Dev|Accounts";
            var actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainUnrelatedEnumerablePaths_Expected_FlattenedDataWithValuesFromUnrelatedEnumerablePathsAtMatchingIndexes()
        {
            IPath path = new JsonPath("Departments().Name", "Departments.Name");
            IPath path1 = new JsonPath("Contractors().Name", "Contractors.Name");
            var paths = new List<IPath> { path, path1 };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            const string expected = "Dev|Accounts||^Roofs Inc.|Glass Inc.|Doors Inc.|Cakes Inc.";
            var actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainNestedEnumerablePaths_Expected_FlattenedDataWithValuesFromOuterEnumerablePathRepeatingForEveryValueFromNestedEnumerablePath()
        {
            IPath path3 = new JsonPath("Name", "Name");
            IPath path = new JsonPath("Departments().Name", "Departments.Name");
            IPath path1 = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");
            IPath path2 = new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset");
            var paths = new List<IPath> { path3, path, path1, path2 };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            var expected = "Dev2|Dev2|Dev2|Dev2^Dev|Dev|Accounts|Accounts^Brendon|Jayd|Bob|Joe^RandomData\r\n    ,\r\n        RandomData1|||";
            var actual = string.Join("|", data[path3].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path2].Select(s => s.ToString().Trim()));
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
        }

        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainASinglePathWhichIsEnumerable_Expected_FlattenedDataWithValuesFromEnumerablePath()
        {
            IPath path = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");
            var paths = new List<IPath> { path };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            const string expected = "Brendon|Jayd|Bob|Joe";
            var actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainASinglePathWhichIsScalar_Expected_FlattenedDataWithValueFromScalarPath()
        {
            IPath path = new JsonPath("Name", "Name");
            var paths = new List<IPath> { path };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            const string expected = "Dev2";
            var actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectEnumerablesAsRelated_WithEnumerableSymbolAndSeperatorSymbol_Expected_PipeDelimited()
        {
            List<IPath> namePath = new List<IPath>() { new JsonPath("().", "().") };

            var JsonNavigator = new JsonNavigator(testData);
            
            var actual = string.Join("|", JsonNavigator.SelectEnumerablesAsRelated(namePath).Values.FirstOrDefault());
            const string expected = @"""Name"": ""Dev2""|""Motto"": ""Eat lots of cake""|""Departments"": [
  {
    ""Name"": ""Dev"",
    ""Employees"": [
      {
        ""Name"": ""Brendon"",
        ""Surename"": ""Page""
      },
      {
        ""Name"": ""Jayd"",
        ""Surename"": ""Page""
      }
    ]
  },
  {
    ""Name"": ""Accounts"",
    ""Employees"": [
      {
        ""Name"": ""Bob"",
        ""Surename"": ""Soap""
      },
      {
        ""Name"": ""Joe"",
        ""Surename"": ""Pants""
      }
    ]
  }
]|""Contractors"": [
  {
    ""Name"": ""Roofs Inc."",
    ""PhoneNumber"": ""123""
  },
  {
    ""Name"": ""Glass Inc."",
    ""PhoneNumber"": ""1234""
  },
  {
    ""Name"": ""Doors Inc."",
    ""PhoneNumber"": ""1235""
  },
  {
    ""Name"": ""Cakes Inc."",
    ""PhoneNumber"": ""1236""
  }
]|""PrimitiveRecordset"": [
  ""\r\n        RandomData\r\n    "",
  ""\r\n        RandomData1\r\n    ""
]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectEnumerablesAsRelated_WithSeperatorSymbol_Expected_UnchangedPath()
        {
            List<IPath> namePath = new List<IPath>() { new JsonPath(".", ".") };

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerablesAsRelated(namePath).Values.FirstOrDefault());

            var expected = @"{
  ""Name"": ""Dev2"",
  ""Motto"": ""Eat lots of cake"",
  ""Departments"": [
    {
      ""Name"": ""Dev"",
      ""Employees"": [
        {
          ""Name"": ""Brendon"",
          ""Surename"": ""Page""
        },
        {
          ""Name"": ""Jayd"",
          ""Surename"": ""Page""
        }
      ]
    },
    {
      ""Name"": ""Accounts"",
      ""Employees"": [
        {
          ""Name"": ""Bob"",
          ""Surename"": ""Soap""
        },
        {
          ""Name"": ""Joe"",
          ""Surename"": ""Pants""
        }
      ]
    }
  ],
  ""Contractors"": [
    {
      ""Name"": ""Roofs Inc."",
      ""PhoneNumber"": ""123""
    },
    {
      ""Name"": ""Glass Inc."",
      ""PhoneNumber"": ""1234""
    },
    {
      ""Name"": ""Doors Inc."",
      ""PhoneNumber"": ""1235""
    },
    {
      ""Name"": ""Cakes Inc."",
      ""PhoneNumber"": ""1236""
    }
  ],
  ""PrimitiveRecordset"": [
    ""\r\n        RandomData\r\n    "",
    ""\r\n        RandomData1\r\n    ""
  ]
}";

            Assert.AreEqual(expected, actual);
        }
    }
}
