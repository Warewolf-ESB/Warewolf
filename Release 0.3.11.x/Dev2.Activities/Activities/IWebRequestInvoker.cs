namespace Dev2.Activities
{
    public interface IWebRequestInvoker
    {
        string ExecuteRequest(string method, string url);
    }
}