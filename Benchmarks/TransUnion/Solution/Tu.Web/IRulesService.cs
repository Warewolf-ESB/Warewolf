using System.ServiceModel;
using Tu.Rules;

namespace Tu
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRulesService" in both code and config file together.
    [ServiceContract]
    [ServiceKnownType(typeof(ValidationResult))]
    public interface IRulesService
    {
        [OperationContract]
        ValidationResult IsValid(string ruleName, string fieldName, object fieldValue);
    }
}
