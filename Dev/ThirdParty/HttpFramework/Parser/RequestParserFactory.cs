
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using HttpFramework.Parser;

namespace HttpFramework
{
    /// <summary>
    /// Creates request parsers when needed.
    /// </summary>
    public class RequestParserFactory : IRequestParserFactory
    {
        /// <summary>
        /// Create a new request parser.
        /// </summary>
        /// <param name="logWriter">Used when logging should be enabled.</param>
        /// <returns>A new request parser.</returns>
        public IHttpRequestParser CreateParser(ILogWriter logWriter)
        {
            return new HttpRequestParser(logWriter);
        }
    }

    /// <summary>
    /// Creates request parsers when needed.
    /// </summary>
    public interface IRequestParserFactory
    {
        /// <summary>
        /// Create a new request parser.
        /// </summary>
        /// <param name="logWriter">Used when logging should be enabled.</param>
        /// <returns>A new request parser.</returns>
        IHttpRequestParser CreateParser(ILogWriter logWriter);
    }
}
