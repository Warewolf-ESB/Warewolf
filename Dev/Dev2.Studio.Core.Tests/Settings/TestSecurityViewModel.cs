/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Studio.Interfaces;
using Moq;

namespace Dev2.Core.Tests.Settings
{
    public class TestSecurityViewModel : SecurityViewModel
    {
        public TestSecurityViewModel(SecuritySettingsTO securitySettings, DirectoryObjectPickerDialog directoryObjectPicker, IWin32Window parentWindow, IServer environment)
            : base(securitySettings, directoryObjectPicker, parentWindow, environment, () => new Mock<IResourcePickerDialog>().Object)
        {
        }

        public TestSecurityViewModel()
            : base(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object,() => new Mock<IResourcePickerDialog>().Object)
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
