



namespace Dev2.Tests.Logging
{
    // ServerLoggerTest MUST NEVER REFERENCE STUDIO CORE TEST! FIX AND RESUBMIT
    ///// <summary>
    ///// Summary description for UnitTest1
    ///// </summary>
    //[TestClass]
    //[ExcludeFromCodeCoverage]
    //public class ServerLoggerTests
    //{
    //    public static object SyncLock = new object();
    //    public ServerLoggerTests()
    //    {
    //        //
    //        // TODO: Add constructor logic here
    //        //
    //    }

    //    private DummyDebugProvider _dummyDebugProvider = new DummyDebugProvider();

    //    private TestContext _testContextInstance;

    //    /// <summary>
    //    ///Gets or sets the test context which provides
    //    ///information about and functionality for the current test run.
    //    ///</summary>
    //    public TestContext TestContext
    //    {
    //        get
    //        {
    //            return _testContextInstance;
    //        }
    //        set
    //        {
    //            _testContextInstance = value;
    //        }
    //    }

    //    #region Additional test attributes
    //    //
    //    // You can use the following additional attributes as you write your tests:
    //    //
    //    // Use ClassInitialize to run code before running the first test in the class
    //    // [ClassInitialize()]
    //    // public static void MyClassInitialize(TestContext testContext) { }
    //    //
    //    // Use ClassCleanup to run code after all tests in a class have run
    //    // [ClassCleanup()]
    //    // public static void MyClassCleanup() { }

    //    // Use TestInitialize to run code before running each test 
    //     [TestInitialize()]
    //     public void MyTestInitialize()
    //     {
    //         Monitor.Enter(SyncLock);
    //     }

    //    // Use TestCleanup to run code after each test has run
    //     [TestCleanup()]
    //     public void MyTestCleanup()
    //     {
    //         Monitor.Exit(SyncLock);
    //     }

    //    #endregion

    //    [TestMethod]
    //    public void ShouldLogReturnsTrue()
    //    {
    //        //Setup
    //        var logging = new Configuration(XmlResource.Fetch("NonEmptySettings")).Logging;
    //        ServerLogger.LoggingSettings = logging;
    //        var state = _dummyDebugProvider.GetDebugState();

    //        //Execute
    //        var shouldLog = ServerLogger.ShouldLog(state);


    //        //Assert
    //        Assert.IsTrue(shouldLog);
    //    }

    //    [TestMethod]
    //    public void ShouldLogReturnsFalseIfLoggingNotEnabled()
    //    {
    //        //Setup
    //        var logging = new Configuration(XmlResource.Fetch("NonEmptySettings")).Logging;
    //        logging.IsLoggingEnabled = false;
    //        ServerLogger.LoggingSettings = logging;
    //        var state = _dummyDebugProvider.GetDebugState();

    //        //Execute
    //        var shouldLog = ServerLogger.ShouldLog(state);

    //        //Assert
    //        Assert.IsFalse(shouldLog);
    //    }

    //    [TestMethod]
    //    public void ShouldLogReturnFalseIfNotConcreteDebugState()
    //    {
    //        //Setup
    //        var logging = new Configuration(XmlResource.Fetch("NonEmptySettings")).Logging;
    //        ServerLogger.LoggingSettings = logging;
    //        var mockDebugState = new Mock<IDebugState>();

    //        //Execute
    //        var shouldLog = ServerLogger.ShouldLog(mockDebugState.Object);

    //        //Assert
    //        Assert.IsFalse(shouldLog);
    //    }

    //    [TestMethod]
    //    public void ShouldLogReturnsTrueIfLogAllTrue()
    //    {
    //        //Setup
    //        var logging = new Configuration(XmlResource.Fetch("NonEmptySettings")).Logging;
    //        logging.Workflows.Clear();
    //        logging.LogAll = true;
    //        ServerLogger.LoggingSettings = logging;
    //        var state = _dummyDebugProvider.GetDebugState();

    //        //Execute
    //        var shouldLog = ServerLogger.ShouldLog(state);

    //        //Assert
    //        Assert.IsTrue(shouldLog);
    //    }

    //    [TestMethod]
    //    public void ShouldLogReturnsFalseIfNotInSettings()
    //    {
    //        //Setup
    //        var logging = new Configuration(XmlResource.Fetch("NonEmptySettings")).Logging;
    //        logging.Workflows.Clear();
    //        ServerLogger.LoggingSettings = logging;
    //        var state = _dummyDebugProvider.GetDebugState();

    //        //Execute
    //        var shouldLog = ServerLogger.ShouldLog(state);

    //        //Assert
    //        Assert.IsFalse(shouldLog);
    //    }

    //    [TestMethod]
    //    public void GetWorkflowNameReturnsNameFromSettings()
    //    {
    //        //Setup
    //        var logging = new Configuration(XmlResource.Fetch("NonEmptySettings")).Logging;
    //        ServerLogger.LoggingSettings = logging;
    //        var state = _dummyDebugProvider.GetDebugState();

    //        //Execute
    //        var workflowName = ServerLogger.GetWorkflowName(state);

    //        //Assert
    //        Assert.AreEqual("TestWorkflow", workflowName);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(NoNullAllowedException))]
    //    public void EmptyNameThrowsNoNullAllowedException()
    //    {
    //        //Setup
    //        var logging = new Configuration(XmlResource.Fetch("NonEmptySettings")).Logging;
    //        logging.Workflows.First().ResourceName = string.Empty;
    //        ServerLogger.LoggingSettings = logging;
    //        var state = _dummyDebugProvider.GetDebugState();
    //        state.DisplayName = "";

    //        //Execute
    //        ServerLogger.LogDebug(GetAccount(), state);
    //    }

    //    private static NetworkAccount GetAccount()
    //    {
    //        return new NetworkAccount("Test", new byte[0]);
    //    }
    //}
}
