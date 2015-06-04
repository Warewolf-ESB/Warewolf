using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Validation.Rules;

namespace Dev2.TO
{
    public class SharepointReadListTo : ValidatedObject, ISharepointReadListTo
    {
        int _indexNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SharepointReadListTo(string variableName, string fieldName)
        {
            FieldName = fieldName;
            VariableName = variableName;
        }

        public string FieldName { get; set; }
        public string VariableName { get; set; }
        public int IndexNumber { get { return _indexNumber; } set { OnPropertyChanged(ref _indexNumber, value); } }

        public bool CanRemove()
        {
            return false;
        }

        public bool CanAdd()
        {
            return false;
        }

        public void ClearRow()
        {
        }

        public bool Inserted { get; set; }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            return new RuleSet();
        }
    }
}