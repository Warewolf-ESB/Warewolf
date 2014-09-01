using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data.TO;

namespace Dev2.DataList.Contract
{
    [Serializable]
    public class ErrorResultTO : IErrorResultTO
    {

        private readonly IList<string> _errorList = new List<string>();

        /// <summary>
        /// Adds the error.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="checkForDuplicates"></param>
        public void AddError(string msg, bool checkForDuplicates = false)
        {
            if(!string.IsNullOrEmpty(msg))
            {
                if((checkForDuplicates && !_errorList.Contains(msg)) || !checkForDuplicates)
                {
                    _errorList.Add(msg);
                }
            }
        }

        /// <summary>
        /// Remove the error from the list
        /// </summary>
        /// <param name="msg"></param>
        public void RemoveError(string msg)
        {
            _errorList.Remove(msg);
        }

        /// <summary>
        /// Fetches the errors.
        /// </summary>
        /// <returns></returns>
        public IList<string> FetchErrors()
        {
            return _errorList;
        }

        /// <summary>
        /// Determines whether this instance has errors.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </returns>
        public bool HasErrors()
        {
            return (_errorList.Count > 0);
        }

        /// <summary>
        /// Merges the errors.
        /// </summary>
        /// <param name="toMerge">To merge.</param>
        public void MergeErrors(IErrorResultTO toMerge)
        {
            if(toMerge != null && toMerge.HasErrors())
            {
                // Flipping Union does not appear to work
                foreach(string wtf in toMerge.FetchErrors())
                {
                    _errorList.Add(wtf);
                }

                toMerge.ClearErrors();
            }
        }
        /// <summary>
        /// Merges the errors.
        /// </summary>
        /// <param name="toMerge">To merge.</param>
        public void MergeErrors(ErrorResultTO toMerge)
        {
            if (toMerge != null && toMerge.HasErrors())
            {
                // Flipping Union does not appear to work
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

        /// <summary>
        /// Makes the error collection user ready.
        /// </summary>
        /// <returns></returns>
        public string MakeUserReady()
        {
            StringBuilder result = new StringBuilder("<Error>");

            foreach(string e in _errorList)
            {
                result.Append(GlobalConstants.InnerErrorTag);
                result.Append(e);
                result.Append(GlobalConstants.InnerErrorTagEnd);
            }

            result.Append("</Error>");

            return result.ToString();
        }

        /// <summary>
        /// Makes the error collection user ready.
        /// </summary>
        /// <returns></returns>
        public string MakeDisplayReady()
        {
            StringBuilder result = new StringBuilder();

            foreach(string e in _errorList)
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
        public string MakeDataListReady(bool AsXML = true)
        {
            StringBuilder result = new StringBuilder();

            if(!AsXML)
            {
                result.Append("\"errors\": [ ");
            }

            int errCnt = 0;
            foreach(string e in _errorList)
            {
                var formattedMsg = FormatErrorMessage(e);
                if(AsXML)
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

            if(!AsXML)
            {
                result.Append("]");
            }

            return result.ToString();
        }

        private string FormatErrorMessage(string s)
        {
            if(s.Contains("Cannot set unknown member"))
            {
                return "Resource has unrecognized formatting, this Warewolf Server may be to outdated to read this resource.";
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
            ErrorResultTO result = new ErrorResultTO();
            try
            {
                if(!string.IsNullOrEmpty(errorsString))
                {
                    errorsString = string.Concat("<Error>", errorsString, "</Error>");
                    XElement errorNode = XElement.Parse(errorsString);
                    foreach(XElement element in errorNode.Elements("InnerError"))
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
