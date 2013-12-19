using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.XPath
{
    public class XPathDesignerViewModel : ActivityCollectionDesignerViewModel<XPathDTO>
    {
        public XPathDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);
        }
        public override string CollectionName { get { return "ResultsCollection"; } }
    }
}