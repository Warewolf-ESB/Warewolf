using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;

[assembly: UITestExtensionPackageAttribute("Workflow Designer View Model Reparent",
typeof(Warewolf.Studio.UITestExtension.WorkflowDesignerViewModelReparent))]

namespace Warewolf.Studio.UITestExtension
{
    public class WorkflowDesignerViewModelReparent : UITestExtensionPackage
    {
        public override object GetService(Type serviceType)
        {
            return new WorkflowDesignerViewModelChildTechnologyManager();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override string PackageName => "Workflow Designer View Model Reparent";
        public override string PackageDescription => "Reparents children of the Warewolf Workflow Designer Viewmodel";
        public override Version PackageVersion => new Version(1, 0, 0, 0);
        public override string PackageVendor => "Dev2";
        public override Version VSVersion => new Version(12, 0, 0, 0);
    }
}