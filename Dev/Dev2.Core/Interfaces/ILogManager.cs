using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Interfaces
{
    public interface ILogManager : IDisposable
    {
        IStateNotifier CreateStateNotifier(IDSFDataObject dsfDataObject);
    }
}
