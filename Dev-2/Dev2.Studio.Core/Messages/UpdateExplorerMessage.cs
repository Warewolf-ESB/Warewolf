namespace Dev2.Studio.Core.Messages
{
    public class UpdateExplorerMessage:IMessage
    {
        public bool Update { get; set; }

        public UpdateExplorerMessage(bool update)
        {
            Update = update;
        }
    }
}