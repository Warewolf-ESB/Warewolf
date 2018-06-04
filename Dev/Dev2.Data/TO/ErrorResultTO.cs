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
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data.TO;
using Warewolf.Resource.Errors;
using Dev2.Common.Common;
using System.Linq;

namespace Dev2.Data.TO
{
    [Serializable]
    public class ErrorResultTO : IErrorResultTO
    {
        readonly IList<StringBuilder> _errorList = new List<StringBuilder>();
        
        public void AddError(string msg) => AddError(msg, false);
        public void AddError(string msg, bool checkForDuplicates)
        {
            if (!string.IsNullOrEmpty(msg) && (checkForDuplicates && !_errorList.Contains(msg.ToStringBuilder()) || !checkForDuplicates))
            {
                _errorList.Add(msg.ToStringBuilder());
            }

        }
        
        public void RemoveError(string msg)
        {
            var found = _errorList.FirstOrDefault(s => s.ToString() == msg);
            if (found != null)
            {
                _errorList.Remove(found);
            }
        }
        
        public IList<string> FetchErrors() => _errorList.Select(e => e.ToString()).ToList();
        
        public bool HasErrors() => _errorList.Count > 0;
        
        public void MergeErrors(IErrorResultTO toMerge)
        {
            if (toMerge != null && toMerge.HasErrors())
            {
                // Flipping Union does not appear to work
                foreach (string wtf in toMerge.FetchErrors())
                {
                    _errorList.Add(wtf.ToStringBuilder());
                }

                toMerge.ClearErrors();
            }
        }
        public void ClearErrors()
        {
            _errorList.Clear();
        }
        
        public string MakeDisplayReady()
        {
            var result = new StringBuilder();

            foreach(StringBuilder e in _errorList)
            {
                result.Append(e);
                if(_errorList.IndexOf(e) + 1 < _errorList.Count)
                {
                    result.AppendLine();
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Makes the error collection data list insert ready.
        /// </summary>
        /// <returns></returns>
        public string MakeDataListReady() => MakeDataListReady(true);
        public string MakeDataListReady(bool asXml)
        {
            var result = new StringBuilder();

            if (!asXml)
            {
                result.Append("\"errors\": [ ");
            }

            var errCnt = 0;
            foreach (StringBuilder e in _errorList)
            {
                var formattedMsg = FormatErrorMessage(e.ToString());
                if(asXml)
                {
                    result.Append(GlobalConstants.InnerErrorTag);
                    result.Append(formattedMsg);
                    result.Append(GlobalConstants.InnerErrorTagEnd);
                }
                else
                {
                    // we want JSON ;)
                    result.Append("\"");
                    result.Append(formattedMsg);
                    result.Append("\"");

                    errCnt++;
                    if(errCnt < _errorList.Count)
                    {
                        result.Append(",");
                    }
                }
            }

            if(!asXml)
            {
                result.Append("]");
            }

            return result.ToString();
        }

        string FormatErrorMessage(string s)
        {
            if (s.Contains("Cannot set unknown member"))
            {
                return ErrorResource.ResourceHasUnrecognizedFormatting;
            }
            return s;
        }

        /// <summary>
        /// Makes ErrorResultTO from a error string from the data list.
        /// </summary>
        /// <param name="errorsString">Error string to convert</param>
        /// <returns>ErrorResultsTO</returns>
        public static ErrorResultTO MakeErrorResultFromDataListString(string errorsString)
        {
            var result = new ErrorResultTO();
            try
            {
                if(!string.IsNullOrEmpty(errorsString))
                {
                    errorsString = string.Concat("<Error>", errorsString, "</Error>");
                    var errorNode = XElement.Parse(errorsString);
                    foreach (XElement element in errorNode.Elements("InnerError"))
                    {
                        result.AddError(element.Value);
                    }
                }
            }
            catch(Exception)
            {
                result.AddError(errorsString);
            }
            return result;
        }



    }
}
