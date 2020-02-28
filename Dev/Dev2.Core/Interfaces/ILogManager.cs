using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Interfaces
{
    public interface ILogManager
    {
        IStateNotifier CreateStateNotifier(IDSFDataObject dsfDataObject);
    }

    public interface ILogManagerImplementation
    {
        IStateNotifier CreateStateNotifierImpl(IDSFDataObject dsfDataObject);
    }
}
