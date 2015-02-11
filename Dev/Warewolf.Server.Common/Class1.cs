using System.Collections.Generic;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.ToolBox;
using Microsoft.Practices.Unity;

namespace Warewolf.Server.Common
{
    public class Bootstrapper
    {
       public  static IUnityContainer _unityContainer = new UnityContainer();

        static Bootstrapper()
        {
            _unityContainer.RegisterInstance(typeof(IToolManager),new ServerToolRepository(new List<string>(),new List<string>() ));
        }
    }
}
