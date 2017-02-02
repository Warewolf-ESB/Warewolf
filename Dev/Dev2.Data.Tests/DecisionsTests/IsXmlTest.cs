using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsXmlTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsXml_Invoke")]
        public void GivenSomeString_IsXml_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isXml = new IsXml();
            string[] cols = new string[2];
            cols[0] = "Eight";
            //------------Execute Test---------------------------
            bool result = isXml.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsXml_Invoke")]
        public void IsXml_Invoke_IsXml_ReturnsTrue()
        {
            const string xmlFragment = @"<InnerError>Index #0
Message: Login failed for user 'testuser2'.
LineNumber: 65536
Source: .Net SqlClient Data Provider
Procedure: 
</InnerError><InnerError>ExecuteReader requires an open and available Connection. The connection's current state is closed.
   at System.Data.SqlClient.SqlCommand.ValidateCommand(String method, Boolean async)
   at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method, TaskCompletionSource`1 completion, Int32 timeout, Task& task, Boolean asyncWrite)
   at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method)
   at System.Data.SqlClient.SqlCommand.ExecuteReader(CommandBehavior behavior, String method)
   at System.Data.SqlClient.SqlCommand.ExecuteReader(CommandBehavior behavior)
   at Dev2.Services.Sql.SqlServer.ExecuteReader[T](SqlCommand command, CommandBehavior commandBehavior, Func`2 handler) in c:\Development\Dev\Dev2.Services.Sql\SqlServer.cs:line 121
   at Dev2.Services.Sql.SqlServer.FetchDataTable(SqlParameter[] parameters) in c:\Development\Dev\Dev2.Services.Sql\SqlServer.cs:line 61
   at Dev2.Services.Execution.DatabaseServiceExecution.SqlExecution(ErrorResultTO errors, Object& executeService) in c:\Development\Dev\Dev2.Services.Execution\DatabaseServiceExecution.cs:line 118</InnerError>";

            //------------Setup for test--------------------------
            var notStartsWith = new IsXml();
            string[] cols = new string[2];
            cols[0] = xmlFragment;
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsXml_HandlesType")]
        public void IsXml_HandlesType_ReturnsIsXmlType()
        {
            var decisionType = enDecisionType.IsXML;
            //------------Setup for test--------------------------
            var isXml = new IsXml();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isXml.HandlesType());
        }
    }
}
