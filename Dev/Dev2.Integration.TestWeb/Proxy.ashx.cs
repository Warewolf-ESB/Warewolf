
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
using System.Web;

namespace Dev2.Integration.TestWeb
{
    public class Proxy : AbstractHttpHandler
    {
        protected override string GetResponse(HttpContext context, string extension)
        {
            var root = context.Request.MapPath("~/Files");

            var path = Path.Combine(root, "test." + extension);
            return File.ReadAllText(path);
        }
    }
}
