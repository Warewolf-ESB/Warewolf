using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ApiTesting.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        [Route("api/Error")]
        public HttpResponseMessage Error()
        {
            var message = $"Error";
            HttpError err = new HttpError(message);
            return Request.CreateResponse(HttpStatusCode.NotFound, err);

        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
