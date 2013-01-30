using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;

namespace Dev2.DataList.Contract
{
    [Serializable]
    public class ErrorResultTO
    {

        private readonly IList<string> _errorList = new List<string>();

        /// <summary>
        /// Adds the error.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void AddError(string msg)
        {
            if (msg != null && msg != string.Empty)
            {
                _errorList.Add(msg);
            }
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
        public void MergeErrors(ErrorResultTO toMerge)
        {
            if(toMerge != null && toMerge.HasErrors())
            {
                // Flipping Union does not appear to work
                foreach(string wtf in toMerge.FetchErrors())
                {
                    _errorList.Add(wtf);
                }
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
        public string MakeUserReady() {
            StringBuilder result = new StringBuilder("<Error>");

            foreach (string e in _errorList) {
                result.Append(GlobalConstants.InnerErrorTag);
                result.Append(e);
                result.Append(GlobalConstants.InnerErrorTagEnd);
            }

            result.Append("</Error>");

            return result.ToString();
        }

        /// <summary>
        /// Makes the error collection data list insert ready.
        /// </summary>
        /// <returns></returns>
        public string MakeDataListReady() {
            StringBuilder result = new StringBuilder();

            foreach (string e in _errorList) {
                result.Append(GlobalConstants.InnerErrorTag);
                result.Append(e);
                result.Append(GlobalConstants.InnerErrorTagEnd);
            }

            return result.ToString();
        }
    }
}
