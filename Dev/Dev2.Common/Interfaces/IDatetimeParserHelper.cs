namespace Dev2.Common.Interfaces
{
    internal interface IDatetimeParserHelper
    {
        bool IsNumberWeekOfYear(string data, bool treatAsTime);
        bool IsNumberDayOfYear(string data, bool treatAsTime);
        bool IsNumberDayOfWeek(string data, bool treatAsTime);
        bool IsNumberDay(string data, bool treatAsTime);
        bool IsNumberMonth(string data, bool treatAsTime);
        bool IsNumber12H(string data, bool treatAsTime);
        bool IsNumber24H(string data, bool treatAsTime);
        bool IsNumberMinutes(string data, bool treatAsTime);
        bool IsNumberMilliseconds(string data, bool treatAsTime);
        bool IsNumberSeconds(string data, bool treatAsTime);
    }
}