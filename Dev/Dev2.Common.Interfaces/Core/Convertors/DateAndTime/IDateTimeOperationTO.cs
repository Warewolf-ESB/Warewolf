using Dev2.Common.Interfaces.PathOperations;

namespace Dev2.Common.Interfaces.Core.Convertors.DateAndTime
{
    public interface IDateTimeOperationTO : IResult
    {
        string InputFormat { get; set; }
        string OutputFormat { get; set; }
        string DateTime { get; set; }
        //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
        string TimeModifierType { get; set; }
        int TimeModifierAmount { get; set; }
    }
}
