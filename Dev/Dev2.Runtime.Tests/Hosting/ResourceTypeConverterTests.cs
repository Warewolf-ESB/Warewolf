using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]    
    public class ResourceTypeConverterTests
    {
        #region ToResourceTypes

        [TestMethod]
        public void ResourceTypeConverterToResourceTypesWithTypeWorkflowServiceReturnsCorrectResourceTypes()
        {
            var result = ResourceTypeConverter.ToResourceTypes(ResourceTypeConverter.TypeWorkflowService);
            Assert.IsTrue(new[] { ResourceType.WorkflowService }.SequenceEqual(result));
        }

        [TestMethod]
        public void ResourceTypeConverterToResourceTypesWithTypeServiceReturnsCorrectResourceTypes()
        {
            var values = Enum.GetValues(typeof(ResourceType));
            var expected = values.Cast<ResourceType>().Where(
                value => value.ToString().EndsWith("Service")
                         && !new[] { ResourceType.WorkflowService, ResourceType.ReservedService }.Contains(value)).ToList();

            var result = ResourceTypeConverter.ToResourceTypes(ResourceTypeConverter.TypeService);
            Assert.IsTrue(expected.SequenceEqual(result));
        }

        [TestMethod]
        public void ResourceTypeConverterToResourceTypesWithTypeSourceReturnsCorrectResourceTypes()
        {
            var values = Enum.GetValues(typeof(ResourceType));
            var expected = values.Cast<ResourceType>().Where(value => value.ToString().EndsWith("Source")).ToList();
            expected.Insert(0, ResourceType.Server); // order matters!

            var result = ResourceTypeConverter.ToResourceTypes(ResourceTypeConverter.TypeSource);
            Assert.IsTrue(expected.SequenceEqual(result));
        }

        [TestMethod]
        public void ResourceTypeConverterToResourceTypesWithTypeReservedServiceReturnsCorrectResourceTypes()
        {
            var result = ResourceTypeConverter.ToResourceTypes(ResourceTypeConverter.TypeReservedService);
            Assert.IsTrue(new[] { ResourceType.ReservedService }.SequenceEqual(result));
        }

        [TestMethod]
        public void ResourceTypeConverterToResourceTypesWithTypeStarAndReturnAllWhenNoMatchIsTrueReturnsCorrectResourceTypes()
        {
            var values = Enum.GetValues(typeof(ResourceType));
            var expected = values.Cast<ResourceType>();
            var result = ResourceTypeConverter.ToResourceTypes("*");
            Assert.IsTrue(expected.SequenceEqual(result));
        }

        [TestMethod]
        public void ResourceTypeConverterToResourceTypesWithTypeStarAndReturnAllWhenNoMatchIsFalseReturnsEmptyList()
        {
            var expected = new ResourceType[0];
            var result = ResourceTypeConverter.ToResourceTypes("*", false);
            Assert.IsTrue(expected.SequenceEqual(result));
        }

        #endregion

        #region ToTypeString

        [TestMethod]
        public void ResourceTypeConverterToTypeStringReturnsCorrectResourceTypeStrings()
        {
            var values = Enum.GetValues(typeof(ResourceType));
            var resourceTypes = values.Cast<ResourceType>();

            foreach(var resourceType in resourceTypes)
            {
                var result = ResourceTypeConverter.ToTypeString(resourceType);
                if(resourceType == ResourceType.WorkflowService)
                {
                    Assert.AreEqual(ResourceTypeConverter.TypeWorkflowService, result);
                }
                else if(resourceType == ResourceType.ReservedService)
                {
                    Assert.AreEqual(ResourceTypeConverter.TypeReservedService, result);
                }
                else if(resourceType == ResourceType.Server || resourceType.ToString().EndsWith("Source"))
                {
                    Assert.AreEqual(ResourceTypeConverter.TypeSource, result);
                }
                else if(resourceType.ToString().EndsWith("Service"))
                {
                    Assert.AreEqual(ResourceTypeConverter.TypeService, result);
                }
                else
                {
                    Assert.AreEqual(ResourceTypeConverter.TypeWildcard, result);
                }

            }
        }

        #endregion

        #region ToResourceType

        [TestMethod]
        public void ResourceTypeConverterToResourceTypeReturnsCorrectResourceType()
        {
            var values = Enum.GetValues(typeof(enSourceType));
            var sourceTypes = values.Cast<enSourceType>();

            foreach(var sourceType in sourceTypes)
            {
                var actual = ResourceTypeConverter.ToResourceType(sourceType);
                switch(sourceType)
                {
                    case enSourceType.SqlDatabase:
                    case enSourceType.MySqlDatabase:
                        Assert.AreEqual(ResourceType.DbSource, actual);
                        break;

                    case enSourceType.Plugin:
                        Assert.AreEqual(ResourceType.PluginSource, actual);
                        break;

                    case enSourceType.Dev2Server:
                        Assert.AreEqual(ResourceType.Server, actual);
                        break;

                    case enSourceType.EmailSource:
                        Assert.AreEqual(ResourceType.EmailSource, actual);
                        break;

                    case enSourceType.WebSource:
                        Assert.AreEqual(ResourceType.WebSource, actual);
                        break;

                    case enSourceType.WebService:
                        Assert.AreEqual(ResourceType.WebService, actual);
                        break;
                }
            }
        }

        #endregion
    }
}
