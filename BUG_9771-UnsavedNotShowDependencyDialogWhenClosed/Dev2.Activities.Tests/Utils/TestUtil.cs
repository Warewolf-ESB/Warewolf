using System.Net;
using System.IO;

namespace ActivityUnitTests.Utils {
    public class TestUtil {

        public static string ExecuteGetRequest(string uri) {
            string result = string.Empty;

            WebRequest req = HttpWebRequest.Create(uri);
            req.Method = "GET";

            using (var response = req.GetResponse() as HttpWebResponse) {
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                    result = reader.ReadToEnd();
                }

            }

            return result;
        }
    }
}
