using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class SelectItemInDeployMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SelectItemInDeployMessage(string displayName, IEnvironmentModel environment)
        {
            Environment = environment;
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }

        public IEnvironmentModel Environment { get; set; }
    }
}