using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class WebPermission
    {
        public bool IsReadOnly { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}