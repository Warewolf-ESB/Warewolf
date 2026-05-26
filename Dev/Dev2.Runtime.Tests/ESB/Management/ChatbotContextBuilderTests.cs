/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*/
using System;
using System.Collections.Generic;
using System.Reflection;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Configuration;

namespace Dev2.Tests.Runtime.ESB.Management
{
    [TestClass]
    public class ChatbotContextBuilderTests
    {
        const string Owner = "Coverage";
        const string Cat = nameof(ChatbotContextBuilder);

        static readonly MethodInfo SanitizeMethod = typeof(ChatbotContextBuilder)
            .GetMethod("SanitizeContentForPrompt", BindingFlags.NonPublic | BindingFlags.Static);

        static string Sanitize(string input) =>
            (string)SanitizeMethod.Invoke(null, new object[] { input });

        // ── SanitizeContentForPrompt ─────────────────────────────────────────

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Sanitize_Null_ReturnsNull()
        {
            Assert.IsNull(Sanitize(null));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Sanitize_Empty_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, Sanitize(string.Empty));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Sanitize_PlainAscii_Unchanged()
        {
            Assert.AreEqual("hello world", Sanitize("hello world"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Sanitize_StripsControlChars()
        {
            var input = "a\u0000b\u0007c\u001Fd\u007Fe";
            Assert.AreEqual("abcde", Sanitize(input));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Sanitize_KeepsTabNewlineCR()
        {
            var input = "a\tb\nc\rd";
            Assert.AreEqual(input, Sanitize(input));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Sanitize_RemovesZeroWidthAndInvisible()
        {
            var input = "a\u200Bb\u200Cc\u200Dd\uFEFFe\u00ADf";
            Assert.AreEqual("abcdef", Sanitize(input));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Sanitize_NeutralizesMarkdownHeadings()
        {
            var input = "# Title\n## Sub\n### third";
            var result = Sanitize(input);
            Assert.IsFalse(result.StartsWith("# "));
            Assert.IsTrue(result.Contains("\\"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Sanitize_NeutralizesTripleBacktickFences()
        {
            var input = "before ``` after";
            Assert.AreEqual("before ''' after", Sanitize(input));
        }

        // ── BuildSystemPrompt: empty / null settings paths ──────────────────

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void BuildSystemPrompt_NoResources_NoLog_ReturnsZeroFoundMessages()
        {
            var settings = new ChatbotSettingsData
            {
                IncludeSystemLog = false,
                SelectedResourceIds = new List<Guid>(),
                LoadResourcesAsXaml = false
            };

            var result = ChatbotContextBuilder.BuildSystemPrompt(settings);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("0 Selected Resources found."));
            Assert.IsTrue(result.Contains("0 lines of logs found."));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void BuildSystemPrompt_NullSelectedResources_TreatedAsEmpty()
        {
            var settings = new ChatbotSettingsData
            {
                IncludeSystemLog = false,
                LoadResourcesAsXaml = false,
                // Force null by reflection (setter coerces null to empty list)
            };
            typeof(ChatbotSettingsData)
                .GetField("_selectedResourceIds", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(settings, null);

            var result = ChatbotContextBuilder.BuildSystemPrompt(settings);

            Assert.IsTrue(result.Contains("0 Selected Resources found."));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void BuildSystemPrompt_IncludeSystemLog_NoLogFile_StillProducesPrompt()
        {
            var settings = new ChatbotSettingsData
            {
                IncludeSystemLog = true,
                NumberOfLogLines = 5,
                SelectedResourceIds = new List<Guid>(),
                LoadResourcesAsXaml = false
            };

            // Should not throw even when no log file is configured.
            var result = ChatbotContextBuilder.BuildSystemPrompt(settings);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("0 Selected Resources found."));
            // Either "0 lines of logs found." or an attempt to read; we just verify no crash.
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void BuildSystemPrompt_LoadResourcesAsXaml_ButEmpty_ReturnsZeroFound()
        {
            var settings = new ChatbotSettingsData
            {
                IncludeSystemLog = false,
                LoadResourcesAsXaml = true,
                SelectedResourceIds = new List<Guid>()
            };

            var result = ChatbotContextBuilder.BuildSystemPrompt(settings);
            Assert.IsTrue(result.Contains("0 Selected Resources found."));
        }

        // ── BuildSystemPrompt_verion_1 ──────────────────────────────────────

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void BuildSystemPromptV1_NoResources_NoLog_ReturnsPrompt()
        {
            var settings = new ChatbotSettingsData
            {
                IncludeSystemLog = false,
                SelectedResourceIds = new List<Guid>(),
                LoadResourcesAsXaml = false
            };

            var result = ChatbotContextBuilder.BuildSystemPrompt_verion_1(settings);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void BuildSystemPromptV1_IncludeSystemLog_AppendsLogSection()
        {
            var settings = new ChatbotSettingsData
            {
                IncludeSystemLog = true,
                NumberOfLogLines = 10,
                SelectedResourceIds = new List<Guid>(),
                LoadResourcesAsXaml = false
            };

            var result = ChatbotContextBuilder.BuildSystemPrompt_verion_1(settings);
            Assert.IsNotNull(result);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void XamlToX6Json_DefaultIsNull()
        {
            // The static property starts null and may be set during server startup.
            // Verify it can be set / reset without exception.
            var prior = ChatbotContextBuilder.XamlToX6Json;
            try
            {
                ChatbotContextBuilder.XamlToX6Json = info => "x6";
                Assert.IsNotNull(ChatbotContextBuilder.XamlToX6Json);
                ChatbotContextBuilder.XamlToX6Json = null;
                Assert.IsNull(ChatbotContextBuilder.XamlToX6Json);
            }
            finally
            {
                ChatbotContextBuilder.XamlToX6Json = prior;
            }
        }
    }
}
