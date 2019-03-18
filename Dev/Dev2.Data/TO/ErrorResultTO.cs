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
