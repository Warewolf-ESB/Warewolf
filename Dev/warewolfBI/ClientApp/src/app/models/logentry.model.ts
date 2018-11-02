export class LogEntry {
  public WorkflowID: string;
  public WorkflowName: string;
  public ExecutionID: string;
  public AuditType: string;
  public PreviousActivity: string;
  public PreviousActivityType: string;
  public PreviousActivityID: string;
  public NextActivity: string;
  public NextActivityType: string;
  public NextActivityID: string;
  public ServerID: string;
  public ParentID: string;
  public ClientID: string;
  public ExecutingUser: string;
  public ExecutionOrigin: number;
  public ExecutionOriginDescription: string;
  public ExecutionToken: string;
  public AdditionalDetail: string;
  public IsSubExecution: number;
  public IsRemoteWorkflow: number;
  public Environment: string;
  public AuditDate: string;


  constructor(values: Object = {}) {
    Object.assign(this, values);
  }
}
