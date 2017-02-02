using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Activities.Designers2.Core
{
    public interface IServiceInputBuilder
    {
        void GetValue(string s, List<IServiceInput> dt);
    }
}