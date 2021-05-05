/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.DeleteRecords
{
    public class DeleteRecordsDesignerViewModel : ActivityDesignerViewModel
    {
        private readonly IShellViewModel _shellViewModel;

        public DeleteRecordsDesignerViewModel(ModelItem modelItem, IShellViewModel shellViewModel)
            : base(modelItem)
        {
            _shellViewModel = shellViewModel;
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Recordset_Delete;
        }

        [ExcludeFromCodeCoverage]
        public DeleteRecordsDesignerViewModel(ModelItem modelItem)
            : this(modelItem, CustomContainer.Get<IShellViewModel>())
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            _shellViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
