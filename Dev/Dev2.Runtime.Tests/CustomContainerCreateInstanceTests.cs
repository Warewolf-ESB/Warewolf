/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Coverage for <see cref="CustomContainer.CreateInstance{T}(object[])"/>
    /// (Crap Score 342, Cyclomatic Complexity 18 on the coverage dashboard).
    ///
    /// CreateInstance scans <see cref="CustomContainer.LoadedTypes"/> for a
    /// concrete, non-abstract, non-generic, public class assignable to T,
    /// then looks for a constructor whose parameter list matches the supplied
    /// arguments (length and type assignability), invoking the first match.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class CustomContainerCreateInstanceTests
    {
        [TestInitialize]
        public void Init()
        {
            CustomContainer.Clear();
            // CreateInstance reads LoadedTypes directly; reset between tests.
            CustomContainer.LoadedTypes = null;
        }

        [TestCleanup]
        public void Cleanup()
        {
            CustomContainer.LoadedTypes = null;
        }

        // -----------------------------------------------------------------
        // Empty / null LoadedTypes
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_NoLoadedTypes_ReturnsDefault()
        {
            // LoadedTypes is null, no scan happens, default(T) returned.
            var result = CustomContainer.CreateInstance<ICiTarget>();

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_EmptyLoadedTypes_ReturnsDefault()
        {
            CustomContainer.LoadedTypes = new List<Type>();

            var result = CustomContainer.CreateInstance<ICiTarget>();

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_LoadedTypesContainsNull_FiltersAndReturnsDefault()
        {
            // The implementation explicitly filters out null entries.
            CustomContainer.AddToLoadedTypes(typeof(CiConcrete));
            CustomContainer.LoadedTypes!.Add(null!);

            var result = CustomContainer.CreateInstance<ICiTarget>("hello");

            Assert.IsNotNull(result);
            Assert.AreEqual("hello", result!.Name);
        }

        // -----------------------------------------------------------------
        // Constructor matching (length + type assignability)
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_MatchingSingleStringArg_InvokesCtor()
        {
            CustomContainer.AddToLoadedTypes(typeof(CiConcrete));

            var result = CustomContainer.CreateInstance<ICiTarget>("alpha");

            Assert.IsNotNull(result);
            Assert.AreEqual("alpha", result!.Name);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_MatchingDualArg_InvokesCorrectOverload()
        {
            CustomContainer.AddToLoadedTypes(typeof(CiConcrete));

            var result = CustomContainer.CreateInstance<ICiTarget>("beta", 42);

            Assert.IsNotNull(result);
            Assert.AreEqual("beta:42", result!.Name);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_ArgCountMismatch_NoCtorFound_ReturnsDefault()
        {
            // CiConcrete has only 1-arg and 2-arg ctors; 3 args ⇒ no match.
            CustomContainer.AddToLoadedTypes(typeof(CiConcrete));

            var result = CustomContainer.CreateInstance<ICiTarget>("a", "b", "c");

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_ParameterTypeMismatch_ReturnsDefault()
        {
            // CiConcrete's 1-arg ctor expects a string, give it an int.
            CustomContainer.AddToLoadedTypes(typeof(CiConcrete));

            var result = CustomContainer.CreateInstance<ICiTarget>(123);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_AssignableParameter_MatchesAndInvokes()
        {
            // Ctor takes ICiPayload; supplying a CiPayloadImpl must satisfy
            // the IsAssignableFrom branch of the matcher.
            CustomContainer.AddToLoadedTypes(typeof(CiAssignable));

            var payload = new CiPayloadImpl { Value = "carried" };
            var result = CustomContainer.CreateInstance<ICiTarget>(payload);

            Assert.IsNotNull(result);
            Assert.AreEqual("carried", result!.Name);
        }

        // -----------------------------------------------------------------
        // Filtering: abstract / generic / non-public / non-class skipped
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_AbstractTypeInList_NotInstantiated()
        {
            // Only the abstract type is loaded; matcher must skip it.
            CustomContainer.AddToLoadedTypes(typeof(CiAbstract));

            var result = CustomContainer.CreateInstance<ICiTarget>("x");

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_GenericTypeInList_NotInstantiated()
        {
            CustomContainer.AddToLoadedTypes(typeof(CiGeneric<>));

            var result = CustomContainer.CreateInstance<ICiTarget>("x");

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void CreateInstance_NotAssignableType_SkippedSilently()
        {
            // OtherConcrete does not implement ICiTarget.
            CustomContainer.AddToLoadedTypes(typeof(OtherConcrete));

            var result = CustomContainer.CreateInstance<ICiTarget>("x");

            Assert.IsNull(result);
        }

        // -----------------------------------------------------------------
        // AddToLoadedTypes idempotence (touched by every CreateInstance path)
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("CustomContainer_CreateInstance")]
        public void AddToLoadedTypes_DuplicateRegistration_Deduplicated()
        {
            CustomContainer.AddToLoadedTypes(typeof(CiConcrete));
            CustomContainer.AddToLoadedTypes(typeof(CiConcrete));

            Assert.AreEqual(1, CustomContainer.LoadedTypes!.Count);
        }
    }

    // ---------- Test fixtures (shapes the matcher must traverse) ----------

    public interface ICiTarget
    {
        string Name { get; }
    }

    public interface ICiPayload
    {
        string Value { get; }
    }

    public class CiConcrete : ICiTarget
    {
        public string Name { get; }
        public CiConcrete(string name) { Name = name; }
        public CiConcrete(string name, int n) { Name = $"{name}:{n}"; }
    }

    public class CiAssignable : ICiTarget
    {
        public string Name { get; }
        public CiAssignable(ICiPayload payload) { Name = payload.Value; }
    }

    public class CiPayloadImpl : ICiPayload
    {
        public string Value { get; set; } = string.Empty;
    }

    public abstract class CiAbstract : ICiTarget
    {
        public string Name => "abstract";
        protected CiAbstract(string _) { }
    }

    public class CiGeneric<T> : ICiTarget
    {
        public string Name => typeof(T).Name;
        public CiGeneric(string _) { }
    }

    public class OtherConcrete
    {
        public OtherConcrete(string _) { }
    }
}
