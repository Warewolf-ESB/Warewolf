// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models
{
    public class BetweenOperatorType : OperatorType
    {
        public BetweenOperatorType()
        {

        }

        public BetweenOperatorType(string operatorName, string friendlyName, string operatorSymbol, DecisionType parent)
            : base(operatorName, friendlyName, operatorSymbol, parent)
        {

        }
    }
}
