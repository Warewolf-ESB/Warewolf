/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Dev2.Activities.Factories.Case
{
    public static class CaseConverter
    {
        #region Class Members

        static readonly Dictionary<string, Func<string, string>> _convertFunctions =
            new Dictionary<string, Func<string, string>>();

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

        #endregion Methods

        #region Private Methods

        /// <summary>
        ///     Creates a list of all valid Convert Types
        /// </summary>
        static void CreateConvertFormatTypes()
        {
            _convertFunctions.Add("UPPER", ConvertToAllUpper);
            _convertFunctions.Add("lower", ConvertToAllLower);
            _convertFunctions.Add("Sentence", ConvertToFirstUpper);
            _convertFunctions.Add("Title Case", ConvertToAllFirstUpper);
            ConvertTypes = new List<string>(_convertFunctions.Keys);
        }
        public static Dictionary<string, Func<string, string>> GetFuncs()
        {
            var convertFunctions = new Dictionary<string, Func<string, string>>();
            convertFunctions.Add("UPPER", ConvertToAllUpper);
            convertFunctions.Add("lower", ConvertToAllLower);
            convertFunctions.Add("Sentence", ConvertToFirstUpper);
            convertFunctions.Add("Title Case", ConvertToAllFirstUpper);
            return convertFunctions;
        }
        /// <summary>
        ///     Make the first letter of the word to upper case
        /// </summary>
        static string MakeFirstLetterUpper(string word)
        {
            // start by converting entire string to lower case
            var lowerCase = word.ToLower();
            // matches the first sentence of a string, as well as subsequent sentences
            //Juries Bug 8725
            var reg = new Regex(@"(?<=(^|[.;:])\s*)[a-z]", RegexOptions.Compiled | RegexOptions.Multiline);
            // MatchEvaluator delegate defines replacement of setence starts to uppercase        
            return reg.Replace(lowerCase, s => s.Value.ToUpper());
        }

        #endregion Private Methods

        #region Convert Methods

        static string ConvertToAllUpper(string stringToConvert)
        {
            return stringToConvert.ToUpper();
        }

        static string ConvertToAllLower(string stringToConvert)
        {
            return stringToConvert.ToLower();
        }

        static string ConvertToFirstUpper(string stringToConvert)
        {
            return MakeFirstLetterUpper(stringToConvert);
        }

        static string ConvertToAllFirstUpper(string stringToConvert)
        {
            var txInfo = CultureInfo.CurrentCulture.TextInfo;
            var str = txInfo.ToTitleCase(stringToConvert);
            return str;
        }

        #endregion Convert Methods
    }
}