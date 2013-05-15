using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.Services.Communication.Mapi
{
    public interface IMailMessage
    {
        RecipientCollection Recipients { get; }
        ArrayList Files { get; }
        void ShowDialog();
    }

}
