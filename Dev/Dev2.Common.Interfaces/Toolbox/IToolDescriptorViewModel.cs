namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolDescriptorViewModel
    {
        IToolDescriptor Tool { get; }
        bool IsEnabled { get; }
    }
}
