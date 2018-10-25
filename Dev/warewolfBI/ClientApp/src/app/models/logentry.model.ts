export class LogEntry  {
  public StartDateTime: string;
  public Status: string;
  public Url: string;
  public Result: string;
  public User: string;
  public CompletedDateTime: string;
  public ExecutionTime: string;
  public Count: number;
  public ExecutionId: string;

  constructor(values: Object = {}) {
    Object.assign(this, values);
  }  
}
