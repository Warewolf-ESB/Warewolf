using System.Collections;

namespace Dev2.Studio.Core.Services.Communication.Mapi
{
    public interface IMailMessage
    {
        RecipientCollection Recipients { get; }
        ArrayList Files { get; }
        void ShowDialog();
    }
}
