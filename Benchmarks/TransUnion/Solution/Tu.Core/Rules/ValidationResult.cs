
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tu.Rules
{
    [DataContract]
    public class ValidationResult
    {
        public ValidationResult()
        {
            Errors = new List<string>();
        }

        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public List<string> Errors { get; set; }
    }
}
