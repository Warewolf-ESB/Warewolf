using System;
using System.Reflection;
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

            var asm = Assembly.GetExecutingAssembly();
            var asmName = asm.GetName();
            var ver = asmName.Version;

            dialogueViewModel.SetupDialogue(StringResources.About_Header_Text,
                                            String.Format(StringResources.About_Content, ver,
                                                          ver), packUri,
                                            StringResources.About_Description_Header, StringResources.EULA_Link, StringResources.EULA_Text);
            return dialogueViewModel;
        }
    }
}
