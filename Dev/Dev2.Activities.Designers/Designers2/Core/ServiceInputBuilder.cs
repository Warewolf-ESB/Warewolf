using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.DB;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Activities.Designers2.Core
{
    public class ServiceInputBuilder:IServiceInputBuilder
    {
        #region Implementation of IServiceInputBuilder

        public void GetValue(string s, List<IServiceInput> dt)
        {
            var exp = FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(s);
            if (exp.IsComplexExpression)
            {
                var item = ((LanguageAST.LanguageExpression.ComplexExpression)exp).Item;
                var vals = item.Where(a => a.IsRecordSetExpression || a.IsScalarExpression || a.IsJsonIdentifierExpression).Select(FsInteropFunctions.LanguageExpressionToString);
                dt.AddRange(vals.Select(a => new ServiceInput(a, "")));
            }
            if (exp.IsScalarExpression)
            {
                dt.Add(new ServiceInput(s, ""));
            }
            if (exp.IsRecordSetExpression)
            {
                dt.Add(new ServiceInput(s, ""));
            }
            if (exp.IsJsonIdentifierExpression)
            {
                dt.Add(new ServiceInput(s, ""));
            }
        }

        #endregion
    }
}