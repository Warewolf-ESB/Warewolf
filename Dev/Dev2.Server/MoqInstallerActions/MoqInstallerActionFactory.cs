
namespace Dev2.InstallerActions
{
    public class MoqInstallerActionFactory
    {

        public static MoqInstallerActions CreateInstallerActions()
        {
            return new InstallerActionsForDevelopment();
        }

        public static WarewolfSecurityOperations CreateSecurityOperationsObject()
        {
            return new WarewolfSecurityOperationsImpl();
        }
    }
}
