using System.Diagnostics;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Common.Wrappers
{
    public class ExternalProcessWrapper:IExternalProsses
    {
        #region Implementation of IExternalProsses

        public Process Start(string fileName)
        {
            return Process.Start(fileName);
        }

        #endregion
    }
}
