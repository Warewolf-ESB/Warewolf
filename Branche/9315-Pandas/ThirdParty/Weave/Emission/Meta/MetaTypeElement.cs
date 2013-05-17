using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Meta
{
    internal abstract class MetaTypeElement
    {
        protected readonly Type sourceType;
        private string _dynamicAssemblyName;

        public string DynamicAssemblyName { get { return _dynamicAssemblyName; } }

        protected MetaTypeElement(Type sourceType, string dynamicAssemblyName)
        {
            this.sourceType = sourceType;
            _dynamicAssemblyName = dynamicAssemblyName;
        }

        internal bool CanBeImplementedExplicitly
        {
            get { return sourceType != null && sourceType.IsInterface; }
        }

        internal abstract void SwitchToExplicitImplementation();
    }
}
