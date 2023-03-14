using System;

namespace Warewolf.Auditing
{
	[Serializable]
	public class SerializableException
	{
		public string Message;
		string[] Data;
		Exception InnerException;
		string TargetSite;
		string StackTrace;
		string HelpLink;
		string Source;
		int HResult;

		public SerializableException(Exception exception)
		{
			Message = exception.Message.ToString();
			Data = new string[0];
			InnerException = exception.InnerException;
			TargetSite = exception.TargetSite?.ToString();
			StackTrace = exception.StackTrace;
			HelpLink = exception.HelpLink;
			Source = exception.Source;
			HResult = 1;
		}
		public SerializableException()
		{
			Message = string.Empty;
			InitializeEmptyException();
		}

		public SerializableException(string message)
		{
			Message = message;
			InitializeEmptyException();
		}

		void InitializeEmptyException()
		{
			Data = new string[0];
			InnerException = null;
			TargetSite = string.Empty;
			StackTrace = string.Empty;
			HelpLink = string.Empty;
			Source = string.Empty;
			HResult = 1;
		}
	}
}