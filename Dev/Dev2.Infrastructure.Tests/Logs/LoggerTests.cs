using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Providers.Logs;
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
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_TraceInfo")]
        public void Logger_TraceInfo_WithStringValue_StringBuilderContainsMethodNameAndMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.TraceInfo("This is some information");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: INFORMATION ->  Logger_TraceInfo_WithStringValue_StringBuilderContainsMethodNameAndMessage : This is some information\r\n");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_TraceInfo")]
        public void Logger_TraceInfo_WithStringValue_StringBuilderContainsGivenMethodNameAndMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.TraceInfo("This is some information","MyMethodName");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: INFORMATION ->  MyMethodName : This is some information\r\n");
            VerifyDateTimeIsLogged(currentlyLogged);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_TraceInfo")]
        public void Logger_TraceInfo_WithNullStringValueWithMethodName_StringBuilderContainsOnlyMehodName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.TraceInfo(null,"MyMethodName");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: INFORMATION ->  MyMethodName : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_TraceInfo")]
        public void Logger_TraceInfo_WithNullStringValueWithNullMethodName_NoMehodNameNoMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.TraceInfo(null,null);
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: INFORMATION ->   : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_TraceInfo")]
        public void Logger_TraceInfo_WithNoParameters_MethodNameStillLogged()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.TraceInfo();
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: INFORMATION ->  Logger_TraceInfo_WithNoParameters_MethodNameStillLogged : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Warning")]
        public void Logger_Warning_WithStringValue_StringBuilderContainsMethodNameAndMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Warning("This is some information");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: WARNING ->  Logger_Warning_WithStringValue_StringBuilderContainsMethodNameAndMessage : This is some information\r\n");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Warning")]
        public void Logger_Warning_WithStringValue_StringBuilderContainsGivenMethodNameAndMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Warning("This is some information", "MyMethodName");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: WARNING ->  MyMethodName : This is some information\r\n");
            VerifyDateTimeIsLogged(currentlyLogged);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Warning")]
        public void Logger_Warning_WithNullStringValueWithMethodName_StringBuilderContainsOnlyMehodName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Warning(null, "MyMethodName");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: WARNING ->  MyMethodName : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Warning")]
        public void Logger_Warning_WithNullStringValueWithNullMethodName_NoMehodNameNoMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Warning(null, null);
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: WARNING ->   : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Warning")]
        public void Logger_Warning_WithNoParameters_MethodNameStillLogged()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Warning();
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: WARNING ->  Logger_Warning_WithNoParameters_MethodNameStillLogged : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithStringValue_StringBuilderContainsMethodNameAndMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Error("This is some information");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: ERROR ->  Logger_Error_WithStringValue_StringBuilderContainsMethodNameAndMessage : This is some information\r\n");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithStringValue_StringBuilderContainsGivenMethodNameAndMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Error("This is some information", "MyMethodName");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: ERROR ->  MyMethodName : This is some information\r\n");
            VerifyDateTimeIsLogged(currentlyLogged);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithNullStringValueWithMethodName_StringBuilderContainsOnlyMehodName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Error((string)null, "MyMethodName");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: ERROR ->  MyMethodName : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithNullStringValueWithNullMethodName_NoMehodNameNoMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Error((string)null, null);
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: ERROR ->   : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithNoParameters_MethodNameStillLogged()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Error();
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: ERROR ->  Logger_Error_WithNoParameters_MethodNameStillLogged : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithExceptionValue_StringBuilderContainsMethodNameAndMessage()
        {
            //------------Setup for test--------------------------
            var myException = new Exception("This is some information");
            //------------Execute Test---------------------------
            Logger.Error(myException);
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: EXCEPTION ->   : This is some information\r\n");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithExceptionValue_StringBuilderContainsGivenMethodNameAndMessage()
        {
            //------------Setup for test--------------------------
            var myException = new Exception("This is some information");
            //------------Execute Test---------------------------
            Logger.Error(myException, "MyMethodName");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: EXCEPTION ->  MyMethodName : {\"ClassName\":\"System.Exception\",\"Message\":\"This is some information\",\"Data\":null,\"InnerException\":null,\"HelpURL\":null,\"StackTraceString\":null,\"RemoteStackTraceString\":null,\"RemoteStackIndex\":0,\"ExceptionMethod\":null,\"HResult\":-2146233088,\"Source\":null,\"WatsonBuckets\":null}");
            VerifyDateTimeIsLogged(currentlyLogged);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithNullExceptionValueWithMethodName_StringBuilderContainsOnlyMehodName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Error((Exception)null, "MyMethodName");
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: EXCEPTION ->  MyMethodName : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithNullExceptionValueWithNullMethodName_NoMehodNameNoMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Logger.Error((Exception)null, null);
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: EXCEPTION ->   : ");
            VerifyDateTimeIsLogged(currentlyLogged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Logger_Error")]
        public void Logger_Error_WithExceptionWithInnerExceptionValue_StringBuilderContainsGivenMethodNameAndMessageAndInnerExceptionMessageStackTrace()
        {
            //------------Setup for test--------------------------
            var myException = new Exception("This is some information",new Exception("This is the one inner exception",new Exception("This is another inner exception")));
            //------------Execute Test---------------------------
            Logger.Error(myException);
            //------------Assert Results-------------------------
            var currentlyLogged = _testTraceListner.CurrentlyLogged;
            StringAssert.Contains(currentlyLogged, ":: EXCEPTION ->   : This is some information\r\n\r\nThis is the one inner exception\r\n\r\nThis is another inner exception\r\n");
            VerifyDateTimeIsLogged(currentlyLogged);
        } 

        static void VerifyDateTimeIsLogged(string currentlyLogged)
        {
            StringAssert.Contains(currentlyLogged, DateTime.Now.ToString("g"));
        }
        #endregion
    }

    public class TestTraceListner:TraceListener
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
