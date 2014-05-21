
namespace Dev2.MoqInstallerActions
{
    public class MoqInstallerActionFactory
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
