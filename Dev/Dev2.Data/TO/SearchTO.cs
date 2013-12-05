using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        internal SearchTO(string fieldsToSearch, string searchType, string searchCriteria, string result)
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
