using System.Collections.Generic;
using Dev2.Interfaces;

namespace Dev2.Common.Interfaces.Core.Convertors.Case
{
    public interface ICaseConvertTO : IDev2TOFn
    {
        string StringToConvert { get; set; }
        string ConvertType { get; set; }
        IList<string> Expressions { get; set; }
        string ExpressionToConvert { get; set; }
        string Result { get; set; }        
        string WatermarkTextVariable { get; set; }
        
    }
}
