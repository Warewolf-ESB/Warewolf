
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
// ReSharper disable ExplicitCallerInfoArgument
namespace Dev2.Infrastructure.Tests.Logs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class LoggerTests
    {
        TestTraceListner _testTraceListner;

        #region Initialization/Cleanup

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            _testTraceListner = new TestTraceListner(new StringBuilder());
            Trace.Listeners.Add(_testTraceListner);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Dev2Logger_EnableTrace")]
        public void Dev2Logger_EnableTrace_Set_ExpectValueIsset()
        {
            //------------Setup for test--------------------------
            Dev2Logger.EnableTraceOutput = true;

            
            //------------Execute Test---------------------------
            Assert.AreEqual(true,Dev2Logger.EnableTraceOutput);
            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Dev2Logger_EnableInfo")]
        public void Dev2Logger_EnableInfo_Set_ExpectValueIsset()
        {
            //------------Setup for test--------------------------
            Dev2Logger.EnableInfoOutput = true;


            //------------Execute Test---------------------------
            Assert.AreEqual(true, Dev2Logger.EnableInfoOutput);
            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Dev2Logger_EnableError")]
        public void Dev2Logger_EnableError_Set_ExpectValueIsset()
        {
            //------------Setup for test--------------------------
            Dev2Logger.EnableErrorOutput = true;


            //------------Execute Test---------------------------
            Assert.AreEqual(true, Dev2Logger.EnableErrorOutput);
            //------------Assert Results-------------------------
        }


        #endregion
    }

    public class TestTraceListner : TraceListener
    {
        readonly StringBuilder _stringBuilder;

        public TestTraceListner(StringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder;
        }

        public string CurrentlyLogged
        {
            get
            {
                return _stringBuilder.ToString();
            }
        }

        #region Overrides of TraceListener

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write. </param>
        public override void Write(string message)
        {
            _stringBuilder.Append(message);
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write. </param>
        public override void WriteLine(string message)
        {
            _stringBuilder.AppendLine(message);
        }

        #endregion
    }
}
