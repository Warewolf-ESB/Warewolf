using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Converters.DateAndTime
{
    public class DateTimeDiffTO : IDateTimeDiffTO
    {
        #region Ctor

        public DateTimeDiffTO(string input1, string input2, string inputFormat, string outputType)
        {
            Input1 = input1;
            Input2 = input2;
            InputFormat = inputFormat;
            OutputType = outputType;
        }

        #endregion Ctor

        #region Properties

        public string Input1 { get; set; }

        public string Input2 { get; set; }

        public string InputFormat { get; set; }

        public string OutputType { get; set; }

        #endregion Properties

    }
}
