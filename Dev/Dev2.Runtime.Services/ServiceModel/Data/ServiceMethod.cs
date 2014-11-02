/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    [DataContract]
    [Serializable]
    public class ServiceMethod
    {
        #region CTOR

        public ServiceMethod()
            : this(string.Empty, string.Empty, null, null, null, null)
        {
        }

        public ServiceMethod(string error, string stackTrace)
            : this("Error : " + error, stackTrace, null, null, null, null)
        {
        }

        public ServiceMethod(string name, string sourceCode, IEnumerable<MethodParameter> parameters,
            IOutputDescription outputDescription, IEnumerable<MethodOutput> outputs, string executeAction)
        {
            Name = name;
            SourceCode = sourceCode;
            OutputDescription = outputDescription;
            Parameters = new List<MethodParameter>();
            Outputs = new List<MethodOutput>();
            ExecuteAction = executeAction;
            if (parameters != null)
            {
                Parameters.AddRange(parameters);
            }

            if (outputs != null)
            {
                Outputs.AddRange(outputs);
            }
        }

        #endregion

        #region Properties

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<MethodParameter> Parameters { get; set; }

        [DataMember]
        public string SourceCode { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string ExecuteAction { get; set; }

        public List<MethodOutput> Outputs { get; private set; }

        public IOutputDescription OutputDescription { get; set; }

        #endregion

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

        #region ToXml

        public XElement ToXml()
        {
            return new XElement("ServiceMethod");
        }

        #endregion
    }
}