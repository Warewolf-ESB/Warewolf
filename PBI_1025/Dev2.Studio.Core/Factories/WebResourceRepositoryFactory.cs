using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Framework;
using Dev2.Composition;

namespace Dev2.Studio.Core.Factories {
    public static class WebResourceRepositoryFactory
    {
        public static IFrameworkRepository<IWebResourceViewModel> CreateWebResourceRepository(IContextualResourceModel resource) {
            IFrameworkRepository<IWebResourceViewModel> webResourceFactory = new WebResourceRepository(resource);
            ImportService.SatisfyImports(webResourceFactory);
            
            return webResourceFactory;
        }
    }
}
