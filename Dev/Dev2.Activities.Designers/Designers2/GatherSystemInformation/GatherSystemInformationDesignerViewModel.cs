using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Data.Enums;

namespace Dev2.Activities.Designers2.GatherSystemInformation
{
    public class GatherSystemInformationDesignerViewModel : ActivityCollectionDesignerViewModel<GatherSystemInformationTO>
    {
        public IList<string> ItemsList { get; private set; }

        public GatherSystemInformationDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.SystemInformationCollection);
            ItemsList = Dev2EnumConverter.ConvertEnumsTypeToStringList<enTypeOfSystemInformationToGather>();
        }
        public override string CollectionName { get { return "SystemInformationCollection"; } }
    }
}