export class LogEntry {
  public $id: string;
  public $type: string;
  public StartDateTime: string;
  public Status: string;
  public CompletedDateTime: string;
  public Url: string;
  public Result: string;
  public ExecutionID: string;
  public User: string;
  public ExecutionTime: string;
  public WorkflowName: string;
  public AuditType: string;
  public Environment: string;
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
  public WorkflowID: string;
  public VersionNumber: string;
  LogEntry: LogEntry;

  constructor(values: Object = {}) {
    Object.assign(this, values);
  }
}
