using System;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.ServerDialogue;

namespace Warewolf.Studio.ViewModels
{
    class TestConnection : IServerConnectionTest
    {

        /// <summary>
        /// Reurns a result from testing the connection
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Test(IServerSource server)
        {


            return "Success";
            //return "Failure";
            //return String.Empty;
        }

    }
}
