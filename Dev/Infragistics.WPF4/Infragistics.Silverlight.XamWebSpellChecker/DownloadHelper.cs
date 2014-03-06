using System.Threading;
using System.ComponentModel;
using System.Net;
using System;
using System.IO;

using System.Windows;
using System.Windows.Resources;
using System.Collections.Generic;
using System.Windows.Threading;
using Infragistics.Controls.Interactions;
using System.Windows.Navigation;



namespace Infragistics
{
	internal class DownloadHelper
	{       
		public delegate void TaskCompletedEventHandler(object sender, TaskCompletedEventArgs args);
		public delegate void TaskProgressEventHandler(object sender, TaskProgressEventArgs args);

        //Fixed Bug 34097 - Mihail Mateev 06/10/2010
        public delegate void TaskProgressWpfEventHandler(object sender, TaskProgressWpfEventArg args);


		#region Fields
		private AsyncOperation asyncOperation;
		private bool canCancelWorker;
		private bool cancellationPending;
		private bool isRunning;
		#endregion // Fields

		#region Events
		private readonly SendOrPostCallback operationCompleted;
		private readonly SendOrPostCallback downloadProgressOp;
        private readonly SendOrPostCallback downloadProgressOpWpf;
		private readonly SendOrPostCallback downloadCompletedOp;


		public event DoWorkEventHandler DoWork;
		public event RunWorkerCompletedEventHandler RunWorkerCompleted;
		public event TaskProgressEventHandler DownloadProgress;

        //Fixed Bug 34097 - Mihail Mateev 06/10/2010
        public event TaskProgressWpfEventHandler DownloadProgressWpf;

        public event TaskCompletedEventHandler DownloadCompleted;
		#endregion // Events

		#region Methods
		public DownloadHelper()
		{
			this.operationCompleted = new SendOrPostCallback(this.AsyncOperationCompleted);
			this.downloadProgressOp = new SendOrPostCallback(this.DownloadProgressReporter);

            //Fixed Bug 34097 - Mihail Mateev 06/10/2010
            this.downloadProgressOpWpf = new SendOrPostCallback(this.DownloadProgressReporterWpf);

			this.downloadCompletedOp = new SendOrPostCallback(this.DownloadOperationCompleted);
		}

		private void AsyncOperationCompleted(object arg)
		{
			this.isRunning = false;
			this.cancellationPending = false;
			this.OnRunWorkerCompleted((RunWorkerCompletedEventArgs)arg);
		}
		private void DownloadProgressReporter(object arg)
		{
			this.OnDownloadProgress((TaskProgressEventArgs)arg);
		}

        //Fixed Bug 34097 - Mihail Mateev 06/10/2010
        private void DownloadProgressReporterWpf(object arg)
        {
            this.OnDownloadProgressWpf((TaskProgressWpfEventArg)arg);
        }

		private void DownloadOperationCompleted(object arg)
		{
			this.OnDownloadCompleted((TaskCompletedEventArgs)arg);
		}

		protected virtual void OnDoWork(DoWorkEventArgs e)
		{
			if (DoWork != null)
			{
				DoWork(this, e);
			}
		}
		protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
		{
			if (RunWorkerCompleted != null)
			{
				RunWorkerCompleted(this, e);
			}
		}
		protected virtual void OnDownloadProgress(TaskProgressEventArgs e)
		{
			if (DownloadProgress != null)
			{
				DownloadProgress(this, e);
			}
		}

        //Fixed Bug 34097 - Mihail Mateev 06/10/2010
        protected virtual void OnDownloadProgressWpf(TaskProgressWpfEventArg e)
        {
            if (DownloadProgressWpf != null)
            {
                DownloadProgressWpf(this, e);
            }
        }

        protected virtual void OnDownloadCompleted(TaskCompletedEventArgs e)
		{
			if (DownloadCompleted != null)
			{
				DownloadCompleted(this, e);
			}
		}

		public void CancelAsync()
		{
			if (!this.WorkerSupportsCancellation)
			{
				throw new InvalidOperationException("BackgroundWorker_WorkerDoesntSupportCancellation");
			}
			this.cancellationPending = true;
		}

		public void ReportDownloadProgress(object taskID, DownloadProgressChangedEventArgs arg)
		{
			lock (asyncOperation)
			{
				TaskProgressEventArgs newArgs = new TaskProgressEventArgs(arg, taskID);
				if (this.asyncOperation != null)
				{
					this.asyncOperation.Post(this.downloadProgressOp, newArgs);
				}
				else
				{
					this.downloadProgressOp(newArgs);
				}
			}
		}



