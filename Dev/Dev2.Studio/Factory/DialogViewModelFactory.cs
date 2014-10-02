
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Administration;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Factory
{
    public static class DialogViewModelFactory
    {
        public static IDialogueViewModel CreateAboutDialog()
        {
            IDialogueViewModel dialogueViewModel = new DialogueViewModel();
            string packUri = StringResources.Warewolf_Logo;

            var ver = VersionInfo.FetchVersionInfo();

            dialogueViewModel.SetupDialogue(StringResources.About_Header_Text,
                                            String.Format(StringResources.About_Content, ver,
                                                          ver), packUri,
                                            StringResources.About_Description_Header, StringResources.EULA_Link, StringResources.EULA_Text);
            return dialogueViewModel;
        }
    }
}
