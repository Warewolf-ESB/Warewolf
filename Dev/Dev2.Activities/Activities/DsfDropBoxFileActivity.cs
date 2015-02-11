using System;
using System.Collections.Generic;
using System.Windows.Media;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Hosting;
using Dev2.Util;
using DropNet;
using DropNet.Models;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;

namespace Dev2.Activities
{

    [ToolDescriptorInfo("ControlFlow-Descision", "Drop box", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Bob", "1.0.0.0", "c:\\", "Dropbox", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfDropBoxFileActivity : DsfBaseActivity
    {
        IDropNetClient _dropnetClient;
        IFile _file;

        public DsfDropBoxFileActivity()
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            DisplayName = "Dropbox File Operation";
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }


        // ReSharper disable MemberCanBePrivate.Global
         public OauthSource SelectedSource { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

         [Inputs("Source File")]
         [FindMissing]
         // ReSharper disable once UnusedMember.Global
         public string SourceFile { get; set; }

         [Inputs("Destination Path")]
         [FindMissing]
         // ReSharper disable once UnusedMember.Global
         // ReSharper disable MemberCanBePrivate.Global
         public string DestinationPath { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

         [Inputs("Operation")]
         // ReSharper disable once UnusedMember.Global
         // ReSharper disable MemberCanBePrivate.Global
         public string Operation { get; set; }
         // ReSharper restore MemberCanBePrivate.Global


         #region Overrides of DsfBaseActivity

         public override string DisplayName { get; set; }

         public IDropNetClient DropNetClient
         {
             private get
             {
                 return _dropnetClient ?? new DropNetClient(GlobalConstants.DropBoxApiKey, GlobalConstants.DropBoxAppSecret){UserLogin = new UserLogin {Secret = SelectedSource.Secret,Token = SelectedSource.Key}};
             }
             set
             {
                 _dropnetClient = value;
             }
         }

        public IFile File
        {
            private get { return _file ?? new FileWrapper();  }
            set { _file = value; }
        }


         protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
         {
             if (SelectedSource != null)
             {
                 var oauthSource = ResourceCatalog.Instance.GetResource<OauthSource>(GlobalConstants.ServerWorkspaceID, SelectedSource.ResourceID);
                 if (oauthSource == null || oauthSource.ResourceType!=ResourceType.OauthSource)
                 {
                     return "Failure: Source has been deleted.";
                 }
                 SelectedSource = oauthSource;
             }

             if (evaluatedValues["Operation"] == "Write File")
             {
                 var destinationFileName = evaluatedValues["DestinationPath"];
                 var destinationPath = "/";
                 if (destinationFileName.Contains("/"))
                 {
                     destinationPath = destinationFileName.Substring(0, 1 + destinationFileName.LastIndexOf("/", StringComparison.Ordinal));
                     destinationFileName = destinationFileName.Substring(destinationFileName.LastIndexOf("/", StringComparison.Ordinal) + 1);
                 }
                 var output = DropNetClient.UploadFile(destinationPath, destinationFileName, File.ReadAllBytes(evaluatedValues["SourceFile"]));
                 if (output == null)
                 {
                     Dev2Logger.Log.Error("Unable to upload. Result is null. This indicates that there is no internet connection");
                     return "Failure";
                 }
                 Dev2Logger.Log.Debug(String.Format("File uploaded to dropbox {0}", output.Path));
                 return "Success";
             }
                 // ReSharper disable RedundantIfElseBlock
             else if(evaluatedValues["Operation"] == "Read File")
                 // ReSharper restore RedundantIfElseBlock
             {
                 var destinationFileName = evaluatedValues["SourceFile"];

                 File.WriteAllBytes(destinationFileName, DropNetClient.GetFile(evaluatedValues["DestinationPath"]));

                 Dev2Logger.Log.Debug(String.Format("File written to local file system {0}", destinationFileName));
                 return "Success"; 
             }
             return "Failure";
         }
    

        #endregion
    }
}