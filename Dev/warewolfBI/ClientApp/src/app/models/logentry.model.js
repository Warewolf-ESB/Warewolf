export class LogEntry {
    //TODO:These will be used with the executionlog format not yet merged
    //public AuditType: string;
    //public PreviousActivity: string;
    //public PreviousActivityType: string;
    //public PreviousActivityID: string;
    //public NextActivity: string;
    //public NextActivityType: string;
    //public NextActivityID: string;
    //public ServerID: string;
    //public ParentID: string;
    //public ClientID: string;
    //public ExecutingUser: string;
    //public ExecutionOrigin: number;
    //public ExecutionOriginDescription: string;
    //public ExecutionToken: string;
    //public AdditionalDetail: string;
    //public IsSubExecution: number;
    //public IsRemoteWorkflow: number;
    //public Environment: string;
    //public AuditDate: string;
    //public WorkflowID: string;
    //public WorkflowName: string;
    constructor(values = {}) {
        Object.assign(this, values);
    }
}
//# sourceMappingURL=logentry.model.js.map