
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.IO;

namespace HttpFramework.Templates
{
    /// <summary>
    /// Interface for template rendering engines
    /// </summary>
    public interface TemplateEngine
    {
        /// <summary>
        /// Render the template
        /// </summary>
        /// <param name="fileName">Filename of the template to render</param>
        /// <param name="variables">A list of key/value pairs to pass to the template</param>
        /// <returns>The rendered output</returns>
        /// <exception cref="FileNotFoundException">If template is not found.</exception>
        string Render(string fileName, IDictionary<string, object> variables);
    }
}
