using Dev2.Common.Interfaces.Core.Convertors.Case;

namespace Dev2
{
    public static class CaseConverterFactory
    {
        /// <summary>
        /// Instantiates a concreate implementation of the ICaseConvertTO
        /// </summary>
        public static ICaseConvertTO CreateCaseConverterTO(string stringToConvert, string convertType, string result, int indexNumber)
        {
            return new CaseConvertTO(stringToConvert, convertType, result, indexNumber);
        }

        /// <summary>
        /// Instantiates a concreate implementation of the ICaseConverter
        /// </summary>
        public static ICaseConverter CreateCaseConverter()
        {
            return new CaseConverter();
        }

    }
}
