using System.Collections.Generic;

namespace Dev2.Runtime.Diagnostics
{
    public class DatabaseValidationResult:ValidationResult
    {
        //PBI 8720
        public IList<string> DatabaseList { get; set; }
    }
}
