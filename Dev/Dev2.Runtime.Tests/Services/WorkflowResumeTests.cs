/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Moq;
using Warewolf.Security.Encryption;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class WorkflowResumeTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowResume))]
        public void WorkflowResume_Returns_HandleType_WorkflowResume()
        {
            //------------Setup for test--------------------------
            var workflowResume = new WorkflowResume();
            //------------Execute Test--------------------------- 
            //------------Assert Results-------------------------
            Assert.AreEqual("WorkflowResume", workflowResume.HandlesType());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Execute_Returns_Execution_Completed()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serializer = new Dev2JsonSerializer();
            var identity = new MockPrincipal(WindowsIdentity.GetCurrent().Name);
            var currentPrincipal = new GenericPrincipal(identity, new[] {"Role1", "Roll2"});
            Thread.CurrentPrincipal = currentPrincipal;
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(resourceID.ToString())},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(currentPrincipal.Identity.Name)}
            };
            var resourceCatalog = new Mock<IResourceCatalog>();
            var newDs = new DynamicService {Name = HandlesType(), ID = resourceID};
            var sa = new ServiceAction {Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};
            newDs.Actions.Add(sa);
            resourceCatalog.Setup(catalog => catalog.GetService(GlobalConstants.ServerWorkspaceID, It.IsAny<Guid>(), "")).Returns(newDs);


            var errors = new ErrorResultTO();
            var mockResumableExecutionContainer = new Mock<IResumableExecutionContainer>();
            mockResumableExecutionContainer.Setup(o => o.Execute(out errors, 0)).Verifiable();

            var mockResumableExecutionContainerFactory = new Mock<IResumableExecutionContainerFactory>();
            mockResumableExecutionContainerFactory.Setup(o => o.New(It.IsAny<Guid>(), It.IsAny<ServiceAction>(), It.IsAny<DsfDataObject>()))
                .Returns(mockResumableExecutionContainer.Object);
            CustomContainer.Register(mockResumableExecutionContainerFactory.Object);
            //------------Execute Test---------------------------

            var workflowResume = new WorkflowResume();
            workflowResume.ResourceCatalogInstance = resourceCatalog.Object;
            var jsonResult = workflowResume.Execute(values, null);

            //------------Assert Results-------------------------
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual("Execution Completed.", result.Message.ToString());
        }

        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Execute_WithEncryptedValues_Returns_Execution_Completed()
        {
            //------------Setup for test--------------------------
            var newexecutionEnvironment = CreateExecutionEnvironment();
            newexecutionEnvironment.Assign("[[UUID]]", "public", 0);
            newexecutionEnvironment.Assign("[[JourneyName]]", "whatever", 0);

            var resourceId = Guid.NewGuid();
            var serializer = new Dev2JsonSerializer();
            var identity = new MockPrincipal(WindowsIdentity.GetCurrent().Name);
            var currentPrincipal = new GenericPrincipal(identity, new[] {"Role1", "Roll2"});
            Thread.CurrentPrincipal = currentPrincipal;
            var user = SecurityEncryption.Encrypt(currentPrincipal.Identity.Name);
            var env = SecurityEncryption.Encrypt(newexecutionEnvironment.ToJson());
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(resourceId.ToString())},
                {"environment", new StringBuilder(env)},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(user)}
            };
            var resourceCatalog = new Mock<IResourceCatalog>();
            var newDs = new DynamicService {Name = HandlesType(), ID = resourceId};
            var sa = new ServiceAction {Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};
            newDs.Actions.Add(sa);
            resourceCatalog.Setup(catalog => catalog.GetService(GlobalConstants.ServerWorkspaceID, It.IsAny<Guid>(), "")).Returns(newDs);

            var errors = new ErrorResultTO();
            var mockResumableExecutionContainer = new Mock<IResumableExecutionContainer>();
            mockResumableExecutionContainer.Setup(o => o.Execute(out errors, 0)).Verifiable();

            var mockResumableExecutionContainerFactory = new Mock<IResumableExecutionContainerFactory>();
            mockResumableExecutionContainerFactory.Setup(o => o.New(It.IsAny<Guid>(), It.IsAny<ServiceAction>(), It.IsAny<DsfDataObject>()))
                .Returns(mockResumableExecutionContainer.Object);
            CustomContainer.Register(mockResumableExecutionContainerFactory.Object);
            //------------Execute Test---------------------------

            var workflowResume = new WorkflowResume();
            workflowResume.ResourceCatalogInstance = resourceCatalog.Object;
            var jsonResult = workflowResume.Execute(values, null);

            //------------Assert Results-------------------------
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual("Execution Completed.", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Values_versionNumber_Missing_Fails()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"currentuserprincipal", new StringBuilder("")}
            };
            var workflowResume = new WorkflowResume();
            //------------Execute Test---------------------------
            var jsonResult = workflowResume.Execute(values, null);
            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("no version Number passed", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Values_environment_Missing_Fails()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder("UserName")}
            };
            var workflowResume = new WorkflowResume();
            //------------Execute Test---------------------------
            var jsonResult = workflowResume.Execute(values, null);
            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("no environment passed", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Values_currentuserprincipal_Missing_Fails()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };
            var workflowResume = new WorkflowResume();
            //------------Execute Test---------------------------
            var jsonResult = workflowResume.Execute(values, null);
            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("no executing user principal passed", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Values_resourceID_Missing_Fails()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
            {
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder("username")}
            };
            var workflowResume = new WorkflowResume();
            //------------Execute Test---------------------------
            var jsonResult = workflowResume.Execute(values, null);
            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("resourceID is missing", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Values_resourceID_NotValidGUID_Fails()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder("currentuserprincipal")}
            };
            var workflowResume = new WorkflowResume();
            //------------Execute Test---------------------------
            var jsonResult = workflowResume.Execute(values, null);
            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("resourceID is not a valid GUID.", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Values_startActivityId_Missing_Fails()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder("currentuserprincipal")}
            };
            var workflowResume = new WorkflowResume();
            //------------Execute Test---------------------------
            var jsonResult = workflowResume.Execute(values, null);
            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("no startActivityId passed.", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Values_startActivityId_NotValidGUID_Fails()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder("currentuserprincipal")}
            };
            var workflowResume = new WorkflowResume();
            //------------Execute Test---------------------------
            var jsonResult = workflowResume.Execute(values, null);
            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("startActivityId is not a valid GUID.", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_DynamicServiceIsNull_Fails()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serializer = new Dev2JsonSerializer();
            var identity = new MockPrincipal(WindowsIdentity.GetCurrent().Name);
            var currentPrincipal = new GenericPrincipal(identity, new[] {"Role1", "Roll2"});
            Thread.CurrentPrincipal = currentPrincipal;
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(resourceID.ToString())},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(currentPrincipal.Identity.Name)}
            };
            var resourceCatalog = new Mock<IResourceCatalog>();
            var newDs = new DynamicService {Name = HandlesType(), ID = resourceID};
            newDs = null;
            resourceCatalog.Setup(catalog => catalog.GetService(GlobalConstants.ServerWorkspaceID, resourceID, "")).Returns(newDs);

            var workflowResume = new WorkflowResume();
            workflowResume.ResourceCatalogInstance = resourceCatalog.Object;
            //------------Execute Test---------------------------
            var jsonResult = workflowResume.Execute(values, null);
            //------------Assert Results-------------------------

            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.IsTrue(result.Message.ToString().Contains("Error resuming. ServiceAction is null for Resource ID"));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_ServiceActionNullForResource_Fails()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serializer = new Dev2JsonSerializer();
            var identity = new MockPrincipal(WindowsIdentity.GetCurrent().Name);
            var currentPrincipal = new GenericPrincipal(identity, new[] {"Role1", "Roll2"});
            Thread.CurrentPrincipal = currentPrincipal;
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(resourceID.ToString())},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(currentPrincipal.Identity.Name)}
            };
            var newDs = new DynamicService {Name = HandlesType()};
            var nullresourceCatalog = new Mock<IResourceCatalog>();
            nullresourceCatalog.Setup(catalog => catalog.GetService(GlobalConstants.ServerWorkspaceID, resourceID, "")).Returns(newDs);

            //------------Execute Test---------------------------

            var workflowResume = new WorkflowResume();
            workflowResume.ResourceCatalogInstance = nullresourceCatalog.Object;
            var jsonResult = workflowResume.Execute(values, null);

            //------------Assert Results-------------------------

            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.IsTrue(result.Message.ToString().Contains("Error resuming. ServiceAction is null for Resource ID"));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Execute_HasErrors_Returns_ErrorMessage()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serializer = new Dev2JsonSerializer();
            var identity = new MockPrincipal(WindowsIdentity.GetCurrent().Name);
            var currentPrincipal = new GenericPrincipal(identity, new[] {"Role1", "Roll2"});
            Thread.CurrentPrincipal = currentPrincipal;
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(resourceID.ToString())},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(currentPrincipal.Identity.Name)}
            };
            var newDs = new DynamicService {Name = HandlesType(), ID = resourceID};
            var sa = new ServiceAction {Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};
            newDs.Actions.Add(sa);
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetService(GlobalConstants.ServerWorkspaceID, It.IsAny<Guid>(), "")).Returns(newDs);


            var errors = new ErrorResultTO();
            errors.AddError("ErrorMessage");
            var mockResumableExecutionContainer = new Mock<IResumableExecutionContainer>();
            mockResumableExecutionContainer.Setup(o => o.Execute(out errors, 0)).Verifiable();

            var mockResumableExecutionContainerFactory = new Mock<IResumableExecutionContainerFactory>();
            mockResumableExecutionContainerFactory.Setup(o => o.New(It.IsAny<Guid>(), It.IsAny<ServiceAction>(), It.IsAny<DsfDataObject>()))
                .Returns(mockResumableExecutionContainer.Object);
            CustomContainer.Register(mockResumableExecutionContainerFactory.Object);
            //------------Execute Test---------------------------

            var workflowResume = new WorkflowResume();
            workflowResume.ResourceCatalogInstance = resourceCatalog.Object;
            var jsonResult = workflowResume.Execute(values, null);

            //------------Assert Results-------------------------
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("ErrorMessage", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowResume))]
        [DoNotParallelize]
        public void WorkflowResume_Execute_InvalidUserContext_Return_Authentication_Error_Fails()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serializer = new Dev2JsonSerializer();
            var currentuserprincipal = GlobalConstants.GenericPrincipal.Identity.Name;
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(resourceID.ToString())},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(currentuserprincipal)}
            };
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<Guid>())).Returns(false);

            var newDs = new DynamicService {Name = HandlesType(), ID = resourceID};
            var sa = new ServiceAction {Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};
            newDs.Actions.Add(sa);
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetService(GlobalConstants.ServerWorkspaceID, It.IsAny<Guid>(), "")).Returns(newDs);


            var errors = new ErrorResultTO();
            var mockResumableExecutionContainer = new Mock<IResumableExecutionContainer>();
            mockResumableExecutionContainer.Setup(o => o.Execute(out errors, 0)).Verifiable();

            var mockResumableExecutionContainerFactory = new Mock<IResumableExecutionContainerFactory>();
            mockResumableExecutionContainerFactory.Setup(o => o.New(It.IsAny<Guid>(), It.IsAny<ServiceAction>(), It.IsAny<DsfDataObject>()))
                .Returns(mockResumableExecutionContainer.Object);
            CustomContainer.Register(mockResumableExecutionContainerFactory.Object);
            //------------Execute Test---------------------------

            var workflowResume = new WorkflowResume();
            workflowResume.ResourceCatalogInstance = resourceCatalog.Object;
            workflowResume.AuthorizationService = authorizationService.Object;
            var jsonResult = workflowResume.Execute(values, null);

            //------------Assert Results-------------------------

            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsTrue(result.HasError);
            Assert.IsTrue(result.Message.ToString().Contains("Authentication Error resuming"));
        }

        public enum MockPrincipalBehavior
        {
            AlwaysReturnTrue,
            WhiteList,
            BlackList
        }

        public class MockPrincipal : IPrincipal, IIdentity
        {
            private HashSet<String> Roles { get; set; }
            public MockPrincipalBehavior Behavior { get; set; }

            public MockPrincipal(String name)
            {
                Roles = new HashSet<String>();
                Name = name;
                IsAuthenticated = true;
                AuthenticationType = "FakeAuthentication";
            }

            public void AddRoles(params String[] roles)
            {
                Behavior = MockPrincipalBehavior.WhiteList;

                if (roles == null || roles.Length == 0) return;

                var rolesToAdd = roles.Where(r => !Roles.Contains(r));

                foreach (var role in rolesToAdd)
                    Roles.Add(role);
            }

            public void IgnoreRoles(params String[] roles)
            {
                Behavior = MockPrincipalBehavior.BlackList;

                AddRoles(roles);
            }

            public void RemoveRoles(params String[] roles)
            {
                if (roles == null || roles.Length == 0) return;

                var rolesToAdd = roles.Where(r => Roles.Contains(r));

                foreach (var role in rolesToAdd)
                    Roles.Remove(role);
            }

            public void RemoveAllRoles()
            {
                Roles.Clear();
            }

            #region IPrincipal Members

            public IIdentity Identity
            {
                get { return this; }
            }

            public bool IsInRole(string role)
            {
                if (Behavior == MockPrincipalBehavior.AlwaysReturnTrue)
                    return true;

                var isInlist = Roles.Contains(role);

                if (Behavior == MockPrincipalBehavior.BlackList)
                    return !isInlist;

                return isInlist;
            }

            #endregion

            #region IIdentity Members

            public string AuthenticationType { get; set; }

            public bool IsAuthenticated { get; set; }

            public string Name { get; set; }

            #endregion
        }

        private static string HandlesType() => "WorkflowResume";
    }
}