using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Dev2.Studio.Controller
{
    public class AbstractController<T> : IController<T> where T : IScreen
    {
        public T ControlledViewModel { get; set; }
    }
}
