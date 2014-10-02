
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;
using System.Text;

namespace HttpFramework.FormDecoders
{
    /// <summary>
    /// Interface for form content decoders.
    /// </summary>
    public interface IFormDecoder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">Stream containing the content</param>
        /// <param name="contentType">Content type (with any additional info like boundry). Content type is always supplied in lower case</param>
        /// <param name="encoding">Stream enconding</param>
        /// <returns>A http form, or null if content could not be parsed.</returns>
        /// <exception cref="InvalidDataException">If contents in the stream is not valid input data.</exception>
        HttpForm Decode(Stream stream, string contentType, Encoding encoding);

        /// <summary>
        /// Checks if the decoder can handle the mime type
        /// </summary>
        /// <param name="contentType">Content type (with any additional info like boundry). Content type is always supplied in lower case.</param>
        /// <returns>True if the decoder can parse the specified content type</returns>
        bool CanParse(string contentType);
    }
}
