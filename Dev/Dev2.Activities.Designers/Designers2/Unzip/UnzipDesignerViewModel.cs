/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.Unzip
{
    public class UnzipDesignerViewModel : FileActivityDesignerViewModel
    {
        public UnzipDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "Zip Name", "Destination")
        {
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_File_Unzip;
        }

        public override void Validate()
        {
            Errors = null;
            var password = ArchivePassword;
            ValidateUserNameAndPassword();
            ValidateDestinationUsernameAndPassword();
            ValidateInputAndOutputPaths();
            ValidateArchivePassword(password, "Archive Password");
        }


        string ArchivePassword => GetProperty<string>();

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
