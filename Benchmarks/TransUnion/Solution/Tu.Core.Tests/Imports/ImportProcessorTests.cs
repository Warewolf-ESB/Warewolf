using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Extensions;
using Tu.Imports;
using Tu.Rules;

namespace Tu.Core.Tests.Imports
{
    [TestClass]
    public class ImportProcessorTests
    {
        //static readonly string TestPackCsv = ResourceFetcher.Fetch("Tu.Core.Tests", "Test-Pack.csv");

        static readonly List<string> TestCsvList = new List<string>(new[]
        {
            "Client Ref No 1|Client Ref No 2|Surname|Forename 1|Forename 2|Forename 3|DOB|RSA ID",
            "7412185778081||BOFu|ROBERT|NKOSIMATHI||19741218|",                     // RSA ID missing and surname not all upper case
            "8808015443086||RAMOROKA|DANNY|||19880801|808015443086",                // RSA ID length to short
            "8301070488089||JONATHAN|LINDIWE|JEANETTE||19830107|8301070488089"      // All ok
        });

        static readonly string TestCsv = string.Join(Environment.NewLine, TestCsvList);

        static Dictionary<string, string> CreateTestInputOutputColumnMapping()
        {
            return new Dictionary<string, string>
            {
                { "RSA ID", "GovID" },
                { "Surname", "Surname" },
                { "Forename 1", "FirstNames" },
                { "Forename 2", "FirstNames" },
                { "Forename 3", "FirstNames" },
                { "DOB", "DOB" },
            };
        }

        static List<OutputColumn> CreateTestOutputColumns(IsValidFunc govIDRule = null, IsValidFunc surnameRule = null, IsValidFunc firstNamesRule = null)
        {
            return new List<OutputColumn>(new[]
            {
                new OutputColumn("GovID", typeof(string), "GovIDRule", govIDRule) { IsKey = true },
                new OutputColumn("Surname", typeof(string), "SurnameRule", surnameRule),
                new OutputColumn("SurnameDate", typeof(DateTime)),
                new OutputColumn("FirstNames", typeof(string), "ForenameRule", firstNamesRule),
                new OutputColumn("DOB", typeof(DateTime)) { InputFormat = "yyyyMMdd" }
            });
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImportProcessor_Constructor_NullOutputColumns_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var importProcessor = new ImportProcessor(null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImportProcessor_Constructor_NullInputOutputColumnMapping_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var importProcessor = new ImportProcessor(new List<OutputColumn>(), null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImportProcessor_Constructor_OutputColumnsWithoutKey_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("GovID", typeof(string)) { IsKey = false },
                new OutputColumn("Surname", typeof(string))
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "RSA ID", "GovID" },
                { "Surname", "Surname" }
            };

            //------------Execute Test---------------------------
            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Constructor")]
        public void ImportProcessor_Constructor_ValidArgs_CreatesErrorsTable()
        {
            //------------Setup for test--------------------------
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("GovID", typeof(string)) { IsKey = true },
                new OutputColumn("Surname", typeof(string))
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "RSA ID", "GovID" },
                { "Surname", "Surname" }
            };

            //------------Execute Test---------------------------
            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Assert Results-------------------------
            Assert.IsNotNull(importProcessor.Errors);

            Assert.AreEqual(2, importProcessor.Errors.Columns.Count);
            Assert.AreEqual("GovID", importProcessor.Errors.Columns[0].ColumnName);
            Assert.AreEqual("Reason", importProcessor.Errors.Columns[1].ColumnName);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Constructor")]
        public void ImportProcessor_Constructor_ValidArgs_CreatesOutputDataTable()
        {
            //------------Setup for test--------------------------
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("GovID", typeof(string)) { IsKey = true },
                new OutputColumn("Surname", typeof(string))
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "RSA ID", "GovID" },
                { "Surname", "Surname" }
            };

            var expectedData = new DataTable();
            expectedData.Columns.Add("IsValid", typeof(bool)).DefaultValue = true;

            foreach(var outputColumn in testOutputColumns)
            {
                expectedData.Columns.Add(outputColumn.Name);
            }


            //------------Execute Test---------------------------
            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Assert Results-------------------------
            Assert.IsNotNull(importProcessor.OutputData);

