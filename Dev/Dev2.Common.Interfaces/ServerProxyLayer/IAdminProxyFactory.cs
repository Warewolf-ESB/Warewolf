namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    public interface IAdminProxyFactory
    {

        IAdminManager CreateAdminManager();
        ILoggingManager CreateLoggingManager();
    }
}