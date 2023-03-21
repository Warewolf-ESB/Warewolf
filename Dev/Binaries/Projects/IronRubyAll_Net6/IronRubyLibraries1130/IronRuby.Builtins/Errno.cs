using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("Errno")]
	public static class Errno
	{
		[Serializable]
		[RubyClass("EAGAIN")]
		public class ResourceTemporarilyUnavailableError : ExternalException
		{
			private const string M = "Resource temporarily unavailable";

			public ResourceTemporarilyUnavailableError()
				: this(null, null)
			{
			}

			public ResourceTemporarilyUnavailableError(string message)
				: this(message, null)
			{
			}

			public ResourceTemporarilyUnavailableError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "Resource temporarily unavailable"), inner)
			{
			}

			public ResourceTemporarilyUnavailableError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "Resource temporarily unavailable"))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected ResourceTemporarilyUnavailableError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("EINTR")]
		public class InterruptedError : ExternalException
		{
			private const string M = "Interrupted function call";

			public InterruptedError()
				: this(null, null)
			{
			}

			public InterruptedError(string message)
				: this(message, null)
			{
			}

			public InterruptedError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "Interrupted function call"), inner)
			{
			}

			public InterruptedError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "Interrupted function call"))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected InterruptedError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("EDOM")]
		public class DomainError : ExternalException
		{
			private const string M = "Domain error";

			public DomainError()
				: this(null, null)
			{
			}

			public DomainError(string message)
				: this(message, null)
			{
			}

			public DomainError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "Domain error"), inner)
			{
			}

			public DomainError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "Domain error"))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected DomainError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[RubyClass("EINVAL", Extends = typeof(InvalidError), Inherits = typeof(ExternalException))]
		public class InvalidErrorOps
		{
			[RubyConstructor]
			public static InvalidError Create(RubyClass self, [DefaultProtocol] MutableString message)
			{
				InvalidError invalidError = new InvalidError(RubyExceptions.MakeMessage(ref message, "Invalid Argument"));
				RubyExceptionData.InitializeException(invalidError, message);
				return invalidError;
			}
		}

		[RubyClass("ENOENT", Extends = typeof(FileNotFoundException), Inherits = typeof(ExternalException))]
		public class FileNotFoundExceptionOps
		{
			[RubyConstructor]
			public static FileNotFoundException Create(RubyClass self, [DefaultProtocol] MutableString message)
			{
				FileNotFoundException ex = new FileNotFoundException(RubyExceptions.MakeMessage(ref message, "No such file or directory"));
				RubyExceptionData.InitializeException(ex, message);
				return ex;
			}
		}

		[RubyClass("ENOTDIR", Extends = typeof(DirectoryNotFoundException), Inherits = typeof(ExternalException))]
		public class DirectoryNotFoundExceptionOps
		{
			[RubyConstructor]
			public static DirectoryNotFoundException Create(RubyClass self, [DefaultProtocol] MutableString message)
			{
				DirectoryNotFoundException ex = new DirectoryNotFoundException(RubyExceptions.MakeMessage(ref message, "Not a directory"));
				RubyExceptionData.InitializeException(ex, message);
				return ex;
			}
		}

		[RubyClass("EACCES", Extends = typeof(UnauthorizedAccessException), Inherits = typeof(ExternalException))]
		public class UnauthorizedAccessExceptionOps
		{
			[RubyConstructor]
			public static UnauthorizedAccessException Create(RubyClass self, [DefaultProtocol] MutableString message)
			{
				UnauthorizedAccessException ex = new UnauthorizedAccessException(RubyExceptions.MakeMessage(ref message, "Permission denied"));
				RubyExceptionData.InitializeException(ex, message);
				return ex;
			}
		}

		[Serializable]
		[RubyClass("ECHILD")]
		public class ChildError : ExternalException
		{
			private const string M = "No child processes";

			public ChildError()
				: this(null, null)
			{
			}

			public ChildError(string message)
				: this(message, null)
			{
			}

			public ChildError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "No child processes"), inner)
			{
			}

			public ChildError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "No child processes"))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected ChildError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[RubyClass("EEXIST", Extends = typeof(ExistError), Inherits = typeof(ExternalException))]
		public class ExistErrorOps
		{
			[RubyConstructor]
			public static ExistError Create(RubyClass self, [DefaultProtocol] MutableString message)
			{
				ExistError existError = new ExistError(RubyExceptions.MakeMessage(ref message, "File exists"));
				RubyExceptionData.InitializeException(existError, message);
				return existError;
			}
		}

		[RubyClass("EBADF", Extends = typeof(BadFileDescriptorError), Inherits = typeof(ExternalException))]
		public class BadFileDescriptorErrorOps
		{
			[RubyConstructor]
			public static BadFileDescriptorError Create(RubyClass self, [DefaultProtocol] MutableString message)
			{
				BadFileDescriptorError badFileDescriptorError = new BadFileDescriptorError(RubyExceptions.MakeMessage(ref message, "Bad file descriptor"));
				RubyExceptionData.InitializeException(badFileDescriptorError, message);
				return badFileDescriptorError;
			}
		}

		[RubyClass("ENOEXEC", Extends = typeof(ExecFormatError), Inherits = typeof(ExternalException))]
		public class ExecFormatErrorOps
		{
			[RubyConstructor]
			public static BadFileDescriptorError Create(RubyClass self, [DefaultProtocol] MutableString message)
			{
				BadFileDescriptorError badFileDescriptorError = new BadFileDescriptorError(RubyExceptions.MakeMessage(ref message, "Exec format error"));
				RubyExceptionData.InitializeException(badFileDescriptorError, message);
				return badFileDescriptorError;
			}
		}

		[Serializable]
		[RubyClass("EPIPE")]
		public class PipeError : ExternalException
		{
			private const string M = "Broken pipe";

			public PipeError()
				: this(null, null)
			{
			}

			public PipeError(string message)
				: this(message, null)
			{
			}

			public PipeError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "Broken pipe"), inner)
			{
			}

			public PipeError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "Broken pipe"))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected PipeError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("EXDEV")]
		public class ImproperLinkError : ExternalException
		{
			private const string M = "Improper link";

			public ImproperLinkError()
				: this(null, null)
			{
			}

			public ImproperLinkError(string message)
				: this(message, null)
			{
			}

			public ImproperLinkError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "Improper link"), inner)
			{
			}

			public ImproperLinkError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "Improper link"))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected ImproperLinkError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("ESPIPE")]
		public class InvalidSeekError : ExternalException
		{
			private const string M = "Invalid seek";

			public InvalidSeekError()
				: this(null, null)
			{
			}

			public InvalidSeekError(string message)
				: this(message, null)
			{
			}

			public InvalidSeekError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "Invalid seek"), inner)
			{
			}

			public InvalidSeekError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "Invalid seek"))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected InvalidSeekError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("EWOULDBLOCK")]
		public class WouldBlockError : ExternalException
		{
			private const string M = "A non-blocking socket operation could not be completed immediately.";

			public override int ErrorCode
			{
				get
				{
					return 10035;
				}
			}

			public WouldBlockError()
				: this(null, null)
			{
			}

			public WouldBlockError(string message)
				: this(message, null)
			{
			}

			public WouldBlockError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "A non-blocking socket operation could not be completed immediately."), inner)
			{
			}

			public WouldBlockError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "A non-blocking socket operation could not be completed immediately."))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected WouldBlockError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("EADDRINUSE")]
		public class AddressInUseError : ExternalException
		{
			private const string M = "Only one usage of each socket address (protocol/network address/port) is normally permitted.";

			public override int ErrorCode
			{
				get
				{
					return 10048;
				}
			}

			public AddressInUseError()
				: this(null, null)
			{
			}

			public AddressInUseError(string message)
				: this(message, null)
			{
			}

			public AddressInUseError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "Only one usage of each socket address (protocol/network address/port) is normally permitted."), inner)
			{
			}

			public AddressInUseError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "Only one usage of each socket address (protocol/network address/port) is normally permitted."))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected AddressInUseError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("ECONNABORTED")]
		public class ConnectionAbortedError : ExternalException
		{
			private const string M = "An established connection was aborted by the software in your host machine.";

			public override int ErrorCode
			{
				get
				{
					return 10053;
				}
			}

			public ConnectionAbortedError()
				: this(null, null)
			{
			}

			public ConnectionAbortedError(string message)
				: this(message, null)
			{
			}

			public ConnectionAbortedError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "An established connection was aborted by the software in your host machine."), inner)
			{
			}

			public ConnectionAbortedError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "An established connection was aborted by the software in your host machine."))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected ConnectionAbortedError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("ECONNRESET")]
		public class ConnectionResetError : ExternalException
		{
			private const string M = "An existing connection was forcibly closed by the remote host.";

			public override int ErrorCode
			{
				get
				{
					return 10054;
				}
			}

			public ConnectionResetError()
				: this(null, null)
			{
			}

			public ConnectionResetError(string message)
				: this(message, null)
			{
			}

			public ConnectionResetError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "An existing connection was forcibly closed by the remote host."), inner)
			{
			}

			public ConnectionResetError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "An existing connection was forcibly closed by the remote host."))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected ConnectionResetError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("ENOTCONN")]
		public class NotConnectedError : ExternalException
		{
			private const string M = "A request to send or receive data was disallowed because the socket is not connected and (when sending on a datagram socket using a sendto call) no address was supplied.";

			public override int ErrorCode
			{
				get
				{
					return 10057;
				}
			}

			public NotConnectedError()
				: this(null, null)
			{
			}

			public NotConnectedError(string message)
				: this(message, null)
			{
			}

			public NotConnectedError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "A request to send or receive data was disallowed because the socket is not connected and (when sending on a datagram socket using a sendto call) no address was supplied."), inner)
			{
			}

			public NotConnectedError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "A request to send or receive data was disallowed because the socket is not connected and (when sending on a datagram socket using a sendto call) no address was supplied."))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected NotConnectedError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("ECONNREFUSED")]
		public class ConnectionRefusedError : ExternalException
		{
			private const string M = "No connection could be made because the target machine actively refused it.";

			public override int ErrorCode
			{
				get
				{
					return 10061;
				}
			}

			public ConnectionRefusedError()
				: this(null, null)
			{
			}

			public ConnectionRefusedError(string message)
				: this(message, null)
			{
			}

			public ConnectionRefusedError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "No connection could be made because the target machine actively refused it."), inner)
			{
			}

			public ConnectionRefusedError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "No connection could be made because the target machine actively refused it."))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected ConnectionRefusedError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[Serializable]
		[RubyClass("EHOSTDOWN")]
		public class HostDownError : ExternalException
		{
			private const string M = "A socket operation failed because the destination host was down.";

			public override int ErrorCode
			{
				get
				{
					return 10064;
				}
			}

			public HostDownError()
				: this(null, null)
			{
			}

			public HostDownError(string message)
				: this(message, null)
			{
			}

			public HostDownError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "A socket operation failed because the destination host was down."), inner)
			{
			}

			public HostDownError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "A socket operation failed because the destination host was down."))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected HostDownError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		internal static UnauthorizedAccessException CreateEACCES()
		{
			return new UnauthorizedAccessException();
		}

		internal static UnauthorizedAccessException CreateEACCES(string message)
		{
			return new UnauthorizedAccessException(message);
		}

		internal static UnauthorizedAccessException CreateEACCES(string message, Exception inner)
		{
			return new UnauthorizedAccessException(message, inner);
		}
	}
}
