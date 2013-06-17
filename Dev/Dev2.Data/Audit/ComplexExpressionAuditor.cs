using System;
using System.Collections.Generic;

namespace Dev2.Data.Audit
{

    /// <summary>
    /// Used to store stepped audit data for complex expressions ;)
    /// </summary>
    [Serializable]
    public class ComplexExpressionAuditItem
    {
        public string Token { get; set; }

        public string TokenBinding { get; set; }

        public string Expression { get; set; }

        public int PassNumber { get; set; }

        public string BoundValue { get; set; }
    }

    [Serializable]
    public class ComplexExpressionAuditor
    {

        private IList<ComplexExpressionAuditItem> _auditItems = new List<ComplexExpressionAuditItem>();

        public void AddAuditStep(string currentExpression, string boundPart, string token, int pass, string boundValue)
        {
            _auditItems.Add(new ComplexExpressionAuditItem()
            {
                Expression = currentExpression,
                Token = token,
                TokenBinding = boundPart,
                PassNumber = pass,
                BoundValue = boundValue
            });
        }

        public IList<ComplexExpressionAuditItem> FetchAuditItems()
        {
            return _auditItems;
        } 
    }
}
