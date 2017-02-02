using System.Collections.Generic;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IFileChooser
    {
        void Cancel();
        void Save();

        ICommand CancelCommand { get; set; }
        ICommand SaveCommand { get; set; }
        IList<string> Attachments { get; set; }
        string DriveName { get; set; }
        string FilesTooltip { get; set; }
        string SelectTooltip { get; set; }
        List<string> GetAttachments();
        bool AllowMultipleSelection { get; set; }
    }
}
