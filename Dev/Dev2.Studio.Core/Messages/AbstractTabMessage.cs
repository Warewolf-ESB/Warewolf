
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public abstract class AbstractTabMessage : IMessage
    {
        protected AbstractTabMessage(object context)
        {
            Context = context;
        }

        public object Context { get; set; }
    }
}
