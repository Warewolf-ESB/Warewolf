namespace Dev2.Studio.Core.Messages
{
    public class AddDeployResourcesMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public AddDeployResourcesMessage(object model)
        {
            Model = model;
        }

        public object Model { get; set; }
    }
}