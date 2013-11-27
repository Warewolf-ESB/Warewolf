using System;
using System.Collections.Generic;
using System.Emission.Emitters;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Generators
{
    internal interface IEmissionGenerator<T>
    {
        T Generate(ClassEmitter @class, EmissionProxyOptions options, IDesignatingScope designatingScope, string dynamicAssemblyName);
    }
}
