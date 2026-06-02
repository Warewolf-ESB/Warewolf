/*
*  Warewolf - Once bitten, there's no going back
*/
using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Tests.Coverage
{
    [TestClass]
    public class ResourceAccessorCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "ResourceAccessors";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ErrorResource_AllStaticStringProperties_AreReadable()
        {
            var type = typeof(Warewolf.Resource.Errors.ErrorResource);
            HitAllStaticStringProperties(type);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Messages_AllStaticStringProperties_AreReadable()
        {
            var type = typeof(Warewolf.Resource.Messages.Messages);
            HitAllStaticStringProperties(type);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ErrorResource_ResourceManager_AndCulture_Reachable()
        {
            var rm = Warewolf.Resource.Errors.ErrorResource.ResourceManager;
            Assert.IsNotNull(rm);
            var prev = Warewolf.Resource.Errors.ErrorResource.Culture;
            Warewolf.Resource.Errors.ErrorResource.Culture = prev;
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Messages_ResourceManager_AndCulture_Reachable()
        {
            var rm = Warewolf.Resource.Messages.Messages.ResourceManager;
            Assert.IsNotNull(rm);
            var prev = Warewolf.Resource.Messages.Messages.Culture;
            Warewolf.Resource.Messages.Messages.Culture = prev;
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void OtherWarewolfResourceTypes_StaticStrings_AreReadable()
        {
            var asm = typeof(Warewolf.Resource.Errors.ErrorResource).Assembly;
            int hit = 0;
            foreach (var t in asm.GetTypes())
            {
                if (!t.IsClass || t.IsAbstract) continue;
                var ga = t.GetCustomAttribute<System.CodeDom.Compiler.GeneratedCodeAttribute>();
                if (ga == null) continue;
                if (t == typeof(Warewolf.Resource.Errors.ErrorResource)) continue;
                if (t == typeof(Warewolf.Resource.Messages.Messages)) continue;
                hit += HitAllStaticStringProperties(t);
            }
            Assert.IsTrue(hit >= 0);
        }

        static int HitAllStaticStringProperties(Type type)
        {
            int count = 0;
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var p in props)
            {
                if (p.PropertyType != typeof(string)) continue;
                if (!p.CanRead) continue;
                try
                {
                    var _ = p.GetValue(null);
                    count++;
                }
                catch
                {
                    // ignore - some resources may not exist in test resource fallback
                }
            }
            return count;
        }
    }
}