        //Fixed Bug 34097 - Mihail Mateev 06/10/2010
        public void ReportDownloadProgressWpf(object taskID, ProgressChangedEventArgs arg)
        {

                lock (asyncOperation)
                {
                    TaskProgressWpfEventArg newArgs = new TaskProgressWpfEventArg(arg, taskID);
                    if (this.asyncOperation != null)
                    {
                        this.asyncOperation.Post(this.downloadProgressOpWpf, newArgs);
                    }
                    else
                    {
                        this.downloadProgressOpWpf(newArgs);
                    }
                }
        }


		public void ReportDownloadCompleted(object taskID, AsyncCompletedEventArgs arg)
		{
			lock (asyncOperation)
			{
				TaskCompletedEventArgs newArgs = new TaskCompletedEventArgs(arg, taskID);
				if (this.asyncOperation != null)
				{
					this.asyncOperation.Post(this.downloadCompletedOp, newArgs);
				}
				else
				{
					this.downloadCompletedOp(newArgs);
				}

				if (newArgs.args.Error != null)
				{
					this.error = newArgs.args.Error;
				}
			}
		}

		public void RunWorkerAsync()
		{
			this.RunWorkerAsync(null);
		}
		public void RunWorkerAsync(object argument)
		{
			if (this.isRunning)
			{
				throw new InvalidOperationException("BackgroundWorker_WorkerAlreadyRunning");
			}
			this.isRunning = true;
			this.cancellationPending = false;
			this.asyncOperation = AsyncOperationManager.CreateOperation(null);
			new Thread(() =>
			{ WorkerThreadStart(argument); }).Start();
		}

		Exception error;
		private void WorkerThreadStart(object argument)
		{
			object result = null;
			bool cancelled = false;
			try
			{
				DoWorkEventArgs e = new DoWorkEventArgs(argument);
				this.OnDoWork(e);
				if (e.Cancel)
				{
					cancelled = true;
				}
				else
				{
					result = e.Result;
				}
			}
			catch (Exception exception2)
			{
				error = exception2;
			}
			RunWorkerCompletedEventArgs arg = new RunWorkerCompletedEventArgs(result, error, cancelled);
			this.asyncOperation.PostOperationCompleted(this.operationCompleted, arg);
		}
		#endregion // Methods

		#region Properties
		public bool CancellationPending
		{
			get
			{
				return this.cancellationPending;
			}
		}
		public bool IsBusy
		{
			get
			{
				return this.isRunning;
			}
		}

		public bool WorkerSupportsCancellation
		{
			get
			{
				return this.canCancelWorker;
			}
			set
			{
				this.canCancelWorker = value;
			}
		}


		#endregion // Properties

	}

	#region SubTask
	internal class Subtask : IDisposable
	{
        private Uri Uri { get; set; }

        private Uri BaseUri { get; set; }

		internal event DownloadProgressChangedEventHandler DownloadProgress;
		internal event AsyncCompletedEventHandler DownloadCompleted;

        //WPF progress changed patch Bug #34097
        internal event ProgressChangedEventHandler ProgressChanged;

		// Signal this ManualResetEvent when the task is finished.
		internal ManualResetEvent Finished = new ManualResetEvent(false);
		internal Stream stream;
        
        Uri downloadUri;
		WebClient webClient;

        private void SetupSubtaskForDownload()
        {

        }


        internal Subtask(Uri uri, Uri baseUri)



		{
            this.Uri = uri;

            this.BaseUri = baseUri;

			webClient = new WebClient();

            if (uri.IsAbsoluteUri && uri.Host.Equals("siteoforigin:,,,"))
            {
                StreamResourceInfo info = Application.GetRemoteStream(uri);

                if (info != null)
                {
                    FileStream str = info.Stream as FileStream;

                    this.downloadUri = new Uri(str.Name, UriKind.Absolute);
                }
            }
            else if (uri.IsAbsoluteUri && uri.Host.Equals("application:,,,"))
            {
                webClient = null;
                this.downloadUri = uri;
            }
            else if (!uri.IsAbsoluteUri)
            {
                webClient = null;
                if (!uri.OriginalString.StartsWith("pack://"))
                {
                    this.downloadUri = new Uri(this.BaseUri, uri.OriginalString);
                }
                else
                {
                    this.downloadUri = uri;
                }

                //StreamResourceInfo info = Application.GetResourceStream(uri);
                //string path = uri.OriginalString;

                //int indx = path.LastIndexOf("/") + 1;
                //string filename = path.Substring(indx);
                //string fullname = filename;

                //if(info == null)
                //{
                //    info = Application.GetContentStream(uri);

                //    while (path.StartsWith("/") || path.StartsWith("\\"))
                //    {
                //        path = path.Substring(1);
                //    }
                //    fullname = path;
                //}

                //if(info != null)
                //{
                //    Stream str = info.Stream as Stream;
                //    byte[] buffer = new byte[str.Length];
                //    str.Read(buffer, 0, (int)str.Length);
                //    System.IO.File.WriteAllBytes(filename, buffer);

                //    string filepath = AppDomain.CurrentDomain.BaseDirectory + fullname;
                //    Uri newUri = new Uri(filepath, UriKind.Absolute);
                //    this.downloadUri = newUri;                                                                      
                //}
                //else
                //{
                //    this.downloadUri = uri;
                //}                

            }
            else
            {
                this.downloadUri = uri;
            }
            



        }

