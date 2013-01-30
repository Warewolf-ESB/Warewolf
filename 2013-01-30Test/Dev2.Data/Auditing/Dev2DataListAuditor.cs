using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Server.Datalist.Auditing
{
    internal class Dev2DataListAuditor : IDev2DataListAuditor
    {
        /// <summary>
        /// The _input changes
        /// </summary>
        private readonly IList<KeyValuePair<string, IBinaryDataListEntry>> _inputChanges = new List<KeyValuePair<string, IBinaryDataListEntry>>();
        /// <summary>
        /// The _output changes
        /// </summary>
        private readonly IList<KeyValuePair<string, IBinaryDataListEntry>> _outputChanges = new List<KeyValuePair<string, IBinaryDataListEntry>>();
        
        internal Dev2DataListAuditor() { }

        /// <summary>
        /// Pushes the change.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The val.</param>
        /// <param name="direction">The direction.</param>
        public void PushChange(string key, IBinaryDataListEntry val, string direction)
        {
            if (direction == "input")
            {
                _inputChanges.Add(new KeyValuePair<string, IBinaryDataListEntry>(key, val));
            }
            else
            {
                _outputChanges.Add(new KeyValuePair<string, IBinaryDataListEntry>(key, val));
            }
        }

        /// <summary>
        /// Fetches the changes.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(string direction)
        {
            IList<KeyValuePair<string, IBinaryDataListEntry>> result = null;

            if (direction == "input"){
                result = _inputChanges;
            }else if(direction == "output"){
                result = _outputChanges;
            }

            return result;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            _inputChanges.Clear();
            _outputChanges.Clear();
        }
    }
}
