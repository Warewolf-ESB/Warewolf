using System.ComponentModel;

namespace Dev2.Interfaces
{
    public interface IDev2TOFn:INotifyPropertyChanged
    {
        bool CanRemove();
        bool CanAdd();
        int IndexNumber { get; set; }
        void ClearRow();
        bool Inserted { get; set; }
    }
}
