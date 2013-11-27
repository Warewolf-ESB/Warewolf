using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Converters.DateAndTime.Interfaces
{
    public interface IDateTimeDiffTO
    {
        string Input1 { get; set; }

        string Input2 { get; set; }

        string InputFormat { get; set; }

        string OutputType { get; set; }
    }
}
