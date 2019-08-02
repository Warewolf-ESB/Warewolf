
using System;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Timers;
using Dev2.SignalR.Wrappers;
using Dev2.SignalR.Wrappers.New;

namespace TestRedirects
{
    class Program
    {
        static AppDomain _currentDomain= AppDomain.CurrentDomain;

        static void Main(string[] args)
        {

             

            CurrentDomain.AssemblyResolve += MyResolveEventHandler;

           Console.WriteLine("sss");
            ServerProxy x = new ServerProxy("http://localhost:3142/dsf",CredentialCache.DefaultNetworkCredentials);
            ServerProxy y = new ServerProxy("http://tst-ci-remote.premier.local:3142/dsf", CredentialCache.DefaultNetworkCredentials);
            x.Connect(Guid.NewGuid());
            y.Connect(Guid.NewGuid());

            Console.WriteLine("sss");
            Console.ReadLine();
        }

        public static AppDomain CurrentDomain
        {
            get
            {
                return _currentDomain;
            }
            set
            {
                _currentDomain = value;
            }
        }

        static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name == "FooLibrary")
            {
                return typeof(ServerProxy).Assembly;
            }
            return null;
        }
    }

    public class ServerProxy 
    {
        System.Timers.Timer _reconnectHeartbeat;
        private const int MillisecondsTimeout = 10000;

        ICredentials _credentials;


        public static bool IsShuttingDown { get; private set; }

        public ServerProxy(string serverUri, ICredentials credentials)
        {

            IsAuthorized = true;



            var uriString = serverUri;
            if (!serverUri.EndsWith("dsf"))
            {
                uriString = serverUri + (serverUri.EndsWith("/") ? "" : "/") + "dsf";
            }
            AppServerUri = new Uri(uriString);
            WebServerUri = new Uri(uriString.Replace("/dsf", ""));
            Credentials = credentials;

 
            try
            {
                HubConnection = new HubConnectionWrapper(uriString) { Credentials = credentials }; ;
                HubConnection.Error += OnHubConnectionError;
                HubConnection.Closed += HubConnectionOnClosed;
                HubConnection.StateChanged += HubConnectionStateChanged;
                //HubConnection.TraceLevel = TraceLevels.All;
                //HubConnection.TraceWriter = new Dev2LoggingTextWriter();
                InitializeEsbProxy();
            }
            catch(Exception err)
            {

                HubConnection = new Dev2.SignalR.Wrappers.Old.HubConnectionWrapperOld(uriString) { Credentials = credentials }; ;
                HubConnection.Error += OnHubConnectionError;
                HubConnection.Closed += HubConnectionOnClosed;
                HubConnection.StateChanged += HubConnectionStateChanged;
                //HubConnection.TraceLevel = TraceLevels.All;
                //HubConnection.TraceWriter = new Dev2LoggingTextWriter();
                InitializeEsbProxy();
            }



        }

        public ICredentials Credentials
        {
            get
            {
                return _credentials;
            }
            set
            {
                _credentials = value;
            }
        }

        public IPrincipal Principal { get; set; }

        public ServerProxy(string webAddress, string userName, string password)
            : this(webAddress, new NetworkCredential(userName, password))
        {
            UserName = userName;
            Password = password;

        }

        public bool IsLocalHost { get { return DisplayName == "localhost"; } }

        protected void InitializeEsbProxy()
        {
            if (EsbProxy == null)
            {
                EsbProxy = HubConnection.CreateHubProxy("esb");
                EsbProxy.On<string>("SendMemo", OnMemoReceived);
                EsbProxy.On<string>("ReceiveResourcesAffectedMemo", OnReceiveResourcesAffectedMemo);
                EsbProxy.On<string>("SendPermissionsMemo", OnPermissionsMemoReceived);
                EsbProxy.On<string>("SendDebugState", OnDebugStateReceived);
                EsbProxy.On<Guid>("SendWorkspaceID", OnWorkspaceIdReceived);
                EsbProxy.On<Guid>("SendServerID", OnServerIdReceived);
                EsbProxy.On<string>("ItemUpdatedMessage", OnItemUpdatedMessageReceived);
                EsbProxy.On<string>("ItemDeletedMessage", OnItemDeletedMessageReceived);
                EsbProxy.On<string>("ItemAddedMessage", OnItemAddedMessageReceived);
            }
        }

        void OnReceiveResourcesAffectedMemo(string obj)
        {
        }

        void OnItemUpdatedMessageReceived(string obj)
        {
        }

        void OnDebugStateReceived(string obj)
        {
        }

        void OnMemoReceived(string obj)
        {
        }

        void HubConnectionOnClosed()
        {
            HasDisconnected();
        }

        void HasDisconnected()
        {

            IsConnected = false;
            if (IsShuttingDown)
            {
                return;
            }
            StartReconnectTimer();

        }

        void OnWorkspaceIdReceived(Guid obj)
        {
            AddDebugWriter(obj);
            WorkspaceID = obj;
        }

        void OnServerIdReceived(Guid obj)
        {
            ServerID = obj;
        }



        protected void HubConnectionStateChanged(IStateChangeWrapped stateChange)
        {
            switch (stateChange.NewState)
            {

            }
        }

        public bool IsConnected { get; set; }
        public string Alias { get; set; }
        public string DisplayName { get; set; }

        public void Connect(Guid id)
        {
            ID = id;
            try
            {
                if (!IsLocalHost)
                {
                    if (HubConnection.State == ConnectionStateWrapped.Reconnecting)
                    {
                        HubConnection.Stop(new TimeSpan(0, 0, 0, 1));
                    }
                }

                if (HubConnection.State == ConnectionStateWrapped.Disconnected)
                {
                    ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                    try
                    {

                
                        if (!HubConnection.Start().Wait(5000))
                        {
                            if (!IsLocalHost)
                            {
                                ConnectionRetry();
                            }
                        }
                    }
                    catch (AggregateException aex)
                    {
               
                        HubConnection = new Dev2.SignalR.Wrappers.Old.HubConnectionWrapperOld(AppServerUri.ToString()) { Credentials = Credentials }; ;
                        HubConnection.Error += OnHubConnectionError;
                        HubConnection.Closed += HubConnectionOnClosed;
                        HubConnection.StateChanged += HubConnectionStateChanged;
                        //HubConnection.TraceLevel = TraceLevels.All;
                        //HubConnection.TraceWriter = new Dev2LoggingTextWriter();
                        InitializeEsbProxy();
                        if (!HubConnection.Start().Wait(5000))
                        {
                            if (!IsLocalHost)
                            {
                                ConnectionRetry();
                            }
                        }

                    }
                }
            }
            catch (AggregateException aex)
            {


                aex.Flatten();
                aex.Handle(ex =>
                {
                   
                    var hex = ex as HttpClientExceptionWrapped;
                    if (hex != null)
                    {
                        switch (hex.Response.StatusCode)
                        {
                            case HttpStatusCode.Unauthorized:
                            case HttpStatusCode.Forbidden:
                                UpdateIsAuthorized(false);
                                throw new UnauthorizedAccessException();
                        }
                    }
                    var hexOld = ex as Dev2.SignalR.Wrappers.New.HttpClientExceptionWrappedOld;
                    if (hexOld != null)
                    {
                        switch (hexOld.Response.StatusCode)
                        {
                            case HttpStatusCode.Unauthorized:
                            case HttpStatusCode.Forbidden:
                                UpdateIsAuthorized(false);
                                throw new UnauthorizedAccessException();
                        }
                    }
                    throw new Exception();
                });
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        void UpdateIsAuthorized(bool p0)
        {
        }

        private void ConnectionRetry()
        {
            HubConnection.Stop(new TimeSpan(0, 0, 0, 1));

          
        }

        bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        void HandleConnectError(Exception e)
        {
        
            StartReconnectTimer();
        }

        protected virtual void StartReconnectTimer()
        {
            if (IsLocalHost)
            {
                if (_reconnectHeartbeat == null)
                {
                    _reconnectHeartbeat = new System.Timers.Timer();
                    _reconnectHeartbeat.Elapsed += OnReconnectHeartbeatElapsed;
                    _reconnectHeartbeat.Interval = 3000;
                    _reconnectHeartbeat.AutoReset = true;
                    _reconnectHeartbeat.Start();
                }
            }
        }

        public virtual void StopReconnectHeartbeat()
        {
            if (_reconnectHeartbeat != null)
            {
                _reconnectHeartbeat.Stop();
                _reconnectHeartbeat.Dispose();
                _reconnectHeartbeat = null;
            }
        }


        void OnReconnectHeartbeatElapsed(object sender, ElapsedEventArgs args)
        {
            Connect(ID);
            if (IsConnected)
            {
                StopReconnectHeartbeat();
            }
        }

        public void Disconnect()
        {
            // It can take some time to shutdown when permissions have changed ;(
            // Give 5 seconds, then force a dispose ;)
            try
            {
                IsShuttingDown = true;
                IsConnected = false;
                HubConnection.Stop(new TimeSpan(0, 0, 0, 5));
            }
            catch (AggregateException aex)
            {
                aex.Flatten();
                aex.Handle(ex =>
                {
                    var hex = ex as HttpClientExceptionWrapped;
                    if (hex != null)
                    {
                        switch (hex.Response.StatusCode)
                        {
                            case HttpStatusCode.Unauthorized:
                            case HttpStatusCode.Forbidden:
                                UpdateIsAuthorized(false);
                                throw new Exception();
                        }
                    }
                    var hexold = ex as Dev2.SignalR.Wrappers.New.HttpClientExceptionWrappedOld;
                    if (hexold != null)
                    {
                        switch (hexold.Response.StatusCode)
                        {
                            case HttpStatusCode.Unauthorized:
                            case HttpStatusCode.Forbidden:
                                UpdateIsAuthorized(false);
                                throw new Exception();
                        }
                    }
                    throw new Exception();
                });
            }
            catch (Exception e)
            {

            }
        }

     


      

        public IHubProxyWrapper EsbProxy { get; protected set; }

        public IHubConnectionWrapper HubConnection { get; private set; }

        void OnHubConnectionError(Exception exception)
        {
          
        }


        void OnPermissionsMemoReceived(string objString)
        {

        }



        void OnItemAddedMessageReceived(string obj)
        {

        }


        void OnItemDeletedMessageReceived(string obj)
        {

        }

       

        public Guid ServerID { get; protected set; }
        public Guid WorkspaceID { get; private set; }
        public Uri AppServerUri { get; private set; }
        public Uri WebServerUri { get; private set; }

        public string UserName { get; private set; }
        public string Password { get; private set; }

        /// <summary>
        /// <code>True</code> unless server returns Unauthorized or Forbidden status.
        /// </summary>
        public bool IsAuthorized { get; private set; }





    


        public void AddDebugWriter(Guid workspaceId)
        {
            var t = EsbProxy.Invoke("AddDebugWriter", workspaceId);
            Wait(t);
        }

        protected virtual void Wait(Task task)
        {
            task.Wait(100);
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        // ReSharper disable InconsistentNaming
        public Guid ID { get; private set; }
        // ReSharper restore InconsistentNaming

    }
}

