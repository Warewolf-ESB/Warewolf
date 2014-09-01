using System.Threading.Tasks;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Communication;

namespace Dev2.Services
{
    public class PushService : IPushService
    {
        readonly ISerializer _serializer;

        public PushService()
            : this(new Dev2JsonSerializer())
        {
        }

        public PushService(ISerializer serializer)
        {
            VerifyArgument.IsNotNull("serializer", serializer);
            _serializer = serializer;
        }

        #region Implementation of IPushService

        public Task<string> ProcessRequest(IHostContext context, string jsonObj)
        {
            VerifyArgument.IsNotNull("context", context);
            VerifyArgument.IsNotNullOrWhitespace("jsonObj", jsonObj);

            return new Task<string>(() =>
            {
                var memo = (IMemo)Memo.Parse(_serializer, jsonObj);

                // TODO: process message

                return memo.ToString(_serializer);
            });
        }

        #endregion
    }
}