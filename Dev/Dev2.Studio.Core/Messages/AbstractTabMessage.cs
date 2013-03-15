
namespace Dev2.Studio.Core.Messages
{
    public abstract class AbstractTabMessage : IMessage
    {
        public AbstractTabMessage(object context)
        {
            Context = context;
        }

        public object Context { get; set; }
    }
}
