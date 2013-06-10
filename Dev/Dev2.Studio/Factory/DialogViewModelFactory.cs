using System;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Administration;

namespace Dev2.Studio.Factory
{
    public static class DialogViewModelFactory
    {
        public static IDialogueViewModel CreateAboutDialog()
        {
            IDialogueViewModel dialogueViewModel = new DialogueViewModel();
            string packUri = StringResources.Warewolf_Logo;

            dialogueViewModel.SetupDialogue(StringResources.About_Header_Text,
                                            String.Format(StringResources.About_Content, StringResources.CurrentVersion,
                                                          StringResources.CurrentVersion), packUri,
                                            StringResources.About_Description_Header, StringResources.EULA_Link, StringResources.EULA_Text);
            return dialogueViewModel;
        }
    }
}
