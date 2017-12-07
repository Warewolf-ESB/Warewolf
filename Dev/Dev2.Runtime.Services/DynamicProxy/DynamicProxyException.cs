
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Text;

namespace Dev2.Runtime.DynamicProxy
{
    public class DynamicProxyException : ApplicationException
    {
        IEnumerable<MetadataConversionError> _importErrors;
        IEnumerable<MetadataConversionError> _codegenErrors;
        IEnumerable<CompilerError> _compilerErrors;

        public DynamicProxyException(string message)
            : base(message)
        {
        }

        public DynamicProxyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public IEnumerable<MetadataConversionError> MetadataImportErrors
        {
            get
            {
                return _importErrors;
            }

            internal set
            {
                _importErrors = value;
            }
        }

        public IEnumerable<MetadataConversionError> CodeGenerationErrors
        {
            get
            {
                return _codegenErrors;
            }

            internal set
            {
                _codegenErrors = value;
            }
        }

        public IEnumerable<CompilerError> CompilationErrors
        {
            get
            {
                return _compilerErrors;
            }

            internal set
            {
                _compilerErrors = value;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());

            if (MetadataImportErrors != null)
            {
                builder.AppendLine("Metadata Import Errors:");
                builder.AppendLine(DynamicProxyFactory.ToString(
                            MetadataImportErrors));
            }

            if (CodeGenerationErrors != null)
            {
                builder.AppendLine("Code Generation Errors:");
                builder.AppendLine(DynamicProxyFactory.ToString(
                            CodeGenerationErrors));
            }

            if (CompilationErrors != null)
            {
                builder.AppendLine("Compilation Errors:");
                builder.AppendLine(DynamicProxyFactory.ToString(
                            CompilationErrors));
            }

            return builder.ToString();
        }
    }
}
