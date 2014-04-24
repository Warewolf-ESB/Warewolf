namespace Dev2
{
    public interface IApplicationMessage
    {
        void SendMessage(string message);
        event MessageEventHandler MessageReceived;
    }
}
