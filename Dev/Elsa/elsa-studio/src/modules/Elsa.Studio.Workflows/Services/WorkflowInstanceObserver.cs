using Elsa.Api.Client.RealTime.Messages;
using Elsa.Studio.Workflows.Contracts;
using Microsoft.AspNetCore.SignalR.Client;

namespace Elsa.Studio.Workflows.Services;

/// <summary>
/// A wrapper around a SignalR connection that receives notifications about workflow instance updates.
/// </summary>
public class WorkflowInstanceObserver : IWorkflowInstanceObserver
{
    private readonly HubConnection _connection;

    public WorkflowInstanceObserver(HubConnection connection)
    {
        _connection = connection;

        connection.On("ActivityExecutionLogUpdatedAsync", async (ActivityExecutionLogUpdatedMessage message) => await OnActivityExecutionLogUpdatedAsync(message));
        connection.On("WorkflowExecutionLogUpdatedAsync", async (WorkflowExecutionLogUpdatedMessage message) => await OnWorkflowJournalUpdatedAsync(message));
        connection.On("WorkflowInstanceUpdatedAsync", async (WorkflowInstanceUpdatedMessage message) => await OnWorkflowInstanceUpdatedAsync(message));
    }

    public event Func<WorkflowExecutionLogUpdatedMessage, Task> WorkflowJournalUpdated = default!;
    public event Func<ActivityExecutionLogUpdatedMessage, Task> ActivityExecutionLogUpdated = default!;
    public event Func<WorkflowInstanceUpdatedMessage, Task> WorkflowInstanceUpdated = default!;

    private async Task OnWorkflowJournalUpdatedAsync(WorkflowExecutionLogUpdatedMessage arg)
    {
        if (WorkflowJournalUpdated != null!) await WorkflowJournalUpdated(arg);
    }

    private async Task OnActivityExecutionLogUpdatedAsync(ActivityExecutionLogUpdatedMessage arg)
    {
        if (ActivityExecutionLogUpdated != null!) await ActivityExecutionLogUpdated(arg);
    }

    private async Task OnWorkflowInstanceUpdatedAsync(WorkflowInstanceUpdatedMessage arg)
    {
        if (WorkflowInstanceUpdated != null!) await WorkflowInstanceUpdated(arg);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}