using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Workspaces;
using Unlimited.Applications.BusinessDesignStudio.Views;

namespace Dev2.Studio.Controller
{
    [Export(typeof(IController<MainViewModel>))]
    public class MainController : AbstractController<MainViewModel>
    {


    }
}
