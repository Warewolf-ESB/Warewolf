
namespace Dev2.Common.Interfaces.Core.Convertors.DateAndTime
{
    public interface IDateTimeComparer
    {
        bool TryCompare(IDateTimeDiffTO dateTimeDiffTo, out string result, out string error);
    }
}