        internal void DoSubtask(object state)
		{
            if (webClient != null)
            {
                webClient.OpenReadCompleted += new System.Net.OpenReadCompletedEventHandler(this.Read_Completed);
                webClient.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(Read_DownloadProgressChanged);
                webClient.OpenReadAsync(downloadUri);
            }

            else
            {
                StreamResourceInfo info = Application.GetResourceStream(this.downloadUri);                               
                stream = info.Stream  ;
                OnDownloadCompleted(new AsyncCompletedEventArgs(null, false, null));
                Finished.Set();
            }

		}

		internal void CancelAsync()
		{
			webClient.CancelAsync();
		}

		private void Read_Completed(object sender, OpenReadCompletedEventArgs e)
		{
			webClient.OpenReadCompleted -= new System.Net.OpenReadCompletedEventHandler(this.Read_Completed);
			webClient.DownloadProgressChanged -= new System.Net.DownloadProgressChangedEventHandler(Read_DownloadProgressChanged);

			if (e.Error == null)
			{
				try
				{
					System.IO.BinaryReader r = new System.IO.BinaryReader(e.Result);




                    // Since we don't know how many bytes there will be,
                    // use a dynamic list to store them
                    List<byte[]> byteList = new List<byte[]>();

                    int totalLength = 0;
                    byte[] bytes;

                    // Keep reading until the reader returns 0 bytes
                    while ((bytes = r.ReadBytes(1024)).Length > 0)
                    {
                        byteList.Add(bytes);
                        totalLength += bytes.Length;
                    }

                    // Create a byte array to store the final result
                    bytes = new byte[totalLength];
                    int position = 0;

                    // Cycle through the list of byte arrays we read in
                    // and copy each in turn to the final result
                    foreach (byte[] b in byteList)
                    {
                        Array.Copy(b, 0, bytes, position, b.Length);
                        position += b.Length;
                        if (this.DownloadProgress != null )
                        {
                            //Fixed Bug 34097 - Mihail Mateev 06/10/2010
                            OnProgressChanged(new ProgressChangedEventArgs(position * 100 / totalLength, e.UserState));
                        }
                    }

					stream = new System.IO.MemoryStream(bytes);
				}
				catch (IOException ex)
				{
					OnDownloadCompleted(new AsyncCompletedEventArgs(ex, e.Cancelled, e.UserState));
					Finished.Set();
					return;
				}
			}

			OnDownloadCompleted(new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
			Finished.Set();
		}


        //Fixed Bug 34097 - Mihail Mateev 06/10/2010
        private void OnProgressChanged(ProgressChangedEventArgs args)
        {
            if (this.ProgressChanged != null)
                this.ProgressChanged(this, args);
        }


		private void OnDownloadCompleted(AsyncCompletedEventArgs args)
		{
			if (this.DownloadCompleted != null)
				this.DownloadCompleted(this, args);
		}

		private void Read_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			if (this.DownloadProgress != null)
				this.DownloadProgress(sender, e);

		}

		#region IDisposable Members

		public void Dispose()
		{
			if (stream != null)
				stream.Close();
			if (Finished != null)
				Finished.Close();

			GC.SuppressFinalize(this);
		}

		#endregion
	}
	#endregion //Subtask
	#region EventArgs
	internal class TaskCompletedEventArgs : EventArgs
	{
		public TaskCompletedEventArgs(AsyncCompletedEventArgs args, object taskID)
		{
			this.args = args;
			this.taskID = taskID;
		}

		internal AsyncCompletedEventArgs args;
		internal object taskID;
	}

	internal class TaskProgressEventArgs : EventArgs
	{
		internal DownloadProgressChangedEventArgs args;
		internal object taskID;

		public TaskProgressEventArgs(DownloadProgressChangedEventArgs args, object taskID)
		{
			this.args = args;
			this.taskID = taskID;
		}

	}


    //Fixed Bug 34097 - Mihail Mateev 06/10/2010
    internal class TaskProgressWpfEventArg : EventArgs
    {
        internal ProgressChangedEventArgs args;
        internal object taskID;

        public TaskProgressWpfEventArg(ProgressChangedEventArgs args, object taskID)
        {
            this.args = args;
            this.taskID = taskID;
        }

    }

	#endregion //EventArgs

}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved