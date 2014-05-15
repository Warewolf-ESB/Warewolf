
namespace Dev2.InstallerActions
{
    public interface WarewolfSecurityOperations
    {
        void AddWarewolfGroup();

        bool DoesWarewolfGroupExist();

        bool IsUserInGroup(string username);

        void AddUserToWarewolf(string currentUser);

        void DeleteWarewolfGroup();

        string FormatUserForInsert(string currentUser, string machineName);
    }
}
