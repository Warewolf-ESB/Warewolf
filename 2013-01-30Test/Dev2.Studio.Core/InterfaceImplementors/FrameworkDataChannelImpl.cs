//using System;
//using System.ComponentModel.Composition;
//using System.Diagnostics;
//using System.ServiceModel;
//using System.Xml;
//using Unlimited.Framework;

//namespace Dev2.Studio.Core
//{
//    public class FrameworkDataChannelImpl : IFrameworkDataChannel
//    {
//        private readonly ChannelFactory<IFrameworkDataChannel> _channelFactory;
//        private string _allowedRoles = "Business Design Studio Developers";

//        private IFrameworkDataChannel _dsf;

//        public FrameworkDataChannelImpl()
//        {
//            _channelFactory = new ChannelFactory<IFrameworkDataChannel>();
//        }

//        public FrameworkDataChannelImpl(IFrameworkSecurityContext securityContext, string name, Uri dsfUri,
//                                        enDsfChannelMode mode, string allowedRoles, string spnEndpointIdentity = "")
//        {
//            Name = name;
//            DsfAddress = dsfUri;
//            Mode = mode;
//            AllowedRoles = allowedRoles;
//            SPNEndpointIdentity = spnEndpointIdentity;


//            CreateDsfChannel();
//        }

//        [Import]
//        public IFrameworkSecurityContext SecurityContext { get; set; }

//        public string Name { get; set; }

//        public Uri DsfAddress { get; set; }

//        public enDsfChannelMode Mode { get; set; }

//        public string SPNEndpointIdentity { get; set; }

//        public ChannelFactory<IFrameworkDataChannel> ChannelFactoryObject
//        {
//            get { return _channelFactory; }
//        }

//        public string AllowedRoles
//        {
//            get { return _allowedRoles; }
//            set { _allowedRoles = value; }
//        }

//        public bool HasAccess
//        {
//            get
//            {
//                if (string.IsNullOrEmpty(AllowedRoles))
//                {
//                    return false;
//                }

//                if (SecurityContext == null)
//                {
//                    return false;
//                }

//                return
//                    SecurityContext.IsUserInRole(AllowedRoles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
//            }
//        }

//        #region IFrameworkDataChannel Members

//        public string ExecuteCommand(string xmlRequest)
//        {
//            if (HasAccess)
//            {
//                try
//                {
//                    Debug.WriteLine(string.Format("Setting up connection to DSF at address '{0}'",
//                                                  DsfAddress.AbsolutePath));
//                    return _dsf.ExecuteCommand(xmlRequest);
//                }
//                catch (Exception ex)
//                {
//                    dynamic error = new UnlimitedObject(ex);
//                    if (DsfConnectionFailure != null)
//                    {
//                        DsfConnectionFailure(error.XmlString);
//                    }


//                    error.BDSError = "Execution Failure";

//                    return error.XmlString;
//                }
//            }
//            else
//            {
//                return "<Error>Access denied</Error>";
//            }
//        }

//        #endregion

//        public event MessageEventHandler DsfConnectionFailure;


//        public IFrameworkDataChannel CreateDsfChannel()
//        {
//            var dsfAddress = new EndpointAddress(DsfAddress);

//            var dsfBinding = new BasicHttpBinding
//            {
//                MaxReceivedMessageSize = 999999999,
//                ReaderQuotas = new XmlDictionaryReaderQuotas
//                {
//                    MaxStringContentLength
//                        = 99999999
//                },
//                SendTimeout = new TimeSpan(1, 0, 0)
//            };

//            var tcpBinding = new NetTcpBinding
//            {
//                MaxReceivedMessageSize = 999999999,
//                ReaderQuotas = new XmlDictionaryReaderQuotas
//                {
//                    MaxStringContentLength
//                        = 99999999
//                },
//                SendTimeout = new TimeSpan(1, 0, 0)
//            };

//            _channelFactory.Endpoint.Binding = dsfBinding;
//            _channelFactory.Endpoint.Address = dsfAddress;

//            _dsf = _channelFactory.CreateChannel();
//            Debug.WriteLine(string.Format("Connected to DSF at address '{0}'", DsfAddress.AbsolutePath));
//            //result the connection
//            _dsf.ExecuteCommand(string.Format("<x><Service>{0}</Service></x>", Guid.NewGuid().ToString()));
//            return _dsf;
//        }
//    }
//}