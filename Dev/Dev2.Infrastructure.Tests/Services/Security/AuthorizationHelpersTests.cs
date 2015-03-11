
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Services.Security;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class AuthorizationHelpersTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_ToReason")]
        public void AuthorizationHelpers_ToReason_IsAuthorized_Null()
        {
            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                //------------Execute Test---------------------------
                var reason = context.ToReason(true);

                //------------Assert Results-------------------------
                Assert.IsNull(reason);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_ToReason")]
        public void AuthorizationHelpers_ToReason_IsNotAuthorized_CorrectReason()
        {
            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                //------------Execute Test---------------------------
                var reason = context.ToReason();

                //------------Assert Results-------------------------
                switch(context)
                {
                    case AuthorizationContext.None:
                        Assert.AreEqual("You are not authorized.", reason);
                        break;
                    case AuthorizationContext.View:
                        Assert.AreEqual("You are not authorized to view this resource.", reason);
                        break;
                    case AuthorizationContext.Execute:
                        Assert.AreEqual("You are not authorized to execute this resource.", reason);
                        break;
                    case AuthorizationContext.Contribute:
                        Assert.AreEqual("You are not authorized to add, update, delete or save this resource.", reason);
                        break;
                    case AuthorizationContext.DeployTo:
                        Assert.AreEqual("You are not authorized to deploy to this server.", reason);
                        break;
                    case AuthorizationContext.DeployFrom:
                        Assert.AreEqual("You are not authorized to deploy from this server.", reason);
                        break;
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_Matches")]
        public void AuthorizationHelpers_Matches_SecurityPermissionIsServer_True()
        {
            //------------Setup for test--------------------------
            var securityPermission = new WindowsGroupPermission { IsServer = true };

            //------------Execute Test---------------------------
            var authorized = securityPermission.Matches(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_Matches")]
        public void AuthorizationHelpers_Matches_ResourceIsNullOrEmpty_False()
        {
            //------------Setup for test--------------------------
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceName = "CATEGORY\\TEST2" };

            //------------Execute Test---------------------------
            var authorized = securityPermission.Matches(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_Matches")]
        public void AuthorizationHelpers_Matches_ResourceIsGuidAndHasSecurityPermission_True()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resourceID };

            //------------Execute Test---------------------------
            var authorized = securityPermission.Matches(resourceID.ToString());

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_Matches")]
        public void AuthorizationHelpers_Matches_ResourceIsGuidAndDoesNotHaveSecurityPermissions_False()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = Guid.NewGuid() };

            //------------Execute Test---------------------------
            var authorized = securityPermission.Matches(resourceID.ToString());

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_Matches")]
        public void AuthorizationHelpers_Matches_ResourceIsStringAndHasSecurityPermission_True()
        {
            //------------Setup for test--------------------------
            const string ResourceName = "Test";
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceName = "CATEGORY\\" + ResourceName };

            //------------Execute Test---------------------------
            var authorized = securityPermission.Matches(ResourceName);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_Matches")]
        public void AuthorizationHelpers_Matches_ResourceIsStringAndDoesNotHaveSecurityPermissions_False()
        {
            //------------Setup for test--------------------------
            const string ResourceName = "Test";
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceName = "CATEGORY\\TEST2" };

            //------------Execute Test---------------------------
            var authorized = securityPermission.Matches(ResourceName);

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_ToPermissions")]
        public void AuthorizationHelpers_ToPermissions_AuthorizationContextCorrectlyTranslated()
        {
            Verify_ToPermissions(AuthorizationContext.None, Permissions.None);
            Verify_ToPermissions(AuthorizationContext.View, Permissions.Administrator | Permissions.Contribute | Permissions.View);
            Verify_ToPermissions(AuthorizationContext.Execute, Permissions.Administrator | Permissions.Contribute | Permissions.Execute);
            Verify_ToPermissions(AuthorizationContext.Contribute, Permissions.Administrator | Permissions.Contribute);
            Verify_ToPermissions(AuthorizationContext.DeployTo, Permissions.Administrator | Permissions.DeployTo);
            Verify_ToPermissions(AuthorizationContext.DeployFrom, Permissions.Administrator | Permissions.DeployFrom);
            Verify_ToPermissions(AuthorizationContext.Administrator, Permissions.Administrator);
            Verify_ToPermissions(AuthorizationContext.Any, Permissions.Administrator | Permissions.View | Permissions.Contribute | Permissions.Execute | Permissions.DeployFrom | Permissions.DeployTo);
        }

        void Verify_ToPermissions(AuthorizationContext authorizationContext, Permissions expectedPermissions)
        {
            //------------Execute Test---------------------------
            var actual = authorizationContext.ToPermissions();

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedPermissions, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_IsContributor")]
        public void AuthorizationHelpers_IsContributor_CorrectlyTranslated()
        {
            Verify_IsContributor(Permissions.Administrator | Permissions.View, true);
            Verify_IsContributor(Permissions.Contribute | Permissions.View, true);
            Verify_IsContributor(Permissions.View | Permissions.Execute, false);
        }

        void Verify_IsContributor(Permissions permissions, bool expected)
        {
            //------------Execute Test---------------------------
            var actual = permissions.IsContributor();

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationHelpers_CanDebug")]
        public void AuthorizationHelpers_CanDebug_CorrectlyTranslated()
        {
            Verify_CanDebug(Permissions.Administrator | Permissions.View, true);
            Verify_CanDebug(Permissions.Contribute | Permissions.View, true);
            Verify_CanDebug(Permissions.View | Permissions.Execute, true);
            Verify_CanDebug(Permissions.View, false);
            Verify_CanDebug(Permissions.Execute, false);
        }

        void Verify_CanDebug(Permissions permissions, bool expected)
        {
            //------------Execute Test---------------------------
            var actual = permissions.CanDebug();

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
