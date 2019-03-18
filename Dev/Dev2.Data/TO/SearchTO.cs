#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    /// <summary>
    /// Class for holding all the properties that are needed by the find records operation
    /// </summary>
    public class SearchTO : IRecsetSearch
    {
        /// <summary>
        /// Internal constructor, must be created from the DTOFactory
        /// </summary>
        public SearchTO(string fieldsToSearch, string searchType, string searchCriteria, string result)
            : this(fieldsToSearch, searchType, searchCriteria, string.Empty, result,false,string.Empty,string.Empty)
        {

        }
        /// <summary>
        /// Internal constructor, must be created from the DTOFactory
        /// </summary>
        internal SearchTO(string fieldsToSearch, string searchType, string searchCriteria, string startIndex, string result, bool matchCase,string from ,string to , bool requireAllFieldsToMatch=false)
        {
            FieldsToSearch = fieldsToSearch;
            SearchType = searchType;
            SearchCriteria = searchCriteria;
            StartIndex = startIndex;
            Result = result;
            MatchCase = matchCase;
            RequireAllFieldsToMatch = requireAllFieldsToMatch;
            From = from;
            To = to;
        }

        public string From { get; set; }

        public string To { get; set; }

        public bool RequireAllFieldsToMatch { get; set; }

        /// <summary>
        /// Property for holding the fields to search in.
        /// </summary>
        public string FieldsToSearch { get; set; }

        /// <summary>
        /// Property for holding the type of search the user would like carryed out
        /// </summary>
        public string SearchType { get; set; }

        /// <summary>
        /// Property for holding the text that the user would like to search for
        /// </summary>
        public string SearchCriteria { get; set; }

        /// <summary>
        /// Property for holding the start index that the user wants to start the search on
        /// </summary>
        public string StartIndex { get; set; }

        /// <summary>
        /// Property holding the data list region that the user wants the results of the operation to go into
        /// </summary>
        public string Result { get; set; }
        
        /// <summary>
        /// Property holding if the search must match case
        /// </summary>
        public bool MatchCase { get; set; }
    }
}
