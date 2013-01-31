using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Common;

namespace Dev2
{
    public class CaseConverter :ICaseConverter
    {
        #region Class Members

        private readonly static Dictionary<string, Func<string, string>> _convertFunctions = new Dictionary<string, Func<string, string>>();

        #endregion Class Members

        #region Properties

        public static List<string> ConvertTypes { get; private set; }

        #endregion Properties

        #region Ctor
        static CaseConverter()
        {
            CreateConvertFormatTypes();
        }
        #endregion Ctor

        #region Methods

        public IBinaryDataListItem TryConvert(string conversionType, IBinaryDataListItem item) {

            Func<string, string> returnedFunc;
            IBinaryDataListItem result = Dev2BinaryDataListFactory.CreateBinaryItem("Error Invalid Conversion Type", GlobalConstants.EvalautionScalar);
            if (_convertFunctions.TryGetValue(conversionType, out returnedFunc)) {
                if (returnedFunc != null) {
                    string tmp = returnedFunc.Invoke(item.TheValue);
                    if (item.Namespace != string.Empty) {
                        result = Dev2BinaryDataListFactory.CreateBinaryItem(tmp, item.Namespace, item.FieldName, item.ItemCollectionIndex);
                    } else {
                        result = Dev2BinaryDataListFactory.CreateBinaryItem(tmp, item.FieldName);
                    }
                }
            } 

            return result;
        }
        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Creates a list of all valid Convert Types
        /// </summary>
        private static void CreateConvertFormatTypes()
        {
            _convertFunctions.Add("UPPER", ConvertToAllUpper);
            _convertFunctions.Add("lower", ConvertToAllLower);
            _convertFunctions.Add("Sentence", ConvertToFirstUpper);
            _convertFunctions.Add("Title Case", ConvertToAllFirstUpper);
            ConvertTypes = new List<string>(_convertFunctions.Keys);
        }

        /// <summary>
        /// Make the first letter of the word to upper case
        /// </summary>
        private static string MakeFirstLetterUpper(string word)
        {               
            // start by converting entire string to lower case
            string lowerCase = word.ToLower();
            // matches the first sentence of a string, as well as subsequent sentences
            Regex reg = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
            // MatchEvaluator delegate defines replacement of setence starts to uppercase        
            return reg.Replace(lowerCase, s => s.Value.ToUpper());
        }

        #endregion Private Methods

        #region Convert Methods

        private static string ConvertToAllUpper(string stringToConvert)
        {
            return stringToConvert.ToUpper();
        }

        private static string ConvertToAllLower(string stringToConvert)
        {
            return stringToConvert.ToLower();
        }

        private static string ConvertToFirstUpper(string stringToConvert)
        {            
            return MakeFirstLetterUpper(stringToConvert);
        }

        private static string ConvertToAllFirstUpper(string stringToConvert)
        {
            string result = string.Empty;
            TextInfo txInfo = CultureInfo.CurrentCulture.TextInfo;
            
            txInfo.ToTitleCase(stringToConvert);
            return txInfo.ToTitleCase(stringToConvert);
        }

        #endregion Convert Methods
    }
}
