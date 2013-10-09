using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.TO;

namespace Dev2.Activities.Designers2.SqlBulkInsert
{
    public class SqlBulkInsertDesignerViewModel : ActivityCollectionDesignerViewModel<SqlBulkInsertTO>
    {
        public SqlBulkInsertDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();

            dynamic mi = ModelItem;
            ModelItemCollection = mi.InputMappings;
        }

        public override string CollectionName { get { return "InputMappings"; } }
    }
}