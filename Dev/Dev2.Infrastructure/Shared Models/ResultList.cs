using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class ResultList<T>
    {
        public bool HasErrors { get; set; }
        public string Errors { get; set; }

        public List<T> Items { get; private set; }

        public ResultList()
        {
            Items = new List<T>();
        }

        public ResultList(string errorFormat, params object[] args)
            : this()
        {
            HasErrors = true;
            Errors = string.Format(errorFormat, args);
        }

        public ResultList(Exception ex)
            : this()
        {
            var errors = new StringBuilder();
            var tmp = ex;
            while(tmp != null)
            {
                errors.AppendLine(tmp.Message);
                tmp = tmp.InnerException;
            }

            HasErrors = true;
            Errors = errors.ToString();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}