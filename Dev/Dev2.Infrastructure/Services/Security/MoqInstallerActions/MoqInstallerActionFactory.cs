
namespace Dev2.Services.Security.MoqInstallerActions
{
    public static class MoqInstallerActionFactory
    {

        public static IMoqInstallerActions CreateInstallerActions()
        {
            return new InstallerActionsForDevelopment();
        }

        public static IWarewolfSecurityOperations CreateSecurityOperationsObject()
        {
            return new WarewolfSecurityOperationsImpl();
        }
    }
}
