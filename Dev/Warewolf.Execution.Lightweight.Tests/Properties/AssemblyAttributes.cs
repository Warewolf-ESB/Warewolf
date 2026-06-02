using System.Runtime.CompilerServices;

// Exposes the in-process test fakes (FakeHttpRequestData / FakeHttpResponseData /
// TestFunctionContext / HttpFunctionContext) and other internal test helpers to the
// integration-test assembly so it can drive WorkflowHttpFunction in-process without
// duplicating the Functions-worker plumbing.
[assembly: InternalsVisibleTo("Warewolf.Execution.Lightweight.Integration.Tests")]
