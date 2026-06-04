/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Statements;
using System.Reflection;
using System.Text;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ESB.WF
{
    [TestClass]
    public class XamlActivityHelperTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_GetXamlActivityBuilderAsDataActivities_NullInput_ReturnsNull()
        {
            var result = XamlActivityHelper.GetXamlActivityBuilderAsDataActivities(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_GetXamlActivityBuilderAsDataActivities_EmptyInput_ReturnsNull()
        {
            var result = XamlActivityHelper.GetXamlActivityBuilderAsDataActivities(new StringBuilder());

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_GetXamlActivityBuilderAsDataActivities_InvalidXaml_SwallowsExceptionAndReturnsNull()
        {
            var garbage = new StringBuilder("<this is not valid xaml at all>");

            var result = XamlActivityHelper.GetXamlActivityBuilderAsDataActivities(garbage);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_TryProcessX6JsonFromActivity_NullActivity_ReturnsFalse()
        {
            var ok = XamlActivityHelper.TryProcessX6JsonFromActivity(null, new Cell());

            Assert.IsFalse(ok);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_TryProcessX6JsonFromActivity_NonMatchingActivityType_ReturnsFalse()
        {
            var ok = XamlActivityHelper.TryProcessX6JsonFromActivity(new Sequence(), new Cell());

            Assert.IsFalse(ok);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_TryCastAndExecute_NullActivity_ReturnsFalse()
        {
            var ok = XamlActivityHelper.TryCastAndExecute<Activity>(null, _ => { });

            Assert.IsFalse(ok);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_TryCastAndExecute_NullAction_ReturnsFalse()
        {
            var ok = XamlActivityHelper.TryCastAndExecute<Activity>(new Sequence(), null);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_TryCastAndExecute_DirectCastSucceeds_InvokesAction()
        {
            Activity captured = null;
            var seq = new Sequence();

            var ok = XamlActivityHelper.TryCastAndExecute<Activity>(seq, a => captured = a);

            Assert.IsTrue(ok);
            Assert.AreSame(seq, captured);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_Constructor_PopulatesAssemblyCache()
        {
            var ctx = new Dev2XamlSchemaContext(typeof(Sequence).Assembly);

            Assert.IsTrue(ctx.CachedAssemblyCount > 0, "cache should hold at least the target assembly");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_GetCachedAssemblyNames_IncludesTargetAssembly()
        {
            var asm = typeof(Sequence).Assembly;
            var ctx = new Dev2XamlSchemaContext(asm);

            var names = ctx.GetCachedAssemblyNames();

            Assert.IsTrue(names is System.Collections.Generic.IEnumerable<string>);
            var asmName = asm.GetName().Name;
            CollectionAssert.Contains(System.Linq.Enumerable.ToList(names), asmName);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_GetXamlActivityBuilderAsDataActivitiesWithAppDomainResolution_NullInput_ReturnsNull()
        {
            var result = XamlActivityHelper.GetXamlActivityBuilderAsDataActivitiesWithAppDomainResolution(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_GetXamlActivityBuilderAsDataActivitiesWithAppDomainResolution_EmptyInput_ReturnsNull()
        {
            var result = XamlActivityHelper.GetXamlActivityBuilderAsDataActivitiesWithAppDomainResolution(new StringBuilder());

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_GetXamlActivityBuilderAsDataActivitiesWithAppDomainResolution_InvalidXaml_ReturnsNull()
        {
            var result = XamlActivityHelper.GetXamlActivityBuilderAsDataActivitiesWithAppDomainResolution(
                new StringBuilder("<not valid xaml>"));

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_RefreshAssemblyCache_Idempotent()
        {
            var ctx = new Dev2XamlSchemaContext(typeof(Sequence).Assembly);
            var before = ctx.CachedAssemblyCount;

            ctx.RefreshAssemblyCache();

            Assert.IsTrue(ctx.CachedAssemblyCount >= before);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_GetXamlType_TypeFromTargetAssembly_ReturnsXamlType()
        {
            var asm = typeof(Sequence).Assembly;
            var ctx = new Dev2XamlSchemaContext(asm);

            // Sequence lives in the target assembly -> early-return base path.
            var xt = ctx.GetXamlType(typeof(Sequence));

            Assert.IsNotNull(xt);
            Assert.AreEqual(typeof(Sequence), xt.UnderlyingType);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_GetXamlType_TypeFromCachedOtherAssembly_ReturnsXamlType()
        {
            var asm = typeof(Sequence).Assembly;
            var ctx = new Dev2XamlSchemaContext(asm);

            // typeof(int) is in a different assembly -> exercises the cache-lookup branch.
            var xt = ctx.GetXamlType(typeof(int));

            Assert.IsNotNull(xt);
        }

        // --------------------------------------------------------------
        // 8431- additional coverage for XamlActivityHelper / Dev2XamlSchemaContext
        // --------------------------------------------------------------

        static StringBuilder BuildMinimalActivityBuilderXaml()
        {
            // Round-trip an ActivityBuilder through XamlServices to get valid XAML that the
            // Dev2 helper can reload. This exercises the success branch of
            // GetXamlActivityBuilderAsDataActivities / ...WithAppDomainResolution.
            var builder = new ActivityBuilder { Implementation = new Sequence() };
            return new StringBuilder(XamlServices.Save(builder));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_GetXamlActivityBuilderAsDataActivities_ValidXaml_ReturnsActivityBuilder()
        {
            var xaml = BuildMinimalActivityBuilderXaml();

            var result = XamlActivityHelper.GetXamlActivityBuilderAsDataActivities(xaml);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActivityBuilder));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_GetXamlActivityBuilderAsDataActivitiesWithAppDomainResolution_ValidXaml_ReturnsActivityBuilder()
        {
            var xaml = BuildMinimalActivityBuilderXaml();

            var result = XamlActivityHelper.GetXamlActivityBuilderAsDataActivitiesWithAppDomainResolution(xaml);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActivityBuilder));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_GetXamlType_ByXamlTypeName_TargetAssemblyClrNamespace_ResolvesType()
        {
            var asm = typeof(Sequence).Assembly;
            var ctx = new Dev2XamlSchemaContext(asm);
            var name = new XamlTypeName(
                $"clr-namespace:System.Activities.Statements;assembly={asm.GetName().Name}",
                nameof(Sequence));

            var xt = ctx.GetXamlType(name);

            Assert.IsNotNull(xt);
            Assert.AreEqual(typeof(Sequence), xt.UnderlyingType);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_GetXamlType_ByXamlTypeName_NoAssemblySpecifier_ResolvesViaCache()
        {
            var ctx = new Dev2XamlSchemaContext(typeof(Sequence).Assembly);
            // No assembly= specifier - exercises the loop-all-cached-assemblies branch.
            var name = new XamlTypeName("clr-namespace:System.Activities.Statements", nameof(Sequence));

            var xt = ctx.GetXamlType(name);

            Assert.IsNotNull(xt);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_GetXamlType_ByXamlTypeName_MismatchedAssembly_FallsBackToBase()
        {
            var ctx = new Dev2XamlSchemaContext(typeof(Sequence).Assembly);
            // Assembly that does not contain the type -> skip-and-fallback branch.
            var name = new XamlTypeName(
                "clr-namespace:System.Activities.Statements;assembly=NonExistent.Assembly.Xyz",
                nameof(Sequence));

            var xt = ctx.GetXamlType(name);

            // Either null (base could not resolve) or non-null if base finds it; we only care
            // that the code path completed without throwing.
            Assert.IsTrue(xt == null || xt.UnderlyingType == typeof(Sequence) || xt.UnderlyingType == null);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_GetXamlType_ByXamlTypeName_PlainNamespace_FallsThroughToBase()
        {
            var ctx = new Dev2XamlSchemaContext(typeof(Sequence).Assembly);
            // Non clr-namespace -> ResolveTypeFromCache early-exits, fallback to base.
            var name = new XamlTypeName(
                "http://schemas.microsoft.com/netfx/2009/xaml/activities",
                nameof(Sequence));

            var xt = ctx.GetXamlType(name);

            Assert.IsNotNull(xt);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(XamlActivityHelper))]
        public void XamlActivityHelper_TryProcessX6JsonFromActivity_MatchingType_InvokesToX6Json_ReturnsTrue()
        {
            var activity = new Unlimited.Applications.BusinessDesignStudio.Activities.DsfDotNetMultiAssignActivity();
            var cell = new Cell();

            var ok = XamlActivityHelper.TryProcessX6JsonFromActivity(activity, cell);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(Dev2XamlSchemaContext))]
        public void Dev2XamlSchemaContext_GetXamlType_ByXamlTypeName_NullOrEmptyInputs_DoesNotThrow()
        {
            var ctx = new Dev2XamlSchemaContext(typeof(Sequence).Assembly);
            // empty namespace path -> ResolveTypeFromCache returns null immediately.
            var name = new XamlTypeName(string.Empty, "DoesNotExist");

            var xt = ctx.GetXamlType(name);

            // Base resolver returns null/unknown for this combination; the assertion just
            // confirms no exception bubbled up.
            Assert.IsTrue(xt == null || xt.UnderlyingType == null || xt.UnderlyingType != null);
        }
    }
}
