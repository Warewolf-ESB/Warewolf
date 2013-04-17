using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Studio.Core.Models
{
    public abstract class DecisionType : IDecisionType
    {


        #region Constructors
        public DecisionType()
        {

        }

        public DecisionType(string decisionTypeName)
            : this()
        {
            this.DecisionTypeName = decisionTypeName;


            OperatorTypes = new List<OperatorType> { 
                        new OperatorType("t.Eq", "Is Equal To",  "{0}({1},{3}{2}{3})", this),
                        new OperatorType("t.NtEq", "Is Not Equal To",  "{0}({1},{3}{2}{3})",this),
                        new OperatorType("t.LsTh", "Is Less Than", "{0}({1},{3}{2}{3})",this),
                        new OperatorType("t.GrTh", "Is Greater Than", "{0}({1},{3}{2}{3})",this),
                        new OperatorType("t.GrThEq", "Is Greater Than Or Equal To","{0}({1},{3}{2}{3})",this),
                        new OperatorType("t.LsThEq","Is Less Than Or Equal To", "{0}({1},{3}{2}{3})",this),
                        new BetweenOperatorType("t.Btw","Is Between", "{0}({1},{3}{2}{3},{3}{4}{3})",this)};
        }

        #endregion

        #region Properties
        public virtual string DecisionTypeName { get; set; }

        public List<OperatorType> OperatorTypes
        {
            get;
            set;

        }
        public virtual string StringDecorator
        {
            get
            {
                return "\"";
            }
        }
        public virtual string FunctionName
        {
            get
            {
                return string.Empty;
            }
        }
        public bool IsValid
        {
            get
            {
                var operations = OperatorTypes.Where(c => c.Selected);
                if (operations.Count() >= 1)
                {
                    return operations.FirstOrDefault().IsValid;
                }
                return false;
            }
        }


        public virtual string GetExpression()
        {
            var operations = OperatorTypes.Where(c => c.Selected);
            if (operations.Count() >= 1)
            {
                var operation = operations.FirstOrDefault();
                return BuildStringExpression(operation.OperatorName, this.StringDecorator, operation.Expression, operation.OperatorSymbol, operation.Value, operation.EndValue);

            }
            else
            {
                return string.Empty;
            }
        }

        public string BuildStringExpression(string functionName, string decorator, string expression, string op, object value, object endvalue)
        {

            if (endvalue != null)
            {
                return string.Format(op, functionName, expression, value == null ? string.Empty : value.ToString(), decorator, endvalue == null ? string.Empty : endvalue.ToString());
            }

            return string.Format(op, functionName, expression, value == null ? string.Empty : value.ToString(), decorator);
        }

        #endregion
    }
}
