using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.PathOperations;

namespace Dev2.Converters.DateAndTime.Interfaces
{
    public interface IDateTimeOperationTO : IResult
    {
        string InputFormat { get; set; }
        string OutputFormat { get; set; }
        string DateTime { get; set; }
        //2012.09.27: massimo.guerrera - Added for the new fucntionality for the time modification
        string TimeModifierType { get; set; }
        int TimeModifierAmount { get; set; }
    }
}
