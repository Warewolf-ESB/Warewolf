/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Runtime.CompilerServices;

// Declared in code rather than as an MSBuild <AssemblyAttribute>: this project sets
// GenerateAssemblyInfo=false (it links the shared AssemblyCommonInfo.cs), so MSBuild
// never emits generated assembly attributes. Same approach as Warewolf.QueueWorker.
//
// Exposes internal helpers to the unit tests: EngineForwarder.BuildPostBody and
// EngineWorkflowClient.BuildRelativeUrl - the mapping and route-building rules that carry
// the on-prem parity contract and are worth asserting directly.
[assembly: InternalsVisibleTo("Warewolf.Execution.QueueProcessor.Tests")]
