using System;
using System.Windows;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolDescriptorViewModel
    {
        IToolDescriptor Tool { get; }
        bool IsEnabled { get; }
        IWarewolfType Designer { get; }
        IWarewolfType Activity { get; }
        DataObject ActivityType { get; }
    }
}
