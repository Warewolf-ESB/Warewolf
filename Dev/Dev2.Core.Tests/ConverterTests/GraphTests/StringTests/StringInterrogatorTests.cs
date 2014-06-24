using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.String;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests.StringTests {
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StringInterrogatorTests {

        internal string XmlGiven() {
            return @"<Company Name='Dev2'>
    <Motto>Eat lots of cake</Motto>
    <PreviousMotto/>
	<Departments TestAttrib='testing'>
		<Department Name='Dev'>
			<Employees>
				<Person Name='Brendon' Surename='Page' />
				<Person Name='Jayd' Surename='Page' />
			</Employees>
		</Department>
		<Department Name='Accounts'>
			<Employees>
				<Person Name='Bob' Surename='Soap' />
				<Person Name='Joe' Surename='Pants' />
			</Employees>
		</Department>
	</Departments>
    <InlineRecordSet>
        RandomData
    </InlineRecordSet>
    <InlineRecordSet>
        RandomData1
    </InlineRecordSet>
    <OuterNestedRecordSet>
        <InnerNestedRecordSet ItemValue='val1' />
        <InnerNestedRecordSet ItemValue='val2' />
    </OuterNestedRecordSet>
    <OuterNestedRecordSet>
        <InnerNestedRecordSet ItemValue='val3' />
        <InnerNestedRecordSet ItemValue='val4' />
    </OuterNestedRecordSet>
</Company>";
        }

        internal string JsonGiven() {
            return @"{
  ""Company"": {
    ""Name"": ""Dev2"",
    ""Motto"": ""Eat lots of cake"",
    ""Departments"": {      
      ""Department"": [
        {
          ""Name"": ""Dev"",
          ""Employees"": {
            ""Person"": [
              {
                ""Name"": ""Brendon"",
                ""Surename"": ""Page""
              },
              {
                ""Name"": ""Jayd"",
                ""Surename"": ""Page""
              }
            ]
          }
        },
        {
          ""Name"": ""Accounts"",
          ""Employees"": {
            ""Person"": [
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
        }
      ]
    },
    ""InlineRecordSet"": [
      ""
        RandomData
    "",
      ""
        RandomData1
    ""
    ],
    ""OuterNestedRecordSet"": [
      {
        ""InnerNestedRecordSet"": [
          { ""ItemValue"": ""val1"" },
          { ""ItemValue"": ""val2"" }
        ]
      },
      {
        ""InnerNestedRecordSet"": [
          { ""ItemValue"": ""val3"" },
          { ""ItemValue"": ""val4"" }
        ]
      }
    ]
  }
}";
        }


        /// <summary>
        /// Create mapper given XML expected XML mapper created.
        /// </summary>
        [TestMethod]
        public void CreateMapper_Expected_XmlMapper() {         
            StringInterrogator stringInterrogator = new StringInterrogator();

            IMapper mapper = stringInterrogator.CreateMapper(XmlGiven());

            Type expected = typeof(XmlMapper);
            Type actual = mapper.GetType();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create navigator given xml expected XML navigator returned.
        /// </summary>
        [TestMethod]
        public void CreateNavigator_Expected_XmlNavigator() {            
            StringInterrogator stringInterrogator = new StringInterrogator();

            INavigator navigator = stringInterrogator.CreateNavigator(XmlGiven(), typeof(XmlPath));

            Type expected = typeof(XmlNavigator);
            Type actual = navigator.GetType();

            Assert.AreEqual(expected, actual);
        }
    }
}
