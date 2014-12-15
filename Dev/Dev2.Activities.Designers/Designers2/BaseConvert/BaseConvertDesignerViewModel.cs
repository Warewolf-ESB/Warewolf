
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
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Converters;

namespace Dev2.Activities.Designers2.BaseConvert
{
    public class BaseConvertDesignerViewModel : ActivityCollectionDesignerViewModel<BaseConvertTO>
    {
        public BaseConvertDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();

            dynamic mi = ModelItem;
            InitializeItems(mi.ConvertCollection);

            ConvertTypes = Dev2EnumConverter.ConvertEnumsTypeToStringList<enDev2BaseConvertType>();
        }

        public IList<string> ConvertTypes { get; set; }

        public override string CollectionName { get { return "ConvertCollection"; } }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            yield break;
        }
    }
}
