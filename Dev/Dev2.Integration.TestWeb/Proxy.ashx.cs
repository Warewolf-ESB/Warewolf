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