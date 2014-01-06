
namespace Dev2.Converters.DateAndTime.Interfaces
{
    public interface IDateTimeComparer
    {
        bool TryCompare(IDateTimeDiffTO dateTimeTO, out string result, out string error);
    }
}
