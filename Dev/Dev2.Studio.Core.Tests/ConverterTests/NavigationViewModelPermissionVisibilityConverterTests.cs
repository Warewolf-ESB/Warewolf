
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
            Verify_Convert(Permissions.None, Permissions.View, Visibility.Collapsed);
            Verify_Convert(Permissions.None, Permissions.Execute, Visibility.Collapsed);
            Verify_Convert(Permissions.None, Permissions.Contribute, Visibility.Collapsed);
            Verify_Convert(Permissions.None, Permissions.Administrator, Visibility.Collapsed);

            Verify_Convert(Permissions.None, Permissions.View | Permissions.Execute, Visibility.Collapsed);
            Verify_Convert(Permissions.None, Permissions.View | Permissions.DeployTo, Visibility.Collapsed);
            Verify_Convert(Permissions.None, Permissions.View | Permissions.DeployFrom, Visibility.Collapsed);
            Verify_Convert(Permissions.None, Permissions.Execute | Permissions.DeployTo, Visibility.Collapsed);
            Verify_Convert(Permissions.None, Permissions.Execute | Permissions.DeployFrom, Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsNoneAndUserPermissionIsNotViewOrExecute_Visible()
        {
            Verify_Convert(Permissions.None, Permissions.None, Visibility.Visible);
            Verify_Convert(Permissions.None, Permissions.DeployFrom, Visibility.Visible);
            Verify_Convert(Permissions.None, Permissions.DeployTo, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsViewAndUserPermissionIsView_Visible()
        {
            Verify_Convert(Permissions.View, Permissions.View, Visibility.Visible);
            Verify_Convert(Permissions.View, Permissions.Contribute, Visibility.Visible);
            Verify_Convert(Permissions.View, Permissions.Administrator, Visibility.Visible);

            Verify_Convert(Permissions.View, Permissions.View | Permissions.Execute, Visibility.Visible);
            Verify_Convert(Permissions.View, Permissions.View | Permissions.DeployTo, Visibility.Visible);
            Verify_Convert(Permissions.View, Permissions.View | Permissions.DeployFrom, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsViewAndUserPermissionIsNotView_Collapsed()
        {
            Verify_Convert(Permissions.View, Permissions.None, Visibility.Collapsed);
            Verify_Convert(Permissions.View, Permissions.Execute, Visibility.Collapsed);
            Verify_Convert(Permissions.View, Permissions.DeployFrom, Visibility.Collapsed);
            Verify_Convert(Permissions.View, Permissions.DeployTo, Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationExecuteModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsExecuteAndUserPermissionIsExecute_Visible()
        {
            Verify_Convert(Permissions.Execute, Permissions.Execute, Visibility.Visible);
            Verify_Convert(Permissions.Execute, Permissions.Contribute, Visibility.Visible);
            Verify_Convert(Permissions.Execute, Permissions.Administrator, Visibility.Visible);

            Verify_Convert(Permissions.Execute, Permissions.Execute | Permissions.View, Visibility.Visible);
            Verify_Convert(Permissions.Execute, Permissions.Execute | Permissions.DeployTo, Visibility.Visible);
            Verify_Convert(Permissions.Execute, Permissions.Execute | Permissions.DeployFrom, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationExecuteModelPermissionVisibilityConverter_Convert")]
        public void NavigationViewModelPermissionVisibilityConverter_Convert_RequiredPermissionIsExecuteAndUserPermissionIsNotExecute_Collapsed()
        {
            Verify_Convert(Permissions.Execute, Permissions.None, Visibility.Collapsed);
            Verify_Convert(Permissions.Execute, Permissions.View, Visibility.Collapsed);
            Verify_Convert(Permissions.Execute, Permissions.DeployFrom, Visibility.Collapsed);
            Verify_Convert(Permissions.Execute, Permissions.DeployTo, Visibility.Collapsed);
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
