using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Data.Audit
{

    /// <summary>
    /// Used to store stepped audit data for complex expressions ;)
    /// </summary>
    [Serializable]
    public class ComplexExpressionAuditItem
    {

        public string RawExpression { get; set; }

        public string Token { get; set; }

        public string TokenBinding { get; set; }

        public string Expression { get; set; }

        public int PassNumber { get; set; }

        public string BoundValue { get; set; }

    }

    [Serializable]
    public class ComplexExpressionAuditor
    {

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private IList<ComplexExpressionAuditItem> _auditItems = new List<ComplexExpressionAuditItem>();
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        int _maxIndex;

        /// <summary>
        /// Adds the audit step.
        /// </summary>
        /// <param name="currentExpression">The current expression.</param>
        /// <param name="boundPart">The bound part.</param>
        /// <param name="token">The token.</param>
        /// <param name="pass">The pass.</param>
        /// <param name="boundValue">The bound value.</param>
        /// <param name="rawExpression">The raw expression.</param>
        public void AddAuditStep(string currentExpression, string boundPart, string token, int pass, string boundValue, string rawExpression)
        {
            _auditItems.Add(new ComplexExpressionAuditItem
            {
                Expression = currentExpression,
                Token = token,
                TokenBinding = boundPart,
                PassNumber = pass,
                BoundValue = boundValue,
                RawExpression = rawExpression,
            });
        }

        /// <summary>
        /// Fetches the audit items.
        /// </summary>
        /// <returns></returns>
        public IList<ComplexExpressionAuditItem> FetchAuditItems()
        {

            if(_auditItems.Any(c => c.PassNumber > 0))
            {
                // Now find the highest PassNumber
                var len = _auditItems.Count;
                var max = _auditItems[len - 1].PassNumber;

                // now find all the finally items we need to compile ;)
                var result = _auditItems.Where(c => c.PassNumber == max);

                return result.ToList();
            }

                // single pass generation, skip the compile phase ;)
                return _auditItems;
            }

        public void SetMaxIndex(int expIdx)
        {
            _maxIndex = expIdx;
        }

        public int GetMaxIndex()
        {
            return _maxIndex;
        }
    }
}
