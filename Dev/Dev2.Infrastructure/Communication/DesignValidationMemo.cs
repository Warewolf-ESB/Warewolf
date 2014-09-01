using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Communication
{
    public class DesignValidationMemo : Memo
    {
        public DesignValidationMemo()
        {
            Errors = new List<IErrorInfo>();
            IsValid = true;
        }

        public string Source { get; set; }
        public string Type { get; set; }
        public Guid ServiceID { get; set; }
        public bool IsValid { get; set; }

        public List<IErrorInfo> Errors { get; set; }
    }
}