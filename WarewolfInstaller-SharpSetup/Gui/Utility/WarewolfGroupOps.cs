
using System;
using System.DirectoryServices;

namespace Gui.Utility
{
    public class WarewolfGroupOps
    {

        public void AddWarewolfGroup()
        {
            var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            DirectoryEntry newGroup = ad.Children.Add("Warewolf", "group");
            newGroup.Invoke("Put", new object[] { "Description", "Warewolf Group" });
            newGroup.CommitChanges();
        }
    }
}
