using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IEmailAttachmentView
    {
        void ShowView(IList<string> current);
        void RequestClose();
    }
}