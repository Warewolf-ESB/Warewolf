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
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Data.Enums;
using Dev2.Interfaces;

namespace Dev2.Activities.Designers2.GatherSystemInformation
{
    public class GatherSystemInformationDesignerViewModel : ActivityCollectionDesignerViewModel<GatherSystemInformationTO>
    {
        public IList<string> ItemsList { get; private set; }

        public GatherSystemInformationDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarLargeToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.SystemInformationCollection);

            if (mi.SystemInformationCollection == null || mi.SystemInformationCollection.Count <= 0)
            {
                mi.SystemInformationCollection.Add(new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, string.Empty, 1));
                mi.SystemInformationCollection.Add(new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, string.Empty, 2));
            }

            ItemsList = Dev2EnumConverter.ConvertEnumsTypeToStringList<enTypeOfSystemInformationToGather>();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Utility_Sys_Info;
        }
        public override string CollectionName => "SystemInformationCollection";

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            yield break;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
