using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class SelectItemInDeployMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SelectItemInDeployMessage(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
}