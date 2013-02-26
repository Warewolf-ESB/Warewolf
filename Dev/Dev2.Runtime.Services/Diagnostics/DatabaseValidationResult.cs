using System.Collections.Generic;

namespace Dev2.Runtime.Diagnostics
{
    public class DatabaseValidationResult:ValidationResult
    {
        //PBI 8720
        public List<string> DatabaseList { get; set; }
    }
}
