using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Converters;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.BaseConvert
{
    public class BaseConvertDesignerViewModel : ActivityCollectionDesignerViewModel<BaseConvertTO>
    {
        public BaseConvertDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
           // AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();

            dynamic mi = ModelItem;
            InitializeItems(mi.ConvertCollection);

            ConvertTypes = Dev2EnumConverter.ConvertEnumsTypeToStringList<enDev2BaseConvertType>();
        }

        public IList<string> ConvertTypes { get; set; }

        public override string CollectionName { get { return "ConvertCollection"; } }
    }
}