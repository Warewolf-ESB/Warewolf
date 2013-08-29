using System;
using System.Collections.Generic;
using System.Activities;
using System.IO;
using Dev2.Common;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class DsfFileForEachActivity : DsfActivityAbstract<bool> {
        private bool _failOnFirstError = false;
        private int _skipRows = 0;

        /// <summary>
        /// Will result in the File For Each quitting and reporting an error 
        /// as soon as any row fails row validation
        /// </summary>
        public bool FailOnFirstError {
            get {
                return _failOnFirstError;
            }
            set {
                _failOnFirstError = value;
            }
        }
        /// <summary>
        /// Defines how many rows to skip before we start to process row data
        /// </summary>
        public int SkipRows {
            get {
                return _skipRows;

            }
            set {
                _skipRows = value;
            }

        }
        /// <summary>
        /// The URI of the file that we will be streaming and processing
        /// </summary>
        public string FileURI { get; set; }
        /// <summary>
        /// Applies to files that will be processes from an FTP server and represents the username to use for authentication
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Applies to files that will be processes from an FTP server and represents the password to use for authentication
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// The Formatter markup to process the data rows
        /// </summary>
        public string DataFormat { get; set; }
        /// <summary>
        /// The formatter markup to process the header row
        /// </summary>
        public string HeaderFormat { get; set; }
        /// <summary>
        /// The formatter markup to process the footer row
        /// </summary>
        public string FooterFormat { get; set; }

        public ActivityFunc<string, bool> HeaderFunc { get; set; }
        public ActivityFunc<string, bool> DataFunc { get; set; }
        public ActivityFunc<string, bool> FooterFunc { get; set; }
        public ActivityFunc<string, bool> ExceptionFunc { get; set; }

        private Variable<Stream> fileStream = new Variable<Stream>("Stream");
        private Variable<IEnumerator<string>> fileLines = new Variable<IEnumerator<string>>("FileLines");
        private Variable<long> rowCount = new Variable<long>("RowCount");

        DelegateInArgument<string> actionArgument = new DelegateInArgument<string>("explicitDataFromParent");
        DelegateInArgument<string> exceptionArgument = new DelegateInArgument<string>("explicitExceptionDataFromParent");
        DelegateInArgument<string> headerArgument = new DelegateInArgument<string>("explicitHeaderDataFromParent");
        DelegateInArgument<string> footerArgument = new DelegateInArgument<string>("explicitFooterDataFromParent");

        public DsfFileForEachActivity() {
            DataFunc = new ActivityFunc<string, bool> {
                DisplayName = "Data Action",
                Argument = actionArgument

            };


            ExceptionFunc = new ActivityFunc<string, bool> {
                DisplayName = "Exception Action",
                Argument = exceptionArgument

            };

            HeaderFunc = new ActivityFunc<string, bool> {
                DisplayName = "Header Action",
                Argument = headerArgument
            };

            FooterFunc = new ActivityFunc<string, bool> {
                DisplayName = "Footer Action",
                Argument = footerArgument
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata) {
            metadata.AddDelegate(DataFunc);
            metadata.AddDelegate(ExceptionFunc);
            metadata.AddDelegate(HeaderFunc);
            metadata.AddDelegate(FooterFunc);
            metadata.AddImplementationVariable(fileStream);
            metadata.AddImplementationVariable(fileLines);
            metadata.AddImplementationVariable(rowCount);
            
            base.CacheMetadata(metadata);

        }

        protected override void OnExecute(NativeActivityContext context) {
            throw new NotImplementedException(GlobalConstants.NoLongerSupportedMsg);
        }



        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }
    }
}
