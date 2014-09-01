using System.Text;

namespace Dev2.Common.Interfaces.Infrastructure.Communication
{
    public interface IExecuteMessage
    {
        bool HasError { get; set; }
        StringBuilder Message { get; set; }

        void SetMessage(string message);
    }
}