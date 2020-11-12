namespace Dev2.Common.Interfaces.Help
{
    public interface IHelpDescriptor
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Help text
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Icon
        /// </summary>
        object Icon { get; }
    }

    public delegate  void HelpTextReceived(object sender,IHelpDescriptor desc);

    public  interface  IHelpWindowViewModel
    {
        /// <summary>
        /// Wpf component binds here
        /// </summary>
        IHelpDescriptorViewModel CurrentHelpText { get; set; }
        bool WebPageVisible { get; set; }

        void UpdateHelpText(string helpText);
    }
}