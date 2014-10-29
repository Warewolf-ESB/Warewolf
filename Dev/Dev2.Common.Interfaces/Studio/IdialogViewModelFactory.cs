using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Common.Interfaces.Studio
{
    public interface IDialogViewModelFactory
    {
        IDialogueViewModel CreateAboutDialog();
         IDialogueViewModel CreateServerAboutDialog(string serverVersion);
    } 
}

