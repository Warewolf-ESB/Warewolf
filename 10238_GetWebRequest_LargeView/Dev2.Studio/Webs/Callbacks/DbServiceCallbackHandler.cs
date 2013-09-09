using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.DataList.Contract;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.InterfaceImplementors.WizardResourceKeys;

namespace Dev2.Studio.Webs.Callbacks
{
    public class DbServiceCallbackHandler : ServiceCallbackHandler
    {
        bool _isEditingSource;
        string _returnUri;

        public DbServiceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public DbServiceCallbackHandler(IEnvironmentRepository currentEnvironmentRepository)
            : base(currentEnvironmentRepository)
        {
        }
    }
}
