
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.ReadFolder
{
    public class ReadFolderDesignerViewModel : FileActivityDesignerViewModel
    {
        public ReadFolderDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "Directory", string.Empty)
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();

            if (!IsFilesAndFoldersSelected && !IsFoldersSelected && !IsFilesSelected)
            {
                IsFilesSelected = true;
            }
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateInputPath();
        }

        bool IsFilesAndFoldersSelected { set { SetProperty(value); } get { return GetProperty<bool>(); } }
        bool IsFoldersSelected { set { SetProperty(value); } get { return GetProperty<bool>(); } }
        bool IsFilesSelected { set { SetProperty(value); } get { return GetProperty<bool>(); } }
    }
}
