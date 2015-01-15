using System.Collections;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IDataExpression
    {
         
    }

    public interface IDatalistViewExpressionConvertor
    {
        IVariableListViewItem Create(IDataExpression expr);
    }
    public interface IWorkflowExressionList:IList<IDataExpression>{};
}