using System.ComponentModel;

namespace Dev2.Common.Interfaces
{

    public interface INameValue : INotifyPropertyChanged
    {
        string Name { get; set; }
        string Value { get; set; }

    }
}
