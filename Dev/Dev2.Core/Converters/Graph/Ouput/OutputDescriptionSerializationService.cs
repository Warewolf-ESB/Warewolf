﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph.Output
{
    /// <summary>
    /// A serialization service which uses the DataContractSerializer to serialize to XML
    /// </summary>
    public class OutputDescriptionSerializationService : IOutputDescriptionSerializationService
    {
        #region Class Members

        private static List<Type> _knownTypes;

        #endregion Class Members

        #region Constructors

        static OutputDescriptionSerializationService()
        {
            _knownTypes = new List<Type>(GetKnownTypes());
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Serializes the given ouput description to XML
        /// </summary>
        public string Serialize(IOutputDescription outputDescription)
        {
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(IOutputDescription), _knownTypes);
            
            StringWriter stringWriter = new StringWriter();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
            dataContractSerializer.WriteObject(xmlTextWriter, outputDescription);

            string data = stringWriter.GetStringBuilder().ToString();

            xmlTextWriter.Close();
            stringWriter.Close();

            return data;
        }

        /// <summary>
        /// Deserialize the given data to an output description
        /// </summary>
        public IOutputDescription Deserialize(string data)
        {
            IOutputDescription outputDescription = null;

            if (data != null)
            {
                data = data.Replace("<![CDATA[", "");
                data = data.Replace("]]>", "");

                DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(IOutputDescription), _knownTypes);
                string a = "";

                StringReader stringReader = new StringReader(data);
                XmlTextReader xmlTextReader = new XmlTextReader(stringReader);

                try
                {
                    outputDescription = dataContractSerializer.ReadObject(xmlTextReader) as IOutputDescription;
                }
                catch(Exception) { 
                    // trap the exception to avoid nasty behavior, aka we want to return null                    
                }

                string b = a;
            }

            return outputDescription;
        }

        #endregion Methods

        #region Private Methods

        private static IEnumerable<Type> GetKnownTypes()
        {
            Type pathType = typeof(IPath);
            Type outputDescriptionType = typeof(IOutputDescription);
            Type dataSourceShapeType = typeof(IDataSourceShape);

            List<Type> knownTypes = typeof(IOutputDescription).Assembly.GetTypes()
                .Where(t => (pathType.IsAssignableFrom(t) && t != pathType) ||
                    (outputDescriptionType.IsAssignableFrom(t) && t != outputDescriptionType) ||
                    (dataSourceShapeType.IsAssignableFrom(t) && t != dataSourceShapeType)).ToList();

            return knownTypes;
        }

        #endregion Private Methods
    }
}
