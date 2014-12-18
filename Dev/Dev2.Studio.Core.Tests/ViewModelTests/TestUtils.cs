
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Linq.Expressions;
using System.Text;
using Dev2.Collections;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Interfaces;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.ViewModelTests
{
    public class TestUtils
    {
        public static Mock<ModelItem> CreateModelItem(Guid uniqueID, Guid serviceID, Guid environmentID, params ModelProperty[] modelProperties)
        {
            var startIndex = 0;
            if(modelProperties == null)
            {
                modelProperties = new ModelProperty[3];
            }
            else
            {
                startIndex = modelProperties.Length;
                Array.Resize(ref modelProperties, startIndex + 3);
            }

            modelProperties[startIndex++] = CreateModelProperty("UniqueID", uniqueID.ToString()).Object;
            modelProperties[startIndex++] = CreateModelProperty("ResourceID", serviceID).Object;
            modelProperties[startIndex] = CreateModelProperty("EnvironmentID", new InArgument<Guid>(environmentID)).Object;

            var properties = new Mock<ModelPropertyCollection>();

            foreach(var modelProperty in modelProperties)
            {
                properties.Protected().Setup<ModelProperty>("Find", modelProperty.Name, true).Returns(modelProperty);
            }

            var modelItem = new Mock<ModelItem>();
            modelItem.Setup(mi => mi.Properties).Returns(properties.Object);
            modelItem.Setup(mi => mi.ItemType).Returns(typeof(DsfActivity));
            return modelItem;
        }

        public static Mock<ModelProperty> CreateModelProperty(string name, object value)
        {
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.Name).Returns(name);
            prop.Setup(p => p.ComputedValue).Returns(value ?? "");
            return prop;
        }

        public static Mock<IContextualResourceModel> CreateResourceModel(Guid resourceID, params IErrorInfo[] resourceErrors)
        {
            return CreateResourceModel(resourceID, false, resourceErrors);
        }

        public static Mock<IContextualResourceModel> CreateResourceModel(Guid resourceID, bool resourceRepositoryReturnsNull, params IErrorInfo[] resourceErrors)
        {
            Mock<IResourceRepository> resourceRepository;
            Mock<IContextualResourceModel> resourceModel = CreateResourceModel(resourceID, out resourceRepository, resourceErrors);
            resourceRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(resourceRepositoryReturnsNull ? null : resourceModel.Object);
            return resourceModel;
        }

        public static Mock<IContextualResourceModel> CreateResourceModel(Guid resourceID, out Mock<IResourceRepository> resourceRepository, params IErrorInfo[] resourceErrors)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);

            var errors = new ObservableReadOnlyList<IErrorInfo>();
            if(resourceErrors != null)
            {
                foreach(var resourceError in resourceErrors)
                {
                    errors.Add(resourceError);
                }
            }

            var model = new Mock<IContextualResourceModel>();
            model.Setup(r => r.ResourceName).Returns("TestResource");
            model.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            model.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            model.Setup(m => m.Errors).Returns(errors);
            model.Setup(m => m.ID).Returns(resourceID);
            model.Setup(m => m.Environment).Returns(environment.Object);
            model.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            model.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceRepository = new Mock<IResourceRepository>();

            environment.Setup(e => e.ResourceRepository).Returns(resourceRepository.Object);
            return model;
        }
    }
}
