/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer.Responses;
using Newtonsoft.Json;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class TokenRequestHandler : AbstractWebRequestHandler

    {
        public TokenRequestHandler(IResourceCatalog catalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog)
            : base(catalog, testCatalog, testCoverageCatalog)
        {
        }

        public TokenRequestHandler()
        {
        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var authToken = CreateToken();
            ctx.Send(authToken);
        }

        static IResponseWriter CreateToken()
        {
            //TODO: Create json response with the auth token encrypted with base64, This will be a list of the warewolf groups.
            //TODO: var loginWorkflow = Config.Server.LoginWorkflow; // or something like that
            //TODO: var response = ExecuteWorkflow(loginWorkflow.Value, true);
            //TODO: var encryptedTokenResponse = EncryptResponse(response);

            const string encryptedTokenResponse = "qweqwewqeqweqwwqeqwe";

            var converter = new JsonSerializer();
            var result = new StringBuilder();
            var jsonTextWriter = new JsonTextWriter(new StringWriter(result)) {Formatting = Formatting.Indented};
            converter.Serialize(jsonTextWriter, encryptedTokenResponse);
            jsonTextWriter.Flush();
            var apisJson = result.ToString();
            var stringResponseWriter = new StringResponseWriter(apisJson, ContentTypes.Json, false);
            return stringResponseWriter;
        }
    }
}