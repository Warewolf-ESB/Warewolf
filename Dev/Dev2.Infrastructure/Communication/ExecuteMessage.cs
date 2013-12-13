using System.Text;

namespace Dev2.Communication
{
    public class ExecuteMessage
    {
        public bool HasError { get; set; }

        public StringBuilder Message { get; set; }

        public ExecuteMessage()
        {
            Message = new StringBuilder();
        }

        public void SetMessage(string message)
        {
            Message.Append(message);
        }
    }
}
