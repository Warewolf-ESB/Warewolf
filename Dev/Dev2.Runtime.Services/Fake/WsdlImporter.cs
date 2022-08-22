using System.ServiceModel;
using System.ServiceModel.Description;
using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;

namespace Dev2.Runtime.DynamicProxy
{
    internal class WsdlImporter
    {
        private CoreWCF.Description.MetadataSet metadataSet;

        public WsdlImporter(CoreWCF.Description.MetadataSet metadataSet)
        {
            this.metadataSet = metadataSet;
        }

        public Dictionary<object, object> State { get; internal set; }
        public IEnumerable<object> WsdlImportExtensions { get; internal set; }
        public IEnumerable<CoreWCF.Description.MetadataConversionError> Errors { get; internal set; }

        internal static  IEnumerable<Binding> ImportAllBindings()
        {
            throw new NotImplementedException();
        }

        internal static  IEnumerable<ServiceEndpoint> ImportAllEndpoints()
        {
            throw new NotImplementedException();
        }

        internal static  IEnumerable<CoreWCF.Description.ContractDescription> ImportAllContracts()
        {
            throw new NotImplementedException();
        }
    }
}