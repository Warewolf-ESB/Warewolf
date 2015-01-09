using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolboxModel
    {
        //IServer Server{get;}
        /// <summary>
        /// gets the description of tools from the connected server
        /// </summary>
        /// <returns></returns>
        IList<IToolDescriptor> GetTools();
        /// <summary>
        /// Checks if a tool is supported by confirming if the local server has the same tool
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        bool IsToolSupported(IToolDescriptor tool);

        ///  <summary>
        ///  Loads a tool onto a server. This will copy  dll onto a server.Refreshes the server tools afterwards.
        /// Only tools available on the local server can be deployed to another server
        ///  </summary>
        /// <param name="tool"></param>
        /// <param name="dllBytes"></param>
        void LoadTool(IToolDescriptor tool, byte[] dllBytes);

        /// <summary>
        /// Delete a tool from the connected server. this will delete the dll associated with the tool and refresh the server
        /// </summary>
        /// <param name="tool"></param>
        void DeleteTool(IToolDescriptor tool);
        /// <summary>
        /// filter the list of available tools
        /// </summary>
        /// <param name="search">the string that the name must contain</param>
        /// <returns></returns>
        IList<IToolDescriptor> Filter(string search);
        /// <summary>
        /// Is connected.
        /// </summary>
        /// <returns></returns>
        bool IsEnabled();
    }
}