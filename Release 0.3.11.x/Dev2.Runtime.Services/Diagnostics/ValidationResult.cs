using System.Collections;
using Newtonsoft.Json;

namespace Dev2.Runtime.Diagnostics
{
    public class ValidationResult
    {
        public ValidationResult()
        {
            IsValid = true;
            ErrorMessage = string.Empty;
            ErrorFields = new ArrayList();
            Result = string.Empty;
        }

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public ArrayList ErrorFields { get; set; }
        public string Result { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
