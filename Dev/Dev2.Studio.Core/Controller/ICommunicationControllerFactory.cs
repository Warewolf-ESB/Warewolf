namespace Dev2.Controller
{
    public interface ICommunicationControllerFactory
    {
        ICommunicationController CreateController(string serviceName);
    }

    public class CommunicationControllerFactory : ICommunicationControllerFactory
    {
        public ICommunicationController CreateController(string serviceName)
        {
            return new CommunicationController { ServiceName = serviceName };
        }
    }
}
