using System;
using Dev2.DynamicServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Warewolf.GraphQL.Tests
{
  [TestClass]
  public class GraphQLTests
  {
    [TestMethod]
    public void Simple_Query_ForAllScalar_ShouldReturnAllScalarsFromEnvironment()
    {
      //------------Setup----------------------------------------------------
      var env = new ExecutionEnvironment();
      env.Assign("[[Name]]", "Bob", 0);
      env.Assign("[[Surname]]", "Mary", 0);
      env.Assign("[[FullName]]", "Bob Mary", 0);
      env.Assign("[[Age]]", "15", 0);
      env.Assign("[[Salary]]", "1550.55", 0);
      //--------------Execute-------------------------------------------------
      var graphQLExecutor = new GraphQLExecutor(env);
      var result = graphQLExecutor.Execute("{ scalars { name, value } }");
      Assert.AreEqual(
                      "{\"data\":{\"scalars\":[{\"name\":\"Age\",\"value\":\"15\"},{\"name\":\"FullName\",\"value\":\"Bob Mary\"},{\"name\":\"Name\",\"value\":\"Bob\"},{\"name\":\"Salary\",\"value\":\"1550.55\"},{\"name\":\"Surname\",\"value\":\"Mary\"}]}}",
                      result);
    }

    [TestMethod]
    public void Simple_Query_ForSpecifiedScalar_ShouldReturnMatchingScalarsFromEnvironment()
    {
      //------------Setup----------------------------------------------------
      var env = new ExecutionEnvironment();
      env.Assign("[[Name]]", "Bob", 0);
      env.Assign("[[Surname]]", "Mary", 0);
      env.Assign("[[FullName]]", "Bob Mary", 0);
      env.Assign("[[Age]]", "15", 0);
      env.Assign("[[Salary]]", "1550.55", 0);
      //--------------Execute-------------------------------------------------
      var graphQLExecutor = new GraphQLExecutor(env);
      var result = graphQLExecutor.Execute("{ scalarName(name: \"Name\")  { name, value } }");
      Assert.AreEqual("{\"data\":{\"scalarName\":{\"name\":\"Name\",\"value\":\"Bob\"}}}", result);
    }

    [TestMethod]
    public void Simple_Query_ForAllRecordsets_ShouldReturnAllRecordsetsFromEnvironment()
    {
      //------------Setup----------------------------------------------------
      var env = new ExecutionEnvironment();
      env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);
      env.AssignWithFrame(new AssignValue("[[rec().b]]", "2"), 0);
      env.AssignWithFrame(new AssignValue("[[rec().c]]", "3"), 0);
      env.AssignWithFrame(new AssignValue("[[rec().a]]", "Warewolf"), 0);
      env.AssignWithFrame(new AssignValue("[[rec().b]]", "Is"), 0);
      env.AssignWithFrame(new AssignValue("[[rec().c]]", "Great!"), 0);
      //--------------Execute-------------------------------------------------
      var graphQLExecutor = new GraphQLExecutor(env);
      var result = graphQLExecutor.Execute("{ recordsets { name, columns { name value } } }");
      Assert.AreEqual(
                      "{\"data\":{\"recordsets\":[{\"name\":\"rec\",\"columns\":[{\"name\":\"a\",\"value\":[\"1\",\"Warewolf\"]},{\"name\":\"b\",\"value\":[\"2\",\"Is\"]},{\"name\":\"c\",\"value\":[\"3\",\"Great!\"]}]}]}}",
                      result);
    }

    [TestMethod]
    public void Simple_Query_ForSpecificRecordset_ShouldReturnQueriedRecordsetFromEnvironment()
    {
      //------------Setup----------------------------------------------------
      var env = new ExecutionEnvironment();
      env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);
      env.AssignWithFrame(new AssignValue("[[rec().b]]", "2"), 0);
      env.AssignWithFrame(new AssignValue("[[rec().c]]", "3"), 0);
      env.AssignWithFrame(new AssignValue("[[table().col1]]", "Warewolf"), 0);
      env.AssignWithFrame(new AssignValue("[[table().col2]]", "Is"), 0);
      env.AssignWithFrame(new AssignValue("[[table().col3]]", "Great!"), 0);
      var graphQLExecutor = new GraphQLExecutor(env);
      var result = graphQLExecutor.Execute("{ recordsetName(name: \"table\") { name, columns { name value } } }");
      Assert.AreEqual(
                      "{\"data\":{\"recordsetName\":{\"name\":\"table\",\"columns\":[{\"name\":\"col1\",\"value\":[\"Warewolf\"]},{\"name\":\"col2\",\"value\":[\"Is\"]},{\"name\":\"col3\",\"value\":[\"Great!\"]}]}}}",
                      result);
    }

  }
}
