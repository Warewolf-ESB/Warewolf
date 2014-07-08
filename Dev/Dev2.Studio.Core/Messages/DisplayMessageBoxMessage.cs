using System.Windows;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class DisplayMessageBoxMessage
    {
        public string Heading { get; set; }
        public string Message { get; set; }
        public MessageBoxImage MessageBoxImage { get; set; }

        public DisplayMessageBoxMessage(string heading, string message, MessageBoxImage messageBoxImage)
        {
            Heading = heading;
            Message = message;
            MessageBoxImage = messageBoxImage;
        }
    }
}