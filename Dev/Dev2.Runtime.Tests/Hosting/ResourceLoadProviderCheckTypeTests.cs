/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Reflection;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Hosting
{
    /// <summary>
    /// Targeted unit tests for <c>ResourceLoadProvider.CheckType&lt;T&gt;</c>
    /// (Crap Score 506, Cyclomatic Complexity 22 on the coverage dashboard).
    ///
    /// CheckType is a private static helper guarding the typed-resource
    /// resolution path. It returns true when a typed lookup of T should be
    /// satisfied by the supplied resource. There are seven distinct branches:
    ///   1.  resource is null                              -> false
    ///   2.  T == DbService     &amp;&amp; ResourceType match  -> true
    ///   3.  T == PluginService &amp;&amp; ResourceType match  -> true
    ///   4.  T == WebService    &amp;&amp; ResourceType match  -> true
    ///   5.  T == Workflow      &amp;&amp; IsService          -> true
    ///   6.  T : IResourceSource &amp;&amp; IsSource          -> true
    ///   7.  otherwise                                     -> false
    /// Each branch (and its negation) is asserted below by invoking
    /// the method through reflection so we can exercise it without
    /// constructing the full ResourceLoadProvider graph.
    /// </summary>
    [TestClass]
    public class ResourceLoadProviderCheckTypeTests
    {
        // Cache the open generic MethodInfo once for the whole fixture.
        private static readonly MethodInfo CheckTypeOpenGeneric = ResolveCheckType();

        private static MethodInfo ResolveCheckType()
        {
            var mi = typeof(ResourceLoadProvider).GetMethod(
                "CheckType",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(mi, "Could not locate ResourceLoadProvider.CheckType<T> via reflection - has it been renamed or moved?");
            return mi!;
        }

        private static bool Invoke<T>(IResource? resource) where T : Resource, new()
        {
            var closed = CheckTypeOpenGeneric.MakeGenericMethod(typeof(T));
            return (bool)closed.Invoke(null, new object?[] { resource })!;
        }

        private static Mock<IResource> ResourceOfType(string resourceType, bool isService = false, bool isSource = false)
        {
            var m = new Mock<IResource>();
            m.SetupGet(r => r.ResourceType).Returns(resourceType);
            m.SetupGet(r => r.IsService).Returns(isService);
            m.SetupGet(r => r.IsSource).Returns(isSource);
            return m;
        }

        // -----------------------------------------------------------------
        // Branch 1: null guard
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_NullResource_AnyType_ReturnsFalse()
        {
            Assert.IsFalse(Invoke<DbService>(null));
            Assert.IsFalse(Invoke<PluginService>(null));
            Assert.IsFalse(Invoke<WebService>(null));
            Assert.IsFalse(Invoke<Workflow>(null));
            Assert.IsFalse(Invoke<DbSource>(null));
            Assert.IsFalse(Invoke<WebSource>(null));
            Assert.IsFalse(Invoke<SharepointSource>(null));
        }

        // -----------------------------------------------------------------
        // Branches 2-4: DbService / PluginService / WebService ResourceType match
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_DbService_WithMatchingResourceType_ReturnsTrue()
        {
            var resource = ResourceOfType("DbService").Object;

            Assert.IsTrue(Invoke<DbService>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_DbService_WithWrongResourceType_ReturnsFalse()
        {
            var resource = ResourceOfType("PluginService").Object;

            Assert.IsFalse(Invoke<DbService>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_PluginService_WithMatchingResourceType_ReturnsTrue()
        {
            var resource = ResourceOfType("PluginService").Object;

            Assert.IsTrue(Invoke<PluginService>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_PluginService_WithWrongResourceType_ReturnsFalse()
        {
            var resource = ResourceOfType("DbService").Object;

            Assert.IsFalse(Invoke<PluginService>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_WebService_WithMatchingResourceType_ReturnsTrue()
        {
            var resource = ResourceOfType("WebService").Object;

            Assert.IsTrue(Invoke<WebService>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_WebService_WithWrongResourceType_ReturnsFalse()
        {
            var resource = ResourceOfType("DbService").Object;

            Assert.IsFalse(Invoke<WebService>(resource));
        }

        // -----------------------------------------------------------------
        // Branch 5: Workflow + IsService
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_Workflow_WhenResourceIsService_ReturnsTrue()
        {
            // ResourceType is intentionally something other than the service
            // strings to prove the IsService branch alone is sufficient.
            var resource = ResourceOfType("WorkflowService", isService: true).Object;

            Assert.IsTrue(Invoke<Workflow>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_Workflow_WhenResourceIsNotService_ReturnsFalse()
        {
            var resource = ResourceOfType("WorkflowService", isService: false).Object;

            Assert.IsFalse(Invoke<Workflow>(resource));
        }

        // -----------------------------------------------------------------
        // Branch 6: IResourceSource + IsSource
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_ResourceSource_DbSource_WhenIsSourceTrue_ReturnsTrue()
        {
            var resource = ResourceOfType("DbSource", isSource: true).Object;

            Assert.IsTrue(Invoke<DbSource>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_ResourceSource_WebSource_WhenIsSourceTrue_ReturnsTrue()
        {
            var resource = ResourceOfType("WebSource", isSource: true).Object;

            Assert.IsTrue(Invoke<WebSource>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_ResourceSource_SharepointSource_WhenIsSourceTrue_ReturnsTrue()
        {
            var resource = ResourceOfType("SharepointServerSource", isSource: true).Object;

            Assert.IsTrue(Invoke<SharepointSource>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_ResourceSource_WhenIsSourceFalse_ReturnsFalse()
        {
            var resource = ResourceOfType("DbSource", isSource: false).Object;

            Assert.IsFalse(Invoke<DbSource>(resource));
        }

        // -----------------------------------------------------------------
        // Branch 7: fall-through
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_UnrelatedType_WithNonMatchingResource_ReturnsFalse()
        {
            // Resource is the base class - it doesn't trigger any of the
            // named ResourceType branches and doesn't implement IResourceSource.
            var resource = ResourceOfType("ReservedService", isService: false, isSource: false).Object;

            Assert.IsFalse(Invoke<Resource>(resource));
        }

        // -----------------------------------------------------------------
        // Cross-branch guards: the early-return order matters
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_DbService_WithResourceTypeMatching_IgnoresIsServiceFlag()
        {
            // ResourceType match must win regardless of IsService value.
            var resource = ResourceOfType("DbService", isService: false).Object;

            Assert.IsTrue(Invoke<DbService>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_Workflow_WithResourceTypeWorkflow_IsServiceFalse_ReturnsFalse()
        {
            // ResourceType "Workflow" is NOT one of the recognised service
            // strings; only the IsService flag controls the Workflow branch.
            var resource = ResourceOfType("Workflow", isService: false).Object;

            Assert.IsFalse(Invoke<Workflow>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_DbSource_RequestedAsWorkflow_OnlyIsSourceSet_ReturnsFalse()
        {
            // A source-only resource must not satisfy a Workflow request.
            var resource = ResourceOfType("DbSource", isService: false, isSource: true).Object;

            Assert.IsFalse(Invoke<Workflow>(resource));
        }

        [TestMethod]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void CheckType_Workflow_RequestedAsSource_OnlyIsServiceSet_ReturnsFalse()
        {
            // A service-only resource must not satisfy a source request.
            var resource = ResourceOfType("WorkflowService", isService: true, isSource: false).Object;

            Assert.IsFalse(Invoke<DbSource>(resource));
        }
    }
}
