/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common.Interfaces;
using Dev2.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Execution.Lightweight.Mcp.Execution;

/// <summary>
/// Maps the Lightweight-native <see cref="TestDefinitionModel"/> (the persisted <c>.test.json</c>
/// shape written by <c>create_test</c>/<c>edit_test</c>) onto the concrete <c>Dev2.Data</c> service-
/// test types (<see cref="ServiceTestModelTO"/>/<see cref="ServiceTestStepTO"/>/
/// <see cref="ServiceTestInputTO"/>/<see cref="ServiceTestOutputTO"/>) that the shared
/// <c>Dev2.Activities</c> assertion engine expects on <c>IDSFDataObject.ServiceTest</c> — see
/// <see cref="Warewolf.Execution.Lightweight.WorkflowExecutor.ExecuteTest"/>.
/// </summary>
internal static class TestDefinitionMapper
{
    internal static IServiceTestModelTO Map(TestDefinitionModel model)
    {
        var authenticationType = Enum.TryParse<Dev2.Runtime.ServiceModel.Data.AuthenticationType>(model.AuthenticationType, ignoreCase: true, out var parsedAuth)
            ? parsedAuth
            : Dev2.Runtime.ServiceModel.Data.AuthenticationType.Public;

        return new ServiceTestModelTO
        {
            TestName = model.TestName,
            UserName = model.UserName,
            Password = model.Password,
            Inputs = (model.Inputs ?? new List<TestInputModel>()).Select(MapInput).Cast<IServiceTestInput>().ToList(),
            Outputs = (model.Outputs ?? new List<TestOutputModel>()).Select(MapOutput).Cast<IServiceTestOutput>().ToList(),
            NoErrorExpected = model.NoErrorExpected,
            ErrorExpected = model.ErrorExpected,
            ErrorContainsText = model.ErrorContainsText,
            Enabled = model.Enabled,
            AuthenticationType = authenticationType,
            TestSteps = (model.TestSteps ?? new List<TestStepModel>()).Select(MapStep).Cast<IServiceTestStep>().ToList(),
        };
    }

    static ServiceTestInputTO MapInput(TestInputModel model) => new()
    {
        Variable = model.Variable,
        Value = model.Value,
        EmptyIsNull = model.EmptyIsNull,
    };

    static ServiceTestOutputTO MapOutput(TestOutputModel model) => new()
    {
        Variable = model.Variable,
        Value = model.Value,
        From = model.From,
        To = model.To,
        AssertOp = string.IsNullOrEmpty(model.AssertOp) ? "=" : model.AssertOp,
        HasOptionsForValue = model.HasOptionsForValue,
        OptionsForValue = model.OptionsForValue,
    };

    static ServiceTestStepTO MapStep(TestStepModel model)
    {
        var activityId = Guid.TryParse(model.ActivityId, out var id) ? id : Guid.Empty;
        var stepType = Enum.TryParse<StepType>(model.Type, ignoreCase: true, out var parsedType) ? parsedType : StepType.Assert;

        return new ServiceTestStepTO
        {
            ActivityID = activityId,
            UniqueID = activityId,
            ActivityType = model.ActivityType,
            Type = stepType,
            StepDescription = model.StepDescription,
            StepOutputs = new ObservableCollection<IServiceTestOutput>(
                (model.StepOutputs ?? new List<TestOutputModel>()).Select(MapOutput).Cast<IServiceTestOutput>()),
            Children = new ObservableCollection<IServiceTestStep>(
                (model.Children ?? new List<TestStepModel>()).Select(MapStep).Cast<IServiceTestStep>()),
        };
    }
}
