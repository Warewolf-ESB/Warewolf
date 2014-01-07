
namespace Dev2.Studio.Core.Models
{
    public class NormalDecisionType : DecisionType
    {
        public NormalDecisionType()
            : base("Normal")
        {
            OperatorTypes.Clear();
        }

        public override string GetExpression()
        {
            return "IsValid And Not HasError";
        }
    }
}
