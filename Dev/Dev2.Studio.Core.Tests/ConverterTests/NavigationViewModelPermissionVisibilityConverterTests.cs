
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Globalization;
using System.Windows;
using Dev2.Common.Interfaces.Security;
using Dev2.Services.Security;
using Dev2.Studio.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    public class NavigationViewModelPermissionVisibilityConverterTests
    {

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var converter = new NavigationViewModelPermissionVisibilityConverter();

            //------------Assert Results-------------------------
            Assert.AreEqual(converter.DefaultValue, Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_ConvertBack")]
        public void NavigationViewModelPermissionVisibilityConverter_ConvertBack_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var converter = new NavigationViewModelPermissionVisibilityConverter();

            //------------Execute Test---------------------------
            var actual = converter.Convert(null, typeof(Visibility), null, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.AreEqual(converter.DefaultValue, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_ValueIsNull_DefaultValue()
        {
            //------------Setup for test--------------------------
            var converter = new NavigationViewModelPermissionVisibilityConverter();

            //------------Execute Test---------------------------
            var actual = converter.ConvertBack(null, typeof(Visibility), null, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.IsNull(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsNoneAndUserPermissionIsViewOrExecute_Collapsed()
        {
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.View, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.Execute, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.Contribute, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.Administrator, expected: Visibility.Collapsed);

            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.View | Permissions.Execute, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.View | Permissions.DeployTo, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.View | Permissions.DeployFrom, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.Execute | Permissions.DeployTo, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.Execute | Permissions.DeployFrom, expected: Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsNoneAndUserPermissionIsNotViewOrExecute_Visible()
        {
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.None, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.DeployFrom, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.None, userPermissions: Permissions.DeployTo, expected: Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsViewAndUserPermissionIsView_Visible()
        {
            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.View, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.Contribute, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.Administrator, expected: Visibility.Visible);

            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.View | Permissions.Execute, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.View | Permissions.DeployTo, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.View | Permissions.DeployFrom, expected: Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsViewAndUserPermissionIsNotView_Collapsed()
        {
            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.None, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.Execute, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.DeployFrom, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.View, userPermissions: Permissions.DeployTo, expected: Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationExecuteModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsExecuteAndUserPermissionIsExecute_Visible()
        {
            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.Execute, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.Contribute, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.Administrator, expected: Visibility.Visible);

            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.Execute | Permissions.View, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.Execute | Permissions.DeployTo, expected: Visibility.Visible);
            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.Execute | Permissions.DeployFrom, expected: Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationExecuteModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsExecuteAndUserPermissionIsNotExecute_Collapsed()
        {
            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.None, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.View, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.DeployFrom, expected: Visibility.Collapsed);
            Verify_Convert(requiredPermission: Permissions.Execute, userPermissions: Permissions.DeployTo, expected: Visibility.Collapsed);
        }

        static void Verify_Convert(Permissions requiredPermission, Permissions userPermissions, Visibility expected)
        {
            //------------Setup for test--------------------------
            var converter = new NavigationViewModelPermissionVisibilityConverter();

            //------------Execute Test---------------------------
            var actual = converter.Convert(userPermissions, typeof(Visibility), requiredPermission.ToString(), CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
