using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Script;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Activities.Designers.Tests.Script
{
    public class TestScriptDesignerViewModel : ScriptDesignerViewModel
    {
        public TestScriptDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }
        public enScriptType ScriptType { set { SetProperty(value); } get { return GetProperty<enScriptType>(); } }
    }
}