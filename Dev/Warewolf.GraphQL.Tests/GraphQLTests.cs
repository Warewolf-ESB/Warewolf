using System;
using Dev2.Data;
using Dev2.DynamicServices;
using GraphQL.Introspection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarewolfParserInterop;

namespace Warewolf.GraphQL.Tests
{
    [TestClass]
    public class GraphQLTests
    {
        [TestMethod]
        public void Simple_Query_ForAllScalar_ShouldReturnAllScalarsInEnvironment()
        {
            //------------Setup----------------------------------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"/>" +
                                    "<Surname Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"/>" +
                                    "<FullName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Age Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Salary Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<rec Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                                    "<a Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<b Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<c Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                                    "</rec>" +
                                    "</DataList>";
            dataObj.Environment.Assign("[[rec().a]]", "1", 0);
            dataObj.Environment.Assign("[[rec().b]]", "2", 0);
            dataObj.Environment.Assign("[[rec().c]]", "3", 0);
            dataObj.Environment.Assign("[[Name]]", "Bob", 0);
            dataObj.Environment.Assign("[[Surname]]", "Mary", 0);
            dataObj.Environment.Assign("[[FullName]]", "Bob Mary", 0);
            dataObj.Environment.Assign("[[Age]]", "15", 0);
            dataObj.Environment.Assign("[[Salary]]", "1550.55", 0);
            //--------------Execute-------------------------------------------------
            var graphQLExecutor = new GraphQLExecutor(dataObj.Environment, dataList);
            var result = graphQLExecutor.Execute("{ scalars { name, value } }");
            Assert.AreEqual("{\"data\":{\"scalars\":[{\"name\":\"Age\",\"value\":\"15\"},{\"name\":\"FullName\",\"value\":\"Bob Mary\"},{\"name\":\"Name\",\"value\":\"Bob\"},{\"name\":\"Salary\",\"value\":\"1550.55\"},{\"name\":\"Surname\",\"value\":\"Mary\"}]}}", result);
        }

        [TestMethod]
        public void Simple_Query_QueryNameBob_ShouldReturnMatchingScalarsInEnvironment()
        {
            //------------Setup----------------------------------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"/>" +
                                    "<Surname Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"/>" +
                                    "<FullName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Age Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Salary Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<rec Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                                    "<a Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<b Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<c Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                                    "</rec>" +
                                    "</DataList>";
            dataObj.Environment.Assign("[[rec().a]]", "1", 0);
            dataObj.Environment.Assign("[[rec().b]]", "2", 0);
            dataObj.Environment.Assign("[[rec().c]]", "3", 0);
            dataObj.Environment.Assign("[[Name]]", "Bob", 0);
            dataObj.Environment.Assign("[[Surname]]", "Mary", 0);
            dataObj.Environment.Assign("[[FullName]]", "Bob Mary", 0);
            dataObj.Environment.Assign("[[Age]]", "15", 0);
            dataObj.Environment.Assign("[[Salary]]", "1550.55", 0);
            //--------------Execute-------------------------------------------------
            var graphQLExecutor = new GraphQLExecutor(dataObj.Environment, dataList);
            var result = graphQLExecutor.Execute("{ scalarName(name: \"Name\")  { name, value } }");
            Assert.AreEqual("{\"data\":{\"scalarName\":{\"name\":\"Name\",\"value\":\"Bob\"}}}", result);
        }

        [TestMethod]
        public void Simple_Query_ForAllRecordsets_ShouldReturnAllRecordsetsInEnvironment()
        {
            //------------Setup----------------------------------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"/>" +
                                    "<Surname Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"/>" +
                                    "<FullName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Age Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Salary Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<rec Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                                    "<a Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<b Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<c Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                                    "</rec>" +
                                    "</DataList>";
            dataObj.Environment.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);
            dataObj.Environment.AssignWithFrame(new AssignValue("[[rec().b]]", "2"), 0);
            dataObj.Environment.AssignWithFrame(new AssignValue("[[rec().c]]", "3"), 0);
            dataObj.Environment.AssignWithFrame(new AssignValue("[[rec().a]]", "Warewolf"), 0);
            dataObj.Environment.AssignWithFrame(new AssignValue("[[rec().b]]", "Is"), 0);
            dataObj.Environment.AssignWithFrame(new AssignValue("[[rec().c]]", "Great!"), 0);
            dataObj.Environment.Assign("[[Name]]", "Bob", 0);
            dataObj.Environment.Assign("[[Surname]]", "Mary", 0);
            dataObj.Environment.Assign("[[FullName]]", "Bob Mary", 0);
            dataObj.Environment.Assign("[[Age]]", "15", 0);
            dataObj.Environment.Assign("[[Salary]]", "1550.55", 0);
            //--------------Execute-------------------------------------------------
            var graphQLExecutor = new GraphQLExecutor(dataObj.Environment, dataList);
            var result = graphQLExecutor.Execute("{ recordsets { name, columns { name value } } }");
            Assert.AreEqual("{\"data\":{\"recordsets\":[{\"name\":\"rec\",\"columns\":[{\"name\":\"a\",\"value\":[\"1\",\"Warewolf\"]},{\"name\":\"b\",\"value\":[\"2\",\"Is\"]},{\"name\":\"c\",\"value\":[\"3\",\"Great!\"]}]}]}}", result);
        }

    }
}
