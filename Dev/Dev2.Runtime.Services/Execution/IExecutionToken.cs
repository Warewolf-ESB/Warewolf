namespace Dev2.Runtime.Execution
{
    public interface IExecutionToken
    {

        bool IsUserCanceled { get; set; }
    }
}
