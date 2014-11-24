
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Forms;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Dialogs;
using Dev2.Services.Security;
using Dev2.Settings.Security;
using Dev2.Studio.Core.Interfaces;
using Moq;

namespace Dev2.Core.Tests.Settings
{
    public class TestSecurityViewModel : SecurityViewModel
    {
        public TestSecurityViewModel(SecuritySettingsTO securitySettings, IResourcePickerDialog resourcePicker, DirectoryObjectPickerDialog directoryObjectPicker, IWin32Window parentWindow, IEnvironmentModel environment)
            : base(securitySettings, resourcePicker, directoryObjectPicker, parentWindow, environment)
        {
        }

        public TestSecurityViewModel()
            : base(new SecuritySettingsTO(), new Mock<IResourcePickerDialog>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object)
        {
        }

        public int SaveHitCount { get; private set; }
        public override void Save(SecuritySettingsTO securitySettings)
        {
            SaveHitCount++;
            base.Save(securitySettings);
        }

        #region Overrides of SecurityViewModel

        public DialogResult Result;
        public override DialogResult ShowDirectoryObjectPickerDialog(IWin32Window parentWindow)
        {
            return Result;
        }

        public DirectoryObject[] SelectedObjects;
        public override DirectoryObject[] GetSelectedObjectsFromDirectoryObjectPickerDialog()
        {
            return SelectedObjects;
        }

        #endregion
    }
}
