
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Web;
using System.Text;

namespace HttpFramework
{
    /// <summary>
    /// Generic helper functions for HTTP
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// Version string for HTTP v1.0
        /// </summary>
        public const string HTTP10 = "HTTP/1.0";

        /// <summary>
        /// Version string for HTTP v1.1
        /// </summary>
        public const string HTTP11 = "HTTP/1.1";

        /// <summary>
        /// Parses a query string.
        /// </summary>
        /// <param name="queryString">Query string (URI encoded)</param>
        /// <param name="contentEncoding">Content encoding</param>
        /// <returns>A <see cref="HttpInput"/> object if successful; otherwise <see cref="HttpInput.Empty"/></returns>
        /// <exception cref="ArgumentNullException"><c>queryString</c> is <c>null</c>.</exception>
		/// <exception cref="FormatException">If string cannot be parsed.</exception>
        public static HttpInput ParseQueryString(string queryString, Encoding contentEncoding)
        {
            if (queryString == null)
                throw new ArgumentNullException("queryString");
            if (queryString == string.Empty)
                return HttpInput.Empty;

			HttpInput input = new HttpInput("QueryString");

        	queryString = queryString.TrimStart('?', '&');

			// a simple value.
			/*
			if (queryString.IndexOf("&") == -1 && !queryString.Contains("%3d") && !queryString.Contains("%3D") && !queryString.Contains("="))
			{
				input.Add(string.Empty, queryString);
				return input;
			}*/
			if (queryString.IndexOf("&") == -1 && !queryString.Contains("="))
			{
				input.Add(string.Empty, queryString);
				return input;
			}

        	int state = 0;
            int startpos = 0;
            string name = null;
            for (int i = 0; i < queryString.Length; ++i)
            {
                int newIndexPos;
                if (state == 0 && IsEqual(queryString, ref i, out newIndexPos))
                {
                    name = queryString.Substring(startpos, i - startpos);
                    i = newIndexPos;
                    startpos = i + 1;
                    ++state;
                }
                else if (state == 1 && IsAmp(queryString, ref i, out newIndexPos))
                {
                    Add(input, name, queryString.Substring(startpos, i - startpos), contentEncoding);
                    i = newIndexPos;
                    startpos = i + 1;
                    state = 0;
                    name = null;
                }
            }

            if (state == 0 && !input.GetEnumerator().MoveNext())
				throw new FormatException("Not a valid query string: " + queryString);

            if (startpos <= queryString.Length)
            {
            	if (name != null)
                    Add(input, name, queryString.Substring(startpos, queryString.Length - startpos), contentEncoding);
				else
                    Add(input, string.Empty, queryString.Substring(startpos, queryString.Length - startpos), contentEncoding);
            }
                

            return input;
        }

		/// <summary>
		/// If you string contains %2d instead of =, you have encoded your string
		/// incorrectly. It's not a bug in the http server that it isn't decoded.
		/// </summary>
		/// <param name="queryStr"></param>
		/// <param name="index"></param>
		/// <param name="outIndex"></param>
		/// <returns></returns>
        private static bool IsEqual(string queryStr, ref int index, out int outIndex)
        {
            outIndex = index;
            if (queryStr[index] == '=')
                return true;
			return false;
            //if (queryStr[index] == '%' && queryStr.Length > index + 2 && queryStr[index + 1] == '3'
            //    && (queryStr[index + 2] == 'd' || queryStr[index + 2] == 'D'))
            //{
            //    outIndex += 2;
            //    return true;
            //}
            //return false;
        }

		/// <summary>
		/// It's really simple. If you string contains %26 instead of &, you have encoded
		/// it incorrectly. It should NOT be used as a parameter by the server.
		/// </summary>
		/// <param name="queryStr"></param>
		/// <param name="index"></param>
		/// <param name="outIndex"></param>
		/// <returns></returns>
        private static bool IsAmp(string queryStr, ref int index, out int outIndex)
        {
			outIndex = index;
			if (queryStr[index] == '&')
			{
				// should NOT be interpreted as an parameter.
				// since it's a & in the text.
				// else users can't use & at all.
				if (queryStr.Length > index + 4
					&& (queryStr[index + 1] == 'a' || queryStr[index + 1] == 'A')
					&& (queryStr[index + 2] == 'm' || queryStr[index + 2] == 'M')
					&& (queryStr[index + 3] == 'p' || queryStr[index + 3] == 'P')
					&& queryStr[index + 4] == ';')
					return false;
				//outIndex += 1;
				return true;
			}
			return false;

			/*
            outIndex = index;
            if (queryStr[index] == '%' && queryStr.Length > index + 2 && queryStr[index + 1] == '2' &&
                queryStr[index + 2] == '6')
                outIndex += 2;
            else if (queryStr[index] == '&')
            {
				// should NOT be interpreted as an parameter.
				// since it's a & in the text.
				// else users can't use & at all.
                if (queryStr.Length > index + 4
                    && (queryStr[index + 1] == 'a' || queryStr[index + 1] == 'A')
                    && (queryStr[index + 2] == 'm' || queryStr[index + 2] == 'M')
                    && (queryStr[index + 3] == 'p' || queryStr[index + 3] == 'P')
                    && queryStr[index + 4] == ';')
                    return false;
            	//outIndex += 1;
            	return true;
            }
            else
                return false;

            return true;
			 */
        }

        private static void Add(IHttpInput input, string name, string value, Encoding contentEncoding)
        {
            input.Add(HttpUtility.UrlDecode(name, contentEncoding), HttpUtility.UrlDecode(value, contentEncoding));
        }
    }
}

