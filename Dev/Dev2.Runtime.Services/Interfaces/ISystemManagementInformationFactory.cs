using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Runtime.Services.Interfaces
{
    public interface ISystemManagementInformationFactory
    {
        ISystemManagementInformationWrapper GetNumberOfCores();
    }
}
