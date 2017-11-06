using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Dev2.TO
{
    public class SharepointReadListTo : ValidatedObject, ISharepointReadListTo
    {
        int _indexNumber;

        public SharepointReadListTo()
        {
        }
        
        public SharepointReadListTo(string variableName, string fieldName, string internalName, string type)
        {
            FieldName = fieldName;
            VariableName = variableName;
            InternalName = internalName;
            Type = type;
        }

        public string InternalName { get; set; }

        public string FieldName { get; set; }
        [FindMissing]
        public string VariableName { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public int IndexNumber { get => _indexNumber; set => OnPropertyChanged(ref _indexNumber, value); }

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

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return FieldName;
        }

        public bool Inserted { get; set; }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            return new RuleSet();
        }
    }
}