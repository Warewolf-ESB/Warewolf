
namespace Dev2.Common.Interfaces.Core.Convertors.DateAndTime
{
    public interface IDateTimeDiffTO
    {
        string Input1 { get; set; }

        string Input2 { get; set; }

        string InputFormat { get; set; }

        string OutputType { get; set; }
    }
}
