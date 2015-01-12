namespace Dev2.Common.Interfaces.Help
{
    public interface IHelpWindowModel
    {
        /// <summary>
        /// Fired when the user click on anything with associated help
        /// </summary>
        event HelpTextReceived OnHelpTextReceived;

        /// <summary>
        /// Send Help descriptor to the Help window
        /// </summary>
        /// <param name="descriptor"></param>
        void SendHelpDescriptor(IHelpDescriptor descriptor);
    }
}