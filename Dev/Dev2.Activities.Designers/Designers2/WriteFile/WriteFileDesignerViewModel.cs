#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.WriteFile
{
    [Obsolete("DsfFileWrite is deprecated. It will be deleted in future releases along with this viewModel.")]
    public class WriteFileDesignerViewModel : FileActivityDesignerViewModel
    {
        readonly ModelItem Modelitem;

        public WriteFileDesignerViewModel(ModelItem modelItem)
            : base(modelItem, string.Empty, "File Name")
        {
            AddTitleBarLargeToggle();

            if (!Overwrite && !AppendTop && !AppendBottom)
            {
                Overwrite = true;
            }

            Modelitem = modelItem;
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_File_Write_File;
        }

        public override void Validate()
        {
            Errors = null;
            var content = FileContents;
            ValidateUserNameAndPassword();
            ValidateOutputPath();
            ValidateFileContent(content, "Contents");
            
        }

        string FileContents => GetProperty<string>();

        bool Overwrite { set => SetProperty(value); get => GetProperty<bool>(); }
        bool AppendTop => GetProperty<bool>();

        bool AppendBottom => GetProperty<bool>();

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
