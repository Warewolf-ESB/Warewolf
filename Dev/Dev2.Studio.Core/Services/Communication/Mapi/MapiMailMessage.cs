//using System;
//using System.Collections;
//using System.Runtime.InteropServices;
//using System.Threading;
//using System.Threading.Tasks;
//using Dev2.Network;
//using Dev2.Providers.Logs;
//
//// ReSharper disable once CheckNamespace
//namespace Dev2.Studio.Core.Services.Communication.Mapi
//{
//    #region Public MapiMailMessage Class
//    /// <summary>
//    /// Represents an email message to be sent through MAPI.
//    // ReSharper disable CSharpWarnings::CS1570
//    /// Original source http://www.vbusers.com/codecsharp/codeget.asp?ThreadID=71&PostID=1
//    // ReSharper restore CSharpWarnings::CS1570
//    /// also see http://weblogs.asp.net/jgalloway/archive/2007/02/24/sending-files-via-the-default-e-mail-client.aspx
//    /// </summary>
//    public class MapiMailMessage : IMailMessage
//    {
//        #region Private MapiFileDescriptor Class
//
//        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
//        private class MapiFileDescriptor
//        {
//        }
//
//        #endregion Private MapiFileDescriptor Class
//
//        #region Enums
//
//        /// <summary>
//        /// Specifies the valid RecipientTypes for a Recipient.
//        /// </summary>
//        public enum RecipientType
//        {
//            /// <summary>
//            /// Recipient will be in the TO list.
//            /// </summary>
//            To = 1,
//
//            /// <summary>
//            /// Recipient will be in the CC list.
//            /// </summary>
//            // ReSharper disable once InconsistentNaming
//            CC = 2,
//
//            /// <summary>
//            /// Recipient will be in the BCC list.
//            /// </summary>
//            // ReSharper disable once InconsistentNaming
//            BCC = 3
//        };
//
//        #endregion Enums
//
//        #region Member Variables
//
//        private string _subject;
//        private string _body;
//        private readonly RecipientCollection _recipientCollection;
//        private readonly ArrayList _files;
//        private readonly ManualResetEvent _manualResetEvent;
//
//        #endregion Member Variables
//
//        #region Constructors
//
//        /// <summary>
//        /// Creates a blank mail message.
//        /// </summary>
//        public MapiMailMessage()
//        {
//            _files = new ArrayList();
//            _recipientCollection = new RecipientCollection();
//            _manualResetEvent = new ManualResetEvent(false);
//        }
//
//        /// <summary>
//        /// Creates a new mail message with the specified subject.
//        /// </summary>
//        public MapiMailMessage(string subject)
//            : this()
//        {
//            _subject = subject;
//        }
//
//        /// <summary>
//        /// Creates a new mail message with the specified subject and body.
//        /// </summary>
//        public MapiMailMessage(string subject, string body)
//            : this()
//        {
//            _subject = subject;
//            _body = body;
//        }
//
//        #endregion Constructors
//
//        #region Public Properties
//
//        /// <summary>
//        /// Gets or sets the subject of this mail message.
//        /// </summary>
//        public string Subject
//        {
//            get { return _subject; }
//            set { _subject = value; }
//        }
//
//        /// <summary>
//        /// Gets or sets the body of this mail message.
//        /// </summary>
//        public string Body
//        {
//            get { return _body; }
//            set { _body = value; }
//        }
//
//        /// <summary>
//        /// Gets the recipient list for this mail message.
//        /// </summary>
//        public RecipientCollection Recipients
//        {
//            get { return _recipientCollection; }
//        }
//
//        /// <summary>
//        /// Gets the file list for this mail message.
//        /// </summary>
//        public ArrayList Files
//        {
//            get { return _files; }
//        }
//
//        #endregion Public Properties
//
//        #region Public Methods
//
//        /// <summary>
//        /// Displays the mail message dialog asynchronously.
//        /// </summary>
//        public void ShowDialog()
//        {
//            // Create the mail message in an STA thread
//                    Task sendMailTask = new Task(ShowMail);
//                    //            var t = new Thread(ShowMail) { IsBackground = true };
//                    //            t.SetApartmentState(ApartmentState.STA);
//                    //            t.Start();
//                    sendMailTask.Start();
//                    sendMailTask.WaitForResult();
//            // only return when the new thread has built it's interop representation
//            //            _manualResetEvent.WaitOne();
//            //            _manualResetEvent.Reset();
//        }
//
//        #endregion Public Methods
//
//        #region Private Methods
//        const int MAPI_LOGON_UI = 0x00000001;
//        const int MAPI_DIALOG = 0x00000008;
//        /// <summary>
//        /// Sends the mail message.
//        /// </summary>
//        private void ShowMail()
//        {
//            var message = new MapiHelperInterop.MapiMessage();
//
//            using(RecipientCollection.InteropRecipientCollection interopRecipients
//                = _recipientCollection.GetInteropRepresentation())
//            {
//
//                message.Subject = _subject;
//                message.NoteText = _body;
//
//                message.Recipients = interopRecipients.Handle;
//                message.RecipientCount = _recipientCollection.Count;
//
//                // Check if we need to add attachments
//                if(_files.Count > 0)
//                {
//                    // Add attachments
//                    message.Files = AllocAttachments(out message.FileCount);
//                }
//
//                // Signal the creating thread (make the remaining code async)
//                //_manualResetEvent.Set();
//
//                //const int MapiDialog = 0x8;
//                //const int MAPI_LOGON_UI = 0x1;
//                const int SuccessSuccess = 0;
//                int error = MapiHelperInterop.MAPISendMail(new IntPtr(0), new IntPtr(0), message, MAPI_LOGON_UI | MAPI_DIALOG, 0);
//
//                if(_files.Count > 0)
//                {
//                    // Deallocate the files
//                    _DeallocFiles(message);
//                }
//
//                // Check for error
//                if(error != SuccessSuccess)
//                {
//                    _LogErrorMapi(error);
//                }
//            }
//        }
//
//        /// <summary>
//        /// Deallocates the files in a message.
//        /// </summary>
//        /// <param name="message">The message to deallocate the files from.</param>
//        private void _DeallocFiles(MapiHelperInterop.MapiMessage message)
//        {
//            if(message.Files != IntPtr.Zero)
//            {
//                Type fileDescType = typeof(MapiFileDescriptor);
//                int fsize = Marshal.SizeOf(fileDescType);
//
//                // Get the ptr to the files
//                int runptr = (int)message.Files;
//                // Release each file
//                for(int i = 0; i < message.FileCount; i++)
//                {
//                    Marshal.DestroyStructure((IntPtr)runptr, fileDescType);
//                    runptr += fsize;
//                }
//                // Release the file
//                Marshal.FreeHGlobal(message.Files);
//            }
//        }
//
//        /// <summary>
//        /// Allocates the file attachments
//        /// </summary>
//        /// <param name="fileCount"></param>
//        /// <returns></returns>
//        private IntPtr AllocAttachments(out int fileCount)
//        {
//            fileCount = 0;
//            if(_files == null)
//            {
//                return IntPtr.Zero;
//            }
//            if((_files.Count <= 0) || (_files.Count > 100))
//            {
//                return IntPtr.Zero;
//            }
//
//            Type atype = typeof(MapiFileDescriptor);
//            int asize = Marshal.SizeOf(atype);
//            IntPtr ptra = Marshal.AllocHGlobal(_files.Count * asize);
//
//            MapiFileDescriptor mfd = new MapiFileDescriptor();
//            int runptr = (int)ptra;
//            // ReSharper disable once ForCanBeConvertedToForeach
//            for(int i = 0; i < _files.Count; i++)
//            {
//                Marshal.StructureToPtr(mfd, (IntPtr)runptr, false);
//                runptr += asize;
//            }
//
//            fileCount = _files.Count;
//            return ptra;
//        }
//
//
//        /// <summary>
//        /// Logs any Mapi errors.
//        /// </summary>
//        private void _LogErrorMapi(int errorCode)
//        {
//            const int MapiUserAbort = 1;
//            const int MapiEFailure = 2;
//            const int MapiELoginFailure = 3;
//            const int MapiEDiskFull = 4;
//            const int MapiEInsufficientMemory = 5;
//            const int MapiEBlkTooSmall = 6;
//            const int MapiETooManySessions = 8;
//            const int MapiETooManyFiles = 9;
//            const int MapiETooManyRecipients = 10;
//            const int MapiEAttachmentNotFound = 11;
//            const int MapiEAttachmentOpenFailure = 12;
//            const int MapiEAttachmentWriteFailure = 13;
//            const int MapiEUnknownRecipient = 14;
//            const int MapiEBadReciptype = 15;
//            const int MapiENoMessages = 16;
//            const int MapiEInvalidMessage = 17;
//            const int MapiETextTooLarge = 18;
//            const int MapiEInvalidSession = 19;
//            const int MapiETypeNotSupported = 20;
//            const int MapiEAmbiguousRecipient = 21;
//            const int MapiEMessageInUse = 22;
//            const int MapiENetworkFailure = 23;
//            const int MapiEInvalidEditfields = 24;
//            const int MapiEInvalidRecips = 25;
//            const int MapiENotSupported = 26;
//            const int MapiENoLibrary = 999;
//            const int MapiEInvalidParameter = 998;
//
//            string error = string.Empty;
//            switch(errorCode)
//            {
//                case MapiUserAbort:
//                    error = "User Aborted.";
//                    break;
//                case MapiEFailure:
//                    error = "MAPI Failure.";
//                    break;
//                case MapiELoginFailure:
//                    error = "Login Failure.";
//                    break;
//                case MapiEDiskFull:
//                    error = "MAPI Disk full.";
//                    break;
//                case MapiEInsufficientMemory:
//                    error = "MAPI Insufficient memory.";
//                    break;
//                case MapiEBlkTooSmall:
//                    error = "MAPI Block too small.";
//                    break;
//                case MapiETooManySessions:
//                    error = "MAPI Too many sessions.";
//                    break;
//                case MapiETooManyFiles:
//                    error = "MAPI too many files.";
//                    break;
//                case MapiETooManyRecipients:
//                    error = "MAPI too many recipients.";
//                    break;
//                case MapiEAttachmentNotFound:
//                    error = "MAPI Attachment not found.";
//                    break;
//                case MapiEAttachmentOpenFailure:
//                    error = "MAPI Attachment open failure.";
//                    break;
//                case MapiEAttachmentWriteFailure:
//                    error = "MAPI Attachment Write Failure.";
//                    break;
//                case MapiEUnknownRecipient:
//                    error = "MAPI Unknown recipient.";
//                    break;
//                case MapiEBadReciptype:
//                    error = "MAPI Bad recipient type.";
//                    break;
//                case MapiENoMessages:
//                    error = "MAPI No messages.";
//                    break;
//                case MapiEInvalidMessage:
//                    error = "MAPI Invalid message.";
//                    break;
//                case MapiETextTooLarge:
//                    error = "MAPI Text too large.";
//                    break;
//                case MapiEInvalidSession:
//                    error = "MAPI Invalid session.";
//                    break;
//                case MapiETypeNotSupported:
//                    error = "MAPI Type not supported.";
//                    break;
//                case MapiEAmbiguousRecipient:
//                    error = "MAPI Ambiguous recipient.";
//                    break;
//                case MapiEMessageInUse:
//                    error = "MAPI Message in use.";
//                    break;
//                case MapiENetworkFailure:
//                    error = "MAPI Network failure.";
//                    break;
//                case MapiEInvalidEditfields:
//                    error = "MAPI Invalid edit fields.";
//                    break;
//                case MapiEInvalidRecips:
//                    error = "MAPI Invalid Recipients.";
//                    break;
//                case MapiENotSupported:
//                    error = "MAPI Not supported.";
//                    break;
//                case MapiENoLibrary:
//                    error = "MAPI No Library.";
//                    break;
//                case MapiEInvalidParameter:
//                    error = "MAPI Invalid parameter.";
//                    break;
//            }
//            string message = string.Format("Error sending MAPI Email. Error: " + error + " (code = " + errorCode + ").");
//            Logger.Error(message);
//        }
//        #endregion Private Methods
//
//        #region Private MAPIHelperInterop Class
//
//        /// <summary>
//        /// Internal class for calling MAPI APIs
//        /// </summary>
//        internal class MapiHelperInterop
//        {
//            #region Constructors
//
//            /// <summary>
//            /// Private constructor.
//            /// </summary>
//            private MapiHelperInterop()
//            {
//                // Intenationally blank
//            }
//
//            #endregion Constructors
//
//            #region Constants
//
//            public const int MapiLogonUi = 0x1;
//
//            #endregion Constants
//
//            #region APIs
//
//            [DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
//            public static extern int MAPILogon(IntPtr hwnd, string prf, string pw, int flg, int rsv, ref IntPtr sess);
//
//            #endregion APIs
//
//            #region Structs
//
//            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
//            public class MapiMessage
//            {
//                public int Reserved = 0;
//                public string Subject = null;
//                public string NoteText = null;
//                public string MessageType = null;
//                public string DateReceived = null;
//                public string ConversationID = null;
//                public int Flags = 0;
//                public IntPtr Originator = IntPtr.Zero;
//                public int RecipientCount = 0;
//                public IntPtr Recipients = IntPtr.Zero;
//                public int FileCount = 0;
//                public IntPtr Files = IntPtr.Zero;
//            }
//
//            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
//            public class MapiRecipDesc
//            {
//                public int Reserved = 0;
//                public int RecipientClass = 0;
//                public string Name = null;
//                public string Address = null;
//                public int eIDSize = 0;
//                public IntPtr EntryID = IntPtr.Zero;
//            }
//
//            [DllImport("MAPI32.DLL")]
//            public static extern int MAPISendMail(IntPtr session, IntPtr hwnd, MapiMessage message, int flg, int rsv);
//
//            #endregion Structs
//        }
//
//        #endregion Private MAPIHelperInterop Class
//    }
//
//    #endregion Public MapiMailMessage Class
//
//    #region Public Recipient Class
//
//    /// <summary>
//    /// Represents a Recipient for a MapiMailMessage.
//    /// </summary>
//    public class Recipient
//    {
//        #region Public Properties
//
//        /// <summary>
//        /// The email address of this recipient.
//        /// </summary>
//        public string Address = null;
//
//        /// <summary>
//        /// The display name of this recipient.
//        /// </summary>
//        public string DisplayName = null;
//
//        /// <summary>
//        /// How the recipient will receive this message (To, CC, BCC).
//        /// </summary>
//        public MapiMailMessage.RecipientType RecipientType = MapiMailMessage.RecipientType.To;
//
//        #endregion Public Properties
//
//        #region Constructors
//
//        /// <summary>
//        /// Creates a new recipient with the specified address.
//        /// </summary>
//        public Recipient(string address)
//        {
//            Address = address;
//        }
//
//        /// <summary>
//        /// Creates a new recipient with the specified address and display name.
//        /// </summary>
//        public Recipient(string address, string displayName)
//        {
//            Address = address;
//            DisplayName = displayName;
//        }
//
//        /// <summary>
//        /// Creates a new recipient with the specified address and recipient type.
//        /// </summary>
//        public Recipient(string address, MapiMailMessage.RecipientType recipientType)
//        {
//            Address = address;
//            RecipientType = recipientType;
//        }
//
//        /// <summary>
//        /// Creates a new recipient with the specified address, display name and recipient type.
//        /// </summary>
//        public Recipient(string address, string displayName, MapiMailMessage.RecipientType recipientType)
//        {
//            Address = address;
//            DisplayName = displayName;
//            RecipientType = recipientType;
//        }
//
//        #endregion Constructors
//
//        #region Internal Methods
//
//        /// <summary>
//        /// Returns an interop representation of a recepient.
//        /// </summary>
//        /// <returns></returns>
//        internal MapiMailMessage.MapiHelperInterop.MapiRecipDesc GetInteropRepresentation()
//        {
//            MapiMailMessage.MapiHelperInterop.MapiRecipDesc interop = new MapiMailMessage.MapiHelperInterop.MapiRecipDesc();
//
//            if(DisplayName == null)
//            {
//                interop.Name = Address;
//            }
//            else
//            {
//                interop.Name = DisplayName;
//                interop.Address = Address;
//            }
//
//            interop.RecipientClass = (int)RecipientType;
//
//            return interop;
//        }
//
//        #endregion Internal Methods
//    }
//
//    #endregion Public Recipient Class
//
//    #region Public RecipientCollection Class
//
//    /// <summary>
//    /// Represents a colleciton of recipients for a mail message.
//    /// </summary>
//    public class RecipientCollection : CollectionBase
//    {
//        /// <summary>
//        /// Adds the specified recipient to this collection.
//        /// </summary>
//        public void Add(Recipient value)
//        {
//            List.Add(value);
//        }
//
//        /// <summary>
//        /// Adds a new recipient with the specified address to this collection.
//        /// </summary>
//        public void Add(string address)
//        {
//            Add(new Recipient(address));
//        }
//
//        /// <summary>
//        /// Adds a new recipient with the specified address and display name to this collection.
//        /// </summary>
//        public void Add(string address, string displayName)
//        {
//            Add(new Recipient(address, displayName));
//        }
//
//        /// <summary>
//        /// Adds a new recipient with the specified address and recipient type to this collection.
//        /// </summary>
//        public void Add(string address, MapiMailMessage.RecipientType recipientType)
//        {
//            Add(new Recipient(address, recipientType));
//        }
//
//        /// <summary>
//        /// Adds a new recipient with the specified address, display name and recipient type to this collection.
//        /// </summary>
//        public void Add(string address, string displayName, MapiMailMessage.RecipientType recipientType)
//        {
//            Add(new Recipient(address, displayName, recipientType));
//        }
//
//        /// <summary>
//        /// Returns the recipient stored in this collection at the specified index.
//        /// </summary>
//        public Recipient this[int index]
//        {
//            get
//            {
//                return (Recipient)List[index];
//            }
//        }
//
//        internal InteropRecipientCollection GetInteropRepresentation()
//        {
//            return new InteropRecipientCollection(this);
//        }
//
//        /// <summary>
//        /// Struct which contains an interop representation of a colleciton of recipients.
//        /// </summary>
//        internal struct InteropRecipientCollection : IDisposable
//        {
//            #region Member Variables
//
//            private IntPtr _handle;
//            private int _count;
//
//            #endregion Member Variables
//
//            #region Constructors
//
//            /// <summary>
//            /// Default constructor for creating InteropRecipientCollection.
//            /// </summary>
//            /// <param name="outer"></param>
//            public InteropRecipientCollection(RecipientCollection outer)
//            {
//                _count = outer.Count;
//
//                if(_count == 0)
//                {
//                    _handle = IntPtr.Zero;
//                    return;
//                }
//
//                // allocate enough memory to hold all recipients
//                int size = Marshal.SizeOf(typeof(MapiMailMessage.MapiHelperInterop.MapiRecipDesc));
//                _handle = Marshal.AllocHGlobal(_count * size);
//
//                // place all interop recipients into the memory just allocated
//                int ptr = (int)_handle;
//                foreach(Recipient native in outer)
//                {
//                    MapiMailMessage.MapiHelperInterop.MapiRecipDesc interop = native.GetInteropRepresentation();
//
//                    // stick it in the memory block
//                    Marshal.StructureToPtr(interop, (IntPtr)ptr, false);
//                    ptr += size;
//                }
//            }
//
//            #endregion Costructors
//
//            #region Public Properties
//
//            public IntPtr Handle
//            {
//                get { return _handle; }
//            }
//
//            #endregion Public Properties
//
//            #region Public Methods
//
//            /// <summary>
//            /// Disposes of resources.
//            /// </summary>
//            public void Dispose()
//            {
//                if(_handle != IntPtr.Zero)
//                {
//                    Type type = typeof(MapiMailMessage.MapiHelperInterop.MapiRecipDesc);
//                    int size = Marshal.SizeOf(type);
//
//                    // destroy all the structures in the memory area
//                    int ptr = (int)_handle;
//                    for(int i = 0; i < _count; i++)
//                    {
//                        Marshal.DestroyStructure((IntPtr)ptr, type);
//                        ptr += size;
//                    }
//
//                    // free the memory
//                    Marshal.FreeHGlobal(_handle);
//
//                    _handle = IntPtr.Zero;
//                    _count = 0;
//                }
//            }
//
//            #endregion Public Methods
//        }
//    }
//
//    #endregion Public RecipientCollection Class
//}