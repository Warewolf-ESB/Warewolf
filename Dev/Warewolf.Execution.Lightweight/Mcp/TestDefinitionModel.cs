/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// The Lightweight-native workflow-test schema (docs/WorkflowTestFramework-Plan.md §4) — plain
/// JSON, camelCase, no <c>$type</c> discriminators. Shared by <c>create_test</c>/<c>edit_test</c>
/// (which parse a caller-supplied <see cref="JsonElement"/> into this shape and persist it) and by
/// <c>execute_test</c>'s <c>TestDefinitionMapper</c> (which reads the persisted JSON back via this
/// same shape and maps it onto the concrete <c>Dev2.Data</c> types the shared assertion engine in
/// <c>Dev2.Activities</c> expects).
/// </summary>
internal sealed record TestDefinitionModel(
    [property: JsonPropertyName("testName")] string TestName = "",
    [property: JsonPropertyName("enabled")] bool Enabled = true,
    [property: JsonPropertyName("authenticationType")] string? AuthenticationType = null,
    [property: JsonPropertyName("userName")] string? UserName = null,
    [property: JsonPropertyName("password")] string? Password = null,
    [property: JsonPropertyName("inputs")] List<TestInputModel>? Inputs = null,
    [property: JsonPropertyName("outputs")] List<TestOutputModel>? Outputs = null,
    [property: JsonPropertyName("noErrorExpected")] bool NoErrorExpected = false,
    [property: JsonPropertyName("errorExpected")] bool ErrorExpected = false,
    [property: JsonPropertyName("errorContainsText")] string? ErrorContainsText = null,
    [property: JsonPropertyName("testSteps")] List<TestStepModel>? TestSteps = null)
{
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}

internal sealed record TestInputModel(
    [property: JsonPropertyName("variable")] string Variable = "",
    [property: JsonPropertyName("value")] string Value = "",
    [property: JsonPropertyName("emptyIsNull")] bool EmptyIsNull = false);

internal sealed record TestOutputModel(
    [property: JsonPropertyName("variable")] string Variable = "",
    [property: JsonPropertyName("value")] string Value = "",
    [property: JsonPropertyName("from")] string? From = null,
    [property: JsonPropertyName("to")] string? To = null,
    [property: JsonPropertyName("assertOp")] string AssertOp = "=",
    [property: JsonPropertyName("hasOptionsForValue")] bool HasOptionsForValue = false,
    [property: JsonPropertyName("optionsForValue")] List<string>? OptionsForValue = null);

internal sealed record TestStepModel(
    [property: JsonPropertyName("activityId")] string ActivityId = "",
    [property: JsonPropertyName("activityType")] string ActivityType = "",
    [property: JsonPropertyName("type")] string Type = "",
    [property: JsonPropertyName("stepDescription")] string? StepDescription = null,
    [property: JsonPropertyName("stepOutputs")] List<TestOutputModel>? StepOutputs = null,
    [property: JsonPropertyName("children")] List<TestStepModel>? Children = null);
