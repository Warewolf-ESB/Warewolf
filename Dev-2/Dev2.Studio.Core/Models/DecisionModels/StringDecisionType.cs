namespace Dev2.Studio.Core.Models {
    public class StringDecisionType : DecisionType {
        public StringDecisionType() : base("Text") {

            OperatorTypes.RemoveAll(c => c.OperatorName.Contains("Gr") || c.OperatorName.Contains("Ls") || c.OperatorName.Contains("Btw"));

            OperatorTypes.Add(new OperatorType("t.Cnt", "Contains" ,"{0}({1},{3}{2}{3})", this));
            OperatorTypes.Add(new OperatorType("t.EnWt", "Ends With", "{0}({1},{3}{2}{3})", this));
            OperatorTypes.Add(new OperatorType("t.StWt", "Starts With", "{0}({1},{3}{2}{3})", this));
            OperatorTypes.Add(new OperatorType("t.IsEmpty", "Is Empty", "{0}({1})", this, false));
        }


    }
}
