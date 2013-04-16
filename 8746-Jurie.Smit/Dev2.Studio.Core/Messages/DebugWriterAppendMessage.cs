namespace Dev2.Studio.Core.Messages
{
    public class DebugWriterAppendMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DebugWriterAppendMessage(object content)
        {
            Content = content;
        }

        public object Content { get; set; }
    }
}