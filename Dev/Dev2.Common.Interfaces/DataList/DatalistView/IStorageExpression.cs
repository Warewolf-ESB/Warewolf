using System.Collections;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IDataExpression
    {
         
    }

    public interface IDatalistViewExpressionConvertor
    {
        IDataListViewItem Create(IDataExpression expr);
    }
    public interface IWorkflowExressionList:IList<IDataExpression>{};
}