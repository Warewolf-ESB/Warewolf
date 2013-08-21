using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Emitters
{
    internal interface IEmissionMemberEmitter
    {
        MemberInfo Member { get; }
        Type ReturnType { get; }

        void EnsureValidCodeBlock();
        void Generate();
    }
}
