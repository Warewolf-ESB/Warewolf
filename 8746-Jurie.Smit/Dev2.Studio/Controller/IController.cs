using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Studio.Controller
{
    public interface IController<T> where T : IScreen
    {
        T ControlledViewModel { get; set; }
    }
}
