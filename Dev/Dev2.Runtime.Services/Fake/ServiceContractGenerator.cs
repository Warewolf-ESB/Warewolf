using CoreWCF.Description;
using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Dev2.Runtime.DynamicProxy
{
    internal class ServiceContractGenerator
    {
        private CodeCompileUnit codeCompileUnit;

        public ServiceContractGenerator(CodeCompileUnit codeCompileUnit)
        {
            this.codeCompileUnit = codeCompileUnit;
        }

        public ServiceContractGenerationOptions Options { get; internal set; }
        public IEnumerable<MetadataConversionError> Errors { get; internal set; }

        internal static void GenerateServiceContractType(ContractDescription contract)
        {
            throw new NotImplementedException();
        }
    }

    public enum ServiceContractGenerationOptions
    {
        ClientClass
    }
}