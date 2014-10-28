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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Unlimited.Framework.Converters.Graph.Ouput;

// ReSharper disable CheckNamespace

namespace Unlimited.Framework.Converters.Graph.Output
// ReSharper restore CheckNamespace
{
    /// <summary>
    ///     A serialization service which uses the DataContractSerializer to serialize to XML
    /// </summary>
    [Serializable]
    public class OutputDescriptionSerializationService : IOutputDescriptionSerializationService
    {
        #region Class Members

        private static readonly List<Type> _knownTypes;

        #endregion Class Members

        #region Constructors

        static OutputDescriptionSerializationService()
        {
            _knownTypes = new List<Type>(GetKnownTypes());
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Serializes the given ouput description to XML
        /// </summary>
        public string Serialize(IOutputDescription outputDescription)
        {
            var dataContractSerializer = new DataContractSerializer(typeof (IOutputDescription), _knownTypes);

            string data;

            using (var stringWriter = new StringWriter())
            {
                using (var xmlTextWriter = new XmlTextWriter(stringWriter))
                {
                    dataContractSerializer.WriteObject(xmlTextWriter, outputDescription);

                    data = stringWriter.GetStringBuilder().ToString();

                    xmlTextWriter.Close();
                    stringWriter.Close();
                }
            }

            return data;
        }

        /// <summary>
        ///     Deserialize the given data to an output description
        /// </summary>
        public IOutputDescription Deserialize(string data)
        {
            IOutputDescription outputDescription = null;

            if (!string.IsNullOrWhiteSpace(data))
            {
                data = data.Replace("<![CDATA[", "");
                data = data.Replace("]]>", "");

                var dataContractSerializer = new DataContractSerializer(typeof (IOutputDescription), _knownTypes);

                using (var stringReader = new StringReader(StripKnownLegacyTags(data)))
                {
                    using (var xmlTextReader = new XmlTextReader(stringReader))
                    {
                        try
                        {
                            outputDescription = dataContractSerializer.ReadObject(xmlTextReader) as IOutputDescription;
                        }
                        catch (Exception ex)
                        {
                            Dev2Logger.Log.Error(ex);
                            // we want to return null                    
                        }
                    }
                }
            }

            return outputDescription;
        }

        #endregion Methods

        #region Private Methods

        private static IEnumerable<Type> GetKnownTypes()
        {
            Type pathType = typeof (IPath);
            Type outputDescriptionType = typeof (IOutputDescription);
            Type dataSourceShapeType = typeof (IDataSourceShape);

            List<Type> knownTypes = typeof (OutputDescription).Assembly.GetTypes()
                .Where(t => (pathType.IsAssignableFrom(t) && t != pathType) ||
                            (outputDescriptionType.IsAssignableFrom(t) && t != outputDescriptionType) ||
                            (dataSourceShapeType.IsAssignableFrom(t) && t != dataSourceShapeType)).ToList();

            return knownTypes;
        }

        private static string StripKnownLegacyTags(string data)
        {
            return data.Replace("<Dev2XMLResult>", null).Replace("</Dev2XMLResult>", null).Replace("<JSON />", null);
        }

        #endregion Private Methods
    }
}