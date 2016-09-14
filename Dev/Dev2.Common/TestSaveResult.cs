namespace Dev2.Common
{
    public class TestSaveResult
    {
        public SaveResult Result { get; set; }
        public string Message { get; set; } 
    }

    public class TestRunReuslt
    {
        public string TestName { get; set; }
        public RunResult Result { get; set; }
        public string Message { get; set; }
    }

    public enum RunResult
    {
        TestPassed,
        TestFailed,
        TestResourceDeleted,
        TestResourcePathUpdated
    }
}