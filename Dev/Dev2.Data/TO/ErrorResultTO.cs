#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data.TO;
using Warewolf.Data;
using Warewolf.Resource.Errors;

namespace Dev2.Data.TO
{
    [Serializable]
    public sealed class ErrorResultTO : IErrorResultTO
    {
        readonly HashSet<string> _errorList = new HashSet<string>();

        public void AddError(string msg) => AddError(msg, false);
        public void AddError(string msg, bool checkForDuplicates) => AddError(msg, checkForDuplicates, false);
        public void AddError(string msg, bool checkForDuplicates, bool checkForInnerErrorDuplicates)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var isErrorDuplicate = checkForDuplicates && !IsErrorDuplicate(msg);

                if (!string.IsNullOrEmpty(msg) && (isErrorDuplicate || !checkForDuplicates))
                {
                    _errorList.Add(msg);
                }
            }
        }

        bool IsErrorDuplicate(string msg)
        {
            return _errorList.Any(o => o.Contains(msg));
        }

        public void RemoveError(string msg)
        {
            var found = _errorList.FirstOrDefault(s => s == msg);
            if (found != null)
            {
                _errorList.Remove(found);
            }
        }

        public IList<string> FetchErrors() => _errorList.ToList();

        public bool HasErrors() => _errorList.Count > 0;

        public void MergeErrors(IErrorResultTO toMerge)
        {
            if (toMerge != null && toMerge.HasErrors())
            {
                foreach (string wtf in toMerge.FetchErrors())
                {
                    _errorList.Add(wtf);
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

            foreach (string e in _errorList)
            {
                result.Append(e);
                if (_errorList.ToList().IndexOf(e) + 1 < _errorList.Count)
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
                result.Append("[ ");
            }

            var errCnt = 0;
            foreach (string e in _errorList)
            {
                var formattedMsg = FormatErrorMessage(e);
                if (asXml)
                {
                    result.Append(GlobalConstants.InnerErrorTag);
                    result.Append(formattedMsg);
                    result.Append(GlobalConstants.InnerErrorTagEnd);
                }
                else
                {
                    result.Append("\"");
                    result.Append(formattedMsg.Replace("\\", "\\\\").Replace("\"", "\\\""));
                    result.Append("\"");

                    errCnt++;
                    if (errCnt < _errorList.Count)
                    {
                        result.Append(",");
                    }
                }
            }

            if (!asXml)
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
        public static ErrorResultTO MakeErrorResultFromDataListString(string errorsString) => MakeErrorResultFromDataListString(errorsString, false);
        public static ErrorResultTO MakeErrorResultFromDataListString(string errorsString, bool checkForDuplicates)
        {
            var result = new ErrorResultTO();
           
            if (!string.IsNullOrEmpty(errorsString))
            {
                errorsString = string.Concat("<Error>", errorsString, "</Error>");
                if (errorsString.IsXElement(out XElement output))
                {
                    foreach (XElement element in output.Elements("InnerError"))
                    {
                        result.AddError(element.Value, checkForDuplicates);
                    }
                }
                else
                {
                    result.AddError(errorsString, checkForDuplicates);
                }
            }
            return result;
        }
    }
}
