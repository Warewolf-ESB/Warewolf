namespace Dev2.Common.Interfaces.Core.Convertors.DateAndTime
{
    public interface IDateTimeFormatter
    {
        bool TryFormat(IDateTimeOperationTO dateTimeTO, out string result, out string error);
    }
}
