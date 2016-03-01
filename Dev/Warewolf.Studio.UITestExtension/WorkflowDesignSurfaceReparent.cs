using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;

[assembly: UITestExtensionPackageAttribute("Workflow Designer View Model Reparent",
typeof(Warewolf.Studio.UITestExtension.WorkflowDesignSurfaceReparent))]

namespace Warewolf.Studio.UITestExtension
{
    public class WorkflowDesignSurfaceReparent : UITestExtensionPackage
    {
        public override object GetService(Type serviceType)
        {
            return new WorkflowDesignSurfaceChildTechnologyManager();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override string PackageName
        {
            get
            {
                return "Workflow Designer View Model Reparent";
            }
        }
        public override string PackageDescription
        {
            get
            {
                return "Reparents children of the Warewolf Workflow Designer Viewmodel";
            }
        }
        public override Version PackageVersion
        {
            get
            {
                return new Version(1, 0, 0, 0);
            }
        }
        public override string PackageVendor
        {
            get
            {
                return "Dev2";
            }
        }
        public override Version VSVersion
        {
            get
            {
                return new Version(12, 0, 0, 0);
            }
        }
    }
}