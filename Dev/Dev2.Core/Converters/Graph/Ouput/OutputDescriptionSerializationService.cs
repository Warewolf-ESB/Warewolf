#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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



namespace Unlimited.Framework.Converters.Graph.Output

{
    /// <summary>
    ///     A serialization service which uses the DataContractSerializer to serialize to XML
    /// </summary>
    [Serializable]
    public class OutputDescriptionSerializationService : IOutputDescriptionSerializationService
    {
        #region Class Members

        static readonly List<Type> _knownTypes;

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
                            Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                            // we want to return null                    
                        }
                    }
                }
            }

            return outputDescription;
        }

        #endregion Methods

        #region Private Methods

        static IEnumerable<Type> GetKnownTypes()
        {
            var pathType = typeof(IPath);
            var outputDescriptionType = typeof(IOutputDescription);
            var dataSourceShapeType = typeof(IDataSourceShape);

            var knownTypes = typeof(OutputDescription).Assembly.GetTypes()
                .Where(t => pathType.IsAssignableFrom(t) && t != pathType ||
                            outputDescriptionType.IsAssignableFrom(t) && t != outputDescriptionType ||
                            dataSourceShapeType.IsAssignableFrom(t) && t != dataSourceShapeType).ToList();

            return knownTypes;
        }

        static string StripKnownLegacyTags(string data) => data.Replace("<Dev2XMLResult>", null).Replace("</Dev2XMLResult>", null).Replace("<JSON />", null);

        #endregion Private Methods
    }
}