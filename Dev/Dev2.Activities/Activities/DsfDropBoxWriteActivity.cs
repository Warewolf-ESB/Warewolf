using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using DropNet;
using Microsoft.VisualBasic.Logging;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Activities
{
    public class DsfDropBoxWriteActivity : DsfBaseActivity
    {

        public DsfDropBoxWriteActivity()
        {
            DisplayName = "Upload";
        }

         public OauthSource SelectedSource { get; set; }

         [Inputs("SourceFile")]
         // ReSharper disable once UnusedMember.Global
         public string SourceFile { get; set; }

         [Inputs("SourceFile")]
         // ReSharper disable once UnusedMember.Global
         public string DestinationPath { get; set; }


         [Outputs("Result")]
         // ReSharper disable once UnusedMember.Global

         #region Overrides of DsfBaseActivity

         public override string DisplayName { get; set; }

         protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
         {
            try
            {

           
                DropNetClient client = new DropNetClient(GlobalConstants.DropBoxApiKey,GlobalConstants.DropBoxAppSecret,SelectedSource.Key,SelectedSource.Secret);
                var destinationFileName = evaluatedValues["DestinationPath"];
                var destinationPath = "//";
                if(destinationFileName.Contains("/"))
                {
                    destinationPath = destinationFileName.Substring(0, DestinationPath.LastIndexOf("/", System.StringComparison.Ordinal));
                    destinationFileName = destinationFileName.Substring(DestinationPath.LastIndexOf("/", System.StringComparison.Ordinal)+1);
                }
                var output = client.UploadFile(destinationPath, destinationFileName, File.ReadAllBytes(evaluatedValues["SourceFile"]));
                if(output == null)
                {
                    Dev2Logger.Log.Error("Unable to upload. Result is null. This indicates that there is no internet connection");
                    return "Failure";
                }
                Dev2Logger.Log.Debug(String.Format("File uploaded to dropbox {0}",output.Path));
                return "Success";
            }
            catch (Exception)
            {

                return "Failure";
            }
         }

        #endregion
    }
}