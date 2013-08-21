using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Dev2.DataList.Contract;

namespace Dev2.Core.Tests.ProperMoqs
{
    [Export(typeof(IDev2ConfigurationProvider))]
    public class MoqConfigurationReader : IDev2ConfigurationProvider
    {

        private IDictionary d = new Dictionary<string, string>();

        public void Init(string[] key, string[] val)
        {
            int pos = 0;
            d.Clear();

            foreach (string k in key)
            {
                d.Add(k, val[pos]);
                pos++;
            }
        }

        public string ReadKey(string key)
        {
            return (string)d[key];
        }

        public void OnReadFailure()
        {

        }

    }
}