            var expectedColumns = expectedData.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
            var actualColumns = importProcessor.OutputData.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
            Assert.IsTrue(expectedColumns.SequenceEqual(actualColumns));
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_NullCsvInput_ClearsAndReturnsEmptyList()
        {
            //------------Setup for test--------------------------
            var testOutputColumns = CreateTestOutputColumns();
            var testInputOutputColumnMapping = CreateTestInputOutputColumnMapping();

            //------------Execute Test---------------------------
            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);
            var result = importProcessor.Run(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, importProcessor.Errors.Rows.Count);
            Assert.AreEqual(0, importProcessor.OutputData.Rows.Count);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_Errors_ClearedBeforeExecution()
        {
            //------------Setup for test--------------------------
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("GovID", typeof(string)) { IsKey = true },
                new OutputColumn("Surname", typeof(string))
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "RSA ID", "GovID" },
                { "Surname", "Surname" }
            };

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);
            importProcessor.Errors.Rows.Add("1234", "There was an error.");
            Assert.AreEqual(1, importProcessor.Errors.Rows.Count);

            //------------Execute Test---------------------------
            importProcessor.Run(null);

            //------------Assert Results-------------------------

            Assert.AreEqual(0, importProcessor.Errors.Rows.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_OutputData_ClearedBeforeExecution()
        {
            //------------Setup for test--------------------------
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("GovID", typeof(string)) { IsKey = true },
                new OutputColumn("Surname", typeof(string))
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "RSA ID", "GovID" },
                { "Surname", "Surname" }
            };

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);
            importProcessor.OutputData.Rows.Add(true, "1234", "xyz");

            Assert.AreEqual(1, importProcessor.OutputData.Rows.Count);

            //------------Execute Test---------------------------
            importProcessor.Run(null);

            //------------Assert Results-------------------------

            Assert.AreEqual(0, importProcessor.OutputData.Rows.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_OutputData_FilledAccordingToInputOutputMappings()
        {
            //------------Setup for test--------------------------
            var csvData = TestCsv.ToDataTable("|");

            var testOutputColumns = CreateTestOutputColumns();
            var testInputOutputColumnMapping = CreateTestInputOutputColumnMapping();

            var expectedData = new DataTable();
            var invalidColumn = expectedData.Columns.Add("IsValid", typeof(bool));
            invalidColumn.DefaultValue = true;

            foreach(var outputColumn in testOutputColumns)
            {
                expectedData.Columns.Add(outputColumn.Name, outputColumn.ColumnType);
            }
            foreach(DataRow testRow in csvData.Rows)
            {
                var expectedRow = expectedData.NewRow();
                expectedRow["GovID"] = testRow["RSA ID"];
                expectedRow["Surname"] = testRow["Surname"];
                expectedRow["FirstNames"] = string.Join(" ", new[] { testRow["Forename 1"], testRow["Forename 2"], testRow["Forename 3"] }).TrimEnd(' ');
                expectedRow["DOB"] = DateTime.ParseExact(testRow["DOB"].ToStringSafe(), "yyyyMMdd", CultureInfo.InvariantCulture);
                expectedData.Rows.Add(expectedRow);
            }

            var processor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            processor.Run(TestCsv);

            //------------Assert Results-------------------------

            Assert.AreEqual(expectedData.Rows.Count, processor.OutputData.Rows.Count);

            for(var i = 0; i < expectedData.Rows.Count; i++)
            {
                var expectedItems = expectedData.Rows[i].ItemArray;
                var actualItems = processor.OutputData.Rows[i].ItemArray;
                Assert.IsTrue(expectedItems.SequenceEqual(actualItems));
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_OutputData_ValidatorInvokedForeachOutputColumn()
        {
            //------------Setup for test--------------------------
            var govIDRuleHitCount = 0;
            var govIDRule = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                govIDRuleHitCount++;
                return new ValidationResult();
            });

            var surnameRuleHitCount = 0;
            var surnameRule = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                surnameRuleHitCount++;
                return new ValidationResult();
            });

            var forenameRuleHitCount = 0;
            var forenameRule = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                forenameRuleHitCount++;
                return new ValidationResult();
            });


            var testOutputColumns = CreateTestOutputColumns(govIDRule, surnameRule, forenameRule);
            var testInputOutputColumnMapping = CreateTestInputOutputColumnMapping();

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            importProcessor.Run(TestCsv);

            //------------Assert Results-------------------------
            var expectedCount = importProcessor.OutputData.Rows.Count;

            Assert.AreEqual(expectedCount, govIDRuleHitCount);
            Assert.AreEqual(expectedCount, surnameRuleHitCount);
            Assert.AreEqual(expectedCount, forenameRuleHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_OutputData_ErrorsAddedForeachColumnInError()
        {
            //------------Setup for test--------------------------
            const int ExpectedErrorCount = 3;  // see TestCsvList comments

            var validator = new Validator(new RegexUtilities());

            var govIDRule = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                var rule = new GovIDRule(validator, "xx");
                var result = new ValidationResult { IsValid = rule.IsValid(fieldValue) };
                result.Errors.AddRange(rule.Errors);
                return result;

            });
            var surnameRule = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                var rule = new SurnameRule(validator, "xx");
                var result = new ValidationResult { IsValid = rule.IsValid(fieldValue) };
                result.Errors.AddRange(rule.Errors);
                return result;

            });
            var forenameRule = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                var rule = new ForenameRule(validator, "xx");
                var result = new ValidationResult { IsValid = rule.IsValid(fieldValue) };
                result.Errors.AddRange(rule.Errors);
                return result;

            });
            var testOutputColumns = CreateTestOutputColumns(govIDRule, surnameRule, forenameRule);
            var testInputOutputColumnMapping = CreateTestInputOutputColumnMapping();

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            importProcessor.Run(TestCsv);

            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedErrorCount, importProcessor.Errors.Rows.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_OutputData_IsValidSetForValidRows()
        {
            //------------Setup for test--------------------------
            const int ValidRowIndex = 1;
            var rowIndex = 0;

            var govIDRule = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                var result = new ValidationResult
                {
                    IsValid = (rowIndex++ == ValidRowIndex)
                };
                result.Errors.Add("Invalid Gov ID");
                return result;
            });

            var surnameRule = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                var result = new ValidationResult
                {
                    IsValid = true
                };
                result.Errors.Add("Invalid Surname");
                return result;
            });

            var forenameRule = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                var result = new ValidationResult
                {
                    IsValid = true
                };
                result.Errors.Add("Invalid Forename");
                return result;
            });

            var testOutputColumns = CreateTestOutputColumns(govIDRule, surnameRule, forenameRule);
            var testInputOutputColumnMapping = CreateTestInputOutputColumnMapping();

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            var validRows = importProcessor.Run(TestCsv);

            //------------Assert Results-------------------------
            for(var i = 0; i < importProcessor.OutputData.Rows.Count; i++)
            {
                var dataRow = importProcessor.OutputData.Rows[i];
                var isValid = (bool)dataRow["IsValid"];
                Assert.AreEqual(i == ValidRowIndex, isValid);
            }

            Assert.AreEqual(1, validRows.Length);

            var actualValidItems = validRows[0].ItemArray;
            var expectedValidItems = importProcessor.OutputData.Rows[ValidRowIndex].ItemArray;
            Assert.IsTrue(expectedValidItems.SequenceEqual(actualValidItems));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_GetOutputValue_NullInputValue_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var csvList = new List<string>(new[]
            {
                "Ref|Col1",
                "23|"
            });

            // Create columns to test each scenario
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("Ref", typeof(string)) { IsKey = true },
                new OutputColumn("Col1", typeof(string)),
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "Ref", "Ref" },
                { "Col1", "Col1" },
            };

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            var result = importProcessor.Run(string.Join(Environment.NewLine, csvList));

            //------------Assert Results-------------------------
            Assert.AreEqual(0, importProcessor.Errors.Rows.Count);
            Assert.AreEqual(1, importProcessor.OutputData.Rows.Count);
            Assert.IsTrue(Convert.IsDBNull(importProcessor.OutputData.Rows[0]["Col1"]));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_GetOutputValue_DateTime_Converted()
        {
            //------------Setup for test--------------------------
            var expectedDate = new DateTime(1974, 12, 18);

            var csvList = new List<string>(new[]
            {
                "Ref|Col1|Col2|Col3",
                string.Format("1|{0:yyyyMMdd}|{0:yyyy/MM/dd}|", expectedDate)
            });

            // Create columns to test each scenario
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("Ref", typeof(string)) { IsKey = true },
                new OutputColumn("Col1", typeof(DateTime)) { InputFormat = "yyyyMMdd" },
                new OutputColumn("Col2", typeof(DateTime)),
                new OutputColumn("Col3", typeof(DateTime))
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "Ref", "Ref" },
                { "Col1", "Col1" },
                { "Col2", "Col2" },
                { "Col3", "Col3" }
            };

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            var result = importProcessor.Run(string.Join(Environment.NewLine, csvList));

            //------------Assert Results-------------------------
            Assert.AreEqual(0, importProcessor.Errors.Rows.Count);
            Assert.AreEqual(1, importProcessor.OutputData.Rows.Count);
            Assert.AreEqual(expectedDate, importProcessor.OutputData.Rows[0]["Col1"]);
            Assert.AreEqual(expectedDate, importProcessor.OutputData.Rows[0]["Col2"]);
            Assert.IsTrue(Convert.IsDBNull(importProcessor.OutputData.Rows[0]["Col3"]));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_GetOutputValue_Boolean_Converted()
        {
            //------------Setup for test--------------------------
            var csvList = new List<string>(new[]
            {
                "Ref|Col1|Col2|Col3",
                "23|1|true|xx"
            });

            // Create columns to test each scenario
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("Ref", typeof(string)) { IsKey = true },
                new OutputColumn("Col1", typeof(bool)),
                new OutputColumn("Col2", typeof(bool)),
                new OutputColumn("Col3", typeof(bool))
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "Ref", "Ref" },
                { "Col1", "Col1" },
                { "Col2", "Col2" },
                { "Col3", "Col3" }
            };

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            var result = importProcessor.Run(string.Join(Environment.NewLine, csvList));

            //------------Assert Results-------------------------
            Assert.AreEqual(0, importProcessor.Errors.Rows.Count);
            Assert.AreEqual(1, importProcessor.OutputData.Rows.Count);
            Assert.AreEqual(true, importProcessor.OutputData.Rows[0]["Col1"]);
            Assert.AreEqual(true, importProcessor.OutputData.Rows[0]["Col2"]);
            Assert.AreEqual(false, importProcessor.OutputData.Rows[0]["Col3"]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_GetOutputValue_Int_Converted()
        {
            //------------Setup for test--------------------------
            var csvList = new List<string>(new[]
            {
                "Ref|Col1|Col2",
                "23|35366|xx"
            });

            // Create columns to test each scenario
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("Ref", typeof(string)) { IsKey = true },
                new OutputColumn("Col1", typeof(int)),
                new OutputColumn("Col2", typeof(int)),
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "Ref", "Ref" },
                { "Col1", "Col1" },
                { "Col2", "Col2" },
            };

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            var result = importProcessor.Run(string.Join(Environment.NewLine, csvList));

            //------------Assert Results-------------------------
            Assert.AreEqual(0, importProcessor.Errors.Rows.Count);
            Assert.AreEqual(1, importProcessor.OutputData.Rows.Count);
            Assert.AreEqual(35366, importProcessor.OutputData.Rows[0]["Col1"]);
            Assert.AreEqual(0, importProcessor.OutputData.Rows[0]["Col2"]);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_GetOutputValue_ConvertibleType_Converted()
        {
            //------------Setup for test--------------------------
            var csvList = new List<string>(new[]
            {
                "Ref|Col1",
                "23|353.66"
            });

            // Create columns to test each scenario
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("Ref", typeof(string)) { IsKey = true },
                new OutputColumn("Col1", typeof(double)),
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "Ref", "Ref" },
                { "Col1", "Col1" },
            };

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            var result = importProcessor.Run(string.Join(Environment.NewLine, csvList));

            //------------Assert Results-------------------------
            Assert.AreEqual(0, importProcessor.Errors.Rows.Count);
            Assert.AreEqual(1, importProcessor.OutputData.Rows.Count);
            Assert.AreEqual(353.66, importProcessor.OutputData.Rows[0]["Col1"]);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ImportProcessor_Run")]
        public void ImportProcessor_Run_GetOutputValue_NonConvertibleType_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var csvList = new List<string>(new[]
            {
                "Ref|Col1",
                "23|xx"
            });

            // Create columns to test each scenario
            var testOutputColumns = new List<OutputColumn>(new[]
            {
                new OutputColumn("Ref", typeof(string)) { IsKey = true },
                new OutputColumn("Col1", typeof(double)),
            });

            var testInputOutputColumnMapping = new Dictionary<string, string>
            {
                { "Ref", "Ref" },
                { "Col1", "Col1" },
            };

            var importProcessor = new ImportProcessor(testOutputColumns, testInputOutputColumnMapping);

            //------------Execute Test---------------------------
            var result = importProcessor.Run(string.Join(Environment.NewLine, csvList));

            //------------Assert Results-------------------------
            Assert.AreEqual(0, importProcessor.Errors.Rows.Count);
            Assert.AreEqual(1, importProcessor.OutputData.Rows.Count);
            Assert.IsTrue(Convert.IsDBNull(importProcessor.OutputData.Rows[0]["Col1"]));
        }
    }
}
