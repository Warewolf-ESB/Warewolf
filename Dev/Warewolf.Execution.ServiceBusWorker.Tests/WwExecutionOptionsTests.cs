/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Pins WwExecutionOptions.EffectiveScope's derive-vs-override behaviour.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Execution.ServiceBusWorker.Tests;

[TestClass]
public class WwExecutionOptionsTests
{
    [TestMethod]
    [TestCategory("UnitTest")]
    public void EffectiveScope_ScopeNotSet_DerivesDefaultFromResourceAppId()
    {
        var options = new WwExecutionOptions { ResourceAppId = "resource-app-1" };

        Assert.AreEqual("api://resource-app-1/.default", options.EffectiveScope);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void EffectiveScope_ScopeSet_ReturnsConfiguredScope()
    {
        var options = new WwExecutionOptions
        {
            ResourceAppId = "resource-app-1",
            Scope = "api://custom-scope/.default",
        };

        Assert.AreEqual("api://custom-scope/.default", options.EffectiveScope);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void EffectiveScope_ScopeBlank_DerivesDefault()
    {
        var options = new WwExecutionOptions { ResourceAppId = "resource-app-1", Scope = "   " };

        Assert.AreEqual("api://resource-app-1/.default", options.EffectiveScope);
    }
}
